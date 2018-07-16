using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace WebAppRfc.Hubs
{
    public class FeedbackHub:Hub
    {
        public static IHubContext<FeedbackHub> GlobalContext { get; private set; }
        public FeedbackHub(IHubContext<FeedbackHub> context) {
            GlobalContext = context;
        }
        public async Task SendMessage(string user, string message) {
            await Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
