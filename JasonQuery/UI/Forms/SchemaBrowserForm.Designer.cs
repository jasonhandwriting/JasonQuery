using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    partial class SchemaBrowserForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchemaBrowserForm));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.lblSchema = new System.Windows.Forms.Label();
            this.cboSchema = new C1.Win.C1Input.C1ComboBox();
            this.c1GridSchemaBrowser = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.lblFilter = new System.Windows.Forms.Label();
            this.txtSchemaFilter = new C1.Win.C1Input.C1TextBox();
            this.lblSchemaType0 = new System.Windows.Forms.Label();
            this.tsSchemaBrowser = new System.Windows.Forms.ToolStrip();
            this.btnRefresh = new System.Windows.Forms.ToolStripButton();
            this.tsSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExpandCollapse = new System.Windows.Forms.ToolStripSplitButton();
            this.mnuExpandAll = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCollapseAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel4 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripLabel5 = new System.Windows.Forms.ToolStripLabel();
            this.tabSchemaBrowser = new C1.Win.C1Command.C1DockingTab();
            this.tabSqlPane = new C1.Win.C1Command.C1DockingTabPage();
            this.editorSqlPane = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.chkCopyAsHTML = new C1.Win.C1Input.C1CheckBox();
            this.tsSqlPane = new System.Windows.Forms.ToolStrip();
            this.btnSelectAllSqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnCopySqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAsSqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrapSqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2SqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharactersSqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2SqlPane = new System.Windows.Forms.ToolStripButton();
            this.tsSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.lblInfo = new System.Windows.Forms.ToolStripLabel();
            this.btnZoomInSqlPane = new System.Windows.Forms.ToolStripButton();
            this.btnZoomOutSqlPane = new System.Windows.Forms.ToolStripButton();
            this.tabTableStructure = new C1.Win.C1Command.C1DockingTabPage();
            this.c1GridStructure = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsTableStructure = new System.Windows.Forms.ToolStrip();
            this.lblTableOrViewName = new System.Windows.Forms.ToolStripLabel();
            this.lblTableName01 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSelectAllTableStructure = new System.Windows.Forms.ToolStripButton();
            this.btnCopyTableStructure = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExportToFileTableStructure = new System.Windows.Forms.ToolStripButton();
            this.tabView100RowsTop = new C1.Win.C1Command.C1DockingTabPage();
            this.c1Grid100RowsTop = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsView100RowsTop = new System.Windows.Forms.ToolStrip();
            this.lblViewName = new System.Windows.Forms.ToolStripLabel();
            this.lblTableName02 = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSelectAllTop100 = new System.Windows.Forms.ToolStripButton();
            this.btnCopyTop100 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExportToFileTop100 = new System.Windows.Forms.ToolStripButton();
            this.tabData = new C1.Win.C1Command.C1DockingTabPage();
            this.lblFind2 = new System.Windows.Forms.Label();
            this.cboFind = new C1.Win.C1Input.C1ComboBox();
            this.chkShowFilterRowData = new C1.Win.C1Input.C1CheckBox();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.lblColumnNamePosition = new System.Windows.Forms.Label();
            this.btnHelp_ColumnName = new C1.Win.C1Input.C1Button();
            this.lblPosition2 = new System.Windows.Forms.Label();
            this.txtColumnFilter = new C1.Win.C1Input.C1TextBox();
            this.c1GridColumns = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsColumnFilter = new System.Windows.Forms.ToolStrip();
            this.lblColumnFilterData = new System.Windows.Forms.ToolStripLabel();
            this.c1GridData = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.tsData = new System.Windows.Forms.ToolStrip();
            this.btnFilterData = new System.Windows.Forms.ToolStripButton();
            this.btnFilterRedData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.btnRefreshData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.btnShowColumnsData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.btnEditCellData = new System.Windows.Forms.ToolStripButton();
            this.btnEditCellWithEditFormData = new System.Windows.Forms.ToolStripButton();
            this.btnSetNullData = new System.Windows.Forms.ToolStripButton();
            this.btnInsertNewRowData = new System.Windows.Forms.ToolStripButton();
            this.btnDuplicateCurrentRowData = new System.Windows.Forms.ToolStripButton();
            this.btnDeleteCurrentRowData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.btnSqlPreviewData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.btnApplyEditData = new System.Windows.Forms.ToolStripButton();
            this.btnCancelEditData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCommitData = new System.Windows.Forms.ToolStripButton();
            this.btnRollbackData = new System.Windows.Forms.ToolStripButton();
            this.spRefresh = new System.Windows.Forms.ToolStripSeparator();
            this.btnSelectAllData = new System.Windows.Forms.ToolStripButton();
            this.btnCopyData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.btnExportToFileData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.btnTopData = new System.Windows.Forms.ToolStripButton();
            this.btnPreviousData = new System.Windows.Forms.ToolStripButton();
            this.btnNextData = new System.Windows.Forms.ToolStripButton();
            this.btnLastData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.lblFindData = new System.Windows.Forms.ToolStripLabel();
            this.lblFind3 = new System.Windows.Forms.ToolStripLabel();
            this.btnFindNextData = new System.Windows.Forms.ToolStripButton();
            this.btnFindPreviousData = new System.Windows.Forms.ToolStripButton();
            this.btnCountData = new System.Windows.Forms.ToolStripButton();
            this.btnHighlightData = new System.Windows.Forms.ToolStripButton();
            this.btnClearHighlightData = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.tabSettings = new C1.Win.C1Command.C1DockingTabPage();
            this.grpEditedColors = new System.Windows.Forms.GroupBox();
            this.txtTemp = new System.Windows.Forms.TextBox();
            this.pnlChangedCellBackColor = new System.Windows.Forms.Panel();
            this.lblChangedCellBackColor = new System.Windows.Forms.Label();
            this.pnlChangedCellForeColor = new System.Windows.Forms.Panel();
            this.lblChangedCellForeColor = new System.Windows.Forms.Label();
            this.pnlDeletedRowBackColor = new System.Windows.Forms.Panel();
            this.lblDeletedRowBackColor = new System.Windows.Forms.Label();
            this.pnlDeletedRowForeColor = new System.Windows.Forms.Panel();
            this.lblDeletedRowForeColor = new System.Windows.Forms.Label();
            this.pnlNewRowBackColor = new System.Windows.Forms.Panel();
            this.lblNewRowBackColor = new System.Windows.Forms.Label();
            this.pnlNewRowForeColor = new System.Windows.Forms.Panel();
            this.lblNewRowForeColor = new System.Windows.Forms.Label();
            this.tabSqlPreview = new C1.Win.C1Command.C1DockingTabPage();
            this.editorSqlPreview = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.tsSqlPreview = new System.Windows.Forms.ToolStrip();
            this.btnSelectAllSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnCopySqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAsSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.btnWordWrapSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2SqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharactersSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2SqlPreview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.btnApplyEditSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnCancelEditSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.btnCommitSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnRollbackSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.btnZoomInSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.btnZoomOutSqlPreview = new System.Windows.Forms.ToolStripButton();
            this.lblLevel = new System.Windows.Forms.Label();
            this.lblSchemaName2 = new System.Windows.Forms.Label();
            this.lblSchemaType2 = new System.Windows.Forms.Label();
            this.tmrMouseDoubleClick = new System.Windows.Forms.Timer(this.components);
            this.tmrMother2Child = new System.Windows.Forms.Timer(this.components);
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.c1SuperTooltip1 = new C1.Win.C1SuperTooltip.C1SuperTooltip(this.components);
            this.c1CommandHolder1 = new C1.Win.C1Command.C1CommandHolder();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboSchema)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSchemaBrowser)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSchemaFilter)).BeginInit();
            this.tsSchemaBrowser.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabSchemaBrowser)).BeginInit();
            this.tabSchemaBrowser.SuspendLayout();
            this.tabSqlPane.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).BeginInit();
            this.tsSqlPane.SuspendLayout();
            this.tabTableStructure.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridStructure)).BeginInit();
            this.tsTableStructure.SuspendLayout();
            this.tabView100RowsTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid100RowsTop)).BeginInit();
            this.tsView100RowsTop.SuspendLayout();
            this.tabData.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboFind)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRowData)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridColumns)).BeginInit();
            this.tsColumnFilter.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridData)).BeginInit();
            this.tsData.SuspendLayout();
            this.tabSettings.SuspendLayout();
            this.grpEditedColors.SuspendLayout();
            this.tabSqlPreview.SuspendLayout();
            this.tsSqlPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).BeginInit();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            this.splitContainer1.Panel1MinSize = 200;
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabSchemaBrowser);
            this.splitContainer1.Panel2.Controls.Add(this.lblLevel);
            this.splitContainer1.Panel2.Controls.Add(this.lblSchemaName2);
            this.splitContainer1.Panel2.Controls.Add(this.lblSchemaType2);
            this.splitContainer1.Panel2MinSize = 432;
            this.splitContainer1.Size = new System.Drawing.Size(1436, 708);
            this.splitContainer1.SplitterDistance = 280;
            this.splitContainer1.SplitterWidth = 3;
            this.splitContainer1.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.splitContainer1, "(default)");
            this.splitContainer1.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer1_SplitterMoving);
            this.splitContainer1.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer1_SplitterMoved);
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
            this.splitContainer2.Panel1.Controls.Add(this.lblSchema);
            this.splitContainer2.Panel1.Controls.Add(this.cboSchema);
            this.c1ThemeController1.SetTheme(this.splitContainer2.Panel1, "(default)");
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.c1GridSchemaBrowser);
            this.splitContainer2.Panel2.Controls.Add(this.lblFilter);
            this.splitContainer2.Panel2.Controls.Add(this.txtSchemaFilter);
            this.splitContainer2.Panel2.Controls.Add(this.lblSchemaType0);
            this.splitContainer2.Panel2.Controls.Add(this.tsSchemaBrowser);
            this.c1ThemeController1.SetTheme(this.splitContainer2.Panel2, "(default)");
            this.splitContainer2.Size = new System.Drawing.Size(280, 708);
            this.splitContainer2.SplitterDistance = 28;
            this.splitContainer2.TabIndex = 92;
            this.c1ThemeController1.SetTheme(this.splitContainer2, "(default)");
            // 
            // lblSchema
            // 
            this.lblSchema.AutoSize = true;
            this.lblSchema.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblSchema.Location = new System.Drawing.Point(15, 10);
            this.lblSchema.Name = "lblSchema";
            this.lblSchema.Size = new System.Drawing.Size(55, 15);
            this.lblSchema.TabIndex = 118;
            this.lblSchema.Text = "Schema:";
            this.c1ThemeController1.SetTheme(this.lblSchema, "(default)");
            // 
            // cboSchema
            // 
            this.cboSchema.AllowSpinLoop = false;
            this.cboSchema.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboSchema.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboSchema.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboSchema.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboSchema.GapHeight = 0;
            this.cboSchema.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboSchema.ItemsDisplayMember = "";
            this.cboSchema.ItemsValueMember = "";
            this.cboSchema.Location = new System.Drawing.Point(72, 8);
            this.cboSchema.Name = "cboSchema";
            this.cboSchema.Size = new System.Drawing.Size(106, 21);
            this.cboSchema.TabIndex = 117;
            this.cboSchema.Tag = null;
            this.cboSchema.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboSchema, "(default)");
            this.cboSchema.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // c1GridSchemaBrowser
            // 
            this.c1GridSchemaBrowser.AllowUpdate = false;
            this.c1GridSchemaBrowser.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridSchemaBrowser.ColumnHeaders = false;
            this.c1GridSchemaBrowser.DataView = C1.Win.C1TrueDBGrid.DataViewEnum.GroupBy;
            this.c1GridSchemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1GridSchemaBrowser.FetchRowStyles = true;
            this.c1GridSchemaBrowser.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridSchemaBrowser.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridSchemaBrowser.GroupByAreaVisible = false;
            this.c1GridSchemaBrowser.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridSchemaBrowser.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridSchemaBrowser.Images"))));
            this.c1GridSchemaBrowser.Location = new System.Drawing.Point(0, 26);
            this.c1GridSchemaBrowser.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridSchemaBrowser.Name = "c1GridSchemaBrowser";
            this.c1GridSchemaBrowser.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridSchemaBrowser.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridSchemaBrowser.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridSchemaBrowser.PreviewInfo.ZoomFactor = 75D;
            this.c1GridSchemaBrowser.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridSchemaBrowser.PrintInfo.MeasurementPrinterName = null;
            this.c1GridSchemaBrowser.RowHeight = 19;
            this.c1GridSchemaBrowser.Size = new System.Drawing.Size(280, 650);
            this.c1GridSchemaBrowser.TabIndex = 5;
            this.c1GridSchemaBrowser.UseCompatibleTextRendering = false;
            this.c1GridSchemaBrowser.ColResize += new C1.Win.C1TrueDBGrid.ColResizeEventHandler(this.c1GridSchemaBrowser_ColResize);
            this.c1GridSchemaBrowser.FetchRowStyle += new C1.Win.C1TrueDBGrid.FetchRowStyleEventHandler(this.c1GridSchemaBrowser_FetchRowStyle);
            this.c1GridSchemaBrowser.Expand += new C1.Win.C1TrueDBGrid.BandEventHandler(this.c1GridSchemaBrowser_Expand);
            this.c1GridSchemaBrowser.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1GridSchemaBrowser_KeyDown);
            this.c1GridSchemaBrowser.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseDoubleClick);
            this.c1GridSchemaBrowser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseDown);
            this.c1GridSchemaBrowser.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridSchemaBrowser_MouseUp);
            this.c1GridSchemaBrowser.PropBag = resources.GetString("c1GridSchemaBrowser.PropBag");
            // 
            // lblFilter
            // 
            this.lblFilter.AutoSize = true;
            this.lblFilter.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblFilter.Location = new System.Drawing.Point(71, 4);
            this.lblFilter.Name = "lblFilter";
            this.lblFilter.Size = new System.Drawing.Size(37, 15);
            this.lblFilter.TabIndex = 91;
            this.lblFilter.Text = "Filter:";
            this.lblFilter.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtSchemaFilter
            // 
            this.txtSchemaFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtSchemaFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtSchemaFilter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtSchemaFilter.Location = new System.Drawing.Point(150, 2);
            this.txtSchemaFilter.Name = "txtSchemaFilter";
            this.txtSchemaFilter.Size = new System.Drawing.Size(121, 21);
            this.txtSchemaFilter.TabIndex = 90;
            this.txtSchemaFilter.Tag = null;
            this.txtSchemaFilter.Text = "*";
            this.txtSchemaFilter.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.txtSchemaFilter, "(default)");
            this.txtSchemaFilter.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtSchemaFilter.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtSchemaFilter_KeyDown);
            // 
            // lblSchemaType0
            // 
            this.lblSchemaType0.AutoSize = true;
            this.lblSchemaType0.Location = new System.Drawing.Point(68, 5);
            this.lblSchemaType0.Name = "lblSchemaType0";
            this.lblSchemaType0.Size = new System.Drawing.Size(0, 15);
            this.lblSchemaType0.TabIndex = 43;
            // 
            // tsSchemaBrowser
            // 
            this.tsSchemaBrowser.AutoSize = false;
            this.tsSchemaBrowser.BackColor = System.Drawing.Color.Transparent;
            this.tsSchemaBrowser.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSchemaBrowser.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnRefresh,
            this.tsSeparator1,
            this.btnExpandCollapse,
            this.toolStripSeparator1,
            this.toolStripLabel4,
            this.toolStripLabel5});
            this.tsSchemaBrowser.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.tsSchemaBrowser.Location = new System.Drawing.Point(0, 0);
            this.tsSchemaBrowser.Name = "tsSchemaBrowser";
            this.tsSchemaBrowser.Size = new System.Drawing.Size(280, 26);
            this.tsSchemaBrowser.TabIndex = 2;
            this.tsSchemaBrowser.Text = "toolStrip2";
            // 
            // btnRefresh
            // 
            this.btnRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefresh.Image = ((System.Drawing.Image)(resources.GetObject("btnRefresh.Image")));
            this.btnRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefresh.Name = "btnRefresh";
            this.btnRefresh.Size = new System.Drawing.Size(23, 20);
            this.btnRefresh.ToolTipText = "Refresh";
            this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);
            // 
            // tsSeparator1
            // 
            this.tsSeparator1.Name = "tsSeparator1";
            this.tsSeparator1.Size = new System.Drawing.Size(6, 23);
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
            this.btnExpandCollapse.Size = new System.Drawing.Size(32, 20);
            // 
            // mnuExpandAll
            // 
            this.mnuExpandAll.Name = "mnuExpandAll";
            this.mnuExpandAll.Size = new System.Drawing.Size(140, 22);
            this.mnuExpandAll.Tag = "ExpandAll";
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
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 23);
            // 
            // toolStripLabel4
            // 
            this.toolStripLabel4.Font = new System.Drawing.Font("微軟正黑體", 3F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripLabel4.Name = "toolStripLabel4";
            this.toolStripLabel4.Size = new System.Drawing.Size(4, 5);
            this.toolStripLabel4.Text = " ";
            // 
            // toolStripLabel5
            // 
            this.toolStripLabel5.Font = new System.Drawing.Font("微軟正黑體", 3F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripLabel5.Name = "toolStripLabel5";
            this.toolStripLabel5.Size = new System.Drawing.Size(4, 5);
            this.toolStripLabel5.Text = " ";
            // 
            // tabSchemaBrowser
            // 
            this.tabSchemaBrowser.Controls.Add(this.tabSqlPane);
            this.tabSchemaBrowser.Controls.Add(this.tabTableStructure);
            this.tabSchemaBrowser.Controls.Add(this.tabView100RowsTop);
            this.tabSchemaBrowser.Controls.Add(this.tabData);
            this.tabSchemaBrowser.Controls.Add(this.tabSettings);
            this.tabSchemaBrowser.Controls.Add(this.tabSqlPreview);
            this.tabSchemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabSchemaBrowser.Location = new System.Drawing.Point(0, 0);
            this.tabSchemaBrowser.Name = "tabSchemaBrowser";
            this.tabSchemaBrowser.Size = new System.Drawing.Size(1153, 708);
            this.tabSchemaBrowser.TabIndex = 41;
            this.tabSchemaBrowser.TabsSpacing = 5;
            this.c1ThemeController1.SetTheme(this.tabSchemaBrowser, "(default)");
            this.tabSchemaBrowser.VisualStyle = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.tabSchemaBrowser.VisualStyleBase = C1.Win.C1Command.VisualStyle.Office2010Blue;
            this.tabSchemaBrowser.TabClick += new System.EventHandler(this.tabSchemaBrowser_TabClick);
            // 
            // tabSqlPane
            // 
            this.tabSqlPane.Controls.Add(this.editorSqlPane);
            this.tabSqlPane.Controls.Add(this.chkCopyAsHTML);
            this.tabSqlPane.Controls.Add(this.tsSqlPane);
            this.tabSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("tabSqlPane.Image")));
            this.tabSqlPane.Location = new System.Drawing.Point(1, 27);
            this.tabSqlPane.Name = "tabSqlPane";
            this.tabSqlPane.Size = new System.Drawing.Size(1151, 680);
            this.tabSqlPane.TabIndex = 0;
            this.tabSqlPane.Tag = "SQL Pane";
            this.tabSqlPane.Text = "SQL Pane";
            // 
            // editorSqlPane
            // 
            this.editorSqlPane.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSqlPane.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlPane.CaretLineVisible = true;
            this.editorSqlPane.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlPane.Location = new System.Drawing.Point(3, 23);
            this.editorSqlPane.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlPane.Name = "editorSqlPane";
            this.editorSqlPane.ReadOnly = true;
            this.editorSqlPane.Size = new System.Drawing.Size(1145, 1044);
            this.editorSqlPane.Styler = null;
            this.editorSqlPane.TabIndex = 1;
            this.editorSqlPane.WhitespaceSize = 3;
            this.editorSqlPane.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlPane.WrapMode = ScintillaNET.WrapMode.Word;
            this.editorSqlPane.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editorSqlPane_DoubleClick);
            this.editorSqlPane.Enter += new System.EventHandler(this.editorSqlPane_Enter);
            this.editorSqlPane.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editorSqlPane_KeyDown);
            this.editorSqlPane.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editorSqlPane_KeyPress);
            this.editorSqlPane.KeyUp += new System.Windows.Forms.KeyEventHandler(this.editorSqlPane_KeyUp);
            this.editorSqlPane.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorSqlPane_MouseDown);
            this.editorSqlPane.MouseUp += new System.Windows.Forms.MouseEventHandler(this.editorSqlPane_MouseUp);
            // 
            // chkCopyAsHTML
            // 
            this.chkCopyAsHTML.AutoSize = true;
            this.chkCopyAsHTML.BackColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderColor = System.Drawing.Color.Transparent;
            this.chkCopyAsHTML.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkCopyAsHTML.ForeColor = System.Drawing.Color.Black;
            this.chkCopyAsHTML.Location = new System.Drawing.Point(236, 2);
            this.chkCopyAsHTML.Name = "chkCopyAsHTML";
            this.chkCopyAsHTML.Padding = new System.Windows.Forms.Padding(1);
            this.chkCopyAsHTML.Size = new System.Drawing.Size(197, 21);
            this.chkCopyAsHTML.TabIndex = 40;
            this.chkCopyAsHTML.Text = "Copy SQL Statement as HTML";
            this.c1ThemeController1.SetTheme(this.chkCopyAsHTML, "(default)");
            this.chkCopyAsHTML.UseVisualStyleBackColor = true;
            this.chkCopyAsHTML.Value = null;
            this.chkCopyAsHTML.Visible = false;
            this.chkCopyAsHTML.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // tsSqlPane
            // 
            this.tsSqlPane.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSqlPane.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAllSqlPane,
            this.btnCopySqlPane,
            this.btnSaveAsSqlPane,
            this.btnWordWrapSqlPane,
            this.btnWordWrap2SqlPane,
            this.btnShowAllCharactersSqlPane,
            this.btnShowAllCharacters2SqlPane,
            this.tsSeparator4,
            this.toolStripLabel2,
            this.lblInfo,
            this.btnZoomInSqlPane,
            this.btnZoomOutSqlPane});
            this.tsSqlPane.Location = new System.Drawing.Point(0, 0);
            this.tsSqlPane.Name = "tsSqlPane";
            this.tsSqlPane.Size = new System.Drawing.Size(1151, 25);
            this.tsSqlPane.TabIndex = 0;
            this.tsSqlPane.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.tsSqlPane, "(default)");
            // 
            // btnSelectAllSqlPane
            // 
            this.btnSelectAllSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAllSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAllSqlPane.Image")));
            this.btnSelectAllSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAllSqlPane.Name = "btnSelectAllSqlPane";
            this.btnSelectAllSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAllSqlPane.Click += new System.EventHandler(this.btnSelectAllSqlPane_Click);
            // 
            // btnCopySqlPane
            // 
            this.btnCopySqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopySqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnCopySqlPane.Image")));
            this.btnCopySqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopySqlPane.Name = "btnCopySqlPane";
            this.btnCopySqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnCopySqlPane.Click += new System.EventHandler(this.btnCopySqlPane_Click);
            // 
            // btnSaveAsSqlPane
            // 
            this.btnSaveAsSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAsSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAsSqlPane.Image")));
            this.btnSaveAsSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAsSqlPane.Name = "btnSaveAsSqlPane";
            this.btnSaveAsSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnSaveAsSqlPane.Click += new System.EventHandler(this.btnSaveAsSqlPane_Click);
            // 
            // btnWordWrapSqlPane
            // 
            this.btnWordWrapSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrapSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrapSqlPane.Image")));
            this.btnWordWrapSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrapSqlPane.Name = "btnWordWrapSqlPane";
            this.btnWordWrapSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrapSqlPane.Text = "Word Wrap";
            this.btnWordWrapSqlPane.Click += new System.EventHandler(this.btnWordWrapSqlPane_Click);
            // 
            // btnWordWrap2SqlPane
            // 
            this.btnWordWrap2SqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2SqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2SqlPane.Image")));
            this.btnWordWrap2SqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2SqlPane.Name = "btnWordWrap2SqlPane";
            this.btnWordWrap2SqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap2SqlPane.Text = "Word Wrap";
            this.btnWordWrap2SqlPane.Visible = false;
            this.btnWordWrap2SqlPane.Click += new System.EventHandler(this.btnWordWrapSqlPane_Click);
            // 
            // btnShowAllCharactersSqlPane
            // 
            this.btnShowAllCharactersSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharactersSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharactersSqlPane.Image")));
            this.btnShowAllCharactersSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharactersSqlPane.Name = "btnShowAllCharactersSqlPane";
            this.btnShowAllCharactersSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharactersSqlPane.Click += new System.EventHandler(this.btnShowAllCharactersSqlPane_Click);
            // 
            // btnShowAllCharacters2SqlPane
            // 
            this.btnShowAllCharacters2SqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2SqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2SqlPane.Image")));
            this.btnShowAllCharacters2SqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2SqlPane.Name = "btnShowAllCharacters2SqlPane";
            this.btnShowAllCharacters2SqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters2SqlPane.Visible = false;
            this.btnShowAllCharacters2SqlPane.Click += new System.EventHandler(this.btnShowAllCharactersSqlPane_Click);
            // 
            // tsSeparator4
            // 
            this.tsSeparator4.Name = "tsSeparator4";
            this.tsSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Font = new System.Drawing.Font("微軟正黑體", 3F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(4, 22);
            this.toolStripLabel2.Text = " ";
            // 
            // lblInfo
            // 
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(43, 22);
            this.lblInfo.Text = "lblInfo";
            this.lblInfo.Visible = false;
            // 
            // btnZoomInSqlPane
            // 
            this.btnZoomInSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomInSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomInSqlPane.Image")));
            this.btnZoomInSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomInSqlPane.Name = "btnZoomInSqlPane";
            this.btnZoomInSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnZoomInSqlPane.Click += new System.EventHandler(this.btnZoomInSqlPane_Click);
            // 
            // btnZoomOutSqlPane
            // 
            this.btnZoomOutSqlPane.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOutSqlPane.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomOutSqlPane.Image")));
            this.btnZoomOutSqlPane.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOutSqlPane.Name = "btnZoomOutSqlPane";
            this.btnZoomOutSqlPane.Size = new System.Drawing.Size(23, 22);
            this.btnZoomOutSqlPane.Click += new System.EventHandler(this.btnZoomOutSqlPane_Click);
            // 
            // tabTableStructure
            // 
            this.tabTableStructure.Controls.Add(this.c1GridStructure);
            this.tabTableStructure.Controls.Add(this.tsTableStructure);
            this.tabTableStructure.Image = ((System.Drawing.Image)(resources.GetObject("tabTableStructure.Image")));
            this.tabTableStructure.Location = new System.Drawing.Point(1, 27);
            this.tabTableStructure.Name = "tabTableStructure";
            this.tabTableStructure.Size = new System.Drawing.Size(1151, 680);
            this.tabTableStructure.TabIndex = 1;
            this.tabTableStructure.TabVisible = false;
            this.tabTableStructure.Tag = "Table Structure";
            this.tabTableStructure.Text = "Table Structure";
            // 
            // c1GridStructure
            // 
            this.c1GridStructure.AllowUpdate = false;
            this.c1GridStructure.AllowUpdateOnBlur = false;
            this.c1GridStructure.AlternatingRows = true;
            this.c1GridStructure.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridStructure.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridStructure.FetchRowStyles = true;
            this.c1GridStructure.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridStructure.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridStructure.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridStructure.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridStructure.Images"))));
            this.c1GridStructure.Location = new System.Drawing.Point(2, 24);
            this.c1GridStructure.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridStructure.Name = "c1GridStructure";
            this.c1GridStructure.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridStructure.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridStructure.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridStructure.PreviewInfo.ZoomFactor = 75D;
            this.c1GridStructure.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridStructure.PrintInfo.MeasurementPrinterName = null;
            this.c1GridStructure.RowHeight = 19;
            this.c1GridStructure.Size = new System.Drawing.Size(1146, 1044);
            this.c1GridStructure.TabIndex = 66;
            this.c1ThemeController1.SetTheme(this.c1GridStructure, "(default)");
            this.c1GridStructure.UseCompatibleTextRendering = false;
            this.c1GridStructure.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridStructure_FetchCellStyle);
            this.c1GridStructure.FetchRowStyle += new C1.Win.C1TrueDBGrid.FetchRowStyleEventHandler(this.c1GridStructure_FetchRowStyle);
            this.c1GridStructure.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1Grid_KeyDown);
            this.c1GridStructure.MouseClick += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseClick);
            this.c1GridStructure.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseDown);
            this.c1GridStructure.PropBag = resources.GetString("c1GridStructure.PropBag");
            // 
            // tsTableStructure
            // 
            this.tsTableStructure.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsTableStructure.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblTableOrViewName,
            this.lblTableName01,
            this.toolStripSeparator3,
            this.btnSelectAllTableStructure,
            this.btnCopyTableStructure,
            this.toolStripSeparator2,
            this.btnExportToFileTableStructure});
            this.tsTableStructure.Location = new System.Drawing.Point(0, 0);
            this.tsTableStructure.Name = "tsTableStructure";
            this.tsTableStructure.Size = new System.Drawing.Size(1151, 25);
            this.tsTableStructure.TabIndex = 67;
            this.tsTableStructure.Text = "toolStrip3";
            this.c1ThemeController1.SetTheme(this.tsTableStructure, "(default)");
            // 
            // lblTableOrViewName
            // 
            this.lblTableOrViewName.Name = "lblTableOrViewName";
            this.lblTableOrViewName.Size = new System.Drawing.Size(45, 22);
            this.lblTableOrViewName.Text = "Name:";
            // 
            // lblTableName01
            // 
            this.lblTableName01.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTableName01.ForeColor = System.Drawing.Color.Blue;
            this.lblTableName01.Name = "lblTableName01";
            this.lblTableName01.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSelectAllTableStructure
            // 
            this.btnSelectAllTableStructure.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAllTableStructure.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAllTableStructure.Image")));
            this.btnSelectAllTableStructure.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAllTableStructure.Name = "btnSelectAllTableStructure";
            this.btnSelectAllTableStructure.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAllTableStructure.Click += new System.EventHandler(this.btnGridSelectAll_Click);
            // 
            // btnCopyTableStructure
            // 
            this.btnCopyTableStructure.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopyTableStructure.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyTableStructure.Image")));
            this.btnCopyTableStructure.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopyTableStructure.Name = "btnCopyTableStructure";
            this.btnCopyTableStructure.Size = new System.Drawing.Size(23, 22);
            this.btnCopyTableStructure.Click += new System.EventHandler(this.btnGridCopy_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnExportToFileTableStructure
            // 
            this.btnExportToFileTableStructure.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExportToFileTableStructure.Image = ((System.Drawing.Image)(resources.GetObject("btnExportToFileTableStructure.Image")));
            this.btnExportToFileTableStructure.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportToFileTableStructure.Name = "btnExportToFileTableStructure";
            this.btnExportToFileTableStructure.Size = new System.Drawing.Size(23, 22);
            this.btnExportToFileTableStructure.Click += new System.EventHandler(this.btnGridExportToFile_Click);
            // 
            // tabView100RowsTop
            // 
            this.tabView100RowsTop.Controls.Add(this.c1Grid100RowsTop);
            this.tabView100RowsTop.Controls.Add(this.tsView100RowsTop);
            this.tabView100RowsTop.Image = ((System.Drawing.Image)(resources.GetObject("tabView100RowsTop.Image")));
            this.tabView100RowsTop.Location = new System.Drawing.Point(1, 27);
            this.tabView100RowsTop.Name = "tabView100RowsTop";
            this.tabView100RowsTop.Size = new System.Drawing.Size(1151, 680);
            this.tabView100RowsTop.TabIndex = 2;
            this.tabView100RowsTop.TabVisible = false;
            this.tabView100RowsTop.Tag = "View Top 100 Rows";
            this.tabView100RowsTop.Text = "View Top 100 Rows";
            // 
            // c1Grid100RowsTop
            // 
            this.c1Grid100RowsTop.AllowUpdate = false;
            this.c1Grid100RowsTop.AllowUpdateOnBlur = false;
            this.c1Grid100RowsTop.AlternatingRows = true;
            this.c1Grid100RowsTop.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1Grid100RowsTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1Grid100RowsTop.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1Grid100RowsTop.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1Grid100RowsTop.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1Grid100RowsTop.Images.Add(((System.Drawing.Image)(resources.GetObject("c1Grid100RowsTop.Images"))));
            this.c1Grid100RowsTop.Location = new System.Drawing.Point(2, 24);
            this.c1Grid100RowsTop.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1Grid100RowsTop.Name = "c1Grid100RowsTop";
            this.c1Grid100RowsTop.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1Grid100RowsTop.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1Grid100RowsTop.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1Grid100RowsTop.PreviewInfo.ZoomFactor = 75D;
            this.c1Grid100RowsTop.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1Grid100RowsTop.PrintInfo.MeasurementPrinterName = null;
            this.c1Grid100RowsTop.RowHeight = 19;
            this.c1Grid100RowsTop.Size = new System.Drawing.Size(1146, 874);
            this.c1Grid100RowsTop.TabIndex = 69;
            this.c1ThemeController1.SetTheme(this.c1Grid100RowsTop, "(default)");
            this.c1Grid100RowsTop.UseCompatibleTextRendering = false;
            this.c1Grid100RowsTop.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1Grid_KeyDown);
            this.c1Grid100RowsTop.MouseClick += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseClick);
            this.c1Grid100RowsTop.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1Grid100RowsTop_MouseDoubleClick);
            this.c1Grid100RowsTop.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseDown);
            this.c1Grid100RowsTop.PropBag = resources.GetString("c1Grid100RowsTop.PropBag");
            // 
            // tsView100RowsTop
            // 
            this.tsView100RowsTop.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsView100RowsTop.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblViewName,
            this.lblTableName02,
            this.toolStripSeparator4,
            this.btnSelectAllTop100,
            this.btnCopyTop100,
            this.toolStripSeparator6,
            this.btnExportToFileTop100});
            this.tsView100RowsTop.Location = new System.Drawing.Point(0, 0);
            this.tsView100RowsTop.Name = "tsView100RowsTop";
            this.tsView100RowsTop.Size = new System.Drawing.Size(1151, 25);
            this.tsView100RowsTop.TabIndex = 68;
            this.tsView100RowsTop.Text = "toolStrip4";
            this.c1ThemeController1.SetTheme(this.tsView100RowsTop, "(default)");
            // 
            // lblViewName
            // 
            this.lblViewName.Name = "lblViewName";
            this.lblViewName.Size = new System.Drawing.Size(75, 22);
            this.lblViewName.Text = "View Name:";
            // 
            // lblTableName02
            // 
            this.lblTableName02.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTableName02.ForeColor = System.Drawing.Color.Blue;
            this.lblTableName02.Name = "lblTableName02";
            this.lblTableName02.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSelectAllTop100
            // 
            this.btnSelectAllTop100.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAllTop100.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAllTop100.Image")));
            this.btnSelectAllTop100.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAllTop100.Name = "btnSelectAllTop100";
            this.btnSelectAllTop100.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAllTop100.Click += new System.EventHandler(this.btnGridSelectAll_Click);
            // 
            // btnCopyTop100
            // 
            this.btnCopyTop100.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopyTop100.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyTop100.Image")));
            this.btnCopyTop100.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopyTop100.Name = "btnCopyTop100";
            this.btnCopyTop100.Size = new System.Drawing.Size(23, 22);
            this.btnCopyTop100.Click += new System.EventHandler(this.btnGridCopy_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // btnExportToFileTop100
            // 
            this.btnExportToFileTop100.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExportToFileTop100.Image = ((System.Drawing.Image)(resources.GetObject("btnExportToFileTop100.Image")));
            this.btnExportToFileTop100.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportToFileTop100.Name = "btnExportToFileTop100";
            this.btnExportToFileTop100.Size = new System.Drawing.Size(23, 22);
            this.btnExportToFileTop100.Click += new System.EventHandler(this.btnGridExportToFile_Click);
            // 
            // tabData
            // 
            this.tabData.Controls.Add(this.lblFind2);
            this.tabData.Controls.Add(this.cboFind);
            this.tabData.Controls.Add(this.chkShowFilterRowData);
            this.tabData.Controls.Add(this.splitContainer3);
            this.tabData.Controls.Add(this.tsData);
            this.tabData.Image = ((System.Drawing.Image)(resources.GetObject("tabData.Image")));
            this.tabData.Location = new System.Drawing.Point(1, 27);
            this.tabData.Name = "tabData";
            this.tabData.Size = new System.Drawing.Size(1151, 680);
            this.tabData.TabIndex = 3;
            this.tabData.TabVisible = false;
            this.tabData.Tag = "Data";
            this.tabData.Text = "Data";
            this.tabData.Enter += new System.EventHandler(this.tabData_Enter);
            // 
            // lblFind2
            // 
            this.lblFind2.AutoSize = true;
            this.lblFind2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblFind2.Location = new System.Drawing.Point(622, 15);
            this.lblFind2.Name = "lblFind2";
            this.lblFind2.Size = new System.Drawing.Size(13, 15);
            this.lblFind2.TabIndex = 131;
            this.lblFind2.Text = "F";
            this.lblFind2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblFind2, "(default)");
            this.lblFind2.Visible = false;
            // 
            // cboFind
            // 
            this.cboFind.AllowSpinLoop = false;
            this.cboFind.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboFind.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboFind.Enabled = false;
            this.cboFind.GapHeight = 0;
            this.cboFind.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboFind.ItemsDisplayMember = "";
            this.cboFind.ItemsValueMember = "";
            this.cboFind.Location = new System.Drawing.Point(658, 2);
            this.cboFind.Name = "cboFind";
            this.cboFind.Size = new System.Drawing.Size(110, 21);
            this.cboFind.TabIndex = 132;
            this.cboFind.Tag = null;
            this.cboFind.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboFind, "(default)");
            this.cboFind.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboFind.SelectedItemChanged += new System.EventHandler(this.cboFind_SelectedIndexChanged);
            this.cboFind.BeforeDropDownOpen += new System.ComponentModel.CancelEventHandler(this.cboFind_BeforeDropDownOpen);
            this.cboFind.TextChanged += new System.EventHandler(this.cboFind_TextChanged);
            this.cboFind.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboFind_KeyPress);
            this.cboFind.KeyUp += new System.Windows.Forms.KeyEventHandler(this.cboFind_KeyUp);
            // 
            // chkShowFilterRowData
            // 
            this.chkShowFilterRowData.AutoSize = true;
            this.chkShowFilterRowData.BackColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRowData.BorderColor = System.Drawing.Color.Transparent;
            this.chkShowFilterRowData.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkShowFilterRowData.Enabled = false;
            this.chkShowFilterRowData.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.chkShowFilterRowData.ForeColor = System.Drawing.Color.Black;
            this.chkShowFilterRowData.Location = new System.Drawing.Point(955, 2);
            this.chkShowFilterRowData.Name = "chkShowFilterRowData";
            this.chkShowFilterRowData.Padding = new System.Windows.Forms.Padding(1);
            this.chkShowFilterRowData.Size = new System.Drawing.Size(117, 21);
            this.chkShowFilterRowData.TabIndex = 82;
            this.chkShowFilterRowData.Text = "Show Filter Row";
            this.c1ThemeController1.SetTheme(this.chkShowFilterRowData, "(default)");
            this.chkShowFilterRowData.UseVisualStyleBackColor = true;
            this.chkShowFilterRowData.Value = null;
            this.chkShowFilterRowData.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkShowFilterRowData.CheckedChanged += new System.EventHandler(this.chkShowFilterRow_CheckedChanged);
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 25);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.lblColumnNamePosition);
            this.splitContainer3.Panel1.Controls.Add(this.btnHelp_ColumnName);
            this.splitContainer3.Panel1.Controls.Add(this.lblPosition2);
            this.splitContainer3.Panel1.Controls.Add(this.txtColumnFilter);
            this.splitContainer3.Panel1.Controls.Add(this.c1GridColumns);
            this.splitContainer3.Panel1.Controls.Add(this.tsColumnFilter);
            this.c1ThemeController1.SetTheme(this.splitContainer3.Panel1, "(default)");
            this.splitContainer3.Panel1MinSize = 150;
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.c1GridData);
            this.c1ThemeController1.SetTheme(this.splitContainer3.Panel2, "(default)");
            this.splitContainer3.Panel2MinSize = 500;
            this.splitContainer3.Size = new System.Drawing.Size(1151, 655);
            this.splitContainer3.SplitterDistance = 200;
            this.splitContainer3.SplitterWidth = 2;
            this.splitContainer3.TabIndex = 83;
            this.c1ThemeController1.SetTheme(this.splitContainer3, "(default)");
            this.splitContainer3.SplitterMoving += new System.Windows.Forms.SplitterCancelEventHandler(this.splitContainer3_SplitterMoving);
            this.splitContainer3.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitContainer3_SplitterMoved);
            // 
            // lblColumnNamePosition
            // 
            this.lblColumnNamePosition.AutoSize = true;
            this.lblColumnNamePosition.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblColumnNamePosition.Location = new System.Drawing.Point(18, 42);
            this.lblColumnNamePosition.Name = "lblColumnNamePosition";
            this.lblColumnNamePosition.Size = new System.Drawing.Size(13, 15);
            this.lblColumnNamePosition.TabIndex = 132;
            this.lblColumnNamePosition.Text = "F";
            this.c1ThemeController1.SetTheme(this.lblColumnNamePosition, "(default)");
            this.lblColumnNamePosition.Visible = false;
            // 
            // btnHelp_ColumnName
            // 
            this.btnHelp_ColumnName.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_ColumnName.Image")));
            this.btnHelp_ColumnName.Location = new System.Drawing.Point(71, 25);
            this.btnHelp_ColumnName.Name = "btnHelp_ColumnName";
            this.btnHelp_ColumnName.Size = new System.Drawing.Size(19, 19);
            this.btnHelp_ColumnName.TabIndex = 131;
            this.c1ThemeController1.SetTheme(this.btnHelp_ColumnName, "(default)");
            this.btnHelp_ColumnName.UseVisualStyleBackColor = true;
            this.btnHelp_ColumnName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_ColumnName.Click += new System.EventHandler(this.btnHelp_ColumnName_Click);
            // 
            // lblPosition2
            // 
            this.lblPosition2.AutoSize = true;
            this.lblPosition2.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblPosition2.Location = new System.Drawing.Point(0, 17);
            this.lblPosition2.Name = "lblPosition2";
            this.lblPosition2.Size = new System.Drawing.Size(13, 15);
            this.lblPosition2.TabIndex = 130;
            this.lblPosition2.Text = "F";
            this.c1ThemeController1.SetTheme(this.lblPosition2, "(default)");
            this.lblPosition2.Visible = false;
            // 
            // txtColumnFilter
            // 
            this.txtColumnFilter.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtColumnFilter.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtColumnFilter.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtColumnFilter.Location = new System.Drawing.Point(60, 2);
            this.txtColumnFilter.Name = "txtColumnFilter";
            this.txtColumnFilter.Size = new System.Drawing.Size(121, 21);
            this.txtColumnFilter.TabIndex = 91;
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
            this.c1GridColumns.AllowUpdate = false;
            this.c1GridColumns.AllowUpdateOnBlur = false;
            this.c1GridColumns.AlternatingRows = true;
            this.c1GridColumns.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridColumns.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridColumns.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridColumns.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridColumns.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridColumns.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridColumns.Images"))));
            this.c1GridColumns.Location = new System.Drawing.Point(-1, 25);
            this.c1GridColumns.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridColumns.Name = "c1GridColumns";
            this.c1GridColumns.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridColumns.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridColumns.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridColumns.PreviewInfo.ZoomFactor = 75D;
            this.c1GridColumns.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridColumns.PrintInfo.MeasurementPrinterName = null;
            this.c1GridColumns.RowHeight = 19;
            this.c1GridColumns.Size = new System.Drawing.Size(201, 630);
            this.c1GridColumns.TabIndex = 71;
            this.c1ThemeController1.SetTheme(this.c1GridColumns, "(default)");
            this.c1GridColumns.UseCompatibleTextRendering = false;
            this.c1GridColumns.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridColumns_MouseUp);
            this.c1GridColumns.PropBag = resources.GetString("c1GridColumns.PropBag");
            // 
            // tsColumnFilter
            // 
            this.tsColumnFilter.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsColumnFilter.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblColumnFilterData});
            this.tsColumnFilter.Location = new System.Drawing.Point(0, 0);
            this.tsColumnFilter.Name = "tsColumnFilter";
            this.tsColumnFilter.Size = new System.Drawing.Size(200, 25);
            this.tsColumnFilter.TabIndex = 1;
            this.tsColumnFilter.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.tsColumnFilter, "(default)");
            // 
            // lblColumnFilterData
            // 
            this.lblColumnFilterData.Name = "lblColumnFilterData";
            this.lblColumnFilterData.Size = new System.Drawing.Size(37, 22);
            this.lblColumnFilterData.Text = "Filter:";
            // 
            // c1GridData
            // 
            this.c1GridData.AllowColMove = false;
            this.c1GridData.AllowColSelect = false;
            this.c1GridData.AllowFilter = false;
            this.c1GridData.AllowRowSelect = false;
            this.c1GridData.AllowSort = false;
            this.c1GridData.AlternatingRows = true;
            this.c1GridData.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridData.CellTips = C1.Win.C1TrueDBGrid.CellTipEnum.Anchored;
            this.c1GridData.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1GridData.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridData.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridData.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridData.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridData.Images"))));
            this.c1GridData.Location = new System.Drawing.Point(0, 0);
            this.c1GridData.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.c1GridData.Name = "c1GridData";
            this.c1GridData.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridData.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridData.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridData.PreviewInfo.ZoomFactor = 75D;
            this.c1GridData.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridData.PrintInfo.MeasurementPrinterName = null;
            this.c1GridData.RowHeight = 19;
            this.c1GridData.Size = new System.Drawing.Size(949, 655);
            this.c1GridData.TabIndex = 70;
            this.c1ThemeController1.SetTheme(this.c1GridData, "(default)");
            this.c1GridData.UseCompatibleTextRendering = false;
            this.c1GridData.AfterColUpdate += new C1.Win.C1TrueDBGrid.ColEventHandler(this.c1GridData_AfterColUpdate);
            this.c1GridData.OwnerDrawCell += new C1.Win.C1TrueDBGrid.OwnerDrawCellEventHandler(this.c1GridData_OwnerDrawCell);
            this.c1GridData.RowColChange += new C1.Win.C1TrueDBGrid.RowColChangeEventHandler(this.c1GridData_RowColChange);
            this.c1GridData.BeforeColEdit += new C1.Win.C1TrueDBGrid.BeforeColEditEventHandler(this.c1GridData_BeforeColEdit);
            this.c1GridData.FetchCellTips += new C1.Win.C1TrueDBGrid.FetchCellTipsEventHandler(this.c1GridData_FetchCellTips);
            this.c1GridData.KeyDown += new System.Windows.Forms.KeyEventHandler(this.c1Grid_KeyDown);
            this.c1GridData.KeyUp += new System.Windows.Forms.KeyEventHandler(this.c1GridData_KeyUp);
            this.c1GridData.MouseClick += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseClick);
            this.c1GridData.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.c1GridData_MouseDoubleClick);
            this.c1GridData.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseDown);
            this.c1GridData.MouseMove += new System.Windows.Forms.MouseEventHandler(this.c1GridData_MouseMove);
            this.c1GridData.MouseUp += new System.Windows.Forms.MouseEventHandler(this.c1GridData_MouseUp);
            this.c1GridData.PropBag = resources.GetString("c1GridData.PropBag");
            // 
            // tsData
            // 
            this.tsData.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsData.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnFilterData,
            this.btnFilterRedData,
            this.toolStripSeparator12,
            this.btnRefreshData,
            this.toolStripSeparator11,
            this.btnShowColumnsData,
            this.toolStripSeparator15,
            this.btnEditCellData,
            this.btnEditCellWithEditFormData,
            this.btnSetNullData,
            this.btnInsertNewRowData,
            this.btnDuplicateCurrentRowData,
            this.btnDeleteCurrentRowData,
            this.toolStripSeparator16,
            this.btnSqlPreviewData,
            this.toolStripSeparator20,
            this.btnApplyEditData,
            this.btnCancelEditData,
            this.toolStripSeparator14,
            this.btnCommitData,
            this.btnRollbackData,
            this.spRefresh,
            this.btnSelectAllData,
            this.btnCopyData,
            this.toolStripSeparator8,
            this.btnExportToFileData,
            this.toolStripSeparator17,
            this.btnTopData,
            this.btnPreviousData,
            this.btnNextData,
            this.btnLastData,
            this.toolStripSeparator10,
            this.lblFindData,
            this.lblFind3,
            this.btnFindNextData,
            this.btnFindPreviousData,
            this.btnCountData,
            this.btnHighlightData,
            this.btnClearHighlightData,
            this.toolStripSeparator18});
            this.tsData.Location = new System.Drawing.Point(0, 0);
            this.tsData.Name = "tsData";
            this.tsData.Size = new System.Drawing.Size(1151, 25);
            this.tsData.TabIndex = 69;
            this.tsData.Text = "toolStrip5";
            this.c1ThemeController1.SetTheme(this.tsData, "(default)");
            // 
            // btnFilterData
            // 
            this.btnFilterData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFilterData.Image = ((System.Drawing.Image)(resources.GetObject("btnFilterData.Image")));
            this.btnFilterData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFilterData.Name = "btnFilterData";
            this.btnFilterData.Size = new System.Drawing.Size(23, 22);
            this.btnFilterData.Tag = "1 = 1";
            this.btnFilterData.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // btnFilterRedData
            // 
            this.btnFilterRedData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFilterRedData.Image = ((System.Drawing.Image)(resources.GetObject("btnFilterRedData.Image")));
            this.btnFilterRedData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFilterRedData.Name = "btnFilterRedData";
            this.btnFilterRedData.Size = new System.Drawing.Size(23, 22);
            this.btnFilterRedData.Visible = false;
            this.btnFilterRedData.Click += new System.EventHandler(this.btnFilter_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(6, 25);
            // 
            // btnRefreshData
            // 
            this.btnRefreshData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRefreshData.Image = ((System.Drawing.Image)(resources.GetObject("btnRefreshData.Image")));
            this.btnRefreshData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRefreshData.Name = "btnRefreshData";
            this.btnRefreshData.Size = new System.Drawing.Size(23, 22);
            this.btnRefreshData.Click += new System.EventHandler(this.btnRefreshData_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(6, 25);
            // 
            // btnShowColumnsData
            // 
            this.btnShowColumnsData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowColumnsData.Image = ((System.Drawing.Image)(resources.GetObject("btnShowColumnsData.Image")));
            this.btnShowColumnsData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowColumnsData.Name = "btnShowColumnsData";
            this.btnShowColumnsData.Size = new System.Drawing.Size(23, 22);
            this.btnShowColumnsData.Click += new System.EventHandler(this.btnShowColumns_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(6, 25);
            // 
            // btnEditCellData
            // 
            this.btnEditCellData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditCellData.Image = ((System.Drawing.Image)(resources.GetObject("btnEditCellData.Image")));
            this.btnEditCellData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditCellData.Name = "btnEditCellData";
            this.btnEditCellData.Size = new System.Drawing.Size(23, 22);
            this.btnEditCellData.Click += new System.EventHandler(this.btnEditCell_Click);
            // 
            // btnEditCellWithEditFormData
            // 
            this.btnEditCellWithEditFormData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnEditCellWithEditFormData.Image = ((System.Drawing.Image)(resources.GetObject("btnEditCellWithEditFormData.Image")));
            this.btnEditCellWithEditFormData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnEditCellWithEditFormData.Name = "btnEditCellWithEditFormData";
            this.btnEditCellWithEditFormData.Size = new System.Drawing.Size(23, 22);
            this.btnEditCellWithEditFormData.Click += new System.EventHandler(this.btnEditCellWithEditForm_Click);
            // 
            // btnSetNullData
            // 
            this.btnSetNullData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetNullData.Image = ((System.Drawing.Image)(resources.GetObject("btnSetNullData.Image")));
            this.btnSetNullData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetNullData.Name = "btnSetNullData";
            this.btnSetNullData.Size = new System.Drawing.Size(23, 22);
            this.btnSetNullData.Click += new System.EventHandler(this.btnSetNull_Click);
            // 
            // btnInsertNewRowData
            // 
            this.btnInsertNewRowData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnInsertNewRowData.Image = ((System.Drawing.Image)(resources.GetObject("btnInsertNewRowData.Image")));
            this.btnInsertNewRowData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnInsertNewRowData.Name = "btnInsertNewRowData";
            this.btnInsertNewRowData.Size = new System.Drawing.Size(23, 22);
            this.btnInsertNewRowData.Click += new System.EventHandler(this.btnInsertNewRow_Click);
            // 
            // btnDuplicateCurrentRowData
            // 
            this.btnDuplicateCurrentRowData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDuplicateCurrentRowData.Image = ((System.Drawing.Image)(resources.GetObject("btnDuplicateCurrentRowData.Image")));
            this.btnDuplicateCurrentRowData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDuplicateCurrentRowData.Name = "btnDuplicateCurrentRowData";
            this.btnDuplicateCurrentRowData.Size = new System.Drawing.Size(23, 22);
            this.btnDuplicateCurrentRowData.Click += new System.EventHandler(this.btnDuplicateCurrentRow_Click);
            // 
            // btnDeleteCurrentRowData
            // 
            this.btnDeleteCurrentRowData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnDeleteCurrentRowData.Image = ((System.Drawing.Image)(resources.GetObject("btnDeleteCurrentRowData.Image")));
            this.btnDeleteCurrentRowData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnDeleteCurrentRowData.Name = "btnDeleteCurrentRowData";
            this.btnDeleteCurrentRowData.Size = new System.Drawing.Size(23, 22);
            this.btnDeleteCurrentRowData.Click += new System.EventHandler(this.btnDeleteCurrentRow_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSqlPreviewData
            // 
            this.btnSqlPreviewData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSqlPreviewData.Image = ((System.Drawing.Image)(resources.GetObject("btnSqlPreviewData.Image")));
            this.btnSqlPreviewData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSqlPreviewData.Name = "btnSqlPreviewData";
            this.btnSqlPreviewData.Size = new System.Drawing.Size(23, 22);
            this.btnSqlPreviewData.Click += new System.EventHandler(this.btnSqlPreview_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(6, 25);
            // 
            // btnApplyEditData
            // 
            this.btnApplyEditData.Image = ((System.Drawing.Image)(resources.GetObject("btnApplyEditData.Image")));
            this.btnApplyEditData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnApplyEditData.Name = "btnApplyEditData";
            this.btnApplyEditData.Size = new System.Drawing.Size(60, 22);
            this.btnApplyEditData.Tag = "NA";
            this.btnApplyEditData.Text = "Apply";
            this.btnApplyEditData.Click += new System.EventHandler(this.btnApplyEdit_Click);
            // 
            // btnCancelEditData
            // 
            this.btnCancelEditData.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelEditData.Image")));
            this.btnCancelEditData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancelEditData.Name = "btnCancelEditData";
            this.btnCancelEditData.Size = new System.Drawing.Size(65, 22);
            this.btnCancelEditData.Text = "Cancel";
            this.btnCancelEditData.Click += new System.EventHandler(this.btnCancelEdit_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCommitData
            // 
            this.btnCommitData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCommitData.Enabled = false;
            this.btnCommitData.Image = ((System.Drawing.Image)(resources.GetObject("btnCommitData.Image")));
            this.btnCommitData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCommitData.Name = "btnCommitData";
            this.btnCommitData.Size = new System.Drawing.Size(23, 22);
            this.btnCommitData.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // btnRollbackData
            // 
            this.btnRollbackData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRollbackData.Enabled = false;
            this.btnRollbackData.Image = ((System.Drawing.Image)(resources.GetObject("btnRollbackData.Image")));
            this.btnRollbackData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRollbackData.Name = "btnRollbackData";
            this.btnRollbackData.Size = new System.Drawing.Size(23, 22);
            this.btnRollbackData.Click += new System.EventHandler(this.btnRollback_Click);
            // 
            // spRefresh
            // 
            this.spRefresh.Name = "spRefresh";
            this.spRefresh.Size = new System.Drawing.Size(6, 25);
            // 
            // btnSelectAllData
            // 
            this.btnSelectAllData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAllData.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAllData.Image")));
            this.btnSelectAllData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAllData.Name = "btnSelectAllData";
            this.btnSelectAllData.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAllData.Click += new System.EventHandler(this.btnGridSelectAll_Click);
            // 
            // btnCopyData
            // 
            this.btnCopyData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopyData.Image = ((System.Drawing.Image)(resources.GetObject("btnCopyData.Image")));
            this.btnCopyData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopyData.Name = "btnCopyData";
            this.btnCopyData.Size = new System.Drawing.Size(23, 22);
            this.btnCopyData.Click += new System.EventHandler(this.btnGridCopy_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 25);
            // 
            // btnExportToFileData
            // 
            this.btnExportToFileData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnExportToFileData.Image = ((System.Drawing.Image)(resources.GetObject("btnExportToFileData.Image")));
            this.btnExportToFileData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnExportToFileData.Name = "btnExportToFileData";
            this.btnExportToFileData.Size = new System.Drawing.Size(23, 22);
            this.btnExportToFileData.Click += new System.EventHandler(this.btnGridExportToFile_Click);
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(6, 25);
            // 
            // btnTopData
            // 
            this.btnTopData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnTopData.Image = ((System.Drawing.Image)(resources.GetObject("btnTopData.Image")));
            this.btnTopData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnTopData.Name = "btnTopData";
            this.btnTopData.Size = new System.Drawing.Size(23, 22);
            this.btnTopData.Click += new System.EventHandler(this.btnTop_Click);
            // 
            // btnPreviousData
            // 
            this.btnPreviousData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPreviousData.Image = ((System.Drawing.Image)(resources.GetObject("btnPreviousData.Image")));
            this.btnPreviousData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPreviousData.Name = "btnPreviousData";
            this.btnPreviousData.Size = new System.Drawing.Size(23, 22);
            this.btnPreviousData.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNextData
            // 
            this.btnNextData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNextData.Image = ((System.Drawing.Image)(resources.GetObject("btnNextData.Image")));
            this.btnNextData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNextData.Name = "btnNextData";
            this.btnNextData.Size = new System.Drawing.Size(23, 22);
            this.btnNextData.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnLastData
            // 
            this.btnLastData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLastData.Image = ((System.Drawing.Image)(resources.GetObject("btnLastData.Image")));
            this.btnLastData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLastData.Name = "btnLastData";
            this.btnLastData.Size = new System.Drawing.Size(23, 22);
            this.btnLastData.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(6, 25);
            // 
            // lblFindData
            // 
            this.lblFindData.Name = "lblFindData";
            this.lblFindData.Size = new System.Drawing.Size(34, 22);
            this.lblFindData.Text = "Find:";
            // 
            // lblFind3
            // 
            this.lblFind3.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.lblFind3.Name = "lblFind3";
            this.lblFind3.Size = new System.Drawing.Size(115, 22);
            this.lblFind3.Text = "                                    ";
            // 
            // btnFindNextData
            // 
            this.btnFindNextData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFindNextData.Enabled = false;
            this.btnFindNextData.Image = ((System.Drawing.Image)(resources.GetObject("btnFindNextData.Image")));
            this.btnFindNextData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindNextData.Name = "btnFindNextData";
            this.btnFindNextData.Size = new System.Drawing.Size(23, 22);
            this.btnFindNextData.Click += new System.EventHandler(this.btnFindNext_Click);
            // 
            // btnFindPreviousData
            // 
            this.btnFindPreviousData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFindPreviousData.Enabled = false;
            this.btnFindPreviousData.Image = ((System.Drawing.Image)(resources.GetObject("btnFindPreviousData.Image")));
            this.btnFindPreviousData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFindPreviousData.Name = "btnFindPreviousData";
            this.btnFindPreviousData.Size = new System.Drawing.Size(23, 22);
            this.btnFindPreviousData.Click += new System.EventHandler(this.btnFindPrevious_Click);
            // 
            // btnCountData
            // 
            this.btnCountData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCountData.Enabled = false;
            this.btnCountData.Image = ((System.Drawing.Image)(resources.GetObject("btnCountData.Image")));
            this.btnCountData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCountData.Name = "btnCountData";
            this.btnCountData.Size = new System.Drawing.Size(23, 22);
            this.btnCountData.Click += new System.EventHandler(this.btnCount_Click);
            // 
            // btnHighlightData
            // 
            this.btnHighlightData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnHighlightData.Enabled = false;
            this.btnHighlightData.Image = ((System.Drawing.Image)(resources.GetObject("btnHighlightData.Image")));
            this.btnHighlightData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnHighlightData.Name = "btnHighlightData";
            this.btnHighlightData.Size = new System.Drawing.Size(23, 22);
            this.btnHighlightData.Click += new System.EventHandler(this.btnHighlight_Click);
            // 
            // btnClearHighlightData
            // 
            this.btnClearHighlightData.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnClearHighlightData.Enabled = false;
            this.btnClearHighlightData.Image = ((System.Drawing.Image)(resources.GetObject("btnClearHighlightData.Image")));
            this.btnClearHighlightData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnClearHighlightData.Name = "btnClearHighlightData";
            this.btnClearHighlightData.Size = new System.Drawing.Size(23, 22);
            this.btnClearHighlightData.Click += new System.EventHandler(this.btnClearHighlight_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(6, 25);
            // 
            // tabSettings
            // 
            this.tabSettings.Controls.Add(this.grpEditedColors);
            this.tabSettings.Image = ((System.Drawing.Image)(resources.GetObject("tabSettings.Image")));
            this.tabSettings.Location = new System.Drawing.Point(1, 27);
            this.tabSettings.Name = "tabSettings";
            this.tabSettings.Size = new System.Drawing.Size(1151, 680);
            this.tabSettings.TabIndex = 4;
            this.tabSettings.TabVisible = false;
            this.tabSettings.Text = "Settings";
            // 
            // grpEditedColors
            // 
            this.grpEditedColors.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpEditedColors.Controls.Add(this.txtTemp);
            this.grpEditedColors.Controls.Add(this.pnlChangedCellBackColor);
            this.grpEditedColors.Controls.Add(this.lblChangedCellBackColor);
            this.grpEditedColors.Controls.Add(this.pnlChangedCellForeColor);
            this.grpEditedColors.Controls.Add(this.lblChangedCellForeColor);
            this.grpEditedColors.Controls.Add(this.pnlDeletedRowBackColor);
            this.grpEditedColors.Controls.Add(this.lblDeletedRowBackColor);
            this.grpEditedColors.Controls.Add(this.pnlDeletedRowForeColor);
            this.grpEditedColors.Controls.Add(this.lblDeletedRowForeColor);
            this.grpEditedColors.Controls.Add(this.pnlNewRowBackColor);
            this.grpEditedColors.Controls.Add(this.lblNewRowBackColor);
            this.grpEditedColors.Controls.Add(this.pnlNewRowForeColor);
            this.grpEditedColors.Controls.Add(this.lblNewRowForeColor);
            this.grpEditedColors.Location = new System.Drawing.Point(22, 12);
            this.grpEditedColors.Name = "grpEditedColors";
            this.grpEditedColors.Size = new System.Drawing.Size(1111, 110);
            this.grpEditedColors.TabIndex = 126;
            this.grpEditedColors.TabStop = false;
            this.grpEditedColors.Text = "Color Config";
            this.c1ThemeController1.SetTheme(this.grpEditedColors, "(default)");
            // 
            // txtTemp
            // 
            this.txtTemp.Location = new System.Drawing.Point(-104, 11);
            this.txtTemp.Name = "txtTemp";
            this.txtTemp.Size = new System.Drawing.Size(100, 23);
            this.txtTemp.TabIndex = 127;
            this.c1ThemeController1.SetTheme(this.txtTemp, "(default)");
            // 
            // pnlChangedCellBackColor
            // 
            this.pnlChangedCellBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlChangedCellBackColor.Location = new System.Drawing.Point(477, 77);
            this.pnlChangedCellBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlChangedCellBackColor.Name = "pnlChangedCellBackColor";
            this.pnlChangedCellBackColor.Size = new System.Drawing.Size(74, 23);
            this.pnlChangedCellBackColor.TabIndex = 51;
            this.c1ThemeController1.SetTheme(this.pnlChangedCellBackColor, "(default)");
            this.pnlChangedCellBackColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblChangedCellBackColor
            // 
            this.lblChangedCellBackColor.AutoSize = true;
            this.lblChangedCellBackColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblChangedCellBackColor.Location = new System.Drawing.Point(347, 80);
            this.lblChangedCellBackColor.Name = "lblChangedCellBackColor";
            this.lblChangedCellBackColor.Size = new System.Drawing.Size(149, 16);
            this.lblChangedCellBackColor.TabIndex = 50;
            this.lblChangedCellBackColor.Text = "Changed Cell Back Color:";
            this.lblChangedCellBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblChangedCellBackColor, "(default)");
            // 
            // pnlChangedCellForeColor
            // 
            this.pnlChangedCellForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlChangedCellForeColor.Location = new System.Drawing.Point(150, 77);
            this.pnlChangedCellForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlChangedCellForeColor.Name = "pnlChangedCellForeColor";
            this.pnlChangedCellForeColor.Size = new System.Drawing.Size(74, 23);
            this.pnlChangedCellForeColor.TabIndex = 53;
            this.c1ThemeController1.SetTheme(this.pnlChangedCellForeColor, "(default)");
            this.pnlChangedCellForeColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblChangedCellForeColor
            // 
            this.lblChangedCellForeColor.AutoSize = true;
            this.lblChangedCellForeColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblChangedCellForeColor.Location = new System.Drawing.Point(28, 80);
            this.lblChangedCellForeColor.Name = "lblChangedCellForeColor";
            this.lblChangedCellForeColor.Size = new System.Drawing.Size(148, 16);
            this.lblChangedCellForeColor.TabIndex = 52;
            this.lblChangedCellForeColor.Text = "Changed Cell Fore Color:";
            this.lblChangedCellForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblChangedCellForeColor, "(default)");
            // 
            // pnlDeletedRowBackColor
            // 
            this.pnlDeletedRowBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDeletedRowBackColor.Location = new System.Drawing.Point(499, 49);
            this.pnlDeletedRowBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlDeletedRowBackColor.Name = "pnlDeletedRowBackColor";
            this.pnlDeletedRowBackColor.Size = new System.Drawing.Size(74, 23);
            this.pnlDeletedRowBackColor.TabIndex = 47;
            this.c1ThemeController1.SetTheme(this.pnlDeletedRowBackColor, "(default)");
            this.pnlDeletedRowBackColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblDeletedRowBackColor
            // 
            this.lblDeletedRowBackColor.AutoSize = true;
            this.lblDeletedRowBackColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDeletedRowBackColor.Location = new System.Drawing.Point(347, 52);
            this.lblDeletedRowBackColor.Name = "lblDeletedRowBackColor";
            this.lblDeletedRowBackColor.Size = new System.Drawing.Size(146, 16);
            this.lblDeletedRowBackColor.TabIndex = 46;
            this.lblDeletedRowBackColor.Text = "Deleted Row Back Color:";
            this.lblDeletedRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblDeletedRowBackColor, "(default)");
            // 
            // pnlDeletedRowForeColor
            // 
            this.pnlDeletedRowForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDeletedRowForeColor.Location = new System.Drawing.Point(150, 49);
            this.pnlDeletedRowForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlDeletedRowForeColor.Name = "pnlDeletedRowForeColor";
            this.pnlDeletedRowForeColor.Size = new System.Drawing.Size(74, 23);
            this.pnlDeletedRowForeColor.TabIndex = 49;
            this.c1ThemeController1.SetTheme(this.pnlDeletedRowForeColor, "(default)");
            this.pnlDeletedRowForeColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblDeletedRowForeColor
            // 
            this.lblDeletedRowForeColor.AutoSize = true;
            this.lblDeletedRowForeColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDeletedRowForeColor.Location = new System.Drawing.Point(28, 52);
            this.lblDeletedRowForeColor.Name = "lblDeletedRowForeColor";
            this.lblDeletedRowForeColor.Size = new System.Drawing.Size(145, 16);
            this.lblDeletedRowForeColor.TabIndex = 48;
            this.lblDeletedRowForeColor.Text = "Deleted Row Fore Color:";
            this.lblDeletedRowForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblDeletedRowForeColor, "(default)");
            // 
            // pnlNewRowBackColor
            // 
            this.pnlNewRowBackColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlNewRowBackColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNewRowBackColor.Location = new System.Drawing.Point(477, 21);
            this.pnlNewRowBackColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlNewRowBackColor.Name = "pnlNewRowBackColor";
            this.pnlNewRowBackColor.Size = new System.Drawing.Size(74, 23);
            this.pnlNewRowBackColor.TabIndex = 43;
            this.pnlNewRowBackColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblNewRowBackColor
            // 
            this.lblNewRowBackColor.AutoSize = true;
            this.lblNewRowBackColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblNewRowBackColor.Location = new System.Drawing.Point(347, 23);
            this.lblNewRowBackColor.Name = "lblNewRowBackColor";
            this.lblNewRowBackColor.Size = new System.Drawing.Size(127, 16);
            this.lblNewRowBackColor.TabIndex = 42;
            this.lblNewRowBackColor.Text = "New Row Back Color:";
            this.lblNewRowBackColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblNewRowBackColor, "(default)");
            // 
            // pnlNewRowForeColor
            // 
            this.pnlNewRowForeColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlNewRowForeColor.Location = new System.Drawing.Point(150, 21);
            this.pnlNewRowForeColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlNewRowForeColor.Name = "pnlNewRowForeColor";
            this.pnlNewRowForeColor.Size = new System.Drawing.Size(74, 23);
            this.pnlNewRowForeColor.TabIndex = 45;
            this.c1ThemeController1.SetTheme(this.pnlNewRowForeColor, "(default)");
            this.pnlNewRowForeColor.Click += new System.EventHandler(this.SelectColor_Click);
            // 
            // lblNewRowForeColor
            // 
            this.lblNewRowForeColor.AutoSize = true;
            this.lblNewRowForeColor.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblNewRowForeColor.Location = new System.Drawing.Point(28, 23);
            this.lblNewRowForeColor.Name = "lblNewRowForeColor";
            this.lblNewRowForeColor.Size = new System.Drawing.Size(126, 16);
            this.lblNewRowForeColor.TabIndex = 44;
            this.lblNewRowForeColor.Text = "New Row Fore Color:";
            this.lblNewRowForeColor.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // tabSqlPreview
            // 
            this.tabSqlPreview.Controls.Add(this.editorSqlPreview);
            this.tabSqlPreview.Controls.Add(this.tsSqlPreview);
            this.tabSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("tabSqlPreview.Image")));
            this.tabSqlPreview.Location = new System.Drawing.Point(1, 27);
            this.tabSqlPreview.Name = "tabSqlPreview";
            this.tabSqlPreview.Size = new System.Drawing.Size(1151, 680);
            this.tabSqlPreview.TabIndex = 5;
            this.tabSqlPreview.TabVisible = false;
            this.tabSqlPreview.Text = "SQL Preview";
            this.tabSqlPreview.Enter += new System.EventHandler(this.tabSqlPreview_Enter);
            // 
            // editorSqlPreview
            // 
            this.editorSqlPreview.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlPreview.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSqlPreview.CaretLineVisible = true;
            this.editorSqlPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorSqlPreview.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlPreview.Location = new System.Drawing.Point(0, 25);
            this.editorSqlPreview.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlPreview.Name = "editorSqlPreview";
            this.editorSqlPreview.ReadOnly = true;
            this.editorSqlPreview.Size = new System.Drawing.Size(1151, 655);
            this.editorSqlPreview.Styler = null;
            this.editorSqlPreview.TabIndex = 45;
            this.editorSqlPreview.WhitespaceSize = 3;
            this.editorSqlPreview.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlPreview.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editorSqlPreview_DoubleClick);
            this.editorSqlPreview.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editorSqlPreview_KeyDown);
            this.editorSqlPreview.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editorSqlPreview_KeyPress);
            this.editorSqlPreview.KeyUp += new System.Windows.Forms.KeyEventHandler(this.editorSqlPreview_KeyUp);
            this.editorSqlPreview.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorSqlPreview_MouseDown);
            // 
            // tsSqlPreview
            // 
            this.tsSqlPreview.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsSqlPreview.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAllSqlPreview,
            this.btnCopySqlPreview,
            this.btnSaveAsSqlPreview,
            this.toolStripSeparator9,
            this.btnWordWrapSqlPreview,
            this.btnWordWrap2SqlPreview,
            this.btnShowAllCharactersSqlPreview,
            this.btnShowAllCharacters2SqlPreview,
            this.toolStripSeparator13,
            this.btnApplyEditSqlPreview,
            this.btnCancelEditSqlPreview,
            this.toolStripSeparator7,
            this.btnCommitSqlPreview,
            this.btnRollbackSqlPreview,
            this.toolStripSeparator19,
            this.btnZoomInSqlPreview,
            this.btnZoomOutSqlPreview});
            this.tsSqlPreview.Location = new System.Drawing.Point(0, 0);
            this.tsSqlPreview.Name = "tsSqlPreview";
            this.tsSqlPreview.Size = new System.Drawing.Size(1151, 25);
            this.tsSqlPreview.TabIndex = 2;
            this.c1ThemeController1.SetTheme(this.tsSqlPreview, "(default)");
            // 
            // btnSelectAllSqlPreview
            // 
            this.btnSelectAllSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAllSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAllSqlPreview.Image")));
            this.btnSelectAllSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAllSqlPreview.Name = "btnSelectAllSqlPreview";
            this.btnSelectAllSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAllSqlPreview.Click += new System.EventHandler(this.btnSelectAllSqlPreview_Click);
            // 
            // btnCopySqlPreview
            // 
            this.btnCopySqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopySqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnCopySqlPreview.Image")));
            this.btnCopySqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopySqlPreview.Name = "btnCopySqlPreview";
            this.btnCopySqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnCopySqlPreview.Click += new System.EventHandler(this.btnCopySqlPreview_Click);
            // 
            // btnSaveAsSqlPreview
            // 
            this.btnSaveAsSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAsSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAsSqlPreview.Image")));
            this.btnSaveAsSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAsSqlPreview.Name = "btnSaveAsSqlPreview";
            this.btnSaveAsSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnSaveAsSqlPreview.Click += new System.EventHandler(this.btnSaveAsSqlPreview_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(6, 25);
            // 
            // btnWordWrapSqlPreview
            // 
            this.btnWordWrapSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrapSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrapSqlPreview.Image")));
            this.btnWordWrapSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrapSqlPreview.Name = "btnWordWrapSqlPreview";
            this.btnWordWrapSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrapSqlPreview.Click += new System.EventHandler(this.btnWordWrapSqlPreview_Click);
            // 
            // btnWordWrap2SqlPreview
            // 
            this.btnWordWrap2SqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2SqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2SqlPreview.Image")));
            this.btnWordWrap2SqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2SqlPreview.Name = "btnWordWrap2SqlPreview";
            this.btnWordWrap2SqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap2SqlPreview.Visible = false;
            this.btnWordWrap2SqlPreview.Click += new System.EventHandler(this.btnWordWrapSqlPreview_Click);
            // 
            // btnShowAllCharactersSqlPreview
            // 
            this.btnShowAllCharactersSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharactersSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharactersSqlPreview.Image")));
            this.btnShowAllCharactersSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharactersSqlPreview.Name = "btnShowAllCharactersSqlPreview";
            this.btnShowAllCharactersSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharactersSqlPreview.Click += new System.EventHandler(this.btnShowAllCharactersSqlPreview_Click);
            // 
            // btnShowAllCharacters2SqlPreview
            // 
            this.btnShowAllCharacters2SqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2SqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2SqlPreview.Image")));
            this.btnShowAllCharacters2SqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2SqlPreview.Name = "btnShowAllCharacters2SqlPreview";
            this.btnShowAllCharacters2SqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters2SqlPreview.Visible = false;
            this.btnShowAllCharacters2SqlPreview.Click += new System.EventHandler(this.btnShowAllCharactersSqlPreview_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(6, 25);
            // 
            // btnApplyEditSqlPreview
            // 
            this.btnApplyEditSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnApplyEditSqlPreview.Image")));
            this.btnApplyEditSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnApplyEditSqlPreview.Name = "btnApplyEditSqlPreview";
            this.btnApplyEditSqlPreview.Size = new System.Drawing.Size(60, 22);
            this.btnApplyEditSqlPreview.Tag = "NA";
            this.btnApplyEditSqlPreview.Text = "Apply";
            this.btnApplyEditSqlPreview.Click += new System.EventHandler(this.btnApplyEdit_Click);
            // 
            // btnCancelEditSqlPreview
            // 
            this.btnCancelEditSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelEditSqlPreview.Image")));
            this.btnCancelEditSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancelEditSqlPreview.Name = "btnCancelEditSqlPreview";
            this.btnCancelEditSqlPreview.Size = new System.Drawing.Size(65, 22);
            this.btnCancelEditSqlPreview.Text = "Cancel";
            this.btnCancelEditSqlPreview.Visible = false;
            this.btnCancelEditSqlPreview.Click += new System.EventHandler(this.btnCancelEdit_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 25);
            // 
            // btnCommitSqlPreview
            // 
            this.btnCommitSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCommitSqlPreview.Enabled = false;
            this.btnCommitSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnCommitSqlPreview.Image")));
            this.btnCommitSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCommitSqlPreview.Name = "btnCommitSqlPreview";
            this.btnCommitSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnCommitSqlPreview.Click += new System.EventHandler(this.btnCommit_Click);
            // 
            // btnRollbackSqlPreview
            // 
            this.btnRollbackSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnRollbackSqlPreview.Enabled = false;
            this.btnRollbackSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnRollbackSqlPreview.Image")));
            this.btnRollbackSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnRollbackSqlPreview.Name = "btnRollbackSqlPreview";
            this.btnRollbackSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnRollbackSqlPreview.Click += new System.EventHandler(this.btnRollback_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(6, 25);
            // 
            // btnZoomInSqlPreview
            // 
            this.btnZoomInSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomInSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomInSqlPreview.Image")));
            this.btnZoomInSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomInSqlPreview.Name = "btnZoomInSqlPreview";
            this.btnZoomInSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnZoomInSqlPreview.Click += new System.EventHandler(this.btnZoomInSqlPreview_Click);
            // 
            // btnZoomOutSqlPreview
            // 
            this.btnZoomOutSqlPreview.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnZoomOutSqlPreview.Image = ((System.Drawing.Image)(resources.GetObject("btnZoomOutSqlPreview.Image")));
            this.btnZoomOutSqlPreview.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnZoomOutSqlPreview.Name = "btnZoomOutSqlPreview";
            this.btnZoomOutSqlPreview.Size = new System.Drawing.Size(23, 22);
            this.btnZoomOutSqlPreview.Click += new System.EventHandler(this.btnZoomOutSqlPreview_Click);
            // 
            // lblLevel
            // 
            this.lblLevel.AutoSize = true;
            this.lblLevel.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblLevel.Location = new System.Drawing.Point(628, 8);
            this.lblLevel.Name = "lblLevel";
            this.lblLevel.Size = new System.Drawing.Size(50, 16);
            this.lblLevel.TabIndex = 39;
            this.lblLevel.Text = "lblLevel";
            this.lblLevel.Visible = false;
            // 
            // lblSchemaName2
            // 
            this.lblSchemaName2.AutoSize = true;
            this.lblSchemaName2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSchemaName2.Location = new System.Drawing.Point(805, 8);
            this.lblSchemaName2.Name = "lblSchemaName2";
            this.lblSchemaName2.Size = new System.Drawing.Size(101, 16);
            this.lblSchemaName2.TabIndex = 38;
            this.lblSchemaName2.Text = "lblSchemaName";
            this.lblSchemaName2.Visible = false;
            // 
            // lblSchemaType2
            // 
            this.lblSchemaType2.AutoSize = true;
            this.lblSchemaType2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblSchemaType2.Location = new System.Drawing.Point(694, 8);
            this.lblSchemaType2.Name = "lblSchemaType2";
            this.lblSchemaType2.Size = new System.Drawing.Size(94, 16);
            this.lblSchemaType2.TabIndex = 37;
            this.lblSchemaType2.Text = "lblSchemaType";
            this.lblSchemaType2.Visible = false;
            // 
            // tmrMouseDoubleClick
            // 
            this.tmrMouseDoubleClick.Tick += new System.EventHandler(this.tmrMouseDoubleClick_Tick);
            // 
            // tmrMother2Child
            // 
            this.tmrMother2Child.Enabled = true;
            this.tmrMother2Child.Tick += new System.EventHandler(this.tmrMother2Child_Tick);
            // 
            // c1SuperTooltip1
            // 
            this.c1SuperTooltip1.Font = new System.Drawing.Font("Tahoma", 8F);
            this.c1SuperTooltip1.RightToLeft = System.Windows.Forms.RightToLeft.Inherit;
            this.c1ThemeController1.SetTheme(this.c1SuperTooltip1, "(default)");
            // 
            // c1CommandHolder1
            // 
            this.c1CommandHolder1.Owner = this;
            // 
            // SchemaBrowserForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1436, 708);
            this.Controls.Add(this.splitContainer1);
            this.Font = new System.Drawing.Font("Microsoft JhengHei UI", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MinimumSize = new System.Drawing.Size(800, 565);
            this.Name = "SchemaBrowserForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Schema Browser";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboSchema)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridSchemaBrowser)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtSchemaFilter)).EndInit();
            this.tsSchemaBrowser.ResumeLayout(false);
            this.tsSchemaBrowser.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.tabSchemaBrowser)).EndInit();
            this.tabSchemaBrowser.ResumeLayout(false);
            this.tabSqlPane.ResumeLayout(false);
            this.tabSqlPane.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.chkCopyAsHTML)).EndInit();
            this.tsSqlPane.ResumeLayout(false);
            this.tsSqlPane.PerformLayout();
            this.tabTableStructure.ResumeLayout(false);
            this.tabTableStructure.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridStructure)).EndInit();
            this.tsTableStructure.ResumeLayout(false);
            this.tsTableStructure.PerformLayout();
            this.tabView100RowsTop.ResumeLayout(false);
            this.tabView100RowsTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid100RowsTop)).EndInit();
            this.tsView100RowsTop.ResumeLayout(false);
            this.tsView100RowsTop.PerformLayout();
            this.tabData.ResumeLayout(false);
            this.tabData.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboFind)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkShowFilterRowData)).EndInit();
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel1.PerformLayout();
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_ColumnName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtColumnFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridColumns)).EndInit();
            this.tsColumnFilter.ResumeLayout(false);
            this.tsColumnFilter.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridData)).EndInit();
            this.tsData.ResumeLayout(false);
            this.tsData.PerformLayout();
            this.tabSettings.ResumeLayout(false);
            this.grpEditedColors.ResumeLayout(false);
            this.grpEditedColors.PerformLayout();
            this.tabSqlPreview.ResumeLayout(false);
            this.tabSqlPreview.PerformLayout();
            this.tsSqlPreview.ResumeLayout(false);
            this.tsSqlPreview.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1CommandHolder1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlPane;
        private System.Windows.Forms.ToolStrip tsSchemaBrowser;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridSchemaBrowser;
        private System.Windows.Forms.ToolStripButton btnRefresh;
        private System.Windows.Forms.ToolStripSeparator tsSeparator1;
        private System.Windows.Forms.Timer tmrMouseDoubleClick;
        private System.Windows.Forms.Label lblSchemaName2;
        private System.Windows.Forms.Label lblSchemaType2;
        private System.Windows.Forms.Label lblLevel;
        private System.Windows.Forms.Timer tmrMother2Child;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Input.C1CheckBox chkCopyAsHTML;
        private C1.Win.C1Command.C1CommandHolder c1CommandHolder1;
        private System.Windows.Forms.Label lblSchemaType0;
        private System.Windows.Forms.ToolStripLabel toolStripLabel4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel5;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private C1.Win.C1Command.C1DockingTab tabSchemaBrowser;
        private C1.Win.C1Command.C1DockingTabPage tabSqlPane;
        private C1.Win.C1Command.C1DockingTabPage tabTableStructure;
        private System.Windows.Forms.ToolStrip tsSqlPane;
        private System.Windows.Forms.ToolStripButton btnSelectAllSqlPane;
        private System.Windows.Forms.ToolStripButton btnCopySqlPane;
        private System.Windows.Forms.ToolStripButton btnWordWrapSqlPane;
        private System.Windows.Forms.ToolStripButton btnWordWrap2SqlPane;
        private System.Windows.Forms.ToolStripSeparator tsSeparator4;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ToolStripLabel lblInfo;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridStructure;
        private System.Windows.Forms.ToolStrip tsTableStructure;
        private System.Windows.Forms.ToolStripButton btnSelectAllTableStructure;
        private System.Windows.Forms.ToolStripButton btnCopyTableStructure;
        private System.Windows.Forms.ToolStripButton btnExportToFileTableStructure;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripLabel lblTableOrViewName;
        private System.Windows.Forms.ToolStripLabel lblTableName01;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private C1.Win.C1Command.C1DockingTabPage tabView100RowsTop;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1Grid100RowsTop;
        private System.Windows.Forms.ToolStrip tsView100RowsTop;
        private System.Windows.Forms.ToolStripLabel lblTableName02;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton btnSelectAllTop100;
        private System.Windows.Forms.ToolStripButton btnCopyTop100;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton btnExportToFileTop100;
        private C1.Win.C1Command.C1DockingTabPage tabData;
        private System.Windows.Forms.ToolStrip tsData;
        private System.Windows.Forms.ToolStripButton btnSelectAllData;
        private System.Windows.Forms.ToolStripButton btnCopyData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton btnExportToFileData;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridData;
        private C1.Win.C1Input.C1TextBox txtSchemaFilter;
        private System.Windows.Forms.Label lblFilter;
        private System.Windows.Forms.ToolStripButton btnFilterData;
        private System.Windows.Forms.ToolStripSeparator spRefresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripButton btnCommitData;
        private System.Windows.Forms.ToolStripButton btnRollbackData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripButton btnRefreshData;
        private System.Windows.Forms.ToolStripButton btnFilterRedData;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.Label lblSchema;
        private C1.Win.C1Input.C1ComboBox cboSchema;
        private C1.Win.C1Command.C1DockingTabPage tabSettings;
        private C1.Win.C1Command.C1DockingTabPage tabSqlPreview;
        private System.Windows.Forms.GroupBox grpEditedColors;
        private System.Windows.Forms.Panel pnlChangedCellBackColor;
        private System.Windows.Forms.Label lblChangedCellBackColor;
        private System.Windows.Forms.Panel pnlChangedCellForeColor;
        private System.Windows.Forms.Label lblChangedCellForeColor;
        private System.Windows.Forms.Panel pnlDeletedRowBackColor;
        private System.Windows.Forms.Label lblDeletedRowBackColor;
        private System.Windows.Forms.Panel pnlDeletedRowForeColor;
        private System.Windows.Forms.Label lblDeletedRowForeColor;
        private System.Windows.Forms.Panel pnlNewRowBackColor;
        private System.Windows.Forms.Label lblNewRowBackColor;
        private System.Windows.Forms.Panel pnlNewRowForeColor;
        private System.Windows.Forms.Label lblNewRowForeColor;
        private System.Windows.Forms.ToolStrip tsSqlPreview;
        private System.Windows.Forms.ToolStripButton btnSelectAllSqlPreview;
        private System.Windows.Forms.ToolStripButton btnCopySqlPreview;
        private System.Windows.Forms.ToolStripButton btnSaveAsSqlPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripButton btnWordWrapSqlPreview;
        private System.Windows.Forms.ToolStripButton btnWordWrap2SqlPreview;
        private System.Windows.Forms.ToolStripButton btnShowAllCharactersSqlPreview;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2SqlPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripButton btnZoomInSqlPreview;
        private System.Windows.Forms.ToolStripButton btnZoomOutSqlPreview;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlPreview;
        private C1.Win.C1Input.C1CheckBox chkShowFilterRowData;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.ToolStrip tsColumnFilter;
        private System.Windows.Forms.ToolStripLabel lblColumnFilterData;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridColumns;
        private C1.Win.C1Input.C1TextBox txtColumnFilter;
        private System.Windows.Forms.Label lblPosition2;
        private System.Windows.Forms.Label lblFind2;
        private System.Windows.Forms.ToolStripLabel lblFindData;
        private C1.Win.C1Input.C1ComboBox cboFind;
        private System.Windows.Forms.ToolStripButton btnShowColumnsData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripButton btnEditCellData;
        private System.Windows.Forms.ToolStripButton btnInsertNewRowData;
        private System.Windows.Forms.ToolStripButton btnDuplicateCurrentRowData;
        private System.Windows.Forms.ToolStripButton btnDeleteCurrentRowData;
        private System.Windows.Forms.ToolStripButton btnSetNullData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripButton btnApplyEditData;
        private System.Windows.Forms.ToolStripButton btnCancelEditData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripButton btnTopData;
        private System.Windows.Forms.ToolStripButton btnPreviousData;
        private System.Windows.Forms.ToolStripButton btnNextData;
        private System.Windows.Forms.ToolStripButton btnLastData;
        private System.Windows.Forms.ToolStripLabel lblFind3;
        private System.Windows.Forms.ToolStripButton btnFindNextData;
        private System.Windows.Forms.ToolStripButton btnFindPreviousData;
        private System.Windows.Forms.ToolStripButton btnCountData;
        private System.Windows.Forms.ToolStripButton btnHighlightData;
        private System.Windows.Forms.ToolStripButton btnClearHighlightData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private C1.Win.C1SuperTooltip.C1SuperTooltip c1SuperTooltip1;
        private System.Windows.Forms.TextBox txtTemp;
        private System.Windows.Forms.ToolStripButton btnApplyEditSqlPreview;
        private System.Windows.Forms.ToolStripButton btnCancelEditSqlPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton btnCommitSqlPreview;
        private System.Windows.Forms.ToolStripButton btnRollbackSqlPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripButton btnSaveAsSqlPane;
        private System.Windows.Forms.ToolStripButton btnSqlPreviewData;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.Label lblColumnNamePosition;
        private C1.Win.C1Input.C1Button btnHelp_ColumnName;
        private System.Windows.Forms.ToolStripButton btnEditCellWithEditFormData;
        private System.Windows.Forms.ToolStripButton btnShowAllCharactersSqlPane;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2SqlPane;
        private System.Windows.Forms.ToolStripSplitButton btnExpandCollapse;
        private System.Windows.Forms.ToolStripMenuItem mnuExpandAll;
        private System.Windows.Forms.ToolStripMenuItem mnuCollapseAll;
        private System.Windows.Forms.ToolStripButton btnZoomInSqlPane;
        private System.Windows.Forms.ToolStripButton btnZoomOutSqlPane;
        private System.Windows.Forms.ToolStripLabel lblViewName;
    }
}

