using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class JsonOptions : ETLOptions
    {
        public JsonOptions(string json) : base()
        {
            ETLOptions options = Parser.DeserializeJson<ETLOptions>(json);
            SendingOptions = options.SendingOptions;
        } 
    }
}
