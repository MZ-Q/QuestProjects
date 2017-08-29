using System;
using System.Windows.Forms;
using System.Collections.Generic;

namespace StatsParser
{
    public partial class EntryPointForm : Form
    {
        public EntryPointForm()
        {
            InitializeComponent();
        }

        private void ParseBtn_Click(object sender, EventArgs e)
        {
            string gameID = this.GameIDTextBox.Text;

            if (gameID != string.Empty)
            {// get types of lvls
                Dictionary<int, string> typesOfLvls = ExcelOperational.GetGameLvlsSpec("TypesOfLvls.xlsx");
                StatsParser parser = new StatsParser(gameID, typesOfLvls);
                Dictionary<string, List<(string TeamName, TimeSpan TimeResult)>> finalTable = parser.GetFinalTable();

                // save stats
                ExcelOperational.WriteToExcel(finalTable, gameID.Split('=')[1]);
            }
            else
            {
                MessageBox.Show("Game URL can't be empty!", "Warning!", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
