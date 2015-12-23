using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalRTest
{
    public class MineHub : Hub
    {
        public void SendToServer(string s_json)
        {
            System.Diagnostics.Debug.WriteLine("call SendToServer");
            Clients.All.sendToClient(s_json);
        }

        override public Task OnConnected()
        { return null; }
        override public Task OnDisconnected(bool stopCalled)
        { return null; }
        override public Task OnReconnected()
        { return null; }
    }
}