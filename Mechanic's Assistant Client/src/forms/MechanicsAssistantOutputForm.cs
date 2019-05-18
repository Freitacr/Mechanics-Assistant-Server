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
    public partial class MechanicsAssistantOutputForm : Form
    {

        public bool ShouldQueryAgain { get; set; } = false;

        internal class TableRowData
        {
            //public string Make { get; set; } = "";
            //public string Model { get; set; } = "";
            public string ListedProblem { get; set; } = "";
            //public string PercentageSimilarity { get; set; } = "";
        }

        public MechanicsAssistantOutputForm()
        {
            InitializeComponent();
        }

        private List<TableRowData> ContainedData = new List<TableRowData>();

        public void AddData(List<ProblemStorage> dataIn)
        {
            foreach (ProblemStorage x in dataIn)
            {
                TableRowData toAdd = new TableRowData
                {
                    ListedProblem = x.Problem
                };
                ContainedData.Add(toAdd);
            }
        }

        private void OnOutputTableLoad(object sender, EventArgs e)
        {
            BindingSource src = new BindingSource();
            src.DataSource = ContainedData;
            MainOutputTable.DataSource = src;
            MainOutputTable.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            MainOutputTable.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCellsExceptHeader;
            for (int i = 0; i < MainOutputTable.ColumnCount; i++)
            {
                MainOutputTable.Columns[i].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            }
            MainOutputTable.Columns[MainOutputTable.ColumnCount - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            //MainOutputTable.Columns[2].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCellsExceptHeader;
        }

        private void CreateNewQueryButton_Click(object sender, EventArgs e)
        {
            ShouldQueryAgain = true;
            Close();
        }

        private void AddProblemButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                "Feature is currently not supported",
                "Apologies...",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
                );
        }
    }
}
