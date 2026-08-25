using System;
using System.Data;

namespace JasonQuery.Core.Database.Metadata.MySql
{
    internal sealed class MySqlMetadataContext
    {
        /// <summary>
        /// 連線名稱，例如 UI 顯示的 Connection Name
        /// </summary>
        public string ConnectionName { get; set; } = string.Empty;

        /// <summary>
        /// 指定的單一 MySQL Database 名稱
        /// </summary>
        public string DatabaseName { get; set; } = string.Empty;

        /// <summary>
        /// 由外部先建立好的目標 Schema DataTable
        /// 例如：CreateSchemaTable(bFromSchemaBrowser)
        /// </summary>
        public DataTable SchemaTable { get; set; }

        /// <summary>
        /// Loader 執行完成後回填最終的新 dtSchema
        /// </summary>
        public DataTable ResultSchemaTable { get; set; }

        /// <summary>
        /// 統一的 metadata 查詢執行委派
        /// Loader 不直接依賴 DatabaseSqlExecutor 內部實作
        /// </summary>
        public Func<string, DataTable> ExecuteQuery { get; set; }
    }
}
