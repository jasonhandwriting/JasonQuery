namespace JasonQuery.UI.Forms
{
    sealed partial class UpdateForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdateForm));
            this.lblInfo = new System.Windows.Forms.Label();
            this.lblInfo2 = new System.Windows.Forms.Label();
            this.lnkCheck = new System.Windows.Forms.LinkLabel();
            this.grpDownloadInfo = new System.Windows.Forms.GroupBox();
            this.lblLength = new System.Windows.Forms.Label();
            this.btnHelp_HowToUpdate = new C1.Win.C1Input.C1Button();
            this.lnkDownloadJasonQuery64 = new System.Windows.Forms.LinkLabel();
            this.lblDownloadInfoWithout = new System.Windows.Forms.Label();
            this.grpCheckInfo = new System.Windows.Forms.GroupBox();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnCheckForUpdates = new C1.Win.C1Input.C1Button();
            this.grpUpdateNow = new System.Windows.Forms.GroupBox();
            this.btnUpdateNow = new C1.Win.C1Input.C1Button();
            this.lblUpdateNow = new System.Windows.Forms.Label();
            this.lnkDownloadJasonQuery64Test = new System.Windows.Forms.LinkLabel();
            this.grpDownloadInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_HowToUpdate)).BeginInit();
            this.grpCheckInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCheckForUpdates)).BeginInit();
            this.grpUpdateNow.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnUpdateNow)).BeginInit();
            this.SuspendLayout();
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblInfo.Location = new System.Drawing.Point(54, 21);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(250, 16);
            this.lblInfo.TabIndex = 1;
            this.lblInfo.Text = "A new version of JasonQuery is available:";
            this.lblInfo.Visible = false;
            // 
            // lblInfo2
            // 
            this.lblInfo2.AutoSize = true;
            this.lblInfo2.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblInfo2.Location = new System.Drawing.Point(54, 43);
            this.lblInfo2.Name = "lblInfo2";
            this.lblInfo2.Size = new System.Drawing.Size(31, 16);
            this.lblInfo2.TabIndex = 2;
            this.lblInfo2.Text = "0.01";
            this.lblInfo2.Visible = false;
            // 
            // lnkCheck
            // 
            this.lnkCheck.AutoSize = true;
            this.lnkCheck.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lnkCheck.Location = new System.Drawing.Point(23, 29);
            this.lnkCheck.Name = "lnkCheck";
            this.lnkCheck.Size = new System.Drawing.Size(305, 16);
            this.lnkCheck.TabIndex = 3;
            this.lnkCheck.TabStop = true;
            this.lnkCheck.Text = "http://www.jasonquery.org/JasonQueryUpdate/jq.txt";
            this.lnkCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheck_LinkClicked);
            // 
            // grpDownloadInfo
            // 
            this.grpDownloadInfo.Controls.Add(this.lnkDownloadJasonQuery64Test);
            this.grpDownloadInfo.Controls.Add(this.lblLength);
            this.grpDownloadInfo.Controls.Add(this.btnHelp_HowToUpdate);
            this.grpDownloadInfo.Controls.Add(this.lnkDownloadJasonQuery64);
            this.grpDownloadInfo.Controls.Add(this.lblDownloadInfoWithout);
            this.grpDownloadInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpDownloadInfo.Location = new System.Drawing.Point(15, 159);
            this.grpDownloadInfo.Name = "grpDownloadInfo";
            this.grpDownloadInfo.Size = new System.Drawing.Size(430, 82);
            this.grpDownloadInfo.TabIndex = 11;
            this.grpDownloadInfo.TabStop = false;
            this.grpDownloadInfo.Text = "Download latest version manually";
            // 
            // lblLength
            // 
            this.lblLength.AutoSize = true;
            this.lblLength.Location = new System.Drawing.Point(406, 19);
            this.lblLength.Name = "lblLength";
            this.lblLength.Size = new System.Drawing.Size(0, 16);
            this.lblLength.TabIndex = 86;
            this.lblLength.Visible = false;
            // 
            // btnHelp_HowToUpdate
            // 
            this.btnHelp_HowToUpdate.Image = ((System.Drawing.Image)(resources.GetObject("btnHelp_HowToUpdate.Image")));
            this.btnHelp_HowToUpdate.Location = new System.Drawing.Point(266, 0);
            this.btnHelp_HowToUpdate.Name = "btnHelp_HowToUpdate";
            this.btnHelp_HowToUpdate.Size = new System.Drawing.Size(21, 19);
            this.btnHelp_HowToUpdate.TabIndex = 6;
            this.btnHelp_HowToUpdate.UseVisualStyleBackColor = true;
            this.btnHelp_HowToUpdate.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnHelp_HowToUpdate.Click += new System.EventHandler(this.btnHelp_HowToUpdate_Click);
            // 
            // lnkDownloadJasonQuery64
            // 
            this.lnkDownloadJasonQuery64.AutoSize = true;
            this.lnkDownloadJasonQuery64.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lnkDownloadJasonQuery64.Location = new System.Drawing.Point(23, 53);
            this.lnkDownloadJasonQuery64.Name = "lnkDownloadJasonQuery64";
            this.lnkDownloadJasonQuery64.Size = new System.Drawing.Size(107, 16);
            this.lnkDownloadJasonQuery64.TabIndex = 4;
            this.lnkDownloadJasonQuery64.TabStop = true;
            this.lnkDownloadJasonQuery64.Tag = "";
            this.lnkDownloadJasonQuery64.Text = "JasonQuery64.zip";
            this.lnkDownloadJasonQuery64.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDownloadJasonQuery64_LinkClicked);
            // 
            // lblDownloadInfoWithout
            // 
            this.lblDownloadInfoWithout.AutoSize = true;
            this.lblDownloadInfoWithout.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblDownloadInfoWithout.Location = new System.Drawing.Point(23, 29);
            this.lblDownloadInfoWithout.Name = "lblDownloadInfoWithout";
            this.lblDownloadInfoWithout.Size = new System.Drawing.Size(204, 16);
            this.lblDownloadInfoWithout.TabIndex = 11;
            this.lblDownloadInfoWithout.Text = "Zip package: portable, multilingual";
            // 
            // grpCheckInfo
            // 
            this.grpCheckInfo.Controls.Add(this.lnkCheck);
            this.grpCheckInfo.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpCheckInfo.Location = new System.Drawing.Point(15, 88);
            this.grpCheckInfo.Name = "grpCheckInfo";
            this.grpCheckInfo.Size = new System.Drawing.Size(430, 63);
            this.grpCheckInfo.TabIndex = 12;
            this.grpCheckInfo.TabStop = false;
            this.grpCheckInfo.Text = "Check for updates manually";
            // 
            // btnClose
            // 
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(379, 359);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(66, 29);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCheckForUpdates
            // 
            this.btnCheckForUpdates.Location = new System.Drawing.Point(124, 32);
            this.btnCheckForUpdates.Name = "btnCheckForUpdates";
            this.btnCheckForUpdates.Size = new System.Drawing.Size(209, 29);
            this.btnCheckForUpdates.TabIndex = 1;
            this.btnCheckForUpdates.Text = "Check for updates now";
            this.btnCheckForUpdates.UseVisualStyleBackColor = true;
            this.btnCheckForUpdates.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCheckForUpdates.Click += new System.EventHandler(this.btnCheckForUpdates_Click);
            // 
            // grpUpdateNow
            // 
            this.grpUpdateNow.Controls.Add(this.btnUpdateNow);
            this.grpUpdateNow.Controls.Add(this.lblUpdateNow);
            this.grpUpdateNow.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.grpUpdateNow.ForeColor = System.Drawing.Color.Black;
            this.grpUpdateNow.Location = new System.Drawing.Point(15, 248);
            this.grpUpdateNow.Name = "grpUpdateNow";
            this.grpUpdateNow.Size = new System.Drawing.Size(430, 100);
            this.grpUpdateNow.TabIndex = 13;
            this.grpUpdateNow.TabStop = false;
            this.grpUpdateNow.Text = "Automatically download and update";
            // 
            // btnUpdateNow
            // 
            this.btnUpdateNow.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnUpdateNow.Location = new System.Drawing.Point(26, 28);
            this.btnUpdateNow.Name = "btnUpdateNow";
            this.btnUpdateNow.Size = new System.Drawing.Size(241, 29);
            this.btnUpdateNow.TabIndex = 15;
            this.btnUpdateNow.Text = "Update Now (One-Click update)";
            this.btnUpdateNow.UseVisualStyleBackColor = true;
            this.btnUpdateNow.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2007Blue;
            this.btnUpdateNow.Click += new System.EventHandler(this.btnUpdateNow_Click);
            // 
            // lblUpdateNow
            // 
            this.lblUpdateNow.AutoSize = true;
            this.lblUpdateNow.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblUpdateNow.ForeColor = System.Drawing.Color.Black;
            this.lblUpdateNow.Location = new System.Drawing.Point(23, 66);
            this.lblUpdateNow.Name = "lblUpdateNow";
            this.lblUpdateNow.Size = new System.Drawing.Size(312, 16);
            this.lblUpdateNow.TabIndex = 11;
            this.lblUpdateNow.Text = "Call Updater.exe to download and update lastest files.";
            // 
            // lnkDownloadJasonQuery64Test
            // 
            this.lnkDownloadJasonQuery64Test.AutoSize = true;
            this.lnkDownloadJasonQuery64Test.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lnkDownloadJasonQuery64Test.Location = new System.Drawing.Point(170, 53);
            this.lnkDownloadJasonQuery64Test.Name = "lnkDownloadJasonQuery64Test";
            this.lnkDownloadJasonQuery64Test.Size = new System.Drawing.Size(130, 16);
            this.lnkDownloadJasonQuery64Test.TabIndex = 87;
            this.lnkDownloadJasonQuery64Test.TabStop = true;
            this.lnkDownloadJasonQuery64Test.Tag = "";
            this.lnkDownloadJasonQuery64Test.Text = "JasonQuery64Test.zip";
            this.lnkDownloadJasonQuery64Test.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkDownloadJasonQuery64Test_LinkClicked);
            // 
            // UpdateForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(459, 402);
            this.Controls.Add(this.grpUpdateNow);
            this.Controls.Add(this.btnCheckForUpdates);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.grpCheckInfo);
            this.Controls.Add(this.grpDownloadInfo);
            this.Controls.Add(this.lblInfo2);
            this.Controls.Add(this.lblInfo);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdateForm";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Check for updates";
            this.Load += new System.EventHandler(this.Form_Load);
            this.grpDownloadInfo.ResumeLayout(false);
            this.grpDownloadInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnHelp_HowToUpdate)).EndInit();
            this.grpCheckInfo.ResumeLayout(false);
            this.grpCheckInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCheckForUpdates)).EndInit();
            this.grpUpdateNow.ResumeLayout(false);
            this.grpUpdateNow.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.btnUpdateNow)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblInfo;
        private System.Windows.Forms.Label lblInfo2;
        private System.Windows.Forms.LinkLabel lnkCheck;
        private System.Windows.Forms.GroupBox grpDownloadInfo;
        private System.Windows.Forms.Label lblDownloadInfoWithout;
        private System.Windows.Forms.GroupBox grpCheckInfo;
        private System.Windows.Forms.LinkLabel lnkDownloadJasonQuery64;
        private C1.Win.C1Input.C1Button btnHelp_HowToUpdate;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnCheckForUpdates;
        private System.Windows.Forms.GroupBox grpUpdateNow;
        private System.Windows.Forms.Label lblUpdateNow;
        private C1.Win.C1Input.C1Button btnUpdateNow;
        private System.Windows.Forms.Label lblLength;
        private System.Windows.Forms.LinkLabel lnkDownloadJasonQuery64Test;
    }
}