using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    public class SendingOptions : Options
    {
        public string SourceDirectory { get; set; } = @"E:\Dirs\SDir";
        public string TargetDirectory { get; set; } = @"E:\Dirs\TDir\zips";
        public string ArchiveDirectory { get; set; } = @"E:\Dirs\TDir\archive";

        public SendingOptions()
        {

        }
    }
}
