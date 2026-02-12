using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace masodikbeadando
{
   
    public partial class LevelSelectWindow : Window
    {
        public LevelSelectWindow()
        {
            InitializeComponent();
        }

        private void OpenGame(string levelPath)
        {
            var gameWindow = new GameWindow(levelPath);
            gameWindow.Show();
            Close();
        }

        private void Stage1_Click(object sender, RoutedEventArgs e) =>
            OpenGame("Levels/lvl1.txt");

        private void Stage2_Click(object sender, RoutedEventArgs e) =>
            OpenGame("Levels/lvl2.txt");

        private void Stage3_Click(object sender, RoutedEventArgs e) =>
            OpenGame("Levels/lvl3.txt");
    }
}
