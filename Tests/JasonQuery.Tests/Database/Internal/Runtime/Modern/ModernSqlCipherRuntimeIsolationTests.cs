using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Database.Internal.Runtime.Modern;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace JasonQuery.Tests.Database.Internal.Runtime.Modern
{
    [TestClass]
    [DoNotParallelize]
    public class ModernSqlCipherRuntimeIsolationTests
    {
        [TestMethod]
        public void MainProcessModernRuntime_DoesNotReferenceSQLitePCL()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernSqlCipherDatabaseRuntime.cs"
            );

            Assert.DoesNotContain("using SQLitePCL;", source);
            Assert.DoesNotContain("SQLite3Provider_sqlcipher", source);
            Assert.DoesNotContain("raw.sqlite3_", source);
        }

        [TestMethod]
        public void ProcessClient_DoesNotPutCredentialsInArgumentsOrEnvironment()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProcessClient.cs"
            );

            Assert.DoesNotContain("Arguments =", source);
            Assert.DoesNotContain("ArgumentList", source);
            Assert.DoesNotContain("EnvironmentVariables", source);
            Assert.DoesNotContain("Environment[", source);
            Assert.Contains("RedirectStandardInput = true", source);
            Assert.Contains("RedirectStandardOutput = true", source);
        }

        [TestMethod]
        public void RuntimeProject_IsX64Net48AndUsesQualifiedProviderPackages()
        {
            var project = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "JasonQuery.ModernDbRuntime.csproj"
            );

            Assert.Contains("<TargetFramework>net48</TargetFramework>", project);
            Assert.Contains("<PlatformTarget>x64</PlatformTarget>", project);
            Assert.Contains("SQLitePCLRaw.core\" Version=\"3.0.5\"", project);
            Assert.Contains("SQLitePCLRaw.provider.sqlcipher\" Version=\"3.0.5\"", project);
            Assert.Contains("Link=\"sqlcipher.dll\"", project);
        }

        [TestMethod]
        public void RuntimeProject_SharesQualifiedSqlCipherFoundationWithMigrationHelper()
        {
            var project = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "JasonQuery.ModernDbRuntime.csproj"
            );

            Assert.Contains(
                "Migration\\ModernSQLite\\JasonQuery.ModernDbMigration\\ModernSqlCipherRuntime.cs"
,
                project            );
        }

        [TestMethod]
        public void RuntimeProcess_UsesBinaryStandardIoDispatcher()
        {
            var program = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "Program.cs"
            );

            Assert.Contains("Console.OpenStandardInput()", program);
            Assert.Contains("Console.OpenStandardOutput()", program);
            Assert.DoesNotContain("args", program);
        }

        [TestMethod]
        public void RuntimeProcess_SupportsRequiredR4F2BOperations()
        {
            var protocol = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProtocol.cs"
            );

            foreach (var operation in new[]
            {
                "Ping = 1",
                "Open = 2",
                "ExecuteQuery = 3",
                "ExecuteNonQuery = 4",
                "ExecuteBatchNonQuery = 5",
                "Close = 6",
                "Shutdown = 7",
                "Rekey = 8",
                "CreateFreshDatabase = 9",
                "CanOpenWithoutKey = 10"
            })
            {
                Assert.Contains(operation, protocol);
            }
        }

        [TestMethod]
        public void FreshCreator_UsesQualifiedMemoryOnlySqlCipherExportPath()
        {
            var creator = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "ModernStorageV2FreshDatabaseCreator.cs"
            );

            Assert.Contains("raw.sqlite3_deserialize", creator);
            Assert.Contains("SELECT sqlcipher_export('", creator);
            Assert.Contains("ATTACH DATABASE ?1 AS ", creator);
            Assert.Contains("raw.sqlite3_bind_blob", creator);
            Assert.Contains("raw.sqlite3_bind_text", creator);
            Assert.DoesNotContain("sqlite3_backup", creator);
            Assert.DoesNotContain("System.Data.SQLite", creator);
            Assert.DoesNotContain("KEY '", creator);
        }

        [TestMethod]
        public void FreshCreateTransport_RemainsBinaryStandardInputWithoutCredentialPersistence()
        {
            var client = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProcessClient.cs"
            );

            Assert.Contains("ModernDatabaseRuntimeOperation.CreateFreshDatabase", client);
            Assert.Contains("ModernDatabaseRuntimeProtocol.WriteByteArray(writer, databasePasswordUtf8)", client);
            Assert.Contains("ModernDatabaseRuntimeProtocol.WriteByteArray(writer, plaintextTemplateBytes)", client);
            Assert.DoesNotContain("Arguments =", client);
            Assert.DoesNotContain("ArgumentList", client);
            Assert.DoesNotContain("EnvironmentVariables", client);
            Assert.DoesNotContain("WriteAllText", client);
            Assert.DoesNotContain("WriteAllBytes", client);
        }

        [TestMethod]
        public void ModernFreshProvider_DeclaresStorageV2WithoutLoadingSQLitePCLInMainProcess()
        {
            var provider = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Security",
                "ModernSqlCipherDatabaseSecurityFreshInstallDatabase.cs"
            );

            Assert.Contains("StorageFormatVersion => JasonQueryDbStorageFormatContract.ModernVersion", provider);
            Assert.Contains("_runtime.CreateFreshDatabase", provider);
            Assert.Contains("_runtime.CanOpenWithoutKey", provider);
            Assert.DoesNotContain("using SQLitePCL;", provider);
            Assert.DoesNotContain("System.Data.SQLite", provider);
            Assert.DoesNotContain("raw.sqlite3_", provider);
        }

        [TestMethod]
        public void FreshCreator_InitializesCurrentConnectionCredentialStorageMarkerInsideEncryptedTarget()
        {
            var creator = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "ModernStorageV2FreshDatabaseCreator.cs"
            );

            Assert.Contains("ConnectionCredentialStorageContract.CurrentVersion", creator);
            Assert.Contains("JasonQuerySecurityMetadata", creator);
            Assert.Contains("ConnectionCredentialStorageVersion", creator);
            Assert.Contains("BEGIN IMMEDIATE;", creator);
            Assert.Contains("ValidateConnectionCredentialStorageVersion", creator);
        }

        [TestMethod]
        public void ModernRuntimeRekey_IsImplementedThroughIsolatedChildOnly()
        {
            var facade = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernSqlCipherDatabaseRuntime.cs"
            );

            var client = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProcessClient.cs"
            );

            var dispatcher = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "ModernDbRuntimeRequestDispatcher.cs"
            );

            var session = ReadRepositorySource
            (
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "ModernDbRuntimeSession.cs"
            );

            var sharedRuntime = ReadRepositorySource
            (
                "Migration",
                "ModernSQLite",
                "JasonQuery.ModernDbMigration",
                "ModernSqlCipherRuntime.cs"
            );

            Assert.DoesNotContain("using SQLitePCL;", facade);
            Assert.DoesNotContain("raw.sqlite3_rekey", facade);
            Assert.Contains("client.Rekey(newCredentialBytes)", facade);
            Assert.Contains("ModernDatabaseRuntimeOperation.Rekey", client);
            Assert.Contains("case ModernDatabaseRuntimeOperation.Rekey:", dispatcher);
            Assert.Contains("ModernSqlCipherRuntime.Rekey(_database, newDatabasePasswordUtf8)", session);
            Assert.Contains("raw.sqlite3_rekey", sharedRuntime);
        }

        [TestMethod]
        public void RekeyTransport_DoesNotUseArgumentsEnvironmentOrPlaintextCredentialFiles()
        {
            var client = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProcessClient.cs"
            );

            Assert.DoesNotContain("Arguments =", client);
            Assert.DoesNotContain("ArgumentList", client);
            Assert.DoesNotContain("EnvironmentVariables", client);
            Assert.DoesNotContain("Environment[", client);
            Assert.DoesNotContain("WriteAllText", client);
            Assert.DoesNotContain("WriteAllBytes", client);
            Assert.Contains("ModernDatabaseRuntimeProtocol.WriteByteArray(writer, newDatabasePasswordUtf8)", client);
        }

        [TestMethod]
        public void SharedSqlCipherRuntime_RekeyUsesQualifiedRawApiWithMutableCredentialBytes()
        {
            var sharedRuntime = ReadRepositorySource
            (
                "Migration",
                "ModernSQLite",
                "JasonQuery.ModernDbMigration",
                "ModernSqlCipherRuntime.cs"
            );

            Assert.Contains(
                "raw.sqlite3_rekey(db, new ReadOnlySpan<byte>(newDatabasePasswordUtf8))"
,
                sharedRuntime            );

            Assert.DoesNotContain("PRAGMA rekey =", sharedRuntime);
        }
        [TestMethod]
        public void R4F2C_ConfiguresModernRuntimeWithoutAddingSQLitePCLToProductionStartup()
        {
            var source = ReadRepositorySource
            (
                "JasonQuery",
                "UI",
                "Forms",
                "MainForm.DatabaseSecurity.cs"
            );

            Assert.Contains("new ModernSqlCipherDatabaseRuntime()", source);
            Assert.Contains("runtime.ProbeQualifiedRuntime();", source);
            Assert.Contains("JasonQueryRepository.ConfigureRuntime(runtime);", source);
            Assert.DoesNotContain("using SQLitePCL;", source);
            Assert.DoesNotContain("raw.sqlite3_", source);
            Assert.AreEqual(JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.LegacyVersion)), JasonQuery.Tests.Infrastructure.RuntimeContractValueReader.GetRawConstant(typeof(JasonQueryDbStorageFormatContract), nameof(JasonQueryDbStorageFormatContract.CurrentVersion)));
        }

        [TestMethod]
        public void QualifiedRuntimeProcess_PingValidatesExpectedNativeIdentity()
        {
            var executable = GetBuiltRuntimeExecutablePath();

            Assert.IsTrue(File.Exists(executable), "Build the Release|x64 solution before running this test.");

            using (var runtime = new ModernSqlCipherDatabaseRuntime(executable))
            {
                var identity = runtime.ProbeQualifiedRuntime();

                Assert.AreEqual("JasonQuery.ModernDbRuntime", identity.RuntimeName);
                Assert.AreEqual(ModernDatabaseRuntimeProtocol.ProtocolVersion, identity.ProtocolVersion);
                Assert.AreEqual("3.53.3", identity.SqliteVersion);
                Assert.AreEqual("4.17.0", identity.SqlCipherVersionPrefix);

                Assert.AreEqual
                (
                    "25852CE7A4067CC79E73309D26C1AD9B5706E876BBDF48BE9A25379180FB9A07",
                    identity.NativeSha256
                );
            }
        }

        [TestMethod]
        public void RuntimeExecutableOutput_DoesNotContainLegacySystemDataSQLite()
        {
            var executable = GetBuiltRuntimeExecutablePath();
            var outputDirectory = Path.GetDirectoryName(executable);

            Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "System.Data.SQLite.dll")));
            Assert.IsFalse(File.Exists(Path.Combine(outputDirectory, "SQLite.Interop.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "SQLitePCLRaw.core.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "SQLitePCLRaw.provider.sqlcipher.dll")));
            Assert.IsTrue(File.Exists(Path.Combine(outputDirectory, "sqlcipher.dll")));
        }

        [TestMethod]
        public void DatabaseOperations_AreOperationScopedAndDoNotLeaveModernRuntimeProcess()
        {
            var executable = GetBuiltRuntimeExecutablePath();

            Assert.IsTrue(File.Exists(executable), "Build the Release|x64 solution before running this test.");

            var tempDirectory = Path.Combine(Path.GetTempPath(), "JasonQuery-B6D-E5-" + Guid.NewGuid().ToString("N"));
            var databasePath = Path.Combine(tempDirectory, "JasonQuery.db");
            const string databasePassword = "B6D-E5-OperationScoped-Test-Credential";
            var connectionString = "Data Source=" + databasePath + ";";
            var expectedProcessIds = GetRuntimeProcessIds(executable);
            byte[] templateBytes = null;

            Directory.CreateDirectory(tempDirectory);

            try
            {
                templateBytes = ReadEmbeddedDatabaseTemplate();

                using (var runtime = new ModernSqlCipherDatabaseRuntime(executable))
                {
                    runtime.CreateFreshDatabase(databasePath, databasePassword, templateBytes);
                    AssertRuntimeProcessSetRestored(executable, expectedProcessIds, "CreateFreshDatabase");

                    runtime.ExecuteNonQuery
                    (
                        connectionString,
                        databasePassword,
                        "CREATE TABLE B6DE5Lifecycle (Id INTEGER NOT NULL);",
                        null
                    );

                    AssertRuntimeProcessSetRestored(executable, expectedProcessIds, "ExecuteNonQuery");

                    runtime.ExecuteBatchNonQuery
                    (
                        connectionString,
                        databasePassword,
                        new[]
                        {
                            "INSERT INTO B6DE5Lifecycle (Id) VALUES (1)",
                            "INSERT INTO B6DE5Lifecycle (Id) VALUES (2)"
                        }
                    );

                    AssertRuntimeProcessSetRestored(executable, expectedProcessIds, "ExecuteBatchNonQuery");

                    var result = runtime.ExecuteQuery
                    (
                        connectionString,
                        databasePassword,
                        "SELECT COUNT(*) AS RowCount FROM B6DE5Lifecycle"
                    );

                    Assert.AreEqual(1, result.Rows.Count);
                    Assert.AreEqual(2L, Convert.ToInt64(result.Rows[0]["RowCount"]));
                    AssertRuntimeProcessSetRestored(executable, expectedProcessIds, "ExecuteQuery");

                    var invalidQueryRejected = false;

                    try
                    {
                        runtime.ExecuteQuery
                        (
                            connectionString,
                            databasePassword,
                            "SELECT * FROM B6DE5_Table_That_Does_Not_Exist"
                        );
                    }
                    catch
                    {
                        invalidQueryRejected = true;
                    }

                    Assert.IsTrue(invalidQueryRejected, "The deliberately invalid Modern runtime query was not rejected.");
                    AssertRuntimeProcessSetRestored(executable, expectedProcessIds, "failed ExecuteQuery");
                }
            }
            finally
            {
                if (templateBytes != null)
                {
                    Array.Clear(templateBytes, 0, templateBytes.Length);
                }

                TryDeleteFile(databasePath);
                TryDeleteFile(databasePath + "-wal");
                TryDeleteFile(databasePath + "-shm");
                TryDeleteFile(databasePath + "-journal");

                try
                {
                    if (Directory.Exists(tempDirectory))
                    {
                        Directory.Delete(tempDirectory, true);
                    }
                }
                catch
                {
                }
            }
        }

        [TestMethod]
        public void OperationScopedLifecycle_UsesFinallyCleanupAndExistingShutdownProtocol()
        {
            var facade = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernSqlCipherDatabaseRuntime.cs"
            );

            var client = ReadRepositorySource
            (
                "JasonQuery",
                "Database",
                "Internal",
                "Runtime",
                "Modern",
                "ModernDatabaseRuntimeProcessClient.cs"
            );

            foreach (var signature in new[]
            {
                "public DataTable ExecuteQuery(string connectionString, string databasePassword, string sql)",
                "public void ExecuteNonQuery(string connectionString, string databasePassword, string sql, IReadOnlyList<JasonQueryDatabaseParameter> parameters)",
                "public void ExecuteBatchNonQuery(string connectionString, string databasePassword, IReadOnlyList<string> sqlStatements)"
            })
            {
                var body = ExtractMethodBody(facade, signature);

                Assert.Contains("finally", body);
                Assert.Contains("DisposeClient();", body);
            }

            Assert.Contains("ModernDatabaseRuntimeOperation.Shutdown", client);
            Assert.Contains("_process.WaitForExit(ExitWaitMilliseconds)", client);
            Assert.Contains("_process.Kill();", client);
        }

        private static byte[] ReadEmbeddedDatabaseTemplate()
        {
            const string resourceName = "JasonQuery.Files.JasonQuery.template.db";
            var assembly = typeof(ModernSqlCipherDatabaseRuntime).Assembly;

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                Assert.IsNotNull(stream, "Embedded JasonQuery database template was not found.");

                using (var memory = new MemoryStream())
                {
                    stream.CopyTo(memory);
                    return memory.ToArray();
                }
            }
        }

        private static int[] GetRuntimeProcessIds(string executablePath)
        {
            var expectedPath = Path.GetFullPath(executablePath);
            var processIds = new List<int>();

            foreach (var process in Process.GetProcessesByName("JasonQuery.ModernDbRuntime"))
            {
                using (process)
                {
                    try
                    {
                        var processPath = process.MainModule?.FileName;

                        if (string.IsNullOrWhiteSpace(processPath))
                        {
                            continue;
                        }

                        if (string.Equals(Path.GetFullPath(processPath), expectedPath, StringComparison.OrdinalIgnoreCase))
                        {
                            processIds.Add(process.Id);
                        }
                    }
                    catch
                    {
                    }
                }
            }

            processIds.Sort();
            return processIds.ToArray();
        }

        private static void AssertRuntimeProcessSetRestored(string executablePath, int[] expectedProcessIds, string operation)
        {
            CollectionAssert.AreEqual
            (
                expectedProcessIds,
                GetRuntimeProcessIds(executablePath),
                operation + " left a JasonQuery.ModernDbRuntime child process running."
            );
        }

        private static string ExtractMethodBody(string source, string signature)
        {
            var signatureIndex = source.IndexOf(signature, StringComparison.Ordinal);

            Assert.IsTrue(signatureIndex >= 0, "Method signature was not found: " + signature);

            var openingBrace = source.IndexOf('{', signatureIndex);

            Assert.IsTrue(openingBrace >= 0, "Method opening brace was not found: " + signature);

            var depth = 0;

            for (var index = openingBrace; index < source.Length; index++)
            {
                if (source[index] == '{')
                {
                    depth++;
                }

                if (source[index] == '}')
                {
                    depth--;

                    if (depth == 0)
                    {
                        return source.Substring(openingBrace, index - openingBrace + 1);
                    }
                }
            }

            Assert.Fail("Method closing brace was not found: " + signature);
            return string.Empty;
        }

        private static void TryDeleteFile(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch
            {
            }
        }

        private static string GetBuiltRuntimeExecutablePath()
        {
            return Path.Combine
            (
                FindRepositoryRoot(),
                "Runtime",
                "ModernSQLite",
                "JasonQuery.ModernDbRuntime",
                "bin",
                "x64",
                "Release",
                "net48",
                "JasonQuery.ModernDbRuntime.exe"
            );
        }

        private static string ReadRepositorySource(params string[] pathParts)
        {
            var path = FindRepositoryRoot();

            foreach (var pathPart in pathParts)
            {
                path = Path.Combine(path, pathPart);
            }

            Assert.IsTrue(File.Exists(path), "Required source file was not found: " + path);
            return File.ReadAllText(path);
        }

        private static string FindRepositoryRoot()
        {
            var root = FindRepositoryRootFrom(Directory.GetCurrentDirectory());

            if (!string.IsNullOrEmpty(root))
            {
                return root;
            }

            root = FindRepositoryRootFrom(AppDomain.CurrentDomain.BaseDirectory);

            if (!string.IsNullOrEmpty(root))
            {
                return root;
            }

            Assert.Fail("Could not locate the JasonQuery repository root for R4F2B tests.");
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
                if (File.Exists(Path.Combine(directory.FullName, "JasonQuery", "JasonQuery.csproj"))
                    && Directory.Exists(Path.Combine(directory.FullName, "Tests", "JasonQuery.Tests")))
                {
                    return directory.FullName;
                }

                directory = directory.Parent;
            }

            return null;
        }
    }
}
