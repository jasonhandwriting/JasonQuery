namespace JasonQuery.UI.Forms
{
    partial class JasonQueryDbRecoveryStartupForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JasonQueryDbRecoveryStartupForm));
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblRecoveryKey = new System.Windows.Forms.Label();
            this.txtRecoveryKey = new System.Windows.Forms.TextBox();
            this.btnBrowseFile = new C1.Win.C1Input.C1Button();
            this.btnExit = new C1.Win.C1Input.C1Button();
            this.btnRecover = new C1.Win.C1Input.C1Button();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRecover)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Location = new System.Drawing.Point(13, 16);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(636, 64);
            this.lblInfo.TabIndex = 0;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // lblRecoveryKey
            // 
            this.lblRecoveryKey.AutoSize = true;
            this.lblRecoveryKey.Location = new System.Drawing.Point(13, 89);
            this.lblRecoveryKey.Name = "lblRecoveryKey";
            this.lblRecoveryKey.Size = new System.Drawing.Size(85, 16);
            this.lblRecoveryKey.TabIndex = 1;
            this.lblRecoveryKey.Text = "Recovery Key:";
            // 
            // txtRecoveryKey
            // 
            this.txtRecoveryKey.Location = new System.Drawing.Point(16, 109);
            this.txtRecoveryKey.Name = "txtRecoveryKey";
            this.txtRecoveryKey.Size = new System.Drawing.Size(600, 23);
            this.txtRecoveryKey.TabIndex = 0;
            // 
            // btnBrowseFile
            // 
            this.btnBrowseFile.Location = new System.Drawing.Point(625, 109);
            this.btnBrowseFile.Name = "btnBrowseFile";
            this.btnBrowseFile.Size = new System.Drawing.Size(21, 23);
            this.btnBrowseFile.TabIndex = 1;
            this.btnBrowseFile.Text = "...";
            this.btnBrowseFile.UseVisualStyleBackColor = true;
            this.btnBrowseFile.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnBrowseFile.Click += new System.EventHandler(this.btnBrowseFile_Click);
            // 
            // btnExit
            // 
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnExit.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnExit.Location = new System.Drawing.Point(548, 161);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(98, 32);
            this.btnExit.TabIndex = 4;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnRecover
            // 
            this.btnRecover.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnRecover.Location = new System.Drawing.Point(393, 161);
            this.btnRecover.Name = "btnRecover";
            this.btnRecover.Size = new System.Drawing.Size(131, 32);
            this.btnRecover.TabIndex = 3;
            this.btnRecover.Text = "Recover";
            this.btnRecover.UseVisualStyleBackColor = true;
            this.btnRecover.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnRecover.Click += new System.EventHandler(this.btnRecover_Click);
            // 
            // grpLocalization
            // 
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(18, 142);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(355, 55);
            this.grpLocalization.TabIndex = 2;
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
            this.cboLocalization.SelectedIndexChanged += new System.EventHandler(this.cboLocalization_SelectedIndexChanged);
            // 
            // JasonQueryDbRecoveryStartupForm
            // 
            this.AcceptButton = this.btnRecover;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(662, 208);
            this.CancelButton = this.btnExit;
            this.Controls.Add(this.grpLocalization);
            this.Controls.Add(this.btnRecover);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.btnBrowseFile);
            this.Controls.Add(this.txtRecoveryKey);
            this.Controls.Add(this.lblRecoveryKey);
            this.Controls.Add(this.lblInfo);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JasonQueryDbRecoveryStartupForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = true;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Recover JasonQuery.db";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnBrowseFile)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnRecover)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblRecoveryKey;
        private System.Windows.Forms.TextBox txtRecoveryKey;
        private C1.Win.C1Input.C1Button btnBrowseFile;
        private C1.Win.C1Input.C1Button btnExit;
        private C1.Win.C1Input.C1Button btnRecover;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
    }
}
