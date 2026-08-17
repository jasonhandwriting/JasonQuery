using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    sealed partial class GenerateSqlForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenerateSqlForm));
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnCopyToClipboard = new C1.Win.C1Input.C1Button();
            this.grpFunction = new System.Windows.Forms.GroupBox();
            this.rdoCreate = new System.Windows.Forms.RadioButton();
            this.rdoTruncate = new System.Windows.Forms.RadioButton();
            this.rdoRename = new System.Windows.Forms.RadioButton();
            this.rdoDrop = new System.Windows.Forms.RadioButton();
            this.rdoAlter = new System.Windows.Forms.RadioButton();
            this.rdoSelectStar = new System.Windows.Forms.RadioButton();
            this.rdoDelete = new System.Windows.Forms.RadioButton();
            this.rdoUpdate = new System.Windows.Forms.RadioButton();
            this.rdoInsert = new System.Windows.Forms.RadioButton();
            this.rdoSelect = new System.Windows.Forms.RadioButton();
            this.grpOptions = new System.Windows.Forms.GroupBox();
            this.chkDisplayAsParameter = new System.Windows.Forms.CheckBox();
            this.chkEncloseGraveAccent = new System.Windows.Forms.CheckBox();
            this.chkEncloseBrackets = new System.Windows.Forms.CheckBox();
            this.txtAliasName = new C1.Win.C1Input.C1TextBox();
            this.chkAliasName = new System.Windows.Forms.CheckBox();
            this.chkLimitInfo = new System.Windows.Forms.CheckBox();
            this.chkColumnTypeInfo = new System.Windows.Forms.CheckBox();
            this.chkPKInfo = new System.Windows.Forms.CheckBox();
            this.cboNumbers = new System.Windows.Forms.ComboBox();
            this.lblNumbers = new System.Windows.Forms.Label();
            this.grpConvertCase = new System.Windows.Forms.GroupBox();
            this.rdoDoNothing = new System.Windows.Forms.RadioButton();
            this.rdoLowerKeywords = new System.Windows.Forms.RadioButton();
            this.rdoLowerAll = new System.Windows.Forms.RadioButton();
            this.rdoUpperKeywords = new System.Windows.Forms.RadioButton();
            this.rdoUpperAll = new System.Windows.Forms.RadioButton();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.editor = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.btnShowSql = new C1.Win.C1Input.C1Button();
            this.rdoTable = new System.Windows.Forms.RadioButton();
            this.rdoView = new System.Windows.Forms.RadioButton();
            this.cboTable = new C1.Win.C1Input.C1ComboBox();
            this.cboView = new C1.Win.C1Input.C1ComboBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.cboSchema = new C1.Win.C1Input.C1ComboBox();
            this.lblSchema = new System.Windows.Forms.Label();
            this.txtDatabase = new C1.Win.C1Input.C1TextBox();
            this.c1GridView = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.c1GridTable = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.c1Grid = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.btnSelectObject = new C1.Win.C1Input.C1Button();
            this.btnPreview = new C1.Win.C1Input.C1Button();
            this.btnSelectAll = new C1.Win.C1Input.C1Button();
            this.btnUnselectAll = new C1.Win.C1Input.C1Button();
            this.btnPasteToQueryEditor = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopyToClipboard)).BeginInit();
            this.grpFunction.SuspendLayout();
            this.grpOptions.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtAliasName)).BeginInit();
            this.grpConvertCase.SuspendLayout();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnShowSql)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSchema)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDatabase)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridTable)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectObject)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPasteToQueryEditor)).BeginInit();
            this.SuspendLayout();
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(729, 642);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(81, 36);
            this.btnClose.TabIndex = 60;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnCopyToClipboard
            // 
            this.btnCopyToClipboard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopyToClipboard.Location = new System.Drawing.Point(535, 642);
            this.btnCopyToClipboard.Name = "btnCopyToClipboard";
            this.btnCopyToClipboard.Size = new System.Drawing.Size(138, 36);
            this.btnCopyToClipboard.TabIndex = 64;
            this.btnCopyToClipboard.Text = "Copy to Clipboard";
            this.btnCopyToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyToClipboard.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCopyToClipboard.Click += new System.EventHandler(this.btnCopyToClipboard_Click);
            // 
            // grpFunction
            // 
            this.grpFunction.AccessibleDescription = "";
            this.grpFunction.Controls.Add(this.rdoCreate);
            this.grpFunction.Controls.Add(this.rdoTruncate);
            this.grpFunction.Controls.Add(this.rdoRename);
            this.grpFunction.Controls.Add(this.rdoDrop);
            this.grpFunction.Controls.Add(this.rdoAlter);
            this.grpFunction.Controls.Add(this.rdoSelectStar);
            this.grpFunction.Controls.Add(this.rdoDelete);
            this.grpFunction.Controls.Add(this.rdoUpdate);
            this.grpFunction.Controls.Add(this.rdoInsert);
            this.grpFunction.Controls.Add(this.rdoSelect);
            this.grpFunction.Location = new System.Drawing.Point(13, 69);
            this.grpFunction.Name = "grpFunction";
            this.grpFunction.Size = new System.Drawing.Size(226, 150);
            this.grpFunction.TabIndex = 65;
            this.grpFunction.TabStop = false;
            this.c1ThemeController1.SetTheme(this.grpFunction, "(default)");
            this.grpFunction.MouseHover += new System.EventHandler(this.GroupBoxe_FocusedOrClick);
            // 
            // rdoCreate
            // 
            this.rdoCreate.AutoSize = true;
            this.rdoCreate.Location = new System.Drawing.Point(102, 45);
            this.rdoCreate.Name = "rdoCreate";
            this.rdoCreate.Size = new System.Drawing.Size(113, 20);
            this.rdoCreate.TabIndex = 9;
            this.rdoCreate.Tag = "";
            this.rdoCreate.Text = "Create/Replace";
            this.c1ThemeController1.SetTheme(this.rdoCreate, "(default)");
            this.rdoCreate.UseVisualStyleBackColor = true;
            this.rdoCreate.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoTruncate
            // 
            this.rdoTruncate.AutoSize = true;
            this.rdoTruncate.Location = new System.Drawing.Point(102, 120);
            this.rdoTruncate.Name = "rdoTruncate";
            this.rdoTruncate.Size = new System.Drawing.Size(74, 20);
            this.rdoTruncate.TabIndex = 8;
            this.rdoTruncate.Tag = "";
            this.rdoTruncate.Text = "Truncate";
            this.c1ThemeController1.SetTheme(this.rdoTruncate, "(default)");
            this.rdoTruncate.UseVisualStyleBackColor = true;
            this.rdoTruncate.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoRename
            // 
            this.rdoRename.AutoSize = true;
            this.rdoRename.Location = new System.Drawing.Point(102, 95);
            this.rdoRename.Name = "rdoRename";
            this.rdoRename.Size = new System.Drawing.Size(72, 20);
            this.rdoRename.TabIndex = 7;
            this.rdoRename.Tag = "";
            this.rdoRename.Text = "Rename";
            this.c1ThemeController1.SetTheme(this.rdoRename, "(default)");
            this.rdoRename.UseVisualStyleBackColor = true;
            this.rdoRename.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoDrop
            // 
            this.rdoDrop.AutoSize = true;
            this.rdoDrop.Location = new System.Drawing.Point(102, 70);
            this.rdoDrop.Name = "rdoDrop";
            this.rdoDrop.Size = new System.Drawing.Size(54, 20);
            this.rdoDrop.TabIndex = 6;
            this.rdoDrop.Tag = "";
            this.rdoDrop.Text = "Drop";
            this.c1ThemeController1.SetTheme(this.rdoDrop, "(default)");
            this.rdoDrop.UseVisualStyleBackColor = true;
            this.rdoDrop.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoAlter
            // 
            this.rdoAlter.AutoSize = true;
            this.rdoAlter.Location = new System.Drawing.Point(102, 20);
            this.rdoAlter.Name = "rdoAlter";
            this.rdoAlter.Size = new System.Drawing.Size(51, 20);
            this.rdoAlter.TabIndex = 5;
            this.rdoAlter.Tag = "";
            this.rdoAlter.Text = "Alter";
            this.c1ThemeController1.SetTheme(this.rdoAlter, "(default)");
            this.rdoAlter.UseVisualStyleBackColor = true;
            this.rdoAlter.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoSelectStar
            // 
            this.rdoSelectStar.AccessibleDescription = "";
            this.rdoSelectStar.AutoSize = true;
            this.rdoSelectStar.Location = new System.Drawing.Point(16, 20);
            this.rdoSelectStar.Name = "rdoSelectStar";
            this.rdoSelectStar.Size = new System.Drawing.Size(67, 20);
            this.rdoSelectStar.TabIndex = 4;
            this.rdoSelectStar.Tag = "";
            this.rdoSelectStar.Text = "Select *";
            this.c1ThemeController1.SetTheme(this.rdoSelectStar, "(default)");
            this.rdoSelectStar.UseVisualStyleBackColor = true;
            this.rdoSelectStar.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoDelete
            // 
            this.rdoDelete.AutoSize = true;
            this.rdoDelete.Location = new System.Drawing.Point(16, 120);
            this.rdoDelete.Name = "rdoDelete";
            this.rdoDelete.Size = new System.Drawing.Size(62, 20);
            this.rdoDelete.TabIndex = 3;
            this.rdoDelete.Tag = "";
            this.rdoDelete.Text = "Delete";
            this.c1ThemeController1.SetTheme(this.rdoDelete, "(default)");
            this.rdoDelete.UseVisualStyleBackColor = true;
            this.rdoDelete.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoUpdate
            // 
            this.rdoUpdate.AutoSize = true;
            this.rdoUpdate.Location = new System.Drawing.Point(16, 95);
            this.rdoUpdate.Name = "rdoUpdate";
            this.rdoUpdate.Size = new System.Drawing.Size(68, 20);
            this.rdoUpdate.TabIndex = 2;
            this.rdoUpdate.Tag = "";
            this.rdoUpdate.Text = "Update";
            this.c1ThemeController1.SetTheme(this.rdoUpdate, "(default)");
            this.rdoUpdate.UseVisualStyleBackColor = true;
            this.rdoUpdate.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoInsert
            // 
            this.rdoInsert.AutoSize = true;
            this.rdoInsert.Location = new System.Drawing.Point(16, 70);
            this.rdoInsert.Name = "rdoInsert";
            this.rdoInsert.Size = new System.Drawing.Size(55, 20);
            this.rdoInsert.TabIndex = 1;
            this.rdoInsert.Tag = "";
            this.rdoInsert.Text = "Insert";
            this.c1ThemeController1.SetTheme(this.rdoInsert, "(default)");
            this.rdoInsert.UseVisualStyleBackColor = true;
            this.rdoInsert.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // rdoSelect
            // 
            this.rdoSelect.AutoSize = true;
            this.rdoSelect.Location = new System.Drawing.Point(16, 45);
            this.rdoSelect.Name = "rdoSelect";
            this.rdoSelect.Size = new System.Drawing.Size(59, 20);
            this.rdoSelect.TabIndex = 0;
            this.rdoSelect.Tag = "";
            this.rdoSelect.Text = "Select";
            this.c1ThemeController1.SetTheme(this.rdoSelect, "(default)");
            this.rdoSelect.UseVisualStyleBackColor = true;
            this.rdoSelect.CheckedChanged += new System.EventHandler(this.Function_CheckedChanged);
            // 
            // grpOptions
            // 
            this.grpOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpOptions.Controls.Add(this.chkDisplayAsParameter);
            this.grpOptions.Controls.Add(this.chkEncloseGraveAccent);
            this.grpOptions.Controls.Add(this.chkEncloseBrackets);
            this.grpOptions.Controls.Add(this.txtAliasName);
            this.grpOptions.Controls.Add(this.chkAliasName);
            this.grpOptions.Controls.Add(this.chkLimitInfo);
            this.grpOptions.Controls.Add(this.chkColumnTypeInfo);
            this.grpOptions.Controls.Add(this.chkPKInfo);
            this.grpOptions.Controls.Add(this.cboNumbers);
            this.grpOptions.Controls.Add(this.lblNumbers);
            this.grpOptions.Location = new System.Drawing.Point(476, 69);
            this.grpOptions.Name = "grpOptions";
            this.grpOptions.Size = new System.Drawing.Size(334, 175);
            this.grpOptions.TabIndex = 68;
            this.grpOptions.TabStop = false;
            this.c1ThemeController1.SetTheme(this.grpOptions, "(default)");
            this.grpOptions.MouseHover += new System.EventHandler(this.GroupBoxe_FocusedOrClick);
            // 
            // chkDisplayAsParameter
            // 
            this.chkDisplayAsParameter.AutoSize = true;
            this.chkDisplayAsParameter.ForeColor = System.Drawing.Color.Green;
            this.chkDisplayAsParameter.Location = new System.Drawing.Point(129, 120);
            this.chkDisplayAsParameter.Name = "chkDisplayAsParameter";
            this.chkDisplayAsParameter.Size = new System.Drawing.Size(198, 20);
            this.chkDisplayAsParameter.TabIndex = 90;
            this.chkDisplayAsParameter.Text = "for Insert, Display as Parameter";
            this.chkDisplayAsParameter.UseVisualStyleBackColor = true;
            this.chkDisplayAsParameter.Visible = false;
            this.chkDisplayAsParameter.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // chkEncloseGraveAccent
            // 
            this.chkEncloseGraveAccent.AutoSize = true;
            this.chkEncloseGraveAccent.Location = new System.Drawing.Point(20, 145);
            this.chkEncloseGraveAccent.Name = "chkEncloseGraveAccent";
            this.chkEncloseGraveAccent.Size = new System.Drawing.Size(237, 20);
            this.chkEncloseGraveAccent.TabIndex = 89;
            this.chkEncloseGraveAccent.Text = "Enclose column name in grave accent";
            this.c1ThemeController1.SetTheme(this.chkEncloseGraveAccent, "(default)");
            this.chkEncloseGraveAccent.UseVisualStyleBackColor = true;
            this.chkEncloseGraveAccent.Visible = false;
            this.chkEncloseGraveAccent.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // chkEncloseBrackets
            // 
            this.chkEncloseBrackets.AutoSize = true;
            this.chkEncloseBrackets.Location = new System.Drawing.Point(20, 145);
            this.chkEncloseBrackets.Name = "chkEncloseBrackets";
            this.chkEncloseBrackets.Size = new System.Drawing.Size(253, 20);
            this.chkEncloseBrackets.TabIndex = 88;
            this.chkEncloseBrackets.Text = "Enclose column name in square brackets";
            this.c1ThemeController1.SetTheme(this.chkEncloseBrackets, "(default)");
            this.chkEncloseBrackets.UseVisualStyleBackColor = true;
            this.chkEncloseBrackets.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // txtAliasName
            // 
            this.txtAliasName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtAliasName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtAliasName.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtAliasName.Location = new System.Drawing.Point(114, 44);
            this.txtAliasName.Name = "txtAliasName";
            this.txtAliasName.Size = new System.Drawing.Size(77, 21);
            this.txtAliasName.TabIndex = 87;
            this.txtAliasName.Tag = null;
            this.txtAliasName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtAliasName.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAliasName_KeyPress);
            this.txtAliasName.Leave += new System.EventHandler(this.txtAliasName_Leave);
            // 
            // chkAliasName
            // 
            this.chkAliasName.AutoSize = true;
            this.chkAliasName.Location = new System.Drawing.Point(20, 45);
            this.chkAliasName.Name = "chkAliasName";
            this.chkAliasName.Size = new System.Drawing.Size(90, 20);
            this.chkAliasName.TabIndex = 5;
            this.chkAliasName.Text = "Alias Name";
            this.c1ThemeController1.SetTheme(this.chkAliasName, "(default)");
            this.chkAliasName.UseVisualStyleBackColor = true;
            this.chkAliasName.CheckedChanged += new System.EventHandler(this.chkAliasName_CheckedChanged);
            // 
            // chkLimitInfo
            // 
            this.chkLimitInfo.AutoSize = true;
            this.chkLimitInfo.Checked = true;
            this.chkLimitInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLimitInfo.Location = new System.Drawing.Point(20, 120);
            this.chkLimitInfo.Name = "chkLimitInfo";
            this.chkLimitInfo.Size = new System.Drawing.Size(78, 20);
            this.chkLimitInfo.TabIndex = 4;
            this.chkLimitInfo.Text = "Limit Info";
            this.c1ThemeController1.SetTheme(this.chkLimitInfo, "(default)");
            this.chkLimitInfo.UseVisualStyleBackColor = true;
            this.chkLimitInfo.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // chkColumnTypeInfo
            // 
            this.chkColumnTypeInfo.AutoSize = true;
            this.chkColumnTypeInfo.Location = new System.Drawing.Point(20, 70);
            this.chkColumnTypeInfo.Name = "chkColumnTypeInfo";
            this.chkColumnTypeInfo.Size = new System.Drawing.Size(126, 20);
            this.chkColumnTypeInfo.TabIndex = 3;
            this.chkColumnTypeInfo.Text = "Column Type Info";
            this.c1ThemeController1.SetTheme(this.chkColumnTypeInfo, "(default)");
            this.chkColumnTypeInfo.UseVisualStyleBackColor = true;
            this.chkColumnTypeInfo.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // chkPKInfo
            // 
            this.chkPKInfo.AutoSize = true;
            this.chkPKInfo.Checked = true;
            this.chkPKInfo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkPKInfo.Location = new System.Drawing.Point(20, 95);
            this.chkPKInfo.Name = "chkPKInfo";
            this.chkPKInfo.Size = new System.Drawing.Size(116, 20);
            this.chkPKInfo.TabIndex = 2;
            this.chkPKInfo.Text = "Primary Key Info";
            this.c1ThemeController1.SetTheme(this.chkPKInfo, "(default)");
            this.chkPKInfo.UseVisualStyleBackColor = true;
            this.chkPKInfo.CheckedChanged += new System.EventHandler(this.Options_CheckedChanged);
            // 
            // cboNumbers
            // 
            this.cboNumbers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboNumbers.FormattingEnabled = true;
            this.cboNumbers.Items.AddRange(new object[] {
            "1",
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "All"});
            this.cboNumbers.Location = new System.Drawing.Point(241, 15);
            this.cboNumbers.Name = "cboNumbers";
            this.cboNumbers.Size = new System.Drawing.Size(42, 24);
            this.cboNumbers.TabIndex = 1;
            this.c1ThemeController1.SetTheme(this.cboNumbers, "(default)");
            this.cboNumbers.SelectedIndexChanged += new System.EventHandler(this.cboNumbers_SelectedIndexChanged);
            // 
            // lblNumbers
            // 
            this.lblNumbers.AutoSize = true;
            this.lblNumbers.Location = new System.Drawing.Point(17, 20);
            this.lblNumbers.Name = "lblNumbers";
            this.lblNumbers.Size = new System.Drawing.Size(202, 16);
            this.lblNumbers.TabIndex = 0;
            this.lblNumbers.Text = "Number of column names per line:";
            this.c1ThemeController1.SetTheme(this.lblNumbers, "(default)");
            // 
            // grpConvertCase
            // 
            this.grpConvertCase.Controls.Add(this.rdoDoNothing);
            this.grpConvertCase.Controls.Add(this.rdoLowerKeywords);
            this.grpConvertCase.Controls.Add(this.rdoLowerAll);
            this.grpConvertCase.Controls.Add(this.rdoUpperKeywords);
            this.grpConvertCase.Controls.Add(this.rdoUpperAll);
            this.grpConvertCase.Location = new System.Drawing.Point(245, 69);
            this.grpConvertCase.Name = "grpConvertCase";
            this.grpConvertCase.Size = new System.Drawing.Size(225, 150);
            this.grpConvertCase.TabIndex = 67;
            this.grpConvertCase.TabStop = false;
            this.c1ThemeController1.SetTheme(this.grpConvertCase, "(default)");
            this.grpConvertCase.MouseHover += new System.EventHandler(this.GroupBoxe_FocusedOrClick);
            // 
            // rdoDoNothing
            // 
            this.rdoDoNothing.AutoSize = true;
            this.rdoDoNothing.Location = new System.Drawing.Point(16, 120);
            this.rdoDoNothing.Name = "rdoDoNothing";
            this.rdoDoNothing.Size = new System.Drawing.Size(89, 20);
            this.rdoDoNothing.TabIndex = 4;
            this.rdoDoNothing.Tag = "DoNothing";
            this.rdoDoNothing.Text = "Do nothing";
            this.c1ThemeController1.SetTheme(this.rdoDoNothing, "(default)");
            this.rdoDoNothing.UseVisualStyleBackColor = true;
            this.rdoDoNothing.Visible = false;
            this.rdoDoNothing.CheckedChanged += new System.EventHandler(this.ChangeCase_CheckedChanged);
            // 
            // rdoLowerKeywords
            // 
            this.rdoLowerKeywords.AutoSize = true;
            this.rdoLowerKeywords.Location = new System.Drawing.Point(16, 95);
            this.rdoLowerKeywords.Name = "rdoLowerKeywords";
            this.rdoLowerKeywords.Size = new System.Drawing.Size(178, 20);
            this.rdoLowerKeywords.TabIndex = 3;
            this.rdoLowerKeywords.Tag = "LowerKeywords";
            this.rdoLowerKeywords.Text = "lower case - Keywords Only";
            this.c1ThemeController1.SetTheme(this.rdoLowerKeywords, "(default)");
            this.rdoLowerKeywords.UseVisualStyleBackColor = true;
            this.rdoLowerKeywords.CheckedChanged += new System.EventHandler(this.ChangeCase_CheckedChanged);
            // 
            // rdoLowerAll
            // 
            this.rdoLowerAll.AutoSize = true;
            this.rdoLowerAll.Location = new System.Drawing.Point(16, 70);
            this.rdoLowerAll.Name = "rdoLowerAll";
            this.rdoLowerAll.Size = new System.Drawing.Size(109, 20);
            this.rdoLowerAll.TabIndex = 2;
            this.rdoLowerAll.Tag = "LowerAll";
            this.rdoLowerAll.Text = "lower case - All";
            this.c1ThemeController1.SetTheme(this.rdoLowerAll, "(default)");
            this.rdoLowerAll.UseVisualStyleBackColor = true;
            this.rdoLowerAll.CheckedChanged += new System.EventHandler(this.ChangeCase_CheckedChanged);
            // 
            // rdoUpperKeywords
            // 
            this.rdoUpperKeywords.AutoSize = true;
            this.rdoUpperKeywords.Checked = true;
            this.rdoUpperKeywords.Location = new System.Drawing.Point(16, 45);
            this.rdoUpperKeywords.Name = "rdoUpperKeywords";
            this.rdoUpperKeywords.Size = new System.Drawing.Size(185, 20);
            this.rdoUpperKeywords.TabIndex = 1;
            this.rdoUpperKeywords.TabStop = true;
            this.rdoUpperKeywords.Tag = "UpperKeywords";
            this.rdoUpperKeywords.Text = "UPPER case - Keywords Only";
            this.c1ThemeController1.SetTheme(this.rdoUpperKeywords, "(default)");
            this.rdoUpperKeywords.UseVisualStyleBackColor = true;
            this.rdoUpperKeywords.CheckedChanged += new System.EventHandler(this.ChangeCase_CheckedChanged);
            // 
            // rdoUpperAll
            // 
            this.rdoUpperAll.AutoSize = true;
            this.rdoUpperAll.Location = new System.Drawing.Point(16, 20);
            this.rdoUpperAll.Name = "rdoUpperAll";
            this.rdoUpperAll.Size = new System.Drawing.Size(116, 20);
            this.rdoUpperAll.TabIndex = 0;
            this.rdoUpperAll.Tag = "UpperAll";
            this.rdoUpperAll.Text = "UPPER case - All";
            this.c1ThemeController1.SetTheme(this.rdoUpperAll, "(default)");
            this.rdoUpperAll.UseVisualStyleBackColor = true;
            this.rdoUpperAll.CheckedChanged += new System.EventHandler(this.ChangeCase_CheckedChanged);
            // 
            // grpPreview
            // 
            this.grpPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpPreview.Controls.Add(this.editor);
            this.grpPreview.Location = new System.Drawing.Point(376, 247);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(434, 379);
            this.grpPreview.TabIndex = 69;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "Preview";
            this.c1ThemeController1.SetTheme(this.grpPreview, "(default)");
            this.grpPreview.MouseHover += new System.EventHandler(this.GroupBoxe_FocusedOrClick);
            // 
            // editor
            // 
            this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editor.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editor.CaretLineVisible = true;
            this.editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editor.Location = new System.Drawing.Point(11, 20);
            this.editor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editor.Name = "editor";
            this.editor.ReadOnly = true;
            this.editor.Size = new System.Drawing.Size(413, 349);
            this.editor.Styler = null;
            this.editor.TabIndex = 42;
            this.editor.Tag = "";
            this.editor.WhitespaceSize = 3;
            this.editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // btnShowSql
            // 
            this.btnShowSql.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnShowSql.Location = new System.Drawing.Point(13, 642);
            this.btnShowSql.Name = "btnShowSql";
            this.btnShowSql.Size = new System.Drawing.Size(200, 36);
            this.btnShowSql.TabIndex = 75;
            this.btnShowSql.Text = "Show \"Get Column Info\" SQL";
            this.c1ThemeController1.SetTheme(this.btnShowSql, "(default)");
            this.btnShowSql.UseVisualStyleBackColor = true;
            this.btnShowSql.Visible = false;
            this.btnShowSql.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnShowSql.Click += new System.EventHandler(this.btnShowSql_Click);
            // 
            // rdoTable
            // 
            this.rdoTable.AccessibleDescription = "";
            this.rdoTable.AutoSize = true;
            this.rdoTable.Location = new System.Drawing.Point(332, 13);
            this.rdoTable.Name = "rdoTable";
            this.rdoTable.Size = new System.Drawing.Size(60, 20);
            this.rdoTable.TabIndex = 77;
            this.rdoTable.Tag = "";
            this.rdoTable.Text = "Table:";
            this.c1ThemeController1.SetTheme(this.rdoTable, "(default)");
            this.rdoTable.UseVisualStyleBackColor = true;
            // 
            // rdoView
            // 
            this.rdoView.AutoSize = true;
            this.rdoView.Location = new System.Drawing.Point(332, 43);
            this.rdoView.Name = "rdoView";
            this.rdoView.Size = new System.Drawing.Size(55, 20);
            this.rdoView.TabIndex = 76;
            this.rdoView.Tag = "";
            this.rdoView.Text = "View:";
            this.c1ThemeController1.SetTheme(this.rdoView, "(default)");
            this.rdoView.UseVisualStyleBackColor = true;
            // 
            // cboTable
            // 
            this.cboTable.AllowSpinLoop = false;
            this.cboTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboTable.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboTable.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboTable.GapHeight = 0;
            this.cboTable.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboTable.ItemsDisplayMember = "";
            this.cboTable.ItemsValueMember = "";
            this.cboTable.Location = new System.Drawing.Point(397, 13);
            this.cboTable.Name = "cboTable";
            this.cboTable.Size = new System.Drawing.Size(196, 21);
            this.cboTable.TabIndex = 108;
            this.cboTable.Tag = null;
            this.cboTable.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboTable, "(default)");
            this.cboTable.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboTable.SelectedIndexChanged += new System.EventHandler(this.cboTable_SelectedIndexChanged);
            this.cboTable.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboTable_BeforeDropDownOpen);
            this.cboTable.DropDownOpened += new System.EventHandler(this.cboTable_DropDownOpened);
            this.cboTable.DropDownClosed += new C1.Win.C1Input.DropDownClosedEventHandler(this.cboTable_DropDownClosed);
            this.cboTable.TextChanged += new System.EventHandler(this.cboTable_TextChanged);
            this.cboTable.Enter += new System.EventHandler(this.cboTable_Enter);
            this.cboTable.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboTable_KeyDown);
            this.cboTable.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboTable_KeyPress);
            this.cboTable.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboTable_KeyUp);
            this.cboTable.Leave += new System.EventHandler(this.cboTable_Leave);
            // 
            // cboView
            // 
            this.cboView.AllowSpinLoop = false;
            this.cboView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboView.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboView.GapHeight = 0;
            this.cboView.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboView.ItemsDisplayMember = "";
            this.cboView.ItemsValueMember = "";
            this.cboView.Location = new System.Drawing.Point(387, 43);
            this.cboView.Name = "cboView";
            this.cboView.Size = new System.Drawing.Size(196, 21);
            this.cboView.TabIndex = 109;
            this.cboView.Tag = null;
            this.cboView.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboView, "(default)");
            this.cboView.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboView.SelectedIndexChanged += new System.EventHandler(this.cboView_SelectedIndexChanged);
            this.cboView.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboView_BeforeDropDownOpen);
            this.cboView.DropDownOpened += new System.EventHandler(this.cboView_DropDownOpened);
            this.cboView.DropDownClosed += new C1.Win.C1Input.DropDownClosedEventHandler(this.cboView_DropDownClosed);
            this.cboView.TextChanged += new System.EventHandler(this.cboView_TextChanged);
            this.cboView.Enter += new System.EventHandler(this.cboView_Enter);
            this.cboView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.cboView_KeyDown);
            this.cboView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboView_KeyPress);
            this.cboView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboView_KeyUp);
            this.cboView.Leave += new System.EventHandler(this.cboView_Leave);
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.Location = new System.Drawing.Point(26, 15);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(64, 16);
            this.lblDatabase.TabIndex = 111;
            this.lblDatabase.Text = "Database:";
            this.c1ThemeController1.SetTheme(this.lblDatabase, "(default)");
            // 
            // cboSchema
            // 
            this.cboSchema.AllowSpinLoop = false;
            this.cboSchema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboSchema.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboSchema.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboSchema.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboSchema.GapHeight = 0;
            this.cboSchema.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboSchema.ItemsDisplayMember = "";
            this.cboSchema.ItemsValueMember = "";
            this.cboSchema.Location = new System.Drawing.Point(97, 43);
            this.cboSchema.Name = "cboSchema";
            this.cboSchema.Size = new System.Drawing.Size(196, 21);
            this.cboSchema.TabIndex = 114;
            this.cboSchema.Tag = null;
            this.cboSchema.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboSchema, "(default)");
            this.cboSchema.Visible = false;
            this.cboSchema.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboSchema.SelectedIndexChanged += new System.EventHandler(this.cboSchema_SelectedIndexChanged);
            // 
            // lblSchema
            // 
            this.lblSchema.AutoSize = true;
            this.lblSchema.Location = new System.Drawing.Point(26, 45);
            this.lblSchema.Name = "lblSchema";
            this.lblSchema.Size = new System.Drawing.Size(55, 16);
            this.lblSchema.TabIndex = 113;
            this.lblSchema.Text = "Schema:";
            this.c1ThemeController1.SetTheme(this.lblSchema, "(default)");
            this.lblSchema.Visible = false;
            // 
            // txtDatabase
            // 
            this.txtDatabase.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtDatabase.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtDatabase.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtDatabase.Location = new System.Drawing.Point(97, 13);
            this.txtDatabase.Name = "txtDatabase";
            this.txtDatabase.Size = new System.Drawing.Size(196, 21);
            this.txtDatabase.TabIndex = 90;
            this.txtDatabase.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtDatabase, "(default)");
            this.txtDatabase.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1GridView
            // 
            this.c1GridView.AllowFilter = false;
            this.c1GridView.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridView.AllowUpdate = false;
            this.c1GridView.AlternatingRows = true;
            this.c1GridView.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridView.CaptionHeight = 19;
            this.c1GridView.ColumnHeaders = false;
            this.c1GridView.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridView.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridView.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridView.Images"))));
            this.c1GridView.Location = new System.Drawing.Point(816, 66);
            this.c1GridView.Name = "c1GridView";
            this.c1GridView.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridView.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridView.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridView.PreviewInfo.ZoomFactor = 75D;
            this.c1GridView.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridView.PrintInfo.MeasurementPrinterName = null;
            this.c1GridView.RowHeight = 19;
            this.c1GridView.Size = new System.Drawing.Size(270, 183);
            this.c1GridView.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridView.TabIndex = 115;
            this.c1ThemeController1.SetTheme(this.c1GridView, "(default)");
            this.c1GridView.UseCompatibleTextRendering = false;
            this.c1GridView.Visible = false;
            this.c1GridView.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridView_KeyPress);
            this.c1GridView.Leave += new System.EventHandler(this.c1GridView_Leave);
            this.c1GridView.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridView_MouseDoubleClick);
            this.c1GridView.PropBag = resources.GetString("c1GridView.PropBag");
            // 
            // c1GridTable
            // 
            this.c1GridTable.AllowFilter = false;
            this.c1GridTable.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.IndividualRows;
            this.c1GridTable.AllowUpdate = false;
            this.c1GridTable.AlternatingRows = true;
            this.c1GridTable.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridTable.CaptionHeight = 19;
            this.c1GridTable.ColumnHeaders = false;
            this.c1GridTable.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridTable.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridTable.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridTable.Images"))));
            this.c1GridTable.Location = new System.Drawing.Point(816, 36);
            this.c1GridTable.Name = "c1GridTable";
            this.c1GridTable.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridTable.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridTable.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridTable.PreviewInfo.ZoomFactor = 75D;
            this.c1GridTable.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridTable.PrintInfo.MeasurementPrinterName = null;
            this.c1GridTable.RowHeight = 19;
            this.c1GridTable.Size = new System.Drawing.Size(270, 183);
            this.c1GridTable.TabAction = C1.Win.C1TrueDBGrid.TabActionEnum.GridNavigation;
            this.c1GridTable.TabIndex = 116;
            this.c1ThemeController1.SetTheme(this.c1GridTable, "(default)");
            this.c1GridTable.UseCompatibleTextRendering = false;
            this.c1GridTable.Visible = false;
            this.c1GridTable.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.c1GridTable_KeyPress);
            this.c1GridTable.Leave += new System.EventHandler(this.c1GridTable_Leave);
            this.c1GridTable.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridTable_MouseDoubleClick);
            this.c1GridTable.PropBag = resources.GetString("c1GridTable.PropBag");
            // 
            // c1Grid
            // 
            this.c1Grid.AlternatingRows = true;
            this.c1Grid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.c1Grid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1Grid.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1Grid.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1Grid.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1Grid.Images.Add(((System.Drawing.Image)(resources.GetObject("c1Grid.Images"))));
            this.c1Grid.Location = new System.Drawing.Point(13, 267);
            this.c1Grid.Name = "c1Grid";
            this.c1Grid.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1Grid.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1Grid.PreviewInfo.ZoomFactor = 75D;
            this.c1Grid.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1Grid.PrintInfo.MeasurementPrinterName = null;
            this.c1Grid.RowHeight = 19;
            this.c1Grid.Size = new System.Drawing.Size(348, 358);
            this.c1Grid.TabIndex = 70;
            this.c1Grid.Text = "c1TrueDBGrid2";
            this.c1Grid.UseCompatibleTextRendering = false;
            this.c1Grid.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1Grid_FetchCellStyle);
            this.c1Grid.PropBag = resources.GetString("c1Grid.PropBag");
            // 
            // btnSelectObject
            // 
            this.btnSelectObject.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnSelectObject.Location = new System.Drawing.Point(619, 13);
            this.btnSelectObject.Name = "btnSelectObject";
            this.btnSelectObject.Size = new System.Drawing.Size(81, 50);
            this.btnSelectObject.TabIndex = 110;
            this.btnSelectObject.Text = "&Select";
            this.btnSelectObject.UseVisualStyleBackColor = true;
            this.btnSelectObject.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSelectObject.Click += new System.EventHandler(this.btnSelectObject_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.AutoSize = true;
            this.btnPreview.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnPreview.Location = new System.Drawing.Point(293, 232);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(68, 26);
            this.btnPreview.TabIndex = 71;
            this.btnPreview.Text = "&Preview";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.AutoSize = true;
            this.btnSelectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSelectAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectAll.Location = new System.Drawing.Point(13, 232);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(76, 26);
            this.btnSelectAll.TabIndex = 72;
            this.btnSelectAll.Tag = "SelectAll";
            this.btnSelectAll.Text = "Select &All";
            this.btnSelectAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnUnselectAll
            // 
            this.btnUnselectAll.AutoSize = true;
            this.btnUnselectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUnselectAll.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnselectAll.Location = new System.Drawing.Point(128, 232);
            this.btnUnselectAll.Name = "btnUnselectAll";
            this.btnUnselectAll.Size = new System.Drawing.Size(91, 26);
            this.btnUnselectAll.TabIndex = 73;
            this.btnUnselectAll.Tag = "UnselectAll";
            this.btnUnselectAll.Text = "&Unselect All";
            this.btnUnselectAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnselectAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUnselectAll.UseVisualStyleBackColor = true;
            this.btnUnselectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnUnselectAll.Click += new System.EventHandler(this.btnSelect_Click);
            // 
            // btnPasteToQueryEditor
            // 
            this.btnPasteToQueryEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPasteToQueryEditor.Location = new System.Drawing.Point(343, 642);
            this.btnPasteToQueryEditor.Name = "btnPasteToQueryEditor";
            this.btnPasteToQueryEditor.Size = new System.Drawing.Size(169, 36);
            this.btnPasteToQueryEditor.TabIndex = 74;
            this.btnPasteToQueryEditor.Text = "Paste to Query Editor";
            this.btnPasteToQueryEditor.UseVisualStyleBackColor = true;
            this.btnPasteToQueryEditor.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnPasteToQueryEditor.Click += new System.EventHandler(this.btnPasteToQueryEditor_Click);
            // 
            // GenerateSqlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(824, 694);
            this.Controls.Add(this.c1GridTable);
            this.Controls.Add(this.c1GridView);
            this.Controls.Add(this.txtDatabase);
            this.Controls.Add(this.cboSchema);
            this.Controls.Add(this.lblSchema);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.btnSelectObject);
            this.Controls.Add(this.cboView);
            this.Controls.Add(this.cboTable);
            this.Controls.Add(this.rdoTable);
            this.Controls.Add(this.rdoView);
            this.Controls.Add(this.btnShowSql);
            this.Controls.Add(this.btnPasteToQueryEditor);
            this.Controls.Add(this.btnUnselectAll);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.c1Grid);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.grpOptions);
            this.Controls.Add(this.grpConvertCase);
            this.Controls.Add(this.grpFunction);
            this.Controls.Add(this.btnCopyToClipboard);
            this.Controls.Add(this.btnClose);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(840, 702);
            this.Name = "GenerateSqlForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Generate SQL Statement";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.Form_MouseClick);
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopyToClipboard)).EndInit();
            this.grpFunction.ResumeLayout(false);
            this.grpFunction.PerformLayout();
            this.grpOptions.ResumeLayout(false);
            this.grpOptions.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtAliasName)).EndInit();
            this.grpConvertCase.ResumeLayout(false);
            this.grpConvertCase.PerformLayout();
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnShowSql)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSchema)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtDatabase)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridTable)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectObject)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPasteToQueryEditor)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnCopyToClipboard;
        private System.Windows.Forms.GroupBox grpFunction;
        private System.Windows.Forms.RadioButton rdoUpdate;
        private System.Windows.Forms.RadioButton rdoInsert;
        private System.Windows.Forms.RadioButton rdoSelect;
        private System.Windows.Forms.RadioButton rdoDelete;
        private System.Windows.Forms.GroupBox grpOptions;
        private System.Windows.Forms.ComboBox cboNumbers;
        private System.Windows.Forms.Label lblNumbers;
        private System.Windows.Forms.GroupBox grpConvertCase;
        private System.Windows.Forms.RadioButton rdoLowerAll;
        private System.Windows.Forms.RadioButton rdoUpperKeywords;
        private System.Windows.Forms.RadioButton rdoUpperAll;
        private System.Windows.Forms.CheckBox chkColumnTypeInfo;
        private System.Windows.Forms.CheckBox chkPKInfo;
        private System.Windows.Forms.GroupBox grpPreview;
        private JasonLibrary.UI.Controls.ScintillaEditor editor;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1Grid;
        private C1.Win.C1Input.C1Button btnPreview;
        private C1.Win.C1Input.C1Button btnSelectAll;
        private C1.Win.C1Input.C1Button btnUnselectAll;
        private System.Windows.Forms.RadioButton rdoCreate;
        private System.Windows.Forms.RadioButton rdoTruncate;
        private System.Windows.Forms.RadioButton rdoRename;
        private System.Windows.Forms.RadioButton rdoDrop;
        private System.Windows.Forms.RadioButton rdoAlter;
        private System.Windows.Forms.RadioButton rdoSelectStar;
        private C1.Win.C1Input.C1Button btnPasteToQueryEditor;
        private System.Windows.Forms.RadioButton rdoLowerKeywords;
        private System.Windows.Forms.CheckBox chkLimitInfo;
        private System.Windows.Forms.CheckBox chkAliasName;
        private C1.Win.C1Input.C1TextBox txtAliasName;
        private System.Windows.Forms.CheckBox chkEncloseBrackets;
        private System.Windows.Forms.CheckBox chkEncloseGraveAccent;
        private System.Windows.Forms.RadioButton rdoDoNothing;
        private C1.Win.C1Input.C1Button btnShowSql;
        private System.Windows.Forms.RadioButton rdoTable;
        private System.Windows.Forms.RadioButton rdoView;
        private C1.Win.C1Input.C1ComboBox cboTable;
        private C1.Win.C1Input.C1ComboBox cboView;
        private C1.Win.C1Input.C1Button btnSelectObject;
        private System.Windows.Forms.Label lblDatabase;
        private C1.Win.C1Input.C1ComboBox cboSchema;
        private System.Windows.Forms.Label lblSchema;
        private C1.Win.C1Input.C1TextBox txtDatabase;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridView;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridTable;
        private System.Windows.Forms.CheckBox chkDisplayAsParameter;
    }
}