using Avalonia.Controls;
using SnakeAvalonia.ViewModels;

namespace SnakeAvalonia.Views
{
    public partial class MainView : UserControl
    {
        public MainView()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }

}
