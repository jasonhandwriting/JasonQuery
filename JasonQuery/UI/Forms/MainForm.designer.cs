namespace JasonQuery.UI.Forms
{
    partial class MainForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置 Managed 資源則為 true，否則為 false。</param>
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mnuMainForm = new System.Windows.Forms.MenuStrip();
            this.mnuFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCloseConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDatabaseConnectionViewMode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuNewSqlEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOpenFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFile_Save = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFile_SaveAs = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem9 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuQuery = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNewSqlEditor2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuQuery_Execute = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuQuery_ExecuteCurrentBlock = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem11 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuQuery_Explain = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuQuery_ExplainAnalyze = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuQuery_ExplainOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem12 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAutoRollback = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAutoCommit = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem13 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuQuery_Commit = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuQuery_Rollback = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuRecentFiles = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMyFavorite = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSwitchDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAssistant = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuWindowsExplorer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuNotepad = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCalculator = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuPaint = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem18 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAsciiTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuBlobViewer = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFileSplitter = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuColorConverter = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem17 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuOpenMethod = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDesktop = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTemporary = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStartupAllUser = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuStartupPersonal = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuJasonQueryLocated = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuTools = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuOptions = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuSqlHistory = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSchemaBrowser = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGenerateSqlStatement = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSchemaSearch = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem14 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCreateTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuImportTableData = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem15 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCompactMDB = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuHelp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCheckForUpdatesManually = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUpdateNow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReleaseNotes = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuReportBugs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAbout = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.tmrTimeAndKeyStatus = new System.Windows.Forms.Timer(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnConnect = new System.Windows.Forms.ToolStripButton();
            this.btnDisconnect = new System.Windows.Forms.ToolStripButton();
            this.btnNewSqlEditor = new System.Windows.Forms.ToolStripButton();
            this.btnSetting = new System.Windows.Forms.ToolStripButton();
            this.tsMainMenuToolBar = new System.Windows.Forms.ToolStrip();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.c1StatusBar1 = new C1.Win.C1Ribbon.C1StatusBar();
            this.ribbonLabel1 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblInfo = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblDBVersion = new C1.Win.C1Ribbon.RibbonLabel();
            this.spDBVersion = new C1.Win.C1Ribbon.RibbonSeparator();
            this.btnDatabase = new C1.Win.C1Ribbon.RibbonButton();
            this.spDatabase = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblNotCommitYet = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblNotCommitYetTime = new C1.Win.C1Ribbon.RibbonLabel();
            this.spNotCommitYet = new C1.Win.C1Ribbon.RibbonSeparator();
            this.btnIP = new C1.Win.C1Ribbon.RibbonButton();
            this.spDomainUser = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblDomainUser = new C1.Win.C1Ribbon.RibbonLabel();
            this.spAutoRollbackOnError = new C1.Win.C1Ribbon.RibbonSeparator();
            this.btnAutoRollbackOnErrorOn = new C1.Win.C1Ribbon.RibbonButton();
            this.btnAutoRollbackOnErrorOff = new C1.Win.C1Ribbon.RibbonButton();
            this.spAutoCommit = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblAutoCommit = new C1.Win.C1Ribbon.RibbonLabel();
            this.pnlArrow = new System.Windows.Forms.Panel();
            this.lblPrompt4NewConnection = new System.Windows.Forms.Label();
            this.picArrow = new System.Windows.Forms.PictureBox();
            this.tmrPendingTransactionIdleCheck = new System.Windows.Forms.Timer(this.components);
            this.tabControl1 = new Crownwood.Magic.Controls.TabControl();
            this.mnuMainForm.SuspendLayout();
            this.tsMainMenuToolBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).BeginInit();
            this.pnlArrow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picArrow)).BeginInit();
            this.SuspendLayout();
            // 
            // mnuMainForm
            // 
            this.mnuMainForm.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.mnuMainForm.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFile,
            this.mnuQuery,
            this.mnuRecentFiles,
            this.mnuMyFavorite,
            this.mnuSwitchDatabase,
            this.mnuAssistant,
            this.mnuTools,
            this.mnuHelp,
            this.mnuInfo});
            this.mnuMainForm.Location = new System.Drawing.Point(0, 0);
            this.mnuMainForm.Name = "mnuMainForm";
            this.mnuMainForm.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.mnuMainForm.ShowItemToolTips = true;
            this.mnuMainForm.Size = new System.Drawing.Size(1008, 26);
            this.mnuMainForm.TabIndex = 9;
            this.mnuMainForm.Text = "menuStrip1";
            this.c1ThemeController1.SetTheme(this.mnuMainForm, "(default)");
            this.mnuMainForm.MouseEnter += new System.EventHandler(this.mnuMainForm_MouseEnter);
            this.mnuMainForm.MouseLeave += new System.EventHandler(this.mnuMainForm_MouseLeave);
            this.mnuMainForm.MouseMove += new System.Windows.Forms.MouseEventHandler(this.mnuMainForm_MouseMove);
            // 
            // mnuFile
            // 
            this.mnuFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewConnection,
            this.mnuOpenConnection,
            this.mnuCloseConnection,
            this.mnuDatabaseConnectionViewMode,
            this.toolStripSeparator2,
            this.mnuNewSqlEditor,
            this.mnuOpenFiles,
            this.toolStripMenuItem2,
            this.mnuFile_Save,
            this.mnuFile_SaveAs,
            this.toolStripMenuItem9,
            this.mnuExit});
            this.mnuFile.Name = "mnuFile";
            this.mnuFile.Size = new System.Drawing.Size(38, 20);
            this.mnuFile.Text = "&File";
            this.mnuFile.Click += new System.EventHandler(this.mnuFile_Click);
            this.mnuFile.MouseHover += new System.EventHandler(this.mnuFile_MouseHover);
            // 
            // mnuNewConnection
            // 
            this.mnuNewConnection.Image = ((System.Drawing.Image)(resources.GetObject("mnuNewConnection.Image")));
            this.mnuNewConnection.Name = "mnuNewConnection";
            this.mnuNewConnection.Size = new System.Drawing.Size(272, 22);
            this.mnuNewConnection.Text = "New Connection";
            this.mnuNewConnection.Click += new System.EventHandler(this.mnuNewConnection_Click);
            // 
            // mnuOpenConnection
            // 
            this.mnuOpenConnection.Name = "mnuOpenConnection";
            this.mnuOpenConnection.Size = new System.Drawing.Size(272, 22);
            this.mnuOpenConnection.Text = "Open Connection";
            this.mnuOpenConnection.Visible = false;
            this.mnuOpenConnection.Click += new System.EventHandler(this.mnuOpenConnection_Click);
            // 
            // mnuCloseConnection
            // 
            this.mnuCloseConnection.Name = "mnuCloseConnection";
            this.mnuCloseConnection.Size = new System.Drawing.Size(272, 22);
            this.mnuCloseConnection.Text = "End Connection";
            this.mnuCloseConnection.Visible = false;
            this.mnuCloseConnection.Click += new System.EventHandler(this.mnuCloseConnection_Click);
            // 
            // mnuDatabaseConnectionViewMode
            // 
            this.mnuDatabaseConnectionViewMode.Image = ((System.Drawing.Image)(resources.GetObject("mnuDatabaseConnectionViewMode.Image")));
            this.mnuDatabaseConnectionViewMode.Name = "mnuDatabaseConnectionViewMode";
            this.mnuDatabaseConnectionViewMode.Size = new System.Drawing.Size(272, 22);
            this.mnuDatabaseConnectionViewMode.Text = "Database Connection - View Mode";
            this.mnuDatabaseConnectionViewMode.Visible = false;
            this.mnuDatabaseConnectionViewMode.Click += new System.EventHandler(this.mnuDatabaseConnectionViewMode_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(269, 6);
            // 
            // mnuNewSqlEditor
            // 
            this.mnuNewSqlEditor.Image = ((System.Drawing.Image)(resources.GetObject("mnuNewSqlEditor.Image")));
            this.mnuNewSqlEditor.Name = "mnuNewSqlEditor";
            this.mnuNewSqlEditor.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuNewSqlEditor.Size = new System.Drawing.Size(272, 22);
            this.mnuNewSqlEditor.Text = "&New SQL Editor";
            this.mnuNewSqlEditor.Click += new System.EventHandler(this.mnuNewSqlEditor_Click);
            // 
            // mnuOpenFiles
            // 
            this.mnuOpenFiles.Image = ((System.Drawing.Image)(resources.GetObject("mnuOpenFiles.Image")));
            this.mnuOpenFiles.Name = "mnuOpenFiles";
            this.mnuOpenFiles.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.O)));
            this.mnuOpenFiles.Size = new System.Drawing.Size(272, 22);
            this.mnuOpenFiles.Text = "&Open File(s)";
            this.mnuOpenFiles.Click += new System.EventHandler(this.mnuOpenQueryFile_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(269, 6);
            // 
            // mnuFile_Save
            // 
            this.mnuFile_Save.Image = ((System.Drawing.Image)(resources.GetObject("mnuFile_Save.Image")));
            this.mnuFile_Save.Name = "mnuFile_Save";
            this.mnuFile_Save.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.S)));
            this.mnuFile_Save.Size = new System.Drawing.Size(272, 22);
            this.mnuFile_Save.Text = "&Save";
            this.mnuFile_Save.Click += new System.EventHandler(this.mnuFile_Save_Click);
            // 
            // mnuFile_SaveAs
            // 
            this.mnuFile_SaveAs.Image = ((System.Drawing.Image)(resources.GetObject("mnuFile_SaveAs.Image")));
            this.mnuFile_SaveAs.Name = "mnuFile_SaveAs";
            this.mnuFile_SaveAs.ShortcutKeys = System.Windows.Forms.Keys.F12;
            this.mnuFile_SaveAs.Size = new System.Drawing.Size(272, 22);
            this.mnuFile_SaveAs.Text = "Save &As";
            this.mnuFile_SaveAs.Click += new System.EventHandler(this.mnuFile_SaveAs_Click);
            // 
            // toolStripMenuItem9
            // 
            this.toolStripMenuItem9.Name = "toolStripMenuItem9";
            this.toolStripMenuItem9.Size = new System.Drawing.Size(269, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Image = ((System.Drawing.Image)(resources.GetObject("mnuExit.Image")));
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Alt | System.Windows.Forms.Keys.F4)));
            this.mnuExit.Size = new System.Drawing.Size(272, 22);
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // mnuQuery
            // 
            this.mnuQuery.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuNewSqlEditor2,
            this.toolStripMenuItem10,
            this.mnuQuery_Execute,
            this.mnuQuery_ExecuteCurrentBlock,
            this.toolStripMenuItem11,
            this.mnuQuery_Explain,
            this.mnuQuery_ExplainAnalyze,
            this.mnuQuery_ExplainOptions,
            this.toolStripMenuItem12,
            this.mnuAutoRollback,
            this.mnuAutoCommit,
            this.toolStripMenuItem13,
            this.mnuQuery_Commit,
            this.mnuQuery_Rollback});
            this.mnuQuery.Name = "mnuQuery";
            this.mnuQuery.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.mnuQuery.Size = new System.Drawing.Size(53, 20);
            this.mnuQuery.Text = "&Query";
            this.mnuQuery.Visible = false;
            // 
            // mnuNewSqlEditor2
            // 
            this.mnuNewSqlEditor2.Image = ((System.Drawing.Image)(resources.GetObject("mnuNewSqlEditor2.Image")));
            this.mnuNewSqlEditor2.Name = "mnuNewSqlEditor2";
            this.mnuNewSqlEditor2.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
            this.mnuNewSqlEditor2.Size = new System.Drawing.Size(216, 22);
            this.mnuNewSqlEditor2.Text = "&New SQL Editor";
            this.mnuNewSqlEditor2.Click += new System.EventHandler(this.mnuNewSqlEditor_Click);
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(213, 6);
            // 
            // mnuQuery_Execute
            // 
            this.mnuQuery_Execute.Image = ((System.Drawing.Image)(resources.GetObject("mnuQuery_Execute.Image")));
            this.mnuQuery_Execute.Name = "mnuQuery_Execute";
            this.mnuQuery_Execute.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.mnuQuery_Execute.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_Execute.Text = "Execute";
            // 
            // mnuQuery_ExecuteCurrentBlock
            // 
            this.mnuQuery_ExecuteCurrentBlock.Image = ((System.Drawing.Image)(resources.GetObject("mnuQuery_ExecuteCurrentBlock.Image")));
            this.mnuQuery_ExecuteCurrentBlock.Name = "mnuQuery_ExecuteCurrentBlock";
            this.mnuQuery_ExecuteCurrentBlock.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_ExecuteCurrentBlock.Text = "Execute Current Block";
            // 
            // toolStripMenuItem11
            // 
            this.toolStripMenuItem11.Name = "toolStripMenuItem11";
            this.toolStripMenuItem11.Size = new System.Drawing.Size(213, 6);
            // 
            // mnuQuery_Explain
            // 
            this.mnuQuery_Explain.Name = "mnuQuery_Explain";
            this.mnuQuery_Explain.ShortcutKeys = System.Windows.Forms.Keys.F7;
            this.mnuQuery_Explain.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_Explain.Text = "Explain";
            this.mnuQuery_Explain.Visible = false;
            // 
            // mnuQuery_ExplainAnalyze
            // 
            this.mnuQuery_ExplainAnalyze.Name = "mnuQuery_ExplainAnalyze";
            this.mnuQuery_ExplainAnalyze.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Shift | System.Windows.Forms.Keys.F7)));
            this.mnuQuery_ExplainAnalyze.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_ExplainAnalyze.Text = "Explain Analyze";
            this.mnuQuery_ExplainAnalyze.Visible = false;
            // 
            // mnuQuery_ExplainOptions
            // 
            this.mnuQuery_ExplainOptions.Name = "mnuQuery_ExplainOptions";
            this.mnuQuery_ExplainOptions.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_ExplainOptions.Text = "Explain Options";
            this.mnuQuery_ExplainOptions.Visible = false;
            // 
            // toolStripMenuItem12
            // 
            this.toolStripMenuItem12.Name = "toolStripMenuItem12";
            this.toolStripMenuItem12.Size = new System.Drawing.Size(213, 6);
            this.toolStripMenuItem12.Visible = false;
            // 
            // mnuAutoRollback
            // 
            this.mnuAutoRollback.Name = "mnuAutoRollback";
            this.mnuAutoRollback.Size = new System.Drawing.Size(216, 22);
            this.mnuAutoRollback.Text = "Auto-Rollback";
            // 
            // mnuAutoCommit
            // 
            this.mnuAutoCommit.Enabled = false;
            this.mnuAutoCommit.Name = "mnuAutoCommit";
            this.mnuAutoCommit.Size = new System.Drawing.Size(216, 22);
            this.mnuAutoCommit.Text = "Auto-Commit";
            // 
            // toolStripMenuItem13
            // 
            this.toolStripMenuItem13.Name = "toolStripMenuItem13";
            this.toolStripMenuItem13.Size = new System.Drawing.Size(213, 6);
            // 
            // mnuQuery_Commit
            // 
            this.mnuQuery_Commit.Name = "mnuQuery_Commit";
            this.mnuQuery_Commit.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_Commit.Text = "Commit";
            // 
            // mnuQuery_Rollback
            // 
            this.mnuQuery_Rollback.Name = "mnuQuery_Rollback";
            this.mnuQuery_Rollback.Size = new System.Drawing.Size(216, 22);
            this.mnuQuery_Rollback.Text = "Rollback";
            // 
            // mnuRecentFiles
            // 
            this.mnuRecentFiles.Enabled = false;
            this.mnuRecentFiles.Name = "mnuRecentFiles";
            this.mnuRecentFiles.Size = new System.Drawing.Size(85, 20);
            this.mnuRecentFiles.Text = "&Recent Files";
            // 
            // mnuMyFavorite
            // 
            this.mnuMyFavorite.Enabled = false;
            this.mnuMyFavorite.Name = "mnuMyFavorite";
            this.mnuMyFavorite.Size = new System.Drawing.Size(85, 20);
            this.mnuMyFavorite.Text = "&My Favorite";
            // 
            // mnuSwitchDatabase
            // 
            this.mnuSwitchDatabase.Name = "mnuSwitchDatabase";
            this.mnuSwitchDatabase.Size = new System.Drawing.Size(112, 20);
            this.mnuSwitchDatabase.Text = "&Switch Database";
            this.mnuSwitchDatabase.Visible = false;
            // 
            // mnuAssistant
            // 
            this.mnuAssistant.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuWindowsExplorer,
            this.mnuNotepad,
            this.mnuCalculator,
            this.mnuPaint,
            this.toolStripMenuItem18,
            this.mnuAsciiTable,
            this.mnuBlobViewer,
            this.mnuFileSplitter,
            this.mnuColorConverter,
            this.toolStripMenuItem17,
            this.mnuOpenMethod,
            this.mnuDesktop,
            this.mnuTemporary,
            this.mnuStartupAllUser,
            this.mnuStartupPersonal,
            this.mnuJasonQueryLocated});
            this.mnuAssistant.Name = "mnuAssistant";
            this.mnuAssistant.Size = new System.Drawing.Size(67, 20);
            this.mnuAssistant.Text = "&Assistant";
            // 
            // mnuWindowsExplorer
            // 
            this.mnuWindowsExplorer.Image = ((System.Drawing.Image)(resources.GetObject("mnuWindowsExplorer.Image")));
            this.mnuWindowsExplorer.Name = "mnuWindowsExplorer";
            this.mnuWindowsExplorer.Size = new System.Drawing.Size(342, 22);
            this.mnuWindowsExplorer.Tag = "Windows Explorer";
            this.mnuWindowsExplorer.Text = "Windows Explorer";
            this.mnuWindowsExplorer.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuNotepad
            // 
            this.mnuNotepad.Image = ((System.Drawing.Image)(resources.GetObject("mnuNotepad.Image")));
            this.mnuNotepad.Name = "mnuNotepad";
            this.mnuNotepad.Size = new System.Drawing.Size(342, 22);
            this.mnuNotepad.Tag = "Notepad";
            this.mnuNotepad.Text = "Notepad";
            this.mnuNotepad.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuCalculator
            // 
            this.mnuCalculator.Image = ((System.Drawing.Image)(resources.GetObject("mnuCalculator.Image")));
            this.mnuCalculator.Name = "mnuCalculator";
            this.mnuCalculator.Size = new System.Drawing.Size(342, 22);
            this.mnuCalculator.Tag = "Calculator";
            this.mnuCalculator.Text = "Calculator";
            this.mnuCalculator.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuPaint
            // 
            this.mnuPaint.Image = ((System.Drawing.Image)(resources.GetObject("mnuPaint.Image")));
            this.mnuPaint.Name = "mnuPaint";
            this.mnuPaint.Size = new System.Drawing.Size(342, 22);
            this.mnuPaint.Tag = "Paint";
            this.mnuPaint.Text = "Paint";
            this.mnuPaint.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // toolStripMenuItem18
            // 
            this.toolStripMenuItem18.Name = "toolStripMenuItem18";
            this.toolStripMenuItem18.Size = new System.Drawing.Size(339, 6);
            // 
            // mnuAsciiTable
            // 
            this.mnuAsciiTable.Image = ((System.Drawing.Image)(resources.GetObject("mnuAsciiTable.Image")));
            this.mnuAsciiTable.Name = "mnuAsciiTable";
            this.mnuAsciiTable.Size = new System.Drawing.Size(342, 22);
            this.mnuAsciiTable.Text = "Ascii Table";
            this.mnuAsciiTable.Click += new System.EventHandler(this.mnuAsciiTable_Click);
            // 
            // mnuBlobViewer
            // 
            this.mnuBlobViewer.Image = ((System.Drawing.Image)(resources.GetObject("mnuBlobViewer.Image")));
            this.mnuBlobViewer.Name = "mnuBlobViewer";
            this.mnuBlobViewer.Size = new System.Drawing.Size(342, 22);
            this.mnuBlobViewer.Text = "Blob/Hex Viewer";
            this.mnuBlobViewer.Click += new System.EventHandler(this.mnuBlobViewer_Click);
            // 
            // mnuFileSplitter
            // 
            this.mnuFileSplitter.Image = ((System.Drawing.Image)(resources.GetObject("mnuFileSplitter.Image")));
            this.mnuFileSplitter.Name = "mnuFileSplitter";
            this.mnuFileSplitter.Size = new System.Drawing.Size(342, 22);
            this.mnuFileSplitter.Text = "Large Text File Splitter";
            this.mnuFileSplitter.Click += new System.EventHandler(this.mnuFileSplitter_Click);
            // 
            // mnuColorConverter
            // 
            this.mnuColorConverter.Image = ((System.Drawing.Image)(resources.GetObject("mnuColorConverter.Image")));
            this.mnuColorConverter.Name = "mnuColorConverter";
            this.mnuColorConverter.Size = new System.Drawing.Size(342, 22);
            this.mnuColorConverter.Text = "Color Converter";
            this.mnuColorConverter.Click += new System.EventHandler(this.mnuColorPicker_Click);
            // 
            // toolStripMenuItem17
            // 
            this.toolStripMenuItem17.Name = "toolStripMenuItem17";
            this.toolStripMenuItem17.Size = new System.Drawing.Size(339, 6);
            // 
            // mnuOpenMethod
            // 
            this.mnuOpenMethod.Enabled = false;
            this.mnuOpenMethod.Name = "mnuOpenMethod";
            this.mnuOpenMethod.Size = new System.Drawing.Size(342, 22);
            this.mnuOpenMethod.Text = "Open the following Path with Windows Explorer";
            // 
            // mnuDesktop
            // 
            this.mnuDesktop.Image = ((System.Drawing.Image)(resources.GetObject("mnuDesktop.Image")));
            this.mnuDesktop.Name = "mnuDesktop";
            this.mnuDesktop.Size = new System.Drawing.Size(342, 22);
            this.mnuDesktop.Tag = "Desktop";
            this.mnuDesktop.Text = "Desktop";
            this.mnuDesktop.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuTemporary
            // 
            this.mnuTemporary.Image = ((System.Drawing.Image)(resources.GetObject("mnuTemporary.Image")));
            this.mnuTemporary.Name = "mnuTemporary";
            this.mnuTemporary.Size = new System.Drawing.Size(342, 22);
            this.mnuTemporary.Tag = "Temporary";
            this.mnuTemporary.Text = "Temporary";
            this.mnuTemporary.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuStartupAllUser
            // 
            this.mnuStartupAllUser.Image = ((System.Drawing.Image)(resources.GetObject("mnuStartupAllUser.Image")));
            this.mnuStartupAllUser.Name = "mnuStartupAllUser";
            this.mnuStartupAllUser.Size = new System.Drawing.Size(342, 22);
            this.mnuStartupAllUser.Tag = "StartupAllUser";
            this.mnuStartupAllUser.Text = "Startup (All User)";
            this.mnuStartupAllUser.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuStartupPersonal
            // 
            this.mnuStartupPersonal.Image = ((System.Drawing.Image)(resources.GetObject("mnuStartupPersonal.Image")));
            this.mnuStartupPersonal.Name = "mnuStartupPersonal";
            this.mnuStartupPersonal.Size = new System.Drawing.Size(342, 22);
            this.mnuStartupPersonal.Tag = "StartupPersonal";
            this.mnuStartupPersonal.Text = "Startup (Personal)";
            this.mnuStartupPersonal.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuJasonQueryLocated
            // 
            this.mnuJasonQueryLocated.Image = ((System.Drawing.Image)(resources.GetObject("mnuJasonQueryLocated.Image")));
            this.mnuJasonQueryLocated.Name = "mnuJasonQueryLocated";
            this.mnuJasonQueryLocated.Size = new System.Drawing.Size(342, 22);
            this.mnuJasonQueryLocated.Tag = "JasonQueryLocated";
            this.mnuJasonQueryLocated.Text = "The Path Where JasonQuery.exe is Located";
            this.mnuJasonQueryLocated.Click += new System.EventHandler(this.Assistant_Click);
            // 
            // mnuTools
            // 
            this.mnuTools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuOptions,
            this.toolStripMenuItem1,
            this.mnuSqlHistory,
            this.mnuSchemaBrowser,
            this.mnuGenerateSqlStatement,
            this.mnuSchemaSearch,
            this.toolStripMenuItem14,
            this.mnuCreateTable,
            this.mnuImportTableData,
            this.toolStripMenuItem15,
            this.mnuCompactMDB});
            this.mnuTools.Name = "mnuTools";
            this.mnuTools.Size = new System.Drawing.Size(50, 20);
            this.mnuTools.Text = "&Tools";
            // 
            // mnuOptions
            // 
            this.mnuOptions.Image = ((System.Drawing.Image)(resources.GetObject("mnuOptions.Image")));
            this.mnuOptions.Name = "mnuOptions";
            this.mnuOptions.Size = new System.Drawing.Size(233, 22);
            this.mnuOptions.Text = "Options";
            this.mnuOptions.Click += new System.EventHandler(this.mnuOptions_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(230, 6);
            // 
            // mnuSqlHistory
            // 
            this.mnuSqlHistory.Image = ((System.Drawing.Image)(resources.GetObject("mnuSqlHistory.Image")));
            this.mnuSqlHistory.Name = "mnuSqlHistory";
            this.mnuSqlHistory.Size = new System.Drawing.Size(233, 22);
            this.mnuSqlHistory.Text = "SQL History";
            this.mnuSqlHistory.Click += new System.EventHandler(this.mnuSqlHistory_Click);
            // 
            // mnuSchemaBrowser
            // 
            this.mnuSchemaBrowser.Image = ((System.Drawing.Image)(resources.GetObject("mnuSchemaBrowser.Image")));
            this.mnuSchemaBrowser.Name = "mnuSchemaBrowser";
            this.mnuSchemaBrowser.Size = new System.Drawing.Size(233, 22);
            this.mnuSchemaBrowser.Text = "Schema Browser";
            this.mnuSchemaBrowser.Click += new System.EventHandler(this.mnuSchemaBrowser_Click);
            // 
            // mnuGenerateSqlStatement
            // 
            this.mnuGenerateSqlStatement.Image = ((System.Drawing.Image)(resources.GetObject("mnuGenerateSqlStatement.Image")));
            this.mnuGenerateSqlStatement.Name = "mnuGenerateSqlStatement";
            this.mnuGenerateSqlStatement.Size = new System.Drawing.Size(233, 22);
            this.mnuGenerateSqlStatement.Text = "Generate SQL Statement";
            this.mnuGenerateSqlStatement.Click += new System.EventHandler(this.mnuGenerateSqlStatement_Click);
            // 
            // mnuSchemaSearch
            // 
            this.mnuSchemaSearch.Enabled = false;
            this.mnuSchemaSearch.Image = ((System.Drawing.Image)(resources.GetObject("mnuSchemaSearch.Image")));
            this.mnuSchemaSearch.Name = "mnuSchemaSearch";
            this.mnuSchemaSearch.Size = new System.Drawing.Size(233, 22);
            this.mnuSchemaSearch.Text = "Advanced Search in Schema";
            this.mnuSchemaSearch.Visible = false;
            // 
            // toolStripMenuItem14
            // 
            this.toolStripMenuItem14.Name = "toolStripMenuItem14";
            this.toolStripMenuItem14.Size = new System.Drawing.Size(230, 6);
            // 
            // mnuCreateTable
            // 
            this.mnuCreateTable.Enabled = false;
            this.mnuCreateTable.Image = ((System.Drawing.Image)(resources.GetObject("mnuCreateTable.Image")));
            this.mnuCreateTable.Name = "mnuCreateTable";
            this.mnuCreateTable.Size = new System.Drawing.Size(233, 22);
            this.mnuCreateTable.Text = "Create Table";
            this.mnuCreateTable.Click += new System.EventHandler(this.mnuCreateTable_Click);
            // 
            // mnuImportTableData
            // 
            this.mnuImportTableData.Enabled = false;
            this.mnuImportTableData.Image = ((System.Drawing.Image)(resources.GetObject("mnuImportTableData.Image")));
            this.mnuImportTableData.Name = "mnuImportTableData";
            this.mnuImportTableData.Size = new System.Drawing.Size(233, 22);
            this.mnuImportTableData.Text = "Import Table Data";
            this.mnuImportTableData.Visible = false;
            // 
            // toolStripMenuItem15
            // 
            this.toolStripMenuItem15.Name = "toolStripMenuItem15";
            this.toolStripMenuItem15.Size = new System.Drawing.Size(230, 6);
            this.toolStripMenuItem15.Visible = false;
            // 
            // mnuCompactMDB
            // 
            this.mnuCompactMDB.Enabled = false;
            this.mnuCompactMDB.Name = "mnuCompactMDB";
            this.mnuCompactMDB.Size = new System.Drawing.Size(233, 22);
            this.mnuCompactMDB.Text = "Compact \'JasonQuery.db\'";
            this.mnuCompactMDB.Visible = false;
            this.mnuCompactMDB.Click += new System.EventHandler(this.mnuCompactMDB_Click);
            // 
            // mnuHelp
            // 
            this.mnuHelp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCheckForUpdatesManually,
            this.mnuUpdateNow,
            this.mnuReleaseNotes,
            this.mnuReportBugs,
            this.mnuAbout});
            this.mnuHelp.Name = "mnuHelp";
            this.mnuHelp.Size = new System.Drawing.Size(46, 20);
            this.mnuHelp.Text = "&Help";
            // 
            // mnuCheckForUpdatesManually
            // 
            this.mnuCheckForUpdatesManually.Image = ((System.Drawing.Image)(resources.GetObject("mnuCheckForUpdatesManually.Image")));
            this.mnuCheckForUpdatesManually.Name = "mnuCheckForUpdatesManually";
            this.mnuCheckForUpdatesManually.Size = new System.Drawing.Size(257, 22);
            this.mnuCheckForUpdatesManually.Text = "Check for updates";
            this.mnuCheckForUpdatesManually.Click += new System.EventHandler(this.mnuCheckForUpdatesManually_Click);
            // 
            // mnuUpdateNow
            // 
            this.mnuUpdateNow.Image = ((System.Drawing.Image)(resources.GetObject("mnuUpdateNow.Image")));
            this.mnuUpdateNow.Name = "mnuUpdateNow";
            this.mnuUpdateNow.Size = new System.Drawing.Size(257, 22);
            this.mnuUpdateNow.Text = "Update Now (One-Click update)";
            this.mnuUpdateNow.Click += new System.EventHandler(this.mnuUpdateNow_Click);
            // 
            // mnuReleaseNotes
            // 
            this.mnuReleaseNotes.Image = ((System.Drawing.Image)(resources.GetObject("mnuReleaseNotes.Image")));
            this.mnuReleaseNotes.Name = "mnuReleaseNotes";
            this.mnuReleaseNotes.Size = new System.Drawing.Size(257, 22);
            this.mnuReleaseNotes.Text = "Release Notes";
            this.mnuReleaseNotes.ToolTipText = "Open the \"releasenotes.html\" with your default browser";
            this.mnuReleaseNotes.Click += new System.EventHandler(this.mnuReleaseNotes_Click);
            // 
            // mnuReportBugs
            // 
            this.mnuReportBugs.Image = ((System.Drawing.Image)(resources.GetObject("mnuReportBugs.Image")));
            this.mnuReportBugs.Name = "mnuReportBugs";
            this.mnuReportBugs.Size = new System.Drawing.Size(257, 22);
            this.mnuReportBugs.Text = "Report Bugs";
            this.mnuReportBugs.ToolTipText = "Open the \"reportbugs.html\" with your default browser";
            this.mnuReportBugs.Click += new System.EventHandler(this.mnuReportBugs_Click);
            // 
            // mnuAbout
            // 
            this.mnuAbout.Image = ((System.Drawing.Image)(resources.GetObject("mnuAbout.Image")));
            this.mnuAbout.Name = "mnuAbout";
            this.mnuAbout.Size = new System.Drawing.Size(257, 22);
            this.mnuAbout.Text = "About";
            this.mnuAbout.Click += new System.EventHandler(this.mnuAbout_Click);
            // 
            // mnuInfo
            // 
            this.mnuInfo.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.mnuInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.mnuInfo.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuInfo.Enabled = false;
            this.mnuInfo.ForeColor = System.Drawing.SystemColors.ControlText;
            this.mnuInfo.Name = "mnuInfo";
            this.mnuInfo.Size = new System.Drawing.Size(12, 20);
            // 
            // tmrTimeAndKeyStatus
            // 
            this.tmrTimeAndKeyStatus.Enabled = true;
            this.tmrTimeAndKeyStatus.Tick += new System.EventHandler(this.tmrTimeAndKeyStatus_Tick);
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(393, 212);
            this.panel1.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.panel1, "(default)");
            // 
            // btnConnect
            // 
            this.btnConnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnConnect.Image = ((System.Drawing.Image)(resources.GetObject("btnConnect.Image")));
            this.btnConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(28, 28);
            this.btnConnect.Text = "toolStripButton2";
            this.btnConnect.ToolTipText = "New Connection";
            // 
            // btnDisconnect
            // 
            this.btnDisconnect.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDisconnect.Image = ((System.Drawing.Image)(resources.GetObject("btnDisconnect.Image")));
            this.btnDisconnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDisconnect.Name = "btnDisconnect";
            this.btnDisconnect.Size = new System.Drawing.Size(28, 28);
            this.btnDisconnect.Text = "toolStripButton1";
            this.btnDisconnect.ToolTipText = "End Connection";
            // 
            // btnNewSqlEditor
            // 
            this.btnNewSqlEditor.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNewSqlEditor.Image = ((System.Drawing.Image)(resources.GetObject("btnNewSqlEditor.Image")));
            this.btnNewSqlEditor.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNewSqlEditor.Name = "btnNewSqlEditor";
            this.btnNewSqlEditor.Size = new System.Drawing.Size(28, 28);
            this.btnNewSqlEditor.Tag = "Open New SQL Editor";
            this.btnNewSqlEditor.ToolTipText = "Open New SQL Editor";
            this.btnNewSqlEditor.Click += new System.EventHandler(this.btnNewSqlEditor_Click);
            // 
            // btnSetting
            // 
            this.btnSetting.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetting.Image = ((System.Drawing.Image)(resources.GetObject("btnSetting.Image")));
            this.btnSetting.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetting.Name = "btnSetting";
            this.btnSetting.Size = new System.Drawing.Size(28, 28);
            this.btnSetting.Tag = "Open Setting Form";
            this.btnSetting.ToolTipText = "Open Setting Form";
            // 
            // tsMainMenuToolBar
            // 
            this.tsMainMenuToolBar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tsMainMenuToolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnConnect,
            this.btnDisconnect,
            this.btnNewSqlEditor,
            this.btnSetting});
            this.tsMainMenuToolBar.Location = new System.Drawing.Point(0, 26);
            this.tsMainMenuToolBar.Name = "tsMainMenuToolBar";
            this.tsMainMenuToolBar.Size = new System.Drawing.Size(1008, 31);
            this.tsMainMenuToolBar.TabIndex = 13;
            this.tsMainMenuToolBar.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.tsMainMenuToolBar, "(default)");
            this.tsMainMenuToolBar.Visible = false;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(9, 7);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(200, 100);
            this.panel2.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.panel2, "(default)");
            // 
            // panel3
            // 
            this.panel3.Location = new System.Drawing.Point(17, 76);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(472, 335);
            this.panel3.TabIndex = 15;
            this.c1ThemeController1.SetTheme(this.panel3, "(default)");
            // 
            // groupBox1
            // 
            this.groupBox1.Location = new System.Drawing.Point(330, 193);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(200, 100);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "groupBox1";
            this.c1ThemeController1.SetTheme(this.groupBox1, "(default)");
            // 
            // c1StatusBar1
            // 
            this.c1StatusBar1.LeftPaneItems.Add(this.ribbonLabel1);
            this.c1StatusBar1.LeftPaneItems.Add(this.lblInfo);
            this.c1StatusBar1.Location = new System.Drawing.Point(0, 707);
            this.c1StatusBar1.Name = "c1StatusBar1";
            this.c1StatusBar1.RightPaneItems.Add(this.lblDBVersion);
            this.c1StatusBar1.RightPaneItems.Add(this.spDBVersion);
            this.c1StatusBar1.RightPaneItems.Add(this.btnDatabase);
            this.c1StatusBar1.RightPaneItems.Add(this.spDatabase);
            this.c1StatusBar1.RightPaneItems.Add(this.lblNotCommitYet);
            this.c1StatusBar1.RightPaneItems.Add(this.lblNotCommitYetTime);
            this.c1StatusBar1.RightPaneItems.Add(this.spNotCommitYet);
            this.c1StatusBar1.RightPaneItems.Add(this.btnIP);
            this.c1StatusBar1.RightPaneItems.Add(this.spDomainUser);
            this.c1StatusBar1.RightPaneItems.Add(this.lblDomainUser);
            this.c1StatusBar1.RightPaneItems.Add(this.spAutoRollbackOnError);
            this.c1StatusBar1.RightPaneItems.Add(this.btnAutoRollbackOnErrorOn);
            this.c1StatusBar1.RightPaneItems.Add(this.btnAutoRollbackOnErrorOff);
            this.c1StatusBar1.RightPaneItems.Add(this.spAutoCommit);
            this.c1StatusBar1.RightPaneItems.Add(this.lblAutoCommit);
            this.c1StatusBar1.Size = new System.Drawing.Size(1008, 22);
            this.c1ThemeController1.SetTheme(this.c1StatusBar1, "(default)");
            this.c1StatusBar1.VisualStyle = C1.Win.C1Ribbon.VisualStyle.Office2010Silver;
            // 
            // ribbonLabel1
            // 
            this.ribbonLabel1.Name = "ribbonLabel1";
            this.ribbonLabel1.Text = "      ";
            // 
            // lblInfo
            // 
            this.lblInfo.Name = "lblInfo";
            // 
            // lblDBVersion
            // 
            this.lblDBVersion.Name = "lblDBVersion";
            // 
            // spDBVersion
            // 
            this.spDBVersion.Name = "spDBVersion";
            this.spDBVersion.Visible = false;
            // 
            // btnDatabase
            // 
            this.btnDatabase.Name = "btnDatabase";
            this.btnDatabase.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnDatabase.SmallImage")));
            this.btnDatabase.Visible = false;
            // 
            // spDatabase
            // 
            this.spDatabase.Name = "spDatabase";
            this.spDatabase.Visible = false;
            // 
            // lblNotCommitYet
            // 
            this.lblNotCommitYet.Name = "lblNotCommitYet";
            this.lblNotCommitYet.SmallImage = ((System.Drawing.Image)(resources.GetObject("lblNotCommitYet.SmallImage")));
            this.lblNotCommitYet.Text = "标签";
            this.lblNotCommitYet.Visible = false;
            // 
            // lblNotCommitYetTime
            // 
            this.lblNotCommitYetTime.Name = "lblNotCommitYetTime";
            this.lblNotCommitYetTime.Text = "标签";
            this.lblNotCommitYetTime.Visible = false;
            // 
            // spNotCommitYet
            // 
            this.spNotCommitYet.Name = "spNotCommitYet";
            this.spNotCommitYet.Visible = false;
            // 
            // btnIP
            // 
            this.btnIP.Name = "btnIP";
            this.btnIP.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnIP.SmallImage")));
            // 
            // spDomainUser
            // 
            this.spDomainUser.Name = "spDomainUser";
            // 
            // lblDomainUser
            // 
            this.lblDomainUser.Name = "lblDomainUser";
            this.lblDomainUser.Text = "DomainUser";
            // 
            // spAutoRollbackOnError
            // 
            this.spAutoRollbackOnError.Name = "spAutoRollbackOnError";
            this.spAutoRollbackOnError.Visible = false;
            // 
            // btnAutoRollbackOnErrorOn
            // 
            this.btnAutoRollbackOnErrorOn.Name = "btnAutoRollbackOnErrorOn";
            this.btnAutoRollbackOnErrorOn.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnAutoRollbackOnErrorOn.SmallImage")));
            this.btnAutoRollbackOnErrorOn.Visible = false;
            this.btnAutoRollbackOnErrorOn.Click += new System.EventHandler(this.btnAutoRollbackOnError_Click);
            // 
            // btnAutoRollbackOnErrorOff
            // 
            this.btnAutoRollbackOnErrorOff.Name = "btnAutoRollbackOnErrorOff";
            this.btnAutoRollbackOnErrorOff.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnAutoRollbackOnErrorOff.SmallImage")));
            this.btnAutoRollbackOnErrorOff.Visible = false;
            this.btnAutoRollbackOnErrorOff.Click += new System.EventHandler(this.btnAutoRollbackOnError_Click);
            // 
            // spAutoCommit
            // 
            this.spAutoCommit.Name = "spAutoCommit";
            // 
            // lblAutoCommit
            // 
            this.lblAutoCommit.Name = "lblAutoCommit";
            this.lblAutoCommit.Text = "Auto Commit is off";
            // 
            // pnlArrow
            // 
            this.pnlArrow.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlArrow.Controls.Add(this.lblPrompt4NewConnection);
            this.pnlArrow.Controls.Add(this.picArrow);
            this.pnlArrow.Location = new System.Drawing.Point(0, 26);
            this.pnlArrow.Name = "pnlArrow";
            this.pnlArrow.Size = new System.Drawing.Size(1008, 680);
            this.pnlArrow.TabIndex = 117;
            this.c1ThemeController1.SetTheme(this.pnlArrow, "(default)");
            // 
            // lblPrompt4NewConnection
            // 
            this.lblPrompt4NewConnection.AutoSize = true;
            this.lblPrompt4NewConnection.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPrompt4NewConnection.Location = new System.Drawing.Point(5, 63);
            this.lblPrompt4NewConnection.Name = "lblPrompt4NewConnection";
            this.lblPrompt4NewConnection.Size = new System.Drawing.Size(458, 16);
            this.lblPrompt4NewConnection.TabIndex = 116;
            this.lblPrompt4NewConnection.Text = "You can create or select database connection from [File]>[New Connection]!";
            this.lblPrompt4NewConnection.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPrompt4NewConnection.Visible = false;
            // 
            // picArrow
            // 
            this.picArrow.Image = ((System.Drawing.Image)(resources.GetObject("picArrow.Image")));
            this.picArrow.Location = new System.Drawing.Point(27, 3);
            this.picArrow.Name = "picArrow";
            this.picArrow.Size = new System.Drawing.Size(15, 60);
            this.picArrow.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picArrow.TabIndex = 0;
            this.picArrow.TabStop = false;
            this.picArrow.Visible = false;
            // 
            // tmrPendingTransactionIdleCheck
            // 
            this.tmrPendingTransactionIdleCheck.Tick += new System.EventHandler(this.tmrPendingTransactionIdleCheck_Tick);
            // 
            // tabControl1
            // 
            this.tabControl1.AllowDrop = false;
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.BoldSelectedPage = true;
            this.tabControl1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabControl1.ForeColor = System.Drawing.Color.Empty;
            this.tabControl1.IDEPixelArea = false;
            this.tabControl1.Location = new System.Drawing.Point(0, 26);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl1.MouseLeaveTimeout = 100;
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.PositionTop = true;
            this.tabControl1.ShowArrows = true;
            this.tabControl1.Size = new System.Drawing.Size(1008, 677);
            this.tabControl1.TabIndex = 8;
            this.tabControl1.TextColor = System.Drawing.Color.Empty;
            this.tabControl1.TextInactiveColor = System.Drawing.Color.Silver;
            this.tabControl1.Visible = false;
            this.tabControl1.ClosePressed += new System.EventHandler(this.tabControl1_ClosePressed);
            this.tabControl1.SelectionChanged += new System.EventHandler(this.tabControl1_SelectionChanged);
            this.tabControl1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseDoubleClick);
            this.tabControl1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseDown);
            this.tabControl1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tabControl1_MouseMove);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.ClientSize = new System.Drawing.Size(1008, 729);
            this.Controls.Add(this.pnlArrow);
            this.Controls.Add(this.c1StatusBar1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.tsMainMenuToolBar);
            this.Controls.Add(this.mnuMainForm);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(800, 600);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Tag = "JasonQuery";
            this.Text = "JasonQuery";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.ResizeBegin += new System.EventHandler(this.MainForm_ResizeBegin);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.SizeChanged += new System.EventHandler(this.MainForm_SizeChanged);
            this.mnuMainForm.ResumeLayout(false);
            this.mnuMainForm.PerformLayout();
            this.tsMainMenuToolBar.ResumeLayout(false);
            this.tsMainMenuToolBar.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).EndInit();
            this.pnlArrow.ResumeLayout(false);
            this.pnlArrow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picArrow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip mnuMainForm;
        private System.Windows.Forms.ToolStripMenuItem mnuFile;
        private System.Windows.Forms.ToolStripMenuItem mnuNewConnection;
        private System.Windows.Forms.Timer tmrTimeAndKeyStatus;
        private System.Windows.Forms.ToolStripMenuItem mnuHelp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripButton btnConnect;
        private System.Windows.Forms.ToolStripButton btnDisconnect;
        private System.Windows.Forms.ToolStripButton btnNewSqlEditor;
        private System.Windows.Forms.ToolStripButton btnSetting;
        private System.Windows.Forms.ToolStrip tsMainMenuToolBar;
        private System.Windows.Forms.Panel panel2;
        private Crownwood.Magic.Controls.TabControl tabControl1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ToolStripMenuItem mnuTools;
        private System.Windows.Forms.ToolStripMenuItem mnuOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuSqlHistory;
        private System.Windows.Forms.ToolStripMenuItem mnuSchemaBrowser;
        private System.Windows.Forms.ToolStripMenuItem mnuCheckForUpdatesManually;
        private System.Windows.Forms.ToolStripMenuItem mnuAbout;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery;
        private System.Windows.Forms.ToolStripMenuItem mnuNewSqlEditor2;
        private System.Windows.Forms.ToolStripMenuItem mnuRecentFiles;
        private System.Windows.Forms.ToolStripMenuItem mnuMyFavorite;
        private System.Windows.Forms.ToolStripMenuItem mnuCompactMDB;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem mnuNewSqlEditor;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenFiles;
        private System.Windows.Forms.ToolStripMenuItem mnuFile_Save;
        private System.Windows.Forms.ToolStripMenuItem mnuFile_SaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem9;
        private System.Windows.Forms.ToolStripMenuItem mnuCloseConnection;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_Execute;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_ExecuteCurrentBlock;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem11;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_Explain;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_ExplainAnalyze;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_ExplainOptions;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem12;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoRollback;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoCommit;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem13;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_Commit;
        private System.Windows.Forms.ToolStripMenuItem mnuQuery_Rollback;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private System.Windows.Forms.Timer tmrPendingTransactionIdleCheck;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenConnection;
        private System.Windows.Forms.ToolStripMenuItem mnuReleaseNotes;
        private System.Windows.Forms.ToolStripMenuItem mnuInfo;
        private System.Windows.Forms.ToolStripMenuItem mnuReportBugs;
        private C1.Win.C1Ribbon.RibbonLabel lblAutoCommit;
        private C1.Win.C1Ribbon.RibbonSeparator spAutoCommit;
        private C1.Win.C1Ribbon.RibbonSeparator spAutoRollbackOnError;
        private C1.Win.C1Ribbon.RibbonLabel lblDomainUser;
        private C1.Win.C1Ribbon.RibbonSeparator spDomainUser;
        private C1.Win.C1Ribbon.RibbonLabel lblInfo;
        private C1.Win.C1Ribbon.RibbonLabel ribbonLabel1;
        private C1.Win.C1Ribbon.C1StatusBar c1StatusBar1;
        private C1.Win.C1Ribbon.RibbonLabel lblDBVersion;
        private C1.Win.C1Ribbon.RibbonSeparator spDBVersion;
        private C1.Win.C1Ribbon.RibbonButton btnIP;
        private C1.Win.C1Ribbon.RibbonButton btnDatabase;
        private C1.Win.C1Ribbon.RibbonSeparator spDatabase;
        private System.Windows.Forms.ToolStripMenuItem mnuSchemaSearch;
        private C1.Win.C1Ribbon.RibbonButton btnAutoRollbackOnErrorOn;
        private C1.Win.C1Ribbon.RibbonButton btnAutoRollbackOnErrorOff;
        private System.Windows.Forms.ToolStripMenuItem mnuSwitchDatabase;
        private System.Windows.Forms.ToolStripMenuItem mnuGenerateSqlStatement;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem15;
        private System.Windows.Forms.ToolStripMenuItem mnuAssistant;
        private System.Windows.Forms.ToolStripMenuItem mnuOpenMethod;
        private System.Windows.Forms.ToolStripMenuItem mnuDesktop;
        private System.Windows.Forms.ToolStripMenuItem mnuTemporary;
        private System.Windows.Forms.ToolStripMenuItem mnuStartupAllUser;
        private System.Windows.Forms.ToolStripMenuItem mnuStartupPersonal;
        private System.Windows.Forms.ToolStripMenuItem mnuJasonQueryLocated;
        private System.Windows.Forms.ToolStripMenuItem mnuWindowsExplorer;
        private System.Windows.Forms.ToolStripMenuItem mnuNotepad;
        private System.Windows.Forms.ToolStripMenuItem mnuCalculator;
        private System.Windows.Forms.ToolStripMenuItem mnuPaint;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem17;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem18;
        private System.Windows.Forms.ToolStripMenuItem mnuAsciiTable;
        private System.Windows.Forms.ToolStripMenuItem mnuFileSplitter;
        private System.Windows.Forms.ToolStripMenuItem mnuBlobViewer;
        private System.Windows.Forms.ToolStripMenuItem mnuColorConverter;
        private System.Windows.Forms.ToolStripMenuItem mnuCreateTable;
        private System.Windows.Forms.ToolStripMenuItem mnuUpdateNow;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem14;
        private System.Windows.Forms.ToolStripMenuItem mnuDatabaseConnectionViewMode;
        private System.Windows.Forms.ToolStripMenuItem mnuImportTableData;
        private System.Windows.Forms.Panel pnlArrow;
        private System.Windows.Forms.PictureBox picArrow;
        private System.Windows.Forms.Label lblPrompt4NewConnection;
        private C1.Win.C1Ribbon.RibbonSeparator spNotCommitYet;
        private C1.Win.C1Ribbon.RibbonLabel lblNotCommitYet;
        private C1.Win.C1Ribbon.RibbonLabel lblNotCommitYetTime;
    }
}