using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
using WebAppRfc.Hubs;

namespace WebAppRfc.Services {
    public enum ActionType
    {
        UpdateDevice = 1,
        BindReceived = 2,

    };
    public class NotificationService {
        private readonly IHubContext<FeedbackHub> _hubContext;
        public NotificationService(IHubContext<FeedbackHub> hubContext) {
            _hubContext = hubContext;
        }

        public async Task NotifyAll<T>(ActionType action, T obj)
        {
            await _hubContext.Clients.All.SendAsync(action.ToString(), obj);
        }
        public async Task NotifyAll<T,TV>(ActionType action, T obj1, TV obj2)
        {
            await _hubContext.Clients.All.SendAsync(action.ToString(), obj1, obj2);
        }
    }
    
}
