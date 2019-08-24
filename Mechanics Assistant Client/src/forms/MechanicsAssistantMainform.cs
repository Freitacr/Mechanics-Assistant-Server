using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace MechanicsAssistantClient.Forms
{
    /*
     * the query creation window
     */
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
        /*
         * Initializer
         */
        public MechanicsAssistant()
        {
            InitializeComponent();
        }

        /*
         * clears the sample text from the make text box when box is clicked on.
         */
        public void MakeTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (MakeBoxHasShadowText)
            {
                MakeEntry.ResetText();
                MakeBoxHasShadowText = false;
            }
        }
        /*
         * clears the sample text from the mode text box when box is clicked on.
         */
        public void ModelTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (ModelBoxHasShadowText)
            {
                ModelTextBox.ResetText();
                ModelBoxHasShadowText = false;
            }
        }
        /*
         * clears the sample text from the complaint text box when box is clicked on.
         */
        public void ComplaintTextBoxClick(object sender, MouseEventArgs eventArgs)
        {
            if (ComplaintBoxHasShadowText)
            {
                ComplaintTextBox.ResetText();
                ComplaintBoxHasShadowText = false;
            }
        }

        /*
         * checks if the text boxes are empty or contain the sample text
         */
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

        /*
         * listener for if the query button is clicked.
         */
        public void QueryButtonClick(object sender, EventArgs eventArgs)
        {
            QueryButton.Focus();
            ShouldQuery = AreTextBoxesFilled();
            this.Close();
        }

        /*
         * listener for when the make text box is no longer selected
         */
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

        /*
         * listener for when the model text box is no longer selected
         */
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

        /*
         * listener for when the complaint text box is no longer selected
         */
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

        /*
         * returns the text from the make text box
         */
        public string GetMakeTextBoxValue()
        {
            return MakeText;
        }

        /*
         * returns the text from the model text box
         */
        public string GetModelTextBoxValue()
        {
            return ModelText;
        }

        /*
         * returns the text from the complaint text box
         */
        public string GetComplaintTextBoxValue()
        {
            return ComplaintText;
        }
    }
}
