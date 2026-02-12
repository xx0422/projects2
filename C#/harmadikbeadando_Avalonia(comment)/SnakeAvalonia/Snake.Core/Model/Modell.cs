using Snake.Core.Model.Events;
using Snake.Core.Persistence;
using System;
using System.Collections.Generic;
using System.IO;
using Snake.Core.Model;

namespace Snake.Core.Model
{
    public class Modell : IDisposable
    {
        private Snake snake = null!;
        private Food food = null!;
        private List<Position> obstacles = new();
        private int width, height;
        private bool gameOver = false;
        private System.Timers.Timer? gameTimer;
        private int tickInterval = 200; // lassítottam a mozgást, hogy jobban látszódjon
        public int score { get; private set; }
        public IReadOnlyList<Position> Obstacles => obstacles;
        public Snake Snake => snake;
        public Food Food => food;

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

        private void Reset()
        {
            gameOver = false;
            score = 0;
            snake = new Snake(new Position(width / 2, height / 2));
            food = new Food(width, height, snake, obstacles);
            Console.WriteLine($"Model reset. Start position: {width / 2},{height / 2}");
        }

        public void Update()
        {
            Console.WriteLine($"UPDATE TICK - Head: {snake.Head.X},{snake.Head.Y}, Score: {score}");

            if (gameOver)
            {
                Console.WriteLine("Game over detected, stopping loop");
                StopGameLoop();
                return;
            }

            snake.Move();
            Console.WriteLine($"Snake moved to: {snake.Head.X},{snake.Head.Y}");
            SnakeMoved?.Invoke(this, new SnakeEventArgs(snake.Head, score));

            if (snake.Head == food.Position)
            {
                Console.WriteLine("Food eaten!");
                snake.Grow();
                IncreaseScore(1);
                food.Respawn(width, height, snake, obstacles);
                SnakeMoved?.Invoke(this, new SnakeEventArgs(snake.Head, score, true));
            }

            if (snake.IsColliding(width, height))
            {
                Console.WriteLine($"Collision with wall! Head at {snake.Head.X},{snake.Head.Y}, bounds: {width}x{height}");
                gameOver = true;
                StopGameLoop();
                GameEnded?.Invoke(this, new SnakeEventArgs(snake.Head, score, false, true));
            }
            else if (obstacles.Contains(snake.Head))
            {
                Console.WriteLine($"Collision with obstacle at {snake.Head.X},{snake.Head.Y}");
                gameOver = true;
                StopGameLoop();
                GameEnded?.Invoke(this, new SnakeEventArgs(snake.Head, score, false, true));
            }
        }

        private void IncreaseScore(int value)
        {
            score += value;
            Console.WriteLine($"Score increased to: {score}");
            ScoreChanged?.Invoke(score);
        }

        public void StartGameLoop()
        {
            Console.WriteLine($"Starting game loop with interval: {tickInterval}ms");

            if (gameTimer != null)
            {
                gameTimer.Stop();
                gameTimer.Dispose();
            }

            gameTimer = new System.Timers.Timer(tickInterval);
            gameTimer.Elapsed += (s, e) => Update();
            gameTimer.AutoReset = true;
            gameTimer.Start();
            Console.WriteLine("Game loop started");
        }

        public void StopGameLoop()
        {
            Console.WriteLine("Stopping game loop");
            gameTimer?.Stop();
        }

        public void Pause()
        {
            Console.WriteLine("Pausing game");
            gameTimer?.Stop();
        }

        public void Resume()
        {
            Console.WriteLine("Resuming game");
            gameTimer?.Start();
        }

        public void Dispose()
        {
            Console.WriteLine("Disposing model");
            gameTimer?.Dispose();
        }

        public void TurnLeft()
        {
            Console.WriteLine("Turn LEFT");
            snake.TurnLeft();
        }

        public void TurnRight()
        {
            Console.WriteLine("Turn RIGHT");
            snake.TurnRight();
        }

        public bool IsGameOver => gameOver;
        public int GetScore => score;
        public int GetWidth => width;
        public int GetHeight => height;
        public Position GetFoodPosition => food.Position;
        public IEnumerable<Position> GetSnakeBody => snake.Body;

        public event Action<int>? ScoreChanged;
        public event EventHandler<SnakeEventArgs>? SnakeMoved;
        public event EventHandler<SnakeEventArgs>? GameEnded;
    }
}