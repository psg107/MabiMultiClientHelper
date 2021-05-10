using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;

namespace MabiMultiClientHelper
{
    /// <summary>
    /// App.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class App : Application
    {
        private static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        private static string[] EmbeddedLibraries =
            ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();

        public App()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) => {
                // Get assembly name
                var assemblyName = new AssemblyName(args.Name).Name + ".dll";

                // Get resource name
                var resourceName = EmbeddedLibraries.FirstOrDefault(x => x.EndsWith(assemblyName));
                if (resourceName == null)
                {
                    return null;
                }

                // Load assembly from resource
                using (var stream = ExecutingAssembly.GetManifestResourceStream(resourceName))
                {
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return Assembly.Load(bytes);
                }
            };
        }

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
