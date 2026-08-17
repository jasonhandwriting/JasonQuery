using System;
using System.Data;

namespace JasonQuery.Core.Data.DataTables
{
    /// <summary>
    /// 對 DataTable 做排序
    /// </summary>
    /// <param name="dt">來源 DataTable</param>
    /// <param name="sortExpression">排序條件，例如 "SchemaName ASC, TableName DESC"</param>
    /// <returns>排序後的新 DataTable</returns>
    internal static class DataTableSortHelper
    {
        public static DataTable SortDataTable(DataTable dt, string sortExpression)
        {
            if (dt == null || dt.Rows.Count == 0 || string.IsNullOrWhiteSpace(sortExpression))
            {
                return dt;
            }

            var dv = dt.DefaultView;

            dv.Sort = sortExpression;
            return dv.ToTable();
        }
    }
}