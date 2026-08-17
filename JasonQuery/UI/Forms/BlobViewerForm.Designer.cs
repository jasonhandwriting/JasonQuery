namespace JasonQuery.UI.Forms
{
    partial class BlobViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BlobViewerForm));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.btnOpenFile = new System.Windows.Forms.ToolStripButton();
            this.btnSave = new System.Windows.Forms.ToolStripButton();
            this.btnSaveAs = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.lblEncoding = new System.Windows.Forms.ToolStripLabel();
            this.cboEncoding = new System.Windows.Forms.ToolStripComboBox();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.lblFontSize = new System.Windows.Forms.ToolStripLabel();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.c1StatusBar1 = new C1.Win.C1Ribbon.C1StatusBar();
            this.lblPosition = new C1.Win.C1Ribbon.RibbonLabel();
            this.sp1 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblBitPosition = new C1.Win.C1Ribbon.RibbonLabel();
            this.sp2 = new C1.Win.C1Ribbon.RibbonSeparator();
            this.lblFileSize = new C1.Win.C1Ribbon.RibbonLabel();
            this.lblLn = new System.Windows.Forms.Label();
            this.lblCol = new System.Windows.Forms.Label();
            this.lblBytes = new System.Windows.Forms.Label();
            this.bitControl1 = new JasonLibrary.UI.Controls.HexBox.BitControl();
            this.lblEncoding2 = new System.Windows.Forms.Label();
            this.chkReadOnly = new C1.Win.C1Input.C1CheckBox();
            this.nudFontSize = new System.Windows.Forms.NumericUpDown();
            this.chkBits = new C1.Win.C1Input.C1CheckBox();
            this.chkTopMost = new C1.Win.C1Input.C1CheckBox();
            this.hexBox = new JasonLibrary.UI.Controls.HexBox.HexBox();
            this.toolStrip.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkReadOnly)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBits)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTopMost)).BeginInit();
            this.SuspendLayout();
            // 
            // toolStrip
            // 
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnOpenFile,
            this.btnSave,
            this.btnSaveAs,
            this.toolStripSeparator1,
            this.lblEncoding,
            this.cboEncoding,
            this.toolStripSeparator2,
            this.lblFontSize});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Size = new System.Drawing.Size(757, 31);
            this.toolStrip.TabIndex = 0;
            this.toolStrip.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.toolStrip, "(default)");
            // 
            // btnOpenFile
            // 
            this.btnOpenFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnOpenFile.Font = new System.Drawing.Font("Microsoft JhengHei", 9F);
            this.btnOpenFile.Image = ((System.Drawing.Image)(resources.GetObject("btnOpenFile.Image")));
            this.btnOpenFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnOpenFile.Name = "btnOpenFile";
            this.btnOpenFile.Size = new System.Drawing.Size(28, 28);
            this.btnOpenFile.Click += new System.EventHandler(this.btnOpenFile_Click);
            // 
            // btnSave
            // 
            this.btnSave.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSave.Font = new System.Drawing.Font("Microsoft JhengHei", 9F);
            this.btnSave.Image = ((System.Drawing.Image)(resources.GetObject("btnSave.Image")));
            this.btnSave.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(28, 28);
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // btnSaveAs
            // 
            this.btnSaveAs.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveAs.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveAs.Image")));
            this.btnSaveAs.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveAs.Name = "btnSaveAs";
            this.btnSaveAs.Size = new System.Drawing.Size(28, 28);
            this.btnSaveAs.Click += new System.EventHandler(this.btnSaveAs_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // lblEncoding
            // 
            this.lblEncoding.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblEncoding.Name = "lblEncoding";
            this.lblEncoding.Size = new System.Drawing.Size(65, 28);
            this.lblEncoding.Text = "Encoding:";
            // 
            // cboEncoding
            // 
            this.cboEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEncoding.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.cboEncoding.Name = "cboEncoding";
            this.cboEncoding.Size = new System.Drawing.Size(155, 31);
            this.cboEncoding.SelectedIndexChanged += new System.EventHandler(this.cboEncoding_SelectedIndexChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 31);
            // 
            // lblFontSize
            // 
            this.lblFontSize.Font = new System.Drawing.Font("Microsoft JhengHei", 9F);
            this.lblFontSize.Name = "lblFontSize";
            this.lblFontSize.Size = new System.Drawing.Size(62, 28);
            this.lblFontSize.Text = "Font Size:";
            // 
            // c1StatusBar1
            // 
            this.c1StatusBar1.LeftPaneItems.Add(this.lblPosition);
            this.c1StatusBar1.LeftPaneItems.Add(this.sp1);
            this.c1StatusBar1.LeftPaneItems.Add(this.lblBitPosition);
            this.c1StatusBar1.LeftPaneItems.Add(this.sp2);
            this.c1StatusBar1.LeftPaneItems.Add(this.lblFileSize);
            this.c1StatusBar1.Location = new System.Drawing.Point(0, 710);
            this.c1StatusBar1.Name = "c1StatusBar1";
            this.c1StatusBar1.RightPaneWidth = 0;
            this.c1StatusBar1.Size = new System.Drawing.Size(757, 22);
            this.c1StatusBar1.SizingGrip = false;
            this.c1ThemeController1.SetTheme(this.c1StatusBar1, "(default)");
            this.c1StatusBar1.VisualStyle = C1.Win.C1Ribbon.VisualStyle.Office2010Blue;
            // 
            // lblPosition
            // 
            this.lblPosition.Name = "lblPosition";
            this.lblPosition.Visible = false;
            // 
            // sp1
            // 
            this.sp1.Name = "sp1";
            this.sp1.Visible = false;
            // 
            // lblBitPosition
            // 
            this.lblBitPosition.Name = "lblBitPosition";
            this.lblBitPosition.Visible = false;
            // 
            // sp2
            // 
            this.sp2.Name = "sp2";
            this.sp2.Visible = false;
            // 
            // lblFileSize
            // 
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Visible = false;
            // 
            // lblLn
            // 
            this.lblLn.AutoSize = true;
            this.lblLn.Location = new System.Drawing.Point(12, -50);
            this.lblLn.Name = "lblLn";
            this.lblLn.Size = new System.Drawing.Size(0, 16);
            this.lblLn.TabIndex = 9;
            this.c1ThemeController1.SetTheme(this.lblLn, "(default)");
            this.lblLn.Visible = false;
            // 
            // lblCol
            // 
            this.lblCol.AutoSize = true;
            this.lblCol.Location = new System.Drawing.Point(12, -50);
            this.lblCol.Name = "lblCol";
            this.lblCol.Size = new System.Drawing.Size(0, 16);
            this.lblCol.TabIndex = 10;
            this.c1ThemeController1.SetTheme(this.lblCol, "(default)");
            this.lblCol.Visible = false;
            // 
            // lblBytes
            // 
            this.lblBytes.AutoSize = true;
            this.lblBytes.Location = new System.Drawing.Point(12, -90);
            this.lblBytes.Name = "lblBytes";
            this.lblBytes.Size = new System.Drawing.Size(0, 16);
            this.lblBytes.TabIndex = 11;
            this.c1ThemeController1.SetTheme(this.lblBytes, "(default)");
            this.lblBytes.Visible = false;
            // 
            // bitControl1
            // 
            this.bitControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.bitControl1.Location = new System.Drawing.Point(1, 657);
            this.bitControl1.Name = "bitControl1";
            this.bitControl1.Size = new System.Drawing.Size(756, 53);
            this.bitControl1.TabIndex = 3;
            this.c1ThemeController1.SetTheme(this.bitControl1, "(default)");
            this.bitControl1.Visible = false;
            this.bitControl1.BitChanged += new System.EventHandler(this.bitControl1_BitChanged);
            // 
            // lblEncoding2
            // 
            this.lblEncoding2.AutoSize = true;
            this.lblEncoding2.Location = new System.Drawing.Point(90, 24);
            this.lblEncoding2.Name = "lblEncoding2";
            this.lblEncoding2.Size = new System.Drawing.Size(65, 16);
            this.lblEncoding2.TabIndex = 14;
            this.lblEncoding2.Text = "Encoding:";
            this.c1ThemeController1.SetTheme(this.lblEncoding2, "(default)");
            this.lblEncoding2.Visible = false;
            // 
            // chkReadOnly
            // 
            this.chkReadOnly.AutoSize = true;
            this.chkReadOnly.BackColor = System.Drawing.Color.Transparent;
            this.chkReadOnly.BorderColor = System.Drawing.Color.Transparent;
            this.chkReadOnly.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkReadOnly.ForeColor = System.Drawing.Color.Black;
            this.chkReadOnly.Location = new System.Drawing.Point(549, 5);
            this.chkReadOnly.Name = "chkReadOnly";
            this.chkReadOnly.Padding = new System.Windows.Forms.Padding(1);
            this.chkReadOnly.Size = new System.Drawing.Size(88, 22);
            this.chkReadOnly.TabIndex = 15;
            this.chkReadOnly.Text = "Read Only";
            this.c1ThemeController1.SetTheme(this.chkReadOnly, "(default)");
            this.chkReadOnly.UseVisualStyleBackColor = true;
            this.chkReadOnly.Value = null;
            this.chkReadOnly.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkReadOnly.CheckedChanged += new System.EventHandler(this.chkReadOnly_CheckedChanged);
            // 
            // nudFontSize
            // 
            this.nudFontSize.Location = new System.Drawing.Point(423, 4);
            this.nudFontSize.Maximum = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFontSize.Minimum = new decimal(new int[] {
            8,
            0,
            0,
            0});
            this.nudFontSize.Name = "nudFontSize";
            this.nudFontSize.Size = new System.Drawing.Size(37, 23);
            this.nudFontSize.TabIndex = 16;
            this.nudFontSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.c1ThemeController1.SetTheme(this.nudFontSize, "(default)");
            this.nudFontSize.Value = new decimal(new int[] {
            15,
            0,
            0,
            0});
            this.nudFontSize.ValueChanged += new System.EventHandler(this.nudFontSize_ValueChanged);
            this.nudFontSize.Enter += new System.EventHandler(this.nudFontSize_Enter);
            this.nudFontSize.Leave += new System.EventHandler(this.nudFontSize_Leave);
            this.nudFontSize.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudFontSize_MouseClick);
            // 
            // chkBits
            // 
            this.chkBits.AutoSize = true;
            this.chkBits.BackColor = System.Drawing.Color.Transparent;
            this.chkBits.BorderColor = System.Drawing.Color.Transparent;
            this.chkBits.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkBits.ForeColor = System.Drawing.Color.Black;
            this.chkBits.Location = new System.Drawing.Point(495, 5);
            this.chkBits.Name = "chkBits";
            this.chkBits.Padding = new System.Windows.Forms.Padding(1);
            this.chkBits.Size = new System.Drawing.Size(48, 22);
            this.chkBits.TabIndex = 13;
            this.chkBits.Text = "Bits";
            this.c1ThemeController1.SetTheme(this.chkBits, "(default)");
            this.chkBits.UseVisualStyleBackColor = true;
            this.chkBits.Value = null;
            this.chkBits.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkBits.CheckedChanged += new System.EventHandler(this.chkBits_CheckedChanged);
            // 
            // chkTopMost
            // 
            this.chkTopMost.AutoSize = true;
            this.chkTopMost.BackColor = System.Drawing.Color.Transparent;
            this.chkTopMost.BorderColor = System.Drawing.Color.Transparent;
            this.chkTopMost.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkTopMost.ForeColor = System.Drawing.Color.Black;
            this.chkTopMost.Location = new System.Drawing.Point(643, 5);
            this.chkTopMost.Name = "chkTopMost";
            this.chkTopMost.Padding = new System.Windows.Forms.Padding(1);
            this.chkTopMost.Size = new System.Drawing.Size(81, 22);
            this.chkTopMost.TabIndex = 18;
            this.chkTopMost.Text = "TopMost";
            this.c1ThemeController1.SetTheme(this.chkTopMost, "(default)");
            this.chkTopMost.UseVisualStyleBackColor = true;
            this.chkTopMost.Value = null;
            this.chkTopMost.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkTopMost.CheckedChanged += new System.EventHandler(this.chkTopMost_CheckedChanged);
            // 
            // hexBox
            // 
            this.hexBox.AllowDrop = true;
            this.hexBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.hexBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.hexBox.ColumnInfoVisible = true;
            this.hexBox.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hexBox.LineInfoVisible = true;
            this.hexBox.Location = new System.Drawing.Point(0, 32);
            this.hexBox.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.hexBox.Name = "hexBox";
            this.hexBox.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hexBox.Size = new System.Drawing.Size(757, 679);
            this.hexBox.StringViewVisible = true;
            this.hexBox.TabIndex = 8;
            this.hexBox.UseFixedBytesPerLine = true;
            this.hexBox.VScrollBarVisible = true;
            this.hexBox.DragDrop += new System.Windows.Forms.DragEventHandler(this.hexBox_DragDrop);
            this.hexBox.DragEnter += new System.Windows.Forms.DragEventHandler(this.hexBox_DragEnter);
            this.hexBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.hexBox_KeyDown);
            this.hexBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.hexBox_MouseDown);
            // 
            // BlobViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(757, 732);
            this.Controls.Add(this.chkTopMost);
            this.Controls.Add(this.nudFontSize);
            this.Controls.Add(this.chkReadOnly);
            this.Controls.Add(this.lblEncoding2);
            this.Controls.Add(this.chkBits);
            this.Controls.Add(this.lblBytes);
            this.Controls.Add(this.lblCol);
            this.Controls.Add(this.lblLn);
            this.Controls.Add(this.bitControl1);
            this.Controls.Add(this.c1StatusBar1);
            this.Controls.Add(this.hexBox);
            this.Controls.Add(this.toolStrip);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "BlobViewerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Blob Viewer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkReadOnly)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudFontSize)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkBits)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkTopMost)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton btnSave;
        private System.Windows.Forms.ToolStripButton btnOpenFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripLabel lblEncoding;
        private System.Windows.Forms.ToolStripComboBox cboEncoding;
        private JasonLibrary.UI.Controls.HexBox.HexBox hexBox;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private C1.Win.C1Ribbon.RibbonLabel lblFileSize;
        private C1.Win.C1Ribbon.RibbonLabel lblPosition;
        private C1.Win.C1Ribbon.C1StatusBar c1StatusBar1;
        private JasonLibrary.UI.Controls.HexBox.BitControl bitControl1;
        private C1.Win.C1Ribbon.RibbonLabel lblBitPosition;
        private C1.Win.C1Ribbon.RibbonSeparator sp1;
        private C1.Win.C1Ribbon.RibbonSeparator sp2;
        private System.Windows.Forms.Label lblLn;
        private System.Windows.Forms.Label lblCol;
        private System.Windows.Forms.Label lblBytes;
        private System.Windows.Forms.ToolStripLabel lblFontSize;
        private C1.Win.C1Input.C1CheckBox chkBits;
        private System.Windows.Forms.Label lblEncoding2;
        private C1.Win.C1Input.C1CheckBox chkReadOnly;
        private System.Windows.Forms.NumericUpDown nudFontSize;
        private C1.Win.C1Input.C1CheckBox chkTopMost;
        private System.Windows.Forms.ToolStripButton btnSaveAs;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
    }
}