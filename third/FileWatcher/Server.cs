using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Server
    {
        private FileSystemWatcher watcher;
        SendingOptions sendingOptions;
        bool enabled = true;

        public Server()
        {
            Config();
        }

        public void OnStart()
        {
            watcher.EnableRaisingEvents = true;
            while(enabled)
            {
                Thread.Sleep(1000);
            }
        }

        public void OnStop()
        {
            watcher.EnableRaisingEvents = false;
            enabled = false;
        }

        void Config()
        {
            sendingOptions = new SendingOptions();
            sendingOptions = Manager.GetOptions();
            watcher = new FileSystemWatcher(sendingOptions.SourceDirectory, "*.txt");
            watcher.NotifyFilter = NotifyFilters.LastAccess
                                | NotifyFilters.LastWrite
                                | NotifyFilters.FileName
                                | NotifyFilters.DirectoryName;
            watcher.Created += OnCreated; 
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            WaitUntilFileIsReady(e.FullPath);
            string[] date = DateTime.Now.ToString().Split('.', ':', ' ');
            string subDir = Path.Combine(sendingOptions.ArchiveDirectory, date[2], date[1], date[0]);
            if (!Directory.Exists(subDir))
                Directory.CreateDirectory(subDir);
            string file = Path.GetFileNameWithoutExtension(e.Name) + '_' + date[3] + '_' + date[4] + '_' + date[5];
            string sDir_file = sendingOptions.SourceDirectory+ "\\" + e.Name ;
            string sDir_gz = sendingOptions.SourceDirectory+ "\\" + file + ".gz";
            string tDir_file = subDir + "\\" + file + ".txt";
            string tDir_gz = sendingOptions.TargetDirectory+ "\\" + file + ".gz";
            using (Aes aes = Aes.Create())
            {
                Coder.Encryprt(sDir_file, aes.Key, aes.IV);
                Archiver.Compress(sDir_file, sDir_gz);
                File.Move(sDir_gz, tDir_gz);
                Archiver.Decompress(tDir_gz, tDir_file);
                Coder.Decryprt(tDir_file, aes.Key, aes.IV);
                File.Delete(sDir_file);
            }
        }
        private static void WaitUntilFileIsReady(string path)
        {
            while (true)
            {
                try
                {
                    using (var fs = File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite)) 
                    {
                        return;
                    }
                }
                catch{}
            }
        }
    }
}
