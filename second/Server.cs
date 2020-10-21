using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace second
{
    class Server
    {
        private FileSystemWatcher watcher;
        private string sPath = @"C:\Documents\second\SDir\";
        private string tPath = @"C:\Documents\second\TDir\";


        public void OnStart()
        {
            watcher = new FileSystemWatcher(@"C:\Documents\second\SDir\");
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;
            watcher.Created += OnCreated;
            watcher.EnableRaisingEvents = true;
            
        }

        private static void OnCreated(object sender, FileSystemEventArgs e)
        {
            Archiver.Compress(@"C:\Documents\second\SDir\" + e.Name, @"C:\Documents\second\TDir\" + Path.GetFileNameWithoutExtension(e.Name) + ".gz");
        }
    }
}
