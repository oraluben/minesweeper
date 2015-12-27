using Apache.NMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRTest.App_Code
{
    public class ActiveMQProducer
    {
        IConnection connection;
        ISession session;
        IDestination destination;
        IMessageProducer producer;

        public ActiveMQProducer(NMSConnectionFactory connectionFactory, String user, String pass, String dest)
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

    public class ActiveMQConsumer
    {
        IConnection connection;
        ISession session;
        IDestination destination;
        IMessageConsumer consumer;

        public ActiveMQConsumer(NMSConnectionFactory connectionFactory, String user, String pass, String dest)
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