using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mechanics_Assistant_Client.Forms
{
    public partial class MechanicsAssistant : Form
    {

        private bool MakeBoxHasShadowText = true;
        private bool ModelBoxHasShadowText = true;
        private bool ComplaintBoxHasShadowText = true;

        private string MakeBoxShadowText = "Please Enter Vehicle's Make...";
        private string ModelBoxShadowText = "Please Enter Vehicle's Model...";
        private string ComplaintBoxShadowText = "Please Enter Customer's Complaint...";


        public MechanicsAssistant()
        {
            InitializeComponent();
        }


        public void MakeTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (MakeBoxHasShadowText)
            {
                MakeEntry.ResetText();
                MakeBoxHasShadowText = false;
            }
        }

        public void ModelTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (ModelBoxHasShadowText)
            {
                ModelTextBox.ResetText();
                ModelBoxHasShadowText = false;
            }
        }

        public void ComplaintTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (ComplaintBoxHasShadowText)
            {
                ComplaintTextBox.ResetText();
                ComplaintBoxHasShadowText = false;
            }
        }

        public void QueryButtonClick(object sender, EventArgs eventArgs)
        {
            this.Close();
        }

        private void MakeTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (MakeEntry.Text == "")
            {
                MakeEntry.Text = MakeBoxShadowText;
                MakeBoxHasShadowText = true;
            }
        }

        private void ModelTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (ModelTextBox.Text == "")
            {
                ModelTextBox.Text = ModelBoxShadowText;
                ModelBoxHasShadowText = true;
            }
        }

        private void ComplaintTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (ComplaintTextBox.Text == "")
            {
                ComplaintTextBox.Text = ComplaintBoxShadowText;
                ComplaintBoxHasShadowText = true;
            }
        }
    }
}
