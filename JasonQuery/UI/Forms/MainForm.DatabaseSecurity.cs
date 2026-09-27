using JasonQuery.Core.Config;
using JasonQuery.Core.Security.JasonQueryDb;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Internal.Security;
using JasonQuery.Database.Internal.Runtime.Modern;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        private const string DatabaseTemplateResourceName = "JasonQuery.Files.JasonQuery.template.db";
        private const string LegacyStorageV1MigrationHelperRelativePath = @"StorageMigration\Legacy\JasonQuery.LegacyDbMigration.exe";
        private const string ModernStorageV2MigrationHelperRelativePath = @"StorageMigration\Modern\JasonQuery.ModernDbMigration.exe";

        private void InitializeDatabaseSecurity(string databaseFilePath)
        {
            try
            {
                InitializeDatabaseSecurityCore(databaseFilePath);
            }
            catch (OperationCanceledException)
            {
                Environment.Exit(1);
            }
            catch (JasonQueryDbSecurityStartupException ex)
            {
                ShowDatabaseSecurityStartupError(ex.ErrorKind);
                Environment.Exit(1);
            }
            catch (Exception)
            {
                ShowDatabaseSecurityStartupError(JasonQueryDbSecurityStartupErrorKind.GeneralSecurityFailure);
                Environment.Exit(1);
            }
        }

        private static void ShowDatabaseSecurityStartupError(JasonQueryDbSecurityStartupErrorKind errorKind)
        {
            try
            {
                using (var form = new JasonQueryDbSecurityStartupErrorForm(errorKind))
                {
                    form.ShowDialog();
                }
            }
            catch (Exception)
            {
                MessageBox.Show
                (
                    "JasonQuery could not safely open its internal database. JasonQuery will now exit.",
                    AppConfigHelper.MessageBoxCaption,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void InitializeDatabaseSecurityCore(string databaseFilePath)
        {
            var metadataFilePath = Path.Combine(Application.StartupPath, JasonQueryDbSecurityConstants.MetadataFileName);
            var metadataStore = new JasonQueryDbSecurityMetadataStore(metadataFilePath);
            var databaseKeyProtector = new JasonQueryDbDpapiKeyProtector();

            RecoverDatabaseStorageMigrationIfNeeded
            (
                databaseFilePath,
                metadataFilePath,
                databaseKeyProtector
            );

            var transitionJournalStore = new JasonQueryDbSecurityTransitionJournalStore
            (
                JasonQueryDbSecurityTransitionManager.GetJournalFilePath(databaseFilePath)
            );

            var transitionManager = new JasonQueryDbSecurityTransitionManager
            (
                metadataStore,
                databaseKeyProtector,
                metadata => StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create(metadata),
                transitionJournalStore
            );

            var transitionRecoveryResult = transitionManager.RecoverInterruptedChangeIfNeeded(databaseFilePath);

            if (transitionRecoveryResult != null)
            {
                var recoveredRoute = JasonQueryDbStorageRuntimeRoutingContract.Resolve
                (
                    transitionRecoveryResult.Metadata.StorageFormatVersion
                );

                if (recoveredRoute.RuntimeKind == JasonQueryDbStorageRuntimeKind.ModernSqlCipher)
                {
                    ConfigureModernStorageV2Runtime
                    (
                        transitionRecoveryResult.Metadata,
                        transitionRecoveryResult.DatabasePassword
                    );

                    return;
                }

                if (recoveredRoute.RuntimeKind == JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)
                {
                    CompleteLegacyStorageV1Startup
                    (
                        transitionRecoveryResult.Metadata,
                        transitionRecoveryResult.DatabasePassword,
                        null
                    );

                    return;
                }

                throw new NotSupportedException
                (
                    $"Recovered storage runtime '{recoveredRoute.RuntimeKind}' is not supported by this JasonQuery build."
                );
            }

            RecoverInterruptedFreshDatabaseInitializationIfNeeded
            (
                databaseFilePath,
                metadataStore,
                databaseKeyProtector
            );

            var storageRoute = JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadataStore);

            if (storageRoute.RuntimeKind == JasonQueryDbStorageRuntimeKind.ModernSqlCipher)
            {
                InitializeModernStorageV2Security(databaseFilePath, metadataStore, databaseKeyProtector);
                return;
            }

            if (storageRoute.RuntimeKind != JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)
            {
                throw new NotSupportedException
                (
                    $"Storage runtime '{storageRoute.RuntimeKind}' is not supported by this JasonQuery build."
                );
            }

            var legacyMigrationBackupFilePath = JasonQueryDbLegacySecurityMigrator.GetBackupFilePath(databaseFilePath);

            if (!metadataStore.Exists && File.Exists(legacyMigrationBackupFilePath))
            {
                var interruptedMigrator = CreateLegacyDatabaseSecurityMigrator
                (
                    metadataStore,
                    databaseKeyProtector
                );

                if (interruptedMigrator.CanRecoverInterruptedMigrationWithDefaultPassword(databaseFilePath))
                {
                    interruptedMigrator.RecoverInterruptedMigrationIfNeeded(databaseFilePath);
                }
                else
                {
                    if (!TryMigrateLegacyCustomPassword(databaseFilePath, interruptedMigrator))
                    {
                        Environment.Exit(1);
                    }

                    return;
                }
            }

            if (!File.Exists(databaseFilePath) && !metadataStore.Exists)
            {
                InitializeFreshStorageV2Database
                (
                    databaseFilePath,
                    metadataStore,
                    databaseKeyProtector
                );

                return;
            }

            JasonQueryRepository.ResetRuntimeToLegacy();

            var migrator = CreateLegacyDatabaseSecurityMigrator
            (
                metadataStore,
                databaseKeyProtector
            );

            var bootstrapper = new JasonQueryDbSecurityBootstrapper(metadataStore, databaseKeyProtector);
            var bootstrapResult = bootstrapper.Resolve(databaseFilePath);

            switch (bootstrapResult.State)
            {
                case JasonQueryDbSecurityStartupState.Legacy:
                    {
                        if (migrator.CanOpenWithDefaultPassword(databaseFilePath))
                        {
                            var migrationResult = migrator.MigrateToWindowsCurrentUser(databaseFilePath);

                            CompleteLegacyStorageV1Startup
                            (
                                migrationResult.Metadata,
                                migrationResult.DatabasePassword,
                                migrator
                            );

                            return;
                        }

                        if (!TryMigrateLegacyCustomPassword(databaseFilePath, migrator))
                        {
                            Environment.Exit(1);
                        }

                        return;
                    }
                case JasonQueryDbSecurityStartupState.V2Ready:
                    {
                        CompleteLegacyStorageV1Startup
                        (
                            bootstrapResult.Metadata,
                            bootstrapResult.DatabasePassword,
                            migrator
                        );

                        return;
                    }
                case JasonQueryDbSecurityStartupState.V2CustomPasswordRequired:
                    {
                        using (var passwordDialog = new JasonQueryDbPasswordDialog(bootstrapper, bootstrapResult.Metadata, databaseFilePath, JasonQueryRepository.CanOpenDatabase))
                        {
                            if (passwordDialog.ShowDialog(this) != DialogResult.OK)
                            {
                                Environment.Exit(1);
                                return;
                            }

                            CompleteLegacyStorageV1Startup
                            (
                                bootstrapResult.Metadata,
                                passwordDialog.DatabasePassword,
                                null
                            );

                            return;
                        }
                    }
                case JasonQueryDbSecurityStartupState.V2RecoveryRequired:
                    {
                        var recoveryResult = RecoverWindowsCurrentUserDatabaseKey
                        (
                            databaseFilePath,
                            metadataStore,
                            databaseKeyProtector
                        );

                        CompleteLegacyStorageV1Startup
                        (
                            recoveryResult.Metadata,
                            recoveryResult.DatabasePassword,
                            migrator
                        );

                        return;
                    }
                case JasonQueryDbSecurityStartupState.DatabaseMissing:
                    {
                        throw new FileNotFoundException("JasonQuery.db was not found after database initialization.", databaseFilePath);
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unexpected database security startup state: {bootstrapResult.State}");
                    }
            }
        }

        private void RecoverDatabaseStorageMigrationIfNeeded(string databaseFilePath, string metadataFilePath, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            var modernHelperFilePath = Path.Combine
            (
                Application.StartupPath,
                ModernStorageV2MigrationHelperRelativePath
            );

            var startupRecovery = new JasonQueryDbStorageMigrationStartupRecoveryCoordinator
            (
                databaseFilePath,
                metadataFilePath
            );

            startupRecovery.RecoverIfNeeded
            (
                modernHelperFilePath,
                (journal, paths) => ResolveDatabaseStorageMigrationTargetPassword
                (
                    journal,
                    paths,
                    modernHelperFilePath,
                    databaseKeyProtector
                )
            );
        }

        private byte[] ResolveDatabaseStorageMigrationTargetPassword(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths,
                                                                     string modernHelperFilePath, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            if (journal == null)
            {
                throw new ArgumentNullException(nameof(journal));
            }

            if (paths == null)
            {
                throw new ArgumentNullException(nameof(paths));
            }

            if (!File.Exists(modernHelperFilePath))
            {
                throw new FileNotFoundException
                (
                    "The qualified Storage V2 migration helper was not found.",
                    modernHelperFilePath
                );
            }

            switch (journal.SourceMetadata.Mode)
            {
                case JasonQueryDbSecurityMode.WindowsCurrentUser:
                    {
                        return ResolveWindowsStorageMigrationTargetPassword
                        (
                            journal.SourceMetadata,
                            databaseKeyProtector
                        );
                    }
                case JasonQueryDbSecurityMode.CustomPassword:
                    {
                        return ResolveCustomStorageMigrationTargetPassword
                        (
                            journal,
                            paths,
                            modernHelperFilePath
                        );
                    }
                default:
                    {
                        throw new InvalidDataException
                        (
                            $"Database security mode '{journal.SourceMetadata.Mode}' is not supported for storage migration recovery."
                        );
                    }
            }
        }

        private static byte[] ResolveWindowsStorageMigrationTargetPassword(JasonQueryDbSecurityMetadata metadata, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            byte[] protectedDatabaseKey = null;
            byte[] databaseKey = null;

            try
            {
                protectedDatabaseKey = Convert.FromBase64String
                (
                    metadata.ProtectedDatabaseKey
                );

                try
                {
                    databaseKey = databaseKeyProtector.Unprotect
                    (
                        protectedDatabaseKey
                    );
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    throw new JasonQueryDbSecurityStartupException
                    (
                        JasonQueryDbSecurityStartupErrorKind.WindowsCurrentUserKeyUnavailable,
                        "The Windows-protected JasonQuery database key could not be unlocked during storage migration recovery.",
                        ex
                    );
                }

                return JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
                (
                    databaseKey
                );
            }
            finally
            {
                if (protectedDatabaseKey != null)
                {
                    Array.Clear
                    (
                        protectedDatabaseKey,
                        0,
                        protectedDatabaseKey.Length
                    );
                }

                if (databaseKey != null)
                {
                    Array.Clear
                    (
                        databaseKey,
                        0,
                        databaseKey.Length
                    );
                }
            }
        }

        private byte[] ResolveCustomStorageMigrationTargetPassword(JasonQueryDbStorageMigrationJournal journal, JasonQueryDbStorageMigrationArtifactPaths paths, string modernHelperFilePath)
        {
            using (var passwordDialog = new JasonQueryDbPasswordDialog
            (
                customPassword => ValidateStorageMigrationCustomPassword
                (
                    customPassword,
                    journal,
                    paths,
                    modernHelperFilePath
                )
            ))
            {
                if (passwordDialog.ShowDialog(this) != DialogResult.OK)
                {
                    throw new OperationCanceledException
                    (
                        "Database storage-migration recovery was canceled."
                    );
                }

                var customPassword = passwordDialog.CustomPassword;
                byte[] salt = null;
                byte[] databaseKey = null;

                try
                {
                    salt = Convert.FromBase64String
                    (
                        journal.SourceMetadata.Salt
                    );

                    databaseKey = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey
                    (
                        customPassword,
                        salt,
                        journal.SourceMetadata.Iterations
                    );

                    return JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
                    (
                        databaseKey
                    );
                }
                finally
                {
                    customPassword = null;

                    if (salt != null)
                    {
                        Array.Clear(salt, 0, salt.Length);
                    }

                    if (databaseKey != null)
                    {
                        Array.Clear(databaseKey, 0, databaseKey.Length);
                    }
                }
            }
        }

        private static bool ValidateStorageMigrationCustomPassword(string customPassword, JasonQueryDbStorageMigrationJournal journal,
                                                                   JasonQueryDbStorageMigrationArtifactPaths paths, string modernHelperFilePath)
        {
            byte[] salt = null;
            byte[] databaseKey = null;
            byte[] databasePasswordUtf8 = null;

            try
            {
                salt = Convert.FromBase64String
                (
                    journal.SourceMetadata.Salt
                );

                databaseKey = JasonQueryDbCustomPasswordKeyDeriver.DeriveDatabaseKey
                (
                    customPassword,
                    salt,
                    journal.SourceMetadata.Iterations
                );

                databasePasswordUtf8 = JasonQueryDbStorageMigrationCredentialEncoder.EncodeDatabaseKey
                (
                    databaseKey
                );

                var validation = new JasonQueryDbStorageV2ReadOnlyValidationProcessRunner().Validate
                (
                    modernHelperFilePath,
                    paths.CandidateDatabaseFilePath,
                    databasePasswordUtf8
                );

                if (validation == null)
                {
                    return false;
                }

                return string.Equals
                (
                    validation.CandidateDatabaseSha256,
                    journal.CandidateDatabaseSha256,
                    StringComparison.Ordinal
                );
            }
            catch (JasonQueryDbStorageV2ReadOnlyValidationException)
            {
                return false;
            }
            finally
            {
                if (salt != null)
                {
                    Array.Clear(salt, 0, salt.Length);
                }

                if (databaseKey != null)
                {
                    Array.Clear(databaseKey, 0, databaseKey.Length);
                }

                if (databasePasswordUtf8 != null)
                {
                    Array.Clear
                    (
                        databasePasswordUtf8,
                        0,
                        databasePasswordUtf8.Length
                    );
                }
            }
        }

        private bool TryMigrateLegacyCustomPassword(string databaseFilePath, JasonQueryDbLegacySecurityMigrator migrator)
        {
            using (var passwordDialog = new JasonQueryDbPasswordDialog(migrator, databaseFilePath))
            {
                if (passwordDialog.ShowDialog(this) != DialogResult.OK)
                {
                    return false;
                }

                var legacyCustomPassword = passwordDialog.CustomPassword;

                try
                {
                    var migrationResult = migrator.MigrateToCustomPassword
                    (
                        databaseFilePath,
                        legacyCustomPassword
                    );

                    CompleteLegacyStorageV1Startup
                    (
                        migrationResult.Metadata,
                        migrationResult.DatabasePassword,
                        migrator
                    );

                    return true;
                }
                finally
                {
                    legacyCustomPassword = null;
                }
            }
        }

        private static void RecoverInterruptedFreshDatabaseInitializationIfNeeded(string databaseFilePath, IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            if (File.Exists(databaseFilePath))
            {
                return;
            }

            var freshInstallDatabase = CreateFreshInstallRecoveryDatabase(metadataStore);

            var freshInitializer = new JasonQueryDbFreshSecurityInitializer
            (
                metadataStore,
                databaseKeyProtector,
                freshInstallDatabase
            );

            freshInitializer.RecoverInterruptedInitializationIfNeeded(databaseFilePath);
        }

        private static IJasonQueryDbFreshInstallDatabase CreateFreshInstallRecoveryDatabase(IJasonQueryDbSecurityMetadataStore metadataStore)
        {
            if (metadataStore == null)
            {
                throw new ArgumentNullException(nameof(metadataStore));
            }

            if (!metadataStore.Exists)
            {
                return new ModernSqlCipherDatabaseSecurityFreshInstallDatabase();
            }

            var metadata = metadataStore.Load();

            if (metadata == null)
            {
                throw new InvalidDataException("Database security metadata could not be loaded for fresh-install recovery routing.");
            }

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadata.StorageFormatVersion);

            switch (route.RuntimeKind)
            {
                case JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite:
                    {
                        return new SqliteDatabaseSecurityFreshInstallDatabase();
                    }
                case JasonQueryDbStorageRuntimeKind.ModernSqlCipher:
                    {
                        return new ModernSqlCipherDatabaseSecurityFreshInstallDatabase();
                    }
                default:
                    {
                        throw new NotSupportedException
                        (
                            $"Storage runtime '{route.RuntimeKind}' does not have a fresh-install recovery provider."
                        );
                    }
            }
        }

        private static void InitializeFreshStorageV2Database(string databaseFilePath, IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            var freshInstallDatabase = new ModernSqlCipherDatabaseSecurityFreshInstallDatabase();

            var freshInitializer = new JasonQueryDbFreshSecurityInitializer
            (
                metadataStore,
                databaseKeyProtector,
                freshInstallDatabase
            );

            using (var templateStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(DatabaseTemplateResourceName))
            {
                if (templateStream == null)
                {
                    throw new InvalidDataException
                    (
                        $"The embedded JasonQuery database template '{DatabaseTemplateResourceName}' was not found."
                    );
                }

                var initializationResult = freshInitializer.InitializeWindowsCurrentUser(databaseFilePath, templateStream);

                ConfigureModernStorageV2Runtime
                (
                    initializationResult.Metadata,
                    initializationResult.DatabasePassword
                );
            }
        }

        private static JasonQueryDbLegacySecurityMigrator CreateLegacyDatabaseSecurityMigrator(IJasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            return new JasonQueryDbLegacySecurityMigrator
            (
                metadataStore,
                databaseKeyProtector,
                new SqliteDatabaseSecurityMigrationDatabase()
            );
        }

        private void CompleteLegacyStorageV1Startup(JasonQueryDbSecurityMetadata metadata, string databasePassword, JasonQueryDbLegacySecurityMigrator migrator)
        {
            JasonQueryRepository.ResetRuntimeToLegacy();
            JasonQueryRepository.DbConnectionPassword = databasePassword;

            using (var connection = JasonQueryRepository.OpenValidatedCurrentDatabaseConnection())
            {
                var credentialStorageStartupGate = new SqliteConnectionCredentialStorageStartupGate();

                credentialStorageStartupGate.EnsureReady(connection);
            }

            //DBInfo.Password logical V2 migration is committed before physical Storage V1 -> V2 migration begins.
            JasonQueryDbSecurityRuntime.SetV2(metadata.Mode);

            if (migrator != null)
            {
                migrator.CleanupCompletedMigrationBackup(JasonQueryRepository.DbFileName);
            }

            var metadataFilePath = Path.Combine(Application.StartupPath, JasonQueryDbSecurityConstants.MetadataFileName);
            var metadataStore = new JasonQueryDbSecurityMetadataStore(metadataFilePath);

            var cutover = new JasonQueryDbStorageRuntimeCutoverCoordinator
            (
                databaseFilePath: JasonQueryRepository.DbFileName,
                metadataFilePath: metadataFilePath,
                metadataStore: metadataStore
            );

            var route = cutover.ResolvePersistedRoute();

            if (route.RuntimeKind == JasonQueryDbStorageRuntimeKind.LegacySystemDataSQLite)
            {
                cutover.MigrateLegacyStorageToModern
                (
                    GetQualifiedLegacyMigrationHelperPath(),
                    GetQualifiedModernMigrationHelperPath(),
                    databasePassword
                );
            }

            var committedMetadata = metadataStore.Load();

            ConfigureModernStorageV2Runtime(committedMetadata, databasePassword);
        }

        private void InitializeModernStorageV2Security(string databaseFilePath, JasonQueryDbSecurityMetadataStore metadataStore, IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            var bootstrapper = new JasonQueryDbSecurityBootstrapper(metadataStore, databaseKeyProtector);
            var bootstrapResult = bootstrapper.Resolve(databaseFilePath);

            switch (bootstrapResult.State)
            {
                case JasonQueryDbSecurityStartupState.V2Ready:
                    {
                        ConfigureModernStorageV2Runtime(bootstrapResult.Metadata, bootstrapResult.DatabasePassword);
                        return;
                    }
                case JasonQueryDbSecurityStartupState.V2CustomPasswordRequired:
                    {
                        using (var passwordDialog = new JasonQueryDbPasswordDialog
                        (
                            bootstrapper,
                            bootstrapResult.Metadata,
                            databaseFilePath,
                            ValidateModernStorageV2Password
                        ))
                        {
                            if (passwordDialog.ShowDialog(this) != DialogResult.OK)
                            {
                                throw new OperationCanceledException("Database password entry was canceled.");
                            }

                            ConfigureModernStorageV2Runtime
                            (
                                bootstrapResult.Metadata,
                                passwordDialog.DatabasePassword
                            );

                            return;
                        }
                    }
                case JasonQueryDbSecurityStartupState.V2RecoveryRequired:
                    {
                        var recoveryResult = RecoverWindowsCurrentUserDatabaseKey
                        (
                            databaseFilePath,
                            metadataStore,
                            databaseKeyProtector
                        );

                        ConfigureModernStorageV2Runtime
                        (
                            recoveryResult.Metadata,
                            recoveryResult.DatabasePassword
                        );

                        return;
                    }
                default:
                    {
                        throw new InvalidDataException
                        (
                            $"Persisted Storage V2 metadata resolved to unexpected security state '{bootstrapResult.State}'."
                        );
                    }
            }
        }

        private JasonQueryDbSecurityBootstrapResult RecoverWindowsCurrentUserDatabaseKey(string databaseFilePath, IJasonQueryDbSecurityMetadataStore metadataStore,
                                                                                         IJasonQueryDbKeyProtector databaseKeyProtector)
        {
            var recoveryManager = new JasonQueryDbRecoveryStartupManager
            (
                metadataStore,
                databaseKeyProtector,
                metadata => StorageAwareDatabaseSecurityMigrationDatabaseFactory.Create(metadata)
            );

            using (var recoveryForm = new JasonQueryDbRecoveryStartupForm(recoveryManager, databaseFilePath))
            {
                if (recoveryForm.ShowDialog(this) != DialogResult.OK || recoveryForm.RecoveryResult == null)
                {
                    throw new OperationCanceledException("Database Recovery Key entry was canceled.");
                }

                return recoveryForm.RecoveryResult;
            }
        }

        private static bool ValidateModernStorageV2Password(string databasePassword)
        {
            using (var runtime = new ModernSqlCipherDatabaseRuntime())
            {
                runtime.ProbeQualifiedRuntime();
                return runtime.CanOpenDatabase(JasonQueryRepository.DbConnectionString, databasePassword);
            }
        }

        private static void ConfigureModernStorageV2Runtime(JasonQueryDbSecurityMetadata metadata, string databasePassword)
        {
            if (metadata == null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            var route = JasonQueryDbStorageRuntimeRoutingContract.Resolve(metadata.StorageFormatVersion);

            if (route.RuntimeKind != JasonQueryDbStorageRuntimeKind.ModernSqlCipher)
            {
                throw new InvalidDataException("Modern runtime configuration requires persisted Storage V2 metadata.");
            }

            ModernSqlCipherDatabaseRuntime runtime = null;

            try
            {
                runtime = new ModernSqlCipherDatabaseRuntime();
                runtime.ProbeQualifiedRuntime();

                if (!runtime.CanOpenDatabase(JasonQueryRepository.DbConnectionString, databasePassword))
                {
                    throw new InvalidDataException("The isolated Modern SQLCipher runtime could not validate JasonQuery.db.");
                }

                ConnectionCredentialStorageRuntimeValidator.EnsureV2Ready
                (
                    runtime,
                    JasonQueryRepository.DbConnectionString,
                    databasePassword
                );

                JasonQueryRepository.DbConnectionPassword = databasePassword;
                JasonQueryRepository.ConfigureRuntime(runtime);
                runtime = null;
                JasonQueryDbSecurityRuntime.SetV2(metadata.Mode);
            }
            finally
            {
                runtime?.Dispose();
            }
        }

        private static string GetQualifiedLegacyMigrationHelperPath()
        {
            return Path.Combine(Application.StartupPath, LegacyStorageV1MigrationHelperRelativePath);
        }

        private static string GetQualifiedModernMigrationHelperPath()
        {
            return Path.Combine(Application.StartupPath, ModernStorageV2MigrationHelperRelativePath);
        }
    }
}
