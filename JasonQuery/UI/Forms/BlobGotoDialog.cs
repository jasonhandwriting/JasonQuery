using System;
using System.Drawing;
using System.Windows.Forms;
using JasonQuery.Core.Localization;

namespace JasonQuery.UI.Forms
{
    public partial class BlobGotoDialog : Form
    {
        public BlobGotoDialog()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LocalizationHelper.ApplyLanguageInfo(this, false, false);
            nudByteNumber.Location = new Point(lblByteNumber.Left + lblByteNumber.Width + 3, nudByteNumber.Top);
        }

        private void Form_Activated(object sender, EventArgs e)
        {
            nudByteNumber.Focus();
            nudByteNumber.Select(0, nudByteNumber.Value.ToString().Length);
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.Cancel)
            {
                btnCancel.PerformClick();
            }
        }

        public void SetDefaultValue(long byteIndex)
        {
            nudByteNumber.Value = byteIndex;
        }

        public void SetMaxByteIndex(long maxByteIndex)
        {
            nudByteNumber.Minimum = 0;
            nudByteNumber.Maximum = maxByteIndex;
        }

        public long GetByteIndex()
        {
            return Convert.ToInt64(nudByteNumber.Value);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        private void nudByteNumber_Enter(object sender, EventArgs e)
        {
            UpDownBase text = nudByteNumber;

            nudByteNumber.Select(0, text.Text.Length);
        }

        private void nudByteNumber_MouseClick(object sender, MouseEventArgs e)
        {
            UpDownBase text = nudByteNumber;

            nudByteNumber.Select(0, text.Text.Length);
        }

        private void nudByteNumber_Leave(object sender, EventArgs e)
        {
            UpDownBase text = nudByteNumber;

            if (string.IsNullOrEmpty(text.Text))
            {
                nudByteNumber.UpButton();
                nudByteNumber.DownButton();
            }
        }

        private void nudByteNumber_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                btnOK.PerformClick();
            }
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
