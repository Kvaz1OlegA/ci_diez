using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class ETLXmlOptions : ETLOptions
    {
        public ETLXmlOptions(string xml) : base()
        {
            ETLOptions options = Converter.DeserializeXML<ETLOptions>(xml);
            SendingOptions = options.SendingOptions;
        }
    }
}
