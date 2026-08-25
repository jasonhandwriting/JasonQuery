using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class RenameTabDialog : Form
    {
        public string AccessibleDescriptionString { get; set; }
        public string TabTitle { get; set; }
        public bool IsStar { get; set; }

        public RenameTabDialog()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            txtTabName.Tag = TabTitle;
            txtTabName.Text = TabTitle;

            LocalizationHelper.ApplyLanguageInfo(this, false);
        }

        private void txtTabName_KeyPress(object sender, KeyPressEventArgs e)
        {
            //忽略輸入 "*" 符號
            if (e.KeyChar == '*')
            {
                e.Handled = true;
            }
        }

        private void txtTabName_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = false;

            if (!string.IsNullOrWhiteSpace(txtTabName.Text) && txtTabName.Text.Trim() != TextHelper.GetSafeString(txtTabName.Tag))
            {
                btnOK.Enabled = true;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var sTemp = IsStar ? "*" : string.Empty;

            MyGlobal.GlobalTemp = $"RenameTabMain`{AccessibleDescriptionString}{MyGlobal.Separator}{sTemp}{txtTabName.Text.Trim()}";
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.Escape)
            {
                Close();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
