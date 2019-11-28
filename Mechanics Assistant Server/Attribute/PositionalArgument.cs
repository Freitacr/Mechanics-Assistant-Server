using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Attribute
{
    /// <summary>
    /// <para>Attribute to used to mark a field as being a recepticle for a positional command line argument</para>
    /// <para>For information on how a positional argument is defined in this context see <see cref="CommandLineArgumentParser(string[])"/></para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class PositionalArgument : System.Attribute
    {
        /// <summary>
        /// Zero-based index of the CommandLineArgumentParser's PositionalArguments to retrieve the value of
        /// </summary>
        public int Position { get; private set; }

        /// <summary>
        /// Marks a field as being the recepticle of a positional argument from a <see cref="CommandLineArgumentParser"/>
        /// </summary>
        /// <param name="position">Zero-based index of the CommandLineArgumentParser's PositionalArguments to retrieve the value of</param>
        public PositionalArgument(int position)
        {
            Position = position;
        }
    }
}
