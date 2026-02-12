using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using masodikbeadando.Model;

namespace masodikbeadando.Model.Events
{
    public class SnakeEventArgs : EventArgs
    {
        public Position HeadPosition { get; }
        public int Score { get; }
        public bool AteFood { get; }
        public bool GameOver { get; }

        public SnakeEventArgs(Position headPosition, int score, bool ateFood = false, bool gameOver = false)
        {
            HeadPosition = headPosition;
            Score = score;
            AteFood = ateFood;
            GameOver = gameOver;
        }
    }
}