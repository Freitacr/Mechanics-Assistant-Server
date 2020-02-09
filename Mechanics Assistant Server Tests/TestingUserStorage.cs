using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Util;

namespace MechanicsAssistantServerTests
{
    class TestingUserStorage
    {
        internal class TestingUser
        {
            public string Email;
            public string Password;
            public string SecurityQuestion;
            public string SecurityAnswer;

            public JsonDictionaryStringConstructor ConstructCreationMessage()
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("Email", Email);
                ret.SetMapping("SecurityQuestion", SecurityQuestion);
                ret.SetMapping("SecurityAnswer", SecurityAnswer);
                ret.SetMapping("Password", Password);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructLoginRequest()
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("Email", Email);
                ret.SetMapping("Password", Password);
                return ret;
            }
        }

        public static readonly TestingUser ValidUser1 = new TestingUser()
        {
            Email = "abcd@ac.com",
            Password = "Pass_Pass_pass_JSON",
            SecurityQuestion = "What is best on toast?",
            SecurityAnswer = "Baked Beans"
        };

    }
}
