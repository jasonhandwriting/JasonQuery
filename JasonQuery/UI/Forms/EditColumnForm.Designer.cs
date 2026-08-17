namespace JasonQuery.UI.Forms
{
    partial class EditColumnForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditColumnForm));
            this.tabColumn = new C1.Win.C1Command.C1DockingTab();
            this.tabAdd = new C1.Win.C1Command.C1DockingTabPage();
            this.lblAddValidationMessage = new System.Windows.Forms.Label();
            this.txtResolvedType_Add = new System.Windows.Forms.TextBox();
            this.lblResolvedType_Add = new System.Windows.Forms.Label();
            this.txtDefaultValue_Add = new System.Windows.Forms.TextBox();
            this.cboDefaultKind_Add = new System.Windows.Forms.ComboBox();
            this.lblDefaultKind_Add = new System.Windows.Forms.Label();
            this.cboOracleLengthSemantics_Add = new System.Windows.Forms.ComboBox();
            this.lblOracleLengthSemantics_Add = new System.Windows.Forms.Label();
            this.txtParameter2_Add = new System.Windows.Forms.TextBox();
            this.lblParameter2_Add = new System.Windows.Forms.Label();
            this.txtParameter1_Add = new System.Windows.Forms.TextBox();
            this.lblParameter1_Add = new System.Windows.Forms.Label();
            this.chkNullAllowed_Add = new System.Windows.Forms.CheckBox();
            this.cboDataType_Add = new System.Windows.Forms.ComboBox();
            this.lblDataType_Add = new System.Windows.Forms.Label();
            this.txtColumnName_Add = new C1.Win.C1Input.C1TextBox();
            this.lblColumnName_Add = new System.Windows.Forms.Label();
            this.lblAddInfo = new System.Windows.Forms.Label();
            this.tabComment = new C1.Win.C1Command.C1DockingTabPage();
            this.txtComment = new C1.Win.C1Input.C1TextBox();
            this.txtColumnName_Comment = new C1.Win.C1Input.C1TextBox();
            this.lblComment = new System.Windows.Forms.Label();
            this.lblColumnName_Comment = new System.Windows.Forms.Label();
            this.lblCommentInfo = new System.Windows.Forms.Label();
            this.tabDrop = new C1.Win.C1Command.C1DockingTabPage();
            this.txtColumnName_Drop = new C1.Win.C1Input.C1TextBox();
            this.lblColumnName_Drop = new System.Windows.Forms.Label();
            this.lblDropInfo = new System.Windows.Forms.Label();
            this.tabRename = new C1.Win.C1Command.C1DockingTabPage();
            this.txtNewColumnName = new C1.Win.C1Input.C1TextBox();
            this.txtColumnName_Rename = new C1.Win.C1Input.C1TextBox();
            this.lblNewColumnName = new System.Windows.Forms.Label();
            this.lblColumnName_Rename = new System.Windows.Forms.Label();
            this.lblRenameInfo = new System.Windows.Forms.Label();
            this.grpSqlPreview = new System.Windows.Forms.GroupBox();
            this.editorSqlPreview = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.tsTool = new System.Windows.Forms.ToolStrip();
            this.btnSelectAll = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2 = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2 = new System.Windows.Forms.ToolStripButton();
            this.btnZoomIn = new System.Windows.Forms.ToolStripButton();
            this.btnZoomOut = new System.Windows.Forms.ToolStripButton();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnExecute = new C1.Win.C1Input.C1Button();
            this.lblPrompt1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblPrompt2 = new System.Windows.Forms.Label();
            this.txtTableName = new C1.Win.C1Input.C1TextBox();
            this.lblTableName = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.tabColumn)).BeginInit();
            this.tabColumn.SuspendLayout();
            this.tabAdd.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Add)).BeginInit();
            this.tabComment.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtComment)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Comment)).BeginInit();
            this.tabDrop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Drop)).BeginInit();
            this.tabRename.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewColumnName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Rename)).BeginInit();
            this.grpSqlPreview.SuspendLayout();
            this.tsTool.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExecute)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTableName)).BeginInit();
            this.SuspendLayout();
            // 
            // tabColumn
            // 
            this.tabColumn.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabColumn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(239)))), ((int)(((byte)(246)))), ((int)(((byte)(253)))));
            this.tabColumn.Controls.Add(this.tabAdd);
            this.tabColumn.Controls.Add(this.tabComment);
            this.tabColumn.Controls.Add(this.tabDrop);
            this.tabColumn.Controls.Add(this.tabRename);
            this.tabColumn.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.tabColumn.Location = new System.Drawing.Point(9, 13);
            this.tabColumn.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabColumn.Name = "tabColumn";
            this.tabColumn.ShowTabs = false;
            this.tabColumn.Size = new System.Drawing.Size(701, 300);
            this.tabColumn.TabIndex = 0;
            this.tabColumn.TabsSpacing = 5;
            this.tabColumn.VisualStyle = C1.Win.C1Command.VisualStyle.Custom;
            // 
            // tabAdd
            // 
            this.tabAdd.BackColor = System.Drawing.Color.White;
            this.tabAdd.Controls.Add(this.lblAddValidationMessage);
            this.tabAdd.Controls.Add(this.txtResolvedType_Add);
            this.tabAdd.Controls.Add(this.lblResolvedType_Add);
            this.tabAdd.Controls.Add(this.txtDefaultValue_Add);
            this.tabAdd.Controls.Add(this.cboDefaultKind_Add);
            this.tabAdd.Controls.Add(this.lblDefaultKind_Add);
            this.tabAdd.Controls.Add(this.cboOracleLengthSemantics_Add);
            this.tabAdd.Controls.Add(this.lblOracleLengthSemantics_Add);
            this.tabAdd.Controls.Add(this.txtParameter2_Add);
            this.tabAdd.Controls.Add(this.lblParameter2_Add);
            this.tabAdd.Controls.Add(this.txtParameter1_Add);
            this.tabAdd.Controls.Add(this.lblParameter1_Add);
            this.tabAdd.Controls.Add(this.chkNullAllowed_Add);
            this.tabAdd.Controls.Add(this.cboDataType_Add);
            this.tabAdd.Controls.Add(this.lblDataType_Add);
            this.tabAdd.Controls.Add(this.txtColumnName_Add);
            this.tabAdd.Controls.Add(this.lblColumnName_Add);
            this.tabAdd.Controls.Add(this.lblAddInfo);
            this.tabAdd.Location = new System.Drawing.Point(1, 1);
            this.tabAdd.Name = "tabAdd";
            this.tabAdd.Size = new System.Drawing.Size(699, 298);
            this.tabAdd.TabIndex = 3;
            this.tabAdd.Text = "Add";
            // 
            // lblAddValidationMessage
            // 
            this.lblAddValidationMessage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAddValidationMessage.AutoEllipsis = true;
            this.lblAddValidationMessage.ForeColor = System.Drawing.Color.DarkRed;
            this.lblAddValidationMessage.Location = new System.Drawing.Point(17, 211);
            this.lblAddValidationMessage.Name = "lblAddValidationMessage";
            this.lblAddValidationMessage.Size = new System.Drawing.Size(670, 18);
            this.lblAddValidationMessage.TabIndex = 135;
            // 
            // txtResolvedType_Add
            // 
            this.txtResolvedType_Add.BackColor = System.Drawing.Color.WhiteSmoke;
            this.txtResolvedType_Add.Location = new System.Drawing.Point(100, 182);
            this.txtResolvedType_Add.Name = "txtResolvedType_Add";
            this.txtResolvedType_Add.ReadOnly = true;
            this.txtResolvedType_Add.Size = new System.Drawing.Size(535, 23);
            this.txtResolvedType_Add.TabIndex = 133;
            this.txtResolvedType_Add.TabStop = false;
            // 
            // lblResolvedType_Add
            // 
            this.lblResolvedType_Add.AutoSize = true;
            this.lblResolvedType_Add.Location = new System.Drawing.Point(17, 186);
            this.lblResolvedType_Add.Name = "lblResolvedType_Add";
            this.lblResolvedType_Add.Size = new System.Drawing.Size(93, 16);
            this.lblResolvedType_Add.TabIndex = 134;
            this.lblResolvedType_Add.Text = "Resolved Type:";
            // 
            // txtDefaultValue_Add
            // 
            this.txtDefaultValue_Add.Location = new System.Drawing.Point(235, 152);
            this.txtDefaultValue_Add.Name = "txtDefaultValue_Add";
            this.txtDefaultValue_Add.Size = new System.Drawing.Size(400, 23);
            this.txtDefaultValue_Add.TabIndex = 132;
            this.txtDefaultValue_Add.TextChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // cboDefaultKind_Add
            // 
            this.cboDefaultKind_Add.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDefaultKind_Add.FormattingEnabled = true;
            this.cboDefaultKind_Add.Location = new System.Drawing.Point(100, 151);
            this.cboDefaultKind_Add.Name = "cboDefaultKind_Add";
            this.cboDefaultKind_Add.Size = new System.Drawing.Size(125, 24);
            this.cboDefaultKind_Add.TabIndex = 131;
            this.cboDefaultKind_Add.SelectedIndexChanged += new System.EventHandler(this.cboDefaultKind_Add_SelectedIndexChanged);
            // 
            // lblDefaultKind_Add
            // 
            this.lblDefaultKind_Add.AutoSize = true;
            this.lblDefaultKind_Add.Location = new System.Drawing.Point(17, 155);
            this.lblDefaultKind_Add.Name = "lblDefaultKind_Add";
            this.lblDefaultKind_Add.Size = new System.Drawing.Size(51, 16);
            this.lblDefaultKind_Add.TabIndex = 133;
            this.lblDefaultKind_Add.Text = "Default:";
            // 
            // cboOracleLengthSemantics_Add
            // 
            this.cboOracleLengthSemantics_Add.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOracleLengthSemantics_Add.FormattingEnabled = true;
            this.cboOracleLengthSemantics_Add.Location = new System.Drawing.Point(485, 121);
            this.cboOracleLengthSemantics_Add.Name = "cboOracleLengthSemantics_Add";
            this.cboOracleLengthSemantics_Add.Size = new System.Drawing.Size(150, 24);
            this.cboOracleLengthSemantics_Add.TabIndex = 130;
            this.cboOracleLengthSemantics_Add.SelectedIndexChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // lblOracleLengthSemantics_Add
            // 
            this.lblOracleLengthSemantics_Add.AutoSize = true;
            this.lblOracleLengthSemantics_Add.Location = new System.Drawing.Point(365, 125);
            this.lblOracleLengthSemantics_Add.Name = "lblOracleLengthSemantics_Add";
            this.lblOracleLengthSemantics_Add.Size = new System.Drawing.Size(109, 16);
            this.lblOracleLengthSemantics_Add.TabIndex = 132;
            this.lblOracleLengthSemantics_Add.Text = "Length Semantics:";
            // 
            // txtParameter2_Add
            // 
            this.txtParameter2_Add.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParameter2_Add.Location = new System.Drawing.Point(270, 121);
            this.txtParameter2_Add.MaxLength = 500;
            this.txtParameter2_Add.Name = "txtParameter2_Add";
            this.txtParameter2_Add.Size = new System.Drawing.Size(70, 23);
            this.txtParameter2_Add.TabIndex = 129;
            this.txtParameter2_Add.TextChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // lblParameter2_Add
            // 
            this.lblParameter2_Add.AutoSize = true;
            this.lblParameter2_Add.Location = new System.Drawing.Point(206, 125);
            this.lblParameter2_Add.Name = "lblParameter2_Add";
            this.lblParameter2_Add.Size = new System.Drawing.Size(40, 16);
            this.lblParameter2_Add.TabIndex = 131;
            this.lblParameter2_Add.Text = "Scale:";
            // 
            // txtParameter1_Add
            // 
            this.txtParameter1_Add.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtParameter1_Add.Location = new System.Drawing.Point(100, 121);
            this.txtParameter1_Add.MaxLength = 500;
            this.txtParameter1_Add.Name = "txtParameter1_Add";
            this.txtParameter1_Add.Size = new System.Drawing.Size(80, 23);
            this.txtParameter1_Add.TabIndex = 130;
            this.txtParameter1_Add.TextChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // lblParameter1_Add
            // 
            this.lblParameter1_Add.AutoSize = true;
            this.lblParameter1_Add.Location = new System.Drawing.Point(17, 125);
            this.lblParameter1_Add.Name = "lblParameter1_Add";
            this.lblParameter1_Add.Size = new System.Drawing.Size(33, 16);
            this.lblParameter1_Add.TabIndex = 129;
            this.lblParameter1_Add.Text = "Size:";
            // 
            // chkNullAllowed_Add
            // 
            this.chkNullAllowed_Add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkNullAllowed_Add.AutoSize = true;
            this.chkNullAllowed_Add.Checked = true;
            this.chkNullAllowed_Add.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkNullAllowed_Add.Location = new System.Drawing.Point(536, 67);
            this.chkNullAllowed_Add.Name = "chkNullAllowed_Add";
            this.chkNullAllowed_Add.Size = new System.Drawing.Size(102, 20);
            this.chkNullAllowed_Add.TabIndex = 128;
            this.chkNullAllowed_Add.Text = "Nulls allowed";
            this.chkNullAllowed_Add.UseVisualStyleBackColor = true;
            this.chkNullAllowed_Add.CheckedChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // cboDataType_Add
            // 
            this.cboDataType_Add.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.cboDataType_Add.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboDataType_Add.FormattingEnabled = true;
            this.cboDataType_Add.Location = new System.Drawing.Point(88, 91);
            this.cboDataType_Add.Name = "cboDataType_Add";
            this.cboDataType_Add.Size = new System.Drawing.Size(250, 24);
            this.cboDataType_Add.TabIndex = 127;
            this.cboDataType_Add.SelectedIndexChanged += new System.EventHandler(this.cboDataType_Add_SelectedIndexChanged);
            this.cboDataType_Add.TextUpdate += new System.EventHandler(this.cboDataType_Add_TextUpdate);
            // 
            // lblDataType_Add
            // 
            this.lblDataType_Add.AutoSize = true;
            this.lblDataType_Add.BackColor = System.Drawing.Color.Transparent;
            this.lblDataType_Add.Location = new System.Drawing.Point(17, 94);
            this.lblDataType_Add.Name = "lblDataType_Add";
            this.lblDataType_Add.Size = new System.Drawing.Size(68, 16);
            this.lblDataType_Add.TabIndex = 123;
            this.lblDataType_Add.Text = "Data Type:";
            // 
            // txtColumnName_Add
            // 
            this.txtColumnName_Add.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtColumnName_Add.BackColor = System.Drawing.Color.White;
            this.txtColumnName_Add.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnName_Add.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtColumnName_Add.Location = new System.Drawing.Point(100, 65);
            this.txtColumnName_Add.MaxLength = 128;
            this.txtColumnName_Add.Name = "txtColumnName_Add";
            this.txtColumnName_Add.ShowContextMenu = false;
            this.txtColumnName_Add.Size = new System.Drawing.Size(412, 21);
            this.txtColumnName_Add.TabIndex = 125;
            this.txtColumnName_Add.Tag = null;
            this.txtColumnName_Add.TextDetached = true;
            this.txtColumnName_Add.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtColumnName_Add.TextChanged += new System.EventHandler(this.AddColumnEditorValueChanged);
            // 
            // lblColumnName_Add
            // 
            this.lblColumnName_Add.AutoSize = true;
            this.lblColumnName_Add.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnName_Add.Location = new System.Drawing.Point(17, 67);
            this.lblColumnName_Add.Name = "lblColumnName_Add";
            this.lblColumnName_Add.Size = new System.Drawing.Size(92, 16);
            this.lblColumnName_Add.TabIndex = 123;
            this.lblColumnName_Add.Text = "Column Name:";
            // 
            // lblAddInfo
            // 
            this.lblAddInfo.AutoSize = true;
            this.lblAddInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblAddInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblAddInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblAddInfo.Location = new System.Drawing.Point(17, 13);
            this.lblAddInfo.Name = "lblAddInfo";
            this.lblAddInfo.Size = new System.Drawing.Size(89, 16);
            this.lblAddInfo.TabIndex = 122;
            this.lblAddInfo.Text = "Add a column";
            // 
            // tabComment
            // 
            this.tabComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabComment.BackColor = System.Drawing.Color.White;
            this.tabComment.Controls.Add(this.txtComment);
            this.tabComment.Controls.Add(this.txtColumnName_Comment);
            this.tabComment.Controls.Add(this.lblComment);
            this.tabComment.Controls.Add(this.lblColumnName_Comment);
            this.tabComment.Controls.Add(this.lblCommentInfo);
            this.tabComment.Location = new System.Drawing.Point(1, 1);
            this.tabComment.Name = "tabComment";
            this.tabComment.Size = new System.Drawing.Size(699, 298);
            this.tabComment.TabIndex = 0;
            this.tabComment.Text = "Comment";
            // 
            // txtComment
            // 
            this.txtComment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtComment.BackColor = System.Drawing.Color.White;
            this.txtComment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtComment.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtComment.Location = new System.Drawing.Point(100, 95);
            this.txtComment.MaxLength = 25;
            this.txtComment.Multiline = true;
            this.txtComment.Name = "txtComment";
            this.txtComment.ShortcutsEnabled = false;
            this.txtComment.ShowContextMenu = false;
            this.txtComment.Size = new System.Drawing.Size(587, 50);
            this.txtComment.TabIndex = 121;
            this.txtComment.Tag = null;
            this.txtComment.TextDetached = true;
            this.txtComment.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtComment.TextChanged += new System.EventHandler(this.txtComment_TextChanged);
            // 
            // txtColumnName_Comment
            // 
            this.txtColumnName_Comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtColumnName_Comment.BackColor = System.Drawing.SystemColors.Control;
            this.txtColumnName_Comment.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnName_Comment.Enabled = false;
            this.txtColumnName_Comment.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtColumnName_Comment.Location = new System.Drawing.Point(100, 67);
            this.txtColumnName_Comment.MaxLength = 25;
            this.txtColumnName_Comment.Name = "txtColumnName_Comment";
            this.txtColumnName_Comment.ReadOnly = true;
            this.txtColumnName_Comment.ShortcutsEnabled = false;
            this.txtColumnName_Comment.ShowContextMenu = false;
            this.txtColumnName_Comment.Size = new System.Drawing.Size(412, 21);
            this.txtColumnName_Comment.TabIndex = 120;
            this.txtColumnName_Comment.Tag = null;
            this.txtColumnName_Comment.TextDetached = true;
            this.txtColumnName_Comment.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.BackColor = System.Drawing.Color.Transparent;
            this.lblComment.Location = new System.Drawing.Point(17, 96);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(66, 16);
            this.lblComment.TabIndex = 4;
            this.lblComment.Text = "Comment:";
            // 
            // lblColumnName_Comment
            // 
            this.lblColumnName_Comment.AutoSize = true;
            this.lblColumnName_Comment.BackColor = System.Drawing.Color.Transparent;
            this.lblColumnName_Comment.Location = new System.Drawing.Point(17, 68);
            this.lblColumnName_Comment.Name = "lblColumnName_Comment";
            this.lblColumnName_Comment.Size = new System.Drawing.Size(92, 16);
            this.lblColumnName_Comment.TabIndex = 3;
            this.lblColumnName_Comment.Text = "Column Name:";
            // 
            // lblCommentInfo
            // 
            this.lblCommentInfo.AutoSize = true;
            this.lblCommentInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblCommentInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCommentInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblCommentInfo.Location = new System.Drawing.Point(17, 13);
            this.lblCommentInfo.Name = "lblCommentInfo";
            this.lblCommentInfo.Size = new System.Drawing.Size(174, 16);
            this.lblCommentInfo.TabIndex = 2;
            this.lblCommentInfo.Text = "Add a comment to a column";
            // 
            // tabDrop
            // 
            this.tabDrop.BackColor = System.Drawing.Color.White;
            this.tabDrop.Controls.Add(this.txtColumnName_Drop);
            this.tabDrop.Controls.Add(this.lblColumnName_Drop);
            this.tabDrop.Controls.Add(this.lblDropInfo);
            this.tabDrop.Location = new System.Drawing.Point(1, 1);
            this.tabDrop.Name = "tabDrop";
            this.tabDrop.Size = new System.Drawing.Size(699, 298);
            this.tabDrop.TabIndex = 1;
            this.tabDrop.Text = "Drop";
            // 
            // txtColumnName_Drop
            // 
            this.txtColumnName_Drop.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtColumnName_Drop.BackColor = System.Drawing.SystemColors.Control;
            this.txtColumnName_Drop.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnName_Drop.Enabled = false;
            this.txtColumnName_Drop.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtColumnName_Drop.Location = new System.Drawing.Point(100, 67);
            this.txtColumnName_Drop.MaxLength = 25;
            this.txtColumnName_Drop.Name = "txtColumnName_Drop";
            this.txtColumnName_Drop.ReadOnly = true;
            this.txtColumnName_Drop.ShortcutsEnabled = false;
            this.txtColumnName_Drop.ShowContextMenu = false;
            this.txtColumnName_Drop.Size = new System.Drawing.Size(412, 21);
            this.txtColumnName_Drop.TabIndex = 125;
            this.txtColumnName_Drop.Tag = null;
            this.txtColumnName_Drop.TextDetached = true;
            this.txtColumnName_Drop.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblColumnName_Drop
            // 
            this.lblColumnName_Drop.AutoSize = true;
            this.lblColumnName_Drop.Location = new System.Drawing.Point(17, 68);
            this.lblColumnName_Drop.Name = "lblColumnName_Drop";
            this.lblColumnName_Drop.Size = new System.Drawing.Size(92, 16);
            this.lblColumnName_Drop.TabIndex = 123;
            this.lblColumnName_Drop.Text = "Column Name:";
            // 
            // lblDropInfo
            // 
            this.lblDropInfo.AutoSize = true;
            this.lblDropInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblDropInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDropInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblDropInfo.Location = new System.Drawing.Point(17, 13);
            this.lblDropInfo.Name = "lblDropInfo";
            this.lblDropInfo.Size = new System.Drawing.Size(94, 16);
            this.lblDropInfo.TabIndex = 122;
            this.lblDropInfo.Text = "Drop a column";
            // 
            // tabRename
            // 
            this.tabRename.BackColor = System.Drawing.Color.White;
            this.tabRename.Controls.Add(this.txtNewColumnName);
            this.tabRename.Controls.Add(this.txtColumnName_Rename);
            this.tabRename.Controls.Add(this.lblNewColumnName);
            this.tabRename.Controls.Add(this.lblColumnName_Rename);
            this.tabRename.Controls.Add(this.lblRenameInfo);
            this.tabRename.Location = new System.Drawing.Point(1, 1);
            this.tabRename.Name = "tabRename";
            this.tabRename.Size = new System.Drawing.Size(699, 298);
            this.tabRename.TabIndex = 2;
            this.tabRename.Text = "Rename";
            // 
            // txtNewColumnName
            // 
            this.txtNewColumnName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNewColumnName.BackColor = System.Drawing.Color.White;
            this.txtNewColumnName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNewColumnName.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtNewColumnName.Location = new System.Drawing.Point(100, 95);
            this.txtNewColumnName.MaxLength = 25;
            this.txtNewColumnName.Name = "txtNewColumnName";
            this.txtNewColumnName.ShortcutsEnabled = false;
            this.txtNewColumnName.ShowContextMenu = false;
            this.txtNewColumnName.Size = new System.Drawing.Size(412, 21);
            this.txtNewColumnName.TabIndex = 126;
            this.txtNewColumnName.Tag = null;
            this.txtNewColumnName.TextDetached = true;
            this.txtNewColumnName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtNewColumnName.TextChanged += new System.EventHandler(this.txtNewColumnName_TextChanged);
            // 
            // txtColumnName_Rename
            // 
            this.txtColumnName_Rename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtColumnName_Rename.BackColor = System.Drawing.SystemColors.Control;
            this.txtColumnName_Rename.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnName_Rename.Enabled = false;
            this.txtColumnName_Rename.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtColumnName_Rename.Location = new System.Drawing.Point(100, 67);
            this.txtColumnName_Rename.MaxLength = 25;
            this.txtColumnName_Rename.Name = "txtColumnName_Rename";
            this.txtColumnName_Rename.ReadOnly = true;
            this.txtColumnName_Rename.ShortcutsEnabled = false;
            this.txtColumnName_Rename.ShowContextMenu = false;
            this.txtColumnName_Rename.Size = new System.Drawing.Size(412, 21);
            this.txtColumnName_Rename.TabIndex = 125;
            this.txtColumnName_Rename.Tag = null;
            this.txtColumnName_Rename.TextDetached = true;
            this.txtColumnName_Rename.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblNewColumnName
            // 
            this.lblNewColumnName.AutoSize = true;
            this.lblNewColumnName.Location = new System.Drawing.Point(17, 96);
            this.lblNewColumnName.Name = "lblNewColumnName";
            this.lblNewColumnName.Size = new System.Drawing.Size(116, 16);
            this.lblNewColumnName.TabIndex = 124;
            this.lblNewColumnName.Text = "Net Column Name:";
            // 
            // lblColumnName_Rename
            // 
            this.lblColumnName_Rename.AutoSize = true;
            this.lblColumnName_Rename.Location = new System.Drawing.Point(17, 68);
            this.lblColumnName_Rename.Name = "lblColumnName_Rename";
            this.lblColumnName_Rename.Size = new System.Drawing.Size(92, 16);
            this.lblColumnName_Rename.TabIndex = 123;
            this.lblColumnName_Rename.Text = "Column Name:";
            // 
            // lblRenameInfo
            // 
            this.lblRenameInfo.AutoSize = true;
            this.lblRenameInfo.BackColor = System.Drawing.Color.Transparent;
            this.lblRenameInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRenameInfo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.lblRenameInfo.Location = new System.Drawing.Point(17, 13);
            this.lblRenameInfo.Name = "lblRenameInfo";
            this.lblRenameInfo.Size = new System.Drawing.Size(112, 16);
            this.lblRenameInfo.TabIndex = 122;
            this.lblRenameInfo.Text = "Rename a column";
            // 
            // grpSqlPreview
            // 
            this.grpSqlPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpSqlPreview.Controls.Add(this.editorSqlPreview);
            this.grpSqlPreview.Controls.Add(this.tsTool);
            this.grpSqlPreview.Location = new System.Drawing.Point(9, 346);
            this.grpSqlPreview.Name = "grpSqlPreview";
            this.grpSqlPreview.Size = new System.Drawing.Size(701, 157);
            this.grpSqlPreview.TabIndex = 1;
            this.grpSqlPreview.TabStop = false;
            this.grpSqlPreview.Text = "SQL Preview";
            // 
            // editorSqlPreview
            // 
            this.editorSqlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlPreview.CaretLineVisible = true;
            this.editorSqlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorSqlPreview.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlPreview.Location = new System.Drawing.Point(3, 44);
            this.editorSqlPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlPreview.Name = "editorSqlPreview";
            this.editorSqlPreview.ReadOnly = true;
            this.editorSqlPreview.Size = new System.Drawing.Size(695, 110);
            this.editorSqlPreview.Styler = null;
            this.editorSqlPreview.TabIndex = 2;
            this.editorSqlPreview.WhitespaceSize = 3;
            this.editorSqlPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlPreview.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorSqlPreview.TextChanged += new System.EventHandler(this.editorSqlPreview_TextChanged);
            this.editorSqlPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorSqlPreview_MouseDown);
            // 
            // tsTool
            // 
            this.tsTool.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAll,
            this.btnCopy,
            this.btnWordWrap,
            this.btnWordWrap2,
            this.btnShowAllCharacters,
            this.btnShowAllCharacters2,
            this.btnZoomIn,
            this.btnZoomOut});
            this.tsTool.Location = new System.Drawing.Point(3, 19);
            this.tsTool.Name = "tsTool";
            this.tsTool.Size = new System.Drawing.Size(695, 25);
            this.tsTool.TabIndex = 1;
            this.tsTool.Text = "toolStrip1";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy.Image")));
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(23, 22);
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnWordWrap
            // 
            this.btnWordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap.Image")));
            this.btnWordWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap.Name = "btnWordWrap";
            this.btnWordWrap.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnWordWrap2
            // 
            this.btnWordWrap2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2.Image")));
            this.btnWordWrap2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2.Name = "btnWordWrap2";
            this.btnWordWrap2.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap2.Visible = false;
            this.btnWordWrap2.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnShowAllCharacters
            // 
            this.btnShowAllCharacters.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters.Image")));
            this.btnShowAllCharacters.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters.Name = "btnShowAllCharacters";
            this.btnShowAllCharacters.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // btnShowAllCharacters2
            // 
            this.btnShowAllCharacters2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2.Image")));
            this.btnShowAllCharacters2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2.Name = "btnShowAllCharacters2";
            this.btnShowAllCharacters2.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters2.ToolTipText = "ters";
            this.btnShowAllCharacters2.Visible = false;
            this.btnShowAllCharacters2.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // btnZoomIn
            // 
            this.btnZoomIn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomIn.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomIn.Image")));
            this.btnZoomIn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomIn.Name = "btnZoomIn";
            this.btnZoomIn.Size = new System.Drawing.Size(23, 22);
            this.btnZoomIn.Click += new System.EventHandler(this.btnZoomIn_Click);
            // 
            // btnZoomOut
            // 
            this.btnZoomOut.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOut.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomOut.Image")));
            this.btnZoomOut.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOut.Name = "btnZoomOut";
            this.btnZoomOut.Size = new System.Drawing.Size(23, 22);
            this.btnZoomOut.Click += new System.EventHandler(this.btnZoomOut_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(644, 319);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(66, 29);
            this.btnClose.TabIndex = 60;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnExecute
            // 
            this.btnExecute.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExecute.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(538, 319);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(90, 29);
            this.btnExecute.TabIndex = 61;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // lblPrompt1
            // 
            this.lblPrompt1.AutoSize = true;
            this.lblPrompt1.BackColor = System.Drawing.Color.Transparent;
            this.lblPrompt1.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPrompt1.ForeColor = System.Drawing.Color.Maroon;
            this.lblPrompt1.Location = new System.Drawing.Point(5, 4);
            this.lblPrompt1.Name = "lblPrompt1";
            this.lblPrompt1.Size = new System.Drawing.Size(374, 32);
            this.lblPrompt1.TabIndex = 3;
            this.lblPrompt1.Text = "\r\n1. All executed SQL statements will be recorded in SQL History.";
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BackColor = System.Drawing.Color.Transparent;
            this.panel1.Controls.Add(this.lblPrompt2);
            this.panel1.Controls.Add(this.lblPrompt1);
            this.panel1.Location = new System.Drawing.Point(24, 250);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(682, 60);
            this.panel1.TabIndex = 62;
            // 
            // lblPrompt2
            // 
            this.lblPrompt2.AutoSize = true;
            this.lblPrompt2.BackColor = System.Drawing.Color.Transparent;
            this.lblPrompt2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPrompt2.ForeColor = System.Drawing.Color.Maroon;
            this.lblPrompt2.Location = new System.Drawing.Point(5, 38);
            this.lblPrompt2.Name = "lblPrompt2";
            this.lblPrompt2.Size = new System.Drawing.Size(651, 16);
            this.lblPrompt2.TabIndex = 4;
            this.lblPrompt2.Text = "2. After executing the SQL statement(s), there will be an implicit commit. (DDL c" +
    "hanges cannot be rolled back.)";
            // 
            // txtTableName
            // 
            this.txtTableName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTableName.BackColor = System.Drawing.SystemColors.Control;
            this.txtTableName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtTableName.Enabled = false;
            this.txtTableName.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtTableName.Location = new System.Drawing.Point(111, 53);
            this.txtTableName.MaxLength = 25;
            this.txtTableName.Name = "txtTableName";
            this.txtTableName.ReadOnly = true;
            this.txtTableName.ShortcutsEnabled = false;
            this.txtTableName.ShowContextMenu = false;
            this.txtTableName.Size = new System.Drawing.Size(412, 21);
            this.txtTableName.TabIndex = 122;
            this.txtTableName.Tag = null;
            this.txtTableName.TextDetached = true;
            this.txtTableName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblTableName
            // 
            this.lblTableName.AutoSize = true;
            this.lblTableName.BackColor = System.Drawing.Color.Transparent;
            this.lblTableName.Location = new System.Drawing.Point(26, 54);
            this.lblTableName.Name = "lblTableName";
            this.lblTableName.Size = new System.Drawing.Size(80, 16);
            this.lblTableName.TabIndex = 121;
            this.lblTableName.Text = "Table Name:";
            // 
            // EditColumnForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(718, 511);
            this.Controls.Add(this.txtTableName);
            this.Controls.Add(this.lblTableName);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.grpSqlPreview);
            this.Controls.Add(this.tabColumn);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(1280, 800);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(734, 550);
            this.Name = "EditColumnForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Column";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.tabColumn)).EndInit();
            this.tabColumn.ResumeLayout(false);
            this.tabAdd.ResumeLayout(false);
            this.tabAdd.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Add)).EndInit();
            this.tabComment.ResumeLayout(false);
            this.tabComment.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtComment)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Comment)).EndInit();
            this.tabDrop.ResumeLayout(false);
            this.tabDrop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Drop)).EndInit();
            this.tabRename.ResumeLayout(false);
            this.tabRename.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewColumnName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnName_Rename)).EndInit();
            this.grpSqlPreview.ResumeLayout(false);
            this.grpSqlPreview.PerformLayout();
            this.tsTool.ResumeLayout(false);
            this.tsTool.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExecute)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.txtTableName)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Command.C1DockingTab tabColumn;
        private C1.Win.C1Command.C1DockingTabPage tabComment;
        private System.Windows.Forms.GroupBox grpSqlPreview;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlPreview;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnExecute;
        private System.Windows.Forms.Label lblCommentInfo;
        private C1.Win.C1Command.C1DockingTabPage tabDrop;
        private C1.Win.C1Command.C1DockingTabPage tabRename;
        private C1.Win.C1Command.C1DockingTabPage tabAdd;
        private System.Windows.Forms.Label lblComment;
        private System.Windows.Forms.Label lblColumnName_Comment;
        private C1.Win.C1Input.C1TextBox txtComment;
        private C1.Win.C1Input.C1TextBox txtColumnName_Comment;
        private System.Windows.Forms.Label lblPrompt1;
        private C1.Win.C1Input.C1TextBox txtColumnName_Drop;
        private System.Windows.Forms.Label lblColumnName_Drop;
        private System.Windows.Forms.Label lblDropInfo;
        private C1.Win.C1Input.C1TextBox txtNewColumnName;
        private C1.Win.C1Input.C1TextBox txtColumnName_Rename;
        private System.Windows.Forms.Label lblNewColumnName;
        private System.Windows.Forms.Label lblColumnName_Rename;
        private System.Windows.Forms.Label lblRenameInfo;
        private C1.Win.C1Input.C1TextBox txtColumnName_Add;
        private System.Windows.Forms.Label lblColumnName_Add;
        private System.Windows.Forms.Label lblAddInfo;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblPrompt2;
        private System.Windows.Forms.ToolStrip tsTool;
        private System.Windows.Forms.ToolStripButton btnSelectAll;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2;
        private System.Windows.Forms.ToolStripButton btnZoomIn;
        private System.Windows.Forms.ToolStripButton btnZoomOut;
        private C1.Win.C1Input.C1TextBox txtTableName;
        private System.Windows.Forms.Label lblTableName;
        private System.Windows.Forms.Label lblDataType_Add;
        private System.Windows.Forms.ComboBox cboDataType_Add;
        private System.Windows.Forms.CheckBox chkNullAllowed_Add;
        private System.Windows.Forms.Label lblParameter1_Add;
        private System.Windows.Forms.TextBox txtParameter1_Add;
        private System.Windows.Forms.Label lblParameter2_Add;
        private System.Windows.Forms.TextBox txtParameter2_Add;
        private System.Windows.Forms.Label lblOracleLengthSemantics_Add;
        private System.Windows.Forms.ComboBox cboOracleLengthSemantics_Add;
        private System.Windows.Forms.Label lblDefaultKind_Add;
        private System.Windows.Forms.ComboBox cboDefaultKind_Add;
        private System.Windows.Forms.TextBox txtDefaultValue_Add;
        private System.Windows.Forms.Label lblResolvedType_Add;
        private System.Windows.Forms.TextBox txtResolvedType_Add;
        private System.Windows.Forms.Label lblAddValidationMessage;
    }
}