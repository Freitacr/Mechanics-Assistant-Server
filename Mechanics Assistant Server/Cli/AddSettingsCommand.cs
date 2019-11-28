using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    /// <summary>
    /// <para><see cref="CommandLineCommand"/> used to add a setting to either all users or all companies in the database</para>
    /// <para>If a user or company already has the new setting, then they are simply skipped, and their setting's value is
    /// NOT modified</para>
    /// </summary>
    class AddSettingsCommand : CommandLineCommand
    {
        /// <summary>
        /// Flag used to differentiate this command from other command line commands in this package
        /// </summary>
        [KeyedArgument("-a", true, "setting")]
        public string Flag = default;

        /// <summary>
        /// <para>Target of the setting addition. Only two targets are valid, "user" and "company"</para>
        /// </summary>
        [PositionalArgument(0)]
        public string Target = default;

        /// <summary>
        /// <para>The key of the setting to add</para>
        /// </summary>
        [PositionalArgument(1)]
        public string Key = default;

        /// <summary>
        /// <para>The value of the setting to add</para>
        /// </summary>
        [PositionalArgument(2)]
        public string Value = default;

        /// <summary>
        /// <para>Uses the supplied <see cref="MySqlDataManipulator"/> to add the setting to all of the specified targets</para>
        /// </summary>
        /// <param name="manipulator"></param>
        public override void PerformFunction(MySqlDataManipulator manipulator)
        {

            if (Target.Equals("user"))
            {
                var users = manipulator.GetUsersWhere("id > 0");
                foreach (OverallUser user in users)
                {
                    //Add the setting to the user if they do not already have a setting with the same key
                    List<UserSettingsEntry> settings = JsonDataObjectUtil<List<UserSettingsEntry>>.ParseObject(user.Settings);
                    bool found = false;
                    foreach(UserSettingsEntry entry in settings)
                    {
                        if (entry.Key.Equals(Key))
                        {
                            found = true;
                            break;
                        }
                    }
                    if(!found)
                    {
                        settings.Add(new UserSettingsEntry() { Key = Key, Value = Value });
                        user.Settings = JsonDataObjectUtil<List<UserSettingsEntry>>.ConvertObject(settings);
                        if (!manipulator.UpdateUsersSettings(user))
                        {
                            Console.WriteLine("Failed to update settings for user " + user.UserId);
                            continue;
                        }
                        Console.WriteLine("Updated settings for user " + user.UserId);
                        continue;
                    }
                    Console.WriteLine("User " + user.UserId + " already had a setting with key " + Key);
                }
            } else if (Target.Equals("company"))
            {
                var companies = manipulator.GetCompaniesWithNamePortion("");
                foreach(CompanyId company in companies)
                {
                    //Add the setting to the company if it does not already have one with the same key
                    int companyId = company.Id;
                    bool found = manipulator.GetCompanySettingsWhere(companyId, "SettingKey = \"" + Key + "\"").Count == 1;
                    if (!found)
                    {
                        if(!manipulator.AddCompanySetting(companyId, new CompanySettingsEntry(Key, Value)))
                        {
                            Console.WriteLine("Company " + company.LegalName + " failed to have the setting added");
                            continue;
                        }
                        Console.WriteLine("Successfully added setting for company " + company.LegalName);
                        continue;
                    }
                    Console.WriteLine("Company " + company.LegalName + " already had a setting with key " + Key);
                }
            }
        }
    }
}
