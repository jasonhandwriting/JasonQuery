namespace Updater
{
    partial class UpdaterForm
    {
        /// <summary>
        /// 設計工具所需的變數。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清除任何使用中的資源。
        /// </summary>
        /// <param name="disposing">如果應該處置受控資源則為 true，否則為 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 設計工具產生的程式碼

        /// <summary>
        /// 此為設計工具支援所需的方法 - 請勿使用程式碼編輯器修改
        /// 這個方法的內容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UpdaterForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblStep1 = new System.Windows.Forms.Label();
            this.picStep1 = new System.Windows.Forms.PictureBox();
            this.lblStep4 = new System.Windows.Forms.Label();
            this.tmrCheck = new System.Windows.Forms.Timer(this.components);
            this.picChecked = new System.Windows.Forms.PictureBox();
            this.picUnchecked = new System.Windows.Forms.PictureBox();
            this.picStep4 = new System.Windows.Forms.PictureBox();
            this.lblFrom = new System.Windows.Forms.Label();
            this.lblTo = new System.Windows.Forms.Label();
            this.picStep2 = new System.Windows.Forms.PictureBox();
            this.lblStep2 = new System.Windows.Forms.Label();
            this.pbDownloadStatus = new System.Windows.Forms.ProgressBar();
            this.chkLaunchJQ = new System.Windows.Forms.CheckBox();
            this.rdoRelease = new System.Windows.Forms.RadioButton();
            this.rdoPreview = new System.Windows.Forms.RadioButton();
            this.grpVersion = new System.Windows.Forms.GroupBox();
            this.lblStep3 = new System.Windows.Forms.Label();
            this.picStep3 = new System.Windows.Forms.PictureBox();
            this.chkClose = new System.Windows.Forms.CheckBox();
            this.grpComplete = new System.Windows.Forms.GroupBox();
            this.txtStep2 = new System.Windows.Forms.RichTextBox();
            this.txtStep3 = new System.Windows.Forms.RichTextBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.txtFrom = new System.Windows.Forms.TextBox();
            this.txtTo = new System.Windows.Forms.TextBox();
            this.txtList = new System.Windows.Forms.TextBox();
            this.grpUpdateSource = new System.Windows.Forms.GroupBox();
            this.rdoJasonQueryOfficial = new System.Windows.Forms.RadioButton();
            this.rdoGitHubOfficial = new System.Windows.Forms.RadioButton();
            this.rdoLocal = new System.Windows.Forms.RadioButton();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.btnBrowseLocalFolder = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.picStep1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChecked)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUnchecked)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStep4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStep2)).BeginInit();
            this.grpVersion.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStep3)).BeginInit();
            this.grpComplete.SuspendLayout();
            this.grpUpdateSource.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.Location = new System.Drawing.Point(15, 15);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(141, 16);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Download and Update";
            // 
            // lblStep1
            // 
            this.lblStep1.AutoSize = true;
            this.lblStep1.ForeColor = System.Drawing.Color.Black;
            this.lblStep1.Location = new System.Drawing.Point(40, 158);
            this.lblStep1.Name = "lblStep1";
            this.lblStep1.Size = new System.Drawing.Size(264, 16);
            this.lblStep1.TabIndex = 1;
            this.lblStep1.Text = "Step 1: Manually close all running JasonQuery";
            // 
            // picStep1
            // 
            this.picStep1.Location = new System.Drawing.Point(20, 158);
            this.picStep1.Name = "picStep1";
            this.picStep1.Size = new System.Drawing.Size(16, 16);
            this.picStep1.TabIndex = 2;
            this.picStep1.TabStop = false;
            // 
            // lblStep4
            // 
            this.lblStep4.AutoSize = true;
            this.lblStep4.Location = new System.Drawing.Point(40, 350);
            this.lblStep4.Name = "lblStep4";
            this.lblStep4.Size = new System.Drawing.Size(272, 16);
            this.lblStep4.TabIndex = 3;
            this.lblStep4.Text = "Step 4: Update files and Delete temporary filles";
            // 
            // tmrCheck
            // 
            this.tmrCheck.Interval = 4000;
            this.tmrCheck.Tick += new System.EventHandler(this.tmrCheck_Tick);
            // 
            // picChecked
            // 
            this.picChecked.Image = ((System.Drawing.Image)(resources.GetObject("picChecked.Image")));
            this.picChecked.Location = new System.Drawing.Point(505, 366);
            this.picChecked.Name = "picChecked";
            this.picChecked.Size = new System.Drawing.Size(16, 16);
            this.picChecked.TabIndex = 87;
            this.picChecked.TabStop = false;
            this.picChecked.Visible = false;
            // 
            // picUnchecked
            // 
            this.picUnchecked.Image = ((System.Drawing.Image)(resources.GetObject("picUnchecked.Image")));
            this.picUnchecked.Location = new System.Drawing.Point(527, 366);
            this.picUnchecked.Name = "picUnchecked";
            this.picUnchecked.Size = new System.Drawing.Size(16, 16);
            this.picUnchecked.TabIndex = 88;
            this.picUnchecked.TabStop = false;
            this.picUnchecked.Visible = false;
            // 
            // picStep4
            // 
            this.picStep4.Location = new System.Drawing.Point(20, 350);
            this.picStep4.Name = "picStep4";
            this.picStep4.Size = new System.Drawing.Size(16, 16);
            this.picStep4.TabIndex = 89;
            this.picStep4.TabStop = false;
            // 
            // lblFrom
            // 
            this.lblFrom.AutoSize = true;
            this.lblFrom.Location = new System.Drawing.Point(40, 373);
            this.lblFrom.Name = "lblFrom";
            this.lblFrom.Size = new System.Drawing.Size(39, 16);
            this.lblFrom.TabIndex = 92;
            this.lblFrom.Text = "From:";
            // 
            // lblTo
            // 
            this.lblTo.AutoSize = true;
            this.lblTo.Location = new System.Drawing.Point(40, 419);
            this.lblTo.Name = "lblTo";
            this.lblTo.Size = new System.Drawing.Size(251, 16);
            this.lblTo.TabIndex = 94;
            this.lblTo.Text = "To: (The path where JasonQuery.exe exists.)";
            // 
            // picStep2
            // 
            this.picStep2.Location = new System.Drawing.Point(20, 276);
            this.picStep2.Name = "picStep2";
            this.picStep2.Size = new System.Drawing.Size(16, 16);
            this.picStep2.TabIndex = 96;
            this.picStep2.TabStop = false;
            // 
            // lblStep2
            // 
            this.lblStep2.AutoSize = true;
            this.lblStep2.Location = new System.Drawing.Point(40, 276);
            this.lblStep2.Name = "lblStep2";
            this.lblStep2.Size = new System.Drawing.Size(311, 16);
            this.lblStep2.TabIndex = 95;
            this.lblStep2.Text = "Step 2: Download JasonQuery.zip to temporary folder";
            this.lblStep2.Visible = false;
            // 
            // pbDownloadStatus
            // 
            this.pbDownloadStatus.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbDownloadStatus.Location = new System.Drawing.Point(43, 296);
            this.pbDownloadStatus.Name = "pbDownloadStatus";
            this.pbDownloadStatus.Size = new System.Drawing.Size(552, 21);
            this.pbDownloadStatus.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbDownloadStatus.TabIndex = 97;
            // 
            // chkLaunchJQ
            // 
            this.chkLaunchJQ.AutoSize = true;
            this.chkLaunchJQ.Checked = true;
            this.chkLaunchJQ.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkLaunchJQ.Location = new System.Drawing.Point(20, 22);
            this.chkLaunchJQ.Name = "chkLaunchJQ";
            this.chkLaunchJQ.Size = new System.Drawing.Size(135, 20);
            this.chkLaunchJQ.TabIndex = 98;
            this.chkLaunchJQ.Text = "Launch JasonQuery";
            this.chkLaunchJQ.UseVisualStyleBackColor = true;
            // 
            // rdoRelease
            // 
            this.rdoRelease.AutoSize = true;
            this.rdoRelease.Location = new System.Drawing.Point(20, 22);
            this.rdoRelease.Name = "rdoRelease";
            this.rdoRelease.Size = new System.Drawing.Size(69, 20);
            this.rdoRelease.TabIndex = 106;
            this.rdoRelease.Tag = "";
            this.rdoRelease.Text = "Release";
            this.rdoRelease.UseVisualStyleBackColor = false;
            this.rdoRelease.CheckedChanged += new System.EventHandler(this.rdoUpdateProduction_CheckedChanged);
            // 
            // rdoPreview
            // 
            this.rdoPreview.AutoSize = true;
            this.rdoPreview.Enabled = false;
            this.rdoPreview.Location = new System.Drawing.Point(20, 48);
            this.rdoPreview.Name = "rdoPreview";
            this.rdoPreview.Size = new System.Drawing.Size(68, 20);
            this.rdoPreview.TabIndex = 107;
            this.rdoPreview.Tag = "";
            this.rdoPreview.Text = "Preview";
            this.rdoPreview.UseVisualStyleBackColor = false;
            this.rdoPreview.CheckedChanged += new System.EventHandler(this.rdoUpdateBeta_CheckedChanged);
            // 
            // grpVersion
            // 
            this.grpVersion.Controls.Add(this.rdoRelease);
            this.grpVersion.Controls.Add(this.rdoPreview);
            this.grpVersion.Location = new System.Drawing.Point(303, 471);
            this.grpVersion.Name = "grpVersion";
            this.grpVersion.Size = new System.Drawing.Size(132, 75);
            this.grpVersion.TabIndex = 104;
            this.grpVersion.TabStop = false;
            this.grpVersion.Text = "Version";
            // 
            // lblStep3
            // 
            this.lblStep3.AutoSize = true;
            this.lblStep3.BackColor = System.Drawing.SystemColors.Control;
            this.lblStep3.ForeColor = System.Drawing.Color.Black;
            this.lblStep3.Location = new System.Drawing.Point(40, 327);
            this.lblStep3.Name = "lblStep3";
            this.lblStep3.Size = new System.Drawing.Size(285, 16);
            this.lblStep3.TabIndex = 100;
            this.lblStep3.Text = "Step 3: Unzip JasonQuery.zip to temporary folder";
            this.lblStep3.Visible = false;
            // 
            // picStep3
            // 
            this.picStep3.Location = new System.Drawing.Point(20, 327);
            this.picStep3.Name = "picStep3";
            this.picStep3.Size = new System.Drawing.Size(16, 16);
            this.picStep3.TabIndex = 101;
            this.picStep3.TabStop = false;
            // 
            // chkClose
            // 
            this.chkClose.AutoSize = true;
            this.chkClose.Checked = true;
            this.chkClose.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClose.Enabled = false;
            this.chkClose.Location = new System.Drawing.Point(20, 48);
            this.chkClose.Name = "chkClose";
            this.chkClose.Size = new System.Drawing.Size(131, 20);
            this.chkClose.TabIndex = 102;
            this.chkClose.Text = "Close the program";
            // 
            // grpComplete
            // 
            this.grpComplete.BackColor = System.Drawing.SystemColors.Control;
            this.grpComplete.Controls.Add(this.chkLaunchJQ);
            this.grpComplete.Controls.Add(this.chkClose);
            this.grpComplete.ForeColor = System.Drawing.Color.Black;
            this.grpComplete.Location = new System.Drawing.Point(43, 471);
            this.grpComplete.Name = "grpComplete";
            this.grpComplete.Size = new System.Drawing.Size(248, 75);
            this.grpComplete.TabIndex = 103;
            this.grpComplete.TabStop = false;
            this.grpComplete.Text = "After the update is complete";
            // 
            // txtStep2
            // 
            this.txtStep2.BackColor = System.Drawing.SystemColors.Control;
            this.txtStep2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStep2.Location = new System.Drawing.Point(43, 276);
            this.txtStep2.Name = "txtStep2";
            this.txtStep2.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtStep2.Size = new System.Drawing.Size(528, 16);
            this.txtStep2.TabIndex = 108;
            this.txtStep2.Text = "";
            // 
            // txtStep3
            // 
            this.txtStep3.BackColor = System.Drawing.SystemColors.Control;
            this.txtStep3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStep3.Location = new System.Drawing.Point(43, 327);
            this.txtStep3.Name = "txtStep3";
            this.txtStep3.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.txtStep3.Size = new System.Drawing.Size(528, 16);
            this.txtStep3.TabIndex = 109;
            this.txtStep3.Text = "";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(601, 295);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(63, 23);
            this.btnCancel.TabIndex = 112;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnUpdate.Location = new System.Drawing.Point(451, 473);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(124, 73);
            this.btnUpdate.TabIndex = 113;
            this.btnUpdate.Text = "&Update";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnDownload.Location = new System.Drawing.Point(588, 473);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(75, 32);
            this.btnDownload.TabIndex = 114;
            this.btnDownload.Text = "&Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.Location = new System.Drawing.Point(588, 514);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 32);
            this.btnClose.TabIndex = 115;
            this.btnClose.Text = "&Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // txtFrom
            // 
            this.txtFrom.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFrom.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtFrom.Location = new System.Drawing.Point(43, 391);
            this.txtFrom.Name = "txtFrom";
            this.txtFrom.Size = new System.Drawing.Size(621, 22);
            this.txtFrom.TabIndex = 116;
            // 
            // txtTo
            // 
            this.txtTo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTo.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.txtTo.Location = new System.Drawing.Point(43, 437);
            this.txtTo.Name = "txtTo";
            this.txtTo.Size = new System.Drawing.Size(621, 22);
            this.txtTo.TabIndex = 117;
            // 
            // txtList
            // 
            this.txtList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtList.Location = new System.Drawing.Point(43, 177);
            this.txtList.Multiline = true;
            this.txtList.Name = "txtList";
            this.txtList.Size = new System.Drawing.Size(621, 86);
            this.txtList.TabIndex = 118;
            // 
            // grpUpdateSource
            // 
            this.grpUpdateSource.Controls.Add(this.btnBrowseLocalFolder);
            this.grpUpdateSource.Controls.Add(this.textBox1);
            this.grpUpdateSource.Controls.Add(this.rdoLocal);
            this.grpUpdateSource.Controls.Add(this.rdoGitHubOfficial);
            this.grpUpdateSource.Controls.Add(this.rdoJasonQueryOfficial);
            this.grpUpdateSource.Location = new System.Drawing.Point(20, 39);
            this.grpUpdateSource.Name = "grpUpdateSource";
            this.grpUpdateSource.Size = new System.Drawing.Size(643, 106);
            this.grpUpdateSource.TabIndex = 119;
            this.grpUpdateSource.TabStop = false;
            this.grpUpdateSource.Text = "Update Source";
            // 
            // rdoJasonQueryOfficial
            // 
            this.rdoJasonQueryOfficial.AutoSize = true;
            this.rdoJasonQueryOfficial.Location = new System.Drawing.Point(20, 22);
            this.rdoJasonQueryOfficial.Name = "rdoJasonQueryOfficial";
            this.rdoJasonQueryOfficial.Size = new System.Drawing.Size(134, 20);
            this.rdoJasonQueryOfficial.TabIndex = 107;
            this.rdoJasonQueryOfficial.Tag = "";
            this.rdoJasonQueryOfficial.Text = "JasonQuery Official";
            this.rdoJasonQueryOfficial.UseVisualStyleBackColor = false;
            // 
            // rdoGitHubOfficial
            // 
            this.rdoGitHubOfficial.AutoSize = true;
            this.rdoGitHubOfficial.Location = new System.Drawing.Point(20, 48);
            this.rdoGitHubOfficial.Name = "rdoGitHubOfficial";
            this.rdoGitHubOfficial.Size = new System.Drawing.Size(108, 20);
            this.rdoGitHubOfficial.TabIndex = 108;
            this.rdoGitHubOfficial.Tag = "";
            this.rdoGitHubOfficial.Text = "GitHub Official";
            this.rdoGitHubOfficial.UseVisualStyleBackColor = false;
            // 
            // rdoLocal
            // 
            this.rdoLocal.AutoSize = true;
            this.rdoLocal.Location = new System.Drawing.Point(20, 74);
            this.rdoLocal.Name = "rdoLocal";
            this.rdoLocal.Size = new System.Drawing.Size(58, 20);
            this.rdoLocal.TabIndex = 109;
            this.rdoLocal.Tag = "";
            this.rdoLocal.Text = "Local:";
            this.rdoLocal.UseVisualStyleBackColor = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Font = new System.Drawing.Font("微軟正黑體", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.textBox1.Location = new System.Drawing.Point(80, 74);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(518, 22);
            this.textBox1.TabIndex = 117;
            // 
            // btnBrowseLocalFolder
            // 
            this.btnBrowseLocalFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBrowseLocalFolder.Font = new System.Drawing.Font("Arial Narrow", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnBrowseLocalFolder.Location = new System.Drawing.Point(604, 73);
            this.btnBrowseLocalFolder.Name = "btnBrowseLocalFolder";
            this.btnBrowseLocalFolder.Size = new System.Drawing.Size(25, 24);
            this.btnBrowseLocalFolder.TabIndex = 118;
            this.btnBrowseLocalFolder.Text = "...";
            this.btnBrowseLocalFolder.UseVisualStyleBackColor = true;
            // 
            // UpdaterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(685, 561);
            this.Controls.Add(this.grpUpdateSource);
            this.Controls.Add(this.txtList);
            this.Controls.Add(this.txtTo);
            this.Controls.Add(this.txtFrom);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.grpVersion);
            this.Controls.Add(this.txtStep3);
            this.Controls.Add(this.txtStep2);
            this.Controls.Add(this.grpComplete);
            this.Controls.Add(this.picStep3);
            this.Controls.Add(this.lblStep3);
            this.Controls.Add(this.pbDownloadStatus);
            this.Controls.Add(this.picStep2);
            this.Controls.Add(this.lblStep2);
            this.Controls.Add(this.lblTo);
            this.Controls.Add(this.lblFrom);
            this.Controls.Add(this.picStep4);
            this.Controls.Add(this.picUnchecked);
            this.Controls.Add(this.picChecked);
            this.Controls.Add(this.lblStep4);
            this.Controls.Add(this.picStep1);
            this.Controls.Add(this.lblStep1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.btnDownload);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "UpdaterForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Updater - JasonQuery";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmUpdater_FormClosing);
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picStep1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picChecked)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picUnchecked)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStep4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.picStep2)).EndInit();
            this.grpVersion.ResumeLayout(false);
            this.grpVersion.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picStep3)).EndInit();
            this.grpComplete.ResumeLayout(false);
            this.grpComplete.PerformLayout();
            this.grpUpdateSource.ResumeLayout(false);
            this.grpUpdateSource.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblStep1;
        private System.Windows.Forms.PictureBox picStep1;
        private System.Windows.Forms.Label lblStep4;
        private System.Windows.Forms.Timer tmrCheck;
        private System.Windows.Forms.PictureBox picChecked;
        private System.Windows.Forms.PictureBox picUnchecked;
        private System.Windows.Forms.PictureBox picStep4;
        private System.Windows.Forms.Label lblFrom;
        private System.Windows.Forms.Label lblTo;
        private System.Windows.Forms.PictureBox picStep2;
        private System.Windows.Forms.Label lblStep2;
        private System.Windows.Forms.ProgressBar pbDownloadStatus;
        private System.Windows.Forms.CheckBox chkLaunchJQ;
        private System.Windows.Forms.PictureBox picStep3;
        private System.Windows.Forms.Label lblStep3;
        private System.Windows.Forms.CheckBox chkClose;
        private System.Windows.Forms.GroupBox grpComplete;
        private System.Windows.Forms.RadioButton rdoRelease;
        private System.Windows.Forms.RadioButton rdoPreview;
        private System.Windows.Forms.RichTextBox txtStep2;
        private System.Windows.Forms.RichTextBox txtStep3;
        private System.Windows.Forms.GroupBox grpVersion;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.TextBox txtFrom;
        private System.Windows.Forms.TextBox txtTo;
        private System.Windows.Forms.TextBox txtList;
        private System.Windows.Forms.GroupBox grpUpdateSource;
        private System.Windows.Forms.Button btnBrowseLocalFolder;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.RadioButton rdoLocal;
        private System.Windows.Forms.RadioButton rdoGitHubOfficial;
        private System.Windows.Forms.RadioButton rdoJasonQueryOfficial;
    }
}

