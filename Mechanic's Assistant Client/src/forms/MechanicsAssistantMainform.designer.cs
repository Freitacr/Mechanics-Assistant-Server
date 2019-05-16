namespace Mechanics_Assistant_Client.Forms
{
    partial class MechanicsAssistant
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.MakeEntry = new System.Windows.Forms.TextBox();
            this.ModelTextBox = new System.Windows.Forms.TextBox();
            this.ComplaintTextBox = new System.Windows.Forms.TextBox();
            this.QueryButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // MakeEntry
            // 
            this.MakeEntry.Location = new System.Drawing.Point(12, 12);
            this.MakeEntry.Name = "MakeEntry";
            this.MakeEntry.Size = new System.Drawing.Size(200, 20);
            this.MakeEntry.TabIndex = 0;
            this.MakeEntry.Text = "Please Enter Vehicle\'s Make...";
            this.MakeEntry.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MakeTextBoxClick);
            this.MakeEntry.Leave += new System.EventHandler(this.MakeTextBoxLeaveFocus);
            // 
            // ModelTextBox
            // 
            this.ModelTextBox.Location = new System.Drawing.Point(12, 38);
            this.ModelTextBox.Name = "ModelTextBox";
            this.ModelTextBox.Size = new System.Drawing.Size(200, 20);
            this.ModelTextBox.TabIndex = 1;
            this.ModelTextBox.Text = "Please Enter Vehicle\'s Model...";
            this.ModelTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ModelTextBoxClick);
            this.ModelTextBox.Leave += new System.EventHandler(this.ModelTextBoxLeaveFocus);
            // 
            // ComplaintTextBox
            // 
            this.ComplaintTextBox.BackColor = System.Drawing.SystemColors.Window;
            this.ComplaintTextBox.Location = new System.Drawing.Point(12, 64);
            this.ComplaintTextBox.Multiline = true;
            this.ComplaintTextBox.Name = "ComplaintTextBox";
            this.ComplaintTextBox.Size = new System.Drawing.Size(325, 108);
            this.ComplaintTextBox.TabIndex = 2;
            this.ComplaintTextBox.Text = "Please Enter Customer\'s Complaint...";
            this.ComplaintTextBox.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ComplaintTextBoxClick);
            this.ComplaintTextBox.Leave += new System.EventHandler(this.ComplaintTextBoxLeaveFocus);
            // 
            // QueryButton
            // 
            this.QueryButton.Location = new System.Drawing.Point(137, 178);
            this.QueryButton.Name = "QueryButton";
            this.QueryButton.Size = new System.Drawing.Size(75, 23);
            this.QueryButton.TabIndex = 3;
            this.QueryButton.Text = "Query";
            this.QueryButton.UseVisualStyleBackColor = true;
            this.QueryButton.Click += new System.EventHandler(this.QueryButtonClick);
            // 
            // MechanicsAssistant
            // 
            this.AcceptButton = this.QueryButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(355, 207);
            this.Controls.Add(this.QueryButton);
            this.Controls.Add(this.ComplaintTextBox);
            this.Controls.Add(this.ModelTextBox);
            this.Controls.Add(this.MakeEntry);
            this.Name = "MechanicsAssistant";
            this.Text = "Mechanic\'s Assistant";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MakeEntry;
        private System.Windows.Forms.TextBox ModelTextBox;
        private System.Windows.Forms.TextBox ComplaintTextBox;
        private System.Windows.Forms.Button QueryButton;
    }
}

