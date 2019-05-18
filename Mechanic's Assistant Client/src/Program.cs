using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mechanics_Assistant_Client.Forms;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Runtime.Serialization;
using Mechanic_Assistant_Client.src;

namespace Mechanics_Assistant_Client
{

    [DataContract]
    class MechanicsEntry
    {

        [DataMember]
        public string Model { get; set; }

        [DataMember]
        public string Make { get; set; }

        [DataMember]
        public string Complaint { get; set; }

        public MechanicsEntry(string make, string model, string complaint)
        {
            Make = make;
            Model = model;
            Complaint = complaint;
        }
    }

    
    [DataContract]
    public class ProblemStorage
    {
        [DataMember]
        public string Problem { get; set; }
        public ProblemStorage(string problem)
        {
            Problem = problem;
        }
    }

    static class Program
    {

        public static void OpenOutfileStream(string fileName, out StreamWriter fileWriterOut)
        {
            try
            {
                fileWriterOut = new StreamWriter(fileName);
            } catch(FileNotFoundException)
            {
                throw new AccessViolationException("Could not access file " + fileName);
            }
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            bool ShouldQuery;
            do
            {
                MechanicsAssistant MainForm = new MechanicsAssistant();
                Application.Run(MainForm);
                ShouldQuery = false;
                if (MainForm.ShouldQuery)
                {
                    string make = MainForm.GetMakeTextBoxValue();
                    string model = MainForm.GetModelTextBoxValue();
                    string complaint = MainForm.GetComplaintTextBoxValue();
                    MechanicsEntry entry = new MechanicsEntry(make, model, complaint);
                    DataContractJsonSerializer entrySerializer = new DataContractJsonSerializer(typeof(MechanicsEntry));
                    OpenOutfileStream("entry.txt", out StreamWriter fileOut);
                    entrySerializer.WriteObject(fileOut.BaseStream, entry);
                    fileOut.Close();
                    ProblemPredictionWrapper programWrapper = new ProblemPredictionWrapper("ProblemPrediction.exe");
                    programWrapper.AddArguments("entry.txt");
                    programWrapper.StartProblemPredictionProgram();
                    StreamReader returnVal = programWrapper.BlockAndGetStream();
                    DataContractJsonSerializer resultSerializer = new DataContractJsonSerializer(typeof(List<ProblemStorage>));
                    var returnValues = (List<ProblemStorage>)resultSerializer.ReadObject(returnVal.BaseStream);
                    returnVal.Close();
                    MechanicsAssistantOutputForm outputForm = new MechanicsAssistantOutputForm();
                    outputForm.AddData(returnValues);
                    Application.Run(outputForm);
                    ShouldQuery = outputForm.ShouldQueryAgain;
                }
            } while (ShouldQuery);
        }
    }
}
