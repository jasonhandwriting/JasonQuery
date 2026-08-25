using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace JasonLibrary.Core.Win32
{
    public static class Win32Api
    {
        [StructLayout(LayoutKind.Sequential)]
        private struct LastInputInfo
        {
            public uint CbSize;
            public uint DwTime;
        }

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetLastInputInfo(ref LastInputInfo lastInputInfo);

        public static uint GetIdleTime()
        {
            var lastInputInfo = CreateLastInputInfo();

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var currentTickCount = unchecked((uint)Environment.TickCount);

            return currentTickCount - lastInputInfo.DwTime;
        }

        public static TimeSpan GetIdleTimeSpan()
        {
            return TimeSpan.FromMilliseconds(GetIdleTime());
        }

        public static long GetTickCount()
        {
            return Environment.TickCount;
        }

        public static long GetLastInputTime()
        {
            return GetLastInputTickCount();
        }

        public static uint GetLastInputTickCount()
        {
            var lastInputInfo = CreateLastInputInfo();

            if (!GetLastInputInfo(ref lastInputInfo))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            return lastInputInfo.DwTime;
        }

        private static LastInputInfo CreateLastInputInfo()
        {
            return new LastInputInfo
            {
                CbSize = (uint)Marshal.SizeOf(typeof(LastInputInfo))
            };
        }
    }
}
