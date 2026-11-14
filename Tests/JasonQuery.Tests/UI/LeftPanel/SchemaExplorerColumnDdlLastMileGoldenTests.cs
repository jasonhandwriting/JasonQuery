using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U3-C2B3B: pure execution qualification and API contracts; no database connection or DDL execution.
    [TestClass]
    [TestCategory("F7U3C2B3B")]
    public sealed class SchemaExplorerColumnDdlLastMileGoldenTests
    {
        [TestMethod]
        public void B701_ExistingColumnHasDedicatedVerifiedFormDispatch()
        {
            var types = new[]
            {
                typeof(string).MakeByRefType(), typeof(bool).MakeByRefType(),
                typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType(), typeof(bool).MakeByRefType()
            };

            var method = typeof(EditColumnForm).GetMethod
            (
                "TryExecuteNonOracleF7ExistingColumn", BindingFlags.Instance | BindingFlags.NonPublic,
                null, types, null
            );

            Assert.IsNotNull(method);
            Assert.AreEqual(typeof(string), method.ReturnType);
        }

        [TestMethod]
        [DataRow(DataSourceType.PostgreSql, EditColumnMode.Comment)]
        [DataRow(DataSourceType.PostgreSql, EditColumnMode.Rename)]
        [DataRow(DataSourceType.PostgreSql, EditColumnMode.Drop)]
        [DataRow(DataSourceType.SqlServer, EditColumnMode.Comment)]
        [DataRow(DataSourceType.SqlServer, EditColumnMode.Rename)]
        [DataRow(DataSourceType.SqlServer, EditColumnMode.Drop)]
        [DataRow(DataSourceType.MySql, EditColumnMode.Drop)]
        public void B702_QualifiedProviderOperationMayEnableVerifiedExecution(DataSourceType provider, EditColumnMode mode)
        {
            Assert.IsTrue(EditColumnForm.CanEnableNonOracleExistingColumnDdl(provider, mode));
        }

        [TestMethod]
        [DataRow(DataSourceType.MySql, EditColumnMode.Comment)]
        [DataRow(DataSourceType.MySql, EditColumnMode.Rename)]
        [DataRow(DataSourceType.MySql, EditColumnMode.Add)]
        [DataRow(DataSourceType.Oracle, EditColumnMode.Drop)]
        [DataRow(DataSourceType.PostgreSql, EditColumnMode.Add)]
        [DataRow(DataSourceType.SqlServer, EditColumnMode.None)]
        public void B703_UnsafeOrUnrelatedOperationsRemainPreviewOnly(DataSourceType provider, EditColumnMode mode)
        {
            Assert.IsFalse(EditColumnForm.CanEnableNonOracleExistingColumnDdl(provider, mode));
        }

        [TestMethod]
        public void B704_MySqlMissingCompleteDefinitionCannotAuthorizeChangeColumn()
        {
            Assert.AreEqual("F7U3C2B_MYSQL_DEFINITION_UNVERIFIED", NonOracleSchemaExplorerDdlExecuteGate.ValidateMySqlColumnDefinitionSafety(ColumnDdlOperation.Rename, false, null, null));
        }

        [TestMethod]
        public void B705_DropUsesQualifiedOriginalColumnName()
        {
            var sql = ColumnDdlSqlBuilder.Build(new ColumnDdlRequest
            {
                DataSourceType = DataSourceType.MySql,
                Operation = ColumnDdlOperation.Drop,
                SchemaDatabase = "appdb",
                TableName = "Orders",
                ColumnName = "X, Y"
            });

            Assert.AreEqual("ALTER TABLE `appdb`.`Orders` DROP COLUMN `X, Y`;", sql);
        }

        [TestMethod]
        public void B706_ExactOriginalColumnMismatchIsRejected()
        {
            var sql = "ALTER TABLE `appdb`.`Orders` DROP COLUMN `Col`;";

            Assert.AreEqual("F7U3C2B_COLUMN_IDENTITY_CHANGED", NonOracleSchemaExplorerDdlExecuteGate.ValidateExistingColumnIntent(DataSourceType.MySql, ColumnDdlOperation.Drop, true, "Col", "col", sql, sql));
        }

        [TestMethod]
        public void B707_ColumnOperationCannotReuseTableDdlSql()
        {
            Assert.IsFalse(NonOracleSchemaExplorerDdlExecuteGate.IsQualifiedColumnDdlSql
            (
                DataSourceType.SqlServer, ColumnDdlOperation.Drop,
                "DROP TABLE [appdb].[dbo].[Orders];"
            ));
        }

        [TestMethod]
        public void B709_SqlServerColumnFormCapturesSourceObjectId()
        {
            var property = typeof(EditColumnForm).GetProperty
            (
                "NonOracleF7ExpectedSqlServerObjectId", BindingFlags.Instance | BindingFlags.NonPublic
            );

            Assert.IsNotNull(property);
            Assert.AreEqual(typeof(int), property.PropertyType);
        }

        [TestMethod]
        public void B708_PreexistingAddColumnGateRemainsSeparate()
        {
            Assert.IsNotNull(typeof(EditColumnForm).GetMethod
            (
                "TryExecuteNonOracleF7", BindingFlags.Instance | BindingFlags.NonPublic
            ));
        }
    }
}
