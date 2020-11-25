using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace FileWatcher
{
    public static class Converter
    {
        public static T DeserializeJson<T>(string json)
        {
            List<ParsedObject> objects = ParseJson(json);
            return Deserialize<T>(objects);
        }

        public static T DeserializeXML<T>(string xml)
        {
            List<ParsedObject> objects = ParseXML(xml, true);
            return Deserialize<T>(objects);
        }

        static T Deserialize<T>(List<ParsedObject> objects)
        {
            T ans;
            Type type = typeof(T);

            if(objects.Count == 1 && objects.First().Key == "")
            {
                if(objects.First().Type == ParsedObject.ObjectType.SimpleObject)
                {
                    return (T)Convert.ChangeType(objects.First().Value.Trim('\"'), type);
                }
                else if(objects.First().Type == ParsedObject.ObjectType.Array)
                {        
                    if(IsArray(type))
                    {
                        
                        Type typeOfList = type.GenericTypeArguments.Length == 0 ? type.GetElementType() : type.GenericTypeArguments[0];
                        IList list = ParseArray(typeOfList, objects.First());
                        return (T)ConvertToArray(list, type);
                    }
                    else
                    {
                        throw new Exception("This string can't be parsed into this type");
                    }
                }
            }
            if(IsArray(type))
            {
                Type typeOfList = type.GenericTypeArguments.Length == 0 ? type.GetElementType() : type.GenericTypeArguments[0];
                if(objects.Count > 0)
                {
                    objects.First().Type = ParsedObject.ObjectType.XmlArray;
                    IList list = ParseArray(typeOfList, objects.First());
                    return (T)ConvertToArray(list, type);
                }
                
            }
            ans = (T)Activator.CreateInstance(type);

            foreach(ParsedObject parsedObject in objects)
            {
                string key = parsedObject.Key;
                string value = parsedObject.Value.Trim('\"');
                Type memberType = GetMemberType(type, key);
                if(parsedObject.Type == ParsedObject.ObjectType.SimpleObject)
                {
                    object converted;
                    if(memberType.IsEnum)
                    {
                        converted = Enum.Parse(memberType, value);
                    }
                    else
                    {
                        converted = Convert.ChangeType(value, memberType);
                    }
                    SetMemberValue(ans, key, converted);
                }
                else if(parsedObject.Type == ParsedObject.ObjectType.ComplexObject)
                {
                    object parsed = typeof(Converter).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Static)
                                                     .MakeGenericMethod(memberType)
                                                     .Invoke(null, new object[] { ParseJson(value) });
                    SetMemberValue(ans, key, parsed);
                }
                else if(parsedObject.Type == ParsedObject.ObjectType.XmlComplex)
                {
                    
                    if(IsArray(GetMemberType(type, key)))
                    {
                        SetArrayValue(ans, key, parsedObject);
                    }
                    else
                    {
                        object parsed = typeof(Converter).GetMethod("Deserialize", BindingFlags.NonPublic | BindingFlags.Static)
                                                     .MakeGenericMethod(memberType)
                                                     .Invoke(null, new object[] { ParseXML(value, false) });
                        SetMemberValue(ans, key, parsed);
                    }
                    
                }
                else
                {
                    if(IsArray(memberType))
                    {
                        SetArrayValue(ans, key, parsedObject);
                    }            
                }
            }
            return ans;
        }

        public static string SerializeJson(object obj)
        {
            return SerializeJson(obj, 0).Trim('\n');
        }

        static string SerializeJson(object obj, int deep)
        {
            Type type = obj.GetType();
            StringBuilder sb;
            if(type.GetCustomAttribute(typeof(JsonIgnore)) != null)
            {
                return "";
            }
            if(type.IsPrimitive || type.IsEnum)
            {
                sb = new StringBuilder($"{obj}");
            }
            else if(type == typeof(string))
            {
                sb = new StringBuilder($"\"{obj}\"");
            }
            else if(IsArray(type))
            {
                bool isComplex = false;
                sb = new StringBuilder("");
                int i = 0;
                int length = ((IEnumerable)obj).Length();
                foreach(object obj1 in (IEnumerable)obj)
                {
                    string value = $"{SerializeJson(obj1, deep + 1)}";
                    if(value.Trim(new char[] {'\n', '\t', ' '}).First() == '{')
                    {
                        isComplex = true;
                    }
                    if(i < length - 1)
                    {
                        value = value.TrimEnd('\n');
                        value += ",";  
                    }
                    sb.Append(value);
                    i++;
                }
                if(isComplex)
                {
                    sb.Insert(0, $"\n{new string('\t', deep)}[");
                    sb.AppendLine();
                    sb.Append($"{new string('\t', deep)}]");
                }
                else
                {
                    sb.Insert(0, "[");
                    sb.Append("]");
                }
            }
            else
            {
                sb = new StringBuilder($"\n{new string('\t', deep)}{{\n");
                MemberInfo[] members = type.GetProperties();
                members = members.Concat(type.GetFields()).ToArray();
                
                for(int i = 0; i < members.Length; i++)
                {
                    if(members[i].GetCustomAttribute(typeof(JsonIgnore)) != null)
                    {
                        continue;
                    }
                    object obj1 = GetMemberValue(obj, members[i].Name);
                    string value = $"{SerializeJson(obj1, deep + 1)}";
                    sb.Append($"{new string('\t', deep + 1)}{members[i].Name} : {value}".TrimEnd('\n'));
                    if(i != members.Length - 1)
                    {
                        sb.Append(',');
                    }
                    sb.AppendLine();
                }
                sb.Append($"{new string('\t', deep)}}}\n");
                
            }
            return sb.ToString();
        }

        public static string SerializeXML(object obj)
        {
            return SerializeXML(obj, 0, "");
        }

        static string SerializeXML(object obj, int deep, string name)
        {
            Type type = obj.GetType();
            StringBuilder sb;
            if(type.GetCustomAttribute(typeof(XMLIgnore)) != null)
            {
                return "";
            }
            if(type.IsPrimitive || type.IsEnum || type == typeof(string))
            {
                name = name == "" ? type.Name : name;
                sb = new StringBuilder($"{new string('\t', deep)}<{name}>{obj}</{name}>\n");
            }
            else if(IsArray(type))
            {
                Type arrayType = type.GenericTypeArguments.Length == 0 ?
                        type.GetElementType() :
                        type.GenericTypeArguments[0];
                if(name == "")
                {
                    name = $"ArrayOf{arrayType.Name}";
                }
                sb = new StringBuilder($"{new string('\t', deep)}<{name}>\n");
                string name1 = arrayType.Name;
                //sb = new StringBuilder();//$"{new string('\t', deep)}<{name}>");
                foreach(object obj1 in (IEnumerable)obj)
                {
                    sb.Append(SerializeXML(obj1, deep + 1, name1));
                }
                sb.AppendLine($"{new string('\t', deep)}</{name}>");
            }
            else
            {
                name = name == "" ? type.Name : name;
                sb = new StringBuilder($"{new string('\t', deep)}<{name}>\n");
                MemberInfo[] members = type.GetProperties();
                members = members.Concat(type.GetFields()).ToArray();

                for(int i = 0; i < members.Length; i++)
                {
                    if(members[i].GetCustomAttribute(typeof(XMLIgnore)) != null)
                    {
                        continue;
                    }
                    object obj1 = GetMemberValue(obj, members[i].Name);
                    string value = SerializeXML(obj1, deep + 1, members[i].Name);
                    if(i == members.Length - 1)
                    {
                        value = value.TrimEnd(new char[] { '\t', '\n', ' ' });
                    }
                    sb.Append(value);
                }
                sb.AppendLine($"\n{new string('\t', deep)}</{name}>");
            }
            return sb.ToString();
        }

        static IList ParseArray(Type type, ParsedObject obj)
        {
            List<object> objects = new List<object>();
            Type listType = typeof(List<>).MakeGenericType(new Type[] { type });
            IList list = Activator.CreateInstance(listType) as IList;
            string methodName;
            if(obj.Type == ParsedObject.ObjectType.XmlComplex)
            {
                methodName = "DeserializeXML";
                obj = ParseXML(obj.Value, false).First();
            }
            else if(obj.Type == ParsedObject.ObjectType.Array)
            {
                methodName = "DeserializeJson";
            }
            else
            {
                methodName = "DeserializeXML";
            }
            foreach(string value1 in obj.ValueArray)
            {
                list.Add(typeof(Converter).GetMethod(methodName)
                                          .MakeGenericMethod(new Type[] { type })
                                          .Invoke(null, new object[] { value1.Trim() }));
            }
            return list;
        }

        static Type GetMemberType(Type type, string memberName)
        {
            Type type1 = type.GetProperty(memberName)?.PropertyType;
            if(type1 == null)
            {
                type1 = type.GetField(memberName)?.FieldType;
            }
            if(type1 == null)
            {
                throw new Exception("This type doesn't contain member with this name");
            }
            return type1;
        }

        static object ConvertToArray(IList list, Type type)
        {
            Type typeOfList = type.GenericTypeArguments.Length == 0 ? type.GetElementType() : type.GenericTypeArguments[0];
            if(type.IsArray)
            {
                Array array = Array.CreateInstance(typeOfList, list.Count);
                list.CopyTo(array, 0);
                return array;
            }
            else
            {
                Type IE = typeof(IEnumerable<>).MakeGenericType(new Type[] { typeOfList });
                ConstructorInfo con = type.GetConstructor(new Type[] { IE });
                if(con != null)
                {
                    return Activator.CreateInstance(type, new object[] { list });
                }
                else
                {
                    throw new Exception("This list cannot be converted to this type");
                }
            }
        }

        static void SetArrayValue(object obj, string memberName, ParsedObject parsedObject)
        {
            Type memberType = GetMemberType(obj.GetType(), memberName);
            Type typeOfList = memberType.GenericTypeArguments.Length == 0 ? memberType.GetElementType() : memberType.GenericTypeArguments[0];
            Type listType = typeof(List<>).MakeGenericType(typeOfList);
            IList list = ParseArray(typeOfList, parsedObject);
            object converted = ConvertToArray(list, GetMemberType(obj.GetType(), memberName));
            SetMemberValue(obj, memberName, converted);
        }

        static object GetMemberValue(object obj, string memberName)
        {
            Type type = obj.GetType();
            if(type.GetProperty(memberName) != null)
            {
                PropertyInfo info = type.GetProperty(memberName);
                return info.GetValue(obj);
            }
            else if(type.GetField(memberName) != null)
            {
                FieldInfo info = type.GetField(memberName);
                return info.GetValue(obj);
            }
            else
            {
                throw new Exception("This type doesn't contain member with this name");
            }
        }

        static void SetMemberValue(object obj, string memberName, object value)
        {
            Type type = obj.GetType();
            if(type.GetProperty(memberName) != null)
            {
                PropertyInfo info = type.GetProperty(memberName);
                info.SetValue(obj, value);
            }
            else if(type.GetField(memberName) != null)
            {
                FieldInfo info = type.GetField(memberName);
                info.SetValue(obj, value);
            }
            else
            {
                throw new Exception("This type doesn't contain member with this name");
            }
            
        }

        static int Length(this IEnumerable ie)
        {
            int length = 0;
            foreach(object item in ie)
            {
                length++;
            }
            return length;
        }

        static List<ParsedObject> ParseJson(string json)
        {
            List<ParsedObject> objects = new List<ParsedObject>();
            List<string> values = new List<string>();
            string key = "", value = "";
            int braces = 0, squares = 0;
            bool isKey = true, quotes = false;
            Regex trimming = new Regex("^\\s*{(.*)}\\s*$", RegexOptions.Singleline);
            Match match = trimming.Match(json);
            if(match.Success)
            {
                json = match.Groups[1].Value;
            }
            for(int i = 0; i < json.Length; i++)
            {
                char c = json[i];
                if(char.IsLetterOrDigit(c) || char.IsPunctuation(c) || quotes)
                {
                    if(c == '\"')
                    {
                        quotes = !quotes;
                    }
                    if(quotes)
                    {
                        if(isKey)
                        {
                            key += c;
                        }
                        else
                        {
                            value += c;
                        }
                        continue;
                    }

                    if(c == '{')
                    {
                        braces++;
                    }
                    else if(c == '}')
                    {
                        braces--;
                    }
                    else if(c == '[' && braces == 0)
                    {
                        squares++;
                        if(squares == 1)
                        {
                            continue;
                        }
                    }
                    else if(c == ']' && braces == 0)
                    {
                        squares--;
                        if(squares == 0)
                        {
                            continue;
                        }
                    }
                    else if(c == ':')
                    {        
                        if(braces == 0 && squares == 0)
                        {
                            isKey = false;
                            continue;
                        }
                        
                    }
                    else if(c == ',' && braces == 0)
                    {
                        if(isKey)
                        {
                            value = key;
                            key = "";
                        }
                        if(braces == 0)
                        {
                            values.Add(value);
                            value = "";
                        }
                        if(squares == 0)
                        {
                            objects.Add(new ParsedObject(key, values));
                            values = new List<string>();
                            key = "";
                            value = "";
                            isKey = true;
                        }
                        continue;
                    }
                    if(isKey)
                    {
                        key += c;
                    }
                    else
                    {
                        value += c;
                    }
                }
            }
            if(braces == 0 && squares == 0)
            {
                if(!(key == "" && value == ""))
                {
                    if(isKey)
                    {
                        values.Add(key);
                        key = "";
                    }
                    else
                    {
                        values.Add(value);
                    }
                    objects.Add(new ParsedObject(key, values));
                } 
            }
            else
            {
                throw new Exception("Json object is incorrect");
            }
            return objects;
        }

        static List<ParsedObject> ParseXML(string xml, bool trim)
        {
            xml = xml.Trim(new char[] { '\n', '\t', '\r', ' ' });
            List<ParsedObject> objects = new List<ParsedObject>();
            List<string> values = new List<string>();
            string tagName;
            Match match;
            try
            {
                tagName = GetNextTag(xml, 0);
                if(trim)
                {
                    Regex trimming = new Regex($"^<{tagName}>(.*)</{tagName}>$", RegexOptions.Singleline);
                    match = trimming.Match(xml);
                    if(match.Success)
                    {
                        xml = match.Groups[1].Value;
                    }
                }
            }
            catch
            {
                return new List<ParsedObject>() { new ParsedObject("", new List<string>() { xml }, ParsedObject.ObjectType.SimpleObject) };
            }

            Regex Tag = new Regex(@"<(/?.*)>");

            Dictionary<string, List<string>> keyValues = new Dictionary<string, List<string>>();
            string mainTag = "", tag = "";
            int deep = 0;
            bool isMainTag = true, isValue = false;
            string value = "";
            bool quotes = false;
            foreach(char c in xml)
            {
                if((c != '\t' && c != '\r' && c != '\n') || quotes)
                {
                    if(c == '\"')
                    {
                        quotes = !quotes;
                    }
                    if(quotes)
                    {
                        value += c;
                        continue;
                    }
                    if(c == '<')
                    {
                        isValue = false;
                        if(!isMainTag)
                        {
                            tag += c;
                        }
                    }
                    else if(c == '>')
                    {
                        if(isMainTag)
                        {
                            isMainTag = false;
                            isValue = true;
                            deep++;
                        }
                        else
                        {
                            tag += c;
                            match = Tag.Match(tag);

                            if(match.Success)
                            {
                                tagName = match.Groups[1].Value;
                                if(tagName[0] == '/')
                                {
                                    if('/' + mainTag == tagName && deep == 1)
                                    {
                                        if(keyValues.ContainsKey(mainTag))
                                        {
                                            keyValues[mainTag].Add(value);
                                        }
                                        else
                                        {
                                            keyValues.Add(mainTag, new List<string>() { value });
                                        }
                                        mainTag = "";
                                        tag = "";
                                        isMainTag = true;
                                        isValue = false;
                                        value = "";
                                    }
                                    else
                                    {
                                        value += tag;
                                        tag = "";
                                        isValue = true;
                                    }
                                    deep--;
                                }
                                else
                                {
                                    deep++;
                                    isValue = true;
                                    value += tag;
                                    tag = "";
                                }
                            }
                            else
                            {
                                throw new Exception("File was damaged");
                            }
                        }
                    }
                    else
                    {
                        if(isValue)
                        {
                            value += c;
                        }
                        else if(isMainTag)
                        {
                            mainTag += c;
                        }
                        else
                        {
                            tag += c;
                        }
                    }
                }
            }
            if(mainTag != "")
            {
                return new List<ParsedObject>() { new ParsedObject("", new List<string>() { mainTag }, ParsedObject.ObjectType.SimpleObject) };
            }
            foreach(KeyValuePair<string, List<string>> pair in keyValues)
            {
                ParsedObject.ObjectType type;
                if(pair.Value.First().Length > 0 && pair.Value.First()[0] == '<')
                {
                    type = ParsedObject.ObjectType.XmlComplex;
                }
                else
                {
                    type = ParsedObject.ObjectType.SimpleObject;
                }
                objects.Add(new ParsedObject(pair.Key, pair.Value, type));
            }
            return objects;
        }

        static string GetNextTag(string xml, int startIndex)
        {
            StringBuilder tag = new StringBuilder("");
            bool isTag = false;
            for(int i = startIndex; i < xml.Length; i++)
            {
                if(xml[i] == '<')
                {
                    isTag = true;
                    continue;
                }
                else if(xml[i] == '>')
                {
                    return tag.ToString();
                }
                else if(isTag)
                {
                    tag.Append(xml[i]);
                }
            }
            throw new Exception("Cant't find matching tag");
        }

        static bool IsArray(Type type)
        {
            if(type == typeof(string))
            {
                return false;
            }
            return type.IsArray || type.GetInterface(nameof(IEnumerable)) != null;
        }
    }
}
