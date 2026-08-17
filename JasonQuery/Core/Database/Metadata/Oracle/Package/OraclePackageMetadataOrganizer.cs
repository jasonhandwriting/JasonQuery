using JasonQuery.Core.Config;
using JasonQuery.Core.Data.DataRows;
using JasonQuery.Core.Data.DataTables;
using JasonQuery.Core.Database.SqlBuilder.Oracle.Metadata.Package;
using JasonQuery.Core.Logging;
using System;
using System.Data;
using System.Linq;

namespace JasonQuery.Core.Database.Metadata.Oracle.Package
{
    internal static class OraclePackageMetadataOrganizer
    {
        #region Entry
        public static void Organize(OracleMetadataContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.TargetSchemaTable == null)
            {
                throw new ArgumentNullException(nameof(context.TargetSchemaTable));
            }

            if (context.ExecuteQuery == null)
            {
                throw new ArgumentNullException(nameof(context.ExecuteQuery));
            }

            if (!context.NeedSchemaRows)
            {
                return;
            }

            DataTable dtPackageInfo = null;

            try
            {
                dtPackageInfo = GetPackageInfo(context);

                var packageSchemaType = SchemaTypeTextBuilder.Build(SchemaObjectNames.Packages, dtPackageInfo?.Rows.Count ?? 0);

                OrganizeSchemaRows(context, dtPackageInfo, packageSchemaType);
            }
            finally
            {
                DataTableLifecycleHelper.DisposeDataTable(ref dtPackageInfo);
            }
        }
        #endregion

        #region Get
        private static DataTable GetPackageInfo(OracleMetadataContext context)
        {
            using (TraceLogger.Time("SQL: Get Package Information"))
            {
                var sql = OraclePackageSqlBuilder.BuildGetPackageInfoSql(context.DbUserNameUppercase);

                return context.ExecuteQuery(sql);
            }
        }
        #endregion

        #region Build
        private static string BuildSchemaNameText(string packageName, string objectType)
        {
            return string.Format("{0}{1}{2}", packageName, MyGlobal.Separator, GetPackageTypeSuffix(objectType));
        }

        private static string GetPackageTypeSuffix(string objectType)
        {
            return string.Equals(objectType, "PACKAGE", StringComparison.OrdinalIgnoreCase) ? "(Spec)" : "(Body)";
        }
        #endregion

        #region Organize / Create
        private static void OrganizeSchemaRows(OracleMetadataContext context, DataTable dtPackageInfo, string packageSchemaType)
        {
            context.TargetSchemaTable.BeginLoadData();

            try
            {
                using (TraceLogger.Time("Organize Package Info"))
                {
                    foreach (DataRow drPackageInfo in dtPackageInfo?.AsEnumerable() ?? Enumerable.Empty<DataRow>())
                    {
                        var packageName = drPackageInfo.GetSafeString("PackageName");
                        var objectType = drPackageInfo.GetSafeString("ObjectType");
                        var packageSchemaName = BuildSchemaNameText(packageName, objectType);

                        var row = OraclePackageSchemaRowBuilder.Build
                        (
                            context.TargetSchemaTable,
                            context.DbConnectionName,
                            packageSchemaType,
                            packageSchemaName
                        );

                        context.TargetSchemaTable.Rows.Add(row);
                    }
                }
            }
            finally
            {
                context.TargetSchemaTable.EndLoadData();
            }
        }
        #endregion
    }
}