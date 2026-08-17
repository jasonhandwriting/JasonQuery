using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using JasonQuery.Core.Text;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class AboutForm : Form
    {
        public AboutForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            lblJasonQuery.Text = TextHelper.GetSafeString("lblJasonQuery.Tag");
            lblVersion.Text = AppConfigHelper.LocalVersion;
            lblVersion.Location = new Point(lblJasonQuery.Left + lblJasonQuery.Width, lblVersion.Top);
            txtSupportInfo.Text = AppConfigHelper.SupportInfo;

            LocalizationHelper.ApplyLanguageInfo(this, false);

            txtTemp.Focus();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void lnkIcons8_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkIcons8.LinkVisited = true;
            Process.Start("https://icons8.com/icons");
        }

        private void lnkFreeFont1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkFreeFont1.LinkVisited = true;
            Process.Start("https://www.facebook.com/groups/549661292148791");
        }

        private void lnkFreeFont2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            lnkFreeFont2.LinkVisited = true;
            Process.Start("https://github.com/jasonhandwriting/JasonHandwriting");
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
