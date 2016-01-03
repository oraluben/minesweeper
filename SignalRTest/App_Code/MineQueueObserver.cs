using Apache.NMS;
using Microsoft.AspNet.SignalR;
using MineGroup;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalRTest.App_Code
{
    public class MineQueueObserver
    {
        private readonly static IHubContext _hub_context = GlobalHost.ConnectionManager.GetHubContext<MineHub>();
        private ActiveMQConsumer c = Utils.getConsumer();

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
                    //Debug.WriteLine("MineQueueObserver get a message, body: {0}", (object)textM.Text);
                    HandleOne(textM);
                }
                catch (NMSException) { c = Utils.getConsumer(); }
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
                    {
                        InitParamMessage d = JsonConvert.DeserializeObject<InitParamMessage>(body_o.param);
                        int group_x = d.group_x;
                        int group_y = d.group_y;

                        using (TestData m = new TestData())
                        {
                            d.data = m.getInit(group_x, group_y);
                        }

                        res = JsonConvert.SerializeObject(new CallBackMessage("init", JsonConvert.SerializeObject(d)));

                        _hub_context.Clients.Client(body_o.connection_id).sendToClient(res);
                        break;
                    }
                case "click":
                    {
                        ClickParamMessage d = JsonConvert.DeserializeObject<ClickParamMessage>(body_o.param);

                        using (TestData context = new TestData())
                        {
                            switch (d.data)  // "left" or "right"
                            {
                                case "left":
                                    {
                                        d.data = context.clickLeft(d.mine_x, d.mine_y);
                                        break;
                                    }
                                case "right":
                                    {
                                        d.data = context.clickRight(d.mine_x, d.mine_y);
                                        break;
                                    }
                                default: break;
                            }
                        }

                        res = JsonConvert.SerializeObject(new CallBackMessage("click", JsonConvert.SerializeObject(d)));

                        _hub_context.Clients.All.sendToClient(res);
                        break;
                    }
            }
        }
    }

    public class TestData : IDisposable
    {
        static Dictionary<int, Dictionary<int, string>> data = new Dictionary<int, Dictionary<int, string>>();
        static Random seed = new Random();

        public void Dispose()
        {
            return;
        }

        public string randomGroup()
        {
            ArrayList res = ArrayList.Repeat(false, 400);
            for (int i = 0; i < 400; i++)
            {
                if (seed.NextDouble() < 0.2) res[i] = true;
            }
            return JsonConvert.SerializeObject(res);
        }

        public bool isMine(int mine_x, int mine_y)
        {
            int offset_x = mine_x % 20, offset_y = mine_y % 20;
            if (offset_x < 0) offset_x += 20;
            if (offset_y < 0) offset_y += 20;
            int group_x = mine_x - offset_x, group_y = mine_y - offset_y;

            if (!data.ContainsKey(group_x)) { data.Add(group_x, new Dictionary<int, string>()); }
            if (!data[group_x].ContainsKey(group_y)) { data[group_x].Add(group_y, randomGroup()); }

            ArrayList res = JsonConvert.DeserializeObject<ArrayList>(data[group_x][group_y]);

            return (bool)res[offset_x + offset_y * 20];
        }

        public int mineCount(int mine_x, int mine_y)
        {
            if (isMine(mine_x, mine_y)) return -1;
            int res = 0;
            for (int _x = -1; _x < 2; _x++)
            {
                for (int _y = -1; _y < 2; _y++)
                {
                    if (_x == 0 && _y == 0) continue;
                    if (isMine(mine_x + _x, mine_y + _y)) res++;
                }
            }
            return res;
        }

        internal string clickLeft(int mine_x, int mine_y)
        {
            MineInfo m = new MineInfo(mine_x, mine_y);
            m.val = mineCount(mine_x, mine_y);
            if (m.val < 0) m.val = -2;
            else m.val += 1;

            return JsonConvert.SerializeObject(ArrayList.Repeat(m, 1));
        }

        internal string clickRight(int mine_x, int mine_y)
        {
            MineInfo m = new MineInfo(mine_x, mine_y);
            m.val = mineCount(mine_x, mine_y);
            if (m.val < 0) m.val = -1;
            else m.val = 0;

            return JsonConvert.SerializeObject(ArrayList.Repeat(m, 1));
        }

        internal string getInit(int group_x, int group_y)
        {
            return JsonConvert.SerializeObject(ArrayList.Repeat(0, 400));
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class InitParamMessage
    {
        [JsonProperty]
        public int group_x { get; set; }
        [JsonProperty]
        public int group_y { get; set; }
        [JsonProperty]
        public string data { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ClickParamMessage
    {
        [JsonProperty]
        public int mine_x { get; set; }
        [JsonProperty]
        public int mine_y { get; set; }
        [JsonProperty]
        public string data { get; set; }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CallBackMessage
    {
        [JsonProperty]
        public string action { get; set; }
        [JsonProperty]
        public string param { get; set; }

        public CallBackMessage(string action, string param)
        {
            this.action = action;
            this.param = param;
        }
    }
}