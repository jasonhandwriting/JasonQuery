namespace JasonQuery.UI.Forms
{
    partial class JasonQueryDbPasswordDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JasonQueryDbPasswordDialog));
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnExit = new C1.Win.C1Input.C1Button();
            this.lblInfo1 = new System.Windows.Forms.Label();
            this.lblInfo2 = new System.Windows.Forms.Label();
            this.lblCustomPassword = new System.Windows.Forms.Label();
            this.txtCustomPassword = new System.Windows.Forms.TextBox();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            this.SuspendLayout();
            //
            // btnOK
            //
            this.btnOK.Location = new System.Drawing.Point(358, 131);
            this.btnOK.Margin = new System.Windows.Forms.Padding(4);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(86, 32);
            this.btnOK.TabIndex = 60;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            //
            // btnExit
            //
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Location = new System.Drawing.Point(459, 131);
            this.btnExit.Margin = new System.Windows.Forms.Padding(4);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(145, 32);
            this.btnExit.TabIndex = 61;
            this.btnExit.Text = "Exit JasonQuery";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            //
            // lblInfo1
            //
            this.lblInfo1.AutoSize = true;
            this.lblInfo1.Location = new System.Drawing.Point(22, 18);
            this.lblInfo1.Name = "lblInfo1";
            this.lblInfo1.Size = new System.Drawing.Size(302, 16);
            this.lblInfo1.TabIndex = 62;
            this.lblInfo1.Text = "JasonQuery.db is protected with a custom password.";
            //
            // lblInfo2
            //
            this.lblInfo2.AutoSize = true;
            this.lblInfo2.Location = new System.Drawing.Point(22, 41);
            this.lblInfo2.Name = "lblInfo2";
            this.lblInfo2.Size = new System.Drawing.Size(184, 16);
            this.lblInfo2.TabIndex = 63;
            this.lblInfo2.Text = "Enter the password to continue.";
            //
            // lblCustomPassword
            //
            this.lblCustomPassword.AutoSize = true;
            this.lblCustomPassword.Location = new System.Drawing.Point(22, 78);
            this.lblCustomPassword.Name = "lblCustomPassword";
            this.lblCustomPassword.Size = new System.Drawing.Size(109, 16);
            this.lblCustomPassword.TabIndex = 64;
            this.lblCustomPassword.Text = "Custom Password:";
            //
            // txtCustomPassword
            //
            this.txtCustomPassword.Location = new System.Drawing.Point(138, 75);
            this.txtCustomPassword.Name = "txtCustomPassword";
            this.txtCustomPassword.PasswordChar = '*';
            this.txtCustomPassword.Size = new System.Drawing.Size(235, 23);
            this.txtCustomPassword.TabIndex = 65;
            //
            // grpLocalization
            //
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(18, 115);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(320, 55);
            this.grpLocalization.TabIndex = 66;
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
            this.cboLocalization.Size = new System.Drawing.Size(280, 21);
            this.cboLocalization.TabIndex = 0;
            this.cboLocalization.TextDetached = true;
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            //
            // JasonQueryDbPasswordDialog
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 182);
            this.Controls.Add(this.grpLocalization);
            this.Controls.Add(this.txtCustomPassword);
            this.Controls.Add(this.lblCustomPassword);
            this.Controls.Add(this.lblInfo2);
            this.Controls.Add(this.lblInfo1);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnOK);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JasonQueryDbPasswordDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "JasonQuery.db Password";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnExit;
        private System.Windows.Forms.Label lblInfo1;
        private System.Windows.Forms.Label lblInfo2;
        private System.Windows.Forms.Label lblCustomPassword;
        private System.Windows.Forms.TextBox txtCustomPassword;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
    }
}
