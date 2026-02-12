using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Snake.Core.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        private bool _isSnake;
        private bool _isFood;
        private bool _isObstacle;

        public int X { get; }
        public int Y { get; }

        public bool IsSnake
        {
            get => _isSnake;
            set { _isSnake = value; OnPropertyChanged(); }
        }

        public bool IsFood
        {
            get => _isFood;
            set { _isFood = value; OnPropertyChanged(); }
        }

        public bool IsObstacle
        {
            get => _isObstacle;
            set { _isObstacle = value; OnPropertyChanged(); }
        }

        public CellViewModel(int x, int y)
        {
            X = x;
            Y = y;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
