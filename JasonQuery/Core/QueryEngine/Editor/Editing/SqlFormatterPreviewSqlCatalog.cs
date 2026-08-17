using JasonLibrary.Core.Database.Enums;

namespace JasonQuery.Core.QueryEngine.Editor.Editing
{
    internal static class SqlFormatterPreviewSqlCatalog
    {
        public static string Get(DatabaseProviderKind providerKind)
        {
            switch (providerKind)
            {
                case DatabaseProviderKind.Oracle:
                    {
                        return "SELECT C.OWNER, C.TABLE_NAME, C.COLUMN_ID, C.COLUMN_NAME, C.DATA_TYPE, C.DATA_LENGTH, C.DATA_PRECISION, C.DATA_SCALE, C.NULLABLE, CC.COMMENTS FROM ALL_TAB_COLUMNS C LEFT JOIN ALL_COL_COMMENTS CC ON CC.OWNER = C.OWNER AND CC.TABLE_NAME = C.TABLE_NAME AND CC.COLUMN_NAME = C.COLUMN_NAME WHERE C.OWNER = USER AND C.TABLE_NAME = 'EMPLOYEE' ORDER BY C.COLUMN_ID";
                    }
                case DatabaseProviderKind.PostgreSql:
                    {
                        return "SELECT n.nspname AS schema_name, cls.relname AS object_name, attr.attnum AS ordinal_position, attr.attname AS column_name, pg_catalog.format_type(attr.atttypid, attr.atttypmod) AS data_type, attr.attnotnull AS not_null, pg_catalog.col_description(attr.attrelid, attr.attnum) AS column_comment FROM pg_catalog.pg_attribute attr JOIN pg_catalog.pg_class cls ON cls.oid = attr.attrelid JOIN pg_catalog.pg_namespace n ON n.oid = cls.relnamespace WHERE attr.attnum > 0 AND NOT attr.attisdropped AND cls.relkind IN ('r', 'v', 'm') AND n.nspname = 'public' ORDER BY cls.relname, attr.attnum";
                    }
                case DatabaseProviderKind.SqlServer:
                    {
                        return "SELECT sch.name AS schema_name, obj.name AS object_name, col.column_id, col.name AS column_name, typ.name AS data_type, col.max_length, col.precision, col.scale, col.is_nullable, CAST(prop.value AS nvarchar(4000)) AS column_description FROM sys.objects obj JOIN sys.schemas sch ON sch.schema_id = obj.schema_id JOIN sys.columns col ON col.object_id = obj.object_id JOIN sys.types typ ON typ.user_type_id = col.user_type_id LEFT JOIN sys.extended_properties prop ON prop.major_id = col.object_id AND prop.minor_id = col.column_id AND prop.name = N'MS_Description' WHERE obj.type IN ('U', 'V') AND obj.is_ms_shipped = 0 ORDER BY sch.name, obj.name, col.column_id";
                    }
                case DatabaseProviderKind.MySql:
                    {
                        return "SELECT c.TABLE_SCHEMA AS schema_name, c.TABLE_NAME AS object_name, t.TABLE_TYPE AS object_type, c.ORDINAL_POSITION AS ordinal_position, c.COLUMN_NAME AS column_name, c.COLUMN_TYPE AS data_type, c.CHARACTER_MAXIMUM_LENGTH AS character_maximum_length, c.NUMERIC_PRECISION AS numeric_precision, c.NUMERIC_SCALE AS numeric_scale, c.IS_NULLABLE AS is_nullable, c.COLUMN_DEFAULT AS column_default, c.COLUMN_COMMENT AS column_comment FROM information_schema.COLUMNS c JOIN information_schema.TABLES t ON t.TABLE_SCHEMA = c.TABLE_SCHEMA AND t.TABLE_NAME = c.TABLE_NAME WHERE c.TABLE_SCHEMA = DATABASE() AND t.TABLE_TYPE IN ('BASE TABLE', 'VIEW') ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION";
                    }
                case DatabaseProviderKind.Sqlite:
                    {
                        return "SELECT master.name AS object_name, master.type AS object_type, info.cid AS ordinal_position, info.name AS column_name, info.type AS data_type, info.notnull AS is_not_null, info.dflt_value AS default_value, info.pk AS primary_key_position FROM sqlite_master master JOIN pragma_table_info(master.name) info WHERE master.type IN ('table', 'view') AND master.name NOT LIKE 'sqlite_%' ORDER BY master.name, info.cid";
                    }
                default:
                    {
                        return "SELECT table_schema AS schema_name, table_name AS object_name, column_name, data_type, is_nullable FROM information_schema.columns ORDER BY table_schema, table_name, ordinal_position";
                    }
            }
        }
    }
}