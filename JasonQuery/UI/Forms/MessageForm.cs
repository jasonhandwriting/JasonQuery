using JasonQuery.Core.Config;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace JasonQuery.UI.Forms
{
    public sealed partial class MessageForm : Form
    {
        private string _info = string.Empty;
        private string _caption = string.Empty;
        private bool _isNeedToMovePosition = false;

        public string Info
        {
            set => _info = value;
        }

        public string Caption
        {
            set => _caption = value;
        }

        public bool IsNeedToMovePosition
        {
            set => _isNeedToMovePosition = value;
        }

        public MessageForm()
        {
            InitializeComponent();
        }

        private void Form_Load(object sender, EventArgs e)
        {
            if (FormBorderStyle == FormBorderStyle.None)
            {
                lblInfo.BorderStyle = BorderStyle.FixedSingle;
                lblInfo.ForeColor = Color.Blue;
                lblInfo.Font = new Font("Microsoft JhengHei", 9f, FontStyle.Regular, GraphicsUnit.Point, 136);
            }

            Text = _caption;
            lblInfo.Text = _info;
            UseWaitCursor = true;

            if (_isNeedToMovePosition)
            {
                Location = new Point(Left, Top - 250);
            }
        }

        private void tmrInfo_Tick(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(MyGlobal.SendMessageToInfo))
            {
                return;
            }

            lblInfo.Text = MyGlobal.SendMessageToInfo;
            MyGlobal.SendMessageToInfo = string.Empty;
            lblInfo.Refresh();
        }
    }
}
