using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.Database.DdlPreview;
using JasonQuery.UI.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using System.Threading;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //D3C-05A: no DB access and no DDL. Exercise the actual Form's verified-receipt readback.
    //Readback of SHOW CREATE must not apply the SQL statement's NBE parsing mode.
    [TestClass]
    [TestCategory("D3C05A")]
    public sealed class SchemaExplorerMySqlCommentFormReadbackGoldenTests
    {
        private const string DatabaseName = "sakila";
        private const string TableName = "jq_c3c_comment_smoke";
        private const string ColumnName = "id";
        private const string OrdinaryMode = "STRICT_TRANS_TABLES";
        private const string NoBackslashMode = "STRICT_TRANS_TABLES,NO_BACKSLASH_ESCAPES";

        private static string ReadOriginalComment(string completeDefinition, string verifiedMode)
        {
            string actual = null;
            Exception failure = null;

            var worker = new Thread(() =>
            {
                try
                {
                    using (var form = new EditColumnForm())
                    {
                        form.SourceType = DataSourceType.MySql;
                        form.EditMode = EditColumnMode.Comment;
                        form.RequireNonOracleF7ExistingColumnGate = true;

                        form.NonOracleF7MySqlCompleteDefinitionPreview = new MySqlVerifiedColumnDefinitionSnapshot
                        (
                            new Guid("4012eb0b-8649-4133-b158-21d10ae9e2ad"),
                            DatabaseName, TableName, ColumnName, "8.0.34",
                            completeDefinition, 7312L, "verified-attributes", verifiedMode
                        );

                        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;

                        var sourceField = typeof(EditColumnForm).GetField("_currentSourceType", flags);
                        var loadMethod = typeof(EditColumnForm).GetMethod("LoadCommentMetadata", flags);
                        var commentField = typeof(EditColumnForm).GetField("_columnComment", flags);

                        if (sourceField == null || loadMethod == null || commentField == null)
                        {
                            throw new MissingMemberException("D3C05A: expected existing form readback members were not found.");
                        }

                        //Form_Load is deliberately not called: it performs unrelated application initialization.
                        sourceField.SetValue(form, DataSourceType.MySql);
                        loadMethod.Invoke(form, null);
                        actual = commentField.GetValue(form) as string;
                    }
                }
                catch (Exception ex)
                {
                    failure = ex;
                }
            });

            worker.IsBackground = true;
            worker.SetApartmentState(ApartmentState.STA);
            worker.Start();

            Assert.IsTrue(worker.Join(TimeSpan.FromSeconds(15)), "D3C05A: the STA form readback did not complete.");

            if (failure != null)
            {
                Assert.Fail("D3C05A: UI readback could not run: " + failure);
            }

            return actual;
        }

        [TestMethod]
        public void D3C05A_G01_NbeCommentWithoutBackslashMustRemainUnchanged()
        {
            const string definition = "int NOT NULL DEFAULT '1' COMMENT 'original'";

            Assert.AreEqual("original", ReadOriginalComment(definition, NoBackslashMode));
        }

        [TestMethod]
        public void D3C05A_G02_OrdinaryModeEscapedOldBackslashMustReadBackOnce()
        {
            const string definition = @"int NOT NULL DEFAULT '1' COMMENT 'C:\\Data'";

            Assert.AreEqual(@"C:\Data", ReadOriginalComment(definition, OrdinaryMode));
        }

        [TestMethod]
        public void D3C05A_R03_NbeVerifiedReceiptMustNotDoubleOldBackslash()
        {
            const string definition = @"int NOT NULL DEFAULT '1' COMMENT 'D3 It''s C:\\Data; SELECT 2; --'";

            Assert.AreEqual(@"D3 It's C:\Data; SELECT 2; --", ReadOriginalComment(definition, NoBackslashMode));
        }

        [TestMethod]
        public void D3C05A_R04_NbeTwoStoredBackslashesMustNotBecomeFour()
        {
            const string definition = @"int NOT NULL DEFAULT '1' COMMENT 'Path C:\\Logs\\Archive'";

            Assert.AreEqual(@"Path C:\Logs\Archive", ReadOriginalComment(definition, NoBackslashMode));
        }
    }
}
