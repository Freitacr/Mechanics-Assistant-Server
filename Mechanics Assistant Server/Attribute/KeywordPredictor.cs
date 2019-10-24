using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class KeywordPredictor : System.Attribute
    {
        public string DefaultLocation { get; private set; }

        public KeywordPredictor(string defaultModelFileLocation)
        {
            DefaultLocation = defaultModelFileLocation;
        }
    }
}
