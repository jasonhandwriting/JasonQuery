using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    partial class SqlVariablesForm
    {
        /// <summary>
        /// Required designer Parameter.
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlVariablesForm));
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.c1GridVariables = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.lblPreview = new System.Windows.Forms.Label();
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnCancel = new C1.Win.C1Input.C1Button();
            this.btnPreview = new C1.Win.C1Input.C1Button();
            this.editorSql = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.picHelp = new System.Windows.Forms.PictureBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.btnSelectAll = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2 = new System.Windows.Forms.ToolStripButton();
            this.panel1 = new System.Windows.Forms.Panel();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVariables)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreview)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).BeginInit();
            this.toolStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // c1GridVariables
            // 
            this.c1GridVariables.AllowColSelect = false;
            this.c1GridVariables.AllowFilter = false;
            this.c1GridVariables.AllowRowSizing = C1.Win.C1TrueDBGrid.RowSizingEnum.None;
            this.c1GridVariables.AllowSort = false;
            this.c1GridVariables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridVariables.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1GridVariables.CaptionHeight = 19;
            this.c1GridVariables.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1GridVariables.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridVariables.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridVariables.Images"))));
            this.c1GridVariables.Location = new System.Drawing.Point(12, 12);
            this.c1GridVariables.MultiSelect = C1.Win.C1TrueDBGrid.MultiSelectEnum.None;
            this.c1GridVariables.Name = "c1GridVariables";
            this.c1GridVariables.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1GridVariables.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridVariables.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridVariables.PreviewInfo.ZoomFactor = 75D;
            this.c1GridVariables.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridVariables.PrintInfo.MeasurementPrinterName = null;
            this.c1GridVariables.RowHeight = 19;
            this.c1GridVariables.Size = new System.Drawing.Size(547, 169);
            this.c1GridVariables.TabIndex = 0;
            this.c1ThemeController1.SetTheme(this.c1GridVariables, "(default)");
            this.c1GridVariables.UseCompatibleTextRendering = false;
            this.c1GridVariables.FetchCellStyle += new C1.Win.C1TrueDBGrid.FetchCellStyleEventHandler(this.c1GridVariables_FetchCellStyle);
            this.c1GridVariables.PropBag = resources.GetString("c1GridVariables.PropBag");
            // 
            // lblPreview
            // 
            this.lblPreview.AutoSize = true;
            this.lblPreview.Location = new System.Drawing.Point(10, 193);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new System.Drawing.Size(53, 16);
            this.lblPreview.TabIndex = 44;
            this.lblPreview.Text = "Preview:";
            this.c1ThemeController1.SetTheme(this.lblPreview, "(default)");
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(572, 12);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(80, 29);
            this.btnOK.TabIndex = 60;
            this.btnOK.Text = "OK";
            this.c1ThemeController1.SetTheme(this.btnOK, "(default)");
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(572, 54);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(80, 29);
            this.btnCancel.TabIndex = 61;
            this.btnCancel.Text = "Cancel";
            this.c1ThemeController1.SetTheme(this.btnCancel, "(default)");
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnPreview.Location = new System.Drawing.Point(572, 152);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(80, 29);
            this.btnPreview.TabIndex = 62;
            this.btnPreview.Text = "Preview";
            this.c1ThemeController1.SetTheme(this.btnPreview, "(default)");
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // editorSql
            // 
            this.editorSql.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.editorSql.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSql.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorSql.CaretLineVisible = true;
            this.editorSql.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSql.Location = new System.Drawing.Point(12, 213);
            this.editorSql.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSql.Name = "editorSql";
            this.editorSql.ReadOnly = true;
            this.editorSql.Size = new System.Drawing.Size(640, 237);
            this.editorSql.Styler = null;
            this.editorSql.TabIndex = 43;
            this.editorSql.Tag = "";
            this.editorSql.WhitespaceSize = 3;
            this.editorSql.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            // 
            // picHelp
            // 
            this.picHelp.Image = ((System.Drawing.Image)(resources.GetObject("picHelp.Image")));
            this.picHelp.Location = new System.Drawing.Point(572, 188);
            this.picHelp.Name = "picHelp";
            this.picHelp.Size = new System.Drawing.Size(16, 16);
            this.picHelp.TabIndex = 86;
            this.picHelp.TabStop = false;
            this.picHelp.Visible = false;
            // 
            // toolStrip1
            // 
            this.toolStrip1.BackColor = System.Drawing.SystemColors.Control;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnSelectAll,
            this.btnCopy,
            this.btnWordWrap,
            this.btnWordWrap2});
            this.toolStrip1.Location = new System.Drawing.Point(4, 1);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(72, 25);
            this.toolStrip1.TabIndex = 87;
            this.toolStrip1.Text = "toolStrip1";
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
            this.btnWordWrap.Text = "Word Wrap";
            this.btnWordWrap.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnWordWrap2
            // 
            this.btnWordWrap2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2.Image")));
            this.btnWordWrap2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2.Name = "btnWordWrap2";
            this.btnWordWrap2.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap2.Text = "Word Wrap";
            this.btnWordWrap2.Visible = false;
            this.btnWordWrap2.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.toolStrip1);
            this.panel1.Location = new System.Drawing.Point(151, 188);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(75, 24);
            this.panel1.TabIndex = 88;
            // 
            // SqlVariablesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(664, 462);
            this.Controls.Add(this.editorSql);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.picHelp);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.lblPreview);
            this.Controls.Add(this.c1GridVariables);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(680, 500);
            this.Name = "SqlVariablesForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Variables";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridVariables)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnPreview)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picHelp)).EndInit();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridVariables;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSql;
        private System.Windows.Forms.Label lblPreview;
        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnCancel;
        private C1.Win.C1Input.C1Button btnPreview;
        private System.Windows.Forms.PictureBox picHelp;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton btnSelectAll;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.Panel panel1;
    }
}