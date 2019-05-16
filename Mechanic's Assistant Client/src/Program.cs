using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using Mechanics_Assistant_Client.Forms;

namespace Mechanics_Assistant_Client
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MechanicsAssistant());
            Application.Run(new MechanicsAssistantOutputForm());
        }
    }
}
