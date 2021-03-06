﻿using System;
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
            public int AccessLevel;

            public JsonDictionaryStringConstructor ConstructCreationMessage()
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("Email", Email);
                ret.SetMapping("SecurityQuestion", SecurityQuestion);
                ret.SetMapping("SecurityAnswer", SecurityAnswer);
                ret.SetMapping("Password", Password);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructReportMessage(int userId, string loginToken, string authToken, string reportedName) {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("UserId", userId);
                ret.SetMapping("LoginToken", loginToken);
                ret.SetMapping("AuthToken", authToken);
                ret.SetMapping("DisplayName", reportedName);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructLoginRequest()
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("Email", Email);
                ret.SetMapping("Password", Password);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructCheckLoginStatusRequest(int userId, string loginToken)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("UserId", userId);
                ret.SetMapping("LoginToken", loginToken);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructChangeSettingRequest(int userId, string loginToken, string authToken, string key, string newValue)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("UserId", userId);
                ret.SetMapping("LoginToken", loginToken);
                ret.SetMapping("AuthToken", authToken);
                ret.SetMapping("Key", key);
                ret.SetMapping("Value", newValue);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructRetrieveSettingsRequest(int userId, string loginToken)
            {
                return ConstructCheckLoginStatusRequest(userId, loginToken);
            }

            public JsonDictionaryStringConstructor ConstructRetrievePreviousRequestsRequest(int userId, string loginToken)
            {
                return ConstructCheckLoginStatusRequest(userId, loginToken);
            }

            public JsonDictionaryStringConstructor ConstructAuthenticationRequest(string loginToken, int userId)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("LoginToken", loginToken);
                ret.SetMapping("UserId", userId);
                ret.SetMapping("SecurityQuestion", SecurityQuestion);
                ret.SetMapping("SecurityAnswer", SecurityAnswer);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructSecurityQuestionRequest(string loginToken, int userId)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("LoginToken", loginToken);
                ret.SetMapping("UserId", userId);
                return ret;
            }

            public JsonDictionaryStringConstructor ConstructCheckAuthenticationStatusRequest(string loginToken, string authToken, int userId)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                ret.SetMapping("LoginToken", loginToken);
                ret.SetMapping("AuthToken", authToken);
                ret.SetMapping("UserId", userId);
                return ret;
            }
        }

        public static readonly TestingUser ValidUser1 = new TestingUser()
        {
            Email = "abcd@ac.com",
            Password = "Pass_Pass_pass_JSON",
            SecurityQuestion = "What is best on toast?",
            SecurityAnswer = "Baked Beans",
            AccessLevel = 1
        };


        public static readonly TestingUser ValidUser2 = new TestingUser()
        {
            Email = "abc@bc.com",
            Password = "12345",
            SecurityQuestion = "Toast?",
            SecurityAnswer = "Toast",
            AccessLevel = 1
        };

        public static readonly TestingUser ValidUser3 = new TestingUser() {
            Email = "cba@cb.com",
            Password= "ManDoorHandHookCarDoor",
            SecurityQuestion = "What is a group of crows called?",
            SecurityAnswer = "A Crowhort",
            AccessLevel = AccessLevelMasks.AdminMask | AccessLevelMasks.PartMask | AccessLevelMasks.SafetyMask | AccessLevelMasks.MechanicMask
        };

        public static readonly TestingUser ValidUser4 = new TestingUser() {
            Email = "cbd",
            Password = "thc",
            SecurityQuestion = "Favourite Building",
            SecurityAnswer = "Dispensory",
            AccessLevel = AccessLevelMasks.MechanicMask | AccessLevelMasks.PartMask
        };

        public static readonly TestingUser ValidUser5 = new TestingUser() {
            Email = "bobbuilder@safety.co.uk",
            Password = "HardHat",
            SecurityQuestion = "What is the best organization on Earth?",
            SecurityAnswer = "OSHA",
            AccessLevel = AccessLevelMasks.MechanicMask | AccessLevelMasks.SafetyMask
        };

    }
}
