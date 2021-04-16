﻿using MabiMultiClientHelper.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MabiMultiClientHelper
{
    /// <summary>
    /// 
    /// </summary>
    public class ClientManager
    {
        public List<ClientInfo> Scan()
        {
            var processes = Process.GetProcessesByName("Client");

            var clients = processes.Select(x => new ClientInfo
            {
                Process = x,
                PID = x.Id,
                Handle = x.MainWindowHandle,
                Name = $"마비노기{x.Id}"
            });

            return clients.ToList();
        }
    }
}