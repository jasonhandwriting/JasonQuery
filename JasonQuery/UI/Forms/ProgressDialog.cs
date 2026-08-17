using JasonQuery.Core.Config;
using JasonQuery.Core.Localization;
using System;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class ProgressDialog : Form
    {
        public string TitleName { get; set; }
        public int TotalQty { get; set; }
        public int Interval { get; set; }

        public ProgressDialog()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            LocalizationHelper.ApplyLanguageInfo(this, false);

            if (Interval == 0)
            {
                Interval = 200;
            }

            tmrProgress.Interval = Interval;
            tmrProgress.Enabled = true;

            Application.UseWaitCursor = true;

            Text = TitleName;
            progressBar1.Value = 0;
            progressBar1.Minimum = 0;
            progressBar1.Maximum = TotalQty;

            Refresh();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            MyGlobal.IsProgressCancel = true;
            Close();
        }

        private void tmrSaveAsInsertInto_Tick(object sender, EventArgs e)
        {
            lblProgress.Text = $"{MyGlobal.ProgressInsertInto} / {TotalQty}";
            progressBar1.Value = MyGlobal.ProgressInsertInto;
            Refresh();

            if (MyGlobal.ProgressInsertInto >= TotalQty)
            {
                Close();
            }
        }

        private void Form_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.UseWaitCursor = false;
        }
    }
}