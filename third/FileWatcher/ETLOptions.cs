using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace FileWatcher
{
    public class ETLOptions : Options
    {
        public SendingOptions SendingOptions { get; set; } = new SendingOptions();

        public string Report { get; protected set; } = "";
        public ETLOptions()
        {

        }

        public ETLOptions(SendingOptions sendingOptions)
        {
            SendingOptions = sendingOptions;
        }
    }
}
