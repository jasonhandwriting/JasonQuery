namespace JasonQuery.UI.Forms
{
    sealed partial class ConnectionExportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionExportForm));
            this.txtEncryptPassword = new C1.Win.C1Input.C1TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnBrowseFile = new C1.Win.C1Input.C1Button();
            this.txtFileName = new C1.Win.C1Input.C1TextBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.chkIncludeDBPassword = new C1.Win.C1Input.C1CheckBox();
            this.grpEncrypt = new System.Windows.Forms.GroupBox();
            this.btnEncryptPasswordView = new C1.Win.C1Input.C1Button();
            this.btnHelp_Password = new C1.Win.C1Input.C1Button();
            this.chkEncrypt = new C1.Win.C1Input.C1CheckBox();
            this.lblRemember = new System.Windows.Forms.Label();
            this.lblCaution = new System.Windows.Forms.Label();
            this.btnExport = new C1.Win.C1Input.C1Button();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.grpExportTo = new System.Windows.Forms.GroupBox();
            this.c1GridDbInfo = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.btnUnselectAll = new C1.Win.C1Input.C1Button();
            this.btnSelectAll = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeDBPassword)).BeginInit();
            this.grpEncrypt.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Password)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEncrypt)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.grpExportTo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridDbInfo)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).BeginInit();
            this.SuspendLayout();
            // 
            // txtEncryptPassword
            // 
            this.txtEncryptPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtEncryptPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEncryptPassword.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtEncryptPassword.Location = new System.Drawing.Point(82, 26);
            this.txtEncryptPassword.MaxLength = 25;
            this.txtEncryptPassword.Name = "txtEncryptPassword";
            this.txtEncryptPassword.PasswordChar = '*';
            this.txtEncryptPassword.ShortcutsEnabled = false;
            this.txtEncryptPassword.ShowContextMenu = false;
            this.txtEncryptPassword.Size = new System.Drawing.Size(180, 21);
            this.txtEncryptPassword.TabIndex = 88;
            this.txtEncryptPassword.Tag = null;
            this.txtEncryptPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtEncryptPassword.TextChanged += new System.EventHandler(this.txtEncryptPassword_TextChanged);
            this.txtEncryptPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEncryptPassword_KeyDown);
            // 
            // lblPassword
            // 
            this.lblPassword.AutoSize = true;
            this.lblPassword.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblPassword.Location = new System.Drawing.Point(16, 28);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(63, 16);
            this.lblPassword.TabIndex = 87;
            this.lblPassword.Text = "Password:";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Location = new System.Drawing.Point(540, 25);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new System.Drawing.Size(21, 21);
            this.btnBrowseFile.TabIndex = 111;
            this.btnBrowseFile.Text = "...";
            this.btnBrowseFile.UseVisualStyleBackColor = true;
            this.btnBrowseFile.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
            // 
            // txtFileName
            // 
            this.txtFileName.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.txtFileName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtFileName.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtFileName.Location = new System.Drawing.Point(145, 25);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(528, 21);
            this.txtFileName.TabIndex = 109;
            this.txtFileName.Tag = null;
            this.txtFileName.TextDetached = true;
            this.txtFileName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblFileName.Location = new System.Drawing.Point(12, 27);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(67, 16);
            this.lblFileName.TabIndex = 110;
            this.lblFileName.Text = "File Name:";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // chkIncludeDBPassword
            // 
            this.chkIncludeDBPassword.AutoSize = true;
            this.chkIncludeDBPassword.BackColor = System.Drawing.SystemColors.Control;
            this.chkIncludeDBPassword.BorderColor = System.Drawing.Color.Transparent;
            this.chkIncludeDBPassword.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkIncludeDBPassword.ForeColor = System.Drawing.Color.Black;
            this.chkIncludeDBPassword.Location = new System.Drawing.Point(418, -1);
            this.chkIncludeDBPassword.Name = "chkIncludeDBPassword";
            this.chkIncludeDBPassword.Padding = new System.Windows.Forms.Padding(1);
            this.chkIncludeDBPassword.Size = new System.Drawing.Size(193, 22);
            this.chkIncludeDBPassword.TabIndex = 112;
            this.chkIncludeDBPassword.Text = "Include Connection Password";
            this.chkIncludeDBPassword.UseVisualStyleBackColor = false;
            this.chkIncludeDBPassword.Value = null;
            this.chkIncludeDBPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            // 
            // grpEncrypt
            // 
            this.grpEncrypt.BackColor = System.Drawing.SystemColors.Control;
            this.grpEncrypt.Controls.Add(this.btnEncryptPasswordView);
            this.grpEncrypt.Controls.Add(this.btnHelp_Password);
            this.grpEncrypt.Controls.Add(this.chkEncrypt);
            this.grpEncrypt.Controls.Add(this.lblRemember);
            this.grpEncrypt.Controls.Add(this.lblCaution);
            this.grpEncrypt.Controls.Add(this.chkIncludeDBPassword);
            this.grpEncrypt.Controls.Add(this.txtEncryptPassword);
            this.grpEncrypt.Controls.Add(this.lblPassword);
            this.grpEncrypt.Location = new System.Drawing.Point(14, 57);
            this.grpEncrypt.Name = "grpEncrypt";
            this.grpEncrypt.Size = new System.Drawing.Size(659, 100);
            this.grpEncrypt.TabIndex = 114;
            this.grpEncrypt.TabStop = false;
            this.grpEncrypt.Text = "  ";
            // 
            // btnEncryptPasswordView
            // 
            this.btnEncryptPasswordView.Image = ((System.Drawing.Image)(resources.GetObject("btnEncryptPasswordView.Image")));
            this.btnEncryptPasswordView.Location = new System.Drawing.Point(255, 26);
            this.btnEncryptPasswordView.Name = "btnEncryptPasswordView";
            this.btnEncryptPasswordView.Size = new System.Drawing.Size(21, 21);
            this.btnEncryptPasswordView.TabIndex = 141;
            this.btnEncryptPasswordView.UseVisualStyleBackColor = true;
            this.btnEncryptPasswordView.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEncryptPasswordView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseDown);
            this.btnEncryptPasswordView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseUp);
            // 
            // btnHelp_Password
            // 
            this.btnHelp_Password.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Password.Image")));
            this.btnHelp_Password.Location = new System.Drawing.Point(282, 26);
            this.btnHelp_Password.Name = "btnHelp_Password";
            this.btnHelp_Password.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Password.TabIndex = 118;
            this.btnHelp_Password.Tag = "Available characters for password";
            this.btnHelp_Password.UseVisualStyleBackColor = true;
            this.btnHelp_Password.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Password.Click += new System.EventHandler(this.btnHelp_Password_Click);
            // 
            // chkEncrypt
            // 
            this.chkEncrypt.AutoSize = true;
            this.chkEncrypt.BackColor = System.Drawing.SystemColors.Control;
            this.chkEncrypt.BorderColor = System.Drawing.Color.Transparent;
            this.chkEncrypt.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.chkEncrypt.Checked = true;
            this.chkEncrypt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEncrypt.ForeColor = System.Drawing.Color.Black;
            this.chkEncrypt.Location = new System.Drawing.Point(13, -1);
            this.chkEncrypt.Name = "chkEncrypt";
            this.chkEncrypt.Padding = new System.Windows.Forms.Padding(1);
            this.chkEncrypt.Size = new System.Drawing.Size(199, 22);
            this.chkEncrypt.TabIndex = 117;
            this.chkEncrypt.Text = "Encrypt the contents of this file";
            this.chkEncrypt.UseVisualStyleBackColor = false;
            this.chkEncrypt.Value = true;
            this.chkEncrypt.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.chkEncrypt.CheckedChanged += new System.EventHandler(this.chkEncrypt_CheckedChanged);
            // 
            // lblRemember
            // 
            this.lblRemember.AutoSize = true;
            this.lblRemember.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemember.Location = new System.Drawing.Point(16, 74);
            this.lblRemember.Name = "lblRemember";
            this.lblRemember.Size = new System.Drawing.Size(269, 16);
            this.lblRemember.TabIndex = 90;
            this.lblRemember.Text = "(Remember that passwords are case-sensitive.)";
            this.lblRemember.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblCaution
            // 
            this.lblCaution.AutoSize = true;
            this.lblCaution.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCaution.Location = new System.Drawing.Point(16, 53);
            this.lblCaution.Name = "lblCaution";
            this.lblCaution.Size = new System.Drawing.Size(381, 16);
            this.lblCaution.TabIndex = 89;
            this.lblCaution.Text = "Caution: If you lose or forget the password, it cannot be recovered.";
            this.lblCaution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnExport
            // 
            this.btnExport.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnExport.Location = new System.Drawing.Point(530, 189);
            this.btnExport.Name = "btnExport";
            this.btnExport.Size = new System.Drawing.Size(73, 40);
            this.btnExport.TabIndex = 115;
            this.btnExport.Text = "Export";
            this.btnExport.UseVisualStyleBackColor = true;
            this.btnExport.VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnExport.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnExport.Click += new System.EventHandler(this.btnExport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClose.Location = new System.Drawing.Point(627, 189);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(73, 40);
            this.btnClose.TabIndex = 116;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // grpExportTo
            // 
            this.grpExportTo.Controls.Add(this.lblFileName);
            this.grpExportTo.Controls.Add(this.btnBrowseFile);
            this.grpExportTo.Controls.Add(this.grpEncrypt);
            this.grpExportTo.Controls.Add(this.txtFileName);
            this.grpExportTo.Location = new System.Drawing.Point(12, 12);
            this.grpExportTo.Name = "grpExportTo";
            this.grpExportTo.Size = new System.Drawing.Size(688, 170);
            this.grpExportTo.TabIndex = 117;
            this.grpExportTo.TabStop = false;
            this.grpExportTo.Text = "Export To";
            // 
            // c1GridDbInfo
            // 
            this.c1GridDbInfo.AlternatingRows = true;
            this.c1GridDbInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.c1GridDbInfo.CaptionHeight = 19;
            this.c1GridDbInfo.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1GridDbInfo.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1GridDbInfo.Images.Add(((System.Drawing.Image)(resources.GetObject("c1GridDbInfo.Images"))));
            this.c1GridDbInfo.Location = new System.Drawing.Point(12, 237);
            this.c1GridDbInfo.Name = "c1GridDbInfo";
            this.c1GridDbInfo.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1GridDbInfo.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1GridDbInfo.PreviewInfo.ZoomFactor = 75D;
            this.c1GridDbInfo.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1GridDbInfo.PrintInfo.MeasurementPrinterName = null;
            this.c1GridDbInfo.RowHeight = 17;
            this.c1GridDbInfo.Size = new System.Drawing.Size(688, 304);
            this.c1GridDbInfo.TabIndex = 118;
            this.c1GridDbInfo.Text = "c1TrueDBGrid2";
            this.c1GridDbInfo.UseCompatibleTextRendering = false;
            this.c1GridDbInfo.PropBag = resources.GetString("c1GridDbInfo.PropBag");
            // 
            // btnUnselectAll
            // 
            this.btnUnselectAll.AutoSize = true;
            this.btnUnselectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnUnselectAll.Location = new System.Drawing.Point(140, 203);
            this.btnUnselectAll.Name = "btnUnselectAll";
            this.btnUnselectAll.Size = new System.Drawing.Size(91, 26);
            this.btnUnselectAll.TabIndex = 120;
            this.btnUnselectAll.Tag = "UnselectAll";
            this.btnUnselectAll.Text = "&Unselect All";
            this.btnUnselectAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUnselectAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnUnselectAll.UseVisualStyleBackColor = true;
            this.btnUnselectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnUnselectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // btnSelectAll
            // 
            this.btnSelectAll.AutoSize = true;
            this.btnSelectAll.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnSelectAll.Location = new System.Drawing.Point(12, 203);
            this.btnSelectAll.Name = "btnSelectAll";
            this.btnSelectAll.Size = new System.Drawing.Size(76, 26);
            this.btnSelectAll.TabIndex = 119;
            this.btnSelectAll.Tag = "SelectAll";
            this.btnSelectAll.Text = "Select &All";
            this.btnSelectAll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnSelectAll.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSelectAll.UseVisualStyleBackColor = true;
            this.btnSelectAll.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnSelectAll.Click += new System.EventHandler(this.btnSelectAll_Click);
            // 
            // ConnectionExportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(711, 553);
            this.Controls.Add(this.btnUnselectAll);
            this.Controls.Add(this.btnSelectAll);
            this.Controls.Add(this.c1GridDbInfo);
            this.Controls.Add(this.grpExportTo);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnExport);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionExportForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = " Export Connection Information";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkIncludeDBPassword)).EndInit();
            this.grpEncrypt.ResumeLayout(false);
            this.grpEncrypt.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Password)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.chkEncrypt)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.grpExportTo.ResumeLayout(false);
            this.grpExportTo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.c1GridDbInfo)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnUnselectAll)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnSelectAll)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Input.C1TextBox txtEncryptPassword;
        private System.Windows.Forms.Label lblPassword;
        private C1.Win.C1Input.C1Button btnBrowseFile;
        private C1.Win.C1Input.C1TextBox txtFileName;
        private System.Windows.Forms.Label lblFileName;
        private C1.Win.C1Input.C1CheckBox chkIncludeDBPassword;
        private System.Windows.Forms.GroupBox grpEncrypt;
        private System.Windows.Forms.Label lblRemember;
        private System.Windows.Forms.Label lblCaution;
        private C1.Win.C1Input.C1Button btnExport;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1CheckBox chkEncrypt;
        private C1.Win.C1Input.C1Button btnHelp_Password;
        private System.Windows.Forms.GroupBox grpExportTo;
        private C1.Win.C1Input.C1Button btnEncryptPasswordView;
        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1GridDbInfo;
        private C1.Win.C1Input.C1Button btnUnselectAll;
        private C1.Win.C1Input.C1Button btnSelectAll;
    }
}