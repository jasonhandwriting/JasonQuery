using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    partial class ExportToFileForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExportToFileForm));
            this.lblFileName = new System.Windows.Forms.Label();
            this.txtWorksheetName = new C1.Win.C1Input.C1TextBox();
            this.lblWorksheetName = new System.Windows.Forms.Label();
            this.chkAutoOpenExportedFile = new C1.Win.C1Input.C1CheckBox();
            this.grpVisualStyle = new System.Windows.Forms.GroupBox();
            this.grpDataGrid = new System.Windows.Forms.GroupBox();
            this.cboGridRowHeight = new C1.Win.C1Input.C1ComboBox();
            this.cboGridFontSize = new C1.Win.C1Input.C1ComboBox();
            this.cboGridFontName = new C1.Win.C1Input.C1FontPicker();
            this.lblFontSize = new System.Windows.Forms.Label();
            this.lblFontName = new System.Windows.Forms.Label();
            this.lblRowHeight = new System.Windows.Forms.Label();
            this.lblExcelDetect = new System.Windows.Forms.Label();
            this.cboOddRowBackColor = new System.Windows.Forms.ComboBox();
            this.cboEvenRowBackColor = new System.Windows.Forms.ComboBox();
            this.cboHeadingBackColor = new System.Windows.Forms.ComboBox();
            this.lblHeadingBackColor = new System.Windows.Forms.Label();
            this.lblOddRowBackColor = new System.Windows.Forms.Label();
            this.lblEvenRowBackColor = new System.Windows.Forms.Label();
            this.rdoCustom = new System.Windows.Forms.RadioButton();
            this.rdoDefault = new System.Windows.Forms.RadioButton();
            this.grpPreview = new System.Windows.Forms.GroupBox();
            this.editorPreview = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.c1GridVisualStyle = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.lblSaveAsType = new System.Windows.Forms.Label();
            this.cboSaveAsType = new C1.Win.C1Input.C1ComboBox();
            this.btnBrowseFile = new C1.Win.C1Input.C1Button();
            this.btnExport = new C1.Win.C1Input.C1Button();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnSaveAllTheSettings = new C1.Win.C1Input.C1Button();
            this.lblInfo = new System.Windows.Forms.Label();
            this.c1XLBook1 = new C1.C1Excel.C1XLBook();
            this.btnExportAndClose = new C1.Win.C1Input.C1Button();
            this.cboFileName = new C1.Win.C1Input.C1ComboBox();
            this.tmrExcelDetect = new System.Windows.Forms.Timer(this.components);
            this.lblCSVDelimiters = new System.Windows.Forms.Label();
            this.cboCSVDelimiters = new C1.Win.C1Input.C1ComboBox();
            this.chkColumnResize = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.lblEncoding = new System.Windows.Forms.Label();
            this.cboEncoding = new C1.Win.C1Input.C1ComboBox();
            this.btnCancel = new C1.Win.C1Input.C1Button();
            this.chkConvertCRLF = new C1.Win.C1Input.C1CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.txtWorksheetName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoOpenExportedFile)).BeginInit();
            this.grpVisualStyle.SuspendLayout();
            this.grpDataGrid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridRowHeight)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontName)).BeginInit();
            this.grpPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVisualStyle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSaveAsType)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveAllTheSettings)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExportAndClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFileName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCSVDelimiters)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEncoding)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkConvertCRLF)).BeginInit();
            this.SuspendLayout();
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Location = new System.Drawing.Point(19, 18);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(64, 16);
            this.lblFileName.TabIndex = 0;
            this.lblFileName.Text = "File name:";
            // 
            // txtWorksheetName
            // 
            this.txtWorksheetName.BackColor = System.Drawing.Color.LightYellow;
            this.txtWorksheetName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtWorksheetName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.txtWorksheetName.Location = new System.Drawing.Point(425, 46);
            this.txtWorksheetName.Name = "txtWorksheetName";
            this.txtWorksheetName.Size = new System.Drawing.Size(174, 21);
            this.txtWorksheetName.TabIndex = 4;
            this.txtWorksheetName.Tag = null;
            this.txtWorksheetName.Text = "data";
            this.txtWorksheetName.TextDetached = true;
            this.txtWorksheetName.Value = "";
            this.txtWorksheetName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtWorksheetName.Leave += new System.EventHandler(this.txtWorksheetName_Leave);
            // 
            // lblWorksheetName
            // 
            this.lblWorksheetName.AutoSize = true;
            this.lblWorksheetName.Location = new System.Drawing.Point(317, 48);
            this.lblWorksheetName.Name = "lblWorksheetName";
            this.lblWorksheetName.Size = new System.Drawing.Size(108, 16);
            this.lblWorksheetName.TabIndex = 69;
            this.lblWorksheetName.Text = "Worksheet Name:";
            // 
            // chkAutoOpenExportedFile
            // 
            this.chkAutoOpenExportedFile.AutoSize = true;
            this.chkAutoOpenExportedFile.BackColor = System.Drawing.Color.Transparent;
            this.chkAutoOpenExportedFile.BorderColor = System.Drawing.Color.Transparent;
            this.chkAutoOpenExportedFile.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkAutoOpenExportedFile.ForeColor = System.Drawing.Color.Black;
            this.chkAutoOpenExportedFile.Location = new System.Drawing.Point(22, 76);
            this.chkAutoOpenExportedFile.Name = "chkAutoOpenExportedFile";
            this.chkAutoOpenExportedFile.Padding = new System.Windows.Forms.Padding(1);
            this.chkAutoOpenExportedFile.Size = new System.Drawing.Size(245, 22);
            this.chkAutoOpenExportedFile.TabIndex = 5;
            this.chkAutoOpenExportedFile.Text = "After exporting, open the exported file";
            this.chkAutoOpenExportedFile.UseVisualStyleBackColor = true;
            this.chkAutoOpenExportedFile.Value = null;
            this.chkAutoOpenExportedFile.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpVisualStyle
            // 
            this.grpVisualStyle.Controls.Add(this.grpDataGrid);
            this.grpVisualStyle.Controls.Add(this.rdoCustom);
            this.grpVisualStyle.Controls.Add(this.rdoDefault);
            this.grpVisualStyle.Location = new System.Drawing.Point(19, 106);
            this.grpVisualStyle.Name = "grpVisualStyle";
            this.grpVisualStyle.Size = new System.Drawing.Size(317, 264);
            this.grpVisualStyle.TabIndex = 78;
            this.grpVisualStyle.TabStop = false;
            this.grpVisualStyle.Text = "Visual Style";
            // 
            // grpDataGrid
            // 
            this.grpDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.grpDataGrid.BackColor = System.Drawing.Color.Transparent;
            this.grpDataGrid.Controls.Add(this.cboGridRowHeight);
            this.grpDataGrid.Controls.Add(this.cboGridFontSize);
            this.grpDataGrid.Controls.Add(this.cboGridFontName);
            this.grpDataGrid.Controls.Add(this.lblFontSize);
            this.grpDataGrid.Controls.Add(this.lblFontName);
            this.grpDataGrid.Controls.Add(this.lblRowHeight);
            this.grpDataGrid.Controls.Add(this.lblExcelDetect);
            this.grpDataGrid.Controls.Add(this.cboOddRowBackColor);
            this.grpDataGrid.Controls.Add(this.cboEvenRowBackColor);
            this.grpDataGrid.Controls.Add(this.cboHeadingBackColor);
            this.grpDataGrid.Controls.Add(this.lblHeadingBackColor);
            this.grpDataGrid.Controls.Add(this.lblOddRowBackColor);
            this.grpDataGrid.Controls.Add(this.lblEvenRowBackColor);
            this.grpDataGrid.Location = new System.Drawing.Point(10, 21);
            this.grpDataGrid.Name = "grpDataGrid";
            this.grpDataGrid.Size = new System.Drawing.Size(288, 230);
            this.grpDataGrid.TabIndex = 79;
            this.grpDataGrid.TabStop = false;
            this.grpDataGrid.Text = "Data Grid";
            // 
            // cboGridRowHeight
            // 
            this.cboGridRowHeight.AllowSpinLoop = false;
            this.cboGridRowHeight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboGridRowHeight.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboGridRowHeight.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboGridRowHeight.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboGridRowHeight.GapHeight = 0;
            this.cboGridRowHeight.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridRowHeight.Items.Add("14");
            this.cboGridRowHeight.Items.Add("16");
            this.cboGridRowHeight.Items.Add("18");
            this.cboGridRowHeight.Items.Add("20");
            this.cboGridRowHeight.Items.Add("22");
            this.cboGridRowHeight.Items.Add("24");
            this.cboGridRowHeight.Items.Add("26");
            this.cboGridRowHeight.Items.Add("28");
            this.cboGridRowHeight.Items.Add("30");
            this.cboGridRowHeight.ItemsDisplayMember = "";
            this.cboGridRowHeight.ItemsValueMember = "";
            this.cboGridRowHeight.Location = new System.Drawing.Point(89, 176);
            this.cboGridRowHeight.Name = "cboGridRowHeight";
            this.cboGridRowHeight.Size = new System.Drawing.Size(51, 21);
            this.cboGridRowHeight.TabIndex = 91;
            this.cboGridRowHeight.Tag = null;
            this.cboGridRowHeight.TextDetached = true;
            this.cboGridRowHeight.Visible = false;
            this.cboGridRowHeight.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridRowHeight.SelectedIndexChanged += new System.EventHandler(this.cboGridRowHeight_SelectedIndexChanged);
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
            this.cboGridFontSize.Location = new System.Drawing.Point(80, 146);
            this.cboGridFontSize.Name = "cboGridFontSize";
            this.cboGridFontSize.Size = new System.Drawing.Size(51, 21);
            this.cboGridFontSize.TabIndex = 90;
            this.cboGridFontSize.Tag = null;
            this.cboGridFontSize.TextDetached = true;
            this.cboGridFontSize.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridFontSize.SelectedIndexChanged += new System.EventHandler(this.cboGridFontSize_SelectedIndexChanged);
            // 
            // cboGridFontName
            // 
            this.cboGridFontName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboGridFontName.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboGridFontName.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboGridFontName.Location = new System.Drawing.Point(95, 116);
            this.cboGridFontName.Name = "cboGridFontName";
            this.cboGridFontName.Size = new System.Drawing.Size(155, 21);
            this.cboGridFontName.TabIndex = 88;
            this.cboGridFontName.Tag = null;
            this.cboGridFontName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboGridFontName.TextChanged += new System.EventHandler(this.cboGridFontName_TextChanged);
            // 
            // lblFontSize
            // 
            this.lblFontSize.AutoSize = true;
            this.lblFontSize.Location = new System.Drawing.Point(14, 148);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(61, 16);
            this.lblFontSize.TabIndex = 87;
            this.lblFontSize.Text = "Font Size:";
            this.lblFontSize.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFontName
            // 
            this.lblFontName.AutoSize = true;
            this.lblFontName.Location = new System.Drawing.Point(14, 118);
            this.lblFontName.Name = "lblFontName";
            this.lblFontName.Size = new System.Drawing.Size(73, 16);
            this.lblFontName.TabIndex = 86;
            this.lblFontName.Text = "Font Name:";
            this.lblFontName.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblRowHeight
            // 
            this.lblRowHeight.AutoSize = true;
            this.lblRowHeight.Location = new System.Drawing.Point(14, 178);
            this.lblRowHeight.Name = "lblRowHeight";
            this.lblRowHeight.Size = new System.Drawing.Size(76, 16);
            this.lblRowHeight.TabIndex = 89;
            this.lblRowHeight.Text = "Row Height:";
            this.lblRowHeight.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblRowHeight.Visible = false;
            // 
            // lblExcelDetect
            // 
            this.lblExcelDetect.AutoSize = true;
            this.lblExcelDetect.Location = new System.Drawing.Point(14, 138);
            this.lblExcelDetect.Name = "lblExcelDetect";
            this.lblExcelDetect.Size = new System.Drawing.Size(0, 16);
            this.lblExcelDetect.TabIndex = 58;
            this.lblExcelDetect.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // cboOddRowBackColor
            // 
            this.cboOddRowBackColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboOddRowBackColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboOddRowBackColor.Font = new System.Drawing.Font("新細明體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboOddRowBackColor.FormattingEnabled = true;
            this.cboOddRowBackColor.Location = new System.Drawing.Point(144, 82);
            this.cboOddRowBackColor.Name = "cboOddRowBackColor";
            this.cboOddRowBackColor.Size = new System.Drawing.Size(90, 24);
            this.cboOddRowBackColor.TabIndex = 57;
            this.cboOddRowBackColor.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.BackColor_DrawItem);
            this.cboOddRowBackColor.SelectedIndexChanged += new System.EventHandler(this.BackColor_SelectedIndexChanged);
            // 
            // cboEvenRowBackColor
            // 
            this.cboEvenRowBackColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboEvenRowBackColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEvenRowBackColor.Font = new System.Drawing.Font("新細明體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboEvenRowBackColor.FormattingEnabled = true;
            this.cboEvenRowBackColor.Location = new System.Drawing.Point(144, 52);
            this.cboEvenRowBackColor.Name = "cboEvenRowBackColor";
            this.cboEvenRowBackColor.Size = new System.Drawing.Size(90, 24);
            this.cboEvenRowBackColor.TabIndex = 56;
            this.cboEvenRowBackColor.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.BackColor_DrawItem);
            this.cboEvenRowBackColor.SelectedIndexChanged += new System.EventHandler(this.BackColor_SelectedIndexChanged);
            // 
            // cboHeadingBackColor
            // 
            this.cboHeadingBackColor.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cboHeadingBackColor.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboHeadingBackColor.DropDownWidth = 75;
            this.cboHeadingBackColor.Font = new System.Drawing.Font("新細明體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboHeadingBackColor.FormattingEnabled = true;
            this.cboHeadingBackColor.Location = new System.Drawing.Point(182, 22);
            this.cboHeadingBackColor.Name = "cboHeadingBackColor";
            this.cboHeadingBackColor.Size = new System.Drawing.Size(90, 24);
            this.cboHeadingBackColor.TabIndex = 55;
            this.cboHeadingBackColor.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.BackColor_DrawItem);
            this.cboHeadingBackColor.SelectedIndexChanged += new System.EventHandler(this.BackColor_SelectedIndexChanged);
            // 
            // lblHeadingBackColor
            // 
            this.lblHeadingBackColor.AutoSize = true;
            this.lblHeadingBackColor.Location = new System.Drawing.Point(14, 25);
            this.lblHeadingBackColor.Name = "lblHeadingBackColor";
            this.lblHeadingBackColor.Size = new System.Drawing.Size(122, 16);
            this.lblHeadingBackColor.TabIndex = 54;
            this.lblHeadingBackColor.Text = "Heading Back Color:";
            this.lblHeadingBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblOddRowBackColor
            // 
            this.lblOddRowBackColor.AutoSize = true;
            this.lblOddRowBackColor.Location = new System.Drawing.Point(14, 85);
            this.lblOddRowBackColor.Name = "lblOddRowBackColor";
            this.lblOddRowBackColor.Size = new System.Drawing.Size(127, 16);
            this.lblOddRowBackColor.TabIndex = 28;
            this.lblOddRowBackColor.Text = "Odd Row Back Color:";
            this.lblOddRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblEvenRowBackColor
            // 
            this.lblEvenRowBackColor.AutoSize = true;
            this.lblEvenRowBackColor.Location = new System.Drawing.Point(14, 55);
            this.lblEvenRowBackColor.Name = "lblEvenRowBackColor";
            this.lblEvenRowBackColor.Size = new System.Drawing.Size(128, 16);
            this.lblEvenRowBackColor.TabIndex = 26;
            this.lblEvenRowBackColor.Text = "Even Row Back Color:";
            this.lblEvenRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rdoCustom
            // 
            this.rdoCustom.AutoSize = true;
            this.rdoCustom.Location = new System.Drawing.Point(143, 22);
            this.rdoCustom.Name = "rdoCustom";
            this.rdoCustom.Size = new System.Drawing.Size(68, 20);
            this.rdoCustom.TabIndex = 8;
            this.rdoCustom.TabStop = true;
            this.rdoCustom.Text = "Custom";
            this.rdoCustom.UseVisualStyleBackColor = true;
            this.rdoCustom.Visible = false;
            this.rdoCustom.CheckedChanged += new System.EventHandler(this.VisualStyleChanged);
            // 
            // rdoDefault
            // 
            this.rdoDefault.AutoSize = true;
            this.rdoDefault.Checked = true;
            this.rdoDefault.Location = new System.Drawing.Point(19, 22);
            this.rdoDefault.Name = "rdoDefault";
            this.rdoDefault.Size = new System.Drawing.Size(66, 20);
            this.rdoDefault.TabIndex = 7;
            this.rdoDefault.TabStop = true;
            this.rdoDefault.Text = "Default";
            this.rdoDefault.UseVisualStyleBackColor = true;
            this.rdoDefault.Visible = false;
            this.rdoDefault.CheckedChanged += new System.EventHandler(this.VisualStyleChanged);
            // 
            // grpPreview
            // 
            this.grpPreview.Controls.Add(this.editorPreview);
            this.grpPreview.Controls.Add(this.c1GridVisualStyle);
            this.grpPreview.Location = new System.Drawing.Point(347, 106);
            this.grpPreview.Name = "grpPreview";
            this.grpPreview.Size = new System.Drawing.Size(449, 264);
            this.grpPreview.TabIndex = 77;
            this.grpPreview.TabStop = false;
            this.grpPreview.Text = "Preview";
            // 
            // editorPreview
            // 
            this.editorPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorPreview.CaretLineVisible = true;
            this.editorPreview.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorPreview.IndentWidth = 2;
            this.editorPreview.Location = new System.Drawing.Point(7, 19);
            this.editorPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorPreview.Name = "editorPreview";
            this.editorPreview.Size = new System.Drawing.Size(427, 231);
            this.editorPreview.Styler = null;
            this.editorPreview.TabIndex = 67;
            this.editorPreview.Visible = false;
            this.editorPreview.WhitespaceSize = 3;
            this.editorPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // c1GridVisualStyle
            // 
            this.c1GridVisualStyle.AllowUpdate = false;
            this.c1GridVisualStyle.AllowUpdateOnBlur = false;
            this.c1GridVisualStyle.AlternatingRows = true;
            this.c1GridVisualStyle.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridVisualStyle.CaptionHeight = 19;
            this.c1GridVisualStyle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridVisualStyle.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridVisualStyle.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridVisualStyle.Images"))));
            this.c1GridVisualStyle.Location = new System.Drawing.Point(7, 19);
            this.c1GridVisualStyle.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridVisualStyle.Name = "c1GridVisualStyle";
            this.c1GridVisualStyle.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridVisualStyle.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridVisualStyle.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridVisualStyle.PreviewInfo.ZoomFactor = 75D;
            this.c1GridVisualStyle.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridVisualStyle.PrintInfo.MeasurementPrinterName = null;
            this.c1GridVisualStyle.RowHeight = 17;
            this.c1GridVisualStyle.Size = new System.Drawing.Size(427, 231);
            this.c1GridVisualStyle.TabIndex = 66;
            this.c1GridVisualStyle.UseCompatibleTextRendering = false;
            this.c1GridVisualStyle.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Custom;
            this.c1GridVisualStyle.PropBag = resources.GetString("c1GridVisualStyle.PropBag");
            // 
            // lblSaveAsType
            // 
            this.lblSaveAsType.AutoSize = true;
            this.lblSaveAsType.Location = new System.Drawing.Point(19, 48);
            this.lblSaveAsType.Name = "lblSaveAsType";
            this.lblSaveAsType.Size = new System.Drawing.Size(80, 16);
            this.lblSaveAsType.TabIndex = 79;
            this.lblSaveAsType.Text = "Save as type:";
            // 
            // cboSaveAsType
            // 
            this.cboSaveAsType.AllowSpinLoop = false;
            this.cboSaveAsType.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSaveAsType.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboSaveAsType.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboSaveAsType.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboSaveAsType.GapHeight = 0;
            this.cboSaveAsType.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboSaveAsType.Items.Add("Excel 2003 (*.xls)");
            this.cboSaveAsType.Items.Add("Excel 2007 (*.xlsx)");
            this.cboSaveAsType.Items.Add("CSV (*.csv)");
            this.cboSaveAsType.Items.Add("JSON (*.json)");
            this.cboSaveAsType.Items.Add("XML (*.xml)");
            this.cboSaveAsType.ItemsDisplayMember = "";
            this.cboSaveAsType.ItemsValueMember = "";
            this.cboSaveAsType.Location = new System.Drawing.Point(84, 46);
            this.cboSaveAsType.Name = "cboSaveAsType";
            this.cboSaveAsType.Size = new System.Drawing.Size(180, 21);
            this.cboSaveAsType.TabIndex = 3;
            this.cboSaveAsType.Tag = null;
            this.cboSaveAsType.TextDetached = true;
            this.cboSaveAsType.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboSaveAsType.TextChanged += new System.EventHandler(this.cboSaveAsType_TextChanged);
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnBrowseFile.Location = new System.Drawing.Point(736, 16);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new System.Drawing.Size(22, 21);
            this.btnBrowseFile.TabIndex = 2;
            this.btnBrowseFile.Tag = "1";
            this.btnBrowseFile.Text = "...";
            this.btnBrowseFile.UseVisualStyleBackColor = true;
            this.btnBrowseFile.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
            // 
            // btnExport
            // 
            this.btnExport.Location = new System.Drawing.Point(493, 383);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(79, 30);
            this.btnExport.TabIndex = 9;
            this.btnExport.Text = "&Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(718, 383);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(79, 30);
            this.btnClose.TabIndex = 10;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSaveAllTheSettings
            // 
            this.btnSaveAllTheSettings.Location = new System.Drawing.Point(636, 76);
            this.btnSaveAllTheSettings.Name = "btnSaveAllTheSettings";
            this.btnSaveAllTheSettings.Size = new System.Drawing.Size(160, 29);
            this.btnSaveAllTheSettings.TabIndex = 6;
            this.btnSaveAllTheSettings.Text = "&Save all the settings";
            this.btnSaveAllTheSettings.UseVisualStyleBackColor = true;
            this.btnSaveAllTheSettings.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSaveAllTheSettings.Click += new System.EventHandler(this.btnSaveAllTheSettings_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Green;
            this.lblInfo.Location = new System.Drawing.Point(20, 391);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 16);
            this.lblInfo.TabIndex = 89;
            // 
            // btnExportAndClose
            // 
            this.btnExportAndClose.Location = new System.Drawing.Point(582, 383);
            this.btnExportAndClose.Name = "btnExportAndClose";
            this.btnExportAndClose.Size = new System.Drawing.Size(126, 30);
            this.btnExportAndClose.TabIndex = 90;
            this.btnExportAndClose.Text = "E&xport && Close";
            this.btnExportAndClose.UseVisualStyleBackColor = true;
            this.btnExportAndClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExportAndClose.Click += new System.EventHandler(this.btnExportAndClose_Click);
            // 
            // cboFileName
            // 
            this.cboFileName.AllowSpinLoop = false;
            this.cboFileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboFileName.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboFileName.GapHeight = 0;
            this.cboFileName.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboFileName.ItemsDisplayMember = "";
            this.cboFileName.ItemsValueMember = "";
            this.cboFileName.Location = new System.Drawing.Point(91, 16);
            this.cboFileName.Name = "cboFileName";
            this.cboFileName.Size = new System.Drawing.Size(653, 21);
            this.cboFileName.TabIndex = 91;
            this.cboFileName.Tag = " ";
            this.cboFileName.TextDetached = true;
            this.cboFileName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboFileName.Leave += new System.EventHandler(this.cboFileName_Leave);
            // 
            // tmrExcelDetect
            // 
            this.tmrExcelDetect.Interval = 5000;
            this.tmrExcelDetect.Tick += new System.EventHandler(this.tmrExcelDetect_Tick);
            // 
            // lblCSVDelimiters
            // 
            this.lblCSVDelimiters.AutoSize = true;
            this.lblCSVDelimiters.Location = new System.Drawing.Point(300, 48);
            this.lblCSVDelimiters.Name = "lblCSVDelimiters";
            this.lblCSVDelimiters.Size = new System.Drawing.Size(66, 16);
            this.lblCSVDelimiters.TabIndex = 92;
            this.lblCSVDelimiters.Text = "Delimiters:";
            this.lblCSVDelimiters.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCSVDelimiters.Visible = false;
            // 
            // cboCSVDelimiters
            // 
            this.cboCSVDelimiters.AllowSpinLoop = false;
            this.cboCSVDelimiters.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboCSVDelimiters.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboCSVDelimiters.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboCSVDelimiters.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboCSVDelimiters.GapHeight = 0;
            this.cboCSVDelimiters.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboCSVDelimiters.Items.Add("Tab");
            this.cboCSVDelimiters.Items.Add("Semicolon");
            this.cboCSVDelimiters.Items.Add("Comma");
            this.cboCSVDelimiters.Items.Add("Space");
            this.cboCSVDelimiters.Items.Add("Colon");
            this.cboCSVDelimiters.Items.Add("Slash");
            this.cboCSVDelimiters.Items.Add("Backslash");
            this.cboCSVDelimiters.Items.Add("Pipe");
            this.cboCSVDelimiters.ItemsDisplayMember = "";
            this.cboCSVDelimiters.ItemsValueMember = "";
            this.cboCSVDelimiters.Location = new System.Drawing.Point(445, 46);
            this.cboCSVDelimiters.Name = "cboCSVDelimiters";
            this.cboCSVDelimiters.Size = new System.Drawing.Size(121, 21);
            this.cboCSVDelimiters.TabIndex = 93;
            this.cboCSVDelimiters.Tag = null;
            this.cboCSVDelimiters.TextDetached = true;
            this.cboCSVDelimiters.Visible = false;
            this.cboCSVDelimiters.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboCSVDelimiters.TextChanged += new System.EventHandler(this.cboCSVDelimiters_TextChanged);
            // 
            // chkColumnResize
            // 
            this.chkColumnResize.AutoSize = true;
            this.chkColumnResize.Location = new System.Drawing.Point(425, 76);
            this.chkColumnResize.Name = "chkColumnResize";
            this.chkColumnResize.Size = new System.Drawing.Size(140, 20);
            this.chkColumnResize.TabIndex = 94;
            this.chkColumnResize.Text = "Adjust column width";
            this.chkColumnResize.UseVisualStyleBackColor = true;
            this.chkColumnResize.CheckedChanged += new System.EventHandler(this.chkColumnResize_CheckedChanged);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(19, 387);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(367, 23);
            this.progressBar1.TabIndex = 95;
            this.progressBar1.Visible = false;
            // 
            // lblEncoding
            // 
            this.lblEncoding.AutoSize = true;
            this.lblEncoding.Location = new System.Drawing.Point(550, 48);
            this.lblEncoding.Name = "lblEncoding";
            this.lblEncoding.Size = new System.Drawing.Size(64, 16);
            this.lblEncoding.TabIndex = 96;
            this.lblEncoding.Text = "Encoding:";
            this.lblEncoding.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblEncoding.Visible = false;
            // 
            // cboEncoding
            // 
            this.cboEncoding.AllowSpinLoop = false;
            this.cboEncoding.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboEncoding.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboEncoding.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboEncoding.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboEncoding.GapHeight = 0;
            this.cboEncoding.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboEncoding.Items.Add("Default");
            this.cboEncoding.Items.Add("ASCII");
            this.cboEncoding.Items.Add("UTF-8");
            this.cboEncoding.Items.Add("Unicode");
            this.cboEncoding.Items.Add("Unicode big endian");
            this.cboEncoding.ItemsDisplayMember = "";
            this.cboEncoding.ItemsValueMember = "";
            this.cboEncoding.Location = new System.Drawing.Point(621, 46);
            this.cboEncoding.Name = "cboEncoding";
            this.cboEncoding.Size = new System.Drawing.Size(137, 21);
            this.cboEncoding.TabIndex = 97;
            this.cboEncoding.Tag = null;
            this.cboEncoding.TextDetached = true;
            this.cboEncoding.Visible = false;
            this.cboEncoding.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(391, 387);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(67, 23);
            this.btnCancel.TabIndex = 98;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Visible = false;
            this.btnCancel.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // chkConvertCRLF
            // 
            this.chkConvertCRLF.AutoSize = true;
            this.chkConvertCRLF.BackColor = System.Drawing.Color.Transparent;
            this.chkConvertCRLF.BorderColor = System.Drawing.Color.Transparent;
            this.chkConvertCRLF.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkConvertCRLF.ForeColor = System.Drawing.Color.Black;
            this.chkConvertCRLF.Location = new System.Drawing.Point(776, 46);
            this.chkConvertCRLF.Name = "chkConvertCRLF";
            this.chkConvertCRLF.Padding = new System.Windows.Forms.Padding(1);
            this.chkConvertCRLF.Size = new System.Drawing.Size(198, 22);
            this.chkConvertCRLF.TabIndex = 99;
            this.chkConvertCRLF.Text = "Convert CR LF to &&#xD; &&#xA;";
            this.chkConvertCRLF.UseVisualStyleBackColor = true;
            this.chkConvertCRLF.Value = null;
            this.chkConvertCRLF.Visible = false;
            this.chkConvertCRLF.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkConvertCRLF.CheckedChanged += new System.EventHandler(this.chkConvertCRLF_CheckedChanged);
            // 
            // ExportToFileForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(815, 430);
            this.Controls.Add(this.lblFileName);
            this.Controls.Add(this.lblWorksheetName);
            this.Controls.Add(this.grpPreview);
            this.Controls.Add(this.chkConvertCRLF);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblCSVDelimiters);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnExport);
            this.Controls.Add(this.cboFileName);
            this.Controls.Add(this.btnExportAndClose);
            this.Controls.Add(this.cboEncoding);
            this.Controls.Add(this.btnBrowseFile);
            this.Controls.Add(this.grpVisualStyle);
            this.Controls.Add(this.cboCSVDelimiters);
            this.Controls.Add(this.lblEncoding);
            this.Controls.Add(this.cboSaveAsType);
            this.Controls.Add(this.txtWorksheetName);
            this.Controls.Add(this.chkColumnResize);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.chkAutoOpenExportedFile);
            this.Controls.Add(this.lblSaveAsType);
            this.Controls.Add(this.btnSaveAllTheSettings);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ExportToFileForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Export all data to File";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form_FormClosed);
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtWorksheetName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkAutoOpenExportedFile)).EndInit();
            this.grpVisualStyle.ResumeLayout(false);
            this.grpVisualStyle.PerformLayout();
            this.grpDataGrid.ResumeLayout(false);
            this.grpDataGrid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridRowHeight)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboGridFontName)).EndInit();
            this.grpPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVisualStyle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboSaveAsType)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSaveAllTheSettings)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExportAndClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboFileName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboCSVDelimiters)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboEncoding)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkConvertCRLF)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblFileName;
        private C1.Win.C1Input.C1TextBox txtWorksheetName;
        private System.Windows.Forms.Label lblWorksheetName;
        private C1.Win.C1Input.C1CheckBox chkAutoOpenExportedFile;
        private System.Windows.Forms.GroupBox grpVisualStyle;
        private System.Windows.Forms.Label lblSaveAsType;
        private C1.Win.C1Input.C1ComboBox cboSaveAsType;
        private System.Windows.Forms.RadioButton rdoDefault;
        private C1.Win.C1Input.C1Button btnBrowseFile;
        private System.Windows.Forms.RadioButton rdoCustom;
        private System.Windows.Forms.GroupBox grpPreview;
        private C1.Win.C1Input.C1Button btnExport;
        private C1.Win.C1Input.C1Button btnClose;
        private System.Windows.Forms.GroupBox grpDataGrid;
        private System.Windows.Forms.Label lblOddRowBackColor;
        private System.Windows.Forms.Label lblEvenRowBackColor;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridVisualStyle;
        private System.Windows.Forms.Label lblHeadingBackColor;
        private C1.Win.C1Input.C1Button btnSaveAllTheSettings;
        private System.Windows.Forms.Label lblInfo;
        private C1.C1Excel.C1XLBook c1XLBook1;
        private System.Windows.Forms.ComboBox cboOddRowBackColor;
        private System.Windows.Forms.ComboBox cboEvenRowBackColor;
        private System.Windows.Forms.ComboBox cboHeadingBackColor;
        private C1.Win.C1Input.C1Button btnExportAndClose;
        private C1.Win.C1Input.C1ComboBox cboFileName;
        private System.Windows.Forms.Label lblExcelDetect;
        private System.Windows.Forms.Timer tmrExcelDetect;
        private System.Windows.Forms.Label lblCSVDelimiters;
        private C1.Win.C1Input.C1ComboBox cboCSVDelimiters;
        private System.Windows.Forms.CheckBox chkColumnResize;
        private System.Windows.Forms.ProgressBar progressBar1;
        private C1.Win.C1Input.C1ComboBox cboGridRowHeight;
        private C1.Win.C1Input.C1ComboBox cboGridFontSize;
        private C1.Win.C1Input.C1FontPicker cboGridFontName;
        private System.Windows.Forms.Label lblFontSize;
        private System.Windows.Forms.Label lblFontName;
        private System.Windows.Forms.Label lblRowHeight;
        private System.Windows.Forms.Label lblEncoding;
        private C1.Win.C1Input.C1ComboBox cboEncoding;
        private C1.Win.C1Input.C1Button btnCancel;
        private C1.Win.C1Input.C1CheckBox chkConvertCRLF;
        private JasonLibrary.UI.Controls.ScintillaEditor editorPreview;
    }
}