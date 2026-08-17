namespace JasonQuery.Core.Database.CreateScript.MySql.Formatting
{
    internal interface IMySqlCreateScriptFormatter
    {
        bool CanFormat(MySqlCreateScriptFormatContext context);

        string Format(MySqlCreateScriptFormatContext context);
    }
}