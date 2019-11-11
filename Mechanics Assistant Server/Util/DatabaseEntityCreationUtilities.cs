using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class DatabaseEntityCreationUtilities
    {
        /// <summary>
        /// Analyzes the command lines stored in <paramref name="argumentsIn"/> and performs any company or user creation necessary
        /// </summary>
        /// <param name="manipulatorIn">Object to use to access the database if needed</param>
        /// <param name="argumentsIn">The command line arguments to analyze</param>
        /// <returns>True if there was a creation request made (thus the program should terminate), false otherwise (the program should continue onward)</returns>
        public static bool PerformRequestedCreation(MySqlDataManipulator manipulatorIn, CommandLineArgumentParser argumentsIn)
        {
            var commands = ReflectionHelper.GetAllCommands();
            foreach(CommandLineMapping mapping in commands)
            {
                if(mapping.KeyedFields.Count != argumentsIn.KeyedArguments.Count)
                    continue;
                if (mapping.PositionalFields.Count != argumentsIn.PositionalArguments.Count)
                    continue;
                bool keysPresent = true;
                foreach(FieldInfo f in mapping.KeyedFields)
                {
                    KeyedArgument arg = (KeyedArgument) System.Attribute.GetCustomAttribute(f, typeof(KeyedArgument));
                    if (!argumentsIn.KeyedArguments.ContainsKey(arg.Key))
                    {
                        keysPresent = false;
                        break;
                    }
                    if (arg.ValueRequired && !argumentsIn.KeyedArguments[arg.Key].Equals(arg.RequiredValue))
                    {
                        keysPresent = false;
                        break;
                    }
                    object value = null;
                    if(f.FieldType.IsPrimitive)
                    {
                        value = f.FieldType.GetMethod("Parse", new[] { typeof(string) }).Invoke(null, new[] { argumentsIn.KeyedArguments[arg.Key] });
                    } else if (f.FieldType.Equals(typeof(string)))
                    {
                        value = argumentsIn.KeyedArguments[arg.Key];
                    }
                    else
                    {
                        throw new ArgumentException("Non primitive field marked as a Keyed Argument in class " + mapping.Command.GetType());
                    }
                    f.SetValue(mapping.Command, value);
                }
                if (!keysPresent)
                    continue;
                keysPresent = true;
                foreach(FieldInfo f in mapping.PositionalFields)
                {
                    PositionalArgument arg = (PositionalArgument)System.Attribute.GetCustomAttribute(f, typeof(PositionalArgument));
                    if(arg.Position > mapping.PositionalFields.Count)
                    {
                        keysPresent = false;
                        break;
                    }
                    object value = null;
                    if (f.FieldType.IsPrimitive)
                    {
                        
                        value = f.FieldType.GetMethod("Parse", new[] { typeof(string) }).Invoke(null, new[] { argumentsIn.PositionalArguments[arg.Position] });
                    }
                    else if (f.FieldType.Equals(typeof(string)))
                    {
                        value = argumentsIn.PositionalArguments[arg.Position];
                    } else
                    {
                        throw new ArgumentException("Non primitive, Non string field marked as a Positional Argument in class " + mapping.Command.GetType());
                    }
                    f.SetValue(mapping.Command, value);
                }
                if (!keysPresent)
                    continue;
                mapping.Command.PerformFunction(manipulatorIn);
                return true;
            }
            return false;
        }
    }
}
