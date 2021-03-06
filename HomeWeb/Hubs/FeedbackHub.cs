﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using HomeWeb.Services;

namespace HomeWeb.Hubs {
    public class FeedbackHub : Hub {

        private readonly ActionHandlerService actionHandlerService;
        public FeedbackHub(ActionHandlerService actionHandlerService) {
            this.actionHandlerService = actionHandlerService;
        }
        public async Task SendMessage(string user, string message) {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
        public override Task OnConnectedAsync() {
            return base.OnConnectedAsync();
        }
        public override Task OnDisconnectedAsync(Exception exception) {
            return base.OnDisconnectedAsync(exception);
        }
    }
}
