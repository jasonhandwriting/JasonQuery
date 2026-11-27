using JasonQuery.Core.Localization;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class RenameTabDialog : Form
    {
        public string TabTitle { get; set; }
        internal string RenamedTabTitle { get; private set; }
        internal Func<string, bool> IsNameAllowed { get; set; }

        public RenameTabDialog()
        {
            InitializeComponent();

            btnOK.DialogResult = DialogResult.None;
            AcceptButton = btnOK;
        }

        private void Form_Load(object sender, EventArgs e)
        {
            txtTabName.Text = TabTitle;

            LocalizationHelper.ApplyLanguageInfo(this, false);
            btnOK.Enabled = IsValidProposedName(txtTabName.Text.Trim());
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
            btnOK.Enabled = IsValidProposedName(txtTabName.Text.Trim());
        }

        private bool IsValidProposedName(string proposedName)
        {
            return !string.IsNullOrWhiteSpace(proposedName)
                   && proposedName.IndexOf('*') < 0
                   && !string.Equals(proposedName, TabTitle, StringComparison.Ordinal)
                   && (IsNameAllowed == null || IsNameAllowed(proposedName));
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            var proposedName = txtTabName.Text.Trim();

            if (!btnOK.Enabled || !IsValidProposedName(proposedName))
            {
                return;
            }

            RenamedTabTitle = proposedName;
            DialogResult = DialogResult.OK;
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
