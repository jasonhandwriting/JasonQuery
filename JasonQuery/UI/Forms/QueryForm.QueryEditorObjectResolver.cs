using JasonQuery.Core.Config;
using JasonQuery.Core.Data;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.Execution;
using JasonQuery.Core.Database.Metadata;
using System;
using System.Text;

namespace JasonQuery.UI.Forms
{
    public partial class QueryForm
    {
        private static class QueryEditorObjectResolver
        {
            public static void Resolve(QueryForm owner, EditorRightClickContext context)
            {
                if (owner == null)
                {
                    throw new ArgumentNullException(nameof(owner));
                }

                if (context == null || string.IsNullOrEmpty(context.Word))
                {
                    return;
                }

                switch (owner._currentSourceType)
                {
                    case DataSourceType.Oracle:
                        {
                            OracleQueryEditorObjectResolver.Resolve(owner, context);
                            break;
                        }
                    case DataSourceType.PostgreSql:
                        {
                            PostgreSqlQueryEditorObjectResolver.Resolve(owner, context);
                            break;
                        }
                    case DataSourceType.SqlServer:
                        {
                            SqlServerQueryEditorObjectResolver.Resolve(owner, context);
                            break;
                        }
                    case DataSourceType.MySql:
                        {
                            MySqlQueryEditorObjectResolver.Resolve(owner, context);
                            break;
                        }
                }
            }
        }

        private static class OracleQueryEditorObjectResolver
        {
            public static void Resolve(QueryForm owner, EditorRightClickContext context)
            {
                if (context == null || string.IsNullOrEmpty(context.Word))
                {
                    return;
                }

                var sbSql = new StringBuilder();
                var wordUpper = QueryEditorObjectResolverSqlText.ToSqlUpperInvariantLiteral(context.Word);
                var ownerName = string.IsNullOrWhiteSpace(context.SchemaNode)
                                ? DatabaseSqlExecutor.DbUserUppercase
                                : context.SchemaNode;

                var ownerUpper = QueryEditorObjectResolverSqlText.ToSqlUpperInvariantLiteral(ownerName);

                context.SchemaNode = ownerName.ToUpperInvariant();

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is View");

                sbSql.AppendLine("SELECT Object_Name AS ViewName");
                sbSql.AppendLine("  FROM All_Objects");
                sbSql.AppendLine(" WHERE Object_Type = 'VIEW'");
                sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUpper}'");
                sbSql.Append($"   AND UPPER(Object_Name) = '{wordUpper}'");

                var sql = sbSql.ToString();
                var dt = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, false);

                if (dt?.Rows.Count > 0)
                {
                    //Oracle 要先找 VIEW 再找 TABLE，如果先找 TABLE 的話，VIEW 也會被誤判為 TABLE 而反饋找到了！
                    context.SchemaType = SchemaObjectNames.Views;
                    context.SchemaName = context.Word;
                    context.IsView = true;
                    return;
                }

                sbSql.Clear();

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is Table");

                sbSql.AppendLine("SELECT Object_Name AS TableName");
                sbSql.AppendLine("  FROM All_Objects");
                sbSql.AppendLine(" WHERE Object_Type = 'TABLE'");
                sbSql.AppendLine($"   AND UPPER(Owner) = '{ownerUpper}'");
                sbSql.Append($"   AND UPPER(Object_Name) = '{wordUpper}'");

                sql = sbSql.ToString();
                dt = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, false);

                if (dt?.Rows.Count > 0)
                {
                    context.SchemaNode = ownerName.ToUpperInvariant();
                    context.SchemaType = SchemaObjectNames.Tables;
                    context.SchemaName = context.Word;
                    context.IsTable = true;
                }
            }
        }

        private static class PostgreSqlQueryEditorObjectResolver
        {
            public static void Resolve(QueryForm owner, EditorRightClickContext context)
            {
                if (context == null || string.IsNullOrEmpty(context.Word))
                {
                    return;
                }

                if (MyGlobal.ShouldSavePoint)
                {
                    var sql0 = SqlTraceHelper.BuildHeaderNewLine("---SavePoint when the right mouse button is pressed");

                    MyGlobal.PostgreSqlReader.SavePoint("jqcc1688ccqj", sql0);
                }

                var sbSql = new StringBuilder();

                //20250920 額外判斷 Table/View 前面是否有指定 Schema 字串，例如 public.tablename
                var wordPrefix2 = QueryEditorObjectResolverSqlText.ToSqlLowerInvariantLiteral(context.SchemaNode); //查到的資料，Table_Schema 的內容為小寫！
                var wordUpper = QueryEditorObjectResolverSqlText.ToSqlUpperInvariantLiteral(context.Word);

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is View");

                sbSql.AppendLine("SELECT * FROM pg_catalog.pg_views");

                if (!string.IsNullOrWhiteSpace(wordPrefix2))
                {
                    sbSql.AppendLine($" WHERE SchemaName  = '{wordPrefix2}'");
                }
                else
                {
                    sbSql.AppendLine(" WHERE SchemaName NOT IN ('pg_catalog', 'information_schema')");
                }

                sbSql.Append($"   AND UPPER(ViewName) = '{wordUpper}'");

                var sql = sbSql.ToString();
                var dt = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql, false);

                if (dt != null && dt.Rows.Count > 0)
                {
                    context.SchemaType = SchemaObjectNames.Views;
                    context.SchemaName = context.Word;
                    context.IsView = true;

                    if (dt.Rows.Count == 1)
                    {
                        context.SchemaNode = dt.Rows[0].GetSafeString("SchemaName");
                    }
                    else
                    {
                        //超過 1筆，表示沒指定 Schema，用預設 public 去尋找
                        context.SchemaNode = DataTableSearchHelper.FindValueFromDataTable(dt, "SchemaName", ("SchemaName", "public"));

                        if (string.IsNullOrWhiteSpace(context.SchemaNode))
                        {
                            context.SchemaType = string.Empty;
                            context.SchemaName = string.Empty;
                            context.IsView = false;
                        }
                    }
                }

                if (context.IsView == false)
                {
                    sbSql.Clear();

                    SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is Table");

                    sbSql.AppendLine("SELECT Table_Schema, Table_Name");
                    sbSql.AppendLine("  FROM Information_Schema.Tables");
                    sbSql.AppendLine(" WHERE Table_Type = 'BASE TABLE'");

                    if (!string.IsNullOrWhiteSpace(wordPrefix2))
                    {
                        sbSql.AppendLine($"   AND Table_Schema  = '{wordPrefix2}'");
                    }
                    else
                    {
                        sbSql.AppendLine("   AND Table_Schema NOT IN ('pg_catalog', 'information_schema')");
                    }

                    sbSql.Append($"   AND UPPER(Table_Name) = '{wordUpper}'");

                    sql = sbSql.ToString();
                    dt = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql, false);

                    if (dt != null && dt.Rows.Count > 0)
                    {
                        context.SchemaType = SchemaObjectNames.Tables;
                        context.SchemaName = context.Word;
                        context.IsTable = true;

                        if (dt.Rows.Count == 1)
                        {
                            context.SchemaNode = dt.Rows[0].GetSafeString("Table_Schema");
                        }
                        else
                        {
                            //超過 1筆，表示沒指定 Schema，以預設的 public 去尋找
                            context.SchemaNode = DataTableSearchHelper.FindValueFromDataTable(dt, "Table_Schema", ("Table_Schema", "public"));

                            if (string.IsNullOrWhiteSpace(context.SchemaNode))
                            {
                                context.SchemaType = string.Empty;
                                context.SchemaName = string.Empty;
                                context.IsTable = false;
                            }
                        }
                    }
                }

                if (MyGlobal.ShouldSavePoint)
                {
                    var sql0 = SqlTraceHelper.BuildHeaderNewLine("---ReleasePoint when the right mouse button is pressed");

                    MyGlobal.PostgreSqlReader.ReleasePoint("jqcc1688ccqj", sql0);
                }
            }
        }

        private static class SqlServerQueryEditorObjectResolver
        {
            public static void Resolve(QueryForm owner, EditorRightClickContext context)
            {
                if (context == null || string.IsNullOrEmpty(context.Word))
                {
                    return;
                }

                var effectiveDatabaseName = string.IsNullOrWhiteSpace(context.SchemaNode)
                                            ? DatabaseSqlExecutor.DatabaseName
                                            : context.SchemaNode;

                if (string.IsNullOrEmpty(effectiveDatabaseName))
                {
                    return;
                }

                var sbSql = new StringBuilder();
                var wordUpper = QueryEditorObjectResolverSqlText.ToSqlUpperInvariantLiteral(context.Word);
                var databaseName = QueryEditorObjectResolverSqlText.ToSqlLiteral(effectiveDatabaseName);

                context.SchemaNode = effectiveDatabaseName;

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is View");

                sbSql.AppendLine("SELECT SCHEMA_NAME(Schema_ID) AS Schema_Dbo");
                sbSql.AppendLine($"  FROM {databaseName}.sys.All_Objects o");
                sbSql.AppendLine(" WHERE o.Type = 'V'");
                sbSql.Append($"   AND UPPER(o.Name) = '{wordUpper}';");

                var sql = sbSql.ToString();
                var dt = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                if (dt?.Rows.Count > 0)
                {
                    context.SchemaType = SchemaObjectNames.Views;
                    context.SchemaName = context.Word;
                    context.IsView = true;
                }
                else
                {
                    sbSql.Clear();

                    SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is Table");

                    sbSql.AppendLine("SELECT o.Name AS TableName");
                    sbSql.AppendLine($"  FROM {databaseName}.sys.SysObjects o");
                    sbSql.AppendLine($"       INNER JOIN {databaseName}.sys.SysIndexes i ON o.ID = i.ID");
                    sbSql.AppendLine(" WHERE i.indid <= 1");
                    sbSql.AppendLine("   AND xtype = 'U'");
                    sbSql.Append($"   AND UPPER(o.Name) = '{wordUpper}'");

                    sql = sbSql.ToString();
                    dt = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                    if (dt?.Rows.Count > 0)
                    {
                        context.SchemaType = SchemaObjectNames.Tables;
                        context.SchemaName = context.Word;
                        context.IsTable = true;
                    }
                }

                //20230924 SQL Server 必須額外取得 Schema_Dbo, Object_ID
                if (context.IsTable || context.IsView)
                {
                    var type = context.IsTable ? "U" : "V";

                    sbSql.Clear();

                    SqlTraceHelper.AppendHeader(sbSql, "---Get SQL Server dbo Name and Object ID");

                    sbSql.AppendLine("SELECT SCHEMA_NAME(o.Schema_ID) AS Schema_Dbo, o.Object_ID");
                    sbSql.AppendLine($"  FROM {databaseName}.sys.Objects o");
                    sbSql.AppendLine($" WHERE Type = '{type}'");
                    sbSql.Append($"   AND UPPER(o.Name) = '{wordUpper}'");

                    sql = sbSql.ToString();
                    dt = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, false);

                    if (dt?.Rows.Count >= 1)
                    {
                        //20250920 額外判斷 Table/View 前面是否有指定 Schema 字串，例如 dbo.tablename
                        var schemaDbo = (context.SchemaDbo ?? string.Empty).ToLowerInvariant();

                        if (string.IsNullOrWhiteSpace(schemaDbo))
                        {
                            //取得 Schema = dbo 記錄中的 Object_ID 值
                            context.ObjectId = DataTableSearchHelper.FindValueFromDataTable(dt, "Object_ID", ("Schema_Dbo", "dbo"));
                        }
                        else
                        {
                            //取得 Schema = context.SchemaDbo 記錄中的 Object_ID 值
                            context.ObjectId = DataTableSearchHelper.FindValueFromDataTable(dt, "Object_ID", ("Schema_Dbo", schemaDbo));
                        }

                        if (string.IsNullOrEmpty(context.ObjectId))
                        {
                            context.SchemaType = string.Empty;
                            context.SchemaName = string.Empty;
                            context.IsTable = false;
                            context.IsView = false;
                            context.SchemaDbo = string.Empty;
                            context.ObjectId = string.Empty;
                        }
                        else
                        {
                            context.SchemaDbo = string.IsNullOrWhiteSpace(schemaDbo) ? "dbo" : schemaDbo;
                        }
                    }
                }
            }
        }

        private static class MySqlQueryEditorObjectResolver
        {
            public static void Resolve(QueryForm owner, EditorRightClickContext context)
            {
                if (context == null || string.IsNullOrEmpty(context.Word))
                {
                    return;
                }

                var effectiveDatabaseName = string.IsNullOrWhiteSpace(context.SchemaNode)
                                            ? DatabaseSqlExecutor.DatabaseName
                                            : context.SchemaNode;

                if (string.IsNullOrEmpty(effectiveDatabaseName))
                {
                    return;
                }

                var sbSql = new StringBuilder();
                var wordUpper = QueryEditorObjectResolverSqlText.ToSqlUpperInvariantLiteral(context.Word);
                var databaseName = QueryEditorObjectResolverSqlText.ToSqlLiteral(effectiveDatabaseName);

                context.SchemaNode = effectiveDatabaseName;

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is View");

                sbSql.AppendLine("SELECT Table_Name AS NAME");
                sbSql.AppendLine("  FROM Information_Schema.Views cc");
                sbSql.AppendLine($" WHERE Table_Schema = '{databaseName}'");
                sbSql.Append($"   AND UPPER(Table_Name) = '{wordUpper}'");

                var sql = sbSql.ToString();
                var dt = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);

                if (dt?.Rows.Count > 0)
                {
                    context.SchemaType = SchemaObjectNames.Views;
                    context.SchemaName = context.Word;
                    context.IsView = true;
                    return;
                }

                sbSql.Clear();

                SqlTraceHelper.AppendHeader(sbSql, "---When the right mouse button is pressed, determine whether the word where the cursor is located is Table");

                sbSql.AppendLine("SELECT Table_Name AS TableName");
                sbSql.AppendLine("  FROM Information_Schema.Tables");
                sbSql.AppendLine($" WHERE Table_Schema = '{databaseName}'");
                sbSql.AppendLine("   AND Table_Type = 'BASE TABLE'");
                sbSql.Append($"   AND UPPER(Table_Name) = '{wordUpper}'");

                sql = sbSql.ToString();
                dt = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, false);

                if (dt?.Rows.Count > 0)
                {
                    context.SchemaType = SchemaObjectNames.Tables;
                    context.SchemaName = context.Word;
                    context.IsTable = true;
                }
            }
        }

        private static class QueryEditorObjectResolverSqlText
        {
            public static string EscapeSqlLiteral(string value)
            {
                return (value ?? string.Empty).Replace("'", "''");
            }

            public static string ToSqlUpperInvariantLiteral(string value)
            {
                return EscapeSqlLiteral((value ?? string.Empty).ToUpperInvariant());
            }

            public static string ToSqlLowerInvariantLiteral(string value)
            {
                return EscapeSqlLiteral((value ?? string.Empty).ToLowerInvariant());
            }

            public static string ToSqlLiteral(string value)
            {
                return EscapeSqlLiteral(value ?? string.Empty);
            }
        }
    }
}
