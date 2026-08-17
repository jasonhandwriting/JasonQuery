using JasonLibrary;
using JasonLibrary.UI.Controls;

namespace JasonQuery.UI.Forms
{
    sealed partial class CellEditorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CellEditorForm));
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
            this.btnSetNull = new System.Windows.Forms.ToolStripButton();
            this.btnViewDataAsHex = new System.Windows.Forms.ToolStripButton();
            this.btnLoadFileToBlobField = new System.Windows.Forms.ToolStripButton();
            this.btnSaveBlobToFile = new System.Windows.Forms.ToolStripButton();
            this.btnApplyEdit = new System.Windows.Forms.ToolStripButton();
            this.btnCancelEdit = new System.Windows.Forms.ToolStripButton();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.c1StatusBar1 = new C1.Win.Ribbon.C1StatusBar();
            this.lblLength = new C1.Win.Ribbon.RibbonLabel();
            this.lblLength2 = new C1.Win.Ribbon.RibbonLabel();
            this.editorCellEditor = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.tmrUpdateText = new System.Windows.Forms.Timer(this.components);
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.tsTool.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).BeginInit();
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
            this.btnSetNull,
            this.btnLoadFileToBlobField,
            this.btnSaveBlobToFile,
            this.btnViewDataAsHex,
            this.toolStripSeparator3,
            this.btnApplyEdit,
            this.btnCancelEdit});
            this.tsTool.Location = new System.Drawing.Point(0, 0);
            this.tsTool.Name = "tsTool";
            this.tsTool.Size = new System.Drawing.Size(714, 25);
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
            // btnSetNull
            // 
            this.btnSetNull.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSetNull.Image = ((System.Drawing.Image)(resources.GetObject("btnSetNull.Image")));
            this.btnSetNull.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSetNull.Name = "btnSetNull";
            this.btnSetNull.Size = new System.Drawing.Size(23, 22);
            this.btnSetNull.Click += new System.EventHandler(this.btnSetNull_Click);
            // 
            // btnViewDataAsHex
            // 
            this.btnViewDataAsHex.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnViewDataAsHex.Enabled = false;
            this.btnViewDataAsHex.Image = ((System.Drawing.Image)(resources.GetObject("btnViewDataAsHex.Image")));
            this.btnViewDataAsHex.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnViewDataAsHex.Name = "btnViewDataAsHex";
            this.btnViewDataAsHex.Size = new System.Drawing.Size(23, 22);
            this.btnViewDataAsHex.Click += new System.EventHandler(this.btnViewDataAsHex_Click);
            // 
            // btnLoadFileToBlobField
            // 
            this.btnLoadFileToBlobField.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnLoadFileToBlobField.Enabled = false;
            this.btnLoadFileToBlobField.Image = ((System.Drawing.Image)(resources.GetObject("btnLoadFileToBlobField.Image")));
            this.btnLoadFileToBlobField.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnLoadFileToBlobField.Name = "btnLoadFileToBlobField";
            this.btnLoadFileToBlobField.Size = new System.Drawing.Size(23, 22);
            this.btnLoadFileToBlobField.Click += new System.EventHandler(this.btnLoadFileToBlobField_Click);
            // 
            // btnSaveBlobToFile
            // 
            this.btnSaveBlobToFile.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnSaveBlobToFile.Enabled = false;
            this.btnSaveBlobToFile.Image = ((System.Drawing.Image)(resources.GetObject("btnSaveBlobToFile.Image")));
            this.btnSaveBlobToFile.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnSaveBlobToFile.Name = "btnSaveBlobToFile";
            this.btnSaveBlobToFile.Size = new System.Drawing.Size(23, 22);
            this.btnSaveBlobToFile.Click += new System.EventHandler(this.btnSaveBlobToFile_Click);
            // 
            // btnApplyEdit
            // 
            this.btnApplyEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnApplyEdit.Image = ((System.Drawing.Image)(resources.GetObject("btnApplyEdit.Image")));
            this.btnApplyEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnApplyEdit.Name = "btnApplyEdit";
            this.btnApplyEdit.Size = new System.Drawing.Size(23, 22);
            this.btnApplyEdit.Tag = "0";
            this.btnApplyEdit.Click += new System.EventHandler(this.btnApplyEdit_Click);
            // 
            // btnCancelEdit
            // 
            this.btnCancelEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.btnCancelEdit.Image = ((System.Drawing.Image)(resources.GetObject("btnCancelEdit.Image")));
            this.btnCancelEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.btnCancelEdit.Name = "btnCancelEdit";
            this.btnCancelEdit.Size = new System.Drawing.Size(23, 22);
            this.btnCancelEdit.Tag = "0";
            this.btnCancelEdit.Click += new System.EventHandler(this.btnCancelEdit_Click);
            // 
            // c1StatusBar1
            // 
            this.c1StatusBar1.LeftPaneItems.Add(this.lblLength);
            this.c1StatusBar1.LeftPaneItems.Add(this.lblLength2);
            this.c1StatusBar1.Location = new System.Drawing.Point(0, 239);
            this.c1StatusBar1.Name = "c1StatusBar1";
            this.c1StatusBar1.Size = new System.Drawing.Size(714, 22);
            this.c1ThemeController1.SetTheme(this.c1StatusBar1, "(default)");
            // 
            // lblLength
            // 
            this.lblLength.Name = "lblLength";
            this.lblLength.Text = "Length:";
            // 
            // lblLength2
            // 
            this.lblLength2.Name = "lblLength2";
            // 
            // editorCellEditor
            // 
            this.editorCellEditor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorCellEditor.CaretLineBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.editorCellEditor.CaretLineVisible = true;
            this.editorCellEditor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editorCellEditor.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorCellEditor.Location = new System.Drawing.Point(0, 25);
            this.editorCellEditor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorCellEditor.Name = "editorCellEditor";
            this.editorCellEditor.Size = new System.Drawing.Size(714, 214);
            this.editorCellEditor.Styler = null;
            this.editorCellEditor.TabIndex = 42;
            this.editorCellEditor.Tag = "";
            this.editorCellEditor.WhitespaceSize = 3;
            this.editorCellEditor.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorCellEditor.BeforeInsert += new System.EventHandler<ScintillaNET.BeforeModificationEventArgs>(this.editorCellEditor_BeforeInsert);
            this.editorCellEditor.TextChanged += new System.EventHandler(this.editorCellEditor_TextChanged);
            this.editorCellEditor.KeyDown += new System.Windows.Forms.KeyEventHandler(this.editorCellEditor_KeyDown);
            this.editorCellEditor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.editorCellEditor_KeyPress);
            this.editorCellEditor.MouseDown += new System.Windows.Forms.MouseEventHandler(this.editorCellViewer_MouseDown);
            // 
            // tmrUpdateText
            // 
            this.tmrUpdateText.Interval = 200;
            this.tmrUpdateText.Tick += new System.EventHandler(this.tmrUpdateText_Tick);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // CellEditorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(714, 261);
            this.Controls.Add(this.editorCellEditor);
            this.Controls.Add(this.tsTool);
            this.Controls.Add(this.c1StatusBar1);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(730, 300);
            this.Name = "CellEditorForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Cell Editor";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            this.ResizeEnd += new System.EventHandler(this.Form_ResizeEnd);
            this.tsTool.ResumeLayout(false);
            this.tsTool.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1StatusBar1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip tsTool;
        private System.Windows.Forms.ToolStripLabel lblColumnType;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
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
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton btnApplyEdit;
        private System.Windows.Forms.ToolStripButton btnCancelEdit;
        private System.Windows.Forms.ToolStripButton btnSetNull;
        private JasonLibrary.UI.Controls.ScintillaEditor editorCellEditor;
        private System.Windows.Forms.Timer tmrUpdateText;
        private C1.Win.Ribbon.C1StatusBar c1StatusBar1;
        private C1.Win.Ribbon.RibbonLabel lblLength;
        private C1.Win.Ribbon.RibbonLabel lblLength2;
        private System.Windows.Forms.ToolStripButton btnLoadFileToBlobField;
        private System.Windows.Forms.ToolStripButton btnSaveBlobToFile;
        private System.Windows.Forms.ToolStripButton btnViewDataAsHex;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
    }
}