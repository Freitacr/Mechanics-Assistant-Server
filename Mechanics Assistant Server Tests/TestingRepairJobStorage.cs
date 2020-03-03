using System;
using System.Collections.Generic;
using System.Text;
using OldManInTheShopServer.Util;
using OldManInTheShopServer.Data.MySql.TableDataTypes;

namespace MechanicsAssistantServerTests.TestNet
{
    class TestingRepairJobStorage
    {
        internal class TestingRepairJob
        {
            //all fields
            public string JobId;
            public string Make;
            public string Model;
            public string Complaint;
            public string Problem;

            public JsonDictionaryStringConstructor ConstructCreationMessage(int Userid,String LoginToken, String AuthToken,int dup)
            {
                JsonDictionaryStringConstructor ret = new JsonDictionaryStringConstructor();
                JsonDictionaryStringConstructor entry = new JsonDictionaryStringConstructor();
                entry.SetMapping("JobId", JobId);
                entry.SetMapping("Make", Make);
                entry.SetMapping("Model", Model);
                entry.SetMapping("Complaint", Complaint);
                entry.SetMapping("Problem", Problem);
                ret.SetMapping("ContainedEntry", entry);
                ret.SetMapping("UserId", Userid);
                ret.SetMapping("LoginToken", LoginToken);
                ret.SetMapping("AuthToken", AuthToken);
                ret.SetMapping("Duplicate", dup);
                return ret;
            }
            public RepairJobEntry CreateEntry()
            {
                RepairJobEntry retJob = new RepairJobEntry();
                retJob.JobId = JobId;
                retJob.Make = Make;
                retJob.Model = Model;
                retJob.Problem = Problem;
                retJob.Complaint = Complaint;

                return retJob;
            }
        }

        public static readonly TestingRepairJob RepairJob1 = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12345",
            Make="AutoMaker",
            Model="Mod",
            Complaint="Wont start",
            Problem="Out of gas"
        };

        public static readonly TestingRepairJob RepairJob2 = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12346",
            Make = "Autocreater",
            Model = "Modified",
            Complaint = "Wont start",
            Problem = "Dead battery"
        };
        public static readonly TestingRepairJob SimilarJob1 = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "AutoMaker",
            Model = "Mod",
            Complaint = "Wont start",
            Problem = "Out of gas"
        };
        public static readonly TestingRepairJob NoMakeJob = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "",
            Model = "Mod",
            Complaint = "Wont start",
            Problem = "Out of gas"
        };
        public static readonly TestingRepairJob NoModelJob = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "AutoMaker",
            Model = "",
            Complaint = "Wont start",
            Problem = "Out of gas"
        };
        public static readonly TestingRepairJob NoComplaintJob= new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "AutoMaker",
            Model = "Mod",
            Complaint = "",
            Problem = "Out of gas"
        };
        public static readonly TestingRepairJob NoProblemJob = new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "AutoMaker",
            Model = "Mod",
            Complaint = "Wont start",
            Problem = ""
        };
        public static readonly TestingRepairJob AttackJob= new TestingRepairJob()
        {
            //initiliz fields
            JobId = "12347",
            Make = "<AutoMaker",
            Model = "Mod",
            Complaint = "Wont start",
            Problem = "Out of gas"
        };
    }
}
