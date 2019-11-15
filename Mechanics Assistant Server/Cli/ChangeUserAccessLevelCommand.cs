﻿using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;

namespace OldManInTheShopServer.Cli
{
    class ChangeUserAccessLevelCommand : CommandLineCommand
    {
        [KeyedArgument("-u", true, "access")]
        public string Flag;

        [PositionalArgument(0)]
        public int UserId;

        [PositionalArgument(1)]
        public int NewAccessLevel;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {
            var user = manipulator.GetUserById(UserId);
            user.AccessLevel = NewAccessLevel;
            if (!manipulator.UpdateUserAccessLevel(user))
            {
                Console.WriteLine("Failed to update user's access level");
                return;
            }
            return;
        }
    }
}