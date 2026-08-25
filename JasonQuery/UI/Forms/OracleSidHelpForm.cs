using JasonQuery.Core.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public partial class OracleSidHelpForm : Form
    {
        private string _copiedText = string.Empty;

        public OracleSidHelpForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            var rtfOracleSidSql = @"{\rtf1\ansi\deff0{\colortbl ;\red0\green0\blue255;\red255\green0\blue0;}" +
                                  @"\cf1 SELECT\cf0  Instance_Name \cf1 AS\cf0 \cf2  ""SID""\cf0 \line " +
                                  @"  \cf1 FROM\cf0  v$instance;}";

            rtbSql.ReadOnly = true;
            rtbSql.BackColor = BackColor;
            rtbSql.Font = new Font("Consolas", 11);
            rtbSql.Rtf = rtfOracleSidSql;

            LocalizationHelper.ApplyLanguageInfo(this, false);

            _copiedText = lblInfo.Text;
        }

        private void btnCopySql_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(rtbSql.Text);

            lblInfo.Text = _copiedText;
            lblInfo.Visible = true;

            Timer timer = new Timer { Interval = 1500 };

            timer.Tick += (s, args) => {
                lblInfo.Text = string.Empty;
                timer.Stop();
            };

            timer.Start();
        }
    }
}
