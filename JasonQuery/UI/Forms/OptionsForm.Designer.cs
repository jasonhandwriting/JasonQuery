using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    sealed partial class OptionsForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OptionsForm));
            this.grpEditorColors = new System.Windows.Forms.GroupBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblToolstripBackground = new System.Windows.Forms.Label();
            this.pnlString = new System.Windows.Forms.Panel();
            this.lblSelectedTextBackground = new System.Windows.Forms.Label();
            this.lblString = new System.Windows.Forms.Label();
            this.pnlToolstripBackground = new System.Windows.Forms.Panel();
            this.lblCharacter = new System.Windows.Forms.Label();
            this.pnlSelectedTextBackground = new System.Windows.Forms.Panel();
            this.pnlBuiltInKeywords = new System.Windows.Forms.Panel();
            this.lblUserDefinedKeywords = new System.Windows.Forms.Label();
            this.pnlCharacter = new System.Windows.Forms.Panel();
            this.pnlUserDefinedKeywords = new System.Windows.Forms.Panel();
            this.lblBuiltInKeywords = new System.Windows.Forms.Label();
            this.pnlOperatorKeywords = new System.Windows.Forms.Panel();
            this.lblUserTables = new System.Windows.Forms.Label();
            this.lblOperatorKeywords = new System.Windows.Forms.Label();
            this.pnlIdentifier = new System.Windows.Forms.Panel();
            this.pnlUserFunctions = new System.Windows.Forms.Panel();
            this.pnlUserTables = new System.Windows.Forms.Panel();
            this.lblUserFunctions = new System.Windows.Forms.Label();
            this.lblIdentifier = new System.Windows.Forms.Label();
            this.lblWhiteSpace = new System.Windows.Forms.Label();
            this.lblBuiltInFunctions = new System.Windows.Forms.Label();
            this.pnlWhiteSpace = new System.Windows.Forms.Panel();
            this.pnlComments = new System.Windows.Forms.Panel();
            this.pnlOperatorSymbol = new System.Windows.Forms.Panel();
            this.pnlBuiltInFunctions = new System.Windows.Forms.Panel();
            this.lblOperatorSymbol = new System.Windows.Forms.Label();
            this.lblComments = new System.Windows.Forms.Label();
            this.lblEditorBackground = new System.Windows.Forms.Label();
            this.lblNumber = new System.Windows.Forms.Label();
            this.lblCurrentLineBackground = new System.Windows.Forms.Label();
            this.pnlCurrentLineBackground = new System.Windows.Forms.Panel();
            this.pnlEditorBackground = new System.Windows.Forms.Panel();
            this.pnlNumber = new System.Windows.Forms.Panel();
            this.grpPreferences = new System.Windows.Forms.GroupBox();
            this.btnHelp_SelectCurrentSqlBlock = new C1.Win.C1Input.C1Button();
            this.grpHighlightStyle = new System.Windows.Forms.GroupBox();
            this.cboHighlightStyle = new C1.Win.C1Input.C1ComboBox();
            this.cboHighlightAlpha = new C1.Win.C1Input.C1ComboBox();
            this.cboHighlightOutlineAlpha = new C1.Win.C1Input.C1ComboBox();
            this.lblStarHighlight = new System.Windows.Forms.Label();
            this.lblHighlightColorAlpha = new System.Windows.Forms.Label();
            this.lblHighlightColorOutlineAlpha = new System.Windows.Forms.Label();
            this.lblHighlightColorStyle = new System.Windows.Forms.Label();
            this.pnlHighlightForeColor = new System.Windows.Forms.Panel();
            this.lblHighlightColorForeColor = new System.Windows.Forms.Label();
            this.grpIndent = new System.Windows.Forms.GroupBox();
            this.chkReplaceTabWithSpace = new C1.Win.C1Input.C1CheckBox();
            this.lblStarTabWidth = new System.Windows.Forms.Label();
            this.chkShowIndentGuide = new C1.Win.C1Input.C1CheckBox();
            this.cboTabWidth = new C1.Win.C1Input.C1ComboBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.lblTabWidth = new System.Windows.Forms.Label();
            this.lblIndentMode = new System.Windows.Forms.Label();
            this.cboIndentMode = new C1.Win.C1Input.C1ComboBox();
            this.lblStarShowIndentGuide = new System.Windows.Forms.Label();
            this.grpIndicate = new System.Windows.Forms.GroupBox();
            this.cboBookmarkStyle = new C1.Win.C1Input.C1ComboBox();
            this.editorIndicator = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.lblStarIndicate = new System.Windows.Forms.Label();
            this.lblErrorLineBackground = new System.Windows.Forms.Label();
            this.pnlErrorLineBackground = new System.Windows.Forms.Panel();
            this.lblBookmarkBackground = new System.Windows.Forms.Label();
            this.pnlBookmarkBackground = new System.Windows.Forms.Panel();
            this.lblBookmarkStyle = new System.Windows.Forms.Label();
            this.grpWordWrap = new System.Windows.Forms.GroupBox();
            this.chkMargin = new C1.Win.C1Input.C1CheckBox();
            this.chkEnd = new C1.Win.C1Input.C1CheckBox();
            this.chkStart = new C1.Win.C1Input.C1CheckBox();
            this.chkWordWrap = new C1.Win.C1Input.C1CheckBox();
            this.lblStarWordWrap = new System.Windows.Forms.Label();
            this.chkOpenFileOnCurrentTab = new C1.Win.C1Input.C1CheckBox();
            this.chkShowSaveAsButton = new C1.Win.C1Input.C1CheckBox();
            this.cboEditorZoom = new C1.Win.C1Input.C1ComboBox();
            this.cboEditorFontSize = new C1.Win.C1Input.C1ComboBox();
            this.cboSaveAsEncoding = new C1.Win.C1Input.C1ComboBox();
            this.chkEntireBlankRowAsEmptyRow4SelectBlock = new C1.Win.C1Input.C1CheckBox();
            this.chkHighlightSelectedText = new C1.Win.C1Input.C1CheckBox();
            this.chkSaveAsEncoding = new C1.Win.C1Input.C1CheckBox();
            this.chkCopyAsHTML = new C1.Win.C1Input.C1CheckBox();
            this.chkBold = new C1.Win.C1Input.C1CheckBox();
            this.cboEditorFontPicker = new C1.Win.C1Input.C1FontPicker();
            this.lblStarHighlightSelection = new System.Windows.Forms.Label();
            this.lblEditorFontName = new System.Windows.Forms.Label();
            this.lblEditorFontSize = new System.Windows.Forms.Label();
            this.lblEditorZoom = new System.Windows.Forms.Label();
            this.chkShowAllCharacters = new C1.Win.C1Input.C1CheckBox();
            this.chkHighlightSelection = new C1.Win.C1Input.C1CheckBox();
            this.grpColorTheme = new System.Windows.Forms.GroupBox();
            this.btnHelp_DarkMode = new C1.Win.C1Input.C1Button();
            this.chkDarkMode = new C1.Win.C1Input.C1CheckBox();
            this.grpQueryEditorPreview = new System.Windows.Forms.GroupBox();
            this.tsEditor = new System.Windows.Forms.ToolStrip();
            this.btnNew = new System.Windows.Forms.ToolStripButton();
            this.btnOpen = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveRed = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.btnQuery = new System.Windows.Forms.ToolStripButton();
            this.btnSelectCurrentBlock = new System.Windows.Forms.ToolStripButton();
            this.btnExecuteCurrentBlock = new System.Windows.Forms.ToolStripButton();
            this.btnCancelQuery = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCode2Sql = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuCSharp2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuVB2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDephi2Sql = new System.Windows.Forms.ToolStripMenuItem();
            this.btnSql2Code = new System.Windows.Forms.ToolStripDropDownButton();
            this.mnuSql2CSharp = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle1 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCSharpStyle3 = new System.Windows.Forms.ToolStripMenuItem();
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
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnComment = new System.Windows.Forms.ToolStripButton();
            this.btnRemoveComment = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnIndent = new System.Windows.Forms.ToolStripButton();
            this.txtIndentWord = new System.Windows.Forms.ToolStripTextBox();
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
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.editor = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpAutoComplete = new System.Windows.Forms.GroupBox();
            this.chkFirstCharChecking = new C1.Win.C1Input.C1CheckBox();
            this.nudMinFragmentLength = new System.Windows.Forms.NumericUpDown();
            this.lblMinFragmentLength = new System.Windows.Forms.Label();
            this.grpAutoCompleteFor = new System.Windows.Forms.GroupBox();
            this.chkUserDefinedViews = new C1.Win.C1Input.C1CheckBox();
            this.chkUserDefinedTriggers = new C1.Win.C1Input.C1CheckBox();
            this.chkUserDefinedTables = new C1.Win.C1Input.C1CheckBox();
            this.chkUserDefinedFunctions = new C1.Win.C1Input.C1CheckBox();
            this.chkUserDefinedKeywords = new C1.Win.C1Input.C1CheckBox();
            this.chkBuiltInKeywords = new C1.Win.C1Input.C1CheckBox();
            this.chkBuiltInFunctions = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_EnableAutoComplete = new C1.Win.C1Input.C1Button();
            this.lblStarAutoComplete = new System.Windows.Forms.Label();
            this.chkEnableAutoComplete = new C1.Win.C1Input.C1CheckBox();
            this.grpAutoReplace = new System.Windows.Forms.GroupBox();
            this.lblAutoReplaceInfo2 = new System.Windows.Forms.Label();
            this.chkShowFilterRowAutoReplace = new C1.Win.C1Input.C1CheckBox();
            this.lblAutoReplaceInfo1 = new System.Windows.Forms.Label();
            this.grpModifyDefinitionAutoReplace = new System.Windows.Forms.GroupBox();
            this.btnHelp_Symbol = new C1.Win.C1Input.C1Button();
            this.editorAutoReplace = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.lblSymbolTips = new System.Windows.Forms.Label();
            this.txtKeyword = new C1.Win.C1Input.C1TextBox();
            this.btnClearAutoReplace = new C1.Win.C1Input.C1Button();
            this.btnCancelAutoReplace = new C1.Win.C1Input.C1Button();
            this.btnSaveAutoReplace = new C1.Win.C1Input.C1Button();
            this.lblReplacement = new System.Windows.Forms.Label();
            this.lblKeyword = new System.Windows.Forms.Label();
            this.grpDefinitionAutoReplace = new System.Windows.Forms.GroupBox();
            this.btnDeleteAutoReplace = new C1.Win.C1Input.C1Button();
            this.btnEditAutoReplace = new C1.Win.C1Input.C1Button();
            this.btnAddAutoReplace = new C1.Win.C1Input.C1Button();
            this.c1GridAutoReplaceInfo = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.btnHelp_EnableAutoReplace = new C1.Win.C1Input.C1Button();
            this.lblStarAutoReplace = new System.Windows.Forms.Label();
            this.chkEnableAutoReplace = new C1.Win.C1Input.C1CheckBox();
            this.btnHelp_ColumnComment = new C1.Win.C1Input.C1Button();
            this.chkShowColumnComment = new C1.Win.C1Input.C1CheckBox();
            this.chkShowGroupingRow = new C1.Win.C1Input.C1CheckBox();
            this.chkSetFocusAfterQuery = new C1.Win.C1Input.C1CheckBox();
            this.chkCtrlMouseWheel = new C1.Win.C1Input.C1CheckBox();
            this.cboGridRowHeightResizing = new C1.Win.C1Input.C1ComboBox();
            this.cboGridFontSize = new C1.Win.C1Input.C1ComboBox();
            this.cboGridVisualStyle = new C1.Win.C1Input.C1ComboBox();
            this.cboResultCopyQuotingWith = new C1.Win.C1Input.C1ComboBox();
            this.cboMaxWidth = new C1.Win.C1Input.C1ComboBox();
            this.chkResize = new C1.Win.C1Input.C1CheckBox();
            this.chkShowFilterRow = new C1.Win.C1Input.C1CheckBox();
            this.chkShowColumnType = new C1.Win.C1Input.C1CheckBox();
            this.cboGridFontPicker = new C1.Win.C1Input.C1FontPicker();
            this.lblMaxWidth = new System.Windows.Forms.Label();
            this.lblGridVisualStyle = new System.Windows.Forms.Label();
            this.lblGridFontSize = new System.Windows.Forms.Label();
            this.lblGridFontName = new System.Windows.Forms.Label();
            this.lblResultCopyQuotingWith = new System.Windows.Forms.Label();
            this.grpNullValueStyle = new System.Windows.Forms.GroupBox();
            this.lblStarNullValueStyle = new System.Windows.Forms.Label();
            this.cboNullShowAs = new C1.Win.C1Input.C1ComboBox();
            this.pnlNullValueForeColor = new System.Windows.Forms.Panel();
            this.lblNullValueForeColor = new System.Windows.Forms.Label();
            this.lblNullValueShowAs = new System.Windows.Forms.Label();
            this.lblGridRowHeightResizing = new System.Windows.Forms.Label();
            this.grpDataGridColor = new System.Windows.Forms.GroupBox();
            this.lblGridHeadingForeColor = new System.Windows.Forms.Label();
            this.lblStarGridColor = new System.Windows.Forms.Label();
            this.pnlGridHeadingForeColor = new System.Windows.Forms.Panel();
            this.pnlGridSelectedBackColor = new System.Windows.Forms.Panel();
            this.lblGridSelectedBackColor = new System.Windows.Forms.Label();
            this.pnlGridSelectedForeColor = new System.Windows.Forms.Panel();
            this.lblGridEvenRowForeColor = new System.Windows.Forms.Label();
            this.lblGridSelectedForeColor = new System.Windows.Forms.Label();
            this.lblGridHighlightForeColor = new System.Windows.Forms.Label();
            this.lblGridOddRowBackColor = new System.Windows.Forms.Label();
            this.pnlGridEvenRowBackColor = new System.Windows.Forms.Panel();
            this.lblGridOddRowForeColor = new System.Windows.Forms.Label();
            this.pnlGridHighlightForeColor = new System.Windows.Forms.Panel();
            this.lblGridEvenRowBackColor = new System.Windows.Forms.Label();
            this.lblGridHighlightBackColor = new System.Windows.Forms.Label();
            this.pnlGridHighlightBackColor = new System.Windows.Forms.Panel();
            this.pnlGridOddRowForeColor = new System.Windows.Forms.Panel();
            this.pnlGridOddRowBackColor = new System.Windows.Forms.Panel();
            this.pnlGridEvenRowForeColor = new System.Windows.Forms.Panel();
            this.grpPreviewGrid = new System.Windows.Forms.GroupBox();
            this.cboFindGrid = new C1.Win.C1Input.C1ComboBox();
            this.tsGrid = new System.Windows.Forms.ToolStrip();
            this.lblFindGrid = new System.Windows.Forms.ToolStripLabel();
            this.cboFindGrid3 = new System.Windows.Forms.ToolStripComboBox();
            this.btnFindNextGrid = new System.Windows.Forms.ToolStripButton();
            this.btnFindPreviousGrid = new System.Windows.Forms.ToolStripButton();
            this.btnCountGrid = new System.Windows.Forms.ToolStripButton();
            this.btnHighlightAllGrid = new System.Windows.Forms.ToolStripButton();
            this.btnClearHighlightsGrid = new System.Windows.Forms.ToolStripButton();
            this.c1GridVisualStyle = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.grpOperatorKeywords = new System.Windows.Forms.GroupBox();
            this.grpFindOperatorKeywords = new System.Windows.Forms.GroupBox();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.txtFindOperatorKeywords = new System.Windows.Forms.ToolStripTextBox();
            this.btnNextOperatorKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousOperatorKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnCloseFindOperatorKeywords = new System.Windows.Forms.ToolStripButton();
            this.picOperatorKeywords = new System.Windows.Forms.PictureBox();
            this.editorOperatorKeywords = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpBuiltInFunctions = new System.Windows.Forms.GroupBox();
            this.grpFindBuiltInFunctions = new System.Windows.Forms.GroupBox();
            this.toolStrip3 = new System.Windows.Forms.ToolStrip();
            this.txtFindBuiltInFunctions = new System.Windows.Forms.ToolStripTextBox();
            this.btnNextBuiltInFunctions = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousBuiltInFunctions = new System.Windows.Forms.ToolStripButton();
            this.btnCloseFindBuiltInFunctions = new System.Windows.Forms.ToolStripButton();
            this.picBuiltInFunctions = new System.Windows.Forms.PictureBox();
            this.lbl1 = new System.Windows.Forms.Label();
            this.editorBuiltInFunctions = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpBuiltInKeywords = new System.Windows.Forms.GroupBox();
            this.grpFindBuiltInKeywords = new System.Windows.Forms.GroupBox();
            this.toolStrip4 = new System.Windows.Forms.ToolStrip();
            this.txtFindBuiltInKeywords = new System.Windows.Forms.ToolStripTextBox();
            this.btnNextBuiltInKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousBuiltInKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnCloseFindBuiltInKeywords = new System.Windows.Forms.ToolStripButton();
            this.picBuiltInKeywords = new System.Windows.Forms.PictureBox();
            this.lbl2 = new System.Windows.Forms.Label();
            this.editorBuiltInKeywords = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpUserDefinedKeywords = new System.Windows.Forms.GroupBox();
            this.grpFindUserDefinedKeywords = new System.Windows.Forms.GroupBox();
            this.toolStrip5 = new System.Windows.Forms.ToolStrip();
            this.txtFindUserDefinedKeywords = new System.Windows.Forms.ToolStripTextBox();
            this.btnNextUserDefinedKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousUserDefinedKeywords = new System.Windows.Forms.ToolStripButton();
            this.btnCloseFindUserDefinedKeywords = new System.Windows.Forms.ToolStripButton();
            this.picUserDefinedKeywords = new System.Windows.Forms.PictureBox();
            this.editorUserDefinedKeywords = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpSqlToCode = new System.Windows.Forms.GroupBox();
            this.txtStringBuilderVariableName = new C1.Win.C1Input.C1TextBox();
            this.lblStringBuilderVariableName = new System.Windows.Forms.Label();
            this.txtSqlVariableName = new C1.Win.C1Input.C1TextBox();
            this.chkStripCode = new C1.Win.C1Input.C1CheckBox();
            this.grpSqlStatementCode = new System.Windows.Forms.GroupBox();
            this.editorSqlToCode = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpPreviewSql = new System.Windows.Forms.GroupBox();
            this.editorSqlToCodePreview = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.lblSqlVariableName = new System.Windows.Forms.Label();
            this.grpStyle = new System.Windows.Forms.GroupBox();
            this.lblStyle4 = new System.Windows.Forms.Label();
            this.rdoStyle4 = new System.Windows.Forms.RadioButton();
            this.lblStyle3 = new System.Windows.Forms.Label();
            this.lblStyle2 = new System.Windows.Forms.Label();
            this.lblStyle1 = new System.Windows.Forms.Label();
            this.rdoStyle1 = new System.Windows.Forms.RadioButton();
            this.rdoStyle3 = new System.Windows.Forms.RadioButton();
            this.rdoStyle2 = new System.Windows.Forms.RadioButton();
            this.grpLanguage = new System.Windows.Forms.GroupBox();
            this.lstLanguage = new System.Windows.Forms.ListBox();
            this.grpSqlFormatter = new System.Windows.Forms.GroupBox();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.grpFormattingOptions = new System.Windows.Forms.GroupBox();
            this.cboSqlFormatterEngine = new System.Windows.Forms.ComboBox();
            this.lblSqlFormatterEngine = new System.Windows.Forms.Label();
            this.cboSqlFormatterIndentSize = new System.Windows.Forms.ComboBox();
            this.lblSqlFormatterIndentSize = new System.Windows.Forms.Label();
            this.cboSqlFormatterBlankLines = new System.Windows.Forms.ComboBox();
            this.lblSqlFormatterBlankLines = new System.Windows.Forms.Label();
            this.cboSqlFormatterListItemsPerLine = new System.Windows.Forms.ComboBox();
            this.lblSqlFormatterListItemsPerLine = new System.Windows.Forms.Label();
            this.txtMaxWidth = new C1.Win.C1Input.C1TextBox();
            this.chkConvertCaseForKeywords = new C1.Win.C1Input.C1CheckBox();
            this.rdoLowerCase = new System.Windows.Forms.RadioButton();
            this.rdoUpperCase = new System.Windows.Forms.RadioButton();
            this.lblMaxWidth2 = new System.Windows.Forms.Label();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.grpSqlStatementFormatter = new System.Windows.Forms.GroupBox();
            this.editorSqlFormatter = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.grpPreviewFormatter = new System.Windows.Forms.GroupBox();
            this.lblSqlFormatterPreviewStatus = new System.Windows.Forms.Label();
            this.editorSqlFormatterPreview = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.lblGlobalOverview = new System.Windows.Forms.Label();
            this.grpCommitRollbackIcon = new System.Windows.Forms.GroupBox();
            this.pictureBox9 = new System.Windows.Forms.PictureBox();
            this.pictureBox10 = new System.Windows.Forms.PictureBox();
            this.rdoCommitRollbackIconStyle6 = new System.Windows.Forms.RadioButton();
            this.pictureBox11 = new System.Windows.Forms.PictureBox();
            this.pictureBox12 = new System.Windows.Forms.PictureBox();
            this.rdoCommitRollbackIconStyle5 = new System.Windows.Forms.RadioButton();
            this.lblCommitRollbackIconInfo = new System.Windows.Forms.Label();
            this.pictureBox7 = new System.Windows.Forms.PictureBox();
            this.pictureBox8 = new System.Windows.Forms.PictureBox();
            this.rdoCommitRollbackIconStyle4 = new System.Windows.Forms.RadioButton();
            this.pictureBox6 = new System.Windows.Forms.PictureBox();
            this.pictureBox5 = new System.Windows.Forms.PictureBox();
            this.rdoCommitRollbackIconStyle3 = new System.Windows.Forms.RadioButton();
            this.lblCommitRollbackIcon = new System.Windows.Forms.Label();
            this.lblStarCommitRollbackIcon = new System.Windows.Forms.Label();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.rdoCommitRollbackIconStyle2 = new System.Windows.Forms.RadioButton();
            this.rdoCommitRollbackIconStyle1 = new System.Windows.Forms.RadioButton();
            this.grpBackup = new System.Windows.Forms.GroupBox();
            this.btnClear3 = new C1.Win.C1Input.C1Button();
            this.chkAskMeBeforeOpenUnsavedFiles = new C1.Win.C1Input.C1CheckBox();
            this.btnBackupPathOpenFolder = new C1.Win.C1Input.C1Button();
            this.btnBrowseBackupPath = new C1.Win.C1Input.C1Button();
            this.txtBackupPath = new C1.Win.C1Input.C1TextBox();
            this.lblBackupPath = new System.Windows.Forms.Label();
            this.chkRememberUnsavedFiles = new C1.Win.C1Input.C1CheckBox();
            this.lblStarBackup = new System.Windows.Forms.Label();
            this.chkEnableBackup = new C1.Win.C1Input.C1CheckBox();
            this.grpMaxEntries = new System.Windows.Forms.GroupBox();
            this.lblMaxEntriesInfo = new System.Windows.Forms.Label();
            this.txtMyFavorite = new C1.Win.C1Input.C1TextBox();
            this.txtRecentFiles = new C1.Win.C1Input.C1TextBox();
            this.lblMyFavorite = new System.Windows.Forms.Label();
            this.lblRecentFiles = new System.Windows.Forms.Label();
            this.grpGeneral = new System.Windows.Forms.GroupBox();
            this.lblStarShowDatabaseName = new System.Windows.Forms.Label();
            this.chkShowDatabaseName = new C1.Win.C1Input.C1CheckBox();
            this.lblStarShowIP = new System.Windows.Forms.Label();
            this.chkShowIP = new C1.Win.C1Input.C1CheckBox();
            this.lblStarShowVersion = new System.Windows.Forms.Label();
            this.chkShowVersion = new C1.Win.C1Input.C1CheckBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            this.cboDateFormat = new C1.Win.C1Input.C1ComboBox();
            this.lblDateFormat = new System.Windows.Forms.Label();
            this.lblLocalization = new System.Windows.Forms.Label();
            this.grpMainFormTabVisualStyle = new System.Windows.Forms.GroupBox();
            this.lblStarMainFormTab = new System.Windows.Forms.Label();
            this.chkMultiLine = new C1.Win.C1Input.C1CheckBox();
            this.chkHoverSelect = new C1.Win.C1Input.C1CheckBox();
            this.chkShowArrows = new C1.Win.C1Input.C1CheckBox();
            this.chkShrinkPages = new C1.Win.C1Input.C1CheckBox();
            this.grpMainFormWindowsState = new System.Windows.Forms.GroupBox();
            this.rdoNormal = new System.Windows.Forms.RadioButton();
            this.rdoMaximized = new System.Windows.Forms.RadioButton();
            this.chkTabBold = new System.Windows.Forms.CheckBox();
            this.tabExample = new Crownwood.Magic.Controls.TabControl();
            this.tabPage1 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage2 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage3 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage4 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage5 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage6 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage7 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage8 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage9 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage10 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage11 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage12 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage13 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage14 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage15 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage16 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage17 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage18 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage19 = new Crownwood.Magic.Controls.TabPage();
            this.tabPage20 = new Crownwood.Magic.Controls.TabPage();
            this.grpAppearance = new System.Windows.Forms.GroupBox();
            this.rdoMultiBox = new System.Windows.Forms.RadioButton();
            this.rdoMultiForm = new System.Windows.Forms.RadioButton();
            this.rdoMultiDocument = new System.Windows.Forms.RadioButton();
            this.grpMainFormTabStyle = new System.Windows.Forms.GroupBox();
            this.rdoPlain = new System.Windows.Forms.RadioButton();
            this.rdoIDE = new System.Windows.Forms.RadioButton();
            this.grpOptionsTab = new System.Windows.Forms.GroupBox();
            this.lblOptionsTabInactiveForeColor = new System.Windows.Forms.Label();
            this.pnlOptionsTabInactiveForeColor = new System.Windows.Forms.Panel();
            this.c1DockingTab1 = new C1.Win.C1Command.C1DockingTab();
            this.tabGlobal2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabGeneral2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabQueryEditor2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabAutoComplete2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabAutoReplace2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabDataGrid2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabKeywords2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabSqlToCode2 = new C1.Win.C1Command.C1DockingTabPage();
            this.tabSqlFormatter2 = new C1.Win.C1Command.C1DockingTabPage();
            this.lblOptionsTabActiveForeColor = new System.Windows.Forms.Label();
            this.lblOptionsTabActiveBackColor = new System.Windows.Forms.Label();
            this.pnlOptionsTabActiveForeColor = new System.Windows.Forms.Panel();
            this.pnlOptionsTabActiveBackColor = new System.Windows.Forms.Panel();
            this.grpCheckForUpdate = new System.Windows.Forms.GroupBox();
            this.rdoCheckOnly = new System.Windows.Forms.RadioButton();
            this.grpCheckOnly = new System.Windows.Forms.GroupBox();
            this.rdoCheckForUpdates0 = new System.Windows.Forms.RadioButton();
            this.rdoCheckForUpdates1 = new System.Windows.Forms.RadioButton();
            this.rdoCheckForUpdates7 = new System.Windows.Forms.RadioButton();
            this.rdoDonotCheck = new System.Windows.Forms.RadioButton();
            this.txtHeightCode = new System.Windows.Forms.TextBox();
            this.txtHeightFormatter = new System.Windows.Forms.TextBox();
            this.button1 = new System.Windows.Forms.Button();
            this.lblRequireRestart = new System.Windows.Forms.Label();
            this.lblStarRequireToRestart = new System.Windows.Forms.Label();
            this.timerMother2Child = new System.Windows.Forms.Timer(this.components);
            this.c1DockingTab = new C1.Win.C1Command.C1DockingTab();
            this.tabGlobal = new C1.Win.C1Command.C1DockingTabPage();
            this.c1DockingTab4 = new C1.Win.C1Command.C1DockingTab();
            this.tabGlobalSettings = new C1.Win.C1Command.C1DockingTabPage();
            this.tabSafetySettings = new C1.Win.C1Command.C1DockingTabPage();
            this.grpLargeBinaryDisplayStrategy = new System.Windows.Forms.GroupBox();
            this.lblLargeBinary = new System.Windows.Forms.Label();
            this.grpLargeTextDisplayStrategy = new System.Windows.Forms.GroupBox();
            this.lblLargeTextNote = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPreviewLength = new System.Windows.Forms.Label();
            this.lblCharacters = new System.Windows.Forms.Label();
            this.cboLargeTextPreviewLength = new C1.Win.C1Input.C1ComboBox();
            this.lblLargeText = new System.Windows.Forms.Label();
            this.grpConnectionSafety = new System.Windows.Forms.GroupBox();
            this.btnHelp_PendingWarning = new C1.Win.C1Input.C1Button();
            this.btnHelp_DisconnectAfterSelect = new C1.Win.C1Input.C1Button();
            this.lblReminder5Minutes = new System.Windows.Forms.Label();
            this.chkPendingWarning = new C1.Win.C1Input.C1CheckBox();
            this.chkDisconnectAfterSelect = new C1.Win.C1Input.C1CheckBox();
            this.tabUpdateSettings = new C1.Win.C1Command.C1DockingTabPage();
            this.grpUpdateInformationSource = new System.Windows.Forms.GroupBox();
            this.btnHelp_LocalUpdateFolder = new C1.Win.C1Input.C1Button();
            this.lblGitHubUpdateUrl = new System.Windows.Forms.Label();
            this.lblOfficialWebsiteUpdateUrl = new System.Windows.Forms.Label();
            this.btnLocalFolderOpenFolder = new C1.Win.C1Input.C1Button();
            this.btnBrowseLocalFolder = new C1.Win.C1Input.C1Button();
            this.txtLocalFolder = new C1.Win.C1Input.C1TextBox();
            this.rdoUpdateSourceLocal = new System.Windows.Forms.RadioButton();
            this.rdoUpdateSourceGitHub = new System.Windows.Forms.RadioButton();
            this.rdoUpdateSourceOfficialWebsite = new System.Windows.Forms.RadioButton();
            this.tabGeneral = new C1.Win.C1Command.C1DockingTabPage();
            this.grpSqlHistory = new System.Windows.Forms.GroupBox();
            this.cboSqlHistoryRetentionDays = new C1.Win.C1Input.C1ComboBox();
            this.chkAutoDeleteSqlHistory = new System.Windows.Forms.CheckBox();
            this.grpDefaultDirectory = new System.Windows.Forms.GroupBox();
            this.btnBrowseFavaritePath = new C1.Win.C1Input.C1Button();
            this.txtFavoriteDirectory = new C1.Win.C1Input.C1TextBox();
            this.lblStarDefaultDirectory = new System.Windows.Forms.Label();
            this.rdoFavoriteDirectory = new System.Windows.Forms.RadioButton();
            this.rdoDefaultDirectory = new System.Windows.Forms.RadioButton();
            this.grpOpenSqlFile = new System.Windows.Forms.GroupBox();
            this.btnClear2 = new C1.Win.C1Input.C1Button();
            this.btnClear1 = new C1.Win.C1Input.C1Button();
            this.btnSpecifiedSqlFile2 = new C1.Win.C1Input.C1Button();
            this.txtSpecifiedSQLFile2 = new C1.Win.C1Input.C1TextBox();
            this.lblFile2 = new System.Windows.Forms.Label();
            this.btnSpecifiedSqlFile1 = new C1.Win.C1Input.C1Button();
            this.txtSpecifiedSQLFile1 = new C1.Win.C1Input.C1TextBox();
            this.lblFile1 = new System.Windows.Forms.Label();
            this.tabQueryEditor = new C1.Win.C1Command.C1DockingTabPage();
            this.grpStatementCompletion = new System.Windows.Forms.GroupBox();
            this.btnHelp_AutoListMembers = new C1.Win.C1Input.C1Button();
            this.btnHelp_SavePoint = new C1.Win.C1Input.C1Button();
            this.chkSavePoint = new System.Windows.Forms.CheckBox();
            this.chkAutoListMembers = new System.Windows.Forms.CheckBox();
            this.grpSchemaInformation = new System.Windows.Forms.GroupBox();
            this.chkDefaultTabSchemaInformation = new C1.Win.C1Input.C1CheckBox();
            this.lblStarShowColumnInfo = new System.Windows.Forms.Label();
            this.lblStarSortByColumnName = new System.Windows.Forms.Label();
            this.btnHelp_ShowColumnInfo = new C1.Win.C1Input.C1Button();
            this.chkShowColumnInfo = new C1.Win.C1Input.C1CheckBox();
            this.chkSortByColumnName = new C1.Win.C1Input.C1CheckBox();
            this.tabAutoComplete = new C1.Win.C1Command.C1DockingTabPage();
            this.tabAutoReplace = new C1.Win.C1Command.C1DockingTabPage();
            this.tabDataGrid = new C1.Win.C1Command.C1DockingTabPage();
            this.c1DockingTab2 = new C1.Win.C1Command.C1DockingTab();
            this.tabDataGridQueryResultBehavior = new C1.Win.C1Command.C1DockingTabPage();
            this.btnHelp_AppendQueryResult = new C1.Win.C1Input.C1Button();
            this.btnHelp_PagedQuery = new C1.Win.C1Input.C1Button();
            this.btnHelp_RawDataMode = new C1.Win.C1Input.C1Button();
            this.chkPagedQuery = new C1.Win.C1Input.C1CheckBox();
            this.lblRowsPerPage = new System.Windows.Forms.Label();
            this.chkRawDataMode = new C1.Win.C1Input.C1CheckBox();
            this.cboRowsPerPage = new C1.Win.C1Input.C1ComboBox();
            this.chkAppendQueryResult = new C1.Win.C1Input.C1CheckBox();
            this.tabDataGridInteraction = new C1.Win.C1Command.C1DockingTabPage();
            this.lblStarDirection = new System.Windows.Forms.Label();
            this.chkDirection = new C1.Win.C1Input.C1CheckBox();
            this.cboDirection = new C1.Win.C1Input.C1ComboBox();
            this.tabDataGridAppearance = new C1.Win.C1Command.C1DockingTabPage();
            this.cboResultCopyFieldSeparator = new C1.Win.C1Input.C1ComboBox();
            this.lblResultCopyFieldSeparator = new System.Windows.Forms.Label();
            this.tabKeywords = new C1.Win.C1Command.C1DockingTabPage();
            this.splitContainer4 = new System.Windows.Forms.SplitContainer();
            this.splitContainer5 = new System.Windows.Forms.SplitContainer();
            this.splitContainer6 = new System.Windows.Forms.SplitContainer();
            this.tabSqlToCode = new C1.Win.C1Command.C1DockingTabPage();
            this.tabSqlFormatter = new C1.Win.C1Command.C1DockingTabPage();
            this.label1 = new System.Windows.Forms.Label();
            this.c1CheckBox4 = new C1.Win.C1Input.C1CheckBox();
            this.c1CheckBox5 = new C1.Win.C1Input.C1CheckBox();
            this.c1CheckBox1 = new C1.Win.C1Input.C1CheckBox();
            this.c1Button1 = new C1.Win.C1Input.C1Button();
            this.c1DockingTab5 = new C1.Win.C1Command.C1DockingTab();
            this.c1DockingTabPage7 = new C1.Win.C1Command.C1DockingTabPage();
            this.c1CheckBox26 = new C1.Win.C1Input.C1CheckBox();
            this.timerTitle = new System.Windows.Forms.Timer(this.components);
            this.pnlCopySettings = new System.Windows.Forms.Panel();
            this.toolStrip6 = new System.Windows.Forms.ToolStrip();
            this.tsCopyFrom = new System.Windows.Forms.ToolStripDropDownButton();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.btnCopySettings = new C1.Win.C1Input.C1SplitButton();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnApply = new C1.Win.C1Input.C1Button();
            this.btnRestoreDefaults = new C1.Win.C1Input.C1Button();
            this.label11 = new System.Windows.Forms.Label();
            this.c1ComboBox10 = new C1.Win.C1Input.C1ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.grpEditorColors.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpPreferences.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SelectCurrentSqlBlock)).BeginInit();
            this.grpHighlightStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightStyle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightAlpha)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightOutlineAlpha)).BeginInit();
            this.grpIndent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkReplaceTabWithSpace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowIndentGuide)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTabWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboIndentMode)).BeginInit();
            this.grpIndicate.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboBookmarkStyle)).BeginInit();
            this.grpWordWrap.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkMargin)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkStart)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkWordWrap)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOpenFileOnCurrentTab)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowSaveAsButton)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorZoom)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSaveAsEncoding)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEntireBlankRowAsEmptyRow4SelectBlock)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHighlightSelectedText)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSaveAsEncoding)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBold)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorFontPicker)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowAllCharacters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHighlightSelection)).BeginInit();
            this.grpColorTheme.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DarkMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDarkMode)).BeginInit();
            this.grpQueryEditorPreview.SuspendLayout();
            this.tsEditor.SuspendLayout();
            this.grpAutoComplete.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkFirstCharChecking)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinFragmentLength)).BeginInit();
            this.grpAutoCompleteFor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedViews)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedTriggers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedTables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedFunctions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedKeywords)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBuiltInKeywords)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBuiltInFunctions)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_EnableAutoComplete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableAutoComplete)).BeginInit();
            this.grpAutoReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRowAutoReplace)).BeginInit();
            this.grpModifyDefinitionAutoReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Symbol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtKeyword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClearAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancelAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveAutoReplace)).BeginInit();
            this.grpDefinitionAutoReplace.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnDeleteAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEditAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAddAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoReplaceInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_EnableAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableAutoReplace)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnComment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnComment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowGroupingRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSetFocusAfterQuery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCtrlMouseWheel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridRowHeightResizing)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridVisualStyle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboResultCopyQuotingWith)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMaxWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkResize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontPicker)).BeginInit();
            this.grpNullValueStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboNullShowAs)).BeginInit();
            this.grpDataGridColor.SuspendLayout();
            this.grpPreviewGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboFindGrid)).BeginInit();
            this.tsGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVisualStyle)).BeginInit();
            this.grpOperatorKeywords.SuspendLayout();
            this.grpFindOperatorKeywords.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOperatorKeywords)).BeginInit();
            this.grpBuiltInFunctions.SuspendLayout();
            this.grpFindBuiltInFunctions.SuspendLayout();
            this.toolStrip3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBuiltInFunctions)).BeginInit();
            this.grpBuiltInKeywords.SuspendLayout();
            this.grpFindBuiltInKeywords.SuspendLayout();
            this.toolStrip4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBuiltInKeywords)).BeginInit();
            this.grpUserDefinedKeywords.SuspendLayout();
            this.grpFindUserDefinedKeywords.SuspendLayout();
            this.toolStrip5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picUserDefinedKeywords)).BeginInit();
            this.grpSqlToCode.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtStringBuilderVariableName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSqlVariableName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkStripCode)).BeginInit();
            this.grpSqlStatementCode.SuspendLayout();
            this.grpPreviewSql.SuspendLayout();
            this.grpStyle.SuspendLayout();
            this.grpLanguage.SuspendLayout();
            this.grpSqlFormatter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.grpFormattingOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxWidth)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkConvertCaseForKeywords)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.grpSqlStatementFormatter.SuspendLayout();
            this.grpPreviewFormatter.SuspendLayout();
            this.grpCommitRollbackIcon.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.grpBackup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAskMeBeforeOpenUnsavedFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBackupPathOpenFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseBackupPath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBackupPath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberUnsavedFiles)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableBackup)).BeginInit();
            this.grpMaxEntries.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMyFavorite)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecentFiles)).BeginInit();
            this.grpGeneral.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowDatabaseName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowIP)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowVersion)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDateFormat)).BeginInit();
            this.grpMainFormTabVisualStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkMultiLine)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHoverSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowArrows)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShrinkPages)).BeginInit();
            this.grpMainFormWindowsState.SuspendLayout();
            this.tabExample.SuspendLayout();
            this.grpAppearance.SuspendLayout();
            this.grpMainFormTabStyle.SuspendLayout();
            this.grpOptionsTab.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).BeginInit();
            this.c1DockingTab1.SuspendLayout();
            this.grpCheckForUpdate.SuspendLayout();
            this.grpCheckOnly.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab)).BeginInit();
            this.c1DockingTab.SuspendLayout();
            this.tabGlobal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab4)).BeginInit();
            this.c1DockingTab4.SuspendLayout();
            this.tabGlobalSettings.SuspendLayout();
            this.tabSafetySettings.SuspendLayout();
            this.grpLargeBinaryDisplayStrategy.SuspendLayout();
            this.grpLargeTextDisplayStrategy.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLargeTextPreviewLength)).BeginInit();
            this.grpConnectionSafety.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PendingWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DisconnectAfterSelect)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPendingWarning)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisconnectAfterSelect)).BeginInit();
            this.tabUpdateSettings.SuspendLayout();
            this.grpUpdateInformationSource.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_LocalUpdateFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLocalFolderOpenFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseLocalFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLocalFolder)).BeginInit();
            this.tabGeneral.SuspendLayout();
            this.grpSqlHistory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboSqlHistoryRetentionDays)).BeginInit();
            this.grpDefaultDirectory.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFavaritePath)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFavoriteDirectory)).BeginInit();
            this.grpOpenSqlFile.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSpecifiedSqlFile2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSpecifiedSQLFile2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSpecifiedSqlFile1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSpecifiedSQLFile1)).BeginInit();
            this.tabQueryEditor.SuspendLayout();
            this.grpStatementCompletion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoListMembers)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SavePoint)).BeginInit();
            this.grpSchemaInformation.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkDefaultTabSchemaInformation)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ShowColumnInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSortByColumnName)).BeginInit();
            this.tabAutoComplete.SuspendLayout();
            this.tabAutoReplace.SuspendLayout();
            this.tabDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).BeginInit();
            this.c1DockingTab2.SuspendLayout();
            this.tabDataGridQueryResultBehavior.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AppendQueryResult)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PagedQuery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_RawDataMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPagedQuery)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRawDataMode)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRowsPerPage)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAppendQueryResult)).BeginInit();
            this.tabDataGridInteraction.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkDirection)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDirection)).BeginInit();
            this.tabDataGridAppearance.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboResultCopyFieldSeparator)).BeginInit();
            this.tabKeywords.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).BeginInit();
            this.splitContainer4.Panel1.SuspendLayout();
            this.splitContainer4.Panel2.SuspendLayout();
            this.splitContainer4.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).BeginInit();
            this.splitContainer5.Panel1.SuspendLayout();
            this.splitContainer5.Panel2.SuspendLayout();
            this.splitContainer5.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).BeginInit();
            this.splitContainer6.Panel1.SuspendLayout();
            this.splitContainer6.Panel2.SuspendLayout();
            this.splitContainer6.SuspendLayout();
            this.tabSqlToCode.SuspendLayout();
            this.tabSqlFormatter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox5)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Button1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab5)).BeginInit();
            this.c1DockingTab5.SuspendLayout();
            this.c1DockingTabPage7.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox26)).BeginInit();
            this.pnlCopySettings.SuspendLayout();
            this.toolStrip6.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopySettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRestoreDefaults)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox10)).BeginInit();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpEditorColors
            // 
            this.grpEditorColors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpEditorColors.BackColor = System.Drawing.Color.Transparent;
            this.grpEditorColors.Controls.Add(this.panel1);
            this.grpEditorColors.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpEditorColors.Location = new System.Drawing.Point(12, 200);
            this.grpEditorColors.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpEditorColors.Name = "grpEditorColors";
            this.grpEditorColors.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpEditorColors.Size = new System.Drawing.Size(317, 473);
            this.grpEditorColors.TabIndex = 24;
            this.grpEditorColors.TabStop = false;
            this.grpEditorColors.Text = "Editor Colors";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.AutoScroll = true;
            this.panel1.AutoScrollMinSize = new System.Drawing.Size(300, 545);
            this.panel1.Controls.Add(this.lblToolstripBackground);
            this.panel1.Controls.Add(this.pnlString);
            this.panel1.Controls.Add(this.lblSelectedTextBackground);
            this.panel1.Controls.Add(this.lblString);
            this.panel1.Controls.Add(this.pnlToolstripBackground);
            this.panel1.Controls.Add(this.lblCharacter);
            this.panel1.Controls.Add(this.pnlSelectedTextBackground);
            this.panel1.Controls.Add(this.pnlBuiltInKeywords);
            this.panel1.Controls.Add(this.lblUserDefinedKeywords);
            this.panel1.Controls.Add(this.pnlCharacter);
            this.panel1.Controls.Add(this.pnlUserDefinedKeywords);
            this.panel1.Controls.Add(this.lblBuiltInKeywords);
            this.panel1.Controls.Add(this.pnlOperatorKeywords);
            this.panel1.Controls.Add(this.lblUserTables);
            this.panel1.Controls.Add(this.lblOperatorKeywords);
            this.panel1.Controls.Add(this.pnlIdentifier);
            this.panel1.Controls.Add(this.pnlUserFunctions);
            this.panel1.Controls.Add(this.pnlUserTables);
            this.panel1.Controls.Add(this.lblUserFunctions);
            this.panel1.Controls.Add(this.lblIdentifier);
            this.panel1.Controls.Add(this.lblWhiteSpace);
            this.panel1.Controls.Add(this.lblBuiltInFunctions);
            this.panel1.Controls.Add(this.pnlWhiteSpace);
            this.panel1.Controls.Add(this.pnlComments);
            this.panel1.Controls.Add(this.pnlOperatorSymbol);
            this.panel1.Controls.Add(this.pnlBuiltInFunctions);
            this.panel1.Controls.Add(this.lblOperatorSymbol);
            this.panel1.Controls.Add(this.lblComments);
            this.panel1.Controls.Add(this.lblEditorBackground);
            this.panel1.Controls.Add(this.lblNumber);
            this.panel1.Controls.Add(this.lblCurrentLineBackground);
            this.panel1.Controls.Add(this.pnlCurrentLineBackground);
            this.panel1.Controls.Add(this.pnlEditorBackground);
            this.panel1.Controls.Add(this.pnlNumber);
            this.panel1.Location = new System.Drawing.Point(0, 10);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(317, 458);
            this.panel1.TabIndex = 39;
            this.c1ThemeController1.SetTheme(this.panel1, "(default)");
            // 
            // lblToolstripBackground
            // 
            this.lblToolstripBackground.Location = new System.Drawing.Point(26, 9);
            this.lblToolstripBackground.Name = "lblToolstripBackground";
            this.lblToolstripBackground.Size = new System.Drawing.Size(195, 16);
            this.lblToolstripBackground.TabIndex = 3;
            this.lblToolstripBackground.Text = "Focused Toolstrip Background:";
            this.lblToolstripBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlString
            // 
            this.pnlString.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlString.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlString.Location = new System.Drawing.Point(222, 277);
            this.pnlString.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlString.Name = "pnlString";
            this.pnlString.Size = new System.Drawing.Size(74, 21);
            this.pnlString.TabIndex = 11;
            this.pnlString.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblSelectedTextBackground
            // 
            this.lblSelectedTextBackground.Location = new System.Drawing.Point(46, 99);
            this.lblSelectedTextBackground.Name = "lblSelectedTextBackground";
            this.lblSelectedTextBackground.Size = new System.Drawing.Size(175, 16);
            this.lblSelectedTextBackground.TabIndex = 38;
            this.lblSelectedTextBackground.Text = "Selected Text Background:";
            this.lblSelectedTextBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblString
            // 
            this.lblString.Location = new System.Drawing.Point(46, 279);
            this.lblString.Name = "lblString";
            this.lblString.Size = new System.Drawing.Size(175, 16);
            this.lblString.TabIndex = 10;
            this.lblString.Text = "String (Double Quoted):";
            this.lblString.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlToolstripBackground
            // 
            this.pnlToolstripBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlToolstripBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlToolstripBackground.Location = new System.Drawing.Point(222, 7);
            this.pnlToolstripBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlToolstripBackground.Name = "pnlToolstripBackground";
            this.pnlToolstripBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlToolstripBackground.TabIndex = 4;
            this.pnlToolstripBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblCharacter
            // 
            this.lblCharacter.Location = new System.Drawing.Point(24, 309);
            this.lblCharacter.Name = "lblCharacter";
            this.lblCharacter.Size = new System.Drawing.Size(197, 16);
            this.lblCharacter.TabIndex = 12;
            this.lblCharacter.Text = "Character (Single Quoted):";
            this.lblCharacter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlSelectedTextBackground
            // 
            this.pnlSelectedTextBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlSelectedTextBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlSelectedTextBackground.Location = new System.Drawing.Point(222, 97);
            this.pnlSelectedTextBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlSelectedTextBackground.Name = "pnlSelectedTextBackground";
            this.pnlSelectedTextBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlSelectedTextBackground.TabIndex = 37;
            this.pnlSelectedTextBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlBuiltInKeywords
            // 
            this.pnlBuiltInKeywords.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlBuiltInKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBuiltInKeywords.Location = new System.Drawing.Point(222, 367);
            this.pnlBuiltInKeywords.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlBuiltInKeywords.Name = "pnlBuiltInKeywords";
            this.pnlBuiltInKeywords.Size = new System.Drawing.Size(74, 21);
            this.pnlBuiltInKeywords.TabIndex = 9;
            this.pnlBuiltInKeywords.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblUserDefinedKeywords
            // 
            this.lblUserDefinedKeywords.Location = new System.Drawing.Point(46, 399);
            this.lblUserDefinedKeywords.Name = "lblUserDefinedKeywords";
            this.lblUserDefinedKeywords.Size = new System.Drawing.Size(175, 16);
            this.lblUserDefinedKeywords.TabIndex = 35;
            this.lblUserDefinedKeywords.Text = "User-defined Keywords:";
            this.lblUserDefinedKeywords.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlCharacter
            // 
            this.pnlCharacter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlCharacter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCharacter.Location = new System.Drawing.Point(222, 307);
            this.pnlCharacter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlCharacter.Name = "pnlCharacter";
            this.pnlCharacter.Size = new System.Drawing.Size(74, 21);
            this.pnlCharacter.TabIndex = 13;
            this.pnlCharacter.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlUserDefinedKeywords
            // 
            this.pnlUserDefinedKeywords.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlUserDefinedKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlUserDefinedKeywords.Location = new System.Drawing.Point(222, 397);
            this.pnlUserDefinedKeywords.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlUserDefinedKeywords.Name = "pnlUserDefinedKeywords";
            this.pnlUserDefinedKeywords.Size = new System.Drawing.Size(74, 21);
            this.pnlUserDefinedKeywords.TabIndex = 36;
            this.pnlUserDefinedKeywords.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblBuiltInKeywords
            // 
            this.lblBuiltInKeywords.Location = new System.Drawing.Point(46, 369);
            this.lblBuiltInKeywords.Name = "lblBuiltInKeywords";
            this.lblBuiltInKeywords.Size = new System.Drawing.Size(175, 16);
            this.lblBuiltInKeywords.TabIndex = 8;
            this.lblBuiltInKeywords.Text = "Built-in Keywords:";
            this.lblBuiltInKeywords.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlOperatorKeywords
            // 
            this.pnlOperatorKeywords.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlOperatorKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOperatorKeywords.Location = new System.Drawing.Point(222, 247);
            this.pnlOperatorKeywords.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlOperatorKeywords.Name = "pnlOperatorKeywords";
            this.pnlOperatorKeywords.Size = new System.Drawing.Size(74, 21);
            this.pnlOperatorKeywords.TabIndex = 34;
            this.pnlOperatorKeywords.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblUserTables
            // 
            this.lblUserTables.Location = new System.Drawing.Point(23, 459);
            this.lblUserTables.Name = "lblUserTables";
            this.lblUserTables.Size = new System.Drawing.Size(198, 16);
            this.lblUserTables.TabIndex = 14;
            this.lblUserTables.Text = "User-defined Tables && Views:";
            this.lblUserTables.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOperatorKeywords
            // 
            this.lblOperatorKeywords.Location = new System.Drawing.Point(46, 249);
            this.lblOperatorKeywords.Name = "lblOperatorKeywords";
            this.lblOperatorKeywords.Size = new System.Drawing.Size(175, 16);
            this.lblOperatorKeywords.TabIndex = 33;
            this.lblOperatorKeywords.Text = "Operator Keywords:";
            this.lblOperatorKeywords.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlIdentifier
            // 
            this.pnlIdentifier.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlIdentifier.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlIdentifier.Location = new System.Drawing.Point(222, 157);
            this.pnlIdentifier.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlIdentifier.Name = "pnlIdentifier";
            this.pnlIdentifier.Size = new System.Drawing.Size(74, 21);
            this.pnlIdentifier.TabIndex = 7;
            this.pnlIdentifier.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlUserFunctions
            // 
            this.pnlUserFunctions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlUserFunctions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlUserFunctions.Location = new System.Drawing.Point(222, 487);
            this.pnlUserFunctions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlUserFunctions.Name = "pnlUserFunctions";
            this.pnlUserFunctions.Size = new System.Drawing.Size(74, 21);
            this.pnlUserFunctions.TabIndex = 28;
            this.pnlUserFunctions.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlUserTables
            // 
            this.pnlUserTables.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlUserTables.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlUserTables.Location = new System.Drawing.Point(222, 457);
            this.pnlUserTables.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlUserTables.Name = "pnlUserTables";
            this.pnlUserTables.Size = new System.Drawing.Size(74, 21);
            this.pnlUserTables.TabIndex = 15;
            this.pnlUserTables.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblUserFunctions
            // 
            this.lblUserFunctions.Location = new System.Drawing.Point(16, 489);
            this.lblUserFunctions.Name = "lblUserFunctions";
            this.lblUserFunctions.Size = new System.Drawing.Size(205, 16);
            this.lblUserFunctions.TabIndex = 27;
            this.lblUserFunctions.Text = "User-defined Functions && Triggers:";
            this.lblUserFunctions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblIdentifier
            // 
            this.lblIdentifier.Location = new System.Drawing.Point(46, 159);
            this.lblIdentifier.Name = "lblIdentifier";
            this.lblIdentifier.Size = new System.Drawing.Size(175, 16);
            this.lblIdentifier.TabIndex = 6;
            this.lblIdentifier.Text = "Text (Identifier):";
            this.lblIdentifier.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblWhiteSpace
            // 
            this.lblWhiteSpace.Location = new System.Drawing.Point(46, 429);
            this.lblWhiteSpace.Name = "lblWhiteSpace";
            this.lblWhiteSpace.Size = new System.Drawing.Size(175, 16);
            this.lblWhiteSpace.TabIndex = 25;
            this.lblWhiteSpace.Text = "WhiteSpace:";
            this.lblWhiteSpace.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblBuiltInFunctions
            // 
            this.lblBuiltInFunctions.Location = new System.Drawing.Point(46, 339);
            this.lblBuiltInFunctions.Name = "lblBuiltInFunctions";
            this.lblBuiltInFunctions.Size = new System.Drawing.Size(175, 16);
            this.lblBuiltInFunctions.TabIndex = 16;
            this.lblBuiltInFunctions.Text = "Built-in Functions:";
            this.lblBuiltInFunctions.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlWhiteSpace
            // 
            this.pnlWhiteSpace.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlWhiteSpace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlWhiteSpace.Location = new System.Drawing.Point(222, 427);
            this.pnlWhiteSpace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlWhiteSpace.Name = "pnlWhiteSpace";
            this.pnlWhiteSpace.Size = new System.Drawing.Size(74, 21);
            this.pnlWhiteSpace.TabIndex = 26;
            this.pnlWhiteSpace.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlComments
            // 
            this.pnlComments.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlComments.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlComments.Location = new System.Drawing.Point(222, 127);
            this.pnlComments.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlComments.Name = "pnlComments";
            this.pnlComments.Size = new System.Drawing.Size(74, 21);
            this.pnlComments.TabIndex = 5;
            this.pnlComments.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlOperatorSymbol
            // 
            this.pnlOperatorSymbol.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlOperatorSymbol.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOperatorSymbol.Location = new System.Drawing.Point(222, 217);
            this.pnlOperatorSymbol.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlOperatorSymbol.Name = "pnlOperatorSymbol";
            this.pnlOperatorSymbol.Size = new System.Drawing.Size(74, 21);
            this.pnlOperatorSymbol.TabIndex = 21;
            this.pnlOperatorSymbol.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlBuiltInFunctions
            // 
            this.pnlBuiltInFunctions.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlBuiltInFunctions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBuiltInFunctions.Location = new System.Drawing.Point(222, 337);
            this.pnlBuiltInFunctions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlBuiltInFunctions.Name = "pnlBuiltInFunctions";
            this.pnlBuiltInFunctions.Size = new System.Drawing.Size(74, 21);
            this.pnlBuiltInFunctions.TabIndex = 17;
            this.pnlBuiltInFunctions.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblOperatorSymbol
            // 
            this.lblOperatorSymbol.Location = new System.Drawing.Point(46, 219);
            this.lblOperatorSymbol.Name = "lblOperatorSymbol";
            this.lblOperatorSymbol.Size = new System.Drawing.Size(175, 16);
            this.lblOperatorSymbol.TabIndex = 20;
            this.lblOperatorSymbol.Text = "Operator Symbol:";
            this.lblOperatorSymbol.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblComments
            // 
            this.lblComments.Location = new System.Drawing.Point(46, 129);
            this.lblComments.Name = "lblComments";
            this.lblComments.Size = new System.Drawing.Size(175, 15);
            this.lblComments.TabIndex = 4;
            this.lblComments.Text = "Comments:";
            this.lblComments.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEditorBackground
            // 
            this.lblEditorBackground.Location = new System.Drawing.Point(46, 39);
            this.lblEditorBackground.Name = "lblEditorBackground";
            this.lblEditorBackground.Size = new System.Drawing.Size(175, 16);
            this.lblEditorBackground.TabIndex = 0;
            this.lblEditorBackground.Text = "Editor Background:";
            this.lblEditorBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNumber
            // 
            this.lblNumber.Location = new System.Drawing.Point(46, 189);
            this.lblNumber.Name = "lblNumber";
            this.lblNumber.Size = new System.Drawing.Size(175, 16);
            this.lblNumber.TabIndex = 18;
            this.lblNumber.Text = "Number:";
            this.lblNumber.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblCurrentLineBackground
            // 
            this.lblCurrentLineBackground.Location = new System.Drawing.Point(46, 69);
            this.lblCurrentLineBackground.Name = "lblCurrentLineBackground";
            this.lblCurrentLineBackground.Size = new System.Drawing.Size(175, 16);
            this.lblCurrentLineBackground.TabIndex = 1;
            this.lblCurrentLineBackground.Text = "Current Line Background:";
            this.lblCurrentLineBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlCurrentLineBackground
            // 
            this.pnlCurrentLineBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlCurrentLineBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlCurrentLineBackground.Location = new System.Drawing.Point(222, 67);
            this.pnlCurrentLineBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlCurrentLineBackground.Name = "pnlCurrentLineBackground";
            this.pnlCurrentLineBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlCurrentLineBackground.TabIndex = 3;
            this.pnlCurrentLineBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlEditorBackground
            // 
            this.pnlEditorBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlEditorBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlEditorBackground.Location = new System.Drawing.Point(222, 37);
            this.pnlEditorBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlEditorBackground.Name = "pnlEditorBackground";
            this.pnlEditorBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlEditorBackground.TabIndex = 2;
            this.pnlEditorBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // pnlNumber
            // 
            this.pnlNumber.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNumber.Location = new System.Drawing.Point(222, 187);
            this.pnlNumber.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlNumber.Name = "pnlNumber";
            this.pnlNumber.Size = new System.Drawing.Size(74, 21);
            this.pnlNumber.TabIndex = 19;
            this.pnlNumber.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // grpPreferences
            // 
            this.grpPreferences.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreferences.BackColor = System.Drawing.Color.Transparent;
            this.grpPreferences.Controls.Add(this.btnHelp_SelectCurrentSqlBlock);
            this.grpPreferences.Controls.Add(this.grpHighlightStyle);
            this.grpPreferences.Controls.Add(this.grpIndent);
            this.grpPreferences.Controls.Add(this.grpIndicate);
            this.grpPreferences.Controls.Add(this.grpWordWrap);
            this.grpPreferences.Controls.Add(this.chkOpenFileOnCurrentTab);
            this.grpPreferences.Controls.Add(this.chkShowSaveAsButton);
            this.grpPreferences.Controls.Add(this.cboEditorZoom);
            this.grpPreferences.Controls.Add(this.cboEditorFontSize);
            this.grpPreferences.Controls.Add(this.cboSaveAsEncoding);
            this.grpPreferences.Controls.Add(this.chkEntireBlankRowAsEmptyRow4SelectBlock);
            this.grpPreferences.Controls.Add(this.chkHighlightSelectedText);
            this.grpPreferences.Controls.Add(this.chkSaveAsEncoding);
            this.grpPreferences.Controls.Add(this.chkCopyAsHTML);
            this.grpPreferences.Controls.Add(this.chkBold);
            this.grpPreferences.Controls.Add(this.cboEditorFontPicker);
            this.grpPreferences.Controls.Add(this.lblStarHighlightSelection);
            this.grpPreferences.Controls.Add(this.lblEditorFontName);
            this.grpPreferences.Controls.Add(this.lblEditorFontSize);
            this.grpPreferences.Controls.Add(this.lblEditorZoom);
            this.grpPreferences.Controls.Add(this.chkShowAllCharacters);
            this.grpPreferences.Controls.Add(this.chkHighlightSelection);
            this.grpPreferences.Location = new System.Drawing.Point(340, 9);
            this.grpPreferences.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreferences.Name = "grpPreferences";
            this.grpPreferences.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreferences.Size = new System.Drawing.Size(861, 295);
            this.grpPreferences.TabIndex = 23;
            this.grpPreferences.TabStop = false;
            this.grpPreferences.Text = "Editor Appearance / Preferences";
            // 
            // btnHelp_SelectCurrentSqlBlock
            // 
            this.btnHelp_SelectCurrentSqlBlock.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_SelectCurrentSqlBlock.Image")));
            this.btnHelp_SelectCurrentSqlBlock.Location = new System.Drawing.Point(755, 248);
            this.btnHelp_SelectCurrentSqlBlock.Name = "btnHelp_SelectCurrentSqlBlock";
            this.btnHelp_SelectCurrentSqlBlock.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_SelectCurrentSqlBlock.TabIndex = 88;
            this.c1ThemeController1.SetTheme(this.btnHelp_SelectCurrentSqlBlock, "(default)");
            this.btnHelp_SelectCurrentSqlBlock.UseVisualStyleBackColor = true;
            this.btnHelp_SelectCurrentSqlBlock.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_SelectCurrentSqlBlock.Click += new System.EventHandler(this.btnHelp_SelectCurrentSqlBlock_Click);
            // 
            // grpHighlightStyle
            // 
            this.grpHighlightStyle.Controls.Add(this.cboHighlightStyle);
            this.grpHighlightStyle.Controls.Add(this.cboHighlightAlpha);
            this.grpHighlightStyle.Controls.Add(this.cboHighlightOutlineAlpha);
            this.grpHighlightStyle.Controls.Add(this.lblStarHighlight);
            this.grpHighlightStyle.Controls.Add(this.lblHighlightColorAlpha);
            this.grpHighlightStyle.Controls.Add(this.lblHighlightColorOutlineAlpha);
            this.grpHighlightStyle.Controls.Add(this.lblHighlightColorStyle);
            this.grpHighlightStyle.Controls.Add(this.pnlHighlightForeColor);
            this.grpHighlightStyle.Controls.Add(this.lblHighlightColorForeColor);
            this.grpHighlightStyle.Location = new System.Drawing.Point(12, 85);
            this.grpHighlightStyle.Name = "grpHighlightStyle";
            this.grpHighlightStyle.Size = new System.Drawing.Size(250, 138);
            this.grpHighlightStyle.TabIndex = 87;
            this.grpHighlightStyle.TabStop = false;
            this.grpHighlightStyle.Text = "HighlightStyle";
            this.c1ThemeController1.SetTheme(this.grpHighlightStyle, "(default)");
            // 
            // cboHighlightStyle
            // 
            this.cboHighlightStyle.AllowSpinLoop = false;
            this.cboHighlightStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboHighlightStyle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboHighlightStyle.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboHighlightStyle.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboHighlightStyle.GapHeight = 0;
            this.cboHighlightStyle.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboHighlightStyle.Items.Add("Box");
            this.cboHighlightStyle.Items.Add("CompositionThick");
            this.cboHighlightStyle.Items.Add("Dash");
            this.cboHighlightStyle.Items.Add("Diagonal");
            this.cboHighlightStyle.Items.Add("StraightBox");
            this.cboHighlightStyle.ItemsDisplayMember = "";
            this.cboHighlightStyle.ItemsValueMember = "";
            this.cboHighlightStyle.Location = new System.Drawing.Point(101, 50);
            this.cboHighlightStyle.Name = "cboHighlightStyle";
            this.cboHighlightStyle.Size = new System.Drawing.Size(128, 21);
            this.cboHighlightStyle.TabIndex = 78;
            this.cboHighlightStyle.Tag = null;
            this.cboHighlightStyle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboHighlightStyle, "(default)");
            this.cboHighlightStyle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboHighlightAlpha
            // 
            this.cboHighlightAlpha.AllowSpinLoop = false;
            this.cboHighlightAlpha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboHighlightAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboHighlightAlpha.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboHighlightAlpha.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboHighlightAlpha.GapHeight = 0;
            this.cboHighlightAlpha.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboHighlightAlpha.Items.Add("50");
            this.cboHighlightAlpha.Items.Add("60");
            this.cboHighlightAlpha.Items.Add("70");
            this.cboHighlightAlpha.Items.Add("80");
            this.cboHighlightAlpha.Items.Add("90");
            this.cboHighlightAlpha.Items.Add("100");
            this.cboHighlightAlpha.Items.Add("110");
            this.cboHighlightAlpha.Items.Add("120");
            this.cboHighlightAlpha.Items.Add("130");
            this.cboHighlightAlpha.Items.Add("140");
            this.cboHighlightAlpha.Items.Add("150");
            this.cboHighlightAlpha.Items.Add("160");
            this.cboHighlightAlpha.Items.Add("170");
            this.cboHighlightAlpha.Items.Add("180");
            this.cboHighlightAlpha.Items.Add("190");
            this.cboHighlightAlpha.Items.Add("200");
            this.cboHighlightAlpha.ItemsDisplayMember = "";
            this.cboHighlightAlpha.ItemsValueMember = "";
            this.cboHighlightAlpha.Location = new System.Drawing.Point(101, 106);
            this.cboHighlightAlpha.Name = "cboHighlightAlpha";
            this.cboHighlightAlpha.Size = new System.Drawing.Size(54, 21);
            this.cboHighlightAlpha.TabIndex = 80;
            this.cboHighlightAlpha.Tag = null;
            this.cboHighlightAlpha.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboHighlightAlpha, "(default)");
            this.cboHighlightAlpha.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboHighlightOutlineAlpha
            // 
            this.cboHighlightOutlineAlpha.AllowSpinLoop = false;
            this.cboHighlightOutlineAlpha.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboHighlightOutlineAlpha.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboHighlightOutlineAlpha.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboHighlightOutlineAlpha.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboHighlightOutlineAlpha.GapHeight = 0;
            this.cboHighlightOutlineAlpha.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboHighlightOutlineAlpha.Items.Add("50");
            this.cboHighlightOutlineAlpha.Items.Add("60");
            this.cboHighlightOutlineAlpha.Items.Add("70");
            this.cboHighlightOutlineAlpha.Items.Add("80");
            this.cboHighlightOutlineAlpha.Items.Add("90");
            this.cboHighlightOutlineAlpha.Items.Add("100");
            this.cboHighlightOutlineAlpha.Items.Add("110");
            this.cboHighlightOutlineAlpha.Items.Add("120");
            this.cboHighlightOutlineAlpha.Items.Add("130");
            this.cboHighlightOutlineAlpha.Items.Add("140");
            this.cboHighlightOutlineAlpha.Items.Add("150");
            this.cboHighlightOutlineAlpha.Items.Add("160");
            this.cboHighlightOutlineAlpha.Items.Add("170");
            this.cboHighlightOutlineAlpha.Items.Add("180");
            this.cboHighlightOutlineAlpha.Items.Add("190");
            this.cboHighlightOutlineAlpha.Items.Add("200");
            this.cboHighlightOutlineAlpha.ItemsDisplayMember = "";
            this.cboHighlightOutlineAlpha.ItemsValueMember = "";
            this.cboHighlightOutlineAlpha.Location = new System.Drawing.Point(101, 78);
            this.cboHighlightOutlineAlpha.Name = "cboHighlightOutlineAlpha";
            this.cboHighlightOutlineAlpha.Size = new System.Drawing.Size(54, 21);
            this.cboHighlightOutlineAlpha.TabIndex = 79;
            this.cboHighlightOutlineAlpha.Tag = null;
            this.cboHighlightOutlineAlpha.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboHighlightOutlineAlpha, "(default)");
            this.cboHighlightOutlineAlpha.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarHighlight
            // 
            this.lblStarHighlight.AutoSize = true;
            this.lblStarHighlight.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarHighlight.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarHighlight.Location = new System.Drawing.Point(101, 3);
            this.lblStarHighlight.Name = "lblStarHighlight";
            this.lblStarHighlight.Size = new System.Drawing.Size(14, 15);
            this.lblStarHighlight.TabIndex = 48;
            this.lblStarHighlight.Text = "*";
            // 
            // lblHighlightColorAlpha
            // 
            this.lblHighlightColorAlpha.Location = new System.Drawing.Point(16, 107);
            this.lblHighlightColorAlpha.Name = "lblHighlightColorAlpha";
            this.lblHighlightColorAlpha.Size = new System.Drawing.Size(82, 16);
            this.lblHighlightColorAlpha.TabIndex = 12;
            this.lblHighlightColorAlpha.Text = "Alpha:";
            this.lblHighlightColorAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblHighlightColorOutlineAlpha
            // 
            this.lblHighlightColorOutlineAlpha.Location = new System.Drawing.Point(-6, 79);
            this.lblHighlightColorOutlineAlpha.Name = "lblHighlightColorOutlineAlpha";
            this.lblHighlightColorOutlineAlpha.Size = new System.Drawing.Size(104, 16);
            this.lblHighlightColorOutlineAlpha.TabIndex = 9;
            this.lblHighlightColorOutlineAlpha.Text = "Outline Alpha:";
            this.lblHighlightColorOutlineAlpha.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblHighlightColorStyle
            // 
            this.lblHighlightColorStyle.Location = new System.Drawing.Point(23, 51);
            this.lblHighlightColorStyle.Name = "lblHighlightColorStyle";
            this.lblHighlightColorStyle.Size = new System.Drawing.Size(75, 16);
            this.lblHighlightColorStyle.TabIndex = 4;
            this.lblHighlightColorStyle.Text = "Style:";
            this.lblHighlightColorStyle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlHighlightForeColor
            // 
            this.pnlHighlightForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlHighlightForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlHighlightForeColor.Location = new System.Drawing.Point(101, 22);
            this.pnlHighlightForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlHighlightForeColor.Name = "pnlHighlightForeColor";
            this.pnlHighlightForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlHighlightForeColor.TabIndex = 3;
            // 
            // lblHighlightColorForeColor
            // 
            this.lblHighlightColorForeColor.Location = new System.Drawing.Point(-2, 23);
            this.lblHighlightColorForeColor.Name = "lblHighlightColorForeColor";
            this.lblHighlightColorForeColor.Size = new System.Drawing.Size(100, 17);
            this.lblHighlightColorForeColor.TabIndex = 2;
            this.lblHighlightColorForeColor.Text = "Fore Color:";
            this.lblHighlightColorForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpIndent
            // 
            this.grpIndent.Controls.Add(this.chkReplaceTabWithSpace);
            this.grpIndent.Controls.Add(this.lblStarTabWidth);
            this.grpIndent.Controls.Add(this.chkShowIndentGuide);
            this.grpIndent.Controls.Add(this.cboTabWidth);
            this.grpIndent.Controls.Add(this.lblLength);
            this.grpIndent.Controls.Add(this.lblTabWidth);
            this.grpIndent.Controls.Add(this.lblIndentMode);
            this.grpIndent.Controls.Add(this.cboIndentMode);
            this.grpIndent.Controls.Add(this.lblStarShowIndentGuide);
            this.grpIndent.Location = new System.Drawing.Point(636, 105);
            this.grpIndent.Name = "grpIndent";
            this.grpIndent.Size = new System.Drawing.Size(245, 130);
            this.grpIndent.TabIndex = 85;
            this.grpIndent.TabStop = false;
            this.grpIndent.Text = "Indent";
            this.c1ThemeController1.SetTheme(this.grpIndent, "(default)");
            // 
            // chkReplaceTabWithSpace
            // 
            this.chkReplaceTabWithSpace.AutoSize = true;
            this.chkReplaceTabWithSpace.BackColor = System.Drawing.Color.Transparent;
            this.chkReplaceTabWithSpace.BorderColor = System.Drawing.Color.Transparent;
            this.chkReplaceTabWithSpace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkReplaceTabWithSpace.Checked = true;
            this.chkReplaceTabWithSpace.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkReplaceTabWithSpace.Enabled = false;
            this.chkReplaceTabWithSpace.ForeColor = System.Drawing.Color.Black;
            this.chkReplaceTabWithSpace.Location = new System.Drawing.Point(25, 103);
            this.chkReplaceTabWithSpace.Name = "chkReplaceTabWithSpace";
            this.chkReplaceTabWithSpace.Padding = new System.Windows.Forms.Padding(1);
            this.chkReplaceTabWithSpace.Size = new System.Drawing.Size(163, 22);
            this.chkReplaceTabWithSpace.TabIndex = 86;
            this.chkReplaceTabWithSpace.Text = "Replace Tab with Space";
            this.c1ThemeController1.SetTheme(this.chkReplaceTabWithSpace, "(default)");
            this.chkReplaceTabWithSpace.UseVisualStyleBackColor = true;
            this.chkReplaceTabWithSpace.Value = true;
            this.chkReplaceTabWithSpace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarTabWidth
            // 
            this.lblStarTabWidth.AutoSize = true;
            this.lblStarTabWidth.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarTabWidth.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarTabWidth.Location = new System.Drawing.Point(169, 80);
            this.lblStarTabWidth.Name = "lblStarTabWidth";
            this.lblStarTabWidth.Size = new System.Drawing.Size(14, 15);
            this.lblStarTabWidth.TabIndex = 85;
            this.lblStarTabWidth.Text = "*";
            this.lblStarTabWidth.Visible = false;
            // 
            // chkShowIndentGuide
            // 
            this.chkShowIndentGuide.AutoSize = true;
            this.chkShowIndentGuide.BackColor = System.Drawing.Color.Transparent;
            this.chkShowIndentGuide.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowIndentGuide.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowIndentGuide.Checked = true;
            this.chkShowIndentGuide.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowIndentGuide.ForeColor = System.Drawing.Color.Black;
            this.chkShowIndentGuide.Location = new System.Drawing.Point(25, 22);
            this.chkShowIndentGuide.Name = "chkShowIndentGuide";
            this.chkShowIndentGuide.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowIndentGuide.Size = new System.Drawing.Size(135, 22);
            this.chkShowIndentGuide.TabIndex = 81;
            this.chkShowIndentGuide.Text = "Show Indent Guide";
            this.c1ThemeController1.SetTheme(this.chkShowIndentGuide, "(default)");
            this.chkShowIndentGuide.UseVisualStyleBackColor = true;
            this.chkShowIndentGuide.Value = true;
            this.chkShowIndentGuide.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowIndentGuide.CheckedChanged += new System.EventHandler(this.chkShowIndentGuide_CheckedChanged);
            // 
            // cboTabWidth
            // 
            this.cboTabWidth.AllowSpinLoop = false;
            this.cboTabWidth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboTabWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboTabWidth.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboTabWidth.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboTabWidth.GapHeight = 0;
            this.cboTabWidth.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboTabWidth.Items.Add("1");
            this.cboTabWidth.Items.Add("2");
            this.cboTabWidth.Items.Add("3");
            this.cboTabWidth.Items.Add("4");
            this.cboTabWidth.Items.Add("5");
            this.cboTabWidth.Items.Add("6");
            this.cboTabWidth.Items.Add("7");
            this.cboTabWidth.Items.Add("8");
            this.cboTabWidth.Items.Add("9");
            this.cboTabWidth.ItemsDisplayMember = "";
            this.cboTabWidth.ItemsValueMember = "";
            this.cboTabWidth.Location = new System.Drawing.Point(117, 76);
            this.cboTabWidth.Name = "cboTabWidth";
            this.cboTabWidth.Size = new System.Drawing.Size(39, 21);
            this.cboTabWidth.TabIndex = 84;
            this.cboTabWidth.Tag = null;
            this.cboTabWidth.Text = "4";
            this.cboTabWidth.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboTabWidth, "(default)");
            this.cboTabWidth.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblLength
            // 
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(111, 16);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(0, 16);
            this.lblLength.TabIndex = 55;
            this.lblLength.Visible = false;
            // 
            // lblTabWidth
            // 
            this.lblTabWidth.AutoSize = true;
            this.lblTabWidth.Location = new System.Drawing.Point(23, 79);
            this.lblTabWidth.Name = "lblTabWidth";
            this.lblTabWidth.Size = new System.Drawing.Size(69, 16);
            this.lblTabWidth.TabIndex = 83;
            this.lblTabWidth.Text = "Tab Width:";
            this.lblTabWidth.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblIndentMode
            // 
            this.lblIndentMode.AutoSize = true;
            this.lblIndentMode.Location = new System.Drawing.Point(23, 50);
            this.lblIndentMode.Name = "lblIndentMode";
            this.lblIndentMode.Size = new System.Drawing.Size(84, 16);
            this.lblIndentMode.TabIndex = 41;
            this.lblIndentMode.Text = "Indent Mode:";
            this.lblIndentMode.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboIndentMode
            // 
            this.cboIndentMode.AllowSpinLoop = false;
            this.cboIndentMode.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboIndentMode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboIndentMode.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboIndentMode.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboIndentMode.GapHeight = 0;
            this.cboIndentMode.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboIndentMode.Items.Add("Fixed");
            this.cboIndentMode.Items.Add("Same");
            this.cboIndentMode.Items.Add("Indent");
            this.cboIndentMode.ItemsDisplayMember = "";
            this.cboIndentMode.ItemsValueMember = "";
            this.cboIndentMode.Location = new System.Drawing.Point(117, 48);
            this.cboIndentMode.Name = "cboIndentMode";
            this.cboIndentMode.Size = new System.Drawing.Size(105, 21);
            this.cboIndentMode.TabIndex = 78;
            this.cboIndentMode.Tag = null;
            this.cboIndentMode.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboIndentMode, "(default)");
            this.cboIndentMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboIndentMode.SelectedIndexChanged += new System.EventHandler(this.cboWordWrapIndentMode_SelectedIndexChanged);
            // 
            // lblStarShowIndentGuide
            // 
            this.lblStarShowIndentGuide.AutoSize = true;
            this.lblStarShowIndentGuide.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarShowIndentGuide.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarShowIndentGuide.Location = new System.Drawing.Point(163, 26);
            this.lblStarShowIndentGuide.Name = "lblStarShowIndentGuide";
            this.lblStarShowIndentGuide.Size = new System.Drawing.Size(14, 15);
            this.lblStarShowIndentGuide.TabIndex = 82;
            this.lblStarShowIndentGuide.Text = "*";
            this.lblStarShowIndentGuide.Visible = false;
            // 
            // grpIndicate
            // 
            this.grpIndicate.Controls.Add(this.cboBookmarkStyle);
            this.grpIndicate.Controls.Add(this.editorIndicator);
            this.grpIndicate.Controls.Add(this.lblStarIndicate);
            this.grpIndicate.Controls.Add(this.lblErrorLineBackground);
            this.grpIndicate.Controls.Add(this.pnlErrorLineBackground);
            this.grpIndicate.Controls.Add(this.lblBookmarkBackground);
            this.grpIndicate.Controls.Add(this.pnlBookmarkBackground);
            this.grpIndicate.Controls.Add(this.lblBookmarkStyle);
            this.grpIndicate.Location = new System.Drawing.Point(290, 15);
            this.grpIndicate.Name = "grpIndicate";
            this.grpIndicate.Size = new System.Drawing.Size(562, 86);
            this.grpIndicate.TabIndex = 52;
            this.grpIndicate.TabStop = false;
            this.grpIndicate.Text = "Indicate (after executing SQL statement)";
            // 
            // cboBookmarkStyle
            // 
            this.cboBookmarkStyle.AllowSpinLoop = false;
            this.cboBookmarkStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboBookmarkStyle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboBookmarkStyle.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboBookmarkStyle.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboBookmarkStyle.GapHeight = 0;
            this.cboBookmarkStyle.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboBookmarkStyle.Items.Add("Normal");
            this.cboBookmarkStyle.Items.Add("SYSDBA");
            this.cboBookmarkStyle.Items.Add("SYSOPER");
            this.cboBookmarkStyle.ItemsDisplayMember = "";
            this.cboBookmarkStyle.ItemsValueMember = "";
            this.cboBookmarkStyle.Location = new System.Drawing.Point(386, 24);
            this.cboBookmarkStyle.Name = "cboBookmarkStyle";
            this.cboBookmarkStyle.Size = new System.Drawing.Size(105, 21);
            this.cboBookmarkStyle.TabIndex = 79;
            this.cboBookmarkStyle.Tag = null;
            this.cboBookmarkStyle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboBookmarkStyle, "(default)");
            this.cboBookmarkStyle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboBookmarkStyle.SelectedIndexChanged += new System.EventHandler(this.cboBookmarkStyle_SelectedIndexChanged);
            // 
            // editorIndicator
            // 
            this.editorIndicator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorIndicator.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editorIndicator.CaretLineVisible = true;
            this.editorIndicator.Enabled = false;
            this.editorIndicator.EndAtLastLine = false;
            this.editorIndicator.HScrollBar = false;
            this.editorIndicator.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorIndicator.Location = new System.Drawing.Point(299, 54);
            this.editorIndicator.Name = "editorIndicator";
            this.editorIndicator.ScrollWidth = 400;
            this.editorIndicator.SelectionEolFilled = true;
            this.editorIndicator.Size = new System.Drawing.Size(234, 21);
            this.editorIndicator.Styler = null;
            this.editorIndicator.TabIndex = 50;
            this.editorIndicator.VScrollBar = false;
            this.editorIndicator.WhitespaceSize = 3;
            this.editorIndicator.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // lblStarIndicate
            // 
            this.lblStarIndicate.AutoSize = true;
            this.lblStarIndicate.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarIndicate.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarIndicate.Location = new System.Drawing.Point(227, 3);
            this.lblStarIndicate.Name = "lblStarIndicate";
            this.lblStarIndicate.Size = new System.Drawing.Size(14, 15);
            this.lblStarIndicate.TabIndex = 49;
            this.lblStarIndicate.Text = "*";
            // 
            // lblErrorLineBackground
            // 
            this.lblErrorLineBackground.AutoSize = true;
            this.lblErrorLineBackground.Location = new System.Drawing.Point(17, 26);
            this.lblErrorLineBackground.Name = "lblErrorLineBackground";
            this.lblErrorLineBackground.Size = new System.Drawing.Size(160, 16);
            this.lblErrorLineBackground.TabIndex = 44;
            this.lblErrorLineBackground.Text = "Error Keyword Background:";
            this.lblErrorLineBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlErrorLineBackground
            // 
            this.pnlErrorLineBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlErrorLineBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlErrorLineBackground.Location = new System.Drawing.Point(181, 24);
            this.pnlErrorLineBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlErrorLineBackground.Name = "pnlErrorLineBackground";
            this.pnlErrorLineBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlErrorLineBackground.TabIndex = 43;
            this.pnlErrorLineBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblBookmarkBackground
            // 
            this.lblBookmarkBackground.AutoSize = true;
            this.lblBookmarkBackground.Location = new System.Drawing.Point(17, 56);
            this.lblBookmarkBackground.Name = "lblBookmarkBackground";
            this.lblBookmarkBackground.Size = new System.Drawing.Size(138, 16);
            this.lblBookmarkBackground.TabIndex = 40;
            this.lblBookmarkBackground.Text = "Bookmark Background:";
            this.lblBookmarkBackground.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlBookmarkBackground
            // 
            this.pnlBookmarkBackground.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlBookmarkBackground.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlBookmarkBackground.Location = new System.Drawing.Point(181, 54);
            this.pnlBookmarkBackground.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlBookmarkBackground.Name = "pnlBookmarkBackground";
            this.pnlBookmarkBackground.Size = new System.Drawing.Size(74, 21);
            this.pnlBookmarkBackground.TabIndex = 39;
            this.pnlBookmarkBackground.Click += new System.EventHandler(this.pnlSelectedClick);
            // 
            // lblBookmarkStyle
            // 
            this.lblBookmarkStyle.AutoSize = true;
            this.lblBookmarkStyle.Location = new System.Drawing.Point(285, 26);
            this.lblBookmarkStyle.Name = "lblBookmarkStyle";
            this.lblBookmarkStyle.Size = new System.Drawing.Size(97, 16);
            this.lblBookmarkStyle.TabIndex = 41;
            this.lblBookmarkStyle.Text = "Bookmark Style:";
            this.lblBookmarkStyle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpWordWrap
            // 
            this.grpWordWrap.Controls.Add(this.chkMargin);
            this.grpWordWrap.Controls.Add(this.chkEnd);
            this.grpWordWrap.Controls.Add(this.chkStart);
            this.grpWordWrap.Controls.Add(this.chkWordWrap);
            this.grpWordWrap.Controls.Add(this.lblStarWordWrap);
            this.grpWordWrap.Location = new System.Drawing.Point(12, 232);
            this.grpWordWrap.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpWordWrap.Name = "grpWordWrap";
            this.grpWordWrap.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpWordWrap.Size = new System.Drawing.Size(250, 51);
            this.grpWordWrap.TabIndex = 42;
            this.grpWordWrap.TabStop = false;
            // 
            // chkMargin
            // 
            this.chkMargin.AutoSize = true;
            this.chkMargin.BackColor = System.Drawing.Color.Transparent;
            this.chkMargin.BorderColor = System.Drawing.Color.Transparent;
            this.chkMargin.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkMargin.ForeColor = System.Drawing.Color.Black;
            this.chkMargin.Location = new System.Drawing.Point(161, 22);
            this.chkMargin.Name = "chkMargin";
            this.chkMargin.Padding = new System.Windows.Forms.Padding(1);
            this.chkMargin.Size = new System.Drawing.Size(69, 22);
            this.chkMargin.TabIndex = 69;
            this.chkMargin.Text = "Margin";
            this.c1ThemeController1.SetTheme(this.chkMargin, "(default)");
            this.chkMargin.UseVisualStyleBackColor = true;
            this.chkMargin.Value = null;
            this.chkMargin.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkMargin.CheckedChanged += new System.EventHandler(this.WordWrapFlags_CheckedChanged);
            // 
            // chkEnd
            // 
            this.chkEnd.AutoSize = true;
            this.chkEnd.BackColor = System.Drawing.Color.Transparent;
            this.chkEnd.BorderColor = System.Drawing.Color.Transparent;
            this.chkEnd.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEnd.ForeColor = System.Drawing.Color.Black;
            this.chkEnd.Location = new System.Drawing.Point(93, 22);
            this.chkEnd.Name = "chkEnd";
            this.chkEnd.Padding = new System.Windows.Forms.Padding(1);
            this.chkEnd.Size = new System.Drawing.Size(50, 22);
            this.chkEnd.TabIndex = 68;
            this.chkEnd.Text = "End";
            this.c1ThemeController1.SetTheme(this.chkEnd, "(default)");
            this.chkEnd.UseVisualStyleBackColor = true;
            this.chkEnd.Value = null;
            this.chkEnd.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkEnd.CheckedChanged += new System.EventHandler(this.WordWrapFlags_CheckedChanged);
            // 
            // chkStart
            // 
            this.chkStart.AutoSize = true;
            this.chkStart.BackColor = System.Drawing.Color.Transparent;
            this.chkStart.BorderColor = System.Drawing.Color.Transparent;
            this.chkStart.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkStart.ForeColor = System.Drawing.Color.Black;
            this.chkStart.Location = new System.Drawing.Point(20, 22);
            this.chkStart.Name = "chkStart";
            this.chkStart.Padding = new System.Windows.Forms.Padding(1);
            this.chkStart.Size = new System.Drawing.Size(54, 22);
            this.chkStart.TabIndex = 67;
            this.chkStart.Text = "Start";
            this.c1ThemeController1.SetTheme(this.chkStart, "(default)");
            this.chkStart.UseVisualStyleBackColor = true;
            this.chkStart.Value = null;
            this.chkStart.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkStart.CheckedChanged += new System.EventHandler(this.WordWrapFlags_CheckedChanged);
            // 
            // chkWordWrap
            // 
            this.chkWordWrap.AutoSize = true;
            this.chkWordWrap.BackColor = System.Drawing.Color.Transparent;
            this.chkWordWrap.BorderColor = System.Drawing.Color.Transparent;
            this.chkWordWrap.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkWordWrap.ForeColor = System.Drawing.Color.Black;
            this.chkWordWrap.Location = new System.Drawing.Point(12, -1);
            this.chkWordWrap.Name = "chkWordWrap";
            this.chkWordWrap.Padding = new System.Windows.Forms.Padding(1);
            this.chkWordWrap.Size = new System.Drawing.Size(94, 22);
            this.chkWordWrap.TabIndex = 66;
            this.chkWordWrap.Text = "Word Wrap";
            this.c1ThemeController1.SetTheme(this.chkWordWrap, "(default)");
            this.chkWordWrap.UseVisualStyleBackColor = true;
            this.chkWordWrap.Value = null;
            this.chkWordWrap.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkWordWrap.CheckedChanged += new System.EventHandler(this.chkWordWrap_CheckedChanged);
            // 
            // lblStarWordWrap
            // 
            this.lblStarWordWrap.AutoSize = true;
            this.lblStarWordWrap.BackColor = System.Drawing.Color.Transparent;
            this.lblStarWordWrap.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarWordWrap.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarWordWrap.Location = new System.Drawing.Point(137, 3);
            this.lblStarWordWrap.Name = "lblStarWordWrap";
            this.lblStarWordWrap.Size = new System.Drawing.Size(14, 15);
            this.lblStarWordWrap.TabIndex = 47;
            this.lblStarWordWrap.Text = "*";
            // 
            // chkOpenFileOnCurrentTab
            // 
            this.chkOpenFileOnCurrentTab.AutoSize = true;
            this.chkOpenFileOnCurrentTab.BackColor = System.Drawing.Color.Transparent;
            this.chkOpenFileOnCurrentTab.BorderColor = System.Drawing.Color.Transparent;
            this.chkOpenFileOnCurrentTab.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkOpenFileOnCurrentTab.Checked = true;
            this.chkOpenFileOnCurrentTab.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkOpenFileOnCurrentTab.ForeColor = System.Drawing.Color.Black;
            this.chkOpenFileOnCurrentTab.Location = new System.Drawing.Point(531, 64);
            this.chkOpenFileOnCurrentTab.Name = "chkOpenFileOnCurrentTab";
            this.chkOpenFileOnCurrentTab.Padding = new System.Windows.Forms.Padding(1);
            this.chkOpenFileOnCurrentTab.Size = new System.Drawing.Size(359, 22);
            this.chkOpenFileOnCurrentTab.TabIndex = 79;
            this.chkOpenFileOnCurrentTab.Text = "Open File button: Open the specified file on the current tab";
            this.c1ThemeController1.SetTheme(this.chkOpenFileOnCurrentTab, "(default)");
            this.chkOpenFileOnCurrentTab.UseVisualStyleBackColor = true;
            this.chkOpenFileOnCurrentTab.Value = true;
            this.chkOpenFileOnCurrentTab.Visible = false;
            this.chkOpenFileOnCurrentTab.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkShowSaveAsButton
            // 
            this.chkShowSaveAsButton.AutoSize = true;
            this.chkShowSaveAsButton.BackColor = System.Drawing.Color.Transparent;
            this.chkShowSaveAsButton.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowSaveAsButton.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowSaveAsButton.Checked = true;
            this.chkShowSaveAsButton.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkShowSaveAsButton.Enabled = false;
            this.chkShowSaveAsButton.ForeColor = System.Drawing.Color.Black;
            this.chkShowSaveAsButton.Location = new System.Drawing.Point(292, 214);
            this.chkShowSaveAsButton.Name = "chkShowSaveAsButton";
            this.chkShowSaveAsButton.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowSaveAsButton.Size = new System.Drawing.Size(151, 22);
            this.chkShowSaveAsButton.TabIndex = 78;
            this.chkShowSaveAsButton.Text = "Show \'Save As\' Button";
            this.c1ThemeController1.SetTheme(this.chkShowSaveAsButton, "(default)");
            this.chkShowSaveAsButton.UseVisualStyleBackColor = true;
            this.chkShowSaveAsButton.Value = true;
            this.chkShowSaveAsButton.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowSaveAsButton.CheckedChanged += new System.EventHandler(this.chkShowSaveAsButton_CheckedChanged);
            // 
            // cboEditorZoom
            // 
            this.cboEditorZoom.AllowSpinLoop = false;
            this.cboEditorZoom.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboEditorZoom.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboEditorZoom.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboEditorZoom.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboEditorZoom.GapHeight = 0;
            this.cboEditorZoom.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboEditorZoom.Items.Add("-2");
            this.cboEditorZoom.Items.Add("-1");
            this.cboEditorZoom.Items.Add("0");
            this.cboEditorZoom.Items.Add("1");
            this.cboEditorZoom.Items.Add("2");
            this.cboEditorZoom.ItemsDisplayMember = "";
            this.cboEditorZoom.ItemsValueMember = "";
            this.cboEditorZoom.Location = new System.Drawing.Point(797, 298);
            this.cboEditorZoom.Name = "cboEditorZoom";
            this.cboEditorZoom.Size = new System.Drawing.Size(40, 21);
            this.cboEditorZoom.TabIndex = 77;
            this.cboEditorZoom.Tag = null;
            this.cboEditorZoom.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboEditorZoom, "(default)");
            this.cboEditorZoom.Visible = false;
            this.cboEditorZoom.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboEditorZoom.SelectedIndexChanged += new System.EventHandler(this.cboEditorZoom_SelectedIndexChanged);
            // 
            // cboEditorFontSize
            // 
            this.cboEditorFontSize.AllowSpinLoop = false;
            this.cboEditorFontSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboEditorFontSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboEditorFontSize.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboEditorFontSize.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboEditorFontSize.GapHeight = 0;
            this.cboEditorFontSize.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboEditorFontSize.Items.Add("10");
            this.cboEditorFontSize.Items.Add("12");
            this.cboEditorFontSize.Items.Add("14");
            this.cboEditorFontSize.Items.Add("16");
            this.cboEditorFontSize.Items.Add("18");
            this.cboEditorFontSize.ItemsDisplayMember = "";
            this.cboEditorFontSize.ItemsValueMember = "";
            this.cboEditorFontSize.Location = new System.Drawing.Point(113, 56);
            this.cboEditorFontSize.Name = "cboEditorFontSize";
            this.cboEditorFontSize.Size = new System.Drawing.Size(40, 21);
            this.cboEditorFontSize.TabIndex = 76;
            this.cboEditorFontSize.Tag = null;
            this.cboEditorFontSize.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboEditorFontSize, "(default)");
            this.cboEditorFontSize.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboEditorFontSize.SelectedIndexChanged += new System.EventHandler(this.cboEditorFontSize_SelectedIndexChanged);
            // 
            // cboSaveAsEncoding
            // 
            this.cboSaveAsEncoding.AllowSpinLoop = false;
            this.cboSaveAsEncoding.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboSaveAsEncoding.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboSaveAsEncoding.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboSaveAsEncoding.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboSaveAsEncoding.Enabled = false;
            this.cboSaveAsEncoding.GapHeight = 0;
            this.cboSaveAsEncoding.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboSaveAsEncoding.InitialSelectedIndex = 0;
            this.cboSaveAsEncoding.Items.Add("UTF-8");
            this.cboSaveAsEncoding.ItemsDisplayMember = "UTF-8";
            this.cboSaveAsEncoding.ItemsValueMember = "";
            this.cboSaveAsEncoding.Location = new System.Drawing.Point(447, 165);
            this.cboSaveAsEncoding.Name = "cboSaveAsEncoding";
            this.cboSaveAsEncoding.Size = new System.Drawing.Size(63, 21);
            this.cboSaveAsEncoding.TabIndex = 74;
            this.cboSaveAsEncoding.Tag = null;
            this.cboSaveAsEncoding.Text = "UTF-8";
            this.cboSaveAsEncoding.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboSaveAsEncoding, "(default)");
            this.cboSaveAsEncoding.Value = "UTF-8";
            this.cboSaveAsEncoding.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkEntireBlankRowAsEmptyRow4SelectBlock
            // 
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.BackColor = System.Drawing.Color.Transparent;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.BorderColor = System.Drawing.Color.Transparent;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Checked = true;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.ForeColor = System.Drawing.Color.Black;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Location = new System.Drawing.Point(292, 238);
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Name = "chkEntireBlankRowAsEmptyRow4SelectBlock";
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Padding = new System.Windows.Forms.Padding(1);
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Size = new System.Drawing.Size(464, 45);
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.TabIndex = 72;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Text = "Select Current SQL Block: Treat lines containing only whitespace as part of the c" +
    "urrent SQL block when executing with Ctrl+Enter";
            this.c1ThemeController1.SetTheme(this.chkEntireBlankRowAsEmptyRow4SelectBlock, "(default)");
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.UseVisualStyleBackColor = true;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.Value = true;
            this.chkEntireBlankRowAsEmptyRow4SelectBlock.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkHighlightSelectedText
            // 
            this.chkHighlightSelectedText.AutoSize = true;
            this.chkHighlightSelectedText.BackColor = System.Drawing.Color.Transparent;
            this.chkHighlightSelectedText.BorderColor = System.Drawing.Color.Transparent;
            this.chkHighlightSelectedText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkHighlightSelectedText.Checked = true;
            this.chkHighlightSelectedText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighlightSelectedText.Enabled = false;
            this.chkHighlightSelectedText.ForeColor = System.Drawing.Color.Black;
            this.chkHighlightSelectedText.Location = new System.Drawing.Point(636, 295);
            this.chkHighlightSelectedText.Name = "chkHighlightSelectedText";
            this.chkHighlightSelectedText.Padding = new System.Windows.Forms.Padding(1);
            this.chkHighlightSelectedText.Size = new System.Drawing.Size(311, 22);
            this.chkHighlightSelectedText.TabIndex = 71;
            this.chkHighlightSelectedText.Text = "Highlight Selected Text When Mouse Double Click";
            this.c1ThemeController1.SetTheme(this.chkHighlightSelectedText, "(default)");
            this.chkHighlightSelectedText.UseVisualStyleBackColor = true;
            this.chkHighlightSelectedText.Value = true;
            this.chkHighlightSelectedText.Visible = false;
            this.chkHighlightSelectedText.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkSaveAsEncoding
            // 
            this.chkSaveAsEncoding.AutoSize = true;
            this.chkSaveAsEncoding.BackColor = System.Drawing.Color.Transparent;
            this.chkSaveAsEncoding.BorderColor = System.Drawing.Color.Transparent;
            this.chkSaveAsEncoding.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkSaveAsEncoding.Checked = true;
            this.chkSaveAsEncoding.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSaveAsEncoding.Enabled = false;
            this.chkSaveAsEncoding.ForeColor = System.Drawing.Color.Black;
            this.chkSaveAsEncoding.Location = new System.Drawing.Point(292, 164);
            this.chkSaveAsEncoding.Name = "chkSaveAsEncoding";
            this.chkSaveAsEncoding.Padding = new System.Windows.Forms.Padding(1);
            this.chkSaveAsEncoding.Size = new System.Drawing.Size(130, 22);
            this.chkSaveAsEncoding.TabIndex = 70;
            this.chkSaveAsEncoding.Text = "Save as Encoding:";
            this.c1ThemeController1.SetTheme(this.chkSaveAsEncoding, "(default)");
            this.chkSaveAsEncoding.UseVisualStyleBackColor = true;
            this.chkSaveAsEncoding.Value = true;
            this.chkSaveAsEncoding.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkCopyAsHTML
            // 
            this.chkCopyAsHTML.AutoSize = true;
            this.chkCopyAsHTML.BackColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCopyAsHTML.ForeColor = System.Drawing.Color.Black;
            this.chkCopyAsHTML.Location = new System.Drawing.Point(292, 139);
            this.chkCopyAsHTML.Name = "chkCopyAsHTML";
            this.chkCopyAsHTML.Padding = new System.Windows.Forms.Padding(1);
            this.chkCopyAsHTML.Size = new System.Drawing.Size(197, 22);
            this.chkCopyAsHTML.TabIndex = 68;
            this.chkCopyAsHTML.Text = "Copy SQL Statement as HTML";
            this.c1ThemeController1.SetTheme(this.chkCopyAsHTML, "(default)");
            this.chkCopyAsHTML.UseVisualStyleBackColor = true;
            this.chkCopyAsHTML.Value = null;
            this.chkCopyAsHTML.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkCopyAsHTML.CheckedChanged += new System.EventHandler(this.chkCopyAsHTML_CheckedChanged);
            // 
            // chkBold
            // 
            this.chkBold.AutoSize = true;
            this.chkBold.BackColor = System.Drawing.Color.Transparent;
            this.chkBold.BorderColor = System.Drawing.Color.Transparent;
            this.chkBold.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkBold.ForeColor = System.Drawing.Color.Black;
            this.chkBold.Location = new System.Drawing.Point(292, 114);
            this.chkBold.Name = "chkBold";
            this.chkBold.Padding = new System.Windows.Forms.Padding(1);
            this.chkBold.Size = new System.Drawing.Size(192, 22);
            this.chkBold.TabIndex = 66;
            this.chkBold.Text = "Using Bold Text for Keywords";
            this.c1ThemeController1.SetTheme(this.chkBold, "(default)");
            this.chkBold.UseVisualStyleBackColor = true;
            this.chkBold.Value = null;
            this.chkBold.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkBold.CheckedChanged += new System.EventHandler(this.chkBold_CheckedChanged);
            // 
            // cboEditorFontPicker
            // 
            this.cboEditorFontPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboEditorFontPicker.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboEditorFontPicker.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboEditorFontPicker.Location = new System.Drawing.Point(113, 24);
            this.cboEditorFontPicker.Name = "cboEditorFontPicker";
            this.cboEditorFontPicker.Size = new System.Drawing.Size(143, 21);
            this.cboEditorFontPicker.TabIndex = 56;
            this.cboEditorFontPicker.Tag = null;
            this.c1ThemeController1.SetTheme(this.cboEditorFontPicker, "(default)");
            this.cboEditorFontPicker.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboEditorFontPicker.TextChanged += new System.EventHandler(this.cboEditorFontPicker_TextChanged);
            // 
            // lblStarHighlightSelection
            // 
            this.lblStarHighlightSelection.AutoSize = true;
            this.lblStarHighlightSelection.Enabled = false;
            this.lblStarHighlightSelection.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarHighlightSelection.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarHighlightSelection.Location = new System.Drawing.Point(859, 90);
            this.lblStarHighlightSelection.Name = "lblStarHighlightSelection";
            this.lblStarHighlightSelection.Size = new System.Drawing.Size(14, 15);
            this.lblStarHighlightSelection.TabIndex = 50;
            this.lblStarHighlightSelection.Text = "*";
            this.lblStarHighlightSelection.Visible = false;
            // 
            // lblEditorFontName
            // 
            this.lblEditorFontName.Location = new System.Drawing.Point(-50, 25);
            this.lblEditorFontName.Name = "lblEditorFontName";
            this.lblEditorFontName.Size = new System.Drawing.Size(160, 16);
            this.lblEditorFontName.TabIndex = 43;
            this.lblEditorFontName.Text = "Font Name:";
            this.lblEditorFontName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEditorFontSize
            // 
            this.lblEditorFontSize.Location = new System.Drawing.Point(-50, 57);
            this.lblEditorFontSize.Name = "lblEditorFontSize";
            this.lblEditorFontSize.Size = new System.Drawing.Size(160, 16);
            this.lblEditorFontSize.TabIndex = 45;
            this.lblEditorFontSize.Text = "Font Size:";
            this.lblEditorFontSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEditorZoom
            // 
            this.lblEditorZoom.Location = new System.Drawing.Point(719, 293);
            this.lblEditorZoom.Name = "lblEditorZoom";
            this.lblEditorZoom.Size = new System.Drawing.Size(75, 16);
            this.lblEditorZoom.TabIndex = 14;
            this.lblEditorZoom.Text = "Zoom:";
            this.lblEditorZoom.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblEditorZoom.Visible = false;
            // 
            // chkShowAllCharacters
            // 
            this.chkShowAllCharacters.AutoSize = true;
            this.chkShowAllCharacters.BackColor = System.Drawing.Color.Transparent;
            this.chkShowAllCharacters.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowAllCharacters.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowAllCharacters.ForeColor = System.Drawing.Color.Black;
            this.chkShowAllCharacters.Location = new System.Drawing.Point(292, 189);
            this.chkShowAllCharacters.Name = "chkShowAllCharacters";
            this.chkShowAllCharacters.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowAllCharacters.Size = new System.Drawing.Size(138, 22);
            this.chkShowAllCharacters.TabIndex = 69;
            this.chkShowAllCharacters.Text = "Show All Characters";
            this.c1ThemeController1.SetTheme(this.chkShowAllCharacters, "(default)");
            this.chkShowAllCharacters.UseVisualStyleBackColor = true;
            this.chkShowAllCharacters.Value = null;
            this.chkShowAllCharacters.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowAllCharacters.CheckedChanged += new System.EventHandler(this.chkShowAllCharacters_CheckedChanged);
            // 
            // chkHighlightSelection
            // 
            this.chkHighlightSelection.AutoSize = true;
            this.chkHighlightSelection.BackColor = System.Drawing.Color.Transparent;
            this.chkHighlightSelection.BorderColor = System.Drawing.Color.Transparent;
            this.chkHighlightSelection.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkHighlightSelection.ForeColor = System.Drawing.Color.Black;
            this.chkHighlightSelection.Location = new System.Drawing.Point(615, 79);
            this.chkHighlightSelection.Name = "chkHighlightSelection";
            this.chkHighlightSelection.Padding = new System.Windows.Forms.Padding(1);
            this.chkHighlightSelection.Size = new System.Drawing.Size(242, 22);
            this.chkHighlightSelection.TabIndex = 73;
            this.chkHighlightSelection.Text = "Highlight Selection When Mouse Click";
            this.c1ThemeController1.SetTheme(this.chkHighlightSelection, "(default)");
            this.chkHighlightSelection.UseVisualStyleBackColor = true;
            this.chkHighlightSelection.Value = null;
            this.chkHighlightSelection.Visible = false;
            this.chkHighlightSelection.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpColorTheme
            // 
            this.grpColorTheme.BackColor = System.Drawing.Color.Transparent;
            this.grpColorTheme.Controls.Add(this.btnHelp_DarkMode);
            this.grpColorTheme.Controls.Add(this.chkDarkMode);
            this.grpColorTheme.Location = new System.Drawing.Point(12, 9);
            this.grpColorTheme.Name = "grpColorTheme";
            this.grpColorTheme.Size = new System.Drawing.Size(158, 58);
            this.grpColorTheme.TabIndex = 43;
            this.grpColorTheme.TabStop = false;
            this.grpColorTheme.Text = "Color Theme";
            // 
            // btnHelp_DarkMode
            // 
            this.btnHelp_DarkMode.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_DarkMode.Image")));
            this.btnHelp_DarkMode.Location = new System.Drawing.Point(115, 23);
            this.btnHelp_DarkMode.Name = "btnHelp_DarkMode";
            this.btnHelp_DarkMode.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_DarkMode.TabIndex = 80;
            this.c1ThemeController1.SetTheme(this.btnHelp_DarkMode, "(default)");
            this.btnHelp_DarkMode.UseVisualStyleBackColor = true;
            this.btnHelp_DarkMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_DarkMode.Click += new System.EventHandler(this.btnHelp_DarkMode_Click);
            // 
            // chkDarkMode
            // 
            this.chkDarkMode.AutoSize = true;
            this.chkDarkMode.BackColor = System.Drawing.Color.Transparent;
            this.chkDarkMode.BorderColor = System.Drawing.Color.Transparent;
            this.chkDarkMode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDarkMode.ForeColor = System.Drawing.Color.Black;
            this.chkDarkMode.Location = new System.Drawing.Point(17, 24);
            this.chkDarkMode.Name = "chkDarkMode";
            this.chkDarkMode.Padding = new System.Windows.Forms.Padding(1);
            this.chkDarkMode.Size = new System.Drawing.Size(92, 22);
            this.chkDarkMode.TabIndex = 79;
            this.chkDarkMode.Text = "Dark Mode";
            this.c1ThemeController1.SetTheme(this.chkDarkMode, "(default)");
            this.chkDarkMode.UseVisualStyleBackColor = true;
            this.chkDarkMode.Value = null;
            this.chkDarkMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpQueryEditorPreview
            // 
            this.grpQueryEditorPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpQueryEditorPreview.BackColor = System.Drawing.Color.Transparent;
            this.grpQueryEditorPreview.Controls.Add(this.tsEditor);
            this.grpQueryEditorPreview.Controls.Add(this.editor);
            this.grpQueryEditorPreview.Location = new System.Drawing.Point(339, 307);
            this.grpQueryEditorPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpQueryEditorPreview.Name = "grpQueryEditorPreview";
            this.grpQueryEditorPreview.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpQueryEditorPreview.Size = new System.Drawing.Size(861, 366);
            this.grpQueryEditorPreview.TabIndex = 20;
            this.grpQueryEditorPreview.TabStop = false;
            this.grpQueryEditorPreview.Text = "Preview";
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
            this.btnSelectCurrentBlock,
            this.btnExecuteCurrentBlock,
            this.btnCancelQuery,
            this.toolStripSeparator4,
            this.btnCode2Sql,
            this.btnSql2Code,
            this.toolStripSeparator9,
            this.toolStripSeparator3,
            this.btnComment,
            this.btnRemoveComment,
            this.toolStripSeparator6,
            this.btnIndent,
            this.txtIndentWord,
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
            this.toolStripSeparator7});
            this.tsEditor.Location = new System.Drawing.Point(3, 20);
            this.tsEditor.Name = "tsEditor";
            this.tsEditor.Size = new System.Drawing.Size(855, 31);
            this.tsEditor.TabIndex = 41;
            this.c1ThemeController1.SetTheme(this.tsEditor, "(default)");
            // 
            // btnNew
            // 
            this.btnNew.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNew.Enabled = false;
            this.btnNew.Image = ((System.Drawing.Image)(resources.GetObject("btnNew.Image")));
            this.btnNew.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNew.Name = "btnNew";
            this.btnNew.Size = new System.Drawing.Size(28, 28);
            this.btnNew.Tag = "";
            // 
            // btnOpen
            // 
            this.btnOpen.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpen.Enabled = false;
            this.btnOpen.Image = ((System.Drawing.Image)(resources.GetObject("btnOpen.Image")));
            this.btnOpen.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(28, 28);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Enabled = false;
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(28, 28);
            // 
            // btnSaveRed
            // 
            this.btnSaveRed.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveRed.Enabled = false;
            this.btnSaveRed.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveRed.Image")));
            this.btnSaveRed.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveRed.Name = "btnSaveRed";
            this.btnSaveRed.Size = new System.Drawing.Size(28, 28);
            this.btnSaveRed.Visible = false;
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAs.Enabled = false;
            this.btnSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Image")));
            this.btnSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(28, 28);
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
            // 
            // btnSelectCurrentBlock
            // 
            this.btnSelectCurrentBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectCurrentBlock.Enabled = false;
            this.btnSelectCurrentBlock.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectCurrentBlock.Image")));
            this.btnSelectCurrentBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectCurrentBlock.Name = "btnSelectCurrentBlock";
            this.btnSelectCurrentBlock.Size = new System.Drawing.Size(28, 28);
            // 
            // btnExecuteCurrentBlock
            // 
            this.btnExecuteCurrentBlock.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExecuteCurrentBlock.Enabled = false;
            this.btnExecuteCurrentBlock.Image = ((System.Drawing.Image)(resources.GetObject("btnExecuteCurrentBlock.Image")));
            this.btnExecuteCurrentBlock.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExecuteCurrentBlock.Name = "btnExecuteCurrentBlock";
            this.btnExecuteCurrentBlock.Size = new System.Drawing.Size(28, 28);
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
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // btnCode2Sql
            // 
            this.btnCode2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCode2Sql.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCSharp2Sql,
            this.mnuVB2Sql,
            this.mnuDephi2Sql});
            this.btnCode2Sql.Enabled = false;
            this.btnCode2Sql.Image = ((System.Drawing.Image)(resources.GetObject("btnCode2Sql.Image")));
            this.btnCode2Sql.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCode2Sql.Name = "btnCode2Sql";
            this.btnCode2Sql.Size = new System.Drawing.Size(37, 28);
            this.btnCode2Sql.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            // 
            // mnuCSharp2Sql
            // 
            this.mnuCSharp2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuCSharp2Sql.Name = "mnuCSharp2Sql";
            this.mnuCSharp2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuCSharp2Sql.Text = "C# to SQL";
            // 
            // mnuVB2Sql
            // 
            this.mnuVB2Sql.Name = "mnuVB2Sql";
            this.mnuVB2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuVB2Sql.Text = "VB.Net/VB6/VBA to SQL";
            // 
            // mnuDephi2Sql
            // 
            this.mnuDephi2Sql.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuDephi2Sql.Name = "mnuDephi2Sql";
            this.mnuDephi2Sql.Size = new System.Drawing.Size(209, 22);
            this.mnuDephi2Sql.Text = "Dephi6 to SQL";
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
            // 
            // mnuSql2CSharp
            // 
            this.mnuSql2CSharp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.mnuSql2CSharp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCSharpStyle1,
            this.mnuCSharpStyle2,
            this.mnuCSharpStyle3});
            this.mnuSql2CSharp.Name = "mnuSql2CSharp";
            this.mnuSql2CSharp.Size = new System.Drawing.Size(165, 22);
            this.mnuSql2CSharp.Text = "SQL to C#";
            // 
            // mnuCSharpStyle1
            // 
            this.mnuCSharpStyle1.Name = "mnuCSharpStyle1";
            this.mnuCSharpStyle1.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle1.Tag = "C#";
            this.mnuCSharpStyle1.Text = "Style 1: Using + operator";
            // 
            // mnuCSharpStyle2
            // 
            this.mnuCSharpStyle2.Name = "mnuCSharpStyle2";
            this.mnuCSharpStyle2.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle2.Tag = "C#";
            this.mnuCSharpStyle2.Text = "Style 2: New line character \\r\\n";
            // 
            // mnuCSharpStyle3
            // 
            this.mnuCSharpStyle3.Name = "mnuCSharpStyle3";
            this.mnuCSharpStyle3.Size = new System.Drawing.Size(340, 22);
            this.mnuCSharpStyle3.Tag = "C#";
            this.mnuCSharpStyle3.Text = "Style 3: New line character Enviroment.NewLine";
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
            this.mnuVBNetStyle1.Tag = "VB.Net";
            this.mnuVBNetStyle1.Text = "Style 1: Using && operator";
            // 
            // mnuVBNetStyle2
            // 
            this.mnuVBNetStyle2.Name = "mnuVBNetStyle2";
            this.mnuVBNetStyle2.Size = new System.Drawing.Size(340, 22);
            this.mnuVBNetStyle2.Tag = "VB.Net";
            this.mnuVBNetStyle2.Text = "Style 2: New line character VbCrLf";
            // 
            // mnuVBNetStyle3
            // 
            this.mnuVBNetStyle3.Name = "mnuVBNetStyle3";
            this.mnuVBNetStyle3.Size = new System.Drawing.Size(340, 22);
            this.mnuVBNetStyle3.Tag = "VB.Net";
            this.mnuVBNetStyle3.Text = "Style 3: New line character Enviroment.NewLine";
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
            this.mnuVB6AStyle1.Tag = "VB6/VBA";
            this.mnuVB6AStyle1.Text = "Style 1: Using && operator";
            // 
            // mnuVB6AStyle2
            // 
            this.mnuVB6AStyle2.Name = "mnuVB6AStyle2";
            this.mnuVB6AStyle2.Size = new System.Drawing.Size(262, 22);
            this.mnuVB6AStyle2.Tag = "VB6/VBA";
            this.mnuVB6AStyle2.Text = "Style 2: New line character VbCrLf";
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
            this.mnuDelphi6Style1.Tag = "Delphi6";
            this.mnuDelphi6Style1.Text = "Style 1: Using + operator";
            // 
            // mnuDelphi6Style2
            // 
            this.mnuDelphi6Style2.Name = "mnuDelphi6Style2";
            this.mnuDelphi6Style2.Size = new System.Drawing.Size(268, 22);
            this.mnuDelphi6Style2.Tag = "Delphi6";
            this.mnuDelphi6Style2.Text = "Style 2: New line character #13#10";
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 31);
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
            // 
            // btnRemoveComment
            // 
            this.btnRemoveComment.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRemoveComment.Enabled = false;
            this.btnRemoveComment.Image = ((System.Drawing.Image)(resources.GetObject("btnRemoveComment.Image")));
            this.btnRemoveComment.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRemoveComment.Name = "btnRemoveComment";
            this.btnRemoveComment.Size = new System.Drawing.Size(28, 28);
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
            // 
            // txtIndentWord
            // 
            this.txtIndentWord.BackColor = System.Drawing.Color.Ivory;
            this.txtIndentWord.Enabled = false;
            this.txtIndentWord.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtIndentWord.MaxLength = 1;
            this.txtIndentWord.Name = "txtIndentWord";
            this.txtIndentWord.Size = new System.Drawing.Size(15, 31);
            this.txtIndentWord.Text = "4";
            this.txtIndentWord.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Center;
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
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 31);
            // 
            // btnHighlightSelection
            // 
            this.btnHighlightSelection.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightSelection.Enabled = false;
            this.btnHighlightSelection.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightSelection.Image")));
            this.btnHighlightSelection.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightSelection.Name = "btnHighlightSelection";
            this.btnHighlightSelection.Size = new System.Drawing.Size(28, 28);
            this.btnHighlightSelection.Visible = false;
            // 
            // btnHighlightSelection2
            // 
            this.btnHighlightSelection2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightSelection2.Enabled = false;
            this.btnHighlightSelection2.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightSelection2.Image")));
            this.btnHighlightSelection2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightSelection2.Name = "btnHighlightSelection2";
            this.btnHighlightSelection2.Size = new System.Drawing.Size(28, 28);
            this.btnHighlightSelection2.Visible = false;
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
            this.btnWordWrap.Enabled = false;
            this.btnWordWrap.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap.Image")));
            this.btnWordWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap.Name = "btnWordWrap";
            this.btnWordWrap.Size = new System.Drawing.Size(28, 28);
            // 
            // btnWordWrap2
            // 
            this.btnWordWrap2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2.Enabled = false;
            this.btnWordWrap2.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2.Image")));
            this.btnWordWrap2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2.Name = "btnWordWrap2";
            this.btnWordWrap2.Size = new System.Drawing.Size(28, 28);
            this.btnWordWrap2.Visible = false;
            // 
            // btnShowAllCharacters
            // 
            this.btnShowAllCharacters.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters.Enabled = false;
            this.btnShowAllCharacters.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters.Image")));
            this.btnShowAllCharacters.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters.Name = "btnShowAllCharacters";
            this.btnShowAllCharacters.Size = new System.Drawing.Size(28, 28);
            // 
            // btnShowAllCharacters2
            // 
            this.btnShowAllCharacters2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2.Enabled = false;
            this.btnShowAllCharacters2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2.Image")));
            this.btnShowAllCharacters2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2.Name = "btnShowAllCharacters2";
            this.btnShowAllCharacters2.Size = new System.Drawing.Size(28, 28);
            this.btnShowAllCharacters2.Visible = false;
            // 
            // btnShowIndentGuide
            // 
            this.btnShowIndentGuide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowIndentGuide.Enabled = false;
            this.btnShowIndentGuide.Image = ((System.Drawing.Image)(resources.GetObject("btnShowIndentGuide.Image")));
            this.btnShowIndentGuide.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowIndentGuide.Name = "btnShowIndentGuide";
            this.btnShowIndentGuide.Size = new System.Drawing.Size(28, 28);
            // 
            // btnShowIndentGuide2
            // 
            this.btnShowIndentGuide2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowIndentGuide2.Enabled = false;
            this.btnShowIndentGuide2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowIndentGuide2.Image")));
            this.btnShowIndentGuide2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowIndentGuide2.Name = "btnShowIndentGuide2";
            this.btnShowIndentGuide2.Size = new System.Drawing.Size(28, 28);
            this.btnShowIndentGuide2.Visible = false;
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 31);
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editor.CaretLineVisible = true;
            this.editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editor.Location = new System.Drawing.Point(12, 55);
            this.editor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editor.Name = "editor";
            this.editor.ScrollWidth = 3618;
            this.editor.Size = new System.Drawing.Size(838, 299);
            this.editor.Styler = null;
            this.editor.TabIndex = 40;
            this.editor.Text = resources.GetString("editor.Text");
            this.editor.WhitespaceSize = 3;
            this.editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editor.WrapMode = ScintillaNET.WrapMode.Word;
            this.editor.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editor_DoubleClick);
            this.editor.ZoomChanged += new System.EventHandler<System.EventArgs>(this.editor_ZoomChanged);
            this.editor.Enter += new System.EventHandler(this.editor_Enter);
            this.editor.Leave += new System.EventHandler(this.editor_Leave);
            this.editor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.editor_MouseClick);
            this.editor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editor_MouseDown);
            // 
            // grpAutoComplete
            // 
            this.grpAutoComplete.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAutoComplete.BackColor = System.Drawing.Color.Transparent;
            this.grpAutoComplete.Controls.Add(this.chkFirstCharChecking);
            this.grpAutoComplete.Controls.Add(this.nudMinFragmentLength);
            this.grpAutoComplete.Controls.Add(this.lblMinFragmentLength);
            this.grpAutoComplete.Controls.Add(this.grpAutoCompleteFor);
            this.grpAutoComplete.Location = new System.Drawing.Point(12, 9);
            this.grpAutoComplete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoComplete.Name = "grpAutoComplete";
            this.grpAutoComplete.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoComplete.Size = new System.Drawing.Size(1186, 664);
            this.grpAutoComplete.TabIndex = 0;
            this.grpAutoComplete.TabStop = false;
            this.grpAutoComplete.Text = "                                                                                 " +
    "   ";
            // 
            // chkFirstCharChecking
            // 
            this.chkFirstCharChecking.AutoSize = true;
            this.chkFirstCharChecking.BackColor = System.Drawing.Color.Transparent;
            this.chkFirstCharChecking.BorderColor = System.Drawing.Color.Transparent;
            this.chkFirstCharChecking.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkFirstCharChecking.ForeColor = System.Drawing.Color.Black;
            this.chkFirstCharChecking.Location = new System.Drawing.Point(21, 55);
            this.chkFirstCharChecking.Name = "chkFirstCharChecking";
            this.chkFirstCharChecking.Padding = new System.Windows.Forms.Padding(1);
            this.chkFirstCharChecking.Size = new System.Drawing.Size(465, 22);
            this.chkFirstCharChecking.TabIndex = 313;
            this.chkFirstCharChecking.Text = "If the first character is not an English letter, autocomplete will not be trigger" +
    "ed.";
            this.c1ThemeController1.SetTheme(this.chkFirstCharChecking, "(default)");
            this.chkFirstCharChecking.UseVisualStyleBackColor = true;
            this.chkFirstCharChecking.Value = null;
            this.chkFirstCharChecking.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // nudMinFragmentLength
            // 
            this.nudMinFragmentLength.ForeColor = System.Drawing.Color.Black;
            this.nudMinFragmentLength.Location = new System.Drawing.Point(357, 26);
            this.nudMinFragmentLength.Maximum = new decimal(new int[] {
            9,
            0,
            0,
            0});
            this.nudMinFragmentLength.Minimum = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudMinFragmentLength.Name = "nudMinFragmentLength";
            this.nudMinFragmentLength.Size = new System.Drawing.Size(31, 23);
            this.nudMinFragmentLength.TabIndex = 311;
            this.nudMinFragmentLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudMinFragmentLength, "Office2010Blue");
            this.nudMinFragmentLength.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            this.nudMinFragmentLength.Leave += new System.EventHandler(this.nudMinFragmentLength_Leave);
            // 
            // lblMinFragmentLength
            // 
            this.lblMinFragmentLength.AutoSize = true;
            this.lblMinFragmentLength.Location = new System.Drawing.Point(21, 28);
            this.lblMinFragmentLength.Name = "lblMinFragmentLength";
            this.lblMinFragmentLength.Size = new System.Drawing.Size(303, 16);
            this.lblMinFragmentLength.TabIndex = 50;
            this.lblMinFragmentLength.Text = "Minimum fragment length to trigger Auto Complete:";
            this.lblMinFragmentLength.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpAutoCompleteFor
            // 
            this.grpAutoCompleteFor.Controls.Add(this.chkUserDefinedViews);
            this.grpAutoCompleteFor.Controls.Add(this.chkUserDefinedTriggers);
            this.grpAutoCompleteFor.Controls.Add(this.chkUserDefinedTables);
            this.grpAutoCompleteFor.Controls.Add(this.chkUserDefinedFunctions);
            this.grpAutoCompleteFor.Controls.Add(this.chkUserDefinedKeywords);
            this.grpAutoCompleteFor.Controls.Add(this.chkBuiltInKeywords);
            this.grpAutoCompleteFor.Controls.Add(this.chkBuiltInFunctions);
            this.grpAutoCompleteFor.Location = new System.Drawing.Point(13, 91);
            this.grpAutoCompleteFor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoCompleteFor.Name = "grpAutoCompleteFor";
            this.grpAutoCompleteFor.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoCompleteFor.Size = new System.Drawing.Size(446, 225);
            this.grpAutoCompleteFor.TabIndex = 1;
            this.grpAutoCompleteFor.TabStop = false;
            this.grpAutoCompleteFor.Text = "Auto Complete for";
            // 
            // chkUserDefinedViews
            // 
            this.chkUserDefinedViews.AutoSize = true;
            this.chkUserDefinedViews.BackColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedViews.BorderColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedViews.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUserDefinedViews.ForeColor = System.Drawing.Color.Black;
            this.chkUserDefinedViews.Location = new System.Drawing.Point(16, 189);
            this.chkUserDefinedViews.Name = "chkUserDefinedViews";
            this.chkUserDefinedViews.Padding = new System.Windows.Forms.Padding(1);
            this.chkUserDefinedViews.Size = new System.Drawing.Size(137, 22);
            this.chkUserDefinedViews.TabIndex = 309;
            this.chkUserDefinedViews.Text = "User-defined Views";
            this.c1ThemeController1.SetTheme(this.chkUserDefinedViews, "(default)");
            this.chkUserDefinedViews.UseVisualStyleBackColor = true;
            this.chkUserDefinedViews.Value = null;
            this.chkUserDefinedViews.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkUserDefinedTriggers
            // 
            this.chkUserDefinedTriggers.AutoSize = true;
            this.chkUserDefinedTriggers.BackColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedTriggers.BorderColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedTriggers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUserDefinedTriggers.ForeColor = System.Drawing.Color.Black;
            this.chkUserDefinedTriggers.Location = new System.Drawing.Point(16, 161);
            this.chkUserDefinedTriggers.Name = "chkUserDefinedTriggers";
            this.chkUserDefinedTriggers.Padding = new System.Windows.Forms.Padding(1);
            this.chkUserDefinedTriggers.Size = new System.Drawing.Size(151, 22);
            this.chkUserDefinedTriggers.TabIndex = 308;
            this.chkUserDefinedTriggers.Text = "User-defined Triggers";
            this.c1ThemeController1.SetTheme(this.chkUserDefinedTriggers, "(default)");
            this.chkUserDefinedTriggers.UseVisualStyleBackColor = true;
            this.chkUserDefinedTriggers.Value = null;
            this.chkUserDefinedTriggers.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkUserDefinedTables
            // 
            this.chkUserDefinedTables.AutoSize = true;
            this.chkUserDefinedTables.BackColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedTables.BorderColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedTables.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUserDefinedTables.ForeColor = System.Drawing.Color.Black;
            this.chkUserDefinedTables.Location = new System.Drawing.Point(16, 134);
            this.chkUserDefinedTables.Name = "chkUserDefinedTables";
            this.chkUserDefinedTables.Padding = new System.Windows.Forms.Padding(1);
            this.chkUserDefinedTables.Size = new System.Drawing.Size(142, 22);
            this.chkUserDefinedTables.TabIndex = 307;
            this.chkUserDefinedTables.Text = "User-defined Tables";
            this.c1ThemeController1.SetTheme(this.chkUserDefinedTables, "(default)");
            this.chkUserDefinedTables.UseVisualStyleBackColor = true;
            this.chkUserDefinedTables.Value = null;
            this.chkUserDefinedTables.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkUserDefinedFunctions
            // 
            this.chkUserDefinedFunctions.AutoSize = true;
            this.chkUserDefinedFunctions.BackColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedFunctions.BorderColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedFunctions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUserDefinedFunctions.ForeColor = System.Drawing.Color.Black;
            this.chkUserDefinedFunctions.Location = new System.Drawing.Point(16, 107);
            this.chkUserDefinedFunctions.Name = "chkUserDefinedFunctions";
            this.chkUserDefinedFunctions.Padding = new System.Windows.Forms.Padding(1);
            this.chkUserDefinedFunctions.Size = new System.Drawing.Size(158, 22);
            this.chkUserDefinedFunctions.TabIndex = 306;
            this.chkUserDefinedFunctions.Text = "User-defined Functions";
            this.c1ThemeController1.SetTheme(this.chkUserDefinedFunctions, "(default)");
            this.chkUserDefinedFunctions.UseVisualStyleBackColor = true;
            this.chkUserDefinedFunctions.Value = null;
            this.chkUserDefinedFunctions.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkUserDefinedKeywords
            // 
            this.chkUserDefinedKeywords.AutoSize = true;
            this.chkUserDefinedKeywords.BackColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedKeywords.BorderColor = System.Drawing.Color.Transparent;
            this.chkUserDefinedKeywords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkUserDefinedKeywords.ForeColor = System.Drawing.Color.Black;
            this.chkUserDefinedKeywords.Location = new System.Drawing.Point(16, 80);
            this.chkUserDefinedKeywords.Name = "chkUserDefinedKeywords";
            this.chkUserDefinedKeywords.Padding = new System.Windows.Forms.Padding(1);
            this.chkUserDefinedKeywords.Size = new System.Drawing.Size(159, 22);
            this.chkUserDefinedKeywords.TabIndex = 304;
            this.chkUserDefinedKeywords.Text = "User-defined Keywords";
            this.c1ThemeController1.SetTheme(this.chkUserDefinedKeywords, "(default)");
            this.chkUserDefinedKeywords.UseVisualStyleBackColor = true;
            this.chkUserDefinedKeywords.Value = null;
            this.chkUserDefinedKeywords.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkBuiltInKeywords
            // 
            this.chkBuiltInKeywords.AutoSize = true;
            this.chkBuiltInKeywords.BackColor = System.Drawing.Color.Transparent;
            this.chkBuiltInKeywords.BorderColor = System.Drawing.Color.Transparent;
            this.chkBuiltInKeywords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkBuiltInKeywords.ForeColor = System.Drawing.Color.Black;
            this.chkBuiltInKeywords.Location = new System.Drawing.Point(16, 53);
            this.chkBuiltInKeywords.Name = "chkBuiltInKeywords";
            this.chkBuiltInKeywords.Padding = new System.Windows.Forms.Padding(1);
            this.chkBuiltInKeywords.Size = new System.Drawing.Size(124, 22);
            this.chkBuiltInKeywords.TabIndex = 303;
            this.chkBuiltInKeywords.Text = "Built-in Keywords";
            this.c1ThemeController1.SetTheme(this.chkBuiltInKeywords, "(default)");
            this.chkBuiltInKeywords.UseVisualStyleBackColor = true;
            this.chkBuiltInKeywords.Value = null;
            this.chkBuiltInKeywords.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkBuiltInFunctions
            // 
            this.chkBuiltInFunctions.AutoSize = true;
            this.chkBuiltInFunctions.BackColor = System.Drawing.Color.Transparent;
            this.chkBuiltInFunctions.BorderColor = System.Drawing.Color.Transparent;
            this.chkBuiltInFunctions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkBuiltInFunctions.ForeColor = System.Drawing.Color.Black;
            this.chkBuiltInFunctions.Location = new System.Drawing.Point(16, 26);
            this.chkBuiltInFunctions.Name = "chkBuiltInFunctions";
            this.chkBuiltInFunctions.Padding = new System.Windows.Forms.Padding(1);
            this.chkBuiltInFunctions.Size = new System.Drawing.Size(123, 22);
            this.chkBuiltInFunctions.TabIndex = 302;
            this.chkBuiltInFunctions.Text = "Built-in Functions";
            this.c1ThemeController1.SetTheme(this.chkBuiltInFunctions, "(default)");
            this.chkBuiltInFunctions.UseVisualStyleBackColor = true;
            this.chkBuiltInFunctions.Value = null;
            this.chkBuiltInFunctions.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnHelp_EnableAutoComplete
            // 
            this.btnHelp_EnableAutoComplete.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_EnableAutoComplete.Image")));
            this.btnHelp_EnableAutoComplete.Location = new System.Drawing.Point(300, 7);
            this.btnHelp_EnableAutoComplete.Name = "btnHelp_EnableAutoComplete";
            this.btnHelp_EnableAutoComplete.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_EnableAutoComplete.TabIndex = 314;
            this.c1ThemeController1.SetTheme(this.btnHelp_EnableAutoComplete, "(default)");
            this.btnHelp_EnableAutoComplete.UseVisualStyleBackColor = true;
            this.btnHelp_EnableAutoComplete.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_EnableAutoComplete.Click += new System.EventHandler(this.btnHelp_EnableAutoComplete_Click);
            // 
            // lblStarAutoComplete
            // 
            this.lblStarAutoComplete.AutoSize = true;
            this.lblStarAutoComplete.BackColor = System.Drawing.Color.Transparent;
            this.lblStarAutoComplete.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarAutoComplete.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarAutoComplete.Location = new System.Drawing.Point(272, 11);
            this.lblStarAutoComplete.Name = "lblStarAutoComplete";
            this.lblStarAutoComplete.Size = new System.Drawing.Size(14, 15);
            this.lblStarAutoComplete.TabIndex = 42;
            this.lblStarAutoComplete.Text = "*";
            // 
            // chkEnableAutoComplete
            // 
            this.chkEnableAutoComplete.AutoSize = true;
            this.chkEnableAutoComplete.BackColor = System.Drawing.Color.Transparent;
            this.chkEnableAutoComplete.BorderColor = System.Drawing.Color.Transparent;
            this.chkEnableAutoComplete.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEnableAutoComplete.ForeColor = System.Drawing.Color.Black;
            this.chkEnableAutoComplete.Location = new System.Drawing.Point(21, 8);
            this.chkEnableAutoComplete.Name = "chkEnableAutoComplete";
            this.chkEnableAutoComplete.Padding = new System.Windows.Forms.Padding(1);
            this.chkEnableAutoComplete.Size = new System.Drawing.Size(227, 22);
            this.chkEnableAutoComplete.TabIndex = 301;
            this.chkEnableAutoComplete.Text = "Enable Auto Complete while typing";
            this.c1ThemeController1.SetTheme(this.chkEnableAutoComplete, "(default)");
            this.chkEnableAutoComplete.UseVisualStyleBackColor = true;
            this.chkEnableAutoComplete.Value = null;
            this.chkEnableAutoComplete.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkEnableAutoComplete.CheckedChanged += new System.EventHandler(this.chkEnableGroupBox_CheckedChanged);
            // 
            // grpAutoReplace
            // 
            this.grpAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpAutoReplace.BackColor = System.Drawing.Color.Transparent;
            this.grpAutoReplace.Controls.Add(this.lblAutoReplaceInfo2);
            this.grpAutoReplace.Controls.Add(this.chkShowFilterRowAutoReplace);
            this.grpAutoReplace.Controls.Add(this.lblAutoReplaceInfo1);
            this.grpAutoReplace.Controls.Add(this.grpModifyDefinitionAutoReplace);
            this.grpAutoReplace.Controls.Add(this.grpDefinitionAutoReplace);
            this.grpAutoReplace.Location = new System.Drawing.Point(12, 9);
            this.grpAutoReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoReplace.Name = "grpAutoReplace";
            this.grpAutoReplace.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpAutoReplace.Size = new System.Drawing.Size(1186, 664);
            this.grpAutoReplace.TabIndex = 0;
            this.grpAutoReplace.TabStop = false;
            this.grpAutoReplace.Text = "                             ";
            // 
            // lblAutoReplaceInfo2
            // 
            this.lblAutoReplaceInfo2.AutoSize = true;
            this.lblAutoReplaceInfo2.Location = new System.Drawing.Point(20, 51);
            this.lblAutoReplaceInfo2.Name = "lblAutoReplaceInfo2";
            this.lblAutoReplaceInfo2.Size = new System.Drawing.Size(404, 16);
            this.lblAutoReplaceInfo2.TabIndex = 403;
            this.lblAutoReplaceInfo2.Text = "Press Ctrl+Z to undo the replacement and restore the original keyword.";
            this.c1ThemeController1.SetTheme(this.lblAutoReplaceInfo2, "(default)");
            // 
            // chkShowFilterRowAutoReplace
            // 
            this.chkShowFilterRowAutoReplace.AutoSize = true;
            this.chkShowFilterRowAutoReplace.BackColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRowAutoReplace.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRowAutoReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowFilterRowAutoReplace.ForeColor = System.Drawing.Color.Black;
            this.chkShowFilterRowAutoReplace.Location = new System.Drawing.Point(555, 60);
            this.chkShowFilterRowAutoReplace.Name = "chkShowFilterRowAutoReplace";
            this.chkShowFilterRowAutoReplace.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowFilterRowAutoReplace.Size = new System.Drawing.Size(117, 22);
            this.chkShowFilterRowAutoReplace.TabIndex = 402;
            this.chkShowFilterRowAutoReplace.Text = "Show Filter Row";
            this.c1ThemeController1.SetTheme(this.chkShowFilterRowAutoReplace, "(default)");
            this.chkShowFilterRowAutoReplace.UseVisualStyleBackColor = true;
            this.chkShowFilterRowAutoReplace.Value = null;
            this.chkShowFilterRowAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowFilterRowAutoReplace.CheckedChanged += new System.EventHandler(this.chkShowFilterRow_CheckedChanged);
            // 
            // lblAutoReplaceInfo1
            // 
            this.lblAutoReplaceInfo1.AutoSize = true;
            this.lblAutoReplaceInfo1.Location = new System.Drawing.Point(20, 26);
            this.lblAutoReplaceInfo1.Name = "lblAutoReplaceInfo1";
            this.lblAutoReplaceInfo1.Size = new System.Drawing.Size(670, 16);
            this.lblAutoReplaceInfo1.TabIndex = 62;
            this.lblAutoReplaceInfo1.Text = "After typing a keyword and pressing the space key, JasonQuery automatically repla" +
    "ces it with the defined replacement.";
            // 
            // grpModifyDefinitionAutoReplace
            // 
            this.grpModifyDefinitionAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.btnHelp_Symbol);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.editorAutoReplace);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.lblSymbolTips);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.txtKeyword);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.btnClearAutoReplace);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.btnCancelAutoReplace);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.btnSaveAutoReplace);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.lblReplacement);
            this.grpModifyDefinitionAutoReplace.Controls.Add(this.lblKeyword);
            this.grpModifyDefinitionAutoReplace.Location = new System.Drawing.Point(726, 77);
            this.grpModifyDefinitionAutoReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpModifyDefinitionAutoReplace.Name = "grpModifyDefinitionAutoReplace";
            this.grpModifyDefinitionAutoReplace.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpModifyDefinitionAutoReplace.Size = new System.Drawing.Size(447, 572);
            this.grpModifyDefinitionAutoReplace.TabIndex = 2;
            this.grpModifyDefinitionAutoReplace.TabStop = false;
            this.grpModifyDefinitionAutoReplace.Tag = "0";
            this.grpModifyDefinitionAutoReplace.Text = "Modify definition";
            // 
            // btnHelp_Symbol
            // 
            this.btnHelp_Symbol.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnHelp_Symbol.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Symbol.Image")));
            this.btnHelp_Symbol.Location = new System.Drawing.Point(267, 450);
            this.btnHelp_Symbol.Name = "btnHelp_Symbol";
            this.btnHelp_Symbol.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Symbol.TabIndex = 413;
            this.c1ThemeController1.SetTheme(this.btnHelp_Symbol, "(default)");
            this.btnHelp_Symbol.UseVisualStyleBackColor = true;
            this.btnHelp_Symbol.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Symbol.Click += new System.EventHandler(this.btnHelp_Symbol_Click);
            // 
            // editorAutoReplace
            // 
            this.editorAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorAutoReplace.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorAutoReplace.CaretLineVisible = true;
            this.editorAutoReplace.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorAutoReplace.Location = new System.Drawing.Point(16, 107);
            this.editorAutoReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorAutoReplace.Name = "editorAutoReplace";
            this.editorAutoReplace.ReadOnly = true;
            this.editorAutoReplace.ScrollWidth = 3618;
            this.editorAutoReplace.Size = new System.Drawing.Size(332, 339);
            this.editorAutoReplace.Styler = null;
            this.editorAutoReplace.TabIndex = 412;
            this.editorAutoReplace.WhitespaceSize = 3;
            this.editorAutoReplace.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorAutoReplace.WrapMode = ScintillaNET.WrapMode.Word;
            // 
            // lblSymbolTips
            // 
            this.lblSymbolTips.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblSymbolTips.AutoSize = true;
            this.lblSymbolTips.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSymbolTips.Location = new System.Drawing.Point(13, 452);
            this.lblSymbolTips.Name = "lblSymbolTips";
            this.lblSymbolTips.Size = new System.Drawing.Size(248, 16);
            this.lblSymbolTips.TabIndex = 79;
            this.lblSymbolTips.Text = "Using the symbol ^ to position your cursor.";
            this.c1ThemeController1.SetTheme(this.lblSymbolTips, "(default)");
            // 
            // txtKeyword
            // 
            this.txtKeyword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtKeyword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtKeyword.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtKeyword.Location = new System.Drawing.Point(16, 44);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.ReadOnly = true;
            this.txtKeyword.Size = new System.Drawing.Size(332, 23);
            this.txtKeyword.TabIndex = 407;
            this.txtKeyword.Tag = null;
            this.txtKeyword.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtKeyword, "(default)");
            this.txtKeyword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtKeyword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_KeyDown);
            // 
            // btnClearAutoReplace
            // 
            this.btnClearAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClearAutoReplace.Enabled = false;
            this.btnClearAutoReplace.Location = new System.Drawing.Point(365, 121);
            this.btnClearAutoReplace.Name = "btnClearAutoReplace";
            this.btnClearAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnClearAutoReplace.TabIndex = 411;
            this.btnClearAutoReplace.Text = "Clear";
            this.c1ThemeController1.SetTheme(this.btnClearAutoReplace, "(default)");
            this.btnClearAutoReplace.UseVisualStyleBackColor = true;
            this.btnClearAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClearAutoReplace.Click += new System.EventHandler(this.btnClear_Click);
            // 
            // btnCancelAutoReplace
            // 
            this.btnCancelAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancelAutoReplace.Enabled = false;
            this.btnCancelAutoReplace.Location = new System.Drawing.Point(365, 61);
            this.btnCancelAutoReplace.Name = "btnCancelAutoReplace";
            this.btnCancelAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnCancelAutoReplace.TabIndex = 410;
            this.btnCancelAutoReplace.Text = "Cancel";
            this.c1ThemeController1.SetTheme(this.btnCancelAutoReplace, "(default)");
            this.btnCancelAutoReplace.UseVisualStyleBackColor = true;
            this.btnCancelAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCancelAutoReplace.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnSaveAutoReplace
            // 
            this.btnSaveAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveAutoReplace.Enabled = false;
            this.btnSaveAutoReplace.Location = new System.Drawing.Point(365, 21);
            this.btnSaveAutoReplace.Name = "btnSaveAutoReplace";
            this.btnSaveAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnSaveAutoReplace.TabIndex = 409;
            this.btnSaveAutoReplace.Text = "Save";
            this.c1ThemeController1.SetTheme(this.btnSaveAutoReplace, "(default)");
            this.btnSaveAutoReplace.UseVisualStyleBackColor = true;
            this.btnSaveAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSaveAutoReplace.Click += new System.EventHandler(this.btnSaveAutoReplace_Click);
            // 
            // lblReplacement
            // 
            this.lblReplacement.AutoSize = true;
            this.lblReplacement.Location = new System.Drawing.Point(13, 88);
            this.lblReplacement.Name = "lblReplacement";
            this.lblReplacement.Size = new System.Drawing.Size(85, 16);
            this.lblReplacement.TabIndex = 13;
            this.lblReplacement.Text = "Replacement:";
            this.lblReplacement.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblKeyword
            // 
            this.lblKeyword.AutoSize = true;
            this.lblKeyword.Location = new System.Drawing.Point(13, 25);
            this.lblKeyword.Name = "lblKeyword";
            this.lblKeyword.Size = new System.Drawing.Size(59, 16);
            this.lblKeyword.TabIndex = 12;
            this.lblKeyword.Text = "Keyword:";
            this.lblKeyword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpDefinitionAutoReplace
            // 
            this.grpDefinitionAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpDefinitionAutoReplace.Controls.Add(this.btnDeleteAutoReplace);
            this.grpDefinitionAutoReplace.Controls.Add(this.btnEditAutoReplace);
            this.grpDefinitionAutoReplace.Controls.Add(this.btnAddAutoReplace);
            this.grpDefinitionAutoReplace.Controls.Add(this.c1GridAutoReplaceInfo);
            this.grpDefinitionAutoReplace.Location = new System.Drawing.Point(13, 77);
            this.grpDefinitionAutoReplace.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpDefinitionAutoReplace.Name = "grpDefinitionAutoReplace";
            this.grpDefinitionAutoReplace.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpDefinitionAutoReplace.Size = new System.Drawing.Size(702, 572);
            this.grpDefinitionAutoReplace.TabIndex = 1;
            this.grpDefinitionAutoReplace.TabStop = false;
            this.grpDefinitionAutoReplace.Tag = "";
            this.grpDefinitionAutoReplace.Text = "Definitions";
            // 
            // btnDeleteAutoReplace
            // 
            this.btnDeleteAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDeleteAutoReplace.Location = new System.Drawing.Point(620, 121);
            this.btnDeleteAutoReplace.Name = "btnDeleteAutoReplace";
            this.btnDeleteAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnDeleteAutoReplace.TabIndex = 406;
            this.btnDeleteAutoReplace.Text = "Delete";
            this.c1ThemeController1.SetTheme(this.btnDeleteAutoReplace, "(default)");
            this.btnDeleteAutoReplace.UseVisualStyleBackColor = true;
            this.btnDeleteAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnDeleteAutoReplace.Click += new System.EventHandler(this.btnDeleteAutoReplace_Click);
            // 
            // btnEditAutoReplace
            // 
            this.btnEditAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnEditAutoReplace.Location = new System.Drawing.Point(620, 61);
            this.btnEditAutoReplace.Name = "btnEditAutoReplace";
            this.btnEditAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnEditAutoReplace.TabIndex = 405;
            this.btnEditAutoReplace.Text = "Edit";
            this.c1ThemeController1.SetTheme(this.btnEditAutoReplace, "(default)");
            this.btnEditAutoReplace.UseVisualStyleBackColor = true;
            this.btnEditAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEditAutoReplace.Click += new System.EventHandler(this.btnEdit_Click);
            // 
            // btnAddAutoReplace
            // 
            this.btnAddAutoReplace.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAddAutoReplace.Location = new System.Drawing.Point(620, 21);
            this.btnAddAutoReplace.Name = "btnAddAutoReplace";
            this.btnAddAutoReplace.Size = new System.Drawing.Size(70, 31);
            this.btnAddAutoReplace.TabIndex = 404;
            this.btnAddAutoReplace.Text = "Add";
            this.c1ThemeController1.SetTheme(this.btnAddAutoReplace, "(default)");
            this.btnAddAutoReplace.UseVisualStyleBackColor = true;
            this.btnAddAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnAddAutoReplace.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // c1GridAutoReplaceInfo
            // 
            this.c1GridAutoReplaceInfo.AllowUpdate = false;
            this.c1GridAutoReplaceInfo.AllowUpdateOnBlur = false;
            this.c1GridAutoReplaceInfo.AlternatingRows = true;
            this.c1GridAutoReplaceInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridAutoReplaceInfo.CaptionHeight = 19;
            this.c1GridAutoReplaceInfo.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridAutoReplaceInfo.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridAutoReplaceInfo.Images"))));
            this.c1GridAutoReplaceInfo.Location = new System.Drawing.Point(10, 21);
            this.c1GridAutoReplaceInfo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridAutoReplaceInfo.Name = "c1GridAutoReplaceInfo";
            this.c1GridAutoReplaceInfo.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridAutoReplaceInfo.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridAutoReplaceInfo.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridAutoReplaceInfo.PreviewInfo.ZoomFactor = 75D;
            this.c1GridAutoReplaceInfo.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridAutoReplaceInfo.PrintInfo.MeasurementPrinterName = null;
            this.c1GridAutoReplaceInfo.RowHeight = 17;
            this.c1GridAutoReplaceInfo.Size = new System.Drawing.Size(599, 540);
            this.c1GridAutoReplaceInfo.TabIndex = 403;
            this.c1GridAutoReplaceInfo.UseCompatibleTextRendering = false;
            this.c1GridAutoReplaceInfo.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
            this.c1GridAutoReplaceInfo.RowColChange += new C1.Win.C1TrueDBGrid.RowColChangeEventHandler(this.c1GridAutoReplaceInfo_RowColChange);
            this.c1GridAutoReplaceInfo.PropBag = resources.GetString("c1GridAutoReplaceInfo.PropBag");
            // 
            // btnHelp_EnableAutoReplace
            // 
            this.btnHelp_EnableAutoReplace.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_EnableAutoReplace.Image")));
            this.btnHelp_EnableAutoReplace.Location = new System.Drawing.Point(446, 6);
            this.btnHelp_EnableAutoReplace.Name = "btnHelp_EnableAutoReplace";
            this.btnHelp_EnableAutoReplace.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_EnableAutoReplace.TabIndex = 315;
            this.c1ThemeController1.SetTheme(this.btnHelp_EnableAutoReplace, "(default)");
            this.btnHelp_EnableAutoReplace.UseVisualStyleBackColor = true;
            this.btnHelp_EnableAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_EnableAutoReplace.Click += new System.EventHandler(this.btnHelp_EnableAutoReplace_Click);
            // 
            // lblStarAutoReplace
            // 
            this.lblStarAutoReplace.AutoSize = true;
            this.lblStarAutoReplace.BackColor = System.Drawing.Color.Transparent;
            this.lblStarAutoReplace.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarAutoReplace.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarAutoReplace.Location = new System.Drawing.Point(422, 11);
            this.lblStarAutoReplace.Name = "lblStarAutoReplace";
            this.lblStarAutoReplace.Size = new System.Drawing.Size(14, 15);
            this.lblStarAutoReplace.TabIndex = 8;
            this.lblStarAutoReplace.Text = "*";
            // 
            // chkEnableAutoReplace
            // 
            this.chkEnableAutoReplace.AutoSize = true;
            this.chkEnableAutoReplace.BackColor = System.Drawing.Color.Transparent;
            this.chkEnableAutoReplace.BorderColor = System.Drawing.Color.Transparent;
            this.chkEnableAutoReplace.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEnableAutoReplace.ForeColor = System.Drawing.Color.Black;
            this.chkEnableAutoReplace.Location = new System.Drawing.Point(21, 8);
            this.chkEnableAutoReplace.Name = "chkEnableAutoReplace";
            this.chkEnableAutoReplace.Padding = new System.Windows.Forms.Padding(1);
            this.chkEnableAutoReplace.Size = new System.Drawing.Size(318, 22);
            this.chkEnableAutoReplace.TabIndex = 401;
            this.chkEnableAutoReplace.Text = "Enable Auto Replace (Quick expansion by keyword)";
            this.c1ThemeController1.SetTheme(this.chkEnableAutoReplace, "(default)");
            this.chkEnableAutoReplace.UseVisualStyleBackColor = true;
            this.chkEnableAutoReplace.Value = null;
            this.chkEnableAutoReplace.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkEnableAutoReplace.CheckedChanged += new System.EventHandler(this.chkEnableGroupBox_CheckedChanged);
            // 
            // btnHelp_ColumnComment
            // 
            this.btnHelp_ColumnComment.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ColumnComment.Image")));
            this.btnHelp_ColumnComment.Location = new System.Drawing.Point(171, 37);
            this.btnHelp_ColumnComment.Name = "btnHelp_ColumnComment";
            this.btnHelp_ColumnComment.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ColumnComment.TabIndex = 101;
            this.c1ThemeController1.SetTheme(this.btnHelp_ColumnComment, "(default)");
            this.btnHelp_ColumnComment.UseVisualStyleBackColor = true;
            this.btnHelp_ColumnComment.Visible = false;
            this.btnHelp_ColumnComment.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ColumnComment.Click += new System.EventHandler(this.btnHelp_ColumnComment_Click);
            // 
            // chkShowColumnComment
            // 
            this.chkShowColumnComment.AutoSize = true;
            this.chkShowColumnComment.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnComment.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnComment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnComment.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnComment.Location = new System.Drawing.Point(28, 37);
            this.chkShowColumnComment.Name = "chkShowColumnComment";
            this.chkShowColumnComment.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnComment.Size = new System.Drawing.Size(165, 22);
            this.chkShowColumnComment.TabIndex = 100;
            this.chkShowColumnComment.Text = "Show Column Comment";
            this.c1ThemeController1.SetTheme(this.chkShowColumnComment, "(default)");
            this.chkShowColumnComment.UseVisualStyleBackColor = true;
            this.chkShowColumnComment.Value = null;
            this.chkShowColumnComment.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkShowGroupingRow
            // 
            this.chkShowGroupingRow.AutoSize = true;
            this.chkShowGroupingRow.BackColor = System.Drawing.Color.Transparent;
            this.chkShowGroupingRow.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowGroupingRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowGroupingRow.ForeColor = System.Drawing.Color.Black;
            this.chkShowGroupingRow.Location = new System.Drawing.Point(28, 63);
            this.chkShowGroupingRow.Name = "chkShowGroupingRow";
            this.chkShowGroupingRow.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowGroupingRow.Size = new System.Drawing.Size(144, 22);
            this.chkShowGroupingRow.TabIndex = 94;
            this.chkShowGroupingRow.Text = "Show Grouping Row";
            this.c1ThemeController1.SetTheme(this.chkShowGroupingRow, "(default)");
            this.chkShowGroupingRow.UseVisualStyleBackColor = true;
            this.chkShowGroupingRow.Value = null;
            this.chkShowGroupingRow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkSetFocusAfterQuery
            // 
            this.chkSetFocusAfterQuery.AutoSize = true;
            this.chkSetFocusAfterQuery.BackColor = System.Drawing.Color.Transparent;
            this.chkSetFocusAfterQuery.BorderColor = System.Drawing.Color.Transparent;
            this.chkSetFocusAfterQuery.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkSetFocusAfterQuery.ForeColor = System.Drawing.Color.Black;
            this.chkSetFocusAfterQuery.Location = new System.Drawing.Point(28, 63);
            this.chkSetFocusAfterQuery.Name = "chkSetFocusAfterQuery";
            this.chkSetFocusAfterQuery.Padding = new System.Windows.Forms.Padding(1);
            this.chkSetFocusAfterQuery.Size = new System.Drawing.Size(327, 22);
            this.chkSetFocusAfterQuery.TabIndex = 91;
            this.chkSetFocusAfterQuery.Text = "Set Focus to Data Grid after Execute Query Statement";
            this.c1ThemeController1.SetTheme(this.chkSetFocusAfterQuery, "(default)");
            this.chkSetFocusAfterQuery.UseVisualStyleBackColor = true;
            this.chkSetFocusAfterQuery.Value = null;
            this.chkSetFocusAfterQuery.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkCtrlMouseWheel
            // 
            this.chkCtrlMouseWheel.AutoSize = true;
            this.chkCtrlMouseWheel.BackColor = System.Drawing.Color.Transparent;
            this.chkCtrlMouseWheel.BorderColor = System.Drawing.Color.Transparent;
            this.chkCtrlMouseWheel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCtrlMouseWheel.Checked = true;
            this.chkCtrlMouseWheel.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCtrlMouseWheel.Enabled = false;
            this.chkCtrlMouseWheel.ForeColor = System.Drawing.Color.Black;
            this.chkCtrlMouseWheel.Location = new System.Drawing.Point(28, 115);
            this.chkCtrlMouseWheel.Name = "chkCtrlMouseWheel";
            this.chkCtrlMouseWheel.Padding = new System.Windows.Forms.Padding(1);
            this.chkCtrlMouseWheel.Size = new System.Drawing.Size(303, 22);
            this.chkCtrlMouseWheel.TabIndex = 81;
            this.chkCtrlMouseWheel.Text = "Change Font Size (Zoom) with Ctrl+MouseWheel";
            this.c1ThemeController1.SetTheme(this.chkCtrlMouseWheel, "(default)");
            this.chkCtrlMouseWheel.UseVisualStyleBackColor = true;
            this.chkCtrlMouseWheel.Value = true;
            this.chkCtrlMouseWheel.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboGridRowHeightResizing
            // 
            this.cboGridRowHeightResizing.AllowSpinLoop = false;
            this.cboGridRowHeightResizing.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboGridRowHeightResizing.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboGridRowHeightResizing.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboGridRowHeightResizing.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboGridRowHeightResizing.GapHeight = 0;
            this.cboGridRowHeightResizing.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridRowHeightResizing.Items.Add("AllRows");
            this.cboGridRowHeightResizing.Items.Add("IndividualRows");
            this.cboGridRowHeightResizing.ItemsDisplayMember = "";
            this.cboGridRowHeightResizing.ItemsValueMember = "";
            this.cboGridRowHeightResizing.Location = new System.Drawing.Point(464, 103);
            this.cboGridRowHeightResizing.Name = "cboGridRowHeightResizing";
            this.cboGridRowHeightResizing.Size = new System.Drawing.Size(143, 21);
            this.cboGridRowHeightResizing.TabIndex = 85;
            this.cboGridRowHeightResizing.Tag = null;
            this.cboGridRowHeightResizing.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboGridRowHeightResizing, "(default)");
            this.cboGridRowHeightResizing.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridRowHeightResizing.SelectedIndexChanged += new System.EventHandler(this.cboGridRowHeightResizing_SelectedIndexChanged);
            // 
            // cboGridFontSize
            // 
            this.cboGridFontSize.AllowSpinLoop = false;
            this.cboGridFontSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboGridFontSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboGridFontSize.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboGridFontSize.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboGridFontSize.GapHeight = 0;
            this.cboGridFontSize.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridFontSize.Items.Add("9");
            this.cboGridFontSize.Items.Add("9.5");
            this.cboGridFontSize.Items.Add("10");
            this.cboGridFontSize.Items.Add("10.5");
            this.cboGridFontSize.Items.Add("11");
            this.cboGridFontSize.Items.Add("11.5");
            this.cboGridFontSize.Items.Add("12");
            this.cboGridFontSize.Items.Add("12.5");
            this.cboGridFontSize.Items.Add("13");
            this.cboGridFontSize.Items.Add("13.5");
            this.cboGridFontSize.Items.Add("14");
            this.cboGridFontSize.Items.Add("14.5");
            this.cboGridFontSize.Items.Add("15");
            this.cboGridFontSize.Items.Add("15.5");
            this.cboGridFontSize.Items.Add("16");
            this.cboGridFontSize.ItemsDisplayMember = "";
            this.cboGridFontSize.ItemsValueMember = "";
            this.cboGridFontSize.Location = new System.Drawing.Point(464, 73);
            this.cboGridFontSize.Name = "cboGridFontSize";
            this.cboGridFontSize.Size = new System.Drawing.Size(51, 21);
            this.cboGridFontSize.TabIndex = 84;
            this.cboGridFontSize.Tag = null;
            this.cboGridFontSize.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboGridFontSize, "(default)");
            this.cboGridFontSize.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridFontSize.SelectedIndexChanged += new System.EventHandler(this.cboGridFontSize_SelectedIndexChanged);
            // 
            // cboGridVisualStyle
            // 
            this.cboGridVisualStyle.AllowSpinLoop = false;
            this.cboGridVisualStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboGridVisualStyle.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboGridVisualStyle.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboGridVisualStyle.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboGridVisualStyle.GapHeight = 0;
            this.cboGridVisualStyle.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridVisualStyle.Items.Add("Office 2007 Blue");
            this.cboGridVisualStyle.Items.Add("Office 2007 Silver");
            this.cboGridVisualStyle.Items.Add("Office 2007 Black");
            this.cboGridVisualStyle.Items.Add("Office 2010 Blue");
            this.cboGridVisualStyle.Items.Add("Office 2010 Silver");
            this.cboGridVisualStyle.Items.Add("Office 2010 Black");
            this.cboGridVisualStyle.ItemsDisplayMember = "";
            this.cboGridVisualStyle.ItemsValueMember = "";
            this.cboGridVisualStyle.Location = new System.Drawing.Point(464, 13);
            this.cboGridVisualStyle.Name = "cboGridVisualStyle";
            this.cboGridVisualStyle.Size = new System.Drawing.Size(143, 21);
            this.cboGridVisualStyle.TabIndex = 82;
            this.cboGridVisualStyle.Tag = null;
            this.cboGridVisualStyle.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboGridVisualStyle, "(default)");
            this.cboGridVisualStyle.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridVisualStyle.SelectedIndexChanged += new System.EventHandler(this.cboGridVisualStyle_SelectedIndexChanged);
            // 
            // cboResultCopyQuotingWith
            // 
            this.cboResultCopyQuotingWith.AllowSpinLoop = false;
            this.cboResultCopyQuotingWith.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboResultCopyQuotingWith.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboResultCopyQuotingWith.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboResultCopyQuotingWith.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboResultCopyQuotingWith.GapHeight = 0;
            this.cboResultCopyQuotingWith.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboResultCopyQuotingWith.Items.Add("None");
            this.cboResultCopyQuotingWith.Items.Add("\"");
            this.cboResultCopyQuotingWith.Items.Add("\'");
            this.cboResultCopyQuotingWith.ItemsDisplayMember = "";
            this.cboResultCopyQuotingWith.ItemsValueMember = "";
            this.cboResultCopyQuotingWith.Location = new System.Drawing.Point(464, 133);
            this.cboResultCopyQuotingWith.Name = "cboResultCopyQuotingWith";
            this.cboResultCopyQuotingWith.Size = new System.Drawing.Size(60, 21);
            this.cboResultCopyQuotingWith.TabIndex = 79;
            this.cboResultCopyQuotingWith.Tag = null;
            this.cboResultCopyQuotingWith.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboResultCopyQuotingWith, "(default)");
            this.cboResultCopyQuotingWith.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboMaxWidth
            // 
            this.cboMaxWidth.AllowSpinLoop = false;
            this.cboMaxWidth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboMaxWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboMaxWidth.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboMaxWidth.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboMaxWidth.GapHeight = 0;
            this.cboMaxWidth.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboMaxWidth.Items.Add("500");
            this.cboMaxWidth.Items.Add("1000");
            this.cboMaxWidth.Items.Add("1500");
            this.cboMaxWidth.Items.Add("2000");
            this.cboMaxWidth.ItemsDisplayMember = "";
            this.cboMaxWidth.ItemsValueMember = "";
            this.cboMaxWidth.Location = new System.Drawing.Point(170, 168);
            this.cboMaxWidth.Name = "cboMaxWidth";
            this.cboMaxWidth.Size = new System.Drawing.Size(60, 21);
            this.cboMaxWidth.TabIndex = 78;
            this.cboMaxWidth.Tag = null;
            this.cboMaxWidth.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboMaxWidth, "(default)");
            this.cboMaxWidth.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboMaxWidth.SelectedIndexChanged += new System.EventHandler(this.cboMaxSize_SelectedIndexChanged);
            // 
            // chkResize
            // 
            this.chkResize.AutoSize = true;
            this.chkResize.BackColor = System.Drawing.Color.Transparent;
            this.chkResize.BorderColor = System.Drawing.Color.Transparent;
            this.chkResize.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkResize.ForeColor = System.Drawing.Color.Black;
            this.chkResize.Location = new System.Drawing.Point(28, 141);
            this.chkResize.Name = "chkResize";
            this.chkResize.Padding = new System.Windows.Forms.Padding(1);
            this.chkResize.Size = new System.Drawing.Size(150, 22);
            this.chkResize.TabIndex = 76;
            this.chkResize.Text = "Auto-fit column width";
            this.c1ThemeController1.SetTheme(this.chkResize, "(default)");
            this.chkResize.UseVisualStyleBackColor = true;
            this.chkResize.Value = null;
            this.chkResize.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkResize.CheckedChanged += new System.EventHandler(this.chkResize_CheckedChanged);
            // 
            // chkShowFilterRow
            // 
            this.chkShowFilterRow.AutoSize = true;
            this.chkShowFilterRow.BackColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowFilterRow.ForeColor = System.Drawing.Color.Black;
            this.chkShowFilterRow.Location = new System.Drawing.Point(28, 89);
            this.chkShowFilterRow.Name = "chkShowFilterRow";
            this.chkShowFilterRow.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowFilterRow.Size = new System.Drawing.Size(117, 22);
            this.chkShowFilterRow.TabIndex = 74;
            this.chkShowFilterRow.Text = "Show Filter Row";
            this.c1ThemeController1.SetTheme(this.chkShowFilterRow, "(default)");
            this.chkShowFilterRow.UseVisualStyleBackColor = true;
            this.chkShowFilterRow.Value = null;
            this.chkShowFilterRow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowFilterRow.CheckedChanged += new System.EventHandler(this.chkShowFilterRow_CheckedChanged);
            // 
            // chkShowColumnType
            // 
            this.chkShowColumnType.AutoSize = true;
            this.chkShowColumnType.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnType.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnType.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnType.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnType.Location = new System.Drawing.Point(28, 11);
            this.chkShowColumnType.Name = "chkShowColumnType";
            this.chkShowColumnType.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnType.Size = new System.Drawing.Size(137, 22);
            this.chkShowColumnType.TabIndex = 72;
            this.chkShowColumnType.Text = "Show Column Type";
            this.c1ThemeController1.SetTheme(this.chkShowColumnType, "(default)");
            this.chkShowColumnType.UseVisualStyleBackColor = true;
            this.chkShowColumnType.Value = null;
            this.chkShowColumnType.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowColumnType.CheckedChanged += new System.EventHandler(this.chkShowColumnDataType_CheckedChanged);
            // 
            // cboGridFontPicker
            // 
            this.cboGridFontPicker.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboGridFontPicker.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboGridFontPicker.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridFontPicker.Location = new System.Drawing.Point(464, 43);
            this.cboGridFontPicker.Name = "cboGridFontPicker";
            this.cboGridFontPicker.Size = new System.Drawing.Size(143, 21);
            this.cboGridFontPicker.TabIndex = 57;
            this.cboGridFontPicker.Tag = null;
            this.c1ThemeController1.SetTheme(this.cboGridFontPicker, "(default)");
            this.cboGridFontPicker.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridFontPicker.TextChanged += new System.EventHandler(this.cboGridFontPicker_TextChanged);
            // 
            // lblMaxWidth
            // 
            this.lblMaxWidth.AutoSize = true;
            this.lblMaxWidth.BackColor = System.Drawing.Color.Transparent;
            this.lblMaxWidth.Enabled = false;
            this.lblMaxWidth.Location = new System.Drawing.Point(45, 170);
            this.lblMaxWidth.Name = "lblMaxWidth";
            this.lblMaxWidth.Size = new System.Drawing.Size(119, 16);
            this.lblMaxWidth.TabIndex = 61;
            this.lblMaxWidth.Text = "Max Column Wdith:";
            this.lblMaxWidth.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridVisualStyle
            // 
            this.lblGridVisualStyle.BackColor = System.Drawing.Color.Transparent;
            this.lblGridVisualStyle.Location = new System.Drawing.Point(260, 14);
            this.lblGridVisualStyle.Name = "lblGridVisualStyle";
            this.lblGridVisualStyle.Size = new System.Drawing.Size(200, 16);
            this.lblGridVisualStyle.TabIndex = 6;
            this.lblGridVisualStyle.Text = "Visual Style:";
            this.lblGridVisualStyle.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridFontSize
            // 
            this.lblGridFontSize.BackColor = System.Drawing.Color.Transparent;
            this.lblGridFontSize.Location = new System.Drawing.Point(260, 74);
            this.lblGridFontSize.Name = "lblGridFontSize";
            this.lblGridFontSize.Size = new System.Drawing.Size(200, 16);
            this.lblGridFontSize.TabIndex = 49;
            this.lblGridFontSize.Text = "Font Size:";
            this.lblGridFontSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridFontName
            // 
            this.lblGridFontName.BackColor = System.Drawing.Color.Transparent;
            this.lblGridFontName.Location = new System.Drawing.Point(260, 44);
            this.lblGridFontName.Name = "lblGridFontName";
            this.lblGridFontName.Size = new System.Drawing.Size(200, 16);
            this.lblGridFontName.TabIndex = 47;
            this.lblGridFontName.Text = "Font Name:";
            this.lblGridFontName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblResultCopyQuotingWith
            // 
            this.lblResultCopyQuotingWith.BackColor = System.Drawing.Color.Transparent;
            this.lblResultCopyQuotingWith.Location = new System.Drawing.Point(260, 134);
            this.lblResultCopyQuotingWith.Name = "lblResultCopyQuotingWith";
            this.lblResultCopyQuotingWith.Size = new System.Drawing.Size(200, 16);
            this.lblResultCopyQuotingWith.TabIndex = 0;
            this.lblResultCopyQuotingWith.Text = "Result Copy Quoting with:";
            this.lblResultCopyQuotingWith.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpNullValueStyle
            // 
            this.grpNullValueStyle.BackColor = System.Drawing.Color.Transparent;
            this.grpNullValueStyle.Controls.Add(this.lblStarNullValueStyle);
            this.grpNullValueStyle.Controls.Add(this.cboNullShowAs);
            this.grpNullValueStyle.Controls.Add(this.pnlNullValueForeColor);
            this.grpNullValueStyle.Controls.Add(this.lblNullValueForeColor);
            this.grpNullValueStyle.Controls.Add(this.lblNullValueShowAs);
            this.grpNullValueStyle.Location = new System.Drawing.Point(28, 93);
            this.grpNullValueStyle.Name = "grpNullValueStyle";
            this.grpNullValueStyle.Size = new System.Drawing.Size(207, 84);
            this.grpNullValueStyle.TabIndex = 12;
            this.grpNullValueStyle.TabStop = false;
            this.grpNullValueStyle.Text = "NULL Value Style";
            // 
            // lblStarNullValueStyle
            // 
            this.lblStarNullValueStyle.AutoSize = true;
            this.lblStarNullValueStyle.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarNullValueStyle.Location = new System.Drawing.Point(161, 27);
            this.lblStarNullValueStyle.Name = "lblStarNullValueStyle";
            this.lblStarNullValueStyle.Size = new System.Drawing.Size(14, 15);
            this.lblStarNullValueStyle.TabIndex = 82;
            this.lblStarNullValueStyle.Text = "*";
            this.c1ThemeController1.SetTheme(this.lblStarNullValueStyle, "(default)");
            // 
            // cboNullShowAs
            // 
            this.cboNullShowAs.AllowSpinLoop = false;
            this.cboNullShowAs.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboNullShowAs.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboNullShowAs.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboNullShowAs.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboNullShowAs.GapHeight = 0;
            this.cboNullShowAs.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboNullShowAs.Items.Add("<NULL>");
            this.cboNullShowAs.Items.Add("<null>");
            this.cboNullShowAs.Items.Add("{NULL}");
            this.cboNullShowAs.Items.Add("{null}");
            this.cboNullShowAs.Items.Add("(NULL)");
            this.cboNullShowAs.Items.Add("(null)");
            this.cboNullShowAs.Items.Add("None");
            this.cboNullShowAs.ItemsDisplayMember = "";
            this.cboNullShowAs.ItemsValueMember = "";
            this.cboNullShowAs.Location = new System.Drawing.Point(79, 23);
            this.cboNullShowAs.Name = "cboNullShowAs";
            this.cboNullShowAs.Size = new System.Drawing.Size(79, 21);
            this.cboNullShowAs.TabIndex = 81;
            this.cboNullShowAs.Tag = null;
            this.cboNullShowAs.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboNullShowAs, "(default)");
            this.cboNullShowAs.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboNullShowAs.SelectedIndexChanged += new System.EventHandler(this.cboNullShowAs_SelectedIndexChanged);
            // 
            // pnlNullValueForeColor
            // 
            this.pnlNullValueForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlNullValueForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNullValueForeColor.Location = new System.Drawing.Point(109, 52);
            this.pnlNullValueForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlNullValueForeColor.Name = "pnlNullValueForeColor";
            this.pnlNullValueForeColor.Size = new System.Drawing.Size(79, 21);
            this.pnlNullValueForeColor.TabIndex = 20;
            this.pnlNullValueForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // lblNullValueForeColor
            // 
            this.lblNullValueForeColor.AutoSize = true;
            this.lblNullValueForeColor.Location = new System.Drawing.Point(21, 54);
            this.lblNullValueForeColor.Name = "lblNullValueForeColor";
            this.lblNullValueForeColor.Size = new System.Drawing.Size(69, 16);
            this.lblNullValueForeColor.TabIndex = 22;
            this.lblNullValueForeColor.Text = "Fore Color:";
            this.lblNullValueForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblNullValueShowAs
            // 
            this.lblNullValueShowAs.AutoSize = true;
            this.lblNullValueShowAs.Location = new System.Drawing.Point(21, 25);
            this.lblNullValueShowAs.Name = "lblNullValueShowAs";
            this.lblNullValueShowAs.Size = new System.Drawing.Size(56, 16);
            this.lblNullValueShowAs.TabIndex = 19;
            this.lblNullValueShowAs.Text = "Show as:";
            this.lblNullValueShowAs.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridRowHeightResizing
            // 
            this.lblGridRowHeightResizing.BackColor = System.Drawing.Color.Transparent;
            this.lblGridRowHeightResizing.Location = new System.Drawing.Point(260, 104);
            this.lblGridRowHeightResizing.Name = "lblGridRowHeightResizing";
            this.lblGridRowHeightResizing.Size = new System.Drawing.Size(200, 16);
            this.lblGridRowHeightResizing.TabIndex = 64;
            this.lblGridRowHeightResizing.Text = "Row (Height) Resizing:";
            this.lblGridRowHeightResizing.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpDataGridColor
            // 
            this.grpDataGridColor.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpDataGridColor.BackColor = System.Drawing.Color.Transparent;
            this.grpDataGridColor.Controls.Add(this.lblGridHeadingForeColor);
            this.grpDataGridColor.Controls.Add(this.lblStarGridColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridHeadingForeColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridSelectedBackColor);
            this.grpDataGridColor.Controls.Add(this.lblGridSelectedBackColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridSelectedForeColor);
            this.grpDataGridColor.Controls.Add(this.lblGridEvenRowForeColor);
            this.grpDataGridColor.Controls.Add(this.lblGridSelectedForeColor);
            this.grpDataGridColor.Controls.Add(this.lblGridHighlightForeColor);
            this.grpDataGridColor.Controls.Add(this.lblGridOddRowBackColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridEvenRowBackColor);
            this.grpDataGridColor.Controls.Add(this.lblGridOddRowForeColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridHighlightForeColor);
            this.grpDataGridColor.Controls.Add(this.lblGridEvenRowBackColor);
            this.grpDataGridColor.Controls.Add(this.lblGridHighlightBackColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridHighlightBackColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridOddRowForeColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridOddRowBackColor);
            this.grpDataGridColor.Controls.Add(this.pnlGridEvenRowForeColor);
            this.grpDataGridColor.Location = new System.Drawing.Point(12, 292);
            this.grpDataGridColor.Name = "grpDataGridColor";
            this.grpDataGridColor.Size = new System.Drawing.Size(267, 383);
            this.grpDataGridColor.TabIndex = 12;
            this.grpDataGridColor.TabStop = false;
            this.grpDataGridColor.Text = "Data Grid Color";
            // 
            // lblGridHeadingForeColor
            // 
            this.lblGridHeadingForeColor.Location = new System.Drawing.Point(18, 24);
            this.lblGridHeadingForeColor.Name = "lblGridHeadingForeColor";
            this.lblGridHeadingForeColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridHeadingForeColor.TabIndex = 27;
            this.lblGridHeadingForeColor.Text = "Heading Fore Color:";
            this.lblGridHeadingForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblGridHeadingForeColor, "(default)");
            // 
            // lblStarGridColor
            // 
            this.lblStarGridColor.AutoSize = true;
            this.lblStarGridColor.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarGridColor.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarGridColor.Location = new System.Drawing.Point(100, 3);
            this.lblStarGridColor.Name = "lblStarGridColor";
            this.lblStarGridColor.Size = new System.Drawing.Size(14, 15);
            this.lblStarGridColor.TabIndex = 52;
            this.lblStarGridColor.Text = "*";
            this.lblStarGridColor.Visible = false;
            // 
            // pnlGridHeadingForeColor
            // 
            this.pnlGridHeadingForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridHeadingForeColor.Location = new System.Drawing.Point(170, 22);
            this.pnlGridHeadingForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridHeadingForeColor.Name = "pnlGridHeadingForeColor";
            this.pnlGridHeadingForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridHeadingForeColor.TabIndex = 26;
            this.c1ThemeController1.SetTheme(this.pnlGridHeadingForeColor, "(default)");
            this.pnlGridHeadingForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // pnlGridSelectedBackColor
            // 
            this.pnlGridSelectedBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridSelectedBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridSelectedBackColor.Location = new System.Drawing.Point(170, 262);
            this.pnlGridSelectedBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridSelectedBackColor.Name = "pnlGridSelectedBackColor";
            this.pnlGridSelectedBackColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridSelectedBackColor.TabIndex = 31;
            this.pnlGridSelectedBackColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // lblGridSelectedBackColor
            // 
            this.lblGridSelectedBackColor.Location = new System.Drawing.Point(18, 264);
            this.lblGridSelectedBackColor.Name = "lblGridSelectedBackColor";
            this.lblGridSelectedBackColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridSelectedBackColor.TabIndex = 32;
            this.lblGridSelectedBackColor.Text = "Selected Back Color:";
            this.lblGridSelectedBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlGridSelectedForeColor
            // 
            this.pnlGridSelectedForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridSelectedForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridSelectedForeColor.Location = new System.Drawing.Point(170, 232);
            this.pnlGridSelectedForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridSelectedForeColor.Name = "pnlGridSelectedForeColor";
            this.pnlGridSelectedForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridSelectedForeColor.TabIndex = 30;
            this.pnlGridSelectedForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // lblGridEvenRowForeColor
            // 
            this.lblGridEvenRowForeColor.Location = new System.Drawing.Point(18, 54);
            this.lblGridEvenRowForeColor.Name = "lblGridEvenRowForeColor";
            this.lblGridEvenRowForeColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridEvenRowForeColor.TabIndex = 25;
            this.lblGridEvenRowForeColor.Text = "Even Row Fore Color:";
            this.lblGridEvenRowForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridSelectedForeColor
            // 
            this.lblGridSelectedForeColor.Location = new System.Drawing.Point(18, 234);
            this.lblGridSelectedForeColor.Name = "lblGridSelectedForeColor";
            this.lblGridSelectedForeColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridSelectedForeColor.TabIndex = 29;
            this.lblGridSelectedForeColor.Text = "Selected Fore Color:";
            this.lblGridSelectedForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridHighlightForeColor
            // 
            this.lblGridHighlightForeColor.Location = new System.Drawing.Point(18, 174);
            this.lblGridHighlightForeColor.Name = "lblGridHighlightForeColor";
            this.lblGridHighlightForeColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridHighlightForeColor.TabIndex = 2;
            this.lblGridHighlightForeColor.Text = "Highlight Fore Color:";
            this.lblGridHighlightForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridOddRowBackColor
            // 
            this.lblGridOddRowBackColor.Location = new System.Drawing.Point(18, 144);
            this.lblGridOddRowBackColor.Name = "lblGridOddRowBackColor";
            this.lblGridOddRowBackColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridOddRowBackColor.TabIndex = 28;
            this.lblGridOddRowBackColor.Text = "Odd Row Back Color:";
            this.lblGridOddRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlGridEvenRowBackColor
            // 
            this.pnlGridEvenRowBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridEvenRowBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridEvenRowBackColor.Location = new System.Drawing.Point(170, 82);
            this.pnlGridEvenRowBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridEvenRowBackColor.Name = "pnlGridEvenRowBackColor";
            this.pnlGridEvenRowBackColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridEvenRowBackColor.TabIndex = 19;
            this.pnlGridEvenRowBackColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // lblGridOddRowForeColor
            // 
            this.lblGridOddRowForeColor.Location = new System.Drawing.Point(18, 114);
            this.lblGridOddRowForeColor.Name = "lblGridOddRowForeColor";
            this.lblGridOddRowForeColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridOddRowForeColor.TabIndex = 27;
            this.lblGridOddRowForeColor.Text = "Odd Row Fore Color:";
            this.lblGridOddRowForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlGridHighlightForeColor
            // 
            this.pnlGridHighlightForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridHighlightForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridHighlightForeColor.Location = new System.Drawing.Point(170, 172);
            this.pnlGridHighlightForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridHighlightForeColor.Name = "pnlGridHighlightForeColor";
            this.pnlGridHighlightForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridHighlightForeColor.TabIndex = 3;
            this.pnlGridHighlightForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // lblGridEvenRowBackColor
            // 
            this.lblGridEvenRowBackColor.Location = new System.Drawing.Point(18, 84);
            this.lblGridEvenRowBackColor.Name = "lblGridEvenRowBackColor";
            this.lblGridEvenRowBackColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridEvenRowBackColor.TabIndex = 26;
            this.lblGridEvenRowBackColor.Text = "Even Row Back Color:";
            this.lblGridEvenRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblGridHighlightBackColor
            // 
            this.lblGridHighlightBackColor.Location = new System.Drawing.Point(18, 204);
            this.lblGridHighlightBackColor.Name = "lblGridHighlightBackColor";
            this.lblGridHighlightBackColor.Size = new System.Drawing.Size(150, 16);
            this.lblGridHighlightBackColor.TabIndex = 4;
            this.lblGridHighlightBackColor.Text = "Highlight Back Color:";
            this.lblGridHighlightBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlGridHighlightBackColor
            // 
            this.pnlGridHighlightBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridHighlightBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridHighlightBackColor.Location = new System.Drawing.Point(170, 202);
            this.pnlGridHighlightBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridHighlightBackColor.Name = "pnlGridHighlightBackColor";
            this.pnlGridHighlightBackColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridHighlightBackColor.TabIndex = 4;
            this.pnlGridHighlightBackColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // pnlGridOddRowForeColor
            // 
            this.pnlGridOddRowForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridOddRowForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridOddRowForeColor.Location = new System.Drawing.Point(170, 112);
            this.pnlGridOddRowForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridOddRowForeColor.Name = "pnlGridOddRowForeColor";
            this.pnlGridOddRowForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridOddRowForeColor.TabIndex = 24;
            this.pnlGridOddRowForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // pnlGridOddRowBackColor
            // 
            this.pnlGridOddRowBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridOddRowBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridOddRowBackColor.Location = new System.Drawing.Point(170, 142);
            this.pnlGridOddRowBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridOddRowBackColor.Name = "pnlGridOddRowBackColor";
            this.pnlGridOddRowBackColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridOddRowBackColor.TabIndex = 22;
            this.pnlGridOddRowBackColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // pnlGridEvenRowForeColor
            // 
            this.pnlGridEvenRowForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlGridEvenRowForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlGridEvenRowForeColor.Location = new System.Drawing.Point(170, 52);
            this.pnlGridEvenRowForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlGridEvenRowForeColor.Name = "pnlGridEvenRowForeColor";
            this.pnlGridEvenRowForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlGridEvenRowForeColor.TabIndex = 23;
            this.pnlGridEvenRowForeColor.Click += new System.EventHandler(this.pnlSelectedGridClick);
            // 
            // grpPreviewGrid
            // 
            this.grpPreviewGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreviewGrid.BackColor = System.Drawing.Color.Transparent;
            this.grpPreviewGrid.Controls.Add(this.cboFindGrid);
            this.grpPreviewGrid.Controls.Add(this.tsGrid);
            this.grpPreviewGrid.Controls.Add(this.c1GridVisualStyle);
            this.grpPreviewGrid.Location = new System.Drawing.Point(288, 292);
            this.grpPreviewGrid.Name = "grpPreviewGrid";
            this.grpPreviewGrid.Size = new System.Drawing.Size(910, 383);
            this.grpPreviewGrid.TabIndex = 2;
            this.grpPreviewGrid.TabStop = false;
            this.grpPreviewGrid.Text = "Preview";
            // 
            // cboFindGrid
            // 
            this.cboFindGrid.AllowSpinLoop = false;
            this.cboFindGrid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboFindGrid.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboFindGrid.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboFindGrid.GapHeight = 0;
            this.cboFindGrid.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboFindGrid.ItemsDisplayMember = "";
            this.cboFindGrid.ItemsValueMember = "";
            this.cboFindGrid.Location = new System.Drawing.Point(38, 24);
            this.cboFindGrid.Name = "cboFindGrid";
            this.cboFindGrid.Size = new System.Drawing.Size(111, 21);
            this.cboFindGrid.TabIndex = 79;
            this.cboFindGrid.Tag = null;
            this.cboFindGrid.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboFindGrid, "(default)");
            this.cboFindGrid.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboFindGrid.SelectedIndexChanged += new System.EventHandler(this.cboFindGrid_SelectedIndexChanged);
            this.cboFindGrid.TextChanged += new System.EventHandler(this.cboFindGrid_TextChanged);
            this.cboFindGrid.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboFindGrid_KeyPress);
            this.cboFindGrid.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboFindGrid_KeyUp);
            // 
            // tsGrid
            // 
            this.tsGrid.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsGrid.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.tsGrid.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblFindGrid,
            this.cboFindGrid3,
            this.btnFindNextGrid,
            this.btnFindPreviousGrid,
            this.btnCountGrid,
            this.btnHighlightAllGrid,
            this.btnClearHighlightsGrid});
            this.tsGrid.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.tsGrid.Location = new System.Drawing.Point(3, 19);
            this.tsGrid.Name = "tsGrid";
            this.tsGrid.Size = new System.Drawing.Size(904, 31);
            this.tsGrid.Stretch = true;
            this.tsGrid.TabIndex = 5;
            this.c1ThemeController1.SetTheme(this.tsGrid, "(default)");
            // 
            // lblFindGrid
            // 
            this.lblFindGrid.Name = "lblFindGrid";
            this.lblFindGrid.Size = new System.Drawing.Size(34, 28);
            this.lblFindGrid.Tag = "";
            this.lblFindGrid.Text = "Find:";
            // 
            // cboFindGrid3
            // 
            this.cboFindGrid3.AutoSize = false;
            this.cboFindGrid3.FlatStyle = System.Windows.Forms.FlatStyle.Standard;
            this.cboFindGrid3.Font = new System.Drawing.Font("微軟正黑體", 6F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboFindGrid3.Name = "cboFindGrid3";
            this.cboFindGrid3.Size = new System.Drawing.Size(110, 18);
            this.cboFindGrid3.DropDown += new System.EventHandler(this.cboFindGrid_DropDown);
            this.cboFindGrid3.SelectedIndexChanged += new System.EventHandler(this.cboFindGrid_SelectedIndexChanged);
            this.cboFindGrid3.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboFindGrid_KeyPress);
            this.cboFindGrid3.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboFindGrid_KeyUp);
            this.cboFindGrid3.TextChanged += new System.EventHandler(this.cboFindGrid_TextChanged);
            // 
            // btnFindNextGrid
            // 
            this.btnFindNextGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFindNextGrid.Enabled = false;
            this.btnFindNextGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnFindNextGrid.Image")));
            this.btnFindNextGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindNextGrid.Name = "btnFindNextGrid";
            this.btnFindNextGrid.Size = new System.Drawing.Size(28, 28);
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
            this.btnFindPreviousGrid.Size = new System.Drawing.Size(28, 28);
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
            this.btnCountGrid.Size = new System.Drawing.Size(28, 28);
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
            this.btnHighlightAllGrid.Size = new System.Drawing.Size(28, 28);
            this.btnHighlightAllGrid.ToolTipText = "Highlight All";
            this.btnHighlightAllGrid.Click += new System.EventHandler(this.btnHighlightAllGrid_Click);
            // 
            // btnClearHighlightsGrid
            // 
            this.btnClearHighlightsGrid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClearHighlightsGrid.Enabled = false;
            this.btnClearHighlightsGrid.Image = ((System.Drawing.Image)(resources.GetObject("btnClearHighlightsGrid.Image")));
            this.btnClearHighlightsGrid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClearHighlightsGrid.Name = "btnClearHighlightsGrid";
            this.btnClearHighlightsGrid.Size = new System.Drawing.Size(28, 28);
            this.btnClearHighlightsGrid.ToolTipText = "Clear Highlights";
            this.btnClearHighlightsGrid.Click += new System.EventHandler(this.btnClearHighlightsGrid_Click);
            // 
            // c1GridVisualStyle
            // 
            this.c1GridVisualStyle.AllowUpdate = false;
            this.c1GridVisualStyle.AllowUpdateOnBlur = false;
            this.c1GridVisualStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridVisualStyle.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.c1GridVisualStyle.CaptionHeight = 19;
            this.c1GridVisualStyle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridVisualStyle.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridVisualStyle.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridVisualStyle.Images"))));
            this.c1GridVisualStyle.Location = new System.Drawing.Point(3, 50);
            this.c1GridVisualStyle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridVisualStyle.Name = "c1GridVisualStyle";
            this.c1GridVisualStyle.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridVisualStyle.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridVisualStyle.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridVisualStyle.PreviewInfo.ZoomFactor = 75D;
            this.c1GridVisualStyle.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridVisualStyle.PrintInfo.MeasurementPrinterName = null;
            this.c1GridVisualStyle.RowHeight = 17;
            this.c1GridVisualStyle.Size = new System.Drawing.Size(904, 329);
            this.c1GridVisualStyle.TabIndex = 65;
            this.c1GridVisualStyle.UseCompatibleTextRendering = false;
            this.c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
            this.c1GridVisualStyle.OwnerDrawCell += new C1.Win.C1TrueDBGrid.OwnerDrawCellEventHandler(this.c1GridVisualStyle_OwnerDrawCell);
            this.c1GridVisualStyle.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridVisualStyle_FetchCellStyle);
            this.c1GridVisualStyle.Enter += new System.EventHandler(this.c1GridVisualStyle_Enter);
            this.c1GridVisualStyle.Leave += new System.EventHandler(this.c1GridVisualStyle_Leave);
            this.c1GridVisualStyle.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridVisualStyle_MouseDoubleClick);
            this.c1GridVisualStyle.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1GridVisualStyle_MouseDown);
            this.c1GridVisualStyle.PropBag = resources.GetString("c1GridVisualStyle.PropBag");
            // 
            // grpOperatorKeywords
            // 
            this.grpOperatorKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOperatorKeywords.BackColor = System.Drawing.Color.Transparent;
            this.grpOperatorKeywords.Controls.Add(this.grpFindOperatorKeywords);
            this.grpOperatorKeywords.Controls.Add(this.picOperatorKeywords);
            this.grpOperatorKeywords.Controls.Add(this.editorOperatorKeywords);
            this.grpOperatorKeywords.Location = new System.Drawing.Point(12, 3);
            this.grpOperatorKeywords.Name = "grpOperatorKeywords";
            this.grpOperatorKeywords.Size = new System.Drawing.Size(1186, 158);
            this.grpOperatorKeywords.TabIndex = 5;
            this.grpOperatorKeywords.TabStop = false;
            this.grpOperatorKeywords.Text = "Operator Keywords";
            // 
            // grpFindOperatorKeywords
            // 
            this.grpFindOperatorKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFindOperatorKeywords.BackColor = System.Drawing.Color.Transparent;
            this.grpFindOperatorKeywords.Controls.Add(this.toolStrip2);
            this.grpFindOperatorKeywords.Location = new System.Drawing.Point(972, 0);
            this.grpFindOperatorKeywords.Name = "grpFindOperatorKeywords";
            this.grpFindOperatorKeywords.Size = new System.Drawing.Size(212, 55);
            this.grpFindOperatorKeywords.TabIndex = 111;
            this.grpFindOperatorKeywords.TabStop = false;
            this.grpFindOperatorKeywords.Tag = "0";
            this.grpFindOperatorKeywords.Visible = false;
            // 
            // toolStrip2
            // 
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtFindOperatorKeywords,
            this.btnNextOperatorKeywords,
            this.btnPreviousOperatorKeywords,
            this.btnCloseFindOperatorKeywords});
            this.toolStrip2.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip2.Location = new System.Drawing.Point(3, 19);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(206, 31);
            this.toolStrip2.Stretch = true;
            this.toolStrip2.TabIndex = 6;
            this.c1ThemeController1.SetTheme(this.toolStrip2, "(default)");
            // 
            // txtFindOperatorKeywords
            // 
            this.txtFindOperatorKeywords.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtFindOperatorKeywords.Name = "txtFindOperatorKeywords";
            this.txtFindOperatorKeywords.Size = new System.Drawing.Size(100, 31);
            this.txtFindOperatorKeywords.Tag = "0";
            this.txtFindOperatorKeywords.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindNextKeywords_KeyPress);
            this.txtFindOperatorKeywords.TextChanged += new System.EventHandler(this.FindKeywords_TextChanged);
            // 
            // btnNextOperatorKeywords
            // 
            this.btnNextOperatorKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextOperatorKeywords.Enabled = false;
            this.btnNextOperatorKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnNextOperatorKeywords.Image")));
            this.btnNextOperatorKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextOperatorKeywords.Name = "btnNextOperatorKeywords";
            this.btnNextOperatorKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnNextOperatorKeywords.Tag = "0";
            this.btnNextOperatorKeywords.ToolTipText = "Find Next";
            this.btnNextOperatorKeywords.Click += new System.EventHandler(this.FindNextKeywords_Click);
            // 
            // btnPreviousOperatorKeywords
            // 
            this.btnPreviousOperatorKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousOperatorKeywords.Enabled = false;
            this.btnPreviousOperatorKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnPreviousOperatorKeywords.Image")));
            this.btnPreviousOperatorKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousOperatorKeywords.Name = "btnPreviousOperatorKeywords";
            this.btnPreviousOperatorKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnPreviousOperatorKeywords.Tag = "0";
            this.btnPreviousOperatorKeywords.ToolTipText = "Find Previous";
            this.btnPreviousOperatorKeywords.Click += new System.EventHandler(this.FindPreviousKeywords_Click);
            // 
            // btnCloseFindOperatorKeywords
            // 
            this.btnCloseFindOperatorKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCloseFindOperatorKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseFindOperatorKeywords.Image")));
            this.btnCloseFindOperatorKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCloseFindOperatorKeywords.Name = "btnCloseFindOperatorKeywords";
            this.btnCloseFindOperatorKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnCloseFindOperatorKeywords.Tag = "0";
            this.btnCloseFindOperatorKeywords.ToolTipText = "Close";
            this.btnCloseFindOperatorKeywords.Click += new System.EventHandler(this.HideOperatorKeywords);
            // 
            // picOperatorKeywords
            // 
            this.picOperatorKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picOperatorKeywords.Image = ((System.Drawing.Image)(resources.GetObject("picOperatorKeywords.Image")));
            this.picOperatorKeywords.Location = new System.Drawing.Point(1169, 7);
            this.picOperatorKeywords.Name = "picOperatorKeywords";
            this.picOperatorKeywords.Size = new System.Drawing.Size(16, 16);
            this.picOperatorKeywords.TabIndex = 14;
            this.picOperatorKeywords.TabStop = false;
            this.picOperatorKeywords.Tag = "0";
            this.picOperatorKeywords.Click += new System.EventHandler(this.ShowOperatorKeywords);
            // 
            // editorOperatorKeywords
            // 
            this.editorOperatorKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorOperatorKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorOperatorKeywords.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editorOperatorKeywords.CaretLineVisible = true;
            this.editorOperatorKeywords.EndAtLastLine = false;
            this.editorOperatorKeywords.HScrollBar = false;
            this.editorOperatorKeywords.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorOperatorKeywords.Location = new System.Drawing.Point(12, 23);
            this.editorOperatorKeywords.Name = "editorOperatorKeywords";
            this.editorOperatorKeywords.ScrollWidth = 400;
            this.editorOperatorKeywords.SelectionEolFilled = true;
            this.editorOperatorKeywords.Size = new System.Drawing.Size(1160, 122);
            this.editorOperatorKeywords.Styler = null;
            this.editorOperatorKeywords.TabIndex = 112;
            this.editorOperatorKeywords.Tag = "0";
            this.editorOperatorKeywords.ViewEol = true;
            this.editorOperatorKeywords.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            this.editorOperatorKeywords.WhitespaceSize = 3;
            this.editorOperatorKeywords.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorOperatorKeywords.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorOperatorKeywords.Leave += new System.EventHandler(this.Keywords_LeaveCheck);
            // 
            // grpBuiltInFunctions
            // 
            this.grpBuiltInFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBuiltInFunctions.Controls.Add(this.grpFindBuiltInFunctions);
            this.grpBuiltInFunctions.Controls.Add(this.picBuiltInFunctions);
            this.grpBuiltInFunctions.Controls.Add(this.lbl1);
            this.grpBuiltInFunctions.Controls.Add(this.editorBuiltInFunctions);
            this.grpBuiltInFunctions.Location = new System.Drawing.Point(12, 3);
            this.grpBuiltInFunctions.Name = "grpBuiltInFunctions";
            this.grpBuiltInFunctions.Size = new System.Drawing.Size(1186, 163);
            this.grpBuiltInFunctions.TabIndex = 6;
            this.grpBuiltInFunctions.TabStop = false;
            this.grpBuiltInFunctions.Text = "Built-in Functions";
            // 
            // grpFindBuiltInFunctions
            // 
            this.grpFindBuiltInFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFindBuiltInFunctions.BackColor = System.Drawing.Color.Transparent;
            this.grpFindBuiltInFunctions.Controls.Add(this.toolStrip3);
            this.grpFindBuiltInFunctions.Location = new System.Drawing.Point(972, 0);
            this.grpFindBuiltInFunctions.Name = "grpFindBuiltInFunctions";
            this.grpFindBuiltInFunctions.Size = new System.Drawing.Size(212, 55);
            this.grpFindBuiltInFunctions.TabIndex = 113;
            this.grpFindBuiltInFunctions.TabStop = false;
            this.grpFindBuiltInFunctions.Tag = "1";
            this.grpFindBuiltInFunctions.Visible = false;
            // 
            // toolStrip3
            // 
            this.toolStrip3.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip3.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip3.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtFindBuiltInFunctions,
            this.btnNextBuiltInFunctions,
            this.btnPreviousBuiltInFunctions,
            this.btnCloseFindBuiltInFunctions});
            this.toolStrip3.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip3.Location = new System.Drawing.Point(3, 19);
            this.toolStrip3.Name = "toolStrip3";
            this.toolStrip3.Size = new System.Drawing.Size(206, 31);
            this.toolStrip3.Stretch = true;
            this.toolStrip3.TabIndex = 6;
            this.c1ThemeController1.SetTheme(this.toolStrip3, "(default)");
            // 
            // txtFindBuiltInFunctions
            // 
            this.txtFindBuiltInFunctions.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtFindBuiltInFunctions.Name = "txtFindBuiltInFunctions";
            this.txtFindBuiltInFunctions.Size = new System.Drawing.Size(100, 31);
            this.txtFindBuiltInFunctions.Tag = "1";
            this.txtFindBuiltInFunctions.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindNextKeywords_KeyPress);
            this.txtFindBuiltInFunctions.TextChanged += new System.EventHandler(this.FindKeywords_TextChanged);
            // 
            // btnNextBuiltInFunctions
            // 
            this.btnNextBuiltInFunctions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextBuiltInFunctions.Enabled = false;
            this.btnNextBuiltInFunctions.Image = ((System.Drawing.Image)(resources.GetObject("btnNextBuiltInFunctions.Image")));
            this.btnNextBuiltInFunctions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextBuiltInFunctions.Name = "btnNextBuiltInFunctions";
            this.btnNextBuiltInFunctions.Size = new System.Drawing.Size(28, 28);
            this.btnNextBuiltInFunctions.Tag = "1";
            this.btnNextBuiltInFunctions.ToolTipText = "Find Next";
            this.btnNextBuiltInFunctions.Click += new System.EventHandler(this.FindNextKeywords_Click);
            // 
            // btnPreviousBuiltInFunctions
            // 
            this.btnPreviousBuiltInFunctions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousBuiltInFunctions.Enabled = false;
            this.btnPreviousBuiltInFunctions.Image = ((System.Drawing.Image)(resources.GetObject("btnPreviousBuiltInFunctions.Image")));
            this.btnPreviousBuiltInFunctions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousBuiltInFunctions.Name = "btnPreviousBuiltInFunctions";
            this.btnPreviousBuiltInFunctions.Size = new System.Drawing.Size(28, 28);
            this.btnPreviousBuiltInFunctions.Tag = "1";
            this.btnPreviousBuiltInFunctions.ToolTipText = "Find Previous";
            this.btnPreviousBuiltInFunctions.Click += new System.EventHandler(this.FindPreviousKeywords_Click);
            // 
            // btnCloseFindBuiltInFunctions
            // 
            this.btnCloseFindBuiltInFunctions.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCloseFindBuiltInFunctions.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseFindBuiltInFunctions.Image")));
            this.btnCloseFindBuiltInFunctions.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCloseFindBuiltInFunctions.Name = "btnCloseFindBuiltInFunctions";
            this.btnCloseFindBuiltInFunctions.Size = new System.Drawing.Size(28, 28);
            this.btnCloseFindBuiltInFunctions.Tag = "1";
            this.btnCloseFindBuiltInFunctions.ToolTipText = "Close";
            this.btnCloseFindBuiltInFunctions.Click += new System.EventHandler(this.HideOperatorKeywords);
            // 
            // picBuiltInFunctions
            // 
            this.picBuiltInFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picBuiltInFunctions.Image = ((System.Drawing.Image)(resources.GetObject("picBuiltInFunctions.Image")));
            this.picBuiltInFunctions.Location = new System.Drawing.Point(1169, 7);
            this.picBuiltInFunctions.Name = "picBuiltInFunctions";
            this.picBuiltInFunctions.Size = new System.Drawing.Size(16, 16);
            this.picBuiltInFunctions.TabIndex = 15;
            this.picBuiltInFunctions.TabStop = false;
            this.picBuiltInFunctions.Tag = "1";
            this.picBuiltInFunctions.Click += new System.EventHandler(this.ShowOperatorKeywords);
            // 
            // lbl1
            // 
            this.lbl1.AutoSize = true;
            this.lbl1.Location = new System.Drawing.Point(6, 17);
            this.lbl1.Name = "lbl1";
            this.lbl1.Size = new System.Drawing.Size(0, 16);
            this.lbl1.TabIndex = 13;
            this.lbl1.Visible = false;
            // 
            // editorBuiltInFunctions
            // 
            this.editorBuiltInFunctions.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorBuiltInFunctions.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorBuiltInFunctions.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editorBuiltInFunctions.CaretLineVisible = true;
            this.editorBuiltInFunctions.EndAtLastLine = false;
            this.editorBuiltInFunctions.HScrollBar = false;
            this.editorBuiltInFunctions.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorBuiltInFunctions.Location = new System.Drawing.Point(12, 23);
            this.editorBuiltInFunctions.Name = "editorBuiltInFunctions";
            this.editorBuiltInFunctions.ScrollWidth = 400;
            this.editorBuiltInFunctions.SelectionEolFilled = true;
            this.editorBuiltInFunctions.Size = new System.Drawing.Size(1160, 127);
            this.editorBuiltInFunctions.Styler = null;
            this.editorBuiltInFunctions.TabIndex = 114;
            this.editorBuiltInFunctions.Tag = "1";
            this.editorBuiltInFunctions.ViewEol = true;
            this.editorBuiltInFunctions.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            this.editorBuiltInFunctions.WhitespaceSize = 3;
            this.editorBuiltInFunctions.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorBuiltInFunctions.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorBuiltInFunctions.Leave += new System.EventHandler(this.Keywords_LeaveCheck);
            // 
            // grpBuiltInKeywords
            // 
            this.grpBuiltInKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpBuiltInKeywords.Controls.Add(this.grpFindBuiltInKeywords);
            this.grpBuiltInKeywords.Controls.Add(this.picBuiltInKeywords);
            this.grpBuiltInKeywords.Controls.Add(this.lbl2);
            this.grpBuiltInKeywords.Controls.Add(this.editorBuiltInKeywords);
            this.grpBuiltInKeywords.Location = new System.Drawing.Point(12, 3);
            this.grpBuiltInKeywords.Name = "grpBuiltInKeywords";
            this.grpBuiltInKeywords.Size = new System.Drawing.Size(1186, 160);
            this.grpBuiltInKeywords.TabIndex = 8;
            this.grpBuiltInKeywords.TabStop = false;
            this.grpBuiltInKeywords.Text = "Built-in Keywords";
            // 
            // grpFindBuiltInKeywords
            // 
            this.grpFindBuiltInKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFindBuiltInKeywords.BackColor = System.Drawing.Color.Transparent;
            this.grpFindBuiltInKeywords.Controls.Add(this.toolStrip4);
            this.grpFindBuiltInKeywords.Location = new System.Drawing.Point(974, 0);
            this.grpFindBuiltInKeywords.Name = "grpFindBuiltInKeywords";
            this.grpFindBuiltInKeywords.Size = new System.Drawing.Size(212, 55);
            this.grpFindBuiltInKeywords.TabIndex = 115;
            this.grpFindBuiltInKeywords.TabStop = false;
            this.grpFindBuiltInKeywords.Tag = "2";
            this.grpFindBuiltInKeywords.Visible = false;
            // 
            // toolStrip4
            // 
            this.toolStrip4.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip4.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip4.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtFindBuiltInKeywords,
            this.btnNextBuiltInKeywords,
            this.btnPreviousBuiltInKeywords,
            this.btnCloseFindBuiltInKeywords});
            this.toolStrip4.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip4.Location = new System.Drawing.Point(3, 19);
            this.toolStrip4.Name = "toolStrip4";
            this.toolStrip4.Size = new System.Drawing.Size(206, 31);
            this.toolStrip4.Stretch = true;
            this.toolStrip4.TabIndex = 6;
            this.c1ThemeController1.SetTheme(this.toolStrip4, "(default)");
            // 
            // txtFindBuiltInKeywords
            // 
            this.txtFindBuiltInKeywords.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtFindBuiltInKeywords.Name = "txtFindBuiltInKeywords";
            this.txtFindBuiltInKeywords.Size = new System.Drawing.Size(100, 31);
            this.txtFindBuiltInKeywords.Tag = "2";
            this.txtFindBuiltInKeywords.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindNextKeywords_KeyPress);
            this.txtFindBuiltInKeywords.TextChanged += new System.EventHandler(this.FindKeywords_TextChanged);
            // 
            // btnNextBuiltInKeywords
            // 
            this.btnNextBuiltInKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextBuiltInKeywords.Enabled = false;
            this.btnNextBuiltInKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnNextBuiltInKeywords.Image")));
            this.btnNextBuiltInKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextBuiltInKeywords.Name = "btnNextBuiltInKeywords";
            this.btnNextBuiltInKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnNextBuiltInKeywords.Tag = "2";
            this.btnNextBuiltInKeywords.ToolTipText = "Find Next";
            this.btnNextBuiltInKeywords.Click += new System.EventHandler(this.FindNextKeywords_Click);
            // 
            // btnPreviousBuiltInKeywords
            // 
            this.btnPreviousBuiltInKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousBuiltInKeywords.Enabled = false;
            this.btnPreviousBuiltInKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnPreviousBuiltInKeywords.Image")));
            this.btnPreviousBuiltInKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousBuiltInKeywords.Name = "btnPreviousBuiltInKeywords";
            this.btnPreviousBuiltInKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnPreviousBuiltInKeywords.Tag = "2";
            this.btnPreviousBuiltInKeywords.ToolTipText = "Find Previous";
            this.btnPreviousBuiltInKeywords.Click += new System.EventHandler(this.FindPreviousKeywords_Click);
            // 
            // btnCloseFindBuiltInKeywords
            // 
            this.btnCloseFindBuiltInKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCloseFindBuiltInKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseFindBuiltInKeywords.Image")));
            this.btnCloseFindBuiltInKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCloseFindBuiltInKeywords.Name = "btnCloseFindBuiltInKeywords";
            this.btnCloseFindBuiltInKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnCloseFindBuiltInKeywords.Tag = "2";
            this.btnCloseFindBuiltInKeywords.ToolTipText = "Close";
            this.btnCloseFindBuiltInKeywords.Click += new System.EventHandler(this.HideOperatorKeywords);
            // 
            // picBuiltInKeywords
            // 
            this.picBuiltInKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picBuiltInKeywords.Image = ((System.Drawing.Image)(resources.GetObject("picBuiltInKeywords.Image")));
            this.picBuiltInKeywords.Location = new System.Drawing.Point(1169, 7);
            this.picBuiltInKeywords.Name = "picBuiltInKeywords";
            this.picBuiltInKeywords.Size = new System.Drawing.Size(16, 16);
            this.picBuiltInKeywords.TabIndex = 16;
            this.picBuiltInKeywords.TabStop = false;
            this.picBuiltInKeywords.Tag = "2";
            this.picBuiltInKeywords.Click += new System.EventHandler(this.ShowOperatorKeywords);
            // 
            // lbl2
            // 
            this.lbl2.AutoSize = true;
            this.lbl2.Location = new System.Drawing.Point(6, 19);
            this.lbl2.Name = "lbl2";
            this.lbl2.Size = new System.Drawing.Size(0, 16);
            this.lbl2.TabIndex = 14;
            this.lbl2.Visible = false;
            // 
            // editorBuiltInKeywords
            // 
            this.editorBuiltInKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorBuiltInKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorBuiltInKeywords.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editorBuiltInKeywords.CaretLineVisible = true;
            this.editorBuiltInKeywords.EndAtLastLine = false;
            this.editorBuiltInKeywords.HScrollBar = false;
            this.editorBuiltInKeywords.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorBuiltInKeywords.Location = new System.Drawing.Point(12, 23);
            this.editorBuiltInKeywords.Name = "editorBuiltInKeywords";
            this.editorBuiltInKeywords.ScrollWidth = 400;
            this.editorBuiltInKeywords.SelectionEolFilled = true;
            this.editorBuiltInKeywords.Size = new System.Drawing.Size(1160, 124);
            this.editorBuiltInKeywords.Styler = null;
            this.editorBuiltInKeywords.TabIndex = 116;
            this.editorBuiltInKeywords.Tag = "2";
            this.editorBuiltInKeywords.ViewEol = true;
            this.editorBuiltInKeywords.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            this.editorBuiltInKeywords.WhitespaceSize = 3;
            this.editorBuiltInKeywords.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorBuiltInKeywords.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorBuiltInKeywords.Leave += new System.EventHandler(this.Keywords_LeaveCheck);
            // 
            // grpUserDefinedKeywords
            // 
            this.grpUserDefinedKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUserDefinedKeywords.Controls.Add(this.grpFindUserDefinedKeywords);
            this.grpUserDefinedKeywords.Controls.Add(this.picUserDefinedKeywords);
            this.grpUserDefinedKeywords.Controls.Add(this.editorUserDefinedKeywords);
            this.grpUserDefinedKeywords.Location = new System.Drawing.Point(12, 3);
            this.grpUserDefinedKeywords.Name = "grpUserDefinedKeywords";
            this.grpUserDefinedKeywords.Size = new System.Drawing.Size(1185, 164);
            this.grpUserDefinedKeywords.TabIndex = 7;
            this.grpUserDefinedKeywords.TabStop = false;
            this.grpUserDefinedKeywords.Text = "User-defined Keywords";
            // 
            // grpFindUserDefinedKeywords
            // 
            this.grpFindUserDefinedKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpFindUserDefinedKeywords.BackColor = System.Drawing.Color.Transparent;
            this.grpFindUserDefinedKeywords.Controls.Add(this.toolStrip5);
            this.grpFindUserDefinedKeywords.Location = new System.Drawing.Point(973, 0);
            this.grpFindUserDefinedKeywords.Name = "grpFindUserDefinedKeywords";
            this.grpFindUserDefinedKeywords.Size = new System.Drawing.Size(212, 55);
            this.grpFindUserDefinedKeywords.TabIndex = 117;
            this.grpFindUserDefinedKeywords.TabStop = false;
            this.grpFindUserDefinedKeywords.Tag = "3";
            this.grpFindUserDefinedKeywords.Visible = false;
            // 
            // toolStrip5
            // 
            this.toolStrip5.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip5.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip5.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.txtFindUserDefinedKeywords,
            this.btnNextUserDefinedKeywords,
            this.btnPreviousUserDefinedKeywords,
            this.btnCloseFindUserDefinedKeywords});
            this.toolStrip5.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip5.Location = new System.Drawing.Point(3, 19);
            this.toolStrip5.Name = "toolStrip5";
            this.toolStrip5.Size = new System.Drawing.Size(206, 31);
            this.toolStrip5.Stretch = true;
            this.toolStrip5.TabIndex = 6;
            this.c1ThemeController1.SetTheme(this.toolStrip5, "(default)");
            // 
            // txtFindUserDefinedKeywords
            // 
            this.txtFindUserDefinedKeywords.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.txtFindUserDefinedKeywords.Name = "txtFindUserDefinedKeywords";
            this.txtFindUserDefinedKeywords.Size = new System.Drawing.Size(100, 31);
            this.txtFindUserDefinedKeywords.Tag = "3";
            this.txtFindUserDefinedKeywords.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.FindNextKeywords_KeyPress);
            this.txtFindUserDefinedKeywords.TextChanged += new System.EventHandler(this.FindKeywords_TextChanged);
            // 
            // btnNextUserDefinedKeywords
            // 
            this.btnNextUserDefinedKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextUserDefinedKeywords.Enabled = false;
            this.btnNextUserDefinedKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnNextUserDefinedKeywords.Image")));
            this.btnNextUserDefinedKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextUserDefinedKeywords.Name = "btnNextUserDefinedKeywords";
            this.btnNextUserDefinedKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnNextUserDefinedKeywords.Tag = "3";
            this.btnNextUserDefinedKeywords.ToolTipText = "Find Next";
            this.btnNextUserDefinedKeywords.Click += new System.EventHandler(this.FindNextKeywords_Click);
            // 
            // btnPreviousUserDefinedKeywords
            // 
            this.btnPreviousUserDefinedKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousUserDefinedKeywords.Enabled = false;
            this.btnPreviousUserDefinedKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnPreviousUserDefinedKeywords.Image")));
            this.btnPreviousUserDefinedKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousUserDefinedKeywords.Name = "btnPreviousUserDefinedKeywords";
            this.btnPreviousUserDefinedKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnPreviousUserDefinedKeywords.Tag = "3";
            this.btnPreviousUserDefinedKeywords.ToolTipText = "Find Previous";
            this.btnPreviousUserDefinedKeywords.Click += new System.EventHandler(this.FindPreviousKeywords_Click);
            // 
            // btnCloseFindUserDefinedKeywords
            // 
            this.btnCloseFindUserDefinedKeywords.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCloseFindUserDefinedKeywords.Image = ((System.Drawing.Image)(resources.GetObject("btnCloseFindUserDefinedKeywords.Image")));
            this.btnCloseFindUserDefinedKeywords.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCloseFindUserDefinedKeywords.Name = "btnCloseFindUserDefinedKeywords";
            this.btnCloseFindUserDefinedKeywords.Size = new System.Drawing.Size(28, 28);
            this.btnCloseFindUserDefinedKeywords.Tag = "3";
            this.btnCloseFindUserDefinedKeywords.ToolTipText = "Close";
            this.btnCloseFindUserDefinedKeywords.Click += new System.EventHandler(this.HideOperatorKeywords);
            // 
            // picUserDefinedKeywords
            // 
            this.picUserDefinedKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picUserDefinedKeywords.Image = ((System.Drawing.Image)(resources.GetObject("picUserDefinedKeywords.Image")));
            this.picUserDefinedKeywords.Location = new System.Drawing.Point(1168, 7);
            this.picUserDefinedKeywords.Name = "picUserDefinedKeywords";
            this.picUserDefinedKeywords.Size = new System.Drawing.Size(16, 16);
            this.picUserDefinedKeywords.TabIndex = 15;
            this.picUserDefinedKeywords.TabStop = false;
            this.picUserDefinedKeywords.Tag = "3";
            this.picUserDefinedKeywords.Click += new System.EventHandler(this.ShowOperatorKeywords);
            // 
            // editorUserDefinedKeywords
            // 
            this.editorUserDefinedKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorUserDefinedKeywords.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorUserDefinedKeywords.CaretLineBackColor = System.Drawing.Color.LightYellow;
            this.editorUserDefinedKeywords.CaretLineVisible = true;
            this.editorUserDefinedKeywords.EndAtLastLine = false;
            this.editorUserDefinedKeywords.HScrollBar = false;
            this.editorUserDefinedKeywords.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorUserDefinedKeywords.Location = new System.Drawing.Point(12, 23);
            this.editorUserDefinedKeywords.Name = "editorUserDefinedKeywords";
            this.editorUserDefinedKeywords.ScrollWidth = 400;
            this.editorUserDefinedKeywords.SelectionEolFilled = true;
            this.editorUserDefinedKeywords.Size = new System.Drawing.Size(1159, 128);
            this.editorUserDefinedKeywords.Styler = null;
            this.editorUserDefinedKeywords.TabIndex = 118;
            this.editorUserDefinedKeywords.Tag = "3";
            this.editorUserDefinedKeywords.ViewEol = true;
            this.editorUserDefinedKeywords.ViewWhitespace = ScintillaNET.WhitespaceMode.VisibleAlways;
            this.editorUserDefinedKeywords.WhitespaceSize = 3;
            this.editorUserDefinedKeywords.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorUserDefinedKeywords.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorUserDefinedKeywords.Leave += new System.EventHandler(this.Keywords_LeaveCheck);
            // 
            // grpSqlToCode
            // 
            this.grpSqlToCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSqlToCode.BackColor = System.Drawing.Color.Transparent;
            this.grpSqlToCode.Controls.Add(this.txtStringBuilderVariableName);
            this.grpSqlToCode.Controls.Add(this.lblStringBuilderVariableName);
            this.grpSqlToCode.Controls.Add(this.txtSqlVariableName);
            this.grpSqlToCode.Controls.Add(this.chkStripCode);
            this.grpSqlToCode.Controls.Add(this.grpSqlStatementCode);
            this.grpSqlToCode.Controls.Add(this.grpPreviewSql);
            this.grpSqlToCode.Controls.Add(this.lblSqlVariableName);
            this.grpSqlToCode.Controls.Add(this.grpStyle);
            this.grpSqlToCode.Controls.Add(this.grpLanguage);
            this.grpSqlToCode.Location = new System.Drawing.Point(12, 9);
            this.grpSqlToCode.Name = "grpSqlToCode";
            this.grpSqlToCode.Size = new System.Drawing.Size(1186, 664);
            this.grpSqlToCode.TabIndex = 14;
            this.grpSqlToCode.TabStop = false;
            this.grpSqlToCode.Text = "SQL to Code";
            // 
            // txtStringBuilderVariableName
            // 
            this.txtStringBuilderVariableName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtStringBuilderVariableName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtStringBuilderVariableName.Location = new System.Drawing.Point(663, 21);
            this.txtStringBuilderVariableName.Name = "txtStringBuilderVariableName";
            this.txtStringBuilderVariableName.Size = new System.Drawing.Size(119, 21);
            this.txtStringBuilderVariableName.TabIndex = 69;
            this.txtStringBuilderVariableName.Tag = null;
            this.txtStringBuilderVariableName.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtStringBuilderVariableName, "(default)");
            this.txtStringBuilderVariableName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtStringBuilderVariableName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtStringBuilderVariableName_KeyPress);
            this.txtStringBuilderVariableName.Leave += new System.EventHandler(this.txtStringBuilderVariableName_Leave);
            // 
            // lblStringBuilderVariableName
            // 
            this.lblStringBuilderVariableName.AutoSize = true;
            this.lblStringBuilderVariableName.Location = new System.Drawing.Point(566, 22);
            this.lblStringBuilderVariableName.Name = "lblStringBuilderVariableName";
            this.lblStringBuilderVariableName.Size = new System.Drawing.Size(170, 16);
            this.lblStringBuilderVariableName.TabIndex = 68;
            this.lblStringBuilderVariableName.Text = "StringBuilder Variable Name:";
            this.lblStringBuilderVariableName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSqlVariableName
            // 
            this.txtSqlVariableName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtSqlVariableName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSqlVariableName.Location = new System.Drawing.Point(402, 21);
            this.txtSqlVariableName.Name = "txtSqlVariableName";
            this.txtSqlVariableName.Size = new System.Drawing.Size(119, 21);
            this.txtSqlVariableName.TabIndex = 67;
            this.txtSqlVariableName.Tag = null;
            this.txtSqlVariableName.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtSqlVariableName, "(default)");
            this.txtSqlVariableName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtSqlVariableName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSqlVariableName_KeyPress);
            this.txtSqlVariableName.Leave += new System.EventHandler(this.txtSqlVariableName_Leave);
            // 
            // chkStripCode
            // 
            this.chkStripCode.AutoSize = true;
            this.chkStripCode.BackColor = System.Drawing.Color.Transparent;
            this.chkStripCode.BorderColor = System.Drawing.Color.Transparent;
            this.chkStripCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkStripCode.Checked = true;
            this.chkStripCode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStripCode.Enabled = false;
            this.chkStripCode.ForeColor = System.Drawing.Color.Black;
            this.chkStripCode.Location = new System.Drawing.Point(13, 22);
            this.chkStripCode.Name = "chkStripCode";
            this.chkStripCode.Padding = new System.Windows.Forms.Padding(1);
            this.chkStripCode.Size = new System.Drawing.Size(201, 22);
            this.chkStripCode.TabIndex = 66;
            this.chkStripCode.Text = "Strip Code copies to clipboard";
            this.c1ThemeController1.SetTheme(this.chkStripCode, "(default)");
            this.chkStripCode.UseVisualStyleBackColor = true;
            this.chkStripCode.Value = true;
            this.chkStripCode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpSqlStatementCode
            // 
            this.grpSqlStatementCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSqlStatementCode.Controls.Add(this.editorSqlToCode);
            this.grpSqlStatementCode.Location = new System.Drawing.Point(299, 49);
            this.grpSqlStatementCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpSqlStatementCode.Name = "grpSqlStatementCode";
            this.grpSqlStatementCode.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpSqlStatementCode.Size = new System.Drawing.Size(875, 366);
            this.grpSqlStatementCode.TabIndex = 20;
            this.grpSqlStatementCode.TabStop = false;
            this.grpSqlStatementCode.Text = "SQL Statement";
            // 
            // editorSqlToCode
            // 
            this.editorSqlToCode.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSqlToCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlToCode.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSqlToCode.CaretLineVisible = true;
            this.editorSqlToCode.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlToCode.Location = new System.Drawing.Point(10, 23);
            this.editorSqlToCode.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlToCode.Name = "editorSqlToCode";
            this.editorSqlToCode.Size = new System.Drawing.Size(855, 332);
            this.editorSqlToCode.Styler = null;
            this.editorSqlToCode.TabIndex = 41;
            this.editorSqlToCode.Tag = "";
            this.editorSqlToCode.WhitespaceSize = 3;
            this.editorSqlToCode.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlToCode.Leave += new System.EventHandler(this.editorSqlStatement_Leave);
            // 
            // grpPreviewSql
            // 
            this.grpPreviewSql.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreviewSql.Controls.Add(this.editorSqlToCodePreview);
            this.grpPreviewSql.Location = new System.Drawing.Point(299, 286);
            this.grpPreviewSql.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreviewSql.Name = "grpPreviewSql";
            this.grpPreviewSql.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreviewSql.Size = new System.Drawing.Size(875, 366);
            this.grpPreviewSql.TabIndex = 19;
            this.grpPreviewSql.TabStop = false;
            this.grpPreviewSql.Text = "Preview";
            // 
            // editorSqlToCodePreview
            // 
            this.editorSqlToCodePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSqlToCodePreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlToCodePreview.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSqlToCodePreview.CaretLineVisible = true;
            this.editorSqlToCodePreview.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlToCodePreview.Location = new System.Drawing.Point(10, 23);
            this.editorSqlToCodePreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlToCodePreview.Name = "editorSqlToCodePreview";
            this.editorSqlToCodePreview.Size = new System.Drawing.Size(855, 332);
            this.editorSqlToCodePreview.Styler = null;
            this.editorSqlToCodePreview.TabIndex = 41;
            this.editorSqlToCodePreview.Tag = "";
            this.editorSqlToCodePreview.WhitespaceSize = 3;
            this.editorSqlToCodePreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // lblSqlVariableName
            // 
            this.lblSqlVariableName.AutoSize = true;
            this.lblSqlVariableName.Location = new System.Drawing.Point(305, 22);
            this.lblSqlVariableName.Name = "lblSqlVariableName";
            this.lblSqlVariableName.Size = new System.Drawing.Size(121, 16);
            this.lblSqlVariableName.TabIndex = 20;
            this.lblSqlVariableName.Text = "SQL Variable Name:";
            this.lblSqlVariableName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // grpStyle
            // 
            this.grpStyle.Controls.Add(this.lblStyle4);
            this.grpStyle.Controls.Add(this.rdoStyle4);
            this.grpStyle.Controls.Add(this.lblStyle3);
            this.grpStyle.Controls.Add(this.lblStyle2);
            this.grpStyle.Controls.Add(this.lblStyle1);
            this.grpStyle.Controls.Add(this.rdoStyle1);
            this.grpStyle.Controls.Add(this.rdoStyle3);
            this.grpStyle.Controls.Add(this.rdoStyle2);
            this.grpStyle.Location = new System.Drawing.Point(13, 185);
            this.grpStyle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpStyle.Name = "grpStyle";
            this.grpStyle.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpStyle.Size = new System.Drawing.Size(274, 232);
            this.grpStyle.TabIndex = 18;
            this.grpStyle.TabStop = false;
            this.grpStyle.Tag = "1";
            // 
            // lblStyle4
            // 
            this.lblStyle4.AutoSize = true;
            this.lblStyle4.Location = new System.Drawing.Point(31, 196);
            this.lblStyle4.Name = "lblStyle4";
            this.lblStyle4.Size = new System.Drawing.Size(0, 16);
            this.lblStyle4.TabIndex = 11;
            this.lblStyle4.Tag = "4";
            this.c1ThemeController1.SetTheme(this.lblStyle4, "(default)");
            // 
            // rdoStyle4
            // 
            this.rdoStyle4.AutoSize = true;
            this.rdoStyle4.Location = new System.Drawing.Point(15, 173);
            this.rdoStyle4.Name = "rdoStyle4";
            this.rdoStyle4.Size = new System.Drawing.Size(62, 20);
            this.rdoStyle4.TabIndex = 10;
            this.rdoStyle4.Tag = "Style 4";
            this.rdoStyle4.Text = "Style 4";
            this.c1ThemeController1.SetTheme(this.rdoStyle4, "(default)");
            this.rdoStyle4.UseVisualStyleBackColor = true;
            this.rdoStyle4.CheckedChanged += new System.EventHandler(this.rdoStyle_CheckedChanged);
            // 
            // lblStyle3
            // 
            this.lblStyle3.AutoSize = true;
            this.lblStyle3.Location = new System.Drawing.Point(31, 146);
            this.lblStyle3.Name = "lblStyle3";
            this.lblStyle3.Size = new System.Drawing.Size(0, 16);
            this.lblStyle3.TabIndex = 9;
            this.lblStyle3.Tag = "3";
            this.c1ThemeController1.SetTheme(this.lblStyle3, "(default)");
            this.lblStyle3.Click += new System.EventHandler(this.lblStyle_Click);
            // 
            // lblStyle2
            // 
            this.lblStyle2.AutoSize = true;
            this.lblStyle2.Location = new System.Drawing.Point(31, 96);
            this.lblStyle2.Name = "lblStyle2";
            this.lblStyle2.Size = new System.Drawing.Size(0, 16);
            this.lblStyle2.TabIndex = 8;
            this.lblStyle2.Tag = "2";
            this.c1ThemeController1.SetTheme(this.lblStyle2, "(default)");
            this.lblStyle2.Click += new System.EventHandler(this.lblStyle_Click);
            // 
            // lblStyle1
            // 
            this.lblStyle1.AutoSize = true;
            this.lblStyle1.Location = new System.Drawing.Point(31, 46);
            this.lblStyle1.Name = "lblStyle1";
            this.lblStyle1.Size = new System.Drawing.Size(0, 16);
            this.lblStyle1.TabIndex = 7;
            this.lblStyle1.Tag = "1";
            this.c1ThemeController1.SetTheme(this.lblStyle1, "(default)");
            this.lblStyle1.Click += new System.EventHandler(this.lblStyle_Click);
            // 
            // rdoStyle1
            // 
            this.rdoStyle1.AutoSize = true;
            this.rdoStyle1.Checked = true;
            this.rdoStyle1.Location = new System.Drawing.Point(15, 23);
            this.rdoStyle1.Name = "rdoStyle1";
            this.rdoStyle1.Size = new System.Drawing.Size(62, 20);
            this.rdoStyle1.TabIndex = 6;
            this.rdoStyle1.TabStop = true;
            this.rdoStyle1.Tag = "Style 1";
            this.rdoStyle1.Text = "Style 1";
            this.rdoStyle1.UseVisualStyleBackColor = true;
            this.rdoStyle1.CheckedChanged += new System.EventHandler(this.rdoStyle_CheckedChanged);
            // 
            // rdoStyle3
            // 
            this.rdoStyle3.AutoSize = true;
            this.rdoStyle3.Location = new System.Drawing.Point(15, 123);
            this.rdoStyle3.Name = "rdoStyle3";
            this.rdoStyle3.Size = new System.Drawing.Size(62, 20);
            this.rdoStyle3.TabIndex = 2;
            this.rdoStyle3.Tag = "Style 3";
            this.rdoStyle3.Text = "Style 3";
            this.rdoStyle3.UseVisualStyleBackColor = true;
            this.rdoStyle3.CheckedChanged += new System.EventHandler(this.rdoStyle_CheckedChanged);
            // 
            // rdoStyle2
            // 
            this.rdoStyle2.AutoSize = true;
            this.rdoStyle2.Location = new System.Drawing.Point(15, 73);
            this.rdoStyle2.Name = "rdoStyle2";
            this.rdoStyle2.Size = new System.Drawing.Size(62, 20);
            this.rdoStyle2.TabIndex = 1;
            this.rdoStyle2.Tag = "Style 2";
            this.rdoStyle2.Text = "Style 2";
            this.rdoStyle2.UseVisualStyleBackColor = true;
            this.rdoStyle2.CheckedChanged += new System.EventHandler(this.rdoStyle_CheckedChanged);
            // 
            // grpLanguage
            // 
            this.grpLanguage.Controls.Add(this.lstLanguage);
            this.grpLanguage.Location = new System.Drawing.Point(13, 54);
            this.grpLanguage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpLanguage.Name = "grpLanguage";
            this.grpLanguage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpLanguage.Size = new System.Drawing.Size(98, 125);
            this.grpLanguage.TabIndex = 17;
            this.grpLanguage.TabStop = false;
            this.grpLanguage.Text = "Language";
            // 
            // lstLanguage
            // 
            this.lstLanguage.FormattingEnabled = true;
            this.lstLanguage.ItemHeight = 16;
            this.lstLanguage.Items.AddRange(new object[] {
            "C#",
            "VB.Net",
            "VB6/VBA",
            "Delphi6"});
            this.lstLanguage.Location = new System.Drawing.Point(15, 26);
            this.lstLanguage.Name = "lstLanguage";
            this.lstLanguage.Size = new System.Drawing.Size(67, 84);
            this.lstLanguage.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.lstLanguage, "(default)");
            this.lstLanguage.SelectedIndexChanged += new System.EventHandler(this.lstLanguage_SelectedIndexChanged);
            // 
            // grpSqlFormatter
            // 
            this.grpSqlFormatter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSqlFormatter.BackColor = System.Drawing.Color.Transparent;
            this.grpSqlFormatter.Controls.Add(this.splitContainer1);
            this.grpSqlFormatter.Location = new System.Drawing.Point(12, 9);
            this.grpSqlFormatter.Name = "grpSqlFormatter";
            this.grpSqlFormatter.Size = new System.Drawing.Size(1186, 664);
            this.grpSqlFormatter.TabIndex = 15;
            this.grpSqlFormatter.TabStop = false;
            this.grpSqlFormatter.Text = "SQL Formatter";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 19);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.grpFormattingOptions);
            this.c1ThemeController1.SetTheme(this.splitContainer1.Panel1, "(default)");
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.c1ThemeController1.SetTheme(this.splitContainer1.Panel2, "(default)");
            this.splitContainer1.Size = new System.Drawing.Size(1180, 642);
            this.splitContainer1.SplitterDistance = 309;
            this.splitContainer1.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.splitContainer1, "(default)");
            // 
            // grpFormattingOptions
            // 
            this.grpFormattingOptions.Controls.Add(this.cboSqlFormatterEngine);
            this.grpFormattingOptions.Controls.Add(this.lblSqlFormatterEngine);
            this.grpFormattingOptions.Controls.Add(this.cboSqlFormatterIndentSize);
            this.grpFormattingOptions.Controls.Add(this.lblSqlFormatterIndentSize);
            this.grpFormattingOptions.Controls.Add(this.cboSqlFormatterBlankLines);
            this.grpFormattingOptions.Controls.Add(this.lblSqlFormatterBlankLines);
            this.grpFormattingOptions.Controls.Add(this.cboSqlFormatterListItemsPerLine);
            this.grpFormattingOptions.Controls.Add(this.lblSqlFormatterListItemsPerLine);
            this.grpFormattingOptions.Controls.Add(this.txtMaxWidth);
            this.grpFormattingOptions.Controls.Add(this.chkConvertCaseForKeywords);
            this.grpFormattingOptions.Controls.Add(this.rdoLowerCase);
            this.grpFormattingOptions.Controls.Add(this.rdoUpperCase);
            this.grpFormattingOptions.Controls.Add(this.lblMaxWidth2);
            this.grpFormattingOptions.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpFormattingOptions.Location = new System.Drawing.Point(0, 0);
            this.grpFormattingOptions.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpFormattingOptions.Name = "grpFormattingOptions";
            this.grpFormattingOptions.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpFormattingOptions.Size = new System.Drawing.Size(309, 642);
            this.grpFormattingOptions.TabIndex = 17;
            this.grpFormattingOptions.TabStop = false;
            this.grpFormattingOptions.Text = "Formatting Options";
            // 
            // cboSqlFormatterEngine
            // 
            this.cboSqlFormatterEngine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSqlFormatterEngine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSqlFormatterEngine.FormattingEnabled = true;
            this.cboSqlFormatterEngine.Location = new System.Drawing.Point(15, 45);
            this.cboSqlFormatterEngine.Name = "cboSqlFormatterEngine";
            this.cboSqlFormatterEngine.Size = new System.Drawing.Size(200, 24);
            this.cboSqlFormatterEngine.TabIndex = 76;
            this.cboSqlFormatterEngine.SelectedIndexChanged += new System.EventHandler(this.cboSqlFormatterEngine_SelectedIndexChanged);
            // 
            // lblSqlFormatterEngine
            // 
            this.lblSqlFormatterEngine.AutoSize = true;
            this.lblSqlFormatterEngine.Location = new System.Drawing.Point(12, 26);
            this.lblSqlFormatterEngine.Name = "lblSqlFormatterEngine";
            this.lblSqlFormatterEngine.Size = new System.Drawing.Size(107, 16);
            this.lblSqlFormatterEngine.TabIndex = 77;
            this.lblSqlFormatterEngine.Text = "Formatter Engine:";
            this.lblSqlFormatterEngine.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSqlFormatterIndentSize
            // 
            this.cboSqlFormatterIndentSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSqlFormatterIndentSize.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSqlFormatterIndentSize.FormattingEnabled = true;
            this.cboSqlFormatterIndentSize.Location = new System.Drawing.Point(15, 103);
            this.cboSqlFormatterIndentSize.Name = "cboSqlFormatterIndentSize";
            this.cboSqlFormatterIndentSize.Size = new System.Drawing.Size(75, 24);
            this.cboSqlFormatterIndentSize.TabIndex = 78;
            this.cboSqlFormatterIndentSize.SelectedIndexChanged += new System.EventHandler(this.cboSqlFormatterLayout_SelectedIndexChanged);
            // 
            // lblSqlFormatterIndentSize
            // 
            this.lblSqlFormatterIndentSize.AutoSize = true;
            this.lblSqlFormatterIndentSize.Location = new System.Drawing.Point(12, 84);
            this.lblSqlFormatterIndentSize.Name = "lblSqlFormatterIndentSize";
            this.lblSqlFormatterIndentSize.Size = new System.Drawing.Size(123, 16);
            this.lblSqlFormatterIndentSize.TabIndex = 79;
            this.lblSqlFormatterIndentSize.Text = "Indent Size (Spaces):";
            this.lblSqlFormatterIndentSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSqlFormatterBlankLines
            // 
            this.cboSqlFormatterBlankLines.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSqlFormatterBlankLines.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSqlFormatterBlankLines.FormattingEnabled = true;
            this.cboSqlFormatterBlankLines.Location = new System.Drawing.Point(15, 161);
            this.cboSqlFormatterBlankLines.Name = "cboSqlFormatterBlankLines";
            this.cboSqlFormatterBlankLines.Size = new System.Drawing.Size(75, 24);
            this.cboSqlFormatterBlankLines.TabIndex = 80;
            this.cboSqlFormatterBlankLines.SelectedIndexChanged += new System.EventHandler(this.cboSqlFormatterLayout_SelectedIndexChanged);
            // 
            // lblSqlFormatterBlankLines
            // 
            this.lblSqlFormatterBlankLines.AutoSize = true;
            this.lblSqlFormatterBlankLines.Location = new System.Drawing.Point(12, 142);
            this.lblSqlFormatterBlankLines.Name = "lblSqlFormatterBlankLines";
            this.lblSqlFormatterBlankLines.Size = new System.Drawing.Size(188, 16);
            this.lblSqlFormatterBlankLines.TabIndex = 81;
            this.lblSqlFormatterBlankLines.Text = "Blank Lines Between Statements:";
            this.lblSqlFormatterBlankLines.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSqlFormatterListItemsPerLine
            // 
            this.cboSqlFormatterListItemsPerLine.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSqlFormatterListItemsPerLine.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSqlFormatterListItemsPerLine.FormattingEnabled = true;
            this.cboSqlFormatterListItemsPerLine.Location = new System.Drawing.Point(15, 219);
            this.cboSqlFormatterListItemsPerLine.Name = "cboSqlFormatterListItemsPerLine";
            this.cboSqlFormatterListItemsPerLine.Size = new System.Drawing.Size(75, 24);
            this.cboSqlFormatterListItemsPerLine.TabIndex = 82;
            this.cboSqlFormatterListItemsPerLine.SelectedIndexChanged += new System.EventHandler(this.cboSqlFormatterLayout_SelectedIndexChanged);
            // 
            // lblSqlFormatterListItemsPerLine
            // 
            this.lblSqlFormatterListItemsPerLine.AutoSize = true;
            this.lblSqlFormatterListItemsPerLine.Location = new System.Drawing.Point(12, 200);
            this.lblSqlFormatterListItemsPerLine.Name = "lblSqlFormatterListItemsPerLine";
            this.lblSqlFormatterListItemsPerLine.Size = new System.Drawing.Size(108, 16);
            this.lblSqlFormatterListItemsPerLine.TabIndex = 83;
            this.lblSqlFormatterListItemsPerLine.Text = "List Items Per Line:";
            this.lblSqlFormatterListItemsPerLine.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtMaxWidth
            // 
            this.txtMaxWidth.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtMaxWidth.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMaxWidth.Location = new System.Drawing.Point(15, 283);
            this.txtMaxWidth.MaxLength = 4;
            this.txtMaxWidth.Name = "txtMaxWidth";
            this.txtMaxWidth.Size = new System.Drawing.Size(50, 21);
            this.txtMaxWidth.TabIndex = 75;
            this.txtMaxWidth.Tag = null;
            this.txtMaxWidth.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtMaxWidth, "(default)");
            this.txtMaxWidth.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtMaxWidth.TextChanged += new System.EventHandler(this.SqlFormat_TextChanged);
            this.txtMaxWidth.Enter += new System.EventHandler(this.txtMaxWidth_Enter);
            this.txtMaxWidth.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMaxWidth_KeyDown);
            this.txtMaxWidth.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMaxWidth_KeyPress);
            this.txtMaxWidth.Leave += new System.EventHandler(this.txtMaxWidth_Leave);
            // 
            // chkConvertCaseForKeywords
            // 
            this.chkConvertCaseForKeywords.AutoSize = true;
            this.chkConvertCaseForKeywords.BackColor = System.Drawing.Color.Transparent;
            this.chkConvertCaseForKeywords.BorderColor = System.Drawing.Color.Transparent;
            this.chkConvertCaseForKeywords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkConvertCaseForKeywords.ForeColor = System.Drawing.Color.Black;
            this.chkConvertCaseForKeywords.Location = new System.Drawing.Point(15, 325);
            this.chkConvertCaseForKeywords.Name = "chkConvertCaseForKeywords";
            this.chkConvertCaseForKeywords.Padding = new System.Windows.Forms.Padding(1);
            this.chkConvertCaseForKeywords.Size = new System.Drawing.Size(178, 22);
            this.chkConvertCaseForKeywords.TabIndex = 74;
            this.chkConvertCaseForKeywords.Text = "Convert Case for Keywords";
            this.c1ThemeController1.SetTheme(this.chkConvertCaseForKeywords, "(default)");
            this.chkConvertCaseForKeywords.UseVisualStyleBackColor = true;
            this.chkConvertCaseForKeywords.Value = null;
            this.chkConvertCaseForKeywords.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkConvertCaseForKeywords.CheckedChanged += new System.EventHandler(this.SqlFormatter_CheckedChanged);
            // 
            // rdoLowerCase
            // 
            this.rdoLowerCase.AutoSize = true;
            this.rdoLowerCase.Location = new System.Drawing.Point(34, 376);
            this.rdoLowerCase.Name = "rdoLowerCase";
            this.rdoLowerCase.Size = new System.Drawing.Size(86, 20);
            this.rdoLowerCase.TabIndex = 35;
            this.rdoLowerCase.TabStop = true;
            this.rdoLowerCase.Text = "lower Case";
            this.rdoLowerCase.UseVisualStyleBackColor = true;
            this.rdoLowerCase.CheckedChanged += new System.EventHandler(this.SqlFormatter2_CheckedChanged);
            // 
            // rdoUpperCase
            // 
            this.rdoUpperCase.AutoSize = true;
            this.rdoUpperCase.Location = new System.Drawing.Point(34, 351);
            this.rdoUpperCase.Name = "rdoUpperCase";
            this.rdoUpperCase.Size = new System.Drawing.Size(93, 20);
            this.rdoUpperCase.TabIndex = 34;
            this.rdoUpperCase.TabStop = true;
            this.rdoUpperCase.Text = "UPPER Case";
            this.rdoUpperCase.UseVisualStyleBackColor = true;
            this.rdoUpperCase.CheckedChanged += new System.EventHandler(this.SqlFormatter2_CheckedChanged);
            // 
            // lblMaxWidth2
            // 
            this.lblMaxWidth2.AutoSize = true;
            this.lblMaxWidth2.Location = new System.Drawing.Point(12, 264);
            this.lblMaxWidth2.Name = "lblMaxWidth2";
            this.lblMaxWidth2.Size = new System.Drawing.Size(143, 16);
            this.lblMaxWidth2.TabIndex = 20;
            this.lblMaxWidth2.Text = "Inline Block Max Length:";
            this.lblMaxWidth2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.grpSqlStatementFormatter);
            this.c1ThemeController1.SetTheme(this.splitContainer2.Panel1, "(default)");
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.grpPreviewFormatter);
            this.c1ThemeController1.SetTheme(this.splitContainer2.Panel2, "(default)");
            this.splitContainer2.Size = new System.Drawing.Size(867, 642);
            this.splitContainer2.SplitterDistance = 242;
            this.splitContainer2.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.splitContainer2, "(default)");
            // 
            // grpSqlStatementFormatter
            // 
            this.grpSqlStatementFormatter.Controls.Add(this.editorSqlFormatter);
            this.grpSqlStatementFormatter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpSqlStatementFormatter.Location = new System.Drawing.Point(0, 0);
            this.grpSqlStatementFormatter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpSqlStatementFormatter.Name = "grpSqlStatementFormatter";
            this.grpSqlStatementFormatter.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpSqlStatementFormatter.Size = new System.Drawing.Size(867, 242);
            this.grpSqlStatementFormatter.TabIndex = 20;
            this.grpSqlStatementFormatter.TabStop = false;
            this.grpSqlStatementFormatter.Text = "SQL Statement";
            // 
            // editorSqlFormatter
            // 
            this.editorSqlFormatter.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSqlFormatter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlFormatter.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSqlFormatter.CaretLineVisible = true;
            this.editorSqlFormatter.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlFormatter.Location = new System.Drawing.Point(10, 23);
            this.editorSqlFormatter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlFormatter.Name = "editorSqlFormatter";
            this.editorSqlFormatter.Size = new System.Drawing.Size(847, 208);
            this.editorSqlFormatter.Styler = null;
            this.editorSqlFormatter.TabIndex = 41;
            this.editorSqlFormatter.Tag = "";
            this.editorSqlFormatter.WhitespaceSize = 3;
            this.editorSqlFormatter.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlFormatter.TextChanged += new System.EventHandler(this.SqlFormat_TextChanged);
            // 
            // grpPreviewFormatter
            // 
            this.grpPreviewFormatter.Controls.Add(this.lblSqlFormatterPreviewStatus);
            this.grpPreviewFormatter.Controls.Add(this.editorSqlFormatterPreview);
            this.grpPreviewFormatter.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpPreviewFormatter.Location = new System.Drawing.Point(0, 0);
            this.grpPreviewFormatter.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreviewFormatter.Name = "grpPreviewFormatter";
            this.grpPreviewFormatter.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.grpPreviewFormatter.Size = new System.Drawing.Size(867, 396);
            this.grpPreviewFormatter.TabIndex = 19;
            this.grpPreviewFormatter.TabStop = false;
            this.grpPreviewFormatter.Text = "Preview";
            // 
            // lblSqlFormatterPreviewStatus
            // 
            this.lblSqlFormatterPreviewStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSqlFormatterPreviewStatus.AutoEllipsis = true;
            this.lblSqlFormatterPreviewStatus.ForeColor = System.Drawing.Color.Green;
            this.lblSqlFormatterPreviewStatus.Location = new System.Drawing.Point(10, 367);
            this.lblSqlFormatterPreviewStatus.Name = "lblSqlFormatterPreviewStatus";
            this.lblSqlFormatterPreviewStatus.Size = new System.Drawing.Size(847, 20);
            this.lblSqlFormatterPreviewStatus.TabIndex = 42;
            this.lblSqlFormatterPreviewStatus.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // editorSqlFormatterPreview
            // 
            this.editorSqlFormatterPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSqlFormatterPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlFormatterPreview.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSqlFormatterPreview.CaretLineVisible = true;
            this.editorSqlFormatterPreview.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlFormatterPreview.Location = new System.Drawing.Point(10, 23);
            this.editorSqlFormatterPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlFormatterPreview.Name = "editorSqlFormatterPreview";
            this.editorSqlFormatterPreview.Size = new System.Drawing.Size(847, 339);
            this.editorSqlFormatterPreview.Styler = null;
            this.editorSqlFormatterPreview.TabIndex = 41;
            this.editorSqlFormatterPreview.Tag = "";
            this.editorSqlFormatterPreview.WhitespaceSize = 3;
            this.editorSqlFormatterPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // lblGlobalOverview
            // 
            this.lblGlobalOverview.AutoSize = true;
            this.lblGlobalOverview.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.lblGlobalOverview.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblGlobalOverview.ForeColor = System.Drawing.Color.Green;
            this.lblGlobalOverview.Location = new System.Drawing.Point(17, 10);
            this.lblGlobalOverview.Name = "lblGlobalOverview";
            this.lblGlobalOverview.Size = new System.Drawing.Size(315, 16);
            this.lblGlobalOverview.TabIndex = 86;
            this.lblGlobalOverview.Text = "[Settings on this tab apply to all database connections]";
            this.lblGlobalOverview.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpCommitRollbackIcon
            // 
            this.grpCommitRollbackIcon.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCommitRollbackIcon.BackColor = System.Drawing.Color.Transparent;
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox9);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox10);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle6);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox11);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox12);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle5);
            this.grpCommitRollbackIcon.Controls.Add(this.lblCommitRollbackIconInfo);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox7);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox8);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle4);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox6);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox5);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle3);
            this.grpCommitRollbackIcon.Controls.Add(this.lblCommitRollbackIcon);
            this.grpCommitRollbackIcon.Controls.Add(this.lblStarCommitRollbackIcon);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox4);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox3);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox2);
            this.grpCommitRollbackIcon.Controls.Add(this.pictureBox1);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle2);
            this.grpCommitRollbackIcon.Controls.Add(this.rdoCommitRollbackIconStyle1);
            this.grpCommitRollbackIcon.Location = new System.Drawing.Point(447, 12);
            this.grpCommitRollbackIcon.Name = "grpCommitRollbackIcon";
            this.grpCommitRollbackIcon.Size = new System.Drawing.Size(723, 107);
            this.grpCommitRollbackIcon.TabIndex = 43;
            this.grpCommitRollbackIcon.TabStop = false;
            this.grpCommitRollbackIcon.Text = "Transaction Icons (Commit / Rollback)";
            // 
            // pictureBox9
            // 
            this.pictureBox9.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox9.Image")));
            this.pictureBox9.Location = new System.Drawing.Point(529, 71);
            this.pictureBox9.Name = "pictureBox9";
            this.pictureBox9.Size = new System.Drawing.Size(24, 24);
            this.pictureBox9.TabIndex = 322;
            this.pictureBox9.TabStop = false;
            // 
            // pictureBox10
            // 
            this.pictureBox10.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox10.Image")));
            this.pictureBox10.Location = new System.Drawing.Point(498, 71);
            this.pictureBox10.Name = "pictureBox10";
            this.pictureBox10.Size = new System.Drawing.Size(24, 24);
            this.pictureBox10.TabIndex = 321;
            this.pictureBox10.TabStop = false;
            // 
            // rdoCommitRollbackIconStyle6
            // 
            this.rdoCommitRollbackIconStyle6.AutoSize = true;
            this.rdoCommitRollbackIconStyle6.Location = new System.Drawing.Point(495, 46);
            this.rdoCommitRollbackIconStyle6.Name = "rdoCommitRollbackIconStyle6";
            this.rdoCommitRollbackIconStyle6.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle6.TabIndex = 320;
            this.rdoCommitRollbackIconStyle6.Text = "Style 6";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle6, "(default)");
            this.rdoCommitRollbackIconStyle6.UseVisualStyleBackColor = true;
            // 
            // pictureBox11
            // 
            this.pictureBox11.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox11.Image")));
            this.pictureBox11.Location = new System.Drawing.Point(434, 71);
            this.pictureBox11.Name = "pictureBox11";
            this.pictureBox11.Size = new System.Drawing.Size(24, 24);
            this.pictureBox11.TabIndex = 319;
            this.pictureBox11.TabStop = false;
            // 
            // pictureBox12
            // 
            this.pictureBox12.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox12.Image")));
            this.pictureBox12.Location = new System.Drawing.Point(403, 71);
            this.pictureBox12.Name = "pictureBox12";
            this.pictureBox12.Size = new System.Drawing.Size(24, 24);
            this.pictureBox12.TabIndex = 318;
            this.pictureBox12.TabStop = false;
            // 
            // rdoCommitRollbackIconStyle5
            // 
            this.rdoCommitRollbackIconStyle5.AutoSize = true;
            this.rdoCommitRollbackIconStyle5.Location = new System.Drawing.Point(400, 46);
            this.rdoCommitRollbackIconStyle5.Name = "rdoCommitRollbackIconStyle5";
            this.rdoCommitRollbackIconStyle5.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle5.TabIndex = 317;
            this.rdoCommitRollbackIconStyle5.Text = "Style 5";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle5, "(default)");
            this.rdoCommitRollbackIconStyle5.UseVisualStyleBackColor = true;
            // 
            // lblCommitRollbackIconInfo
            // 
            this.lblCommitRollbackIconInfo.AutoSize = true;
            this.lblCommitRollbackIconInfo.Location = new System.Drawing.Point(18, 24);
            this.lblCommitRollbackIconInfo.Name = "lblCommitRollbackIconInfo";
            this.lblCommitRollbackIconInfo.Size = new System.Drawing.Size(315, 16);
            this.lblCommitRollbackIconInfo.TabIndex = 316;
            this.lblCommitRollbackIconInfo.Text = "Customize icon colors and styles for Commit / Rollback";
            this.c1ThemeController1.SetTheme(this.lblCommitRollbackIconInfo, "(default)");
            // 
            // pictureBox7
            // 
            this.pictureBox7.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox7.Image")));
            this.pictureBox7.Location = new System.Drawing.Point(339, 71);
            this.pictureBox7.Name = "pictureBox7";
            this.pictureBox7.Size = new System.Drawing.Size(24, 24);
            this.pictureBox7.TabIndex = 315;
            this.pictureBox7.TabStop = false;
            // 
            // pictureBox8
            // 
            this.pictureBox8.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox8.Image")));
            this.pictureBox8.Location = new System.Drawing.Point(308, 71);
            this.pictureBox8.Name = "pictureBox8";
            this.pictureBox8.Size = new System.Drawing.Size(24, 24);
            this.pictureBox8.TabIndex = 314;
            this.pictureBox8.TabStop = false;
            // 
            // rdoCommitRollbackIconStyle4
            // 
            this.rdoCommitRollbackIconStyle4.AutoSize = true;
            this.rdoCommitRollbackIconStyle4.Location = new System.Drawing.Point(305, 46);
            this.rdoCommitRollbackIconStyle4.Name = "rdoCommitRollbackIconStyle4";
            this.rdoCommitRollbackIconStyle4.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle4.TabIndex = 313;
            this.rdoCommitRollbackIconStyle4.Text = "Style 4";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle4, "(default)");
            this.rdoCommitRollbackIconStyle4.UseVisualStyleBackColor = true;
            // 
            // pictureBox6
            // 
            this.pictureBox6.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox6.Image")));
            this.pictureBox6.Location = new System.Drawing.Point(244, 71);
            this.pictureBox6.Name = "pictureBox6";
            this.pictureBox6.Size = new System.Drawing.Size(24, 24);
            this.pictureBox6.TabIndex = 312;
            this.pictureBox6.TabStop = false;
            // 
            // pictureBox5
            // 
            this.pictureBox5.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox5.Image")));
            this.pictureBox5.Location = new System.Drawing.Point(213, 71);
            this.pictureBox5.Name = "pictureBox5";
            this.pictureBox5.Size = new System.Drawing.Size(24, 24);
            this.pictureBox5.TabIndex = 311;
            this.pictureBox5.TabStop = false;
            // 
            // rdoCommitRollbackIconStyle3
            // 
            this.rdoCommitRollbackIconStyle3.AutoSize = true;
            this.rdoCommitRollbackIconStyle3.Location = new System.Drawing.Point(210, 46);
            this.rdoCommitRollbackIconStyle3.Name = "rdoCommitRollbackIconStyle3";
            this.rdoCommitRollbackIconStyle3.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle3.TabIndex = 310;
            this.rdoCommitRollbackIconStyle3.Text = "Style 3";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle3, "(default)");
            this.rdoCommitRollbackIconStyle3.UseVisualStyleBackColor = true;
            // 
            // lblCommitRollbackIcon
            // 
            this.lblCommitRollbackIcon.AutoSize = true;
            this.lblCommitRollbackIcon.Location = new System.Drawing.Point(7, 12);
            this.lblCommitRollbackIcon.Name = "lblCommitRollbackIcon";
            this.lblCommitRollbackIcon.Size = new System.Drawing.Size(0, 16);
            this.lblCommitRollbackIcon.TabIndex = 309;
            this.lblCommitRollbackIcon.Visible = false;
            // 
            // lblStarCommitRollbackIcon
            // 
            this.lblStarCommitRollbackIcon.AutoSize = true;
            this.lblStarCommitRollbackIcon.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarCommitRollbackIcon.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarCommitRollbackIcon.Location = new System.Drawing.Point(224, 3);
            this.lblStarCommitRollbackIcon.Name = "lblStarCommitRollbackIcon";
            this.lblStarCommitRollbackIcon.Size = new System.Drawing.Size(14, 15);
            this.lblStarCommitRollbackIcon.TabIndex = 304;
            this.lblStarCommitRollbackIcon.Text = "*";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox4.Image")));
            this.pictureBox4.Location = new System.Drawing.Point(149, 71);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(24, 24);
            this.pictureBox4.TabIndex = 308;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox3.Image")));
            this.pictureBox3.Location = new System.Drawing.Point(118, 71);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(24, 24);
            this.pictureBox3.TabIndex = 307;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox2.Image")));
            this.pictureBox2.Location = new System.Drawing.Point(54, 71);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(24, 24);
            this.pictureBox2.TabIndex = 306;
            this.pictureBox2.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(23, 71);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(24, 24);
            this.pictureBox1.TabIndex = 305;
            this.pictureBox1.TabStop = false;
            // 
            // rdoCommitRollbackIconStyle2
            // 
            this.rdoCommitRollbackIconStyle2.AutoSize = true;
            this.rdoCommitRollbackIconStyle2.Location = new System.Drawing.Point(115, 46);
            this.rdoCommitRollbackIconStyle2.Name = "rdoCommitRollbackIconStyle2";
            this.rdoCommitRollbackIconStyle2.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle2.TabIndex = 4;
            this.rdoCommitRollbackIconStyle2.Text = "Style 2";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle2, "(default)");
            this.rdoCommitRollbackIconStyle2.UseVisualStyleBackColor = true;
            // 
            // rdoCommitRollbackIconStyle1
            // 
            this.rdoCommitRollbackIconStyle1.AutoSize = true;
            this.rdoCommitRollbackIconStyle1.Checked = true;
            this.rdoCommitRollbackIconStyle1.Location = new System.Drawing.Point(20, 46);
            this.rdoCommitRollbackIconStyle1.Name = "rdoCommitRollbackIconStyle1";
            this.rdoCommitRollbackIconStyle1.Size = new System.Drawing.Size(62, 20);
            this.rdoCommitRollbackIconStyle1.TabIndex = 3;
            this.rdoCommitRollbackIconStyle1.TabStop = true;
            this.rdoCommitRollbackIconStyle1.Text = "Style 1";
            this.c1ThemeController1.SetTheme(this.rdoCommitRollbackIconStyle1, "(default)");
            this.rdoCommitRollbackIconStyle1.UseVisualStyleBackColor = true;
            // 
            // grpBackup
            // 
            this.grpBackup.BackColor = System.Drawing.Color.Transparent;
            this.grpBackup.Controls.Add(this.btnClear3);
            this.grpBackup.Controls.Add(this.chkAskMeBeforeOpenUnsavedFiles);
            this.grpBackup.Controls.Add(this.btnBackupPathOpenFolder);
            this.grpBackup.Controls.Add(this.btnBrowseBackupPath);
            this.grpBackup.Controls.Add(this.txtBackupPath);
            this.grpBackup.Controls.Add(this.lblBackupPath);
            this.grpBackup.Controls.Add(this.chkRememberUnsavedFiles);
            this.grpBackup.Location = new System.Drawing.Point(13, 182);
            this.grpBackup.Name = "grpBackup";
            this.grpBackup.Size = new System.Drawing.Size(536, 115);
            this.grpBackup.TabIndex = 42;
            this.grpBackup.TabStop = false;
            this.grpBackup.Text = "  ";
            // 
            // btnClear3
            // 
            this.btnClear3.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear3.Image = ((System.Drawing.Image)(resources.GetObject("btnClear3.Image")));
            this.btnClear3.Location = new System.Drawing.Point(458, 81);
            this.btnClear3.Name = "btnClear3";
            this.btnClear3.Size = new System.Drawing.Size(22, 21);
            this.btnClear3.TabIndex = 318;
            this.btnClear3.Tag = "1";
            this.c1ThemeController1.SetTheme(this.btnClear3, "(default)");
            this.btnClear3.UseVisualStyleBackColor = true;
            this.btnClear3.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClear3.Click += new System.EventHandler(this.btnClear3_Click);
            // 
            // chkAskMeBeforeOpenUnsavedFiles
            // 
            this.chkAskMeBeforeOpenUnsavedFiles.AutoSize = true;
            this.chkAskMeBeforeOpenUnsavedFiles.BackColor = System.Drawing.Color.Transparent;
            this.chkAskMeBeforeOpenUnsavedFiles.BorderColor = System.Drawing.Color.Transparent;
            this.chkAskMeBeforeOpenUnsavedFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkAskMeBeforeOpenUnsavedFiles.ForeColor = System.Drawing.Color.Black;
            this.chkAskMeBeforeOpenUnsavedFiles.Location = new System.Drawing.Point(25, 26);
            this.chkAskMeBeforeOpenUnsavedFiles.Name = "chkAskMeBeforeOpenUnsavedFiles";
            this.chkAskMeBeforeOpenUnsavedFiles.Padding = new System.Windows.Forms.Padding(1);
            this.chkAskMeBeforeOpenUnsavedFiles.Size = new System.Drawing.Size(416, 22);
            this.chkAskMeBeforeOpenUnsavedFiles.TabIndex = 317;
            this.chkAskMeBeforeOpenUnsavedFiles.Text = "When starting JasonQuery, ask me if I want to open \"All unsaved files\"";
            this.c1ThemeController1.SetTheme(this.chkAskMeBeforeOpenUnsavedFiles, "(default)");
            this.chkAskMeBeforeOpenUnsavedFiles.UseVisualStyleBackColor = true;
            this.chkAskMeBeforeOpenUnsavedFiles.Value = false;
            this.chkAskMeBeforeOpenUnsavedFiles.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnBackupPathOpenFolder
            // 
            this.btnBackupPathOpenFolder.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBackupPathOpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnBackupPathOpenFolder.Image")));
            this.btnBackupPathOpenFolder.Location = new System.Drawing.Point(433, 81);
            this.btnBackupPathOpenFolder.Name = "btnBackupPathOpenFolder";
            this.btnBackupPathOpenFolder.Size = new System.Drawing.Size(22, 21);
            this.btnBackupPathOpenFolder.TabIndex = 316;
            this.btnBackupPathOpenFolder.Tag = "1";
            this.c1ThemeController1.SetTheme(this.btnBackupPathOpenFolder, "(default)");
            this.btnBackupPathOpenFolder.UseVisualStyleBackColor = true;
            this.btnBackupPathOpenFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBackupPathOpenFolder.Click += new System.EventHandler(this.btnBackupPathOpenFolder_Click);
            // 
            // btnBrowseBackupPath
            // 
            this.btnBrowseBackupPath.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowseBackupPath.Location = new System.Drawing.Point(408, 81);
            this.btnBrowseBackupPath.Name = "btnBrowseBackupPath";
            this.btnBrowseBackupPath.Size = new System.Drawing.Size(22, 21);
            this.btnBrowseBackupPath.TabIndex = 315;
            this.btnBrowseBackupPath.Tag = "1";
            this.btnBrowseBackupPath.Text = "...";
            this.c1ThemeController1.SetTheme(this.btnBrowseBackupPath, "(default)");
            this.btnBrowseBackupPath.UseVisualStyleBackColor = true;
            this.btnBrowseBackupPath.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseBackupPath.Click += new System.EventHandler(this.btnBrowseBackupPath_Click);
            // 
            // txtBackupPath
            // 
            this.txtBackupPath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtBackupPath.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBackupPath.Location = new System.Drawing.Point(107, 81);
            this.txtBackupPath.Name = "txtBackupPath";
            this.txtBackupPath.ReadOnly = true;
            this.txtBackupPath.Size = new System.Drawing.Size(290, 21);
            this.txtBackupPath.TabIndex = 314;
            this.txtBackupPath.Tag = null;
            this.txtBackupPath.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtBackupPath, "(default)");
            this.txtBackupPath.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblBackupPath
            // 
            this.lblBackupPath.AutoSize = true;
            this.lblBackupPath.Location = new System.Drawing.Point(23, 83);
            this.lblBackupPath.Name = "lblBackupPath";
            this.lblBackupPath.Size = new System.Drawing.Size(80, 16);
            this.lblBackupPath.TabIndex = 313;
            this.lblBackupPath.Text = "Backup path:";
            this.lblBackupPath.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblBackupPath, "(default)");
            // 
            // chkRememberUnsavedFiles
            // 
            this.chkRememberUnsavedFiles.AutoSize = true;
            this.chkRememberUnsavedFiles.BackColor = System.Drawing.Color.Transparent;
            this.chkRememberUnsavedFiles.BorderColor = System.Drawing.Color.Transparent;
            this.chkRememberUnsavedFiles.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkRememberUnsavedFiles.Checked = true;
            this.chkRememberUnsavedFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRememberUnsavedFiles.Enabled = false;
            this.chkRememberUnsavedFiles.ForeColor = System.Drawing.Color.Black;
            this.chkRememberUnsavedFiles.Location = new System.Drawing.Point(25, 53);
            this.chkRememberUnsavedFiles.Name = "chkRememberUnsavedFiles";
            this.chkRememberUnsavedFiles.Padding = new System.Windows.Forms.Padding(1);
            this.chkRememberUnsavedFiles.Size = new System.Drawing.Size(262, 22);
            this.chkRememberUnsavedFiles.TabIndex = 304;
            this.chkRememberUnsavedFiles.Text = "Remember current session for next launch";
            this.c1ThemeController1.SetTheme(this.chkRememberUnsavedFiles, "(default)");
            this.chkRememberUnsavedFiles.UseVisualStyleBackColor = true;
            this.chkRememberUnsavedFiles.Value = true;
            this.chkRememberUnsavedFiles.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarBackup
            // 
            this.lblStarBackup.AutoSize = true;
            this.lblStarBackup.BackColor = System.Drawing.Color.Transparent;
            this.lblStarBackup.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarBackup.Location = new System.Drawing.Point(330, 185);
            this.lblStarBackup.Name = "lblStarBackup";
            this.lblStarBackup.Size = new System.Drawing.Size(14, 15);
            this.lblStarBackup.TabIndex = 303;
            this.lblStarBackup.Text = "*";
            // 
            // chkEnableBackup
            // 
            this.chkEnableBackup.AutoSize = true;
            this.chkEnableBackup.BackColor = System.Drawing.Color.Transparent;
            this.chkEnableBackup.BorderColor = System.Drawing.Color.Transparent;
            this.chkEnableBackup.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEnableBackup.ForeColor = System.Drawing.Color.Black;
            this.chkEnableBackup.Location = new System.Drawing.Point(22, 181);
            this.chkEnableBackup.Name = "chkEnableBackup";
            this.chkEnableBackup.Padding = new System.Windows.Forms.Padding(1);
            this.chkEnableBackup.Size = new System.Drawing.Size(284, 22);
            this.chkEnableBackup.TabIndex = 302;
            this.chkEnableBackup.Text = "Enable session snapshot and periodic backup";
            this.c1ThemeController1.SetTheme(this.chkEnableBackup, "(default)");
            this.chkEnableBackup.UseVisualStyleBackColor = true;
            this.chkEnableBackup.Value = null;
            this.chkEnableBackup.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpMaxEntries
            // 
            this.grpMaxEntries.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMaxEntries.BackColor = System.Drawing.Color.Transparent;
            this.grpMaxEntries.Controls.Add(this.lblMaxEntriesInfo);
            this.grpMaxEntries.Controls.Add(this.txtMyFavorite);
            this.grpMaxEntries.Controls.Add(this.txtRecentFiles);
            this.grpMaxEntries.Controls.Add(this.lblMyFavorite);
            this.grpMaxEntries.Controls.Add(this.lblRecentFiles);
            this.grpMaxEntries.Location = new System.Drawing.Point(566, 134);
            this.grpMaxEntries.Name = "grpMaxEntries";
            this.grpMaxEntries.Size = new System.Drawing.Size(606, 80);
            this.grpMaxEntries.TabIndex = 41;
            this.grpMaxEntries.TabStop = false;
            this.grpMaxEntries.Text = "Max. number of entries";
            // 
            // lblMaxEntriesInfo
            // 
            this.lblMaxEntriesInfo.AutoSize = true;
            this.lblMaxEntriesInfo.Location = new System.Drawing.Point(18, 24);
            this.lblMaxEntriesInfo.Name = "lblMaxEntriesInfo";
            this.lblMaxEntriesInfo.Size = new System.Drawing.Size(472, 16);
            this.lblMaxEntriesInfo.TabIndex = 317;
            this.lblMaxEntriesInfo.Text = "Controls the maximum number of items shown in \"Recent Files\" and \"My Favorites\".";
            this.c1ThemeController1.SetTheme(this.lblMaxEntriesInfo, "(default)");
            // 
            // txtMyFavorite
            // 
            this.txtMyFavorite.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtMyFavorite.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMyFavorite.Location = new System.Drawing.Point(350, 46);
            this.txtMyFavorite.MaxLength = 2;
            this.txtMyFavorite.Name = "txtMyFavorite";
            this.txtMyFavorite.Size = new System.Drawing.Size(30, 21);
            this.txtMyFavorite.TabIndex = 6;
            this.txtMyFavorite.Tag = null;
            this.txtMyFavorite.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtMyFavorite.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtMyFavorite, "(default)");
            this.txtMyFavorite.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtMyFavorite.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtMyFavorite_MouseClick);
            this.txtMyFavorite.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtMyFavorite_KeyDown);
            this.txtMyFavorite.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtMyFavorite_KeyPress);
            this.txtMyFavorite.Leave += new System.EventHandler(this.txtMyFavorite_Leave);
            // 
            // txtRecentFiles
            // 
            this.txtRecentFiles.AutoSize = false;
            this.txtRecentFiles.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRecentFiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRecentFiles.Location = new System.Drawing.Point(132, 46);
            this.txtRecentFiles.MaxLength = 2;
            this.txtRecentFiles.Name = "txtRecentFiles";
            this.txtRecentFiles.Size = new System.Drawing.Size(30, 21);
            this.txtRecentFiles.TabIndex = 4;
            this.txtRecentFiles.Tag = null;
            this.txtRecentFiles.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRecentFiles.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtRecentFiles, "(default)");
            this.txtRecentFiles.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtRecentFiles.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtRecentFiles_MouseClick);
            this.txtRecentFiles.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtRecentFiles_KeyDown);
            this.txtRecentFiles.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtRecentFiles_KeyPress);
            this.txtRecentFiles.Leave += new System.EventHandler(this.txtRecentFiles_Leave);
            // 
            // lblMyFavorite
            // 
            this.lblMyFavorite.AutoSize = true;
            this.lblMyFavorite.Location = new System.Drawing.Point(236, 48);
            this.lblMyFavorite.Name = "lblMyFavorite";
            this.lblMyFavorite.Size = new System.Drawing.Size(76, 16);
            this.lblMyFavorite.TabIndex = 1;
            this.lblMyFavorite.Text = "My Favorite:";
            // 
            // lblRecentFiles
            // 
            this.lblRecentFiles.AutoSize = true;
            this.lblRecentFiles.Location = new System.Drawing.Point(17, 48);
            this.lblRecentFiles.Name = "lblRecentFiles";
            this.lblRecentFiles.Size = new System.Drawing.Size(74, 16);
            this.lblRecentFiles.TabIndex = 0;
            this.lblRecentFiles.Text = "Recent files:";
            // 
            // grpGeneral
            // 
            this.grpGeneral.BackColor = System.Drawing.Color.Transparent;
            this.grpGeneral.Controls.Add(this.lblStarShowDatabaseName);
            this.grpGeneral.Controls.Add(this.chkShowDatabaseName);
            this.grpGeneral.Controls.Add(this.lblStarShowIP);
            this.grpGeneral.Controls.Add(this.chkShowIP);
            this.grpGeneral.Controls.Add(this.lblStarShowVersion);
            this.grpGeneral.Controls.Add(this.chkShowVersion);
            this.grpGeneral.Controls.Add(this.cboLocalization);
            this.grpGeneral.Controls.Add(this.cboDateFormat);
            this.grpGeneral.Controls.Add(this.lblDateFormat);
            this.grpGeneral.Controls.Add(this.lblLocalization);
            this.grpGeneral.Location = new System.Drawing.Point(13, 12);
            this.grpGeneral.Name = "grpGeneral";
            this.grpGeneral.Size = new System.Drawing.Size(536, 163);
            this.grpGeneral.TabIndex = 41;
            this.grpGeneral.TabStop = false;
            this.grpGeneral.Text = "Interface & Display";
            // 
            // lblStarShowDatabaseName
            // 
            this.lblStarShowDatabaseName.AutoSize = true;
            this.lblStarShowDatabaseName.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarShowDatabaseName.Location = new System.Drawing.Point(400, 80);
            this.lblStarShowDatabaseName.Name = "lblStarShowDatabaseName";
            this.lblStarShowDatabaseName.Size = new System.Drawing.Size(14, 15);
            this.lblStarShowDatabaseName.TabIndex = 85;
            this.lblStarShowDatabaseName.Text = "*";
            this.c1ThemeController1.SetTheme(this.lblStarShowDatabaseName, "(default)");
            // 
            // chkShowDatabaseName
            // 
            this.chkShowDatabaseName.AutoSize = true;
            this.chkShowDatabaseName.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowDatabaseName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowDatabaseName.ForeColor = System.Drawing.Color.Black;
            this.chkShowDatabaseName.Location = new System.Drawing.Point(19, 76);
            this.chkShowDatabaseName.Name = "chkShowDatabaseName";
            this.chkShowDatabaseName.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowDatabaseName.Size = new System.Drawing.Size(261, 22);
            this.chkShowDatabaseName.TabIndex = 84;
            this.chkShowDatabaseName.Text = "Display the database name in the title bar";
            this.c1ThemeController1.SetTheme(this.chkShowDatabaseName, "(default)");
            this.chkShowDatabaseName.UseVisualStyleBackColor = true;
            this.chkShowDatabaseName.Value = null;
            this.chkShowDatabaseName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarShowIP
            // 
            this.lblStarShowIP.AutoSize = true;
            this.lblStarShowIP.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarShowIP.Location = new System.Drawing.Point(283, 130);
            this.lblStarShowIP.Name = "lblStarShowIP";
            this.lblStarShowIP.Size = new System.Drawing.Size(14, 15);
            this.lblStarShowIP.TabIndex = 83;
            this.lblStarShowIP.Text = "*";
            this.c1ThemeController1.SetTheme(this.lblStarShowIP, "(default)");
            // 
            // chkShowIP
            // 
            this.chkShowIP.AutoSize = true;
            this.chkShowIP.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowIP.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowIP.ForeColor = System.Drawing.Color.Black;
            this.chkShowIP.Location = new System.Drawing.Point(19, 126);
            this.chkShowIP.Name = "chkShowIP";
            this.chkShowIP.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowIP.Size = new System.Drawing.Size(229, 22);
            this.chkShowIP.TabIndex = 82;
            this.chkShowIP.Text = "Display local IP address in status bar";
            this.c1ThemeController1.SetTheme(this.chkShowIP, "(default)");
            this.chkShowIP.UseVisualStyleBackColor = true;
            this.chkShowIP.Value = null;
            this.chkShowIP.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarShowVersion
            // 
            this.lblStarShowVersion.AutoSize = true;
            this.lblStarShowVersion.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarShowVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarShowVersion.Location = new System.Drawing.Point(383, 105);
            this.lblStarShowVersion.Name = "lblStarShowVersion";
            this.lblStarShowVersion.Size = new System.Drawing.Size(14, 15);
            this.lblStarShowVersion.TabIndex = 80;
            this.lblStarShowVersion.Text = "*";
            // 
            // chkShowVersion
            // 
            this.chkShowVersion.AutoSize = true;
            this.chkShowVersion.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowVersion.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowVersion.ForeColor = System.Drawing.Color.Black;
            this.chkShowVersion.Location = new System.Drawing.Point(19, 101);
            this.chkShowVersion.Name = "chkShowVersion";
            this.chkShowVersion.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowVersion.Size = new System.Drawing.Size(332, 22);
            this.chkShowVersion.TabIndex = 79;
            this.chkShowVersion.Text = "Display the JasonQuery version number  in the title bar";
            this.c1ThemeController1.SetTheme(this.chkShowVersion, "(default)");
            this.chkShowVersion.UseVisualStyleBackColor = true;
            this.chkShowVersion.Value = null;
            this.chkShowVersion.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
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
            this.cboLocalization.Items.Add("yyyy/MM/dd");
            this.cboLocalization.Items.Add("yyyy-MM-dd");
            this.cboLocalization.Items.Add("MM/dd/yyyy");
            this.cboLocalization.Items.Add("MM-dd-yyyy");
            this.cboLocalization.Items.Add("dd/MM/yyyy");
            this.cboLocalization.Items.Add("dd-MM-yyyy");
            this.cboLocalization.ItemsDisplayMember = "";
            this.cboLocalization.ItemsValueMember = "";
            this.cboLocalization.Location = new System.Drawing.Point(96, 20);
            this.cboLocalization.Name = "cboLocalization";
            this.cboLocalization.Size = new System.Drawing.Size(215, 21);
            this.cboLocalization.TabIndex = 78;
            this.cboLocalization.Tag = null;
            this.cboLocalization.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboLocalization, "(default)");
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboLocalization.SelectedIndexChanged += new System.EventHandler(this.cboLocalization_SelectedIndexChanged);
            // 
            // cboDateFormat
            // 
            this.cboDateFormat.AllowSpinLoop = false;
            this.cboDateFormat.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboDateFormat.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDateFormat.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboDateFormat.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboDateFormat.GapHeight = 0;
            this.cboDateFormat.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDateFormat.Items.Add("yyyy/MM/dd");
            this.cboDateFormat.Items.Add("yyyy-MM-dd");
            this.cboDateFormat.Items.Add("MM/dd/yyyy");
            this.cboDateFormat.Items.Add("MM-dd-yyyy");
            this.cboDateFormat.Items.Add("dd/MM/yyyy");
            this.cboDateFormat.Items.Add("dd-MM-yyyy");
            this.cboDateFormat.ItemsDisplayMember = "";
            this.cboDateFormat.ItemsValueMember = "";
            this.cboDateFormat.Location = new System.Drawing.Point(102, 46);
            this.cboDateFormat.Name = "cboDateFormat";
            this.cboDateFormat.Size = new System.Drawing.Size(106, 21);
            this.cboDateFormat.TabIndex = 77;
            this.cboDateFormat.Tag = null;
            this.cboDateFormat.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDateFormat, "(default)");
            this.cboDateFormat.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDateFormat.SelectedIndexChanged += new System.EventHandler(this.cboDateFormat_SelectedIndexChanged);
            // 
            // lblDateFormat
            // 
            this.lblDateFormat.AutoSize = true;
            this.lblDateFormat.Location = new System.Drawing.Point(17, 49);
            this.lblDateFormat.Name = "lblDateFormat";
            this.lblDateFormat.Size = new System.Drawing.Size(80, 16);
            this.lblDateFormat.TabIndex = 58;
            this.lblDateFormat.Text = "Date Format:";
            this.lblDateFormat.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblLocalization
            // 
            this.lblLocalization.AutoSize = true;
            this.lblLocalization.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblLocalization.Location = new System.Drawing.Point(17, 22);
            this.lblLocalization.Name = "lblLocalization";
            this.lblLocalization.Size = new System.Drawing.Size(78, 16);
            this.lblLocalization.TabIndex = 12;
            this.lblLocalization.Text = "Localization:";
            this.lblLocalization.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpMainFormTabVisualStyle
            // 
            this.grpMainFormTabVisualStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpMainFormTabVisualStyle.BackColor = System.Drawing.Color.Transparent;
            this.grpMainFormTabVisualStyle.Controls.Add(this.lblStarMainFormTab);
            this.grpMainFormTabVisualStyle.Controls.Add(this.chkMultiLine);
            this.grpMainFormTabVisualStyle.Controls.Add(this.chkHoverSelect);
            this.grpMainFormTabVisualStyle.Controls.Add(this.chkShowArrows);
            this.grpMainFormTabVisualStyle.Controls.Add(this.chkShrinkPages);
            this.grpMainFormTabVisualStyle.Controls.Add(this.grpMainFormWindowsState);
            this.grpMainFormTabVisualStyle.Controls.Add(this.chkTabBold);
            this.grpMainFormTabVisualStyle.Controls.Add(this.tabExample);
            this.grpMainFormTabVisualStyle.Controls.Add(this.grpAppearance);
            this.grpMainFormTabVisualStyle.Controls.Add(this.grpMainFormTabStyle);
            this.grpMainFormTabVisualStyle.Location = new System.Drawing.Point(13, 304);
            this.grpMainFormTabVisualStyle.Name = "grpMainFormTabVisualStyle";
            this.grpMainFormTabVisualStyle.Size = new System.Drawing.Size(1159, 180);
            this.grpMainFormTabVisualStyle.TabIndex = 39;
            this.grpMainFormTabVisualStyle.TabStop = false;
            this.grpMainFormTabVisualStyle.Text = "Startup && Window Behavior";
            // 
            // lblStarMainFormTab
            // 
            this.lblStarMainFormTab.AutoSize = true;
            this.lblStarMainFormTab.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarMainFormTab.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarMainFormTab.Location = new System.Drawing.Point(189, 3);
            this.lblStarMainFormTab.Name = "lblStarMainFormTab";
            this.lblStarMainFormTab.Size = new System.Drawing.Size(14, 15);
            this.lblStarMainFormTab.TabIndex = 70;
            this.lblStarMainFormTab.Text = "*";
            // 
            // chkMultiLine
            // 
            this.chkMultiLine.AutoSize = true;
            this.chkMultiLine.BackColor = System.Drawing.Color.Transparent;
            this.chkMultiLine.BorderColor = System.Drawing.Color.Transparent;
            this.chkMultiLine.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkMultiLine.ForeColor = System.Drawing.Color.Black;
            this.chkMultiLine.Location = new System.Drawing.Point(946, 58);
            this.chkMultiLine.Name = "chkMultiLine";
            this.chkMultiLine.Padding = new System.Windows.Forms.Padding(1);
            this.chkMultiLine.Size = new System.Drawing.Size(80, 22);
            this.chkMultiLine.TabIndex = 69;
            this.chkMultiLine.Text = "MultiLine";
            this.c1ThemeController1.SetTheme(this.chkMultiLine, "(default)");
            this.chkMultiLine.UseVisualStyleBackColor = true;
            this.chkMultiLine.Value = null;
            this.chkMultiLine.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkMultiLine.CheckedChanged += new System.EventHandler(this.chkTabVisualStyle_CheckedChanged);
            // 
            // chkHoverSelect
            // 
            this.chkHoverSelect.AutoSize = true;
            this.chkHoverSelect.BackColor = System.Drawing.Color.Transparent;
            this.chkHoverSelect.BorderColor = System.Drawing.Color.Transparent;
            this.chkHoverSelect.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkHoverSelect.ForeColor = System.Drawing.Color.Black;
            this.chkHoverSelect.Location = new System.Drawing.Point(946, 31);
            this.chkHoverSelect.Name = "chkHoverSelect";
            this.chkHoverSelect.Padding = new System.Windows.Forms.Padding(1);
            this.chkHoverSelect.Size = new System.Drawing.Size(99, 22);
            this.chkHoverSelect.TabIndex = 68;
            this.chkHoverSelect.Text = "Hover Select";
            this.c1ThemeController1.SetTheme(this.chkHoverSelect, "(default)");
            this.chkHoverSelect.UseVisualStyleBackColor = true;
            this.chkHoverSelect.Value = null;
            this.chkHoverSelect.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkHoverSelect.CheckedChanged += new System.EventHandler(this.chkTabVisualStyle_CheckedChanged);
            // 
            // chkShowArrows
            // 
            this.chkShowArrows.AutoSize = true;
            this.chkShowArrows.BackColor = System.Drawing.Color.Transparent;
            this.chkShowArrows.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowArrows.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowArrows.ForeColor = System.Drawing.Color.Black;
            this.chkShowArrows.Location = new System.Drawing.Point(779, 58);
            this.chkShowArrows.Name = "chkShowArrows";
            this.chkShowArrows.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowArrows.Size = new System.Drawing.Size(100, 22);
            this.chkShowArrows.TabIndex = 67;
            this.chkShowArrows.Text = "Show Arrows";
            this.c1ThemeController1.SetTheme(this.chkShowArrows, "(default)");
            this.chkShowArrows.UseVisualStyleBackColor = true;
            this.chkShowArrows.Value = null;
            this.chkShowArrows.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowArrows.CheckedChanged += new System.EventHandler(this.chkTabVisualStyle_CheckedChanged);
            // 
            // chkShrinkPages
            // 
            this.chkShrinkPages.AutoSize = true;
            this.chkShrinkPages.BackColor = System.Drawing.Color.Transparent;
            this.chkShrinkPages.BorderColor = System.Drawing.Color.Transparent;
            this.chkShrinkPages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShrinkPages.ForeColor = System.Drawing.Color.Black;
            this.chkShrinkPages.Location = new System.Drawing.Point(779, 31);
            this.chkShrinkPages.Name = "chkShrinkPages";
            this.chkShrinkPages.Padding = new System.Windows.Forms.Padding(1);
            this.chkShrinkPages.Size = new System.Drawing.Size(99, 22);
            this.chkShrinkPages.TabIndex = 66;
            this.chkShrinkPages.Text = "Shrink Pages";
            this.c1ThemeController1.SetTheme(this.chkShrinkPages, "(default)");
            this.chkShrinkPages.UseVisualStyleBackColor = true;
            this.chkShrinkPages.Value = null;
            this.chkShrinkPages.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShrinkPages.CheckedChanged += new System.EventHandler(this.chkTabVisualStyle_CheckedChanged);
            // 
            // grpMainFormWindowsState
            // 
            this.grpMainFormWindowsState.Controls.Add(this.rdoNormal);
            this.grpMainFormWindowsState.Controls.Add(this.rdoMaximized);
            this.grpMainFormWindowsState.Location = new System.Drawing.Point(13, 22);
            this.grpMainFormWindowsState.Name = "grpMainFormWindowsState";
            this.grpMainFormWindowsState.Size = new System.Drawing.Size(298, 58);
            this.grpMainFormWindowsState.TabIndex = 40;
            this.grpMainFormWindowsState.TabStop = false;
            this.grpMainFormWindowsState.Text = "Windows State";
            // 
            // rdoNormal
            // 
            this.rdoNormal.AutoSize = true;
            this.rdoNormal.Location = new System.Drawing.Point(161, 23);
            this.rdoNormal.Name = "rdoNormal";
            this.rdoNormal.Size = new System.Drawing.Size(68, 20);
            this.rdoNormal.TabIndex = 2;
            this.rdoNormal.Text = "Normal";
            this.rdoNormal.UseVisualStyleBackColor = true;
            // 
            // rdoMaximized
            // 
            this.rdoMaximized.AutoSize = true;
            this.rdoMaximized.Checked = true;
            this.rdoMaximized.Location = new System.Drawing.Point(20, 23);
            this.rdoMaximized.Name = "rdoMaximized";
            this.rdoMaximized.Size = new System.Drawing.Size(88, 20);
            this.rdoMaximized.TabIndex = 1;
            this.rdoMaximized.TabStop = true;
            this.rdoMaximized.Text = "Maximized";
            this.rdoMaximized.UseVisualStyleBackColor = true;
            // 
            // chkTabBold
            // 
            this.chkTabBold.AutoSize = true;
            this.chkTabBold.Checked = true;
            this.chkTabBold.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkTabBold.Location = new System.Drawing.Point(940, 70);
            this.chkTabBold.Name = "chkTabBold";
            this.chkTabBold.Size = new System.Drawing.Size(178, 20);
            this.chkTabBold.TabIndex = 11;
            this.chkTabBold.Text = "Active Tab is shown in bold";
            this.chkTabBold.UseVisualStyleBackColor = true;
            this.chkTabBold.Visible = false;
            this.chkTabBold.CheckedChanged += new System.EventHandler(this.chkTabVisualStyle_CheckedChanged);
            // 
            // tabExample
            // 
            this.tabExample.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabExample.BackColor = System.Drawing.Color.Transparent;
            this.tabExample.BoldSelectedPage = true;
            this.tabExample.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabExample.ForeColor = System.Drawing.Color.Empty;
            this.tabExample.IDEPixelArea = false;
            this.tabExample.Location = new System.Drawing.Point(13, 94);
            this.tabExample.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabExample.Name = "tabExample";
            this.tabExample.PositionTop = true;
            this.tabExample.SelectedIndex = 0;
            this.tabExample.SelectedTab = this.tabPage1;
            this.tabExample.ShowClose = true;
            this.tabExample.Size = new System.Drawing.Size(1134, 63);
            this.tabExample.TabIndex = 10;
            this.tabExample.TabPages.AddRange(new Crownwood.Magic.Controls.TabPage[] {
            this.tabPage1,
            this.tabPage2,
            this.tabPage3,
            this.tabPage4,
            this.tabPage5,
            this.tabPage6,
            this.tabPage7,
            this.tabPage8,
            this.tabPage9,
            this.tabPage10,
            this.tabPage11,
            this.tabPage12,
            this.tabPage13,
            this.tabPage14,
            this.tabPage15,
            this.tabPage16,
            this.tabPage17,
            this.tabPage18,
            this.tabPage19,
            this.tabPage20});
            this.tabExample.TextColor = System.Drawing.Color.Empty;
            this.tabExample.TextInactiveColor = System.Drawing.Color.Silver;
            this.c1ThemeController1.SetTheme(this.tabExample, "(default)");
            // 
            // tabPage1
            // 
            this.tabPage1.Location = new System.Drawing.Point(0, 27);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(1134, 36);
            this.tabPage1.TabIndex = 3;
            this.tabPage1.Visible = false;
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(0, 27);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Selected = false;
            this.tabPage2.Size = new System.Drawing.Size(1134, 36);
            this.tabPage2.TabIndex = 4;
            this.tabPage2.Visible = false;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(0, 27);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Selected = false;
            this.tabPage3.Size = new System.Drawing.Size(1134, 36);
            this.tabPage3.TabIndex = 5;
            this.tabPage3.Visible = false;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(0, 27);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Selected = false;
            this.tabPage4.Size = new System.Drawing.Size(1134, 36);
            this.tabPage4.TabIndex = 6;
            this.tabPage4.Visible = false;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(0, 27);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Selected = false;
            this.tabPage5.Size = new System.Drawing.Size(1134, 36);
            this.tabPage5.TabIndex = 7;
            this.tabPage5.Visible = false;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(0, 27);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Selected = false;
            this.tabPage6.Size = new System.Drawing.Size(1134, 36);
            this.tabPage6.TabIndex = 8;
            this.tabPage6.Visible = false;
            // 
            // tabPage7
            // 
            this.tabPage7.Location = new System.Drawing.Point(0, 27);
            this.tabPage7.Name = "tabPage7";
            this.tabPage7.Selected = false;
            this.tabPage7.Size = new System.Drawing.Size(1134, 36);
            this.tabPage7.TabIndex = 9;
            this.tabPage7.Visible = false;
            // 
            // tabPage8
            // 
            this.tabPage8.Location = new System.Drawing.Point(0, 27);
            this.tabPage8.Name = "tabPage8";
            this.tabPage8.Selected = false;
            this.tabPage8.Size = new System.Drawing.Size(1134, 36);
            this.tabPage8.TabIndex = 10;
            this.tabPage8.Visible = false;
            // 
            // tabPage9
            // 
            this.tabPage9.Location = new System.Drawing.Point(0, 27);
            this.tabPage9.Name = "tabPage9";
            this.tabPage9.Selected = false;
            this.tabPage9.Size = new System.Drawing.Size(1134, 36);
            this.tabPage9.TabIndex = 11;
            this.tabPage9.Visible = false;
            // 
            // tabPage10
            // 
            this.tabPage10.Location = new System.Drawing.Point(0, 27);
            this.tabPage10.Name = "tabPage10";
            this.tabPage10.Selected = false;
            this.tabPage10.Size = new System.Drawing.Size(1134, 36);
            this.tabPage10.TabIndex = 12;
            this.tabPage10.Visible = false;
            // 
            // tabPage11
            // 
            this.tabPage11.Location = new System.Drawing.Point(0, 27);
            this.tabPage11.Name = "tabPage11";
            this.tabPage11.Selected = false;
            this.tabPage11.Size = new System.Drawing.Size(1134, 36);
            this.tabPage11.TabIndex = 13;
            // 
            // tabPage12
            // 
            this.tabPage12.Location = new System.Drawing.Point(0, 27);
            this.tabPage12.Name = "tabPage12";
            this.tabPage12.Selected = false;
            this.tabPage12.Size = new System.Drawing.Size(1134, 36);
            this.tabPage12.TabIndex = 14;
            // 
            // tabPage13
            // 
            this.tabPage13.Location = new System.Drawing.Point(0, 27);
            this.tabPage13.Name = "tabPage13";
            this.tabPage13.Selected = false;
            this.tabPage13.Size = new System.Drawing.Size(1134, 36);
            this.tabPage13.TabIndex = 15;
            // 
            // tabPage14
            // 
            this.tabPage14.Location = new System.Drawing.Point(0, 27);
            this.tabPage14.Name = "tabPage14";
            this.tabPage14.Selected = false;
            this.tabPage14.Size = new System.Drawing.Size(1134, 36);
            this.tabPage14.TabIndex = 16;
            // 
            // tabPage15
            // 
            this.tabPage15.Location = new System.Drawing.Point(0, 27);
            this.tabPage15.Name = "tabPage15";
            this.tabPage15.Selected = false;
            this.tabPage15.Size = new System.Drawing.Size(1134, 36);
            this.tabPage15.TabIndex = 17;
            // 
            // tabPage16
            // 
            this.tabPage16.Location = new System.Drawing.Point(0, 27);
            this.tabPage16.Name = "tabPage16";
            this.tabPage16.Selected = false;
            this.tabPage16.Size = new System.Drawing.Size(1134, 36);
            this.tabPage16.TabIndex = 18;
            // 
            // tabPage17
            // 
            this.tabPage17.Location = new System.Drawing.Point(0, 27);
            this.tabPage17.Name = "tabPage17";
            this.tabPage17.Selected = false;
            this.tabPage17.Size = new System.Drawing.Size(1134, 36);
            this.tabPage17.TabIndex = 19;
            // 
            // tabPage18
            // 
            this.tabPage18.Location = new System.Drawing.Point(0, 27);
            this.tabPage18.Name = "tabPage18";
            this.tabPage18.Selected = false;
            this.tabPage18.Size = new System.Drawing.Size(1134, 36);
            this.tabPage18.TabIndex = 20;
            // 
            // tabPage19
            // 
            this.tabPage19.Location = new System.Drawing.Point(0, 27);
            this.tabPage19.Name = "tabPage19";
            this.tabPage19.Selected = false;
            this.tabPage19.Size = new System.Drawing.Size(1134, 36);
            this.tabPage19.TabIndex = 21;
            // 
            // tabPage20
            // 
            this.tabPage20.Location = new System.Drawing.Point(0, 27);
            this.tabPage20.Name = "tabPage20";
            this.tabPage20.Selected = false;
            this.tabPage20.Size = new System.Drawing.Size(1134, 36);
            this.tabPage20.TabIndex = 22;
            // 
            // grpAppearance
            // 
            this.grpAppearance.Controls.Add(this.rdoMultiBox);
            this.grpAppearance.Controls.Add(this.rdoMultiForm);
            this.grpAppearance.Controls.Add(this.rdoMultiDocument);
            this.grpAppearance.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpAppearance.Location = new System.Drawing.Point(491, 22);
            this.grpAppearance.Name = "grpAppearance";
            this.grpAppearance.Size = new System.Drawing.Size(255, 58);
            this.grpAppearance.TabIndex = 2;
            this.grpAppearance.TabStop = false;
            this.grpAppearance.Text = "Appearance";
            // 
            // rdoMultiBox
            // 
            this.rdoMultiBox.AutoSize = true;
            this.rdoMultiBox.Location = new System.Drawing.Point(143, 23);
            this.rdoMultiBox.Name = "rdoMultiBox";
            this.rdoMultiBox.Size = new System.Drawing.Size(75, 20);
            this.rdoMultiBox.TabIndex = 0;
            this.rdoMultiBox.Text = "MultiBox";
            this.c1ThemeController1.SetTheme(this.rdoMultiBox, "(default)");
            this.rdoMultiBox.CheckedChanged += new System.EventHandler(this.Style_CheckedChanged);
            // 
            // rdoMultiForm
            // 
            this.rdoMultiForm.AutoSize = true;
            this.rdoMultiForm.Location = new System.Drawing.Point(22, 23);
            this.rdoMultiForm.Name = "rdoMultiForm";
            this.rdoMultiForm.Size = new System.Drawing.Size(83, 20);
            this.rdoMultiForm.TabIndex = 0;
            this.rdoMultiForm.Text = "MultiForm";
            this.c1ThemeController1.SetTheme(this.rdoMultiForm, "(default)");
            this.rdoMultiForm.CheckedChanged += new System.EventHandler(this.Style_CheckedChanged);
            // 
            // rdoMultiDocument
            // 
            this.rdoMultiDocument.AutoSize = true;
            this.rdoMultiDocument.Location = new System.Drawing.Point(256, 23);
            this.rdoMultiDocument.Name = "rdoMultiDocument";
            this.rdoMultiDocument.Size = new System.Drawing.Size(113, 20);
            this.rdoMultiDocument.TabIndex = 0;
            this.rdoMultiDocument.Text = "MultiDocument";
            this.c1ThemeController1.SetTheme(this.rdoMultiDocument, "(default)");
            this.rdoMultiDocument.Visible = false;
            this.rdoMultiDocument.CheckedChanged += new System.EventHandler(this.Style_CheckedChanged);
            // 
            // grpMainFormTabStyle
            // 
            this.grpMainFormTabStyle.Controls.Add(this.rdoPlain);
            this.grpMainFormTabStyle.Controls.Add(this.rdoIDE);
            this.grpMainFormTabStyle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpMainFormTabStyle.Location = new System.Drawing.Point(320, 22);
            this.grpMainFormTabStyle.Name = "grpMainFormTabStyle";
            this.grpMainFormTabStyle.Size = new System.Drawing.Size(161, 58);
            this.grpMainFormTabStyle.TabIndex = 1;
            this.grpMainFormTabStyle.TabStop = false;
            this.grpMainFormTabStyle.Text = "Style";
            // 
            // rdoPlain
            // 
            this.rdoPlain.AutoSize = true;
            this.rdoPlain.Location = new System.Drawing.Point(88, 23);
            this.rdoPlain.Name = "rdoPlain";
            this.rdoPlain.Size = new System.Drawing.Size(52, 20);
            this.rdoPlain.TabIndex = 0;
            this.rdoPlain.Text = "Plain";
            this.c1ThemeController1.SetTheme(this.rdoPlain, "(default)");
            this.rdoPlain.CheckedChanged += new System.EventHandler(this.Style_CheckedChanged);
            // 
            // rdoIDE
            // 
            this.rdoIDE.AutoSize = true;
            this.rdoIDE.Location = new System.Drawing.Point(22, 23);
            this.rdoIDE.Name = "rdoIDE";
            this.rdoIDE.Size = new System.Drawing.Size(44, 20);
            this.rdoIDE.TabIndex = 0;
            this.rdoIDE.Text = "IDE";
            this.c1ThemeController1.SetTheme(this.rdoIDE, "(default)");
            this.rdoIDE.CheckedChanged += new System.EventHandler(this.Style_CheckedChanged);
            // 
            // grpOptionsTab
            // 
            this.grpOptionsTab.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOptionsTab.BackColor = System.Drawing.Color.Transparent;
            this.grpOptionsTab.Controls.Add(this.lblOptionsTabInactiveForeColor);
            this.grpOptionsTab.Controls.Add(this.pnlOptionsTabInactiveForeColor);
            this.grpOptionsTab.Controls.Add(this.c1DockingTab1);
            this.grpOptionsTab.Controls.Add(this.lblOptionsTabActiveForeColor);
            this.grpOptionsTab.Controls.Add(this.lblOptionsTabActiveBackColor);
            this.grpOptionsTab.Controls.Add(this.pnlOptionsTabActiveForeColor);
            this.grpOptionsTab.Controls.Add(this.pnlOptionsTabActiveBackColor);
            this.grpOptionsTab.Location = new System.Drawing.Point(566, 12);
            this.grpOptionsTab.Name = "grpOptionsTab";
            this.grpOptionsTab.Size = new System.Drawing.Size(606, 115);
            this.grpOptionsTab.TabIndex = 4;
            this.grpOptionsTab.TabStop = false;
            this.grpOptionsTab.Text = "Options Tab";
            // 
            // lblOptionsTabInactiveForeColor
            // 
            this.lblOptionsTabInactiveForeColor.AutoSize = true;
            this.lblOptionsTabInactiveForeColor.Location = new System.Drawing.Point(526, 26);
            this.lblOptionsTabInactiveForeColor.Name = "lblOptionsTabInactiveForeColor";
            this.lblOptionsTabInactiveForeColor.Size = new System.Drawing.Size(115, 16);
            this.lblOptionsTabInactiveForeColor.TabIndex = 9;
            this.lblOptionsTabInactiveForeColor.Text = "Inactive Fore Color:";
            this.lblOptionsTabInactiveForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlOptionsTabInactiveForeColor
            // 
            this.pnlOptionsTabInactiveForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlOptionsTabInactiveForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOptionsTabInactiveForeColor.Location = new System.Drawing.Point(644, 24);
            this.pnlOptionsTabInactiveForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlOptionsTabInactiveForeColor.Name = "pnlOptionsTabInactiveForeColor";
            this.pnlOptionsTabInactiveForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlOptionsTabInactiveForeColor.TabIndex = 10;
            this.pnlOptionsTabInactiveForeColor.Click += new System.EventHandler(this.pnlOptionsTabClick);
            // 
            // c1DockingTab1
            // 
            this.c1DockingTab1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab1.Controls.Add(this.tabGlobal2);
            this.c1DockingTab1.Controls.Add(this.tabGeneral2);
            this.c1DockingTab1.Controls.Add(this.tabQueryEditor2);
            this.c1DockingTab1.Controls.Add(this.tabAutoComplete2);
            this.c1DockingTab1.Controls.Add(this.tabAutoReplace2);
            this.c1DockingTab1.Controls.Add(this.tabDataGrid2);
            this.c1DockingTab1.Controls.Add(this.tabKeywords2);
            this.c1DockingTab1.Controls.Add(this.tabSqlToCode2);
            this.c1DockingTab1.Controls.Add(this.tabSqlFormatter2);
            this.c1DockingTab1.Location = new System.Drawing.Point(15, 53);
            this.c1DockingTab1.Name = "c1DockingTab1";
            this.c1DockingTab1.SelectedTabBold = true;
            this.c1DockingTab1.Size = new System.Drawing.Size(579, 50);
            this.c1DockingTab1.TabIndex = 8;
            this.c1DockingTab1.TabsSpacing = -1;
            this.c1ThemeController1.SetTheme(this.c1DockingTab1, "(default)");
            this.c1DockingTab1.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab1.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            // 
            // tabGlobal2
            // 
            this.tabGlobal2.Location = new System.Drawing.Point(1, 27);
            this.tabGlobal2.Name = "tabGlobal2";
            this.tabGlobal2.Size = new System.Drawing.Size(577, 22);
            this.tabGlobal2.TabIndex = 0;
            this.tabGlobal2.Text = "Global";
            // 
            // tabGeneral2
            // 
            this.tabGeneral2.Location = new System.Drawing.Point(1, 27);
            this.tabGeneral2.Name = "tabGeneral2";
            this.tabGeneral2.Size = new System.Drawing.Size(577, 22);
            this.tabGeneral2.TabIndex = 8;
            this.tabGeneral2.Text = "General";
            // 
            // tabQueryEditor2
            // 
            this.tabQueryEditor2.Location = new System.Drawing.Point(1, 27);
            this.tabQueryEditor2.Name = "tabQueryEditor2";
            this.tabQueryEditor2.Size = new System.Drawing.Size(577, 22);
            this.tabQueryEditor2.TabIndex = 1;
            this.tabQueryEditor2.Text = "Query Editor";
            // 
            // tabAutoComplete2
            // 
            this.tabAutoComplete2.Location = new System.Drawing.Point(1, 27);
            this.tabAutoComplete2.Name = "tabAutoComplete2";
            this.tabAutoComplete2.Size = new System.Drawing.Size(577, 22);
            this.tabAutoComplete2.TabIndex = 2;
            this.tabAutoComplete2.Text = "Auto Complete";
            // 
            // tabAutoReplace2
            // 
            this.tabAutoReplace2.Location = new System.Drawing.Point(1, 27);
            this.tabAutoReplace2.Name = "tabAutoReplace2";
            this.tabAutoReplace2.Size = new System.Drawing.Size(577, 22);
            this.tabAutoReplace2.TabIndex = 3;
            this.tabAutoReplace2.Text = "Auto Replace";
            // 
            // tabDataGrid2
            // 
            this.tabDataGrid2.Location = new System.Drawing.Point(1, 27);
            this.tabDataGrid2.Name = "tabDataGrid2";
            this.tabDataGrid2.Size = new System.Drawing.Size(577, 22);
            this.tabDataGrid2.TabIndex = 4;
            this.tabDataGrid2.Text = "Data Grid";
            // 
            // tabKeywords2
            // 
            this.tabKeywords2.Location = new System.Drawing.Point(1, 27);
            this.tabKeywords2.Name = "tabKeywords2";
            this.tabKeywords2.Size = new System.Drawing.Size(577, 22);
            this.tabKeywords2.TabIndex = 5;
            this.tabKeywords2.Text = "Keywords";
            // 
            // tabSqlToCode2
            // 
            this.tabSqlToCode2.Location = new System.Drawing.Point(1, 27);
            this.tabSqlToCode2.Name = "tabSqlToCode2";
            this.tabSqlToCode2.Size = new System.Drawing.Size(577, 22);
            this.tabSqlToCode2.TabIndex = 6;
            this.tabSqlToCode2.Text = "SQL to Code";
            // 
            // tabSqlFormatter2
            // 
            this.tabSqlFormatter2.Location = new System.Drawing.Point(1, 27);
            this.tabSqlFormatter2.Name = "tabSqlFormatter2";
            this.tabSqlFormatter2.Size = new System.Drawing.Size(577, 22);
            this.tabSqlFormatter2.TabIndex = 7;
            this.tabSqlFormatter2.Text = "SQL Formatter";
            // 
            // lblOptionsTabActiveForeColor
            // 
            this.lblOptionsTabActiveForeColor.AutoSize = true;
            this.lblOptionsTabActiveForeColor.Location = new System.Drawing.Point(15, 26);
            this.lblOptionsTabActiveForeColor.Name = "lblOptionsTabActiveForeColor";
            this.lblOptionsTabActiveForeColor.Size = new System.Drawing.Size(106, 16);
            this.lblOptionsTabActiveForeColor.TabIndex = 4;
            this.lblOptionsTabActiveForeColor.Text = "Active Fore Color:";
            this.lblOptionsTabActiveForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOptionsTabActiveBackColor
            // 
            this.lblOptionsTabActiveBackColor.AutoSize = true;
            this.lblOptionsTabActiveBackColor.Location = new System.Drawing.Point(264, 26);
            this.lblOptionsTabActiveBackColor.Name = "lblOptionsTabActiveBackColor";
            this.lblOptionsTabActiveBackColor.Size = new System.Drawing.Size(107, 16);
            this.lblOptionsTabActiveBackColor.TabIndex = 5;
            this.lblOptionsTabActiveBackColor.Text = "Active Back Color:";
            this.lblOptionsTabActiveBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // pnlOptionsTabActiveForeColor
            // 
            this.pnlOptionsTabActiveForeColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlOptionsTabActiveForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOptionsTabActiveForeColor.Location = new System.Drawing.Point(124, 24);
            this.pnlOptionsTabActiveForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlOptionsTabActiveForeColor.Name = "pnlOptionsTabActiveForeColor";
            this.pnlOptionsTabActiveForeColor.Size = new System.Drawing.Size(74, 21);
            this.pnlOptionsTabActiveForeColor.TabIndex = 6;
            this.pnlOptionsTabActiveForeColor.Click += new System.EventHandler(this.pnlOptionsTabClick);
            // 
            // pnlOptionsTabActiveBackColor
            // 
            this.pnlOptionsTabActiveBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlOptionsTabActiveBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlOptionsTabActiveBackColor.Location = new System.Drawing.Point(374, 24);
            this.pnlOptionsTabActiveBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlOptionsTabActiveBackColor.Name = "pnlOptionsTabActiveBackColor";
            this.pnlOptionsTabActiveBackColor.Size = new System.Drawing.Size(74, 21);
            this.pnlOptionsTabActiveBackColor.TabIndex = 7;
            this.pnlOptionsTabActiveBackColor.Click += new System.EventHandler(this.pnlOptionsTabClick);
            // 
            // grpCheckForUpdate
            // 
            this.grpCheckForUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCheckForUpdate.BackColor = System.Drawing.Color.Transparent;
            this.grpCheckForUpdate.Controls.Add(this.rdoCheckOnly);
            this.grpCheckForUpdate.Controls.Add(this.grpCheckOnly);
            this.grpCheckForUpdate.Controls.Add(this.rdoDonotCheck);
            this.grpCheckForUpdate.Location = new System.Drawing.Point(13, 12);
            this.grpCheckForUpdate.Name = "grpCheckForUpdate";
            this.grpCheckForUpdate.Size = new System.Drawing.Size(606, 123);
            this.grpCheckForUpdate.TabIndex = 17;
            this.grpCheckForUpdate.TabStop = false;
            this.grpCheckForUpdate.Text = "Check for Updates";
            // 
            // rdoCheckOnly
            // 
            this.rdoCheckOnly.AutoSize = true;
            this.rdoCheckOnly.Location = new System.Drawing.Point(20, 49);
            this.rdoCheckOnly.Name = "rdoCheckOnly";
            this.rdoCheckOnly.Size = new System.Drawing.Size(431, 20);
            this.rdoCheckOnly.TabIndex = 69;
            this.rdoCheckOnly.TabStop = true;
            this.rdoCheckOnly.Text = "Notify me when an update is available, but let me download it manually.";
            this.c1ThemeController1.SetTheme(this.rdoCheckOnly, "(default)");
            this.rdoCheckOnly.UseVisualStyleBackColor = true;
            this.rdoCheckOnly.CheckedChanged += new System.EventHandler(this.CheckForUpdates);
            // 
            // grpCheckOnly
            // 
            this.grpCheckOnly.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpCheckOnly.Controls.Add(this.rdoCheckForUpdates0);
            this.grpCheckOnly.Controls.Add(this.rdoCheckForUpdates1);
            this.grpCheckOnly.Controls.Add(this.rdoCheckForUpdates7);
            this.grpCheckOnly.Location = new System.Drawing.Point(11, 51);
            this.grpCheckOnly.Name = "grpCheckOnly";
            this.grpCheckOnly.Size = new System.Drawing.Size(582, 60);
            this.grpCheckOnly.TabIndex = 68;
            this.grpCheckOnly.TabStop = false;
            this.grpCheckOnly.Text = "  ";
            // 
            // rdoCheckForUpdates0
            // 
            this.rdoCheckForUpdates0.AutoSize = true;
            this.rdoCheckForUpdates0.Location = new System.Drawing.Point(24, 27);
            this.rdoCheckForUpdates0.Name = "rdoCheckForUpdates0";
            this.rdoCheckForUpdates0.Size = new System.Drawing.Size(82, 20);
            this.rdoCheckForUpdates0.TabIndex = 11;
            this.rdoCheckForUpdates0.TabStop = true;
            this.rdoCheckForUpdates0.Text = "on startup";
            this.rdoCheckForUpdates0.UseVisualStyleBackColor = true;
            // 
            // rdoCheckForUpdates1
            // 
            this.rdoCheckForUpdates1.AutoSize = true;
            this.rdoCheckForUpdates1.Location = new System.Drawing.Point(152, 27);
            this.rdoCheckForUpdates1.Name = "rdoCheckForUpdates1";
            this.rdoCheckForUpdates1.Size = new System.Drawing.Size(79, 20);
            this.rdoCheckForUpdates1.TabIndex = 12;
            this.rdoCheckForUpdates1.TabStop = true;
            this.rdoCheckForUpdates1.Text = "every day";
            this.rdoCheckForUpdates1.UseVisualStyleBackColor = true;
            // 
            // rdoCheckForUpdates7
            // 
            this.rdoCheckForUpdates7.AutoSize = true;
            this.rdoCheckForUpdates7.Location = new System.Drawing.Point(270, 27);
            this.rdoCheckForUpdates7.Name = "rdoCheckForUpdates7";
            this.rdoCheckForUpdates7.Size = new System.Drawing.Size(94, 20);
            this.rdoCheckForUpdates7.TabIndex = 17;
            this.rdoCheckForUpdates7.TabStop = true;
            this.rdoCheckForUpdates7.Text = "every 7 days";
            this.rdoCheckForUpdates7.UseVisualStyleBackColor = true;
            // 
            // rdoDonotCheck
            // 
            this.rdoDonotCheck.AutoSize = true;
            this.rdoDonotCheck.Location = new System.Drawing.Point(20, 22);
            this.rdoDonotCheck.Name = "rdoDonotCheck";
            this.rdoDonotCheck.Size = new System.Drawing.Size(428, 20);
            this.rdoDonotCheck.TabIndex = 67;
            this.rdoDonotCheck.TabStop = true;
            this.rdoDonotCheck.Text = "Don\'t automatically check for updates. I will check for updates manually.";
            this.rdoDonotCheck.UseVisualStyleBackColor = true;
            this.rdoDonotCheck.CheckedChanged += new System.EventHandler(this.CheckForUpdates);
            // 
            // txtHeightCode
            // 
            this.txtHeightCode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.txtHeightCode.BackColor = System.Drawing.SystemColors.Control;
            this.txtHeightCode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHeightCode.Location = new System.Drawing.Point(3, 97);
            this.txtHeightCode.Multiline = true;
            this.txtHeightCode.Name = "txtHeightCode";
            this.txtHeightCode.Size = new System.Drawing.Size(10, 594);
            this.txtHeightCode.TabIndex = 11;
            this.txtHeightCode.Visible = false;
            // 
            // txtHeightFormatter
            // 
            this.txtHeightFormatter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtHeightFormatter.BackColor = System.Drawing.SystemColors.Control;
            this.txtHeightFormatter.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtHeightFormatter.Location = new System.Drawing.Point(1217, 69);
            this.txtHeightFormatter.Multiline = true;
            this.txtHeightFormatter.Name = "txtHeightFormatter";
            this.txtHeightFormatter.Size = new System.Drawing.Size(10, 623);
            this.txtHeightFormatter.TabIndex = 21;
            this.txtHeightFormatter.Visible = false;
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.button1.Location = new System.Drawing.Point(203, 734);
            this.button1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(71, 36);
            this.button1.TabIndex = 4;
            this.button1.Text = "測試用";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Visible = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lblRequireRestart
            // 
            this.lblRequireRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblRequireRestart.AutoSize = true;
            this.lblRequireRestart.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRequireRestart.Location = new System.Drawing.Point(24, 744);
            this.lblRequireRestart.Name = "lblRequireRestart";
            this.lblRequireRestart.Size = new System.Drawing.Size(173, 16);
            this.lblRequireRestart.TabIndex = 9;
            this.lblRequireRestart.Text = "Require to restart JasonQuery";
            this.lblRequireRestart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblStarRequireToRestart
            // 
            this.lblStarRequireToRestart.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblStarRequireToRestart.AutoSize = true;
            this.lblStarRequireToRestart.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarRequireToRestart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarRequireToRestart.Location = new System.Drawing.Point(12, 746);
            this.lblStarRequireToRestart.Name = "lblStarRequireToRestart";
            this.lblStarRequireToRestart.Size = new System.Drawing.Size(14, 15);
            this.lblStarRequireToRestart.TabIndex = 8;
            this.lblStarRequireToRestart.Text = "*";
            // 
            // timerMother2Child
            // 
            this.timerMother2Child.Enabled = true;
            this.timerMother2Child.Tick += new System.EventHandler(this.tmrMother2Child_Tick);
            // 
            // c1DockingTab
            // 
            this.c1DockingTab.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.c1DockingTab.Controls.Add(this.tabGlobal);
            this.c1DockingTab.Controls.Add(this.tabGeneral);
            this.c1DockingTab.Controls.Add(this.tabQueryEditor);
            this.c1DockingTab.Controls.Add(this.tabAutoComplete);
            this.c1DockingTab.Controls.Add(this.tabAutoReplace);
            this.c1DockingTab.Controls.Add(this.tabDataGrid);
            this.c1DockingTab.Controls.Add(this.tabKeywords);
            this.c1DockingTab.Controls.Add(this.tabSqlToCode);
            this.c1DockingTab.Controls.Add(this.tabSqlFormatter);
            this.c1DockingTab.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1DockingTab.Location = new System.Drawing.Point(6, 6);
            this.c1DockingTab.Name = "c1DockingTab";
            this.c1DockingTab.SelectedTabBold = true;
            this.c1DockingTab.Size = new System.Drawing.Size(1212, 714);
            this.c1DockingTab.TabIndex = 24;
            this.c1DockingTab.TabsSpacing = -1;
            this.c1ThemeController1.SetTheme(this.c1DockingTab, "(default)");
            this.c1DockingTab.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab.SelectedIndexChanged += new System.EventHandler(this.c1DockingTab_SelectedIndexChanged);
            this.c1DockingTab.SelectedTabChanged += new System.EventHandler(this.c1DockingTab_SelectedTabChanged);
            this.c1DockingTab.SizeChanged += new System.EventHandler(this.c1DockingTab_SizeChanged);
            // 
            // tabGlobal
            // 
            this.tabGlobal.Controls.Add(this.c1DockingTab4);
            this.tabGlobal.Controls.Add(this.lblGlobalOverview);
            this.tabGlobal.Location = new System.Drawing.Point(1, 27);
            this.tabGlobal.Name = "tabGlobal";
            this.tabGlobal.Size = new System.Drawing.Size(1210, 686);
            this.tabGlobal.TabIndex = 8;
            this.tabGlobal.Text = "Global";
            // 
            // c1DockingTab4
            // 
            this.c1DockingTab4.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab4.Controls.Add(this.tabGlobalSettings);
            this.c1DockingTab4.Controls.Add(this.tabSafetySettings);
            this.c1DockingTab4.Controls.Add(this.tabUpdateSettings);
            this.c1DockingTab4.Location = new System.Drawing.Point(12, 36);
            this.c1DockingTab4.Name = "c1DockingTab4";
            this.c1DockingTab4.SelectedTabBold = true;
            this.c1DockingTab4.Size = new System.Drawing.Size(1186, 638);
            this.c1DockingTab4.TabIndex = 16;
            this.c1DockingTab4.TabsSpacing = -1;
            this.c1ThemeController1.SetTheme(this.c1DockingTab4, "(default)");
            this.c1DockingTab4.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab4.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            // 
            // tabGlobalSettings
            // 
            this.tabGlobalSettings.Controls.Add(this.lblStarBackup);
            this.tabGlobalSettings.Controls.Add(this.grpGeneral);
            this.tabGlobalSettings.Controls.Add(this.grpMainFormTabVisualStyle);
            this.tabGlobalSettings.Controls.Add(this.chkEnableBackup);
            this.tabGlobalSettings.Controls.Add(this.grpBackup);
            this.tabGlobalSettings.Controls.Add(this.grpMaxEntries);
            this.tabGlobalSettings.Controls.Add(this.grpOptionsTab);
            this.tabGlobalSettings.Location = new System.Drawing.Point(1, 27);
            this.tabGlobalSettings.Name = "tabGlobalSettings";
            this.tabGlobalSettings.Size = new System.Drawing.Size(1184, 610);
            this.tabGlobalSettings.TabIndex = 9;
            this.tabGlobalSettings.Text = "Global Settings";
            // 
            // tabSafetySettings
            // 
            this.tabSafetySettings.CaptionText = "Safety Settings";
            this.tabSafetySettings.Controls.Add(this.grpLargeBinaryDisplayStrategy);
            this.tabSafetySettings.Controls.Add(this.grpLargeTextDisplayStrategy);
            this.tabSafetySettings.Controls.Add(this.grpConnectionSafety);
            this.tabSafetySettings.Controls.Add(this.grpCommitRollbackIcon);
            this.tabSafetySettings.Location = new System.Drawing.Point(1, 27);
            this.tabSafetySettings.Name = "tabSafetySettings";
            this.tabSafetySettings.Size = new System.Drawing.Size(1184, 610);
            this.tabSafetySettings.TabIndex = 1;
            this.tabSafetySettings.Text = "Safety Settings";
            // 
            // grpLargeBinaryDisplayStrategy
            // 
            this.grpLargeBinaryDisplayStrategy.BackColor = System.Drawing.Color.Transparent;
            this.grpLargeBinaryDisplayStrategy.Controls.Add(this.lblLargeBinary);
            this.grpLargeBinaryDisplayStrategy.Location = new System.Drawing.Point(13, 254);
            this.grpLargeBinaryDisplayStrategy.Name = "grpLargeBinaryDisplayStrategy";
            this.grpLargeBinaryDisplayStrategy.Size = new System.Drawing.Size(1157, 70);
            this.grpLargeBinaryDisplayStrategy.TabIndex = 325;
            this.grpLargeBinaryDisplayStrategy.TabStop = false;
            this.grpLargeBinaryDisplayStrategy.Text = "Large Binary Field Display Strategy";
            // 
            // lblLargeBinary
            // 
            this.lblLargeBinary.AutoSize = true;
            this.lblLargeBinary.Location = new System.Drawing.Point(19, 24);
            this.lblLargeBinary.Name = "lblLargeBinary";
            this.lblLargeBinary.Size = new System.Drawing.Size(419, 32);
            this.lblLargeBinary.TabIndex = 87;
            this.lblLargeBinary.Text = "Show a summary only for binary fields, for example: (BLOB)(27,500 bytes).\r\nDouble" +
    "-click the cell to view the full content in the Cell Viewer.";
            this.lblLargeBinary.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblLargeBinary, "(default)");
            // 
            // grpLargeTextDisplayStrategy
            // 
            this.grpLargeTextDisplayStrategy.BackColor = System.Drawing.Color.Transparent;
            this.grpLargeTextDisplayStrategy.Controls.Add(this.lblLargeTextNote);
            this.grpLargeTextDisplayStrategy.Controls.Add(this.label2);
            this.grpLargeTextDisplayStrategy.Controls.Add(this.lblPreviewLength);
            this.grpLargeTextDisplayStrategy.Controls.Add(this.lblCharacters);
            this.grpLargeTextDisplayStrategy.Controls.Add(this.cboLargeTextPreviewLength);
            this.grpLargeTextDisplayStrategy.Controls.Add(this.lblLargeText);
            this.grpLargeTextDisplayStrategy.Location = new System.Drawing.Point(13, 126);
            this.grpLargeTextDisplayStrategy.Name = "grpLargeTextDisplayStrategy";
            this.grpLargeTextDisplayStrategy.Size = new System.Drawing.Size(1157, 121);
            this.grpLargeTextDisplayStrategy.TabIndex = 324;
            this.grpLargeTextDisplayStrategy.TabStop = false;
            this.grpLargeTextDisplayStrategy.Text = "Large Text Field Display Strategy";
            // 
            // lblLargeTextNote
            // 
            this.lblLargeTextNote.AutoSize = true;
            this.lblLargeTextNote.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblLargeTextNote.Location = new System.Drawing.Point(38, 93);
            this.lblLargeTextNote.Name = "lblLargeTextNote";
            this.lblLargeTextNote.Size = new System.Drawing.Size(474, 16);
            this.lblLargeTextNote.TabIndex = 88;
            this.lblLargeTextNote.Text = "Note: Loading longer text content may impact performance and UI responsiveness.";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Image = ((System.Drawing.Image)(resources.GetObject("label2.Image")));
            this.label2.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.label2.Location = new System.Drawing.Point(19, 92);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(28, 16);
            this.label2.TabIndex = 325;
            this.label2.Text = "       ";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblPreviewLength
            // 
            this.lblPreviewLength.AutoSize = true;
            this.lblPreviewLength.Location = new System.Drawing.Point(18, 65);
            this.lblPreviewLength.Name = "lblPreviewLength";
            this.lblPreviewLength.Size = new System.Drawing.Size(92, 16);
            this.lblPreviewLength.TabIndex = 324;
            this.lblPreviewLength.Text = "Preview length:";
            this.c1ThemeController1.SetTheme(this.lblPreviewLength, "(default)");
            // 
            // lblCharacters
            // 
            this.lblCharacters.AutoSize = true;
            this.lblCharacters.Location = new System.Drawing.Point(206, 65);
            this.lblCharacters.Name = "lblCharacters";
            this.lblCharacters.Size = new System.Drawing.Size(72, 16);
            this.lblCharacters.TabIndex = 323;
            this.lblCharacters.Text = "(characters)";
            this.c1ThemeController1.SetTheme(this.lblCharacters, "(default)");
            // 
            // cboLargeTextPreviewLength
            // 
            this.cboLargeTextPreviewLength.AllowSpinLoop = false;
            this.cboLargeTextPreviewLength.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboLargeTextPreviewLength.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboLargeTextPreviewLength.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboLargeTextPreviewLength.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboLargeTextPreviewLength.GapHeight = 0;
            this.cboLargeTextPreviewLength.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboLargeTextPreviewLength.Items.Add("50");
            this.cboLargeTextPreviewLength.Items.Add("100");
            this.cboLargeTextPreviewLength.Items.Add("200");
            this.cboLargeTextPreviewLength.Items.Add("500");
            this.cboLargeTextPreviewLength.Items.Add("1000");
            this.cboLargeTextPreviewLength.ItemsDisplayMember = "";
            this.cboLargeTextPreviewLength.ItemsValueMember = "";
            this.cboLargeTextPreviewLength.Location = new System.Drawing.Point(117, 63);
            this.cboLargeTextPreviewLength.Name = "cboLargeTextPreviewLength";
            this.cboLargeTextPreviewLength.Size = new System.Drawing.Size(83, 21);
            this.cboLargeTextPreviewLength.TabIndex = 87;
            this.cboLargeTextPreviewLength.Tag = null;
            this.cboLargeTextPreviewLength.Text = "50";
            this.cboLargeTextPreviewLength.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.cboLargeTextPreviewLength.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboLargeTextPreviewLength, "(default)");
            this.cboLargeTextPreviewLength.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblLargeText
            // 
            this.lblLargeText.AutoSize = true;
            this.lblLargeText.Location = new System.Drawing.Point(18, 24);
            this.lblLargeText.Name = "lblLargeText";
            this.lblLargeText.Size = new System.Drawing.Size(392, 32);
            this.lblLargeText.TabIndex = 86;
            this.lblLargeText.Text = "Show only the first selected number of characters for large text fields.\r\nDouble-" +
    "click the cell to view the full content in the Cell Viewer.";
            this.lblLargeText.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblLargeText, "(default)");
            // 
            // grpConnectionSafety
            // 
            this.grpConnectionSafety.BackColor = System.Drawing.Color.Transparent;
            this.grpConnectionSafety.Controls.Add(this.btnHelp_PendingWarning);
            this.grpConnectionSafety.Controls.Add(this.btnHelp_DisconnectAfterSelect);
            this.grpConnectionSafety.Controls.Add(this.lblReminder5Minutes);
            this.grpConnectionSafety.Controls.Add(this.chkPendingWarning);
            this.grpConnectionSafety.Controls.Add(this.chkDisconnectAfterSelect);
            this.grpConnectionSafety.Location = new System.Drawing.Point(13, 12);
            this.grpConnectionSafety.Name = "grpConnectionSafety";
            this.grpConnectionSafety.Size = new System.Drawing.Size(420, 107);
            this.grpConnectionSafety.TabIndex = 76;
            this.grpConnectionSafety.TabStop = false;
            this.grpConnectionSafety.Text = "Connection and Transaction Safety";
            // 
            // btnHelp_PendingWarning
            // 
            this.btnHelp_PendingWarning.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_PendingWarning.Image")));
            this.btnHelp_PendingWarning.Location = new System.Drawing.Point(203, 54);
            this.btnHelp_PendingWarning.Name = "btnHelp_PendingWarning";
            this.btnHelp_PendingWarning.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_PendingWarning.TabIndex = 323;
            this.c1ThemeController1.SetTheme(this.btnHelp_PendingWarning, "(default)");
            this.btnHelp_PendingWarning.UseVisualStyleBackColor = true;
            this.btnHelp_PendingWarning.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_PendingWarning.Click += new System.EventHandler(this.btnHelp_PendingWarning_Click);
            // 
            // btnHelp_DisconnectAfterSelect
            // 
            this.btnHelp_DisconnectAfterSelect.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_DisconnectAfterSelect.Image")));
            this.btnHelp_DisconnectAfterSelect.Location = new System.Drawing.Point(297, 25);
            this.btnHelp_DisconnectAfterSelect.Name = "btnHelp_DisconnectAfterSelect";
            this.btnHelp_DisconnectAfterSelect.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_DisconnectAfterSelect.TabIndex = 322;
            this.c1ThemeController1.SetTheme(this.btnHelp_DisconnectAfterSelect, "(default)");
            this.btnHelp_DisconnectAfterSelect.UseVisualStyleBackColor = true;
            this.btnHelp_DisconnectAfterSelect.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_DisconnectAfterSelect.Click += new System.EventHandler(this.btnHelp_DisconnectAfterSelect_Click);
            // 
            // lblReminder5Minutes
            // 
            this.lblReminder5Minutes.AutoSize = true;
            this.lblReminder5Minutes.Location = new System.Drawing.Point(34, 78);
            this.lblReminder5Minutes.Name = "lblReminder5Minutes";
            this.lblReminder5Minutes.Size = new System.Drawing.Size(306, 16);
            this.lblReminder5Minutes.TabIndex = 320;
            this.lblReminder5Minutes.Text = "Reminder 5 minutes after a pending transaction starts";
            this.c1ThemeController1.SetTheme(this.lblReminder5Minutes, "(default)");
            // 
            // chkPendingWarning
            // 
            this.chkPendingWarning.AutoSize = true;
            this.chkPendingWarning.BackColor = System.Drawing.Color.Transparent;
            this.chkPendingWarning.BorderColor = System.Drawing.Color.Transparent;
            this.chkPendingWarning.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPendingWarning.ForeColor = System.Drawing.Color.Black;
            this.chkPendingWarning.Location = new System.Drawing.Point(17, 53);
            this.chkPendingWarning.Name = "chkPendingWarning";
            this.chkPendingWarning.Padding = new System.Windows.Forms.Padding(1);
            this.chkPendingWarning.Size = new System.Drawing.Size(188, 22);
            this.chkPendingWarning.TabIndex = 319;
            this.chkPendingWarning.Text = "Pending transaction warning";
            this.c1ThemeController1.SetTheme(this.chkPendingWarning, "(default)");
            this.chkPendingWarning.UseVisualStyleBackColor = true;
            this.chkPendingWarning.Value = false;
            this.chkPendingWarning.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkDisconnectAfterSelect
            // 
            this.chkDisconnectAfterSelect.AutoSize = true;
            this.chkDisconnectAfterSelect.BackColor = System.Drawing.Color.Transparent;
            this.chkDisconnectAfterSelect.BorderColor = System.Drawing.Color.Transparent;
            this.chkDisconnectAfterSelect.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDisconnectAfterSelect.Checked = true;
            this.chkDisconnectAfterSelect.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDisconnectAfterSelect.Enabled = false;
            this.chkDisconnectAfterSelect.ForeColor = System.Drawing.Color.Black;
            this.chkDisconnectAfterSelect.Location = new System.Drawing.Point(17, 25);
            this.chkDisconnectAfterSelect.Name = "chkDisconnectAfterSelect";
            this.chkDisconnectAfterSelect.Padding = new System.Windows.Forms.Padding(1);
            this.chkDisconnectAfterSelect.Size = new System.Drawing.Size(281, 22);
            this.chkDisconnectAfterSelect.TabIndex = 318;
            this.chkDisconnectAfterSelect.Text = "Disconnect immediately after SELECT queries";
            this.c1ThemeController1.SetTheme(this.chkDisconnectAfterSelect, "(default)");
            this.chkDisconnectAfterSelect.UseVisualStyleBackColor = true;
            this.chkDisconnectAfterSelect.Value = true;
            this.chkDisconnectAfterSelect.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // tabUpdateSettings
            // 
            this.tabUpdateSettings.Controls.Add(this.grpUpdateInformationSource);
            this.tabUpdateSettings.Controls.Add(this.grpCheckForUpdate);
            this.tabUpdateSettings.Location = new System.Drawing.Point(1, 27);
            this.tabUpdateSettings.Name = "tabUpdateSettings";
            this.tabUpdateSettings.Size = new System.Drawing.Size(1184, 610);
            this.tabUpdateSettings.TabIndex = 10;
            this.tabUpdateSettings.Text = "Update Settings";
            // 
            // grpUpdateInformationSource
            // 
            this.grpUpdateInformationSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpUpdateInformationSource.BackColor = System.Drawing.Color.Transparent;
            this.grpUpdateInformationSource.Controls.Add(this.btnHelp_LocalUpdateFolder);
            this.grpUpdateInformationSource.Controls.Add(this.lblGitHubUpdateUrl);
            this.grpUpdateInformationSource.Controls.Add(this.lblOfficialWebsiteUpdateUrl);
            this.grpUpdateInformationSource.Controls.Add(this.btnLocalFolderOpenFolder);
            this.grpUpdateInformationSource.Controls.Add(this.btnBrowseLocalFolder);
            this.grpUpdateInformationSource.Controls.Add(this.txtLocalFolder);
            this.grpUpdateInformationSource.Controls.Add(this.rdoUpdateSourceLocal);
            this.grpUpdateInformationSource.Controls.Add(this.rdoUpdateSourceGitHub);
            this.grpUpdateInformationSource.Controls.Add(this.rdoUpdateSourceOfficialWebsite);
            this.grpUpdateInformationSource.Location = new System.Drawing.Point(13, 142);
            this.grpUpdateInformationSource.Name = "grpUpdateInformationSource";
            this.grpUpdateInformationSource.Size = new System.Drawing.Size(606, 167);
            this.grpUpdateInformationSource.TabIndex = 18;
            this.grpUpdateInformationSource.TabStop = false;
            this.grpUpdateInformationSource.Text = "Update Information Source";
            // 
            // btnHelp_LocalUpdateFolder
            // 
            this.btnHelp_LocalUpdateFolder.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnHelp_LocalUpdateFolder.Location = new System.Drawing.Point(188, 130);
            this.btnHelp_LocalUpdateFolder.Name = "btnHelp_LocalUpdateFolder";
            this.btnHelp_LocalUpdateFolder.Size = new System.Drawing.Size(22, 21);
            this.btnHelp_LocalUpdateFolder.TabIndex = 322;
            this.btnHelp_LocalUpdateFolder.Text = "?";
            this.btnHelp_LocalUpdateFolder.UseVisualStyleBackColor = true;
            this.btnHelp_LocalUpdateFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_LocalUpdateFolder.Click += new System.EventHandler(this.btnHelp_LocalUpdateFolder_Click);
            // 
            // lblGitHubUpdateUrl
            // 
            this.lblGitHubUpdateUrl.AutoSize = true;
            this.lblGitHubUpdateUrl.Location = new System.Drawing.Point(36, 103);
            this.lblGitHubUpdateUrl.Name = "lblGitHubUpdateUrl";
            this.lblGitHubUpdateUrl.Size = new System.Drawing.Size(481, 16);
            this.lblGitHubUpdateUrl.TabIndex = 321;
            this.lblGitHubUpdateUrl.Text = "https://api.github.com/repos/JasonHandwriting/JasonQuery/releases?per_page=20";
            // 
            // lblOfficialWebsiteUpdateUrl
            // 
            this.lblOfficialWebsiteUpdateUrl.AutoSize = true;
            this.lblOfficialWebsiteUpdateUrl.Location = new System.Drawing.Point(36, 49);
            this.lblOfficialWebsiteUpdateUrl.Name = "lblOfficialWebsiteUpdateUrl";
            this.lblOfficialWebsiteUpdateUrl.Size = new System.Drawing.Size(386, 16);
            this.lblOfficialWebsiteUpdateUrl.TabIndex = 320;
            this.lblOfficialWebsiteUpdateUrl.Text = "https://jasonquery.org/JasonQueryUpdate/jasonquery-update.json";
            // 
            // btnLocalFolderOpenFolder
            // 
            this.btnLocalFolderOpenFolder.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnLocalFolderOpenFolder.Image = ((System.Drawing.Image)(resources.GetObject("btnLocalFolderOpenFolder.Image")));
            this.btnLocalFolderOpenFolder.Location = new System.Drawing.Point(571, 130);
            this.btnLocalFolderOpenFolder.Name = "btnLocalFolderOpenFolder";
            this.btnLocalFolderOpenFolder.Size = new System.Drawing.Size(22, 21);
            this.btnLocalFolderOpenFolder.TabIndex = 319;
            this.btnLocalFolderOpenFolder.Tag = "1";
            this.btnLocalFolderOpenFolder.UseVisualStyleBackColor = true;
            this.btnLocalFolderOpenFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnLocalFolderOpenFolder.Click += new System.EventHandler(this.btnLocalFolderOpenFolder_Click);
            // 
            // btnBrowseLocalFolder
            // 
            this.btnBrowseLocalFolder.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowseLocalFolder.Location = new System.Drawing.Point(544, 130);
            this.btnBrowseLocalFolder.Name = "btnBrowseLocalFolder";
            this.btnBrowseLocalFolder.Size = new System.Drawing.Size(22, 21);
            this.btnBrowseLocalFolder.TabIndex = 318;
            this.btnBrowseLocalFolder.Tag = "1";
            this.btnBrowseLocalFolder.Text = "...";
            this.btnBrowseLocalFolder.UseVisualStyleBackColor = true;
            this.btnBrowseLocalFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseLocalFolder.Click += new System.EventHandler(this.btnBrowseLocalFolder_Click);
            // 
            // txtLocalFolder
            // 
            this.txtLocalFolder.BackColor = System.Drawing.SystemColors.Window;
            this.txtLocalFolder.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtLocalFolder.Location = new System.Drawing.Point(215, 130);
            this.txtLocalFolder.Name = "txtLocalFolder";
            this.txtLocalFolder.Size = new System.Drawing.Size(323, 21);
            this.txtLocalFolder.TabIndex = 317;
            this.txtLocalFolder.Tag = null;
            this.txtLocalFolder.TextDetached = true;
            this.txtLocalFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtLocalFolder.TextChanged += new System.EventHandler(this.txtLocalFolder_TextChanged);
            // 
            // rdoUpdateSourceLocal
            // 
            this.rdoUpdateSourceLocal.AutoSize = true;
            this.rdoUpdateSourceLocal.Location = new System.Drawing.Point(20, 130);
            this.rdoUpdateSourceLocal.Name = "rdoUpdateSourceLocal";
            this.rdoUpdateSourceLocal.Size = new System.Drawing.Size(168, 20);
            this.rdoUpdateSourceLocal.TabIndex = 2;
            this.rdoUpdateSourceLocal.Text = "Company Update Folder:";
            this.rdoUpdateSourceLocal.UseVisualStyleBackColor = true;
            this.rdoUpdateSourceLocal.CheckedChanged += new System.EventHandler(this.UpdateMetadataSource_CheckedChanged);
            // 
            // rdoUpdateSourceGitHub
            // 
            this.rdoUpdateSourceGitHub.AutoSize = true;
            this.rdoUpdateSourceGitHub.Enabled = false;
            this.rdoUpdateSourceGitHub.Location = new System.Drawing.Point(20, 76);
            this.rdoUpdateSourceGitHub.Name = "rdoUpdateSourceGitHub";
            this.rdoUpdateSourceGitHub.Size = new System.Drawing.Size(117, 20);
            this.rdoUpdateSourceGitHub.TabIndex = 1;
            this.rdoUpdateSourceGitHub.Text = "GitHub Releases";
            this.rdoUpdateSourceGitHub.UseVisualStyleBackColor = true;
            this.rdoUpdateSourceGitHub.CheckedChanged += new System.EventHandler(this.UpdateMetadataSource_CheckedChanged);
            // 
            // rdoUpdateSourceOfficialWebsite
            // 
            this.rdoUpdateSourceOfficialWebsite.AutoSize = true;
            this.rdoUpdateSourceOfficialWebsite.Checked = true;
            this.rdoUpdateSourceOfficialWebsite.Location = new System.Drawing.Point(20, 22);
            this.rdoUpdateSourceOfficialWebsite.Name = "rdoUpdateSourceOfficialWebsite";
            this.rdoUpdateSourceOfficialWebsite.Size = new System.Drawing.Size(239, 20);
            this.rdoUpdateSourceOfficialWebsite.TabIndex = 0;
            this.rdoUpdateSourceOfficialWebsite.TabStop = true;
            this.rdoUpdateSourceOfficialWebsite.Text = "JasonQuery Website (Recommended)";
            this.rdoUpdateSourceOfficialWebsite.UseVisualStyleBackColor = true;
            this.rdoUpdateSourceOfficialWebsite.CheckedChanged += new System.EventHandler(this.UpdateMetadataSource_CheckedChanged);
            // 
            // tabGeneral
            // 
            this.tabGeneral.Controls.Add(this.grpSqlHistory);
            this.tabGeneral.Controls.Add(this.grpDefaultDirectory);
            this.tabGeneral.Controls.Add(this.grpOpenSqlFile);
            this.tabGeneral.Controls.Add(this.grpColorTheme);
            this.tabGeneral.Location = new System.Drawing.Point(1, 27);
            this.tabGeneral.Name = "tabGeneral";
            this.tabGeneral.Size = new System.Drawing.Size(1210, 686);
            this.tabGeneral.TabIndex = 9;
            this.tabGeneral.Text = "General";
            // 
            // grpSqlHistory
            //
            this.grpSqlHistory.BackColor = System.Drawing.Color.Transparent;
            this.grpSqlHistory.Controls.Add(this.cboSqlHistoryRetentionDays);
            this.grpSqlHistory.Controls.Add(this.chkAutoDeleteSqlHistory);
            this.grpSqlHistory.Location = new System.Drawing.Point(12, 169);
            this.grpSqlHistory.Name = "grpSqlHistory";
            this.grpSqlHistory.Size = new System.Drawing.Size(680, 90);
            this.grpSqlHistory.TabIndex = 89;
            this.grpSqlHistory.TabStop = false;
            this.grpSqlHistory.Text = "SQL History";
            //
            // cboSqlHistoryRetentionDays
            //
            this.cboSqlHistoryRetentionDays.AllowSpinLoop = false;
            this.cboSqlHistoryRetentionDays.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboSqlHistoryRetentionDays.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboSqlHistoryRetentionDays.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboSqlHistoryRetentionDays.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboSqlHistoryRetentionDays.Enabled = false;
            this.cboSqlHistoryRetentionDays.GapHeight = 0;
            this.cboSqlHistoryRetentionDays.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboSqlHistoryRetentionDays.ItemsDisplayMember = "";
            this.cboSqlHistoryRetentionDays.ItemsValueMember = "";
            this.cboSqlHistoryRetentionDays.Location = new System.Drawing.Point(40, 54);
            this.cboSqlHistoryRetentionDays.Name = "cboSqlHistoryRetentionDays";
            this.cboSqlHistoryRetentionDays.Size = new System.Drawing.Size(106, 21);
            this.cboSqlHistoryRetentionDays.TabIndex = 78;
            this.cboSqlHistoryRetentionDays.Tag = null;
            this.cboSqlHistoryRetentionDays.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboSqlHistoryRetentionDays, "(default)");
            this.cboSqlHistoryRetentionDays.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            //
            // chkAutoDeleteSqlHistory
            //
            this.chkAutoDeleteSqlHistory.AutoSize = true;
            this.chkAutoDeleteSqlHistory.Location = new System.Drawing.Point(20, 26);
            this.chkAutoDeleteSqlHistory.Name = "chkAutoDeleteSqlHistory";
            this.chkAutoDeleteSqlHistory.Size = new System.Drawing.Size(379, 20);
            this.chkAutoDeleteSqlHistory.TabIndex = 0;
            this.chkAutoDeleteSqlHistory.Text = "Automatically delete SQL history for this connection older than:";
            this.c1ThemeController1.SetTheme(this.chkAutoDeleteSqlHistory, "(default)");
            this.chkAutoDeleteSqlHistory.UseVisualStyleBackColor = true;
            this.chkAutoDeleteSqlHistory.CheckedChanged += new System.EventHandler(this.chkAutoDeleteSqlHistory_CheckedChanged);
            //
            // grpDefaultDirectory
            // 
            this.grpDefaultDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDefaultDirectory.BackColor = System.Drawing.Color.Transparent;
            this.grpDefaultDirectory.Controls.Add(this.btnBrowseFavaritePath);
            this.grpDefaultDirectory.Controls.Add(this.txtFavoriteDirectory);
            this.grpDefaultDirectory.Controls.Add(this.lblStarDefaultDirectory);
            this.grpDefaultDirectory.Controls.Add(this.rdoFavoriteDirectory);
            this.grpDefaultDirectory.Controls.Add(this.rdoDefaultDirectory);
            this.grpDefaultDirectory.Location = new System.Drawing.Point(12, 400);
            this.grpDefaultDirectory.Name = "grpDefaultDirectory";
            this.grpDefaultDirectory.Size = new System.Drawing.Size(680, 88);
            this.grpDefaultDirectory.TabIndex = 70;
            this.grpDefaultDirectory.TabStop = false;
            this.grpDefaultDirectory.Text = "Default Directory (Open/Save)     ";
            this.grpDefaultDirectory.Visible = false;
            // 
            // btnBrowseFavaritePath
            // 
            this.btnBrowseFavaritePath.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowseFavaritePath.Location = new System.Drawing.Point(490, 52);
            this.btnBrowseFavaritePath.Name = "btnBrowseFavaritePath";
            this.btnBrowseFavaritePath.Size = new System.Drawing.Size(22, 22);
            this.btnBrowseFavaritePath.TabIndex = 81;
            this.btnBrowseFavaritePath.Text = "...";
            this.c1ThemeController1.SetTheme(this.btnBrowseFavaritePath, "(default)");
            this.btnBrowseFavaritePath.UseVisualStyleBackColor = true;
            this.btnBrowseFavaritePath.Visible = false;
            this.btnBrowseFavaritePath.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtFavoriteDirectory
            // 
            this.txtFavoriteDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFavoriteDirectory.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtFavoriteDirectory.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFavoriteDirectory.Enabled = false;
            this.txtFavoriteDirectory.Location = new System.Drawing.Point(148, 52);
            this.txtFavoriteDirectory.Name = "txtFavoriteDirectory";
            this.txtFavoriteDirectory.Size = new System.Drawing.Size(488, 21);
            this.txtFavoriteDirectory.TabIndex = 10;
            this.txtFavoriteDirectory.Tag = null;
            this.txtFavoriteDirectory.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtFavoriteDirectory, "(default)");
            this.txtFavoriteDirectory.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarDefaultDirectory
            // 
            this.lblStarDefaultDirectory.AutoSize = true;
            this.lblStarDefaultDirectory.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarDefaultDirectory.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarDefaultDirectory.Location = new System.Drawing.Point(182, 3);
            this.lblStarDefaultDirectory.Name = "lblStarDefaultDirectory";
            this.lblStarDefaultDirectory.Size = new System.Drawing.Size(14, 15);
            this.lblStarDefaultDirectory.TabIndex = 9;
            this.lblStarDefaultDirectory.Text = "*";
            // 
            // rdoFavoriteDirectory
            // 
            this.rdoFavoriteDirectory.AutoSize = true;
            this.rdoFavoriteDirectory.Location = new System.Drawing.Point(20, 51);
            this.rdoFavoriteDirectory.Name = "rdoFavoriteDirectory";
            this.rdoFavoriteDirectory.Size = new System.Drawing.Size(126, 20);
            this.rdoFavoriteDirectory.TabIndex = 2;
            this.rdoFavoriteDirectory.Text = "Favarite Directory:";
            this.rdoFavoriteDirectory.UseVisualStyleBackColor = true;
            // 
            // rdoDefaultDirectory
            // 
            this.rdoDefaultDirectory.AutoSize = true;
            this.rdoDefaultDirectory.Checked = true;
            this.rdoDefaultDirectory.Location = new System.Drawing.Point(20, 24);
            this.rdoDefaultDirectory.Name = "rdoDefaultDirectory";
            this.rdoDefaultDirectory.Size = new System.Drawing.Size(163, 20);
            this.rdoDefaultDirectory.TabIndex = 0;
            this.rdoDefaultDirectory.TabStop = true;
            this.rdoDefaultDirectory.Text = "System Default Directory";
            this.rdoDefaultDirectory.UseVisualStyleBackColor = true;
            // 
            // grpOpenSqlFile
            // 
            this.grpOpenSqlFile.BackColor = System.Drawing.Color.Transparent;
            this.grpOpenSqlFile.Controls.Add(this.btnClear2);
            this.grpOpenSqlFile.Controls.Add(this.btnClear1);
            this.grpOpenSqlFile.Controls.Add(this.btnSpecifiedSqlFile2);
            this.grpOpenSqlFile.Controls.Add(this.txtSpecifiedSQLFile2);
            this.grpOpenSqlFile.Controls.Add(this.lblFile2);
            this.grpOpenSqlFile.Controls.Add(this.btnSpecifiedSqlFile1);
            this.grpOpenSqlFile.Controls.Add(this.txtSpecifiedSQLFile1);
            this.grpOpenSqlFile.Controls.Add(this.lblFile1);
            this.grpOpenSqlFile.Location = new System.Drawing.Point(12, 73);
            this.grpOpenSqlFile.Name = "grpOpenSqlFile";
            this.grpOpenSqlFile.Size = new System.Drawing.Size(680, 90);
            this.grpOpenSqlFile.TabIndex = 69;
            this.grpOpenSqlFile.TabStop = false;
            this.grpOpenSqlFile.Text = "Open the specified SQL file on startup";
            // 
            // btnClear2
            // 
            this.btnClear2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear2.Image = ((System.Drawing.Image)(resources.GetObject("btnClear2.Image")));
            this.btnClear2.Location = new System.Drawing.Point(642, 55);
            this.btnClear2.Name = "btnClear2";
            this.btnClear2.Size = new System.Drawing.Size(22, 21);
            this.btnClear2.TabIndex = 88;
            this.btnClear2.Tag = "2";
            this.c1ThemeController1.SetTheme(this.btnClear2, "(default)");
            this.btnClear2.UseVisualStyleBackColor = true;
            this.btnClear2.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClear2.Click += new System.EventHandler(this.btnClearFile_Click);
            // 
            // btnClear1
            // 
            this.btnClear1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClear1.Image = ((System.Drawing.Image)(resources.GetObject("btnClear1.Image")));
            this.btnClear1.Location = new System.Drawing.Point(642, 24);
            this.btnClear1.Name = "btnClear1";
            this.btnClear1.Size = new System.Drawing.Size(22, 21);
            this.btnClear1.TabIndex = 87;
            this.btnClear1.Tag = "1";
            this.c1ThemeController1.SetTheme(this.btnClear1, "(default)");
            this.btnClear1.UseVisualStyleBackColor = true;
            this.btnClear1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClear1.Click += new System.EventHandler(this.btnClearFile_Click);
            // 
            // btnSpecifiedSqlFile2
            // 
            this.btnSpecifiedSqlFile2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSpecifiedSqlFile2.Location = new System.Drawing.Point(617, 55);
            this.btnSpecifiedSqlFile2.Name = "btnSpecifiedSqlFile2";
            this.btnSpecifiedSqlFile2.Size = new System.Drawing.Size(22, 21);
            this.btnSpecifiedSqlFile2.TabIndex = 86;
            this.btnSpecifiedSqlFile2.Tag = "2";
            this.btnSpecifiedSqlFile2.Text = "...";
            this.c1ThemeController1.SetTheme(this.btnSpecifiedSqlFile2, "(default)");
            this.btnSpecifiedSqlFile2.UseVisualStyleBackColor = true;
            this.btnSpecifiedSqlFile2.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSpecifiedSqlFile2.Click += new System.EventHandler(this.btnSpecifiedSQLFile_Click);
            // 
            // txtSpecifiedSQLFile2
            // 
            this.txtSpecifiedSQLFile2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtSpecifiedSQLFile2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSpecifiedSQLFile2.Location = new System.Drawing.Point(63, 55);
            this.txtSpecifiedSQLFile2.Name = "txtSpecifiedSQLFile2";
            this.txtSpecifiedSQLFile2.ReadOnly = true;
            this.txtSpecifiedSQLFile2.Size = new System.Drawing.Size(530, 21);
            this.txtSpecifiedSQLFile2.TabIndex = 85;
            this.txtSpecifiedSQLFile2.Tag = null;
            this.txtSpecifiedSQLFile2.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtSpecifiedSQLFile2, "(default)");
            this.txtSpecifiedSQLFile2.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblFile2
            // 
            this.lblFile2.AutoSize = true;
            this.lblFile2.Location = new System.Drawing.Point(14, 57);
            this.lblFile2.Name = "lblFile2";
            this.lblFile2.Size = new System.Drawing.Size(29, 16);
            this.lblFile2.TabIndex = 84;
            this.lblFile2.Text = "File:";
            this.c1ThemeController1.SetTheme(this.lblFile2, "(default)");
            // 
            // btnSpecifiedSqlFile1
            // 
            this.btnSpecifiedSqlFile1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnSpecifiedSqlFile1.Location = new System.Drawing.Point(617, 24);
            this.btnSpecifiedSqlFile1.Name = "btnSpecifiedSqlFile1";
            this.btnSpecifiedSqlFile1.Size = new System.Drawing.Size(22, 21);
            this.btnSpecifiedSqlFile1.TabIndex = 83;
            this.btnSpecifiedSqlFile1.Tag = "1";
            this.btnSpecifiedSqlFile1.Text = "...";
            this.c1ThemeController1.SetTheme(this.btnSpecifiedSqlFile1, "(default)");
            this.btnSpecifiedSqlFile1.UseVisualStyleBackColor = true;
            this.btnSpecifiedSqlFile1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSpecifiedSqlFile1.Click += new System.EventHandler(this.btnSpecifiedSQLFile_Click);
            // 
            // txtSpecifiedSQLFile1
            // 
            this.txtSpecifiedSQLFile1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtSpecifiedSQLFile1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSpecifiedSQLFile1.Location = new System.Drawing.Point(63, 24);
            this.txtSpecifiedSQLFile1.Name = "txtSpecifiedSQLFile1";
            this.txtSpecifiedSQLFile1.ReadOnly = true;
            this.txtSpecifiedSQLFile1.Size = new System.Drawing.Size(530, 21);
            this.txtSpecifiedSQLFile1.TabIndex = 82;
            this.txtSpecifiedSQLFile1.Tag = null;
            this.txtSpecifiedSQLFile1.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtSpecifiedSQLFile1, "(default)");
            this.txtSpecifiedSQLFile1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblFile1
            // 
            this.lblFile1.AutoSize = true;
            this.lblFile1.Location = new System.Drawing.Point(14, 26);
            this.lblFile1.Name = "lblFile1";
            this.lblFile1.Size = new System.Drawing.Size(29, 16);
            this.lblFile1.TabIndex = 0;
            this.lblFile1.Text = "File:";
            this.c1ThemeController1.SetTheme(this.lblFile1, "(default)");
            // 
            // tabQueryEditor
            // 
            this.tabQueryEditor.CaptionText = "Data Grid";
            this.tabQueryEditor.Controls.Add(this.grpStatementCompletion);
            this.tabQueryEditor.Controls.Add(this.grpSchemaInformation);
            this.tabQueryEditor.Controls.Add(this.grpEditorColors);
            this.tabQueryEditor.Controls.Add(this.grpPreferences);
            this.tabQueryEditor.Controls.Add(this.grpQueryEditorPreview);
            this.tabQueryEditor.Location = new System.Drawing.Point(1, 27);
            this.tabQueryEditor.Name = "tabQueryEditor";
            this.tabQueryEditor.Size = new System.Drawing.Size(1210, 686);
            this.tabQueryEditor.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabQueryEditor.TabIndex = 1;
            this.tabQueryEditor.Tag = "";
            this.tabQueryEditor.Text = "Query Editor";
            // 
            // grpStatementCompletion
            // 
            this.grpStatementCompletion.BackColor = System.Drawing.Color.Transparent;
            this.grpStatementCompletion.Controls.Add(this.btnHelp_AutoListMembers);
            this.grpStatementCompletion.Controls.Add(this.btnHelp_SavePoint);
            this.grpStatementCompletion.Controls.Add(this.chkSavePoint);
            this.grpStatementCompletion.Controls.Add(this.chkAutoListMembers);
            this.grpStatementCompletion.Location = new System.Drawing.Point(12, 114);
            this.grpStatementCompletion.Name = "grpStatementCompletion";
            this.grpStatementCompletion.Size = new System.Drawing.Size(317, 81);
            this.grpStatementCompletion.TabIndex = 26;
            this.grpStatementCompletion.TabStop = false;
            this.grpStatementCompletion.Text = "Statement Completion";
            // 
            // btnHelp_AutoListMembers
            // 
            this.btnHelp_AutoListMembers.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_AutoListMembers.Image")));
            this.btnHelp_AutoListMembers.Location = new System.Drawing.Point(152, 23);
            this.btnHelp_AutoListMembers.Name = "btnHelp_AutoListMembers";
            this.btnHelp_AutoListMembers.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_AutoListMembers.TabIndex = 83;
            this.c1ThemeController1.SetTheme(this.btnHelp_AutoListMembers, "(default)");
            this.btnHelp_AutoListMembers.UseVisualStyleBackColor = true;
            this.btnHelp_AutoListMembers.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_AutoListMembers.Click += new System.EventHandler(this.btnHelp_AutoListMembers_Click);
            // 
            // btnHelp_SavePoint
            // 
            this.btnHelp_SavePoint.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_SavePoint.Image")));
            this.btnHelp_SavePoint.Location = new System.Drawing.Point(249, 49);
            this.btnHelp_SavePoint.Name = "btnHelp_SavePoint";
            this.btnHelp_SavePoint.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_SavePoint.TabIndex = 82;
            this.c1ThemeController1.SetTheme(this.btnHelp_SavePoint, "(default)");
            this.btnHelp_SavePoint.UseVisualStyleBackColor = true;
            this.btnHelp_SavePoint.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_SavePoint.Click += new System.EventHandler(this.btnHelp_UseSavePoint_Click);
            // 
            // chkSavePoint
            // 
            this.chkSavePoint.AutoSize = true;
            this.chkSavePoint.Location = new System.Drawing.Point(36, 50);
            this.chkSavePoint.Name = "chkSavePoint";
            this.chkSavePoint.Size = new System.Drawing.Size(205, 20);
            this.chkSavePoint.TabIndex = 1;
            this.chkSavePoint.Text = "SavePoint (only for PostgreSQL)";
            this.c1ThemeController1.SetTheme(this.chkSavePoint, "(default)");
            this.chkSavePoint.UseVisualStyleBackColor = true;
            // 
            // chkAutoListMembers
            // 
            this.chkAutoListMembers.AutoSize = true;
            this.chkAutoListMembers.Location = new System.Drawing.Point(20, 24);
            this.chkAutoListMembers.Name = "chkAutoListMembers";
            this.chkAutoListMembers.Size = new System.Drawing.Size(131, 20);
            this.chkAutoListMembers.TabIndex = 0;
            this.chkAutoListMembers.Text = "Auto List Members";
            this.c1ThemeController1.SetTheme(this.chkAutoListMembers, "(default)");
            this.chkAutoListMembers.UseVisualStyleBackColor = true;
            // 
            // grpSchemaInformation
            // 
            this.grpSchemaInformation.BackColor = System.Drawing.Color.Transparent;
            this.grpSchemaInformation.Controls.Add(this.chkDefaultTabSchemaInformation);
            this.grpSchemaInformation.Controls.Add(this.lblStarShowColumnInfo);
            this.grpSchemaInformation.Controls.Add(this.lblStarSortByColumnName);
            this.grpSchemaInformation.Controls.Add(this.btnHelp_ShowColumnInfo);
            this.grpSchemaInformation.Controls.Add(this.chkShowColumnInfo);
            this.grpSchemaInformation.Controls.Add(this.chkSortByColumnName);
            this.grpSchemaInformation.Location = new System.Drawing.Point(12, 9);
            this.grpSchemaInformation.Name = "grpSchemaInformation";
            this.grpSchemaInformation.Size = new System.Drawing.Size(317, 100);
            this.grpSchemaInformation.TabIndex = 25;
            this.grpSchemaInformation.TabStop = false;
            this.grpSchemaInformation.Text = "Schema Browser";
            // 
            // chkDefaultTabSchemaInformation
            // 
            this.chkDefaultTabSchemaInformation.AutoSize = true;
            this.chkDefaultTabSchemaInformation.BackColor = System.Drawing.Color.Transparent;
            this.chkDefaultTabSchemaInformation.BorderColor = System.Drawing.Color.Transparent;
            this.chkDefaultTabSchemaInformation.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDefaultTabSchemaInformation.ForeColor = System.Drawing.Color.Black;
            this.chkDefaultTabSchemaInformation.Location = new System.Drawing.Point(20, 22);
            this.chkDefaultTabSchemaInformation.Name = "chkDefaultTabSchemaInformation";
            this.chkDefaultTabSchemaInformation.Padding = new System.Windows.Forms.Padding(1);
            this.chkDefaultTabSchemaInformation.Size = new System.Drawing.Size(241, 22);
            this.chkDefaultTabSchemaInformation.TabIndex = 108;
            this.chkDefaultTabSchemaInformation.Text = "Make Schema Browser the default tab";
            this.c1ThemeController1.SetTheme(this.chkDefaultTabSchemaInformation, "(default)");
            this.chkDefaultTabSchemaInformation.UseVisualStyleBackColor = true;
            this.chkDefaultTabSchemaInformation.Value = null;
            this.chkDefaultTabSchemaInformation.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblStarShowColumnInfo
            // 
            this.lblStarShowColumnInfo.AutoSize = true;
            this.lblStarShowColumnInfo.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarShowColumnInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarShowColumnInfo.Location = new System.Drawing.Point(229, 76);
            this.lblStarShowColumnInfo.Name = "lblStarShowColumnInfo";
            this.lblStarShowColumnInfo.Size = new System.Drawing.Size(14, 15);
            this.lblStarShowColumnInfo.TabIndex = 83;
            this.lblStarShowColumnInfo.Text = "*";
            // 
            // lblStarSortByColumnName
            // 
            this.lblStarSortByColumnName.AutoSize = true;
            this.lblStarSortByColumnName.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarSortByColumnName.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblStarSortByColumnName.Location = new System.Drawing.Point(171, 51);
            this.lblStarSortByColumnName.Name = "lblStarSortByColumnName";
            this.lblStarSortByColumnName.Size = new System.Drawing.Size(14, 15);
            this.lblStarSortByColumnName.TabIndex = 82;
            this.lblStarSortByColumnName.Text = "*";
            // 
            // btnHelp_ShowColumnInfo
            // 
            this.btnHelp_ShowColumnInfo.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ShowColumnInfo.Image")));
            this.btnHelp_ShowColumnInfo.Location = new System.Drawing.Point(200, 72);
            this.btnHelp_ShowColumnInfo.Name = "btnHelp_ShowColumnInfo";
            this.btnHelp_ShowColumnInfo.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_ShowColumnInfo.TabIndex = 81;
            this.c1ThemeController1.SetTheme(this.btnHelp_ShowColumnInfo, "(default)");
            this.btnHelp_ShowColumnInfo.UseVisualStyleBackColor = true;
            this.btnHelp_ShowColumnInfo.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ShowColumnInfo.Click += new System.EventHandler(this.btnHelp_ShowColumnInfo_Click);
            // 
            // chkShowColumnInfo
            // 
            this.chkShowColumnInfo.AutoSize = true;
            this.chkShowColumnInfo.BackColor = System.Drawing.Color.Transparent;
            this.chkShowColumnInfo.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowColumnInfo.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowColumnInfo.ForeColor = System.Drawing.Color.Black;
            this.chkShowColumnInfo.Location = new System.Drawing.Point(20, 72);
            this.chkShowColumnInfo.Name = "chkShowColumnInfo";
            this.chkShowColumnInfo.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowColumnInfo.Size = new System.Drawing.Size(175, 22);
            this.chkShowColumnInfo.TabIndex = 68;
            this.chkShowColumnInfo.Text = "Show Column Information";
            this.c1ThemeController1.SetTheme(this.chkShowColumnInfo, "(default)");
            this.chkShowColumnInfo.UseVisualStyleBackColor = true;
            this.chkShowColumnInfo.Value = null;
            this.chkShowColumnInfo.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkSortByColumnName
            // 
            this.chkSortByColumnName.AutoSize = true;
            this.chkSortByColumnName.BackColor = System.Drawing.Color.Transparent;
            this.chkSortByColumnName.BorderColor = System.Drawing.Color.Transparent;
            this.chkSortByColumnName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkSortByColumnName.ForeColor = System.Drawing.Color.Black;
            this.chkSortByColumnName.Location = new System.Drawing.Point(20, 47);
            this.chkSortByColumnName.Name = "chkSortByColumnName";
            this.chkSortByColumnName.Padding = new System.Windows.Forms.Padding(1);
            this.chkSortByColumnName.Size = new System.Drawing.Size(153, 22);
            this.chkSortByColumnName.TabIndex = 67;
            this.chkSortByColumnName.Text = "Sort by Column Name";
            this.c1ThemeController1.SetTheme(this.chkSortByColumnName, "(default)");
            this.chkSortByColumnName.UseVisualStyleBackColor = true;
            this.chkSortByColumnName.Value = null;
            this.chkSortByColumnName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // tabAutoComplete
            // 
            this.tabAutoComplete.CaptionText = "SQL History";
            this.tabAutoComplete.Controls.Add(this.btnHelp_EnableAutoComplete);
            this.tabAutoComplete.Controls.Add(this.lblStarAutoComplete);
            this.tabAutoComplete.Controls.Add(this.chkEnableAutoComplete);
            this.tabAutoComplete.Controls.Add(this.grpAutoComplete);
            this.tabAutoComplete.Location = new System.Drawing.Point(1, 27);
            this.tabAutoComplete.Name = "tabAutoComplete";
            this.tabAutoComplete.Size = new System.Drawing.Size(1210, 686);
            this.tabAutoComplete.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabAutoComplete.TabIndex = 2;
            this.tabAutoComplete.Tag = "";
            this.tabAutoComplete.Text = "Auto Complete";
            // 
            // tabAutoReplace
            // 
            this.tabAutoReplace.Controls.Add(this.btnHelp_EnableAutoReplace);
            this.tabAutoReplace.Controls.Add(this.lblStarAutoReplace);
            this.tabAutoReplace.Controls.Add(this.chkEnableAutoReplace);
            this.tabAutoReplace.Controls.Add(this.grpAutoReplace);
            this.tabAutoReplace.Location = new System.Drawing.Point(1, 27);
            this.tabAutoReplace.Name = "tabAutoReplace";
            this.tabAutoReplace.Size = new System.Drawing.Size(1210, 686);
            this.tabAutoReplace.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabAutoReplace.TabIndex = 3;
            this.tabAutoReplace.Text = "Auto Replace";
            // 
            // tabDataGrid
            // 
            this.tabDataGrid.Controls.Add(this.c1DockingTab2);
            this.tabDataGrid.Controls.Add(this.grpDataGridColor);
            this.tabDataGrid.Controls.Add(this.grpPreviewGrid);
            this.tabDataGrid.Location = new System.Drawing.Point(1, 27);
            this.tabDataGrid.Name = "tabDataGrid";
            this.tabDataGrid.Size = new System.Drawing.Size(1210, 686);
            this.tabDataGrid.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabDataGrid.TabIndex = 4;
            this.tabDataGrid.Text = "Data Grid (Query Result)";
            // 
            // c1DockingTab2
            // 
            this.c1DockingTab2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab2.Controls.Add(this.tabDataGridQueryResultBehavior);
            this.c1DockingTab2.Controls.Add(this.tabDataGridInteraction);
            this.c1DockingTab2.Controls.Add(this.tabDataGridAppearance);
            this.c1DockingTab2.Location = new System.Drawing.Point(12, 9);
            this.c1DockingTab2.Name = "c1DockingTab2";
            this.c1DockingTab2.SelectedTabBold = true;
            this.c1DockingTab2.Size = new System.Drawing.Size(1186, 273);
            this.c1DockingTab2.TabIndex = 15;
            this.c1DockingTab2.TabsSpacing = -1;
            this.c1ThemeController1.SetTheme(this.c1DockingTab2, "(default)");
            this.c1DockingTab2.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab2.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            // 
            // tabDataGridQueryResultBehavior
            // 
            this.tabDataGridQueryResultBehavior.Controls.Add(this.btnHelp_AppendQueryResult);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.btnHelp_PagedQuery);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.btnHelp_RawDataMode);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.chkPagedQuery);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.lblRowsPerPage);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.chkRawDataMode);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.cboRowsPerPage);
            this.tabDataGridQueryResultBehavior.Controls.Add(this.chkAppendQueryResult);
            this.tabDataGridQueryResultBehavior.Location = new System.Drawing.Point(1, 27);
            this.tabDataGridQueryResultBehavior.Name = "tabDataGridQueryResultBehavior";
            this.tabDataGridQueryResultBehavior.Size = new System.Drawing.Size(1184, 245);
            this.tabDataGridQueryResultBehavior.TabIndex = 9;
            this.tabDataGridQueryResultBehavior.Text = "Query Result Behavior";
            // 
            // btnHelp_AppendQueryResult
            // 
            this.btnHelp_AppendQueryResult.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_AppendQueryResult.Image")));
            this.btnHelp_AppendQueryResult.Location = new System.Drawing.Point(193, 63);
            this.btnHelp_AppendQueryResult.Name = "btnHelp_AppendQueryResult";
            this.btnHelp_AppendQueryResult.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_AppendQueryResult.TabIndex = 93;
            this.c1ThemeController1.SetTheme(this.btnHelp_AppendQueryResult, "(default)");
            this.btnHelp_AppendQueryResult.UseVisualStyleBackColor = true;
            this.btnHelp_AppendQueryResult.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_AppendQueryResult.Click += new System.EventHandler(this.btnHelp_AppendQueryResult_Click);
            // 
            // btnHelp_PagedQuery
            // 
            this.btnHelp_PagedQuery.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_PagedQuery.Image")));
            this.btnHelp_PagedQuery.Location = new System.Drawing.Point(178, 11);
            this.btnHelp_PagedQuery.Name = "btnHelp_PagedQuery";
            this.btnHelp_PagedQuery.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_PagedQuery.TabIndex = 94;
            this.c1ThemeController1.SetTheme(this.btnHelp_PagedQuery, "(default)");
            this.btnHelp_PagedQuery.UseVisualStyleBackColor = true;
            this.btnHelp_PagedQuery.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_PagedQuery.Click += new System.EventHandler(this.btnHelp_PagedQuery_Click);
            // 
            // btnHelp_RawDataMode
            // 
            this.btnHelp_RawDataMode.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_RawDataMode.Image")));
            this.btnHelp_RawDataMode.Location = new System.Drawing.Point(171, 89);
            this.btnHelp_RawDataMode.Name = "btnHelp_RawDataMode";
            this.btnHelp_RawDataMode.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_RawDataMode.TabIndex = 103;
            this.c1ThemeController1.SetTheme(this.btnHelp_RawDataMode, "(default)");
            this.btnHelp_RawDataMode.UseVisualStyleBackColor = true;
            this.btnHelp_RawDataMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_RawDataMode.Click += new System.EventHandler(this.btnHelp_RawDataMode_Click);
            // 
            // chkPagedQuery
            // 
            this.chkPagedQuery.AutoSize = true;
            this.chkPagedQuery.BackColor = System.Drawing.Color.Transparent;
            this.chkPagedQuery.BorderColor = System.Drawing.Color.Transparent;
            this.chkPagedQuery.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkPagedQuery.ForeColor = System.Drawing.Color.Black;
            this.chkPagedQuery.Location = new System.Drawing.Point(28, 11);
            this.chkPagedQuery.Name = "chkPagedQuery";
            this.chkPagedQuery.Padding = new System.Windows.Forms.Padding(1);
            this.chkPagedQuery.Size = new System.Drawing.Size(144, 22);
            this.chkPagedQuery.TabIndex = 88;
            this.chkPagedQuery.Text = "Enable Paged Query";
            this.c1ThemeController1.SetTheme(this.chkPagedQuery, "(default)");
            this.chkPagedQuery.UseVisualStyleBackColor = true;
            this.chkPagedQuery.Value = null;
            this.chkPagedQuery.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkPagedQuery.CheckedChanged += new System.EventHandler(this.chkPagedQuery_CheckedChanged);
            // 
            // lblRowsPerPage
            // 
            this.lblRowsPerPage.AutoSize = true;
            this.lblRowsPerPage.BackColor = System.Drawing.Color.Transparent;
            this.lblRowsPerPage.Enabled = false;
            this.lblRowsPerPage.Location = new System.Drawing.Point(45, 40);
            this.lblRowsPerPage.Name = "lblRowsPerPage";
            this.lblRowsPerPage.Size = new System.Drawing.Size(93, 16);
            this.lblRowsPerPage.TabIndex = 89;
            this.lblRowsPerPage.Text = "Rows Per Page:";
            this.lblRowsPerPage.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // chkRawDataMode
            // 
            this.chkRawDataMode.AutoSize = true;
            this.chkRawDataMode.BackColor = System.Drawing.Color.Transparent;
            this.chkRawDataMode.BorderColor = System.Drawing.Color.Transparent;
            this.chkRawDataMode.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkRawDataMode.ForeColor = System.Drawing.Color.Black;
            this.chkRawDataMode.Location = new System.Drawing.Point(28, 89);
            this.chkRawDataMode.Name = "chkRawDataMode";
            this.chkRawDataMode.Padding = new System.Windows.Forms.Padding(1);
            this.chkRawDataMode.Size = new System.Drawing.Size(120, 22);
            this.chkRawDataMode.TabIndex = 102;
            this.chkRawDataMode.Text = "Raw Data Mode";
            this.c1ThemeController1.SetTheme(this.chkRawDataMode, "(default)");
            this.chkRawDataMode.UseVisualStyleBackColor = true;
            this.chkRawDataMode.Value = null;
            this.chkRawDataMode.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboRowsPerPage
            // 
            this.cboRowsPerPage.AllowSpinLoop = false;
            this.cboRowsPerPage.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboRowsPerPage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboRowsPerPage.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboRowsPerPage.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboRowsPerPage.GapHeight = 0;
            this.cboRowsPerPage.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboRowsPerPage.Items.Add("100");
            this.cboRowsPerPage.Items.Add("200");
            this.cboRowsPerPage.Items.Add("300");
            this.cboRowsPerPage.Items.Add("400");
            this.cboRowsPerPage.Items.Add("500");
            this.cboRowsPerPage.Items.Add("1000");
            this.cboRowsPerPage.Items.Add("2000");
            this.cboRowsPerPage.Items.Add("5000");
            this.cboRowsPerPage.ItemsDisplayMember = "";
            this.cboRowsPerPage.ItemsValueMember = "";
            this.cboRowsPerPage.Location = new System.Drawing.Point(166, 38);
            this.cboRowsPerPage.Name = "cboRowsPerPage";
            this.cboRowsPerPage.Size = new System.Drawing.Size(60, 21);
            this.cboRowsPerPage.TabIndex = 90;
            this.cboRowsPerPage.Tag = null;
            this.cboRowsPerPage.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboRowsPerPage, "(default)");
            this.cboRowsPerPage.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // chkAppendQueryResult
            // 
            this.chkAppendQueryResult.AutoSize = true;
            this.chkAppendQueryResult.BackColor = System.Drawing.Color.Transparent;
            this.chkAppendQueryResult.BorderColor = System.Drawing.Color.Transparent;
            this.chkAppendQueryResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkAppendQueryResult.ForeColor = System.Drawing.Color.Black;
            this.chkAppendQueryResult.Location = new System.Drawing.Point(48, 63);
            this.chkAppendQueryResult.Name = "chkAppendQueryResult";
            this.chkAppendQueryResult.Padding = new System.Windows.Forms.Padding(1);
            this.chkAppendQueryResult.Size = new System.Drawing.Size(153, 22);
            this.chkAppendQueryResult.TabIndex = 92;
            this.chkAppendQueryResult.Text = "Append Query Results";
            this.c1ThemeController1.SetTheme(this.chkAppendQueryResult, "(default)");
            this.chkAppendQueryResult.UseVisualStyleBackColor = true;
            this.chkAppendQueryResult.Value = null;
            this.chkAppendQueryResult.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // tabDataGridInteraction
            // 
            this.tabDataGridInteraction.Controls.Add(this.chkSetFocusAfterQuery);
            this.tabDataGridInteraction.Controls.Add(this.chkCtrlMouseWheel);
            this.tabDataGridInteraction.Controls.Add(this.chkShowFilterRow);
            this.tabDataGridInteraction.Controls.Add(this.chkResize);
            this.tabDataGridInteraction.Controls.Add(this.cboMaxWidth);
            this.tabDataGridInteraction.Controls.Add(this.lblMaxWidth);
            this.tabDataGridInteraction.Controls.Add(this.lblStarDirection);
            this.tabDataGridInteraction.Controls.Add(this.chkDirection);
            this.tabDataGridInteraction.Controls.Add(this.cboDirection);
            this.tabDataGridInteraction.Location = new System.Drawing.Point(1, 27);
            this.tabDataGridInteraction.Name = "tabDataGridInteraction";
            this.tabDataGridInteraction.Size = new System.Drawing.Size(1184, 245);
            this.tabDataGridInteraction.TabIndex = 1;
            this.tabDataGridInteraction.Text = "Interaction";
            // 
            // lblStarDirection
            // 
            this.lblStarDirection.AutoSize = true;
            this.lblStarDirection.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblStarDirection.Location = new System.Drawing.Point(328, 16);
            this.lblStarDirection.Name = "lblStarDirection";
            this.lblStarDirection.Size = new System.Drawing.Size(14, 15);
            this.lblStarDirection.TabIndex = 106;
            this.lblStarDirection.Text = "*";
            this.c1ThemeController1.SetTheme(this.lblStarDirection, "(default)");
            this.lblStarDirection.Visible = false;
            // 
            // chkDirection
            // 
            this.chkDirection.AutoSize = true;
            this.chkDirection.BackColor = System.Drawing.Color.Transparent;
            this.chkDirection.BorderColor = System.Drawing.Color.Transparent;
            this.chkDirection.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkDirection.Checked = true;
            this.chkDirection.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDirection.Enabled = false;
            this.chkDirection.ForeColor = System.Drawing.Color.Black;
            this.chkDirection.Location = new System.Drawing.Point(28, 11);
            this.chkDirection.Name = "chkDirection";
            this.chkDirection.Padding = new System.Windows.Forms.Padding(1);
            this.chkDirection.Size = new System.Drawing.Size(295, 22);
            this.chkDirection.TabIndex = 105;
            this.chkDirection.Text = "The direction of movement after pressing Enter:";
            this.c1ThemeController1.SetTheme(this.chkDirection, "(default)");
            this.chkDirection.UseVisualStyleBackColor = true;
            this.chkDirection.Value = true;
            this.chkDirection.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // cboDirection
            // 
            this.cboDirection.AllowSpinLoop = false;
            this.cboDirection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboDirection.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboDirection.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboDirection.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboDirection.GapHeight = 0;
            this.cboDirection.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboDirection.Items.Add("100");
            this.cboDirection.Items.Add("200");
            this.cboDirection.Items.Add("300");
            this.cboDirection.Items.Add("400");
            this.cboDirection.Items.Add("500");
            this.cboDirection.Items.Add("1000");
            this.cboDirection.Items.Add("2000");
            this.cboDirection.Items.Add("5000");
            this.cboDirection.ItemsDisplayMember = "";
            this.cboDirection.ItemsValueMember = "";
            this.cboDirection.Location = new System.Drawing.Point(48, 37);
            this.cboDirection.Name = "cboDirection";
            this.cboDirection.Size = new System.Drawing.Size(60, 21);
            this.cboDirection.TabIndex = 104;
            this.cboDirection.Tag = null;
            this.cboDirection.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboDirection, "(default)");
            this.cboDirection.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDirection.SelectedIndexChanged += new System.EventHandler(this.cboDirection_SelectedIndexChanged);
            // 
            // tabDataGridAppearance
            // 
            this.tabDataGridAppearance.Controls.Add(this.cboResultCopyFieldSeparator);
            this.tabDataGridAppearance.Controls.Add(this.lblResultCopyFieldSeparator);
            this.tabDataGridAppearance.Controls.Add(this.btnHelp_ColumnComment);
            this.tabDataGridAppearance.Controls.Add(this.cboGridRowHeightResizing);
            this.tabDataGridAppearance.Controls.Add(this.cboGridFontSize);
            this.tabDataGridAppearance.Controls.Add(this.cboGridVisualStyle);
            this.tabDataGridAppearance.Controls.Add(this.cboResultCopyQuotingWith);
            this.tabDataGridAppearance.Controls.Add(this.chkShowColumnComment);
            this.tabDataGridAppearance.Controls.Add(this.cboGridFontPicker);
            this.tabDataGridAppearance.Controls.Add(this.chkShowGroupingRow);
            this.tabDataGridAppearance.Controls.Add(this.lblGridVisualStyle);
            this.tabDataGridAppearance.Controls.Add(this.lblGridFontSize);
            this.tabDataGridAppearance.Controls.Add(this.chkShowColumnType);
            this.tabDataGridAppearance.Controls.Add(this.lblGridFontName);
            this.tabDataGridAppearance.Controls.Add(this.lblResultCopyQuotingWith);
            this.tabDataGridAppearance.Controls.Add(this.grpNullValueStyle);
            this.tabDataGridAppearance.Controls.Add(this.lblGridRowHeightResizing);
            this.tabDataGridAppearance.Location = new System.Drawing.Point(1, 27);
            this.tabDataGridAppearance.Name = "tabDataGridAppearance";
            this.tabDataGridAppearance.Size = new System.Drawing.Size(1184, 245);
            this.tabDataGridAppearance.TabIndex = 0;
            this.tabDataGridAppearance.Text = "Appearance";
            // 
            // cboResultCopyFieldSeparator
            // 
            this.cboResultCopyFieldSeparator.AllowSpinLoop = false;
            this.cboResultCopyFieldSeparator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboResultCopyFieldSeparator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboResultCopyFieldSeparator.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboResultCopyFieldSeparator.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboResultCopyFieldSeparator.GapHeight = 0;
            this.cboResultCopyFieldSeparator.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboResultCopyFieldSeparator.Items.Add(",");
            this.cboResultCopyFieldSeparator.Items.Add(";");
            this.cboResultCopyFieldSeparator.Items.Add("|");
            this.cboResultCopyFieldSeparator.ItemsDisplayMember = "";
            this.cboResultCopyFieldSeparator.ItemsValueMember = "";
            this.cboResultCopyFieldSeparator.Location = new System.Drawing.Point(464, 163);
            this.cboResultCopyFieldSeparator.Name = "cboResultCopyFieldSeparator";
            this.cboResultCopyFieldSeparator.Size = new System.Drawing.Size(48, 21);
            this.cboResultCopyFieldSeparator.TabIndex = 103;
            this.cboResultCopyFieldSeparator.Tag = null;
            this.cboResultCopyFieldSeparator.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboResultCopyFieldSeparator, "(default)");
            this.cboResultCopyFieldSeparator.Visible = false;
            this.cboResultCopyFieldSeparator.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblResultCopyFieldSeparator
            // 
            this.lblResultCopyFieldSeparator.BackColor = System.Drawing.Color.Transparent;
            this.lblResultCopyFieldSeparator.Location = new System.Drawing.Point(260, 164);
            this.lblResultCopyFieldSeparator.Name = "lblResultCopyFieldSeparator";
            this.lblResultCopyFieldSeparator.Size = new System.Drawing.Size(200, 16);
            this.lblResultCopyFieldSeparator.TabIndex = 102;
            this.lblResultCopyFieldSeparator.Text = "Result Copy Field Separator:";
            this.lblResultCopyFieldSeparator.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblResultCopyFieldSeparator.Visible = false;
            // 
            // tabKeywords
            // 
            this.tabKeywords.Controls.Add(this.splitContainer4);
            this.tabKeywords.Location = new System.Drawing.Point(1, 27);
            this.tabKeywords.Name = "tabKeywords";
            this.tabKeywords.Size = new System.Drawing.Size(1210, 686);
            this.tabKeywords.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabKeywords.TabIndex = 5;
            this.tabKeywords.Text = "Keywords";
            // 
            // splitContainer4
            // 
            this.splitContainer4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer4.Location = new System.Drawing.Point(0, 0);
            this.splitContainer4.Name = "splitContainer4";
            this.splitContainer4.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer4.Panel1
            // 
            this.splitContainer4.Panel1.Controls.Add(this.splitContainer5);
            // 
            // splitContainer4.Panel2
            // 
            this.splitContainer4.Panel2.Controls.Add(this.splitContainer6);
            this.splitContainer4.Size = new System.Drawing.Size(1210, 686);
            this.splitContainer4.SplitterDistance = 338;
            this.splitContainer4.TabIndex = 0;
            // 
            // splitContainer5
            // 
            this.splitContainer5.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer5.Location = new System.Drawing.Point(0, 0);
            this.splitContainer5.Name = "splitContainer5";
            this.splitContainer5.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer5.Panel1
            // 
            this.splitContainer5.Panel1.Controls.Add(this.grpOperatorKeywords);
            // 
            // splitContainer5.Panel2
            // 
            this.splitContainer5.Panel2.Controls.Add(this.grpBuiltInFunctions);
            this.splitContainer5.Size = new System.Drawing.Size(1210, 338);
            this.splitContainer5.SplitterDistance = 162;
            this.splitContainer5.TabIndex = 0;
            // 
            // splitContainer6
            // 
            this.splitContainer6.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer6.Location = new System.Drawing.Point(0, 0);
            this.splitContainer6.Name = "splitContainer6";
            this.splitContainer6.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer6.Panel1
            // 
            this.splitContainer6.Panel1.Controls.Add(this.grpBuiltInKeywords);
            // 
            // splitContainer6.Panel2
            // 
            this.splitContainer6.Panel2.Controls.Add(this.grpUserDefinedKeywords);
            this.splitContainer6.Size = new System.Drawing.Size(1210, 344);
            this.splitContainer6.SplitterDistance = 166;
            this.splitContainer6.TabIndex = 0;
            // 
            // tabSqlToCode
            // 
            this.tabSqlToCode.Controls.Add(this.grpSqlToCode);
            this.tabSqlToCode.Location = new System.Drawing.Point(1, 27);
            this.tabSqlToCode.Name = "tabSqlToCode";
            this.tabSqlToCode.Size = new System.Drawing.Size(1210, 686);
            this.tabSqlToCode.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabSqlToCode.TabIndex = 6;
            this.tabSqlToCode.Text = "SQL to Code";
            // 
            // tabSqlFormatter
            // 
            this.tabSqlFormatter.Controls.Add(this.grpSqlFormatter);
            this.tabSqlFormatter.Location = new System.Drawing.Point(1, 27);
            this.tabSqlFormatter.Name = "tabSqlFormatter";
            this.tabSqlFormatter.Size = new System.Drawing.Size(1210, 686);
            this.tabSqlFormatter.TabBackColorSelected = System.Drawing.Color.LightCyan;
            this.tabSqlFormatter.TabIndex = 7;
            this.tabSqlFormatter.Text = "SQL Formatter";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(34, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(251, 16);
            this.label1.TabIndex = 320;
            this.c1ThemeController1.SetTheme(this.label1, "(default)");
            // 
            // c1CheckBox4
            // 
            this.c1CheckBox4.AutoSize = true;
            this.c1CheckBox4.BackColor = System.Drawing.Color.Transparent;
            this.c1CheckBox4.BorderColor = System.Drawing.Color.Transparent;
            this.c1CheckBox4.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1CheckBox4.ForeColor = System.Drawing.Color.Black;
            this.c1CheckBox4.Location = new System.Drawing.Point(17, 53);
            this.c1CheckBox4.Name = "c1CheckBox4";
            this.c1CheckBox4.Padding = new System.Windows.Forms.Padding(1);
            this.c1CheckBox4.Size = new System.Drawing.Size(312, 22);
            this.c1CheckBox4.TabIndex = 319;
            this.c1CheckBox4.Text = "Pending Transaction Warning (enabled by default)";
            this.c1ThemeController1.SetTheme(this.c1CheckBox4, "(default)");
            this.c1CheckBox4.UseVisualStyleBackColor = true;
            this.c1CheckBox4.Value = false;
            this.c1CheckBox4.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1CheckBox5
            // 
            this.c1CheckBox5.AutoSize = true;
            this.c1CheckBox5.BackColor = System.Drawing.Color.Transparent;
            this.c1CheckBox5.BorderColor = System.Drawing.Color.Transparent;
            this.c1CheckBox5.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1CheckBox5.ForeColor = System.Drawing.Color.Black;
            this.c1CheckBox5.Location = new System.Drawing.Point(17, 25);
            this.c1CheckBox5.Name = "c1CheckBox5";
            this.c1CheckBox5.Padding = new System.Windows.Forms.Padding(1);
            this.c1CheckBox5.Size = new System.Drawing.Size(399, 22);
            this.c1CheckBox5.TabIndex = 318;
            this.c1CheckBox5.Text = "Disconnect immediately after SELECT queries (enabled by default)";
            this.c1ThemeController1.SetTheme(this.c1CheckBox5, "(default)");
            this.c1CheckBox5.UseVisualStyleBackColor = true;
            this.c1CheckBox5.Value = false;
            this.c1CheckBox5.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1CheckBox1
            // 
            this.c1CheckBox1.AutoSize = true;
            this.c1CheckBox1.BackColor = System.Drawing.Color.Transparent;
            this.c1CheckBox1.BorderColor = System.Drawing.Color.Transparent;
            this.c1CheckBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1CheckBox1.ForeColor = System.Drawing.Color.Black;
            this.c1CheckBox1.Location = new System.Drawing.Point(17, 24);
            this.c1CheckBox1.Name = "c1CheckBox1";
            this.c1CheckBox1.Padding = new System.Windows.Forms.Padding(1);
            this.c1CheckBox1.Size = new System.Drawing.Size(175, 22);
            this.c1CheckBox1.TabIndex = 318;
            this.c1CheckBox1.Text = "When starting JasonQuery";
            this.c1ThemeController1.SetTheme(this.c1CheckBox1, "(default)");
            this.c1CheckBox1.UseVisualStyleBackColor = true;
            this.c1CheckBox1.Value = false;
            this.c1CheckBox1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1Button1
            // 
            this.c1Button1.Image = ((System.Drawing.Image)(resources.GetObject("c1Button1.Image")));
            this.c1Button1.Location = new System.Drawing.Point(314, 23);
            this.c1Button1.Name = "c1Button1";
            this.c1Button1.Size = new System.Drawing.Size(21, 21);
            this.c1Button1.TabIndex = 73;
            this.c1ThemeController1.SetTheme(this.c1Button1, "(default)");
            this.c1Button1.UseVisualStyleBackColor = true;
            this.c1Button1.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1DockingTab5
            // 
            this.c1DockingTab5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1DockingTab5.Controls.Add(this.c1DockingTabPage7);
            this.c1DockingTab5.Location = new System.Drawing.Point(11, 10);
            this.c1DockingTab5.Name = "c1DockingTab5";
            this.c1DockingTab5.SelectedTabBold = true;
            this.c1DockingTab5.Size = new System.Drawing.Size(841, 224);
            this.c1DockingTab5.TabIndex = 9;
            this.c1DockingTab5.TabsSpacing = -1;
            this.c1ThemeController1.SetTheme(this.c1DockingTab5, "(default)");
            this.c1DockingTab5.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.c1DockingTab5.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            // 
            // c1DockingTabPage7
            // 
            this.c1DockingTabPage7.Controls.Add(this.c1CheckBox26);
            this.c1DockingTabPage7.Location = new System.Drawing.Point(1, 10);
            this.c1DockingTabPage7.Name = "c1DockingTabPage7";
            this.c1DockingTabPage7.Size = new System.Drawing.Size(839, 213);
            this.c1DockingTabPage7.TabIndex = 1;
            // 
            // c1CheckBox26
            // 
            this.c1CheckBox26.AutoSize = true;
            this.c1CheckBox26.BackColor = System.Drawing.Color.Transparent;
            this.c1CheckBox26.BorderColor = System.Drawing.Color.Transparent;
            this.c1CheckBox26.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.c1CheckBox26.Checked = true;
            this.c1CheckBox26.CheckState = System.Windows.Forms.CheckState.Checked;
            this.c1CheckBox26.ForeColor = System.Drawing.Color.Black;
            this.c1CheckBox26.Location = new System.Drawing.Point(28, 38);
            this.c1CheckBox26.Name = "c1CheckBox26";
            this.c1CheckBox26.Padding = new System.Windows.Forms.Padding(1);
            this.c1CheckBox26.Size = new System.Drawing.Size(337, 18);
            this.c1CheckBox26.TabIndex = 87;
            this.c1CheckBox26.Text = "Allow large text fields to display full content inline in Query Editor";
            this.c1ThemeController1.SetTheme(this.c1CheckBox26, "(default)");
            this.c1CheckBox26.UseVisualStyleBackColor = true;
            this.c1CheckBox26.Value = true;
            this.c1CheckBox26.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // timerTitle
            // 
            this.timerTitle.Enabled = true;
            this.timerTitle.Interval = 250;
            this.timerTitle.Tick += new System.EventHandler(this.timerTitle_Tick);
            // 
            // pnlCopySettings
            // 
            this.pnlCopySettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.pnlCopySettings.Controls.Add(this.toolStrip6);
            this.pnlCopySettings.Location = new System.Drawing.Point(307, 734);
            this.pnlCopySettings.Name = "pnlCopySettings";
            this.pnlCopySettings.Size = new System.Drawing.Size(171, 31);
            this.pnlCopySettings.TabIndex = 27;
            this.pnlCopySettings.Visible = false;
            // 
            // toolStrip6
            // 
            this.toolStrip6.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip6.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.toolStrip6.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsCopyFrom});
            this.toolStrip6.Location = new System.Drawing.Point(0, 0);
            this.toolStrip6.Name = "toolStrip6";
            this.toolStrip6.Size = new System.Drawing.Size(171, 25);
            this.toolStrip6.TabIndex = 0;
            this.toolStrip6.Text = "toolStrip6";
            this.c1ThemeController1.SetTheme(this.toolStrip6, "(default)");
            // 
            // tsCopyFrom
            // 
            this.tsCopyFrom.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.tsCopyFrom.Image = ((System.Drawing.Image)(resources.GetObject("tsCopyFrom.Image")));
            this.tsCopyFrom.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.tsCopyFrom.Name = "tsCopyFrom";
            this.tsCopyFrom.Size = new System.Drawing.Size(145, 22);
            this.tsCopyFrom.Text = "Copy Settings && Close";
            // 
            // btnCopySettings
            // 
            this.btnCopySettings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopySettings.Location = new System.Drawing.Point(532, 732);
            this.btnCopySettings.Name = "btnCopySettings";
            this.btnCopySettings.Size = new System.Drawing.Size(143, 36);
            this.btnCopySettings.TabIndex = 28;
            this.btnCopySettings.Text = "Copy Settings...";
            this.c1ThemeController1.SetTheme(this.btnCopySettings, "(default)");
            this.btnCopySettings.UseVisualStyleBackColor = true;
            this.btnCopySettings.Visible = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(1082, 731);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(136, 36);
            this.btnClose.TabIndex = 56;
            this.btnClose.Text = "Cancel && &Close";
            this.c1ThemeController1.SetTheme(this.btnClose, "(default)");
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(933, 731);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(126, 36);
            this.btnApply.TabIndex = 57;
            this.btnApply.Text = "&Apply && Close";
            this.c1ThemeController1.SetTheme(this.btnApply, "(default)");
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnRestoreDefaults
            // 
            this.btnRestoreDefaults.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRestoreDefaults.Location = new System.Drawing.Point(716, 732);
            this.btnRestoreDefaults.Name = "btnRestoreDefaults";
            this.btnRestoreDefaults.Size = new System.Drawing.Size(143, 36);
            this.btnRestoreDefaults.TabIndex = 58;
            this.btnRestoreDefaults.Text = "&Restore Defaults";
            this.c1ThemeController1.SetTheme(this.btnRestoreDefaults, "(default)");
            this.btnRestoreDefaults.UseVisualStyleBackColor = true;
            this.btnRestoreDefaults.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnRestoreDefaults.Click += new System.EventHandler(this.btnRestoreDefaults_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Consolas", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(161, 27);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(0, 15);
            this.label11.TabIndex = 82;
            this.c1ThemeController1.SetTheme(this.label11, "(default)");
            // 
            // c1ComboBox10
            // 
            this.c1ComboBox10.AllowSpinLoop = false;
            this.c1ComboBox10.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.c1ComboBox10.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.c1ComboBox10.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.c1ComboBox10.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.c1ComboBox10.GapHeight = 0;
            this.c1ComboBox10.ImagePadding = new System.Windows.Forms.Padding(0);
            this.c1ComboBox10.Items.Add("<NULL>");
            this.c1ComboBox10.Items.Add("<null>");
            this.c1ComboBox10.Items.Add("{NULL}");
            this.c1ComboBox10.Items.Add("{null}");
            this.c1ComboBox10.Items.Add("(NULL)");
            this.c1ComboBox10.Items.Add("(null)");
            this.c1ComboBox10.Items.Add("None");
            this.c1ComboBox10.ItemsDisplayMember = "";
            this.c1ComboBox10.ItemsValueMember = "";
            this.c1ComboBox10.Location = new System.Drawing.Point(79, 23);
            this.c1ComboBox10.Name = "c1ComboBox10";
            this.c1ComboBox10.Size = new System.Drawing.Size(79, 20);
            this.c1ComboBox10.TabIndex = 81;
            this.c1ComboBox10.Tag = null;
            this.c1ComboBox10.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.c1ComboBox10, "(default)");
            this.c1ComboBox10.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // label9
            // 
            this.label9.BackColor = System.Drawing.Color.Transparent;
            this.label9.Location = new System.Drawing.Point(252, 44);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(200, 16);
            this.label9.TabIndex = 47;
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label10
            // 
            this.label10.BackColor = System.Drawing.Color.Transparent;
            this.label10.Location = new System.Drawing.Point(252, 134);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(200, 16);
            this.label10.TabIndex = 0;
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // groupBox2
            // 
            this.groupBox2.BackColor = System.Drawing.Color.Transparent;
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.c1ComboBox10);
            this.groupBox2.Controls.Add(this.panel4);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Controls.Add(this.label13);
            this.groupBox2.Location = new System.Drawing.Point(28, 93);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(207, 84);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.panel4.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel4.Location = new System.Drawing.Point(109, 52);
            this.panel4.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(79, 21);
            this.panel4.TabIndex = 20;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(21, 54);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(0, 12);
            this.label12.TabIndex = 22;
            this.label12.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(21, 25);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(0, 12);
            this.label13.TabIndex = 19;
            this.label13.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label14
            // 
            this.label14.BackColor = System.Drawing.Color.Transparent;
            this.label14.Location = new System.Drawing.Point(252, 104);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(200, 16);
            this.label14.TabIndex = 64;
            this.label14.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // OptionsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1226, 780);
            this.Controls.Add(this.btnCopySettings);
            this.Controls.Add(this.pnlCopySettings);
            this.Controls.Add(this.c1DockingTab);
            this.Controls.Add(this.lblRequireRestart);
            this.Controls.Add(this.lblStarRequireToRestart);
            this.Controls.Add(this.txtHeightFormatter);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.txtHeightCode);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnRestoreDefaults);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OptionsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            this.grpEditorColors.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.grpPreferences.ResumeLayout(false);
            this.grpPreferences.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SelectCurrentSqlBlock)).EndInit();
            this.grpHighlightStyle.ResumeLayout(false);
            this.grpHighlightStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightStyle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightAlpha)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHighlightOutlineAlpha)).EndInit();
            this.grpIndent.ResumeLayout(false);
            this.grpIndent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkReplaceTabWithSpace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowIndentGuide)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTabWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboIndentMode)).EndInit();
            this.grpIndicate.ResumeLayout(false);
            this.grpIndicate.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboBookmarkStyle)).EndInit();
            this.grpWordWrap.ResumeLayout(false);
            this.grpWordWrap.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkMargin)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkStart)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkWordWrap)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkOpenFileOnCurrentTab)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowSaveAsButton)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorZoom)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSaveAsEncoding)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEntireBlankRowAsEmptyRow4SelectBlock)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHighlightSelectedText)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSaveAsEncoding)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBold)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEditorFontPicker)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowAllCharacters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHighlightSelection)).EndInit();
            this.grpColorTheme.ResumeLayout(false);
            this.grpColorTheme.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DarkMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDarkMode)).EndInit();
            this.grpQueryEditorPreview.ResumeLayout(false);
            this.grpQueryEditorPreview.PerformLayout();
            this.tsEditor.ResumeLayout(false);
            this.tsEditor.PerformLayout();
            this.grpAutoComplete.ResumeLayout(false);
            this.grpAutoComplete.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkFirstCharChecking)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudMinFragmentLength)).EndInit();
            this.grpAutoCompleteFor.ResumeLayout(false);
            this.grpAutoCompleteFor.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedViews)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedTriggers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedTables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedFunctions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkUserDefinedKeywords)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBuiltInKeywords)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBuiltInFunctions)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_EnableAutoComplete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableAutoComplete)).EndInit();
            this.grpAutoReplace.ResumeLayout(false);
            this.grpAutoReplace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRowAutoReplace)).EndInit();
            this.grpModifyDefinitionAutoReplace.ResumeLayout(false);
            this.grpModifyDefinitionAutoReplace.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Symbol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtKeyword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClearAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancelAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveAutoReplace)).EndInit();
            this.grpDefinitionAutoReplace.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnDeleteAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEditAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAddAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridAutoReplaceInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_EnableAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableAutoReplace)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnComment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnComment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowGroupingRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSetFocusAfterQuery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCtrlMouseWheel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridRowHeightResizing)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridVisualStyle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboResultCopyQuotingWith)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboMaxWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkResize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontPicker)).EndInit();
            this.grpNullValueStyle.ResumeLayout(false);
            this.grpNullValueStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboNullShowAs)).EndInit();
            this.grpDataGridColor.ResumeLayout(false);
            this.grpDataGridColor.PerformLayout();
            this.grpPreviewGrid.ResumeLayout(false);
            this.grpPreviewGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboFindGrid)).EndInit();
            this.tsGrid.ResumeLayout(false);
            this.tsGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVisualStyle)).EndInit();
            this.grpOperatorKeywords.ResumeLayout(false);
            this.grpFindOperatorKeywords.ResumeLayout(false);
            this.grpFindOperatorKeywords.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picOperatorKeywords)).EndInit();
            this.grpBuiltInFunctions.ResumeLayout(false);
            this.grpBuiltInFunctions.PerformLayout();
            this.grpFindBuiltInFunctions.ResumeLayout(false);
            this.grpFindBuiltInFunctions.PerformLayout();
            this.toolStrip3.ResumeLayout(false);
            this.toolStrip3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBuiltInFunctions)).EndInit();
            this.grpBuiltInKeywords.ResumeLayout(false);
            this.grpBuiltInKeywords.PerformLayout();
            this.grpFindBuiltInKeywords.ResumeLayout(false);
            this.grpFindBuiltInKeywords.PerformLayout();
            this.toolStrip4.ResumeLayout(false);
            this.toolStrip4.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picBuiltInKeywords)).EndInit();
            this.grpUserDefinedKeywords.ResumeLayout(false);
            this.grpFindUserDefinedKeywords.ResumeLayout(false);
            this.grpFindUserDefinedKeywords.PerformLayout();
            this.toolStrip5.ResumeLayout(false);
            this.toolStrip5.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picUserDefinedKeywords)).EndInit();
            this.grpSqlToCode.ResumeLayout(false);
            this.grpSqlToCode.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtStringBuilderVariableName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSqlVariableName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkStripCode)).EndInit();
            this.grpSqlStatementCode.ResumeLayout(false);
            this.grpPreviewSql.ResumeLayout(false);
            this.grpStyle.ResumeLayout(false);
            this.grpStyle.PerformLayout();
            this.grpLanguage.ResumeLayout(false);
            this.grpSqlFormatter.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.grpFormattingOptions.ResumeLayout(false);
            this.grpFormattingOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMaxWidth)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkConvertCaseForKeywords)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.grpSqlStatementFormatter.ResumeLayout(false);
            this.grpPreviewFormatter.ResumeLayout(false);
            this.grpCommitRollbackIcon.ResumeLayout(false);
            this.grpCommitRollbackIcon.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox9)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox10)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox11)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox12)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox7)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox8)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox6)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.grpBackup.ResumeLayout(false);
            this.grpBackup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAskMeBeforeOpenUnsavedFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBackupPathOpenFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseBackupPath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtBackupPath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRememberUnsavedFiles)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEnableBackup)).EndInit();
            this.grpMaxEntries.ResumeLayout(false);
            this.grpMaxEntries.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtMyFavorite)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRecentFiles)).EndInit();
            this.grpGeneral.ResumeLayout(false);
            this.grpGeneral.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowDatabaseName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowIP)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowVersion)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDateFormat)).EndInit();
            this.grpMainFormTabVisualStyle.ResumeLayout(false);
            this.grpMainFormTabVisualStyle.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkMultiLine)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkHoverSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowArrows)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShrinkPages)).EndInit();
            this.grpMainFormWindowsState.ResumeLayout(false);
            this.grpMainFormWindowsState.PerformLayout();
            this.tabExample.ResumeLayout(false);
            this.grpAppearance.ResumeLayout(false);
            this.grpAppearance.PerformLayout();
            this.grpMainFormTabStyle.ResumeLayout(false);
            this.grpMainFormTabStyle.PerformLayout();
            this.grpOptionsTab.ResumeLayout(false);
            this.grpOptionsTab.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab1)).EndInit();
            this.c1DockingTab1.ResumeLayout(false);
            this.grpCheckForUpdate.ResumeLayout(false);
            this.grpCheckForUpdate.PerformLayout();
            this.grpCheckOnly.ResumeLayout(false);
            this.grpCheckOnly.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab)).EndInit();
            this.c1DockingTab.ResumeLayout(false);
            this.tabGlobal.ResumeLayout(false);
            this.tabGlobal.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab4)).EndInit();
            this.c1DockingTab4.ResumeLayout(false);
            this.tabGlobalSettings.ResumeLayout(false);
            this.tabGlobalSettings.PerformLayout();
            this.tabSafetySettings.ResumeLayout(false);
            this.grpLargeBinaryDisplayStrategy.ResumeLayout(false);
            this.grpLargeBinaryDisplayStrategy.PerformLayout();
            this.grpLargeTextDisplayStrategy.ResumeLayout(false);
            this.grpLargeTextDisplayStrategy.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLargeTextPreviewLength)).EndInit();
            this.grpConnectionSafety.ResumeLayout(false);
            this.grpConnectionSafety.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PendingWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_DisconnectAfterSelect)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPendingWarning)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkDisconnectAfterSelect)).EndInit();
            this.tabUpdateSettings.ResumeLayout(false);
            this.grpUpdateInformationSource.ResumeLayout(false);
            this.grpUpdateInformationSource.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_LocalUpdateFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnLocalFolderOpenFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseLocalFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtLocalFolder)).EndInit();
            this.tabGeneral.ResumeLayout(false);
            this.grpSqlHistory.ResumeLayout(false);
            this.grpSqlHistory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboSqlHistoryRetentionDays)).EndInit();
            this.grpDefaultDirectory.ResumeLayout(false);
            this.grpDefaultDirectory.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFavaritePath)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFavoriteDirectory)).EndInit();
            this.grpOpenSqlFile.ResumeLayout(false);
            this.grpOpenSqlFile.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClear1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSpecifiedSqlFile2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSpecifiedSQLFile2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSpecifiedSqlFile1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSpecifiedSQLFile1)).EndInit();
            this.tabQueryEditor.ResumeLayout(false);
            this.grpStatementCompletion.ResumeLayout(false);
            this.grpStatementCompletion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AutoListMembers)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_SavePoint)).EndInit();
            this.grpSchemaInformation.ResumeLayout(false);
            this.grpSchemaInformation.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkDefaultTabSchemaInformation)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ShowColumnInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowColumnInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkSortByColumnName)).EndInit();
            this.tabAutoComplete.ResumeLayout(false);
            this.tabAutoComplete.PerformLayout();
            this.tabAutoReplace.ResumeLayout(false);
            this.tabAutoReplace.PerformLayout();
            this.tabDataGrid.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab2)).EndInit();
            this.c1DockingTab2.ResumeLayout(false);
            this.tabDataGridQueryResultBehavior.ResumeLayout(false);
            this.tabDataGridQueryResultBehavior.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_AppendQueryResult)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_PagedQuery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_RawDataMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkPagedQuery)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkRawDataMode)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboRowsPerPage)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAppendQueryResult)).EndInit();
            this.tabDataGridInteraction.ResumeLayout(false);
            this.tabDataGridInteraction.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkDirection)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDirection)).EndInit();
            this.tabDataGridAppearance.ResumeLayout(false);
            this.tabDataGridAppearance.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboResultCopyFieldSeparator)).EndInit();
            this.tabKeywords.ResumeLayout(false);
            this.splitContainer4.Panel1.ResumeLayout(false);
            this.splitContainer4.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer4)).EndInit();
            this.splitContainer4.ResumeLayout(false);
            this.splitContainer5.Panel1.ResumeLayout(false);
            this.splitContainer5.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer5)).EndInit();
            this.splitContainer5.ResumeLayout(false);
            this.splitContainer6.Panel1.ResumeLayout(false);
            this.splitContainer6.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer6)).EndInit();
            this.splitContainer6.ResumeLayout(false);
            this.tabSqlToCode.ResumeLayout(false);
            this.tabSqlFormatter.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox5)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Button1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1DockingTab5)).EndInit();
            this.c1DockingTab5.ResumeLayout(false);
            this.c1DockingTabPage7.ResumeLayout(false);
            this.c1DockingTabPage7.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1CheckBox26)).EndInit();
            this.pnlCopySettings.ResumeLayout(false);
            this.pnlCopySettings.PerformLayout();
            this.toolStrip6.ResumeLayout(false);
            this.toolStrip6.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopySettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRestoreDefaults)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ComboBox10)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblEditorBackground;
        private System.Windows.Forms.Panel pnlBuiltInFunctions;
        private System.Windows.Forms.Label lblBuiltInFunctions;
        private System.Windows.Forms.Panel pnlUserTables;
        private System.Windows.Forms.Label lblUserTables;
        private System.Windows.Forms.Panel pnlCharacter;
        private System.Windows.Forms.Label lblCharacter;
        private System.Windows.Forms.Panel pnlString;
        private System.Windows.Forms.Label lblString;
        private System.Windows.Forms.Panel pnlBuiltInKeywords;
        private System.Windows.Forms.Label lblBuiltInKeywords;
        private System.Windows.Forms.Panel pnlIdentifier;
        private System.Windows.Forms.Label lblIdentifier;
        private System.Windows.Forms.Panel pnlComments;
        private System.Windows.Forms.Label lblComments;
        private System.Windows.Forms.Panel pnlCurrentLineBackground;
        private System.Windows.Forms.Panel pnlEditorBackground;
        private System.Windows.Forms.Label lblCurrentLineBackground;
        private System.Windows.Forms.GroupBox grpQueryEditorPreview;
        private JasonLibrary.UI.Controls.ScintillaEditor editor;
        private System.Windows.Forms.Panel pnlNumber;
        private System.Windows.Forms.Label lblNumber;
        private System.Windows.Forms.GroupBox grpEditorColors;
        private System.Windows.Forms.GroupBox grpPreferences;
        private System.Windows.Forms.Panel pnlOperatorSymbol;
        private System.Windows.Forms.Label lblOperatorSymbol;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label lblWhiteSpace;
        private System.Windows.Forms.Panel pnlWhiteSpace;
        private System.Windows.Forms.Label lblIndentMode;
        private System.Windows.Forms.GroupBox grpWordWrap;
        private System.Windows.Forms.Panel pnlUserFunctions;
        private System.Windows.Forms.Label lblUserFunctions;
        private System.Windows.Forms.GroupBox grpAutoComplete;
        private System.Windows.Forms.GroupBox grpAutoCompleteFor;
        private System.Windows.Forms.GroupBox grpAutoReplace;
        private System.Windows.Forms.GroupBox grpModifyDefinitionAutoReplace;
        private System.Windows.Forms.GroupBox grpDefinitionAutoReplace;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridAutoReplaceInfo;
        private System.Windows.Forms.Label lblStarAutoReplace;
        private System.Windows.Forms.Label lblReplacement;
        private System.Windows.Forms.Label lblKeyword;
        private System.Windows.Forms.Label lblRequireRestart;
        private System.Windows.Forms.Label lblStarRequireToRestart;
        private System.Windows.Forms.GroupBox grpPreviewGrid;
        private System.Windows.Forms.Label lblGridVisualStyle;
        private System.Windows.Forms.GroupBox grpDataGridColor;
        private System.Windows.Forms.GroupBox grpNullValueStyle;
        private System.Windows.Forms.Label lblResultCopyQuotingWith;
        private System.Windows.Forms.Panel pnlGridOddRowBackColor;
        private System.Windows.Forms.Panel pnlGridEvenRowBackColor;
        private System.Windows.Forms.Panel pnlNullValueForeColor;
        private System.Windows.Forms.Label lblNullValueForeColor;
        private System.Windows.Forms.Label lblNullValueShowAs;
        private System.Windows.Forms.Label lblEditorZoom;
        private System.Windows.Forms.Label lblEditorFontName;
        private System.Windows.Forms.Label lblEditorFontSize;
        private System.Windows.Forms.Label lblGridFontSize;
        private System.Windows.Forms.Label lblGridFontName;
        private System.Windows.Forms.GroupBox grpOptionsTab;
        private System.Windows.Forms.Panel pnlOperatorKeywords;
        private System.Windows.Forms.Label lblOperatorKeywords;
        private System.Windows.Forms.Label lblUserDefinedKeywords;
        private System.Windows.Forms.Panel pnlUserDefinedKeywords;
        private System.Windows.Forms.Label lblStarWordWrap;
        private System.Windows.Forms.Label lblStarAutoComplete;
        private System.Windows.Forms.GroupBox grpSqlToCode;
        private System.Windows.Forms.GroupBox grpLanguage;
        private System.Windows.Forms.GroupBox grpPreviewSql;
        private System.Windows.Forms.GroupBox grpStyle;
        private System.Windows.Forms.Label lblMinFragmentLength;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlToCodePreview;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridVisualStyle;
        private System.Windows.Forms.Timer timerMother2Child;
        private System.Windows.Forms.GroupBox grpOperatorKeywords;
        private System.Windows.Forms.GroupBox grpBuiltInFunctions;
        private System.Windows.Forms.GroupBox grpBuiltInKeywords;
        private System.Windows.Forms.GroupBox grpUserDefinedKeywords;
        private System.Windows.Forms.Label lbl1;
        private System.Windows.Forms.Label lbl2;
        private System.Windows.Forms.ListBox lstLanguage;
        private System.Windows.Forms.RadioButton rdoStyle3;
        private System.Windows.Forms.RadioButton rdoStyle2;
        private System.Windows.Forms.RadioButton rdoStyle1;
        private System.Windows.Forms.Label lblSqlVariableName;
        private System.Windows.Forms.GroupBox grpSqlStatementCode;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlToCode;
        private System.Windows.Forms.GroupBox grpSqlFormatter;
        private System.Windows.Forms.GroupBox grpSqlStatementFormatter;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlFormatter;
        private System.Windows.Forms.GroupBox grpPreviewFormatter;
        private System.Windows.Forms.Label lblSqlFormatterPreviewStatus;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlFormatterPreview;
        private System.Windows.Forms.GroupBox grpFormattingOptions;
        private System.Windows.Forms.ComboBox cboSqlFormatterEngine;
        private System.Windows.Forms.Label lblSqlFormatterEngine;
        private System.Windows.Forms.ComboBox cboSqlFormatterIndentSize;
        private System.Windows.Forms.Label lblSqlFormatterIndentSize;
        private System.Windows.Forms.ComboBox cboSqlFormatterBlankLines;
        private System.Windows.Forms.Label lblSqlFormatterBlankLines;
        private System.Windows.Forms.ComboBox cboSqlFormatterListItemsPerLine;
        private System.Windows.Forms.Label lblSqlFormatterListItemsPerLine;
        private System.Windows.Forms.Label lblMaxWidth2;
        private System.Windows.Forms.RadioButton rdoUpperCase;
        private System.Windows.Forms.RadioButton rdoLowerCase;
        private System.Windows.Forms.Label lblStarHighlightSelection;
        private System.Windows.Forms.Panel pnlGridHighlightBackColor;
        private System.Windows.Forms.Label lblGridHighlightBackColor;
        private System.Windows.Forms.Panel pnlGridHighlightForeColor;
        private System.Windows.Forms.Label lblGridHighlightForeColor;
        private System.Windows.Forms.ToolStrip tsGrid;
        private System.Windows.Forms.ToolStripLabel lblFindGrid;
        private System.Windows.Forms.ToolStripComboBox cboFindGrid3;
        private System.Windows.Forms.ToolStripButton btnFindNextGrid;
        private System.Windows.Forms.ToolStripButton btnFindPreviousGrid;
        private System.Windows.Forms.ToolStripButton btnHighlightAllGrid;
        private System.Windows.Forms.ToolStripButton btnClearHighlightsGrid;
        private System.Windows.Forms.ToolStripButton btnCountGrid;
        private JasonLibrary.UI.Controls.ScintillaEditor editorOperatorKeywords;
        private JasonLibrary.UI.Controls.ScintillaEditor editorBuiltInFunctions;
        private JasonLibrary.UI.Controls.ScintillaEditor editorBuiltInKeywords;
        private JasonLibrary.UI.Controls.ScintillaEditor editorUserDefinedKeywords;
        private System.Windows.Forms.GroupBox grpFindOperatorKeywords;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton btnNextOperatorKeywords;
        private System.Windows.Forms.ToolStripButton btnPreviousOperatorKeywords;
        private System.Windows.Forms.ToolStripButton btnCloseFindOperatorKeywords;
        private System.Windows.Forms.PictureBox picOperatorKeywords;
        private System.Windows.Forms.ToolStripTextBox txtFindOperatorKeywords;
        private System.Windows.Forms.PictureBox picBuiltInFunctions;
        private System.Windows.Forms.PictureBox picBuiltInKeywords;
        private System.Windows.Forms.PictureBox picUserDefinedKeywords;
        private System.Windows.Forms.GroupBox grpFindBuiltInFunctions;
        private System.Windows.Forms.ToolStrip toolStrip3;
        private System.Windows.Forms.ToolStripTextBox txtFindBuiltInFunctions;
        private System.Windows.Forms.ToolStripButton btnNextBuiltInFunctions;
        private System.Windows.Forms.ToolStripButton btnPreviousBuiltInFunctions;
        private System.Windows.Forms.ToolStripButton btnCloseFindBuiltInFunctions;
        private System.Windows.Forms.GroupBox grpFindBuiltInKeywords;
        private System.Windows.Forms.ToolStrip toolStrip4;
        private System.Windows.Forms.ToolStripTextBox txtFindBuiltInKeywords;
        private System.Windows.Forms.ToolStripButton btnNextBuiltInKeywords;
        private System.Windows.Forms.ToolStripButton btnPreviousBuiltInKeywords;
        private System.Windows.Forms.ToolStripButton btnCloseFindBuiltInKeywords;
        private System.Windows.Forms.GroupBox grpFindUserDefinedKeywords;
        private System.Windows.Forms.ToolStrip toolStrip5;
        private System.Windows.Forms.ToolStripTextBox txtFindUserDefinedKeywords;
        private System.Windows.Forms.ToolStripButton btnNextUserDefinedKeywords;
        private System.Windows.Forms.ToolStripButton btnPreviousUserDefinedKeywords;
        private System.Windows.Forms.ToolStripButton btnCloseFindUserDefinedKeywords;
        private System.Windows.Forms.Label lblSelectedTextBackground;
        private System.Windows.Forms.Panel pnlSelectedTextBackground;
        private System.Windows.Forms.Label lblGridOddRowBackColor;
        private System.Windows.Forms.Label lblGridOddRowForeColor;
        private System.Windows.Forms.Label lblGridEvenRowBackColor;
        private System.Windows.Forms.Label lblGridEvenRowForeColor;
        private System.Windows.Forms.Panel pnlGridOddRowForeColor;
        private System.Windows.Forms.Panel pnlGridEvenRowForeColor;
        private System.Windows.Forms.Panel pnlGridSelectedBackColor;
        private System.Windows.Forms.Label lblGridSelectedBackColor;
        private System.Windows.Forms.Panel pnlGridSelectedForeColor;
        private System.Windows.Forms.Label lblGridSelectedForeColor;
        private System.Windows.Forms.Label lblBookmarkBackground;
        private System.Windows.Forms.Panel pnlBookmarkBackground;
        private System.Windows.Forms.GroupBox grpIndicate;
        private System.Windows.Forms.Label lblBookmarkStyle;
        private System.Windows.Forms.Label lblErrorLineBackground;
        private System.Windows.Forms.Panel pnlErrorLineBackground;
        private System.Windows.Forms.Label lblStarIndicate;
        private JasonLibrary.UI.Controls.ScintillaEditor editorIndicator;
        private System.Windows.Forms.Label lblStarGridColor;
        private System.Windows.Forms.Label lblMaxWidth;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.TextBox txtHeightCode;
        private System.Windows.Forms.TextBox txtHeightFormatter;
        private System.Windows.Forms.Label lblAutoReplaceInfo1;
        private System.Windows.Forms.GroupBox grpCheckForUpdate;
        private System.Windows.Forms.RadioButton rdoCheckForUpdates1;
        private System.Windows.Forms.RadioButton rdoCheckForUpdates0;
        private System.Windows.Forms.Label lblGridRowHeightResizing;
        private C1.Win.C1Command.C1DockingTab c1DockingTab;
        private C1.Win.C1Command.C1DockingTabPage tabQueryEditor;
        private C1.Win.C1Command.C1DockingTabPage tabAutoComplete;
        private C1.Win.C1Command.C1DockingTabPage tabAutoReplace;
        private C1.Win.C1Command.C1DockingTabPage tabDataGrid;
        private C1.Win.C1Command.C1DockingTabPage tabKeywords;
        private C1.Win.C1Command.C1DockingTabPage tabSqlToCode;
        private C1.Win.C1Command.C1DockingTabPage tabSqlFormatter;
        private System.Windows.Forms.SplitContainer splitContainer4;
        private System.Windows.Forms.SplitContainer splitContainer5;
        private System.Windows.Forms.SplitContainer splitContainer6;
        private System.Windows.Forms.RadioButton rdoCheckForUpdates7;
        private C1.Win.C1Input.C1FontPicker cboEditorFontPicker;
        private C1.Win.C1Input.C1FontPicker cboGridFontPicker;
        private System.Windows.Forms.GroupBox grpMainFormTabVisualStyle;
        private System.Windows.Forms.GroupBox grpAppearance;
        private System.Windows.Forms.RadioButton rdoMultiBox;
        private System.Windows.Forms.RadioButton rdoMultiForm;
        private System.Windows.Forms.RadioButton rdoMultiDocument;
        private System.Windows.Forms.GroupBox grpMainFormTabStyle;
        private System.Windows.Forms.RadioButton rdoPlain;
        private System.Windows.Forms.RadioButton rdoIDE;
        private Crownwood.Magic.Controls.TabControl tabExample;
        private Crownwood.Magic.Controls.TabPage tabPage1;
        private System.Windows.Forms.Timer timerTitle;
        private Crownwood.Magic.Controls.TabPage tabPage2;
        private Crownwood.Magic.Controls.TabPage tabPage3;
        private Crownwood.Magic.Controls.TabPage tabPage4;
        private Crownwood.Magic.Controls.TabPage tabPage5;
        private Crownwood.Magic.Controls.TabPage tabPage6;
        private Crownwood.Magic.Controls.TabPage tabPage7;
        private Crownwood.Magic.Controls.TabPage tabPage8;
        private Crownwood.Magic.Controls.TabPage tabPage9;
        private Crownwood.Magic.Controls.TabPage tabPage10;
        private Crownwood.Magic.Controls.TabPage tabPage11;
        private Crownwood.Magic.Controls.TabPage tabPage12;
        private Crownwood.Magic.Controls.TabPage tabPage13;
        private Crownwood.Magic.Controls.TabPage tabPage14;
        private Crownwood.Magic.Controls.TabPage tabPage15;
        private Crownwood.Magic.Controls.TabPage tabPage16;
        private Crownwood.Magic.Controls.TabPage tabPage17;
        private Crownwood.Magic.Controls.TabPage tabPage18;
        private Crownwood.Magic.Controls.TabPage tabPage19;
        private Crownwood.Magic.Controls.TabPage tabPage20;
        private System.Windows.Forms.Label lblLocalization;
        private System.Windows.Forms.CheckBox chkTabBold;
        private C1.Win.C1Command.C1DockingTab c1DockingTab1;
        private C1.Win.C1Command.C1DockingTabPage tabGlobal2;
        private C1.Win.C1Command.C1DockingTabPage tabQueryEditor2;
        private C1.Win.C1Command.C1DockingTabPage tabAutoComplete2;
        private C1.Win.C1Command.C1DockingTabPage tabAutoReplace2;
        private C1.Win.C1Command.C1DockingTabPage tabDataGrid2;
        private C1.Win.C1Command.C1DockingTabPage tabKeywords2;
        private C1.Win.C1Command.C1DockingTabPage tabSqlToCode2;
        private C1.Win.C1Command.C1DockingTabPage tabSqlFormatter2;
        private System.Windows.Forms.Label lblOptionsTabActiveForeColor;
        private System.Windows.Forms.Label lblOptionsTabActiveBackColor;
        private System.Windows.Forms.Panel pnlOptionsTabActiveForeColor;
        private System.Windows.Forms.Panel pnlOptionsTabActiveBackColor;
        private System.Windows.Forms.Label lblDateFormat;
        private System.Windows.Forms.GroupBox grpMainFormWindowsState;
        private System.Windows.Forms.RadioButton rdoMaximized;
        private System.Windows.Forms.RadioButton rdoNormal;
        private System.Windows.Forms.GroupBox grpGeneral;
        private System.Windows.Forms.Panel pnlCopySettings;
        private System.Windows.Forms.ToolStrip toolStrip6;
        private System.Windows.Forms.ToolStripDropDownButton tsCopyFrom;
        private System.Windows.Forms.Label lblToolstripBackground;
        private System.Windows.Forms.Panel pnlToolstripBackground;
        private System.Windows.Forms.ToolStrip tsEditor;
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
        private System.Windows.Forms.ToolStripMenuItem mnuDephi2Sql;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton btnComment;
        private System.Windows.Forms.ToolStripButton btnRemoveComment;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnIndent;
        private System.Windows.Forms.ToolStripTextBox txtIndentWord;
        private System.Windows.Forms.ToolStripButton btnUnIndent;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnHighlightSelection;
        private System.Windows.Forms.ToolStripButton btnHighlightSelection2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.GroupBox grpMaxEntries;
        private System.Windows.Forms.Label lblMyFavorite;
        private System.Windows.Forms.Label lblRecentFiles;
        private System.Windows.Forms.Label lblOptionsTabInactiveForeColor;
        private System.Windows.Forms.Panel pnlOptionsTabInactiveForeColor;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Input.C1SplitButton btnCopySettings;
        private C1.Win.C1Command.C1DockingTabPage tabGlobal;
        private C1.Win.C1Input.C1CheckBox chkShrinkPages;
        private C1.Win.C1Input.C1CheckBox chkWordWrap;
        private C1.Win.C1Input.C1CheckBox chkMultiLine;
        private C1.Win.C1Input.C1CheckBox chkHoverSelect;
        private C1.Win.C1Input.C1CheckBox chkShowArrows;
        private C1.Win.C1Input.C1CheckBox chkHighlightSelection;
        private C1.Win.C1Input.C1CheckBox chkEntireBlankRowAsEmptyRow4SelectBlock;
        private C1.Win.C1Input.C1CheckBox chkHighlightSelectedText;
        private C1.Win.C1Input.C1CheckBox chkSaveAsEncoding;
        private C1.Win.C1Input.C1CheckBox chkCopyAsHTML;
        private C1.Win.C1Input.C1CheckBox chkBold;
        private C1.Win.C1Input.C1CheckBox chkMargin;
        private C1.Win.C1Input.C1CheckBox chkEnd;
        private C1.Win.C1Input.C1CheckBox chkStart;
        private C1.Win.C1Input.C1CheckBox chkShowAllCharacters;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRowAC;
        private C1.Win.C1Input.C1CheckBox chkUserDefinedViews;
        private C1.Win.C1Input.C1CheckBox chkUserDefinedTriggers;
        private C1.Win.C1Input.C1CheckBox chkUserDefinedTables;
        private C1.Win.C1Input.C1CheckBox chkUserDefinedFunctions;
        private C1.Win.C1Input.C1CheckBox chkUserDefinedKeywords;
        private C1.Win.C1Input.C1CheckBox chkBuiltInKeywords;
        private C1.Win.C1Input.C1CheckBox chkBuiltInFunctions;
        private C1.Win.C1Input.C1CheckBox chkEnableAutoComplete;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRowAutoReplace;
        private C1.Win.C1Input.C1CheckBox chkEnableAutoReplace;
        private C1.Win.C1Input.C1CheckBox chkResize;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRow;
        private C1.Win.C1Input.C1CheckBox chkShowColumnType;
        private C1.Win.C1Input.C1CheckBox chkStripCode;
        private C1.Win.C1Input.C1CheckBox chkConvertCaseForKeywords;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnApply;
        private C1.Win.C1Input.C1Button btnRestoreDefaults;
        private C1.Win.C1Input.C1Button btnClearAutoReplace;
        private C1.Win.C1Input.C1Button btnCancelAutoReplace;
        private C1.Win.C1Input.C1Button btnSaveAutoReplace;
        private C1.Win.C1Input.C1Button btnDeleteAutoReplace;
        private C1.Win.C1Input.C1Button btnEditAutoReplace;
        private C1.Win.C1Input.C1Button btnAddAutoReplace;
        private C1.Win.C1Input.C1ComboBox cboSaveAsEncoding;
        private C1.Win.C1Input.C1ComboBox cboEditorZoom;
        private C1.Win.C1Input.C1ComboBox cboEditorFontSize;
        private C1.Win.C1Input.C1ComboBox cboBookmarkStyle;
        private C1.Win.C1Input.C1ComboBox cboIndentMode;
        private C1.Win.C1Input.C1TextBox txtRecentFiles;
        private C1.Win.C1Input.C1TextBox txtMyFavorite;
        private C1.Win.C1Input.C1TextBox txtKeyword;
        private C1.Win.C1Input.C1TextBox txtSqlVariableName;
        private C1.Win.C1Input.C1TextBox txtMaxWidth;
        private C1.Win.C1Input.C1ComboBox cboMaxWidth;
        private C1.Win.C1Input.C1ComboBox cboDateFormat;
        private C1.Win.C1Input.C1ComboBox cboGridRowHeightResizing;
        private C1.Win.C1Input.C1ComboBox cboGridFontSize;
        private C1.Win.C1Input.C1ComboBox cboGridVisualStyle;
        private C1.Win.C1Input.C1ComboBox cboResultCopyQuotingWith;
        private C1.Win.C1Input.C1ComboBox cboNullShowAs;
        private C1.Win.C1Input.C1ComboBox cboFindGrid;
        private System.Windows.Forms.Label lblGridHeadingForeColor;
        private System.Windows.Forms.Panel pnlGridHeadingForeColor;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
        private System.Windows.Forms.Label lblSymbolTips;
        private C1.Win.C1Input.C1CheckBox chkShowSaveAsButton;
        private System.Windows.Forms.GroupBox grpColorTheme;
        private C1.Win.C1Input.C1CheckBox chkDarkMode;
        private C1.Win.C1Input.C1Button btnHelp_DarkMode;
        private C1.Win.C1Input.C1CheckBox chkOpenFileOnCurrentTab;
        private C1.Win.C1Command.C1DockingTabPage tabGeneral;
        private System.Windows.Forms.GroupBox grpOpenSqlFile;
        private System.Windows.Forms.RadioButton rdoCheckOnly;
        private System.Windows.Forms.GroupBox grpCheckOnly;
        private System.Windows.Forms.RadioButton rdoDonotCheck;
        private C1.Win.C1Command.C1DockingTabPage tabGeneral2;
        private System.Windows.Forms.Label lblFile1;
        private C1.Win.C1Input.C1Button btnSpecifiedSqlFile1;
        private C1.Win.C1Input.C1TextBox txtSpecifiedSQLFile1;
        private C1.Win.C1Input.C1Button btnSpecifiedSqlFile2;
        private C1.Win.C1Input.C1TextBox txtSpecifiedSQLFile2;
        private System.Windows.Forms.Label lblFile2;
        private System.Windows.Forms.GroupBox grpDefaultDirectory;
        private C1.Win.C1Input.C1Button btnBrowseFavaritePath;
        private C1.Win.C1Input.C1TextBox txtFavoriteDirectory;
        private System.Windows.Forms.Label lblStarDefaultDirectory;
        private System.Windows.Forms.RadioButton rdoFavoriteDirectory;
        private System.Windows.Forms.RadioButton rdoDefaultDirectory;
        private C1.Win.C1Input.C1Button btnClear2;
        private C1.Win.C1Input.C1Button btnClear1;
        private C1.Win.C1Input.C1CheckBox chkShowVersion;
        private C1.Win.C1Input.C1CheckBox chkCtrlMouseWheel;
        private System.Windows.Forms.Label lblStarShowVersion;
        private System.Windows.Forms.Label lblAutoReplaceInfo2;
        private C1.Win.C1Input.C1CheckBox chkShowIndentGuide;
        private System.Windows.Forms.Label lblStarShowIndentGuide;
        private C1.Win.C1Input.C1ComboBox cboTabWidth;
        private System.Windows.Forms.Label lblTabWidth;
        private System.Windows.Forms.GroupBox grpIndent;
        private System.Windows.Forms.Label lblStarTabWidth;
        private C1.Win.C1Input.C1CheckBox chkReplaceTabWithSpace;
        private System.Windows.Forms.ToolStripButton btnShowIndentGuide;
        private System.Windows.Forms.ToolStripButton btnShowIndentGuide2;
        private C1.Win.C1Input.C1CheckBox chkSetFocusAfterQuery;
        private System.Windows.Forms.Label lblStyle3;
        private System.Windows.Forms.Label lblStyle2;
        private System.Windows.Forms.Label lblStyle1;
        private System.Windows.Forms.Label lblStarMainFormTab;
        private System.Windows.Forms.GroupBox grpSchemaInformation;
        private C1.Win.C1Input.C1CheckBox chkShowColumnInfo;
        private C1.Win.C1Input.C1CheckBox chkSortByColumnName;
        private C1.Win.C1Input.C1Button btnHelp_ShowColumnInfo;
        private System.Windows.Forms.Label lblStarShowColumnInfo;
        private System.Windows.Forms.Label lblStarSortByColumnName;
        private C1.Win.C1Input.C1CheckBox chkDefaultTabSchemaInformation;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox grpStatementCompletion;
        private C1.Win.C1Input.C1Button btnHelp_SavePoint;
        private System.Windows.Forms.CheckBox chkSavePoint;
        private System.Windows.Forms.CheckBox chkAutoListMembers;
        private JasonLibrary.UI.Controls.ScintillaEditor editorAutoReplace;
        private System.Windows.Forms.NumericUpDown nudMinFragmentLength;
        private C1.Win.C1Input.C1CheckBox chkFirstCharChecking;
        private C1.Win.C1Input.C1CheckBox chkShowGroupingRow;
        private System.Windows.Forms.Label lblStarShowIP;
        private C1.Win.C1Input.C1CheckBox chkShowIP;
        private System.Windows.Forms.Label lblStarShowDatabaseName;
        private C1.Win.C1Input.C1CheckBox chkShowDatabaseName;
        private System.Windows.Forms.GroupBox grpBackup;
        private C1.Win.C1Input.C1Button btnBrowseBackupPath;
        private C1.Win.C1Input.C1TextBox txtBackupPath;
        private System.Windows.Forms.Label lblBackupPath;
        private C1.Win.C1Input.C1CheckBox chkRememberUnsavedFiles;
        private System.Windows.Forms.Label lblStarBackup;
        private C1.Win.C1Input.C1CheckBox chkEnableBackup;
        private C1.Win.C1Input.C1CheckBox c1CheckBox2;
        private C1.Win.C1Input.C1Button btnBackupPathOpenFolder;
        private C1.Win.C1Input.C1CheckBox chkAskMeBeforeOpenUnsavedFiles;
        private System.Windows.Forms.GroupBox grpCommitRollbackIcon;
        private System.Windows.Forms.Label lblStarCommitRollbackIcon;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle2;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label lblCommitRollbackIcon;
        private C1.Win.C1Input.C1Button btnHelp_ColumnComment;
        private C1.Win.C1Input.C1CheckBox chkShowColumnComment;
        private System.Windows.Forms.PictureBox pictureBox6;
        private System.Windows.Forms.PictureBox pictureBox5;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle3;
        private C1.Win.C1Input.C1Button btnClear3;
        private System.Windows.Forms.PictureBox pictureBox7;
        private System.Windows.Forms.PictureBox pictureBox8;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle4;
        private C1.Win.C1Input.C1CheckBox c1CheckBox3;
        private C1.Win.C1Input.C1CheckBox c1CheckBox1;
        private C1.Win.C1Input.C1Button c1Button1;
        private System.Windows.Forms.Label lblStyle4;
        private System.Windows.Forms.RadioButton rdoStyle4;
        private C1.Win.C1Input.C1TextBox txtStringBuilderVariableName;
        private System.Windows.Forms.Label lblStringBuilderVariableName;
        private C1.Win.C1Command.C1DockingTab c1DockingTab2;
        private C1.Win.C1Command.C1DockingTabPage tabDataGridAppearance;
        private C1.Win.C1Command.C1DockingTabPage tabDataGridInteraction;
        private C1.Win.C1Input.C1CheckBox chkRawDataMode;
        private C1.Win.C1Input.C1Button btnHelp_RawDataMode;
        private System.Windows.Forms.Label lblStarDirection;
        private C1.Win.C1Input.C1CheckBox chkDirection;
        private C1.Win.C1Input.C1ComboBox cboDirection;
        private C1.Win.C1Input.C1Button btnHelp_AutoListMembers;
        private C1.Win.C1Input.C1Button btnHelp_EnableAutoComplete;
        private C1.Win.C1Input.C1Button btnHelp_EnableAutoReplace;
        private C1.Win.C1Input.C1Button btnHelp_Symbol;
        private C1.Win.C1Command.C1DockingTabPage tabDataGridQueryResultBehavior;
        private C1.Win.C1Input.C1Button btnHelp_PagedQuery;
        private C1.Win.C1Input.C1CheckBox chkPagedQuery;
        private System.Windows.Forms.Label lblRowsPerPage;
        private C1.Win.C1Input.C1ComboBox cboRowsPerPage;
        private C1.Win.C1Input.C1CheckBox chkAppendQueryResult;
        private C1.Win.C1Input.C1Button btnHelp_AppendQueryResult;
        private System.Windows.Forms.GroupBox grpHighlightStyle;
        private C1.Win.C1Input.C1ComboBox cboHighlightStyle;
        private C1.Win.C1Input.C1ComboBox cboHighlightAlpha;
        private C1.Win.C1Input.C1ComboBox cboHighlightOutlineAlpha;
        private System.Windows.Forms.Label lblStarHighlight;
        private System.Windows.Forms.Label lblHighlightColorAlpha;
        private System.Windows.Forms.Label lblHighlightColorOutlineAlpha;
        private System.Windows.Forms.Label lblHighlightColorStyle;
        private System.Windows.Forms.Panel pnlHighlightForeColor;
        private System.Windows.Forms.Label lblHighlightColorForeColor;
        private C1.Win.C1Input.C1Button btnHelp_SelectCurrentSqlBlock;
        private System.Windows.Forms.Label lblGlobalOverview;
        private System.Windows.Forms.Label lblCommitRollbackIconInfo;
        private System.Windows.Forms.Label lblMaxEntriesInfo;
        private System.Windows.Forms.Label lblStarNullValueStyle;
        private C1.Win.C1Input.C1CheckBox c1CheckBox6;
        private System.Windows.Forms.Label label1;
        private C1.Win.C1Input.C1CheckBox c1CheckBox4;
        private C1.Win.C1Input.C1CheckBox c1CheckBox5;
        private C1.Win.C1Command.C1DockingTab c1DockingTab4;
        private C1.Win.C1Command.C1DockingTabPage tabGlobalSettings;
        private C1.Win.C1Command.C1DockingTabPage tabSafetySettings;
        private C1.Win.C1Input.C1CheckBox c1CheckBox18;
        private C1.Win.C1Input.C1CheckBox c1CheckBox19;
        private C1.Win.C1Input.C1CheckBox c1CheckBox20;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label11;
        private C1.Win.C1Input.C1ComboBox c1ComboBox10;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label label14;
        private C1.Win.C1Command.C1DockingTab c1DockingTab5;
        private C1.Win.C1Command.C1DockingTabPage c1DockingTabPage7;
        private C1.Win.C1Input.C1CheckBox c1CheckBox25;
        private C1.Win.C1Input.C1CheckBox c1CheckBox26;
        private System.Windows.Forms.GroupBox grpConnectionSafety;
        private System.Windows.Forms.Label lblReminder5Minutes;
        private C1.Win.C1Input.C1CheckBox chkPendingWarning;
        private C1.Win.C1Input.C1CheckBox chkDisconnectAfterSelect;
        private C1.Win.C1Input.C1Button btnHelp_PendingWarning;
        private C1.Win.C1Input.C1Button btnHelp_DisconnectAfterSelect;
        private System.Windows.Forms.PictureBox pictureBox9;
        private System.Windows.Forms.PictureBox pictureBox10;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle6;
        private System.Windows.Forms.PictureBox pictureBox11;
        private System.Windows.Forms.PictureBox pictureBox12;
        private System.Windows.Forms.RadioButton rdoCommitRollbackIconStyle5;
        private System.Windows.Forms.GroupBox grpLargeTextDisplayStrategy;
        private System.Windows.Forms.GroupBox grpLargeBinaryDisplayStrategy;
        private C1.Win.C1Input.C1ComboBox cboLargeTextPreviewLength;
        private System.Windows.Forms.Label lblLargeText;
        private System.Windows.Forms.Label lblLargeTextNote;
        private System.Windows.Forms.Label lblLargeBinary;
        private System.Windows.Forms.Label lblPreviewLength;
        private System.Windows.Forms.Label lblCharacters;
        private C1.Win.C1Input.C1ComboBox cboResultCopyFieldSeparator;
        private System.Windows.Forms.Label lblResultCopyFieldSeparator;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private C1.Win.C1Command.C1DockingTabPage tabUpdateSettings;
        private System.Windows.Forms.GroupBox grpUpdateInformationSource;
        private C1.Win.C1Input.C1Button btnHelp_LocalUpdateFolder;
        private C1.Win.C1Input.C1Button btnLocalFolderOpenFolder;
        private C1.Win.C1Input.C1Button btnBrowseLocalFolder;
        private C1.Win.C1Input.C1TextBox txtLocalFolder;
        private System.Windows.Forms.RadioButton rdoUpdateSourceLocal;
        private System.Windows.Forms.RadioButton rdoUpdateSourceGitHub;
        private System.Windows.Forms.RadioButton rdoUpdateSourceOfficialWebsite;
        private System.Windows.Forms.Label lblGitHubUpdateUrl;
        private System.Windows.Forms.Label lblOfficialWebsiteUpdateUrl;
        private System.Windows.Forms.GroupBox grpSqlHistory;
        private C1.Win.C1Input.C1ComboBox cboSqlHistoryRetentionDays;
        private System.Windows.Forms.CheckBox chkAutoDeleteSqlHistory;
    }
}
