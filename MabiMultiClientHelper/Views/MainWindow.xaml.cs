using MabiMultiClientHelper.Helpers;
using MabiMultiClientHelper.ViewModels;
using MahApps.Metro.Controls;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
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

#warning 코드 정리 필요
        protected override void OnClosing(CancelEventArgs e)
        {
            Keylogger.UninstallHook();

            if (this.DataContext is MainViewModel vm)
            {
                if (vm.Running)
                {
                    vm.StopCommand.Execute(null);
                    e.Cancel = true;
                }
                else
                {
                    var clients = vm.MainClients.Concat(vm.SubClients);
                    foreach (var client in clients)
                    {
                        WinAPI.SetWindowText(client.Handle, "마비노기");
                    }
                }
            }
        }
    }
}
