using JasonLibrary.Core.Update;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Updater.Core;

namespace JasonLibrary.Tests.Core.Update
{
    [TestClass]
    public sealed class UpdateLocalFolderEndToEndTests
    {
        private const string InstalledVersion = "0.93.0";
        private const string TargetVersion = "0.94.0";

        [TestMethod]
        [TestCategory("Update")]
        [TestCategory("EndToEnd")]
        public async Task ProductionUpdate_UpdatesApplicationPreservesUserDataAndRestoresBackup()
        {
            var scenario = CreateScenario();

            try
            {
                WriteOldInstallation(scenario);
                CreateProductionPackage(scenario, false);
                WriteProductionMetadata(scenario);

                var selection = await LoadSelectionAsync(scenario);
                var packageLocation = ResolvePackage(scenario, selection);

                var verification = UpdatePackageVerifier.Verify
                (
                    packageLocation.Value,
                    selection.Asset.Size,
                    selection.Asset.Digest
                );

                Assert.AreEqual(selection.Asset.Size, verification.Size);

                var payloadRoot = SafeZipExtractor.ExtractAndResolvePayloadRoot
                (
                    packageLocation.Value,
                    scenario.ExtractionRoot
                );

                var stages = new List<UpdateTransactionStage>();
                var transaction = new FileUpdateTransaction();

                var result = transaction.Execute
                (
                    payloadRoot,
                    scenario.InstallationRoot,
                    scenario.BackupRoot,
                    InstalledVersion,
                    TargetVersion,
                    stages.Add
                );

                Assert.AreEqual(3, result.UpdatedFileCount);
                Assert.AreEqual("new executable", ReadFile(scenario.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("new library", ReadFile(scenario.InstallationRoot, "JasonLibrary.dll"));
                Assert.AreEqual("new dependency", ReadFile(scenario.InstallationRoot, "NewDependency.dll"));
                AssertUserDataIsPreserved(scenario);

                CollectionAssert.AreEqual
                (
                    new[]
                    {
                        UpdateTransactionStage.CreatingVerifiedBackup,
                        UpdateTransactionStage.ApplyingAndVerifyingUpdate
                    },
                    stages
                );

                Assert.IsTrue(File.Exists(result.BackupManifestPath));

                Assert.AreEqual
                (
                    "old executable",
                    ReadFile(Path.Combine(scenario.BackupRoot, "files"), "JasonQuery.exe")
                );

                Assert.AreEqual
                (
                    "old library",
                    ReadFile(Path.Combine(scenario.BackupRoot, "files"), "JasonLibrary.dll")
                );

                var backupManifest = ReadBackupManifest(result.BackupManifestPath);

                Assert.AreEqual(InstalledVersion, backupManifest.InstalledVersion);
                Assert.AreEqual(TargetVersion, backupManifest.TargetVersion);
                Assert.AreEqual(3, backupManifest.Files.Count);

                transaction.RestoreBackup(result.BackupManifestPath);

                Assert.AreEqual("old executable", ReadFile(scenario.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("old library", ReadFile(scenario.InstallationRoot, "JasonLibrary.dll"));
                Assert.IsFalse(File.Exists(Path.Combine(scenario.InstallationRoot, "NewDependency.dll")));
                AssertUserDataIsPreserved(scenario);
            }
            finally
            {
                DeleteScenario(scenario);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        [TestCategory("EndToEnd")]
        public async Task ProductionUpdate_WhenPackageIsTampered_RejectsBeforeInstallationChanges()
        {
            var scenario = CreateScenario();

            try
            {
                WriteOldInstallation(scenario);
                CreateProductionPackage(scenario, false);
                WriteProductionMetadata(scenario);
                TamperWithPackageWithoutChangingSize(scenario.PackagePath);

                var selection = await LoadSelectionAsync(scenario);
                var packageLocation = ResolvePackage(scenario, selection);

                var exception = Assert.ThrowsExactly<UpdatePackageVerificationException>
                (
                    () => UpdatePackageVerifier.Verify
                    (
                        packageLocation.Value,
                        selection.Asset.Size,
                        selection.Asset.Digest
                    )
                );

                Assert.AreEqual(UpdatePackageVerificationFailureKind.DigestMismatch, exception.FailureKind);
                Assert.AreEqual("old executable", ReadFile(scenario.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("old library", ReadFile(scenario.InstallationRoot, "JasonLibrary.dll"));
                AssertUserDataIsPreserved(scenario);
                Assert.IsFalse(Directory.Exists(scenario.ExtractionRoot));
                Assert.IsFalse(Directory.Exists(scenario.BackupRoot));
            }
            finally
            {
                DeleteScenario(scenario);
            }
        }

        [TestMethod]
        [TestCategory("Update")]
        [TestCategory("EndToEnd")]
        public async Task ProductionUpdate_WhenPayloadContainsDatabase_RejectsBeforeInstallationChanges()
        {
            var scenario = CreateScenario();

            try
            {
                WriteOldInstallation(scenario);
                CreateProductionPackage(scenario, true);
                WriteProductionMetadata(scenario);

                var selection = await LoadSelectionAsync(scenario);
                var packageLocation = ResolvePackage(scenario, selection);

                UpdatePackageVerifier.Verify
                (
                    packageLocation.Value,
                    selection.Asset.Size,
                    selection.Asset.Digest
                );

                var payloadRoot = SafeZipExtractor.ExtractAndResolvePayloadRoot
                (
                    packageLocation.Value,
                    scenario.ExtractionRoot
                );

                var exception = Assert.ThrowsExactly<ProtectedUpdatePathException>
                (
                    () => new FileUpdateTransaction().Execute
                    (
                        payloadRoot,
                        scenario.InstallationRoot,
                        scenario.BackupRoot,
                        InstalledVersion,
                        TargetVersion
                    )
                );

                Assert.AreEqual("JasonQuery.db", exception.RelativePath);
                Assert.AreEqual("old executable", ReadFile(scenario.InstallationRoot, "JasonQuery.exe"));
                Assert.AreEqual("old library", ReadFile(scenario.InstallationRoot, "JasonLibrary.dll"));
                AssertUserDataIsPreserved(scenario);
                Assert.IsFalse(Directory.Exists(scenario.BackupRoot));
            }
            finally
            {
                DeleteScenario(scenario);
            }
        }

        private static async Task<UpdateReleaseSelection> LoadSelectionAsync(UpdateScenario scenario)
        {
            UpdateMetadataManifest manifest;

            using (var provider = new UpdateMetadataProvider("JasonQuery.Tests/Step6"))
            {
                manifest = await provider.LoadAsync
                (
                    UpdateMetadataSourceKind.LocalFolder,
                    scenario.UpdateSourceRoot,
                    CancellationToken.None
                );
            }

            var selection = new UpdateReleaseSelector().FindUpdate(manifest, InstalledVersion);

            Assert.IsNotNull(selection);
            Assert.AreEqual(TargetVersion, selection.Version);
            Assert.AreEqual(UpdateChannel.Production, selection.Channel);
            Assert.AreEqual(UpdateMetadataSettingsContract.ProductionPackageFileName, selection.Asset.Name);

            return selection;
        }

        private static UpdateContentLocation ResolvePackage(UpdateScenario scenario, UpdateReleaseSelection selection)
        {
            var location = UpdateSourceResolver.ResolvePackage
            (
                UpdateMetadataSourceKind.LocalFolder,
                scenario.UpdateSourceRoot,
                selection.Asset
            );

            Assert.IsTrue(location.IsLocalFile);

            Assert.IsTrue
            (
                string.Equals
                (
                    Path.GetFullPath(scenario.PackagePath),
                    Path.GetFullPath(location.Value),
                    StringComparison.OrdinalIgnoreCase
                )
            );

            return location;
        }

        private static UpdateScenario CreateScenario()
        {
            var root = Path.Combine
            (
                Path.GetTempPath(),
                "JasonQuery-Step6",
                Guid.NewGuid().ToString("N")
            );

            var scenario = new UpdateScenario
            {
                Root = root,
                UpdateSourceRoot = Path.Combine(root, "company-update-source"),
                InstallationRoot = Path.Combine(root, "old-installation"),
                ExtractionRoot = Path.Combine(root, "extracted-package"),
                BackupRoot = Path.Combine(root, "verified-backup")
            };

            scenario.PackagePath = Path.Combine
            (
                scenario.UpdateSourceRoot,
                UpdateMetadataSettingsContract.ProductionPackageFileName
            );

            scenario.MetadataPath = Path.Combine
            (
                scenario.UpdateSourceRoot,
                UpdateMetadataSettingsContract.MetadataFileName
            );

            Directory.CreateDirectory(scenario.UpdateSourceRoot);
            Directory.CreateDirectory(scenario.InstallationRoot);

            return scenario;
        }

        private static void WriteOldInstallation(UpdateScenario scenario)
        {
            WriteFile(scenario.InstallationRoot, "JasonQuery.exe", "old executable");
            WriteFile(scenario.InstallationRoot, "JasonLibrary.dll", "old library");
            WriteFile(scenario.InstallationRoot, "JasonQuery.db", "user database");
            WriteFile(scenario.InstallationRoot, "log/session.log", "user log");
            WriteFile(scenario.InstallationRoot, "backup/saved.sql", "user backup");
        }

        private static void CreateProductionPackage(UpdateScenario scenario, bool includeProtectedDatabase)
        {
            using (var archive = ZipFile.Open(scenario.PackagePath, ZipArchiveMode.Create))
            {
                WriteEntry(archive, "JasonQuery x64/JasonQuery.exe", "new executable");
                WriteEntry(archive, "JasonQuery x64/JasonLibrary.dll", "new library");
                WriteEntry(archive, "JasonQuery x64/NewDependency.dll", "new dependency");

                if (includeProtectedDatabase)
                {
                    WriteEntry(archive, "JasonQuery x64/JasonQuery.db", "malicious database");
                }
            }
        }

        private static void WriteProductionMetadata(UpdateScenario scenario)
        {
            var packageInfo = new FileInfo(scenario.PackagePath);

            var manifest = new UpdateMetadataManifest
            {
                SchemaVersion = 1,
                Product = "JasonQuery",
                GeneratedAt = DateTimeOffset.UtcNow.ToString("O")
            };

            manifest.Releases.Add
            (
                new UpdateReleaseMetadata
                {
                    TagName = "v" + TargetVersion,
                    Name = "JasonQuery " + TargetVersion,
                    Body = string.Empty,
                    Draft = false,
                    Prerelease = false,
                    PublishedAt = DateTimeOffset.UtcNow.ToString("O"),
                    HtmlUrl = "https://jasonquery.org/",
                    Assets = new List<UpdateAssetMetadata>
                    {
                        new UpdateAssetMetadata
                        {
                            Name = UpdateMetadataSettingsContract.ProductionPackageFileName,
                            State = "uploaded",
                            ContentType = "application/zip",
                            Size = packageInfo.Length,
                            Digest = "sha256:" + Sha256Digest.ComputeFile(scenario.PackagePath),
                            BrowserDownloadUrl = "https://jasonquery.org/JasonQueryUpdate/JasonQuery64.zip"
                        }
                    }
                }
            );

            var serializer = new DataContractJsonSerializer(typeof(UpdateMetadataManifest));

            using (var stream = new FileStream(scenario.MetadataPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                serializer.WriteObject(stream, manifest);
                stream.Flush(true);
            }
        }

        private static UpdateBackupManifest ReadBackupManifest(string manifestPath)
        {
            var serializer = new DataContractJsonSerializer(typeof(UpdateBackupManifest));

            using (var stream = new FileStream(manifestPath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return (UpdateBackupManifest)serializer.ReadObject(stream);
            }
        }

        private static void TamperWithPackageWithoutChangingSize(string packagePath)
        {
            using (var stream = new FileStream(packagePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
                stream.Position = stream.Length / 2;

                var original = stream.ReadByte();

                if (original < 0)
                {
                    throw new InvalidDataException("The test package is empty.");
                }

                stream.Position--;
                stream.WriteByte((byte)(original ^ 0xFF));
                stream.Flush(true);
            }
        }

        private static void AssertUserDataIsPreserved(UpdateScenario scenario)
        {
            Assert.AreEqual("user database", ReadFile(scenario.InstallationRoot, "JasonQuery.db"));
            Assert.AreEqual("user log", ReadFile(scenario.InstallationRoot, "log/session.log"));
            Assert.AreEqual("user backup", ReadFile(scenario.InstallationRoot, "backup/saved.sql"));
        }

        private static void WriteEntry(ZipArchive archive, string entryName, string content)
        {
            var entry = archive.CreateEntry(entryName);

            using (var stream = entry.Open())
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static void WriteFile(string root, string relativePath, string content)
        {
            var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(fullPath, content, new UTF8Encoding(false));
        }

        private static string ReadFile(string root, string relativePath)
        {
            var fullPath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));

            return File.ReadAllText(fullPath, Encoding.UTF8);
        }

        private static void DeleteScenario(UpdateScenario scenario)
        {
            if (Directory.Exists(scenario.Root))
            {
                Directory.Delete(scenario.Root, true);
            }
        }

        private sealed class UpdateScenario
        {
            public string Root { get; set; }

            public string UpdateSourceRoot { get; set; }

            public string InstallationRoot { get; set; }

            public string ExtractionRoot { get; set; }

            public string BackupRoot { get; set; }

            public string PackagePath { get; set; }

            public string MetadataPath { get; set; }
        }
    }
}
