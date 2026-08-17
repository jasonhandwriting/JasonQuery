namespace JasonQuery.UI.Forms
{
    sealed partial class FileSplitterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FileSplitterForm));
            this.lblDestinationFolder = new System.Windows.Forms.Label();
            this.lblOriginalFile = new System.Windows.Forms.Label();
            this.lblLineBreakSymbol = new System.Windows.Forms.Label();
            this.lblSizeOfAPieceFile = new System.Windows.Forms.Label();
            this.lblFileSize0 = new System.Windows.Forms.Label();
            this.lblSaveAsEncoding = new System.Windows.Forms.Label();
            this.txtOriginalFile = new System.Windows.Forms.TextBox();
            this.txtDestinationFolder = new System.Windows.Forms.TextBox();
            this.lblFileSize = new System.Windows.Forms.Label();
            this.txtSplitSize = new System.Windows.Forms.TextBox();
            this.cboEncoding = new System.Windows.Forms.ComboBox();
            this.cboLineBreakSymbol = new System.Windows.Forms.ComboBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.cboSizeLimit = new System.Windows.Forms.ComboBox();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnSplit = new C1.Win.C1Input.C1Button();
            this.btnBrowseFolder = new C1.Win.C1Input.C1Button();
            this.btnBrowseFile = new C1.Win.C1Input.C1Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSplit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFolder)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).BeginInit();
            this.SuspendLayout();
            // 
            // lblDestinationFolder
            // 
            this.lblDestinationFolder.Location = new System.Drawing.Point(18, 74);
            this.lblDestinationFolder.Name = "lblDestinationFolder";
            this.lblDestinationFolder.Size = new System.Drawing.Size(120, 16);
            this.lblDestinationFolder.TabIndex = 1;
            this.lblDestinationFolder.Text = "Destination folder:";
            this.lblDestinationFolder.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblOriginalFile
            // 
            this.lblOriginalFile.Location = new System.Drawing.Point(18, 19);
            this.lblOriginalFile.Name = "lblOriginalFile";
            this.lblOriginalFile.Size = new System.Drawing.Size(120, 16);
            this.lblOriginalFile.TabIndex = 4;
            this.lblOriginalFile.Text = "Original file:";
            this.lblOriginalFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblLineBreakSymbol
            // 
            this.lblLineBreakSymbol.Location = new System.Drawing.Point(18, 137);
            this.lblLineBreakSymbol.Name = "lblLineBreakSymbol";
            this.lblLineBreakSymbol.Size = new System.Drawing.Size(120, 16);
            this.lblLineBreakSymbol.TabIndex = 5;
            this.lblLineBreakSymbol.Text = "Line break symbol:";
            this.lblLineBreakSymbol.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSizeOfAPieceFile
            // 
            this.lblSizeOfAPieceFile.Location = new System.Drawing.Point(18, 169);
            this.lblSizeOfAPieceFile.Name = "lblSizeOfAPieceFile";
            this.lblSizeOfAPieceFile.Size = new System.Drawing.Size(120, 16);
            this.lblSizeOfAPieceFile.TabIndex = 6;
            this.lblSizeOfAPieceFile.Text = "Size of a piece file:";
            this.lblSizeOfAPieceFile.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblFileSize0
            // 
            this.lblFileSize0.Location = new System.Drawing.Point(18, 46);
            this.lblFileSize0.Name = "lblFileSize0";
            this.lblFileSize0.Size = new System.Drawing.Size(120, 16);
            this.lblFileSize0.TabIndex = 7;
            this.lblFileSize0.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblSaveAsEncoding
            // 
            this.lblSaveAsEncoding.Location = new System.Drawing.Point(18, 106);
            this.lblSaveAsEncoding.Name = "lblSaveAsEncoding";
            this.lblSaveAsEncoding.Size = new System.Drawing.Size(120, 16);
            this.lblSaveAsEncoding.TabIndex = 8;
            this.lblSaveAsEncoding.Text = "Save as Encoding:";
            this.lblSaveAsEncoding.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOriginalFile
            // 
            this.txtOriginalFile.AllowDrop = true;
            this.txtOriginalFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtOriginalFile.Location = new System.Drawing.Point(139, 17);
            this.txtOriginalFile.Name = "txtOriginalFile";
            this.txtOriginalFile.Size = new System.Drawing.Size(348, 23);
            this.txtOriginalFile.TabIndex = 2;
            this.txtOriginalFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtSourceFile_DragDrop);
            this.txtOriginalFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtSourceFile_DragEnter);
            // 
            // txtDestinationFolder
            // 
            this.txtDestinationFolder.AllowDrop = true;
            this.txtDestinationFolder.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtDestinationFolder.Location = new System.Drawing.Point(139, 72);
            this.txtDestinationFolder.Name = "txtDestinationFolder";
            this.txtDestinationFolder.Size = new System.Drawing.Size(348, 23);
            this.txtDestinationFolder.TabIndex = 4;
            this.txtDestinationFolder.DragDrop += new System.Windows.Forms.DragEventHandler(this.txtDestinationFolder_DragDrop);
            this.txtDestinationFolder.DragEnter += new System.Windows.Forms.DragEventHandler(this.txtDestinationFolder_DragEnter);
            // 
            // lblFileSize
            // 
            this.lblFileSize.AutoSize = true;
            this.lblFileSize.Location = new System.Drawing.Point(142, 47);
            this.lblFileSize.Name = "lblFileSize";
            this.lblFileSize.Size = new System.Drawing.Size(0, 16);
            this.lblFileSize.TabIndex = 14;
            this.lblFileSize.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtSplitSize
            // 
            this.txtSplitSize.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.txtSplitSize.Location = new System.Drawing.Point(139, 167);
            this.txtSplitSize.MaxLength = 3;
            this.txtSplitSize.Name = "txtSplitSize";
            this.txtSplitSize.Size = new System.Drawing.Size(28, 23);
            this.txtSplitSize.TabIndex = 5;
            this.txtSplitSize.Text = "10";
            this.txtSplitSize.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtSplitSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtSplitSize_KeyPress);
            this.txtSplitSize.Leave += new System.EventHandler(this.txtSplitSize_Leave);
            // 
            // cboEncoding
            // 
            this.cboEncoding.Enabled = false;
            this.cboEncoding.FormattingEnabled = true;
            this.cboEncoding.Location = new System.Drawing.Point(139, 104);
            this.cboEncoding.Name = "cboEncoding";
            this.cboEncoding.Size = new System.Drawing.Size(61, 24);
            this.cboEncoding.TabIndex = 17;
            this.cboEncoding.Text = "UTF-8";
            // 
            // cboLineBreakSymbol
            // 
            this.cboLineBreakSymbol.Enabled = false;
            this.cboLineBreakSymbol.FormattingEnabled = true;
            this.cboLineBreakSymbol.Location = new System.Drawing.Point(139, 135);
            this.cboLineBreakSymbol.Name = "cboLineBreakSymbol";
            this.cboLineBreakSymbol.Size = new System.Drawing.Size(118, 24);
            this.cboLineBreakSymbol.TabIndex = 18;
            this.cboLineBreakSymbol.Text = "Windows (CR LF)";
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Location = new System.Drawing.Point(25, 195);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(298, 16);
            this.lblInfo.TabIndex = 19;
            this.lblInfo.Text = "(Split based on \"File Size\" and \"Line break symbol\" )";
            this.lblInfo.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // cboSizeLimit
            // 
            this.cboSizeLimit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.cboSizeLimit.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSizeLimit.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.cboSizeLimit.ForeColor = System.Drawing.Color.Black;
            this.cboSizeLimit.FormattingEnabled = true;
            this.cboSizeLimit.Items.AddRange(new object[] {
            "MBytes",
            "KBytes"});
            this.cboSizeLimit.Location = new System.Drawing.Point(172, 166);
            this.cboSizeLimit.Name = "cboSizeLimit";
            this.cboSizeLimit.Size = new System.Drawing.Size(65, 24);
            this.cboSizeLimit.TabIndex = 6;
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(522, 181);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(77, 29);
            this.btnClose.TabIndex = 60;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnSplit
            // 
            this.btnSplit.Location = new System.Drawing.Point(425, 181);
            this.btnSplit.Name = "btnSplit";
            this.btnSplit.Size = new System.Drawing.Size(77, 29);
            this.btnSplit.TabIndex = 61;
            this.btnSplit.Text = "Split";
            this.btnSplit.UseVisualStyleBackColor = true;
            this.btnSplit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSplit.Click += new System.EventHandler(this.btnSplit_Click);
            // 
            // btnBrowseFolder
            // 
            this.btnBrowseFolder.Location = new System.Drawing.Point(494, 71);
            this.btnBrowseFolder.Name = "btnBrowseFolder";
            this.btnBrowseFolder.Size = new System.Drawing.Size(105, 26);
            this.btnBrowseFolder.TabIndex = 62;
            this.btnBrowseFolder.Text = "Browse Folder";
            this.btnBrowseFolder.UseVisualStyleBackColor = true;
            this.btnBrowseFolder.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFolder.Click += new System.EventHandler(this.btnBrowseFolder_Click);
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Location = new System.Drawing.Point(494, 16);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new System.Drawing.Size(105, 26);
            this.btnBrowseFile.TabIndex = 63;
            this.btnBrowseFile.Text = "Browse File";
            this.btnBrowseFile.UseVisualStyleBackColor = true;
            this.btnBrowseFile.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(295, 46);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(191, 19);
            this.progressBar1.TabIndex = 64;
            this.progressBar1.Visible = false;
            // 
            // FileSplitterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(614, 229);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.btnBrowseFile);
            this.Controls.Add(this.btnBrowseFolder);
            this.Controls.Add(this.btnSplit);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.cboSizeLimit);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.cboLineBreakSymbol);
            this.Controls.Add(this.cboEncoding);
            this.Controls.Add(this.txtSplitSize);
            this.Controls.Add(this.lblFileSize);
            this.Controls.Add(this.txtDestinationFolder);
            this.Controls.Add(this.txtOriginalFile);
            this.Controls.Add(this.lblSaveAsEncoding);
            this.Controls.Add(this.lblFileSize0);
            this.Controls.Add(this.lblSizeOfAPieceFile);
            this.Controls.Add(this.lblLineBreakSymbol);
            this.Controls.Add(this.lblOriginalFile);
            this.Controls.Add(this.lblDestinationFolder);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FileSplitterForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Large Text File Splitter";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSplit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFolder)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblDestinationFolder;
        private System.Windows.Forms.Label lblOriginalFile;
        private System.Windows.Forms.Label lblLineBreakSymbol;
        private System.Windows.Forms.Label lblSizeOfAPieceFile;
        private System.Windows.Forms.Label lblFileSize0;
        private System.Windows.Forms.Label lblSaveAsEncoding;
        private System.Windows.Forms.TextBox txtOriginalFile;
        private System.Windows.Forms.TextBox txtDestinationFolder;
        private System.Windows.Forms.Label lblFileSize;
        private System.Windows.Forms.TextBox txtSplitSize;
        private System.Windows.Forms.ComboBox cboEncoding;
        private System.Windows.Forms.ComboBox cboLineBreakSymbol;
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.ComboBox cboSizeLimit;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnSplit;
        private C1.Win.C1Input.C1Button btnBrowseFolder;
        private C1.Win.C1Input.C1Button btnBrowseFile;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}