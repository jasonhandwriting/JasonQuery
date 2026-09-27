namespace JasonQuery.UI.Forms
{
    partial class JasonQueryDbRecoveryKeyForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(JasonQueryDbRecoveryKeyForm));
            this.lblRecoveryStatus = new System.Windows.Forms.Label();
            this.btnCreateOrRegenerate = new C1.Win.C1Input.C1Button();
            this.btnClose = new C1.Win.C1Input.C1Button();
            this.btnDisable = new C1.Win.C1Input.C1Button();
            this.lblInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateOrRegenerate)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDisable)).BeginInit();
            this.SuspendLayout();
            // 
            // lblRecoveryStatus
            // 
            this.lblRecoveryStatus.AutoSize = true;
            this.lblRecoveryStatus.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblRecoveryStatus.ForeColor = System.Drawing.Color.Navy;
            this.lblRecoveryStatus.Location = new System.Drawing.Point(25, 23);
            this.lblRecoveryStatus.Name = "lblRecoveryStatus";
            this.lblRecoveryStatus.Size = new System.Drawing.Size(184, 16);
            this.lblRecoveryStatus.TabIndex = 84;
            this.lblRecoveryStatus.Text = "Recovery Key: Not configured";
            // 
            // btnCreateOrRegenerate
            // 
            this.btnCreateOrRegenerate.Location = new System.Drawing.Point(19, 147);
            this.btnCreateOrRegenerate.Margin = new System.Windows.Forms.Padding(4);
            this.btnCreateOrRegenerate.Name = "btnCreateOrRegenerate";
            this.btnCreateOrRegenerate.Size = new System.Drawing.Size(245, 28);
            this.btnCreateOrRegenerate.TabIndex = 83;
            this.btnCreateOrRegenerate.Text = "Create Recovery Key...";
            this.btnCreateOrRegenerate.UseVisualStyleBackColor = true;
            this.btnCreateOrRegenerate.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCreateOrRegenerate.Click += new System.EventHandler(this.btnCreateOrRegenerate_Click);
            // 
            // btnClose
            // 
            this.btnClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnClose.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnClose.Location = new System.Drawing.Point(593, 147);
            this.btnClose.Margin = new System.Windows.Forms.Padding(4);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(96, 28);
            this.btnClose.TabIndex = 82;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // btnDisable
            // 
            this.btnDisable.Location = new System.Drawing.Point(287, 147);
            this.btnDisable.Margin = new System.Windows.Forms.Padding(4);
            this.btnDisable.Name = "btnDisable";
            this.btnDisable.Size = new System.Drawing.Size(245, 28);
            this.btnDisable.TabIndex = 81;
            this.btnDisable.Text = "Disable Recovery Key";
            this.btnDisable.UseVisualStyleBackColor = true;
            this.btnDisable.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnDisable.Click += new System.EventHandler(this.btnDisable_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblInfo.Location = new System.Drawing.Point(25, 53);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(659, 83);
            this.lblInfo.TabIndex = 85;
            this.lblInfo.Text = resources.GetString("lblInfo.Text");
            // 
            // JasonQueryDbRecoveryKeyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnClose;
            this.ClientSize = new System.Drawing.Size(709, 191);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.lblRecoveryStatus);
            this.Controls.Add(this.btnCreateOrRegenerate);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnDisable);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "JasonQueryDbRecoveryKeyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "JasonQuery.db Recovery Key";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnCreateOrRegenerate)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClose)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnDisable)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblRecoveryStatus;
        private C1.Win.C1Input.C1Button btnCreateOrRegenerate;
        private C1.Win.C1Input.C1Button btnClose;
        private C1.Win.C1Input.C1Button btnDisable;
        private System.Windows.Forms.Label lblInfo;
    }
}
