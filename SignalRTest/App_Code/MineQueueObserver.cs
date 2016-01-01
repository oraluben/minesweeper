using Apache.NMS;
using Microsoft.AspNet.SignalR;
using MineGroup;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalRTest.App_Code
{
    public class MineQueueObserver
    {
        private readonly static IHubContext _hub_context = GlobalHost.ConnectionManager.GetHubContext<MineHub>();
        private readonly ActiveMQConsumer c = Utils.getConsumer();

        public MineQueueObserver()
        {
            Debug.WriteLine("MineQueueObserver instance started");
            Task.Run(() => { Run(); });
        }

        public Task Run()
        {
            while (true)
            {
                try
                {
                    IMessage m = c.getMessage();

                    Debug.Assert(m is ITextMessage, "unknown message type");
                    Debug.Assert(m.Properties["ConnectionID"].ToString() != "", "ConnectionID is blank");

                    string ConnectionID = m.Properties["ConnectionID"].ToString();

                    ITextMessage textM = m as ITextMessage;
                    Debug.WriteLine("MineQueueObserver get a message, body: {0}", (object)textM.Text);
                    HandleOne(textM);
                }
                catch (Exception e) { Debug.WriteLine("meet an unhandled exception: {0}", e); }
            }
        }

        public void HandleOne(ITextMessage message)
        {
            string body_json = message.Text;

            ClientMessage body_o = JsonConvert.DeserializeObject<ClientMessage>(body_json);

            Debug.Assert(body_o.connection_id == message.Properties["ConnectionID"].ToString(),
                "connection id in body and header do not match: {0} and {1}", body_o.connection_id, message.Properties["ConnectionID"].ToString());

            string res = "";
            switch (body_o.action)
            {
                case "init":
                    //ArrayList s = JsonConvert.DeserializeObject<ArrayList>(body_o.param);

                    int group_x = 0, group_y = 0;
                    InitDataStruct d = new InitDataStruct();
                    d.group_x = group_x;
                    d.group_y = group_y;

                    using (MineGroupUtils m = new MineGroupUtils())
                    {
                        byte[] data = m.getBin(group_x, group_y);
                    }

                    res = JsonConvert.SerializeObject(d);
                    Debug.WriteLine(res);

                    _hub_context.Clients.Client(body_o.connection_id).sendToClient(res);
                    break;
                case "click":
                    res = "";

                    _hub_context.Clients.All.sendToClient(res);
                    break;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class InitDataStruct
    {
        [JsonProperty]
        const string type = "init";
        [JsonProperty]
        public int group_x { get; set; }
        [JsonProperty]
        public int group_y { get; set; }
        [JsonProperty]
        public string data { get; set; }
    }
}