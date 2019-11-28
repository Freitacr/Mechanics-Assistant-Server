using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Attribute;
using OldManInTheShopServer.Data.MySql;
using OldManInTheShopServer.Data.MySql.TableDataTypes;
using OldManInTheShopServer.Util;

namespace OldManInTheShopServer.Cli
{
    class AddSettingsCommand : CommandLineCommand
    {
        [KeyedArgument("-a", true, "setting")]
        public string Flag = default;

        [PositionalArgument(0)]
        public string Target = default;

        [PositionalArgument(1)]
        public string Key = default;

        [PositionalArgument(2)]
        public string Value = default;

        public override void PerformFunction(MySqlDataManipulator manipulator)
        {

            if (Target.Equals("user"))
            {
                var users = manipulator.GetUsersWhere("id > 0");
                foreach (OverallUser user in users)
                {
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
