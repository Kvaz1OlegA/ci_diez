using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Manager
    {
        internal static SendingOptions GetOptions()
        {
            SendingOptions sendingOptions;
            bool jsonOk, xmlOk;
            try
            {
                using (StreamReader sr = new StreamReader("appsettings.json"))
                {
                    string json = sr.ReadToEnd();
                    sendingOptions = Converter.DeserializeJson<SendingOptions>(json);
                    return sendingOptions;
                }
            }
            catch
            {
                jsonOk = false;
            }
            try
            {
                using (StreamReader sr = new StreamReader("config.xml"))
                {
                    string xml = sr.ReadToEnd();
                    sendingOptions = Converter.DeserializeXML<SendingOptions>(xml);
                    return sendingOptions;
                }
            }
            catch
            {
                xmlOk = false;
            }
            sendingOptions = new SendingOptions();
            return sendingOptions;

        }
    }
}
