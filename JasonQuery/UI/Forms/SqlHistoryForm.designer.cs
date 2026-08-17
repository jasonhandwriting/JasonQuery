using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    partial class SqlHistoryForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlHistoryForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.pnlHideCheckBox = new System.Windows.Forms.Panel();
            this.c1GridSqlHistory = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.chkCopyAsHTML = new C1.Win.C1Input.C1CheckBox();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.editorMessage = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.editorSql = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.btnSelectAll2 = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2 = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.tmrMother2Child = new System.Windows.Forms.Timer(this.components);
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnCompact = new C1.Win.C1Input.C1Button();
            this.btnDelete = new C1.Win.C1Input.C1Button();
            this.btnUnselectAll = new C1.Win.C1Input.C1Button();
            this.btnSelectAll = new C1.Win.C1Input.C1Button();
            this.btnSearch = new C1.Win.C1Input.C1Button();
            this.cboHistoryPeriod = new C1.Win.C1Input.C1ComboBox();
            this.cboConnectionName = new C1.Win.C1Input.C1ComboBox();
            this.cboDataSource = new C1.Win.C1Input.C1ComboBox();
            this.chkShowFilterRow = new C1.Win.C1Input.C1CheckBox();
            this.picSeparator2 = new System.Windows.Forms.PictureBox();
            this.lblHistoryPeriod = new System.Windows.Forms.Label();
            this.picSeparator1 = new System.Windows.Forms.PictureBox();
            this.lblConnectionName = new System.Windows.Forms.Label();
            this.lblDataSource = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSqlHistory)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCompact)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSearch)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHistoryPeriod)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnectionName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDataSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSeparator2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSeparator1)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1019, 25);
            this.toolStrip1.TabIndex = 15;
            this.toolStrip1.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.toolStrip1, "(default)");
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 28);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.pnlHideCheckBox);
            this.splitContainer1.Panel1.Controls.Add(this.c1GridSqlHistory);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.chkCopyAsHTML);
            this.splitContainer1.Panel2.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel2.Controls.Add(this.toolStrip2);
            this.splitContainer1.Size = new System.Drawing.Size(1019, 425);
            this.splitContainer1.SplitterDistance = 270;
            this.splitContainer1.TabIndex = 17;
            this.c1ThemeController1.SetTheme(this.splitContainer1, "(default)");
            // 
            // pnlHideCheckBox
            // 
            this.pnlHideCheckBox.Location = new System.Drawing.Point(19, 25);
            this.pnlHideCheckBox.Name = "pnlHideCheckBox";
            this.pnlHideCheckBox.Size = new System.Drawing.Size(20, 17);
            this.pnlHideCheckBox.TabIndex = 15;
            this.c1ThemeController1.SetTheme(this.pnlHideCheckBox, "(default)");
            this.pnlHideCheckBox.Visible = false;
            // 
            // c1GridSqlHistory
            // 
            this.c1GridSqlHistory.AlternatingRows = true;
            this.c1GridSqlHistory.CaptionHeight = 19;
            this.c1GridSqlHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1GridSqlHistory.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridSqlHistory.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridSqlHistory.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridSqlHistory.Images"))));
            this.c1GridSqlHistory.Location = new System.Drawing.Point(0, 0);
            this.c1GridSqlHistory.Name = "c1GridSqlHistory";
            this.c1GridSqlHistory.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridSqlHistory.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridSqlHistory.PreviewInfo.ZoomFactor = 75D;
            this.c1GridSqlHistory.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridSqlHistory.PrintInfo.MeasurementPrinterName = null;
            this.c1GridSqlHistory.RowHeight = 17;
            this.c1GridSqlHistory.Size = new System.Drawing.Size(1019, 270);
            this.c1GridSqlHistory.TabIndex = 14;
            this.c1GridSqlHistory.Text = "c1TrueDBGrid2";
            this.c1GridSqlHistory.UseCompatibleTextRendering = false;
            this.c1GridSqlHistory.VisualStyle = C1.Win.C1TrueDBGrid.VisualStyle.Office2010Blue;
            this.c1GridSqlHistory.AfterColUpdate += new C1.Win.C1TrueDBGrid.ColEventHandler(this.c1GridSqlHistory_AfterColUpdate);
            this.c1GridSqlHistory.RowColChange += new C1.Win.C1TrueDBGrid.RowColChangeEventHandler(this.c1GridSqlHistory_RowColChange);
            this.c1GridSqlHistory.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridSqlHistory_FetchCellStyle);
            this.c1GridSqlHistory.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1GridSqlHistory_MouseDown);
            this.c1GridSqlHistory.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridSqlHistory_MouseUp);
            this.c1GridSqlHistory.PropBag = resources.GetString("c1GridSqlHistory.PropBag");
            // 
            // chkCopyAsHTML
            // 
            this.chkCopyAsHTML.AutoSize = true;
            this.chkCopyAsHTML.BackColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCopyAsHTML.ForeColor = System.Drawing.Color.Black;
            this.chkCopyAsHTML.Location = new System.Drawing.Point(71, 4);
            this.chkCopyAsHTML.Name = "chkCopyAsHTML";
            this.chkCopyAsHTML.Padding = new System.Windows.Forms.Padding(1);
            this.chkCopyAsHTML.Size = new System.Drawing.Size(198, 22);
            this.chkCopyAsHTML.TabIndex = 4;
            this.chkCopyAsHTML.Text = "Copy SQL Statement as HTML";
            this.c1ThemeController1.SetTheme(this.chkCopyAsHTML, "(default)");
            this.chkCopyAsHTML.UseVisualStyleBackColor = true;
            this.chkCopyAsHTML.Value = null;
            this.chkCopyAsHTML.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 31);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.editorMessage);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.editorSql);
            this.splitContainer2.Size = new System.Drawing.Size(1019, 120);
            this.splitContainer2.SplitterDistance = 372;
            this.splitContainer2.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.splitContainer2, "(default)");
            // 
            // editorMessage
            // 
            this.editorMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorMessage.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorMessage.CaretLineVisible = true;
            this.editorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorMessage.IndentationGuides = ScintillaNET.IndentView.None;
            this.editorMessage.Location = new System.Drawing.Point(0, 0);
            this.editorMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorMessage.Name = "editorMessage";
            this.editorMessage.ReadOnly = true;
            this.editorMessage.Size = new System.Drawing.Size(372, 120);
            this.editorMessage.Styler = null;
            this.editorMessage.TabIndex = 42;
            this.editorMessage.Tag = "";
            this.editorMessage.WhitespaceSize = 3;
            this.editorMessage.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorMessage.Enter += new System.EventHandler(this.editorMessage_Enter);
            this.editorMessage.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorMessage_MouseDown);
            // 
            // editorSql
            // 
            this.editorSql.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSql.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSql.CaretLineVisible = true;
            this.editorSql.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorSql.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSql.Location = new System.Drawing.Point(0, 0);
            this.editorSql.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSql.Name = "editorSql";
            this.editorSql.Size = new System.Drawing.Size(643, 120);
            this.editorSql.Styler = null;
            this.editorSql.TabIndex = 43;
            this.editorSql.Tag = "";
            this.editorSql.WhitespaceSize = 3;
            this.editorSql.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSql.Enter += new System.EventHandler(this.editorSql_Enter);
            this.editorSql.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorSql_MouseDown);
            // 
            // toolStrip2
            // 
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAll2,
            this.btnCopy,
            this.btnWordWrap,
            this.btnWordWrap2,
            this.btnShowAllCharacters,
            this.btnShowAllCharacters2,
            this.toolStripSeparator2,
            this.toolStripLabel5});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(1019, 31);
            this.toolStrip2.TabIndex = 2;
            this.toolStrip2.Text = "toolStrip2";
            this.c1ThemeController1.SetTheme(this.toolStrip2, "(default)");
            // 
            // btnSelectAll2
            // 
            this.btnSelectAll2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAll2.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll2.Image")));
            this.btnSelectAll2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAll2.Name = "btnSelectAll2";
            this.btnSelectAll2.Size = new System.Drawing.Size(28, 28);
            this.btnSelectAll2.ToolTipText = "Select All";
            this.btnSelectAll2.Visible = false;
            this.btnSelectAll2.Click += new System.EventHandler(this.btnSelectAll2_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy.Image")));
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(28, 28);
            this.btnCopy.ToolTipText = "Copy";
            this.btnCopy.Visible = false;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
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
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Font = new System.Drawing.Font("Symbol", 5.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(2)));
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(7, 28);
            this.toolStripLabel5.Text = " ";
            // 
            // tmrMother2Child
            // 
            this.tmrMother2Child.Enabled = true;
            this.tmrMother2Child.Tick += new System.EventHandler(this.tmrMother2Child_Tick);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Controls.Add(this.btnCompact);
            this.panel1.Controls.Add(this.btnDelete);
            this.panel1.Controls.Add(this.btnUnselectAll);
            this.panel1.Controls.Add(this.btnSelectAll);
            this.panel1.Controls.Add(this.btnSearch);
            this.panel1.Controls.Add(this.cboHistoryPeriod);
            this.panel1.Controls.Add(this.cboConnectionName);
            this.panel1.Controls.Add(this.cboDataSource);
            this.panel1.Controls.Add(this.chkShowFilterRow);
            this.panel1.Controls.Add(this.picSeparator2);
            this.panel1.Controls.Add(this.lblHistoryPeriod);
            this.panel1.Controls.Add(this.picSeparator1);
            this.panel1.Controls.Add(this.lblConnectionName);
            this.panel1.Controls.Add(this.lblDataSource);
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1081, 25);
            this.panel1.TabIndex = 18;
            this.c1ThemeController1.SetTheme(this.panel1, "(default)");
            // 
            // btnCompact
            // 
            this.btnCompact.Image = ((System.Drawing.Image)(resources.GetObject("btnCompact.Image")));
            this.btnCompact.Location = new System.Drawing.Point(1014, 2);
            this.btnCompact.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnCompact.Name = "btnCompact";
            this.btnCompact.Size = new System.Drawing.Size(21, 21);
            this.btnCompact.TabIndex = 109;
            this.c1ThemeController1.SetTheme(this.btnCompact, "(default)");
            this.btnCompact.UseVisualStyleBackColor = true;
            this.btnCompact.Visible = false;
            this.btnCompact.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCompact.Click += new System.EventHandler(this.btnCompact_Click);
            // 
            // btnDelete
            // 
            this.btnDelete.Image = ((System.Drawing.Image)(resources.GetObject("btnDelete.Image")));
            this.btnDelete.Location = new System.Drawing.Point(822, 3);
            this.btnDelete.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnDelete.Name = "btnDelete";
            this.btnDelete.Size = new System.Drawing.Size(21, 21);
            this.btnDelete.TabIndex = 108;
            this.btnDelete.UseVisualStyleBackColor = true;
            this.btnDelete.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnDelete.Click += new System.EventHandler(this.btnDelete_Click);
            // 
            // btnUnselectAll
            // 
            this.btnUnselectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnUnselectAll.Image")));
            this.btnUnselectAll.Location = new System.Drawing.Point(791, 3);
            this.btnUnselectAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnUnselectAll.Name = "btnUnselectAll";
            this.btnUnselectAll.Size = new System.Drawing.Size(21, 21);
            this.btnUnselectAll.TabIndex = 108;
            this.btnUnselectAll.Tag = "UnselectAll";
            this.btnUnselectAll.UseVisualStyleBackColor = true;
            this.btnUnselectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnUnselectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.Location = new System.Drawing.Point(759, 3);
            this.btnSelectAll.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(21, 21);
            this.btnSelectAll.TabIndex = 108;
            this.btnSelectAll.Tag = "SelectAll";
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSearch
            // 
            this.btnSearch.Image = ((System.Drawing.Image)(resources.GetObject("btnSearch.Image")));
            this.btnSearch.Location = new System.Drawing.Point(728, 3);
            this.btnSearch.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnSearch.Name = "btnSearch";
            this.btnSearch.Size = new System.Drawing.Size(21, 21);
            this.btnSearch.TabIndex = 107;
            this.btnSearch.UseVisualStyleBackColor = true;
            this.btnSearch.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
            // 
            // cboHistoryPeriod
            // 
            this.cboHistoryPeriod.AllowSpinLoop = false;
            this.cboHistoryPeriod.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboHistoryPeriod.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboHistoryPeriod.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboHistoryPeriod.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboHistoryPeriod.GapHeight = 0;
            this.cboHistoryPeriod.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboHistoryPeriod.ItemsDisplayMember = "";
            this.cboHistoryPeriod.ItemsValueMember = "";
            this.cboHistoryPeriod.Location = new System.Drawing.Point(570, 3);
            this.cboHistoryPeriod.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboHistoryPeriod.Name = "cboHistoryPeriod";
            this.cboHistoryPeriod.Size = new System.Drawing.Size(131, 21);
            this.cboHistoryPeriod.TabIndex = 44;
            this.cboHistoryPeriod.Tag = null;
            this.cboHistoryPeriod.TextDetached = true;
            this.cboHistoryPeriod.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboHistoryPeriod.SelectedIndexChanged += new System.EventHandler(this.Query_SelectedIndexChanged);
            // 
            // cboConnectionName
            // 
            this.cboConnectionName.AllowSpinLoop = false;
            this.cboConnectionName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboConnectionName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboConnectionName.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboConnectionName.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboConnectionName.GapHeight = 0;
            this.cboConnectionName.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboConnectionName.ItemsDisplayMember = "";
            this.cboConnectionName.ItemsValueMember = "";
            this.cboConnectionName.Location = new System.Drawing.Point(316, 3);
            this.cboConnectionName.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboConnectionName.Name = "cboConnectionName";
            this.cboConnectionName.Size = new System.Drawing.Size(153, 21);
            this.cboConnectionName.TabIndex = 45;
            this.cboConnectionName.Tag = null;
            this.cboConnectionName.TextDetached = true;
            this.cboConnectionName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboConnectionName.SelectedIndexChanged += new System.EventHandler(this.Query_SelectedIndexChanged);
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
            this.cboDataSource.ItemsDisplayMember = "";
            this.cboDataSource.ItemsValueMember = "";
            this.cboDataSource.Location = new System.Drawing.Point(83, 3);
            this.cboDataSource.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboDataSource.Name = "cboDataSource";
            this.cboDataSource.Size = new System.Drawing.Size(105, 21);
            this.cboDataSource.TabIndex = 46;
            this.cboDataSource.Tag = null;
            this.cboDataSource.TextDetached = true;
            this.cboDataSource.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboDataSource.SelectedIndexChanged += new System.EventHandler(this.Query_SelectedIndexChanged);
            // 
            // chkShowFilterRow
            // 
            this.chkShowFilterRow.AutoSize = true;
            this.chkShowFilterRow.BackColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRow.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowFilterRow.ForeColor = System.Drawing.Color.Black;
            this.chkShowFilterRow.Location = new System.Drawing.Point(878, 3);
            this.chkShowFilterRow.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkShowFilterRow.Name = "chkShowFilterRow";
            this.chkShowFilterRow.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowFilterRow.Size = new System.Drawing.Size(118, 22);
            this.chkShowFilterRow.TabIndex = 43;
            this.chkShowFilterRow.Text = "Show Filter Row";
            this.c1ThemeController1.SetTheme(this.chkShowFilterRow, "(default)");
            this.chkShowFilterRow.UseVisualStyleBackColor = true;
            this.chkShowFilterRow.Value = null;
            this.chkShowFilterRow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowFilterRow.CheckedChanged += new System.EventHandler(this.chkShowFilterRow_Click);
            // 
            // picSeparator2
            // 
            this.picSeparator2.BackColor = System.Drawing.Color.Transparent;
            this.picSeparator2.Image = ((System.Drawing.Image)(resources.GetObject("picSeparator2.Image")));
            this.picSeparator2.Location = new System.Drawing.Point(852, 1);
            this.picSeparator2.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.picSeparator2.Name = "picSeparator2";
            this.picSeparator2.Size = new System.Drawing.Size(12, 24);
            this.picSeparator2.TabIndex = 12;
            this.picSeparator2.TabStop = false;
            // 
            // lblHistoryPeriod
            // 
            this.lblHistoryPeriod.AutoSize = true;
            this.lblHistoryPeriod.Location = new System.Drawing.Point(480, 5);
            this.lblHistoryPeriod.Name = "lblHistoryPeriod";
            this.lblHistoryPeriod.Size = new System.Drawing.Size(90, 16);
            this.lblHistoryPeriod.TabIndex = 5;
            this.lblHistoryPeriod.Text = "History Period:";
            // 
            // picSeparator1
            // 
            this.picSeparator1.BackColor = System.Drawing.Color.Transparent;
            this.picSeparator1.Image = ((System.Drawing.Image)(resources.GetObject("picSeparator1.Image")));
            this.picSeparator1.Location = new System.Drawing.Point(709, 1);
            this.picSeparator1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.picSeparator1.Name = "picSeparator1";
            this.picSeparator1.Size = new System.Drawing.Size(12, 24);
            this.picSeparator1.TabIndex = 4;
            this.picSeparator1.TabStop = false;
            // 
            // lblConnectionName
            // 
            this.lblConnectionName.AutoSize = true;
            this.lblConnectionName.Location = new System.Drawing.Point(202, 5);
            this.lblConnectionName.Name = "lblConnectionName";
            this.lblConnectionName.Size = new System.Drawing.Size(114, 16);
            this.lblConnectionName.TabIndex = 2;
            this.lblConnectionName.Text = "Connection Name:";
            // 
            // lblDataSource
            // 
            this.lblDataSource.AutoSize = true;
            this.lblDataSource.Location = new System.Drawing.Point(3, 5);
            this.lblDataSource.Name = "lblDataSource";
            this.lblDataSource.Size = new System.Drawing.Size(80, 16);
            this.lblDataSource.TabIndex = 0;
            this.lblDataSource.Text = "Data Source:";
            // 
            // SqlHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1019, 453);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SqlHistoryForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SQL History";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSqlHistory)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnCompact)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDelete)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSearch)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboHistoryPeriod)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboConnectionName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboDataSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRow)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSeparator2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picSeparator1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridSqlHistory;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2;
        private System.Windows.Forms.ToolStripButton btnSelectAll2;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.Timer tmrMother2Child;
        private System.Windows.Forms.Panel pnlHideCheckBox;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private JasonLibrary.UI.Controls.ScintillaEditor editorMessage;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSql;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Input.C1CheckBox chkCopyAsHTML;
        private System.Windows.Forms.Panel panel1;
        private C1.Win.C1Input.C1Button btnCompact;
        private C1.Win.C1Input.C1Button btnDelete;
        private C1.Win.C1Input.C1Button btnUnselectAll;
        private C1.Win.C1Input.C1Button btnSelectAll;
        private C1.Win.C1Input.C1Button btnSearch;
        private C1.Win.C1Input.C1ComboBox cboHistoryPeriod;
        private C1.Win.C1Input.C1ComboBox cboConnectionName;
        private C1.Win.C1Input.C1ComboBox cboDataSource;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRow;
        private System.Windows.Forms.PictureBox picSeparator2;
        private System.Windows.Forms.Label lblHistoryPeriod;
        private System.Windows.Forms.PictureBox picSeparator1;
        private System.Windows.Forms.Label lblConnectionName;
        private System.Windows.Forms.Label lblDataSource;
    }
}

