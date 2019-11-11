using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class KeyedArgument : System.Attribute
    {
        public string Key { get; private set; }
        public bool ValueRequired { get; private set; }
        public string RequiredValue { get; private set; }

        public KeyedArgument(string requiredKey, bool valueRequired = false, string requiredValue = "")
        {
            Key = requiredKey;
            ValueRequired = valueRequired;
            RequiredValue = requiredValue;
        }
    }
}
