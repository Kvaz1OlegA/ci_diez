using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    public partial class Service1 : ServiceBase
    {
        Server server;
        public Service1()
        {
            InitializeComponent();
            this.CanStop = true;
            this.CanPauseAndContinue = true;
            this.AutoLog = true;
        }

        protected override void OnStart(string[] args)
        {
            server = new Server();
            Thread serverThread = new Thread(new ThreadStart(server.OnStart));
            serverThread.Start();
        }

        protected override void OnStop()
        {
            server.OnStop();
            Thread.Sleep(1000);
        }
    }
}
