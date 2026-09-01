using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.Core.Security.Database;
using JasonQuery.Database.Internal.Repositories;
using System;
using System.IO;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class MainForm
    {
        private void InitializeDatabaseSecurity(string databaseFilePath)
        {
            var bootstrapper = DatabaseSecurityBootstrapper.CreateDefault(Application.StartupPath);
            var bootstrapResult = bootstrapper.Resolve(databaseFilePath);

            switch (bootstrapResult.State)
            {
                case DatabaseSecurityStartupState.Legacy:
                    {
                        InitializeLegacyDatabaseSecurity();
                        return;
                    }
                case DatabaseSecurityStartupState.V2Ready:
                    {
                        JasonQueryRepository.DbConnectionPassword = bootstrapResult.DatabasePassword;

                        if (!JasonQueryRepository.CheckCurrentDatabasePassword())
                        {
                            throw new InvalidDataException("JasonQuery Database Encryption V2 metadata was loaded, " + "but the database could not be opened with the resolved key.");
                        }

                        return;
                    }
                case DatabaseSecurityStartupState.V2CustomPasswordRequired:
                    {
                        throw new NotSupportedException("This JasonQuery.db uses Database Encryption V2 with a custom password. " + "The V2 custom-password startup dialog will be connected in the next security step.");
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

        private void InitializeLegacyDatabaseSecurity()
        {
            if (JasonQueryRepository.CheckDBPassword(string.Empty))
            {
                return;
            }

            using (TraceLogger.Time("Load Localization XML file"))
            {
                LocalizationHelper.LoadLocalizationXML();
            }

            using (TraceLogger.Time("Apply Localization"))
            {
                ApplyLocalization();
            }

            using (var form = new CustomPasswordDialog())
            {
                form.ShowDialog();
            }
        }
    }
}
