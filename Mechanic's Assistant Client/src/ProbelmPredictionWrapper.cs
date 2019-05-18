using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Mechanic_Assistant_Client.src
{
    class ProblemPredictionWrapper
    {

        private ProcessStartInfo ProcessInfo;
        private Process ProblemPredictionProgram;
        private string OutFileName;

        public ProblemPredictionWrapper(string fileName)
        {
            ProcessInfo = new ProcessStartInfo(fileName);
        }
        
        public void AddArguments(string filename, params string[] args)
        {
            if (args != null)
            {
                ProcessInfo.Arguments = "-f " + filename + " " + string.Join(" ", args);
            } else
            {
                ProcessInfo.Arguments = "-f " + filename;
            }
            OutFileName = filename;
        }

        public void StartProblemPredictionProgram()
        {
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            ProblemPredictionProgram = Process.Start(ProcessInfo);
        }

        public StreamReader BlockAndGetStream()
        {
            ProblemPredictionProgram.WaitForExit();
            StreamReader fileReader = new StreamReader(OutFileName);
            return fileReader;
        }

        public string BlockAndGetResults()
        {
            ProblemPredictionProgram.WaitForExit();
            StreamReader fileReader = new StreamReader(OutFileName);
            string ret = fileReader.ReadToEnd();
            fileReader.Close();
            return ret;
        }

    }
}
