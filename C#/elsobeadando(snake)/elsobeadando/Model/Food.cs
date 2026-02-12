using System;
using System.Collections.Generic;
using System.Drawing;

namespace elsobeadando
{
    public class Food
    {
        public Point Position { get; private set; }
        private Random random = new Random();

        public Food(int width, int height)
        {
            Respawn(width, height, new Snake(new Point(0,0)), new List<Point>());
        }

        public void Respawn(int width, int height, Snake snake, List<Point> obstacles)
        {
            Point newPos;
            int tries = 0;
            int maxTries = width * height * 3;

            do
            {
                newPos = new Point(random.Next(0, width), random.Next(0, height));
                tries++;

                if (tries > maxTries)
                {
                    newPos = new Point(0, 0);
                    break;
                }
            }
            while (snake.Body.Contains(newPos) || obstacles.Contains(newPos));

            Position = newPos;
        }

    }
}
