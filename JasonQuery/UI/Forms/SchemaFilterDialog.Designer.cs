namespace JasonQuery.UI.Forms
{
    partial class SchemaFilterDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchemaFilterDialog));
            this.cboColumn = new C1.Win.C1Input.C1ComboBox();
            this.cboOperator = new C1.Win.C1Input.C1ComboBox();
            this.txtCondition = new C1.Win.C1Input.C1TextBox();
            this.btnAnd = new C1.Win.C1Input.C1Button();
            this.btnOr = new C1.Win.C1Input.C1Button();
            this.btnClearFilter = new C1.Win.C1Input.C1Button();
            this.editorSqlCondition = new JasonLibrary.UI.Controls.ScintillaEditor();
            this.btnOK = new C1.Win.C1Input.C1Button();
            this.btnCancel = new C1.Win.C1Input.C1Button();
            this.lblColumnTypeOK = new C1.Win.C1Input.C1Label();
            this.c1ThemeController1 = new C1.Win.C1Themes.C1ThemeController();
            this.lblColumn = new System.Windows.Forms.Label();
            this.lblColumnType = new System.Windows.Forms.Label();
            this.lblColumnTypeValue = new System.Windows.Forms.Label();
            this.lblLimit500 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.cboColumn)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOperator)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCondition)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAnd)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOr)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClearFilter)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblColumnTypeOK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).BeginInit();
            this.SuspendLayout();
            // 
            // cboColumn
            // 
            this.cboColumn.AllowSpinLoop = false;
            this.cboColumn.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboColumn.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboColumn.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboColumn.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboColumn.GapHeight = 0;
            this.cboColumn.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboColumn.ItemsDisplayMember = "";
            this.cboColumn.ItemsValueMember = "";
            this.cboColumn.Location = new System.Drawing.Point(92, 16);
            this.cboColumn.Name = "cboColumn";
            this.cboColumn.Size = new System.Drawing.Size(150, 21);
            this.cboColumn.TabIndex = 4;
            this.cboColumn.Tag = null;
            this.cboColumn.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboColumn, "(default)");
            this.cboColumn.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.cboColumn.Leave += new System.EventHandler(this.cboColumn_Leave);
            // 
            // cboOperator
            // 
            this.cboOperator.AllowSpinLoop = false;
            this.cboOperator.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
            this.cboOperator.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
            this.cboOperator.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.cboOperator.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.cboOperator.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.cboOperator.GapHeight = 0;
            this.cboOperator.ImagePadding = new System.Windows.Forms.Padding(0);
            this.cboOperator.Items.Add("=");
            this.cboOperator.Items.Add("<>");
            this.cboOperator.Items.Add(">");
            this.cboOperator.Items.Add(">=");
            this.cboOperator.Items.Add("<");
            this.cboOperator.Items.Add("<=");
            this.cboOperator.ItemsDisplayMember = "";
            this.cboOperator.ItemsValueMember = "";
            this.cboOperator.Location = new System.Drawing.Point(249, 16);
            this.cboOperator.MaxDropDownItems = 15;
            this.cboOperator.Name = "cboOperator";
            this.cboOperator.Size = new System.Drawing.Size(96, 21);
            this.cboOperator.TabIndex = 6;
            this.cboOperator.Tag = null;
            this.cboOperator.TextDetached = true;
            this.c1ThemeController1.SetTheme(this.cboOperator, "(default)");
            this.cboOperator.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // txtCondition
            // 
            this.txtCondition.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(234)))), ((int)(((byte)(242)))), ((int)(((byte)(255)))));
            this.txtCondition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtCondition.Location = new System.Drawing.Point(352, 16);
            this.txtCondition.Name = "txtCondition";
            this.txtCondition.Size = new System.Drawing.Size(149, 21);
            this.txtCondition.TabIndex = 7;
            this.txtCondition.Tag = null;
            this.c1ThemeController1.SetTheme(this.txtCondition, "(default)");
            this.txtCondition.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            // 
            // btnAnd
            // 
            this.btnAnd.Enabled = false;
            this.btnAnd.Location = new System.Drawing.Point(507, 15);
            this.btnAnd.Name = "btnAnd";
            this.btnAnd.Size = new System.Drawing.Size(50, 24);
            this.btnAnd.TabIndex = 8;
            this.btnAnd.Tag = "AND";
            this.btnAnd.Text = "(AND)";
            this.btnAnd.UseVisualStyleBackColor = true;
            this.btnAnd.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnAnd.Click += new System.EventHandler(this.btnAndOr_Click);
            // 
            // btnOr
            // 
            this.btnOr.Enabled = false;
            this.btnOr.Location = new System.Drawing.Point(563, 15);
            this.btnOr.Name = "btnOr";
            this.btnOr.Size = new System.Drawing.Size(45, 24);
            this.btnOr.TabIndex = 9;
            this.btnOr.Tag = "OR";
            this.btnOr.Text = "(OR)";
            this.btnOr.UseVisualStyleBackColor = true;
            this.btnOr.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnOr.Click += new System.EventHandler(this.btnAndOr_Click);
            // 
            // btnClearFilter
            // 
            this.btnClearFilter.Location = new System.Drawing.Point(291, 46);
            this.btnClearFilter.Name = "btnClearFilter";
            this.btnClearFilter.Size = new System.Drawing.Size(134, 24);
            this.btnClearFilter.TabIndex = 10;
            this.btnClearFilter.Text = "Clear Filter";
            this.btnClearFilter.UseVisualStyleBackColor = true;
            this.btnClearFilter.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnClearFilter.Click += new System.EventHandler(this.btnClearFilter_Click);
            // 
            // editorSqlCondition
            // 
            this.editorSqlCondition.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.editorSqlCondition.CaretLineVisible = true;
            this.editorSqlCondition.IndentationGuides = ScintillaNET.IndentView.LookBoth;
            this.editorSqlCondition.Location = new System.Drawing.Point(11, 77);
            this.editorSqlCondition.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.editorSqlCondition.Name = "editorSqlCondition";
            this.editorSqlCondition.ScrollWidth = 3618;
            this.editorSqlCondition.Size = new System.Drawing.Size(597, 227);
            this.editorSqlCondition.Styler = null;
            this.editorSqlCondition.TabIndex = 41;
            this.editorSqlCondition.WhitespaceSize = 3;
            this.editorSqlCondition.WrapIndentMode = ScintillaNET.WrapIndentMode.Same;
            this.editorSqlCondition.WrapMode = ScintillaNET.WrapMode.Word;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(461, 46);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(67, 24);
            this.btnOK.TabIndex = 42;
            this.btnOK.Text = "OK";
            this.btnOK.UseVisualStyleBackColor = true;
            this.btnOK.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(541, 46);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(67, 24);
            this.btnCancel.TabIndex = 43;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.VisualStyleBaseStyle = C1.Win.C1Input.VisualStyle.Office2010Blue;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // lblColumnTypeOK
            // 
            this.lblColumnTypeOK.AutoSize = true;
            this.lblColumnTypeOK.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lblColumnTypeOK.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.lblColumnTypeOK.Location = new System.Drawing.Point(224, 49);
            this.lblColumnTypeOK.Name = "lblColumnTypeOK";
            this.lblColumnTypeOK.Size = new System.Drawing.Size(0, 16);
            this.lblColumnTypeOK.TabIndex = 48;
            this.lblColumnTypeOK.Tag = null;
            this.lblColumnTypeOK.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lblColumnTypeOK.TextDetached = true;
            this.lblColumnTypeOK.Visible = false;
            // 
            // lblColumn
            // 
            this.lblColumn.Location = new System.Drawing.Point(11, 18);
            this.lblColumn.Name = "lblColumn";
            this.lblColumn.Size = new System.Drawing.Size(80, 17);
            this.lblColumn.TabIndex = 49;
            this.lblColumn.Text = "Column:";
            this.lblColumn.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblColumn, "(default)");
            // 
            // lblColumnType
            // 
            this.lblColumnType.Location = new System.Drawing.Point(11, 49);
            this.lblColumnType.Name = "lblColumnType";
            this.lblColumnType.Size = new System.Drawing.Size(80, 17);
            this.lblColumnType.TabIndex = 50;
            this.lblColumnType.Text = "Type:";
            this.lblColumnType.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.c1ThemeController1.SetTheme(this.lblColumnType, "(default)");
            this.lblColumnType.Visible = false;
            // 
            // lblColumnTypeValue
            // 
            this.lblColumnTypeValue.Location = new System.Drawing.Point(92, 49);
            this.lblColumnTypeValue.Name = "lblColumnTypeValue";
            this.lblColumnTypeValue.Size = new System.Drawing.Size(190, 17);
            this.lblColumnTypeValue.TabIndex = 51;
            this.lblColumnTypeValue.Text = "Type:";
            this.lblColumnTypeValue.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblColumnTypeValue, "(default)");
            this.lblColumnTypeValue.Visible = false;
            // 
            // lblLimit500
            // 
            this.lblLimit500.AutoSize = true;
            this.lblLimit500.Location = new System.Drawing.Point(11, 310);
            this.lblLimit500.Name = "lblLimit500";
            this.lblLimit500.Size = new System.Drawing.Size(262, 16);
            this.lblLimit500.TabIndex = 52;
            this.lblLimit500.Text = "Please note: Each query is limited to 500 rows";
            this.lblLimit500.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.c1ThemeController1.SetTheme(this.lblLimit500, "(default)");
            // 
            // SchemaFilterDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(618, 336);
            this.Controls.Add(this.lblLimit500);
            this.Controls.Add(this.lblColumnTypeValue);
            this.Controls.Add(this.lblColumnType);
            this.Controls.Add(this.lblColumn);
            this.Controls.Add(this.lblColumnTypeOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.editorSqlCondition);
            this.Controls.Add(this.btnClearFilter);
            this.Controls.Add(this.btnOr);
            this.Controls.Add(this.btnAnd);
            this.Controls.Add(this.txtCondition);
            this.Controls.Add(this.cboOperator);
            this.Controls.Add(this.cboColumn);
            this.Font = new System.Drawing.Font("微軟正黑體", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(136)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SchemaFilterDialog";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Table Filter";
            this.c1ThemeController1.SetTheme(this, "(default)");
            this.Load += new System.EventHandler(this.Form_Load);
            ((System.ComponentModel.ISupportInitialize)(this.cboColumn)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.cboOperator)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.txtCondition)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnAnd)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOr)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnClearFilter)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.btnCancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.lblColumnTypeOK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.c1ThemeController1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        internal C1.Win.C1Input.C1ComboBox cboColumn;
        internal C1.Win.C1Input.C1ComboBox cboOperator;
        private C1.Win.C1Input.C1TextBox txtCondition;
        private C1.Win.C1Input.C1Button btnAnd;
        private C1.Win.C1Input.C1Button btnOr;
        private C1.Win.C1Input.C1Button btnClearFilter;
        private JasonLibrary.UI.Controls.ScintillaEditor editorSqlCondition;
        private C1.Win.C1Input.C1Button btnOK;
        private C1.Win.C1Input.C1Button btnCancel;
        private C1.Win.C1Input.C1Label lblColumnTypeOK;
        private C1.Win.C1Themes.C1ThemeController c1ThemeController1;
        private System.Windows.Forms.Label lblColumn;
        private System.Windows.Forms.Label lblColumnType;
        private System.Windows.Forms.Label lblColumnTypeValue;
        private System.Windows.Forms.Label lblLimit500;
    }
}