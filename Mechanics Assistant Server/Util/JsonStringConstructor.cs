﻿using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    public class JsonDictionaryStringConstructor
    {
        private Dictionary<string, object> Pairs;

        public JsonDictionaryStringConstructor()
        {
            Pairs = new Dictionary<string, object>();
        }

        public void SetMapping(string key, object value)
        {
            Pairs[key] = value;
        }

        public bool RemoveMapping(string key)
        {
            return Pairs.Remove(key);
        }

        public override string ToString()
        {
            return ParseDictionaryStrObj(Pairs);
        }

        private string ParseDictionaryStrObj(Dictionary<string, object> dictIn)
        {
            List<string> retStrings = new List<string>();
            foreach (KeyValuePair<string, object> pair in dictIn)
            {
                string toAdd = "";
                toAdd += "\"" + pair.Key + "\":";
                if (pair.Value.GetType().Equals(typeof(Dictionary<string, object>)))
                {
                    toAdd += "{";
                    toAdd += ParseDictionaryStrObj((Dictionary<string, object>)pair.Value);
                    toAdd += "}";
                }
                else if (typeof(List<object>).IsAssignableFrom(pair.Value.GetType()))
                {
                    List<string> strings = new List<string>();
                    foreach (object o in (List<object>)pair.Value)
                        strings.Add(o.ToString());
                    toAdd += "[";
                    toAdd += string.Join(',', strings);
                    toAdd += "]";
                }
                else if (pair.Value.GetType().Equals(typeof(string)))
                {
                    toAdd += "\"";
                    toAdd += pair.Value;
                    toAdd += "\"";
                }
                else
                {
                    toAdd += pair.Value.ToString();
                }
                retStrings.Add(toAdd);
            }
            return "{" + string.Join(',', retStrings) + "}";
        }
    }

    public class JsonListStringConstructor
    {
        private List<object> Elements;

        public JsonListStringConstructor()
        {
            Elements = new List<object>();
        }

        public bool AddElement(object element)
        {
            Elements.Add(element);
            return true;
        }

        public override string ToString()
        {
            return ParseObjectList(Elements);
        }

        private string ParseObjectList(List<object> elements)
        {
            StringBuilder ret = new StringBuilder("[");
            List<string> internalStrings = new List<string>();
            foreach(object o in elements)
            {
                string toAdd = "";
                if (o.GetType().Equals(typeof(Dictionary<string, object>)))
                {
                    toAdd += ParseDictionaryStrObj((Dictionary<string, object>)o);
                }
                else if (typeof(List<object>).IsAssignableFrom(o.GetType()))
                {
                    toAdd += ParseObjectList(o as List<object>);
                }
                else if (o.GetType().Equals(typeof(string)))
                {
                    toAdd += "\"";
                    toAdd += o;
                    toAdd += "\"";
                }
                else
                {
                    toAdd += o.ToString();
                }
                internalStrings.Add(toAdd);
            }
            ret.Append(string.Join(',', internalStrings));
            ret.Append("]");
            return ret.ToString();
        }

        private string ParseDictionaryStrObj(Dictionary<string, object> dictIn)
        {
            JsonDictionaryStringConstructor subConstructor = new JsonDictionaryStringConstructor();
            foreach(KeyValuePair<string, object> pair in dictIn)
                subConstructor.SetMapping(pair.Key, pair.Value);
            return subConstructor.ToString();
        }
    }
}
