using System;
using JasonLibrary.Core.Database.Enums;
using JasonLibrary.Core.Text.Formatting;
using JasonQuery.Core.Database.Connection;
using JasonQuery.Core.QueryEngine.Editor.Editing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JasonQuery.Tests.Core.QueryEngine.Editor.Editing
{
    [TestClass]
    public sealed class EditorSqlFormatterServiceTests
    {
        [DataTestMethod]
        [DataRow(DataSourceType.None, DatabaseProviderKind.Unknown)]
        [DataRow(DataSourceType.Oracle, DatabaseProviderKind.Oracle)]
        [DataRow(DataSourceType.PostgreSql, DatabaseProviderKind.PostgreSql)]
        [DataRow(DataSourceType.SqlServer, DatabaseProviderKind.SqlServer)]
        [DataRow(DataSourceType.MySql, DatabaseProviderKind.MySql)]
        public void TryFormatSelection_MapsDataSourceAndUsesDefaultEngine(DataSourceType dataSourceType, DatabaseProviderKind expectedProviderKind)
        {
            var editor = CreateEditor("select 1");
            var options = new SqlFormatOptions();
            var formatCallCount = 0;

            var service = new EditorSqlFormatterService
            (
                editor,
                request =>
                {
                    formatCallCount++;
                    Assert.AreEqual(expectedProviderKind, request.ProviderKind);
                    Assert.AreEqual(SqlFormatterEngineKind.Unknown, request.EngineKind);
                    Assert.AreSame(options, request.Options);

                    return SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, request.Sql);
                }
            );

            var formatted = service.TryFormatSelection
            (
                dataSourceType,
                SqlFormatterEngineKind.Unknown,
                options,
                out var errorMessage
            );

            Assert.IsTrue(formatted);
            Assert.AreEqual(1, formatCallCount);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void TryFormatSelection_Success_ReplacesAndReselectsFormattedSql()
        {
            const string formattedSql = "SELECT\r\n    1";

            var editor = new FakeTextEditor
            {
                Text = "prefix select 1 suffix",
                SelectionStart = 7,
                SelectionEnd = 15
            };

            var service = CreateService
            (
                editor,
                request => SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, formattedSql)
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsTrue(formatted);
            Assert.AreEqual("prefix " + formattedSql + " suffix", editor.Text);
            Assert.AreEqual(7, editor.SelectionStart);
            Assert.AreEqual(7 + formattedSql.Length, editor.SelectionEnd);
            Assert.AreEqual(1, editor.ReplaceSelectionCount);
            Assert.AreEqual(1, editor.ScrollCaretCount);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void TryFormatSelection_FailedResult_PreservesEditorAndSelection()
        {
            var editor = CreateEditor("select 1");

            var service = CreateService
            (
                editor,
                request => SqlFormatResult.Failed(SqlFormatterEngineKind.Hogimn, "changed", "unsafe output")
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsFalse(formatted);
            Assert.AreEqual("select 1", editor.Text);
            Assert.AreEqual(0, editor.SelectionStart);
            Assert.AreEqual(8, editor.SelectionEnd);
            Assert.AreEqual(0, editor.ReplaceSelectionCount);
            Assert.AreEqual(0, editor.ScrollCaretCount);
            Assert.AreEqual("unsafe output", errorMessage);
        }

        [TestMethod]
        public void TryFormatSelection_WhiteSpaceSelection_DoesNotCallFormatter()
        {
            var editor = CreateEditor(" \t\r\n");
            var formatCallCount = 0;

            var service = CreateService
            (
                editor,
                request =>
                {
                    formatCallCount++;
                    return SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, request.Sql);
                }
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsFalse(formatted);
            Assert.AreEqual(0, formatCallCount);
            Assert.AreEqual(string.Empty, errorMessage);
            Assert.AreEqual(0, editor.ReplaceSelectionCount);
        }

        [TestMethod]
        public void TryFormatSelection_NullResult_PreservesEditor()
        {
            var editor = CreateEditor("select 1");
            var service = CreateService(editor, request => null);

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsFalse(formatted);
            Assert.AreEqual("select 1", editor.Text);
            Assert.AreEqual(0, editor.ReplaceSelectionCount);
            StringAssert.Contains(errorMessage, "no result");
        }

        [TestMethod]
        public void TryFormatSelection_EmptySuccessfulResult_PreservesEditor()
        {
            var editor = CreateEditor("select 1");

            var service = CreateService
            (
                editor,
                request => SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, string.Empty)
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsFalse(formatted);
            Assert.AreEqual("select 1", editor.Text);
            Assert.AreEqual(0, editor.ReplaceSelectionCount);
            StringAssert.Contains(errorMessage, "empty SQL");
        }

        [TestMethod]
        public void TryFormatSelection_UnchangedResult_DoesNotCreateUndoEdit()
        {
            var editor = CreateEditor("select 1");

            var service = CreateService
            (
                editor,
                request => SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, request.Sql)
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.PostgreSql,
                SqlFormatterEngineKind.Unknown,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsTrue(formatted);
            Assert.AreEqual(0, editor.ReplaceSelectionCount);
            Assert.AreEqual(0, editor.ScrollCaretCount);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void TryFormatSelection_RequestedOverride_IsPassedToCoordinator()
        {
            var editor = CreateEditor("select 1");

            var service = CreateService
            (
                editor,
                request =>
                {
                    Assert.AreEqual(SqlFormatterEngineKind.Hogimn, request.EngineKind);
                    return SqlFormatResult.Succeeded(SqlFormatterEngineKind.Hogimn, request.Sql);
                }
            );

            var formatted = service.TryFormatSelection
            (
                DataSourceType.SqlServer,
                SqlFormatterEngineKind.Hogimn,
                new SqlFormatOptions(),
                out var errorMessage
            );

            Assert.IsTrue(formatted);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void Constructor_NullEditor_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => new EditorSqlFormatterService(null, request => null)
            );
        }

        [TestMethod]
        public void Constructor_NullFormatter_ThrowsArgumentNullException()
        {
            Assert.ThrowsException<ArgumentNullException>
            (
                () => new EditorSqlFormatterService(CreateEditor("select 1"), null)
            );
        }

        private static EditorSqlFormatterService CreateService(FakeTextEditor editor, Func<SqlFormatRequest, SqlFormatResult> format)
        {
            return new EditorSqlFormatterService(editor, format);
        }

        private static FakeTextEditor CreateEditor(string text)
        {
            return new FakeTextEditor
            {
                Text = text,
                SelectionStart = 0,
                SelectionEnd = text.Length
            };
        }

        private sealed class FakeTextEditor : ITextEditor
        {
            public string Text { get; set; } = string.Empty;

            public string SelectedText
            {
                get
                {
                    var start = Math.Min(SelectionStart, SelectionEnd);
                    var end = Math.Max(SelectionStart, SelectionEnd);

                    return Text.Substring(start, end - start);
                }
            }

            public int SelectionStart { get; set; }

            public int SelectionEnd { get; set; }

            public int CurrentPosition { get; set; }

            public int CurrentLine => 0;

            public object Tag { get; set; }

            public int ReplaceSelectionCount { get; private set; }

            public int ScrollCaretCount { get; private set; }

            public void ReplaceSelection(string text)
            {
                var start = Math.Min(SelectionStart, SelectionEnd);
                var end = Math.Max(SelectionStart, SelectionEnd);

                Text = Text.Substring(0, start) + text + Text.Substring(end);
                SelectionStart = start;
                SelectionEnd = start + text.Length;
                ReplaceSelectionCount++;
            }

            public void Select()
            {
            }

            public void ScrollCaret()
            {
                ScrollCaretCount++;
            }

            public string GetTextRange(int start, int length)
            {
                return Text.Substring(start, length);
            }
        }
    }
}