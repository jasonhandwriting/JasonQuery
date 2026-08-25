using JasonQuery.Core.Config;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace JasonQuery.UI.Helpers
{
    public static class MessageBoxHelper
    {
        //for MessageBox's Position
        [DllImport("user32.dll")]
        private static extern IntPtr FindWindow(IntPtr classname, string title);

        [DllImport("user32.dll")]
        private static extern void MoveWindow(IntPtr hwnd, int x, int y, int nWidth, int nHeight, bool rePaint);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hwnd, out Rectangle rect);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags flags);

        [Flags]
        private enum SetWindowPosFlags : uint
        {
            SWP_NOSIZE = 0x0001,
            SWP_NOZORDER = 0x0004,
            SWP_NOREDRAW = 0x0008
        }

        public static void ShowNearCursor(string text, string caption, MessageBoxButtons buttons = MessageBoxButtons.OK, MessageBoxIcon icon = MessageBoxIcon.None, bool topMost = true)
        {
            var cursorPos = Cursor.Position;
            var estimatedSize = EstimateMessageBoxSize(text);
            var targetPos = CalculateSafePosition(cursorPos, estimatedSize);

            FindAndMoveMessageBox(targetPos.X, targetPos.Y, topMost, caption);
            MessageBox.Show(text, caption, buttons, icon);
        }

        private static void FindAndMoveMessageBox(int x, int y, bool repaint, string caption)
        {
            ThreadPool.QueueUserWorkItem(_ =>
            {
                const int retryIntervalMs = 20;
                const int maxRetry = 100;

                IntPtr msgBox = IntPtr.Zero;

                for (int i = 0; i < maxRetry; i++)
                {
                    msgBox = FindWindow(IntPtr.Zero, caption);

                    if (msgBox != IntPtr.Zero)
                    {
                        break;
                    }

                    Thread.Sleep(retryIntervalMs);
                }

                if (msgBox == IntPtr.Zero)
                {
                    return;
                }

                SetWindowPos(msgBox, IntPtr.Zero, x, y, 0, 0,
                             SetWindowPosFlags.SWP_NOZORDER |
                             SetWindowPosFlags.SWP_NOSIZE |
                             (repaint ? 0 : SetWindowPosFlags.SWP_NOREDRAW));
            });
        }

        private static Size EstimateMessageBoxSize(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return new Size(300, 150);
            }

            var lineCount = text.Split('\n').Length;
            var maxLineLength = text.Split('\n').Max(l => l.Length);

            //基礎尺寸 (單行)
            int width = 280;
            int height = 140;

            //寬度：依最長行推估
            width += Math.Min(maxLineLength * 7, 500);

            //高度：依行數推估
            height += lineCount * 22;

            //合理上限，避免過度偏移
            width = Math.Min(width, 800);
            height = Math.Min(height, 600);

            return new Size(width, height);
        }

        private static Point CalculateSafePosition(Point anchor, Size estimatedSize, int margin = 20)
        {
            var screen = Screen.FromPoint(anchor).WorkingArea;
            int x = anchor.X + margin;
            int y = anchor.Y + margin;

            //右邊超出
            if (x + estimatedSize.Width > screen.Right)
            {
                x = anchor.X - estimatedSize.Width - margin;
            }

            //下方超出
            if (y + estimatedSize.Height > screen.Bottom)
            {
                y = anchor.Y - estimatedSize.Height - margin;
            }

            //最終保底修正
            x = Math.Max(screen.Left + margin, x);
            y = Math.Max(screen.Top + margin, y);

            return new Point(x, y);
        }
    }

    public static class MessageBoxManager
    {
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        private delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);

        private const int WH_CALLWNDPROCRET = 12;
        private const int WM_DESTROY = 0x0002;
        private const int WM_INITDIALOG = 0x0110;
        private const int WM_TIMER = 0x0113;
        private const int WM_USER = 0x400;
        private const int DM_GETDEFID = WM_USER + 0;

        private const int MBOK = 1;
        private const int MBCancel = 2;
        private const int MBAbort = 3;
        private const int MBRetry = 4;
        private const int MBIgnore = 5;
        private const int MBYes = 6;
        private const int MBNo = 7;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hInstance, int threadId);

        [DllImport("user32.dll")]
        private static extern int UnhookWindowsHookEx(IntPtr idHook);

        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr idHook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextLengthW", CharSet = CharSet.Unicode)]
        private static extern int GetWindowTextLength(IntPtr hWnd);

        [DllImport("user32.dll", EntryPoint = "GetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int maxLength);

        [DllImport("user32.dll")]
        private static extern int EndDialog(IntPtr hDlg, IntPtr nResult);

        [DllImport("user32.dll")]
        private static extern bool EnumChildWindows(IntPtr hWndParent, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", EntryPoint = "GetClassNameW", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll")]
        private static extern int GetDlgCtrlID(IntPtr hwndCtl);

        [DllImport("user32.dll")]
        private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

        [DllImport("user32.dll", EntryPoint = "SetWindowTextW", CharSet = CharSet.Unicode)]
        private static extern bool SetWindowText(IntPtr hWnd, string lpString);

        //20191116 add
        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [StructLayout(LayoutKind.Sequential)]
        private struct CWPRETSTRUCT
        {
            private IntPtr lResult;
            private IntPtr lParam;
            private IntPtr wParam;
            public uint message;
            public IntPtr hwnd;
        };

        private static HookProc hookProc;
        private static EnumChildProc enumProc;
        [ThreadStatic]
        private static IntPtr hHook;
        [ThreadStatic]
        private static int nButton;

        public static string OK = "&OK";
        public static string Cancel = "&Cancel";
        public static string Abort = "&Abort";
        public static string Retry = "&Retry";
        public static string Ignore = "&Ignore";
        public static string Yes = "&Yes";
        public static string No = "&No";
        public static string CreateNewFolder = "&Make New Folder";
        public static string FolderBrowserDialogTitle = "Browse For Folder";

        #region 20240306
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetParent(IntPtr hWnd);

        const int GWL_STYLE = -16;
        const int WS_CAPTION = 0x00C00000;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

        [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
        public static extern IntPtr GetActiveWindow();
        #endregion

        static MessageBoxManager()
        {
            hookProc = MessageBoxHookProc;
            enumProc = MessageBoxEnumProc;
            hHook = IntPtr.Zero;
        }

        public static void Register()
        {
            if (hHook != IntPtr.Zero)
            {
                throw new NotSupportedException("One hook per thread allowed.");
            }

            //20191116 以下改用 [DllImport("kernel32.dll")] GetCurrentThreadId，測試結果OK
            hHook = SetWindowsHookEx(WH_CALLWNDPROCRET, hookProc, IntPtr.Zero, GetCurrentThreadId());
        }

        public static void Unregister()
        {
            if (hHook == IntPtr.Zero)
            {
                return;
            }

            UnhookWindowsHookEx(hHook);
            hHook = IntPtr.Zero;
        }

        private static IntPtr MessageBoxHookProc(int code, IntPtr wParam, IntPtr lParam)
        {
            if (code < 0)
            {
                return CallNextHookEx(hHook, code, wParam, lParam);
            }

            var msg = (CWPRETSTRUCT)Marshal.PtrToStructure(lParam, typeof(CWPRETSTRUCT));
            var hook = hHook;

            if (msg.message != WM_INITDIALOG)
            {
                return CallNextHookEx(hook, code, wParam, lParam);
            }

            //var length = GetWindowTextLength(msg.hwnd);
            var className = new StringBuilder(10);

            GetClassName(msg.hwnd, className, className.Capacity);

            if (className.ToString() != "#32770")
            {
                return CallNextHookEx(hook, code, wParam, lParam);
            }

            nButton = 0;
            EnumChildWindows(msg.hwnd, enumProc, IntPtr.Zero);

            if (nButton != 1)
            {
                return CallNextHookEx(hook, code, wParam, lParam);
            }

            var button = GetDlgItem(msg.hwnd, MBCancel);

            if (button != IntPtr.Zero)
            {
                SetWindowText(button, OK);
            }

            return CallNextHookEx(hook, code, wParam, lParam);
        }

        private static bool MessageBoxEnumProc(IntPtr hWnd, IntPtr lParam)
        {
            var className = new StringBuilder(10);

            GetClassName(hWnd, className, className.Capacity);

            if (className.ToString() != "Button")
            {
                return true;
            }

            var ctlId = GetDlgCtrlID(hWnd);

            switch (ctlId)
            {
                case MBOK:
                    {
                        SetWindowText(hWnd, OK);
                        break;
                    }
                case MBCancel:
                    {
                        SetWindowText(hWnd, Cancel);
                        break;
                    }
                case MBAbort:
                    {
                        SetWindowText(hWnd, Abort);
                        break;
                    }
                case MBRetry:
                    {
                        SetWindowText(hWnd, Retry);
                        break;
                    }
                case MBIgnore:
                    {
                        SetWindowText(hWnd, Ignore);
                        break;
                    }
                case MBYes:
                    {
                        SetWindowText(hWnd, Yes);
                        break;
                    }
                case MBNo:
                    {
                        SetWindowText(hWnd, No);
                        break;
                    }
                default:
                    {
                        if (MyGlobal.GlobalTempDialog == "BrowseForFolder")
                        {
                            MyGlobal.GlobalTempDialog = string.Empty;

                            //20240306 修改按鈕：建立新資料夾的顯示文字
                            SetWindowText(hWnd, CreateNewFolder);

                            //20240306 修改「瀏覽資料夾」標題列的文字
                            IntPtr hwnd = GetActiveWindow();
                            int style = GetWindowLong(GetParent(hwnd), GWL_STYLE);

                            SetWindowLong(GetParent(hwnd), GWL_STYLE, style);
                            SetWindowText(hwnd, FolderBrowserDialogTitle);
                        }

                        break;
                    }
            }

            nButton++;
            return true;
        }
    }
}
