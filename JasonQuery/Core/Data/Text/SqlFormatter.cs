namespace JasonQuery.Core.Data.Text
{
    public static class SqlFormatter
    {
        public static string CleanSql(string sql)
        {
            if (string.IsNullOrEmpty(sql))
            {
                return string.Empty;
            }

            return sql.Trim().TrimEnd(';');
        }
    }
}