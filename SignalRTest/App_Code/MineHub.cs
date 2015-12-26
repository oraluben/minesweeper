using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;

namespace SignalRTest.App_Code
{
    /// <summary>
    /// Receive messages from single client and send these messages to ActiveMQ
    /// 
    /// </summary>
    public class MineHub : Hub
    {
        /// <summary>
        /// use singleton pattern to make sure there is only one consumer for ActiveMQ
        /// </summary>
        private static MineQueueObserver m = new MineQueueObserver();

        /// <summary>
        /// each Hub has one producer for ActiveMQ
        /// </summary>
        private Producer p = Utils.getProducer();
        private String ConnectionID;

        public void SendToServer(string s_json)
        {
            Debug.WriteLine("Hub receive message: {0}", s_json);
            // todo: send info and clientID to AMQ
            // could be: init / click
        }

        public override Task OnConnected()
        {
            ConnectionID = Context.ConnectionId;
            return base.OnConnected();
        }
    }
}