using C1.Win.C1TrueDBGrid;
using JasonLibrary.Core;
using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Data.Schema;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.CreateScript.MySql;
using JasonQuery.Core.Database.CreateScript.Oracle;
using JasonQuery.Core.Database.CreateScript.PostgreSql;
using JasonQuery.Core.Database.CreateScript.SqlServer;
using JasonQuery.Core.Database.Metadata;
using JasonQuery.Core.Database.Metadata.MySql;
using JasonQuery.Core.Database.Metadata.MySql.QueryEditor;
using JasonQuery.Core.Database.Metadata.Oracle;
using JasonQuery.Core.Database.Metadata.Oracle.QueryEditor;
using JasonQuery.Core.Database.Metadata.PostgreSql.QueryEditor;
using JasonQuery.Core.Database.Metadata.PostgreSql.View;
using JasonQuery.Core.Database.Metadata.SqlServer;
using JasonQuery.Core.Database.Metadata.SqlServer.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.MySql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.PostgreSql.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.QueryEditor;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.Table;
using JasonQuery.Core.Database.SqlBuilder.SqlServer.Metadata.View;
using JasonQuery.Core.Logging;
using JasonQuery.UI.LeftPanel;
using JasonQuery.UI.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace JasonQuery.Core.Database.Execution
{
    public static class DatabaseSqlExecutor
    {
        public static DataTable dtSchema;
        public static DataTable dtTableColumn4SchemaSearch; //取得所有的 Tables + Column (Search in Schema 會用到)
        public static DataTable dtTFVTP4SchemaSearch; //取得所有的 Tables + Functions + Views + Triggers + Procedures (Search in Schema 會用到)
        public static DataTable dtTableAndViews; //取得所有的 Tables + Views's Name (Query Editor AutoComplete 會用到)
        public static DataTable dtDatabaseName; //取得所有的 Database Name (Query Editor AutoComplete 會用到, for SQL Server & MySQL)

        private static readonly DatabaseConnectionContext _currentConnection = new DatabaseConnectionContext();

        public static DatabaseConnectionContext CurrentConnection
        {
            get
            {
                return _currentConnection;
            }
        }

        #region 20260805 新增資料庫訊息收集功能
        public static DatabaseServerVersionInfo ServerVersionInfo
        {
            get
            {
                return CurrentConnection.ServerVersionInfo ?? DatabaseServerVersionInfo.Unknown;
            }
        }

        public static string DatabaseVersionDisplayText
        {
            get
            {
                return ServerVersionInfo.DisplayText;
            }
        }

        public static string DatabaseVersionDiagnosticText
        {
            get
            {
                return ServerVersionInfo.DiagnosticText;
            }
        }

        public static void SetDatabaseServerVersion(DataSourceType dataSourceType, string rawVersion)
        {
            CurrentConnection.ServerVersionInfo = DatabaseServerVersionInfo.Create(dataSourceType, rawVersion);

            if (dataSourceType == DataSourceType.SqlServer)
            {
                var sqlServerVersion = SqlServerVersionInfo.Parse(rawVersion);

                CurrentConnection.SqlServerVersion = sqlServerVersion;
                CurrentConnection.DbServerVersion = sqlServerVersion.ProductVersion;
            }
        }
        #endregion

        #region 20260414~0417 Compatibility Properties
        private static DataSourceType _currentDataSource = DataSourceType.None;
        private static string _dataSourceDisplayName = string.Empty;

        public static DataSourceType CurrentDataSource
        {
            get
            {
                return _currentDataSource;
            }
            set
            {
                _currentDataSource = value;
                _dataSourceDisplayName = DataSourceTypeHelper.ToDisplayName(value);
            }
        }

        public static string DataSourceDisplayName
        {
            get
            {
                return _dataSourceDisplayName;
            }
            set
            {
                _dataSourceDisplayName = value ?? string.Empty;
            }
        }

        private static DataSourceType GetEffectiveDataSourceType()
        {
            return CurrentDataSource;
        }

        public static string DbUser
        {
            get
            {
                return CurrentConnection.DbUser;
            }
            set
            {
                CurrentConnection.DbUser = value;
            }
        }

        public static string DbUserUppercase
        {
            get
            {
                return CurrentConnection.DbUserUppercase;
            }
        }

        public static string DbPassword
        {
            get
            {
                return CurrentConnection.DbPassword;
            }
            set
            {
                CurrentConnection.DbPassword = value;
            }
        }

        public static string DbServerVersion
        {
            get
            {
                return CurrentConnection.DbServerVersion;
            }
            set
            {
                CurrentConnection.DbServerVersion = value;
            }
        }

        public static SqlServerVersionInfo SqlServerVersion
        {
            get
            {
                return CurrentConnection.SqlServerVersion ?? SqlServerVersionInfo.Unknown;
            }
        }

        public static void SetSqlServerVersion(string productVersion)
        {
            SetDatabaseServerVersion(DataSourceType.SqlServer, productVersion);
        }

        public static void ClearDbServerVersion()
        {
            CurrentConnection.DbServerVersion = string.Empty;
            CurrentConnection.SqlServerVersion = SqlServerVersionInfo.Unknown;
            CurrentConnection.ServerVersionInfo = DatabaseServerVersionInfo.Unknown;
        }

        public static string OracleSid
        {
            get
            {
                return CurrentConnection.OracleSid;
            }
            set
            {
                CurrentConnection.OracleSid = value;
            }
        }

        public static string OracleConnectAs
        {
            get
            {
                return CurrentConnection.OracleConnectAs;
            }
            set
            {
                CurrentConnection.OracleConnectAs = value;
            }
        }

        public static string DatabaseName
        {
            get
            {
                return CurrentConnection.DatabaseName;
            }
            set
            {
                CurrentConnection.DatabaseName = value;
            }
        }

        public static int DbConnectionPort
        {
            get
            {
                return CurrentConnection.DbConnectionPort;
            }
            set
            {
                CurrentConnection.DbConnectionPort = value;
            }
        }

        public static string DbConnectionString
        {
            get
            {
                return CurrentConnection.DbConnectionString;
            }
            set
            {
                CurrentConnection.DbConnectionString = value;
            }
        }

        public static string DbConnectionTitle
        {
            get
            {
                return CurrentConnection.DbConnectionTitle;
            }
            set
            {
                CurrentConnection.DbConnectionTitle = value;
            }
        }

        public static string DbConnectionName
        {
            get
            {
                return CurrentConnection.DbConnectionName;
            }
            set
            {
                CurrentConnection.DbConnectionName = value;
            }
        }

        public static string DbConnectionServer
        {
            get
            {
                return CurrentConnection.DbConnectionServer;
            }
            set
            {
                CurrentConnection.DbConnectionServer = value;
            }
        }

        public static bool UseAutoRollback
        {
            get
            {
                return CurrentConnection.Options.UseAutoRollback;
            }
            set
            {
                CurrentConnection.Options.UseAutoRollback = value;
            }
        }

        public static bool UseDirectMode
        {
            get
            {
                return CurrentConnection.Options.UseDirectMode;
            }
            set
            {
                CurrentConnection.Options.UseDirectMode = value;
            }
        }

        public static bool UseConnectionPooling
        {
            get
            {
                return CurrentConnection.Options.UseConnectionPooling;
            }
            set
            {
                CurrentConnection.Options.UseConnectionPooling = value;
            }
        }

        public static bool ExcludeNativeDatabase
        {
            get
            {
                return CurrentConnection.Options.ExcludeNativeDatabase;
            }
            set
            {
                CurrentConnection.Options.ExcludeNativeDatabase = value;
            }
        }

        public static bool UseUnicode
        {
            get
            {
                return CurrentConnection.Options.UseUnicode;
            }
            set
            {
                CurrentConnection.Options.UseUnicode = value;
            }
        }

        public static int QueryTimeoutSeconds
        {
            get
            {
                return CurrentConnection.Options.QueryTimeoutSeconds;
            }
            set
            {
                CurrentConnection.Options.QueryTimeoutSeconds = value;
            }
        }
        #endregion

        public static void ResetCurrentConnection()
        {
            CurrentConnection.Reset();
        }

        public static string GetCreateScript_Oracle(string schemaType, string schemaName, string packageSpecBody = "", string owner = "")
        {
            var oracleReader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

            oracleReader.EnsureConnectionOpen();

            return OracleCreateScriptDispatcher.Build
            (
                schemaType,
                schemaName,
                packageSpecBody,
                owner,
                DbUserUppercase,
                ExecuteMetadataQuery
            );
        }

        public static string GetCreateScript_PostgreSql(string schemaNode, string schemaType, string schemaName)
        {
            var postgreSqlReader = MyGlobal.PostgreSqlReader ?? throw new InvalidOperationException("PostgreSqlReader has not been initialized.");

            postgreSqlReader.EnsureConnectionOpen();

            return PostgreSqlCreateScriptDispatcher.Build
            (
                schemaNode,
                schemaType,
                schemaName,
                DatabaseName,
                DbServerVersion,
                ExecuteMetadataQuery
            );
        }

        public static string GetCreateScript_SqlServer(string schemaType, string schemaNode, string schemaDbo, string schemaName, string objectID = "")
        {
            var sqlServerReader = MyGlobal.SqlServerReader ?? throw new InvalidOperationException("SqlServerReader has not been initialized.");

            sqlServerReader.EnsureConnectionOpen();

            var request = SqlServerCreateScriptRequest.Create
            (
                schemaType,
                schemaNode,
                schemaDbo,
                schemaName,
                objectID,
                ExcludeNativeDatabase,
                $"{MyLibrary.DateFormat} HH:mm:ss",
                SqlServerVersion.MajorVersion
            );

            return SqlServerCreateScriptDispatcher.Build
            (
                request,
                ExecuteMetadataQuery
            );
        }

        public static string GetCreateScript_MySql(string schemaType, string schemaNode, string schemaName)
        {
            var mySqlReader = MyGlobal.MySqlReader ?? throw new InvalidOperationException("MySqlReader has not been initialized.");

            mySqlReader.EnsureConnectionOpen();

            var request = MySqlCreateScriptRequest.Create(schemaType, schemaNode, schemaName);

            return MySqlCreateScriptDispatcher.Build
            (
                request,
                ExecuteMetadataQuery
            );
        }

        public static string GetDataTypeFormat_PostgreSql(DataRow row, out string dataType)
        {
            dataType = string.Empty;

            var schema = string.Empty;
            var dataTypeResult = string.Empty;

            if (row == null)
            {
                return string.Empty;
            }

            var type = row.GetSafeString("ProviderType");
            var systemDataType = row.GetSafeString("DataType");
            var columnSize = row.GetSafeInt("ColumnSize");
            var numericScale = row.GetSafeInt("NumericScale");
            var numericPrecision = row.GetSafeInt("NumericPrecision");
            var providerSpecificDataType = row.GetSafeString("ProviderSpecificDataType");
            var columnSizeBrackets = columnSize > 0 ? $"({columnSize})" : string.Empty;

            switch (type)
            {
                case "16":
                    {
                        dataTypeResult = "boolean";
                        schema = "boolean";
                        break;
                    }
                case "1000":
                    {
                        dataTypeResult = "string";
                        schema = "boolean[]";
                        break;
                    }
                case "17":
                    {
                        dataTypeResult = "string";
                        schema = "bytea";
                        break;
                    }
                case "1001":
                    {
                        dataTypeResult = "string";
                        schema = "bytea[]";
                        break;
                    }
                case "20": //bigint, bigserial
                    {
                        dataTypeResult = "int";
                        schema = "bigint";
                        break;
                    }
                case "1016":
                    {
                        dataTypeResult = "int";
                        schema = "bigint[]";
                        break;
                    }
                case "21": //SmallInt (int16), smallserial
                    {
                        dataTypeResult = "int";
                        schema = "smallint";
                        break;
                    }
                case "1005": //SmallInt (int16)
                    {
                        dataTypeResult = "int";
                        schema = "smallint[]";
                        break;
                    }
                case "23": //signed four-byte integer (int32), oid, serial
                    {
                        dataTypeResult = "int";
                        schema = "integer";
                        break;
                    }
                case "25":
                    {
                        dataTypeResult = "string";
                        schema = "text";
                        break;
                    }
                case "1009":
                    {
                        dataTypeResult = "string";
                        schema = "text[]";
                        break;
                    }
                case "700":
                    {
                        dataTypeResult = "number";
                        schema = "real";
                        break;
                    }
                case "1021":
                    {
                        dataTypeResult = "number";
                        schema = "real[]";
                        break;
                    }
                case "701":
                    {
                        dataTypeResult = "number";
                        schema = "double precision";
                        break;
                    }
                case "1022":
                    {
                        dataTypeResult = "number";
                        schema = "double precision[]";
                        break;
                    }
                case "1002":
                    {
                        dataTypeResult = "string";
                        schema = "\"char\"[]";
                        break;
                    }
                case "1007":
                    {
                        dataTypeResult = "int";
                        schema = "integer[]";
                        break;
                    }
                case "1034":
                    {
                        dataTypeResult = "string";
                        schema = "aclitem[]";
                        break;
                    }
                case "1042": //character
                    {
                        if (columnSize == -1)
                        {
                            schema = "character";
                        }
                        else
                        {
                            schema = $"character({columnSize})";
                        }

                        dataTypeResult = "string";
                        break;
                    }
                case "1014": //character[]
                    {
                        if (columnSize == -1)
                        {
                            schema = "character[]";
                        }
                        else
                        {
                            schema = $"character({columnSize})[]";
                        }

                        dataTypeResult = "string";
                        break;
                    }
                case "1015": //character varying 或是「使用者自定字串」, ex:'e_name' as "EngName"
                case "1043": //string array, xid, xid8, cid, gtsvector, jsonpath, name, pg_lsn, pg_dependencies, pg_brin_minmax_multi_summary, pg_brin_bloom_summary, pg_mcv_list, pg_ndistinct, pg_node_tree, pg_snapshot, regclass, regcollation, regconfig, regdictionary, regnamespace, regoper, regoperator, regproc, regprocedure, regrole, regtype, tid, tsquery, tsvector, txid_snapshot
                    {
                        if (columnSize == -1)
                        {
                            schema = "character varying";
                        }
                        else
                        {
                            schema = $"character varying({columnSize})";
                        }

                        schema += type == "1015" ? "[]" : string.Empty;
                        dataTypeResult = "string";
                        break;
                    }
                case "1012":
                    {
                        dataTypeResult = "string";
                        schema = "cid[]";
                        break;
                    }
                case "1028":
                    {
                        dataTypeResult = "string";
                        schema = "oid[]";
                        break;
                    }
                case "10001":
                    {
                        dataTypeResult = "string";
                        schema = "oidvector";
                        break;
                    }
                case "1231":
                    {
                        if (numericScale == 0)
                        {
                            schema = "numeric";
                        }
                        else
                        {
                            //有小數點
                            schema = $"numeric({numericPrecision},{numericScale})";
                        }

                        dataTypeResult = "number";
                        schema = $"{schema}[]";
                        break;
                    }
                case "1700":
                    {
                        if (numericScale == 0)
                        {
                            dataTypeResult = "int";

                            if (numericPrecision == 0)
                            {
                                schema = "numeric";
                            }
                            else
                            {
                                schema = $"numeric({numericPrecision})";
                            }
                        }
                        else
                        {
                            dataTypeResult = "number"; //有小數點
                            schema = $"numeric({numericPrecision},{numericScale})";
                        }

                        schema += type == "1231" ? "[]" : string.Empty;
                        break;
                    }
                case "1082":
                    {
                        dataTypeResult = "datetime";
                        schema = "date";
                        break;
                    }
                case "1182":
                    {
                        dataTypeResult = "string";
                        schema = "date[]";
                        break;
                    }
                case "1266":
                    {
                        dataTypeResult = "datetime";

                        if (numericPrecision > 0)
                        {
                            schema = $"time with time zone({numericPrecision})";
                        }
                        else
                        {
                            schema = "time with time zone";
                        }

                        break;
                    }
                case "1270":
                    {
                        dataTypeResult = "string";

                        if (numericPrecision > 0)
                        {
                            schema = $"time with time zone[]({numericPrecision})";
                        }
                        else
                        {
                            schema = "time with time zone[]";
                        }

                        break;
                    }
                case "1083":
                    {
                        dataTypeResult = "datetime";

                        if (numericPrecision > 0)
                        {
                            schema = $"time without time zone({numericPrecision})";
                        }
                        else
                        {
                            schema = "time without time zone";
                        }

                        break;
                    }
                case "1183":
                    {
                        dataTypeResult = "string";

                        if (numericPrecision > 0)
                        {
                            schema = $"time without time zone[]({numericPrecision})";
                        }
                        else
                        {
                            schema = "time without time zone[]";
                        }

                        break;
                    }
                case "1184":
                    {
                        dataTypeResult = "datetime";

                        if (numericPrecision > 0)
                        {
                            schema = $"timestamp with time zone({numericPrecision})";
                        }
                        else
                        {
                            schema = "timestamp with time zone";
                        }

                        break;
                    }
                case "1185":
                    {
                        dataTypeResult = "string";

                        if (numericPrecision > 0)
                        {
                            schema = $"timestamp with time zone[]({numericPrecision})";
                        }
                        else
                        {
                            schema = "timestamp with time zone[]";
                        }

                        break;
                    }
                case "1114":
                    {
                        dataTypeResult = "datetime";

                        if (numericPrecision > 0)
                        {
                            schema = $"timestamp without time zone({numericPrecision})";
                        }
                        else
                        {
                            schema = "timestamp without time zone";
                        }

                        break;
                    }
                case "1115":
                    {
                        dataTypeResult = "string";

                        if (numericPrecision > 0)
                        {
                            schema = $"timestamp without time zone[]({numericPrecision})";
                        }
                        else
                        {
                            schema = "timestamp without time zone[]";
                        }

                        break;
                    }
                case "1560":
                    {
                        dataTypeResult = "string";
                        schema = $"bit{columnSizeBrackets}";
                        break;
                    }
                case "1561": //20240810 陣列取不到 ColumnSize
                    {
                        dataTypeResult = "string";
                        schema = $"bit[]{columnSizeBrackets}";
                        break;
                    }
                case "1562":
                    {
                        dataTypeResult = "string";
                        schema = $"bit varying{columnSizeBrackets}";
                        break;
                    }
                case "1563": //20240810 陣列取不到 ColumnSize
                    {
                        dataTypeResult = "string";
                        schema = $"bit varying[]{columnSizeBrackets}";
                        break;
                    }
                case "2950":
                    {
                        dataTypeResult = "string";
                        schema = "uuid";
                        break;
                    }
                case "2951":
                    {
                        dataTypeResult = "string";
                        schema = "uuid[]";
                        break;
                    }
                case "142":
                    {
                        dataTypeResult = "string";
                        schema = "xml";
                        break;
                    }
                case "143":
                    {
                        dataTypeResult = "string";
                        schema = "xml[]";
                        break;
                    }
                case "1011":
                    {
                        dataTypeResult = "string";
                        schema = "xid[]";
                        break;
                    }
                case "271":
                    {
                        dataTypeResult = "string";
                        schema = "xid8[]";
                        break;
                    }
                case "603":
                    {
                        dataTypeResult = "string";
                        schema = "box";
                        break;
                    }
                case "1020":
                    {
                        dataTypeResult = "string";
                        schema = "box[]";
                        break;
                    }
                case "650":
                    {
                        dataTypeResult = "string";
                        schema = "cidr";
                        break;
                    }
                case "651":
                    {
                        dataTypeResult = "string";
                        schema = "cidr[]";
                        break;
                    }
                case "718":
                    {
                        dataTypeResult = "string";
                        schema = "circle";
                        break;
                    }
                case "719":
                    {
                        dataTypeResult = "string";
                        schema = "circle[]";
                        break;
                    }
                case "3912":
                    {
                        dataTypeResult = "string";
                        schema = "daterange";
                        break;
                    }
                case "3913":
                    {
                        dataTypeResult = "string";
                        schema = "daterange[]";
                        break;
                    }
                case "4535":
                    {
                        dataTypeResult = "string";
                        schema = "datemultirange";
                        break;
                    }
                case "6155":
                    {
                        dataTypeResult = "string";
                        schema = "datemultirange[]";
                        break;
                    }
                case "3644":
                    {
                        dataTypeResult = "string";
                        schema = "gtsvector[]";
                        break;
                    }
                case "869":
                    {
                        dataTypeResult = "string";
                        schema = "inet";
                        break;
                    }
                case "1041":
                    {
                        dataTypeResult = "string";
                        schema = "inet[]";
                        break;
                    }
                case "22":
                    {
                        dataTypeResult = "string";
                        schema = "int2vector";
                        break;
                    }
                case "1006":
                    {
                        dataTypeResult = "string";
                        schema = "int2vector[]";
                        break;
                    }
                case "4451":
                    {
                        dataTypeResult = "string";
                        schema = "int4multirange";
                        break;
                    }
                case "6150":
                    {
                        dataTypeResult = "string";
                        schema = "int4multirange[]";
                        break;
                    }
                case "4536":
                    {
                        dataTypeResult = "string";
                        schema = "int8multirange";
                        break;
                    }
                case "6157":
                    {
                        dataTypeResult = "string";
                        schema = "int8multirange[]";
                        break;
                    }
                case "3904":
                    {
                        dataTypeResult = "string";
                        schema = "int4range";
                        break;
                    }
                case "3905":
                    {
                        dataTypeResult = "string";
                        schema = "int4range[]";
                        break;
                    }
                case "3926":
                    {
                        dataTypeResult = "string";
                        schema = "int8range";
                        break;
                    }
                case "3927":
                    {
                        dataTypeResult = "string";
                        schema = "int8range[]";
                        break;
                    }
                case "1186":
                    {
                        dataTypeResult = "string";

                        var sText = numericPrecision > 0 ? $"({numericPrecision})" : string.Empty;

                        schema = $"interval{sText}";
                        break;
                    }
                case "1187":
                    {
                        dataTypeResult = "string";
                        schema = "interval[]";
                        break;
                    }
                case "114":
                    {
                        dataTypeResult = "string";
                        schema = "json";
                        break;
                    }
                case "199":
                    {
                        dataTypeResult = "string";
                        schema = "json[]";
                        break;
                    }
                case "3802":
                    {
                        dataTypeResult = "string";
                        schema = "jsonb";
                        break;
                    }
                case "3807":
                    {
                        dataTypeResult = "string";
                        schema = "jsonb[]";
                        break;
                    }
                case "4073":
                    {
                        dataTypeResult = "string";
                        schema = "jsonpath[]";
                        break;
                    }
                case "628":
                    {
                        dataTypeResult = "string";
                        schema = "line";
                        break;
                    }
                case "629":
                    {
                        dataTypeResult = "string";
                        schema = "line[]";
                        break;
                    }
                case "601":
                    {
                        dataTypeResult = "string";
                        schema = "lseg";
                        break;
                    }
                case "1018":
                    {
                        dataTypeResult = "string";
                        schema = "lseg[]";
                        break;
                    }
                case "829":
                    {
                        dataTypeResult = "string";
                        schema = "macaddr";
                        break;
                    }
                case "1040":
                    {
                        dataTypeResult = "string";
                        schema = "macaddr[]";
                        break;
                    }
                case "774":
                    {
                        dataTypeResult = "string";
                        schema = "macaddr8";
                        break;
                    }
                case "775":
                    {
                        dataTypeResult = "string";
                        schema = "macaddr8[]";
                        break;
                    }
                case "790":
                    {
                        dataTypeResult = "string";
                        schema = "money";
                        break;
                    }
                case "791":
                    {
                        dataTypeResult = "string";
                        schema = "money[]";
                        break;
                    }
                case "1003":
                    {
                        dataTypeResult = "string";
                        schema = "name[]";
                        break;
                    }
                case "4532":
                    {
                        dataTypeResult = "string";
                        schema = "nummultirange";
                        break;
                    }
                case "6151":
                    {
                        dataTypeResult = "string";
                        schema = "nummultirange[]";
                        break;
                    }
                case "3906":
                    {
                        dataTypeResult = "string";
                        schema = "numrange";
                        break;
                    }
                case "3907":
                    {
                        dataTypeResult = "string";
                        schema = "numrange[]";
                        break;
                    }
                case "1013":
                    {
                        dataTypeResult = "string";
                        schema = "oidvector[]";
                        break;
                    }
                case "3221":
                    {
                        dataTypeResult = "string";
                        schema = "pg_lsn[]";
                        break;
                    }
                case "602":
                    {
                        dataTypeResult = "string";
                        schema = "path";
                        break;
                    }
                case "1019":
                    {
                        dataTypeResult = "string";
                        schema = "path[]";
                        break;
                    }
                case "5039":
                    {
                        dataTypeResult = "string";
                        schema = "pg_snapshot[]";
                        break;
                    }
                case "600":
                    {
                        dataTypeResult = "string";
                        schema = "point";
                        break;
                    }
                case "1017":
                    {
                        dataTypeResult = "string";
                        schema = "point[]";
                        break;
                    }
                case "604":
                    {
                        dataTypeResult = "string";
                        schema = "polygon";
                        break;
                    }
                case "1027":
                    {
                        dataTypeResult = "string";
                        schema = "polygon[]";
                        break;
                    }
                case "1790":
                    {
                        dataTypeResult = "string";
                        schema = "refcursor";
                        break;
                    }
                case "2201":
                    {
                        dataTypeResult = "string";
                        schema = "refcursor[]";
                        break;
                    }
                case "2210":
                    {
                        dataTypeResult = "string";
                        schema = "regclass[]";
                        break;
                    }
                case "4192":
                    {
                        dataTypeResult = "string";
                        schema = "regcollation[]";
                        break;
                    }
                case "3735":
                    {
                        dataTypeResult = "string";
                        schema = "regconfig[]";
                        break;
                    }
                case "3770":
                    {
                        dataTypeResult = "string";
                        schema = "regdictionary[]";
                        break;
                    }
                case "4090":
                    {
                        dataTypeResult = "string";
                        schema = "regnamespace[]";
                        break;
                    }
                case "2208":
                    {
                        dataTypeResult = "string";
                        schema = "regoper[]";
                        break;
                    }
                case "2209":
                    {
                        dataTypeResult = "string";
                        schema = "regoperator[]";
                        break;
                    }
                case "1008":
                    {
                        dataTypeResult = "string";
                        schema = "regproc[]";
                        break;
                    }
                case "2207":
                    {
                        dataTypeResult = "string";
                        schema = "regprocedure[]";
                        break;
                    }
                case "4097":
                    {
                        dataTypeResult = "string";
                        schema = "regrole[]";
                        break;
                    }
                case "2211":
                    {
                        dataTypeResult = "string";
                        schema = "regtype[]";
                        break;
                    }
                case "1010":
                    {
                        dataTypeResult = "string";
                        schema = "tid[]";
                        break;
                    }
                case "3645":
                    {
                        dataTypeResult = "string";
                        schema = "tsquery[]";
                        break;
                    }
                case "3908":
                    {
                        dataTypeResult = "string";
                        schema = "tsrange";
                        break;
                    }
                case "3909":
                    {
                        dataTypeResult = "string";
                        schema = "tsrange[]";
                        break;
                    }
                case "3910":
                    {
                        dataTypeResult = "string";
                        schema = "tstzrange";
                        break;
                    }
                case "3911":
                    {
                        dataTypeResult = "string";
                        schema = "tstzrange[]";
                        break;
                    }
                case "4534":
                    {
                        dataTypeResult = "string";
                        schema = "tstzmultirange";
                        break;
                    }
                case "6153":
                    {
                        dataTypeResult = "string";
                        schema = "tstzmultirange[]";
                        break;
                    }
                case "3643":
                    {
                        dataTypeResult = "string";
                        schema = "tsvector[]";
                        break;
                    }
                case "2949":
                    {
                        dataTypeResult = "string";
                        schema = "txid_snapshot[]";
                        break;
                    }
                case "4533":
                    {
                        dataTypeResult = "string";
                        schema = "tsmultirange";
                        break;
                    }
                case "6152":
                    {
                        dataTypeResult = "string";
                        schema = "tsmultirange[]";
                        break;
                    }
                default:
                    {
                        dataTypeResult = "string";
                        schema = $"{systemDataType}({type})";
                        break;
                    }
            }

            dataType = dataTypeResult;
            return schema;
        }

        public static void GetColumnInfoAndPKInfo_Oracle(string schemaType, string schemaName, ref DataTable dtColumnName, ref string pk, ref string columnInfoSql)
        {
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                pk = LoadOraclePrimaryKeyText(schemaName);
            }

            columnInfoSql = OracleTableColumnInfoSqlBuilder.BuildColumnInfo(schemaName);

            ExecuteQueryToDataTable(columnInfoSql, ref dtColumnName);
        }

        private static string LoadOraclePrimaryKeyText(string schemaName)
        {
            var pkSql = OracleTableColumnInfoSqlBuilder.BuildPrimaryKeyInfo(schemaName, DbUserUppercase);

            return LoadPrimaryKeyText(pkSql);
        }

        private static string LoadPrimaryKeyText(string pkSql)
        {
            var dtPk = new DataTable();

            ExecuteQueryToDataTable(pkSql, ref dtPk);
            return BuildPrimaryKeyText(dtPk);
        }

        private static string BuildPrimaryKeyText(DataTable dtPk)
        {
            if (dtPk?.Rows.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder("`");

            sb.Append(string.Join("`", dtPk.AsEnumerable().Select(dr => dr.Field<string>("Column_Name") ?? string.Empty)));
            return sb.ToString();
        }

        public static void GetColumnInfoAndPKInfo_PostgreSql(string schemaNode, string schemaType, string schemaName, ref DataTable dtColumnName, ref string pk, ref string columnInfoSql)
        {
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                pk = LoadPostgreSqlPrimaryKeyText(schemaNode, schemaName);

                columnInfoSql = PostgreSqlTableColumnInfoSqlBuilder.BuildTableColumnInfo(schemaNode, schemaName);

                ExecuteQueryToDataTable(columnInfoSql, ref dtColumnName);
                return;
            }

            var sql = PostgreSqlTableColumnInfoSqlBuilder.BuildViewColumnInfoProbeSql(schemaNode, schemaName);
            var dtSchema = new DataTable();
            var dtData = MyGlobal.PostgreSqlReader.ExecuteQueryPaged100Rows(sql, 0, 0, out bool isRollback, out bool isPermissionDenied, out string errorMessage, out var errCode, out dtSchema);
            var columnInfoCollector = SchemaColumnInfoBuilder.Build(CurrentDataSource, dtSchema);

            if (dtData != null && string.IsNullOrEmpty(errorMessage))
            {
                dtColumnName = PostgreSqlViewColumnInfoTableBuilder.Build(dtSchema, columnInfoCollector);
            }
        }

        private static string LoadPostgreSqlPrimaryKeyText(string schemaNode, string schemaName)
        {
            var pkSql = PostgreSqlTableColumnInfoSqlBuilder.BuildPrimaryKeyInfo(schemaNode, schemaName);

            return LoadPrimaryKeyText(pkSql);
        }

        public static void GetColumnInfoAndPKInfo_SqlServer(string schemaNode, string schemaType, string schemaName, string schemaDbo, string objectId, ref DataTable dtColumnName, ref string pk, ref string columnInfoSql)
        {
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                pk = LoadSqlServerPrimaryKeyText(schemaNode, schemaDbo, schemaName);

                columnInfoSql = SqlServerTableColumnInfoSqlBuilder.BuildTableColumnInfo(schemaNode, schemaDbo, schemaName);

                ExecuteQueryToDataTable(columnInfoSql, ref dtColumnName);
                return;
            }

            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Views))
            {
                var resolvedObjectId = ResolveSqlServerViewObjectId(schemaNode, schemaDbo, schemaName, objectId);

                if (!int.TryParse(resolvedObjectId, out var viewObjectId) || viewObjectId <= 0)
                {
                    throw new InvalidOperationException($"Could not resolve a valid SQL Server View ObjectId for '{schemaDbo}.{schemaName}'.");
                }

                columnInfoSql = SqlServerViewMetadataSqlBuilder.BuildViewColumnInfo(schemaNode, schemaDbo, viewObjectId.ToString());

                ExecuteQueryToDataTable(columnInfoSql, ref dtColumnName);
                return;
            }
        }

        private static string LoadSqlServerPrimaryKeyText(string schemaNode, string schemaDbo, string schemaName)
        {
            var pkSql = SqlServerTableColumnInfoSqlBuilder.BuildPrimaryKeyInfo(schemaNode, schemaDbo, schemaName);

            return LoadPrimaryKeyText(pkSql);
        }

        private static string ResolveSqlServerViewObjectId(string schemaNode, string schemaDbo, string schemaName, string objectId)
        {
            if (!string.IsNullOrWhiteSpace(objectId))
            {
                return objectId;
            }

            var dtObjectId = new DataTable();
            var sqlViewObjectId = SqlServerViewMetadataSqlBuilder.BuildViewObjectId(schemaNode, schemaDbo, schemaName);

            ExecuteQueryToDataTable(sqlViewObjectId, ref dtObjectId);

            if (dtObjectId?.Rows.Count > 0)
            {
                return dtObjectId.Rows[0].GetSafeString("Object_ID");
            }

            return string.Empty;
        }

        public static void GetColumnInfoAndPKInfo_MySql(string schemaNode, string schemaType, string schemaName, ref DataTable dtColumnName, ref string pk, ref string columnInfoSql)
        {
            if (SchemaObjectTypeHelper.Is(schemaType, SchemaObjectNames.Tables))
            {
                pk = LoadMySqlPrimaryKeyText(schemaNode, schemaName);
            }

            columnInfoSql = MySqlTableColumnInfoSqlBuilder.BuildColumnInfo(schemaNode, schemaName);

            ExecuteQueryToDataTable(columnInfoSql, ref dtColumnName);
        }

        private static string LoadMySqlPrimaryKeyText(string schemaNode, string schemaName)
        {
            var pkSql = MySqlTableColumnInfoSqlBuilder.BuildPrimaryKeyInfo(schemaNode, schemaName);

            return LoadPrimaryKeyText(pkSql);
        }

        public static void ExecuteQueryToDataTable(string sql, ref DataTable dt, bool showAlertOnError = true)
        {
            switch (GetEffectiveDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        dt = MyGlobal.OracleReader.ExecuteQueryToDataTable(sql, showAlertOnError);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        dt = MyGlobal.PostgreSqlReader.ExecuteQueryToDataTable(sql, showAlertOnError);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        dt = MyGlobal.SqlServerReader.ExecuteQueryToDataTable(sql, showAlertOnError);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        dt = MyGlobal.MySqlReader.ExecuteQueryToDataTable(sql, showAlertOnError);
                        break;
                    }
                default:
                    {
                        dt = new DataTable();
                        break;
                    }
            }
        }

        private static OracleMetadataContext CreateOracleMetadataContext(DataTable dtTargetSchemaTable, bool isFromSchemaBrowser)
        {
            return new OracleMetadataContext
            {
                TargetSchemaTable = dtTargetSchemaTable,
                DbConnectionName = DbConnectionName,
                DbUserNameUppercase = DbUserUppercase,

                NeedSchemaRows = MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser),

                SortByColumnName = MyGlobal.IsSortByColumnName,

                ExecuteQuery = ExecuteMetadataQuery
            };
        }

        private static OracleQueryEditorMetadataLoadOptions CreateOracleMetadataLoadOptions()
        {
            return new OracleQueryEditorMetadataLoadOptions
            {
                EnableTableAutoComplete = MyLibrary.EnableAutoComplete && MyLibrary.AutoCompleteUserDefinedTables,
                EnableViewAutoComplete = MyLibrary.EnableAutoComplete && MyLibrary.AutoCompleteUserDefinedViews,
                EnableTriggerAutoComplete = MyLibrary.EnableAutoComplete && MyLibrary.AutoCompleteUserDefinedTriggers,
                EnableFunctionAutoComplete = MyLibrary.EnableAutoComplete && MyLibrary.AutoCompleteUserDefinedFunctions
            };
        }

        private static DataTable ExecuteMetadataQuery(string sql)
        {
            DataTable dtTemp = null;

            ExecuteQueryToDataTable(sql, ref dtTemp);
            return dtTemp;
        }

        private static DataTable CreateAutoCompleteLookupTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("DB"); //Database
            dt.Columns.Add("DBAndNode"); //Database+Node
            dt.Columns.Add("SchemaNode");
            dt.Columns.Add("SchemaName");
            dt.Columns.Add("SchemaType");
            dt.Columns.Add("Memo");

            return dt;
        }

        private static DataTable CreateAutoCompleteForAllTable()
        {
            var dt = new DataTable();

            dt.Columns.Add("ObjectName");
            dt.Columns.Add("ObjectSource");

            return dt;
        }

        private static DataTable CreateSchemaTable(bool isFromSchemaBrowser = true)
        {
            var dtNewSchema = new DataTable();
            var effectiveDataSourceType = GetEffectiveDataSourceType();

            dtNewSchema.Columns.Add("SchemaObject");

            if (effectiveDataSourceType == DataSourceType.PostgreSql || effectiveDataSourceType == DataSourceType.SqlServer || effectiveDataSourceType == DataSourceType.MySql)
            {
                dtNewSchema.Columns.Add("SchemaNode");
            }

            dtNewSchema.Columns.Add("SchemaType");
            dtNewSchema.Columns.Add("SchemaName");

            if (MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser))
            {
                dtNewSchema.Columns.Add("ColumnInfo");
            }

            if (effectiveDataSourceType == DataSourceType.Oracle)
            {
                //Internal metadata provenance only. Never derive DDL authorization from this field alone.
                dtNewSchema.Columns.Add("OracleMetadataOwner");
            }

            switch (effectiveDataSourceType)
            {
                case DataSourceType.SqlServer:
                    {
                        dtNewSchema.Columns.Add("SchemaDbo");
                        dtNewSchema.Columns.Add("ObjectID");
                        dtNewSchema.Columns.Add("CreateDate");
                        dtNewSchema.Columns.Add("ModifyDate");
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        dtNewSchema.Columns.Add("CreateDate");
                        dtNewSchema.Columns.Add("ModifyDate");
                        break;
                    }
            }

            return dtNewSchema;
        }

        private static void AddDatabaseAutoCompleteRow(DataTable dtTarget, string databaseName)
        {
            var row = dtTarget.NewRow();

            row["DB"] = databaseName;
            row["Memo"] = "DB";

            dtTarget.Rows.Add(row);
        }

        private static void AddSqlServerTableOrViewAutoCompleteRow(DataTable dtTarget, DataRow dr)
        {
            var db = dr.GetSafeString("DB");
            var schemaNode = dr.GetSafeString("SchemaNode");
            var schemaName = dr.GetSafeString("SchemaName");
            var schemaType = dr.GetSafeString("SchemaType");
            var row = dtTarget.NewRow();

            row["DB"] = db;
            row["DBAndNode"] = $"{db}.{schemaNode}";
            row["SchemaNode"] = schemaNode.ToUpper();
            row["SchemaName"] = schemaName;
            row["SchemaType"] = $"{schemaType}, [{schemaNode}]"; //20260628 後方加上 schemaNode，避免相同的 TableName/ViewName 重複出現，讓使用者以為是 bug。

            dtTarget.Rows.Add(row);
        }

        private static void AddAutoCompleteLookupRows(DataTable dtTarget, IEnumerable<DataRow> sourceRows, Action<DataTable, DataRow> addRow)
        {
            if (dtTarget == null || sourceRows == null || addRow == null)
            {
                return;
            }

            dtTarget.BeginLoadData();

            try
            {
                foreach (var sourceRow in sourceRows)
                {
                    if (sourceRow == null)
                    {
                        continue;
                    }

                    addRow(dtTarget, sourceRow);
                }
            }
            finally
            {
                dtTarget.EndLoadData();
            }
        }

        private static void AddMySqlTableOrViewAutoCompleteRow(DataTable dtTarget, DataRow dr)
        {
            var db = dr.GetSafeString("DB");
            var schemaName = dr.GetSafeString("SchemaName");
            var schemaType = dr.GetSafeString("SchemaType");
            var row = dtTarget.NewRow();

            row["DB"] = db;
            row["SchemaName"] = schemaName;
            row["SchemaType"] = schemaType;

            dtTarget.Rows.Add(row);
        }

        private static bool CanUseDatabaseScopedAutoComplete()
        {
            return MyLibrary.EnableAutoComplete && !string.IsNullOrWhiteSpace(DatabaseName);
        }

        private static void AppendCommonAutoCompleteKeywords(bool requireDatabaseName)
        {
            using (TraceLogger.Time("Organize Auto Complete Info"))
            {
                var canAppend = requireDatabaseName ? CanUseDatabaseScopedAutoComplete() : MyLibrary.EnableAutoComplete;

                if (!canAppend)
                {
                    return;
                }

                AutoCompleteKeywordAppender.Append
                (
                    MyLibrary.KeywordsBuiltInKeywords,
                    MyLibrary.AutoCompleteBuiltInKeywords,
                    AutoCompleteSourceNames.BuiltInKeywords
                );

                AutoCompleteKeywordAppender.Append
                (
                    MyLibrary.KeywordsBuiltInFunctions,
                    MyLibrary.AutoCompleteBuiltInFunctions,
                    AutoCompleteSourceNames.Functions
                );

                AutoCompleteKeywordAppender.Append
                (
                    MyLibrary.KeywordsUserDefinedKeywords,
                    MyLibrary.AutoCompleteUserDefinedKeywords,
                    AutoCompleteSourceNames.UserDefinedKeywords
                );
            }
        }

        private static void UpdateSchemaGrid(C1TrueDBGrid c1Grid, bool shouldUpdate, bool isFromSchemaBrowser)
        {
            //The shared MainForm F7 reloads metadata without handing its Designer Grid to a legacy QueryForm layout routine. QueryForm views are updated afterwards.
            if (!shouldUpdate || c1Grid == null)
            {
                return;
            }

            using (TraceLogger.Time("Update Schema Data"))
            {
                GridHelper.UpdateSchemaData(c1Grid, true, isFromSchemaBrowser);
            }
        }

        private static void AddOracleCandidateAutoCompleteRow(DataTable target, string name, string source)
        {
            var row = target.NewRow();

            row["ObjectName"] = name;
            row["ObjectSource"] = string.Format("[{0}]", source);

            target.Rows.Add(row);
        }

        private static void AppendOracleCandidateNames(DataTable target, IEnumerable<string> names, bool enabled, string source)
        {
            if (!enabled || names == null)
            {
                return;
            }

            foreach (var name in names.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase))
            {
                AddOracleCandidateAutoCompleteRow(target, name, source);
            }
        }

        private static void AppendOracleCandidateKeywordText(DataTable target, string keywordText, bool enabled, string source)
        {
            if (!enabled || string.IsNullOrWhiteSpace(keywordText))
            {
                return;
            }

            foreach (var name in keywordText.Split(new[] { " " }, StringSplitOptions.RemoveEmptyEntries))
            {
                AddOracleCandidateAutoCompleteRow(target, name, source);
            }
        }

        private static void AppendOracleCandidateCommonKeywords(DataTable target)
        {
            if (!MyLibrary.EnableAutoComplete)
            {
                return;
            }

            AppendOracleCandidateKeywordText(target, MyLibrary.KeywordsBuiltInKeywords, MyLibrary.AutoCompleteBuiltInKeywords, AutoCompleteSourceNames.BuiltInKeywords);
            AppendOracleCandidateKeywordText(target, MyLibrary.KeywordsBuiltInFunctions, MyLibrary.AutoCompleteBuiltInFunctions, AutoCompleteSourceNames.Functions);
            AppendOracleCandidateKeywordText(target, MyLibrary.KeywordsUserDefinedKeywords, MyLibrary.AutoCompleteUserDefinedKeywords, AutoCompleteSourceNames.UserDefinedKeywords);
        }

        //Only the real Oracle entry issues a completed-load receipt. Never infer authority from a UI rebind.
        //The injectable candidate core remains private and is shared with the atomic failure Golden tests.
        private static SharedOracleMetadataCompletedLoadReceipt RunOracleMetadataAtomicLoad(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            var reader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

            //A deliberate refresh may start after a normal SELECT closed the physical session.
            //Open once BEFORE the first observation; never reconnect silently inside an issued receipt.
            reader.EnsureConnectionOpen();

            return RunOracleMetadataAtomicLoadWithVerifiedScope
            (
                executeQuery,
                isFromSchemaBrowser,
                () => ReferenceEquals(MyGlobal.OracleReader, reader) ? ReadVerifiedOracleScope(reader) : null
            );
        }

        private static OracleVerifiedSessionScope ReadVerifiedOracleScope(JasonQuery.Database.Providers.Readers.OracleReader reader)
        {
            if (reader == null)
            {
                return null;
            }

            if (!OracleVerifiedSessionScopePolicy.TryCreate
                (
                    reader.VerifiedSessionUser,
                    reader.VerifiedDatabaseName,
                    reader.VerifiedContainerName,
                    reader.VerifiedConnectionId,
                    out var verifiedScope
                ))
            {
                return null;
            }

            return verifiedScope;
        }

        private static OracleVerifiedSessionScope RequireOracleScope(OracleVerifiedSessionScope observed)
        {
            if (observed == null || !OracleVerifiedSessionScopePolicy.TryCreate
                (
                    observed.SessionUser,
                    observed.DatabaseName,
                    observed.ContainerName,
                    observed.ConnectionId,
                    out var immutableObservation
                ))
            {
                throw new InvalidOperationException("Oracle metadata load requires a verified physical session scope.");
            }

            //Copy primitive values: the caller's mutable observation object is not retained as evidence.
            return immutableObservation;
        }

        private static void EnsureSameOracleScope(OracleVerifiedSessionScope initial, OracleVerifiedSessionScope current)
        {
            var observed = RequireOracleScope(current);

            if (!OracleVerifiedSessionScopePolicy.CanBindMetadataToSession
                (
                    observed,
                    initial.ConnectionId,
                    initial.SessionUser,
                    initial.DatabaseName,
                    initial.ContainerName
                ))
            {
                throw new InvalidOperationException("Oracle metadata load crossed a verified Session/DB/PDB boundary; publication rejected.");
            }
        }

        //Private injection seam. Production supplies observations from OracleReader only.
        //Every query is surrounded by the verified scope checks, including a final pre-publication check.
        private static SharedOracleMetadataCompletedLoadReceipt RunOracleMetadataAtomicLoadWithVerifiedScope(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser, Func<OracleVerifiedSessionScope> observeScope)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            if (observeScope == null)
            {
                throw new ArgumentNullException(nameof(observeScope));
            }

            var initialScope = RequireOracleScope(observeScope());
            var initialLogicalScope = GetOracleLoadLogicalScopeKey();

            Func<string, DataTable> checkedQuery = sql =>
            {
                if (!string.Equals(initialLogicalScope, GetOracleLoadLogicalScopeKey(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Oracle logical connection changed during metadata loading.");
                }

                EnsureSameOracleScope(initialScope, observeScope());

                var result = executeQuery(sql);

                EnsureSameOracleScope(initialScope, observeScope());

                if (result == null)
                {
                    throw new InvalidOperationException("Oracle metadata query returned null; publication rejected.");
                }

                return result;
            };

            Action finalCheck = () =>
            {
                if (!string.Equals(initialLogicalScope, GetOracleLoadLogicalScopeKey(), StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Oracle logical connection changed before metadata publication.");
                }

                EnsureSameOracleScope(initialScope, observeScope());
            };

            return PrepareAndPublishOracleMetadata(checkedQuery, isFromSchemaBrowser, initialScope, initialLogicalScope, finalCheck);
        }

        //The preexisting AP tests exercise the SAME candidate builder with synthetic DataTables, but a test delegate has no physical-session authority and therefore cannot issue a receipt.
        private static void RunOracleMetadataAtomicLoadUnverifiedForTest(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser)
        {
            PrepareAndPublishOracleMetadata(executeQuery, isFromSchemaBrowser, null, string.Empty, null);
        }

        internal static string GetOracleLoadLogicalScopeKey()
        {
            //No password or SQL text is included. This is a change detector, NOT a logical revision proof.
            return string.Join("\u001F", new[]
            {
                DbConnectionName ?? string.Empty,
                DbConnectionServer ?? string.Empty,
                DbUser ?? string.Empty,
                OracleSid ?? string.Empty,
                OracleConnectAs ?? string.Empty,
                DbConnectionPort.ToString(System.Globalization.CultureInfo.InvariantCulture),
                DatabaseName ?? string.Empty
            });
        }

        private static SharedOracleMetadataCompletedLoadReceipt PrepareAndPublishOracleMetadata(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser, OracleVerifiedSessionScope verifiedScope, string logicalScopeKey, Action beforePublication)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            DataTable candidateTableAndViews = null;
            DataTable candidateSchema = null;
            DataTable candidateAutoComplete = null;

            try
            {
                OracleQueryEditorMetadataLoader.LoadTableAndViewNames
                (
                    ref candidateTableAndViews,
                    sql => executeQuery(sql),
                    DbUserUppercase
                );

                candidateSchema = CreateSchemaTable(isFromSchemaBrowser);
                candidateAutoComplete = CreateAutoCompleteForAllTable();

                var context = CreateOracleMetadataContext(candidateSchema, isFromSchemaBrowser);

                context.ExecuteQuery = executeQuery;

                var options = CreateOracleMetadataLoadOptions();
                var names = OracleQueryEditorMetadataLoader.CollectSchemaMetadata(context);

                AppendOracleCandidateNames(candidateAutoComplete, names.Functions, options.EnableFunctionAutoComplete, AutoCompleteSourceNames.Functions);
                AppendOracleCandidateNames(candidateAutoComplete, names.Tables, options.EnableTableAutoComplete, AutoCompleteSourceNames.Tables);
                AppendOracleCandidateNames(candidateAutoComplete, names.Triggers, options.EnableTriggerAutoComplete, AutoCompleteSourceNames.Triggers);
                AppendOracleCandidateNames(candidateAutoComplete, names.Views, options.EnableViewAutoComplete, AutoCompleteSourceNames.Views);
                AppendOracleCandidateCommonKeywords(candidateAutoComplete);

                var functionKeywords = ScintillaKeywordTextBuilder.Build(names.Functions);
                var tableKeywords = ScintillaKeywordTextBuilder.Build(names.Tables);
                var triggerKeywords = ScintillaKeywordTextBuilder.Build(names.Triggers);
                var viewKeywords = ScintillaKeywordTextBuilder.Build(names.Views);

                //No query, user-interface call or connection recovery is permitted after this gate.
                beforePublication?.Invoke();

                SharedOracleMetadataCompletedLoadReceipt receipt = null;

                if (verifiedScope != null && SharedOracleMetadataCompletedLoadReceipt.HasRawOracleIdentifiers(candidateSchema))
                {
                    receipt = new SharedOracleMetadataCompletedLoadReceipt
                    (
                        candidateSchema,
                        verifiedScope.ConnectionId,
                        verifiedScope.SessionUser,
                        verifiedScope.DatabaseName,
                        verifiedScope.ContainerName,
                        Guid.NewGuid(),
                        logicalScopeKey
                    );
                }

                //Logical publish of one complete candidate batch; old bound DataTables remain alive.
                dtSchema = candidateSchema;
                dtTableAndViews = candidateTableAndViews;

                MyGlobal.dtAutoCompleteForAll = candidateAutoComplete;
                MyLibrary.KeywordsUserDefinedFunctions = functionKeywords;
                MyLibrary.KeywordsUserDefinedTables = tableKeywords;
                MyLibrary.KeywordsUserDefinedTriggers = triggerKeywords;
                MyLibrary.KeywordsUserDefinedViews = viewKeywords;
                _lastOracleCompletedLoadReceipt = receipt;

                candidateSchema = null;
                candidateTableAndViews = null;
                candidateAutoComplete = null;

                return receipt;
            }
            finally
            {
                candidateSchema?.Dispose();
                candidateTableAndViews?.Dispose();
                candidateAutoComplete?.Dispose();
            }
        }

        //The previous receipt can be inspected, but is not by itself DDL or logical-revision authority.
        private static SharedOracleMetadataCompletedLoadReceipt _lastOracleCompletedLoadReceipt;
        internal static SharedOracleMetadataCompletedLoadReceipt LastOracleCompletedLoadReceipt => _lastOracleCompletedLoadReceipt;

        public static void UpdateSchemaData_Oracle(C1TrueDBGrid c1Grid, bool isFromSchemaBrowser = false)
        {
            RunOracleMetadataAtomicLoad(ExecuteMetadataQuery, isFromSchemaBrowser);

            //UI bindings are updated only after the entire candidate batch was published.
            UpdateSchemaGrid(c1Grid, true, isFromSchemaBrowser);

            MyGlobal.ClearMemory();
        }

        public static void GetTableInfo_Oracle(string schemaName, out DataTable dtTable)
        {
            var normalizedSchemaName = NormalizeOracleSchemaName(schemaName);
            var sql = OracleQueryEditorSqlBuilder.BuildTableInfo(normalizedSchemaName);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        public static void GetViewInfo_Oracle(string schemaName, out DataTable dtTable)
        {
            var normalizedSchemaName = NormalizeOracleSchemaName(schemaName);
            var sql = OracleQueryEditorSqlBuilder.BuildViewInfo(normalizedSchemaName);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        //The production entry and S1B2 Golden use the same injected-query core.
        //Each provider prepares candidates independently; failed queries never publish partial metadata.
        private static void RunThreeProviderMetadataAtomicLoad(DataSourceType sourceType, Func<string, DataTable> executeQuery, bool fromSchemaBrowser)
        {
            if (executeQuery == null)
            {
                throw new ArgumentNullException(nameof(executeQuery));
            }

            if (sourceType != GetEffectiveDataSourceType())
            {
                throw new InvalidOperationException("Metadata source type does not match the current connection.");
            }

            switch (sourceType)
            {
                case DataSourceType.PostgreSql:
                    {
                        RunPostgreSqlMetadataAtomicLoad(executeQuery, fromSchemaBrowser);
                        return;
                    }
                case DataSourceType.SqlServer:
                    {
                        RunSqlServerMetadataAtomicLoad(executeQuery, fromSchemaBrowser);
                        return;
                    }
                case DataSourceType.MySql:
                    {
                        RunMySqlMetadataAtomicLoad(executeQuery, fromSchemaBrowser);
                        return;
                    }
                default:
                    {
                        throw new ArgumentOutOfRangeException(nameof(sourceType));
                    }
            }
        }

        private static void RunPostgreSqlMetadataAtomicLoad(Func<string, DataTable> executeQuery, bool fromSchemaBrowser)
        {
            DataTable candidateNames = null;
            DataTable candidateSchema = null;
            DataTable candidateAutoComplete = null;

            try
            {
                PostgreSqlQueryEditorMetadataLoader.LoadTableAndViewNames(ref candidateNames, sql => executeQuery(sql));

                candidateSchema = CreateSchemaTable(fromSchemaBrowser);
                candidateAutoComplete = CreateAutoCompleteForAllTable();

                var canUseDatabaseScopedAutoComplete = CanUseDatabaseScopedAutoComplete();

                var options = new PostgreSqlQueryEditorMetadataLoadOptions
                {
                    TargetSchemaTable = candidateSchema,
                    DbConnectionName = DbConnectionName,
                    IsVersion11OrGreater = DbServerVersion == ">=11",
                    SortByColumnName = MyGlobal.IsSortByColumnName,
                    EnableFunctionAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedFunctions,
                    EnableTableAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTables,
                    EnableTriggerAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTriggers,
                    EnableViewAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedViews
                };

                string functionKeywords = null;
                string tableKeywords = null;
                string triggerKeywords = null;
                string viewKeywords = null;

                PostgreSqlQueryEditorMetadataLoader.LoadSchemaMetadata
                (
                    options,
                    sql => executeQuery(sql),
                    (functions, tables, triggers, views) =>
                    {
                        functionKeywords = ScintillaKeywordTextBuilder.Build(functions);
                        tableKeywords = ScintillaKeywordTextBuilder.Build(tables);
                        triggerKeywords = ScintillaKeywordTextBuilder.Build(triggers);
                        viewKeywords = ScintillaKeywordTextBuilder.Build(views);

                        AppendOracleCandidateNames(candidateAutoComplete, functions, options.EnableFunctionAutoComplete, AutoCompleteSourceNames.Functions);
                        AppendOracleCandidateNames(candidateAutoComplete, tables, options.EnableTableAutoComplete, AutoCompleteSourceNames.Tables);
                        AppendOracleCandidateNames(candidateAutoComplete, triggers, options.EnableTriggerAutoComplete, AutoCompleteSourceNames.Triggers);
                        AppendOracleCandidateNames(candidateAutoComplete, views, options.EnableViewAutoComplete, AutoCompleteSourceNames.Views);
                    }
                );

                if (canUseDatabaseScopedAutoComplete)
                {
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInKeywords, MyLibrary.AutoCompleteBuiltInKeywords, AutoCompleteSourceNames.BuiltInKeywords);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInFunctions, MyLibrary.AutoCompleteBuiltInFunctions, AutoCompleteSourceNames.Functions);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsUserDefinedKeywords, MyLibrary.AutoCompleteUserDefinedKeywords, AutoCompleteSourceNames.UserDefinedKeywords);
                }

                //Publish only after every metadata query and organizer has succeeded.
                //Previously bound tables must remain alive for any legacy view holding a reference.
                dtSchema = candidateSchema;
                dtTableAndViews = candidateNames;

                MyGlobal.dtAutoCompleteForAll = candidateAutoComplete;
                MyLibrary.KeywordsUserDefinedFunctions = functionKeywords;
                MyLibrary.KeywordsUserDefinedTables = tableKeywords;
                MyLibrary.KeywordsUserDefinedTriggers = triggerKeywords;
                MyLibrary.KeywordsUserDefinedViews = viewKeywords;

                candidateSchema = null;
                candidateNames = null;
                candidateAutoComplete = null;
            }
            finally
            {
                candidateSchema?.Dispose();
                candidateNames?.Dispose();
                candidateAutoComplete?.Dispose();
            }
        }

        public static void UpdateSchemaData_PostgreSql(C1TrueDBGrid c1Grid, bool bUpdate = true, bool isFromSchemaBrowser = false)
        {
            RunThreeProviderMetadataAtomicLoad(DataSourceType.PostgreSql, ExecuteMetadataQuery, isFromSchemaBrowser);

            UpdateSchemaGrid(c1Grid, bUpdate, isFromSchemaBrowser);
            MyGlobal.ClearMemory();
        }

        public static void GetSchemaInfo_PostgreSql(out DataTable dtTable)
        {
            var sql = PostgreSqlQueryEditorSqlBuilder.BuildSchemaInfo();

            ExecuteQueryToOutDataTable(sql, out dtTable, false);
        }

        public static void GetSchemaInfo_SqlServer(out DataTable dtTable)
        {
            var sql = SqlServerQueryEditorSqlBuilder.BuildSchemaInfo();

            ExecuteQueryToOutDataTable(sql, out dtTable, false);
        }

        public static void GetTableInfo_PostgreSql(out DataTable dtTable, string schemaNode = "")
        {
            var sql = PostgreSqlQueryEditorSqlBuilder.BuildTableInfo(schemaNode);

            ExecuteQueryToOutDataTable(sql, out dtTable, false);
        }

        public static void GetViewInfo_PostgreSql(out DataTable dtTable, string schemaNode = "")
        {
            var sql = PostgreSqlQueryEditorSqlBuilder.BuildViewInfo(schemaNode);

            ExecuteQueryToOutDataTable(sql, out dtTable, false);
        }

        //SQL Server metadata, both lookup tables, keywords, and editor candidates share one publication gate.
        private static void RunSqlServerMetadataAtomicLoad(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser)
        {
            DataTable candidateSchema = null;
            DataTable candidateNames = null;
            DataTable candidateDatabaseNames = null;
            DataTable candidateAutoComplete = null;

            try
            {
                candidateSchema = CreateSchemaTable(isFromSchemaBrowser);
                candidateNames = CreateAutoCompleteLookupTable();
                candidateDatabaseNames = CreateAutoCompleteLookupTable();
                candidateAutoComplete = CreateAutoCompleteForAllTable();

                var canUseDatabaseScopedAutoComplete = CanUseDatabaseScopedAutoComplete();

                var options = new SqlServerQueryEditorMetadataLoadOptions
                {
                    excludeIsMsShippedClause = ExcludeNativeDatabase ? "\r\n   AND Is_Ms_Shipped <> 1" : string.Empty,
                    sortByColumnName = MyGlobal.IsSortByColumnName ? "Column_Name;" : "Ordinal_Position;",
                    enableFunctionAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedFunctions,
                    enableTableAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTables,
                    enableTriggerAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTriggers,
                    enableViewAutoComplete = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedViews
                };

                string functionKeywords = null;
                string tableKeywords = null;
                string triggerKeywords = null;
                string viewKeywords = null;

                var metadataContext = CreateSqlServerMetadataContext(candidateSchema);
                var loader = new SqlServerQueryEditorMetadataLoader();
                loader.Load
                (
                    metadataContext,
                    options,
                    executeQuery,
                    (functions, tables, triggers, views) =>
                    {
                        functionKeywords = ScintillaKeywordTextBuilder.Build(functions);
                        tableKeywords = ScintillaKeywordTextBuilder.Build(tables);
                        triggerKeywords = ScintillaKeywordTextBuilder.Build(triggers);
                        viewKeywords = ScintillaKeywordTextBuilder.Build(views);

                        AppendOracleCandidateNames(candidateAutoComplete, functions, options.enableFunctionAutoComplete, AutoCompleteSourceNames.Functions);
                        AppendOracleCandidateNames(candidateAutoComplete, tables, options.enableTableAutoComplete, AutoCompleteSourceNames.Tables);
                        AppendOracleCandidateNames(candidateAutoComplete, triggers, options.enableTriggerAutoComplete, AutoCompleteSourceNames.Triggers);
                        AppendOracleCandidateNames(candidateAutoComplete, views, options.enableViewAutoComplete, AutoCompleteSourceNames.Views);
                    }
                );

                //Lookup query 10: database names. Do not publish or mutate shared state yet.
                var dbSql = SqlServerQueryEditorSqlBuilder.BuildDatabaseInfoForAutoComplete(ExcludeNativeDatabase);

                using (var dtDatabaseInfo = RequireThreeProviderMetadataTable(executeQuery(dbSql)))
                {
                    AddAutoCompleteLookupRows
                    (
                        candidateDatabaseNames,
                        dtDatabaseInfo.AsEnumerable(),
                        (target, row) => AddDatabaseAutoCompleteRow(target, row.GetSafeString("Name"))
                    );
                }

                //Lookup query 11: selected database tables and views.
                var tableViewSql = SqlServerQueryEditorSqlBuilder.BuildTableAndViewInfoForAutoComplete(DatabaseName, ExcludeNativeDatabase);

                using (var dtTableViewInfo = RequireThreeProviderMetadataTable(executeQuery(tableViewSql)))
                {
                    AddAutoCompleteLookupRows(candidateNames, dtTableViewInfo.AsEnumerable(), AddSqlServerTableOrViewAutoCompleteRow);
                }

                candidateNames.Merge(candidateDatabaseNames);

                if (canUseDatabaseScopedAutoComplete)
                {
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInKeywords, MyLibrary.AutoCompleteBuiltInKeywords, AutoCompleteSourceNames.BuiltInKeywords);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInFunctions, MyLibrary.AutoCompleteBuiltInFunctions, AutoCompleteSourceNames.Functions);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsUserDefinedKeywords, MyLibrary.AutoCompleteUserDefinedKeywords, AutoCompleteSourceNames.UserDefinedKeywords);

                    foreach (DataRow row in candidateDatabaseNames.Rows)
                    {
                        var db = row.GetSafeString("DB");

                        if (!string.IsNullOrWhiteSpace(db))
                        {
                            AddOracleCandidateAutoCompleteRow(candidateAutoComplete, db, AutoCompleteSourceNames.Databases);
                        }
                    }
                }

                //Logical publication: no database query, organizer or UI notification inside this section.
                //Old tables remain alive because existing F7 / editor bindings may still reference them.
                dtSchema = candidateSchema;
                dtTableAndViews = candidateNames;
                dtDatabaseName = candidateDatabaseNames;

                MyGlobal.dtAutoCompleteForAll = candidateAutoComplete;
                MyLibrary.KeywordsUserDefinedFunctions = functionKeywords;
                MyLibrary.KeywordsUserDefinedTables = tableKeywords;
                MyLibrary.KeywordsUserDefinedTriggers = triggerKeywords;
                MyLibrary.KeywordsUserDefinedViews = viewKeywords;

                candidateSchema = null;
                candidateNames = null;
                candidateDatabaseNames = null;
                candidateAutoComplete = null;
            }
            finally
            {
                candidateSchema?.Dispose();
                candidateNames?.Dispose();
                candidateDatabaseNames?.Dispose();
                candidateAutoComplete?.Dispose();
            }
        }

        //MySQL: prepare all seven schema metadata queries plus both AutoComplete lookups before publishing any shared DataTable or keyword state.
        private static void RunMySqlMetadataAtomicLoad(Func<string, DataTable> executeQuery, bool isFromSchemaBrowser)
        {
            DataTable candidateSchema = null;
            DataTable candidateNames = null;
            DataTable candidateDatabaseNames = null;
            DataTable candidateAutoComplete = null;

            try
            {
                candidateSchema = CreateSchemaTable(isFromSchemaBrowser);
                candidateNames = CreateAutoCompleteLookupTable();
                candidateDatabaseNames = CreateAutoCompleteLookupTable();
                candidateAutoComplete = CreateAutoCompleteForAllTable();

                var canUseDatabaseScopedAutoComplete = CanUseDatabaseScopedAutoComplete();

                var options = new MySqlQueryEditorMetadataLoadOptions
                {
                    AddSchemaRow = MyGlobal.IsShowColumnInfo && (MyGlobal.ShowColumnInfo != -1 || isFromSchemaBrowser),
                    AutoCompleteFunction = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedFunctions,
                    AutoCompleteTable = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTables,
                    AutoCompleteTrigger = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedTriggers,
                    AutoCompleteView = canUseDatabaseScopedAutoComplete && MyLibrary.AutoCompleteUserDefinedViews,
                    FromSchemaBrowser = isFromSchemaBrowser
                };

                var context = new MySqlMetadataContext
                {
                    ConnectionName = DbConnectionName,
                    DatabaseName = DatabaseName,
                    SchemaTable = candidateSchema,
                    ExecuteQuery = executeQuery
                };

                string functionKeywords = null;
                string tableKeywords = null;
                string triggerKeywords = null;
                string viewKeywords = null;

                MySqlQueryEditorMetadataLoader.LoadSchemaMetadata
                (
                    context,
                    options,
                    (functions, tables, triggers, views) =>
                    {
                        //Preserve the original MySQL keyword formatting, including trailing space.
                        functionKeywords = string.Join(" ", functions) + " ";
                        tableKeywords = string.Join(" ", tables) + " ";
                        triggerKeywords = string.Join(" ", triggers) + " ";
                        viewKeywords = string.Join(" ", views) + " ";

                        AppendOracleCandidateNames(candidateAutoComplete, functions, options.AutoCompleteFunction, AutoCompleteSourceNames.Functions);
                        AppendOracleCandidateNames(candidateAutoComplete, tables, options.AutoCompleteTable, AutoCompleteSourceNames.Tables);
                        AppendOracleCandidateNames(candidateAutoComplete, triggers, options.AutoCompleteTrigger, AutoCompleteSourceNames.Triggers);
                        AppendOracleCandidateNames(candidateAutoComplete, views, options.AutoCompleteView, AutoCompleteSourceNames.Views);
                    }
                );

                candidateSchema = context.ResultSchemaTable ?? candidateSchema;

                //Query 8: database names. The candidate tables are not globally visible yet.
                var databaseSql = MySqlQueryEditorSqlBuilder.BuildDatabaseInfoForAutoComplete();

                using (var databaseInfo = RequireThreeProviderMetadataTable(executeQuery(databaseSql)))
                {
                    AddAutoCompleteLookupRows
                    (
                        candidateDatabaseNames,
                        databaseInfo.AsEnumerable(),
                        (target, row) => AddDatabaseAutoCompleteRow(target, row.GetSafeString("Name"))
                    );
                }

                //Query 9: current database table and view names.
                var tableViewSql = MySqlQueryEditorSqlBuilder.BuildTableAndViewInfoForAutoComplete(DatabaseName);

                using (var tableViewInfo = RequireThreeProviderMetadataTable(executeQuery(tableViewSql)))
                {
                    AddAutoCompleteLookupRows(candidateNames, tableViewInfo.AsEnumerable(), AddMySqlTableOrViewAutoCompleteRow);
                }

                candidateNames.Merge(candidateDatabaseNames);

                if (canUseDatabaseScopedAutoComplete)
                {
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInKeywords, MyLibrary.AutoCompleteBuiltInKeywords, AutoCompleteSourceNames.BuiltInKeywords);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsBuiltInFunctions, MyLibrary.AutoCompleteBuiltInFunctions, AutoCompleteSourceNames.Functions);
                    AppendOracleCandidateKeywordText(candidateAutoComplete, MyLibrary.KeywordsUserDefinedKeywords, MyLibrary.AutoCompleteUserDefinedKeywords, AutoCompleteSourceNames.UserDefinedKeywords);

                    foreach (DataRow row in candidateDatabaseNames.Rows)
                    {
                        var db = row.GetSafeString("DB");

                        if (!string.IsNullOrWhiteSpace(db))
                        {
                            AddOracleCandidateAutoCompleteRow(candidateAutoComplete, db, AutoCompleteSourceNames.Databases);
                        }
                    }
                }

                //The only publication section: no SQL, organizer, or UI call here.
                //Keep old tables alive for F7/Editor instances that may still reference them.
                dtSchema = candidateSchema;
                dtTableAndViews = candidateNames;
                dtDatabaseName = candidateDatabaseNames;

                MyGlobal.dtAutoCompleteForAll = candidateAutoComplete;
                MyLibrary.KeywordsUserDefinedFunctions = functionKeywords;
                MyLibrary.KeywordsUserDefinedTables = tableKeywords;
                MyLibrary.KeywordsUserDefinedTriggers = triggerKeywords;
                MyLibrary.KeywordsUserDefinedViews = viewKeywords;

                candidateSchema = null;
                candidateNames = null;
                candidateDatabaseNames = null;
                candidateAutoComplete = null;
            }
            finally
            {
                candidateSchema?.Dispose();
                candidateNames?.Dispose();
                candidateDatabaseNames?.Dispose();
                candidateAutoComplete?.Dispose();
            }
        }

        private static DataTable RequireThreeProviderMetadataTable(DataTable table)
        {
            if (table == null)
            {
                throw new InvalidOperationException("Metadata query returned null");
            }

            return table;
        }

        public static void UpdateSchemaData_SqlServer(C1TrueDBGrid c1Grid, bool isSchemaBrowserForm = true, bool isFromSchemaBrowser = false)
        {
            RunThreeProviderMetadataAtomicLoad(DataSourceType.SqlServer, ExecuteMetadataQuery, isFromSchemaBrowser);

            //UI binding is updated only after the entire candidate batch has been published.
            UpdateSchemaGrid(c1Grid, true, isFromSchemaBrowser);
            MyGlobal.ClearMemory();
        }

        private static SqlServerMetadataContext CreateSqlServerMetadataContext(DataTable dtSchema)
        {
            return new SqlServerMetadataContext
            {
                connectionName = DbConnectionName,
                databaseName = DatabaseName,
                separator = MyGlobal.Separator,
                dateTimeFormat = $"{MyLibrary.DateFormat} HH:mm:ss",
                dtSchema = dtSchema
            };
        }

        public static void UpdateDatabaseInfoForAutoComplete_SqlServer() //一開始沒有指定資料庫，只要取得所有的資料庫名稱 (USE 指令會用到)
        {
            dtDatabaseName = CreateAutoCompleteLookupTable();

            var dtTemp = new DataTable();
            var sql = SqlServerQueryEditorSqlBuilder.BuildDatabaseInfoForAutoComplete(ExcludeNativeDatabase);

            ExecuteQueryToDataTable(sql, ref dtTemp);

            AddAutoCompleteLookupRows
            (
                dtDatabaseName,
                dtTemp?.AsEnumerable(),
                (dtTarget, dr) => AddDatabaseAutoCompleteRow(dtTarget, dr.GetSafeString("Name"))
            );
        }

        public static void UpdateTableAndViewInfoForAutoComplete_SqlServer(string schemaName) //切換資料庫時，也要一併更新 Table & View Info
        {
            //for Query Editor AutoComplete, 取得指定資料庫的所有 Table + View Name
            dtTableAndViews = CreateAutoCompleteLookupTable();

            var sql = SqlServerQueryEditorSqlBuilder.BuildTableAndViewInfoForAutoComplete(schemaName, ExcludeNativeDatabase);
            var dtTemp = new DataTable();

            ExecuteQueryToDataTable(sql, ref dtTemp);

            AddAutoCompleteLookupRows(dtTableAndViews, dtTemp?.AsEnumerable(), AddSqlServerTableOrViewAutoCompleteRow);
        }

        public static void GetTableInfo_SqlServer(string schemaName, string schema, out DataTable dtTable)
        {
            var sql = SqlServerQueryEditorSqlBuilder.BuildTableInfo(schemaName, schema);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        public static void GetViewInfo_SqlServer(string schemaName, string schema, out DataTable dtTable)
        {
            var sql = SqlServerQueryEditorSqlBuilder.BuildViewInfo(schemaName, schema);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        public static void UpdateSchemaData_MySql(C1TrueDBGrid c1Grid, bool isFromSchemaBrowser = false)
        {
            RunThreeProviderMetadataAtomicLoad(DataSourceType.MySql, ExecuteMetadataQuery, isFromSchemaBrowser);

            //Do not touch the grid or shared AutoComplete until publication succeeded.
            UpdateSchemaGrid(c1Grid, true, isFromSchemaBrowser);

            MyGlobal.ClearMemory();
        }

        public static void UpdateDatabaseInfoForAutoComplete_MySql()
        {
            dtDatabaseName = CreateAutoCompleteLookupTable();

            var sql = MySqlQueryEditorSqlBuilder.BuildDatabaseInfoForAutoComplete();
            var dtTemp = new DataTable();

            ExecuteQueryToDataTable(sql, ref dtTemp);

            AddAutoCompleteLookupRows
            (
                dtDatabaseName,
                dtTemp?.AsEnumerable(),
                (dtTarget, dr) => AddDatabaseAutoCompleteRow(dtTarget, dr.GetSafeString("Name"))
            );
        }

        public static void UpdateTableAndViewInfoForAutoComplete_MySql(string schemaName) //切換資料庫時，也要一併更新 Table & View Info
        {
            //for Query Editor AutoComplete, 取得指定資料庫的所有 Table + View Name
            dtTableAndViews = CreateAutoCompleteLookupTable();

            var sql = MySqlQueryEditorSqlBuilder.BuildTableAndViewInfoForAutoComplete(schemaName);
            var dtTemp = new DataTable();

            ExecuteQueryToDataTable(sql, ref dtTemp);

            AddAutoCompleteLookupRows(dtTableAndViews, dtTemp?.AsEnumerable(), AddMySqlTableOrViewAutoCompleteRow);
        }

        public static void GetTableInfo_MySql(string schemaName, out DataTable dtTable)
        {
            var sql = MySqlQueryEditorSqlBuilder.BuildTableInfo(schemaName);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        public static void GetViewInfo_MySql(string schemaName, out DataTable dtTable)
        {
            var sql = MySqlQueryEditorSqlBuilder.BuildViewInfo(schemaName);

            ExecuteQueryToOutDataTable(sql, out dtTable);
        }

        public static string OracleCreateTableScriptBeautifier(string script, string schemaName, string schemaType)
        {
            var oracleReader = MyGlobal.OracleReader ?? throw new InvalidOperationException("OracleReader has not been initialized.");

            oracleReader.EnsureConnectionOpen();

            return CreateScript.Oracle.OracleCreateTableScriptBeautifier.Beautify(script, schemaName, DbUserUppercase, ExecuteMetadataQuery);
        }

        public static DataTable GetColumnComments(IEnumerable<(string Schema, string Table)> tables)
        {
            var dtResult = new DataTable();

            if (tables == null)
            {
                return dtResult;
            }

            var tableList = tables
                .Where(t => !string.IsNullOrWhiteSpace(t.Table))
                .Select
                 (
                     t =>
                     (
                         Schema: (t.Schema ?? string.Empty).Trim(),
                         Table: t.Table.Trim()
                     )
                 )
                .Distinct()
                .ToList();

            if (tableList.Count == 0)
            {
                return dtResult;
            }

            var sql = string.Empty;

            switch (GetEffectiveDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        sql = OracleTableColumnCommentsSqlBuilder.Build(tableList, DbUserUppercase);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        sql = PostgreSqlTableColumnCommentsSqlBuilder.Build(tableList);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        sql = SqlServerTableColumnCommentsSqlBuilder.Build(tableList, DatabaseName);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        sql = MySqlTableColumnCommentsSqlBuilder.Build(tableList);
                        break;
                    }
                default:
                    {
                        return dtResult;
                    }
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                return dtResult;
            }

            ExecuteQueryToDataTable(sql, ref dtResult);
            return dtResult;
        }

        public static DataTable GetColumnDefaultValue(string baseSchemaName, string baseTableName, string baseSchemaNode = "")
        {
            var dtResult = new DataTable();
            var sql = string.Empty;

            switch (GetEffectiveDataSourceType())
            {
                case DataSourceType.Oracle:
                    {
                        sql = OracleTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(baseSchemaName, baseTableName);
                        break;
                    }
                case DataSourceType.PostgreSql:
                    {
                        sql = PostgreSqlTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(baseSchemaName, baseTableName);
                        break;
                    }
                case DataSourceType.SqlServer:
                    {
                        sql = SqlServerTableColumnInfoSqlBuilder.BuildColumnInfo(baseSchemaNode, baseSchemaName, baseTableName);
                        break;
                    }
                case DataSourceType.MySql:
                    {
                        sql = MySqlTableColumnInfoSqlBuilder.BuildColumnInfoIncludeDefaultValue(baseSchemaName, baseTableName);
                        break;
                    }
                default:
                    {
                        return dtResult;
                    }
            }

            if (string.IsNullOrWhiteSpace(sql))
            {
                return dtResult;
            }

            ExecuteQueryToDataTable(sql, ref dtResult);
            return dtResult;
        }

        private static void ExecuteQueryToOutDataTable(string sql, out DataTable dtTable, bool showAlertOnError = true)
        {
            dtTable = null;
            ExecuteQueryToDataTable(sql, ref dtTable, showAlertOnError);
        }

        private static string NormalizeOracleSchemaName(string schemaName)
        {
            if (string.IsNullOrWhiteSpace(schemaName))
            {
                return DbUserUppercase;
            }

            return schemaName.ToUpper();
        }
    }
}
