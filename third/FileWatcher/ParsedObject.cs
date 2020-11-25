using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileWatcher
{
    class ParsedObject
    {
        public enum ObjectType
        {
            SimpleObject,
            ComplexObject,
            Array,
            XmlComplex,
            XmlArray
        }
        public string Key { get; }
        public ObjectType Type { get; set; }
        public string Value { get => ValueArray.First(); }
        public List<string> ValueArray { get; set; }

        public ParsedObject(string key, List<string> values)
        {
            Key = key;
            ValueArray = values;
            if(ValueArray.Count > 1)
            {
                Type = ObjectType.Array;
            }
            else if(Value.Contains('{'))
            {
                Type = ObjectType.ComplexObject;
            }
            else
            {
                Type = ObjectType.SimpleObject;
            }
        }

        public ParsedObject(string key, List<string> values, ObjectType type)
        {
            Key = key;
            ValueArray = values;
            Type = type;
        }
    }
}
