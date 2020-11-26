using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class XmlOptions : ETLOptions
    {
        public XmlOptions(string xml) : base()
        {
            ETLOptions options = Parser.DeserializeXML<ETLOptions>(xml);
            SendingOptions = options.SendingOptions;
        }
    }
}
