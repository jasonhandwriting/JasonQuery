namespace JasonQuery.UI.Forms
{
    partial class OracleSidHelpForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OracleSidHelpForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.rtbSql = new System.Windows.Forms.RichTextBox();
            this.btnExit = new C1.Win.C1Input.C1Button();
            this.btnCopySql = new C1.Win.C1Input.C1Button();
            this.lblInfo = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopySql)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(18, 18);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(395, 32);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "You can execute the following SQL statement to obtain the SID value.\r\nRun this qu" +
    "ery on your Oracle connection:";
            // 
            // rtbSql
            // 
            this.rtbSql.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbSql.Location = new System.Drawing.Point(21, 59);
            this.rtbSql.Name = "rtbSql";
            this.rtbSql.ReadOnly = true;
            this.rtbSql.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.rtbSql.Size = new System.Drawing.Size(264, 43);
            this.rtbSql.TabIndex = 1;
            this.rtbSql.Text = "";
            // 
            // btnExit
            // 
            this.btnExit.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnExit.Location = new System.Drawing.Point(348, 109);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(66, 29);
            this.btnExit.TabIndex = 60;
            this.btnExit.Text = "OK";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnCopySql
            // 
            this.btnCopySql.Location = new System.Drawing.Point(240, 109);
            this.btnCopySql.Name = "btnCopySql";
            this.btnCopySql.Size = new System.Drawing.Size(89, 29);
            this.btnCopySql.TabIndex = 61;
            this.btnCopySql.Text = "Copy SQL";
            this.btnCopySql.UseVisualStyleBackColor = true;
            this.btnCopySql.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCopySql.Click += new System.EventHandler(this.btnCopySql_Click);
            // 
            // lblInfo
            // 
            this.lblInfo.AutoSize = true;
            this.lblInfo.ForeColor = System.Drawing.Color.Green;
            this.lblInfo.Location = new System.Drawing.Point(24, 114);
            this.lblInfo.Name = "lblInfo";
            this.lblInfo.Size = new System.Drawing.Size(0, 16);
            this.lblInfo.TabIndex = 62;
            this.lblInfo.Visible = false;
            // 
            // OracleSidHelpForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(438, 153);
            this.Controls.Add(this.lblInfo);
            this.Controls.Add(this.btnCopySql);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.rtbSql);
            this.Controls.Add(this.lblTitle);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "OracleSidHelpForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "How to obtain an Oracle SID";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopySql)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.RichTextBox rtbSql;
        private C1.Win.C1Input.C1Button btnExit;
        private C1.Win.C1Input.C1Button btnCopySql;
        private System.Windows.Forms.Label lblInfo;
    }
}