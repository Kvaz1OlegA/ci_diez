using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace FileWatcher
{
    class Converter
    {
        public static T DeserializeJson<T>(string json) where T : new()
        {
            List<string> list = JsonParse(json);
            return Deserialize<T>(list);
        }

        public static T DeserializeXML<T>(string XML) where T : new()
        {
            List<string> list = JsonParse(XML);
            return Deserialize<T>(list);
        }


        public static T Deserialize<T>(List<string> list) where T : new()
        {
            T ans = new T();
            Type type = typeof(T);
            string key = "", value = "";
            Regex complex = new Regex(@"(\w+)\s*:\s*{(.*)}", RegexOptions.Singleline);
            Regex simple = new Regex(@"(\w+)\s*:\s*(.*)", RegexOptions.Singleline);
            Match match;
            foreach(string option in list)
            {
                if(complex.IsMatch(option))
                {
                    match = complex.Match(option);

                    key = match.Groups[1].Value;
                    value = match.Groups[2].Value;

                    FieldInfo info = type.GetField(key);
                    info.SetValue(ans, typeof(Converter)
                        .GetMethod("Deserialize")
                        .MakeGenericMethod(new Type[] { info.FieldType })
                        .Invoke(null, new object[] { value}));
                }
                else if(simple.IsMatch(option))
                {
                    match = simple.Match(option);

                    key = match.Groups[1].Value;
                    value = match.Groups[2].Value;

                    FieldInfo info = type.GetField(key);
                    info.SetValue(ans, Convert.ChangeType(value, info.FieldType));
                }
            }
            return ans;
        }

        public static List<string> JsonParse(string json)
        {
            json = json.Trim(new char[] { ' ', '{', '}' });
            List<string> list = new List<string>();
            int brackets = 0;
            StringBuilder option = new StringBuilder();
            foreach(char symbol in json)
            {
                if(char.IsLetterOrDigit(symbol) || char.IsPunctuation(symbol))
                {
                    if(symbol == ',' && brackets == 0)
                    {
                        list.Add(option.ToString());
                        option.Clear();
                    }
                    else if(symbol == '{')
                    {
                        option.Append(symbol);
                        brackets++;
                    }
                    else if(symbol == '}')
                    {
                        option.Append(symbol);
                        brackets--;
                    }
                    else
                    {
                        option.Append(symbol);
                    }
                }
            }
            if(option.Length != 0)
                list.Add(option.ToString());
            return list;
        }
        public static List<string> XMLParse(string xml, bool trim)
        {
            xml = xml.Trim(new char[] { '\n', '\t', '\r', ' ' }); 
            string tagName;
            Match match;
            tagName = GetNextTag(xml, 0);
            if (trim)
            {
                Regex trimming = new Regex($"^<{tagName}>(.*)</{tagName}>$", RegexOptions.Singleline);
                match = trimming.Match(xml);
                if (match.Success)
                {
                    xml = match.Groups[1].Value;
                }
            }
            List<string> list = new List<string>();
            bool isTag = false;
            StringBuilder option = new StringBuilder();
            string tag="", value="";
            int tagNum = 0;
            foreach(char symbol in xml)
            {
                if (symbol != '\t' && symbol != '\r' && symbol != '\n')
                {
                    if (symbol == '<')
                    {
                        isTag = true;
                        tagNum++;
                        continue;
                    }
                    if (symbol == '>')
                    {
                        isTag = false;
                        if (tagNum == 2)
                        {
                            tag = tag.Remove(tag.IndexOf('/'), tag.Length - tag.IndexOf('/'));
                            option.Append(tag + ':' + value);
                            tagNum = 0;
                            tag = "";
                            value = "";
                            list.Add(option.ToString());
                            option.Clear();
                        }
                        continue;
                    }
                    if (isTag)
                    {
                        tag += symbol;
                    }
                    else
                    {
                        value += symbol;
                    }
                }
            }
            return list;
        }

        static string GetNextTag(string xml, int startIndex)
        {
            StringBuilder tag = new StringBuilder("");
            bool isTag = false;
            for (int i = startIndex; i < xml.Length; i++)
            {
                if (xml[i] == '<')
                {
                    isTag = true;
                    continue;
                }
                else if (xml[i] == '>')
                {
                    return tag.ToString();
                }
                else if (isTag)
                {
                    tag.Append(xml[i]);
                }
            }
            throw new Exception("Cant't find matching tag");
        }
    }
}
