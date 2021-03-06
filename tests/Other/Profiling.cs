﻿using System;
using NUnit.Framework;
using Mighty.Profiling;
using SqlProfiler.Simple;

using MightyTests.Profiling;

namespace Mighty.XAllTests.SqlProfiling
{
    [TestFixture]
    public class Profiling
    {
        [Test]
        public void ExecuteDbDataReaderCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(1397, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(702, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#elif NETCOREAPP
#if NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(1188, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1315, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#elif NETCOREAPP2_0
#if DISABLE_DEVART
            Assert.AreEqual(795, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1315, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(1120, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#else
            Assert.AreEqual(1249, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteDbDataReader]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteNonQueryCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(341, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(176, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#elif NETCOREAPP
#if NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(222, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(268, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#elif NETCOREAPP2_0
#if DISABLE_DEVART
            Assert.AreEqual(192, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(268, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(152, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#else
            Assert.AreEqual(198, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteNonQuery]);
#endif
#endif
#endif
        }

        [Test]
        public void ExecuteScalarCount()
        {
#if NETFRAMEWORK
#if !NET40
            Assert.AreEqual(397, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(200, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#elif NETCOREAPP
#if NETCOREAPP2_0 || NETCOREAPP3_0 || NETCOREAPP3_1
#if DISABLE_DEVART
            Assert.AreEqual(263, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(359, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#else
#if DISABLE_DEVART
            Assert.AreEqual(225, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#else
            Assert.AreEqual(323, ((MightyTestsProfiler)MightyOrm.GlobalDataProfiler).DbCommandMethodCounts[DbCommandMethod.ExecuteScalar]);
#endif
#endif
#endif
        }

    }

}

namespace Mighty.AllTests.SqlProfiling
{
    [TestFixture]
    public class Profiling
    {
        [Test]
        public void AddProfiling()
        {
            MightyOrm.GlobalDataProfiler = new MightyTestsProfiler();
        }
    }
}
