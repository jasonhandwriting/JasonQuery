namespace JasonQuery.UI.Forms
{
    partial class AsciiTableForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AsciiTableForm));
            this.c1Grid = new C1.Win.C1TrueDBGrid.C1TrueDBGrid();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            this.SuspendLayout();
            // 
            // c1Grid
            // 
            this.c1Grid.AllowSort = false;
            this.c1Grid.AllowUpdate = false;
            this.c1Grid.AllowUpdateOnBlur = false;
            this.c1Grid.AlternatingRows = true;
            this.c1Grid.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(240)))), ((int)(((byte)(240)))));
            this.c1Grid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.c1Grid.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.c1Grid.ForeColor = System.Drawing.SystemColors.ControlText;
            this.c1Grid.GroupByCaption = "將欄位標題拖拽到這裡以便按照該欄位進行分組";
            this.c1Grid.Images.Add(((System.Drawing.Image)(resources.GetObject("c1Grid.Images"))));
            this.c1Grid.Location = new System.Drawing.Point(0, 0);
            this.c1Grid.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
            this.c1Grid.Name = "c1Grid";
            this.c1Grid.PreviewInfo.Caption = "PrintPreview窗口";
            this.c1Grid.PreviewInfo.Location = new System.Drawing.Point(0, 0);
            this.c1Grid.PreviewInfo.Size = new System.Drawing.Size(0, 0);
            this.c1Grid.PreviewInfo.ZoomFactor = 75D;
            this.c1Grid.PrintInfo.MeasurementDevice = C1.Win.C1TrueDBGrid.PrintInfo.MeasurementDeviceEnum.Screen;
            this.c1Grid.PrintInfo.MeasurementPrinterName = null;
            this.c1Grid.RowHeight = 19;
            this.c1Grid.Size = new System.Drawing.Size(646, 887);
            this.c1Grid.TabIndex = 67;
            this.c1Grid.UseCompatibleTextRendering = false;
            this.c1Grid.MouseDown += new System.Windows.Forms.MouseEventHandler(this.c1Grid_MouseDown);
            this.c1Grid.PropBag = resources.GetString("c1Grid.PropBag");
            // 
            // AsciiTableForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(646, 887);
            this.Controls.Add(this.c1Grid);
            this.Font = new System.Drawing.Font("Microsoft JhengHei", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AsciiTableForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Ascii Table";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.c1Grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private C1.Win.C1TrueDBGrid.C1TrueDBGrid c1Grid;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
    }
}