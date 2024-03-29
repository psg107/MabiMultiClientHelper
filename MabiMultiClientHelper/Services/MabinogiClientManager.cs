﻿using MabiMultiClientHelper.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MabiMultiClientHelper.Services
{
    /// <summary>
    /// 
    /// </summary>
    public class MabinogiClientManager
    {
        public List<ClientInfo> Scan()
        {
            var processes = Process.GetProcessesByName("Client")
                                   .Where(x => x.HandleCount > 100);

            var clients = processes
                .OrderBy(x => x.StartTime)
                .Select(x => new ClientInfo
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
