using OldManInTheShopServer.Data.MySql;
using System;
using System.Collections.Generic;
using System.Text;

namespace OldManInTheShopServer.Cli
{
    abstract class CommandLineCommand
    {
        public abstract void PerformFunction(MySqlDataManipulator manipulator);
    }
}
