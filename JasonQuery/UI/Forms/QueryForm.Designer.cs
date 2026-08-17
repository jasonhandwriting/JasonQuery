using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    partial class QueryForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(QueryForm));
            this.c1XLBook1 = new C1.C1Excel.C1XLBook();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.c1DockingTab2 = new C1.Win.C1Command.C1DockingTab();
            this.tabAutoReplace = new C1.Win.C1Command.C1DockingTabPage();
            this.lblAutoReplacePosition = new System.Windows.Forms.Label();
            this.btnHelp_AutoReplace = new C1.Win.C1Input.C1Button();
            this.c1GridAutoReplaceInfo = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsAutoReplace = new System.Windows.Forms.ToolStrip();
            this.lblAutoReplace = new System.Windows.Forms.ToolStripLabel();
            this.tabSchemaInformation = new C1.Win.C1Command.C1DockingTabPage();
            this.lblSchemaFilterPosition = new System.Windows.Forms.Label();
            this.txtSchemaFilter = new C1.Win.C1Input.C1TextBox();
            this.tsSchemaBrowser = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.btnExpandCollapse = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSettingOfFocus = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuFocusOnDataGrid = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuFocusOnQueryEditor = new System.Windows.Forms.ToolStripMenuItem();
            this.btnHelp_SchemaFilter = new System.Windows.Forms.ToolStripButton();
            this.lblSchemaFilter = new System.Windows.Forms.ToolStripLabel();
            this.c1GridSchemaBrowser = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tabTabList = new C1.Win.C1Command.C1DockingTabPage();
            this.c1GridTabList = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tabSqlNavigator = new C1.Win.C1Command.C1DockingTabPage();
            this.c1GridSqlNavigator = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.c1GridAutoCompleteForAll = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.nudQueryTimeout = new System.Windows.Forms.NumericUpDown();
            this.editor = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.lblQueryTimeoutPosition = new System.Windows.Forms.Label();
            this.txtIndentWord = new C1.Win.C1Input.C1TextBox();
            this.btnHelp_QueryTimeout = new C1.Win.C1Input.C1Button();
            this.tsEditor = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveRed = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnQuery = new System.Windows.Forms.ToolStripButton();
            this.btnExecuteCurrentBlock = new System.Windows.Forms.ToolStripButton();
            this.btnExecuteCurrentLine = new System.Windows.Forms.ToolStripButton();
            this.btnCancelQuery = new System.Windows.Forms.ToolStripButton();
            this.btnCommit = new System.Windows.Forms.ToolStripButton();
            this.btnRollback = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSelectCurrentBlock = new System.Windows.Forms.ToolStripButton();
            this.btnSelectCurrentLine = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveTrailingBlanks = new System.Windows.Forms.ToolStripButton();
            this.btnCode2Sql = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuCSharp2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVB2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelphi2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSql2Code = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuSql2CSharp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle4 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSql2VBNet = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVBNetStyle1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVBNetStyle2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVBNetStyle3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSql2VB6A = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVB6AStyle1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVB6AStyle2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSql2Delphi = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelphi6Style1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDelphi6Style2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.btnLeftAndRight = new System.Windows.Forms.ToolStripButton();
            this.btnUpAndDown = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnComment = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveComment = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnIndent = new System.Windows.Forms.ToolStripButton();
            this.lblIndentWord = new System.Windows.Forms.ToolStripLabel();
            this.txtIndentWord2 = new System.Windows.Forms.ToolStripTextBox();
            this.btnUnIndent = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.btnHighlightSelection = new System.Windows.Forms.ToolStripButton();
            this.btnHighlightSelection2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnWordWrap = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2 = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2 = new System.Windows.Forms.ToolStripButton();
            this.btnShowIndentGuide = new System.Windows.Forms.ToolStripButton();
            this.btnShowIndentGuide2 = new System.Windows.Forms.ToolStripButton();
            this.tsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.lblQueryTimeout = new System.Windows.Forms.ToolStripLabel();
            this.lblInfoEditor = new System.Windows.Forms.Label();
            this.c1StatusBar2 = new C1.Win.C1Ribbon.C1StatusBar();
            this.btnDatabase = new C1.Win.C1Ribbon.RibbonButton();
            this.spDatabase = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblEditorLength = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonLabel2 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblEditorLines = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator1 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblEditorLn = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonLabel3 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblEditorCol = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonLabel4 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblEditorPos = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonLabel5 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblEditorSel = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator7 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblEndOfLineStyle = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator8 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblEncode = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblTemp = new C1.Win.C1Ribbon.RibbonLabel();
            this.chkShowFilterRow = new C1.Win.C1Input.C1CheckBox();
            this.chkSize = new C1.Win.C1Input.C1CheckBox();
            this.chkShowColumnComments = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_RawDataMode = new C1.Win.C1Input.C1Button();
            this.chkRawDataMode = new C1.Win.C1Input.C1CheckBox();
            this.chkShowColumnType = new C1.Win.C1Input.C1CheckBox();
            this.c1DockingTab1 = new C1.Win.C1Command.C1DockingTab();
            this.tabMessage = new C1.Win.C1Command.C1DockingTabPage();
            this.pnlEditorMessageWelcome = new System.Windows.Forms.Panel();
            this.lblWelcome44 = new System.Windows.Forms.Label();
            this.lblWelcomeCountdown = new System.Windows.Forms.Label();
            this.lblWelcome223 = new System.Windows.Forms.Label();
            this.lblWelcome222 = new System.Windows.Forms.Label();
            this.lblWelcome221 = new System.Windows.Forms.Label();
            this.lblWelcome = new System.Windows.Forms.Label();
            this.BlankLabel = new System.Windows.Forms.Label();
            this.lblWelcome43 = new System.Windows.Forms.Label();
            this.lblWelcome42 = new System.Windows.Forms.Label();
            this.lblWelcome41 = new System.Windows.Forms.Label();
            this.lblWelcome4 = new System.Windows.Forms.Label();
            this.lblWelcome33 = new System.Windows.Forms.Label();
            this.lblWelcome23 = new System.Windows.Forms.Label();
            this.lblWelcome32 = new System.Windows.Forms.Label();
            this.lblWelcome31 = new System.Windows.Forms.Label();
            this.lblWelcome22 = new System.Windows.Forms.Label();
            this.lblWelcome21 = new System.Windows.Forms.Label();
            this.lblWelcome13 = new System.Windows.Forms.Label();
            this.lblWelcome12 = new System.Windows.Forms.Label();
            this.lblWelcome11 = new System.Windows.Forms.Label();
            this.lblWelcome3 = new System.Windows.Forms.Label();
            this.lblWelcome2 = new System.Windows.Forms.Label();
            this.lblWelcome1 = new System.Windows.Forms.Label();
            this.editorMessage = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.tabDataGrid = new C1.Win.C1Command.C1DockingTabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.lblColumnNamePosition = new System.Windows.Forms.Label();
            this.btnHelp_ColumnName = new C1.Win.C1Input.C1Button();
            this.lblColumnFilterPosition = new System.Windows.Forms.Label();
            this.txtColumnFilter = new C1.Win.C1Input.C1TextBox();
            this.c1GridColumns = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsColumnFilter = new System.Windows.Forms.ToolStrip();
            this.lblColumnFilter = new System.Windows.Forms.ToolStripLabel();
            this.c1TrueDBGrid1 = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.chkShowGroupingRow = new C1.Win.C1Input.C1CheckBox();
            this.cboFindGrid = new C1.Win.C1Input.C1ComboBox();
            this.tsDataGrid = new System.Windows.Forms.ToolStrip();
            this.btnExportToFile = new System.Windows.Forms.ToolStripButton();
            this.btnShowColumns = new System.Windows.Forms.ToolStripButton();
            this.btnAutoSort = new System.Windows.Forms.ToolStripButton();
            this.btnOptions = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuResultCopyQuotingWith = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyQuotingWithNone = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyQuotingWithDoubleQuoting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyQuotingWithSingleQuoting = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyFieldSeparator = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyFieldSeparatorComma = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyFieldSeparatorSemicolon = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuResultCopyFieldSeparatorI = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.lblSpace = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.lblFindGrid = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel6 = new System.Windows.Forms.ToolStripLabel();
            this.btnFindNextGrid = new System.Windows.Forms.ToolStripButton();
            this.btnFindPreviousGrid = new System.Windows.Forms.ToolStripButton();
            this.btnCountGrid = new System.Windows.Forms.ToolStripButton();
            this.btnHighlightAllGrid = new System.Windows.Forms.ToolStripButton();
            this.btnClearHighlightsGrid = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.lblResultCopyQuotingWith = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel7 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.lblResultCopyFieldSeparator = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel8 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.c1StatusBar1 = new C1.Win.C1Ribbon.C1StatusBar();
            this.lblAverage = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblAverageValue = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblSeparator1 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblCount = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblCountValue = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblSeparator2 = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblSummary = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblSummaryValue = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblSeparator3 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblExecTime = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator2 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblQueryTime = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator5 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblRows = new C1.Win.C1Ribbon.RibbonLabel();
            this.ribbonSeparator3 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.btnPaginationOn = new C1.Win.C1Ribbon.RibbonButton();
            this.btnPaginationOff = new C1.Win.C1Ribbon.RibbonButton();
            this.btnAppendingQueriesOn = new C1.Win.C1Ribbon.RibbonButton();
            this.btnAppendingQueriesOff = new C1.Win.C1Ribbon.RibbonButton();
            this.btnNextPage = new C1.Win.C1Ribbon.RibbonButton();
            this.c1GridAutoCompleteForSpace = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tmrlblInfo = new System.Windows.Forms.Timer(this.components);
            this.tmrMother2Child = new System.Windows.Forms.Timer(this.components);
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.lblInfo = new System.Windows.Forms.Label();
            this.c1GridAutoCompleteForPeriod = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tmrExecTime = new System.Windows.Forms.Timer(this.components);
            this.tmrlblInfoEditor = new System.Windows.Forms.Timer(this.components);
            this.tmrQueryTime = new System.Windows.Forms.Timer(this.components);
            this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
            this.tmrBackup = new System.Windows.Forms.Timer(this.components);
            this.tmrCheckIdleTime = new System.Windows.Forms.Timer(this.components);
            this.tmrHideMessageWelcome = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).BeginInit();
            this.c1DockingTab2.SuspendLayout();
            this.tabAutoReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoReplaceInfo)).BeginInit();
            this.tsAutoReplace.SuspendLayout();
            this.tabSchemaInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSchemaFilter)).BeginInit();
            this.tsSchemaBrowser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSchemaBrowser)).BeginInit();
            this.tabTabList.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridTabList)).BeginInit();
            this.tabSqlNavigator.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSqlNavigator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtIndentWord)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout)).BeginInit();
            this.tsEditor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnComments)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_RawDataMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRawDataMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).BeginInit();
            this.c1DockingTab1.SuspendLayout();
            this.tabMessage.SuspendLayout();
            this.pnlEditorMessageWelcome.SuspendLayout();
            this.tabDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridColumns)).BeginInit();
            this.tsColumnFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1TrueDBGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowGroupingRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFindGrid)).BeginInit();
            this.tsDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForSpace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1MinSize = 150;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer3);
            this.splitContainer1.Panel2MinSize = 500;
            this.splitContainer1.Size = new System.Drawing.Size(1159, 535);
            this.splitContainer1.SplitterDistance = 235;
            this.splitContainer1.SplitterWidth = 2;
            this.splitContainer1.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.splitContainer1, "(default)");
            this.splitContainer1.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer1_SplitterMoving);
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
            this.splitContainer1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.splitContainer1_KeyDown);
            this.splitContainer1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.splitContainer1_KeyPress);
            // 
            // splitContainer2
            // 
            this.splitContainer2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.c1DockingTab2);
            this.splitContainer2.Panel1MinSize = 49;
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer2.Panel2Collapsed = true;
            this.splitContainer2.Panel2MinSize = 0;
            this.splitContainer2.Size = new System.Drawing.Size(235, 535);
            this.splitContainer2.SplitterDistance = 454;
            this.splitContainer2.SplitterWidth = 2;
            this.splitContainer2.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.splitContainer2, "(default)");
            // 
            // c1DockingTab2
            // 
            this.c1DockingTab2.Alignment = System.Windows.Forms.TabAlignment.Left;
            this.c1DockingTab2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab2.Controls.Add(this.tabAutoReplace);
            this.c1DockingTab2.Controls.Add(this.tabSchemaInformation);
            this.c1DockingTab2.Controls.Add(this.tabTabList);
            this.c1DockingTab2.Controls.Add(this.tabSqlNavigator);
            this.c1DockingTab2.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1DockingTab2.Location = new System.Drawing.Point(0, 0);
            this.c1DockingTab2.MultiLine = true;
            this.c1DockingTab2.Name = "c1DockingTab2";
            this.c1DockingTab2.SelectedIndex = 3;
            this.c1DockingTab2.ShowToolTips = true;
            this.c1DockingTab2.Size = new System.Drawing.Size(234, 534);
            this.c1DockingTab2.TabIndex = 67;
            this.c1DockingTab2.TabsSpacing = 0;
            this.c1DockingTab2.TextDirection = C1.Win.C1Command.TabTextDirectionEnum.VerticalRight;
            this.c1ThemeController1.SetTheme(this.c1DockingTab2, "(default)");
            this.c1DockingTab2.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab2.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab2.TabClick += new System.EventHandler(this.c1DockingTab2_TabClick);
            this.c1DockingTab2.Enter += new System.EventHandler(this.c1DockingTab2_Enter);
            this.c1DockingTab2.Leave += new System.EventHandler(this.c1DockingTab2_Leave);
            // 
            // tabAutoReplace
            // 
            this.tabAutoReplace.Controls.Add(this.lblAutoReplacePosition);
            this.tabAutoReplace.Controls.Add(this.btnHelp_AutoReplace);
            this.tabAutoReplace.Controls.Add(this.c1GridAutoReplaceInfo);
            this.tabAutoReplace.Controls.Add(this.tsAutoReplace);
            this.tabAutoReplace.Image = ((System.Drawing.Image)(resources.GetObject("tabAutoReplace.Image")));
            this.tabAutoReplace.Location = new System.Drawing.Point(26, 1);
            this.tabAutoReplace.Name = "tabAutoReplace";
            this.tabAutoReplace.Size = new System.Drawing.Size(207, 532);
            this.tabAutoReplace.TabIndex = 0;
            this.tabAutoReplace.Text = "Auto Replace";
            // 
            // lblAutoReplacePosition
            // 
            this.lblAutoReplacePosition.AutoSize = true;
            this.lblAutoReplacePosition.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.lblAutoReplacePosition.Location = new System.Drawing.Point(0, 15);
            this.lblAutoReplacePosition.Name = "lblAutoReplacePosition";
            this.lblAutoReplacePosition.Size = new System.Drawing.Size(15, 16);
            this.lblAutoReplacePosition.TabIndex = 98;
            this.lblAutoReplacePosition.Text = "A";
            this.c1ThemeController1.SetTheme(this.lblAutoReplacePosition, "(default)");
            this.lblAutoReplacePosition.Visible = false;
            // 
            // btnHelp_AutoReplace
            // 
            this.btnHelp_AutoReplace.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_AutoReplace.Image")));
            this.btnHelp_AutoReplace.Location = new System.Drawing.Point(86, 1);
            this.btnHelp_AutoReplace.Name = "btnHelp_AutoReplace";
            this.btnHelp_AutoReplace.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_AutoReplace.TabIndex = 90;
            this.c1ThemeController1.SetTheme(this.btnHelp_AutoReplace, "(default)");
            this.btnHelp_AutoReplace.UseVisualStyleBackColor = true;
            this.btnHelp_AutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_AutoReplace.Click += new System.EventHandler(this.btnHelp_AutoReplace_Click);
            // 
            // c1GridAutoReplaceInfo
            // 
            this.c1GridAutoReplaceInfo.AllowUpdateOnBlur = false;
            this.c1GridAutoReplaceInfo.AlternatingRows = true;
            this.c1GridAutoReplaceInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridAutoReplaceInfo.CaptionHeight = 19;
            this.c1GridAutoReplaceInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridAutoReplaceInfo.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridAutoReplaceInfo.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridAutoReplaceInfo.Images"))));
            this.c1GridAutoReplaceInfo.Location = new System.Drawing.Point(1, 24);
            this.c1GridAutoReplaceInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridAutoReplaceInfo.Name = "c1GridAutoReplaceInfo";
            this.c1GridAutoReplaceInfo.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridAutoReplaceInfo.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridAutoReplaceInfo.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridAutoReplaceInfo.PreviewInfo.ZoomFactor = 75D;
            this.c1GridAutoReplaceInfo.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridAutoReplaceInfo.PrintInfo.MeasurementPrinterName = null;
            this.c1GridAutoReplaceInfo.RowHeight = 17;
            this.c1GridAutoReplaceInfo.Size = new System.Drawing.Size(205, 507);
            this.c1GridAutoReplaceInfo.TabIndex = 65;
            this.c1GridAutoReplaceInfo.UseCompatibleTextRendering = false;
            this.c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
            this.c1GridAutoReplaceInfo.AfterUpdate += new System.EventHandler(this.c1GridAutoReplaceInfo_AfterUpdate);
            this.c1GridAutoReplaceInfo.Enter += new System.EventHandler(this.c1GridAutoReplaceInfo_Enter);
            this.c1GridAutoReplaceInfo.Leave += new System.EventHandler(this.c1GridAutoReplaceInfo_Leave);
            this.c1GridAutoReplaceInfo.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1GridAutoReplaceInfo_MouseMove);
            this.c1GridAutoReplaceInfo.PropBag = resources.GetString("c1GridAutoReplaceInfo.PropBag");
            // 
            // tsAutoReplace
            // 
            this.tsAutoReplace.BackColor = System.Drawing.Color.Transparent;
            this.tsAutoReplace.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsAutoReplace.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblAutoReplace});
            this.tsAutoReplace.Location = new System.Drawing.Point(0, 0);
            this.tsAutoReplace.Name = "tsAutoReplace";
            this.tsAutoReplace.Size = new System.Drawing.Size(207, 25);
            this.tsAutoReplace.TabIndex = 67;
            this.tsAutoReplace.Text = "toolStrip1";
            // 
            // lblAutoReplace
            // 
            this.lblAutoReplace.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.lblAutoReplace.Name = "lblAutoReplace";
            this.lblAutoReplace.Size = new System.Drawing.Size(83, 22);
            this.lblAutoReplace.Text = "Auto Replace";
            // 
            // tabSchemaInformation
            // 
            this.tabSchemaInformation.Controls.Add(this.lblSchemaFilterPosition);
            this.tabSchemaInformation.Controls.Add(this.txtSchemaFilter);
            this.tabSchemaInformation.Controls.Add(this.tsSchemaBrowser);
            this.tabSchemaInformation.Controls.Add(this.c1GridSchemaBrowser);
            this.tabSchemaInformation.Image = ((System.Drawing.Image)(resources.GetObject("tabSchemaInformation.Image")));
            this.tabSchemaInformation.Location = new System.Drawing.Point(26, 1);
            this.tabSchemaInformation.Name = "tabSchemaInformation";
            this.tabSchemaInformation.Size = new System.Drawing.Size(207, 532);
            this.tabSchemaInformation.TabIndex = 1;
            this.tabSchemaInformation.Text = "Schema Browser";
            // 
            // lblSchemaFilterPosition
            // 
            this.lblSchemaFilterPosition.AutoSize = true;
            this.lblSchemaFilterPosition.BackColor = System.Drawing.Color.Transparent;
            this.lblSchemaFilterPosition.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSchemaFilterPosition.Location = new System.Drawing.Point(116, 14);
            this.lblSchemaFilterPosition.Name = "lblSchemaFilterPosition";
            this.lblSchemaFilterPosition.Size = new System.Drawing.Size(13, 16);
            this.lblSchemaFilterPosition.TabIndex = 95;
            this.lblSchemaFilterPosition.Text = "F";
            this.lblSchemaFilterPosition.Visible = false;
            // 
            // txtSchemaFilter
            // 
            this.txtSchemaFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtSchemaFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSchemaFilter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtSchemaFilter.Location = new System.Drawing.Point(154, 2);
            this.txtSchemaFilter.Name = "txtSchemaFilter";
            this.txtSchemaFilter.Size = new System.Drawing.Size(74, 20);
            this.txtSchemaFilter.TabIndex = 89;
            this.txtSchemaFilter.Tag = null;
            this.txtSchemaFilter.Text = "*";
            this.txtSchemaFilter.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtSchemaFilter, "(default)");
            this.txtSchemaFilter.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtSchemaFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSchemaFilter_KeyDown);
            this.txtSchemaFilter.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtSchemaFilter_KeyUp);
            // 
            // tsSchemaBrowser
            // 
            this.tsSchemaBrowser.BackColor = System.Drawing.Color.Transparent;
            this.tsSchemaBrowser.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSchemaBrowser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.btnExpandCollapse,
            this.toolStripSeparator15,
            this.btnSettingOfFocus,
            this.btnHelp_SchemaFilter,
            this.lblSchemaFilter});
            this.tsSchemaBrowser.Location = new System.Drawing.Point(0, 0);
            this.tsSchemaBrowser.Name = "tsSchemaBrowser";
            this.tsSchemaBrowser.Size = new System.Drawing.Size(207, 25);
            this.tsSchemaBrowser.TabIndex = 7;
            this.tsSchemaBrowser.Text = "toolStrip1";
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(23, 22);
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // btnExpandCollapse
            // 
            this.btnExpandCollapse.BackColor = System.Drawing.Color.Transparent;
            this.btnExpandCollapse.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExpandCollapse.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuExpandAll,
            this.mnuCollapseAll});
            this.btnExpandCollapse.Image = ((System.Drawing.Image)(resources.GetObject("btnExpandCollapse.Image")));
            this.btnExpandCollapse.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExpandCollapse.Name = "btnExpandCollapse";
            this.btnExpandCollapse.Size = new System.Drawing.Size(32, 22);
            // 
            // mnuExpandAll
            // 
            this.mnuExpandAll.Name = "mnuExpandAll";
            this.mnuExpandAll.Size = new System.Drawing.Size(140, 22);
            this.mnuExpandAll.Tag = "mnuExpandAll";
            this.mnuExpandAll.Text = "Expand All";
            this.mnuExpandAll.Click += new System.EventHandler(this.mnuExpandAll_Click);
            // 
            // mnuCollapseAll
            // 
            this.mnuCollapseAll.Name = "mnuCollapseAll";
            this.mnuCollapseAll.Size = new System.Drawing.Size(140, 22);
            this.mnuCollapseAll.Tag = "mnuCollapseAll";
            this.mnuCollapseAll.Text = "Collapse All";
            this.mnuCollapseAll.Click += new System.EventHandler(this.mnuCollapseAll_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSettingOfFocus
            // 
            this.btnSettingOfFocus.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSettingOfFocus.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuFocusOnDataGrid,
            this.mnuFocusOnQueryEditor});
            this.btnSettingOfFocus.Image = ((System.Drawing.Image)(resources.GetObject("btnSettingOfFocus.Image")));
            this.btnSettingOfFocus.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSettingOfFocus.Name = "btnSettingOfFocus";
            this.btnSettingOfFocus.Size = new System.Drawing.Size(32, 22);
            this.btnSettingOfFocus.Text = "toolStripSplitButton1";
            this.btnSettingOfFocus.ButtonClick += new System.EventHandler(this.btnSettingOfFocus_ButtonClick);
            // 
            // mnuFocusOnDataGrid
            // 
            this.mnuFocusOnDataGrid.Name = "mnuFocusOnDataGrid";
            this.mnuFocusOnDataGrid.Size = new System.Drawing.Size(364, 22);
            this.mnuFocusOnDataGrid.Text = "After Pasting to Query Editor, Focus on Data Grid";
            this.mnuFocusOnDataGrid.Click += new System.EventHandler(this.mnuFocusOnDataGrid_Click);
            // 
            // mnuFocusOnQueryEditor
            // 
            this.mnuFocusOnQueryEditor.Name = "mnuFocusOnQueryEditor";
            this.mnuFocusOnQueryEditor.Size = new System.Drawing.Size(364, 22);
            this.mnuFocusOnQueryEditor.Text = "After Pasting to Query Editor, Focus on Query Editor";
            this.mnuFocusOnQueryEditor.Click += new System.EventHandler(this.mnuFocusOnQueryEditor_Click);
            // 
            // btnHelp_SchemaFilter
            // 
            this.btnHelp_SchemaFilter.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHelp_SchemaFilter.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_SchemaFilter.Image")));
            this.btnHelp_SchemaFilter.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHelp_SchemaFilter.Name = "btnHelp_SchemaFilter";
            this.btnHelp_SchemaFilter.Size = new System.Drawing.Size(23, 22);
            this.btnHelp_SchemaFilter.Click += new System.EventHandler(this.btnHelp_SchemaFilter_Click);
            // 
            // lblSchemaFilter
            // 
            this.lblSchemaFilter.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSchemaFilter.Name = "lblSchemaFilter";
            this.lblSchemaFilter.Size = new System.Drawing.Size(37, 22);
            this.lblSchemaFilter.Text = "Filter:";
            // 
            // c1GridSchemaBrowser
            // 
            this.c1GridSchemaBrowser.AllowUpdate = false;
            this.c1GridSchemaBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridSchemaBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridSchemaBrowser.ColumnHeaders = false;
            this.c1GridSchemaBrowser.DataView = C1.Win.C1TrueDBGrid.DataViewEnum.GroupBy;
            this.c1GridSchemaBrowser.FetchRowStyles = true;
            this.c1GridSchemaBrowser.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridSchemaBrowser.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridSchemaBrowser.GroupByAreaVisible = false;
            this.c1GridSchemaBrowser.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridSchemaBrowser.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridSchemaBrowser.Images"))));
            this.c1GridSchemaBrowser.Location = new System.Drawing.Point(1, 25);
            this.c1GridSchemaBrowser.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridSchemaBrowser.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None;
            this.c1GridSchemaBrowser.Name = "c1GridSchemaBrowser";
            this.c1GridSchemaBrowser.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridSchemaBrowser.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridSchemaBrowser.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridSchemaBrowser.PreviewInfo.ZoomFactor = 75D;
            this.c1GridSchemaBrowser.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridSchemaBrowser.PrintInfo.MeasurementPrinterName = null;
            this.c1GridSchemaBrowser.RowHeight = 19;
            this.c1GridSchemaBrowser.Size = new System.Drawing.Size(205, 506);
            this.c1GridSchemaBrowser.TabIndex = 6;
            this.c1ThemeController1.SetTheme(this.c1GridSchemaBrowser, "(default)");
            this.c1GridSchemaBrowser.UseCompatibleTextRendering = false;
            this.c1GridSchemaBrowser.FetchRowStyle += new C1.Win.C1TrueDBGrid.FetchRowStyleEventHandler(this.c1GridSchemaBrowser_FetchRowStyle);
            this.c1GridSchemaBrowser.Enter += new System.EventHandler(this.c1GridSchemaBrowser_Enter);
            this.c1GridSchemaBrowser.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1GridSchemaBrowser_KeyDown);
            this.c1GridSchemaBrowser.Leave += new System.EventHandler(this.c1GridSchemaBrowser_Leave);
            this.c1GridSchemaBrowser.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseDoubleClick);
            this.c1GridSchemaBrowser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseDown);
            this.c1GridSchemaBrowser.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseMove);
            this.c1GridSchemaBrowser.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseUp);
            this.c1GridSchemaBrowser.PropBag = resources.GetString("c1GridSchemaBrowser.PropBag");
            // 
            // tabTabList
            // 
            this.tabTabList.Controls.Add(this.c1GridTabList);
            this.tabTabList.Image = ((System.Drawing.Image)(resources.GetObject("tabTabList.Image")));
            this.tabTabList.Location = new System.Drawing.Point(26, 1);
            this.tabTabList.Name = "tabTabList";
            this.tabTabList.Size = new System.Drawing.Size(207, 532);
            this.tabTabList.TabIndex = 2;
            this.tabTabList.Text = "Tab List";
            // 
            // c1GridTabList
            // 
            this.c1GridTabList.AllowUpdateOnBlur = false;
            this.c1GridTabList.AlternatingRows = true;
            this.c1GridTabList.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridTabList.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridTabList.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridTabList.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridTabList.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridTabList.Images"))));
            this.c1GridTabList.Location = new System.Drawing.Point(1, 1);
            this.c1GridTabList.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridTabList.Name = "c1GridTabList";
            this.c1GridTabList.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridTabList.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridTabList.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridTabList.PreviewInfo.ZoomFactor = 75D;
            this.c1GridTabList.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridTabList.PrintInfo.MeasurementPrinterName = null;
            this.c1GridTabList.RowHeight = 19;
            this.c1GridTabList.Size = new System.Drawing.Size(205, 499);
            this.c1GridTabList.TabIndex = 66;
            this.c1ThemeController1.SetTheme(this.c1GridTabList, "(default)");
            this.c1GridTabList.UseCompatibleTextRendering = false;
            this.c1GridTabList.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridTabList_MouseDoubleClick);
            this.c1GridTabList.PropBag = resources.GetString("c1GridTabList.PropBag");
            // 
            // tabSqlNavigator
            // 
            this.tabSqlNavigator.Controls.Add(this.c1GridSqlNavigator);
            this.tabSqlNavigator.Image = ((System.Drawing.Image)(resources.GetObject("tabSqlNavigator.Image")));
            this.tabSqlNavigator.Location = new System.Drawing.Point(26, 1);
            this.tabSqlNavigator.Name = "tabSqlNavigator";
            this.tabSqlNavigator.Size = new System.Drawing.Size(207, 532);
            this.tabSqlNavigator.TabIndex = 3;
            this.tabSqlNavigator.Text = "SQL Navigator";
            // 
            // c1GridSqlNavigator
            // 
            this.c1GridSqlNavigator.AllowFilter = false;
            this.c1GridSqlNavigator.AllowSort = false;
            this.c1GridSqlNavigator.AllowUpdate = false;
            this.c1GridSqlNavigator.AllowUpdateOnBlur = false;
            this.c1GridSqlNavigator.AlternatingRows = true;
            this.c1GridSqlNavigator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridSqlNavigator.FilterBar = true;
            this.c1GridSqlNavigator.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridSqlNavigator.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridSqlNavigator.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridSqlNavigator.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridSqlNavigator.Images"))));
            this.c1GridSqlNavigator.Location = new System.Drawing.Point(1, 1);
            this.c1GridSqlNavigator.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridSqlNavigator.Name = "c1GridSqlNavigator";
            this.c1GridSqlNavigator.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridSqlNavigator.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridSqlNavigator.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridSqlNavigator.PreviewInfo.ZoomFactor = 75D;
            this.c1GridSqlNavigator.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridSqlNavigator.PrintInfo.MeasurementPrinterName = null;
            this.c1GridSqlNavigator.RowHeight = 19;
            this.c1GridSqlNavigator.Size = new System.Drawing.Size(205, 499);
            this.c1GridSqlNavigator.TabIndex = 67;
            this.c1ThemeController1.SetTheme(this.c1GridSqlNavigator, "(default)");
            this.c1GridSqlNavigator.UseCompatibleTextRendering = false;
            this.c1GridSqlNavigator.Filter += new C1.Win.C1TrueDBGrid.FilterEventHandler(this.C1GridSqlNavigator_Filter);
            this.c1GridSqlNavigator.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridSqlNavigator_MouseDoubleClick);
            this.c1GridSqlNavigator.PropBag = resources.GetString("c1GridSqlNavigator.PropBag");
            // 
            // splitContainer3
            // 
            this.splitContainer3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.c1GridAutoCompleteForAll);
            this.splitContainer3.Panel1.Controls.Add(this.nudQueryTimeout);
            this.splitContainer3.Panel1.Controls.Add(this.editor);
            this.splitContainer3.Panel1.Controls.Add(this.lblQueryTimeoutPosition);
            this.splitContainer3.Panel1.Controls.Add(this.txtIndentWord);
            this.splitContainer3.Panel1.Controls.Add(this.btnHelp_QueryTimeout);
            this.splitContainer3.Panel1.Controls.Add(this.tsEditor);
            this.splitContainer3.Panel1.Controls.Add(this.lblInfoEditor);
            this.splitContainer3.Panel1.Controls.Add(this.c1StatusBar2);
            this.c1ThemeController1.SetTheme(this.splitContainer3.Panel1, "(default)");
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.chkShowFilterRow);
            this.splitContainer3.Panel2.Controls.Add(this.chkSize);
            this.splitContainer3.Panel2.Controls.Add(this.chkShowColumnComments);
            this.splitContainer3.Panel2.Controls.Add(this.btnHelp_RawDataMode);
            this.splitContainer3.Panel2.Controls.Add(this.chkRawDataMode);
            this.splitContainer3.Panel2.Controls.Add(this.chkShowColumnType);
            this.splitContainer3.Panel2.Controls.Add(this.c1DockingTab1);
            this.splitContainer3.Panel2.Controls.Add(this.chkShowGroupingRow);
            this.splitContainer3.Panel2.Controls.Add(this.cboFindGrid);
            this.splitContainer3.Panel2.Controls.Add(this.tsDataGrid);
            this.c1ThemeController1.SetTheme(this.splitContainer3.Panel2, "(default)");
            this.splitContainer3.Size = new System.Drawing.Size(922, 535);
            this.splitContainer3.SplitterDistance = 267;
            this.splitContainer3.SplitterWidth = 2;
            this.splitContainer3.TabIndex = 87;
            this.c1ThemeController1.SetTheme(this.splitContainer3, "(default)");
            this.splitContainer3.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer3_SplitterMoving);
            this.splitContainer3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer3_SplitterMoved);
            this.splitContainer3.MouseMove += new System.Windows.Forms.MouseEventHandler(this.splitContainer3_MouseMove);
            // 
            // c1GridAutoCompleteForAll
            // 
            this.c1GridAutoCompleteForAll.AllowFilter = false;
            this.c1GridAutoCompleteForAll.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridAutoCompleteForAll.AllowUpdate = false;
            this.c1GridAutoCompleteForAll.AlternatingRows = true;
            this.c1GridAutoCompleteForAll.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridAutoCompleteForAll.CaptionHeight = 19;
            this.c1GridAutoCompleteForAll.ColumnHeaders = false;
            this.c1GridAutoCompleteForAll.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridAutoCompleteForAll.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridAutoCompleteForAll.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridAutoCompleteForAll.Images"))));
            this.c1GridAutoCompleteForAll.Location = new System.Drawing.Point(325, 41);
            this.c1GridAutoCompleteForAll.Name = "c1GridAutoCompleteForAll";
            this.c1GridAutoCompleteForAll.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridAutoCompleteForAll.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridAutoCompleteForAll.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridAutoCompleteForAll.PreviewInfo.ZoomFactor = 75D;
            this.c1GridAutoCompleteForAll.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridAutoCompleteForAll.PrintInfo.MeasurementPrinterName = null;
            this.c1GridAutoCompleteForAll.RowHeight = 19;
            this.c1GridAutoCompleteForAll.Size = new System.Drawing.Size(270, 183);
            this.c1GridAutoCompleteForAll.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridAutoCompleteForAll.TabIndex = 117;
            this.c1ThemeController1.SetTheme(this.c1GridAutoCompleteForAll, "(default)");
            this.c1GridAutoCompleteForAll.UseCompatibleTextRendering = false;
            this.c1GridAutoCompleteForAll.Visible = false;
            this.c1GridAutoCompleteForAll.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1GridAutoCompleteForAll_KeyDown);
            this.c1GridAutoCompleteForAll.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridAutoCompleteForAll_KeyPress);
            this.c1GridAutoCompleteForAll.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridAutoCompleteForAll_MouseDoubleClick);
            this.c1GridAutoCompleteForAll.PropBag = resources.GetString("c1GridAutoCompleteForAll.PropBag");
            // 
            // nudQueryTimeout
            // 
            this.nudQueryTimeout.Location = new System.Drawing.Point(805, 3);
            this.nudQueryTimeout.Maximum = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout.Name = "nudQueryTimeout";
            this.nudQueryTimeout.Size = new System.Drawing.Size(48, 23);
            this.nudQueryTimeout.TabIndex = 96;
            this.nudQueryTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudQueryTimeout, "(default)");
            this.nudQueryTimeout.Value = new decimal(new int[] {
            3600,
            0,
            0,
            0});
            this.nudQueryTimeout.Enter += new System.EventHandler(this.nudQueryTimeout_Enter);
            this.nudQueryTimeout.Leave += new System.EventHandler(this.nudQueryTimeout_Leave);
            this.nudQueryTimeout.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudQueryTimeout_MouseClick);
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editor.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editor.CaretLineVisible = true;
            this.editor.EndAtLastLine = false;
            this.editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editor.Location = new System.Drawing.Point(-1, 29);
            this.editor.Name = "editor";
            this.editor.ScrollWidth = 400;
            this.editor.SelectionEolFilled = true;
            this.editor.Size = new System.Drawing.Size(922, 215);
            this.editor.Styler = null;
            this.editor.TabIndex = 1;
            this.editor.WhitespaceSize = 3;
            this.editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editor.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editor_DoubleClick);
            this.editor.Insert += new System.EventHandler<ScintillaNET.ModificationEventArgs>(this.editor_Insert);
            this.editor.UpdateUI += new System.EventHandler<ScintillaNET.UpdateUIEventArgs>(this.editor_UpdateUI);
            this.editor.ZoomChanged += new System.EventHandler<System.EventArgs>(this.editor_ZoomChanged);
            this.editor.TextChanged += new System.EventHandler(this.editor_TextChanged);
            this.editor.Enter += new System.EventHandler(this.editor_Enter);
            this.editor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editor_KeyDown);
            this.editor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editor_KeyPress);
            this.editor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.editor_KeyUp);
            this.editor.Leave += new System.EventHandler(this.editor_Leave);
            this.editor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editor_MouseDown);
            this.editor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.editor_MouseMove);
            // 
            // lblQueryTimeoutPosition
            // 
            this.lblQueryTimeoutPosition.AutoSize = true;
            this.lblQueryTimeoutPosition.Location = new System.Drawing.Point(695, 17);
            this.lblQueryTimeoutPosition.Name = "lblQueryTimeoutPosition";
            this.lblQueryTimeoutPosition.Size = new System.Drawing.Size(17, 16);
            this.lblQueryTimeoutPosition.TabIndex = 97;
            this.lblQueryTimeoutPosition.Text = "Q";
            this.c1ThemeController1.SetTheme(this.lblQueryTimeoutPosition, "(default)");
            this.lblQueryTimeoutPosition.Visible = false;
            // 
            // txtIndentWord
            // 
            this.txtIndentWord.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtIndentWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtIndentWord.Location = new System.Drawing.Point(554, 5);
            this.txtIndentWord.MaxLength = 1;
            this.txtIndentWord.Name = "txtIndentWord";
            this.txtIndentWord.Size = new System.Drawing.Size(15, 21);
            this.txtIndentWord.TabIndex = 5;
            this.txtIndentWord.Tag = null;
            this.txtIndentWord.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtIndentWord.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtIndentWord, "(default)");
            this.txtIndentWord.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtIndentWord.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtIndentWord_MouseClick);
            this.txtIndentWord.Enter += new System.EventHandler(this.txtIndentWord_Enter);
            this.txtIndentWord.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtIndentWord_KeyDown);
            this.txtIndentWord.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIndentWord_KeyPress);
            this.txtIndentWord.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtIndentWord_KeyUp);
            this.txtIndentWord.Leave += new System.EventHandler(this.txtIndentWord_Leave);
            // 
            // btnHelp_QueryTimeout
            // 
            this.btnHelp_QueryTimeout.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_QueryTimeout.Image")));
            this.btnHelp_QueryTimeout.Location = new System.Drawing.Point(899, 4);
            this.btnHelp_QueryTimeout.Name = "btnHelp_QueryTimeout";
            this.btnHelp_QueryTimeout.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_QueryTimeout.TabIndex = 89;
            this.c1ThemeController1.SetTheme(this.btnHelp_QueryTimeout, "(default)");
            this.btnHelp_QueryTimeout.UseVisualStyleBackColor = true;
            this.btnHelp_QueryTimeout.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_QueryTimeout.Click += new System.EventHandler(this.btnHelp_QueryTimeout_Click);
            // 
            // tsEditor
            // 
            this.tsEditor.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsEditor.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tsEditor.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnNew,
            this.btnOpen,
            this.btnSave,
            this.btnSaveRed,
            this.btnSaveAs,
            this.toolStripSeparator5,
            this.btnQuery,
            this.btnExecuteCurrentBlock,
            this.btnExecuteCurrentLine,
            this.btnCancelQuery,
            this.btnCommit,
            this.btnRollback,
            this.toolStripSeparator7,
            this.btnSelectCurrentBlock,
            this.btnSelectCurrentLine,
            this.btnRemoveTrailingBlanks,
            this.btnCode2Sql,
            this.toolStripSeparator4,
            this.btnSql2Code,
            this.toolStripSeparator9,
            this.btnLeftAndRight,
            this.btnUpAndDown,
            this.toolStripSeparator3,
            this.btnComment,
            this.btnRemoveComment,
            this.toolStripSeparator6,
            this.btnIndent,
            this.lblIndentWord,
            this.txtIndentWord2,
            this.btnUnIndent,
            this.toolStripSeparator8,
            this.btnHighlightSelection,
            this.btnHighlightSelection2,
            this.toolStripSeparator1,
            this.btnWordWrap,
            this.btnWordWrap2,
            this.btnShowAllCharacters,
            this.btnShowAllCharacters2,
            this.btnShowIndentGuide,
            this.btnShowIndentGuide2,
            this.tsSeparator,
            this.lblQueryTimeout});
            this.tsEditor.Location = new System.Drawing.Point(0, 0);
            this.tsEditor.Name = "tsEditor";
            this.tsEditor.Size = new System.Drawing.Size(920, 31);
            this.tsEditor.TabIndex = 2;
            this.c1ThemeController1.SetTheme(this.tsEditor, "(default)");
            this.tsEditor.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tsEditor_MouseMove);
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(28, 28);
            this.btnNew.Tag = "";
            this.btnNew.ToolTipText = "Open New SQL Editor";
            this.btnNew.Click += new System.EventHandler(this.btnNew_Click);
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(28, 28);
            this.btnOpen.ToolTipText = "Open Query file";
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Enabled = false;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(28, 28);
            this.btnSave.ToolTipText = "Save (Ctrl+S)\r\nSave as Encoding: UTF8";
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveRed
            // 
            this.btnSaveRed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveRed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveRed.Name = "btnSaveRed";
            this.btnSaveRed.Size = new System.Drawing.Size(23, 28);
            this.btnSaveRed.ToolTipText = "Save (Ctrl+S)\r\nSave as Encoding: UTF8";
            this.btnSaveRed.Visible = false;
            this.btnSaveRed.Click += new System.EventHandler(this.btnSaveRed_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Image")));
            this.btnSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(28, 28);
            this.btnSaveAs.ToolTipText = "Save As (F12)\r\nSave as Encoding: UTF8";
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 31);
            // 
            // btnQuery
            // 
            this.btnQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnQuery.Enabled = false;
            this.btnQuery.Image = ((System.Drawing.Image)(resources.GetObject("btnQuery.Image")));
            this.btnQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnQuery.Name = "btnQuery";
            this.btnQuery.Size = new System.Drawing.Size(28, 28);
            this.btnQuery.Tag = "";
            this.btnQuery.ToolTipText = "Execute Statement (F5)\r\n(Selected or All)";
            this.btnQuery.Click += new System.EventHandler(this.btnQuery_Click);
            // 
            // btnExecuteCurrentBlock
            // 
            this.btnExecuteCurrentBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExecuteCurrentBlock.Enabled = false;
            this.btnExecuteCurrentBlock.Image = ((System.Drawing.Image)(resources.GetObject("btnExecuteCurrentBlock.Image")));
            this.btnExecuteCurrentBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExecuteCurrentBlock.Name = "btnExecuteCurrentBlock";
            this.btnExecuteCurrentBlock.Size = new System.Drawing.Size(28, 28);
            this.btnExecuteCurrentBlock.ToolTipText = "Execute Current Block (Ctrl+Enter)";
            this.btnExecuteCurrentBlock.Click += new System.EventHandler(this.btnExecuteCurrentBlock_Click);
            // 
            // btnExecuteCurrentLine
            // 
            this.btnExecuteCurrentLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExecuteCurrentLine.Enabled = false;
            this.btnExecuteCurrentLine.Image = ((System.Drawing.Image)(resources.GetObject("btnExecuteCurrentLine.Image")));
            this.btnExecuteCurrentLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExecuteCurrentLine.Name = "btnExecuteCurrentLine";
            this.btnExecuteCurrentLine.Size = new System.Drawing.Size(28, 28);
            this.btnExecuteCurrentLine.Click += new System.EventHandler(this.btnExecuteCurrentLine_Click);
            // 
            // btnCancelQuery
            // 
            this.btnCancelQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCancelQuery.Enabled = false;
            this.btnCancelQuery.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelQuery.Image")));
            this.btnCancelQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancelQuery.Name = "btnCancelQuery";
            this.btnCancelQuery.Size = new System.Drawing.Size(28, 28);
            this.btnCancelQuery.Tag = "";
            this.btnCancelQuery.ToolTipText = "Cancel Query";
            this.btnCancelQuery.Click += new System.EventHandler(this.btnCancelQuery_Click);
            // 
            // btnCommit
            // 
            this.btnCommit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCommit.Enabled = false;
            this.btnCommit.Image = ((System.Drawing.Image)(resources.GetObject("btnCommit.Image")));
            this.btnCommit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCommit.Name = "btnCommit";
            this.btnCommit.Size = new System.Drawing.Size(28, 28);
            this.btnCommit.Tag = "N";
            this.btnCommit.Text = "toolStripButton1";
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // btnRollback
            // 
            this.btnRollback.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRollback.Enabled = false;
            this.btnRollback.Image = ((System.Drawing.Image)(resources.GetObject("btnRollback.Image")));
            this.btnRollback.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRollback.Name = "btnRollback";
            this.btnRollback.Size = new System.Drawing.Size(28, 28);
            this.btnRollback.Tag = "N";
            this.btnRollback.Text = "toolStripButton2";
            this.btnRollback.Click += new System.EventHandler(this.btnRollback_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 31);
            // 
            // btnSelectCurrentBlock
            // 
            this.btnSelectCurrentBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectCurrentBlock.Enabled = false;
            this.btnSelectCurrentBlock.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectCurrentBlock.Image")));
            this.btnSelectCurrentBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectCurrentBlock.Name = "btnSelectCurrentBlock";
            this.btnSelectCurrentBlock.Size = new System.Drawing.Size(28, 28);
            this.btnSelectCurrentBlock.ToolTipText = "Select Current Block (Ctrl+B)";
            this.btnSelectCurrentBlock.Click += new System.EventHandler(this.btnSelectCurrentBlock_Click);
            // 
            // btnSelectCurrentLine
            // 
            this.btnSelectCurrentLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectCurrentLine.Enabled = false;
            this.btnSelectCurrentLine.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectCurrentLine.Image")));
            this.btnSelectCurrentLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectCurrentLine.Name = "btnSelectCurrentLine";
            this.btnSelectCurrentLine.Size = new System.Drawing.Size(28, 28);
            this.btnSelectCurrentLine.Click += new System.EventHandler(this.btnSelectCurrentLine_Click);
            // 
            // btnRemoveTrailingBlanks
            // 
            this.btnRemoveTrailingBlanks.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemoveTrailingBlanks.Enabled = false;
            this.btnRemoveTrailingBlanks.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveTrailingBlanks.Image")));
            this.btnRemoveTrailingBlanks.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveTrailingBlanks.Name = "btnRemoveTrailingBlanks";
            this.btnRemoveTrailingBlanks.Size = new System.Drawing.Size(28, 28);
            this.btnRemoveTrailingBlanks.Click += new System.EventHandler(this.btnRemoveTrailingBlanks_Click);
            // 
            // btnCode2Sql
            // 
            this.btnCode2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCode2Sql.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCSharp2Sql,
            this.mnuVB2Sql,
            this.mnuDelphi2Sql});
            this.btnCode2Sql.Enabled = false;
            this.btnCode2Sql.Image = ((System.Drawing.Image)(resources.GetObject("btnCode2Sql.Image")));
            this.btnCode2Sql.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCode2Sql.Name = "btnCode2Sql";
            this.btnCode2Sql.Size = new System.Drawing.Size(37, 28);
            this.btnCode2Sql.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnCode2Sql.ToolTipText = "Code to SQL\r\n(Strip SQL from non-SQL code)";
            this.btnCode2Sql.Click += new System.EventHandler(this.btnCode2Sql_Click);
            // 
            // mnuCSharp2Sql
            // 
            this.mnuCSharp2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuCSharp2Sql.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuCSharp2Sql.Name = "mnuCSharp2Sql";
            this.mnuCSharp2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuCSharp2Sql.Text = "C# to SQL";
            this.mnuCSharp2Sql.Click += new System.EventHandler(this.mnuCSharp2Sql_Click);
            // 
            // mnuVB2Sql
            // 
            this.mnuVB2Sql.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuVB2Sql.Name = "mnuVB2Sql";
            this.mnuVB2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuVB2Sql.Text = "VB.Net/VB6/VBA to SQL";
            this.mnuVB2Sql.Click += new System.EventHandler(this.mnuVB2Sql_Click);
            // 
            // mnuDelphi2Sql
            // 
            this.mnuDelphi2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuDelphi2Sql.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.mnuDelphi2Sql.Name = "mnuDelphi2Sql";
            this.mnuDelphi2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuDelphi2Sql.Text = "Delphi6 to SQL";
            this.mnuDelphi2Sql.Click += new System.EventHandler(this.mnuDelphi2Sql_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // btnSql2Code
            // 
            this.btnSql2Code.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSql2Code.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSql2CSharp,
            this.mnuSql2VBNet,
            this.mnuSql2VB6A,
            this.mnuSql2Delphi});
            this.btnSql2Code.Enabled = false;
            this.btnSql2Code.Image = ((System.Drawing.Image)(resources.GetObject("btnSql2Code.Image")));
            this.btnSql2Code.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSql2Code.Name = "btnSql2Code";
            this.btnSql2Code.Size = new System.Drawing.Size(37, 28);
            this.btnSql2Code.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnSql2Code.ToolTipText = "SQL to Code\r\n(Make non-SQL code statement from SQL)";
            this.btnSql2Code.Click += new System.EventHandler(this.btnSql2Code_Click);
            // 
            // mnuSql2CSharp
            // 
            this.mnuSql2CSharp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuSql2CSharp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCSharpStyle1,
            this.mnuCSharpStyle2,
            this.mnuCSharpStyle3,
            this.mnuCSharpStyle4});
            this.mnuSql2CSharp.Name = "mnuSql2CSharp";
            this.mnuSql2CSharp.Size = new System.Drawing.Size(165, 22);
            this.mnuSql2CSharp.Text = "SQL to C#";
            // 
            // mnuCSharpStyle1
            // 
            this.mnuCSharpStyle1.Name = "mnuCSharpStyle1";
            this.mnuCSharpStyle1.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle1.Tag = "C#`1";
            this.mnuCSharpStyle1.Text = "Style 1: Using + operator";
            this.mnuCSharpStyle1.Click += new System.EventHandler(this.mnuCSharpStyle1_Click);
            // 
            // mnuCSharpStyle2
            // 
            this.mnuCSharpStyle2.Name = "mnuCSharpStyle2";
            this.mnuCSharpStyle2.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle2.Tag = "C#`2";
            this.mnuCSharpStyle2.Text = "Style 2: New line character \\r\\n";
            this.mnuCSharpStyle2.Click += new System.EventHandler(this.mnuCSharpStyle2_Click);
            // 
            // mnuCSharpStyle3
            // 
            this.mnuCSharpStyle3.Name = "mnuCSharpStyle3";
            this.mnuCSharpStyle3.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle3.Tag = "C#`3";
            this.mnuCSharpStyle3.Text = "Style 3: New line character Enviroment.NewLine";
            this.mnuCSharpStyle3.Click += new System.EventHandler(this.mnuCSharpStyle3_Click);
            // 
            // mnuCSharpStyle4
            // 
            this.mnuCSharpStyle4.Name = "mnuCSharpStyle4";
            this.mnuCSharpStyle4.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle4.Tag = "C#`4";
            this.mnuCSharpStyle4.Text = "Style 4: StringBuilder";
            this.mnuCSharpStyle4.Click += new System.EventHandler(this.mnuCSharpStyle4_Click);
            // 
            // mnuSql2VBNet
            // 
            this.mnuSql2VBNet.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuVBNetStyle1,
            this.mnuVBNetStyle2,
            this.mnuVBNetStyle3});
            this.mnuSql2VBNet.Name = "mnuSql2VBNet";
            this.mnuSql2VBNet.Size = new System.Drawing.Size(165, 22);
            this.mnuSql2VBNet.Text = "SQL to VB.Net";
            // 
            // mnuVBNetStyle1
            // 
            this.mnuVBNetStyle1.Name = "mnuVBNetStyle1";
            this.mnuVBNetStyle1.Size = new System.Drawing.Size(340, 22);
            this.mnuVBNetStyle1.Tag = "VB.Net`1";
            this.mnuVBNetStyle1.Text = "Style 1: Using && operator";
            this.mnuVBNetStyle1.Click += new System.EventHandler(this.mnuVBNetStyle1_Click);
            // 
            // mnuVBNetStyle2
            // 
            this.mnuVBNetStyle2.Name = "mnuVBNetStyle2";
            this.mnuVBNetStyle2.Size = new System.Drawing.Size(340, 22);
            this.mnuVBNetStyle2.Tag = "VB.Net`2";
            this.mnuVBNetStyle2.Text = "Style 2: New line character VbCrLf";
            this.mnuVBNetStyle2.Click += new System.EventHandler(this.mnuVBNetStyle2_Click);
            // 
            // mnuVBNetStyle3
            // 
            this.mnuVBNetStyle3.Name = "mnuVBNetStyle3";
            this.mnuVBNetStyle3.Size = new System.Drawing.Size(340, 22);
            this.mnuVBNetStyle3.Tag = "VB.Net`3";
            this.mnuVBNetStyle3.Text = "Style 3: New line character Enviroment.NewLine";
            this.mnuVBNetStyle3.Click += new System.EventHandler(this.mnuVBNetStyle3_Click);
            // 
            // mnuSql2VB6A
            // 
            this.mnuSql2VB6A.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuSql2VB6A.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuVB6AStyle1,
            this.mnuVB6AStyle2});
            this.mnuSql2VB6A.Name = "mnuSql2VB6A";
            this.mnuSql2VB6A.Size = new System.Drawing.Size(165, 22);
            this.mnuSql2VB6A.Text = "SQL to VB6/VBA";
            // 
            // mnuVB6AStyle1
            // 
            this.mnuVB6AStyle1.Name = "mnuVB6AStyle1";
            this.mnuVB6AStyle1.Size = new System.Drawing.Size(262, 22);
            this.mnuVB6AStyle1.Tag = "VB6/VBA`1";
            this.mnuVB6AStyle1.Text = "Style 1: Using && operator";
            this.mnuVB6AStyle1.Click += new System.EventHandler(this.mnuVB6AStyle1_Click);
            // 
            // mnuVB6AStyle2
            // 
            this.mnuVB6AStyle2.Name = "mnuVB6AStyle2";
            this.mnuVB6AStyle2.Size = new System.Drawing.Size(262, 22);
            this.mnuVB6AStyle2.Tag = "VB6/VBA`2";
            this.mnuVB6AStyle2.Text = "Style 2: New line character VbCrLf";
            this.mnuVB6AStyle2.Click += new System.EventHandler(this.mnuVB6AStyle2_Click);
            // 
            // mnuSql2Delphi
            // 
            this.mnuSql2Delphi.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuDelphi6Style1,
            this.mnuDelphi6Style2});
            this.mnuSql2Delphi.Name = "mnuSql2Delphi";
            this.mnuSql2Delphi.Size = new System.Drawing.Size(165, 22);
            this.mnuSql2Delphi.Text = "SQL to Delphi6";
            // 
            // mnuDelphi6Style1
            // 
            this.mnuDelphi6Style1.Name = "mnuDelphi6Style1";
            this.mnuDelphi6Style1.Size = new System.Drawing.Size(268, 22);
            this.mnuDelphi6Style1.Tag = "Delphi6`1";
            this.mnuDelphi6Style1.Text = "Style 1: Using + operator";
            this.mnuDelphi6Style1.Click += new System.EventHandler(this.mnuDelphi6Style1_Click);
            // 
            // mnuDelphi6Style2
            // 
            this.mnuDelphi6Style2.Name = "mnuDelphi6Style2";
            this.mnuDelphi6Style2.Size = new System.Drawing.Size(268, 22);
            this.mnuDelphi6Style2.Tag = "Delphi6`2";
            this.mnuDelphi6Style2.Text = "Style 2: New line character #13#10";
            this.mnuDelphi6Style2.Click += new System.EventHandler(this.mnuDelphi6Style2_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 31);
            // 
            // btnLeftAndRight
            // 
            this.btnLeftAndRight.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLeftAndRight.Image = ((System.Drawing.Image)(resources.GetObject("btnLeftAndRight.Image")));
            this.btnLeftAndRight.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLeftAndRight.Name = "btnLeftAndRight";
            this.btnLeftAndRight.Size = new System.Drawing.Size(28, 28);
            this.btnLeftAndRight.Text = "LeftAndRight";
            this.btnLeftAndRight.ToolTipText = "Save SplitContainer\'s horizontal width";
            this.btnLeftAndRight.Visible = false;
            // 
            // btnUpAndDown
            // 
            this.btnUpAndDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUpAndDown.Image = ((System.Drawing.Image)(resources.GetObject("btnUpAndDown.Image")));
            this.btnUpAndDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUpAndDown.Name = "btnUpAndDown";
            this.btnUpAndDown.Size = new System.Drawing.Size(28, 28);
            this.btnUpAndDown.Text = "UpAndDown";
            this.btnUpAndDown.ToolTipText = "Save SplitContainer\'s vertical height";
            this.btnUpAndDown.Visible = false;
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 31);
            this.toolStripSeparator3.Visible = false;
            // 
            // btnComment
            // 
            this.btnComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnComment.Enabled = false;
            this.btnComment.Image = ((System.Drawing.Image)(resources.GetObject("btnComment.Image")));
            this.btnComment.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnComment.Name = "btnComment";
            this.btnComment.Size = new System.Drawing.Size(28, 28);
            this.btnComment.ToolTipText = "Comment";
            this.btnComment.Click += new System.EventHandler(this.btnComment_Click);
            // 
            // btnRemoveComment
            // 
            this.btnRemoveComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemoveComment.Enabled = false;
            this.btnRemoveComment.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveComment.Image")));
            this.btnRemoveComment.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveComment.Name = "btnRemoveComment";
            this.btnRemoveComment.Size = new System.Drawing.Size(28, 28);
            this.btnRemoveComment.ToolTipText = "Un-Comment";
            this.btnRemoveComment.Click += new System.EventHandler(this.btnRemoveComment_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 31);
            // 
            // btnIndent
            // 
            this.btnIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnIndent.Enabled = false;
            this.btnIndent.Image = ((System.Drawing.Image)(resources.GetObject("btnIndent.Image")));
            this.btnIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnIndent.Name = "btnIndent";
            this.btnIndent.Size = new System.Drawing.Size(28, 28);
            this.btnIndent.ToolTipText = "Indent";
            this.btnIndent.Click += new System.EventHandler(this.btnIndent_Click);
            // 
            // lblIndentWord
            // 
            this.lblIndentWord.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblIndentWord.Name = "lblIndentWord";
            this.lblIndentWord.Size = new System.Drawing.Size(19, 28);
            this.lblIndentWord.Text = "    ";
            // 
            // txtIndentWord2
            // 
            this.txtIndentWord2.BackColor = System.Drawing.Color.Ivory;
            this.txtIndentWord2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtIndentWord2.MaxLength = 1;
            this.txtIndentWord2.Name = "txtIndentWord2";
            this.txtIndentWord2.Size = new System.Drawing.Size(15, 31);
            this.txtIndentWord2.Text = "4";
            this.txtIndentWord2.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtIndentWord2.ToolTipText = "Indent / Un-Indent Width";
            this.txtIndentWord2.Visible = false;
            this.txtIndentWord2.Enter += new System.EventHandler(this.txtIndentWord_Enter);
            this.txtIndentWord2.Leave += new System.EventHandler(this.txtIndentWord_Leave);
            this.txtIndentWord2.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtIndentWord_KeyDown);
            this.txtIndentWord2.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtIndentWord_KeyPress);
            // 
            // btnUnIndent
            // 
            this.btnUnIndent.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnUnIndent.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnUnIndent.Enabled = false;
            this.btnUnIndent.Image = ((System.Drawing.Image)(resources.GetObject("btnUnIndent.Image")));
            this.btnUnIndent.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnUnIndent.Name = "btnUnIndent";
            this.btnUnIndent.Size = new System.Drawing.Size(28, 28);
            this.btnUnIndent.ToolTipText = "Un-Indent";
            this.btnUnIndent.Click += new System.EventHandler(this.btnUnIndent_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 31);
            // 
            // btnHighlightSelection
            // 
            this.btnHighlightSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightSelection.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightSelection.Image")));
            this.btnHighlightSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightSelection.Name = "btnHighlightSelection";
            this.btnHighlightSelection.Size = new System.Drawing.Size(28, 28);
            this.btnHighlightSelection.ToolTipText = "Disable Highlight Selection When Mouse Click";
            this.btnHighlightSelection.Visible = false;
            this.btnHighlightSelection.Click += new System.EventHandler(this.btnHighlightSelection_Click);
            // 
            // btnHighlightSelection2
            // 
            this.btnHighlightSelection2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightSelection2.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightSelection2.Image")));
            this.btnHighlightSelection2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightSelection2.Name = "btnHighlightSelection2";
            this.btnHighlightSelection2.Size = new System.Drawing.Size(28, 28);
            this.btnHighlightSelection2.ToolTipText = "Enable Highlight Selection When Mouse Click";
            this.btnHighlightSelection2.Visible = false;
            this.btnHighlightSelection2.Click += new System.EventHandler(this.btnHighlightSelection_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            this.toolStripSeparator1.Visible = false;
            // 
            // btnWordWrap
            // 
            this.btnWordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap.Image")));
            this.btnWordWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap.Name = "btnWordWrap";
            this.btnWordWrap.Size = new System.Drawing.Size(28, 28);
            this.btnWordWrap.ToolTipText = "Word Wrap";
            this.btnWordWrap.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnWordWrap2
            // 
            this.btnWordWrap2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2.Image")));
            this.btnWordWrap2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2.Name = "btnWordWrap2";
            this.btnWordWrap2.Size = new System.Drawing.Size(28, 28);
            this.btnWordWrap2.ToolTipText = "Word Wrap";
            this.btnWordWrap2.Visible = false;
            this.btnWordWrap2.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnShowAllCharacters
            // 
            this.btnShowAllCharacters.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters.Image")));
            this.btnShowAllCharacters.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters.Name = "btnShowAllCharacters";
            this.btnShowAllCharacters.Size = new System.Drawing.Size(28, 28);
            this.btnShowAllCharacters.ToolTipText = "Show All Characters";
            this.btnShowAllCharacters.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // btnShowAllCharacters2
            // 
            this.btnShowAllCharacters2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2.Image")));
            this.btnShowAllCharacters2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2.Name = "btnShowAllCharacters2";
            this.btnShowAllCharacters2.Size = new System.Drawing.Size(28, 28);
            this.btnShowAllCharacters2.ToolTipText = "Show All Characters";
            this.btnShowAllCharacters2.Visible = false;
            this.btnShowAllCharacters2.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // btnShowIndentGuide
            // 
            this.btnShowIndentGuide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowIndentGuide.Image = ((System.Drawing.Image)(resources.GetObject("btnShowIndentGuide.Image")));
            this.btnShowIndentGuide.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowIndentGuide.Name = "btnShowIndentGuide";
            this.btnShowIndentGuide.Size = new System.Drawing.Size(28, 28);
            this.btnShowIndentGuide.Text = "Show Indent Guide";
            this.btnShowIndentGuide.Visible = false;
            this.btnShowIndentGuide.Click += new System.EventHandler(this.btnShowIndentGuide_Click);
            // 
            // btnShowIndentGuide2
            // 
            this.btnShowIndentGuide2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowIndentGuide2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowIndentGuide2.Image")));
            this.btnShowIndentGuide2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowIndentGuide2.Name = "btnShowIndentGuide2";
            this.btnShowIndentGuide2.Size = new System.Drawing.Size(28, 28);
            this.btnShowIndentGuide2.Text = "Show Indent Guide";
            this.btnShowIndentGuide2.Click += new System.EventHandler(this.btnShowIndentGuide_Click);
            // 
            // tsSeparator
            // 
            this.tsSeparator.Name = "tsSeparator";
            this.tsSeparator.Size = new System.Drawing.Size(6, 31);
            // 
            // lblQueryTimeout
            // 
            this.lblQueryTimeout.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblQueryTimeout.Name = "lblQueryTimeout";
            this.lblQueryTimeout.Size = new System.Drawing.Size(94, 28);
            this.lblQueryTimeout.Text = "Query Timeout:";
            // 
            // lblInfoEditor
            // 
            this.lblInfoEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInfoEditor.AutoSize = true;
            this.lblInfoEditor.Location = new System.Drawing.Point(3, 247);
            this.lblInfoEditor.Name = "lblInfoEditor";
            this.lblInfoEditor.Size = new System.Drawing.Size(0, 16);
            this.lblInfoEditor.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.lblInfoEditor, "(default)");
            // 
            // c1StatusBar2
            // 
            this.c1StatusBar2.Location = new System.Drawing.Point(0, 243);
            this.c1StatusBar2.Name = "c1StatusBar2";
            this.c1StatusBar2.RightPaneItems.Add(this.btnDatabase);
            this.c1StatusBar2.RightPaneItems.Add(this.spDatabase);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorLength);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonLabel2);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorLines);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonSeparator1);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorLn);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonLabel3);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorCol);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonLabel4);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorPos);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonLabel5);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEditorSel);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonSeparator7);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEndOfLineStyle);
            this.c1StatusBar2.RightPaneItems.Add(this.ribbonSeparator8);
            this.c1StatusBar2.RightPaneItems.Add(this.lblEncode);
            this.c1StatusBar2.RightPaneItems.Add(this.lblTemp);
            this.c1StatusBar2.ShowDropDownsOnTop = false;
            this.c1StatusBar2.Size = new System.Drawing.Size(920, 22);
            this.c1StatusBar2.SizingGrip = false;
            this.c1ThemeController1.SetTheme(this.c1StatusBar2, "(default)");
            this.c1StatusBar2.VisualStyle = C1.Win.C1Ribbon.VisualStyle.Windows7;
            this.c1StatusBar2.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1StatusBar2_MouseMove);
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
            // lblEditorLength
            // 
            this.lblEditorLength.Name = "lblEditorLength";
            this.lblEditorLength.Text = "Length:";
            // 
            // ribbonLabel2
            // 
            this.ribbonLabel2.Name = "ribbonLabel2";
            // 
            // lblEditorLines
            // 
            this.lblEditorLines.Name = "lblEditorLines";
            this.lblEditorLines.Text = "Lines:";
            // 
            // ribbonSeparator1
            // 
            this.ribbonSeparator1.Name = "ribbonSeparator1";
            // 
            // lblEditorLn
            // 
            this.lblEditorLn.Name = "lblEditorLn";
            this.lblEditorLn.Text = "Ln:";
            // 
            // ribbonLabel3
            // 
            this.ribbonLabel3.Name = "ribbonLabel3";
            // 
            // lblEditorCol
            // 
            this.lblEditorCol.Name = "lblEditorCol";
            this.lblEditorCol.Text = "Col:";
            // 
            // ribbonLabel4
            // 
            this.ribbonLabel4.Name = "ribbonLabel4";
            // 
            // lblEditorPos
            // 
            this.lblEditorPos.Name = "lblEditorPos";
            this.lblEditorPos.Text = "Pos:";
            // 
            // ribbonLabel5
            // 
            this.ribbonLabel5.Name = "ribbonLabel5";
            // 
            // lblEditorSel
            // 
            this.lblEditorSel.Name = "lblEditorSel";
            this.lblEditorSel.Text = "Sel:";
            // 
            // ribbonSeparator7
            // 
            this.ribbonSeparator7.Name = "ribbonSeparator7";
            // 
            // lblEndOfLineStyle
            // 
            this.lblEndOfLineStyle.Name = "lblEndOfLineStyle";
            this.lblEndOfLineStyle.Text = "Windows (CR LF)";
            // 
            // ribbonSeparator8
            // 
            this.ribbonSeparator8.Name = "ribbonSeparator8";
            // 
            // lblEncode
            // 
            this.lblEncode.Name = "lblEncode";
            this.lblEncode.Text = "UTF-8";
            // 
            // lblTemp
            // 
            this.lblTemp.Name = "lblTemp";
            // 
            // chkShowFilterRow
            // 
            this.chkShowFilterRow.AutoSize = true;
            this.chkShowFilterRow.BackColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowFilterRow.ForeColor = System.Drawing.Color.Black;
            this.chkShowFilterRow.Location = new System.Drawing.Point(110, 2);
            this.chkShowFilterRow.Name = "chkShowFilterRow";
            this.chkShowFilterRow.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowFilterRow.Size = new System.Drawing.Size(83, 22);
            this.chkShowFilterRow.TabIndex = 70;
            this.chkShowFilterRow.Text = "Filter Row";
            this.c1ThemeController1.SetTheme(this.chkShowFilterRow, "(default)");
            this.chkShowFilterRow.UseVisualStyleBackColor = true;
            this.chkShowFilterRow.Value = null;
            this.chkShowFilterRow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowFilterRow.Click += new System.EventHandler(this.chkShowFilterRow_Click);
            // 
            // chkSize
            // 
            this.chkSize.AutoSize = true;
            this.chkSize.BackColor = System.Drawing.Color.Transparent;
            this.chkSize.BorderColor = System.Drawing.Color.Transparent;
            this.chkSize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkSize.ForeColor = System.Drawing.Color.Black;
            this.chkSize.Location = new System.Drawing.Point(168, 2);
            this.chkSize.Name = "chkSize";
            this.chkSize.Padding = new System.Windows.Forms.Padding(1);
            this.chkSize.Size = new System.Drawing.Size(68, 22);
            this.chkSize.TabIndex = 72;
            this.chkSize.Text = "AutoFit";
            this.c1ThemeController1.SetTheme(this.chkSize, "(default)");
            this.chkSize.UseVisualStyleBackColor = true;
            this.chkSize.Value = null;
            this.chkSize.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkSize.Click += new System.EventHandler(this.chkSize_Click);
            // 
            // chkShowColumnComments
            // 
            this.chkShowColumnComments.AutoSize = true;
            this.chkShowColumnComments.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnComments.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnComments.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnComments.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnComments.Location = new System.Drawing.Point(271, 2);
            this.chkShowColumnComments.Name = "chkShowColumnComments";
            this.chkShowColumnComments.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnComments.Size = new System.Drawing.Size(84, 22);
            this.chkShowColumnComments.TabIndex = 87;
            this.chkShowColumnComments.Text = "Comment";
            this.c1ThemeController1.SetTheme(this.chkShowColumnComments, "(default)");
            this.chkShowColumnComments.UseVisualStyleBackColor = true;
            this.chkShowColumnComments.Value = null;
            this.chkShowColumnComments.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowColumnComments.CheckedChanged += new System.EventHandler(this.chkShowColumnComments_CheckedChanged);
            // 
            // btnHelp_RawDataMode
            // 
            this.btnHelp_RawDataMode.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_RawDataMode.Image")));
            this.btnHelp_RawDataMode.Location = new System.Drawing.Point(827, 2);
            this.btnHelp_RawDataMode.Name = "btnHelp_RawDataMode";
            this.btnHelp_RawDataMode.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_RawDataMode.TabIndex = 84;
            this.c1ThemeController1.SetTheme(this.btnHelp_RawDataMode, "(default)");
            this.btnHelp_RawDataMode.UseVisualStyleBackColor = true;
            this.btnHelp_RawDataMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_RawDataMode.Click += new System.EventHandler(this.btnHelp_RawDataMode_Click);
            // 
            // chkRawDataMode
            // 
            this.chkRawDataMode.AutoSize = true;
            this.chkRawDataMode.BackColor = System.Drawing.Color.Transparent;
            this.chkRawDataMode.BorderColor = System.Drawing.Color.Transparent;
            this.chkRawDataMode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkRawDataMode.ForeColor = System.Drawing.Color.Black;
            this.chkRawDataMode.Location = new System.Drawing.Point(856, 2);
            this.chkRawDataMode.Name = "chkRawDataMode";
            this.chkRawDataMode.Padding = new System.Windows.Forms.Padding(1);
            this.chkRawDataMode.Size = new System.Drawing.Size(52, 22);
            this.chkRawDataMode.TabIndex = 82;
            this.chkRawDataMode.Text = "Raw";
            this.c1ThemeController1.SetTheme(this.chkRawDataMode, "(default)");
            this.chkRawDataMode.UseVisualStyleBackColor = true;
            this.chkRawDataMode.Value = null;
            this.chkRawDataMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkRawDataMode.CheckedChanged += new System.EventHandler(this.chkRawDataMode_CheckedChanged);
            this.chkRawDataMode.Click += new System.EventHandler(this.chkRawDataMode_Click);
            // 
            // chkShowColumnType
            // 
            this.chkShowColumnType.AutoSize = true;
            this.chkShowColumnType.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnType.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnType.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnType.Location = new System.Drawing.Point(225, 2);
            this.chkShowColumnType.Name = "chkShowColumnType";
            this.chkShowColumnType.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnType.Size = new System.Drawing.Size(56, 22);
            this.chkShowColumnType.TabIndex = 80;
            this.chkShowColumnType.Text = "Type";
            this.c1ThemeController1.SetTheme(this.chkShowColumnType, "(default)");
            this.chkShowColumnType.UseVisualStyleBackColor = true;
            this.chkShowColumnType.Value = null;
            this.chkShowColumnType.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowColumnType.CheckedChanged += new System.EventHandler(this.chkShowColumnType_CheckedChanged);
            // 
            // c1DockingTab1
            // 
            this.c1DockingTab1.Alignment = System.Windows.Forms.TabAlignment.Bottom;
            this.c1DockingTab1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1DockingTab1.Controls.Add(this.tabMessage);
            this.c1DockingTab1.Controls.Add(this.tabDataGrid);
            this.c1DockingTab1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1DockingTab1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1DockingTab1.Location = new System.Drawing.Point(0, 25);
            this.c1DockingTab1.Name = "c1DockingTab1";
            this.c1DockingTab1.SelectedTabBold = true;
            this.c1DockingTab1.Size = new System.Drawing.Size(920, 239);
            this.c1DockingTab1.TabIndex = 5;
            this.c1DockingTab1.TabsSpacing = 5;
            this.c1ThemeController1.SetTheme(this.c1DockingTab1, "(default)");
            this.c1DockingTab1.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab1.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab1.SelectedTabChanged += new System.EventHandler(this.c1DockingTab1_SelectedTabChanged);
            this.c1DockingTab1.Enter += new System.EventHandler(this.c1DockingTab1_Enter);
            this.c1DockingTab1.Leave += new System.EventHandler(this.c1DockingTab1_Leave);
            // 
            // tabMessage
            // 
            this.tabMessage.Controls.Add(this.pnlEditorMessageWelcome);
            this.tabMessage.Controls.Add(this.editorMessage);
            this.tabMessage.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabMessage.Image = ((System.Drawing.Image)(resources.GetObject("tabMessage.Image")));
            this.tabMessage.Location = new System.Drawing.Point(0, 0);
            this.tabMessage.Name = "tabMessage";
            this.tabMessage.Size = new System.Drawing.Size(920, 212);
            this.tabMessage.TabIndex = 0;
            this.tabMessage.Tag = "Message";
            this.tabMessage.Text = "Message";
            this.tabMessage.Enter += new System.EventHandler(this.tabMessage_Enter);
            this.tabMessage.Leave += new System.EventHandler(this.tabMessage_Leave);
            // 
            // pnlEditorMessageWelcome
            // 
            this.pnlEditorMessageWelcome.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlEditorMessageWelcome.AutoScroll = true;
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome44);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcomeCountdown);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome223);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome222);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome221);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome);
            this.pnlEditorMessageWelcome.Controls.Add(this.BlankLabel);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome43);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome42);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome41);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome4);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome33);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome23);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome32);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome31);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome22);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome21);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome13);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome12);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome11);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome3);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome2);
            this.pnlEditorMessageWelcome.Controls.Add(this.lblWelcome1);
            this.pnlEditorMessageWelcome.Location = new System.Drawing.Point(0, 1);
            this.pnlEditorMessageWelcome.Name = "pnlEditorMessageWelcome";
            this.pnlEditorMessageWelcome.Padding = new System.Windows.Forms.Padding(0, 0, 25, 25);
            this.pnlEditorMessageWelcome.Size = new System.Drawing.Size(920, 210);
            this.pnlEditorMessageWelcome.TabIndex = 43;
            this.c1ThemeController1.SetTheme(this.pnlEditorMessageWelcome, "none");
            // 
            // lblWelcome44
            // 
            this.lblWelcome44.AutoSize = true;
            this.lblWelcome44.Location = new System.Drawing.Point(33, 437);
            this.lblWelcome44.Name = "lblWelcome44";
            this.lblWelcome44.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome44.TabIndex = 22;
            this.lblWelcome44.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome44, "(default)");
            // 
            // lblWelcomeCountdown
            // 
            this.lblWelcomeCountdown.AutoSize = true;
            this.lblWelcomeCountdown.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcomeCountdown.Location = new System.Drawing.Point(492, 16);
            this.lblWelcomeCountdown.Name = "lblWelcomeCountdown";
            this.lblWelcomeCountdown.Size = new System.Drawing.Size(38, 16);
            this.lblWelcomeCountdown.TabIndex = 21;
            this.lblWelcomeCountdown.Text = "02:00";
            this.c1ThemeController1.SetTheme(this.lblWelcomeCountdown, "(default)");
            // 
            // lblWelcome223
            // 
            this.lblWelcome223.AutoSize = true;
            this.lblWelcome223.Location = new System.Drawing.Point(42, 227);
            this.lblWelcome223.Name = "lblWelcome223";
            this.lblWelcome223.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome223.TabIndex = 20;
            this.lblWelcome223.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome223, "(default)");
            // 
            // lblWelcome222
            // 
            this.lblWelcome222.AutoSize = true;
            this.lblWelcome222.Location = new System.Drawing.Point(42, 207);
            this.lblWelcome222.Name = "lblWelcome222";
            this.lblWelcome222.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome222.TabIndex = 19;
            this.lblWelcome222.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome222, "(default)");
            // 
            // lblWelcome221
            // 
            this.lblWelcome221.AutoSize = true;
            this.lblWelcome221.Location = new System.Drawing.Point(42, 187);
            this.lblWelcome221.Name = "lblWelcome221";
            this.lblWelcome221.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome221.TabIndex = 18;
            this.lblWelcome221.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome221, "(default)");
            // 
            // lblWelcome
            // 
            this.lblWelcome.AutoSize = true;
            this.lblWelcome.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome.ForeColor = System.Drawing.Color.Green;
            this.lblWelcome.Location = new System.Drawing.Point(21, 16);
            this.lblWelcome.Name = "lblWelcome";
            this.lblWelcome.Size = new System.Drawing.Size(38, 17);
            this.lblWelcome.TabIndex = 17;
            this.lblWelcome.Text = "label";
            // 
            // BlankLabel
            // 
            this.BlankLabel.AutoSize = true;
            this.BlankLabel.Location = new System.Drawing.Point(33, 466);
            this.BlankLabel.Name = "BlankLabel";
            this.BlankLabel.Size = new System.Drawing.Size(0, 16);
            this.BlankLabel.TabIndex = 16;
            this.c1ThemeController1.SetTheme(this.BlankLabel, "(default)");
            // 
            // lblWelcome43
            // 
            this.lblWelcome43.AutoSize = true;
            this.lblWelcome43.Location = new System.Drawing.Point(33, 417);
            this.lblWelcome43.Name = "lblWelcome43";
            this.lblWelcome43.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome43.TabIndex = 15;
            this.lblWelcome43.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome43, "(default)");
            // 
            // lblWelcome42
            // 
            this.lblWelcome42.AutoSize = true;
            this.lblWelcome42.Location = new System.Drawing.Point(33, 397);
            this.lblWelcome42.Name = "lblWelcome42";
            this.lblWelcome42.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome42.TabIndex = 14;
            this.lblWelcome42.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome42, "(default)");
            // 
            // lblWelcome41
            // 
            this.lblWelcome41.AutoSize = true;
            this.lblWelcome41.Location = new System.Drawing.Point(33, 377);
            this.lblWelcome41.Name = "lblWelcome41";
            this.lblWelcome41.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome41.TabIndex = 13;
            this.lblWelcome41.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome41, "(default)");
            // 
            // lblWelcome4
            // 
            this.lblWelcome4.AutoSize = true;
            this.lblWelcome4.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome4.ForeColor = System.Drawing.Color.Green;
            this.lblWelcome4.Location = new System.Drawing.Point(21, 357);
            this.lblWelcome4.Name = "lblWelcome4";
            this.lblWelcome4.Size = new System.Drawing.Size(46, 17);
            this.lblWelcome4.TabIndex = 12;
            this.lblWelcome4.Text = "label4";
            // 
            // lblWelcome33
            // 
            this.lblWelcome33.AutoSize = true;
            this.lblWelcome33.Location = new System.Drawing.Point(31, 332);
            this.lblWelcome33.Name = "lblWelcome33";
            this.lblWelcome33.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome33.TabIndex = 11;
            this.lblWelcome33.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome33, "(default)");
            // 
            // lblWelcome23
            // 
            this.lblWelcome23.AutoSize = true;
            this.lblWelcome23.Location = new System.Drawing.Point(31, 247);
            this.lblWelcome23.Name = "lblWelcome23";
            this.lblWelcome23.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome23.TabIndex = 10;
            this.lblWelcome23.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome23, "(default)");
            // 
            // lblWelcome32
            // 
            this.lblWelcome32.AutoSize = true;
            this.lblWelcome32.Location = new System.Drawing.Point(31, 312);
            this.lblWelcome32.Name = "lblWelcome32";
            this.lblWelcome32.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome32.TabIndex = 9;
            this.lblWelcome32.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome32, "(default)");
            // 
            // lblWelcome31
            // 
            this.lblWelcome31.AutoSize = true;
            this.lblWelcome31.Location = new System.Drawing.Point(31, 292);
            this.lblWelcome31.Name = "lblWelcome31";
            this.lblWelcome31.Size = new System.Drawing.Size(30, 12);
            this.lblWelcome31.TabIndex = 8;
            this.lblWelcome31.Text = "label:";
            this.c1ThemeController1.SetTheme(this.lblWelcome31, "(default)");
            // 
            // lblWelcome22
            // 
            this.lblWelcome22.AutoSize = true;
            this.lblWelcome22.Location = new System.Drawing.Point(31, 167);
            this.lblWelcome22.Name = "lblWelcome22";
            this.lblWelcome22.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome22.TabIndex = 7;
            this.lblWelcome22.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome22, "(default)");
            // 
            // lblWelcome21
            // 
            this.lblWelcome21.AutoSize = true;
            this.lblWelcome21.Location = new System.Drawing.Point(31, 147);
            this.lblWelcome21.Name = "lblWelcome21";
            this.lblWelcome21.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome21.TabIndex = 6;
            this.lblWelcome21.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome21, "(default)");
            // 
            // lblWelcome13
            // 
            this.lblWelcome13.AutoSize = true;
            this.lblWelcome13.Location = new System.Drawing.Point(31, 102);
            this.lblWelcome13.Name = "lblWelcome13";
            this.lblWelcome13.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome13.TabIndex = 5;
            this.lblWelcome13.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome13, "(default)");
            // 
            // lblWelcome12
            // 
            this.lblWelcome12.AutoSize = true;
            this.lblWelcome12.Location = new System.Drawing.Point(31, 82);
            this.lblWelcome12.Name = "lblWelcome12";
            this.lblWelcome12.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome12.TabIndex = 4;
            this.lblWelcome12.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome12, "(default)");
            // 
            // lblWelcome11
            // 
            this.lblWelcome11.AutoSize = true;
            this.lblWelcome11.Location = new System.Drawing.Point(31, 62);
            this.lblWelcome11.Name = "lblWelcome11";
            this.lblWelcome11.Size = new System.Drawing.Size(33, 12);
            this.lblWelcome11.TabIndex = 3;
            this.lblWelcome11.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblWelcome11, "(default)");
            // 
            // lblWelcome3
            // 
            this.lblWelcome3.AutoSize = true;
            this.lblWelcome3.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome3.ForeColor = System.Drawing.Color.Green;
            this.lblWelcome3.Location = new System.Drawing.Point(19, 272);
            this.lblWelcome3.Name = "lblWelcome3";
            this.lblWelcome3.Size = new System.Drawing.Size(46, 17);
            this.lblWelcome3.TabIndex = 2;
            this.lblWelcome3.Text = "label3";
            // 
            // lblWelcome2
            // 
            this.lblWelcome2.AutoSize = true;
            this.lblWelcome2.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome2.ForeColor = System.Drawing.Color.Green;
            this.lblWelcome2.Location = new System.Drawing.Point(19, 127);
            this.lblWelcome2.Name = "lblWelcome2";
            this.lblWelcome2.Size = new System.Drawing.Size(46, 17);
            this.lblWelcome2.TabIndex = 1;
            this.lblWelcome2.Text = "label2";
            // 
            // lblWelcome1
            // 
            this.lblWelcome1.AutoSize = true;
            this.lblWelcome1.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblWelcome1.ForeColor = System.Drawing.Color.Green;
            this.lblWelcome1.Location = new System.Drawing.Point(19, 42);
            this.lblWelcome1.Name = "lblWelcome1";
            this.lblWelcome1.Size = new System.Drawing.Size(46, 17);
            this.lblWelcome1.TabIndex = 0;
            this.lblWelcome1.Text = "label1";
            // 
            // editorMessage
            // 
            this.editorMessage.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorMessage.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorMessage.CaretLineVisible = true;
            this.editorMessage.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorMessage.Location = new System.Drawing.Point(-1, 0);
            this.editorMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorMessage.Name = "editorMessage";
            this.editorMessage.ReadOnly = true;
            this.editorMessage.Size = new System.Drawing.Size(922, 213);
            this.editorMessage.Styler = null;
            this.editorMessage.TabIndex = 42;
            this.editorMessage.Tag = "";
            this.editorMessage.WhitespaceSize = 3;
            this.editorMessage.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorMessage.Enter += new System.EventHandler(this.editorMessage_Enter);
            this.editorMessage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorMessage_MouseDown);
            this.editorMessage.MouseMove += new System.Windows.Forms.MouseEventHandler(this.editorMessage_MouseMove);
            // 
            // tabDataGrid
            // 
            this.tabDataGrid.CaptionText = "Data Grid";
            this.tabDataGrid.Controls.Add(this.splitContainer4);
            this.tabDataGrid.Location = new System.Drawing.Point(0, 0);
            this.tabDataGrid.Name = "tabDataGrid";
            this.tabDataGrid.Size = new System.Drawing.Size(920, 212);
            this.tabDataGrid.TabIndex = 1;
            this.tabDataGrid.Tag = "Data Grid";
            this.tabDataGrid.Text = "Data Grid";
            this.tabDataGrid.Enter += new System.EventHandler(this.tabDataGrid_Enter);
            this.tabDataGrid.Leave += new System.EventHandler(this.tabDataGrid_Leave);
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.lblColumnNamePosition);
            this.splitContainer4.Panel1.Controls.Add(this.btnHelp_ColumnName);
            this.splitContainer4.Panel1.Controls.Add(this.lblColumnFilterPosition);
            this.splitContainer4.Panel1.Controls.Add(this.txtColumnFilter);
            this.splitContainer4.Panel1.Controls.Add(this.c1GridColumns);
            this.splitContainer4.Panel1.Controls.Add(this.tsColumnFilter);
            this.c1ThemeController1.SetTheme(this.splitContainer4.Panel1, "(default)");
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.c1TrueDBGrid1);
            this.c1ThemeController1.SetTheme(this.splitContainer4.Panel2, "(default)");
            this.splitContainer4.Size = new System.Drawing.Size(920, 212);
            this.splitContainer4.SplitterDistance = 250;
            this.splitContainer4.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.splitContainer4, "(default)");
            this.splitContainer4.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer4_SplitterMoving);
            this.splitContainer4.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer4_SplitterMoved);
            // 
            // lblColumnNamePosition
            // 
            this.lblColumnNamePosition.AutoSize = true;
            this.lblColumnNamePosition.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblColumnNamePosition.Location = new System.Drawing.Point(18, 28);
            this.lblColumnNamePosition.Name = "lblColumnNamePosition";
            this.lblColumnNamePosition.Size = new System.Drawing.Size(13, 15);
            this.lblColumnNamePosition.TabIndex = 133;
            this.lblColumnNamePosition.Text = "F";
            this.c1ThemeController1.SetTheme(this.lblColumnNamePosition, "(default)");
            this.lblColumnNamePosition.Visible = false;
            // 
            // btnHelp_ColumnName
            // 
            this.btnHelp_ColumnName.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ColumnName.Image")));
            this.btnHelp_ColumnName.Location = new System.Drawing.Point(56, 26);
            this.btnHelp_ColumnName.Name = "btnHelp_ColumnName";
            this.btnHelp_ColumnName.Size = new System.Drawing.Size(19, 19);
            this.btnHelp_ColumnName.TabIndex = 132;
            this.c1ThemeController1.SetTheme(this.btnHelp_ColumnName, "(default)");
            this.btnHelp_ColumnName.UseVisualStyleBackColor = true;
            this.btnHelp_ColumnName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ColumnName.Click += new System.EventHandler(this.btnHelp_ColumnName_Click);
            // 
            // lblColumnFilterPosition
            // 
            this.lblColumnFilterPosition.AutoSize = true;
            this.lblColumnFilterPosition.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblColumnFilterPosition.Location = new System.Drawing.Point(0, 15);
            this.lblColumnFilterPosition.Name = "lblColumnFilterPosition";
            this.lblColumnFilterPosition.Size = new System.Drawing.Size(13, 15);
            this.lblColumnFilterPosition.TabIndex = 131;
            this.lblColumnFilterPosition.Text = "F";
            this.c1ThemeController1.SetTheme(this.lblColumnFilterPosition, "(default)");
            this.lblColumnFilterPosition.Visible = false;
            // 
            // txtColumnFilter
            // 
            this.txtColumnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtColumnFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnFilter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtColumnFilter.Location = new System.Drawing.Point(111, 1);
            this.txtColumnFilter.Name = "txtColumnFilter";
            this.txtColumnFilter.Size = new System.Drawing.Size(121, 21);
            this.txtColumnFilter.TabIndex = 92;
            this.txtColumnFilter.Tag = null;
            this.txtColumnFilter.Text = "*";
            this.txtColumnFilter.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtColumnFilter, "(default)");
            this.txtColumnFilter.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtColumnFilter.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtColumnFilter_MouseClick);
            this.txtColumnFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtColumnFilter_KeyDown);
            // 
            // c1GridColumns
            // 
            this.c1GridColumns.AllowSort = false;
            this.c1GridColumns.AllowUpdate = false;
            this.c1GridColumns.AllowUpdateOnBlur = false;
            this.c1GridColumns.AlternatingRows = true;
            this.c1GridColumns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridColumns.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1GridColumns.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridColumns.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridColumns.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridColumns.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridColumns.Images"))));
            this.c1GridColumns.Location = new System.Drawing.Point(0, 25);
            this.c1GridColumns.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridColumns.Name = "c1GridColumns";
            this.c1GridColumns.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridColumns.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridColumns.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridColumns.PreviewInfo.ZoomFactor = 75D;
            this.c1GridColumns.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridColumns.PrintInfo.MeasurementPrinterName = null;
            this.c1GridColumns.RowHeight = 19;
            this.c1GridColumns.Size = new System.Drawing.Size(250, 187);
            this.c1GridColumns.TabIndex = 67;
            this.c1ThemeController1.SetTheme(this.c1GridColumns, "(default)");
            this.c1GridColumns.UseCompatibleTextRendering = false;
            this.c1GridColumns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridColumns_MouseUp);
            this.c1GridColumns.PropBag = resources.GetString("c1GridColumns.PropBag");
            // 
            // tsColumnFilter
            // 
            this.tsColumnFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsColumnFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblColumnFilter});
            this.tsColumnFilter.Location = new System.Drawing.Point(0, 0);
            this.tsColumnFilter.Name = "tsColumnFilter";
            this.tsColumnFilter.Size = new System.Drawing.Size(250, 25);
            this.tsColumnFilter.TabIndex = 68;
            this.tsColumnFilter.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.tsColumnFilter, "(default)");
            // 
            // lblColumnFilter
            // 
            this.lblColumnFilter.Name = "lblColumnFilter";
            this.lblColumnFilter.Size = new System.Drawing.Size(37, 22);
            this.lblColumnFilter.Text = "Filter:";
            // 
            // c1TrueDBGrid1
            // 
            this.c1TrueDBGrid1.AllowSort = false;
            this.c1TrueDBGrid1.AllowUpdate = false;
            this.c1TrueDBGrid1.AllowUpdateOnBlur = false;
            this.c1TrueDBGrid1.AlternatingRows = true;
            this.c1TrueDBGrid1.CaptionHeight = 19;
            this.c1TrueDBGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1TrueDBGrid1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1TrueDBGrid1.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1TrueDBGrid1.Images.Add(((System.Drawing.Image)(resources.GetObject("c1TrueDBGrid1.Images"))));
            this.c1TrueDBGrid1.Location = new System.Drawing.Point(0, 0);
            this.c1TrueDBGrid1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1TrueDBGrid1.Name = "c1TrueDBGrid1";
            this.c1TrueDBGrid1.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1TrueDBGrid1.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1TrueDBGrid1.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1TrueDBGrid1.PreviewInfo.ZoomFactor = 75D;
            this.c1TrueDBGrid1.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1TrueDBGrid1.PrintInfo.MeasurementPrinterName = null;
            this.c1TrueDBGrid1.RowHeight = 17;
            this.c1TrueDBGrid1.Size = new System.Drawing.Size(666, 212);
            this.c1TrueDBGrid1.TabIndex = 66;
            this.c1TrueDBGrid1.UseCompatibleTextRendering = false;
            this.c1TrueDBGrid1.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
            this.c1TrueDBGrid1.OwnerDrawCell += new C1.Win.C1TrueDBGrid.OwnerDrawCellEventHandler(this.c1TrueDBGrid1_OwnerDrawCell);
            this.c1TrueDBGrid1.Scroll += new C1.Win.C1TrueDBGrid.CancelEventHandler(this.c1TrueDBGrid1_Scroll);
            this.c1TrueDBGrid1.Click += new System.EventHandler(this.c1TrueDBGrid1_Click);
            this.c1TrueDBGrid1.KeyUp += new System.Windows.Forms.KeyEventHandler(this.c1TrueDBGrid1_KeyUp);
            this.c1TrueDBGrid1.MouseClick += new System.Windows.Forms.MouseEventHandler(this.c1TrueDBGrid1_MouseClick);
            this.c1TrueDBGrid1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1TrueDBGrid1_MouseDoubleClick);
            this.c1TrueDBGrid1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1TrueDBGrid1_MouseDown);
            this.c1TrueDBGrid1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1TrueDBGrid1_MouseMove);
            this.c1TrueDBGrid1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1TrueDBGrid1_MouseUp);
            this.c1TrueDBGrid1.PropBag = resources.GetString("c1TrueDBGrid1.PropBag");
            // 
            // chkShowGroupingRow
            // 
            this.chkShowGroupingRow.AutoSize = true;
            this.chkShowGroupingRow.BackColor = System.Drawing.Color.Transparent;
            this.chkShowGroupingRow.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowGroupingRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowGroupingRow.ForeColor = System.Drawing.Color.Black;
            this.chkShowGroupingRow.Location = new System.Drawing.Point(636, 2);
            this.chkShowGroupingRow.Name = "chkShowGroupingRow";
            this.chkShowGroupingRow.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowGroupingRow.Size = new System.Drawing.Size(82, 22);
            this.chkShowGroupingRow.TabIndex = 86;
            this.chkShowGroupingRow.Text = "Grouping";
            this.c1ThemeController1.SetTheme(this.chkShowGroupingRow, "(default)");
            this.chkShowGroupingRow.UseVisualStyleBackColor = true;
            this.chkShowGroupingRow.Value = null;
            this.chkShowGroupingRow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowGroupingRow.CheckedChanged += new System.EventHandler(this.chkShowGroupingRow_CheckedChanged);
            this.chkShowGroupingRow.Click += new System.EventHandler(this.chkShowGroupingRow_Click);
            // 
            // cboFindGrid
            // 
            this.cboFindGrid.AllowSpinLoop = false;
            this.cboFindGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboFindGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboFindGrid.Enabled = false;
            this.cboFindGrid.GapHeight = 0;
            this.cboFindGrid.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboFindGrid.ItemsDisplayMember = "";
            this.cboFindGrid.ItemsValueMember = "";
            this.cboFindGrid.Location = new System.Drawing.Point(396, 2);
            this.cboFindGrid.Name = "cboFindGrid";
            this.cboFindGrid.Size = new System.Drawing.Size(110, 21);
            this.cboFindGrid.TabIndex = 75;
            this.cboFindGrid.Tag = null;
            this.cboFindGrid.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboFindGrid, "(default)");
            this.cboFindGrid.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboFindGrid.SelectedIndexChanged += new System.EventHandler(this.cboFindGrid_SelectedIndexChanged);
            this.cboFindGrid.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboFindGrid_BeforeDropDownOpen);
            this.cboFindGrid.TextChanged += new System.EventHandler(this.cboFindGrid_TextChanged);
            this.cboFindGrid.Enter += new System.EventHandler(this.cboFindGrid_Enter);
            this.cboFindGrid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboFindGrid_KeyPress);
            this.cboFindGrid.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboFindGrid_KeyUp);
            this.cboFindGrid.Leave += new System.EventHandler(this.cboFindGrid_Leave);
            // 
            // tsDataGrid
            // 
            this.tsDataGrid.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsDataGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExportToFile,
            this.btnShowColumns,
            this.btnAutoSort,
            this.btnOptions,
            this.toolStripSeparator10,
            this.toolStripLabel2,
            this.lblSpace,
            this.toolStripSeparator11,
            this.lblFindGrid,
            this.toolStripLabel6,
            this.btnFindNextGrid,
            this.btnFindPreviousGrid,
            this.btnCountGrid,
            this.btnHighlightAllGrid,
            this.btnClearHighlightsGrid,
            this.toolStripSeparator2,
            this.toolStripLabel1,
            this.toolStripSeparator12,
            this.lblResultCopyQuotingWith,
            this.toolStripLabel7,
            this.toolStripSeparator13,
            this.toolStripLabel3,
            this.lblResultCopyFieldSeparator,
            this.toolStripLabel8,
            this.toolStripSeparator14});
            this.tsDataGrid.Location = new System.Drawing.Point(0, 0);
            this.tsDataGrid.Name = "tsDataGrid";
            this.tsDataGrid.Size = new System.Drawing.Size(920, 25);
            this.tsDataGrid.TabIndex = 2;
            this.c1ThemeController1.SetTheme(this.tsDataGrid, "(default)");
            this.tsDataGrid.Enter += new System.EventHandler(this.tsDataGrid_Enter);
            this.tsDataGrid.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tsDataGrid_MouseMove);
            // 
            // btnExportToFile
            // 
            this.btnExportToFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExportToFile.Enabled = false;
            this.btnExportToFile.Image = ((System.Drawing.Image)(resources.GetObject("btnExportToFile.Image")));
            this.btnExportToFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportToFile.Name = "btnExportToFile";
            this.btnExportToFile.Size = new System.Drawing.Size(23, 22);
            this.btnExportToFile.ToolTipText = "Export all data to Excel";
            this.btnExportToFile.Click += new System.EventHandler(this.btnExportToFile_Click);
            // 
            // btnShowColumns
            // 
            this.btnShowColumns.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowColumns.Enabled = false;
            this.btnShowColumns.Image = ((System.Drawing.Image)(resources.GetObject("btnShowColumns.Image")));
            this.btnShowColumns.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowColumns.Name = "btnShowColumns";
            this.btnShowColumns.Size = new System.Drawing.Size(23, 22);
            this.btnShowColumns.Text = "toolStripButton1";
            this.btnShowColumns.Click += new System.EventHandler(this.btnShowColumns_Click);
            // 
            // btnAutoSort
            // 
            this.btnAutoSort.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnAutoSort.Enabled = false;
            this.btnAutoSort.Image = ((System.Drawing.Image)(resources.GetObject("btnAutoSort.Image")));
            this.btnAutoSort.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnAutoSort.Name = "btnAutoSort";
            this.btnAutoSort.Size = new System.Drawing.Size(23, 22);
            this.btnAutoSort.ToolTipText = "Auto Sort Data Result based on Column\'s Header";
            this.btnAutoSort.Click += new System.EventHandler(this.btnAutoSort_Click);
            // 
            // btnOptions
            // 
            this.btnOptions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOptions.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuResultCopyQuotingWith,
            this.mnuResultCopyFieldSeparator});
            this.btnOptions.Image = ((System.Drawing.Image)(resources.GetObject("btnOptions.Image")));
            this.btnOptions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(29, 22);
            // 
            // mnuResultCopyQuotingWith
            // 
            this.mnuResultCopyQuotingWith.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuResultCopyQuotingWithNone,
            this.mnuResultCopyQuotingWithDoubleQuoting,
            this.mnuResultCopyQuotingWithSingleQuoting});
            this.mnuResultCopyQuotingWith.Name = "mnuResultCopyQuotingWith";
            this.mnuResultCopyQuotingWith.Size = new System.Drawing.Size(230, 22);
            this.mnuResultCopyQuotingWith.Tag = "None";
            this.mnuResultCopyQuotingWith.Text = "Result Copy Quoting With";
            // 
            // mnuResultCopyQuotingWithNone
            // 
            this.mnuResultCopyQuotingWithNone.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuResultCopyQuotingWithNone.Name = "mnuResultCopyQuotingWithNone";
            this.mnuResultCopyQuotingWithNone.Size = new System.Drawing.Size(106, 22);
            this.mnuResultCopyQuotingWithNone.Tag = "None";
            this.mnuResultCopyQuotingWithNone.Text = "None";
            this.mnuResultCopyQuotingWithNone.Click += new System.EventHandler(this.mnuResultCopyQuotingWith_Click);
            // 
            // mnuResultCopyQuotingWithDoubleQuoting
            // 
            this.mnuResultCopyQuotingWithDoubleQuoting.Name = "mnuResultCopyQuotingWithDoubleQuoting";
            this.mnuResultCopyQuotingWithDoubleQuoting.Size = new System.Drawing.Size(106, 22);
            this.mnuResultCopyQuotingWithDoubleQuoting.Tag = "\"";
            this.mnuResultCopyQuotingWithDoubleQuoting.Text = "\"";
            this.mnuResultCopyQuotingWithDoubleQuoting.Click += new System.EventHandler(this.mnuResultCopyQuotingWith_Click);
            // 
            // mnuResultCopyQuotingWithSingleQuoting
            // 
            this.mnuResultCopyQuotingWithSingleQuoting.Name = "mnuResultCopyQuotingWithSingleQuoting";
            this.mnuResultCopyQuotingWithSingleQuoting.Size = new System.Drawing.Size(106, 22);
            this.mnuResultCopyQuotingWithSingleQuoting.Tag = "\'";
            this.mnuResultCopyQuotingWithSingleQuoting.Text = "\'";
            this.mnuResultCopyQuotingWithSingleQuoting.Click += new System.EventHandler(this.mnuResultCopyQuotingWith_Click);
            // 
            // mnuResultCopyFieldSeparator
            // 
            this.mnuResultCopyFieldSeparator.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuResultCopyFieldSeparatorComma,
            this.mnuResultCopyFieldSeparatorSemicolon,
            this.mnuResultCopyFieldSeparatorI});
            this.mnuResultCopyFieldSeparator.Name = "mnuResultCopyFieldSeparator";
            this.mnuResultCopyFieldSeparator.Size = new System.Drawing.Size(230, 22);
            this.mnuResultCopyFieldSeparator.Tag = ",";
            this.mnuResultCopyFieldSeparator.Text = "Result Copy Field Separator";
            this.mnuResultCopyFieldSeparator.Visible = false;
            // 
            // mnuResultCopyFieldSeparatorComma
            // 
            this.mnuResultCopyFieldSeparatorComma.Name = "mnuResultCopyFieldSeparatorComma";
            this.mnuResultCopyFieldSeparatorComma.Size = new System.Drawing.Size(77, 22);
            this.mnuResultCopyFieldSeparatorComma.Tag = ",";
            this.mnuResultCopyFieldSeparatorComma.Text = ",";
            this.mnuResultCopyFieldSeparatorComma.Click += new System.EventHandler(this.mnuResultCopyFieldSeparator_Click);
            // 
            // mnuResultCopyFieldSeparatorSemicolon
            // 
            this.mnuResultCopyFieldSeparatorSemicolon.Name = "mnuResultCopyFieldSeparatorSemicolon";
            this.mnuResultCopyFieldSeparatorSemicolon.Size = new System.Drawing.Size(77, 22);
            this.mnuResultCopyFieldSeparatorSemicolon.Tag = ";";
            this.mnuResultCopyFieldSeparatorSemicolon.Text = ";";
            this.mnuResultCopyFieldSeparatorSemicolon.Click += new System.EventHandler(this.mnuResultCopyFieldSeparator_Click);
            // 
            // mnuResultCopyFieldSeparatorI
            // 
            this.mnuResultCopyFieldSeparatorI.Name = "mnuResultCopyFieldSeparatorI";
            this.mnuResultCopyFieldSeparatorI.Size = new System.Drawing.Size(77, 22);
            this.mnuResultCopyFieldSeparatorI.Tag = "|";
            this.mnuResultCopyFieldSeparatorI.Text = "|";
            this.mnuResultCopyFieldSeparatorI.Click += new System.EventHandler(this.mnuResultCopyFieldSeparator_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Font = new System.Drawing.Font("Consolas", 2.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(4, 22);
            this.toolStripLabel2.Text = " ";
            // 
            // lblSpace
            // 
            this.lblSpace.Font = new System.Drawing.Font("微軟正黑體", 9F);
            this.lblSpace.Name = "lblSpace";
            this.lblSpace.Size = new System.Drawing.Size(244, 22);
            this.lblSpace.Text = "                                                                               ";
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
            // 
            // lblFindGrid
            // 
            this.lblFindGrid.Name = "lblFindGrid";
            this.lblFindGrid.Size = new System.Drawing.Size(34, 22);
            this.lblFindGrid.Tag = "";
            this.lblFindGrid.Text = "Find:";
            // 
            // toolStripLabel6
            // 
            this.toolStripLabel6.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripLabel6.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel6.Name = "toolStripLabel6";
            this.toolStripLabel6.Size = new System.Drawing.Size(119, 22);
            this.toolStripLabel6.Text = "                ";
            // 
            // btnFindNextGrid
            // 
            this.btnFindNextGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFindNextGrid.Enabled = false;
            this.btnFindNextGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnFindNextGrid.Image")));
            this.btnFindNextGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindNextGrid.Name = "btnFindNextGrid";
            this.btnFindNextGrid.Size = new System.Drawing.Size(23, 22);
            this.btnFindNextGrid.Tag = "";
            this.btnFindNextGrid.ToolTipText = "Find Next";
            this.btnFindNextGrid.Click += new System.EventHandler(this.btnFindNextGrid_Click);
            // 
            // btnFindPreviousGrid
            // 
            this.btnFindPreviousGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFindPreviousGrid.Enabled = false;
            this.btnFindPreviousGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnFindPreviousGrid.Image")));
            this.btnFindPreviousGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindPreviousGrid.Name = "btnFindPreviousGrid";
            this.btnFindPreviousGrid.Size = new System.Drawing.Size(23, 22);
            this.btnFindPreviousGrid.Tag = "";
            this.btnFindPreviousGrid.ToolTipText = "Find Previous";
            this.btnFindPreviousGrid.Click += new System.EventHandler(this.btnFindPreviousGrid_Click);
            // 
            // btnCountGrid
            // 
            this.btnCountGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCountGrid.Enabled = false;
            this.btnCountGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnCountGrid.Image")));
            this.btnCountGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCountGrid.Name = "btnCountGrid";
            this.btnCountGrid.Size = new System.Drawing.Size(23, 22);
            this.btnCountGrid.ToolTipText = "Count";
            this.btnCountGrid.Click += new System.EventHandler(this.btnCountGrid_Click);
            // 
            // btnHighlightAllGrid
            // 
            this.btnHighlightAllGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightAllGrid.Enabled = false;
            this.btnHighlightAllGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightAllGrid.Image")));
            this.btnHighlightAllGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightAllGrid.Name = "btnHighlightAllGrid";
            this.btnHighlightAllGrid.Size = new System.Drawing.Size(23, 22);
            this.btnHighlightAllGrid.ToolTipText = "Highlight All";
            this.btnHighlightAllGrid.Click += new System.EventHandler(this.btnHighlightAllGrid_Click);
            // 
            // btnClearHighlightsGrid
            // 
            this.btnClearHighlightsGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClearHighlightsGrid.Enabled = false;
            this.btnClearHighlightsGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClearHighlightsGrid.Name = "btnClearHighlightsGrid";
            this.btnClearHighlightsGrid.Size = new System.Drawing.Size(23, 22);
            this.btnClearHighlightsGrid.ToolTipText = "Clear Highlights";
            this.btnClearHighlightsGrid.Click += new System.EventHandler(this.btnClearHighlightsGrid_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Font = new System.Drawing.Font("微軟正黑體", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(4, 22);
            this.toolStripLabel1.Text = " ";
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator12.Visible = false;
            // 
            // lblResultCopyQuotingWith
            // 
            this.lblResultCopyQuotingWith.Name = "lblResultCopyQuotingWith";
            this.lblResultCopyQuotingWith.Size = new System.Drawing.Size(153, 22);
            this.lblResultCopyQuotingWith.Text = "Result Copy Quoting with:";
            this.lblResultCopyQuotingWith.Visible = false;
            // 
            // toolStripLabel7
            // 
            this.toolStripLabel7.Name = "toolStripLabel7";
            this.toolStripLabel7.Size = new System.Drawing.Size(64, 22);
            this.toolStripLabel7.Text = "                   ";
            this.toolStripLabel7.Visible = false;
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator13.Visible = false;
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Font = new System.Drawing.Font("微軟正黑體", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(4, 22);
            this.toolStripLabel3.Text = " ";
            // 
            // lblResultCopyFieldSeparator
            // 
            this.lblResultCopyFieldSeparator.Name = "lblResultCopyFieldSeparator";
            this.lblResultCopyFieldSeparator.Size = new System.Drawing.Size(166, 22);
            this.lblResultCopyFieldSeparator.Text = "Result Copy Field Separator:";
            this.lblResultCopyFieldSeparator.Visible = false;
            // 
            // toolStripLabel8
            // 
            this.toolStripLabel8.Name = "toolStripLabel8";
            this.toolStripLabel8.Size = new System.Drawing.Size(43, 22);
            this.toolStripLabel8.Text = "            ";
            this.toolStripLabel8.Visible = false;
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(6, 25);
            this.toolStripSeparator14.Visible = false;
            // 
            // c1StatusBar1
            // 
            this.c1StatusBar1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1StatusBar1.Location = new System.Drawing.Point(0, 535);
            this.c1StatusBar1.Name = "c1StatusBar1";
            this.c1StatusBar1.RightPaneItems.Add(this.lblAverage);
            this.c1StatusBar1.RightPaneItems.Add(this.lblAverageValue);
            this.c1StatusBar1.RightPaneItems.Add(this.lblSeparator1);
            this.c1StatusBar1.RightPaneItems.Add(this.lblCount);
            this.c1StatusBar1.RightPaneItems.Add(this.lblCountValue);
            this.c1StatusBar1.RightPaneItems.Add(this.lblSeparator2);
            this.c1StatusBar1.RightPaneItems.Add(this.lblSummary);
            this.c1StatusBar1.RightPaneItems.Add(this.lblSummaryValue);
            this.c1StatusBar1.RightPaneItems.Add(this.lblSeparator3);
            this.c1StatusBar1.RightPaneItems.Add(this.lblExecTime);
            this.c1StatusBar1.RightPaneItems.Add(this.ribbonSeparator2);
            this.c1StatusBar1.RightPaneItems.Add(this.lblQueryTime);
            this.c1StatusBar1.RightPaneItems.Add(this.ribbonSeparator5);
            this.c1StatusBar1.RightPaneItems.Add(this.lblRows);
            this.c1StatusBar1.RightPaneItems.Add(this.ribbonSeparator3);
            this.c1StatusBar1.RightPaneItems.Add(this.btnPaginationOn);
            this.c1StatusBar1.RightPaneItems.Add(this.btnPaginationOff);
            this.c1StatusBar1.RightPaneItems.Add(this.btnAppendingQueriesOn);
            this.c1StatusBar1.RightPaneItems.Add(this.btnAppendingQueriesOff);
            this.c1StatusBar1.RightPaneItems.Add(this.btnNextPage);
            this.c1StatusBar1.Size = new System.Drawing.Size(1159, 22);
            this.c1StatusBar1.SizingGrip = false;
            this.c1ThemeController1.SetTheme(this.c1StatusBar1, "(default)");
            this.c1StatusBar1.VisualStyle = C1.Win.C1Ribbon.VisualStyle.Windows7;
            this.c1StatusBar1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1StatusBar1_MouseMove);
            // 
            // lblAverage
            // 
            this.lblAverage.Name = "lblAverage";
            this.lblAverage.Text = "Average:";
            // 
            // lblAverageValue
            // 
            this.lblAverageValue.Name = "lblAverageValue";
            this.lblAverageValue.Text = "0";
            // 
            // lblSeparator1
            // 
            this.lblSeparator1.Name = "lblSeparator1";
            this.lblSeparator1.Text = "  ";
            // 
            // lblCount
            // 
            this.lblCount.Name = "lblCount";
            this.lblCount.Text = "Count:";
            // 
            // lblCountValue
            // 
            this.lblCountValue.Name = "lblCountValue";
            this.lblCountValue.Text = "0";
            // 
            // lblSeparator2
            // 
            this.lblSeparator2.Name = "lblSeparator2";
            this.lblSeparator2.Text = "  ";
            // 
            // lblSummary
            // 
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Text = "Sum:";
            // 
            // lblSummaryValue
            // 
            this.lblSummaryValue.Name = "lblSummaryValue";
            this.lblSummaryValue.Text = "0";
            // 
            // lblSeparator3
            // 
            this.lblSeparator3.Name = "lblSeparator3";
            // 
            // lblExecTime
            // 
            this.lblExecTime.Name = "lblExecTime";
            this.lblExecTime.Text = "Exec Time: 00:00.000";
            // 
            // ribbonSeparator2
            // 
            this.ribbonSeparator2.Name = "ribbonSeparator2";
            // 
            // lblQueryTime
            // 
            this.lblQueryTime.Name = "lblQueryTime";
            this.lblQueryTime.Text = "Query Time: 00:00.000";
            // 
            // ribbonSeparator5
            // 
            this.ribbonSeparator5.Name = "ribbonSeparator5";
            // 
            // lblRows
            // 
            this.lblRows.Name = "lblRows";
            this.lblRows.Text = "0 row(s)";
            // 
            // ribbonSeparator3
            // 
            this.ribbonSeparator3.Name = "ribbonSeparator3";
            // 
            // btnPaginationOn
            // 
            this.btnPaginationOn.Name = "btnPaginationOn";
            this.btnPaginationOn.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnPaginationOn.SmallImage")));
            this.btnPaginationOn.Text = "Paging Query";
            this.btnPaginationOn.Click += new System.EventHandler(this.btnPagination_Click);
            // 
            // btnPaginationOff
            // 
            this.btnPaginationOff.Name = "btnPaginationOff";
            this.btnPaginationOff.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnPaginationOff.SmallImage")));
            this.btnPaginationOff.Text = "Paging Query";
            this.btnPaginationOff.Click += new System.EventHandler(this.btnPagination_Click);
            // 
            // btnAppendingQueriesOn
            // 
            this.btnAppendingQueriesOn.Name = "btnAppendingQueriesOn";
            this.btnAppendingQueriesOn.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnAppendingQueriesOn.SmallImage")));
            this.btnAppendingQueriesOn.Text = "Appending Queries";
            this.btnAppendingQueriesOn.Click += new System.EventHandler(this.btnAppendingQueries_Click);
            // 
            // btnAppendingQueriesOff
            // 
            this.btnAppendingQueriesOff.Name = "btnAppendingQueriesOff";
            this.btnAppendingQueriesOff.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnAppendingQueriesOff.SmallImage")));
            this.btnAppendingQueriesOff.Text = "Appending Queries";
            this.btnAppendingQueriesOff.Click += new System.EventHandler(this.btnAppendingQueries_Click);
            // 
            // btnNextPage
            // 
            this.btnNextPage.Name = "btnNextPage";
            this.btnNextPage.SmallImage = ((System.Drawing.Image)(resources.GetObject("btnNextPage.SmallImage")));
            this.btnNextPage.Click += new System.EventHandler(this.btnNextPage_Click);
            // 
            // c1GridAutoCompleteForSpace
            // 
            this.c1GridAutoCompleteForSpace.AllowFilter = false;
            this.c1GridAutoCompleteForSpace.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridAutoCompleteForSpace.AllowUpdate = false;
            this.c1GridAutoCompleteForSpace.AlternatingRows = true;
            this.c1GridAutoCompleteForSpace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridAutoCompleteForSpace.CaptionHeight = 19;
            this.c1GridAutoCompleteForSpace.ColumnHeaders = false;
            this.c1GridAutoCompleteForSpace.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridAutoCompleteForSpace.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridAutoCompleteForSpace.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridAutoCompleteForSpace.Images"))));
            this.c1GridAutoCompleteForSpace.Location = new System.Drawing.Point(929, 45);
            this.c1GridAutoCompleteForSpace.Name = "c1GridAutoCompleteForSpace";
            this.c1GridAutoCompleteForSpace.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridAutoCompleteForSpace.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridAutoCompleteForSpace.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridAutoCompleteForSpace.PreviewInfo.ZoomFactor = 75D;
            this.c1GridAutoCompleteForSpace.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridAutoCompleteForSpace.PrintInfo.MeasurementPrinterName = null;
            this.c1GridAutoCompleteForSpace.RowHeight = 19;
            this.c1GridAutoCompleteForSpace.Size = new System.Drawing.Size(270, 183);
            this.c1GridAutoCompleteForSpace.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridAutoCompleteForSpace.TabIndex = 114;
            this.c1ThemeController1.SetTheme(this.c1GridAutoCompleteForSpace, "(default)");
            this.c1GridAutoCompleteForSpace.UseCompatibleTextRendering = false;
            this.c1GridAutoCompleteForSpace.Visible = false;
            this.c1GridAutoCompleteForSpace.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridAutoCompleteForSpace_FetchCellStyle);
            this.c1GridAutoCompleteForSpace.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1GridAutoCompleteForSpace_KeyDown);
            this.c1GridAutoCompleteForSpace.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridAutoCompleteForSpace_KeyPress);
            this.c1GridAutoCompleteForSpace.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridAutoCompleteForSpace_MouseDoubleClick);
            this.c1GridAutoCompleteForSpace.PropBag = resources.GetString("c1GridAutoCompleteForSpace.PropBag");
            // 
            // tmrlblInfo
            // 
            this.tmrlblInfo.Enabled = true;
            this.tmrlblInfo.Interval = 1000;
            this.tmrlblInfo.Tick += new System.EventHandler(this.tmrlblInfo_Tick);
            // 
            // tmrMother2Child
            // 
            this.tmrMother2Child.Enabled = true;
            this.tmrMother2Child.Tick += new System.EventHandler(this.tmrMother2Child_Tick);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(3, 539);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(42, 16);
            this.lblInfo.TabIndex = 68;
            this.lblInfo.Text = "label1";
            this.c1ThemeController1.SetTheme(this.lblInfo, "(default)");
            // 
            // c1GridAutoCompleteForPeriod
            // 
            this.c1GridAutoCompleteForPeriod.AllowFilter = false;
            this.c1GridAutoCompleteForPeriod.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridAutoCompleteForPeriod.AllowUpdate = false;
            this.c1GridAutoCompleteForPeriod.AlternatingRows = true;
            this.c1GridAutoCompleteForPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridAutoCompleteForPeriod.CaptionHeight = 19;
            this.c1GridAutoCompleteForPeriod.ColumnHeaders = false;
            this.c1GridAutoCompleteForPeriod.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridAutoCompleteForPeriod.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridAutoCompleteForPeriod.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridAutoCompleteForPeriod.Images"))));
            this.c1GridAutoCompleteForPeriod.Location = new System.Drawing.Point(857, 55);
            this.c1GridAutoCompleteForPeriod.Name = "c1GridAutoCompleteForPeriod";
            this.c1GridAutoCompleteForPeriod.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridAutoCompleteForPeriod.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridAutoCompleteForPeriod.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridAutoCompleteForPeriod.PreviewInfo.ZoomFactor = 75D;
            this.c1GridAutoCompleteForPeriod.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridAutoCompleteForPeriod.PrintInfo.MeasurementPrinterName = null;
            this.c1GridAutoCompleteForPeriod.RowHeight = 19;
            this.c1GridAutoCompleteForPeriod.Size = new System.Drawing.Size(270, 183);
            this.c1GridAutoCompleteForPeriod.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridAutoCompleteForPeriod.TabIndex = 84;
            this.c1GridAutoCompleteForPeriod.UseCompatibleTextRendering = false;
            this.c1GridAutoCompleteForPeriod.Visible = false;
            this.c1GridAutoCompleteForPeriod.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Custom;
            this.c1GridAutoCompleteForPeriod.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridAutoCompleteForPeriod_FetchCellStyle);
            this.c1GridAutoCompleteForPeriod.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1GridAutoCompleteForPeriod_KeyDown);
            this.c1GridAutoCompleteForPeriod.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridAutoCompleteForPeriod_KeyPress);
            this.c1GridAutoCompleteForPeriod.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridAutoCompleteForPeriod_MouseDoubleClick);
            this.c1GridAutoCompleteForPeriod.PropBag = resources.GetString("c1GridAutoCompleteForPeriod.PropBag");
            // 
            // tmrExecTime
            // 
            this.tmrExecTime.Tick += new System.EventHandler(this.tmrExecTime_Tick);
            // 
            // tmrlblInfoEditor
            // 
            this.tmrlblInfoEditor.Enabled = true;
            this.tmrlblInfoEditor.Interval = 1000;
            this.tmrlblInfoEditor.Tick += new System.EventHandler(this.tmrlblInfoEditor_Tick);
            // 
            // tmrQueryTime
            // 
            this.tmrQueryTime.Interval = 50;
            this.tmrQueryTime.Tick += new System.EventHandler(this.tmrQueryTime_Tick);
            // 
            // c1CommandHolder1
            // 
            this.c1CommandHolder1.Owner = this;
            // 
            // tmrBackup
            // 
            this.tmrBackup.Tick += new System.EventHandler(this.tmrBackup_Tick);
            // 
            // tmrCheckIdleTime
            // 
            this.tmrCheckIdleTime.Tick += new System.EventHandler(this.tmrCheckIdleTime_Tick);
            // 
            // tmrHideMessageWelcome
            // 
            this.tmrHideMessageWelcome.Enabled = true;
            this.tmrHideMessageWelcome.Interval = 1000;
            this.tmrHideMessageWelcome.Tick += new System.EventHandler(this.tmrHideMessageWelcome_Tick);
            // 
            // QueryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1159, 557);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.c1GridAutoCompleteForPeriod);
            this.Controls.Add(this.c1GridAutoCompleteForSpace);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.c1StatusBar1);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "QueryForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Query";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
            this.Load += new System.EventHandler(this.Form_Load);
            this.SizeChanged += new System.EventHandler(this.Form_SizeChanged);
            this.Leave += new System.EventHandler(this.Form_Leave);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).EndInit();
            this.c1DockingTab2.ResumeLayout(false);
            this.tabAutoReplace.ResumeLayout(false);
            this.tabAutoReplace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoReplaceInfo)).EndInit();
            this.tsAutoReplace.ResumeLayout(false);
            this.tsAutoReplace.PerformLayout();
            this.tabSchemaInformation.ResumeLayout(false);
            this.tabSchemaInformation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtSchemaFilter)).EndInit();
            this.tsSchemaBrowser.ResumeLayout(false);
            this.tsSchemaBrowser.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSchemaBrowser)).EndInit();
            this.tabTabList.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1GridTabList)).EndInit();
            this.tabSqlNavigator.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSqlNavigator)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            this.splitContainer3.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudQueryTimeout)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtIndentWord)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_QueryTimeout)).EndInit();
            this.tsEditor.ResumeLayout(false);
            this.tsEditor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnComments)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_RawDataMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRawDataMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).EndInit();
            this.c1DockingTab1.ResumeLayout(false);
            this.tabMessage.ResumeLayout(false);
            this.pnlEditorMessageWelcome.ResumeLayout(false);
            this.pnlEditorMessageWelcome.PerformLayout();
            this.tabDataGrid.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel1.PerformLayout();
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridColumns)).EndInit();
            this.tsColumnFilter.ResumeLayout(false);
            this.tsColumnFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1TrueDBGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowGroupingRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFindGrid)).EndInit();
            this.tsDataGrid.ResumeLayout(false);
            this.tsDataGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForSpace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoCompleteForPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Ribbon.C1StatusBar c1StatusBar1;
        private System.Windows.Forms.ToolStrip tsEditor;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private C1.Win.C1Command.C1DockingTab c1DockingTab2;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private C1.Win.C1Ribbon.C1StatusBar c1StatusBar2;
        private JasonLibrary.UI.Controls.ScintillaEditor editor;
        private System.Windows.Forms.ToolStrip tsDataGrid;
        private C1.Win.C1Command.C1DockingTab c1DockingTab1;
        private System.Windows.Forms.ToolStripButton btnNew;
        private System.Windows.Forms.ToolStripButton btnOpen;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnSaveRed;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton btnQuery;
        private System.Windows.Forms.ToolStripButton btnSelectCurrentBlock;
        private System.Windows.Forms.ToolStripButton btnExecuteCurrentBlock;
        private System.Windows.Forms.ToolStripButton btnCancelQuery;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripDropDownButton btnCode2Sql;
        private System.Windows.Forms.ToolStripMenuItem mnuCSharp2Sql;
        private System.Windows.Forms.ToolStripMenuItem mnuVB2Sql;
        private System.Windows.Forms.ToolStripMenuItem mnuDelphi2Sql;
        private System.Windows.Forms.ToolStripDropDownButton btnSql2Code;
        private System.Windows.Forms.ToolStripMenuItem mnuSql2CSharp;
        private System.Windows.Forms.ToolStripMenuItem mnuCSharpStyle1;
        private System.Windows.Forms.ToolStripMenuItem mnuCSharpStyle2;
        private System.Windows.Forms.ToolStripMenuItem mnuCSharpStyle3;
        private System.Windows.Forms.ToolStripMenuItem mnuSql2VBNet;
        private System.Windows.Forms.ToolStripMenuItem mnuVBNetStyle1;
        private System.Windows.Forms.ToolStripMenuItem mnuVBNetStyle2;
        private System.Windows.Forms.ToolStripMenuItem mnuVBNetStyle3;
        private System.Windows.Forms.ToolStripMenuItem mnuSql2VB6A;
        private System.Windows.Forms.ToolStripMenuItem mnuVB6AStyle1;
        private System.Windows.Forms.ToolStripMenuItem mnuVB6AStyle2;
        private System.Windows.Forms.ToolStripMenuItem mnuSql2Delphi;
        private System.Windows.Forms.ToolStripMenuItem mnuDelphi6Style1;
        private System.Windows.Forms.ToolStripMenuItem mnuDelphi6Style2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton btnLeftAndRight;
        private System.Windows.Forms.ToolStripButton btnUpAndDown;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnComment;
        private System.Windows.Forms.ToolStripButton btnRemoveComment;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnIndent;
        private System.Windows.Forms.ToolStripTextBox txtIndentWord2;
        private System.Windows.Forms.ToolStripButton btnUnIndent;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnHighlightSelection;
        private System.Windows.Forms.ToolStripButton btnHighlightSelection2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridAutoReplaceInfo;
        private System.Windows.Forms.ToolStripButton btnExportToFile;
        private System.Windows.Forms.ToolStripButton btnAutoSort;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel lblFindGrid;
        private System.Windows.Forms.ToolStripButton btnFindNextGrid;
        private System.Windows.Forms.ToolStripButton btnFindPreviousGrid;
        private System.Windows.Forms.ToolStripButton btnCountGrid;
        private System.Windows.Forms.ToolStripButton btnHighlightAllGrid;
        private System.Windows.Forms.ToolStripButton btnClearHighlightsGrid;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.ToolStripLabel lblResultCopyQuotingWith;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private System.Windows.Forms.ToolStripLabel lblResultCopyFieldSeparator;
        private C1.Win.C1Ribbon.RibbonLabel lblExecTime;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator5;
        private C1.Win.C1Ribbon.RibbonLabel lblRows;
        private C1.Win.C1Command.C1DockingTabPage tabMessage;
        private JasonLibrary.UI.Controls.ScintillaEditor editorMessage;
        private C1.Win.C1Command.C1DockingTabPage tabDataGrid;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1TrueDBGrid1;
        private System.Windows.Forms.Timer tmrlblInfo;
        private System.Windows.Forms.Timer tmrMother2Child;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Input.C1TextBox txtIndentWord;
        private System.Windows.Forms.ToolStripLabel lblIndentWord;
        private System.Windows.Forms.ToolStripLabel lblSpace;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRow;
        private System.Windows.Forms.ToolStripButton btnCommit;
        private System.Windows.Forms.ToolStripButton btnRollback;
        private C1.Win.C1Input.C1CheckBox chkSize;
        private C1.Win.C1Input.C1ComboBox cboFindGrid;
        private System.Windows.Forms.ToolStripLabel toolStripLabel6;
        private System.Windows.Forms.ToolStripLabel toolStripLabel7;
        private System.Windows.Forms.Timer tmrExecTime;
        private C1.Win.C1Input.C1CheckBox chkShowColumnType;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripLabel toolStripLabel8;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorLength;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorLines;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator1;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorLn;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorCol;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorPos;
        private C1.Win.C1Ribbon.RibbonLabel lblEditorSel;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator7;
        private C1.Win.C1Ribbon.RibbonLabel lblEndOfLineStyle;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator8;
        private C1.Win.C1Ribbon.RibbonLabel lblEncode;
        private System.Windows.Forms.Timer tmrlblInfoEditor;
        private C1.Win.C1Ribbon.RibbonLabel lblTemp;
        private C1.Win.C1Ribbon.RibbonLabel ribbonLabel2;
        private C1.Win.C1Ribbon.RibbonLabel ribbonLabel3;
        private C1.Win.C1Ribbon.RibbonLabel ribbonLabel4;
        private C1.Win.C1Ribbon.RibbonLabel ribbonLabel5;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator2;
        private C1.Win.C1Ribbon.RibbonLabel lblQueryTime;
        private System.Windows.Forms.Timer tmrQueryTime;
        private System.Windows.Forms.ToolStripButton btnShowIndentGuide;
        private System.Windows.Forms.ToolStripButton btnShowIndentGuide2;
        private System.Windows.Forms.ToolStripDropDownButton btnOptions;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyQuotingWith;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyQuotingWithNone;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyQuotingWithDoubleQuoting;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyQuotingWithSingleQuoting;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyFieldSeparator;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyFieldSeparatorComma;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyFieldSeparatorSemicolon;
        private System.Windows.Forms.ToolStripMenuItem mnuResultCopyFieldSeparatorI;
        private C1.Win.C1Input.C1CheckBox chkRawDataMode;
        private C1.Win.C1Input.C1Button btnHelp_RawDataMode;
        private C1.C1Excel.C1XLBook c1XLBook1;
        private C1.Win.C1Ribbon.RibbonButton btnPaginationOn;
        private C1.Win.C1Ribbon.RibbonButton btnPaginationOff;
        private C1.Win.C1Ribbon.RibbonButton btnNextPage;
        private C1.Win.C1Ribbon.RibbonSeparator ribbonSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private C1.Win.C1Ribbon.RibbonLabel lblAverage;
        private C1.Win.C1Ribbon.RibbonLabel lblAverageValue;
        private C1.Win.C1Ribbon.RibbonLabel lblSeparator1;
        private C1.Win.C1Ribbon.RibbonLabel lblCount;
        private C1.Win.C1Ribbon.RibbonLabel lblCountValue;
        private C1.Win.C1Ribbon.RibbonLabel lblSeparator2;
        private C1.Win.C1Ribbon.RibbonLabel lblSummary;
        private C1.Win.C1Ribbon.RibbonLabel lblSummaryValue;
        private C1.Win.C1Ribbon.RibbonSeparator lblSeparator3;
        private C1.Win.C1Command.C1DockingTabPage tabAutoReplace;
        private System.Windows.Forms.ToolStrip tsAutoReplace;
        private System.Windows.Forms.ToolStripLabel lblAutoReplace;
        private C1.Win.C1Command.C1DockingTabPage tabSchemaInformation;
        private System.Windows.Forms.ToolStrip tsSchemaBrowser;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridSchemaBrowser;
        private C1.Win.C1Command.C1CommandHolder c1CommandHolder1;
        private System.Windows.Forms.ToolStripSplitButton btnExpandCollapse;
        private C1.Win.C1Input.C1TextBox txtSchemaFilter;
        private System.Windows.Forms.ToolStripMenuItem mnuCollapseAll;
        private System.Windows.Forms.ToolStripMenuItem mnuExpandAll;
        private System.Windows.Forms.Label lblSchemaFilterPosition;
        private System.Windows.Forms.ToolStripLabel lblSchemaFilter;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private C1.Win.C1Ribbon.RibbonButton btnAppendingQueriesOn;
        private C1.Win.C1Ribbon.RibbonButton btnAppendingQueriesOff;
        private C1.Win.C1Ribbon.RibbonSeparator spDatabase;
        private C1.Win.C1Ribbon.RibbonButton btnDatabase;
        private System.Windows.Forms.ToolStripSplitButton btnSettingOfFocus;
        private System.Windows.Forms.ToolStripMenuItem mnuFocusOnDataGrid;
        private System.Windows.Forms.ToolStripMenuItem mnuFocusOnQueryEditor;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridAutoCompleteForPeriod;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridAutoCompleteForSpace;
        private C1.Win.C1Input.C1CheckBox chkShowGroupingRow;
        private System.Windows.Forms.NumericUpDown nudQueryTimeout;
        private System.Windows.Forms.ToolStripLabel lblQueryTimeout;
        private System.Windows.Forms.Label lblQueryTimeoutPosition;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnExecuteCurrentLine;
        private System.Windows.Forms.ToolStripButton btnSelectCurrentLine;
        private System.Windows.Forms.ToolStripButton btnRemoveTrailingBlanks;
        private System.Windows.Forms.Timer tmrBackup;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private C1.Win.C1Input.C1CheckBox chkShowColumnComments;
        private System.Windows.Forms.Label lblInfoEditor;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridColumns;
        private System.Windows.Forms.ToolStripButton btnShowColumns;
        private System.Windows.Forms.ToolStrip tsColumnFilter;
        private System.Windows.Forms.ToolStripLabel lblColumnFilter;
        private C1.Win.C1Input.C1TextBox txtColumnFilter;
        private System.Windows.Forms.Label lblColumnFilterPosition;
        private C1.Win.C1Input.C1Button btnHelp_ColumnName;
        private System.Windows.Forms.Label lblColumnNamePosition;
        private C1.Win.C1Command.C1DockingTabPage tabTabList;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridTabList;
        private C1.Win.C1Command.C1DockingTabPage tabSqlNavigator;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridSqlNavigator;
        private C1.Win.C1Input.C1Button btnHelp_QueryTimeout;
        private System.Windows.Forms.Label lblAutoReplacePosition;
        private C1.Win.C1Input.C1Button btnHelp_AutoReplace;
        private System.Windows.Forms.Timer tmrCheckIdleTime;
        private System.Windows.Forms.ToolStripMenuItem mnuCSharpStyle4;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ToolStripButton btnHelp_SchemaFilter;
        private System.Windows.Forms.ToolStripSeparator tsSeparator;
        private System.Windows.Forms.Panel pnlEditorMessageWelcome;
        private System.Windows.Forms.Label lblWelcome13;
        private System.Windows.Forms.Label lblWelcome12;
        private System.Windows.Forms.Label lblWelcome11;
        private System.Windows.Forms.Label lblWelcome3;
        private System.Windows.Forms.Label lblWelcome2;
        private System.Windows.Forms.Label lblWelcome1;
        private System.Windows.Forms.Label lblWelcome32;
        private System.Windows.Forms.Label lblWelcome31;
        private System.Windows.Forms.Label lblWelcome22;
        private System.Windows.Forms.Label lblWelcome21;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridAutoCompleteForAll;
        private System.Windows.Forms.Timer tmrHideMessageWelcome;
        private System.Windows.Forms.Label lblWelcome23;
        private System.Windows.Forms.Label lblWelcome33;
        private System.Windows.Forms.Label lblWelcome43;
        private System.Windows.Forms.Label lblWelcome42;
        private System.Windows.Forms.Label lblWelcome41;
        private System.Windows.Forms.Label lblWelcome4;
        private System.Windows.Forms.Label BlankLabel;
        private System.Windows.Forms.Label lblWelcome;
        private System.Windows.Forms.Label lblWelcome222;
        private System.Windows.Forms.Label lblWelcome221;
        private System.Windows.Forms.Label lblWelcome223;
        private System.Windows.Forms.Label lblWelcomeCountdown;
        private System.Windows.Forms.Label lblWelcome44;
    }
}