namespace Mighty
{
    /// <summary>
    /// For use where you are using the WithParams or AsProcedure variant of Execute, but you still want to get the ADO.NET rowcount back amongst your return values.
    /// e.g. outParams: new { RowCount = new RowCount() }
    /// </summary>
    public class RowCount
    {
    }
}