using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Util
{
    /// <summary>
    /// Utility class for parsing command line arguments and listing them in a simplistic and easy to access way
    /// </summary>
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
            CombineQuotedPositionalArguments();
        }

        private void CombineQuotedPositionalArguments()
        {
            List<int> indicesToRemove = new List<int>();
            for(int i = 0; i < PositionalArguments.Count; i++)
            {
                if (PositionalArguments[i].StartsWith("\""))
                {
                    int originalIndex = i;
                    i++;
                    bool endFound = false;
                    while(i < PositionalArguments.Count && !PositionalArguments[i].EndsWith("\"")){
                        PositionalArguments[originalIndex] += PositionalArguments[i];
                        indicesToRemove.Add(i);
                        i++;
                        if (i < PositionalArguments.Count && PositionalArguments[i].EndsWith("\""))
                            endFound = true;
                    }
                    if (!endFound)
                        throw new FormatException("Quoted arguments did not contain and ending pair of quotes");
                    PositionalArguments[originalIndex].Remove(0, 1);
                    PositionalArguments[originalIndex].Remove(PositionalArguments[originalIndex].Length - 1);
                }
            }
            for(int i = indicesToRemove.Count-1; i >= 0; i--)
            {
                PositionalArguments.RemoveAt(i);
            }
        }
    }
}
