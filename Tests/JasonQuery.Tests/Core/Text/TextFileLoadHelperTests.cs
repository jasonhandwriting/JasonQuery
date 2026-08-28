using JasonQuery.Core.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Text
{
    [TestClass]
    public sealed class TextFileLoadHelperTests
    {
        [TestMethod]
        [TestCategory("Unit")]
        [DataRow(true, "UTF-8 BOM")]
        [DataRow(false, "UTF-8")]
        public void Load_WithUtf8File_ReportsExpectedEncoding(bool emitBom, string expectedEncodingName)
        {
            const string expectedText = "SELECT '測試';\r\n";
            var fileName = WriteTemporaryFile(expectedText, new UTF8Encoding(emitBom));

            try
            {
                var result = TextFileLoadHelper.Load(fileName, false);

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual(expectedEncodingName, result.EncodingName);
                Assert.AreEqual("Windows (CR LF)", result.EndOfLineStyle);
                Assert.AreEqual(expectedText, result.Text);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        [TestMethod]
        [TestCategory("Unit")]
        public void Load_WithBackupFileAssumption_ReportsUtf8Bom()
        {
            const string expectedText = "SELECT 1;\r\n";
            var fileName = WriteTemporaryFile(expectedText, new UTF8Encoding(true));

            try
            {
                var result = TextFileLoadHelper.Load(fileName, true);

                Assert.IsTrue(result.IsSuccess);
                Assert.AreEqual("UTF-8 BOM", result.EncodingName);
                Assert.AreEqual("Windows (CR LF)", result.EndOfLineStyle);
                Assert.AreEqual(expectedText, result.Text);
            }
            finally
            {
                File.Delete(fileName);
            }
        }

        private static string WriteTemporaryFile(string contents, Encoding encoding)
        {
            var fileName = Path.Combine
            (
                Path.GetTempPath(),
                $"JasonQuery-TextFileLoad-{Guid.NewGuid():N}.sql"
            );

            File.WriteAllText(fileName, contents, encoding);
            return fileName;
        }
    }
}
