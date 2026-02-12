using Snake.Core.Model;
using Snake.Core.Model.Events;
using System; // Ez kell az "Action"-höz
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Threading;
using static Snake.Core.Model.Directions;

namespace Snake.Core.ViewModels
{
    public class GameViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly Modell _model;
        private readonly SynchronizationContext? _uiContext;

        // JAVÍTÁS: Nem MainViewModel-t tárolunk, hanem egy egyszerű utasítást (Action)
        private readonly Action _onExitGame;

        private bool _disposed;
        private bool _isPaused;
        private int _score;
        private bool _isGameOver;
        private int _gameOverScore;

        public bool IsGameOver { get => _isGameOver; private set { _isGameOver = value; OnPropertyChanged(); } }
        public int GameOverScore { get => _gameOverScore; private set { _gameOverScore = value; OnPropertyChanged(); } }

        private readonly System.Timers.Timer _uiTimer;
        private TimeSpan _elapsedTime;
        public TimeSpan ElapsedTime { get => _elapsedTime; private set { _elapsedTime = value; OnPropertyChanged(); } }

        public int Width => _model.GetWidth;
        public int Height => _model.GetHeight;
        public int Score { get => _score; private set { _score = value; OnPropertyChanged(); } }
        public bool IsPaused { get => _isPaused; private set { _isPaused = value; OnPropertyChanged(); } }
        public ObservableCollection<CellViewModel> Cells { get; } = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public ICommand TurnLeftCommand { get; }
        public ICommand TurnRightCommand { get; }
        public ICommand PauseToggleCommand { get; }
        public ICommand ExitGameCommand { get; }

        // KONSTRUKTOR: Itt kérjük be az "onExit" utasítást
        public GameViewModel(string levelPath, Action onExit)
        {
            _onExitGame = onExit; // Eltároljuk az utasítást
            _uiContext = SynchronizationContext.Current;

            _model = new Modell(10, 10);
            _model.LoadLevel(levelPath);

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Cells.Add(new CellViewModel(x, y));

            _elapsedTime = TimeSpan.Zero;
            _uiTimer = new System.Timers.Timer(1000);
            _uiTimer.Elapsed += (s, e) => {
                if (!IsPaused && !IsGameOver)
                {
                    if (_uiContext != null) _uiContext.Post(_ => ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1)), null);
                    else ElapsedTime = ElapsedTime.Add(TimeSpan.FromSeconds(1));
                }
            };
            _uiTimer.AutoReset = true;
            _uiTimer.Start();

            _model.ScoreChanged += (score) => { if (_uiContext != null) _uiContext.Post(_ => Score = score, null); else Score = score; };
            _model.SnakeMoved += (s, e) => { if (_uiContext != null) _uiContext.Post(_ => UpdateBoard(), null); else UpdateBoard(); };

            _model.GameEnded += (s, e) => {
                void EndAction()
                {
                    _model.StopGameLoop();
                    _uiTimer.Stop();
                    GameOverScore = e.Score;
                    IsGameOver = true;
                }
                if (_uiContext != null) _uiContext.Post(_ => EndAction(), null);
                else EndAction();
            };

            UpdateBoard();

            TurnLeftCommand = new RelayCommand(() => _model.TurnLeft());
            TurnRightCommand = new RelayCommand(() => _model.TurnRight());
            PauseToggleCommand = new RelayCommand(() => { if (IsPaused) _model.Resume(); else _model.Pause(); IsPaused = !IsPaused; });

            // ITT A JAVÍTÁS:
            // Amikor megnyomják a gombot, végrehajtjuk az utasítást
            ExitGameCommand = new RelayCommand(() =>
            {
                _onExitGame?.Invoke();
            });

            _model.StartGameLoop();
        }

        private void UpdateBoard()
        {
            foreach (var cell in Cells) { cell.IsSnake = false; cell.IsFood = false; cell.IsObstacle = false; }
            foreach (var p in _model.Obstacles) { var cell = Cells.FirstOrDefault(c => c.X == p.X && c.Y == p.Y); if (cell != null) cell.IsObstacle = true; }
            foreach (var p in _model.GetSnakeBody) { var cell = Cells.FirstOrDefault(c => c.X == p.X && c.Y == p.Y); if (cell != null) cell.IsSnake = true; }
            var foodPos = _model.GetFoodPosition;
            var foodCell = Cells.FirstOrDefault(c => c.X == foodPos.X && c.Y == foodPos.Y);
            if (foodCell != null) foodCell.IsFood = true;
        }

        public void Dispose()
        {
            if (!_disposed) { _uiTimer.Dispose(); _model.Dispose(); _disposed = true; }
        }

        protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}