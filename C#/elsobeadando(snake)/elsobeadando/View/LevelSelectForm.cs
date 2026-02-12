using System;
using System.Windows.Forms;

namespace elsobeadando
{
    public partial class LevelSelectForm : Form
    {
        public string? SelectedLevelPath { get; private set; }

        public LevelSelectForm()
        {
            InitializeComponent();
        }

        private void buttonLvl1_Click(object sender, EventArgs e)
        {
            SelectedLevelPath = "Levels/lvl1.txt";
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonLvl2_Click(object sender, EventArgs e)
        {
            SelectedLevelPath = "Levels/lvl2.txt";
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonLvl3_Click(object sender, EventArgs e)
        {
            SelectedLevelPath = "Levels/lvl3.txt";
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
