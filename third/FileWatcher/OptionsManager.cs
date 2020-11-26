using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileWatcher
{
    class OptionsManager
    {
        ETLOptions DefaultOptions;
        JsonOptions Json;
        XmlOptions Xml;
        bool jsonConfigured, xmlConfigured;

        public OptionsManager(string path)
        {
            DefaultOptions = new ETLOptions();
            string options;
            try
            {
                using(StreamReader sr = new StreamReader($"{path}\\config.xml"))
                {
                    options = sr.ReadToEnd();
                }
                Xml = new XmlOptions(options);
                xmlConfigured = true;
            }
            catch
            {
                xmlConfigured = false;
            }
            try
            {
                using(StreamReader sr = new StreamReader($"{path}\\appsettings.json"))
                {
                    options = sr.ReadToEnd();
                }
                Json = new JsonOptions(options);
                jsonConfigured = true;
            }
            catch
            {
                jsonConfigured = false;
            }
            if(!jsonConfigured && !xmlConfigured)
            {
                if(!File.Exists($"{path}\\appsettings.json"))
                {
                    string json = Parser.SerializeJson(DefaultOptions);
                    using(StreamWriter sw = new StreamWriter($"{path}\\appsettings.json"))
                    {
                        sw.Write(json);
                    }
                }
                if(!File.Exists($"{path}\\config.xml"))
                {
                    string xml = Parser.SerializeXML(DefaultOptions);
                    using(StreamWriter sw = new StreamWriter($"{path}\\config.xml"))
                    {
                        sw.Write(xml);
                    }
                }
            }
        }

        public Options GetOptions<T>()
        {
            if(jsonConfigured)
            {
                return SeekForOption<T>(Json);
            }
            else if(xmlConfigured)
            {
                return SeekForOption<T>(Xml);
            }
            else
            {
                return SeekForOption<T>(DefaultOptions);
            }
        }

        Options SeekForOption<T>(ETLOptions options)
        {
            if(typeof(T) == typeof(ETLOptions))
            {
                return options;
            }
            string name = typeof(T).Name;
            try
            {
                return options.GetType().GetProperty(name).GetValue(options, null) as Options;
            }
            catch
            {
                throw new NotImplementedException();
            }
        }
    }
}
