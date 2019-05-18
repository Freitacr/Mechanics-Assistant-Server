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

        private string ModelText = "";
        private string MakeText = "";
        private string ComplaintText = "";

        public bool ShouldQuery { get; set; }

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

        private bool AreTextBoxesFilled()
        {
            if (ComplaintTextBox.Text == "" | ComplaintBoxHasShadowText)
                return false;
            if (ModelTextBox.Text == "" | ModelBoxHasShadowText)
                return false;
            if (MakeEntry.Text == "" | MakeBoxHasShadowText)
                return false;
            return true;
        }

        public void QueryButtonClick(object sender, EventArgs eventArgs)
        {
            QueryButton.Focus();
            ShouldQuery = AreTextBoxesFilled();
            this.Close();
        }

        private void MakeTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (MakeEntry.Text == "")
            {
                MakeEntry.Text = MakeBoxShadowText;
                MakeBoxHasShadowText = true;
            } else if (MakeEntry.Text != MakeBoxShadowText)
            {
                MakeText = MakeEntry.Text;
                MakeBoxHasShadowText = false;
            }
        }

        private void ModelTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (ModelTextBox.Text == "")
            {
                ModelTextBox.Text = ModelBoxShadowText;
                ModelBoxHasShadowText = true;
            } else if (ModelTextBox.Text != ModelBoxShadowText)
            {
                ModelText = ModelTextBox.Text;
                ModelBoxHasShadowText = false;
            }
        }

        private void ComplaintTextBoxLeaveFocus(object sender, EventArgs e)
        {
            if (ComplaintTextBox.Text == "")
            {
                ComplaintTextBox.Text = ComplaintBoxShadowText;
                ComplaintBoxHasShadowText = true;
            } else if (ComplaintTextBox.Text != ComplaintBoxShadowText)
            {
                ComplaintText = ComplaintTextBox.Text;
                ComplaintBoxHasShadowText = false;
            }
        }

        public string GetMakeTextBoxValue()
        {
            return MakeText;
        }

        public string GetModelTextBoxValue()
        {
            return ModelText;
        }

        public string GetComplaintTextBoxValue()
        {
            return ComplaintText;
        }
    }
}
