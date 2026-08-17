using JasonLibrary;

namespace JasonQuery.UI.Forms
{
    sealed partial class CellViewerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CellViewerForm));
            this.tsTool = new System.Windows.Forms.ToolStrip();
            this.lblColumnName = new System.Windows.Forms.ToolStripLabel();
            this.lblColumnName2 = new System.Windows.Forms.ToolStripLabel();
            this.lblSpaceColumnName = new System.Windows.Forms.ToolStripLabel();
            this.lblColumnType = new System.Windows.Forms.ToolStripLabel();
            this.lblColumnType2 = new System.Windows.Forms.ToolStripLabel();
            this.lblSpaceColumnType = new System.Windows.Forms.ToolStripLabel();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripLabel3 = new System.Windows.Forms.ToolStripLabel();
            this.btnSelectAll = new System.Windows.Forms.ToolStripButton();
            this.btnCopy = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap = new System.Windows.Forms.ToolStripButton();
            this.btnWordWrap2 = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters = new System.Windows.Forms.ToolStripButton();
            this.btnShowAllCharacters2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.btnFirst = new System.Windows.Forms.ToolStripButton();
            this.btnPrevious = new System.Windows.Forms.ToolStripButton();
            this.btnNext = new System.Windows.Forms.ToolStripButton();
            this.btnLast = new System.Windows.Forms.ToolStripButton();
            this.spViewDataAsHex = new System.Windows.Forms.ToolStripSeparator();
            this.btnViewDataAsHex = new System.Windows.Forms.ToolStripButton();
            this.editor = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.tsTool.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            this.SuspendLayout();
            // 
            // tsTool
            // 
            this.tsTool.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.tsTool.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.lblColumnName,
            this.lblColumnName2,
            this.lblSpaceColumnName,
            this.lblColumnType,
            this.lblColumnType2,
            this.lblSpaceColumnType,
            this.toolStripSeparator1,
            this.toolStripLabel3,
            this.btnSelectAll,
            this.btnCopy,
            this.btnWordWrap,
            this.btnWordWrap2,
            this.btnShowAllCharacters,
            this.btnShowAllCharacters2,
            this.toolStripSeparator2,
            this.btnFirst,
            this.btnPrevious,
            this.btnNext,
            this.btnLast,
            this.spViewDataAsHex,
            this.btnViewDataAsHex});
            this.tsTool.Location = new System.Drawing.Point(0, 0);
            this.tsTool.Name = "tsTool";
            this.tsTool.Size = new System.Drawing.Size(911, 25);
            this.tsTool.TabIndex = 0;
            this.tsTool.Text = "toolStrip1";
            this.c1ThemeController1.SetTheme(this.tsTool, "(default)");
            // 
            // lblColumnName
            // 
            this.lblColumnName.Name = "lblColumnName";
            this.lblColumnName.Size = new System.Drawing.Size(92, 22);
            this.lblColumnName.Text = "Column Name:";
            // 
            // lblColumnName2
            // 
            this.lblColumnName2.ForeColor = System.Drawing.Color.Blue;
            this.lblColumnName2.Name = "lblColumnName2";
            this.lblColumnName2.Size = new System.Drawing.Size(86, 22);
            this.lblColumnName2.Text = "ColumnName";
            // 
            // lblSpaceColumnName
            // 
            this.lblSpaceColumnName.Font = new System.Drawing.Font("Microsoft Sans Serif", 5F);
            this.lblSpaceColumnName.Name = "lblSpaceColumnName";
            this.lblSpaceColumnName.Size = new System.Drawing.Size(8, 22);
            this.lblSpaceColumnName.Text = "  ";
            // 
            // lblColumnType
            // 
            this.lblColumnType.Name = "lblColumnType";
            this.lblColumnType.Size = new System.Drawing.Size(85, 22);
            this.lblColumnType.Text = "Column Type:";
            // 
            // lblColumnType2
            // 
            this.lblColumnType2.ForeColor = System.Drawing.Color.Blue;
            this.lblColumnType2.Name = "lblColumnType2";
            this.lblColumnType2.Size = new System.Drawing.Size(79, 22);
            this.lblColumnType2.Text = "ColumnType";
            // 
            // lblSpaceColumnType
            // 
            this.lblSpaceColumnType.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpaceColumnType.Name = "lblSpaceColumnType";
            this.lblSpaceColumnType.Size = new System.Drawing.Size(5, 22);
            this.lblSpaceColumnType.Text = "  ";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripLabel3
            // 
            this.toolStripLabel3.Font = new System.Drawing.Font("Microsoft Sans Serif", 3.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripLabel3.Name = "toolStripLabel3";
            this.toolStripLabel3.Size = new System.Drawing.Size(4, 22);
            this.toolStripLabel3.Text = " ";
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSelectAll.Image = ((System.Drawing.Image)(resources.GetObject("btnSelectAll.Image")));
            this.btnSelectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(23, 22);
            this.btnSelectAll.Text = "toolStripButton1";
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnCopy
            // 
            this.btnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCopy.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy.Image")));
            this.btnCopy.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(23, 22);
            this.btnCopy.Text = "toolStripButton1";
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // btnWordWrap
            // 
            this.btnWordWrap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap.Image")));
            this.btnWordWrap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap.Name = "btnWordWrap";
            this.btnWordWrap.Size = new System.Drawing.Size(23, 22);
            this.btnWordWrap.ToolTipText = "Word Wrap";
            this.btnWordWrap.Click += new System.EventHandler(this.btnWordWrap_Click);
            // 
            // btnWordWrap2
            // 
            this.btnWordWrap2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnWordWrap2.Image = ((System.Drawing.Image)(resources.GetObject("btnWordWrap2.Image")));
            this.btnWordWrap2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnWordWrap2.Name = "btnWordWrap2";
            this.btnWordWrap2.Size = new System.Drawing.Size(23, 22);
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
            this.btnShowAllCharacters.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters.ToolTipText = "Show All Characters";
            this.btnShowAllCharacters.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // btnShowAllCharacters2
            // 
            this.btnShowAllCharacters2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnShowAllCharacters2.Image = ((System.Drawing.Image)(resources.GetObject("btnShowAllCharacters2.Image")));
            this.btnShowAllCharacters2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnShowAllCharacters2.Name = "btnShowAllCharacters2";
            this.btnShowAllCharacters2.Size = new System.Drawing.Size(23, 22);
            this.btnShowAllCharacters2.ToolTipText = "Show All Characters";
            this.btnShowAllCharacters2.Visible = false;
            this.btnShowAllCharacters2.Click += new System.EventHandler(this.btnShowAllCharacters_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // btnFirst
            // 
            this.btnFirst.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnFirst.Image = ((System.Drawing.Image)(resources.GetObject("btnFirst.Image")));
            this.btnFirst.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(23, 22);
            this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnPrevious.Image = ((System.Drawing.Image)(resources.GetObject("btnPrevious.Image")));
            this.btnPrevious.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(23, 22);
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // btnNext
            // 
            this.btnNext.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnNext.Image = ((System.Drawing.Image)(resources.GetObject("btnNext.Image")));
            this.btnNext.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(23, 22);
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnLast
            // 
            this.btnLast.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLast.Image = ((System.Drawing.Image)(resources.GetObject("btnLast.Image")));
            this.btnLast.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(23, 22);
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // spViewDataAsHex
            // 
            this.spViewDataAsHex.Name = "spViewDataAsHex";
            this.spViewDataAsHex.Size = new System.Drawing.Size(6, 25);
            // 
            // btnViewDataAsHex
            // 
            this.btnViewDataAsHex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnViewDataAsHex.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDataAsHex.Image")));
            this.btnViewDataAsHex.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnViewDataAsHex.Name = "btnViewDataAsHex";
            this.btnViewDataAsHex.Size = new System.Drawing.Size(23, 22);
            this.btnViewDataAsHex.Click += new System.EventHandler(this.btnViewDataAsHex_Click);
            // 
            // editor
            // 
            this.editor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editor.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editor.CaretLineVisible = true;
            this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editor.Location = new System.Drawing.Point(0, 25);
            this.editor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(911, 236);
            this.editor.Styler = null;
            this.editor.TabIndex = 42;
            this.editor.Tag = "";
            this.editor.WhitespaceSize = 3;
            this.editor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editor.DoubleClick += new System.EventHandler<ScintillaNET.DoubleClickEventArgs>(this.editor_DoubleClick);
            this.editor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editor_MouseDown);
            // 
            // CellViewerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(911, 261);
            this.Controls.Add(this.editor);
            this.Controls.Add(this.tsTool);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(730, 299);
            this.Name = "CellViewerForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cell Viewer";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            this.tsTool.ResumeLayout(false);
            this.tsTool.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsTool;
        private System.Windows.Forms.ToolStripLabel lblColumnType;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private JasonLibrary.UI.Controls.ScintillaEditor editor;
        private System.Windows.Forms.ToolStripButton btnWordWrap;
        private System.Windows.Forms.ToolStripButton btnWordWrap2;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters;
        private System.Windows.Forms.ToolStripButton btnShowAllCharacters2;
        private System.Windows.Forms.ToolStripLabel lblColumnName;
        private System.Windows.Forms.ToolStripLabel lblColumnName2;
        private System.Windows.Forms.ToolStripLabel lblColumnType2;
        private System.Windows.Forms.ToolStripButton btnCopy;
        private System.Windows.Forms.ToolStripButton btnSelectAll;
        private System.Windows.Forms.ToolStripLabel lblSpaceColumnName;
        private System.Windows.Forms.ToolStripLabel lblSpaceColumnType;
        private System.Windows.Forms.ToolStripLabel toolStripLabel3;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private System.Windows.Forms.ToolStripSeparator spViewDataAsHex;
        private System.Windows.Forms.ToolStripButton btnViewDataAsHex;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnFirst;
        private System.Windows.Forms.ToolStripButton btnPrevious;
        private System.Windows.Forms.ToolStripButton btnNext;
        private System.Windows.Forms.ToolStripButton btnLast;
    }
}