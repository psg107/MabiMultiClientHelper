using Newtonsoft.Json;
using System;
using System.IO;

namespace MabiMultiClientHelper
{
    public class SettingManager
    {
        private const string SETTING_FILE_NAME = "Setting.json";

        #region Singleton Access

        private static readonly Lazy<SettingManager> instance
            = new Lazy<SettingManager>(() => new SettingManager());

        public static SettingManager Instance
        {
            get { return instance.Value; }
        }

        private SettingManager()
        {
            //기본값
            bool skipWhenSubClientActivated = true;
            bool changeClientAffinity = true;
            int suspendInterval = 100;

            if (File.Exists(SETTING_FILE_NAME))
            {
                var serializedSettingFile = File.ReadAllText(SETTING_FILE_NAME);
                try
                {
                    var setting = JsonConvert.DeserializeObject<Setting>(serializedSettingFile);
                    if (setting != null)
                    {
                        if (setting.SuspendInterval < 10 || setting.SuspendInterval > 200)
                        {
                            setting.SuspendInterval = 200;
                        }
                    }

                    this.setting = setting;
                }
                catch (System.Exception)
                {
                    var setting = new Setting
                    {
                        ChangeClientAffinity = skipWhenSubClientActivated,
                        SkipWhenSubClientActivated = changeClientAffinity,
                        SuspendInterval = suspendInterval
                    };
                    var serializedSetting = JsonConvert.SerializeObject(setting);
                    File.WriteAllText(SETTING_FILE_NAME, serializedSetting);

                    this.setting = setting;
                }
            }
        }

        #endregion

        private Setting setting;

        public Setting LoadSetting()
        {
            return this.setting;
        }

        public void SaveSetting(Setting setting)
        {
            var serializedSetting = JsonConvert.SerializeObject(setting);
            var settingFileName = "Setting.json";
            File.WriteAllText(settingFileName, serializedSetting);
        }
    }
}
