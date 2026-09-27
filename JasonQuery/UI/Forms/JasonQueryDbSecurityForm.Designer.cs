namespace JasonQuery.UI.Forms
{
    partial class JasonQueryDbSecurityForm
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
            this.lblCurrentProtection = new System.Windows.Forms.Label();
            this.lblCurrentProtectionValue = new System.Windows.Forms.Label();
            this.grpProtection = new System.Windows.Forms.GroupBox();
            this.btnManageRecoveryKey = new C1.Win.C1Input.C1Button();
            this.lblRecoveryStatus = new System.Windows.Forms.Label();
            this.rdoCustomPassword = new System.Windows.Forms.RadioButton();
            this.rdoWindowsProtected = new System.Windows.Forms.RadioButton();
            this.lblCurrentPassword = new System.Windows.Forms.Label();
            this.txtCurrentPassword = new System.Windows.Forms.TextBox();
            this.txtNewCustomPassword = new System.Windows.Forms.TextBox();
            this.lblNewCustomPassword = new System.Windows.Forms.Label();
            this.txtConfirmPassword = new System.Windows.Forms.TextBox();
            this.lblConfirmPassword = new System.Windows.Forms.Label();
            this.lblInfo1 = new System.Windows.Forms.Label();
            this.lblInfo2 = new System.Windows.Forms.Label();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnApply = new C1.Win.C1Input.C1Button();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            this.grpProtection.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnManageRecoveryKey)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            this.SuspendLayout();
            // 
            // lblCurrentProtection
            // 
            this.lblCurrentProtection.AutoSize = true;
            this.lblCurrentProtection.Location = new System.Drawing.Point(22, 18);
            this.lblCurrentProtection.Name = "lblCurrentProtection";
            this.lblCurrentProtection.Size = new System.Drawing.Size(112, 16);
            this.lblCurrentProtection.TabIndex = 64;
            this.lblCurrentProtection.Text = "Current Protection:";
            // 
            // lblCurrentProtectionValue
            // 
            this.lblCurrentProtectionValue.AutoSize = true;
            this.lblCurrentProtectionValue.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCurrentProtectionValue.Location = new System.Drawing.Point(151, 18);
            this.lblCurrentProtectionValue.Name = "lblCurrentProtectionValue";
            this.lblCurrentProtectionValue.Size = new System.Drawing.Size(114, 16);
            this.lblCurrentProtectionValue.TabIndex = 65;
            this.lblCurrentProtectionValue.Text = "Custom Password";
            // 
            // grpProtection
            // 
            this.grpProtection.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpProtection.Controls.Add(this.btnManageRecoveryKey);
            this.grpProtection.Controls.Add(this.lblRecoveryStatus);
            this.grpProtection.Controls.Add(this.rdoCustomPassword);
            this.grpProtection.Controls.Add(this.rdoWindowsProtected);
            this.grpProtection.Location = new System.Drawing.Point(20, 51);
            this.grpProtection.Name = "grpProtection";
            this.grpProtection.Size = new System.Drawing.Size(673, 88);
            this.grpProtection.TabIndex = 66;
            this.grpProtection.TabStop = false;
            this.grpProtection.Text = "Protection";
            // 
            // btnManageRecoveryKey
            // 
            this.btnManageRecoveryKey.Location = new System.Drawing.Point(395, 48);
            this.btnManageRecoveryKey.Margin = new System.Windows.Forms.Padding(4);
            this.btnManageRecoveryKey.Name = "btnManageRecoveryKey";
            this.btnManageRecoveryKey.Size = new System.Drawing.Size(265, 26);
            this.btnManageRecoveryKey.TabIndex = 77;
            this.btnManageRecoveryKey.Text = "Manage Recovery Key...";
            this.btnManageRecoveryKey.UseVisualStyleBackColor = true;
            this.btnManageRecoveryKey.Visible = false;
            this.btnManageRecoveryKey.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnManageRecoveryKey.Click += new System.EventHandler(this.btnManageRecoveryKey_Click);
            // 
            // lblRecoveryStatus
            // 
            this.lblRecoveryStatus.AutoSize = true;
            this.lblRecoveryStatus.Location = new System.Drawing.Point(394, 25);
            this.lblRecoveryStatus.Name = "lblRecoveryStatus";
            this.lblRecoveryStatus.Size = new System.Drawing.Size(175, 16);
            this.lblRecoveryStatus.TabIndex = 65;
            this.lblRecoveryStatus.Text = "Recovery Key: Not configured";
            this.lblRecoveryStatus.Visible = false;
            // 
            // rdoCustomPassword
            // 
            this.rdoCustomPassword.AutoSize = true;
            this.rdoCustomPassword.Location = new System.Drawing.Point(30, 53);
            this.rdoCustomPassword.Name = "rdoCustomPassword";
            this.rdoCustomPassword.Size = new System.Drawing.Size(124, 20);
            this.rdoCustomPassword.TabIndex = 1;
            this.rdoCustomPassword.TabStop = true;
            this.rdoCustomPassword.Text = "Custom Password";
            this.rdoCustomPassword.UseVisualStyleBackColor = true;
            // 
            // rdoWindowsProtected
            // 
            this.rdoWindowsProtected.AutoSize = true;
            this.rdoWindowsProtected.Location = new System.Drawing.Point(30, 25);
            this.rdoWindowsProtected.Name = "rdoWindowsProtected";
            this.rdoWindowsProtected.Size = new System.Drawing.Size(135, 20);
            this.rdoWindowsProtected.TabIndex = 0;
            this.rdoWindowsProtected.TabStop = true;
            this.rdoWindowsProtected.Text = "Windows Protected";
            this.rdoWindowsProtected.UseVisualStyleBackColor = true;
            // 
            // lblCurrentPassword
            // 
            this.lblCurrentPassword.AutoSize = true;
            this.lblCurrentPassword.Location = new System.Drawing.Point(22, 158);
            this.lblCurrentPassword.Name = "lblCurrentPassword";
            this.lblCurrentPassword.Size = new System.Drawing.Size(107, 16);
            this.lblCurrentPassword.TabIndex = 67;
            this.lblCurrentPassword.Text = "Current Password:";
            // 
            // txtCurrentPassword
            // 
            this.txtCurrentPassword.Location = new System.Drawing.Point(140, 155);
            this.txtCurrentPassword.Name = "txtCurrentPassword";
            this.txtCurrentPassword.PasswordChar = '*';
            this.txtCurrentPassword.Size = new System.Drawing.Size(331, 23);
            this.txtCurrentPassword.TabIndex = 68;
            // 
            // txtNewCustomPassword
            // 
            this.txtNewCustomPassword.Location = new System.Drawing.Point(172, 188);
            this.txtNewCustomPassword.Name = "txtNewCustomPassword";
            this.txtNewCustomPassword.PasswordChar = '*';
            this.txtNewCustomPassword.Size = new System.Drawing.Size(331, 23);
            this.txtNewCustomPassword.TabIndex = 70;
            // 
            // lblNewCustomPassword
            // 
            this.lblNewCustomPassword.AutoSize = true;
            this.lblNewCustomPassword.Location = new System.Drawing.Point(22, 191);
            this.lblNewCustomPassword.Name = "lblNewCustomPassword";
            this.lblNewCustomPassword.Size = new System.Drawing.Size(138, 16);
            this.lblNewCustomPassword.TabIndex = 69;
            this.lblNewCustomPassword.Text = "New Custom Password:";
            // 
            // txtConfirmPassword
            // 
            this.txtConfirmPassword.Location = new System.Drawing.Point(140, 221);
            this.txtConfirmPassword.Name = "txtConfirmPassword";
            this.txtConfirmPassword.PasswordChar = '*';
            this.txtConfirmPassword.Size = new System.Drawing.Size(331, 23);
            this.txtConfirmPassword.TabIndex = 72;
            // 
            // lblConfirmPassword
            // 
            this.lblConfirmPassword.AutoSize = true;
            this.lblConfirmPassword.Location = new System.Drawing.Point(22, 224);
            this.lblConfirmPassword.Name = "lblConfirmPassword";
            this.lblConfirmPassword.Size = new System.Drawing.Size(111, 16);
            this.lblConfirmPassword.TabIndex = 71;
            this.lblConfirmPassword.Text = "Confirm Password:";
            // 
            // lblInfo1
            // 
            this.lblInfo1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo1.Location = new System.Drawing.Point(22, 260);
            this.lblInfo1.Name = "lblInfo1";
            this.lblInfo1.Size = new System.Drawing.Size(669, 45);
            this.lblInfo1.TabIndex = 73;
            this.lblInfo1.Text = "Custom passwords cannot be recovered by JasonQuery.";
            // 
            // lblInfo2
            // 
            this.lblInfo2.AutoSize = true;
            this.lblInfo2.Location = new System.Drawing.Point(22, 284);
            this.lblInfo2.Name = "lblInfo2";
            this.lblInfo2.Size = new System.Drawing.Size(331, 16);
            this.lblInfo2.TabIndex = 74;
            this.lblInfo2.Text = "You must enter the password each time JasonQuery starts.";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(592, 330);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 32);
            this.btnClose.TabIndex = 76;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnApply
            // 
            this.btnApply.Location = new System.Drawing.Point(477, 330);
            this.btnApply.Margin = new System.Windows.Forms.Padding(4);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(95, 32);
            this.btnApply.TabIndex = 75;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // grpLocalization
            // 
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(18, 314);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(355, 55);
            this.grpLocalization.TabIndex = 77;
            this.grpLocalization.TabStop = false;
            this.grpLocalization.Text = "Localization";
            // 
            // cboLocalization
            // 
            this.cboLocalization.AllowSpinLoop = false;
            this.cboLocalization.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(237)))), ((int)(((byte)(237)))), ((int)(((byte)(239)))));
            this.cboLocalization.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboLocalization.Cursor = System.Windows.Forms.Cursors.Arrow;
            this.cboLocalization.DropDownStyle = C1.Win.C1Input.DropDownStyle.DropDownList;
            this.cboLocalization.GapHeight = 0;
            this.cboLocalization.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboLocalization.ItemsDisplayMember = "";
            this.cboLocalization.ItemsValueMember = "";
            this.cboLocalization.Location = new System.Drawing.Point(20, 22);
            this.cboLocalization.Name = "cboLocalization";
            this.cboLocalization.Size = new System.Drawing.Size(315, 21);
            this.cboLocalization.TabIndex = 0;
            this.cboLocalization.Tag = null;
            this.cboLocalization.TextDetached = true;
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // JasonQueryDbSecurityForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(710, 385);
            this.Controls.Add(this.grpLocalization);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.lblInfo2);
            this.Controls.Add(this.lblInfo1);
            this.Controls.Add(this.txtConfirmPassword);
            this.Controls.Add(this.lblConfirmPassword);
            this.Controls.Add(this.txtNewCustomPassword);
            this.Controls.Add(this.lblNewCustomPassword);
            this.Controls.Add(this.txtCurrentPassword);
            this.Controls.Add(this.lblCurrentPassword);
            this.Controls.Add(this.grpProtection);
            this.Controls.Add(this.lblCurrentProtectionValue);
            this.Controls.Add(this.lblCurrentProtection);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JasonQueryDbSecurityForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "JasonQuery.db Security";
            this.Load += new System.EventHandler(this.Form_Load);
            this.grpProtection.ResumeLayout(false);
            this.grpProtection.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnManageRecoveryKey)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblCurrentProtection;
        private System.Windows.Forms.Label lblCurrentProtectionValue;
        private System.Windows.Forms.GroupBox grpProtection;
        private System.Windows.Forms.RadioButton rdoCustomPassword;
        private System.Windows.Forms.RadioButton rdoWindowsProtected;
        private System.Windows.Forms.Label lblCurrentPassword;
        private System.Windows.Forms.TextBox txtCurrentPassword;
        private System.Windows.Forms.TextBox txtNewCustomPassword;
        private System.Windows.Forms.Label lblNewCustomPassword;
        private System.Windows.Forms.TextBox txtConfirmPassword;
        private System.Windows.Forms.Label lblConfirmPassword;
        private System.Windows.Forms.Label lblInfo1;
        private System.Windows.Forms.Label lblInfo2;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnApply;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
        private System.Windows.Forms.Label lblRecoveryStatus;
        private C1.Win.C1Input.C1Button btnManageRecoveryKey;
    }
}
