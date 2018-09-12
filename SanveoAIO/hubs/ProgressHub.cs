using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace SanveoAIO.hubs
{
    public class ProgressHub :Hub
    {
        public string msg = "Initializing and Preparing...";
        public int count = 1;

        public static void SendMessage(string msg, int count)
        {
            var message = "Process completed for " + msg;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.sendMessage(string.Format(message), count);
        }

        public static void GetLoop(string msg, int count, int userid)
        {
            var userdata = "progressBar" + userid;
            var message = "Process completed for " + msg;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.getloop(string.Format(message), count, string.Format(userdata));
        }

        public void GetCountAndMessage()
        {
            Clients.Caller.sendMessage(string.Format(msg), count);
        }

        public static void GetReportProgressLoop(string msg, int count, int userid)
        {
            var userdata = "ReportprogressBar" + userid;
            var message = "Process completed for " + msg;
            var hubContext = GlobalHost.ConnectionManager.GetHubContext<ProgressHub>();
            hubContext.Clients.All.getreportProgressLoop(string.Format(message), count, string.Format(userdata));
        }
    }
}