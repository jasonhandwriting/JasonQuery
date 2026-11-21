using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.UI.LeftPanel
{
    //F7-U4B-A-v1.1: source-level Golden for QueryForm (legacy F7) -> MainForm parity.
    //Do not instantiate C1 controls or connect to any database in these tests.
    //This is a wiring guard, NOT a substitute for Normal/Dark GUI acceptance.
    [TestClass]
    [TestCategory("F7U4BA")]
    public sealed class SchemaExplorerVisualParityGoldenTests
    {
        private const string VisualMethodName = "ApplySharedSchemaExplorerVisualParity";

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

        private static string NewGridSource()
        {
            return Source("JasonQuery", "UI", "Forms", "MainForm.SharedSchemaExplorerGrid.cs");
        }

        private static string MainFormSource()
        {
            return Source("JasonQuery", "UI", "Forms", "MainForm.cs");
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

        private static string VisualMethod()
        {
            return MethodBody(NewGridSource(), "private void " + VisualMethodName + "()");
        }

        [TestMethod]
        public void A01_LegacyF7HasKnownEffectiveFontAndRowHeightBaseline_GREEN()
        {
            //The legacy F7 shown beside the shared F7 belongs to QueryForm, not to the standalone SchemaBrowserForm and its GridZoom() path.
            var load = Source("JasonQuery", "UI", "Forms", "QueryForm.FormLoad.cs");
            var designer = Source("JasonQuery", "UI", "Forms", "QueryForm.Designer.cs");

            Assert.Contains("GridHelper.SetGridVisualStyle(c1GridSchemaBrowser, 10);", load);
            Assert.Contains("this.c1GridSchemaBrowser.RowHeight = 19;", designer);
            Assert.Contains("this.c1GridSchemaBrowser.Font = new System.Drawing.Font(\"微軟正黑體\", 9F", designer);
        }

        [TestMethod]
        public void A02_SharedF7RetainsTheQualifiedHighlightRowBaseline_GREEN()
        {
            var source = NewGridSource();
            var method = MethodBody(source, "private void InitializeSharedSchemaExplorerGrid()");

            Assert.Contains("grid.MarqueeStyle = MarqueeEnum.HighlightRow;", method);
            Assert.Contains("grid.Styles[\"HighlightRow\"].BackColor = SystemColors.Highlight;", method);
            Assert.Contains("grid.Styles[\"HighlightRow\"].ForeColor = SystemColors.HighlightText;", method);
        }

        [TestMethod]
        public void A03_VisualParityRunsAfterConnectionGridConfigIsLoaded_GREEN()
        {
            var main = MainFormSource();
            var initialLoad = MethodBody(main, "private void MainForm_Load(object sender, EventArgs e)");
            var connectionLoad = MethodBody(main, "private bool LoadConnectionForm()");
            var settings = connectionLoad.IndexOf("LoadDefaultSetting();", StringComparison.Ordinal);
            var parity = connectionLoad.IndexOf(VisualMethodName + "();", StringComparison.Ordinal);
            var cleanup = connectionLoad.IndexOf("TryAutoDeleteSqlHistoryForCurrentConnection();", StringComparison.Ordinal);

            Assert.DoesNotContain(initialLoad, VisualMethodName + "();", "Do not apply F7 styles before per-connection GridConfig exists.");
            Assert.IsTrue(settings >= 0 && parity > settings && cleanup > parity, "Apply F7 style after LoadDefaultSetting, before connection-driven QueryForm work.");
        }

        [TestMethod]
        public void A04_SharedF7UsesQueryFormGridStyleAndLoadedFont_GREEN()
        {
            var method = VisualMethod();
            var helper = Source("JasonQuery", "UI", "Helpers", "GridHelper.cs");
            var main = MainFormSource();

            Assert.DoesNotContain(method, "GridHelper.SetGridVisualStyle(grid);", "Legacy QueryForm F7 does not apply a separate C1 VisualStyle.");
            Assert.Contains("GridHelper.SetGridVisualStyle(grid, 10);", method);
            Assert.Contains("c1Grid.Font = new Font(MyLibrary.GridFontName, fontSize", helper);
            Assert.Contains("MyLibrary.GridFontName = GetStringValue(\"GridConfig\", \"FontName\");", main);
            Assert.DoesNotContain(method, "grid.GroupStyle.Font =", "Do not impose a larger group font than legacy QueryForm.");
            Assert.DoesNotContain(method, "const float fontSize =", "The legacy shared-grid font is set by the existing helper.");
        }

        [TestMethod]
        public void A05_SharedF7UsesStableQueryFormBaseDimensions_RED()
        {
            var source = NewGridSource();
            var method = VisualMethod();

            Assert.Contains("_sharedF7VisualBaseRowHeight", source);
            Assert.Contains("_sharedF7VisualBaseRecordSelectorWidth", source);
            Assert.Contains("grid.RowHeight =", method);
            Assert.Contains("grid.RecordSelectorWidth =", method);
            Assert.Contains("grid.RowHeight = _sharedF7VisualBaseRowHeight;", method);
            Assert.Contains("grid.RecordSelectorWidth = _sharedF7VisualBaseRecordSelectorWidth;", method);
            Assert.DoesNotContain(method, "pcnt", "The QueryForm F7 tree does not use SchemaBrowserForm.GridZoom().");
        }

        [TestMethod]
        public void A06_DarkModeUsesEstablishedF7ThemeAndBackground_RED()
        {
            var method = VisualMethod();

            Assert.Contains("MyLibrary.IsDarkMode", method);
            Assert.Contains("#2D2D30", method);
            Assert.DoesNotContain(method, "c1ThemeController1.SetTheme(grid,", "Legacy QueryForm F7 does not call SetTheme on the schema grid.");
            Assert.Contains("grid.RowDivider.Color =", method);
        }

        [TestMethod]
        public void A07_NoZebraStripesAndNoHighlightCellRegression_GREEN()
        {
            var method = VisualMethod();
            var helper = method.IndexOf("GridHelper.SetGridVisualStyle(grid, 10);", StringComparison.Ordinal);
            var odd = method.IndexOf("grid.OddRowStyle.BackColor = rowBackground;", StringComparison.Ordinal);
            var even = method.IndexOf("grid.EvenRowStyle.BackColor = rowBackground;", StringComparison.Ordinal);
            var highlight = method.IndexOf("grid.MarqueeStyle = MarqueeEnum.HighlightRow;", StringComparison.Ordinal);

            Assert.IsTrue(helper >= 0 && odd > helper && even > helper && highlight > helper, "Undo helper row colors and HighlightCell changes after applying the legacy font style.");
            Assert.DoesNotContain(method, "GridOddRowBackColor");
            Assert.DoesNotContain(method, "GridEvenRowBackColor");
            Assert.DoesNotContain(method, "MarqueeEnum.HighlightCell");
            Assert.Contains("grid.OddRowStyle.ForeColor = rowForeground;", method);
            Assert.Contains("grid.EvenRowStyle.ForeColor = rowForeground;", method);
            Assert.Contains("grid.GroupStyle.ForeColor = rowForeground;", method);
            Assert.Contains("grid.Styles[\"HighlightRow\"].ForeColor = SystemColors.HighlightText;", method);
        }

        [TestMethod]
        public void A08_VisualParityMustNotRebindOrExecuteDatabaseWork_RED()
        {
            var method = VisualMethod();

            foreach (var forbidden in new[] { "DatabaseSqlExecutor", "DataSource =", "GroupedColumns", "Commit(", "Rollback(", "ExecuteNonQuery", "RefreshSchema", "SelectedRows.Clear", "Rows.Clear" })
            {
                Assert.IsFalse(method.Contains(forbidden), "Visual-only F7 method must not change metadata, connection, transaction or selection: " + forbidden);
            }
        }

        [TestMethod]
        public void A09_FetchRowStyleSubscribesOnlyOnceAndDetachesOnDispose_GREEN()
        {
            var source = NewGridSource();
            var initialize = MethodBody(source, "private void InitializeSharedSchemaExplorerGrid()");
            var dispose = MethodBody(source, "private void DisposeSharedSchemaExplorerGrid()");

            const string attach = "grid.FetchRowStyle += SharedSchemaExplorerGrid_FetchRowStyle;";
            const string detach = "grid.FetchRowStyle -= SharedSchemaExplorerGrid_FetchRowStyle;";

            Assert.Contains("if (_sharedSchemaExplorerGrid != null)", initialize);
            Assert.Contains(attach, initialize);
            Assert.AreEqual(initialize.IndexOf(attach, StringComparison.Ordinal), initialize.LastIndexOf(attach, StringComparison.Ordinal), "Subscribe only once per initialized grid.");
            Assert.Contains(detach, dispose);
            Assert.AreEqual(dispose.IndexOf(detach, StringComparison.Ordinal), dispose.LastIndexOf(detach, StringComparison.Ordinal), "Detach exactly once during disposal.");
        }

        [TestMethod]
        public void A10_FetchRowStyleGuardsSplitBoundsAndOnlyStylesDataRows_GREEN()
        {
            var method = MethodBody(NewGridSource(), "private void SharedSchemaExplorerGrid_FetchRowStyle(");

            Assert.Contains("ReferenceEquals(grid, _sharedSchemaExplorerGrid)", method);
            Assert.Contains("e.CellStyle == null", method);
            Assert.Contains("e.Row < 0", method);
            Assert.Contains("e.Split < 0", method);
            Assert.Contains("e.Split >= grid.Splits.Count", method);
            Assert.Contains("grid.Splits[e.Split].Rows", method);
            Assert.Contains("e.Row >= rows.Count", method);
            Assert.Contains("rows[e.Row].RowType != RowTypeEnum.DataRow", method);
            Assert.DoesNotContain(method, "GetDataBoundItem", "Grouped view rows must not rely on a DataRowView cast for foreground styling.");
        }

        [TestMethod]
        public void A11_FetchRowStyleUsesLegacyNormalAndDarkColumnColors_GREEN()
        {
            var method = MethodBody(NewGridSource(), "private void SharedSchemaExplorerGrid_FetchRowStyle(");

            Assert.Contains("e.CellStyle.ForeColor = MyLibrary.IsDarkMode ? Color.Yellow : Color.Blue;", method);
            Assert.DoesNotContain(method, "Color.Black", "Legacy F7 Column rows are blue in Normal Mode.");
            Assert.DoesNotContain(method, "Color.White", "Legacy F7 Column rows are yellow in Dark Mode.");
        }

        [TestMethod]
        public void A12_FetchRowStyleIsPresentationOnlyAndPreservesSelectionContract_GREEN()
        {
            var source = NewGridSource();
            var handler = MethodBody(source, "private void SharedSchemaExplorerGrid_FetchRowStyle(");
            var initialize = MethodBody(source, "private void InitializeSharedSchemaExplorerGrid()");
            var visual = VisualMethod();

            Assert.Contains("e.CellStyle.ForeColor =", handler);

            foreach (var forbidden in new[]
            {
                "DatabaseSqlExecutor", "DataSource =", "GroupedColumns", "ExecuteNonQuery",
                "Commit(", "Rollback(", "RefreshSchema", "SelectedRows.", "Rows.Clear",
                "_sharedSchemaMetadataSnapshot", "MarqueeStyle =", "GetDataBoundItem"
            })
            {
                Assert.DoesNotContain(handler, forbidden, "Row styling must never change metadata, database or selection state: " + forbidden);
            }

            Assert.Contains("grid.MarqueeStyle = MarqueeEnum.HighlightRow;", initialize);
            Assert.Contains("grid.MarqueeStyle = MarqueeEnum.HighlightRow;", visual);
            Assert.Contains("grid.Styles[\"HighlightRow\"].ForeColor = SystemColors.HighlightText;", visual);
        }
    }
}
