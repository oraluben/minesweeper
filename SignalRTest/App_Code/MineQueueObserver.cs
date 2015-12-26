using Apache.NMS;
using Microsoft.AspNet.SignalR;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SignalRTest.App_Code
{
    public class MineQueueObserver
    {
        private readonly static IHubContext _hub_context = GlobalHost.ConnectionManager.GetHubContext<MineHub>();
        private readonly Consumer c = Utils.getConsumer();

        public MineQueueObserver()
        {
            Debug.WriteLine("MineQueueObserver instance started");
            Task.Run(() => { Run(); });
        }

        public Task Run()
        {
            while (true)
            {
                IMessage m = c.getMessage();
                Debug.Assert(m is ITextMessage);
                ITextMessage textM = m as ITextMessage;
                Debug.WriteLine("MineQueueObserver get a message, body: {0}", textM.Text);
                HandleOne(textM);
            }
        }

        public static void HandleOne(ITextMessage m)
        {
            _hub_context.Clients.All.sendToClient("test data");

            // handle

            // send result to hubs
        }
    }
}