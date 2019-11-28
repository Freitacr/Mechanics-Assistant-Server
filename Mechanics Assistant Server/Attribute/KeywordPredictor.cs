using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    /// <summary>
    /// <para>Class for marking Keyword Predictors for global loading from and saving to a default file location</para>
    /// <para>The field DefaultLocation is used to mark the file path that a IKeywordPredictor should be located at</para>
    /// </summary>
    /// <remarks>
    /// An example of marking a class with this attribute follows:
    ///     [KeywordPredictor("Models/predictor.nbmdl")]
    /// This would mark a class as a KeywordPredictor that should be saved and loaded from the relative path
    /// "Models/predictor.nbmdl"
    /// </remarks>
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
