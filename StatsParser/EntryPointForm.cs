using System;
using System.Drawing;
using System.Windows.Forms;

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

            MonitorParser mp1 = new MonitorParser(gameID);
            var table = mp1.ParseStats();

        }

        private void EntryPointForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, 369, 195);
        }
    }
}
