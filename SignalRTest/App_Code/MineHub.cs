using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SignalRTest.App_Code
{
    public class MineHub : Hub
    {
        private static MineQueueObserver m = new MineQueueObserver();

        public void SendToServer(string s_json)
        {
            System.Diagnostics.Debug.WriteLine("call SendToServer");
            Clients.All.sendToClient(s_json);
        }
    }
}