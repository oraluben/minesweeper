using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using Newtonsoft.Json;

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
        private ActiveMQProducer p = Utils.getProducer();
        private String ConnectionID
        {
            get { return Context.ConnectionId; }
        }

        public void SendToServer(string s_json)
        {
            Debug.Assert(ConnectionID != "", ConnectionID);
            Debug.WriteLine("Hub receive message: {0}", (object)s_json);

            ClientMessage clientMessage = JsonConvert.DeserializeObject<ClientMessage>(s_json);
            clientMessage.connection_id = ConnectionID;

            string body = JsonConvert.SerializeObject(clientMessage);

            Dictionary<string, string> headers = new Dictionary<string, string>
            {
                {"ConnectionID", ConnectionID },
            };

            p.Send(body, headers);
        }
    }
}