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
    /*
     * Output window, used to display the list of problems the AI returns from the Query
     */
    public partial class MechanicsAssistantOutputForm : Form
    {

        public bool ShouldQueryAgain { get; set; } = false;

        /*
         * Table format
         */
        internal class TableRowData
        {
            //public string Make { get; set; } = "";
            //public string Model { get; set; } = "";
            public string ListedProblem { get; set; } = "";
            //public string PercentageSimilarity { get; set; } = "";
        }

        /*
         * Initializer
         */
        public MechanicsAssistantOutputForm()
        {
            InitializeComponent();
        }

        private List<TableRowData> ContainedData = new List<TableRowData>();

        /*
         * adds data to containedData to be displayed later
         */
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

        /*
         * listener for when the table window is opened. fills the table with data from containedData
         */
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

        /*
         * listener for when the new query button is clicked. returns to the create query window
         */
        private void CreateNewQueryButton_Click(object sender, EventArgs e)
        {
            ShouldQueryAgain = true;
            Close();
        }

        /*
         * listener for when the add problem button is clicked. 
         *      **currently not implemented**
         */
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
