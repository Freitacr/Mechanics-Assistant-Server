using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    class CommandLineArgumentParser
    {
        public readonly List<string> PositionalArguments;
        public readonly Dictionary<string, string> KeyedArguments;

        public CommandLineArgumentParser(string[] args)
        {
            PositionalArguments = new List<string>();
            KeyedArguments = new Dictionary<string, string>();
            for(int i = 0; i < args.Length; i++)
            {
                if(args[i].StartsWith('-') || args[i].StartsWith("--"))
                {
                    string key = args[i];
                    if(i + 1 == args.Length || args[i+1].StartsWith('-') || args[i+1].StartsWith("--"))
                    {
                        KeyedArguments[key] = "";
                        continue;
                    }
                    KeyedArguments[key] = args[i + 1];
                    i++;
                } else
                {
                    PositionalArguments.Add(args[i]);
                }
            }
        }


    }
}
