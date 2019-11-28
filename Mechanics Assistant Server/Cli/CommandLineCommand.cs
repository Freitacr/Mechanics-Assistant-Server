using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <para>Abstract class to mark a class as a command that should be run if the command line arguments match its fields</para>
    /// <para>As all commands in this package are intended to only be run by developers with knowledge of the database structure
    /// and the commands themselves, the error reporting of the commands is minimal.</para>
    /// <para>If a more detailed error reporting is desired, a developer can instead use the website to perform many of the same
    /// functions these commands can do</para>
    /// </summary>
    abstract class CommandLineCommand
    {
        /// <summary>
        /// Performs the function the command was set up to do, such as adding a user or company.
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to access the database</param>
        public abstract void PerformFunction(MySqlDataManipulator manipulator);
    }
}
