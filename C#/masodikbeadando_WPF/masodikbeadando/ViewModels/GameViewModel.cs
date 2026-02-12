using masodikbeadando; 
using masodikbeadando.Model;
using masodikbeadando.Model.Events;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static masodikbeadando.Model.Directions;

namespace masodikbeadando.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Modell _model;
        private bool _disposed;
        private bool _isPaused;
        private int _score;
        public bool IsGameOver { get; private set; }
        public int GameOverScore { get; private set; }
        private readonly System.Windows.Threading.DispatcherTimer _uiTimer = null!;

        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime
        {
            get => _elapsedTime;
            private set
            {
                _elapsedTime = value;
                OnPropertyChanged();
            }
        }

        public event EventHandler? GameOverRequested;

        public int Width => _model.GetWidth;
        public int Height => _model.GetHeight;

        public int Score
        {
            get => _score;
            private set { _score = value; OnPropertyChanged(); }
        }

        public bool IsPaused
        {
            get => _isPaused;
            private set { _isPaused = value; OnPropertyChanged(); }
        }

        public ObservableCollection<CellViewModel> Cells { get; } = new();

        public ICommand TurnLeftCommand { get; }
        public ICommand TurnRightCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand ResumeCommand { get; }
        public ICommand PauseToggleCommand { get; }

        public ICommand RestartCommand { get; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public GameViewModel(string levelPath)
        {
            
            _model = new Modell(10, 10);
            _model.LoadLevel(levelPath);

            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    Cells.Add(new CellViewModel(x, y));
                }
            }

            _elapsedTime = TimeSpan.Zero;

            _uiTimer = new System.Windows.Threading.DispatcherTimer();
            _uiTimer.Interval = TimeSpan.FromSeconds(1);
            _uiTimer.Tick += (s, e) =>
            {
                if (!IsPaused && !IsGameOver)
                    ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
            };
            _uiTimer.Start();

            _model.ScoreChanged += OnScoreChanged;
            _model.SnakeMoved += OnSnakeMoved;
            _model.GameEnded += OnGameEnded;

            UpdateBoard();

            TurnLeftCommand = new RelayCommand(() => _model.TurnLeft());
            TurnRightCommand = new RelayCommand(() => _model.TurnRight());
            PauseCommand = new RelayCommand(Pause, () => !IsPaused);
            ResumeCommand = new RelayCommand(Resume, () => IsPaused);
            PauseToggleCommand = new RelayCommand(() =>
            {
                if (IsPaused)
                    Resume();
                else
                    Pause();
            });

            RestartCommand = new RelayCommand(Restart);



            _model.StartGameLoop();
        }

        private void OnScoreChanged(int score)
        {
            Score = score;
        }


        private void OnSnakeMoved(object? sender, SnakeEventArgs e)
        {
            UpdateBoard();
        }

        private void OnGameEnded(object? sender, SnakeEventArgs e)
        {
            IsGameOver = true;
            GameOverScore = e.Score;

            _model.StopGameLoop();

            GameOverRequested?.Invoke(this, EventArgs.Empty);

            _uiTimer.Stop();
            IsGameOver = true;

        }

        private void UpdateBoard()
        {
            foreach (var cell in Cells)
            {
                cell.IsSnake = false;
                cell.IsFood = false;
                cell.IsObstacle = false;
            }

            foreach (Position p in _model.Obstacles)
            {
                var cell = GetCell(p.X, p.Y);
                if (cell != null) cell.IsObstacle = true;
            }

            foreach (Position p in _model.GetSnakeBody)
            {
                var cell = GetCell(p.X, p.Y);
                if (cell != null) cell.IsSnake = true;
            }

            var foodPos = _model.GetFoodPosition;
            var foodCell = GetCell(foodPos.X, foodPos.Y);
            if (foodCell != null) foodCell.IsFood = true;
        }

        private CellViewModel? GetCell(int x, int y) =>
            Cells.FirstOrDefault(c => c.X == x && c.Y == y);

        private void Pause()
        {
            _model.Pause();
            IsPaused = true;
            (PauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ResumeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void Resume()
        {
            _model.Resume();
            IsPaused = false;
            (PauseCommand as RelayCommand)?.RaiseCanExecuteChanged();
            (ResumeCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }

        private void Restart()
        {
           
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public void Dispose()
        {
            if (!_disposed)
            {
                _model.ScoreChanged -= OnScoreChanged;
                _model.SnakeMoved -= OnSnakeMoved;
                _model.GameEnded -= OnGameEnded;
                _model.Dispose();
                _disposed = true;
            }
        }
    }
}
