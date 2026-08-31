using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Logging;
using JasonQuery.UI.Helpers;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ConnectionForm
    {
        private void btnHelp_AutoRollback_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This feature is enabled by default.\r\n\r\nIn PostgreSQL, when an SQL error occurs without a rollback, the current transaction enters a failed state, and no further SQL can be executed until a rollback is issued.\r\n\r\nWhen enabled, JasonQuery will automatically perform a rollback to prevent the transaction from being stuck.", "form", GetType().Name, "msg", "Help_AutoRollback", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_ConnectionName_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This name is used only as an identifier (alias) for the connection.\r\nIt does not affect the actual database connection or permissions.\r\n\r\nYou can rename it later when editing the connection settings.", "form", GetType().Name, "msg", "Help_ConnectionName", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DarkMode_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("You can change the color theme from [Tools] > [Options] > [General].", "form", GetType().Name, "msg", "Help_DarkMode", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_DirectMode_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When Direct Mode is enabled, JasonQuery connects to the Oracle database directly via TCP/IP without using the Oracle Client.\r\n\r\nThis mode is suitable when:\r\n• Oracle Client is not installed, or you prefer not to rely on it\r\n• You want a simpler and more stable way to connect to Oracle\r\n\r\nIn Direct Mode, only the SID is required.\r\nThere is no need to configure or verify Oracle Client settings such as tnsnames.ora.\r\n\r\nA valid SID is required when Direct Mode is enabled.\r\n\r\nDirect Mode provides a connection experience comparable to using the Oracle Client, with a simplified configuration process.", "form", GetType().Name, "msg", "Help_DirectMode", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_ExcludeNativeDatabase_SQLServer_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When enabled, JasonQuery will exclude native objects that are shipped with or created by SQL Server, such as system databases (e.g. master, tempdb).\r\n\r\nThis option helps simplify the object list and focus on user-defined database objects.", "form", GetType().Name, "msg", "Help_ExcludeNativeObject", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Log_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When enabled, JasonQuery records structured runtime information for debugging and performance analysis.\r\n\r\nEach logging session creates a separate CSV file that can be opened or imported in Excel. Log files older than 7 days are deleted automatically.\r\n\r\nLogs may include the database type and version, connection name, and object names. JasonQuery does not intentionally record passwords, connection strings, SQL parameter values, or row data.\r\n\r\nThis option is not required for normal usage. Enabling it may slightly affect performance.\r\n\r\nThe current log file or log folder is shown below:\r\n", "form", GetType().Name, "msg", "Help_Log", "Text");
            var logLocation = TraceLogger.IsEnabled && !string.IsNullOrWhiteSpace(TraceLogger.CurrentLogFilePath) ? TraceLogger.CurrentLogFilePath : TraceLogger.LogDirectoryPath;

            MessageBoxHelper.ShowNearCursor($"{message}{logLocation}", AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_MainFormIconStyle_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Change the Main Form Icon Style:", "form", GetType().Name, "msg", "Help_MainFormIconStyle0", "Text") + "\r\n\r\n";

            message += LocalizationHelper.GetLanguageString("When you open multiple JasonQuery, it is easier to identify what kind of database it is and whether it is a production or test environment.", "form", GetType().Name, "msg", "Help_MainFormIconStyle1", "Text") + "\r\n\r\n";
            message += LocalizationHelper.GetLanguageString("Each database connection can be configured with an independent icon style.", "form", GetType().Name, "msg", "Help_MainFormIconStyle2", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_PasswordEncrypt_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("If enabled, the password will be stored locally in \"JasonQuery.db\" in encrypted form, and can only be used under the current user account.\r\n\r\nIf you do not want the password to be stored, please uncheck \"Save Password\".\r\nYou will need to enter the password again next time.", "form", GetType().Name, "msg", "Help_PasswordEncrypt", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Pooling_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This is an advanced option that controls whether connection pooling is enabled.\r\n\r\nWhen enabled, database connections may be kept in a connection pool after being closed, allowing JasonQuery to reuse them without establishing a new physical connection each time.\r\n\r\nThis improves performance when connections are opened frequently and does not affect query results or transaction behavior.\r\n\r\nWhen disabled, database connections are physically closed each time.\r\nA new connection will be established when executing SQL again.\r\n\r\nIn most cases, it is recommended to keep this option enabled.", "form", GetType().Name, "msg", "Help_Pooling", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_QueryTimeout(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("This setting specifies the maximum time (in seconds) to wait for a server response when executing a query command before generating an error.\r\n\r\nThe timeout is measured from the moment the query command is sent to the server.\r\nIt includes only the waiting time for the server response and does not include the time required to fetch data.\r\n\r\nThis is a connection-level setting and applies to all query editors created using this connection.\r\n\r\nThe timeout value set in a Query Editor toolbar affects only that editor and takes precedence over this setting.\r\n\r\nA value of 0 indicates no time limit.", "Global", "Global", "msg", "Help_QueryTimeout", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_ShowColumnInfo_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("Display column information for Tables and Views in the Query Editor's \"Schema Information\".\r\n\r\nWhen enabled, JasonQuery retrieves all column names and data types while loading the schema information.\r\n\r\nIf the database contains a large number of Tables or Views, the schema initialization time may increase significantly.\r\n\r\nIf you only need to browse object names, disabling this option can improve loading performance.\r\n\r\nYou can change the setting from [Tools] > [Options] > [Query Editor].", "Global", "Global", "msg", "Help_ShowColumnInfo", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_SID_Click(object sender, EventArgs e)
        {
            //20260207 create new form: OracleSidHelpForm
            using (var form = new OracleSidHelpForm())
            {
                Point mousePos = Cursor.Position;

                form.Location = new Point(mousePos.X + 10, mousePos.Y + 10);
                form.ShowDialog();
            }
        }

        private void btnHelp_Unicode_PostgreSQL_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When the server and client use different character encodings, SQL execution may fail with encoding errors such as:\r\ncharacter with byte sequence 0xe6 0xb8 0xa9 in encoding \"UTF8\" has no equivalent in encoding \"BIG5\"\r\n\r\nEnabling Unicode allows JasonQuery to communicate with PostgreSQL using Unicode encoding to avoid such issues.", "form", GetType().Name, "msg", "Help_Unicode_PostgreSQL", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnHelp_Unicode_MySQL_Click(object sender, EventArgs e)
        {
            var message = LocalizationHelper.GetLanguageString("When the server and client use different character encodings, SQL execution may fail with encoding errors such as:\r\ncharacter with byte sequence 0xe6 0xb8 0xa9 in encoding \"UTF8\" has no equivalent in encoding \"BIG5\"\r\n\r\nEnabling Unicode allows JasonQuery to communicate with PostgreSQL using Unicode encoding to avoid such issues.", "form", GetType().Name, "msg", "Help_Unicode_MySQL", "Text");

            MessageBoxHelper.ShowNearCursor(message, AppConfigHelper.MessageBoxCaption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
