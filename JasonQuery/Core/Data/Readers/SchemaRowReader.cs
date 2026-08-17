using JasonQuery.Core.Data.DataRows;
using System.Data;

namespace JasonQuery.Core.Data.Readers
{
    internal class SchemaRowReader
    {
        public static string GetColumnName(DataRow dr) => dr.GetSafeString("ColumnName");
        public static string GetBaseSchemaName(DataRow dr) => dr.GetSafeString("BaseSchemaName");
        public static string GetBaseTableName(DataRow dr) => dr.GetSafeString("BaseTableName");
        public static string GetDataTypeName(DataRow dr) => dr.GetSafeString("DataTypeName");
        public static string GetTypeName(DataRow dr) => dr.GetSafeString("TypeName");
        public static string GetDataType(DataRow dr) => dr.GetSafeString("DataType");
        public static int GetColumnSize(DataRow dr) => dr.GetSafeInt("ColumnSize");
        public static int GetNumericPrecision(DataRow dr) => dr.GetSafeInt("NumericPrecision");
        public static int GetNumericScale(DataRow dr) => dr.GetSafeInt("NumericScale");
        public static int GetProviderType(DataRow dr) => dr.GetSafeInt("ProviderType");
        public static string GetProviderSpecificDataType(DataRow dr) => dr.GetSafeString("ProviderSpecificDataType");
        public static string GetComment(DataRow dr) => dr.GetSafeString("Comment");
        public static bool GetIsEnum(DataRow dr) => dr.GetSafeBool("IsEnum"); //MySQL's enum
        public static bool GetIsSet(DataRow dr) => dr.GetSafeBool("IsSet"); //MySQL's set
        public static bool GetIsKey(DataRow dr) => dr.GetSafeBool("IsKey"); //PrimaryKey
        public static bool GetIsArray(DataRow dr) => dr.GetSafeBool("IsArray"); //PostgreSQL's array, like char[]
        public static bool GetAllowDBNull(DataRow dr) => dr.GetSafeBool("AllowDBNull", defaultValue: true);
    }
}