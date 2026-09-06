using JasonQuery.Core.Config;
using JasonQuery.Core.Security.Database;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Internal.Security;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        private const string DatabaseTemplateResourceName = "JasonQuery.Files.JasonQuery.template.db";

        private void InitializeDatabaseSecurity(string databaseFilePath)
        {
            try
            {
                InitializeDatabaseSecurityCore(databaseFilePath);
            }
            catch (DatabaseSecurityStartupException ex)
            {
                ShowDatabaseSecurityStartupError(ex.ErrorKind);
                Environment.Exit(1);
            }
            catch (Exception)
            {
                ShowDatabaseSecurityStartupError(DatabaseSecurityStartupErrorKind.GeneralSecurityFailure);
                Environment.Exit(1);
            }
        }

        private static void ShowDatabaseSecurityStartupError(DatabaseSecurityStartupErrorKind errorKind)
        {
            try
            {
                using (var form = new DatabaseSecurityStartupErrorForm(errorKind))
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
            var metadataFilePath = Path.Combine(Application.StartupPath, DatabaseSecurityConstants.MetadataFileName);
            var metadataStore = new DatabaseSecurityMetadataStore(metadataFilePath);
            var databaseKeyProtector = new DpapiDatabaseKeyProtector();
            var migrationDatabase = new SqliteDatabaseSecurityMigrationDatabase();
            var transitionJournalStore = new DatabaseSecurityTransitionJournalStore
            (
                DatabaseSecurityTransitionManager.GetJournalFilePath(databaseFilePath)
            );
            var transitionManager = new DatabaseSecurityTransitionManager
            (
                metadataStore,
                databaseKeyProtector,
                migrationDatabase,
                transitionJournalStore
            );
            var migrator = new LegacyDatabaseSecurityMigrator(metadataStore, databaseKeyProtector, migrationDatabase);
            var freshInstallDatabase = new SqliteDatabaseSecurityFreshInstallDatabase();
            var freshInitializer = new FreshDatabaseSecurityInitializer(metadataStore, databaseKeyProtector, freshInstallDatabase);

            var transitionRecoveryResult = transitionManager.RecoverInterruptedChangeIfNeeded(databaseFilePath);

            if (transitionRecoveryResult != null)
            {
                ApplyResolvedV2DatabaseSecurity
                (
                    transitionRecoveryResult.Metadata,
                    transitionRecoveryResult.DatabasePassword,
                    null
                );
                return;
            }

            migrator.RecoverInterruptedMigrationIfNeeded(databaseFilePath);
            freshInitializer.RecoverInterruptedInitializationIfNeeded(databaseFilePath);

            if (!File.Exists(databaseFilePath))
            {
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

                    ApplyResolvedV2DatabaseSecurity(initializationResult.Metadata, initializationResult.DatabasePassword, null);
                    return;
                }
            }

            var bootstrapper = new DatabaseSecurityBootstrapper(metadataStore, databaseKeyProtector);
            var bootstrapResult = bootstrapper.Resolve(databaseFilePath);

            switch (bootstrapResult.State)
            {
                case DatabaseSecurityStartupState.Legacy:
                    {
                        var migrationResult = migrator.MigrateToWindowsCurrentUser(databaseFilePath);

                        ApplyResolvedV2DatabaseSecurity
                        (
                            migrationResult.Metadata,
                            migrationResult.DatabasePassword,
                            migrator
                        );

                        return;
                    }
                case DatabaseSecurityStartupState.V2Ready:
                    {
                        ApplyResolvedV2DatabaseSecurity
                        (
                            bootstrapResult.Metadata,
                            bootstrapResult.DatabasePassword,
                            migrator
                        );

                        return;
                    }
                case DatabaseSecurityStartupState.V2CustomPasswordRequired:
                    {
                        using (var passwordDialog = new DatabasePasswordDialog(bootstrapper, bootstrapResult.Metadata, databaseFilePath))
                        {
                            if (passwordDialog.ShowDialog(this) != DialogResult.OK)
                            {
                                Environment.Exit(1);
                                return;
                            }

                            ApplyResolvedV2DatabaseSecurity
                            (
                                bootstrapResult.Metadata,
                                passwordDialog.DatabasePassword,
                                null
                            );
                            return;
                        }
                    }
                case DatabaseSecurityStartupState.DatabaseMissing:
                    {
                        throw new FileNotFoundException("JasonQuery.db was not found after database initialization.", databaseFilePath);
                    }
                default:
                    {
                        throw new InvalidOperationException($"Unexpected database security startup state: {bootstrapResult.State}");
                    }
            }
        }

        private static void ApplyResolvedV2DatabaseSecurity(DatabaseSecurityMetadata metadata, string databasePassword,
                                                            LegacyDatabaseSecurityMigrator migrator)
        {
            JasonQueryRepository.DbConnectionPassword = databasePassword;

            if (!JasonQueryRepository.CheckCurrentDatabasePassword())
            {
                throw new InvalidDataException
                (
                    "Database Encryption V2 metadata was resolved, but JasonQuery.db could not be opened with the resolved key."
                );
            }

            DatabaseSecurityRuntime.SetV2(metadata.Mode);

            if (migrator != null)
            {
                migrator.CleanupCompletedMigrationBackup(JasonQueryRepository.DbFileName);
            }
        }
    }
}
