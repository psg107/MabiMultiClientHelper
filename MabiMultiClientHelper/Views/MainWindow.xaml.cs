using MabiMultiClientHelper.Helpers;
using MabiMultiClientHelper.ViewModels;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
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
            Keylogger.KeyPressed += (keycode, ctrl, alt, shift, win) =>
            {
#warning 현재 modifer 안들어오도록 막아둠..

                var key = KeyInterop.KeyFromVirtualKey(keycode);

                //Debug.WriteLine($"key: {key}, ctrl: {ctrl}, alt: {alt}, shift: {shift}, win: {win}");

                var modifier = new ModifierKeys();
                if (ctrl)
                {
                    modifier = modifier | ModifierKeys.Control;
                }
                if (alt)
                {
                    modifier = modifier | ModifierKeys.Alt;
                }
                if (shift)
                {
                    modifier = modifier | ModifierKeys.Shift;
                }
                if (win)
                {
                    modifier = modifier | ModifierKeys.Windows;
                }

                switch (key)
                {
                    case Key k when GlobalHotkey.Instance.ActiveMultiClientHelperHotKey != null &&
                                    key == GlobalHotkey.Instance.ActiveMultiClientHelperHotKey.Key &&
                                    GlobalHotkey.Instance.ActiveMultiClientHelperHotKey.ModifierKeys.HasFlag(modifier):
                        this.Topmost = true;
                        WinAPI.SetForegroundWindow(new WindowInteropHelper(this).Handle);
                        SetBinding(TopmostProperty, new Binding
                        {
                            ElementName = "TopMostCheckBox",
                            Path = new PropertyPath("IsChecked")
                        });
                        break;

                    case Key k when GlobalHotkey.Instance.ActiveNextMainClientHotKey != null &&
                                    key == GlobalHotkey.Instance.ActiveNextMainClientHotKey.Key &&
                                    GlobalHotkey.Instance.ActiveNextMainClientHotKey.ModifierKeys.HasFlag(modifier):
                        {
                            var vm = this.DataContext as MainViewModel;
                            vm.ActiveNextMainClientCommand.Execute();
                        }
                        break;

                    case Key k when GlobalHotkey.Instance.ActiveNextSubClientHotKey != null &&
                                    key == GlobalHotkey.Instance.ActiveNextSubClientHotKey.Key &&
                                    GlobalHotkey.Instance.ActiveNextSubClientHotKey.ModifierKeys.HasFlag(modifier):
                        {
                            var vm = this.DataContext as MainViewModel;
                            vm.ActiveNextSubClientCommand.Execute();
                        }
                        break;

                    default:
                        return;
                }
            };
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            PopupMenu.IsOpen = true;
        }
    }
}
