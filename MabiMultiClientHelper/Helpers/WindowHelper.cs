using GalaSoft.MvvmLight;
using System.Windows;

namespace MabiMultiClientHelper.Helpers
{
    /// <summary>
    /// 
    /// </summary>
    public static class WindowHelper
    {
        public static Window GetWindowFromBindableObject(ViewModelBase bindableBase)
        {
            Window owner = null;

            if (bindableBase != null)
            {
                foreach (Window win in App.Current.Windows)
                {
                    if (win.DataContext == bindableBase)
                    {
                        owner = win;
                        break;
                    }
                }
            }

            return owner;
        }
    }
}
