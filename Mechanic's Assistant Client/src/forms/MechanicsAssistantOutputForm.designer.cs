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
            this.MakeColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ModelColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ProblemColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.PercentageColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
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
            this.MainOutputTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.MakeColumn,
            this.ModelColumn,
            this.ProblemColumn,
            this.PercentageColumn});
            this.MainOutputTable.Location = new System.Drawing.Point(12, 12);
            this.MainOutputTable.Name = "MainOutputTable";
            this.MainOutputTable.ReadOnly = true;
            this.MainOutputTable.Size = new System.Drawing.Size(776, 372);
            this.MainOutputTable.TabIndex = 0;
            // 
            // MakeColumn
            // 
            this.MakeColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.MakeColumn.HeaderText = "Make";
            this.MakeColumn.Name = "MakeColumn";
            this.MakeColumn.ReadOnly = true;
            this.MakeColumn.Width = 59;
            // 
            // ModelColumn
            // 
            this.ModelColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
            this.ModelColumn.HeaderText = "Model";
            this.ModelColumn.Name = "ModelColumn";
            this.ModelColumn.ReadOnly = true;
            this.ModelColumn.Width = 61;
            // 
            // ProblemColumn
            // 
            this.ProblemColumn.HeaderText = "Listed Problem";
            this.ProblemColumn.Name = "ProblemColumn";
            this.ProblemColumn.ReadOnly = true;
            // 
            // PercentageColumn
            // 
            this.PercentageColumn.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
            this.PercentageColumn.HeaderText = "Percentage Match";
            this.PercentageColumn.Name = "PercentageColumn";
            this.PercentageColumn.ReadOnly = true;
            this.PercentageColumn.Width = 120;
            // 
            // CreateNewQueryButton
            // 
            this.CreateNewQueryButton.Location = new System.Drawing.Point(218, 390);
            this.CreateNewQueryButton.Name = "CreateNewQueryButton";
            this.CreateNewQueryButton.Size = new System.Drawing.Size(165, 48);
            this.CreateNewQueryButton.TabIndex = 1;
            this.CreateNewQueryButton.Text = "New Query";
            this.CreateNewQueryButton.UseVisualStyleBackColor = true;
            // 
            // AddProblemButton
            // 
            this.AddProblemButton.Location = new System.Drawing.Point(419, 390);
            this.AddProblemButton.Name = "AddProblemButton";
            this.AddProblemButton.Size = new System.Drawing.Size(165, 48);
            this.AddProblemButton.TabIndex = 2;
            this.AddProblemButton.Text = "Add Problem";
            this.AddProblemButton.UseVisualStyleBackColor = true;
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
        private System.Windows.Forms.DataGridViewTextBoxColumn MakeColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ModelColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ProblemColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn PercentageColumn;
        private System.Windows.Forms.Button CreateNewQueryButton;
        private System.Windows.Forms.Button AddProblemButton;
    }
}