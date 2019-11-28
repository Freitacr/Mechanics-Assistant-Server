using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <see cref="CommandLineCommand"/> used to provide the format of all <see cref="CommandLineCommand"/>
    /// classes supported by this program
    /// </summary>
    class HelpCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from the other commands in this package
        /// </summary>
        [KeyedArgument("-h")]
        public string Flag = default;

        /// <summary>
        /// Constructs and prints out a help string for all <see cref="CommandLineCommand"/> classes supported by this program
        /// </summary>
        /// <param name="manipulator">Unused parameter</param>
        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            var commands = ReflectionHelper.GetAllCommands();
            string format = "Command{0}:\n\tKeyed Arguments: {1}\n\tPositional Arguments: {2}\n";
            int index = 1;
            foreach(CommandLineMapping mapping in commands)
            {
                //Determine all keyed and positional arguments required by the current command
                //these arguments are determined by the attributes applied to the fields of the current command
                List<string> keyedArguments = new List<string>();
                List<string> positionalArguments = new List<string>();
                foreach(FieldInfo f in mapping.KeyedFields)
                {
                    KeyedArgument arg = (KeyedArgument)System.Attribute.GetCustomAttribute(f, typeof(KeyedArgument));
                    string toAdd = null;
                    if (arg.ValueRequired)
                    {
                        toAdd = arg.Key + ' ' + arg.RequiredValue;
                    }
                    else
                    {
                        toAdd = arg.Key + ' ' + "%" + f.Name + "%: " + f.FieldType.Name;
                    }
                    keyedArguments.Add(toAdd);
                }
                foreach (FieldInfo f in mapping.PositionalFields)
                {
                    PositionalArgument arg = (PositionalArgument)System.Attribute.GetCustomAttribute(f, typeof(PositionalArgument));
                        while (arg.Position >= positionalArguments.Count)
                            positionalArguments.Add(null);

                    positionalArguments[arg.Position] = "\"%" + f.Name + "%\": " + f.FieldType.Name;
                }
                //Display the formatted help string
                string toPrint = string.Format(format, index, string.Join(' ', keyedArguments), string.Join(' ', positionalArguments));
                Console.WriteLine(toPrint);
                index++;
            }
        }
    }
}
