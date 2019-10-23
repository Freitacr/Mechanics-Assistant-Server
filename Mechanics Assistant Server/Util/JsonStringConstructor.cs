using System;
using System.Collections.Generic;
using System.Text;

namespace OldManinTheShopServer.Util
{
    public class JsonStringConstructor
    {
        private Dictionary<string, object> Pairs;

        public JsonStringConstructor()
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
            StringBuilder retBuilder = new StringBuilder();
            retBuilder.Append("{");
            retBuilder.Append(ParseDictionaryStrObj(Pairs));
            retBuilder.Append("}");
            return retBuilder.ToString();
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
            return string.Join(',', retStrings);
        }
    }
}
