using JasonQuery.Core.Config;
using JasonQuery.Core.Security.Database;
using JasonQuery.Database.Internal.Repositories;
using JasonQuery.Database.Internal.Security;
using System;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
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
            var migrator = new LegacyDatabaseSecurityMigrator(metadataStore, databaseKeyProtector, migrationDatabase);

            migrator.RecoverInterruptedMigrationIfNeeded(databaseFilePath);

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
                        DatabaseSecurityRuntime.SetV2(DatabaseSecurityMode.CustomPassword);

                        throw new NotSupportedException
                        (
                            "This JasonQuery.db uses Database Encryption V2 with a custom password. " +
                            "The V2 custom-password startup dialog will be connected in a later security step."
                        );

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
            migrator.CleanupCompletedMigrationBackup(JasonQueryRepository.DbFileName);
        }
    }
}
