using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace JasonQuery.Tests.Core.Security.ConnectionCredentials
{
    [TestClass]
    public class ConnectionCredentialExportImportRegressionGuardTests
    {
        [TestMethod]
        public void Export_UsesLiveGridCheckboxStateWhenSelectingRows()
        {
            var repositoryRoot = FindRepositoryRoot();

            var exportSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionExportForm.cs"
            );

            Assert.Contains("var value = c1GridDbInfo[row, \" \"].ToString();", exportSource);
            Assert.DoesNotContain("var value = dr.GetSafeString(\" \");", exportSource, "Connection export must use the live C1 grid checkbox state instead of a detached DataRow checkbox value.");
        }

        [TestMethod]
        public void Import_WrongPasswordPath_ReturnsBeforeWorkbookProcessing()
        {
            var repositoryRoot = FindRepositoryRoot();

            var importSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionImportForm.cs"
            );

            Assert.DoesNotContain("isOpenNG", importSource, "Connection import must not rely on the legacy isOpenNG flag for wrong-password flow control.");

            var extractIndex = importSource.IndexOf
            (
                "zip.Entries.Extract(0, fileNameXls);",
                StringComparison.Ordinal
            );

            var wrongPasswordIndex = importSource.IndexOf
            (
                "\"Wrong password!\"",
                extractIndex >= 0 ? extractIndex : 0,
                StringComparison.Ordinal
            );

            var returnIndex = importSource.IndexOf
            (
                "return;",
                wrongPasswordIndex >= 0 ? wrongPasswordIndex : 0,
                StringComparison.Ordinal
            );

            var workbookLoadIndex = importSource.IndexOf
            (
                "book.Load(fileNameXls);",
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, extractIndex, "The .jqc extraction call was not found.");
            Assert.IsGreaterThan(extractIndex, wrongPasswordIndex, "The wrong-password handler was not found after extraction.");
            Assert.IsGreaterThan(wrongPasswordIndex, returnIndex, "Wrong-password handling must return immediately.");
            Assert.IsGreaterThan(returnIndex, workbookLoadIndex, "Workbook processing must remain downstream from the wrong-password return path.");
        }

        [TestMethod]
        public void ExportAndImport_TemporaryCredentialPayloadFiles_AreCleanedInFinally()
        {
            var repositoryRoot = FindRepositoryRoot();

            var exportSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionExportForm.cs"
            );

            var importSource = ReadSource
            (
                repositoryRoot,
                "JasonQuery",
                "UI",
                "Forms",
                "ConnectionImportForm.cs"
            );

            var exportDisposeIndex = exportSource.IndexOf
            (
                "book?.Dispose();",
                StringComparison.Ordinal
            );

            var exportFinallyIndex = exportSource.LastIndexOf("finally", StringComparison.Ordinal);

            var exportPayloadCleanupIndex = exportSource.IndexOf
            (
                "DeleteTempFileIfExists(fileName);",
                exportFinallyIndex >= 0 ? exportFinallyIndex : 0,
                StringComparison.Ordinal
            );

            var exportPlaceholderCleanupIndex = exportSource.IndexOf
            (
                "DeleteTempFileIfExists(originalTempFileName);",
                exportFinallyIndex >= 0 ? exportFinallyIndex : 0,
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, exportDisposeIndex, "Connection export must dispose the temporary workbook.");
            Assert.IsGreaterThan(exportDisposeIndex, exportFinallyIndex, "Export temp-file cleanup must remain downstream from workbook disposal.");
            Assert.IsGreaterThan(exportFinallyIndex, exportPayloadCleanupIndex, "Export plaintext workbook cleanup must run from finally.");
            Assert.IsGreaterThan(exportFinallyIndex, exportPlaceholderCleanupIndex, "Export temporary placeholder cleanup must run from finally.");
            Assert.Contains("TraceLogger.LogError(\"ConnectionExportTempCleanup\", ex);", exportSource);

            var importBookDisposeIndex = importSource.LastIndexOf("book.Dispose();", StringComparison.Ordinal);

            var importWorkbookCleanupIndex = importSource.IndexOf
            (
                "DeleteTempFileIfExists(fileNameXls);",
                importBookDisposeIndex >= 0 ? importBookDisposeIndex : 0,
                StringComparison.Ordinal
            );

            var importArchiveCleanupIndex = importSource.IndexOf
            (
                "DeleteTempFileIfExists(fileNameZip);",
                importBookDisposeIndex >= 0 ? importBookDisposeIndex : 0,
                StringComparison.Ordinal
            );

            Assert.IsGreaterThanOrEqualTo(0, importBookDisposeIndex, "Connection import must dispose the workbook before deleting its plaintext temp payload.");
            Assert.IsGreaterThan(importBookDisposeIndex, importWorkbookCleanupIndex, "Import plaintext workbook cleanup must run after workbook disposal.");
            Assert.IsGreaterThan(importBookDisposeIndex, importArchiveCleanupIndex, "Import temporary archive cleanup must run from the outer finally path.");
            Assert.Contains("new FileInfo(fileNameXls).Length == 0", importSource);
            Assert.Contains("TraceLogger.LogError(\"ConnectionImportTempCleanup\", ex);", importSource);
        }

        private static string FindRepositoryRoot()
        {
            var currentDirectoryRoot = FindRepositoryRootFrom(Directory.GetCurrentDirectory());

            if (!string.IsNullOrEmpty(currentDirectoryRoot))
            {
                return currentDirectoryRoot;
            }

            var baseDirectoryRoot = FindRepositoryRootFrom(AppDomain.CurrentDomain.BaseDirectory);

            if (!string.IsNullOrEmpty(baseDirectoryRoot))
            {
                return baseDirectoryRoot;
            }

            Assert.Fail("Could not locate the JasonQuery repository root for source guard tests.");
            return string.Empty;
        }

        private static string FindRepositoryRootFrom(string startPath)
        {
            if (string.IsNullOrWhiteSpace(startPath))
            {
                return null;
            }

            var directory = new DirectoryInfo(Path.GetFullPath(startPath));

            while (directory != null)
            {
                var projectFilePath = Path.Combine(directory.FullName, "JasonQuery", "JasonQuery.csproj");
                var testsDirectoryPath = Path.Combine(directory.FullName, "Tests", "JasonQuery.Tests");

                if (File.Exists(projectFilePath) && Directory.Exists(testsDirectoryPath))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }

        private static string ReadSource(string repositoryRoot, params string[] pathParts)
        {
            var sourcePath = repositoryRoot;

            foreach (var pathPart in pathParts)
            {
                sourcePath = Path.Combine(sourcePath, pathPart);
            }

            Assert.IsTrue(File.Exists(sourcePath), $"Required source file was not found: {sourcePath}");
            return File.ReadAllText(sourcePath);
        }
    }
}
