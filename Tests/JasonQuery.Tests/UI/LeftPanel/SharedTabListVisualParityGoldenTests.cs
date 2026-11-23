using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F8-E2: source-level guard for legacy F8 -> MainForm visual parity.
    //This is not a substitute for Light/Dark mode GUI qualification.
    [TestClass]
    [TestCategory("F8E2")]
    public sealed class SharedTabListVisualParityGoldenTests
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

        private static string FormSource(string filename)
        {
            return Source("JasonQuery", "UI", "Forms", filename);
        }

        private static string MethodBody(string source, string signature)
        {
            var start = source.IndexOf(signature, StringComparison.Ordinal);

            Assert.IsTrue(start >= 0, "EXPECTED RED: missing " + signature);

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

        [TestMethod]
        public void E01_LegacyF8UsesRuntimeVisualStyleAndTheme_GREEN()
        {
            var queryForm = FormSource("QueryForm.cs");
            var localization = FormSource("QueryForm.Localization.cs");
            var helper = Source("JasonQuery", "UI", "Helpers", "GridHelper.cs");

            Assert.Contains("ApplyGridVisualStyle(c1GridTabList);", queryForm);
            Assert.Contains("GridHelper.SetGridVisualStyle(grid);", queryForm);
            Assert.Contains("SetTheme(c1GridTabList, \"VS2013Dark\");", localization);
            Assert.Contains("SetTheme(c1GridTabList, \"(default)\");", localization);
            Assert.Contains("var style = MyLibrary.IsDarkMode ? \"Office 2010 Black\" : MyLibrary.GridVisualStyle;", helper);
        }

        [TestMethod]
        public void E02_DesignerOwnedSharedF8RetainsReadOnlyAndDockBaseline_GREEN()
        {
            var designer = FormSource("MainForm.designer.cs");
            var shared = FormSource("MainForm.SharedTabList.cs");

            Assert.Contains("this.c1GridSharedTabList.RowHeight = 19;", designer);
            Assert.Contains("this.c1GridSharedTabList.Dock = System.Windows.Forms.DockStyle.Fill;", designer);
            Assert.Contains("this.c1GridSharedTabList.PropBag = resources.GetString(\"c1GridSharedTabList.PropBag\");", designer);

            var refresh = MethodBody(shared, "private void RefreshSharedTabList()");

            Assert.Contains("this.c1GridSharedTabList.AllowUpdate = false;", designer);
            Assert.Contains("this.c1GridSharedTabList.AllowUpdateOnBlur = false;", designer);
            Assert.Contains("c1GridSharedTabList.AllowUpdate = false;", refresh);
            Assert.Contains("SharedTabListSnapshotPolicy.Create(MyGlobal.dtTabList)", refresh);
        }

        [TestMethod]
        public void E03_SharedF8VisualParityIsReappliedAfterConfigAndLocalization_RED()
        {
            var main = FormSource("MainForm.cs");
            var settings = MethodBody(main, "private void LoadDefaultSetting()");
            var localization = MethodBody(main, "private void ApplyLocalization()");

            var loaded = settings.IndexOf("LoadGridConfig();", StringComparison.Ordinal);
            var styled = settings.IndexOf("ApplySharedTabListVisualParity();", StringComparison.Ordinal);
            var caption = localization.IndexOf("ApplySharedTabListLocalization();", StringComparison.Ordinal);
            var reapply = localization.IndexOf("ApplySharedTabListVisualParity();", StringComparison.Ordinal);

            Assert.IsTrue(loaded >= 0 && styled > loaded, "EXPECTED RED: apply F8 styles after GridConfig has loaded.");
            Assert.IsTrue(caption >= 0 && reapply > caption, "EXPECTED RED: reapply F8 styles during localization/theme refresh.");
        }

        [TestMethod]
        public void E04_SharedF8ParityUsesLegacyThemeAndDoesNotRebind_RED()
        {
            var shared = FormSource("MainForm.SharedTabList.cs");
            var method = MethodBody(shared, "private void ApplySharedTabListVisualParity()");

            Assert.Contains("GridHelper.SetGridVisualStyle(grid);", method);
            Assert.Contains("GridHelper.SetGridVisualStyle(grid, 10);", method);
            Assert.Contains("var originalMarqueeStyle = grid.MarqueeStyle;", method);
            Assert.Contains("grid.MarqueeStyle = originalMarqueeStyle;", method);

            var visualStyleIndex = method.IndexOf("GridHelper.SetGridVisualStyle(grid);", StringComparison.Ordinal);
            var configuredStyleIndex = method.IndexOf("GridHelper.SetGridVisualStyle(grid, 10);", StringComparison.Ordinal);
            var restoreIndex = method.IndexOf("grid.MarqueeStyle = originalMarqueeStyle;", StringComparison.Ordinal);

            Assert.IsTrue
            (
                visualStyleIndex >= 0
                && configuredStyleIndex > visualStyleIndex
                && restoreIndex > configuredStyleIndex,
                "Configured font/colors must follow VisualStyle, then restore selection behavior."
            );

            Assert.IsTrue
            (
                Regex.IsMatch
                (
                    method,
                    @"c1ThemeController1\s*\.\s*SetTheme\s*\(\s*grid\s*,",
                    RegexOptions.CultureInvariant
                ),
                "The shared F8 theme must be applied to the expected grid."
            );

            Assert.Contains("MyLibrary.IsDarkMode", method);
            Assert.Contains("VS2013Dark", method);
            Assert.Contains("#2D2D30", method);
            Assert.Contains("#F0F0F0", method);

            foreach (var forbidden in new[] { "DataSource =", "TryActivateTab(", "Commit(", "Rollback(", "ExecuteNonQuery", "Rows.Clear", "SelectedRows.Clear", "RowHeight =", "AllowUpdate = true", "AllowUpdateOnBlur = true" })
            {
                Assert.DoesNotContain
                (
                    method,
                    forbidden,
                    "Visual parity must not change navigation, data, transaction or selection: " + forbidden
                );
            }
        }

        [TestMethod]
        public void E05_DarkModeRespectsRequestedFontSize_AndQueryGridKeepsConfiguration()
        {
            var helper = Source("JasonQuery", "UI", "Helpers", "GridHelper.cs");

            var method = MethodBody
            (
                helper,
                "public static void SetGridVisualStyle(C1TrueDBGrid c1Grid, float fontSize)"
            );

            Assert.Contains
            (
                "c1Grid.Font = new Font(MyLibrary.GridFontName, fontSize,",
                method
            );

            Assert.Contains
            (
                "c1Grid.HeadingStyle.Font = new Font(gridFontName, fontSize,",
                method
            );

            Assert.DoesNotContain
            (
                method,
                "MyLibrary.GridFontSize",
                "The helper must respect the caller's fontSize in Dark Mode."
            );

            var sharedF7 = FormSource("MainForm.SharedSchemaExplorerGrid.cs");
            var sharedF8 = FormSource("MainForm.SharedTabList.cs");
            var queryLoad = FormSource("QueryForm.FormLoad.cs");

            Assert.Contains("GridHelper.SetGridVisualStyle(grid, 10);", sharedF7);
            Assert.Contains("GridHelper.SetGridVisualStyle(grid, 10);", sharedF8);

            Assert.Contains
            (
                "GridHelper.SetGridVisualStyle(c1TrueDBGrid1, MyLibrary.GridFontSize);",
                queryLoad
            );
        }
    }
}
