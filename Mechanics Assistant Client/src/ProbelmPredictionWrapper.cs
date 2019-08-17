using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;

namespace Mechanic_Assistant_Client.src
{
    /*
     * Creates and retrieves data from ProblemPredictionProgram
     */
    class ProblemPredictionWrapper
    {

        private ProcessStartInfo ProcessInfo;
        private Process ProblemPredictionProgram;
        private string OutFileName;

        public ProblemPredictionWrapper(string fileName)
        {
            ProcessInfo = new ProcessStartInfo(fileName);
        }

        /*
         * sets the arguments needed to run ProblemPredictionProgram
         */
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

        /*
         * starts the ProblemPredictionProgram windowsless
         */
        public void StartProblemPredictionProgram()
        {
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            ProblemPredictionProgram = Process.Start(ProcessInfo);
        }

        /*
         * waits for ProblemPredictionProgram to end then creates the file reader and returns it
         */
        public StreamReader BlockAndGetStream()
        {
            ProblemPredictionProgram.WaitForExit();
            StreamReader fileReader = new StreamReader(OutFileName);
            return fileReader;
        }

        /*
         * waits for ProblemPredictionProgram to end then reads the data from the file and returns it
         */
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
