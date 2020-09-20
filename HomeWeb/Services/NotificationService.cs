using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Home.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;

namespace Home.Web.Services {
    public enum ActionType
    {
        DeviceUpdate,
        RequestAdd, 
        RequestDelete,
        RequestUpdate,
        DeviceAdd,
        DeviceDelete,
        AutomationUpdate
    };
    public class NotificationService {
        private readonly IHubContext<DeviceHub> _hubContext;
        public NotificationService(IHubContext<DeviceHub> hubContext) {
            _hubContext = hubContext;
        }

        public async Task NotifyAll<T>(ActionType action, T obj)
        {
            await _hubContext.Clients.All.SendAsync(action.ToString(), obj);
        }
        public async Task NotifyAll<T,V>(ActionType action, T obj1, V obj2)
        {
            await _hubContext.Clients.All.SendAsync(action.ToString(), obj1, obj2);
        }
    }
}
