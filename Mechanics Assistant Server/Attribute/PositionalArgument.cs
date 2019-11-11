using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PositionalArgument : System.Attribute
    {
        public int Position { get; private set; }

        /// <summary>
        /// Marks a field as being the recepticle of a positional argument from the command line argument parser
        /// </summary>
        /// <param name="position">Zero-based index of the position to retrieve the value from is</param>
        public PositionalArgument(int position)
        {
            Position = position;
        }
    }
}
