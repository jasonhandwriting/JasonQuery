namespace JasonQuery.UI.Forms
{
    partial class CustomPasswordDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CustomPasswordDialog));
            this.txtEncryptPassword = new C1.Win.C1Input.C1TextBox();
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnExit = new C1.Win.C1Input.C1Button();
            this.lblRemember = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            this.lblInfo = new System.Windows.Forms.Label();
            this.btnEncryptPasswordView = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).BeginInit();
            this.SuspendLayout();
            // 
            // txtEncryptPassword
            // 
            this.txtEncryptPassword.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtEncryptPassword.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtEncryptPassword.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.txtEncryptPassword.Location = new System.Drawing.Point(20, 40);
            this.txtEncryptPassword.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtEncryptPassword.Name = "txtEncryptPassword";
            this.txtEncryptPassword.PasswordChar = '*';
            this.txtEncryptPassword.ShowContextMenu = false;
            this.txtEncryptPassword.Size = new System.Drawing.Size(180, 21);
            this.txtEncryptPassword.TabIndex = 0;
            this.txtEncryptPassword.Tag = null;
            this.txtEncryptPassword.TextDetached = true;
            this.txtEncryptPassword.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtEncryptPassword.TextChanged += new System.EventHandler(this.txtEncryptPassword_TextChanged);
            this.txtEncryptPassword.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtEncryptPassword_KeyDown);
            // 
            // btnOK
            // 
            this.btnOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOK.Location = new System.Drawing.Point(379, 113);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(77, 29);
            this.btnOK.TabIndex = 130;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnExit
            // 
            this.btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnExit.Location = new System.Drawing.Point(329, 155);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(127, 29);
            this.btnExit.TabIndex = 129;
            this.btnExit.Text = "Exit JasonQuery";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // lblRemember
            // 
            this.lblRemember.AutoSize = true;
            this.lblRemember.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRemember.Location = new System.Drawing.Point(17, 66);
            this.lblRemember.Name = "lblRemember";
            this.lblRemember.Size = new System.Drawing.Size(269, 16);
            this.lblRemember.TabIndex = 134;
            this.lblRemember.Text = "(Remember that passwords are case-sensitive.)";
            this.lblRemember.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.Location = new System.Drawing.Point(16, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(406, 17);
            this.lblTitle.TabIndex = 132;
            this.lblTitle.Text = "Please enter your custom password to connect to JasonQuery.db:";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // grpLocalization
            // 
            this.grpLocalization.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(21, 133);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(260, 53);
            this.grpLocalization.TabIndex = 138;
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
            this.cboLocalization.Items.Add("yyyy/MM/dd");
            this.cboLocalization.Items.Add("yyyy-MM-dd");
            this.cboLocalization.Items.Add("MM/dd/yyyy");
            this.cboLocalization.Items.Add("MM-dd-yyyy");
            this.cboLocalization.Items.Add("dd/MM/yyyy");
            this.cboLocalization.Items.Add("dd-MM-yyyy");
            this.cboLocalization.ItemsDisplayMember = "";
            this.cboLocalization.ItemsValueMember = "";
            this.cboLocalization.Location = new System.Drawing.Point(29, 20);
            this.cboLocalization.Name = "cboLocalization";
            this.cboLocalization.Size = new System.Drawing.Size(216, 21);
            this.cboLocalization.TabIndex = 79;
            this.cboLocalization.Tag = null;
            this.cboLocalization.TextDetached = true;
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboLocalization.SelectedIndexChanged += new System.EventHandler(this.cboLocalization_SelectedIndexChanged);
            // 
            // lblInfo
            // 
            this.lblInfo.ForeColor = System.Drawing.Color.Maroon;
            this.lblInfo.Location = new System.Drawing.Point(18, 90);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(336, 39);
            this.lblInfo.TabIndex = 139;
            this.lblInfo.Text = "Since you set a custom password, it must be entered when starting JasonQuery.";
            // 
            // btnEncryptPasswordView
            // 
            this.btnEncryptPasswordView.Image = ((System.Drawing.Image)(resources.GetObject("btnEncryptPasswordView.Image")));
            this.btnEncryptPasswordView.Location = new System.Drawing.Point(206, 40);
            this.btnEncryptPasswordView.Name = "btnEncryptPasswordView";
            this.btnEncryptPasswordView.Size = new System.Drawing.Size(21, 21);
            this.btnEncryptPasswordView.TabIndex = 140;
            this.btnEncryptPasswordView.UseVisualStyleBackColor = true;
            this.btnEncryptPasswordView.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnEncryptPasswordView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseDown);
            this.btnEncryptPasswordView.MouseUp += new System.Windows.Forms.MouseEventHandler(this.btnEncryptPasswordView_MouseUp);
            // 
            // CustomPasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(471, 201);
            this.ControlBox = false;
            this.Controls.Add(this.btnEncryptPasswordView);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.grpLocalization);
            this.Controls.Add(this.lblRemember);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.txtEncryptPassword);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CustomPasswordDialog";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Password Protection for JasonQuery.db";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.txtEncryptPassword)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnEncryptPasswordView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private C1.Win.C1Input.C1TextBox txtEncryptPassword;
        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnExit;
        private System.Windows.Forms.Label lblRemember;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
        private System.Windows.Forms.Label lblInfo;
        private C1.Win.C1Input.C1Button btnEncryptPasswordView;
    }
}