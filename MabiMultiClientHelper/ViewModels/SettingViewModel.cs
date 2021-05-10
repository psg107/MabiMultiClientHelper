using GalaSoft.MvvmLight;
using MabiMultiClientHelper.Helpers;
using MahApps.Metro.Controls;
using Prism.Commands;

namespace MabiMultiClientHelper.ViewModels
{
    /// <summary>
    /// <seealso cref="Views.SettingWindow"/>
    /// </summary>
    public class SettingViewModel : ViewModelBase
    {
        /// <summary>
        /// 멀티 클라이언트 도우미 활성화
        /// </summary>
        public HotKey ActiveMultiClientHelperHotKey
        {
            get => activeMultiClientHelperHotKey;
            set
            {
                if (IsDuplicateHotKey(value)) 
                {
                    return;
                }
                if (value.ModifierKeys != System.Windows.Input.ModifierKeys.None)
                {
                    return;
                }
                Set(ref activeMultiClientHelperHotKey, value);
            }
        }

        /// <summary>
        /// 다음 메인 클라이언트 활성화
        /// </summary>
        public HotKey ActiveNextMainClientHotKey
        {
            get => activeNextMainClientHotKey;
            set
            {
                if (IsDuplicateHotKey(value))
                {
                    return;
                }
                if (value.ModifierKeys != System.Windows.Input.ModifierKeys.None)
                {
                    return;
                }
                Set(ref activeNextMainClientHotKey, value);
            }
        }

        /// <summary>
        /// 다음 서브 클라이언트 활성화
        /// </summary>
        public HotKey ActiveNextSubClientHotKey
        {
            get => activeNextSubClientHotKey;
            set
            {
                if (IsDuplicateHotKey(value))
                {
                    return;
                }
                if (value.ModifierKeys != System.Windows.Input.ModifierKeys.None)
                {
                    return;
                }
                Set(ref activeNextSubClientHotKey, value);
            }
        }

        private HotKey activeMultiClientHelperHotKey;
        private HotKey activeNextMainClientHotKey;
        private HotKey activeNextSubClientHotKey;

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand LoadCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    ActiveMultiClientHelperHotKey = GlobalHotkey.Instance.ActiveMultiClientHelperHotKey;
                    ActiveNextMainClientHotKey = GlobalHotkey.Instance.ActiveNextMainClientHotKey;
                    ActiveNextSubClientHotKey = GlobalHotkey.Instance.ActiveNextSubClientHotKey;
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand SaveCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    GlobalHotkey.Instance.ActiveMultiClientHelperHotKey = ActiveMultiClientHelperHotKey;
                    GlobalHotkey.Instance.ActiveNextMainClientHotKey = ActiveNextMainClientHotKey;
                    GlobalHotkey.Instance.ActiveNextSubClientHotKey = ActiveNextSubClientHotKey;

                    var setting = SettingManager.Instance.LoadSetting();
                    setting.ActiveMultiClientHelperHotKey = ActiveMultiClientHelperHotKey;
                    setting.ActiveNextMainClientHotKey = ActiveNextMainClientHotKey;
                    setting.ActiveNextSubClientHotKey = ActiveNextSubClientHotKey;
                    SettingManager.Instance.SaveSetting(setting);

                    var win = WindowHelper.GetWindowFromBindableObject(this);
                    win.Close();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public DelegateCommand CancelCommand
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    var win = WindowHelper.GetWindowFromBindableObject(this);
                    win.Close();
                });
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hotkey"></param>
        /// <returns></returns>
        private bool IsDuplicateHotKey(HotKey hotkey)
        {
            if (hotkey == null)
            {
                return false;
            }

            if (ActiveMultiClientHelperHotKey != null &&
                hotkey.Key == ActiveMultiClientHelperHotKey.Key &&
                ActiveMultiClientHelperHotKey.ModifierKeys.HasFlag(hotkey.ModifierKeys))
            {
                return true;
            }
            if (ActiveNextMainClientHotKey != null &&
                hotkey.Key == ActiveNextMainClientHotKey.Key &&
                ActiveNextMainClientHotKey.ModifierKeys.HasFlag(hotkey.ModifierKeys))
            {
                return true;
            }
            if (ActiveNextSubClientHotKey != null &&
                hotkey.Key == ActiveNextSubClientHotKey.Key &&
                ActiveNextSubClientHotKey.ModifierKeys.HasFlag(hotkey.ModifierKeys))
            {
                return true;
            }
            return false;
        }
    }
}
