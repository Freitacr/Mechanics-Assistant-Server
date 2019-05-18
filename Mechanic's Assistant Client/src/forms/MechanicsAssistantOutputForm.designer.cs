namespace Mechanics_Assistant_Client.Forms
{
    partial class MechanicsAssistantOutputForm
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
            this.MainOutputTable = new System.Windows.Forms.DataGridView();
            this.CreateNewQueryButton = new System.Windows.Forms.Button();
            this.AddProblemButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.MainOutputTable)).BeginInit();
            this.SuspendLayout();
            // 
            // MainOutputTable
            // 
            this.MainOutputTable.AllowUserToAddRows = false;
            this.MainOutputTable.AllowUserToDeleteRows = false;
            this.MainOutputTable.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.MainOutputTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MainOutputTable.Location = new System.Drawing.Point(12, 12);
            this.MainOutputTable.Name = "MainOutputTable";
            this.MainOutputTable.ReadOnly = true;
            this.MainOutputTable.Size = new System.Drawing.Size(776, 372);
            this.MainOutputTable.TabIndex = 0;
            this.MainOutputTable.VisibleChanged += new System.EventHandler(this.OnOutputTableLoad);
            // 
            // CreateNewQueryButton
            // 
            this.CreateNewQueryButton.Location = new System.Drawing.Point(218, 390);
            this.CreateNewQueryButton.Name = "CreateNewQueryButton";
            this.CreateNewQueryButton.Size = new System.Drawing.Size(165, 48);
            this.CreateNewQueryButton.TabIndex = 1;
            this.CreateNewQueryButton.Text = "New Query";
            this.CreateNewQueryButton.UseVisualStyleBackColor = true;
            this.CreateNewQueryButton.Click += new System.EventHandler(this.CreateNewQueryButton_Click);
            // 
            // AddProblemButton
            // 
            this.AddProblemButton.Location = new System.Drawing.Point(419, 390);
            this.AddProblemButton.Name = "AddProblemButton";
            this.AddProblemButton.Size = new System.Drawing.Size(165, 48);
            this.AddProblemButton.TabIndex = 2;
            this.AddProblemButton.Text = "Add Problem";
            this.AddProblemButton.UseVisualStyleBackColor = true;
            this.AddProblemButton.Click += new System.EventHandler(this.AddProblemButton_Click);
            // 
            // MechanicsAssistantOutputForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.AddProblemButton);
            this.Controls.Add(this.CreateNewQueryButton);
            this.Controls.Add(this.MainOutputTable);
            this.Name = "MechanicsAssistantOutputForm";
            this.Text = "Mechanics Assistant";
            ((System.ComponentModel.ISupportInitialize)(this.MainOutputTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView MainOutputTable;
        private System.Windows.Forms.Button CreateNewQueryButton;
        private System.Windows.Forms.Button AddProblemButton;
    }
}