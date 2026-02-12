using Avalonia.Controls;
using Snake.Core.ViewModels;
using SnakeAvalonia.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace SnakeAvalonia.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private UserControl _currentView = null!;

        public UserControl CurrentView
        {
            get => _currentView;
            set
            {
                if (_currentView != value)
                {
                    _currentView = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            GoToMenu();
        }

        public void GoToMenu()
        {
            CurrentView = new LevelSelectView
            {
                DataContext = new LevelSelectViewModel(this)
            };
        }

        public void StartGame(string levelPath)
        {
            System.Diagnostics.Debug.WriteLine($"[MainViewModel] Játék indítása: {levelPath}");

            // JAVÍTÁS: Így kell átadni a parancsot (Action):
            // "() => GoToMenu()" jelentése: "Ha baj van, ezt a függvényt hívd meg!"
            var gameVM = new GameViewModel(levelPath, () => GoToMenu());

            CurrentView = new GameView
            {
                DataContext = gameVM
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string? p = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(p));
    }
}