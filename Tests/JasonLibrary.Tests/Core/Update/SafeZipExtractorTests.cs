using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.IO.Compression;
using System.Text;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class SafeZipExtractorTests
    {
        [TestMethod]
        [TestCategory("Update")]
        public void ExtractAndResolvePayloadRoot_ExtractsSingleJasonQueryPayload()
        {
            var root = CreateTemporaryDirectory();
            var packagePath = Path.Combine(root, "package.zip");
            var extractionRoot = Path.Combine(root, "extracted");

            try
            {
                using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
                {
                    WriteEntry(archive, "JasonQuery x64/JasonQuery.exe", "exe");
                    WriteEntry(archive, "JasonQuery x64/localization/english.xml", "xml");
                }

                var payloadRoot = SafeZipExtractor.ExtractAndResolvePayloadRoot(packagePath, extractionRoot);

                Assert.IsTrue(File.Exists(Path.Combine(payloadRoot, "JasonQuery.exe")));
                Assert.IsTrue(File.Exists(Path.Combine(payloadRoot, "localization", "english.xml")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void ExtractAndResolvePayloadRoot_RejectsDirectoryTraversal()
        {
            var root = CreateTemporaryDirectory();
            var packagePath = Path.Combine(root, "package.zip");
            var extractionRoot = Path.Combine(root, "extracted");

            try
            {
                using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
                {
                    WriteEntry(archive, "JasonQuery x64/JasonQuery.exe", "exe");
                    WriteEntry(archive, "../escaped.txt", "unsafe");
                }

                Assert.ThrowsException<InvalidDataException>
                (
                    () => SafeZipExtractor.ExtractAndResolvePayloadRoot(packagePath, extractionRoot)
                );

                Assert.IsFalse(File.Exists(Path.Combine(root, "escaped.txt")));
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        public void ExtractAndResolvePayloadRoot_RejectsAlternateDataStreamPath()
        {
            var root = CreateTemporaryDirectory();
            var packagePath = Path.Combine(root, "package.zip");
            var extractionRoot = Path.Combine(root, "extracted");

            try
            {
                using (var archive = ZipFile.Open(packagePath, ZipArchiveMode.Create))
                {
                    WriteEntry(archive, "JasonQuery x64/JasonQuery.exe", "exe");
                    WriteEntry(archive, "JasonQuery x64/JasonQuery.exe:payload", "unsafe");
                }

                Assert.ThrowsException<InvalidDataException>
                (
                    () => SafeZipExtractor.ExtractAndResolvePayloadRoot(packagePath, extractionRoot)
                );
            }
            finally
            {
                Directory.Delete(root, true);
            }
        }

        private static void WriteEntry(ZipArchive archive, string name, string content)
        {
            var entry = archive.CreateEntry(name);

            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static string CreateTemporaryDirectory()
        {
            var path = Path.Combine(Path.GetTempPath(), "JasonQuery-UpdaterTests", Path.GetRandomFileName());

            Directory.CreateDirectory(path);

            return path;
        }
    }
}
