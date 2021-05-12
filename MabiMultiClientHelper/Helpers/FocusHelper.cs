using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace MabiMultiClientHelper.Helpers
{
    public class FocusHelper
    {
        public static void ActivateMultiClientHelper()
        {
            var mainWindow = Application.Current.MainWindow;
            var handle = new WindowInteropHelper(mainWindow).Handle;
            WinAPI.SetForegroundWindow(handle);
            WinAPI.ForceForegroundWindow(handle);
            mainWindow.Focus();
        }
    }
}
