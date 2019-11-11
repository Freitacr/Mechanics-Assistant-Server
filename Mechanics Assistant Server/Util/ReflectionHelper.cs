using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Models.KeywordPrediction;
using System;
using System.Collections.Generic;
using System.Linq;
using OldManInTheShopServer.Cli;
using System.Reflection;

namespace OldManInTheShopServer.Util
{
    class KeywordPredictorFileMapping
    {
        public Type KeywordPredictorType;
        public string DefaultFileLocation;
    }

    class CommandLineMapping
    {
        public CommandLineCommand Command;
        public List<FieldInfo> PositionalFields = new List<FieldInfo>();
        public List<FieldInfo> KeyedFields = new List<FieldInfo>();
    }

    /// <summary>
    /// Helper class for dealing with the reflection required to retrieve all IKeywordPredictor implementing classes for global instantiation
    /// </summary>
    class ReflectionHelper
    {
        /// <summary>
        /// Attempts to retrieve all IKeywordPredictor implementing classes, parse their KeywordPredictor attribute for a default file location, and return
        /// a mapping of the IKeywordPredictor implementing classes to their default file location
        /// </summary>
        /// <returns>a mapping of the IKeywordPredictor implementing classes to their default file location</returns>
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

        public static IEnumerable<CommandLineMapping> GetAllCommands()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(x => x.GetTypes());
            Type toFind = typeof(CommandLineCommand);
            types = types.Where(x => toFind.IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
            LinkedList<CommandLineMapping> ret = new LinkedList<CommandLineMapping>();
            foreach(Type t in types)
            {
                CommandLineCommand cmd = (CommandLineCommand)t.GetConstructor(new Type[0]).Invoke(null);
                CommandLineMapping mapping = new CommandLineMapping() { Command = cmd };
                var fields = t.GetFields();
                var members = t.GetMembers();
                foreach(FieldInfo f in fields)
                {
                    foreach(CustomAttributeData d in f.CustomAttributes)
                    {
                        if(d.AttributeType.Equals(typeof(KeyedArgument)))
                        {
                            mapping.KeyedFields.Add(f);
                            
                            break;
                        }
                        if (d.AttributeType.Equals(typeof(PositionalArgument)))
                        {
                            mapping.PositionalFields.Add(f);
                            break;
                        }
                    }
                }
                if(mapping.PositionalFields.Count == 0 && mapping.KeyedFields.Count == 0)
                {
                    throw new Exception("Type " + t.Name + " did not have a positional field or keyed field");
                }
                ret.AddLast(mapping);
            }
            return ret;
        }
    }
}
