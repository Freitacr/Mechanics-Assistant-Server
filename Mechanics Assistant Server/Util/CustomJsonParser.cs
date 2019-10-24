using System;
using System.Collections.Generic;
using System.IO;

namespace OldManInTheShopServer.Util
{
    /**<summary>Custom parser for arbitrary JSON objects, designed because the part of speech tagger came in arbitrary json format and needed to be read.</summary>*/
    public static class CustomJsonParser
    {

        public static List<object> ParseList(StreamReader textReader)
        {
            char currChar = (char)textReader.Peek();
            if (currChar != '[')
                throw new FormatException("JSON list was unparsable. Starting character was not \'[\'");
            currChar = (char)textReader.Read();
            List<object> ret = new List<object>();
            while (true)
            {
                object currObject = ParseValue(textReader); 
                ret.Add(currObject);
                currChar = (char)textReader.Read();
                if (currChar == ']')
                    break;
                else if (currChar != ',')
                    throw new FormatException("JSON List unparseable. No comma or closing square bracket detected after a list element");
                while ((char)textReader.Peek() == ' ')
                    textReader.Read();
            }
            return ret;
        }

        public static Dictionary<object, object> ParseDictionary(StreamReader textReader)
        {
            
            char currChar = (char)textReader.Peek();
            if (currChar != '{')
                throw new FormatException("JSON dictionary was unparsable. Starting character was not \'{\'");
            currChar = (char)textReader.Read();
            Dictionary<object, object> ret = new Dictionary<object, object>();
            if ((char)textReader.Peek() == '}')
            {
                textReader.Read();
                return ret;
            }
            while (true)
            {
                object key = ParseKey(textReader);
                if ((char)textReader.Peek() != ':')
                    throw new FormatException("JSON Dictionary unparsable. Key and value not separated by a colon (\':\')");
                textReader.Read();
                while (textReader.Peek() == ' ')
                    textReader.Read();
                object value = ParseValue(textReader);
                ret[key] = value;
                currChar = (char)textReader.Read();
                if (currChar == '}')
                    break;
                else if (currChar != ',')
                    throw new FormatException("JSON Dictionary unparsable. No comma or closing parenthesis decteded after value in dictionary");
                while (textReader.Peek() == ' ')
                    textReader.Read();
            }
            return ret;
        }

        private static object ParseKey(StreamReader readerIn)
        {
            char currChar = (char)readerIn.Peek();
            if (currChar == '\"')
            {
                string ret = "";
                currChar = (char)readerIn.Read();
                currChar = (char)readerIn.Read();
                while (currChar != '\"')
                {
                    ret += currChar;
                    currChar = (char)readerIn.Read();
                }
                return ret;
            }
            else return null; //Parser is not complete, nor needs to be at the current moment, so this remains.
        }

        private static object ParseValue(StreamReader readerIn)
        {
            char currChar = (char)readerIn.Peek();
            if (currChar == '\"')
            {
                string ret = "";
                currChar = (char)readerIn.Read();
                currChar = (char)readerIn.Read();
                while (currChar != '\"')
                {
                    ret += currChar;
                    currChar = (char)readerIn.Read();
                }
                return ret;
            }
            else if (currChar == '[')
                return ParseList(readerIn);
            else if (currChar == '{')
                return ParseDictionary(readerIn);
            else
            {
                string ret = "";
                //Assume this is a number of some description:
                while (readerIn.Peek() != ',' && readerIn.Peek() != '}')
                    ret += (char)readerIn.Read();
                return ret;
            }
        }
    }
}
