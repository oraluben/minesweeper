using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRTest.App_Code
{
    public class Utils
    {
        private readonly static IHubContext _instance = GlobalHost.ConnectionManager.GetHubContext<MineHub>();
    }
}