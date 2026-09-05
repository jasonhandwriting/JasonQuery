using JasonQuery.Core.Security.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Text;

namespace JasonQuery.Tests.Core.Security.Database
{
    [TestClass]
    public class FreshDatabaseSecurityInitializerTests
    {
        private const string PlaintextTemplateMarker = "PLAINTEXT-TEMPLATE";

        [TestMethod]
        public void InitializeWindowsCurrentUser_PlaintextTemplate_CreatesEncryptedDatabaseAndMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataStore = CreateMetadataStore(directory);
                var freshDatabase = new FakeFreshInstallDatabase();
                var initializer = CreateInitializer(metadataStore, freshDatabase);

                using (var templateStream = CreateTemplateStream())
                {
                    var result = initializer.InitializeWindowsCurrentUser(databasePath, templateStream);

                    Assert.AreEqual(DatabaseSecurityStartupState.V2Ready, result.State);
                    Assert.AreEqual(DatabaseSecurityMode.WindowsCurrentUser, result.Metadata.Mode);
                    Assert.IsTrue(metadataStore.Exists);
                    Assert.IsTrue(File.Exists(databasePath));
                    Assert.AreEqual(result.DatabasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                    Assert.IsFalse(freshDatabase.CanOpenWithoutPassword(databasePath));
                    Assert.IsFalse(File.Exists(FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath)));
                }
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void InitializeWindowsCurrentUser_ExistingDatabase_FailsWithoutMutation()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataStore = CreateMetadataStore(directory);
                var initializer = CreateInitializer(metadataStore, new FakeFreshInstallDatabase());

                File.WriteAllText(databasePath, "EXISTING-DATABASE", Encoding.UTF8);

                using (var templateStream = CreateTemplateStream())
                {
                    Assert.ThrowsException<InvalidOperationException>
                    (
                        () => initializer.InitializeWindowsCurrentUser(databasePath, templateStream)
                    );
                }

                Assert.AreEqual("EXISTING-DATABASE", File.ReadAllText(databasePath, Encoding.UTF8));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void InitializeWindowsCurrentUser_EncryptionFailure_RollsBackIncompleteFiles()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataStore = CreateMetadataStore(directory);
                var freshDatabase = new FakeFreshInstallDatabase { ThrowOnCreate = true };
                var initializer = CreateInitializer(metadataStore, freshDatabase);

                using (var templateStream = CreateTemplateStream())
                {
                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => initializer.InitializeWindowsCurrentUser(databasePath, templateStream)
                    );
                }

                Assert.IsFalse(File.Exists(databasePath));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void InitializeWindowsCurrentUser_MetadataSaveFailure_RollsBackIncompleteFiles()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataPath = Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName);
                var metadataStore = new ThrowingMetadataStore(metadataPath);
                var initializer = CreateInitializer(metadataStore, new FakeFreshInstallDatabase());

                using (var templateStream = CreateTemplateStream())
                {
                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => initializer.InitializeWindowsCurrentUser(databasePath, templateStream)
                    );
                }

                Assert.IsFalse(File.Exists(databasePath));
                Assert.IsFalse(metadataStore.Exists);
                Assert.IsFalse(File.Exists(FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath)));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void InitializeWindowsCurrentUser_NonPlaintextTemplate_FailsWithoutMetadata()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataStore = CreateMetadataStore(directory);
                var initializer = CreateInitializer(metadataStore, new FakeFreshInstallDatabase());

                using (var templateStream = new MemoryStream(Encoding.UTF8.GetBytes("NOT-A-PLAINTEXT-TEMPLATE")))
                {
                    Assert.ThrowsException<InvalidDataException>
                    (
                        () => initializer.InitializeWindowsCurrentUser(databasePath, templateStream)
                    );
                }

                Assert.IsFalse(File.Exists(databasePath));
                Assert.IsFalse(metadataStore.Exists);
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedInitializationIfNeeded_CandidateWithoutMetadata_DeletesCandidateAndReturnsFalse()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var candidatePath = FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath);
                var initializer = CreateInitializer(CreateMetadataStore(directory), new FakeFreshInstallDatabase());

                File.WriteAllText(candidatePath, "INCOMPLETE-CANDIDATE", Encoding.UTF8);

                var recovered = initializer.RecoverInterruptedInitializationIfNeeded(databasePath);

                Assert.IsFalse(recovered);
                Assert.IsFalse(File.Exists(databasePath));
                Assert.IsFalse(File.Exists(candidatePath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedInitializationIfNeeded_MetadataAndEncryptedCandidate_FinalizesDatabase()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var candidatePath = FreshDatabaseSecurityInitializer.GetCandidateFilePath(databasePath);
                var metadataStore = CreateMetadataStore(directory);
                var databaseKey = DatabaseKeyGenerator.Generate();

                try
                {
                    var databasePassword = DatabaseKeyGenerator.ToDatabasePassword(databaseKey);

                    metadataStore.Save
                    (
                        DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey))
                    );

                    File.WriteAllText(candidatePath, databasePassword, Encoding.UTF8);

                    var initializer = CreateInitializer(metadataStore, new FakeFreshInstallDatabase());
                    var recovered = initializer.RecoverInterruptedInitializationIfNeeded(databasePath);

                    Assert.IsTrue(recovered);
                    Assert.IsTrue(File.Exists(databasePath));
                    Assert.IsFalse(File.Exists(candidatePath));
                    Assert.AreEqual(databasePassword, File.ReadAllText(databasePath, Encoding.UTF8));
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        [TestMethod]
        public void RecoverInterruptedInitializationIfNeeded_MetadataWithoutDatabaseOrCandidate_FailsClosed()
        {
            var directory = CreateTemporaryDirectory();

            try
            {
                var databasePath = Path.Combine(directory, "JasonQuery.db");
                var metadataStore = CreateMetadataStore(directory);
                var databaseKey = DatabaseKeyGenerator.Generate();

                try
                {
                    metadataStore.Save
                    (
                        DatabaseSecurityMetadata.CreateWindowsCurrentUser(Convert.ToBase64String(databaseKey))
                    );
                }
                finally
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }

                var initializer = CreateInitializer(metadataStore, new FakeFreshInstallDatabase());

                Assert.ThrowsException<InvalidDataException>
                (
                    () => initializer.RecoverInterruptedInitializationIfNeeded(databasePath)
                );

                Assert.IsTrue(metadataStore.Exists);
                Assert.IsFalse(File.Exists(databasePath));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }

        private static FreshDatabaseSecurityInitializer CreateInitializer(IDatabaseSecurityMetadataStore metadataStore,
                                                                          IDatabaseSecurityFreshInstallDatabase freshDatabase)
        {
            return new FreshDatabaseSecurityInitializer
            (
                metadataStore,
                new PassThroughKeyProtector(),
                freshDatabase
            );
        }

        private static DatabaseSecurityMetadataStore CreateMetadataStore(string directory)
        {
            return new DatabaseSecurityMetadataStore
            (
                Path.Combine(directory, DatabaseSecurityConstants.MetadataFileName)
            );
        }

        private static MemoryStream CreateTemplateStream()
        {
            return new MemoryStream(Encoding.UTF8.GetBytes(PlaintextTemplateMarker));
        }

        private static string CreateTemporaryDirectory()
        {
            var directory = Path.Combine(Path.GetTempPath(), "JasonQuery.Tests", Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(directory);
            return directory;
        }

        private sealed class PassThroughKeyProtector : IDatabaseKeyProtector
        {
            public byte[] Protect(byte[] databaseKey)
            {
                return (byte[])databaseKey.Clone();
            }

            public byte[] Unprotect(byte[] protectedDatabaseKey)
            {
                return (byte[])protectedDatabaseKey.Clone();
            }
        }

        private sealed class FakeFreshInstallDatabase : IDatabaseSecurityFreshInstallDatabase
        {
            public bool ThrowOnCreate { get; set; }

            public void CreateEncryptedDatabase(Stream plaintextTemplateStream, string destinationDatabaseFilePath, string databasePassword)
            {
                if (ThrowOnCreate)
                {
                    throw new InvalidOperationException("Simulated fresh database encryption failure.");
                }

                string templateText;

                using (var reader = new StreamReader(plaintextTemplateStream, Encoding.UTF8, true, 1024, true))
                {
                    templateText = reader.ReadToEnd();
                }

                if (!string.Equals(templateText, PlaintextTemplateMarker, StringComparison.Ordinal))
                {
                    throw new InvalidDataException("The fake template is not plaintext.");
                }

                File.WriteAllText(destinationDatabaseFilePath, databasePassword, Encoding.UTF8);
            }

            public bool CanOpen(string databaseFilePath, string databasePassword)
            {
                if (!File.Exists(databaseFilePath))
                {
                    return false;
                }

                return string.Equals
                (
                    File.ReadAllText(databaseFilePath, Encoding.UTF8),
                    databasePassword,
                    StringComparison.Ordinal
                );
            }

            public bool CanOpenWithoutPassword(string databaseFilePath)
            {
                if (!File.Exists(databaseFilePath))
                {
                    return false;
                }

                return string.Equals
                (
                    File.ReadAllText(databaseFilePath, Encoding.UTF8),
                    PlaintextTemplateMarker,
                    StringComparison.Ordinal
                );
            }
        }

        private sealed class ThrowingMetadataStore : IDatabaseSecurityMetadataStore
        {
            public ThrowingMetadataStore(string metadataFilePath)
            {
                MetadataFilePath = metadataFilePath;
            }

            public string MetadataFilePath { get; }

            public bool Exists => File.Exists(MetadataFilePath);

            public DatabaseSecurityMetadata Load()
            {
                throw new FileNotFoundException();
            }

            public void Save(DatabaseSecurityMetadata metadata)
            {
                throw new IOException("Simulated metadata save failure.");
            }

            public void Delete()
            {
                if (File.Exists(MetadataFilePath))
                {
                    File.Delete(MetadataFilePath);
                }
            }
        }
    }
}
