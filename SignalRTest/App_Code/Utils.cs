using Apache.NMS;
using System;
using System.Collections.Generic;
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

        public static Consumer getConsumer()
        {
            return new Consumer(connectionFactory, user, pass, dest);
        }

        public static Producer getProducer()
        {
            return new Producer(connectionFactory, user, pass, dest);
        }
    }

    public class Producer
    {
        IConnection connection;
        ISession session;
        IDestination destination;
        IMessageProducer producer;

        public Producer(NMSConnectionFactory connectionFactory, String user, String pass, String dest)
        {
            connection = connectionFactory.CreateConnection(user, pass);
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            destination = session.GetQueue(dest);

            producer = session.CreateProducer(destination);
        }

        public void Send(String body, IDictionary<String, String> headers)
        {
            ITextMessage message = session.CreateTextMessage(body);

            foreach (KeyValuePair<string, string> header in headers)
            {
                message.Properties[header.Key] = header.Value;
            }

            producer.Send(message);
        }
    }

    public class Consumer
    {
        IConnection connection;
        ISession session;
        IDestination destination;
        IMessageConsumer consumer;

        public Consumer(NMSConnectionFactory connectionFactory, String user, String pass, String dest)
        {
            connection = connectionFactory.CreateConnection(user, pass);
            connection.Start();

            session = connection.CreateSession(AcknowledgementMode.AutoAcknowledge);
            destination = session.GetQueue(dest);

            consumer = session.CreateConsumer(destination);
        }

        public IMessage getMessage()
        {
            return consumer.Receive();
        }
    }
}