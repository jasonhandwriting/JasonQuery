namespace JasonQuery.Core.Database.CreateScript.SqlServer.Formatting
{
    internal interface ISqlServerCreateScriptFormatter
    {
        bool CanFormat(SqlServerCreateScriptFormatContext context);

        string Format(SqlServerCreateScriptFormatContext context);
    }
}
