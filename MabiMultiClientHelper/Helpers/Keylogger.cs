using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MabiMultiClientHelper.Helpers
{
    public delegate void KeyloggerKeyDownHandler(int keycode, bool controlKeyDown, bool altKeyDown, bool shiftDown, bool winDown);

    public static class Keylogger
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        private static bool CONTROL_DOWN = false;
        private static bool SHIFT_DOWN = false;
        private static bool ALT_DOWN = false;
        private static bool WIN_DOWN = false;

        private const int ALT_KEY = 164;
        private const int WIN_KEY = 91;
        private const int LCTRL_KEY = 162;
        private const int RCTRL_KEY = 25;
        private const int LSHIFT_KEY = 160;
        private const int RSHIFT_KEY = 161;

        public static event KeyloggerKeyDownHandler KeyPressed;

        public static void InstallHook()
        {
            _hookID = SetHook(_proc);
        }
        public static void UninstallHook()
        {
            UnhookWindowsHookEx(_hookID);
        }
        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //Debug.WriteLine($"code: {nCode}, wParam: {wParam}");

            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                string theKey = ((Keys)vkCode).ToString();

                if (vkCode == LCTRL_KEY || vkCode == RCTRL_KEY)
                {
                    CONTROL_DOWN = true;
                }
                else if (vkCode == LSHIFT_KEY || vkCode == RSHIFT_KEY)
                {
                    SHIFT_DOWN = true;
                }
                else if (vkCode == WIN_KEY)
                {
                    WIN_DOWN = true;
                }
                else
                {
                    KeyPressed(vkCode, CONTROL_DOWN, ALT_DOWN, SHIFT_DOWN, WIN_DOWN);
                }
            }
            else if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == LCTRL_KEY || vkCode == RCTRL_KEY)
                {
                    CONTROL_DOWN = false;
                }
                else if (vkCode == LSHIFT_KEY || vkCode == RSHIFT_KEY)
                {
                    SHIFT_DOWN = false;
                }
                else if (vkCode == ALT_KEY)
                {
                    ALT_DOWN = false;
                }
                else if (vkCode == WIN_KEY)
                {
                    WIN_DOWN = false;
                }
            }
            else if (wParam == (IntPtr)WM_SYSKEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == ALT_KEY)
                {
                    ALT_DOWN = true;
                }
            }
            else if (wParam == (IntPtr)WM_SYSKEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);

                KeyPressed(vkCode, CONTROL_DOWN, ALT_DOWN, SHIFT_DOWN, WIN_DOWN);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}
