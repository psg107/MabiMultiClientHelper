using Prism.Mvvm;
using System;
using System.Diagnostics;

namespace MabiMultiClientHelper.Models
{
    public class ClientInfo : BindableBase
    {
        /// <summary>
        /// 
        /// </summary>
        public Process Process { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int PID { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public IntPtr Handle { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public bool IsHiddenWindow 
        {
            get => isHiddenWindow;
            set => SetProperty(ref isHiddenWindow, value);
        }

        private bool isHiddenWindow = false;
    }
}
