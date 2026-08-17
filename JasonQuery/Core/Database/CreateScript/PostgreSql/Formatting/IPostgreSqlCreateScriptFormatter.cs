namespace JasonQuery.Core.Database.CreateScript.PostgreSql.Formatting
{
    internal interface IPostgreSqlCreateScriptFormatter
    {
        bool CanFormat(PostgreSqlCreateScriptFormatContext context);

        string Format(PostgreSqlCreateScriptFormatContext context);
    }
}