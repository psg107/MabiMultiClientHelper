using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MabiMultiClientHelper
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private RunOneInstance runOneApp = new RunOneInstance();

        protected override void OnStartup(StartupEventArgs e)
        {
            if (runOneApp.CreateOnlyOneMutex(null) == false)
            {
                MessageBox.Show("이미 실행중입니다.", "마비노기 멀티 클라이언트 도우미");
                Environment.Exit(0);
                return;
            }

            base.OnStartup(e);
        }
    }
}
