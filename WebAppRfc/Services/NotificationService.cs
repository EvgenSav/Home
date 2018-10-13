using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using WebAppRfc.Hubs;

namespace WebAppRfc.Services {
    public class NotificationService {
        private readonly IHubContext<FeedbackHub> hubContext;
        private readonly ActionHandlerService actionHandlerService;
        public NotificationService(IHubContext<FeedbackHub> hubContext, ActionHandlerService actionHandlerService) {
            this.hubContext = hubContext;
            this.actionHandlerService = actionHandlerService;
        }
        //public async Task NotifyAll(ActionType actionType) {
        //    await hubContext.Clients.All.SendAsync()
        //}
    }
    enum ActionType {
        DevUpdated = 1,
    };
}
