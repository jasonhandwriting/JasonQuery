namespace JasonQuery.UI.Forms
{
    partial class ConnectionForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionForm));
            this.grpConnect = new System.Windows.Forms.GroupBox();
            this.pnlPostgreSQL = new System.Windows.Forms.Panel();
            this.c1GridPostgreSQL = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.btnHelp_QueryTimeout_PostgreSQL = new C1.Win.C1Input.C1Button();
            this.lblSeconds_PostgreSQL = new System.Windows.Forms.Label();
            this.nudQueryTimeout_PostgreSQL = new System.Windows.Forms.NumericUpDown();
            this.lblQueryTimeout_PostgreSQL = new System.Windows.Forms.Label();
            this.cboDatabase_PostgreSQL = new C1.Win.C1Input.C1ComboBox();
            this.chkPooling_PostgreSQL = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_Pooling_PostgreSQL = new C1.Win.C1Input.C1Button();
            this.btnHelp_ConnectionName_PostgreSQL = new C1.Win.C1Input.C1Button();
            this.txtPort_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.txtPassword_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.txtUserID_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.txtConnectionName_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.txtServer_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.txtRemark_PostgreSQL = new C1.Win.C1Input.C1TextBox();
            this.lblRemark_PostgreSQL = new System.Windows.Forms.Label();
            this.lblPassword_PostgreSQL = new System.Windows.Forms.Label();
            this.lblUserID_PostgreSQL = new System.Windows.Forms.Label();
            this.lblDatabase_PostgreSQL = new System.Windows.Forms.Label();
            this.lblServer_PostgreSQL = new System.Windows.Forms.Label();
            this.lblPort_PostgreSQL = new System.Windows.Forms.Label();
            this.lblConnectionName_PostgreSQL = new System.Windows.Forms.Label();
            this.chkUnicode_PostgreSQL = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_AutoRollback_PostgreSQL = new C1.Win.C1Input.C1Button();
            this.btnHelp_Unicode_PostgreSQL = new C1.Win.C1Input.C1Button();
            this.chkAutoRollback_PostgreSQL = new C1.Win.C1Input.C1CheckBox();
            this.lblPrompt = new System.Windows.Forms.Label();
            this.pnlOracle = new System.Windows.Forms.Panel();
            this.btnHelp_QueryTimeout_Oracle = new C1.Win.C1Input.C1Button();
            this.lblSeconds_Oracle = new System.Windows.Forms.Label();
            this.nudQueryTimeout_Oracle = new System.Windows.Forms.NumericUpDown();
            this.lblQueryTimeout_Oracle = new System.Windows.Forms.Label();
            this.chkUnicode_Oracle = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_Pooling_Oracle = new C1.Win.C1Input.C1Button();
            this.chkPooling_Oracle = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_SID_Oracle = new C1.Win.C1Input.C1Button();
            this.lblSID_Oracle = new System.Windows.Forms.Label();
            this.btnHelp_ConnectionName_Oracle = new C1.Win.C1Input.C1Button();
            this.btnHelp_DirectMode = new C1.Win.C1Input.C1Button();
            this.txtPort_Oracle = new C1.Win.C1Input.C1TextBox();
            this.cboConnectAs_Oracle = new C1.Win.C1Input.C1ComboBox();
            this.txtSID_Oracle = new C1.Win.C1Input.C1TextBox();
            this.txtPassword_Oracle = new C1.Win.C1Input.C1TextBox();
            this.txtUserID_Oracle = new C1.Win.C1Input.C1TextBox();
            this.txtConnectionName_Oracle = new C1.Win.C1Input.C1TextBox();
            this.txtServer_Oracle = new C1.Win.C1Input.C1TextBox();
            this.txtRemark_Oracle = new C1.Win.C1Input.C1TextBox();
            this.lblRemark_Oracle = new System.Windows.Forms.Label();
            this.lblPassword_Oracle = new System.Windows.Forms.Label();
            this.lblUserID_Oracle = new System.Windows.Forms.Label();
            this.lblServer_Oracle = new System.Windows.Forms.Label();
            this.lblPort_Oracle = new System.Windows.Forms.Label();
            this.lblConnectionName_Oracle = new System.Windows.Forms.Label();
            this.chkDirectMode_Oracle = new C1.Win.C1Input.C1CheckBox();
            this.lblConnectAs_Oracle = new System.Windows.Forms.Label();
            this.textBox2 = new System.Windows.Forms.TextBox();
            this.btnHelp_Log = new C1.Win.C1Input.C1Button();
            this.chkLog = new C1.Win.C1Input.C1CheckBox();
            this.pnlMySQL = new System.Windows.Forms.Panel();
            this.c1GridMySQL = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.btnHelp_QueryTimeout_MySQL = new C1.Win.C1Input.C1Button();
            this.lblSeconds_MySQL = new System.Windows.Forms.Label();
            this.nudQueryTimeout_MySQL = new System.Windows.Forms.NumericUpDown();
            this.lblQueryTimeout_MySQL = new System.Windows.Forms.Label();
            this.txtPort_MySQL = new C1.Win.C1Input.C1TextBox();
            this.lblPort_MySQL = new System.Windows.Forms.Label();
            this.cboDatabase_MySQL = new C1.Win.C1Input.C1ComboBox();
            this.btnHelp_Unicode_MySQL = new C1.Win.C1Input.C1Button();
            this.chkUnicode_MySQL = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_Pooling_MySQL = new C1.Win.C1Input.C1Button();
            this.chkPooling_MySQL = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_ConnectionName_MySQL = new C1.Win.C1Input.C1Button();
            this.txtPassword_MySQL = new C1.Win.C1Input.C1TextBox();
            this.txtUserID_MySQL = new C1.Win.C1Input.C1TextBox();
            this.txtConnectionName_MySQL = new C1.Win.C1Input.C1TextBox();
            this.txtServer_MySQL = new C1.Win.C1Input.C1TextBox();
            this.txtRemark_MySQL = new C1.Win.C1Input.C1TextBox();
            this.lblRemark_MySQL = new System.Windows.Forms.Label();
            this.lblPassword_MySQL = new System.Windows.Forms.Label();
            this.lblUserID_MySQL = new System.Windows.Forms.Label();
            this.lblDatabase_MySQL = new System.Windows.Forms.Label();
            this.lblServer_MySQL = new System.Windows.Forms.Label();
            this.lblConnectionName_MySQL = new System.Windows.Forms.Label();
            this.btnExit = new C1.Win.C1Input.C1Button();
            this.btnUpdateNow = new C1.Win.C1Input.C1Button();
            this.pnlConnect = new System.Windows.Forms.Panel();
            this.btnConnect = new C1.Win.C1Input.C1Button();
            this.grpTabColor = new System.Windows.Forms.GroupBox();
            this.tabExample = new Crownwood.Magic.Controls.TabControl();
            this.tabPage1 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage2 = new Crownwood.Magic.Controls.TabPage();
            this.lblInactiveForeColor = new System.Windows.Forms.Label();
            this.pnlBackColor = new System.Windows.Forms.Panel();
            this.lblBackColor = new System.Windows.Forms.Label();
            this.pnlActiveForeColor = new System.Windows.Forms.Panel();
            this.pnlInactiveForeColor = new System.Windows.Forms.Panel();
            this.lblActiveForeColor = new System.Windows.Forms.Label();
            this.pnlSQLite = new System.Windows.Forms.Panel();
            this.btnHelp_DataType = new C1.Win.C1Input.C1Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btnEncryptWithPassword = new C1.Win.C1Input.C1Button();
            this.btnRemovePassword = new C1.Win.C1Input.C1Button();
            this.rdoWithPassword = new System.Windows.Forms.RadioButton();
            this.rdoWithoutPassword = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.rdoCreate_SQLite = new System.Windows.Forms.RadioButton();
            this.rdoOpen_SQLite = new System.Windows.Forms.RadioButton();
            this.btnBrowseFile_SQLite = new C1.Win.C1Input.C1Button();
            this.cboDatabaseType_SQLite = new C1.Win.C1Input.C1ComboBox();
            this.c1CheckBox1 = new C1.Win.C1Input.C1CheckBox();
            this.c1Button1 = new C1.Win.C1Input.C1Button();
            this.btnHelp_ConnectionName_SQLite = new C1.Win.C1Input.C1Button();
            this.txtPassword_SQLite = new C1.Win.C1Input.C1TextBox();
            this.txtConnectionName_SQLite = new C1.Win.C1Input.C1TextBox();
            this.txtFile_SQLite = new C1.Win.C1Input.C1TextBox();
            this.txtRemark_SQLite = new C1.Win.C1Input.C1TextBox();
            this.lblRemark_SQLite = new System.Windows.Forms.Label();
            this.lblPassword_SQLite = new System.Windows.Forms.Label();
            this.lblDatabaseType_SQLite = new System.Windows.Forms.Label();
            this.lblDatabaseFile_SQLite = new System.Windows.Forms.Label();
            this.lblConnectionName_SQLite = new System.Windows.Forms.Label();
            this.pnlSQLServer = new System.Windows.Forms.Panel();
            this.c1GridSQLServer = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.lblQueryTimeout_SQLServer = new System.Windows.Forms.Label();
            this.btnHelp_QueryTimeout_SQLServer = new C1.Win.C1Input.C1Button();
            this.lblSeconds_SQLServer = new System.Windows.Forms.Label();
            this.nudQueryTimeout_SQLServer = new System.Windows.Forms.NumericUpDown();
            this.btnHelp_ExcludeNativeDatabase_SQLServer = new C1.Win.C1Input.C1Button();
            this.chkExcludeNativeObject_SQLServer = new C1.Win.C1Input.C1CheckBox();
            this.txtPort_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.lblPort_SQLServer = new System.Windows.Forms.Label();
            this.cboDatabase_SQLServer = new C1.Win.C1Input.C1ComboBox();
            this.btnHelp_Pooling_SQLServer = new C1.Win.C1Input.C1Button();
            this.chkPooling_SQLServer = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_ConnectionName_SQLServer = new C1.Win.C1Input.C1Button();
            this.txtPassword_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.txtUserID_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.txtConnectionName_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.txtServer_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.txtRemark_SQLServer = new C1.Win.C1Input.C1TextBox();
            this.lblRemark_SQLServer = new System.Windows.Forms.Label();
            this.lblPassword_SQLServer = new System.Windows.Forms.Label();
            this.lblUserID_SQLServer = new System.Windows.Forms.Label();
            this.lblDatabase_SQLServer = new System.Windows.Forms.Label();
            this.lblServer_SQLServer = new System.Windows.Forms.Label();
            this.lblConnectionName_SQLServer = new System.Windows.Forms.Label();
            this.btnHelp_MainFormIconStyle = new C1.Win.C1Input.C1Button();
            this.grpMainFormIconStyle = new System.Windows.Forms.GroupBox();
            this.picIconStyle9 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle9 = new System.Windows.Forms.RadioButton();
            this.picIconStyle8 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle8 = new System.Windows.Forms.RadioButton();
            this.picIconStyle7 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle7 = new System.Windows.Forms.RadioButton();
            this.picIconStyle6 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle6 = new System.Windows.Forms.RadioButton();
            this.picIconStyle5 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle5 = new System.Windows.Forms.RadioButton();
            this.picIconStyle4 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle4 = new System.Windows.Forms.RadioButton();
            this.picIconStyle3 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle3 = new System.Windows.Forms.RadioButton();
            this.picIconStyle2 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle2 = new System.Windows.Forms.RadioButton();
            this.picIconStyle1 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle1 = new System.Windows.Forms.RadioButton();
            this.picIconStyle0 = new System.Windows.Forms.PictureBox();
            this.rdoIconStyle0 = new System.Windows.Forms.RadioButton();
            this.btnHelp_DarkMode = new C1.Win.C1Input.C1Button();
            this.btnEncryptWithCustomPW = new C1.Win.C1Input.C1Button();
            this.chkDarkMode = new C1.Win.C1Input.C1CheckBox();
            this.picSQLServer = new System.Windows.Forms.PictureBox();
            this.picPostgreSQL = new System.Windows.Forms.PictureBox();
            this.picOracle = new System.Windows.Forms.PictureBox();
            this.btnHelp_PasswordEncrypt = new C1.Win.C1Input.C1Button();
            this.chkSavePasswords = new C1.Win.C1Input.C1CheckBox();
            this.btnCreateShortcut = new C1.Win.C1Input.C1Button();
            this.btnCopy = new C1.Win.C1Input.C1Button();
            this.txtSupportInfo = new System.Windows.Forms.TextBox();
            this.lblSupportInfo = new System.Windows.Forms.Label();
            this.grpColorTheme = new System.Windows.Forms.GroupBox();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            this.txtAlignment = new System.Windows.Forms.TextBox();
            this.cboDataSource = new C1.Win.C1Input.C1ComboBox();
            this.btnNew = new C1.Win.C1Input.C1Button();
            this.lblDomainUser = new System.Windows.Forms.Label();
            this.lblPID = new System.Windows.Forms.Label();
            this.lblDataSource = new System.Windows.Forms.Label();
            this.btnSave = new C1.Win.C1Input.C1Button();
            this.btnDelete = new C1.Win.C1Input.C1Button();
            this.btnTest = new C1.Win.C1Input.C1Button();
            this.picSQLite = new System.Windows.Forms.PictureBox();
            this.picMySQL = new System.Windows.Forms.PictureBox();
            this.btnImport = new C1.Win.C1Input.C1Button();
            this.btnExport = new C1.Win.C1Input.C1Button();
            this.picDatabase = new System.Windows.Forms.PictureBox();
            this.lblMainFormIconStyle = new System.Windows.Forms.Label();
            this.btnHelp_ShowColumnInfo = new C1.Win.C1Input.C1Button();
            this.chkShowColumnInfo = new C1.Win.C1Input.C1CheckBox();
            this.c1GridDBInfo = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.grpOpenFile = new System.Windows.Forms.GroupBox();
            this.txtOpenSpecifiedFile = new System.Windows.Forms.TextBox();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.grpConnect.SuspendLayout();
            this.pnlPostgreSQL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridPostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoRollback_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Unicode_PostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoRollback_PostgreSQL)).BeginInit();
            this.pnlOracle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SID_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DirectMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnectAs_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSID_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDirectMode_Oracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Log)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLog)).BeginInit();
            this.pnlMySQL.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridMySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Unicode_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_MySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUpdateNow)).BeginInit();
            this.pnlConnect.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnConnect)).BeginInit();
            this.grpTabColor.SuspendLayout();
            this.tabExample.SuspendLayout();
            this.pnlSQLite.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DataType)).BeginInit();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptWithPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRemovePassword)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabaseType_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Button1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFile_SQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_SQLite)).BeginInit();
            this.pnlSQLServer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ExcludeNativeDatabase_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExcludeNativeObject_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_SQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_MainFormIconStyle)).BeginInit();
            this.grpMainFormIconStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle0)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DarkMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptWithCustomPW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDarkMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSQLServer)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPostgreSQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOracle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PasswordEncrypt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSavePasswords)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateShortcut)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDataSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNew)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSave)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTest)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSQLite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMySQL)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnImport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDatabase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ShowColumnInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridDBInfo)).BeginInit();
            this.grpOpenFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            this.SuspendLayout();
            // 
            // grpConnect
            // 
            this.grpConnect.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpConnect.Controls.Add(this.pnlPostgreSQL);
            this.grpConnect.Controls.Add(this.lblPrompt);
            this.grpConnect.Controls.Add(this.pnlOracle);
            this.grpConnect.Controls.Add(this.btnHelp_Log);
            this.grpConnect.Controls.Add(this.chkLog);
            this.grpConnect.Controls.Add(this.pnlMySQL);
            this.grpConnect.Controls.Add(this.btnExit);
            this.grpConnect.Controls.Add(this.btnUpdateNow);
            this.grpConnect.Controls.Add(this.pnlConnect);
            this.grpConnect.Controls.Add(this.grpTabColor);
            this.grpConnect.Controls.Add(this.pnlSQLite);
            this.grpConnect.Controls.Add(this.pnlSQLServer);
            this.grpConnect.Controls.Add(this.btnHelp_MainFormIconStyle);
            this.grpConnect.Controls.Add(this.grpMainFormIconStyle);
            this.grpConnect.Controls.Add(this.btnHelp_DarkMode);
            this.grpConnect.Controls.Add(this.btnEncryptWithCustomPW);
            this.grpConnect.Controls.Add(this.chkDarkMode);
            this.grpConnect.Controls.Add(this.picSQLServer);
            this.grpConnect.Controls.Add(this.picPostgreSQL);
            this.grpConnect.Controls.Add(this.picOracle);
            this.grpConnect.Controls.Add(this.btnHelp_PasswordEncrypt);
            this.grpConnect.Controls.Add(this.chkSavePasswords);
            this.grpConnect.Controls.Add(this.btnCreateShortcut);
            this.grpConnect.Controls.Add(this.btnCopy);
            this.grpConnect.Controls.Add(this.txtSupportInfo);
            this.grpConnect.Controls.Add(this.lblSupportInfo);
            this.grpConnect.Controls.Add(this.grpColorTheme);
            this.grpConnect.Controls.Add(this.grpLocalization);
            this.grpConnect.Controls.Add(this.txtAlignment);
            this.grpConnect.Controls.Add(this.cboDataSource);
            this.grpConnect.Controls.Add(this.btnNew);
            this.grpConnect.Controls.Add(this.lblDomainUser);
            this.grpConnect.Controls.Add(this.lblPID);
            this.grpConnect.Controls.Add(this.lblDataSource);
            this.grpConnect.Controls.Add(this.btnSave);
            this.grpConnect.Controls.Add(this.btnDelete);
            this.grpConnect.Controls.Add(this.btnTest);
            this.grpConnect.Controls.Add(this.picSQLite);
            this.grpConnect.Controls.Add(this.picMySQL);
            this.grpConnect.Controls.Add(this.btnImport);
            this.grpConnect.Controls.Add(this.btnExport);
            this.grpConnect.Controls.Add(this.picDatabase);
            this.grpConnect.Controls.Add(this.lblMainFormIconStyle);
            this.grpConnect.Controls.Add(this.btnHelp_ShowColumnInfo);
            this.grpConnect.Controls.Add(this.chkShowColumnInfo);
            this.grpConnect.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.grpConnect.Location = new System.Drawing.Point(10, 2);
            this.grpConnect.Name = "grpConnect";
            this.grpConnect.Size = new System.Drawing.Size(847, 440);
            this.grpConnect.TabIndex = 2;
            this.grpConnect.TabStop = false;
            this.c1ThemeController1.SetTheme(this.grpConnect, "(default)");
            // 
            // pnlPostgreSQL
            // 
            this.pnlPostgreSQL.Controls.Add(this.c1GridPostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.btnHelp_QueryTimeout_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblSeconds_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.nudQueryTimeout_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblQueryTimeout_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.cboDatabase_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.chkPooling_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.btnHelp_Pooling_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.btnHelp_ConnectionName_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtPort_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtPassword_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtUserID_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtConnectionName_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtServer_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.txtRemark_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblRemark_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblPassword_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblUserID_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblDatabase_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblServer_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblPort_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.lblConnectionName_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.chkUnicode_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.btnHelp_AutoRollback_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.btnHelp_Unicode_PostgreSQL);
            this.pnlPostgreSQL.Controls.Add(this.chkAutoRollback_PostgreSQL);
            this.pnlPostgreSQL.Location = new System.Drawing.Point(9, 37);
            this.pnlPostgreSQL.Name = "pnlPostgreSQL";
            this.pnlPostgreSQL.Size = new System.Drawing.Size(517, 223);
            this.pnlPostgreSQL.TabIndex = 77;
            this.c1ThemeController1.SetTheme(this.pnlPostgreSQL, "(default)");
            // 
            // c1GridPostgreSQL
            // 
            this.c1GridPostgreSQL.AllowFilter = false;
            this.c1GridPostgreSQL.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridPostgreSQL.AllowUpdate = false;
            this.c1GridPostgreSQL.AlternatingRows = true;
            this.c1GridPostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridPostgreSQL.CaptionHeight = 19;
            this.c1GridPostgreSQL.ColumnHeaders = false;
            this.c1GridPostgreSQL.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridPostgreSQL.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridPostgreSQL.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridPostgreSQL.Images"))));
            this.c1GridPostgreSQL.Location = new System.Drawing.Point(129, 82);
            this.c1GridPostgreSQL.Name = "c1GridPostgreSQL";
            this.c1GridPostgreSQL.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridPostgreSQL.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridPostgreSQL.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridPostgreSQL.PreviewInfo.ZoomFactor = 75D;
            this.c1GridPostgreSQL.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridPostgreSQL.PrintInfo.MeasurementPrinterName = null;
            this.c1GridPostgreSQL.RowHeight = 19;
            this.c1GridPostgreSQL.Size = new System.Drawing.Size(270, 183);
            this.c1GridPostgreSQL.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridPostgreSQL.TabIndex = 101;
            this.c1ThemeController1.SetTheme(this.c1GridPostgreSQL, "(default)");
            this.c1GridPostgreSQL.UseCompatibleTextRendering = false;
            this.c1GridPostgreSQL.Visible = false;
            this.c1GridPostgreSQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridPostgreSQL_KeyPress);
            this.c1GridPostgreSQL.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridPostgreSQL_MouseDoubleClick);
            this.c1GridPostgreSQL.PropBag = resources.GetString("c1GridPostgreSQL.PropBag");
            // 
            // btnHelp_QueryTimeout_PostgreSQL
            // 
            this.btnHelp_QueryTimeout_PostgreSQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_QueryTimeout_PostgreSQL.Image")));
            this.btnHelp_QueryTimeout_PostgreSQL.Location = new System.Drawing.Point(232, 116);
            this.btnHelp_QueryTimeout_PostgreSQL.Name = "btnHelp_QueryTimeout_PostgreSQL";
            this.btnHelp_QueryTimeout_PostgreSQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_QueryTimeout_PostgreSQL.TabIndex = 129;
            this.c1ThemeController1.SetTheme(this.btnHelp_QueryTimeout_PostgreSQL, "(default)");
            this.btnHelp_QueryTimeout_PostgreSQL.UseVisualStyleBackColor = true;
            this.btnHelp_QueryTimeout_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_QueryTimeout_PostgreSQL.Click += new System.EventHandler(this.btnHelp_QueryTimeout);
            // 
            // lblSeconds_PostgreSQL
            // 
            this.lblSeconds_PostgreSQL.AutoSize = true;
            this.lblSeconds_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSeconds_PostgreSQL.Location = new System.Drawing.Point(179, 117);
            this.lblSeconds_PostgreSQL.Name = "lblSeconds_PostgreSQL";
            this.lblSeconds_PostgreSQL.Size = new System.Drawing.Size(53, 16);
            this.lblSeconds_PostgreSQL.TabIndex = 128;
            this.lblSeconds_PostgreSQL.Text = "seconds";
            this.lblSeconds_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblSeconds_PostgreSQL, "(default)");
            // 
            // nudQueryTimeout_PostgreSQL
            // 
            this.nudQueryTimeout_PostgreSQL.Location = new System.Drawing.Point(129, 115);
            this.nudQueryTimeout_PostgreSQL.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout_PostgreSQL.Name = "nudQueryTimeout_PostgreSQL";
            this.nudQueryTimeout_PostgreSQL.Size = new System.Drawing.Size(48, 23);
            this.nudQueryTimeout_PostgreSQL.TabIndex = 127;
            this.nudQueryTimeout_PostgreSQL.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudQueryTimeout_PostgreSQL, "(default)");
            this.nudQueryTimeout_PostgreSQL.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudQueryTimeout_PostgreSQL.Enter += new System.EventHandler(this.nudQueryTimeout_Enter);
            this.nudQueryTimeout_PostgreSQL.Leave += new System.EventHandler(this.nudQueryTimeout_Leave);
            this.nudQueryTimeout_PostgreSQL.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudQueryTimeout_MouseClick);
            // 
            // lblQueryTimeout_PostgreSQL
            // 
            this.lblQueryTimeout_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblQueryTimeout_PostgreSQL.Location = new System.Drawing.Point(3, 117);
            this.lblQueryTimeout_PostgreSQL.Name = "lblQueryTimeout_PostgreSQL";
            this.lblQueryTimeout_PostgreSQL.Size = new System.Drawing.Size(125, 16);
            this.lblQueryTimeout_PostgreSQL.TabIndex = 126;
            this.lblQueryTimeout_PostgreSQL.Text = "Query Timeout:";
            this.lblQueryTimeout_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblQueryTimeout_PostgreSQL, "(default)");
            // 
            // cboDatabase_PostgreSQL
            // 
            this.cboDatabase_PostgreSQL.AllowSpinLoop = false;
            this.cboDatabase_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboDatabase_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDatabase_PostgreSQL.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboDatabase_PostgreSQL.GapHeight = 0;
            this.cboDatabase_PostgreSQL.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDatabase_PostgreSQL.Items.Add("yyyy/MM/dd");
            this.cboDatabase_PostgreSQL.Items.Add("yyyy-MM-dd");
            this.cboDatabase_PostgreSQL.Items.Add("MM/dd/yyyy");
            this.cboDatabase_PostgreSQL.Items.Add("MM-dd-yyyy");
            this.cboDatabase_PostgreSQL.Items.Add("dd/MM/yyyy");
            this.cboDatabase_PostgreSQL.Items.Add("dd-MM-yyyy");
            this.cboDatabase_PostgreSQL.ItemsDisplayMember = "";
            this.cboDatabase_PostgreSQL.ItemsValueMember = "";
            this.cboDatabase_PostgreSQL.Location = new System.Drawing.Point(129, 59);
            this.cboDatabase_PostgreSQL.Name = "cboDatabase_PostgreSQL";
            this.cboDatabase_PostgreSQL.Size = new System.Drawing.Size(163, 21);
            this.cboDatabase_PostgreSQL.TabIndex = 107;
            this.cboDatabase_PostgreSQL.Tag = null;
            this.cboDatabase_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDatabase_PostgreSQL, "(default)");
            this.cboDatabase_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDatabase_PostgreSQL.SelectedIndexChanged += new System.EventHandler(this.cboDatabase_PostgreSQL_SelectedIndexChanged);
            this.cboDatabase_PostgreSQL.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboDatabase_PostgreSQL_BeforeDropDownOpen);
            this.cboDatabase_PostgreSQL.DropDownOpened += new System.EventHandler(this.cboDatabase_PostgreSQL_DropDownOpened);
            this.cboDatabase_PostgreSQL.DropDownClosed += new C1.Win.C1Input.DropDownClosedEventHandler(this.cboDatabase_PostgreSQL_DropDownClosed);
            this.cboDatabase_PostgreSQL.TextChanged += new System.EventHandler(this.cboDatabase_PostgreSQL_TextChanged);
            this.cboDatabase_PostgreSQL.Enter += new System.EventHandler(this.cboDatabase_PostgreSQL_Enter);
            this.cboDatabase_PostgreSQL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_PostgreSQL_KeyDown);
            this.cboDatabase_PostgreSQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboDatabase_PostgreSQL_KeyPress);
            this.cboDatabase_PostgreSQL.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_PostgreSQL_KeyUp);
            this.cboDatabase_PostgreSQL.Leave += new System.EventHandler(this.cboDatabase_PostgreSQL_Leave);
            // 
            // chkPooling_PostgreSQL
            // 
            this.chkPooling_PostgreSQL.AutoSize = true;
            this.chkPooling_PostgreSQL.BackColor = System.Drawing.Color.Transparent;
            this.chkPooling_PostgreSQL.BorderColor = System.Drawing.Color.Transparent;
            this.chkPooling_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPooling_PostgreSQL.ForeColor = System.Drawing.Color.Black;
            this.chkPooling_PostgreSQL.Location = new System.Drawing.Point(337, 59);
            this.chkPooling_PostgreSQL.Name = "chkPooling_PostgreSQL";
            this.chkPooling_PostgreSQL.Padding = new System.Windows.Forms.Padding(1);
            this.chkPooling_PostgreSQL.Size = new System.Drawing.Size(114, 22);
            this.chkPooling_PostgreSQL.TabIndex = 106;
            this.chkPooling_PostgreSQL.Text = "Enable Pooling";
            this.c1ThemeController1.SetTheme(this.chkPooling_PostgreSQL, "(default)");
            this.chkPooling_PostgreSQL.UseVisualStyleBackColor = true;
            this.chkPooling_PostgreSQL.Value = null;
            this.chkPooling_PostgreSQL.Visible = false;
            this.chkPooling_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_Pooling_PostgreSQL
            // 
            this.btnHelp_Pooling_PostgreSQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Pooling_PostgreSQL.Image")));
            this.btnHelp_Pooling_PostgreSQL.Location = new System.Drawing.Point(481, 58);
            this.btnHelp_Pooling_PostgreSQL.Name = "btnHelp_Pooling_PostgreSQL";
            this.btnHelp_Pooling_PostgreSQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Pooling_PostgreSQL.TabIndex = 105;
            this.c1ThemeController1.SetTheme(this.btnHelp_Pooling_PostgreSQL, "(default)");
            this.btnHelp_Pooling_PostgreSQL.UseVisualStyleBackColor = true;
            this.btnHelp_Pooling_PostgreSQL.Visible = false;
            this.btnHelp_Pooling_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Pooling_PostgreSQL.Click += new System.EventHandler(this.btnHelp_Pooling_Click);
            // 
            // btnHelp_ConnectionName_PostgreSQL
            // 
            this.btnHelp_ConnectionName_PostgreSQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ConnectionName_PostgreSQL.Image")));
            this.btnHelp_ConnectionName_PostgreSQL.Location = new System.Drawing.Point(271, 3);
            this.btnHelp_ConnectionName_PostgreSQL.Name = "btnHelp_ConnectionName_PostgreSQL";
            this.btnHelp_ConnectionName_PostgreSQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ConnectionName_PostgreSQL.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.btnHelp_ConnectionName_PostgreSQL, "(default)");
            this.btnHelp_ConnectionName_PostgreSQL.UseVisualStyleBackColor = true;
            this.btnHelp_ConnectionName_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ConnectionName_PostgreSQL.Click += new System.EventHandler(this.btnHelp_ConnectionName_Click);
            // 
            // txtPort_PostgreSQL
            // 
            this.txtPort_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPort_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPort_PostgreSQL.Location = new System.Drawing.Point(129, 88);
            this.txtPort_PostgreSQL.Name = "txtPort_PostgreSQL";
            this.txtPort_PostgreSQL.Size = new System.Drawing.Size(57, 21);
            this.txtPort_PostgreSQL.TabIndex = 91;
            this.txtPort_PostgreSQL.Tag = null;
            this.txtPort_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPort_PostgreSQL, "(default)");
            this.txtPort_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtPort_PostgreSQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_Numeric_KeyPress);
            // 
            // txtPassword_PostgreSQL
            // 
            this.txtPassword_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPassword_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtPassword_PostgreSQL.Location = new System.Drawing.Point(393, 31);
            this.txtPassword_PostgreSQL.Name = "txtPassword_PostgreSQL";
            this.txtPassword_PostgreSQL.PasswordChar = '*';
            this.txtPassword_PostgreSQL.Size = new System.Drawing.Size(109, 21);
            this.txtPassword_PostgreSQL.TabIndex = 98;
            this.txtPassword_PostgreSQL.Tag = null;
            this.txtPassword_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPassword_PostgreSQL, "(default)");
            this.txtPassword_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtUserID_PostgreSQL
            // 
            this.txtUserID_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtUserID_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserID_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtUserID_PostgreSQL.Location = new System.Drawing.Point(393, 3);
            this.txtUserID_PostgreSQL.Name = "txtUserID_PostgreSQL";
            this.txtUserID_PostgreSQL.Size = new System.Drawing.Size(109, 21);
            this.txtUserID_PostgreSQL.TabIndex = 95;
            this.txtUserID_PostgreSQL.Tag = null;
            this.txtUserID_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtUserID_PostgreSQL, "(default)");
            this.txtUserID_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtConnectionName_PostgreSQL
            // 
            this.txtConnectionName_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConnectionName_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConnectionName_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConnectionName_PostgreSQL.Location = new System.Drawing.Point(129, 3);
            this.txtConnectionName_PostgreSQL.Name = "txtConnectionName_PostgreSQL";
            this.txtConnectionName_PostgreSQL.Size = new System.Drawing.Size(138, 21);
            this.txtConnectionName_PostgreSQL.TabIndex = 86;
            this.txtConnectionName_PostgreSQL.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtConnectionName_PostgreSQL, "(default)");
            this.txtConnectionName_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtServer_PostgreSQL
            // 
            this.txtServer_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtServer_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtServer_PostgreSQL.Location = new System.Drawing.Point(129, 31);
            this.txtServer_PostgreSQL.Name = "txtServer_PostgreSQL";
            this.txtServer_PostgreSQL.Size = new System.Drawing.Size(138, 21);
            this.txtServer_PostgreSQL.TabIndex = 88;
            this.txtServer_PostgreSQL.Tag = null;
            this.txtServer_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtServer_PostgreSQL, "(default)");
            this.txtServer_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtRemark_PostgreSQL
            // 
            this.txtRemark_PostgreSQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRemark_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark_PostgreSQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtRemark_PostgreSQL.Location = new System.Drawing.Point(129, 144);
            this.txtRemark_PostgreSQL.Multiline = true;
            this.txtRemark_PostgreSQL.Name = "txtRemark_PostgreSQL";
            this.txtRemark_PostgreSQL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemark_PostgreSQL.Size = new System.Drawing.Size(373, 67);
            this.txtRemark_PostgreSQL.TabIndex = 103;
            this.txtRemark_PostgreSQL.Tag = null;
            this.txtRemark_PostgreSQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRemark_PostgreSQL, "(default)");
            this.txtRemark_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblRemark_PostgreSQL
            // 
            this.lblRemark_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemark_PostgreSQL.Location = new System.Drawing.Point(39, 144);
            this.lblRemark_PostgreSQL.Name = "lblRemark_PostgreSQL";
            this.lblRemark_PostgreSQL.Size = new System.Drawing.Size(88, 16);
            this.lblRemark_PostgreSQL.TabIndex = 104;
            this.lblRemark_PostgreSQL.Text = "Remark:";
            this.lblRemark_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword_PostgreSQL
            // 
            this.lblPassword_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword_PostgreSQL.Location = new System.Drawing.Point(292, 33);
            this.lblPassword_PostgreSQL.Name = "lblPassword_PostgreSQL";
            this.lblPassword_PostgreSQL.Size = new System.Drawing.Size(100, 16);
            this.lblPassword_PostgreSQL.TabIndex = 96;
            this.lblPassword_PostgreSQL.Text = "Password:";
            this.lblPassword_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUserID_PostgreSQL
            // 
            this.lblUserID_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUserID_PostgreSQL.Location = new System.Drawing.Point(292, 5);
            this.lblUserID_PostgreSQL.Name = "lblUserID_PostgreSQL";
            this.lblUserID_PostgreSQL.Size = new System.Drawing.Size(100, 16);
            this.lblUserID_PostgreSQL.TabIndex = 94;
            this.lblUserID_PostgreSQL.Text = "User ID:";
            this.lblUserID_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDatabase_PostgreSQL
            // 
            this.lblDatabase_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDatabase_PostgreSQL.Location = new System.Drawing.Point(39, 61);
            this.lblDatabase_PostgreSQL.Name = "lblDatabase_PostgreSQL";
            this.lblDatabase_PostgreSQL.Size = new System.Drawing.Size(88, 16);
            this.lblDatabase_PostgreSQL.TabIndex = 93;
            this.lblDatabase_PostgreSQL.Text = "Database:";
            this.lblDatabase_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblServer_PostgreSQL
            // 
            this.lblServer_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblServer_PostgreSQL.Location = new System.Drawing.Point(3, 33);
            this.lblServer_PostgreSQL.Name = "lblServer_PostgreSQL";
            this.lblServer_PostgreSQL.Size = new System.Drawing.Size(125, 16);
            this.lblServer_PostgreSQL.TabIndex = 92;
            this.lblServer_PostgreSQL.Text = "Server/Host:";
            this.lblServer_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPort_PostgreSQL
            // 
            this.lblPort_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPort_PostgreSQL.Location = new System.Drawing.Point(39, 89);
            this.lblPort_PostgreSQL.Name = "lblPort_PostgreSQL";
            this.lblPort_PostgreSQL.Size = new System.Drawing.Size(88, 16);
            this.lblPort_PostgreSQL.TabIndex = 89;
            this.lblPort_PostgreSQL.Text = "Port:";
            this.lblPort_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionName_PostgreSQL
            // 
            this.lblConnectionName_PostgreSQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectionName_PostgreSQL.Location = new System.Drawing.Point(3, 5);
            this.lblConnectionName_PostgreSQL.Name = "lblConnectionName_PostgreSQL";
            this.lblConnectionName_PostgreSQL.Size = new System.Drawing.Size(125, 16);
            this.lblConnectionName_PostgreSQL.TabIndex = 85;
            this.lblConnectionName_PostgreSQL.Text = "Connection Name:";
            this.lblConnectionName_PostgreSQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkUnicode_PostgreSQL
            // 
            this.chkUnicode_PostgreSQL.AutoSize = true;
            this.chkUnicode_PostgreSQL.BackColor = System.Drawing.Color.Transparent;
            this.chkUnicode_PostgreSQL.BorderColor = System.Drawing.Color.Transparent;
            this.chkUnicode_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUnicode_PostgreSQL.ForeColor = System.Drawing.Color.Black;
            this.chkUnicode_PostgreSQL.Location = new System.Drawing.Point(357, 86);
            this.chkUnicode_PostgreSQL.Name = "chkUnicode_PostgreSQL";
            this.chkUnicode_PostgreSQL.Padding = new System.Windows.Forms.Padding(1);
            this.chkUnicode_PostgreSQL.Size = new System.Drawing.Size(118, 22);
            this.chkUnicode_PostgreSQL.TabIndex = 100;
            this.chkUnicode_PostgreSQL.Text = "Enable Unicode";
            this.c1ThemeController1.SetTheme(this.chkUnicode_PostgreSQL, "(default)");
            this.chkUnicode_PostgreSQL.UseVisualStyleBackColor = true;
            this.chkUnicode_PostgreSQL.Value = null;
            this.chkUnicode_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_AutoRollback_PostgreSQL
            // 
            this.btnHelp_AutoRollback_PostgreSQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_AutoRollback_PostgreSQL.Image")));
            this.btnHelp_AutoRollback_PostgreSQL.Location = new System.Drawing.Point(481, 112);
            this.btnHelp_AutoRollback_PostgreSQL.Name = "btnHelp_AutoRollback_PostgreSQL";
            this.btnHelp_AutoRollback_PostgreSQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_AutoRollback_PostgreSQL.TabIndex = 83;
            this.c1ThemeController1.SetTheme(this.btnHelp_AutoRollback_PostgreSQL, "(default)");
            this.btnHelp_AutoRollback_PostgreSQL.UseVisualStyleBackColor = true;
            this.btnHelp_AutoRollback_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_AutoRollback_PostgreSQL.Click += new System.EventHandler(this.btnHelp_AutoRollback_Click);
            // 
            // btnHelp_Unicode_PostgreSQL
            // 
            this.btnHelp_Unicode_PostgreSQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Unicode_PostgreSQL.Image")));
            this.btnHelp_Unicode_PostgreSQL.Location = new System.Drawing.Point(481, 85);
            this.btnHelp_Unicode_PostgreSQL.Name = "btnHelp_Unicode_PostgreSQL";
            this.btnHelp_Unicode_PostgreSQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Unicode_PostgreSQL.TabIndex = 82;
            this.c1ThemeController1.SetTheme(this.btnHelp_Unicode_PostgreSQL, "(default)");
            this.btnHelp_Unicode_PostgreSQL.UseVisualStyleBackColor = true;
            this.btnHelp_Unicode_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Unicode_PostgreSQL.Click += new System.EventHandler(this.btnHelp_Unicode_PostgreSQL_Click);
            // 
            // chkAutoRollback_PostgreSQL
            // 
            this.chkAutoRollback_PostgreSQL.AutoSize = true;
            this.chkAutoRollback_PostgreSQL.BackColor = System.Drawing.Color.Transparent;
            this.chkAutoRollback_PostgreSQL.BorderColor = System.Drawing.Color.Transparent;
            this.chkAutoRollback_PostgreSQL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkAutoRollback_PostgreSQL.ForeColor = System.Drawing.Color.Black;
            this.chkAutoRollback_PostgreSQL.Location = new System.Drawing.Point(320, 113);
            this.chkAutoRollback_PostgreSQL.Name = "chkAutoRollback_PostgreSQL";
            this.chkAutoRollback_PostgreSQL.Padding = new System.Windows.Forms.Padding(1);
            this.chkAutoRollback_PostgreSQL.Size = new System.Drawing.Size(149, 22);
            this.chkAutoRollback_PostgreSQL.TabIndex = 102;
            this.chkAutoRollback_PostgreSQL.Text = "Enable Auto Rollback";
            this.c1ThemeController1.SetTheme(this.chkAutoRollback_PostgreSQL, "(default)");
            this.chkAutoRollback_PostgreSQL.UseVisualStyleBackColor = true;
            this.chkAutoRollback_PostgreSQL.Value = null;
            this.chkAutoRollback_PostgreSQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblPrompt
            // 
            this.lblPrompt.AutoSize = true;
            this.lblPrompt.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPrompt.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblPrompt.Location = new System.Drawing.Point(307, 22);
            this.lblPrompt.Name = "lblPrompt";
            this.lblPrompt.Size = new System.Drawing.Size(184, 17);
            this.lblPrompt.TabIndex = 115;
            this.lblPrompt.Text = "Oracle is selected by default.";
            this.lblPrompt.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPrompt.Visible = false;
            // 
            // pnlOracle
            // 
            this.pnlOracle.Controls.Add(this.btnHelp_QueryTimeout_Oracle);
            this.pnlOracle.Controls.Add(this.lblSeconds_Oracle);
            this.pnlOracle.Controls.Add(this.nudQueryTimeout_Oracle);
            this.pnlOracle.Controls.Add(this.lblQueryTimeout_Oracle);
            this.pnlOracle.Controls.Add(this.chkUnicode_Oracle);
            this.pnlOracle.Controls.Add(this.btnHelp_Pooling_Oracle);
            this.pnlOracle.Controls.Add(this.chkPooling_Oracle);
            this.pnlOracle.Controls.Add(this.btnHelp_SID_Oracle);
            this.pnlOracle.Controls.Add(this.lblSID_Oracle);
            this.pnlOracle.Controls.Add(this.btnHelp_ConnectionName_Oracle);
            this.pnlOracle.Controls.Add(this.btnHelp_DirectMode);
            this.pnlOracle.Controls.Add(this.txtPort_Oracle);
            this.pnlOracle.Controls.Add(this.cboConnectAs_Oracle);
            this.pnlOracle.Controls.Add(this.txtSID_Oracle);
            this.pnlOracle.Controls.Add(this.txtPassword_Oracle);
            this.pnlOracle.Controls.Add(this.txtUserID_Oracle);
            this.pnlOracle.Controls.Add(this.txtConnectionName_Oracle);
            this.pnlOracle.Controls.Add(this.txtServer_Oracle);
            this.pnlOracle.Controls.Add(this.txtRemark_Oracle);
            this.pnlOracle.Controls.Add(this.lblRemark_Oracle);
            this.pnlOracle.Controls.Add(this.lblPassword_Oracle);
            this.pnlOracle.Controls.Add(this.lblUserID_Oracle);
            this.pnlOracle.Controls.Add(this.lblServer_Oracle);
            this.pnlOracle.Controls.Add(this.lblPort_Oracle);
            this.pnlOracle.Controls.Add(this.lblConnectionName_Oracle);
            this.pnlOracle.Controls.Add(this.chkDirectMode_Oracle);
            this.pnlOracle.Controls.Add(this.lblConnectAs_Oracle);
            this.pnlOracle.Controls.Add(this.textBox2);
            this.pnlOracle.Location = new System.Drawing.Point(26, 26);
            this.pnlOracle.Name = "pnlOracle";
            this.pnlOracle.Size = new System.Drawing.Size(517, 223);
            this.pnlOracle.TabIndex = 78;
            this.c1ThemeController1.SetTheme(this.pnlOracle, "(default)");
            // 
            // btnHelp_QueryTimeout_Oracle
            // 
            this.btnHelp_QueryTimeout_Oracle.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_QueryTimeout_Oracle.Image")));
            this.btnHelp_QueryTimeout_Oracle.Location = new System.Drawing.Point(232, 115);
            this.btnHelp_QueryTimeout_Oracle.Name = "btnHelp_QueryTimeout_Oracle";
            this.btnHelp_QueryTimeout_Oracle.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_QueryTimeout_Oracle.TabIndex = 113;
            this.c1ThemeController1.SetTheme(this.btnHelp_QueryTimeout_Oracle, "(default)");
            this.btnHelp_QueryTimeout_Oracle.UseVisualStyleBackColor = true;
            this.btnHelp_QueryTimeout_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_QueryTimeout_Oracle.Click += new System.EventHandler(this.btnHelp_QueryTimeout);
            // 
            // lblSeconds_Oracle
            // 
            this.lblSeconds_Oracle.AutoSize = true;
            this.lblSeconds_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSeconds_Oracle.Location = new System.Drawing.Point(179, 116);
            this.lblSeconds_Oracle.Name = "lblSeconds_Oracle";
            this.lblSeconds_Oracle.Size = new System.Drawing.Size(53, 16);
            this.lblSeconds_Oracle.TabIndex = 112;
            this.lblSeconds_Oracle.Text = "seconds";
            this.lblSeconds_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblSeconds_Oracle, "(default)");
            // 
            // nudQueryTimeout_Oracle
            // 
            this.nudQueryTimeout_Oracle.Location = new System.Drawing.Point(129, 114);
            this.nudQueryTimeout_Oracle.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout_Oracle.Name = "nudQueryTimeout_Oracle";
            this.nudQueryTimeout_Oracle.Size = new System.Drawing.Size(48, 23);
            this.nudQueryTimeout_Oracle.TabIndex = 111;
            this.nudQueryTimeout_Oracle.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudQueryTimeout_Oracle, "(default)");
            this.nudQueryTimeout_Oracle.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudQueryTimeout_Oracle.Enter += new System.EventHandler(this.nudQueryTimeout_Enter);
            this.nudQueryTimeout_Oracle.Leave += new System.EventHandler(this.nudQueryTimeout_Leave);
            this.nudQueryTimeout_Oracle.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudQueryTimeout_MouseClick);
            // 
            // lblQueryTimeout_Oracle
            // 
            this.lblQueryTimeout_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblQueryTimeout_Oracle.Location = new System.Drawing.Point(3, 116);
            this.lblQueryTimeout_Oracle.Name = "lblQueryTimeout_Oracle";
            this.lblQueryTimeout_Oracle.Size = new System.Drawing.Size(125, 16);
            this.lblQueryTimeout_Oracle.TabIndex = 110;
            this.lblQueryTimeout_Oracle.Text = "Query Timeout:";
            this.lblQueryTimeout_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblQueryTimeout_Oracle, "(default)");
            // 
            // chkUnicode_Oracle
            // 
            this.chkUnicode_Oracle.AutoSize = true;
            this.chkUnicode_Oracle.BackColor = System.Drawing.Color.Transparent;
            this.chkUnicode_Oracle.BorderColor = System.Drawing.Color.Transparent;
            this.chkUnicode_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUnicode_Oracle.ForeColor = System.Drawing.Color.Black;
            this.chkUnicode_Oracle.Location = new System.Drawing.Point(361, 59);
            this.chkUnicode_Oracle.Name = "chkUnicode_Oracle";
            this.chkUnicode_Oracle.Padding = new System.Windows.Forms.Padding(1);
            this.chkUnicode_Oracle.Size = new System.Drawing.Size(118, 22);
            this.chkUnicode_Oracle.TabIndex = 109;
            this.chkUnicode_Oracle.Text = "Enable Unicode";
            this.c1ThemeController1.SetTheme(this.chkUnicode_Oracle, "(default)");
            this.chkUnicode_Oracle.UseVisualStyleBackColor = true;
            this.chkUnicode_Oracle.Value = null;
            this.chkUnicode_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_Pooling_Oracle
            // 
            this.btnHelp_Pooling_Oracle.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Pooling_Oracle.Image")));
            this.btnHelp_Pooling_Oracle.Location = new System.Drawing.Point(481, 59);
            this.btnHelp_Pooling_Oracle.Name = "btnHelp_Pooling_Oracle";
            this.btnHelp_Pooling_Oracle.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Pooling_Oracle.TabIndex = 108;
            this.c1ThemeController1.SetTheme(this.btnHelp_Pooling_Oracle, "(default)");
            this.btnHelp_Pooling_Oracle.UseVisualStyleBackColor = true;
            this.btnHelp_Pooling_Oracle.Visible = false;
            this.btnHelp_Pooling_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Pooling_Oracle.Click += new System.EventHandler(this.btnHelp_Pooling_Click);
            // 
            // chkPooling_Oracle
            // 
            this.chkPooling_Oracle.AutoSize = true;
            this.chkPooling_Oracle.BackColor = System.Drawing.Color.Transparent;
            this.chkPooling_Oracle.BorderColor = System.Drawing.Color.Transparent;
            this.chkPooling_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPooling_Oracle.ForeColor = System.Drawing.Color.Black;
            this.chkPooling_Oracle.Location = new System.Drawing.Point(346, 59);
            this.chkPooling_Oracle.Name = "chkPooling_Oracle";
            this.chkPooling_Oracle.Padding = new System.Windows.Forms.Padding(1);
            this.chkPooling_Oracle.Size = new System.Drawing.Size(114, 22);
            this.chkPooling_Oracle.TabIndex = 107;
            this.chkPooling_Oracle.Text = "Enable Pooling";
            this.c1ThemeController1.SetTheme(this.chkPooling_Oracle, "(default)");
            this.chkPooling_Oracle.UseVisualStyleBackColor = true;
            this.chkPooling_Oracle.Value = null;
            this.chkPooling_Oracle.Visible = false;
            this.chkPooling_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_SID_Oracle
            // 
            this.btnHelp_SID_Oracle.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_SID_Oracle.Image")));
            this.btnHelp_SID_Oracle.Location = new System.Drawing.Point(481, 112);
            this.btnHelp_SID_Oracle.Name = "btnHelp_SID_Oracle";
            this.btnHelp_SID_Oracle.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_SID_Oracle.TabIndex = 106;
            this.c1ThemeController1.SetTheme(this.btnHelp_SID_Oracle, "(default)");
            this.btnHelp_SID_Oracle.UseVisualStyleBackColor = true;
            this.btnHelp_SID_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_SID_Oracle.Click += new System.EventHandler(this.btnHelp_SID_Click);
            // 
            // lblSID_Oracle
            // 
            this.lblSID_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSID_Oracle.Location = new System.Drawing.Point(292, 114);
            this.lblSID_Oracle.Name = "lblSID_Oracle";
            this.lblSID_Oracle.Size = new System.Drawing.Size(100, 16);
            this.lblSID_Oracle.TabIndex = 105;
            this.lblSID_Oracle.Text = "SID:";
            this.lblSID_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblSID_Oracle, "(default)");
            // 
            // btnHelp_ConnectionName_Oracle
            // 
            this.btnHelp_ConnectionName_Oracle.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ConnectionName_Oracle.Image")));
            this.btnHelp_ConnectionName_Oracle.Location = new System.Drawing.Point(271, 3);
            this.btnHelp_ConnectionName_Oracle.Name = "btnHelp_ConnectionName_Oracle";
            this.btnHelp_ConnectionName_Oracle.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ConnectionName_Oracle.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.btnHelp_ConnectionName_Oracle, "(default)");
            this.btnHelp_ConnectionName_Oracle.UseVisualStyleBackColor = true;
            this.btnHelp_ConnectionName_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ConnectionName_Oracle.Click += new System.EventHandler(this.btnHelp_ConnectionName_Click);
            // 
            // btnHelp_DirectMode
            // 
            this.btnHelp_DirectMode.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_DirectMode.Image")));
            this.btnHelp_DirectMode.Location = new System.Drawing.Point(481, 84);
            this.btnHelp_DirectMode.Name = "btnHelp_DirectMode";
            this.btnHelp_DirectMode.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_DirectMode.TabIndex = 64;
            this.c1ThemeController1.SetTheme(this.btnHelp_DirectMode, "(default)");
            this.btnHelp_DirectMode.UseVisualStyleBackColor = true;
            this.btnHelp_DirectMode.Click += new System.EventHandler(this.btnHelp_DirectMode_Click);
            // 
            // txtPort_Oracle
            // 
            this.txtPort_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPort_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort_Oracle.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPort_Oracle.Location = new System.Drawing.Point(129, 59);
            this.txtPort_Oracle.Name = "txtPort_Oracle";
            this.txtPort_Oracle.Size = new System.Drawing.Size(57, 21);
            this.txtPort_Oracle.TabIndex = 91;
            this.txtPort_Oracle.Tag = null;
            this.txtPort_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPort_Oracle, "(default)");
            this.txtPort_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtPort_Oracle.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_Numeric_KeyPress);
            // 
            // cboConnectAs_Oracle
            // 
            this.cboConnectAs_Oracle.AllowSpinLoop = false;
            this.cboConnectAs_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboConnectAs_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboConnectAs_Oracle.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboConnectAs_Oracle.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboConnectAs_Oracle.GapHeight = 0;
            this.cboConnectAs_Oracle.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboConnectAs_Oracle.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.cboConnectAs_Oracle.Items.Add("Normal");
            this.cboConnectAs_Oracle.Items.Add("SYSDBA");
            this.cboConnectAs_Oracle.Items.Add("SYSOPER");
            this.cboConnectAs_Oracle.ItemsDisplayMember = "";
            this.cboConnectAs_Oracle.ItemsValueMember = "";
            this.cboConnectAs_Oracle.Location = new System.Drawing.Point(129, 87);
            this.cboConnectAs_Oracle.Name = "cboConnectAs_Oracle";
            this.cboConnectAs_Oracle.Size = new System.Drawing.Size(84, 21);
            this.cboConnectAs_Oracle.TabIndex = 99;
            this.cboConnectAs_Oracle.Tag = null;
            this.cboConnectAs_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboConnectAs_Oracle, "(default)");
            this.cboConnectAs_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtSID_Oracle
            // 
            this.txtSID_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtSID_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSID_Oracle.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtSID_Oracle.Location = new System.Drawing.Point(393, 112);
            this.txtSID_Oracle.Name = "txtSID_Oracle";
            this.txtSID_Oracle.Size = new System.Drawing.Size(84, 21);
            this.txtSID_Oracle.TabIndex = 101;
            this.txtSID_Oracle.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtSID_Oracle, "(default)");
            this.txtSID_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtPassword_Oracle
            // 
            this.txtPassword_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPassword_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword_Oracle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtPassword_Oracle.Location = new System.Drawing.Point(393, 31);
            this.txtPassword_Oracle.Name = "txtPassword_Oracle";
            this.txtPassword_Oracle.PasswordChar = '*';
            this.txtPassword_Oracle.Size = new System.Drawing.Size(109, 21);
            this.txtPassword_Oracle.TabIndex = 98;
            this.txtPassword_Oracle.Tag = null;
            this.txtPassword_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPassword_Oracle, "(default)");
            this.txtPassword_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtUserID_Oracle
            // 
            this.txtUserID_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtUserID_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserID_Oracle.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtUserID_Oracle.Location = new System.Drawing.Point(393, 3);
            this.txtUserID_Oracle.Name = "txtUserID_Oracle";
            this.txtUserID_Oracle.Size = new System.Drawing.Size(109, 21);
            this.txtUserID_Oracle.TabIndex = 95;
            this.txtUserID_Oracle.Tag = null;
            this.txtUserID_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtUserID_Oracle, "(default)");
            this.txtUserID_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtConnectionName_Oracle
            // 
            this.txtConnectionName_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConnectionName_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConnectionName_Oracle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConnectionName_Oracle.Location = new System.Drawing.Point(129, 3);
            this.txtConnectionName_Oracle.Name = "txtConnectionName_Oracle";
            this.txtConnectionName_Oracle.Size = new System.Drawing.Size(138, 21);
            this.txtConnectionName_Oracle.TabIndex = 86;
            this.txtConnectionName_Oracle.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtConnectionName_Oracle, "(default)");
            this.txtConnectionName_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtServer_Oracle
            // 
            this.txtServer_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtServer_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer_Oracle.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtServer_Oracle.Location = new System.Drawing.Point(129, 31);
            this.txtServer_Oracle.Name = "txtServer_Oracle";
            this.txtServer_Oracle.Size = new System.Drawing.Size(138, 21);
            this.txtServer_Oracle.TabIndex = 88;
            this.txtServer_Oracle.Tag = null;
            this.txtServer_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtServer_Oracle, "(default)");
            this.txtServer_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtRemark_Oracle
            // 
            this.txtRemark_Oracle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRemark_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark_Oracle.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtRemark_Oracle.Location = new System.Drawing.Point(129, 143);
            this.txtRemark_Oracle.Multiline = true;
            this.txtRemark_Oracle.Name = "txtRemark_Oracle";
            this.txtRemark_Oracle.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemark_Oracle.Size = new System.Drawing.Size(373, 65);
            this.txtRemark_Oracle.TabIndex = 103;
            this.txtRemark_Oracle.Tag = null;
            this.txtRemark_Oracle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRemark_Oracle, "(default)");
            this.txtRemark_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblRemark_Oracle
            // 
            this.lblRemark_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemark_Oracle.Location = new System.Drawing.Point(39, 145);
            this.lblRemark_Oracle.Name = "lblRemark_Oracle";
            this.lblRemark_Oracle.Size = new System.Drawing.Size(88, 16);
            this.lblRemark_Oracle.TabIndex = 104;
            this.lblRemark_Oracle.Text = "Remark:";
            this.lblRemark_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblRemark_Oracle, "(default)");
            // 
            // lblPassword_Oracle
            // 
            this.lblPassword_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword_Oracle.Location = new System.Drawing.Point(292, 33);
            this.lblPassword_Oracle.Name = "lblPassword_Oracle";
            this.lblPassword_Oracle.Size = new System.Drawing.Size(100, 16);
            this.lblPassword_Oracle.TabIndex = 96;
            this.lblPassword_Oracle.Text = "Password:";
            this.lblPassword_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblPassword_Oracle, "(default)");
            // 
            // lblUserID_Oracle
            // 
            this.lblUserID_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUserID_Oracle.Location = new System.Drawing.Point(292, 5);
            this.lblUserID_Oracle.Name = "lblUserID_Oracle";
            this.lblUserID_Oracle.Size = new System.Drawing.Size(100, 16);
            this.lblUserID_Oracle.TabIndex = 94;
            this.lblUserID_Oracle.Text = "User ID:";
            this.lblUserID_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblUserID_Oracle, "(default)");
            // 
            // lblServer_Oracle
            // 
            this.lblServer_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblServer_Oracle.Location = new System.Drawing.Point(3, 33);
            this.lblServer_Oracle.Name = "lblServer_Oracle";
            this.lblServer_Oracle.Size = new System.Drawing.Size(125, 16);
            this.lblServer_Oracle.TabIndex = 92;
            this.lblServer_Oracle.Text = "Server/Host:";
            this.lblServer_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblServer_Oracle, "(default)");
            // 
            // lblPort_Oracle
            // 
            this.lblPort_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPort_Oracle.Location = new System.Drawing.Point(39, 61);
            this.lblPort_Oracle.Name = "lblPort_Oracle";
            this.lblPort_Oracle.Size = new System.Drawing.Size(88, 16);
            this.lblPort_Oracle.TabIndex = 89;
            this.lblPort_Oracle.Text = "Port:";
            this.lblPort_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblPort_Oracle, "(default)");
            // 
            // lblConnectionName_Oracle
            // 
            this.lblConnectionName_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectionName_Oracle.Location = new System.Drawing.Point(3, 5);
            this.lblConnectionName_Oracle.Name = "lblConnectionName_Oracle";
            this.lblConnectionName_Oracle.Size = new System.Drawing.Size(125, 16);
            this.lblConnectionName_Oracle.TabIndex = 85;
            this.lblConnectionName_Oracle.Text = "Connection Name:";
            this.lblConnectionName_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblConnectionName_Oracle, "(default)");
            // 
            // chkDirectMode_Oracle
            // 
            this.chkDirectMode_Oracle.AutoSize = true;
            this.chkDirectMode_Oracle.BackColor = System.Drawing.Color.Transparent;
            this.chkDirectMode_Oracle.BorderColor = System.Drawing.Color.Transparent;
            this.chkDirectMode_Oracle.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDirectMode_Oracle.ForeColor = System.Drawing.Color.Black;
            this.chkDirectMode_Oracle.Location = new System.Drawing.Point(346, 84);
            this.chkDirectMode_Oracle.Name = "chkDirectMode_Oracle";
            this.chkDirectMode_Oracle.Padding = new System.Windows.Forms.Padding(1);
            this.chkDirectMode_Oracle.Size = new System.Drawing.Size(141, 22);
            this.chkDirectMode_Oracle.TabIndex = 63;
            this.chkDirectMode_Oracle.Text = "Enable Direct Mode";
            this.c1ThemeController1.SetTheme(this.chkDirectMode_Oracle, "(default)");
            this.chkDirectMode_Oracle.UseVisualStyleBackColor = true;
            this.chkDirectMode_Oracle.Value = null;
            this.chkDirectMode_Oracle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkDirectMode_Oracle.CheckedChanged += new System.EventHandler(this.chkDirectMode_CheckedChanged);
            // 
            // lblConnectAs_Oracle
            // 
            this.lblConnectAs_Oracle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectAs_Oracle.Location = new System.Drawing.Point(3, 88);
            this.lblConnectAs_Oracle.Name = "lblConnectAs_Oracle";
            this.lblConnectAs_Oracle.Size = new System.Drawing.Size(125, 16);
            this.lblConnectAs_Oracle.TabIndex = 97;
            this.lblConnectAs_Oracle.Text = "Connect As:";
            this.lblConnectAs_Oracle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblConnectAs_Oracle, "(default)");
            // 
            // textBox2
            // 
            this.textBox2.Location = new System.Drawing.Point(481, 2);
            this.textBox2.Multiline = true;
            this.textBox2.Name = "textBox2";
            this.textBox2.ReadOnly = true;
            this.textBox2.Size = new System.Drawing.Size(35, 151);
            this.textBox2.TabIndex = 81;
            this.c1ThemeController1.SetTheme(this.textBox2, "(default)");
            this.textBox2.Visible = false;
            // 
            // btnHelp_Log
            // 
            this.btnHelp_Log.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Log.Image")));
            this.btnHelp_Log.Location = new System.Drawing.Point(595, 258);
            this.btnHelp_Log.Name = "btnHelp_Log";
            this.btnHelp_Log.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Log.TabIndex = 118;
            this.c1ThemeController1.SetTheme(this.btnHelp_Log, "(default)");
            this.btnHelp_Log.UseVisualStyleBackColor = true;
            this.btnHelp_Log.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Log.Click += new System.EventHandler(this.btnHelp_Log_Click);
            // 
            // chkLog
            // 
            this.chkLog.AutoSize = true;
            this.chkLog.BackColor = System.Drawing.Color.Transparent;
            this.chkLog.BorderColor = System.Drawing.Color.Transparent;
            this.chkLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkLog.ForeColor = System.Drawing.Color.Black;
            this.chkLog.Location = new System.Drawing.Point(539, 258);
            this.chkLog.Name = "chkLog";
            this.chkLog.Padding = new System.Windows.Forms.Padding(1);
            this.chkLog.Size = new System.Drawing.Size(105, 22);
            this.chkLog.TabIndex = 117;
            this.chkLog.Text = "Runtime Logs";
            this.c1ThemeController1.SetTheme(this.chkLog, "(default)");
            this.chkLog.UseVisualStyleBackColor = true;
            this.chkLog.Value = false;
            this.chkLog.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // pnlMySQL
            // 
            this.pnlMySQL.Controls.Add(this.c1GridMySQL);
            this.pnlMySQL.Controls.Add(this.btnHelp_QueryTimeout_MySQL);
            this.pnlMySQL.Controls.Add(this.lblSeconds_MySQL);
            this.pnlMySQL.Controls.Add(this.nudQueryTimeout_MySQL);
            this.pnlMySQL.Controls.Add(this.lblQueryTimeout_MySQL);
            this.pnlMySQL.Controls.Add(this.txtPort_MySQL);
            this.pnlMySQL.Controls.Add(this.lblPort_MySQL);
            this.pnlMySQL.Controls.Add(this.cboDatabase_MySQL);
            this.pnlMySQL.Controls.Add(this.btnHelp_Unicode_MySQL);
            this.pnlMySQL.Controls.Add(this.chkUnicode_MySQL);
            this.pnlMySQL.Controls.Add(this.btnHelp_Pooling_MySQL);
            this.pnlMySQL.Controls.Add(this.chkPooling_MySQL);
            this.pnlMySQL.Controls.Add(this.btnHelp_ConnectionName_MySQL);
            this.pnlMySQL.Controls.Add(this.txtPassword_MySQL);
            this.pnlMySQL.Controls.Add(this.txtUserID_MySQL);
            this.pnlMySQL.Controls.Add(this.txtConnectionName_MySQL);
            this.pnlMySQL.Controls.Add(this.txtServer_MySQL);
            this.pnlMySQL.Controls.Add(this.txtRemark_MySQL);
            this.pnlMySQL.Controls.Add(this.lblRemark_MySQL);
            this.pnlMySQL.Controls.Add(this.lblPassword_MySQL);
            this.pnlMySQL.Controls.Add(this.lblUserID_MySQL);
            this.pnlMySQL.Controls.Add(this.lblDatabase_MySQL);
            this.pnlMySQL.Controls.Add(this.lblServer_MySQL);
            this.pnlMySQL.Controls.Add(this.lblConnectionName_MySQL);
            this.pnlMySQL.Location = new System.Drawing.Point(17, 33);
            this.pnlMySQL.Name = "pnlMySQL";
            this.pnlMySQL.Size = new System.Drawing.Size(517, 223);
            this.pnlMySQL.TabIndex = 103;
            this.c1ThemeController1.SetTheme(this.pnlMySQL, "(default)");
            // 
            // c1GridMySQL
            // 
            this.c1GridMySQL.AllowFilter = false;
            this.c1GridMySQL.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridMySQL.AllowUpdate = false;
            this.c1GridMySQL.AlternatingRows = true;
            this.c1GridMySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridMySQL.CaptionHeight = 19;
            this.c1GridMySQL.ColumnHeaders = false;
            this.c1GridMySQL.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridMySQL.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridMySQL.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridMySQL.Images"))));
            this.c1GridMySQL.Location = new System.Drawing.Point(129, 82);
            this.c1GridMySQL.Name = "c1GridMySQL";
            this.c1GridMySQL.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridMySQL.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridMySQL.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridMySQL.PreviewInfo.ZoomFactor = 75D;
            this.c1GridMySQL.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridMySQL.PrintInfo.MeasurementPrinterName = null;
            this.c1GridMySQL.RowHeight = 19;
            this.c1GridMySQL.Size = new System.Drawing.Size(270, 183);
            this.c1GridMySQL.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridMySQL.TabIndex = 102;
            this.c1ThemeController1.SetTheme(this.c1GridMySQL, "(default)");
            this.c1GridMySQL.UseCompatibleTextRendering = false;
            this.c1GridMySQL.Visible = false;
            this.c1GridMySQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridMySQL_KeyPress);
            this.c1GridMySQL.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridMySQL_MouseDoubleClick);
            this.c1GridMySQL.PropBag = resources.GetString("c1GridMySQL.PropBag");
            // 
            // btnHelp_QueryTimeout_MySQL
            // 
            this.btnHelp_QueryTimeout_MySQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_QueryTimeout_MySQL.Image")));
            this.btnHelp_QueryTimeout_MySQL.Location = new System.Drawing.Point(232, 116);
            this.btnHelp_QueryTimeout_MySQL.Name = "btnHelp_QueryTimeout_MySQL";
            this.btnHelp_QueryTimeout_MySQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_QueryTimeout_MySQL.TabIndex = 121;
            this.c1ThemeController1.SetTheme(this.btnHelp_QueryTimeout_MySQL, "(default)");
            this.btnHelp_QueryTimeout_MySQL.UseVisualStyleBackColor = true;
            this.btnHelp_QueryTimeout_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_QueryTimeout_MySQL.Click += new System.EventHandler(this.btnHelp_QueryTimeout);
            // 
            // lblSeconds_MySQL
            // 
            this.lblSeconds_MySQL.AutoSize = true;
            this.lblSeconds_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSeconds_MySQL.Location = new System.Drawing.Point(179, 117);
            this.lblSeconds_MySQL.Name = "lblSeconds_MySQL";
            this.lblSeconds_MySQL.Size = new System.Drawing.Size(53, 16);
            this.lblSeconds_MySQL.TabIndex = 120;
            this.lblSeconds_MySQL.Text = "seconds";
            this.lblSeconds_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblSeconds_MySQL, "(default)");
            // 
            // nudQueryTimeout_MySQL
            // 
            this.nudQueryTimeout_MySQL.Location = new System.Drawing.Point(129, 115);
            this.nudQueryTimeout_MySQL.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout_MySQL.Name = "nudQueryTimeout_MySQL";
            this.nudQueryTimeout_MySQL.Size = new System.Drawing.Size(48, 23);
            this.nudQueryTimeout_MySQL.TabIndex = 119;
            this.nudQueryTimeout_MySQL.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudQueryTimeout_MySQL, "(default)");
            this.nudQueryTimeout_MySQL.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudQueryTimeout_MySQL.Enter += new System.EventHandler(this.nudQueryTimeout_Enter);
            this.nudQueryTimeout_MySQL.Leave += new System.EventHandler(this.nudQueryTimeout_Leave);
            this.nudQueryTimeout_MySQL.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudQueryTimeout_MouseClick);
            // 
            // lblQueryTimeout_MySQL
            // 
            this.lblQueryTimeout_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblQueryTimeout_MySQL.Location = new System.Drawing.Point(3, 117);
            this.lblQueryTimeout_MySQL.Name = "lblQueryTimeout_MySQL";
            this.lblQueryTimeout_MySQL.Size = new System.Drawing.Size(125, 16);
            this.lblQueryTimeout_MySQL.TabIndex = 118;
            this.lblQueryTimeout_MySQL.Text = "Query Timeout:";
            this.lblQueryTimeout_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblQueryTimeout_MySQL, "(default)");
            // 
            // txtPort_MySQL
            // 
            this.txtPort_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPort_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort_MySQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPort_MySQL.Location = new System.Drawing.Point(129, 87);
            this.txtPort_MySQL.Name = "txtPort_MySQL";
            this.txtPort_MySQL.Size = new System.Drawing.Size(57, 21);
            this.txtPort_MySQL.TabIndex = 111;
            this.txtPort_MySQL.Tag = null;
            this.txtPort_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPort_MySQL, "(default)");
            this.txtPort_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtPort_MySQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_Numeric_KeyPress);
            this.txtPort_MySQL.Leave += new System.EventHandler(this.txtPort_MySQL_Leave);
            // 
            // lblPort_MySQL
            // 
            this.lblPort_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPort_MySQL.Location = new System.Drawing.Point(39, 89);
            this.lblPort_MySQL.Name = "lblPort_MySQL";
            this.lblPort_MySQL.Size = new System.Drawing.Size(88, 16);
            this.lblPort_MySQL.TabIndex = 110;
            this.lblPort_MySQL.Text = "Port:";
            this.lblPort_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblPort_MySQL, "(default)");
            // 
            // cboDatabase_MySQL
            // 
            this.cboDatabase_MySQL.AllowSpinLoop = false;
            this.cboDatabase_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboDatabase_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDatabase_MySQL.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboDatabase_MySQL.GapHeight = 0;
            this.cboDatabase_MySQL.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDatabase_MySQL.Items.Add("yyyy/MM/dd");
            this.cboDatabase_MySQL.Items.Add("yyyy-MM-dd");
            this.cboDatabase_MySQL.Items.Add("MM/dd/yyyy");
            this.cboDatabase_MySQL.Items.Add("MM-dd-yyyy");
            this.cboDatabase_MySQL.Items.Add("dd/MM/yyyy");
            this.cboDatabase_MySQL.Items.Add("dd-MM-yyyy");
            this.cboDatabase_MySQL.ItemsDisplayMember = "";
            this.cboDatabase_MySQL.ItemsValueMember = "";
            this.cboDatabase_MySQL.Location = new System.Drawing.Point(129, 59);
            this.cboDatabase_MySQL.Name = "cboDatabase_MySQL";
            this.cboDatabase_MySQL.Size = new System.Drawing.Size(163, 21);
            this.cboDatabase_MySQL.TabIndex = 109;
            this.cboDatabase_MySQL.Tag = null;
            this.cboDatabase_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDatabase_MySQL, "(default)");
            this.cboDatabase_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDatabase_MySQL.SelectedIndexChanged += new System.EventHandler(this.cboDatabase_MySQL_SelectedIndexChanged);
            this.cboDatabase_MySQL.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboDatabase_MySQL_BeforeDropDownOpen);
            this.cboDatabase_MySQL.DropDownOpened += new System.EventHandler(this.cboDatabase_MySQL_DropDownOpened);
            this.cboDatabase_MySQL.DropDownClosed += new C1.Win.C1Input.DropDownClosedEventHandler(this.cboDatabase_MySQL_DropDownClosed);
            this.cboDatabase_MySQL.TextChanged += new System.EventHandler(this.cboDatabase_MySQL_TextChanged);
            this.cboDatabase_MySQL.Enter += new System.EventHandler(this.cboDatabase_MySQL_Enter);
            this.cboDatabase_MySQL.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_MySQL_KeyDown);
            this.cboDatabase_MySQL.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboDatabase_MySQL_KeyPress);
            this.cboDatabase_MySQL.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_MySQL_KeyUp);
            this.cboDatabase_MySQL.Leave += new System.EventHandler(this.cboDatabase_MySQL_Leave);
            // 
            // btnHelp_Unicode_MySQL
            // 
            this.btnHelp_Unicode_MySQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Unicode_MySQL.Image")));
            this.btnHelp_Unicode_MySQL.Location = new System.Drawing.Point(481, 85);
            this.btnHelp_Unicode_MySQL.Name = "btnHelp_Unicode_MySQL";
            this.btnHelp_Unicode_MySQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Unicode_MySQL.TabIndex = 108;
            this.c1ThemeController1.SetTheme(this.btnHelp_Unicode_MySQL, "(default)");
            this.btnHelp_Unicode_MySQL.UseVisualStyleBackColor = true;
            this.btnHelp_Unicode_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Unicode_MySQL.Click += new System.EventHandler(this.btnHelp_Unicode_MySQL_Click);
            // 
            // chkUnicode_MySQL
            // 
            this.chkUnicode_MySQL.AutoSize = true;
            this.chkUnicode_MySQL.BackColor = System.Drawing.Color.Transparent;
            this.chkUnicode_MySQL.BorderColor = System.Drawing.Color.Transparent;
            this.chkUnicode_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUnicode_MySQL.ForeColor = System.Drawing.Color.Black;
            this.chkUnicode_MySQL.Location = new System.Drawing.Point(327, 86);
            this.chkUnicode_MySQL.Name = "chkUnicode_MySQL";
            this.chkUnicode_MySQL.Padding = new System.Windows.Forms.Padding(1);
            this.chkUnicode_MySQL.Size = new System.Drawing.Size(118, 22);
            this.chkUnicode_MySQL.TabIndex = 107;
            this.chkUnicode_MySQL.Text = "Enable Unicode";
            this.c1ThemeController1.SetTheme(this.chkUnicode_MySQL, "(default)");
            this.chkUnicode_MySQL.UseVisualStyleBackColor = true;
            this.chkUnicode_MySQL.Value = null;
            this.chkUnicode_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_Pooling_MySQL
            // 
            this.btnHelp_Pooling_MySQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Pooling_MySQL.Image")));
            this.btnHelp_Pooling_MySQL.Location = new System.Drawing.Point(481, 58);
            this.btnHelp_Pooling_MySQL.Name = "btnHelp_Pooling_MySQL";
            this.btnHelp_Pooling_MySQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Pooling_MySQL.TabIndex = 106;
            this.c1ThemeController1.SetTheme(this.btnHelp_Pooling_MySQL, "(default)");
            this.btnHelp_Pooling_MySQL.UseVisualStyleBackColor = true;
            this.btnHelp_Pooling_MySQL.Visible = false;
            this.btnHelp_Pooling_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Pooling_MySQL.Click += new System.EventHandler(this.btnHelp_Pooling_Click);
            // 
            // chkPooling_MySQL
            // 
            this.chkPooling_MySQL.AutoSize = true;
            this.chkPooling_MySQL.BackColor = System.Drawing.Color.Transparent;
            this.chkPooling_MySQL.BorderColor = System.Drawing.Color.Transparent;
            this.chkPooling_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPooling_MySQL.ForeColor = System.Drawing.Color.Black;
            this.chkPooling_MySQL.Location = new System.Drawing.Point(327, 59);
            this.chkPooling_MySQL.Name = "chkPooling_MySQL";
            this.chkPooling_MySQL.Padding = new System.Windows.Forms.Padding(1);
            this.chkPooling_MySQL.Size = new System.Drawing.Size(114, 22);
            this.chkPooling_MySQL.TabIndex = 105;
            this.chkPooling_MySQL.Text = "Enable Pooling";
            this.c1ThemeController1.SetTheme(this.chkPooling_MySQL, "(default)");
            this.chkPooling_MySQL.UseVisualStyleBackColor = true;
            this.chkPooling_MySQL.Value = null;
            this.chkPooling_MySQL.Visible = false;
            this.chkPooling_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_ConnectionName_MySQL
            // 
            this.btnHelp_ConnectionName_MySQL.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ConnectionName_MySQL.Image")));
            this.btnHelp_ConnectionName_MySQL.Location = new System.Drawing.Point(271, 3);
            this.btnHelp_ConnectionName_MySQL.Name = "btnHelp_ConnectionName_MySQL";
            this.btnHelp_ConnectionName_MySQL.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ConnectionName_MySQL.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.btnHelp_ConnectionName_MySQL, "(default)");
            this.btnHelp_ConnectionName_MySQL.UseVisualStyleBackColor = true;
            this.btnHelp_ConnectionName_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ConnectionName_MySQL.Click += new System.EventHandler(this.btnHelp_ConnectionName_Click);
            // 
            // txtPassword_MySQL
            // 
            this.txtPassword_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPassword_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword_MySQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtPassword_MySQL.Location = new System.Drawing.Point(393, 31);
            this.txtPassword_MySQL.Name = "txtPassword_MySQL";
            this.txtPassword_MySQL.PasswordChar = '*';
            this.txtPassword_MySQL.Size = new System.Drawing.Size(109, 21);
            this.txtPassword_MySQL.TabIndex = 98;
            this.txtPassword_MySQL.Tag = null;
            this.txtPassword_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPassword_MySQL, "(default)");
            this.txtPassword_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtUserID_MySQL
            // 
            this.txtUserID_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtUserID_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserID_MySQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtUserID_MySQL.Location = new System.Drawing.Point(393, 3);
            this.txtUserID_MySQL.Name = "txtUserID_MySQL";
            this.txtUserID_MySQL.Size = new System.Drawing.Size(109, 21);
            this.txtUserID_MySQL.TabIndex = 95;
            this.txtUserID_MySQL.Tag = null;
            this.txtUserID_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtUserID_MySQL, "(default)");
            this.txtUserID_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtConnectionName_MySQL
            // 
            this.txtConnectionName_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConnectionName_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConnectionName_MySQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConnectionName_MySQL.Location = new System.Drawing.Point(129, 3);
            this.txtConnectionName_MySQL.Name = "txtConnectionName_MySQL";
            this.txtConnectionName_MySQL.Size = new System.Drawing.Size(138, 21);
            this.txtConnectionName_MySQL.TabIndex = 86;
            this.txtConnectionName_MySQL.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtConnectionName_MySQL, "(default)");
            this.txtConnectionName_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtServer_MySQL
            // 
            this.txtServer_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtServer_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer_MySQL.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtServer_MySQL.Location = new System.Drawing.Point(129, 31);
            this.txtServer_MySQL.Name = "txtServer_MySQL";
            this.txtServer_MySQL.Size = new System.Drawing.Size(138, 21);
            this.txtServer_MySQL.TabIndex = 88;
            this.txtServer_MySQL.Tag = null;
            this.txtServer_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtServer_MySQL, "(default)");
            this.txtServer_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtRemark_MySQL
            // 
            this.txtRemark_MySQL.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRemark_MySQL.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark_MySQL.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtRemark_MySQL.Location = new System.Drawing.Point(129, 145);
            this.txtRemark_MySQL.Multiline = true;
            this.txtRemark_MySQL.Name = "txtRemark_MySQL";
            this.txtRemark_MySQL.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemark_MySQL.Size = new System.Drawing.Size(373, 65);
            this.txtRemark_MySQL.TabIndex = 103;
            this.txtRemark_MySQL.Tag = null;
            this.txtRemark_MySQL.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRemark_MySQL, "(default)");
            this.txtRemark_MySQL.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblRemark_MySQL
            // 
            this.lblRemark_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemark_MySQL.Location = new System.Drawing.Point(39, 148);
            this.lblRemark_MySQL.Name = "lblRemark_MySQL";
            this.lblRemark_MySQL.Size = new System.Drawing.Size(88, 16);
            this.lblRemark_MySQL.TabIndex = 104;
            this.lblRemark_MySQL.Text = "Remark:";
            this.lblRemark_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword_MySQL
            // 
            this.lblPassword_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword_MySQL.Location = new System.Drawing.Point(292, 33);
            this.lblPassword_MySQL.Name = "lblPassword_MySQL";
            this.lblPassword_MySQL.Size = new System.Drawing.Size(100, 16);
            this.lblPassword_MySQL.TabIndex = 96;
            this.lblPassword_MySQL.Text = "Password:";
            this.lblPassword_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUserID_MySQL
            // 
            this.lblUserID_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUserID_MySQL.Location = new System.Drawing.Point(292, 5);
            this.lblUserID_MySQL.Name = "lblUserID_MySQL";
            this.lblUserID_MySQL.Size = new System.Drawing.Size(100, 16);
            this.lblUserID_MySQL.TabIndex = 94;
            this.lblUserID_MySQL.Text = "User ID:";
            this.lblUserID_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDatabase_MySQL
            // 
            this.lblDatabase_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDatabase_MySQL.Location = new System.Drawing.Point(39, 61);
            this.lblDatabase_MySQL.Name = "lblDatabase_MySQL";
            this.lblDatabase_MySQL.Size = new System.Drawing.Size(88, 16);
            this.lblDatabase_MySQL.TabIndex = 93;
            this.lblDatabase_MySQL.Text = "Database:";
            this.lblDatabase_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblServer_MySQL
            // 
            this.lblServer_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblServer_MySQL.Location = new System.Drawing.Point(3, 33);
            this.lblServer_MySQL.Name = "lblServer_MySQL";
            this.lblServer_MySQL.Size = new System.Drawing.Size(125, 16);
            this.lblServer_MySQL.TabIndex = 92;
            this.lblServer_MySQL.Text = "Server/Host:";
            this.lblServer_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionName_MySQL
            // 
            this.lblConnectionName_MySQL.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectionName_MySQL.Location = new System.Drawing.Point(3, 5);
            this.lblConnectionName_MySQL.Name = "lblConnectionName_MySQL";
            this.lblConnectionName_MySQL.Size = new System.Drawing.Size(125, 16);
            this.lblConnectionName_MySQL.TabIndex = 85;
            this.lblConnectionName_MySQL.Text = "Connection Name:";
            this.lblConnectionName_MySQL.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(768, 173);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(67, 21);
            this.btnExit.TabIndex = 114;
            this.btnExit.Text = "Exit";
            this.c1ThemeController1.SetTheme(this.btnExit, "(default)");
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnUpdateNow.Location = new System.Drawing.Point(540, 173);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(218, 21);
            this.btnUpdateNow.TabIndex = 113;
            this.btnUpdateNow.Text = "Update Now (One-Click update)";
            this.c1ThemeController1.SetTheme(this.btnUpdateNow, "(default)");
            this.btnUpdateNow.UseVisualStyleBackColor = true;
            this.btnUpdateNow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // pnlConnect
            // 
            this.pnlConnect.Controls.Add(this.btnConnect);
            this.pnlConnect.Location = new System.Drawing.Point(701, 61);
            this.pnlConnect.Name = "pnlConnect";
            this.pnlConnect.Size = new System.Drawing.Size(136, 107);
            this.pnlConnect.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.pnlConnect, "(default)");
            // 
            // btnConnect
            // 
            this.btnConnect.Font = new System.Drawing.Font("微軟正黑體", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnConnect.Location = new System.Drawing.Point(0, 0);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(134, 104);
            this.btnConnect.TabIndex = 0;
            this.btnConnect.Text = "Connect";
            this.c1ThemeController1.SetTheme(this.btnConnect, "(default)");
            this.btnConnect.UseVisualStyleBackColor = true;
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // grpTabColor
            // 
            this.grpTabColor.Controls.Add(this.tabExample);
            this.grpTabColor.Controls.Add(this.lblInactiveForeColor);
            this.grpTabColor.Controls.Add(this.pnlBackColor);
            this.grpTabColor.Controls.Add(this.lblBackColor);
            this.grpTabColor.Controls.Add(this.pnlActiveForeColor);
            this.grpTabColor.Controls.Add(this.pnlInactiveForeColor);
            this.grpTabColor.Controls.Add(this.lblActiveForeColor);
            this.grpTabColor.Location = new System.Drawing.Point(13, 284);
            this.grpTabColor.Name = "grpTabColor";
            this.grpTabColor.Size = new System.Drawing.Size(500, 93);
            this.grpTabColor.TabIndex = 44;
            this.grpTabColor.TabStop = false;
            this.grpTabColor.Text = "Tab Color";
            this.c1ThemeController1.SetTheme(this.grpTabColor, "(default)");
            // 
            // tabExample
            // 
            this.tabExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabExample.BoldSelectedPage = true;
            this.tabExample.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.tabExample.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabExample.IDEPixelArea = false;
            this.tabExample.Location = new System.Drawing.Point(192, 17);
            this.tabExample.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabExample.Name = "tabExample";
            this.tabExample.PositionTop = true;
            this.tabExample.SelectedIndex = 0;
            this.tabExample.SelectedTab = this.tabPage1;
            this.tabExample.ShrinkPagesToFit = false;
            this.tabExample.Size = new System.Drawing.Size(295, 34);
            this.tabExample.TabIndex = 39;
            this.tabExample.TabPages.AddRange(new Crownwood.Magic.Controls.TabPage[] {
            this.tabPage1,
            this.tabPage2});
            this.tabExample.TextInactiveColor = System.Drawing.Color.Silver;
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(0, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(295, 9);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Title = "SQL Editor 1";
            this.tabPage1.Visible = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(0, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Selected = false;
            this.tabPage2.Size = new System.Drawing.Size(295, 9);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Title = "* SQL Editor 2";
            this.tabPage2.Visible = false;
            // 
            // lblInactiveForeColor
            // 
            this.lblInactiveForeColor.AutoSize = true;
            this.lblInactiveForeColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblInactiveForeColor.Location = new System.Drawing.Point(235, 62);
            this.lblInactiveForeColor.Name = "lblInactiveForeColor";
            this.lblInactiveForeColor.Size = new System.Drawing.Size(115, 16);
            this.lblInactiveForeColor.TabIndex = 8;
            this.lblInactiveForeColor.Text = "Inactive Fore Color:";
            this.lblInactiveForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlBackColor
            // 
            this.pnlBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBackColor.Location = new System.Drawing.Point(89, 21);
            this.pnlBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlBackColor.Name = "pnlBackColor";
            this.pnlBackColor.Size = new System.Drawing.Size(74, 24);
            this.pnlBackColor.TabIndex = 43;
            this.pnlBackColor.Click += new System.EventHandler(this.SelectedTabClick);
            // 
            // lblBackColor
            // 
            this.lblBackColor.AutoSize = true;
            this.lblBackColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblBackColor.Location = new System.Drawing.Point(15, 24);
            this.lblBackColor.Name = "lblBackColor";
            this.lblBackColor.Size = new System.Drawing.Size(70, 16);
            this.lblBackColor.TabIndex = 42;
            this.lblBackColor.Text = "Back Color:";
            this.lblBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlActiveForeColor
            // 
            this.pnlActiveForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlActiveForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlActiveForeColor.Location = new System.Drawing.Point(126, 59);
            this.pnlActiveForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlActiveForeColor.Name = "pnlActiveForeColor";
            this.pnlActiveForeColor.Size = new System.Drawing.Size(74, 24);
            this.pnlActiveForeColor.TabIndex = 41;
            this.pnlActiveForeColor.Click += new System.EventHandler(this.SelectedTabClick);
            // 
            // pnlInactiveForeColor
            // 
            this.pnlInactiveForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlInactiveForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlInactiveForeColor.Location = new System.Drawing.Point(354, 59);
            this.pnlInactiveForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlInactiveForeColor.Name = "pnlInactiveForeColor";
            this.pnlInactiveForeColor.Size = new System.Drawing.Size(74, 24);
            this.pnlInactiveForeColor.TabIndex = 38;
            this.pnlInactiveForeColor.Click += new System.EventHandler(this.SelectedTabClick);
            // 
            // lblActiveForeColor
            // 
            this.lblActiveForeColor.AutoSize = true;
            this.lblActiveForeColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblActiveForeColor.Location = new System.Drawing.Point(16, 62);
            this.lblActiveForeColor.Name = "lblActiveForeColor";
            this.lblActiveForeColor.Size = new System.Drawing.Size(106, 16);
            this.lblActiveForeColor.TabIndex = 40;
            this.lblActiveForeColor.Text = "Active Fore Color:";
            this.lblActiveForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSQLite
            // 
            this.pnlSQLite.Controls.Add(this.btnHelp_DataType);
            this.pnlSQLite.Controls.Add(this.panel2);
            this.pnlSQLite.Controls.Add(this.panel1);
            this.pnlSQLite.Controls.Add(this.btnBrowseFile_SQLite);
            this.pnlSQLite.Controls.Add(this.cboDatabaseType_SQLite);
            this.pnlSQLite.Controls.Add(this.c1CheckBox1);
            this.pnlSQLite.Controls.Add(this.c1Button1);
            this.pnlSQLite.Controls.Add(this.btnHelp_ConnectionName_SQLite);
            this.pnlSQLite.Controls.Add(this.txtPassword_SQLite);
            this.pnlSQLite.Controls.Add(this.txtConnectionName_SQLite);
            this.pnlSQLite.Controls.Add(this.txtFile_SQLite);
            this.pnlSQLite.Controls.Add(this.txtRemark_SQLite);
            this.pnlSQLite.Controls.Add(this.lblRemark_SQLite);
            this.pnlSQLite.Controls.Add(this.lblPassword_SQLite);
            this.pnlSQLite.Controls.Add(this.lblDatabaseType_SQLite);
            this.pnlSQLite.Controls.Add(this.lblDatabaseFile_SQLite);
            this.pnlSQLite.Controls.Add(this.lblConnectionName_SQLite);
            this.pnlSQLite.Location = new System.Drawing.Point(6, 50);
            this.pnlSQLite.Name = "pnlSQLite";
            this.pnlSQLite.Size = new System.Drawing.Size(517, 223);
            this.pnlSQLite.TabIndex = 100;
            this.c1ThemeController1.SetTheme(this.pnlSQLite, "(default)");
            // 
            // btnHelp_DataType
            // 
            this.btnHelp_DataType.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_DataType.Image")));
            this.btnHelp_DataType.Location = new System.Drawing.Point(297, 121);
            this.btnHelp_DataType.Name = "btnHelp_DataType";
            this.btnHelp_DataType.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_DataType.TabIndex = 113;
            this.c1ThemeController1.SetTheme(this.btnHelp_DataType, "(default)");
            this.btnHelp_DataType.UseVisualStyleBackColor = true;
            this.btnHelp_DataType.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btnEncryptWithPassword);
            this.panel2.Controls.Add(this.btnRemovePassword);
            this.panel2.Controls.Add(this.rdoWithPassword);
            this.panel2.Controls.Add(this.rdoWithoutPassword);
            this.panel2.Location = new System.Drawing.Point(129, 87);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(580, 28);
            this.panel2.TabIndex = 112;
            this.c1ThemeController1.SetTheme(this.panel2, "(default)");
            // 
            // btnEncryptWithPassword
            // 
            this.btnEncryptWithPassword.Location = new System.Drawing.Point(359, 3);
            this.btnEncryptWithPassword.Name = "btnEncryptWithPassword";
            this.btnEncryptWithPassword.Size = new System.Drawing.Size(212, 22);
            this.btnEncryptWithPassword.TabIndex = 114;
            this.btnEncryptWithPassword.Text = "Encrypt and Save with Password";
            this.c1ThemeController1.SetTheme(this.btnEncryptWithPassword, "(default)");
            this.btnEncryptWithPassword.UseVisualStyleBackColor = true;
            this.btnEncryptWithPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEncryptWithPassword.Click += new System.EventHandler(this.btnRemoveOrEncrypt_Click);
            // 
            // btnRemovePassword
            // 
            this.btnRemovePassword.Location = new System.Drawing.Point(158, 3);
            this.btnRemovePassword.Name = "btnRemovePassword";
            this.btnRemovePassword.Size = new System.Drawing.Size(193, 22);
            this.btnRemovePassword.TabIndex = 113;
            this.btnRemovePassword.Text = "Remove Password and Save";
            this.c1ThemeController1.SetTheme(this.btnRemovePassword, "(default)");
            this.btnRemovePassword.UseVisualStyleBackColor = true;
            this.btnRemovePassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnRemovePassword.Click += new System.EventHandler(this.btnRemoveOrEncrypt_Click);
            // 
            // rdoWithPassword
            // 
            this.rdoWithPassword.AutoSize = true;
            this.rdoWithPassword.Checked = true;
            this.rdoWithPassword.ForeColor = System.Drawing.Color.Green;
            this.rdoWithPassword.Location = new System.Drawing.Point(4, 4);
            this.rdoWithPassword.Name = "rdoWithPassword";
            this.rdoWithPassword.Size = new System.Drawing.Size(104, 20);
            this.rdoWithPassword.TabIndex = 109;
            this.rdoWithPassword.TabStop = true;
            this.rdoWithPassword.Text = "with Password";
            this.rdoWithPassword.UseVisualStyleBackColor = true;
            // 
            // rdoWithoutPassword
            // 
            this.rdoWithoutPassword.AutoSize = true;
            this.rdoWithoutPassword.Location = new System.Drawing.Point(110, 4);
            this.rdoWithoutPassword.Name = "rdoWithoutPassword";
            this.rdoWithoutPassword.Size = new System.Drawing.Size(123, 20);
            this.rdoWithoutPassword.TabIndex = 110;
            this.rdoWithoutPassword.Text = "without Password";
            this.rdoWithoutPassword.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.rdoCreate_SQLite);
            this.panel1.Controls.Add(this.rdoOpen_SQLite);
            this.panel1.Location = new System.Drawing.Point(129, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(385, 28);
            this.panel1.TabIndex = 111;
            this.c1ThemeController1.SetTheme(this.panel1, "(default)");
            // 
            // rdoCreate_SQLite
            // 
            this.rdoCreate_SQLite.AutoSize = true;
            this.rdoCreate_SQLite.Location = new System.Drawing.Point(4, 4);
            this.rdoCreate_SQLite.Name = "rdoCreate_SQLite";
            this.rdoCreate_SQLite.Size = new System.Drawing.Size(164, 20);
            this.rdoCreate_SQLite.TabIndex = 109;
            this.rdoCreate_SQLite.Text = "Create new database file";
            this.c1ThemeController1.SetTheme(this.rdoCreate_SQLite, "(default)");
            this.rdoCreate_SQLite.UseVisualStyleBackColor = true;
            this.rdoCreate_SQLite.CheckedChanged += new System.EventHandler(this.CreateOrNewCheckedChanged);
            // 
            // rdoOpen_SQLite
            // 
            this.rdoOpen_SQLite.AutoSize = true;
            this.rdoOpen_SQLite.Checked = true;
            this.rdoOpen_SQLite.Location = new System.Drawing.Point(187, 4);
            this.rdoOpen_SQLite.Name = "rdoOpen_SQLite";
            this.rdoOpen_SQLite.Size = new System.Drawing.Size(179, 20);
            this.rdoOpen_SQLite.TabIndex = 110;
            this.rdoOpen_SQLite.TabStop = true;
            this.rdoOpen_SQLite.Text = "Open existing database file";
            this.c1ThemeController1.SetTheme(this.rdoOpen_SQLite, "(default)");
            this.rdoOpen_SQLite.UseVisualStyleBackColor = true;
            // 
            // btnBrowseFile_SQLite
            // 
            this.btnBrowseFile_SQLite.Location = new System.Drawing.Point(481, 62);
            this.btnBrowseFile_SQLite.Name = "btnBrowseFile_SQLite";
            this.btnBrowseFile_SQLite.Size = new System.Drawing.Size(21, 21);
            this.btnBrowseFile_SQLite.TabIndex = 108;
            this.btnBrowseFile_SQLite.Text = "...";
            this.c1ThemeController1.SetTheme(this.btnBrowseFile_SQLite, "(default)");
            this.btnBrowseFile_SQLite.UseVisualStyleBackColor = true;
            this.btnBrowseFile_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFile_SQLite.Click += new System.EventHandler(this.btnBrowseFile_Sqlite_Click);
            // 
            // cboDatabaseType_SQLite
            // 
            this.cboDatabaseType_SQLite.AllowSpinLoop = false;
            this.cboDatabaseType_SQLite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboDatabaseType_SQLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDatabaseType_SQLite.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboDatabaseType_SQLite.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboDatabaseType_SQLite.DropDownWidth = 0;
            this.cboDatabaseType_SQLite.GapHeight = 0;
            this.cboDatabaseType_SQLite.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDatabaseType_SQLite.ItemsDisplayMember = "";
            this.cboDatabaseType_SQLite.ItemsValueMember = "";
            this.cboDatabaseType_SQLite.Location = new System.Drawing.Point(129, 121);
            this.cboDatabaseType_SQLite.Name = "cboDatabaseType_SQLite";
            this.cboDatabaseType_SQLite.Size = new System.Drawing.Size(163, 21);
            this.cboDatabaseType_SQLite.TabIndex = 107;
            this.cboDatabaseType_SQLite.Tag = null;
            this.cboDatabaseType_SQLite.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDatabaseType_SQLite, "(default)");
            this.cboDatabaseType_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1CheckBox1
            // 
            this.c1CheckBox1.AutoSize = true;
            this.c1CheckBox1.BackColor = System.Drawing.Color.Transparent;
            this.c1CheckBox1.BorderColor = System.Drawing.Color.Transparent;
            this.c1CheckBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1CheckBox1.ForeColor = System.Drawing.Color.Black;
            this.c1CheckBox1.Location = new System.Drawing.Point(338, 5);
            this.c1CheckBox1.Name = "c1CheckBox1";
            this.c1CheckBox1.Padding = new System.Windows.Forms.Padding(1);
            this.c1CheckBox1.Size = new System.Drawing.Size(114, 22);
            this.c1CheckBox1.TabIndex = 106;
            this.c1CheckBox1.Text = "Enable Pooling";
            this.c1ThemeController1.SetTheme(this.c1CheckBox1, "(default)");
            this.c1CheckBox1.UseVisualStyleBackColor = true;
            this.c1CheckBox1.Value = null;
            this.c1CheckBox1.Visible = false;
            this.c1CheckBox1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1Button1
            // 
            this.c1Button1.Image = ((System.Drawing.Image)(resources.GetObject("c1Button1.Image")));
            this.c1Button1.Location = new System.Drawing.Point(482, 4);
            this.c1Button1.Name = "c1Button1";
            this.c1Button1.Size = new System.Drawing.Size(21, 21);
            this.c1Button1.TabIndex = 105;
            this.c1ThemeController1.SetTheme(this.c1Button1, "(default)");
            this.c1Button1.UseVisualStyleBackColor = true;
            this.c1Button1.Visible = false;
            this.c1Button1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_ConnectionName_SQLite
            // 
            this.btnHelp_ConnectionName_SQLite.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ConnectionName_SQLite.Image")));
            this.btnHelp_ConnectionName_SQLite.Location = new System.Drawing.Point(271, 3);
            this.btnHelp_ConnectionName_SQLite.Name = "btnHelp_ConnectionName_SQLite";
            this.btnHelp_ConnectionName_SQLite.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ConnectionName_SQLite.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.btnHelp_ConnectionName_SQLite, "(default)");
            this.btnHelp_ConnectionName_SQLite.UseVisualStyleBackColor = true;
            this.btnHelp_ConnectionName_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ConnectionName_SQLite.Click += new System.EventHandler(this.btnHelp_ConnectionName_Click);
            // 
            // txtPassword_SQLite
            // 
            this.txtPassword_SQLite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPassword_SQLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword_SQLite.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtPassword_SQLite.Location = new System.Drawing.Point(393, 121);
            this.txtPassword_SQLite.Name = "txtPassword_SQLite";
            this.txtPassword_SQLite.PasswordChar = '*';
            this.txtPassword_SQLite.Size = new System.Drawing.Size(109, 21);
            this.txtPassword_SQLite.TabIndex = 98;
            this.txtPassword_SQLite.Tag = null;
            this.txtPassword_SQLite.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPassword_SQLite, "(default)");
            this.txtPassword_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtConnectionName_SQLite
            // 
            this.txtConnectionName_SQLite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConnectionName_SQLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConnectionName_SQLite.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConnectionName_SQLite.Location = new System.Drawing.Point(129, 3);
            this.txtConnectionName_SQLite.Name = "txtConnectionName_SQLite";
            this.txtConnectionName_SQLite.Size = new System.Drawing.Size(138, 21);
            this.txtConnectionName_SQLite.TabIndex = 86;
            this.txtConnectionName_SQLite.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtConnectionName_SQLite, "(default)");
            this.txtConnectionName_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtFile_SQLite
            // 
            this.txtFile_SQLite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtFile_SQLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFile_SQLite.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtFile_SQLite.Location = new System.Drawing.Point(129, 62);
            this.txtFile_SQLite.Name = "txtFile_SQLite";
            this.txtFile_SQLite.ReadOnly = true;
            this.txtFile_SQLite.Size = new System.Drawing.Size(347, 21);
            this.txtFile_SQLite.TabIndex = 88;
            this.txtFile_SQLite.Tag = null;
            this.txtFile_SQLite.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtFile_SQLite, "(default)");
            this.txtFile_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtRemark_SQLite
            // 
            this.txtRemark_SQLite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRemark_SQLite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark_SQLite.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtRemark_SQLite.Location = new System.Drawing.Point(129, 149);
            this.txtRemark_SQLite.Multiline = true;
            this.txtRemark_SQLite.Name = "txtRemark_SQLite";
            this.txtRemark_SQLite.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemark_SQLite.Size = new System.Drawing.Size(373, 62);
            this.txtRemark_SQLite.TabIndex = 103;
            this.txtRemark_SQLite.Tag = null;
            this.txtRemark_SQLite.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRemark_SQLite, "(default)");
            this.txtRemark_SQLite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblRemark_SQLite
            // 
            this.lblRemark_SQLite.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemark_SQLite.Location = new System.Drawing.Point(39, 151);
            this.lblRemark_SQLite.Name = "lblRemark_SQLite";
            this.lblRemark_SQLite.Size = new System.Drawing.Size(88, 16);
            this.lblRemark_SQLite.TabIndex = 104;
            this.lblRemark_SQLite.Text = "Remark:";
            this.lblRemark_SQLite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword_SQLite
            // 
            this.lblPassword_SQLite.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword_SQLite.ForeColor = System.Drawing.Color.Green;
            this.lblPassword_SQLite.Location = new System.Drawing.Point(293, 123);
            this.lblPassword_SQLite.Name = "lblPassword_SQLite";
            this.lblPassword_SQLite.Size = new System.Drawing.Size(100, 16);
            this.lblPassword_SQLite.TabIndex = 96;
            this.lblPassword_SQLite.Text = "Password:";
            this.lblPassword_SQLite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDatabaseType_SQLite
            // 
            this.lblDatabaseType_SQLite.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDatabaseType_SQLite.Location = new System.Drawing.Point(3, 122);
            this.lblDatabaseType_SQLite.Name = "lblDatabaseType_SQLite";
            this.lblDatabaseType_SQLite.Size = new System.Drawing.Size(125, 16);
            this.lblDatabaseType_SQLite.TabIndex = 93;
            this.lblDatabaseType_SQLite.Text = "Database Type:";
            this.lblDatabaseType_SQLite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDatabaseFile_SQLite
            // 
            this.lblDatabaseFile_SQLite.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDatabaseFile_SQLite.Location = new System.Drawing.Point(3, 64);
            this.lblDatabaseFile_SQLite.Name = "lblDatabaseFile_SQLite";
            this.lblDatabaseFile_SQLite.Size = new System.Drawing.Size(125, 16);
            this.lblDatabaseFile_SQLite.TabIndex = 92;
            this.lblDatabaseFile_SQLite.Text = "Database File:";
            this.lblDatabaseFile_SQLite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionName_SQLite
            // 
            this.lblConnectionName_SQLite.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectionName_SQLite.Location = new System.Drawing.Point(3, 5);
            this.lblConnectionName_SQLite.Name = "lblConnectionName_SQLite";
            this.lblConnectionName_SQLite.Size = new System.Drawing.Size(125, 16);
            this.lblConnectionName_SQLite.TabIndex = 85;
            this.lblConnectionName_SQLite.Text = "Connection Name:";
            this.lblConnectionName_SQLite.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSQLServer
            // 
            this.pnlSQLServer.Controls.Add(this.c1GridSQLServer);
            this.pnlSQLServer.Controls.Add(this.lblQueryTimeout_SQLServer);
            this.pnlSQLServer.Controls.Add(this.btnHelp_QueryTimeout_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblSeconds_SQLServer);
            this.pnlSQLServer.Controls.Add(this.nudQueryTimeout_SQLServer);
            this.pnlSQLServer.Controls.Add(this.btnHelp_ExcludeNativeDatabase_SQLServer);
            this.pnlSQLServer.Controls.Add(this.chkExcludeNativeObject_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtPort_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblPort_SQLServer);
            this.pnlSQLServer.Controls.Add(this.cboDatabase_SQLServer);
            this.pnlSQLServer.Controls.Add(this.btnHelp_Pooling_SQLServer);
            this.pnlSQLServer.Controls.Add(this.chkPooling_SQLServer);
            this.pnlSQLServer.Controls.Add(this.btnHelp_ConnectionName_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtPassword_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtUserID_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtConnectionName_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtServer_SQLServer);
            this.pnlSQLServer.Controls.Add(this.txtRemark_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblRemark_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblPassword_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblUserID_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblDatabase_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblServer_SQLServer);
            this.pnlSQLServer.Controls.Add(this.lblConnectionName_SQLServer);
            this.pnlSQLServer.Location = new System.Drawing.Point(1, 61);
            this.pnlSQLServer.Name = "pnlSQLServer";
            this.pnlSQLServer.Size = new System.Drawing.Size(517, 223);
            this.pnlSQLServer.TabIndex = 100;
            this.c1ThemeController1.SetTheme(this.pnlSQLServer, "(default)");
            // 
            // c1GridSQLServer
            // 
            this.c1GridSQLServer.AllowFilter = false;
            this.c1GridSQLServer.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridSQLServer.AllowUpdate = false;
            this.c1GridSQLServer.AlternatingRows = true;
            this.c1GridSQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridSQLServer.CaptionHeight = 19;
            this.c1GridSQLServer.ColumnHeaders = false;
            this.c1GridSQLServer.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridSQLServer.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridSQLServer.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridSQLServer.Images"))));
            this.c1GridSQLServer.Location = new System.Drawing.Point(129, 82);
            this.c1GridSQLServer.Name = "c1GridSQLServer";
            this.c1GridSQLServer.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridSQLServer.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridSQLServer.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridSQLServer.PreviewInfo.ZoomFactor = 75D;
            this.c1GridSQLServer.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridSQLServer.PrintInfo.MeasurementPrinterName = null;
            this.c1GridSQLServer.RowHeight = 19;
            this.c1GridSQLServer.Size = new System.Drawing.Size(270, 183);
            this.c1GridSQLServer.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridSQLServer.TabIndex = 100;
            this.c1ThemeController1.SetTheme(this.c1GridSQLServer, "(default)");
            this.c1GridSQLServer.UseCompatibleTextRendering = false;
            this.c1GridSQLServer.Visible = false;
            this.c1GridSQLServer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridSQLServer_KeyPress);
            this.c1GridSQLServer.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridSQLServer_MouseDoubleClick);
            this.c1GridSQLServer.PropBag = resources.GetString("c1GridSQLServer.PropBag");
            // 
            // lblQueryTimeout_SQLServer
            // 
            this.lblQueryTimeout_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblQueryTimeout_SQLServer.Location = new System.Drawing.Point(3, 117);
            this.lblQueryTimeout_SQLServer.Name = "lblQueryTimeout_SQLServer";
            this.lblQueryTimeout_SQLServer.Size = new System.Drawing.Size(125, 16);
            this.lblQueryTimeout_SQLServer.TabIndex = 122;
            this.lblQueryTimeout_SQLServer.Text = "Query Timeout:";
            this.lblQueryTimeout_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblQueryTimeout_SQLServer, "(default)");
            // 
            // btnHelp_QueryTimeout_SQLServer
            // 
            this.btnHelp_QueryTimeout_SQLServer.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_QueryTimeout_SQLServer.Image")));
            this.btnHelp_QueryTimeout_SQLServer.Location = new System.Drawing.Point(232, 116);
            this.btnHelp_QueryTimeout_SQLServer.Name = "btnHelp_QueryTimeout_SQLServer";
            this.btnHelp_QueryTimeout_SQLServer.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_QueryTimeout_SQLServer.TabIndex = 125;
            this.c1ThemeController1.SetTheme(this.btnHelp_QueryTimeout_SQLServer, "(default)");
            this.btnHelp_QueryTimeout_SQLServer.UseVisualStyleBackColor = true;
            this.btnHelp_QueryTimeout_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_QueryTimeout_SQLServer.Click += new System.EventHandler(this.btnHelp_QueryTimeout);
            // 
            // lblSeconds_SQLServer
            // 
            this.lblSeconds_SQLServer.AutoSize = true;
            this.lblSeconds_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSeconds_SQLServer.Location = new System.Drawing.Point(179, 117);
            this.lblSeconds_SQLServer.Name = "lblSeconds_SQLServer";
            this.lblSeconds_SQLServer.Size = new System.Drawing.Size(53, 16);
            this.lblSeconds_SQLServer.TabIndex = 124;
            this.lblSeconds_SQLServer.Text = "seconds";
            this.lblSeconds_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblSeconds_SQLServer, "(default)");
            // 
            // nudQueryTimeout_SQLServer
            // 
            this.nudQueryTimeout_SQLServer.Location = new System.Drawing.Point(129, 115);
            this.nudQueryTimeout_SQLServer.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout_SQLServer.Name = "nudQueryTimeout_SQLServer";
            this.nudQueryTimeout_SQLServer.Size = new System.Drawing.Size(48, 23);
            this.nudQueryTimeout_SQLServer.TabIndex = 123;
            this.nudQueryTimeout_SQLServer.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudQueryTimeout_SQLServer, "(default)");
            this.nudQueryTimeout_SQLServer.Value = new decimal(new int[] {
            30,
            0,
            0,
            0});
            this.nudQueryTimeout_SQLServer.Enter += new System.EventHandler(this.nudQueryTimeout_Enter);
            this.nudQueryTimeout_SQLServer.Leave += new System.EventHandler(this.nudQueryTimeout_Leave);
            this.nudQueryTimeout_SQLServer.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudQueryTimeout_MouseClick);
            // 
            // btnHelp_ExcludeNativeDatabase_SQLServer
            // 
            this.btnHelp_ExcludeNativeDatabase_SQLServer.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ExcludeNativeDatabase_SQLServer.Image")));
            this.btnHelp_ExcludeNativeDatabase_SQLServer.Location = new System.Drawing.Point(481, 86);
            this.btnHelp_ExcludeNativeDatabase_SQLServer.Name = "btnHelp_ExcludeNativeDatabase_SQLServer";
            this.btnHelp_ExcludeNativeDatabase_SQLServer.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ExcludeNativeDatabase_SQLServer.TabIndex = 111;
            this.c1ThemeController1.SetTheme(this.btnHelp_ExcludeNativeDatabase_SQLServer, "(default)");
            this.btnHelp_ExcludeNativeDatabase_SQLServer.UseVisualStyleBackColor = true;
            this.btnHelp_ExcludeNativeDatabase_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ExcludeNativeDatabase_SQLServer.Click += new System.EventHandler(this.btnHelp_ExcludeNativeDatabase_SQLServer_Click);
            // 
            // chkExcludeNativeObject_SQLServer
            // 
            this.chkExcludeNativeObject_SQLServer.AutoSize = true;
            this.chkExcludeNativeObject_SQLServer.BackColor = System.Drawing.Color.Transparent;
            this.chkExcludeNativeObject_SQLServer.BorderColor = System.Drawing.Color.Transparent;
            this.chkExcludeNativeObject_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkExcludeNativeObject_SQLServer.ForeColor = System.Drawing.Color.Black;
            this.chkExcludeNativeObject_SQLServer.Location = new System.Drawing.Point(315, 87);
            this.chkExcludeNativeObject_SQLServer.Name = "chkExcludeNativeObject_SQLServer";
            this.chkExcludeNativeObject_SQLServer.Padding = new System.Windows.Forms.Padding(1);
            this.chkExcludeNativeObject_SQLServer.Size = new System.Drawing.Size(153, 22);
            this.chkExcludeNativeObject_SQLServer.TabIndex = 110;
            this.chkExcludeNativeObject_SQLServer.Text = "Exclude Native Object";
            this.c1ThemeController1.SetTheme(this.chkExcludeNativeObject_SQLServer, "(default)");
            this.chkExcludeNativeObject_SQLServer.UseVisualStyleBackColor = true;
            this.chkExcludeNativeObject_SQLServer.Value = null;
            this.chkExcludeNativeObject_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtPort_SQLServer
            // 
            this.txtPort_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPort_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPort_SQLServer.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtPort_SQLServer.Location = new System.Drawing.Point(129, 87);
            this.txtPort_SQLServer.Name = "txtPort_SQLServer";
            this.txtPort_SQLServer.Size = new System.Drawing.Size(57, 21);
            this.txtPort_SQLServer.TabIndex = 109;
            this.txtPort_SQLServer.Tag = null;
            this.txtPort_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPort_SQLServer, "(default)");
            this.txtPort_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtPort_SQLServer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TextBox_Numeric_KeyPress);
            this.txtPort_SQLServer.Leave += new System.EventHandler(this.txtPort_SQLServer_Leave);
            // 
            // lblPort_SQLServer
            // 
            this.lblPort_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPort_SQLServer.Location = new System.Drawing.Point(39, 89);
            this.lblPort_SQLServer.Name = "lblPort_SQLServer";
            this.lblPort_SQLServer.Size = new System.Drawing.Size(88, 16);
            this.lblPort_SQLServer.TabIndex = 108;
            this.lblPort_SQLServer.Text = "Port:";
            this.lblPort_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblPort_SQLServer, "(default)");
            // 
            // cboDatabase_SQLServer
            // 
            this.cboDatabase_SQLServer.AllowSpinLoop = false;
            this.cboDatabase_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboDatabase_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDatabase_SQLServer.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboDatabase_SQLServer.GapHeight = 0;
            this.cboDatabase_SQLServer.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDatabase_SQLServer.ItemsDisplayMember = "";
            this.cboDatabase_SQLServer.ItemsValueMember = "";
            this.cboDatabase_SQLServer.Location = new System.Drawing.Point(129, 59);
            this.cboDatabase_SQLServer.Name = "cboDatabase_SQLServer";
            this.cboDatabase_SQLServer.Size = new System.Drawing.Size(163, 21);
            this.cboDatabase_SQLServer.TabIndex = 107;
            this.cboDatabase_SQLServer.Tag = null;
            this.cboDatabase_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDatabase_SQLServer, "(default)");
            this.cboDatabase_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDatabase_SQLServer.SelectedIndexChanged += new System.EventHandler(this.cboDatabase_SQLServer_SelectedIndexChanged);
            this.cboDatabase_SQLServer.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboDatabase_SQLServer_BeforeDropDownOpen);
            this.cboDatabase_SQLServer.DropDownOpened += new System.EventHandler(this.cboDatabase_SQLServer_DropDownOpened);
            this.cboDatabase_SQLServer.DropDownClosed += new C1.Win.C1Input.DropDownClosedEventHandler(this.cboDatabase_SQLServer_DropDownClosed);
            this.cboDatabase_SQLServer.TextChanged += new System.EventHandler(this.cboDatabase_SQLServer_TextChanged);
            this.cboDatabase_SQLServer.Enter += new System.EventHandler(this.cboDatabase_SQLServer_Enter);
            this.cboDatabase_SQLServer.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_SQLServer_KeyDown);
            this.cboDatabase_SQLServer.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboDatabase_SQLServer_KeyPress);
            this.cboDatabase_SQLServer.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboDatabase_SQLServer_KeyUp);
            this.cboDatabase_SQLServer.Leave += new System.EventHandler(this.cboDatabase_SQLServer_Leave);
            // 
            // btnHelp_Pooling_SQLServer
            // 
            this.btnHelp_Pooling_SQLServer.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Pooling_SQLServer.Image")));
            this.btnHelp_Pooling_SQLServer.Location = new System.Drawing.Point(481, 58);
            this.btnHelp_Pooling_SQLServer.Name = "btnHelp_Pooling_SQLServer";
            this.btnHelp_Pooling_SQLServer.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Pooling_SQLServer.TabIndex = 106;
            this.c1ThemeController1.SetTheme(this.btnHelp_Pooling_SQLServer, "(default)");
            this.btnHelp_Pooling_SQLServer.UseVisualStyleBackColor = true;
            this.btnHelp_Pooling_SQLServer.Visible = false;
            this.btnHelp_Pooling_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Pooling_SQLServer.Click += new System.EventHandler(this.btnHelp_Pooling_Click);
            // 
            // chkPooling_SQLServer
            // 
            this.chkPooling_SQLServer.AutoSize = true;
            this.chkPooling_SQLServer.BackColor = System.Drawing.Color.Transparent;
            this.chkPooling_SQLServer.BorderColor = System.Drawing.Color.Transparent;
            this.chkPooling_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPooling_SQLServer.ForeColor = System.Drawing.Color.Black;
            this.chkPooling_SQLServer.Location = new System.Drawing.Point(327, 59);
            this.chkPooling_SQLServer.Name = "chkPooling_SQLServer";
            this.chkPooling_SQLServer.Padding = new System.Windows.Forms.Padding(1);
            this.chkPooling_SQLServer.Size = new System.Drawing.Size(114, 22);
            this.chkPooling_SQLServer.TabIndex = 105;
            this.chkPooling_SQLServer.Text = "Enable Pooling";
            this.c1ThemeController1.SetTheme(this.chkPooling_SQLServer, "(default)");
            this.chkPooling_SQLServer.UseVisualStyleBackColor = true;
            this.chkPooling_SQLServer.Value = null;
            this.chkPooling_SQLServer.Visible = false;
            this.chkPooling_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_ConnectionName_SQLServer
            // 
            this.btnHelp_ConnectionName_SQLServer.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ConnectionName_SQLServer.Image")));
            this.btnHelp_ConnectionName_SQLServer.Location = new System.Drawing.Point(271, 3);
            this.btnHelp_ConnectionName_SQLServer.Name = "btnHelp_ConnectionName_SQLServer";
            this.btnHelp_ConnectionName_SQLServer.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ConnectionName_SQLServer.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.btnHelp_ConnectionName_SQLServer, "(default)");
            this.btnHelp_ConnectionName_SQLServer.UseVisualStyleBackColor = true;
            this.btnHelp_ConnectionName_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ConnectionName_SQLServer.Click += new System.EventHandler(this.btnHelp_ConnectionName_Click);
            // 
            // txtPassword_SQLServer
            // 
            this.txtPassword_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtPassword_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtPassword_SQLServer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtPassword_SQLServer.Location = new System.Drawing.Point(393, 31);
            this.txtPassword_SQLServer.Name = "txtPassword_SQLServer";
            this.txtPassword_SQLServer.PasswordChar = '*';
            this.txtPassword_SQLServer.Size = new System.Drawing.Size(109, 21);
            this.txtPassword_SQLServer.TabIndex = 98;
            this.txtPassword_SQLServer.Tag = null;
            this.txtPassword_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtPassword_SQLServer, "(default)");
            this.txtPassword_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtUserID_SQLServer
            // 
            this.txtUserID_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtUserID_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtUserID_SQLServer.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtUserID_SQLServer.Location = new System.Drawing.Point(393, 3);
            this.txtUserID_SQLServer.Name = "txtUserID_SQLServer";
            this.txtUserID_SQLServer.Size = new System.Drawing.Size(109, 21);
            this.txtUserID_SQLServer.TabIndex = 95;
            this.txtUserID_SQLServer.Tag = null;
            this.txtUserID_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtUserID_SQLServer, "(default)");
            this.txtUserID_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtConnectionName_SQLServer
            // 
            this.txtConnectionName_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConnectionName_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConnectionName_SQLServer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConnectionName_SQLServer.Location = new System.Drawing.Point(129, 3);
            this.txtConnectionName_SQLServer.Name = "txtConnectionName_SQLServer";
            this.txtConnectionName_SQLServer.Size = new System.Drawing.Size(138, 21);
            this.txtConnectionName_SQLServer.TabIndex = 86;
            this.txtConnectionName_SQLServer.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtConnectionName_SQLServer, "(default)");
            this.txtConnectionName_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtServer_SQLServer
            // 
            this.txtServer_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtServer_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServer_SQLServer.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtServer_SQLServer.Location = new System.Drawing.Point(129, 31);
            this.txtServer_SQLServer.Name = "txtServer_SQLServer";
            this.txtServer_SQLServer.Size = new System.Drawing.Size(138, 21);
            this.txtServer_SQLServer.TabIndex = 88;
            this.txtServer_SQLServer.Tag = null;
            this.txtServer_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtServer_SQLServer, "(default)");
            this.txtServer_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtRemark_SQLServer
            // 
            this.txtRemark_SQLServer.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRemark_SQLServer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRemark_SQLServer.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtRemark_SQLServer.Location = new System.Drawing.Point(129, 145);
            this.txtRemark_SQLServer.Multiline = true;
            this.txtRemark_SQLServer.Name = "txtRemark_SQLServer";
            this.txtRemark_SQLServer.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtRemark_SQLServer.Size = new System.Drawing.Size(373, 65);
            this.txtRemark_SQLServer.TabIndex = 103;
            this.txtRemark_SQLServer.Tag = null;
            this.txtRemark_SQLServer.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRemark_SQLServer, "(default)");
            this.txtRemark_SQLServer.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblRemark_SQLServer
            // 
            this.lblRemark_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemark_SQLServer.Location = new System.Drawing.Point(39, 148);
            this.lblRemark_SQLServer.Name = "lblRemark_SQLServer";
            this.lblRemark_SQLServer.Size = new System.Drawing.Size(88, 16);
            this.lblRemark_SQLServer.TabIndex = 104;
            this.lblRemark_SQLServer.Text = "Remark:";
            this.lblRemark_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPassword_SQLServer
            // 
            this.lblPassword_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword_SQLServer.Location = new System.Drawing.Point(292, 33);
            this.lblPassword_SQLServer.Name = "lblPassword_SQLServer";
            this.lblPassword_SQLServer.Size = new System.Drawing.Size(100, 16);
            this.lblPassword_SQLServer.TabIndex = 96;
            this.lblPassword_SQLServer.Text = "Password:";
            this.lblPassword_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblUserID_SQLServer
            // 
            this.lblUserID_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUserID_SQLServer.Location = new System.Drawing.Point(292, 5);
            this.lblUserID_SQLServer.Name = "lblUserID_SQLServer";
            this.lblUserID_SQLServer.Size = new System.Drawing.Size(100, 16);
            this.lblUserID_SQLServer.TabIndex = 94;
            this.lblUserID_SQLServer.Text = "User ID:";
            this.lblUserID_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblDatabase_SQLServer
            // 
            this.lblDatabase_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDatabase_SQLServer.Location = new System.Drawing.Point(39, 61);
            this.lblDatabase_SQLServer.Name = "lblDatabase_SQLServer";
            this.lblDatabase_SQLServer.Size = new System.Drawing.Size(88, 16);
            this.lblDatabase_SQLServer.TabIndex = 93;
            this.lblDatabase_SQLServer.Text = "Database:";
            this.lblDatabase_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblServer_SQLServer
            // 
            this.lblServer_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblServer_SQLServer.Location = new System.Drawing.Point(3, 33);
            this.lblServer_SQLServer.Name = "lblServer_SQLServer";
            this.lblServer_SQLServer.Size = new System.Drawing.Size(125, 16);
            this.lblServer_SQLServer.TabIndex = 92;
            this.lblServer_SQLServer.Text = "Server/Host:";
            this.lblServer_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConnectionName_SQLServer
            // 
            this.lblConnectionName_SQLServer.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConnectionName_SQLServer.Location = new System.Drawing.Point(3, 5);
            this.lblConnectionName_SQLServer.Name = "lblConnectionName_SQLServer";
            this.lblConnectionName_SQLServer.Size = new System.Drawing.Size(125, 16);
            this.lblConnectionName_SQLServer.TabIndex = 85;
            this.lblConnectionName_SQLServer.Text = "Connection Name:";
            this.lblConnectionName_SQLServer.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnHelp_MainFormIconStyle
            // 
            this.btnHelp_MainFormIconStyle.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_MainFormIconStyle.Image")));
            this.btnHelp_MainFormIconStyle.Location = new System.Drawing.Point(758, 288);
            this.btnHelp_MainFormIconStyle.Name = "btnHelp_MainFormIconStyle";
            this.btnHelp_MainFormIconStyle.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_MainFormIconStyle.TabIndex = 107;
            this.c1ThemeController1.SetTheme(this.btnHelp_MainFormIconStyle, "(default)");
            this.btnHelp_MainFormIconStyle.UseVisualStyleBackColor = true;
            this.btnHelp_MainFormIconStyle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_MainFormIconStyle.Click += new System.EventHandler(this.btnHelp_MainFormIconStyle_Click);
            // 
            // grpMainFormIconStyle
            // 
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle9);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle9);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle8);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle8);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle7);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle7);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle6);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle6);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle5);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle5);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle4);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle4);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle3);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle3);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle2);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle2);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle1);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle1);
            this.grpMainFormIconStyle.Controls.Add(this.picIconStyle0);
            this.grpMainFormIconStyle.Controls.Add(this.rdoIconStyle0);
            this.grpMainFormIconStyle.Location = new System.Drawing.Point(540, 290);
            this.grpMainFormIconStyle.Name = "grpMainFormIconStyle";
            this.grpMainFormIconStyle.Size = new System.Drawing.Size(295, 87);
            this.grpMainFormIconStyle.TabIndex = 45;
            this.grpMainFormIconStyle.TabStop = false;
            this.grpMainFormIconStyle.Tag = "";
            this.grpMainFormIconStyle.Text = "Main Form Icon Style";
            this.c1ThemeController1.SetTheme(this.grpMainFormIconStyle, "(default)");
            // 
            // picIconStyle9
            // 
            this.picIconStyle9.Location = new System.Drawing.Point(254, 57);
            this.picIconStyle9.Name = "picIconStyle9";
            this.picIconStyle9.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle9.TabIndex = 29;
            this.picIconStyle9.TabStop = false;
            this.picIconStyle9.Tag = "";
            this.picIconStyle9.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle9
            // 
            this.rdoIconStyle9.AutoSize = true;
            this.rdoIconStyle9.Location = new System.Drawing.Point(236, 58);
            this.rdoIconStyle9.Name = "rdoIconStyle9";
            this.rdoIconStyle9.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle9.TabIndex = 109;
            this.rdoIconStyle9.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle9, "(default)");
            this.rdoIconStyle9.UseVisualStyleBackColor = true;
            this.rdoIconStyle9.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle8
            // 
            this.picIconStyle8.Location = new System.Drawing.Point(201, 57);
            this.picIconStyle8.Name = "picIconStyle8";
            this.picIconStyle8.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle8.TabIndex = 27;
            this.picIconStyle8.TabStop = false;
            this.picIconStyle8.Tag = "";
            this.picIconStyle8.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle8
            // 
            this.rdoIconStyle8.AutoSize = true;
            this.rdoIconStyle8.Location = new System.Drawing.Point(183, 58);
            this.rdoIconStyle8.Name = "rdoIconStyle8";
            this.rdoIconStyle8.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle8.TabIndex = 108;
            this.rdoIconStyle8.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle8, "(default)");
            this.rdoIconStyle8.UseVisualStyleBackColor = true;
            this.rdoIconStyle8.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle7
            // 
            this.picIconStyle7.Location = new System.Drawing.Point(148, 57);
            this.picIconStyle7.Name = "picIconStyle7";
            this.picIconStyle7.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle7.TabIndex = 25;
            this.picIconStyle7.TabStop = false;
            this.picIconStyle7.Tag = "";
            this.picIconStyle7.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle7
            // 
            this.rdoIconStyle7.AutoSize = true;
            this.rdoIconStyle7.Location = new System.Drawing.Point(130, 58);
            this.rdoIconStyle7.Name = "rdoIconStyle7";
            this.rdoIconStyle7.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle7.TabIndex = 107;
            this.rdoIconStyle7.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle7, "(default)");
            this.rdoIconStyle7.UseVisualStyleBackColor = true;
            this.rdoIconStyle7.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle6
            // 
            this.picIconStyle6.Location = new System.Drawing.Point(95, 57);
            this.picIconStyle6.Name = "picIconStyle6";
            this.picIconStyle6.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle6.TabIndex = 23;
            this.picIconStyle6.TabStop = false;
            this.picIconStyle6.Tag = "";
            this.picIconStyle6.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle6
            // 
            this.rdoIconStyle6.AutoSize = true;
            this.rdoIconStyle6.Location = new System.Drawing.Point(77, 58);
            this.rdoIconStyle6.Name = "rdoIconStyle6";
            this.rdoIconStyle6.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle6.TabIndex = 106;
            this.rdoIconStyle6.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle6, "(default)");
            this.rdoIconStyle6.UseVisualStyleBackColor = true;
            this.rdoIconStyle6.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle5
            // 
            this.picIconStyle5.Location = new System.Drawing.Point(42, 57);
            this.picIconStyle5.Name = "picIconStyle5";
            this.picIconStyle5.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle5.TabIndex = 21;
            this.picIconStyle5.TabStop = false;
            this.picIconStyle5.Tag = "";
            this.picIconStyle5.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle5
            // 
            this.rdoIconStyle5.AutoSize = true;
            this.rdoIconStyle5.Location = new System.Drawing.Point(24, 58);
            this.rdoIconStyle5.Name = "rdoIconStyle5";
            this.rdoIconStyle5.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle5.TabIndex = 105;
            this.rdoIconStyle5.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle5, "(default)");
            this.rdoIconStyle5.UseVisualStyleBackColor = true;
            this.rdoIconStyle5.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle4
            // 
            this.picIconStyle4.Location = new System.Drawing.Point(254, 28);
            this.picIconStyle4.Name = "picIconStyle4";
            this.picIconStyle4.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle4.TabIndex = 19;
            this.picIconStyle4.TabStop = false;
            this.picIconStyle4.Tag = "";
            this.picIconStyle4.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle4
            // 
            this.rdoIconStyle4.AutoSize = true;
            this.rdoIconStyle4.Location = new System.Drawing.Point(236, 29);
            this.rdoIconStyle4.Name = "rdoIconStyle4";
            this.rdoIconStyle4.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle4.TabIndex = 104;
            this.rdoIconStyle4.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle4, "(default)");
            this.rdoIconStyle4.UseVisualStyleBackColor = true;
            this.rdoIconStyle4.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle3
            // 
            this.picIconStyle3.Location = new System.Drawing.Point(201, 28);
            this.picIconStyle3.Name = "picIconStyle3";
            this.picIconStyle3.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle3.TabIndex = 17;
            this.picIconStyle3.TabStop = false;
            this.picIconStyle3.Tag = "";
            this.picIconStyle3.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle3
            // 
            this.rdoIconStyle3.AutoSize = true;
            this.rdoIconStyle3.Location = new System.Drawing.Point(183, 29);
            this.rdoIconStyle3.Name = "rdoIconStyle3";
            this.rdoIconStyle3.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle3.TabIndex = 103;
            this.rdoIconStyle3.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle3, "(default)");
            this.rdoIconStyle3.UseVisualStyleBackColor = true;
            this.rdoIconStyle3.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle2
            // 
            this.picIconStyle2.Location = new System.Drawing.Point(148, 29);
            this.picIconStyle2.Name = "picIconStyle2";
            this.picIconStyle2.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle2.TabIndex = 15;
            this.picIconStyle2.TabStop = false;
            this.picIconStyle2.Tag = "";
            this.picIconStyle2.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle2
            // 
            this.rdoIconStyle2.AutoSize = true;
            this.rdoIconStyle2.Location = new System.Drawing.Point(130, 30);
            this.rdoIconStyle2.Name = "rdoIconStyle2";
            this.rdoIconStyle2.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle2.TabIndex = 102;
            this.rdoIconStyle2.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle2, "(default)");
            this.rdoIconStyle2.UseVisualStyleBackColor = true;
            this.rdoIconStyle2.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle1
            // 
            this.picIconStyle1.Location = new System.Drawing.Point(95, 29);
            this.picIconStyle1.Name = "picIconStyle1";
            this.picIconStyle1.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle1.TabIndex = 13;
            this.picIconStyle1.TabStop = false;
            this.picIconStyle1.Tag = "";
            this.picIconStyle1.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle1
            // 
            this.rdoIconStyle1.AutoSize = true;
            this.rdoIconStyle1.Location = new System.Drawing.Point(77, 30);
            this.rdoIconStyle1.Name = "rdoIconStyle1";
            this.rdoIconStyle1.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle1.TabIndex = 101;
            this.rdoIconStyle1.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle1, "(default)");
            this.rdoIconStyle1.UseVisualStyleBackColor = true;
            this.rdoIconStyle1.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // picIconStyle0
            // 
            this.picIconStyle0.Location = new System.Drawing.Point(42, 29);
            this.picIconStyle0.Name = "picIconStyle0";
            this.picIconStyle0.Size = new System.Drawing.Size(16, 16);
            this.picIconStyle0.TabIndex = 11;
            this.picIconStyle0.TabStop = false;
            this.picIconStyle0.Tag = "";
            this.picIconStyle0.Click += new System.EventHandler(this.IconStylePicture_Click);
            // 
            // rdoIconStyle0
            // 
            this.rdoIconStyle0.AutoSize = true;
            this.rdoIconStyle0.Location = new System.Drawing.Point(24, 30);
            this.rdoIconStyle0.Name = "rdoIconStyle0";
            this.rdoIconStyle0.Size = new System.Drawing.Size(14, 13);
            this.rdoIconStyle0.TabIndex = 100;
            this.rdoIconStyle0.Tag = "";
            this.c1ThemeController1.SetTheme(this.rdoIconStyle0, "(default)");
            this.rdoIconStyle0.UseVisualStyleBackColor = true;
            this.rdoIconStyle0.CheckedChanged += new System.EventHandler(this.IconStyle_CheckedChanged);
            // 
            // btnHelp_DarkMode
            // 
            this.btnHelp_DarkMode.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_DarkMode.Image")));
            this.btnHelp_DarkMode.Location = new System.Drawing.Point(743, 233);
            this.btnHelp_DarkMode.Name = "btnHelp_DarkMode";
            this.btnHelp_DarkMode.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_DarkMode.TabIndex = 82;
            this.c1ThemeController1.SetTheme(this.btnHelp_DarkMode, "(default)");
            this.btnHelp_DarkMode.UseVisualStyleBackColor = true;
            this.btnHelp_DarkMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_DarkMode.Click += new System.EventHandler(this.btnHelp_DarkMode_Click);
            // 
            // btnEncryptWithCustomPW
            // 
            this.btnEncryptWithCustomPW.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnEncryptWithCustomPW.Location = new System.Drawing.Point(540, 201);
            this.btnEncryptWithCustomPW.Name = "btnEncryptWithCustomPW";
            this.btnEncryptWithCustomPW.Size = new System.Drawing.Size(295, 21);
            this.btnEncryptWithCustomPW.TabIndex = 111;
            this.btnEncryptWithCustomPW.Text = "Set Password Protection for JasonQuery.db";
            this.c1ThemeController1.SetTheme(this.btnEncryptWithCustomPW, "(default)");
            this.btnEncryptWithCustomPW.UseVisualStyleBackColor = true;
            this.btnEncryptWithCustomPW.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEncryptWithCustomPW.Click += new System.EventHandler(this.chkEncryptWithCustomPW_Click);
            // 
            // chkDarkMode
            // 
            this.chkDarkMode.AutoSize = true;
            this.chkDarkMode.BackColor = System.Drawing.Color.Transparent;
            this.chkDarkMode.BorderColor = System.Drawing.Color.Transparent;
            this.chkDarkMode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDarkMode.Enabled = false;
            this.chkDarkMode.ForeColor = System.Drawing.Color.Black;
            this.chkDarkMode.Location = new System.Drawing.Point(686, 233);
            this.chkDarkMode.Name = "chkDarkMode";
            this.chkDarkMode.Padding = new System.Windows.Forms.Padding(1);
            this.chkDarkMode.Size = new System.Drawing.Size(92, 22);
            this.chkDarkMode.TabIndex = 80;
            this.chkDarkMode.Text = "Dark Mode";
            this.c1ThemeController1.SetTheme(this.chkDarkMode, "(default)");
            this.chkDarkMode.UseVisualStyleBackColor = true;
            this.chkDarkMode.Value = null;
            this.chkDarkMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // picSQLServer
            // 
            this.picSQLServer.BackColor = System.Drawing.Color.Transparent;
            this.picSQLServer.Location = new System.Drawing.Point(13, 381);
            this.picSQLServer.Name = "picSQLServer";
            this.picSQLServer.Size = new System.Drawing.Size(48, 48);
            this.picSQLServer.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picSQLServer.TabIndex = 102;
            this.picSQLServer.TabStop = false;
            // 
            // picPostgreSQL
            // 
            this.picPostgreSQL.BackColor = System.Drawing.Color.Transparent;
            this.picPostgreSQL.Location = new System.Drawing.Point(13, 381);
            this.picPostgreSQL.Name = "picPostgreSQL";
            this.picPostgreSQL.Size = new System.Drawing.Size(48, 48);
            this.picPostgreSQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picPostgreSQL.TabIndex = 102;
            this.picPostgreSQL.TabStop = false;
            // 
            // picOracle
            // 
            this.picOracle.BackColor = System.Drawing.Color.Transparent;
            this.picOracle.Location = new System.Drawing.Point(13, 381);
            this.picOracle.Name = "picOracle";
            this.picOracle.Size = new System.Drawing.Size(48, 48);
            this.picOracle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picOracle.TabIndex = 101;
            this.picOracle.TabStop = false;
            // 
            // btnHelp_PasswordEncrypt
            // 
            this.btnHelp_PasswordEncrypt.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_PasswordEncrypt.Image")));
            this.btnHelp_PasswordEncrypt.Location = new System.Drawing.Point(652, 233);
            this.btnHelp_PasswordEncrypt.Name = "btnHelp_PasswordEncrypt";
            this.btnHelp_PasswordEncrypt.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_PasswordEncrypt.TabIndex = 84;
            this.c1ThemeController1.SetTheme(this.btnHelp_PasswordEncrypt, "(default)");
            this.btnHelp_PasswordEncrypt.UseVisualStyleBackColor = true;
            this.btnHelp_PasswordEncrypt.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_PasswordEncrypt.Click += new System.EventHandler(this.btnHelp_PasswordEncrypt_Click);
            // 
            // chkSavePasswords
            // 
            this.chkSavePasswords.AutoSize = true;
            this.chkSavePasswords.BackColor = System.Drawing.Color.Transparent;
            this.chkSavePasswords.BorderColor = System.Drawing.Color.Transparent;
            this.chkSavePasswords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkSavePasswords.Checked = true;
            this.chkSavePasswords.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSavePasswords.ForeColor = System.Drawing.Color.Black;
            this.chkSavePasswords.Location = new System.Drawing.Point(539, 233);
            this.chkSavePasswords.Name = "chkSavePasswords";
            this.chkSavePasswords.Padding = new System.Windows.Forms.Padding(1);
            this.chkSavePasswords.Size = new System.Drawing.Size(116, 22);
            this.chkSavePasswords.TabIndex = 76;
            this.chkSavePasswords.Text = "Save Passwords";
            this.c1ThemeController1.SetTheme(this.chkSavePasswords, "(default)");
            this.chkSavePasswords.UseVisualStyleBackColor = true;
            this.chkSavePasswords.Value = true;
            this.chkSavePasswords.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnCreateShortcut
            // 
            this.btnCreateShortcut.Enabled = false;
            this.btnCreateShortcut.Location = new System.Drawing.Point(540, 89);
            this.btnCreateShortcut.Name = "btnCreateShortcut";
            this.btnCreateShortcut.Size = new System.Drawing.Size(70, 21);
            this.btnCreateShortcut.TabIndex = 57;
            this.btnCreateShortcut.Text = "Shortcut";
            this.c1ThemeController1.SetTheme(this.btnCreateShortcut, "(default)");
            this.btnCreateShortcut.UseVisualStyleBackColor = true;
            this.btnCreateShortcut.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCreateShortcut.Click += new System.EventHandler(this.btnCreateShortcut_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.Enabled = false;
            this.btnCopy.Location = new System.Drawing.Point(620, 117);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(70, 21);
            this.btnCopy.TabIndex = 54;
            this.btnCopy.Text = "Copy";
            this.c1ThemeController1.SetTheme(this.btnCopy, "(default)");
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // txtSupportInfo
            // 
            this.txtSupportInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtSupportInfo.Enabled = false;
            this.txtSupportInfo.Location = new System.Drawing.Point(67, 382);
            this.txtSupportInfo.Multiline = true;
            this.txtSupportInfo.Name = "txtSupportInfo";
            this.txtSupportInfo.ReadOnly = true;
            this.txtSupportInfo.Size = new System.Drawing.Size(729, 48);
            this.txtSupportInfo.TabIndex = 65;
            this.txtSupportInfo.Text = resources.GetString("txtSupportInfo.Text");
            this.c1ThemeController1.SetTheme(this.txtSupportInfo, "(default)");
            // 
            // lblSupportInfo
            // 
            this.lblSupportInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSupportInfo.AutoSize = true;
            this.lblSupportInfo.Location = new System.Drawing.Point(20, 315);
            this.lblSupportInfo.Name = "lblSupportInfo";
            this.lblSupportInfo.Size = new System.Drawing.Size(0, 16);
            this.lblSupportInfo.TabIndex = 35;
            this.lblSupportInfo.Tag = "SQL Server support: SQL Azure, SQL Server 2000, 2005, 2008, 2012, 2014, 2016, 201" +
    "7 and MSDE";
            // 
            // grpColorTheme
            // 
            this.grpColorTheme.Location = new System.Drawing.Point(851, 70);
            this.grpColorTheme.Name = "grpColorTheme";
            this.grpColorTheme.Size = new System.Drawing.Size(260, 53);
            this.grpColorTheme.TabIndex = 59;
            this.grpColorTheme.TabStop = false;
            this.grpColorTheme.Text = "Color Theme";
            this.c1ThemeController1.SetTheme(this.grpColorTheme, "(default)");
            this.grpColorTheme.Visible = false;
            // 
            // grpLocalization
            // 
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(540, 11);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(295, 45);
            this.grpLocalization.TabIndex = 57;
            this.grpLocalization.TabStop = false;
            this.grpLocalization.Text = "Localization";
            this.c1ThemeController1.SetTheme(this.grpLocalization, "(default)");
            // 
            // cboLocalization
            // 
            this.cboLocalization.AllowSpinLoop = false;
            this.cboLocalization.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboLocalization.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboLocalization.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboLocalization.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboLocalization.GapHeight = 0;
            this.cboLocalization.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboLocalization.ItemsDisplayMember = "";
            this.cboLocalization.ItemsValueMember = "";
            this.cboLocalization.Location = new System.Drawing.Point(29, 18);
            this.cboLocalization.Name = "cboLocalization";
            this.cboLocalization.Size = new System.Drawing.Size(236, 21);
            this.cboLocalization.TabIndex = 79;
            this.cboLocalization.Tag = null;
            this.cboLocalization.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboLocalization, "(default)");
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboLocalization.SelectedIndexChanged += new System.EventHandler(this.cboLocalization_SelectedIndexChanged);
            // 
            // txtAlignment
            // 
            this.txtAlignment.Location = new System.Drawing.Point(489, 5);
            this.txtAlignment.Multiline = true;
            this.txtAlignment.Name = "txtAlignment";
            this.txtAlignment.ReadOnly = true;
            this.txtAlignment.Size = new System.Drawing.Size(35, 151);
            this.txtAlignment.TabIndex = 80;
            this.txtAlignment.Visible = false;
            // 
            // cboDataSource
            // 
            this.cboDataSource.AllowSpinLoop = false;
            this.cboDataSource.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboDataSource.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDataSource.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboDataSource.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboDataSource.GapHeight = 0;
            this.cboDataSource.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDataSource.Items.Add("Oracle");
            this.cboDataSource.Items.Add("PostgreSQL");
            this.cboDataSource.Items.Add("SQL Server");
            this.cboDataSource.Items.Add("MySQL/MariaDB");
            this.cboDataSource.ItemsDisplayMember = "";
            this.cboDataSource.ItemsValueMember = "";
            this.cboDataSource.Location = new System.Drawing.Point(137, 20);
            this.cboDataSource.Name = "cboDataSource";
            this.cboDataSource.Size = new System.Drawing.Size(138, 21);
            this.cboDataSource.TabIndex = 1;
            this.cboDataSource.Tag = null;
            this.cboDataSource.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDataSource, "(default)");
            this.cboDataSource.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDataSource.TextChanged += new System.EventHandler(this.cboDataSource_TextChanged);
            // 
            // btnNew
            // 
            this.btnNew.Location = new System.Drawing.Point(620, 61);
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(70, 21);
            this.btnNew.TabIndex = 52;
            this.btnNew.Text = "New";
            this.c1ThemeController1.SetTheme(this.btnNew, "(default)");
            this.btnNew.UseVisualStyleBackColor = true;
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // lblDomainUser
            // 
            this.lblDomainUser.AutoSize = true;
            this.lblDomainUser.Enabled = false;
            this.lblDomainUser.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDomainUser.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblDomainUser.Location = new System.Drawing.Point(367, 57);
            this.lblDomainUser.Name = "lblDomainUser";
            this.lblDomainUser.Size = new System.Drawing.Size(77, 16);
            this.lblDomainUser.TabIndex = 25;
            this.lblDomainUser.Text = "DomainUser";
            this.lblDomainUser.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblDomainUser.Visible = false;
            // 
            // lblPID
            // 
            this.lblPID.Enabled = false;
            this.lblPID.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPID.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblPID.Location = new System.Drawing.Point(409, 278);
            this.lblPID.Name = "lblPID";
            this.lblPID.Size = new System.Drawing.Size(92, 16);
            this.lblPID.TabIndex = 24;
            this.lblPID.Text = "PID";
            this.lblPID.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.lblPID.Visible = false;
            // 
            // lblDataSource
            // 
            this.lblDataSource.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDataSource.Location = new System.Drawing.Point(25, 21);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(110, 16);
            this.lblDataSource.TabIndex = 0;
            this.lblDataSource.Text = "Data Source:";
            this.lblDataSource.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // btnSave
            // 
            this.btnSave.Location = new System.Drawing.Point(620, 89);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(70, 21);
            this.btnSave.TabIndex = 53;
            this.btnSave.Text = "Save";
            this.c1ThemeController1.SetTheme(this.btnSave, "(default)");
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Location = new System.Drawing.Point(620, 145);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(70, 21);
            this.btnDelete.TabIndex = 55;
            this.btnDelete.Text = "Delete";
            this.c1ThemeController1.SetTheme(this.btnDelete, "(default)");
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(540, 61);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(70, 21);
            this.btnTest.TabIndex = 56;
            this.btnTest.Text = "Test";
            this.c1ThemeController1.SetTheme(this.btnTest, "(default)");
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // picSQLite
            // 
            this.picSQLite.BackColor = System.Drawing.Color.Transparent;
            this.picSQLite.Location = new System.Drawing.Point(13, 381);
            this.picSQLite.Name = "picSQLite";
            this.picSQLite.Size = new System.Drawing.Size(48, 48);
            this.picSQLite.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picSQLite.TabIndex = 107;
            this.picSQLite.TabStop = false;
            // 
            // picMySQL
            // 
            this.picMySQL.BackColor = System.Drawing.Color.Transparent;
            this.picMySQL.Location = new System.Drawing.Point(13, 381);
            this.picMySQL.Name = "picMySQL";
            this.picMySQL.Size = new System.Drawing.Size(48, 48);
            this.picMySQL.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picMySQL.TabIndex = 104;
            this.picMySQL.TabStop = false;
            // 
            // btnImport
            // 
            this.btnImport.Location = new System.Drawing.Point(540, 145);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(70, 21);
            this.btnImport.TabIndex = 109;
            this.btnImport.Text = "Import";
            this.c1ThemeController1.SetTheme(this.btnImport, "(default)");
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(540, 117);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(70, 21);
            this.btnExport.TabIndex = 108;
            this.btnExport.Text = "Export";
            this.c1ThemeController1.SetTheme(this.btnExport, "(default)");
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // picDatabase
            // 
            this.picDatabase.Location = new System.Drawing.Point(280, 22);
            this.picDatabase.Name = "picDatabase";
            this.picDatabase.Size = new System.Drawing.Size(16, 16);
            this.picDatabase.TabIndex = 112;
            this.picDatabase.TabStop = false;
            this.picDatabase.Visible = false;
            // 
            // lblMainFormIconStyle
            // 
            this.lblMainFormIconStyle.AutoSize = true;
            this.lblMainFormIconStyle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblMainFormIconStyle.Location = new System.Drawing.Point(546, 244);
            this.lblMainFormIconStyle.Name = "lblMainFormIconStyle";
            this.lblMainFormIconStyle.Size = new System.Drawing.Size(19, 16);
            this.lblMainFormIconStyle.TabIndex = 44;
            this.lblMainFormIconStyle.Text = "M";
            this.lblMainFormIconStyle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblMainFormIconStyle, "(default)");
            this.lblMainFormIconStyle.Visible = false;
            // 
            // btnHelp_ShowColumnInfo
            // 
            this.btnHelp_ShowColumnInfo.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ShowColumnInfo.Image")));
            this.btnHelp_ShowColumnInfo.Location = new System.Drawing.Point(722, 233);
            this.btnHelp_ShowColumnInfo.Name = "btnHelp_ShowColumnInfo";
            this.btnHelp_ShowColumnInfo.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ShowColumnInfo.TabIndex = 106;
            this.c1ThemeController1.SetTheme(this.btnHelp_ShowColumnInfo, "(default)");
            this.btnHelp_ShowColumnInfo.UseVisualStyleBackColor = true;
            this.btnHelp_ShowColumnInfo.Visible = false;
            this.btnHelp_ShowColumnInfo.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ShowColumnInfo.Click += new System.EventHandler(this.btnHelp_ShowColumnInfo_Click);
            // 
            // chkShowColumnInfo
            // 
            this.chkShowColumnInfo.AutoSize = true;
            this.chkShowColumnInfo.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnInfo.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnInfo.Enabled = false;
            this.chkShowColumnInfo.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnInfo.Location = new System.Drawing.Point(539, 233);
            this.chkShowColumnInfo.Name = "chkShowColumnInfo";
            this.chkShowColumnInfo.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnInfo.Size = new System.Drawing.Size(175, 22);
            this.chkShowColumnInfo.TabIndex = 105;
            this.chkShowColumnInfo.Text = "Show Column Information";
            this.c1ThemeController1.SetTheme(this.chkShowColumnInfo, "(default)");
            this.chkShowColumnInfo.UseVisualStyleBackColor = true;
            this.chkShowColumnInfo.Value = null;
            this.chkShowColumnInfo.Visible = false;
            this.chkShowColumnInfo.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1GridDBInfo
            // 
            this.c1GridDBInfo.AllowUpdate = false;
            this.c1GridDBInfo.AlternatingRows = true;
            this.c1GridDBInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridDBInfo.CaptionHeight = 19;
            this.c1GridDBInfo.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridDBInfo.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridDBInfo.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridDBInfo.Images"))));
            this.c1GridDBInfo.Location = new System.Drawing.Point(10, 448);
            this.c1GridDBInfo.Name = "c1GridDBInfo";
            this.c1GridDBInfo.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridDBInfo.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridDBInfo.PreviewInfo.ZoomFactor = 75D;
            this.c1GridDBInfo.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridDBInfo.PrintInfo.MeasurementPrinterName = null;
            this.c1GridDBInfo.RowHeight = 17;
            this.c1GridDBInfo.Size = new System.Drawing.Size(847, 253);
            this.c1GridDBInfo.TabIndex = 99;
            this.c1GridDBInfo.Text = "c1TrueDBGrid2";
            this.c1GridDBInfo.UseCompatibleTextRendering = false;
            this.c1GridDBInfo.RowColChange += new C1.Win.C1TrueDBGrid.RowColChangeEventHandler(this.c1GridDBInfo_RowColChange);
            this.c1GridDBInfo.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridDBInfo_FetchCellStyle);
            this.c1GridDBInfo.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridDBInfo_MouseDoubleClick);
            this.c1GridDBInfo.PropBag = resources.GetString("c1GridDBInfo.PropBag");
            // 
            // grpOpenFile
            // 
            this.grpOpenFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOpenFile.Controls.Add(this.txtOpenSpecifiedFile);
            this.grpOpenFile.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpOpenFile.ForeColor = System.Drawing.Color.Green;
            this.grpOpenFile.Location = new System.Drawing.Point(10, 448);
            this.grpOpenFile.Name = "grpOpenFile";
            this.grpOpenFile.Size = new System.Drawing.Size(847, 56);
            this.grpOpenFile.TabIndex = 15;
            this.grpOpenFile.TabStop = false;
            this.grpOpenFile.Text = "Open the specified file below after connecting to database";
            this.grpOpenFile.Visible = false;
            // 
            // txtOpenSpecifiedFile
            // 
            this.txtOpenSpecifiedFile.Enabled = false;
            this.txtOpenSpecifiedFile.Location = new System.Drawing.Point(18, 21);
            this.txtOpenSpecifiedFile.Name = "txtOpenSpecifiedFile";
            this.txtOpenSpecifiedFile.Size = new System.Drawing.Size(801, 23);
            this.txtOpenSpecifiedFile.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.txtOpenSpecifiedFile, "(default)");
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(869, 712);
            this.Controls.Add(this.grpConnect);
            this.Controls.Add(this.grpOpenFile);
            this.Controls.Add(this.c1GridDBInfo);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Database Connection";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            this.grpConnect.ResumeLayout(false);
            this.grpConnect.PerformLayout();
            this.pnlPostgreSQL.ResumeLayout(false);
            this.pnlPostgreSQL.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridPostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoRollback_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Unicode_PostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoRollback_PostgreSQL)).EndInit();
            this.pnlOracle.ResumeLayout(false);
            this.pnlOracle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SID_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DirectMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnectAs_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSID_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDirectMode_Oracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Log)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkLog)).EndInit();
            this.pnlMySQL.ResumeLayout(false);
            this.pnlMySQL.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridMySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Unicode_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUnicode_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_MySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUpdateNow)).EndInit();
            this.pnlConnect.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnConnect)).EndInit();
            this.grpTabColor.ResumeLayout(false);
            this.grpTabColor.PerformLayout();
            this.tabExample.ResumeLayout(false);
            this.pnlSQLite.ResumeLayout(false);
            this.pnlSQLite.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DataType)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptWithPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRemovePassword)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabaseType_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Button1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFile_SQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_SQLite)).EndInit();
            this.pnlSQLServer.ResumeLayout(false);
            this.pnlSQLServer.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ExcludeNativeDatabase_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkExcludeNativeObject_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPort_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDatabase_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Pooling_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPooling_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ConnectionName_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtPassword_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtUserID_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConnectionName_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtServer_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRemark_SQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_MainFormIconStyle)).EndInit();
            this.grpMainFormIconStyle.ResumeLayout(false);
            this.grpMainFormIconStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picIconStyle0)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DarkMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptWithCustomPW)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDarkMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSQLServer)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picPostgreSQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picOracle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PasswordEncrypt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSavePasswords)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateShortcut)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDataSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnNew)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSave)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnTest)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSQLite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picMySQL)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnImport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picDatabase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ShowColumnInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridDBInfo)).EndInit();
            this.grpOpenFile.ResumeLayout(false);
            this.grpOpenFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.GroupBox grpConnect;
        private System.Windows.Forms.Label lblDataSource;
        private System.Windows.Forms.Label lblInactiveForeColor;
        private System.Windows.Forms.Label lblDomainUser;
        private System.Windows.Forms.Label lblPID;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridDBInfo;
        private System.Windows.Forms.GroupBox grpOpenFile;
        private System.Windows.Forms.TextBox txtOpenSpecifiedFile;
        private System.Windows.Forms.Label lblSupportInfo;
        private System.Windows.Forms.Panel pnlInactiveForeColor;
        private Crownwood.Magic.Controls.TabControl tabExample;
        private Crownwood.Magic.Controls.TabPage tabPage1;
        private Crownwood.Magic.Controls.TabPage tabPage2;
        private System.Windows.Forms.Panel pnlActiveForeColor;
        private System.Windows.Forms.Label lblActiveForeColor;
        private System.Windows.Forms.Panel pnlBackColor;
        private System.Windows.Forms.Label lblBackColor;
        private System.Windows.Forms.GroupBox grpTabColor;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Input.C1Button btnNew;
        private C1.Win.C1Input.C1Button btnSave;
        private C1.Win.C1Input.C1Button btnConnect;
        private C1.Win.C1Input.C1Button btnDelete;
        private C1.Win.C1Input.C1Button btnTest;
        private C1.Win.C1Input.C1Button btnHelp_DirectMode;
        private C1.Win.C1Input.C1ComboBox cboDataSource;
        private C1.Win.C1Input.C1CheckBox chkDirectMode_Oracle;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
        private System.Windows.Forms.GroupBox grpColorTheme;
        private C1.Win.C1Input.C1CheckBox chkDarkMode;
        private System.Windows.Forms.TextBox txtAlignment;
        private C1.Win.C1Input.C1Button btnHelp_DarkMode;
        private System.Windows.Forms.TextBox txtSupportInfo;
        private C1.Win.C1Input.C1Button btnCopy;
        private C1.Win.C1Input.C1Button btnCreateShortcut;
        private C1.Win.C1Input.C1CheckBox chkSavePasswords;
        private System.Windows.Forms.Panel pnlPostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_ConnectionName_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtPort_PostgreSQL;
        private C1.Win.C1Input.C1CheckBox chkAutoRollback_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtPassword_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtUserID_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtConnectionName_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtServer_PostgreSQL;
        private C1.Win.C1Input.C1TextBox txtRemark_PostgreSQL;
        private System.Windows.Forms.Label lblRemark_PostgreSQL;
        private System.Windows.Forms.Label lblPassword_PostgreSQL;
        private System.Windows.Forms.Label lblUserID_PostgreSQL;
        private System.Windows.Forms.Label lblDatabase_PostgreSQL;
        private System.Windows.Forms.Label lblServer_PostgreSQL;
        private System.Windows.Forms.Label lblPort_PostgreSQL;
        private System.Windows.Forms.Label lblConnectionName_PostgreSQL;
        private C1.Win.C1Input.C1CheckBox chkUnicode_PostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_AutoRollback_PostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_Unicode_PostgreSQL;
        private System.Windows.Forms.Panel pnlOracle;
        private C1.Win.C1Input.C1Button btnHelp_ConnectionName_Oracle;
        private C1.Win.C1Input.C1TextBox txtPort_Oracle;
        private C1.Win.C1Input.C1ComboBox cboConnectAs_Oracle;
        private C1.Win.C1Input.C1TextBox txtSID_Oracle;
        private C1.Win.C1Input.C1TextBox txtPassword_Oracle;
        private C1.Win.C1Input.C1TextBox txtUserID_Oracle;
        private C1.Win.C1Input.C1TextBox txtConnectionName_Oracle;
        private C1.Win.C1Input.C1TextBox txtServer_Oracle;
        private C1.Win.C1Input.C1TextBox txtRemark_Oracle;
        private System.Windows.Forms.Label lblRemark_Oracle;
        private System.Windows.Forms.Label lblPassword_Oracle;
        private System.Windows.Forms.Label lblUserID_Oracle;
        private System.Windows.Forms.Label lblServer_Oracle;
        private System.Windows.Forms.Label lblPort_Oracle;
        private System.Windows.Forms.Label lblConnectionName_Oracle;
        private System.Windows.Forms.Label lblConnectAs_Oracle;
        private System.Windows.Forms.TextBox textBox2;
        private System.Windows.Forms.Label lblSID_Oracle;
        private C1.Win.C1Input.C1Button btnHelp_SID_Oracle;
        private System.Windows.Forms.Panel pnlSQLServer;
        private C1.Win.C1Input.C1Button btnHelp_ConnectionName_SQLServer;
        private C1.Win.C1Input.C1TextBox txtPassword_SQLServer;
        private C1.Win.C1Input.C1TextBox txtUserID_SQLServer;
        private C1.Win.C1Input.C1TextBox txtConnectionName_SQLServer;
        private C1.Win.C1Input.C1TextBox txtServer_SQLServer;
        private C1.Win.C1Input.C1TextBox txtRemark_SQLServer;
        private System.Windows.Forms.Label lblRemark_SQLServer;
        private System.Windows.Forms.Label lblPassword_SQLServer;
        private System.Windows.Forms.Label lblUserID_SQLServer;
        private System.Windows.Forms.Label lblDatabase_SQLServer;
        private System.Windows.Forms.Label lblServer_SQLServer;
        private System.Windows.Forms.Label lblConnectionName_SQLServer;
        private C1.Win.C1Input.C1Button btnHelp_PasswordEncrypt;
        private C1.Win.C1Input.C1CheckBox chkPooling_SQLServer;
        private C1.Win.C1Input.C1Button btnHelp_Pooling_Oracle;
        private C1.Win.C1Input.C1CheckBox chkPooling_Oracle;
        private C1.Win.C1Input.C1CheckBox chkPooling_PostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_Pooling_PostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_Pooling_SQLServer;
        private System.Windows.Forms.PictureBox picOracle;
        private System.Windows.Forms.PictureBox picPostgreSQL;
        private System.Windows.Forms.PictureBox picSQLServer;
        private System.Windows.Forms.Panel pnlMySQL;
        private C1.Win.C1Input.C1Button btnHelp_Pooling_MySQL;
        private C1.Win.C1Input.C1CheckBox chkPooling_MySQL;
        private C1.Win.C1Input.C1Button btnHelp_ConnectionName_MySQL;
        private C1.Win.C1Input.C1TextBox txtPassword_MySQL;
        private C1.Win.C1Input.C1TextBox txtUserID_MySQL;
        private C1.Win.C1Input.C1TextBox txtConnectionName_MySQL;
        private C1.Win.C1Input.C1TextBox txtServer_MySQL;
        private C1.Win.C1Input.C1TextBox txtRemark_MySQL;
        private System.Windows.Forms.Label lblRemark_MySQL;
        private System.Windows.Forms.Label lblPassword_MySQL;
        private System.Windows.Forms.Label lblUserID_MySQL;
        private System.Windows.Forms.Label lblDatabase_MySQL;
        private System.Windows.Forms.Label lblServer_MySQL;
        private System.Windows.Forms.Label lblConnectionName_MySQL;
        private System.Windows.Forms.PictureBox picMySQL;
        private C1.Win.C1Input.C1CheckBox chkUnicode_MySQL;
        private C1.Win.C1Input.C1Button btnHelp_Unicode_MySQL;
        private C1.Win.C1Input.C1Button btnHelp_ShowColumnInfo;
        private C1.Win.C1Input.C1CheckBox chkShowColumnInfo;
        private C1.Win.C1Input.C1ComboBox cboDatabase_MySQL;
        private C1.Win.C1Input.C1ComboBox cboDatabase_PostgreSQL;
        private C1.Win.C1Input.C1ComboBox cboDatabase_SQLServer;
        private C1.Win.C1Input.C1CheckBox chkUnicode_Oracle;
        private C1.Win.C1Input.C1TextBox txtPort_SQLServer;
        private System.Windows.Forms.Label lblPort_SQLServer;
        private C1.Win.C1Input.C1TextBox txtPort_MySQL;
        private System.Windows.Forms.Label lblPort_MySQL;
        private System.Windows.Forms.Panel pnlSQLite;
        private C1.Win.C1Input.C1Button btnBrowseFile_SQLite;
        private C1.Win.C1Input.C1ComboBox cboDatabaseType_SQLite;
        private C1.Win.C1Input.C1CheckBox c1CheckBox1;
        private C1.Win.C1Input.C1Button c1Button1;
        private C1.Win.C1Input.C1Button btnHelp_ConnectionName_SQLite;
        private C1.Win.C1Input.C1TextBox txtPassword_SQLite;
        private C1.Win.C1Input.C1TextBox txtConnectionName_SQLite;
        private C1.Win.C1Input.C1TextBox txtFile_SQLite;
        private C1.Win.C1Input.C1TextBox txtRemark_SQLite;
        private System.Windows.Forms.Label lblRemark_SQLite;
        private System.Windows.Forms.Label lblPassword_SQLite;
        private System.Windows.Forms.Label lblDatabaseType_SQLite;
        private System.Windows.Forms.Label lblDatabaseFile_SQLite;
        private System.Windows.Forms.Label lblConnectionName_SQLite;
        private System.Windows.Forms.RadioButton rdoOpen_SQLite;
        private System.Windows.Forms.RadioButton rdoCreate_SQLite;
        private C1.Win.C1Input.C1CheckBox chkExcludeNativeObject_SQLServer;
        private C1.Win.C1Input.C1Button btnHelp_ExcludeNativeDatabase_SQLServer;
        private System.Windows.Forms.PictureBox picSQLite;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.RadioButton rdoWithPassword;
        private System.Windows.Forms.RadioButton rdoWithoutPassword;
        private System.Windows.Forms.Panel panel1;
        private C1.Win.C1Input.C1Button btnEncryptWithPassword;
        private C1.Win.C1Input.C1Button btnRemovePassword;
        private C1.Win.C1Input.C1Button btnHelp_DataType;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridSQLServer;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridMySQL;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridPostgreSQL;
        private C1.Win.C1Input.C1Button btnImport;
        private C1.Win.C1Input.C1Button btnExport;
        private C1.Win.C1Input.C1Button btnEncryptWithCustomPW;
        private System.Windows.Forms.Label lblQueryTimeout_Oracle;
        private C1.Win.C1Input.C1Button btnHelp_QueryTimeout_Oracle;
        private System.Windows.Forms.Label lblSeconds_Oracle;
        private System.Windows.Forms.NumericUpDown nudQueryTimeout_Oracle;
        private C1.Win.C1Input.C1Button btnHelp_QueryTimeout_PostgreSQL;
        private System.Windows.Forms.Label lblSeconds_PostgreSQL;
        private System.Windows.Forms.NumericUpDown nudQueryTimeout_PostgreSQL;
        private System.Windows.Forms.Label lblQueryTimeout_PostgreSQL;
        private C1.Win.C1Input.C1Button btnHelp_QueryTimeout_MySQL;
        private System.Windows.Forms.Label lblSeconds_MySQL;
        private System.Windows.Forms.NumericUpDown nudQueryTimeout_MySQL;
        private System.Windows.Forms.Label lblQueryTimeout_MySQL;
        private C1.Win.C1Input.C1Button btnHelp_QueryTimeout_SQLServer;
        private System.Windows.Forms.Label lblSeconds_SQLServer;
        private System.Windows.Forms.NumericUpDown nudQueryTimeout_SQLServer;
        private System.Windows.Forms.Label lblQueryTimeout_SQLServer;
        private System.Windows.Forms.PictureBox picDatabase;
        private System.Windows.Forms.GroupBox grpMainFormIconStyle;
        private System.Windows.Forms.RadioButton rdoIconStyle0;
        private System.Windows.Forms.PictureBox picIconStyle0;
        private System.Windows.Forms.PictureBox picIconStyle1;
        private System.Windows.Forms.RadioButton rdoIconStyle1;
        private C1.Win.C1Input.C1Button btnHelp_MainFormIconStyle;
        private System.Windows.Forms.Label lblMainFormIconStyle;
        private System.Windows.Forms.Panel pnlConnect;
        private C1.Win.C1Input.C1Button btnExit;
        private C1.Win.C1Input.C1Button btnUpdateNow;
        private C1.Win.C1Input.C1Button btnHelp_Log;
        private C1.Win.C1Input.C1CheckBox chkLog;
        private System.Windows.Forms.Label lblPrompt;
        private System.Windows.Forms.PictureBox picIconStyle2;
        private System.Windows.Forms.RadioButton rdoIconStyle2;
        private System.Windows.Forms.PictureBox picIconStyle6;
        private System.Windows.Forms.RadioButton rdoIconStyle6;
        private System.Windows.Forms.PictureBox picIconStyle5;
        private System.Windows.Forms.RadioButton rdoIconStyle5;
        private System.Windows.Forms.PictureBox picIconStyle4;
        private System.Windows.Forms.RadioButton rdoIconStyle4;
        private System.Windows.Forms.PictureBox picIconStyle3;
        private System.Windows.Forms.RadioButton rdoIconStyle3;
        private System.Windows.Forms.PictureBox picIconStyle9;
        private System.Windows.Forms.RadioButton rdoIconStyle9;
        private System.Windows.Forms.PictureBox picIconStyle8;
        private System.Windows.Forms.RadioButton rdoIconStyle8;
        private System.Windows.Forms.PictureBox picIconStyle7;
        private System.Windows.Forms.RadioButton rdoIconStyle7;
    }
}

