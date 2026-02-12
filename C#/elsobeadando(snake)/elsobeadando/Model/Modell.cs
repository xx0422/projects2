using elsobeadando.Model.Events;
using elsobeadando.Persistence;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using static elsobeadando.Model.Directions;
using static System.Formats.Asn1.AsnWriter;

namespace elsobeadando
{
    public class Modell : IDisposable
    {
        private Snake snake = null!;
        private Food food = null!;
        private List<Point> obstacles = new();
        private int width, height;
        private bool gameOver = false;
        private System.Timers.Timer? gameTimer;
        private int tickInterval = 150;
        public int score { get; private set; }
        public IReadOnlyList<Point> Obstacles => obstacles;

        public Modell(int width, int height)
        {
            this.width = width;
            this.height = height;
            Reset();
        }

        public void LoadLevel(string filePath)
        {
            var dataAccess = new LevelDataAccess();
            var (newWidth, newHeight, newObstacles) = dataAccess.LoadLevel(filePath);

            width = newWidth;
            height = newHeight;
            obstacles = newObstacles;

            Reset();
        }

        public void Reset()
        {
            snake = new Snake(new Point(width / 2, height / 2));
            food = new Food(width, height);
            score = 0;
            gameOver = false;
        }

        public void Update()
        {
            if (gameOver)
            {
                StopGameLoop(); 
                return;
            }

            snake.Move();
            SnakeMoved?.Invoke(this, new SnakeEventArgs(snake.Head, score));

            if (snake.Head == food.Position)
            {
                snake.Grow();
                IncreaseScore(1);
                food.Respawn(width, height, snake, obstacles);
                SnakeMoved?.Invoke(this, new SnakeEventArgs(snake.Head, score, true));
            }

            if (snake.IsColliding(width, height) || obstacles.Contains(snake.Head))
            {
                gameOver = true;
                StopGameLoop();
                GameEnded?.Invoke(this, new SnakeEventArgs(snake.Head, score, false, true));
            }
        }

        private void IncreaseScore(int value)
        {
            score += value;
            ScoreChanged?.Invoke(score);
        }
        public void StartGameLoop()
        {
            if (gameTimer != null)
                gameTimer.Stop();

            gameTimer = new System.Timers.Timer(tickInterval);
            gameTimer.Elapsed += (s, e) => Update();
            gameTimer.AutoReset = true;
            gameTimer.Start();
        }

        public void StopGameLoop()
        {
            gameTimer?.Stop();
        }

        public void Pause()
        {
            gameTimer?.Stop();
        }

        public void Resume()
        {
            gameTimer?.Start();
        }

        public void Dispose()
        {
            gameTimer?.Dispose();
        }

        public void ChangeDirection(Direction dir) => snake.ChangeDirection(dir);
        public bool IsGameOver => gameOver;
        public int GetScore => score;
        public int GetWidth => width;
        public int GetHeight => height;
        public Point GetFoodPosition => food.Position;
        public IEnumerable<Point> GetSnakeBody => snake.Body;


        public event Action<int>? ScoreChanged;
        public event EventHandler<SnakeEventArgs>? SnakeMoved;
        public event EventHandler<SnakeEventArgs>? GameEnded;


    }
}
