using System;
using OldManInTheShopServer.Util;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Attribute
{
    /// <summary>
    /// <para>Attribute to mark a CommandLineCommand's Field as a Keyed Argument</para>
    /// <para>A KeyedArgument's Key field is used to specify the flag that must be present for a keyed argument on the command line to
    /// match the field marked with this attribute. An example is provided in the remarks section</para>s
    /// <para>The ValueRequired and RequiredValue field work in tandem with one another, with the ValueRequired field is used to specify
    /// whether the value of RequiredValue is necessary for a keyed argument to match a field marked with this attribute. As before, an
    /// example is provided in the remarks section</para>
    /// <para>For examples of how keys are interpreted in command line arguments see <see cref="CommandLineArgumentParser(string[])"/></para>
    /// </summary>
    /// <remarks>
    /// In this context, a keyed argument is one that is passed in the command line in the following way (assume the program executable is foo.exe:
    ///     <code>foo -c command</code>
    /// In the above case, the command line argument would match a fields marked with
    ///     [KeyedArgument(Key="-c", ValueRequired=true, RequiredValue="command")] and
    ///     [KeyedArgument(Key="-c"]
    /// but would not match a field marked with
    ///     [KeyedArgument(Key="-d")]
    ///     [KeyedArgument(Key="-c", ValueRequired=true, RequiredValue="command2")]
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field)]
    public class KeyedArgument : System.Attribute
    {
        public string Key { get; private set; }
        public bool ValueRequired { get; private set; }
        public string RequiredValue { get; private set; }

        /// <summary>
        /// <para>Constructs a KeyedArgument Attribute for marking a Field as being a recepticle for a keyed command line argument</para>
        /// <para>For examples of the usage of this class see <see cref="KeyedArgument"/></para>
        /// <para>For examples of how keys are interpreted in command line arguments see <see cref="OldManInTheShopServer.Util.CommandLineArgumentParser"/></para>
        /// </summary>
        /// <param name="requiredKey"></param>
        /// <param name="valueRequired"></param>
        /// <param name="requiredValue"></param>
        public KeyedArgument(string requiredKey, bool valueRequired = false, string requiredValue = "")
        {
            Key = requiredKey;
            ValueRequired = valueRequired;
            RequiredValue = requiredValue;
        }
    }
}
