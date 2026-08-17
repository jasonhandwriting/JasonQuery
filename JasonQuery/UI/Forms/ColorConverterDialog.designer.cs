namespace JasonQuery.UI.Forms
{
    partial class ColorConverterDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColorConverterDialog));
            this.pnlColor = new System.Windows.Forms.Panel();
            this.lblR = new System.Windows.Forms.Label();
            this.rdoPickColor = new System.Windows.Forms.RadioButton();
            this.rdoHtmlColor = new System.Windows.Forms.RadioButton();
            this.rdoRgbColor = new System.Windows.Forms.RadioButton();
            this.lblG = new System.Windows.Forms.Label();
            this.lblB = new System.Windows.Forms.Label();
            this.nudR = new System.Windows.Forms.NumericUpDown();
            this.nudG = new System.Windows.Forms.NumericUpDown();
            this.nudB = new System.Windows.Forms.NumericUpDown();
            this.txtHtmlColor = new C1.Win.C1Input.C1TextBox();
            this.txtRgbColor = new C1.Win.C1Input.C1TextBox();
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnCopy_HTML = new C1.Win.C1Input.C1Button();
            this.btnCopy_RGB = new C1.Win.C1Input.C1Button();
            this.lblInvalidColorCode = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.nudR)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudG)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudB)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtHtmlColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRgbColor)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy_HTML)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy_RGB)).BeginInit();
            this.SuspendLayout();
            // 
            // pnlColor
            // 
            this.pnlColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.pnlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlColor.Location = new System.Drawing.Point(126, 17);
            this.pnlColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pnlColor.Name = "pnlColor";
            this.pnlColor.Size = new System.Drawing.Size(74, 24);
            this.pnlColor.TabIndex = 45;
            this.pnlColor.Click += new System.EventHandler(this.SelectedColorClick);
            // 
            // lblR
            // 
            this.lblR.AutoSize = true;
            this.lblR.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblR.Location = new System.Drawing.Point(21, 117);
            this.lblR.Name = "lblR";
            this.lblR.Size = new System.Drawing.Size(18, 16);
            this.lblR.TabIndex = 48;
            this.lblR.Text = "R:";
            this.lblR.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // rdoPickColor
            // 
            this.rdoPickColor.AutoSize = true;
            this.rdoPickColor.Checked = true;
            this.rdoPickColor.Location = new System.Drawing.Point(22, 19);
            this.rdoPickColor.Name = "rdoPickColor";
            this.rdoPickColor.Size = new System.Drawing.Size(84, 20);
            this.rdoPickColor.TabIndex = 0;
            this.rdoPickColor.TabStop = true;
            this.rdoPickColor.Text = "Pick Color:";
            this.rdoPickColor.UseVisualStyleBackColor = true;
            // 
            // rdoHtmlColor
            // 
            this.rdoHtmlColor.AutoSize = true;
            this.rdoHtmlColor.Location = new System.Drawing.Point(22, 53);
            this.rdoHtmlColor.Name = "rdoHtmlColor";
            this.rdoHtmlColor.Size = new System.Drawing.Size(73, 20);
            this.rdoHtmlColor.TabIndex = 1;
            this.rdoHtmlColor.Text = "HTML: #";
            this.rdoHtmlColor.UseVisualStyleBackColor = true;
            // 
            // rdoRgbColor
            // 
            this.rdoRgbColor.AutoSize = true;
            this.rdoRgbColor.Location = new System.Drawing.Point(22, 87);
            this.rdoRgbColor.Name = "rdoRgbColor";
            this.rdoRgbColor.Size = new System.Drawing.Size(52, 20);
            this.rdoRgbColor.TabIndex = 3;
            this.rdoRgbColor.Text = "RGB:";
            this.rdoRgbColor.UseVisualStyleBackColor = true;
            // 
            // lblG
            // 
            this.lblG.AutoSize = true;
            this.lblG.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblG.Location = new System.Drawing.Point(111, 117);
            this.lblG.Name = "lblG";
            this.lblG.Size = new System.Drawing.Size(19, 16);
            this.lblG.TabIndex = 52;
            this.lblG.Text = "G:";
            this.lblG.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblB
            // 
            this.lblB.AutoSize = true;
            this.lblB.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblB.Location = new System.Drawing.Point(201, 117);
            this.lblB.Name = "lblB";
            this.lblB.Size = new System.Drawing.Size(17, 16);
            this.lblB.TabIndex = 53;
            this.lblB.Text = "B:";
            this.lblB.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // nudR
            // 
            this.nudR.Location = new System.Drawing.Point(42, 115);
            this.nudR.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudR.Name = "nudR";
            this.nudR.Size = new System.Drawing.Size(48, 23);
            this.nudR.TabIndex = 5;
            this.nudR.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudR.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudR.ValueChanged += new System.EventHandler(this.nudColor_ValueChanged);
            this.nudR.Enter += new System.EventHandler(this.nudColor_Enter);
            this.nudR.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nudColor_KeyUp);
            this.nudR.Leave += new System.EventHandler(this.nudColor_Leave);
            this.nudR.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudColor_MouseClick);
            // 
            // nudG
            // 
            this.nudG.Location = new System.Drawing.Point(132, 115);
            this.nudG.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudG.Name = "nudG";
            this.nudG.Size = new System.Drawing.Size(48, 23);
            this.nudG.TabIndex = 6;
            this.nudG.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudG.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudG.ValueChanged += new System.EventHandler(this.nudColor_ValueChanged);
            this.nudG.Enter += new System.EventHandler(this.nudColor_Enter);
            this.nudG.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nudColor_KeyUp);
            this.nudG.Leave += new System.EventHandler(this.nudColor_Leave);
            this.nudG.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudColor_MouseClick);
            // 
            // nudB
            // 
            this.nudB.Location = new System.Drawing.Point(222, 115);
            this.nudB.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudB.Name = "nudB";
            this.nudB.Size = new System.Drawing.Size(48, 23);
            this.nudB.TabIndex = 7;
            this.nudB.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.nudB.Value = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.nudB.ValueChanged += new System.EventHandler(this.nudColor_ValueChanged);
            this.nudB.Enter += new System.EventHandler(this.nudColor_Enter);
            this.nudB.KeyUp += new System.Windows.Forms.KeyEventHandler(this.nudColor_KeyUp);
            this.nudB.Leave += new System.EventHandler(this.nudColor_Leave);
            this.nudB.MouseClick += new System.Windows.Forms.MouseEventHandler(this.nudColor_MouseClick);
            // 
            // txtHtmlColor
            // 
            this.txtHtmlColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtHtmlColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtHtmlColor.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            this.txtHtmlColor.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtHtmlColor.Location = new System.Drawing.Point(92, 52);
            this.txtHtmlColor.MaxLength = 6;
            this.txtHtmlColor.Name = "txtHtmlColor";
            this.txtHtmlColor.Size = new System.Drawing.Size(58, 21);
            this.txtHtmlColor.TabIndex = 2;
            this.txtHtmlColor.Tag = null;
            this.txtHtmlColor.Text = "FFFFFF";
            this.txtHtmlColor.TextDetached = true;
            this.txtHtmlColor.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.txtHtmlColor.MouseClick += new System.Windows.Forms.MouseEventHandler(this.txtHtmlColor_MouseClick);
            this.txtHtmlColor.Enter += new System.EventHandler(this.txtHtmlColor_Enter);
            this.txtHtmlColor.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtHtmlColor_KeyPress);
            this.txtHtmlColor.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtHtmlColor_KeyUp);
            this.txtHtmlColor.Leave += new System.EventHandler(this.txtHtmlColor_Leave);
            // 
            // txtRgbColor
            // 
            this.txtRgbColor.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtRgbColor.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtRgbColor.ImeMode = System.Windows.Forms.ImeMode.Disable;
            this.txtRgbColor.Location = new System.Drawing.Point(74, 87);
            this.txtRgbColor.Name = "txtRgbColor";
            this.txtRgbColor.ReadOnly = true;
            this.txtRgbColor.Size = new System.Drawing.Size(82, 21);
            this.txtRgbColor.TabIndex = 4;
            this.txtRgbColor.Tag = null;
            this.txtRgbColor.Text = "255,255,255";
            this.txtRgbColor.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.txtRgbColor.TextDetached = true;
            this.txtRgbColor.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnOK.Location = new System.Drawing.Point(311, 109);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(66, 29);
            this.btnOK.TabIndex = 8;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnCopy_HTML
            // 
            this.btnCopy_HTML.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCopy_HTML.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy_HTML.Image")));
            this.btnCopy_HTML.Location = new System.Drawing.Point(154, 52);
            this.btnCopy_HTML.Name = "btnCopy_HTML";
            this.btnCopy_HTML.Size = new System.Drawing.Size(22, 21);
            this.btnCopy_HTML.TabIndex = 88;
            this.btnCopy_HTML.Tag = "1";
            this.btnCopy_HTML.UseVisualStyleBackColor = true;
            this.btnCopy_HTML.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCopy_HTML.Click += new System.EventHandler(this.btnCopy_HTML_Click);
            // 
            // btnCopy_RGB
            // 
            this.btnCopy_RGB.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.btnCopy_RGB.Image = ((System.Drawing.Image)(resources.GetObject("btnCopy_RGB.Image")));
            this.btnCopy_RGB.Location = new System.Drawing.Point(160, 87);
            this.btnCopy_RGB.Name = "btnCopy_RGB";
            this.btnCopy_RGB.Size = new System.Drawing.Size(22, 21);
            this.btnCopy_RGB.TabIndex = 89;
            this.btnCopy_RGB.Tag = "1";
            this.btnCopy_RGB.UseVisualStyleBackColor = true;
            this.btnCopy_RGB.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCopy_RGB.Click += new System.EventHandler(this.btnCopy_RGB_Click);
            // 
            // lblInvalidColorCode
            // 
            this.lblInvalidColorCode.AutoSize = true;
            this.lblInvalidColorCode.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.lblInvalidColorCode.Location = new System.Drawing.Point(186, 55);
            this.lblInvalidColorCode.Name = "lblInvalidColorCode";
            this.lblInvalidColorCode.Size = new System.Drawing.Size(112, 16);
            this.lblInvalidColorCode.TabIndex = 90;
            this.lblInvalidColorCode.Text = "Invalid color code!";
            this.lblInvalidColorCode.Visible = false;
            // 
            // ColorConverterDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(398, 159);
            this.Controls.Add(this.pnlColor);
            this.Controls.Add(this.lblInvalidColorCode);
            this.Controls.Add(this.btnCopy_RGB);
            this.Controls.Add(this.btnCopy_HTML);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.txtRgbColor);
            this.Controls.Add(this.txtHtmlColor);
            this.Controls.Add(this.nudB);
            this.Controls.Add(this.nudG);
            this.Controls.Add(this.nudR);
            this.Controls.Add(this.lblB);
            this.Controls.Add(this.lblG);
            this.Controls.Add(this.rdoRgbColor);
            this.Controls.Add(this.rdoHtmlColor);
            this.Controls.Add(this.rdoPickColor);
            this.Controls.Add(this.lblR);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColorConverterDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Color Converter";
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.nudR)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudG)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudB)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtHtmlColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtRgbColor)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy_HTML)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCopy_RGB)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel pnlColor;
        private System.Windows.Forms.Label lblR;
        private System.Windows.Forms.RadioButton rdoPickColor;
        private System.Windows.Forms.RadioButton rdoHtmlColor;
        private System.Windows.Forms.RadioButton rdoRgbColor;
        private System.Windows.Forms.Label lblG;
        private System.Windows.Forms.Label lblB;
        private System.Windows.Forms.NumericUpDown nudR;
        private System.Windows.Forms.NumericUpDown nudG;
        private System.Windows.Forms.NumericUpDown nudB;
        private C1.Win.C1Input.C1TextBox txtHtmlColor;
        private C1.Win.C1Input.C1TextBox txtRgbColor;
        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnCopy_HTML;
        private C1.Win.C1Input.C1Button btnCopy_RGB;
        private System.Windows.Forms.Label lblInvalidColorCode;
    }
}