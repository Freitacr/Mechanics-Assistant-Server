using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    class HelpCommand : CommandLineCommand
    {
        [KeyedArgument("-h")]
        public string Flag;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            var commands = ReflectionHelper.GetAllCommands();
            string format = "Command{0}:\n\tKeyed Arguments: {1}\n\tPositional Arguments: {2}\n";
            int index = 1;
            foreach(CommandLineMapping mapping in commands)
            {
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
                string toPrint = string.Format(format, index, string.Join(' ', keyedArguments), string.Join(' ', positionalArguments));
                Console.WriteLine(toPrint);
                index++;
            }
        }
    }
}
