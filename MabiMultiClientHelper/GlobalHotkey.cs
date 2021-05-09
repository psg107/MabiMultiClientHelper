using MahApps.Metro.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MabiMultiClientHelper
{
    public class GlobalHotkey
    {
        #region Singleton Access

        private static readonly Lazy<GlobalHotkey> instance
            = new Lazy<GlobalHotkey>(() => new GlobalHotkey());

        public static GlobalHotkey Instance
        {
            get { return instance.Value; }
        }

        private GlobalHotkey()
        {
            var setting = SettingManager.Instance.LoadSetting();
            this.ActiveMultiClientHelperHotKey = setting.ActiveMultiClientHelperHotKey;
            this.ActiveNextMainClientHotKey = setting.ActiveNextMainClientHotKey;
            this.ActiveNextSubClientHotKey = setting.ActiveNextSubClientHotKey;
        }

        #endregion

        public HotKey ActiveMultiClientHelperHotKey { get; set; }

        public HotKey ActiveNextMainClientHotKey { get; set; }

        public HotKey ActiveNextSubClientHotKey { get; set; }
    }
}
