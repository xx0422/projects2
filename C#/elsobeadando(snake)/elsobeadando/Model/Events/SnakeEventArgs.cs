using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elsobeadando.Model.Events
{
    public class SnakeEventArgs : EventArgs
    {
        public Point HeadPosition { get; }
        public int Score { get; }
        public bool AteFood { get; }
        public bool GameOver { get; }

        public SnakeEventArgs(Point headPosition, int score, bool ateFood = false, bool gameOver = false)
        {
            HeadPosition = headPosition;
            Score = score;
            AteFood = ateFood;
            GameOver = gameOver;
        }
    }
}