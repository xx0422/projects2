using masodikbeadando.ViewModels;
using System;
using System.Windows;
using System.Windows.Input;

namespace masodikbeadando
{
    public partial class GameWindow : Window,IDisposable
    {
        private readonly GameViewModel _viewModel;
        private bool _disposed;


        public GameWindow(string levelPath)
        {
            InitializeComponent();
            _viewModel = new GameViewModel(levelPath);
            DataContext = _viewModel;
            _viewModel.GameOverRequested += OnGameOver;
            this.Loaded += (s, e) => this.Focus();
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel.Dispose();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _viewModel.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left)
            {
                _viewModel.TurnLeftCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Right)
            {
                _viewModel.TurnRightCommand.Execute(null);
                e.Handled = true;
            }
            else if (e.Key == Key.Space)
            {
                _viewModel.PauseCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnGameOver(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"GAME OVER!\nPontszámod: {_viewModel.GameOverScore}",
                    "Vége a játéknak",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                );

                var selectWindow = new LevelSelectWindow();
                selectWindow.Show();

                this.Close();
            });
        }




    }
}
