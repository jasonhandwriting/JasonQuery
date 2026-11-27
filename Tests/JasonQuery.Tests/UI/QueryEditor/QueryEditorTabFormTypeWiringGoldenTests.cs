using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.QueryEditor
{
    //TAB-R-C2b3b: source guards for typed tab classification.
    //Runtime dispatch and provider behavior require VM GUI qualification.
    [TestClass]
    [TestCategory("TABRC2B3B")]
    public sealed class QueryEditorTabFormTypeWiringGoldenTests
    {
        private static string ReadMainForm()
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, "JasonQuery", "UI", "Forms", "MainForm.cs");

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("MainForm.cs not found.");
            return string.Empty;
        }

        private static string BetweenMethods(string source, string startAnchor, string endAnchor)
        {
            var start = source.IndexOf(startAnchor, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "Missing start anchor: " + startAnchor);

            var end = source.IndexOf(endAnchor, start + startAnchor.Length, StringComparison.Ordinal);

            Assert.IsTrue(end > start, "Missing end anchor: " + endAnchor);
            return source.Substring(start, end - start);
        }

        [TestMethod]
        public void TABRC2B3B01_TransactionRecipientsUseFormTypeRatherThanTitle()
        {
            var source = ReadMainForm();
            var broadcast = BetweenMethods(source, "private bool ShouldBroadcastCommitRollbackButtonState(", "private List<string> GetNormalQueryFormAccessibleDescriptions()");
            var normal = BetweenMethods(source, "private List<string> GetNormalQueryFormAccessibleDescriptions()", "private void ApplyEditorTabState(");
            var timer = BetweenMethods(source, "private void tmrTimeAndKeyStatus_Tick(", "private void PasteFromSchemaBrowser(");

            Assert.IsTrue(broadcast.Contains("tab?.Control is QueryForm || tab?.Control is SchemaBrowserForm"));
            Assert.IsTrue(normal.Contains(".Where(tab => tab.Control is QueryForm)"));
            Assert.IsTrue(timer.Contains("if (tabPage.Control is QueryForm)"));
            Assert.IsTrue(timer.Contains("if (tabPage.Control is SchemaBrowserForm)"));
            Assert.IsFalse(Regex.IsMatch(broadcast + normal + timer, @"SpecialTabName\s*\.\s*Contains"));
            Assert.IsTrue(timer.Contains("ExecuteCommitCommitRollback`"));
            Assert.IsTrue(timer.Contains("UpdateCommitRollbackButton`"));
        }

        [TestMethod]
        public void TABRC2B3B02_SchemaAndDisconnectNotificationsKeepTypedRecipientScopes()
        {
            var source = ReadMainForm();
            var schema = BetweenMethods(source, "else if (newValue.StartsWith(\"UpdateSchemaInformation`\"", "else if (newValue.StartsWith(\"DoubleClickToSwitchTab`\"");
            var disconnect = BetweenMethods(source, "else if (newValue.StartsWith(\"DisconnectAfterExecuteError`\"", "else if (newValue.StartsWith(\"ReloadLocalization`\"");
            var closeConnection = BetweenMethods(source, "private void mnuCloseConnection_Click(", "private void DisconnectDatabase(");

            Assert.AreEqual(2, Regex.Matches(schema, @"tabPage\.Control is QueryForm && temp != tabAccessibleDescription").Count);
            Assert.AreEqual(2, Regex.Matches(schema, @"tabPage\.Control is SchemaBrowserForm && temp != tabAccessibleDescription").Count);
            Assert.IsTrue(disconnect.Contains("if (tabPage.Control is QueryForm)"));
            Assert.IsTrue(closeConnection.Contains(".Where(tab => tab.Control is QueryForm)"));
            Assert.IsFalse(Regex.IsMatch(schema + disconnect + closeConnection, @"SpecialTabName|\.Title\b"));
        }

        [TestMethod]
        public void TABRC2B3B03_DatabaseSwitchUsesActualEditorIdentityAndPreservesProviderDispatch()
        {
            var source = ReadMainForm();
            var postgreSql = BetweenMethods(source, "private void SwitchDatabase_PostgreSql(", "private void LoadSqlServerDatabase(");
            var sqlServer = BetweenMethods(source, "private void SwitchDatabase_SqlServer(", "private void LoadMySqlDatabase(");
            var mySql = BetweenMethods(source, "private void SwitchDatabase_MySql(", "private void mnuBlobViewer_Click(");
            var useNotifications = BetweenMethods(source, "private void ValueUpdated(", "else if (newValue.StartsWith(\"P`\"");

            Assert.IsTrue(postgreSql.Contains("if (!(theTab.Control is QueryForm))"));
            Assert.IsTrue(postgreSql.Contains("CloseQueryFormAndCheckCommit`"));

            foreach (var body in new[] { sqlServer, mySql })
            {
                var validate = body.IndexOf("!ContainsTabPage(targetPage)", StringComparison.Ordinal);
                var dispatch = body.IndexOf("MyGlobal.GlobalTemp =", StringComparison.Ordinal);

                Assert.IsTrue(body.Contains("targetPage?.Control is QueryForm query"));
                Assert.IsTrue(body.Contains("!ReferenceEquals(targetPage.Control, query)"));
                Assert.IsTrue(validate >= 0 && dispatch > validate);
                Assert.IsTrue(body.Contains("tab.Control is QueryForm && tab.AccessibleDescription != accessibleDescription"));
                Assert.IsTrue(Regex.IsMatch(body, @"finally\s*\{\s*Application\.UseWaitCursor = false;"));
            }

            Assert.AreEqual(2, Regex.Matches(useNotifications, @"tab\.Control is QueryForm && tab\.AccessibleDescription != info").Count);
            Assert.IsTrue(sqlServer.Contains("SQLServerSwitchDatabaseFromMainForm"));
            Assert.IsTrue(mySql.Contains("MySQLSwitchDatabaseFromMainForm"));
            Assert.IsFalse(Regex.IsMatch(postgreSql + sqlServer + mySql + useNotifications, @"SpecialTabName|\.Title\b"));
        }

        [TestMethod]
        public void TABRC2B3B04_LocalizationOnlyRelabelsActualSpecialForms()
        {
            var source = ReadMainForm();
            var localization = BetweenMethods(source, "else if (newValue.StartsWith(\"ReloadLocalization`\"", "else if (newValue.StartsWith(\"TransferSelectSQL`\"");

            foreach (var pair in new[] { "OptionsForm|Options", "SchemaBrowserForm|SchemaBrowser", "SqlHistoryForm|SqlHistory", "OracleCreateTableWizardForm|CreateTable" })
            {
                var parts = pair.Split('|');
                var pattern = @"localizationTarget is " + parts[0] + @"\)\s*\{\s*tabPage\.Title = MyGlobal\." + parts[1] + "TabName;";

                Assert.IsTrue(Regex.IsMatch(localization, pattern), pair);
            }

            Assert.IsFalse(Regex.IsMatch(localization, @"TabName_Before|\.Tag\b"));
            Assert.IsTrue(localization.Contains("UpdateTabList();"));
            Assert.IsTrue(localization.Contains("UpdateTabTooltip(ResolveTabTooltip(tabControl1.SelectedTab));"));
        }

        [TestMethod]
        public void TABRC2B3B05_SpecialTabLookupUsesFormTypeAndRetiresTabPageTag()
        {
            var source = ReadMainForm();
            var lookup = BetweenMethods(source, "private bool TrySelectSpecialTab<TForm>()", "private bool CheckOpenedFilePath(");
            var create = BetweenMethods(source, "private void CreateNewTab(", "private string CheckTabNameExist()");
            var newEditorLookup = BetweenMethods(source, "private bool CheckTabNameExist(string tabName)", "private bool TrySelectSpecialTab<TForm>()");

            Assert.IsTrue(lookup.Contains("tabPage.Control is TForm form"));
            Assert.IsFalse(Regex.IsMatch(lookup + newEditorLookup, @"\.Title\b|\.Tag\b"));
            Assert.IsTrue(newEditorLookup.Contains("query.EditorTabDisplayName"));
            Assert.IsFalse(create.Contains("tabPage.Tag"));

            foreach (var formType in new[] { "OptionsForm", "SchemaBrowserForm", "SqlHistoryForm", "OracleCreateTableWizardForm" })
            {
                Assert.IsTrue(source.Contains("TrySelectSpecialTab<" + formType + ">()"), formType);
            }
        }

        [TestMethod]
        public void TABRC2B3B06_TabListKeepsAllRowsButOnlyNotifiesEditorsAndReservesNamesSeparately()
        {
            var source = ReadMainForm();
            var list = BetweenMethods(source, "private void UpdateTabList()", "private static class MapColumn");
            var autoComplete = BetweenMethods(source, "private void HideAutoCompleteList()", "private void LoadPostgreSqlDatabase()");
            var settings = BetweenMethods(source, "else if (newValue.StartsWith(\"ReloadQueryEditorSetting`\"", "else if (newValue.StartsWith(\"F7EditorStructuralDdl`\"");

            Assert.IsTrue(Regex.IsMatch(list, @"if\s*\(theTab\.Control is QueryForm\)\s*\{[^}]*\}\s*var row = MyGlobal\.dtTabList\.NewRow\(\);"));
            Assert.IsTrue(list.Contains("row[\"Tab\"] = tabTitle;"));
            Assert.IsTrue(list.Contains("row[\"AccessibleDescription\"] = tabAccessibleDescription;"));
            Assert.IsTrue(list.Contains("RefreshSharedTabList();"));
            Assert.IsTrue(autoComplete.Contains("if (theTab.Control is QueryForm)"));
            Assert.IsTrue(settings.Contains("tabPage.Control is QueryForm || tabPage.Control is SchemaBrowserForm"));
            Assert.AreEqual(1, Regex.Matches(source, @"SpecialTabName\s*\.\s*Contains\s*\(").Count);
            Assert.IsTrue(source.Contains("form.IsNameAllowed = name => SpecialTabName == null || !SpecialTabName.Contains(name);"));
        }
    }
}
