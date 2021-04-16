using MabiMultiClientHelper.ViewModels;
using MahApps.Metro.Controls;
using System.ComponentModel;

namespace MabiMultiClientHelper.Views
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();
        }

#warning 코드 정리 필요
        protected override void OnClosing(CancelEventArgs e)
        {
            if (this.DataContext is MainViewModel vm)
            {
                if (vm.Running)
                {
                    vm.StopCommand.Execute(null);
                    e.Cancel = true;
                }
            }
        }
    }
}
