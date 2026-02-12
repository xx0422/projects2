using System;
using System.Collections.Generic;
using System.Drawing;
using masodikbeadando.Model;

namespace masodikbeadando.Model
{
    public class Food
    {
        public Position Position { get; private set; }
        private Random random = new Random();

        public Food(int width, int height, Snake snake, List<Position> obstacles)
        {
            Respawn(width, height, snake, obstacles);
        }



        public void Respawn(int width, int height, Snake snake, List<Position> obstacles)
        {
            Position newPos;
            int tries = 0;
            int maxTries = width * height * 3;

            do
            {
                newPos = new Position(random.Next(0, width), random.Next(0, height));
                tries++;

                if (tries > maxTries)
                {
                    newPos = new Position(0, 0);
                    break;
                }
            }
            while (snake.Body.Contains(newPos) || obstacles.Contains(newPos));

            Position = newPos;
        }

    }
}
