using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Models;

namespace ConfigurationManager
{
    public class Manager
    {
        string jsonPath;
        string xmlPath;
        public Manager(string json, string xml) 
        {
            jsonPath = json;
            xmlPath = xml;
        }
        public T GetOptions<T>() where T : new()
        {
            T options;
            try
            {
                using (StreamReader sr = new StreamReader(jsonPath))
                {
                    string json = sr.ReadToEnd();
                    options = Converter.DeserializeJson<T>(json);
                    return options;
                }
            }
            catch
            {
            }
            try
            {
                using (StreamReader sr = new StreamReader(xmlPath))
                {
                    string xml = sr.ReadToEnd();
                    options = Converter.DeserializeXML<T>(xml);
                    return options;
                }
            }
            catch
            {
            }
            options = new T();
            return options;

        }
    }
}
