using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.QueryEngine.Editor.AutoComplete.Resolvers
{
    internal static class QueryEditorAutoCompleteSchemaResolver
    {

        public static DataTable DetectSchemaForPeriod(DataSourceType currentSourceType, string aliasName)
        {
            var dt = new DataTable();

            dt.Columns.Add("ColumnName");
            dt.Columns.Add("DataType");
            dt.Columns.Add("BaseTableName");
            dt.Columns.Add("AllowDBNull");

            if (currentSourceType == DataSourceType.Oracle)
            {
                //暫不需處理
                return dt;
            }

            switch (currentSourceType)
            {
                case DataSourceType.PostgreSql:
                    {
                        //20250719 改寫
                        var filteredRows = DatabaseSqlExecutor.dtTableAndViews
                                                              .AsEnumerable()
                                                              .Where
                                                               (
                                                                   r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName"))
                                                                        && r.GetSafeString("SchemaNode").Equals(aliasName, StringComparison.OrdinalIgnoreCase)
                                                               )
                                                              .OrderBy
                                                               (
                                                                   r => r.GetSafeString("SchemaName")
                                                               );

                        if (filteredRows.Any())
                        {
                            dt = filteredRows
                                 .Select
                                  (
                                      r =>
                                      {
                                          var row = dt.NewRow();

                                          row["ColumnName"] = r.GetSafeString("SchemaName");
                                          row["DataType"] = r.GetSafeString("SchemaType");
                                          return row;
                                      }
                                  )
                                 .CopyToDataTable();
                        }

                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        if (aliasName.IndexOf('.') == -1) //關鍵字沒有包含「.」符號，可能是 DB (例如 MyDB) 或是 Node (例如 dbo)
                        {
                            //20250719 改寫
                            var filteredRows = DatabaseSqlExecutor.dtTableAndViews.AsEnumerable()
                                                                  .Where
                                                                   (
                                                                       r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaNode"))
                                                                            && r.GetSafeString("DB").Equals(aliasName, StringComparison.OrdinalIgnoreCase)
                                                                   )
                                                                  .GroupBy
                                                                   (
                                                                       r => r.GetSafeString("SchemaNode") //群組化，移除重複的 SchemaNode
                                                                   )
                                                                  .Select
                                                                   (
                                                                       g => g.First() //每組取第一筆
                                                                   )
                                                                  .OrderBy
                                                                   (
                                                                       r => r.GetSafeString("SchemaNode")
                                                                   );

                            if (filteredRows.Any()) //DB (例如 MyDB)
                            {
                                dt = filteredRows.Select
                                                  (
                                                      r =>
                                                      {
                                                          var row = dt.NewRow();

                                                          row["ColumnName"] = r.GetSafeString("SchemaNode");
                                                          row["DataType"] = $"[{r.GetSafeString("DB")}]";
                                                          return row;
                                                      }
                                                  )
                                                 .CopyToDataTable();
                            }
                            else //Node (例如 dbo)
                            {
                                //20250719 改寫
                                var filteredRows2 = DatabaseSqlExecutor.dtTableAndViews
                                                                       .AsEnumerable()
                                                                       .Where
                                                                        (
                                                                            r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName"))
                                                                                 && r.GetSafeString("SchemaNode").Equals(aliasName, StringComparison.OrdinalIgnoreCase)
                                                                        )
                                                                       .OrderBy
                                                                        (
                                                                            r => r.GetSafeString("SchemaName")
                                                                        );

                                if (filteredRows2.Any())
                                {
                                    dt = filteredRows2
                                        .Select
                                         (
                                             r =>
                                             {
                                                 var row = dt.NewRow();

                                                 row["ColumnName"] = r.GetSafeString("SchemaName");
                                                 row["DataType"] = r.GetSafeString("SchemaType");
                                                 return row;
                                             }
                                         )
                                        .CopyToDataTable();
                                }
                            }
                        }
                        else //DB + Node
                        {
                            //20250719 改寫
                            var filteredRows = DatabaseSqlExecutor.dtTableAndViews.AsEnumerable()
                                                                  .Where
                                                                   (
                                                                       r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName"))
                                                                            && r.GetSafeString("DBAndNode").Equals(aliasName, StringComparison.OrdinalIgnoreCase)
                                                                   )
                                                                  .OrderBy
                                                                   (
                                                                       r => r.GetSafeString("SchemaName")
                                                                   );

                            if (filteredRows.Any())
                            {
                                dt = filteredRows.Select
                                                  (
                                                       r =>
                                                       {
                                                           var row = dt.NewRow();

                                                           row["ColumnName"] = r.GetSafeString("SchemaName");
                                                           row["DataType"] = r.GetSafeString("SchemaType");
                                                           return row;
                                                       }
                                                  )
                                                 .CopyToDataTable();
                            }
                        }

                        break;
                    }
                case DataSourceType.MySql:
                    {
                        //20250719 改寫
                        var filteredRows = DatabaseSqlExecutor.dtTableAndViews.AsEnumerable()
                                                              .Where
                                                               (
                                                                   r => !string.IsNullOrWhiteSpace(r.GetSafeString("SchemaName"))
                                                                        && r.GetSafeString("DB").Equals(aliasName, StringComparison.OrdinalIgnoreCase)
                                                               )
                                                              .OrderBy
                                                               (
                                                                   r => r.GetSafeString("SchemaName")
                                                               );

                        if (filteredRows.Any())
                        {
                            dt = filteredRows.Select
                                              (
                                                  r =>
                                                  {
                                                      var row = dt.NewRow();

                                                      row["ColumnName"] = r.GetSafeString("SchemaName");
                                                      row["DataType"] = r.GetSafeString("SchemaType");
                                                      return row;
                                                  }
                                              )
                                             .CopyToDataTable();
                        }

                        break;
                    }
            }

            return dt;
        }
    }
}