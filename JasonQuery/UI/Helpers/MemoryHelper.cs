using C1.Win.C1TrueDBGrid;
using JasonQuery.UI.Forms;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace JasonQuery.UI.Helpers
{
    public static class MemoryHelper
    {
        private const long BytesPerMegabyte = 1024L * 1024L;
        private const long PrivateMemoryThresholdBytes = 400L * BytesPerMegabyte;

        //超過此筆數代表目前仍有大型結果集存活；此時不主動壓縮 Working Set，
        //避免將仍會使用的資料頁移出實體記憶體後，隨即又產生大量 page fault。
        private const long LargeLiveResultRowThreshold = 100000L;

        //Application.Idle 可能非常頻繁，因此只允許每 30 秒進行一次輕量檢查。
        private static readonly TimeSpan MemoryCheckInterval = TimeSpan.FromSeconds(30);

        //Full GC 會暫停 UI；即使一直超過門檻，也至少間隔 2 分鐘。
        private static readonly TimeSpan MinimumFullGcInterval = TimeSpan.FromMinutes(2);

        private static readonly IntPtr FlushWorkingSetValue = new IntPtr(-1);

        private static DateTime _lastMemoryCheckTimeUtc = DateTime.MinValue;
        private static DateTime _lastFullGcTimeUtc = DateTime.MinValue;
        private static bool _isClearingMemory;

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetProcessWorkingSetSize(IntPtr processHandle, IntPtr minimumWorkingSetSize, IntPtr maximumWorkingSetSize);

        /// <summary>
        /// 最近一次記憶體檢查時，所有 QueryForm 查詢結果 Grid 的總資料筆數。
        /// </summary>
        public static long LastObservedLiveRowCount { get; private set; }

        /// <summary>
        /// 供 Application.Idle 或查詢結果清理完成後呼叫。
        /// 方法內部已限制檢查及 Full GC 頻率。
        /// </summary>
        public static void ClearMemory()
        {
            if (_isClearingMemory)
            {
                return;
            }

            DateTime nowUtc = DateTime.UtcNow;

            if (nowUtc - _lastMemoryCheckTimeUtc < MemoryCheckInterval)
            {
                return;
            }

            _lastMemoryCheckTimeUtc = nowUtc;

            using (Process process = Process.GetCurrentProcess())
            {
                process.Refresh();

                if (process.PrivateMemorySize64 <= PrivateMemoryThresholdBytes)
                {
                    return;
                }

                if (nowUtc - _lastFullGcTimeUtc < MinimumFullGcInterval)
                {
                    return;
                }

                _isClearingMemory = true;

                try
                {
                    LastObservedLiveRowCount = GetTotalLiveRowCount();

                    //第一輪回收無法到達的物件，接著等待具有 Finalizer 的物件完成清理。
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);
                    GC.WaitForPendingFinalizers();

                    //第二輪回收因 Finalizer 執行完成後才變成不可到達的物件。
                    GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, false);

                    _lastFullGcTimeUtc = DateTime.UtcNow;

                    process.Refresh();

                    //大型結果集仍綁定時，Working Set 中多數頁面仍屬有效資料。
                    //此時強制壓縮只會讓後續操作重新產生 page fault，因此跳過。
                    if (LastObservedLiveRowCount < LargeLiveResultRowThreshold
                        && process.WorkingSet64 > PrivateMemoryThresholdBytes)
                    {
                        TrimWorkingSet(process.Handle);
                    }
                }
                finally
                {
                    _isClearingMemory = false;
                }
            }
        }

        /// <summary>
        /// 統計所有已開啟 QueryForm 中，c1DockingTab1 之下所有 C1TrueDBGrid 目前實際綁定的資料筆數。
        /// </summary>
        public static long GetTotalLiveRowCount()
        {
            long totalRows = 0;

            foreach (Form form in Application.OpenForms)
            {
                QueryForm queryForm = form as QueryForm;

                if (queryForm == null || queryForm.IsDisposed || queryForm.Disposing)
                {
                    continue;
                }

                Control dockingTab = FindControlByName(queryForm, "c1DockingTab1");

                if (dockingTab == null)
                {
                    continue;
                }

                totalRows += GetGridRowCountFromControlTree(dockingTab);
            }

            return totalRows;
        }

        private static long GetGridRowCountFromControlTree(Control parent)
        {
            long totalRows = 0;

            foreach (Control control in parent.Controls)
            {
                C1TrueDBGrid grid = control as C1TrueDBGrid;

                if (grid != null && !grid.IsDisposed && !grid.Disposing)
                {
                    totalRows += GetBoundRowCount(grid);
                }

                if (control.HasChildren)
                {
                    totalRows += GetGridRowCountFromControlTree(control);
                }
            }

            return totalRows;
        }

        private static int GetBoundRowCount(C1TrueDBGrid grid)
        {
            if (grid.DataSource == null)
            {
                return 0;
            }

            try
            {
                CurrencyManager currencyManager = grid.BindingContext[grid.DataSource, grid.DataMember ?? string.Empty] as CurrencyManager;

                if (currencyManager != null)
                {
                    return Math.Max(0, currencyManager.Count);
                }
            }
            catch (ArgumentException)
            {
                //DataMember 與目前 DataSource 暫時不同步時，改用 Grid.RowCount。
            }
            catch (InvalidOperationException)
            {
                //Grid 正在解除或重新建立繫結時，改用 Grid.RowCount。
            }

            try
            {
                return Math.Max(0, grid.RowCount);
            }
            catch (InvalidOperationException)
            {
                return 0;
            }
        }

        private static Control FindControlByName(Control parent, string controlName)
        {
            if (string.Equals(parent.Name, controlName, StringComparison.Ordinal))
            {
                return parent;
            }

            foreach (Control control in parent.Controls)
            {
                Control result = FindControlByName(control, controlName);

                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void TrimWorkingSet(IntPtr processHandle)
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
            {
                return;
            }

            SetProcessWorkingSetSize(processHandle, FlushWorkingSetValue, FlushWorkingSetValue);
        }
    }
}
