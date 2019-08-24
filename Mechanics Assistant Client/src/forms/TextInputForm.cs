using System;
using System.Windows.Forms;

namespace MechanicsAssistantClient.Forms
{
    public partial class TextInputForm : Form
    {
        public DialogResult Result { get; private set; }
        public string InputBoxContents { get { return TextInputBox.Text; } }

        private TextInputForm()
        {
            InitializeComponent();
        }

        public static TextInputForm Show(string prompt, string formTitle)
        {
            TextInputForm ret = new TextInputForm();
            ret.Text = formTitle;
            ret.PromptLabel.Text = prompt;
            ret.Result = ret.ShowDialog();
            return ret;
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
