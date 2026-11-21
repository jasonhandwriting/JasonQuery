using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4B-B: source-only GOLDEN RED for integrated F7 context-menu visual parity.
    //No WinForms grid instantiation, no metadata query and no database execution.
    [TestClass]
    [TestCategory("F7U4BB")]
    public sealed class SchemaExplorerContextMenuVisualParityGoldenTests
    {
        private static string Source(params string[] parts)
        {
            foreach (var origin in new[] { AppDomain.CurrentDomain.BaseDirectory, Directory.GetCurrentDirectory() })
            {
                var directory = new DirectoryInfo(origin);

                for (var depth = 0; depth < 12 && directory != null; depth++)
                {
                    var path = Path.Combine(directory.FullName, Path.Combine(parts));

                    if (File.Exists(path))
                    {
                        return File.ReadAllText(path);
                    }

                    directory = directory.Parent;
                }
            }

            Assert.Fail("Source file not found: " + Path.Combine(parts));
            return string.Empty;
        }

        private static string EventsSource()
        {
            return Source("JasonQuery", "UI", "Forms", "MainForm.SharedSchemaExplorerEvents.cs");
        }

        private static string MethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "EXPECTED RED: missing method " + signature);

            var open = source.IndexOf('{', start + signature.Length);

            Assert.IsTrue(open >= 0, "Missing opening brace: " + signature);

            var depth = 0;

            for (var i = open; i < source.Length; i++)
            {
                if (source[i] == '{')
                {
                    depth++;
                }
                else if (source[i] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(open, i - open + 1);
                    }
                }
            }

            Assert.Fail("Missing closing brace: " + signature);
            return string.Empty;
        }

        private static string CaseSection(string method, string name)
        {
            var marker = "case SharedSchemaExplorerMenuCommand." + name + ":";
            var start = method.IndexOf(marker, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "EXPECTED RED: missing case " + marker);

            var nextCase = method.IndexOf("case SharedSchemaExplorerMenuCommand.", start + marker.Length, StringComparison.Ordinal);
            var nextDefault = method.IndexOf("default:", start + marker.Length, StringComparison.Ordinal);
            var end = method.Length;

            if (nextCase >= 0 && nextCase < end)
            {
                end = nextCase;
            }

            if (nextDefault >= 0 && nextDefault < end)
            {
                end = nextDefault;
            }

            return method.Substring(start, end - start);
        }

        [TestMethod]
        public void B01_LegacyQueryFormF7DeclaresTheExpectedMenuImages_GREEN()
        {
            var legacy = Source("JasonQuery", "UI", "Helpers", "UIHelper.cs");

            foreach (var image in new[]
            {
                "Show SQL Statement 16x16.ico", "Schema Browser 16x16.ico",
                "Generate SQL Statement 16x16.ico", "Table Comment.ico",
                "Table Drop.ico", "Table Rename.ico", "Table Truncate.ico",
                "Column Add.ico", "Column Comment.ico", "Column Drop.ico",
                "Column Rename.ico", "Copy 16x16.ico", "Paste2Editor 16x16.ico"
            })
            {
                Assert.Contains("IconManager.GetImage(MyGlobal.IconLibrary, \"" + image + "\")", legacy, "The image name must come from the established legacy F7 implementation: " + image);
            }
        }

        [TestMethod]
        public void B02_SharedMenuMapsEveryLegacyTableAndColumnImage_RED()
        {
            var method = MethodBody(EventsSource(), "private static string GetSharedSchemaMenuIconName(SharedSchemaExplorerMenuCommand command)");

            var expected = new[]
            {
                new[] { "ShowDdl", "Show SQL Statement 16x16.ico" },
                new[] { "OpenSchemaBrowser", "Schema Browser 16x16.ico" },
                new[] { "GenerateSqlStatement", "Generate SQL Statement 16x16.ico" },
                new[] { "CommentTable", "Table Comment.ico" },
                new[] { "DropTable", "Table Drop.ico" },
                new[] { "RenameTable", "Table Rename.ico" },
                new[] { "TruncateTable", "Table Truncate.ico" },
                new[] { "AddColumn", "Column Add.ico" },
                new[] { "CommentColumn", "Column Comment.ico" },
                new[] { "DropColumn", "Column Drop.ico" },
                new[] { "RenameColumn", "Column Rename.ico" },
                new[] { "CopyToClipboard", "Copy 16x16.ico" },
                new[] { "PasteSelectAll", "Paste2Editor 16x16.ico" },
                new[] { "PasteName", "Paste2Editor 16x16.ico" }
            };

            foreach (var pair in expected)
            {
                var section = CaseSection(method, pair[0]);

                Assert.Contains("return \"" + pair[1] + "\";", section, "F7-U4B-B legacy icon mismatch for " + pair[0]);
            }
        }

        [TestMethod]
        public void B03_SharedMenuAssignsIconsWithoutChangingCommandAvailability_RED()
        {
            var mouseUp = MethodBody(EventsSource(), "private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)");

            Assert.Contains("GetSharedSchemaMenuIconName(command)", mouseUp);
            Assert.Contains("IconManager.GetImage(MyGlobal.IconLibrary,", mouseUp);
            Assert.Contains("item.Image =", mouseUp);

            var create = mouseUp.IndexOf("var item = new ToolStripMenuItem(", StringComparison.Ordinal);
            var assign = mouseUp.IndexOf("item.Image =", StringComparison.Ordinal);
            var enabled = mouseUp.IndexOf("item.Enabled =", StringComparison.Ordinal);

            Assert.IsTrue(create >= 0 && assign > create && enabled > assign, "Icon presentation must be independent of command availability and must not enable disabled actions.");
        }

        [TestMethod]
        public void B04_SharedMenuRetainsTableColumnAndCopySeparators_GREEN()
        {
            var mouseUp = MethodBody(EventsSource(), "private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)");

            Assert.Contains("heading.Enabled = false;", mouseUp);
            Assert.Contains("if (command == SharedSchemaExplorerMenuCommand.OpenSchemaBrowser || command == SharedSchemaExplorerMenuCommand.CommentTable", mouseUp);
            Assert.Contains("command == SharedSchemaExplorerMenuCommand.AddColumn && selection.Kind != SchemaExplorerSelectionKind.ColumnInfo", mouseUp);
            Assert.Contains("if (command == SharedSchemaExplorerMenuCommand.CopyToClipboard && menu.Items.Count > 2)", mouseUp);
            Assert.Contains("menu.Items.Add(\"-\");", mouseUp);
        }

        [TestMethod]
        public void B05_ShowDdlCaptionUsesLocalizedTextAndTrailingEllipsis_RED()
        {
            var caption = MethodBody(EventsSource(), "private static string GetSharedSchemaMenuCaption(");
            var showDdl = CaseSection(caption, "ShowDdl");

            Assert.Contains("LocalizationHelper.GetLanguageString(\"Show DDL Statement\", \"Global\", \"Global\", \"msg\", \"ShowDDL\", \"Text\")", showDdl);
            Assert.Contains("return $\"{text}...\";", showDdl, "Append ellipsis only to this menu caption; do not alter the shared localization resource.");
        }

        [TestMethod]
        public void B06_MenuContinuesToUseTheExistingSecurityPolicies_GREEN()
        {
            var mouseUp = MethodBody(EventsSource(), "private void SharedSchemaExplorerGridMouseUp(object sender, MouseEventArgs e)");

            foreach (var required in new[]
            {
                "SharedSchemaExplorerContextMenuPolicy.Plan(context.ActiveTarget, selection)",
                "SharedSchemaExplorerPastePolicy.IsPasteCommand(command)",
                "SharedSchemaDdlInteractionPolicy.CanOpenGenerator",
                "SharedSchemaExplorerObjectCommandPolicy.CanExecute",
                "item.Enabled = isCopy || (isPaste && pasteReceiverReady) || isObjectAction;",
                "SharedSchemaObjectMenuClick", "SharedSchemaPasteClick", "SharedSchemaCopyNameClick",
                "menu.Show(grid, new Point(e.X, e.Y));"
            })
            {
                Assert.Contains(required, mouseUp, "Preserve the existing selection / availability / click gate: " + required);
            }
        }

        [TestMethod]
        public void B07_MainFormLeftTabsReuseGlobalOptionsTabAppearance_GREEN()
        {
            var shared = Source("JasonQuery", "UI", "Forms", "MainForm.SharedLeftPanelHost.cs");
            var method = MethodBody(shared, "private void ApplySharedLeftPanelTabAppearance()");

            Assert.Contains("if (c1DockingTabLeftPanel == null || c1DockingTabLeftPanel.IsDisposed)", method);
            Assert.Contains("UIHelper.SetDockingTabColor", method);
            Assert.Contains("c1DockingTabLeftPanel,", method);

            foreach (var color in new[] { "ColorOptionsTabActiveBackColor", "ColorOptionsTabActiveForeColor", "ColorOptionsTabInactiveForeColor" })
            {
                Assert.Contains("string.IsNullOrWhiteSpace(MyLibrary." + color + ")", method);
                Assert.Contains("ColorTranslator.FromHtml(MyLibrary." + color + ")", method);
            }
        }

        [TestMethod]
        public void B08_MainFormReappliesLeftTabAppearanceAfterSettingsAndLocalization_GREEN()
        {
            var main = Source("JasonQuery", "UI", "Forms", "MainForm.cs");
            var localization = MethodBody(main, "private void ApplyLocalization()");
            var localizationText = localization.IndexOf("ApplySharedSchemaExplorerLocalization();", StringComparison.Ordinal);
            var localizationAppearance = localization.IndexOf("ApplySharedLeftPanelTabAppearance();", StringComparison.Ordinal);

            Assert.IsTrue(localizationText >= 0 && localizationAppearance > localizationText, "Left-tab appearance must be applied after the shared-tab localization pass.");

            var connection = MethodBody(main, "private bool LoadConnectionForm()");
            var settings = connection.IndexOf("LoadDefaultSetting();", StringComparison.Ordinal);
            var grid = connection.IndexOf("ApplySharedSchemaExplorerVisualParity();", StringComparison.Ordinal);
            var appearance = connection.IndexOf("ApplySharedLeftPanelTabAppearance();", StringComparison.Ordinal);
            var cleanup = connection.IndexOf("TryAutoDeleteSqlHistoryForCurrentConnection();", StringComparison.Ordinal);

            Assert.IsTrue(settings >= 0 && grid > settings && appearance > grid && cleanup > appearance, "Reapply global tab colors after the connection theme/grid settings are loaded.");
        }

        [TestMethod]
        public void B09_OptionsGlobalOverviewLabelAvoidsFixedLightBackground_GREEN()
        {
            var designer = Source("JasonQuery", "UI", "Forms", "OptionsForm.Designer.cs");
            var start = designer.IndexOf("// lblGlobalOverview", StringComparison.Ordinal);
            var end = designer.IndexOf("// grpCommitRollbackIcon", start >= 0 ? start : 0, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0 && end > start, "Options Global Overview Designer section is required.");

            var label = designer.Substring(start, end - start);

            Assert.Contains("this.lblGlobalOverview.BackColor = System.Drawing.Color.Transparent;", label, "The global-settings label must not retain a fixed pale background in Dark Mode.");
        }

    }
}
