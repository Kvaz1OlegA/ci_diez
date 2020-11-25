using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class ETLJsonOptions : ETLOptions
    {
        public ETLJsonOptions(string json) : base()
        {
            ETLOptions options = Converter.DeserializeJson<ETLOptions>(json);
            SendingOptions = options.SendingOptions;
        } 
    }
}
