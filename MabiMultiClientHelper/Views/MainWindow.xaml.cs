using MabiMultiClientHelper.Helpers;
using MabiMultiClientHelper.ViewModels;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interop;

namespace MabiMultiClientHelper.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// <seealso cref="ViewModels.MainViewModel"/>
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

#warning 코드 정리 필요;; 
            Keylogger.InstallHook();
            Keylogger.KeyPressed += (keycode) =>
            {
                if (keycode == 145)
                {
                    this.Topmost = true;
                    WinAPI.SetForegroundWindow(new WindowInteropHelper(this).Handle);
                    SetBinding(TopmostProperty, new Binding
                    {
                        ElementName = "TopMostCheckBox",
                        Path = new PropertyPath("IsChecked")
                    });
                }
            };
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu.IsOpen = true;
        }
    }
}
