namespace JasonQuery.UI.Forms
{
    sealed partial class ConnectionImportForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionImportForm));
            this.txtEncryptPassword = new C1.Win.C1Input.C1TextBox();
            this.lblPassword = new System.Windows.Forms.Label();
            this.btnBrowseFile = new C1.Win.C1Input.C1Button();
            this.txtFileName = new C1.Win.C1Input.C1TextBox();
            this.lblFileName = new System.Windows.Forms.Label();
            this.lblRemember = new System.Windows.Forms.Label();
            this.btnImport = new C1.Win.C1Input.C1Button();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.grpImportFrom = new System.Windows.Forms.GroupBox();
            this.btnEncryptPasswordView = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnImport)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            this.grpImportFrom.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).BeginInit();
            this.SuspendLayout();
            // 
            // txtEncryptPassword
            // 
            this.txtEncryptPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtEncryptPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEncryptPassword.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtEncryptPassword.Location = new System.Drawing.Point(78, 55);
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
            this.lblPassword.Location = new System.Drawing.Point(12, 57);
            this.lblPassword.Name = "lblPassword";
            this.lblPassword.Size = new System.Drawing.Size(63, 16);
            this.lblPassword.TabIndex = 87;
            this.lblPassword.Text = "Password:";
            this.lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Location = new System.Drawing.Point(514, 27);
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
            this.txtFileName.Location = new System.Drawing.Point(84, 27);
            this.txtFileName.Name = "txtFileName";
            this.txtFileName.ReadOnly = true;
            this.txtFileName.Size = new System.Drawing.Size(396, 21);
            this.txtFileName.TabIndex = 109;
            this.txtFileName.Tag = null;
            this.txtFileName.TextDetached = true;
            this.txtFileName.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // lblFileName
            // 
            this.lblFileName.AutoSize = true;
            this.lblFileName.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblFileName.Location = new System.Drawing.Point(12, 29);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(67, 16);
            this.lblFileName.TabIndex = 110;
            this.lblFileName.Text = "File Name:";
            this.lblFileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblRemember
            // 
            this.lblRemember.AutoSize = true;
            this.lblRemember.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemember.Location = new System.Drawing.Point(12, 84);
            this.lblRemember.Name = "lblRemember";
            this.lblRemember.Size = new System.Drawing.Size(269, 16);
            this.lblRemember.TabIndex = 90;
            this.lblRemember.Text = "(Remember that passwords are case-sensitive.)";
            this.lblRemember.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnImport
            // 
            this.btnImport.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnImport.Location = new System.Drawing.Point(582, 21);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(73, 54);
            this.btnImport.TabIndex = 115;
            this.btnImport.Text = "Import";
            this.btnImport.UseVisualStyleBackColor = true;
            this.btnImport.VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnImport.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
            // 
            // btnClose
            // 
            this.btnClose.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnClose.Location = new System.Drawing.Point(582, 94);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(73, 54);
            this.btnClose.TabIndex = 116;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // grpImportFrom
            // 
            this.grpImportFrom.Controls.Add(this.btnEncryptPasswordView);
            this.grpImportFrom.Controls.Add(this.lblRemember);
            this.grpImportFrom.Controls.Add(this.lblFileName);
            this.grpImportFrom.Controls.Add(this.lblPassword);
            this.grpImportFrom.Controls.Add(this.btnBrowseFile);
            this.grpImportFrom.Controls.Add(this.txtEncryptPassword);
            this.grpImportFrom.Controls.Add(this.txtFileName);
            this.grpImportFrom.Location = new System.Drawing.Point(12, 12);
            this.grpImportFrom.Name = "grpImportFrom";
            this.grpImportFrom.Size = new System.Drawing.Size(557, 137);
            this.grpImportFrom.TabIndex = 118;
            this.grpImportFrom.TabStop = false;
            this.grpImportFrom.Text = "Import From";
            // 
            // btnEncryptPasswordView
            // 
            this.btnEncryptPasswordView.Image = ((System.Drawing.Image)(resources.GetObject("btnEncryptPasswordView.Image")));
            this.btnEncryptPasswordView.Location = new System.Drawing.Point(268, 55);
            this.btnEncryptPasswordView.Name = "btnEncryptPasswordView";
            this.btnEncryptPasswordView.Size = new System.Drawing.Size(21, 21);
            this.btnEncryptPasswordView.TabIndex = 142;
            this.btnEncryptPasswordView.UseVisualStyleBackColor = true;
            this.btnEncryptPasswordView.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEncryptPasswordView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseDown);
            this.btnEncryptPasswordView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseUp);
            // 
            // ConnectionImportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(670, 165);
            this.Controls.Add(this.grpImportFrom);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnImport);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConnectionImportForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Import Connection Information";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtFileName)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnImport)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            this.grpImportFrom.ResumeLayout(false);
            this.grpImportFrom.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private C1.Win.C1Input.C1TextBox txtEncryptPassword;
        private System.Windows.Forms.Label lblPassword;
        private C1.Win.C1Input.C1Button btnBrowseFile;
        private C1.Win.C1Input.C1TextBox txtFileName;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.Label lblRemember;
        private C1.Win.C1Input.C1Button btnImport;
        private C1.Win.C1Input.C1Button btnClose;
        private System.Windows.Forms.GroupBox grpImportFrom;
        private C1.Win.C1Input.C1Button btnEncryptPasswordView;
    }
}