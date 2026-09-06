namespace JasonQuery.UI.Forms
{
    partial class DatabaseSecurityStartupErrorForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DatabaseSecurityStartupErrorForm));
            this.picError = new System.Windows.Forms.PictureBox();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblMissingReason = new System.Windows.Forms.Label();
            this.lblMissingRecovery = new System.Windows.Forms.Label();
            this.lblGeneralReason = new System.Windows.Forms.Label();
            this.lblGeneralRecovery = new System.Windows.Forms.Label();
            this.lblNoMigration = new System.Windows.Forms.Label();
            this.grpLocalization = new System.Windows.Forms.GroupBox();
            this.cboLocalization = new C1.Win.C1Input.C1ComboBox();
            this.btnExit = new C1.Win.C1Input.C1Button();
            ((System.ComponentModel.ISupportInitialize)(this.picError)).BeginInit();
            this.grpLocalization.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).BeginInit();
            this.SuspendLayout();
            //
            // picError
            //
            this.picError.Image = ((System.Drawing.Image)(resources.GetObject("picError.Image")));
            this.picError.Location = new System.Drawing.Point(20, 20);
            this.picError.Name = "picError";
            this.picError.Size = new System.Drawing.Size(32, 32);
            this.picError.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picError.TabIndex = 0;
            this.picError.TabStop = false;
            //
            // lblTitle
            //
            this.lblTitle.Font = new System.Drawing.Font("微軟正黑體", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblTitle.Location = new System.Drawing.Point(65, 20);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(565, 34);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "JasonQuery could not safely open its internal database.";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            //
            // lblMissingReason
            //
            this.lblMissingReason.Location = new System.Drawing.Point(20, 70);
            this.lblMissingReason.Name = "lblMissingReason";
            this.lblMissingReason.Size = new System.Drawing.Size(610, 55);
            this.lblMissingReason.TabIndex = 2;
            this.lblMissingReason.Text = "The database security information required to open JasonQuery.db is missing, or t" +
    "he database still uses a custom password created by an earlier version of JasonQ" +
    "uery.";
            //
            // lblMissingRecovery
            //
            this.lblMissingRecovery.Location = new System.Drawing.Point(20, 130);
            this.lblMissingRecovery.Name = "lblMissingRecovery";
            this.lblMissingRecovery.Size = new System.Drawing.Size(610, 85);
            this.lblMissingRecovery.TabIndex = 3;
            this.lblMissingRecovery.Text = resources.GetString("lblMissingRecovery.Text");
            //
            // lblGeneralReason
            //
            this.lblGeneralReason.Location = new System.Drawing.Point(20, 70);
            this.lblGeneralReason.Name = "lblGeneralReason";
            this.lblGeneralReason.Size = new System.Drawing.Size(610, 55);
            this.lblGeneralReason.TabIndex = 4;
            this.lblGeneralReason.Text = "JasonQuery detected a problem while checking the security information for its int" +
    "ernal database.";
            //
            // lblGeneralRecovery
            //
            this.lblGeneralRecovery.Location = new System.Drawing.Point(20, 130);
            this.lblGeneralRecovery.Name = "lblGeneralRecovery";
            this.lblGeneralRecovery.Size = new System.Drawing.Size(610, 85);
            this.lblGeneralRecovery.TabIndex = 5;
            this.lblGeneralRecovery.Text = "JasonQuery has stopped to protect the database. Restore the matching JasonQuery.d" +
    "b and JasonQuery.security.json files from a known-good backup, or contact suppor" +
    "t before trying again.";
            //
            // lblNoMigration
            //
            this.lblNoMigration.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblNoMigration.Location = new System.Drawing.Point(20, 220);
            this.lblNoMigration.Name = "lblNoMigration";
            this.lblNoMigration.Size = new System.Drawing.Size(610, 38);
            this.lblNoMigration.TabIndex = 6;
            this.lblNoMigration.Text = "No database migration was performed and JasonQuery.db was not modified.";
            //
            // grpLocalization
            //
            this.grpLocalization.Controls.Add(this.cboLocalization);
            this.grpLocalization.Location = new System.Drawing.Point(20, 270);
            this.grpLocalization.Name = "grpLocalization";
            this.grpLocalization.Size = new System.Drawing.Size(329, 55);
            this.grpLocalization.TabIndex = 7;
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
            this.cboLocalization.Size = new System.Drawing.Size(289, 21);
            this.cboLocalization.TabIndex = 0;
            this.cboLocalization.TextDetached = true;
            this.cboLocalization.Tag = null;
            this.cboLocalization.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboLocalization.SelectedIndexChanged += new System.EventHandler(this.cboLocalization_SelectedIndexChanged);
            //
            // btnExit
            //
            this.btnExit.Location = new System.Drawing.Point(483, 288);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(147, 35);
            this.btnExit.TabIndex = 8;
            this.btnExit.Text = "Exit JasonQuery";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            //
            // DatabaseSecurityStartupErrorForm
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(650, 343);
            this.ControlBox = false;
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.grpLocalization);
            this.Controls.Add(this.lblNoMigration);
            this.Controls.Add(this.lblGeneralRecovery);
            this.Controls.Add(this.lblGeneralReason);
            this.Controls.Add(this.lblMissingRecovery);
            this.Controls.Add(this.lblMissingReason);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.picError);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DatabaseSecurityStartupErrorForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Security for JasonQuery.db";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.picError)).EndInit();
            this.grpLocalization.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.cboLocalization)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnExit)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox picError;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblMissingReason;
        private System.Windows.Forms.Label lblMissingRecovery;
        private System.Windows.Forms.Label lblGeneralReason;
        private System.Windows.Forms.Label lblGeneralRecovery;
        private System.Windows.Forms.Label lblNoMigration;
        private System.Windows.Forms.GroupBox grpLocalization;
        private C1.Win.C1Input.C1ComboBox cboLocalization;
        private C1.Win.C1Input.C1Button btnExit;
    }
}
