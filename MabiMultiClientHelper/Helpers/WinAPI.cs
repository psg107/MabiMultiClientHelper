using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace MabiMultiClientHelper.Helpers
{
    public static class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowThreadProcessId(IntPtr hWnd, out uint ProcessId);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public static string GetActiveProcessFileName()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);
            Process p = Process.GetProcessById((int)pid);
            return p.ProcessName;
        }

        public static uint GetActiveProcessId()
        {
            IntPtr hwnd = GetForegroundWindow();
            uint pid;
            GetWindowThreadProcessId(hwnd, out pid);

            return pid;
        }
    }
}
