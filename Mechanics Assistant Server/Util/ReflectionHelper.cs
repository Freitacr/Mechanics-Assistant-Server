using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Models.KeywordPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OldManInTheShopServer.Util
{
    class KeywordPredictorFileMapping
    {
        public Type KeywordPredictorType;
        public string DefaultFileLocation;
    }

    class ReflectionHelper
    {
        public static IEnumerable<KeywordPredictorFileMapping> GetAllKeywordPredictors()
        {
            LinkedList<KeywordPredictorFileMapping> ret = new LinkedList<KeywordPredictorFileMapping>();
            var predictors = GetAllKeywordPredictorTypes();
            foreach (Type t in predictors)
            {
                KeywordPredictor attribute = (KeywordPredictor)System.Attribute.GetCustomAttribute(t, typeof(KeywordPredictor));
                if (attribute == null)
                    throw new NullReferenceException("Class with type " + t.Name + " did not contain the attribute of type KeywordPredictor");
                ret.AddLast(new KeywordPredictorFileMapping() { DefaultFileLocation = attribute.DefaultLocation, KeywordPredictorType = t });
            }
            return ret;
        }

        private static IEnumerable<Type> GetAllKeywordPredictorTypes()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            Type toFind = typeof(IKeywordPredictor);
            return types.Where(x => toFind.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
        }
    }
}
