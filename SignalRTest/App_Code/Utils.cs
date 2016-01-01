using Apache.NMS;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace SignalRTest.App_Code
{
    public class Utils
    {
        const String user = "admin";
        const String pass = "password";
        const String host = "localhost";
        const int port = 61613;
        const String dest = "/queue/mine";

        static readonly String brokerUri = "stomp:tcp://" + host + ":" + port + "?transport.useLogging=true";
        static readonly NMSConnectionFactory connectionFactory = new NMSConnectionFactory(brokerUri);

        public static ActiveMQConsumer getConsumer()
        {
            return new ActiveMQConsumer(connectionFactory, user, pass, dest);
        }

        public static ActiveMQProducer getProducer()
        {
            return new ActiveMQProducer(connectionFactory, user, pass, dest);
        }
    }

    public class MineGroup
    {
        public char[] getBin(int group_x, int group_y)
        {
            SqlConnection sqlConnection1 = new SqlConnection("Data Source=C-ASUS\\SQLEXPRESS;Initial Catalog=Mine;Integrated Security=True");
            return null;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class ClientMessage
    {
        [JsonProperty]
        public string action { get; set; }
        [JsonProperty]
        public string param { get; set; }
        [JsonProperty]
        public string connection_id { get; set; }
    }
}