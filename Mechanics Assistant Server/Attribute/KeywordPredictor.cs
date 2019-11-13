using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    /// <summary>
    /// Class for marking Keyword Predictors for global loading
    /// </summary>
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
