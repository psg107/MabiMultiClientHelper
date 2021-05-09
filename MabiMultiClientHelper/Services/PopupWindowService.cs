using GalaSoft.MvvmLight;
using MabiMultiClientHelper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MabiMultiClientHelper.Services
{
    public class PopupWindowService
    {
        private readonly ViewModelBase dataContext;

        public PopupWindowService(ViewModelBase dataContext)
        {
            this.dataContext = dataContext;
        }

        public void ShowPopup<T>() where T : Window
        {
            var popupWindowType = typeof(T);

            var win = Activator.CreateInstance(popupWindowType) as Window;
            var owner = WindowHelper.GetWindowFromBindableObject(this.dataContext);

            win.Owner = owner;
            win.ShowDialog();
        }
    }
}
