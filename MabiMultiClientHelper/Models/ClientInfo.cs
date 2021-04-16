using System;
using System.Diagnostics;

namespace MabiMultiClientHelper.Models
{
    public class ClientInfo
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
    }
}
