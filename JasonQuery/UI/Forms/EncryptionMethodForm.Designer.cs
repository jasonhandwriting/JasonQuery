namespace JasonQuery.UI.Forms
{
    partial class EncryptionMethodForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EncryptionMethodForm));
            this.rdoDefaultPassword = new System.Windows.Forms.RadioButton();
            this.txtCustomPassword = new C1.Win.C1Input.C1TextBox();
            this.lblOldPassword = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.rdoCustomPassword = new System.Windows.Forms.RadioButton();
            this.rdoChangeCustomPassword = new System.Windows.Forms.RadioButton();
            this.lblNewPassword = new System.Windows.Forms.Label();
            this.lblConfirmNewPassword = new System.Windows.Forms.Label();
            this.txtOldPassword = new C1.Win.C1Input.C1TextBox();
            this.txtNewPassword = new C1.Win.C1Input.C1TextBox();
            this.txtConfirmNewPassword = new C1.Win.C1Input.C1TextBox();
            this.lblCaution2 = new System.Windows.Forms.Label();
            this.lblCaution0 = new System.Windows.Forms.Label();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnApply = new C1.Win.C1Input.C1Button();
            this.lblCaution1 = new System.Windows.Forms.Label();
            this.tmrNewPassword = new System.Windows.Forms.Timer(this.components);
            this.btnCustomPasswordView = new C1.Win.C1Input.C1Button();
            this.txtOldPassword2 = new C1.Win.C1Input.C1TextBox();
            this.lblOldPassword2 = new System.Windows.Forms.Label();
            this.lblEncryptionMothed = new System.Windows.Forms.Label();
            this.lblChangeToDefaultPassword = new System.Windows.Forms.Label();
            this.btnHelp_Password = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtOldPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConfirmNewPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCustomPasswordView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtOldPassword2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Password)).BeginInit();
            this.SuspendLayout();
            // 
            // rdoDefaultPassword
            // 
            this.rdoDefaultPassword.AutoSize = true;
            this.rdoDefaultPassword.Checked = true;
            this.rdoDefaultPassword.Location = new System.Drawing.Point(19, 45);
            this.rdoDefaultPassword.Name = "rdoDefaultPassword";
            this.rdoDefaultPassword.Size = new System.Drawing.Size(122, 20);
            this.rdoDefaultPassword.TabIndex = 112;
            this.rdoDefaultPassword.TabStop = true;
            this.rdoDefaultPassword.Tag = "0";
            this.rdoDefaultPassword.Text = "Default Password";
            this.rdoDefaultPassword.UseVisualStyleBackColor = true;
            this.rdoDefaultPassword.CheckedChanged += new System.EventHandler(this.rdoPassword_CheckedChanged);
            // 
            // txtCustomPassword
            // 
            this.txtCustomPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtCustomPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCustomPassword.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtCustomPassword.Location = new System.Drawing.Point(148, 97);
            this.txtCustomPassword.Name = "txtCustomPassword";
            this.txtCustomPassword.PasswordChar = '*';
            this.txtCustomPassword.ShowContextMenu = false;
            this.txtCustomPassword.Size = new System.Drawing.Size(180, 21);
            this.txtCustomPassword.TabIndex = 111;
            this.txtCustomPassword.Tag = null;
            this.txtCustomPassword.TextDetached = true;
            this.txtCustomPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtCustomPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            this.txtCustomPassword.Enter += new System.EventHandler(this.txtCustomPassword_Enter);
            this.txtCustomPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // lblOldPassword
            // 
            this.lblOldPassword.AutoSize = true;
            this.lblOldPassword.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblOldPassword.Location = new System.Drawing.Point(35, 151);
            this.lblOldPassword.Name = "lblOldPassword";
            this.lblOldPassword.Size = new System.Drawing.Size(133, 16);
            this.lblOldPassword.TabIndex = 110;
            this.lblOldPassword.Text = "Old Custom Password:";
            this.lblOldPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.Location = new System.Drawing.Point(16, 14);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(293, 17);
            this.lblTitle.TabIndex = 113;
            this.lblTitle.Text = "Encrypt JasonQuery.db with Custom Password";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // rdoCustomPassword
            // 
            this.rdoCustomPassword.AutoSize = true;
            this.rdoCustomPassword.Location = new System.Drawing.Point(19, 97);
            this.rdoCustomPassword.Name = "rdoCustomPassword";
            this.rdoCustomPassword.Size = new System.Drawing.Size(127, 20);
            this.rdoCustomPassword.TabIndex = 115;
            this.rdoCustomPassword.Tag = "1";
            this.rdoCustomPassword.Text = "Custom Password:";
            this.rdoCustomPassword.UseVisualStyleBackColor = true;
            this.rdoCustomPassword.CheckedChanged += new System.EventHandler(this.rdoPassword_CheckedChanged);
            // 
            // rdoChangeCustomPassword
            // 
            this.rdoChangeCustomPassword.AutoSize = true;
            this.rdoChangeCustomPassword.Location = new System.Drawing.Point(19, 124);
            this.rdoChangeCustomPassword.Name = "rdoChangeCustomPassword";
            this.rdoChangeCustomPassword.Size = new System.Drawing.Size(171, 20);
            this.rdoChangeCustomPassword.TabIndex = 116;
            this.rdoChangeCustomPassword.Tag = "2";
            this.rdoChangeCustomPassword.Text = "Change Custom Password";
            this.rdoChangeCustomPassword.UseVisualStyleBackColor = true;
            this.rdoChangeCustomPassword.CheckedChanged += new System.EventHandler(this.rdoPassword_CheckedChanged);
            // 
            // lblNewPassword
            // 
            this.lblNewPassword.AutoSize = true;
            this.lblNewPassword.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblNewPassword.Location = new System.Drawing.Point(35, 178);
            this.lblNewPassword.Name = "lblNewPassword";
            this.lblNewPassword.Size = new System.Drawing.Size(92, 16);
            this.lblNewPassword.TabIndex = 118;
            this.lblNewPassword.Text = "New Password:";
            this.lblNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblConfirmNewPassword
            // 
            this.lblConfirmNewPassword.AutoSize = true;
            this.lblConfirmNewPassword.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblConfirmNewPassword.Location = new System.Drawing.Point(35, 205);
            this.lblConfirmNewPassword.Name = "lblConfirmNewPassword";
            this.lblConfirmNewPassword.Size = new System.Drawing.Size(111, 16);
            this.lblConfirmNewPassword.TabIndex = 119;
            this.lblConfirmNewPassword.Text = "Confirm Password:";
            this.lblConfirmNewPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // txtOldPassword
            // 
            this.txtOldPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtOldPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOldPassword.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtOldPassword.Location = new System.Drawing.Point(172, 149);
            this.txtOldPassword.Name = "txtOldPassword";
            this.txtOldPassword.PasswordChar = '*';
            this.txtOldPassword.ShowContextMenu = false;
            this.txtOldPassword.Size = new System.Drawing.Size(180, 21);
            this.txtOldPassword.TabIndex = 120;
            this.txtOldPassword.Tag = null;
            this.txtOldPassword.TextDetached = true;
            this.txtOldPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtOldPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            this.txtOldPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
            this.txtOldPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // txtNewPassword
            // 
            this.txtNewPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtNewPassword.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtNewPassword.Location = new System.Drawing.Point(172, 176);
            this.txtNewPassword.Name = "txtNewPassword";
            this.txtNewPassword.PasswordChar = '*';
            this.txtNewPassword.ShowContextMenu = false;
            this.txtNewPassword.Size = new System.Drawing.Size(180, 21);
            this.txtNewPassword.TabIndex = 121;
            this.txtNewPassword.Tag = null;
            this.txtNewPassword.TextDetached = true;
            this.txtNewPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtNewPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            this.txtNewPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
            this.txtNewPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // txtConfirmNewPassword
            // 
            this.txtConfirmNewPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtConfirmNewPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtConfirmNewPassword.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtConfirmNewPassword.Location = new System.Drawing.Point(172, 203);
            this.txtConfirmNewPassword.Name = "txtConfirmNewPassword";
            this.txtConfirmNewPassword.PasswordChar = '*';
            this.txtConfirmNewPassword.ShowContextMenu = false;
            this.txtConfirmNewPassword.Size = new System.Drawing.Size(180, 21);
            this.txtConfirmNewPassword.TabIndex = 122;
            this.txtConfirmNewPassword.Tag = null;
            this.txtConfirmNewPassword.TextDetached = true;
            this.txtConfirmNewPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtConfirmNewPassword.TextChanged += new System.EventHandler(this.txtPassword_TextChanged);
            this.txtConfirmNewPassword.Enter += new System.EventHandler(this.txtPassword_Enter);
            this.txtConfirmNewPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPassword_KeyDown);
            // 
            // lblCaution2
            // 
            this.lblCaution2.AutoSize = true;
            this.lblCaution2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCaution2.ForeColor = System.Drawing.Color.Maroon;
            this.lblCaution2.Location = new System.Drawing.Point(19, 287);
            this.lblCaution2.Name = "lblCaution2";
            this.lblCaution2.Size = new System.Drawing.Size(269, 16);
            this.lblCaution2.TabIndex = 124;
            this.lblCaution2.Text = "(Remember that passwords are case-sensitive.)";
            this.lblCaution2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCaution2.Visible = false;
            // 
            // lblCaution0
            // 
            this.lblCaution0.AutoSize = true;
            this.lblCaution0.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblCaution0.ForeColor = System.Drawing.Color.Maroon;
            this.lblCaution0.Location = new System.Drawing.Point(20, 242);
            this.lblCaution0.Name = "lblCaution0";
            this.lblCaution0.Size = new System.Drawing.Size(502, 16);
            this.lblCaution0.TabIndex = 123;
            this.lblCaution0.Text = "You will need to enter the custom password each time you start JasonQuery in the " +
    "future.";
            this.lblCaution0.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblCaution0.Visible = false;
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(339, 194);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(125, 29);
            this.btnClose.TabIndex = 127;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(339, 153);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(125, 29);
            this.btnApply.TabIndex = 0;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // lblCaution1
            // 
            this.lblCaution1.AutoSize = true;
            this.lblCaution1.ForeColor = System.Drawing.Color.Maroon;
            this.lblCaution1.Location = new System.Drawing.Point(20, 262);
            this.lblCaution1.Name = "lblCaution1";
            this.lblCaution1.Size = new System.Drawing.Size(381, 16);
            this.lblCaution1.TabIndex = 128;
            this.lblCaution1.Text = "Caution: If you lose or forget the password, it cannot be recovered.";
            this.lblCaution1.Visible = false;
            // 
            // tmrNewPassword
            // 
            this.tmrNewPassword.Interval = 300;
            this.tmrNewPassword.Tick += new System.EventHandler(this.tmrNewPassword_Tick);
            // 
            // btnCustomPasswordView
            // 
            this.btnCustomPasswordView.Image = ((System.Drawing.Image)(resources.GetObject("btnCustomPasswordView.Image")));
            this.btnCustomPasswordView.Location = new System.Drawing.Point(292, 97);
            this.btnCustomPasswordView.Name = "btnCustomPasswordView";
            this.btnCustomPasswordView.Size = new System.Drawing.Size(21, 21);
            this.btnCustomPasswordView.TabIndex = 129;
            this.btnCustomPasswordView.UseVisualStyleBackColor = true;
            this.btnCustomPasswordView.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCustomPasswordView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnCustomPasswordView_MouseDown);
            this.btnCustomPasswordView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnCustomPasswordView_MouseUp);
            // 
            // txtOldPassword2
            // 
            this.txtOldPassword2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtOldPassword2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtOldPassword2.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtOldPassword2.Location = new System.Drawing.Point(316, 45);
            this.txtOldPassword2.Name = "txtOldPassword2";
            this.txtOldPassword2.PasswordChar = '*';
            this.txtOldPassword2.ShowContextMenu = false;
            this.txtOldPassword2.Size = new System.Drawing.Size(180, 21);
            this.txtOldPassword2.TabIndex = 131;
            this.txtOldPassword2.Tag = null;
            this.txtOldPassword2.TextDetached = true;
            this.txtOldPassword2.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtOldPassword2.Enter += new System.EventHandler(this.txtOldPassword2_Enter);
            // 
            // lblOldPassword2
            // 
            this.lblOldPassword2.AutoSize = true;
            this.lblOldPassword2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblOldPassword2.Location = new System.Drawing.Point(179, 47);
            this.lblOldPassword2.Name = "lblOldPassword2";
            this.lblOldPassword2.Size = new System.Drawing.Size(133, 16);
            this.lblOldPassword2.TabIndex = 130;
            this.lblOldPassword2.Text = "Old Custom Password:";
            this.lblOldPassword2.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblEncryptionMothed
            // 
            this.lblEncryptionMothed.AutoSize = true;
            this.lblEncryptionMothed.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblEncryptionMothed.ForeColor = System.Drawing.Color.Green;
            this.lblEncryptionMothed.Location = new System.Drawing.Point(336, 14);
            this.lblEncryptionMothed.Name = "lblEncryptionMothed";
            this.lblEncryptionMothed.Size = new System.Drawing.Size(0, 17);
            this.lblEncryptionMothed.TabIndex = 132;
            this.lblEncryptionMothed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblChangeToDefaultPassword
            // 
            this.lblChangeToDefaultPassword.AutoSize = true;
            this.lblChangeToDefaultPassword.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblChangeToDefaultPassword.ForeColor = System.Drawing.Color.Maroon;
            this.lblChangeToDefaultPassword.Location = new System.Drawing.Point(34, 71);
            this.lblChangeToDefaultPassword.Name = "lblChangeToDefaultPassword";
            this.lblChangeToDefaultPassword.Size = new System.Drawing.Size(622, 16);
            this.lblChangeToDefaultPassword.TabIndex = 133;
            this.lblChangeToDefaultPassword.Text = "(You must enter your custom password before you can change the custom password to" +
    " the default password.)";
            this.lblChangeToDefaultPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblChangeToDefaultPassword.Visible = false;
            // 
            // btnHelp_Password
            // 
            this.btnHelp_Password.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_Password.Image")));
            this.btnHelp_Password.Location = new System.Drawing.Point(339, 97);
            this.btnHelp_Password.Name = "btnHelp_Password";
            this.btnHelp_Password.Size = new System.Drawing.Size(21, 21);
            this.btnHelp_Password.TabIndex = 134;
            this.btnHelp_Password.Tag = "Available characters for password";
            this.btnHelp_Password.UseVisualStyleBackColor = true;
            this.btnHelp_Password.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnHelp_Password.Click += new System.EventHandler(this.btnHelp_Password_Click);
            // 
            // EncryptionMethodForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 316);
            this.Controls.Add(this.btnHelp_Password);
            this.Controls.Add(this.lblChangeToDefaultPassword);
            this.Controls.Add(this.lblEncryptionMothed);
            this.Controls.Add(this.txtOldPassword2);
            this.Controls.Add(this.lblOldPassword2);
            this.Controls.Add(this.btnCustomPasswordView);
            this.Controls.Add(this.txtConfirmNewPassword);
            this.Controls.Add(this.txtNewPassword);
            this.Controls.Add(this.txtOldPassword);
            this.Controls.Add(this.txtCustomPassword);
            this.Controls.Add(this.lblCaution1);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.lblCaution2);
            this.Controls.Add(this.lblCaution0);
            this.Controls.Add(this.lblConfirmNewPassword);
            this.Controls.Add(this.lblNewPassword);
            this.Controls.Add(this.lblOldPassword);
            this.Controls.Add(this.rdoChangeCustomPassword);
            this.Controls.Add(this.rdoCustomPassword);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.rdoDefaultPassword);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "EncryptionMethodForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Password Protection for JasonQuery.db";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtCustomPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtOldPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtNewPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtConfirmNewPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnApply)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCustomPasswordView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtOldPassword2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_Password)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton rdoDefaultPassword;
        private C1.Win.C1Input.C1TextBox txtCustomPassword;
        private System.Windows.Forms.Label lblOldPassword;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RadioButton rdoCustomPassword;
        private System.Windows.Forms.RadioButton rdoChangeCustomPassword;
        private System.Windows.Forms.Label lblNewPassword;
        private System.Windows.Forms.Label lblConfirmNewPassword;
        private C1.Win.C1Input.C1TextBox txtOldPassword;
        private C1.Win.C1Input.C1TextBox txtNewPassword;
        private C1.Win.C1Input.C1TextBox txtConfirmNewPassword;
        private System.Windows.Forms.Label lblCaution2;
        private System.Windows.Forms.Label lblCaution0;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnApply;
        private System.Windows.Forms.Label lblCaution1;
        private System.Windows.Forms.Timer tmrNewPassword;
        private C1.Win.C1Input.C1Button btnCustomPasswordView;
        private C1.Win.C1Input.C1TextBox txtOldPassword2;
        private System.Windows.Forms.Label lblOldPassword2;
        private System.Windows.Forms.Label lblEncryptionMothed;
        private System.Windows.Forms.Label lblChangeToDefaultPassword;
        private C1.Win.C1Input.C1Button btnHelp_Password;
    }
}