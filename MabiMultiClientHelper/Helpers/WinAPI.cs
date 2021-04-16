using System;
using System.Runtime.InteropServices;

namespace MabiMultiClientHelper.Helpers
{
    public static class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowText(IntPtr hWnd, string text);

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);
    }
}
