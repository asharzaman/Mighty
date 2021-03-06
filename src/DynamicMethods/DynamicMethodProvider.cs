﻿#if DYNAMIC_METHODS
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Threading;

namespace Mighty
{
    /// <summary>
    /// Allow dynamic methods on instances of <see cref="MightyOrm"/>, implementing them via a wrapper object. 
    /// </summary>
    /// <remarks>
    /// We can't make MightyOrm directly implement <see cref="DynamicObject"/>, since it inherits from
    /// <see cref="Interfaces.MightyOrmAbstractInterface{T}"/> and C# doesn't allow multiple inheritance.
    /// </remarks>
    /// <typeparam name="T">The generic type for items returned by this instance</typeparam>
    public partial class MightyOrm<T> : IDynamicMetaObjectProvider where T : class, new()
    {
        private DynamicMethodProvider<T> DynamicObjectWrapper;

        /// <summary>
        /// Implements IDynamicMetaObjectProvider.
        /// </summary>
        /// <param name="expression">The expression defining what is needed</param>
        /// <returns></returns>
        /// <remarks>
        /// Inspired by http://stackoverflow.com/a/17634595/795690
        /// </remarks>
        /// <remarks>
        /// Support dynamic methods via a wrapper object (needed as we can't do direct multiple inheritance)
        /// This code is being called all the time (ALL methods calls to <see cref="IDynamicMetaObjectProvider"/> objects go through this, even
        /// when not stored in dynamic). This is the case for all <see cref="DynamicObject"/>s too (e.g. as in Massive) but you don't see it
        /// when debugging in that case, as GetMetaObject() is not user code if you inherit directly from DynamicObject.
        /// </remarks>
        public DynamicMetaObject GetMetaObject(Expression expression)
        {
            return new DelegatingMetaObject(this, DynamicObjectWrapper, expression);
        }
    }

    /// <summary>
    /// Wrapper to provide dynamic methods (wrapper object on mighty needed as we can't do direct multiple inheritance).
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// This is included mainly for easy compatibility with Massive.
    /// Pros: The methods this provides are quite cool...
    /// Cons: They're never visible in IntelliSense; you don't really need them; it turns out this adds overhead to
    /// *every* call to the micro-ORM, even when the object is not stored in a dynamic.
    /// </remarks>
    internal class DynamicMethodProvider<T> : DynamicObject where T : class, new()
    {
        private MightyOrm<T> Mighty;

        /// <summary>
        /// Wrap MightyOrm to provide Massive-compatible dynamic methods.
        /// You can access almost all this functionality non-dynamically (and if you do, you get IntelliSense, which makes life easier).
        /// </summary>
        /// <param name="me">The <see cref="MightyOrm{T}"/> instance</param>
        internal DynamicMethodProvider(MightyOrm<T> me)
        {
            Mighty = me;
        }

        /// <summary>
        /// Provides the implementation for operations that invoke a member. This method implementation tries to create queries from the methods being invoked based on the name
        /// of the invoked method.
        /// </summary>
        /// <param name="binder">Provides information about the dynamic operation. The binder.Name property provides the name of the member on which the dynamic operation is performed. 
        /// For example, for the statement sampleObject.SampleMethod(100), where sampleObject is an instance of the class derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, 
        /// binder.Name returns "SampleMethod". The binder.IgnoreCase property specifies whether the member name is case-sensitive.</param>
        /// <param name="args">The arguments that are passed to the object member during the invoke operation. For example, for the statement sampleObject.SampleMethod(100), where sampleObject is 
        /// derived from the <see cref="T:System.Dynamic.DynamicObject"/> class, <paramref name="args"/>[0] is equal to 100.</param>
        /// <param name="result">The result of the member invocation.</param>
        /// <returns>
        /// true if the operation is successful; otherwise, false. If this method returns false, the run-time binder of the language determines the behavior. (In most cases, a language-specific 
        /// run-time exception is thrown.)
        /// </returns>
        /// <remarks>Massive code (see CREDITS file), with added columns support (which is only possible using named arguments).</remarks>
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            result = null;
            var info = binder.CallInfo;
            if (info.ArgumentNames.Count != args.Length)
            {
                throw new InvalidOperationException("No non-dynamic method was found and named arguments are required for dynamically invoked methods. You can use: '<column-name>:', 'orderBy:', 'columns:', 'where:' or 'args:'");
            }

            var columns = "*";
            var orderBy = Mighty.PrimaryKeyFields;
            var wherePredicates = new List<string>();
            var nameValueArgs = new ExpandoObject();
            var nameValueDictionary = nameValueArgs.ToDictionary();
            var cancellationToken = CancellationToken.None;
            object[] userArgs = null;
            if (info.ArgumentNames.Count > 0)
            {
                for (int i = 0; i < args.Length; i++)
                {
                    var name = info.ArgumentNames[i];
                    switch (name.ToLowerInvariant())
                    {
                        case "cancellationtoken":
                            var usable = args[i] as CancellationToken?;
                            if (usable == null)
                            {
                                throw new InvalidOperationException($"Invalid {nameof(CancellationToken)} passed to dynamically invoked method");
                            }
                            cancellationToken = (CancellationToken)usable;
                            break;
                        case "orderby":
                            orderBy = args[i].ToString();
                            break;
                        case "columns":
                            columns = args[i].ToString();
                            break;
                        case "where":
                            // this is an arbitrary SQL WHERE specification, so we have to wrap it in brackets to avoid operator precedence issues
                            wherePredicates.Add("(" + args[i].ToString().Unthingify("WHERE") + ")");
                            break;
                        case "args":
                            // wrap anything other than an array in an array (this is what C# params basically does anyway)
                            userArgs = args[i] as object[];
                            if (userArgs == null)
                            {
                                userArgs = new object[] { args[i] };
                            }
                            break;
                        default:
                            // treat anything else as a name-value pair
                            wherePredicates.Add(string.Format("{0} = {1}", name, Mighty.Plugin.PrefixParameterName(name)));
                            nameValueDictionary.Add(name, args[i]);
                            break;
                    }
                }
            }
            var whereClause = string.Empty;
            if (wherePredicates.Count > 0)
            {
                whereClause = " WHERE " + string.Join(" AND ", wherePredicates);
            }

            var op = binder.Name;
            var uOp = op.ToUpperInvariant();

            bool useAsync = uOp.EndsWith("ASYNC");
            if (useAsync)
            {
#if NET40
                throw new InvalidOperationException($"Dynamically invoked method $op: async is not supported on .NET Framework 4.0");
#else
                uOp = uOp.Substring(0, uOp.Length - 5);
#endif
            }

            switch (uOp)
            {
                case "COUNT":
                case "SUM":
                case "MAX":
                case "MIN":
                case "AVG":
                    if (useAsync)
                    {
#if !NET40
                        result = Mighty.AggregateWithParamsAsync(uOp, columns, cancellationToken, whereClause, inParams: nameValueArgs, args: userArgs);
#endif
                    }
                    else
                    {
                        result = Mighty.AggregateWithParams(uOp, columns, whereClause, inParams: nameValueArgs, args: userArgs);
                    }
                    break;
                default:
                    var justOne = uOp.StartsWith("FIRST") || uOp.StartsWith("LAST") || uOp.StartsWith("GET") || uOp.StartsWith("FIND") || uOp.StartsWith("SINGLE");
                    // For Last only, sort by DESC on the PK (PK sort is the default)
                    if (uOp.StartsWith("LAST"))
                    {
                        // this will be incorrect if multiple PKs are present, but note that the ORDER BY may be from a dynamic method
                        // argument by this point; this could be done correctly for compound PKs, but not in general for user input (it
                        // would involve parsing SQL, which we never do)
                        orderBy = orderBy + " DESC";
                    }
                    if (justOne)
                    {
                        if (useAsync)
                        {
#if !NET40
                            result = Mighty.SingleWithParamsAsync(whereClause, cancellationToken, orderBy, columns, inParams: nameValueArgs, args: userArgs);
#endif
                        }
                        else
                        {
                            result = Mighty.SingleWithParams(whereClause, orderBy, columns, inParams: nameValueArgs, args: userArgs);
                        }
                    }
                    else
                    {
                        if (useAsync)
                        {
#if !NET40
                            result = Mighty.AllWithParamsAsync(cancellationToken, whereClause, orderBy, columns, inParams: nameValueArgs, args: userArgs);
#endif
                        }
                        else
                        {
                            result = Mighty.AllWithParams(whereClause, orderBy, columns, inParams: nameValueArgs, args: userArgs);
                        }
                    }
                    break;
            }
            return true;
        }
    }
}
#endif