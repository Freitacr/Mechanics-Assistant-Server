using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <see cref="CommandLineCommand"/> used to change a user's access level
    /// </summary>
    class ChangeUserAccessLevelCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from the other commands in this package
        /// </summary>
        [KeyedArgument("-u", true, "access")]
        public string Flag = default;

        /// <summary>
        /// Database id of the user to update the access level of
        /// </summary>
        [PositionalArgument(0)]
        public int UserId = default;

        /// <summary>
        /// <para>Int representing the new access level of the user with the specified id</para>
        /// <para>For more information on access levels see <see cref="AccessLevelMasks"/></para>
        /// </summary>
        [PositionalArgument(1)]
        public int NewAccessLevel = default;

        /// <summary>
        /// Updates the user specified by this object's fields to have the specified access level
        /// </summary>
        /// <param name="manipulator"><see cref="MySqlDataManipulator"/> used to update the user's access level</param>
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
