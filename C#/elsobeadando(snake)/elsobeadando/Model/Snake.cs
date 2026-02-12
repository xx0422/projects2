using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static elsobeadando.Model.Directions;

namespace elsobeadando
{
    public class Snake
    {
        public List<Point> Body { get; private set; } = new();
        private Direction direction = Direction.Right;
        public Point Head => Body.First();
        private bool hasGrown = false;

        public Snake(Point start)
        {
            int initialLength = 5;

            for (int i = 0; i < initialLength; i++)
            {
                Body.Add(new Point(start.X - i, start.Y));
            }
        }

        public void Move()
        {
            Point newHead = Head;

            switch (direction)
            {
                case Direction.Up: newHead.Y -= 1; break;
                case Direction.Down: newHead.Y += 1; break;
                case Direction.Left: newHead.X -= 1; break;
                case Direction.Right: newHead.X += 1; break;
            }

            Body.Insert(0, newHead);

            if (!hasGrown)
                Body.RemoveAt(Body.Count - 1);
            else
                hasGrown = false; 
        }

        public void Grow()
        {
            hasGrown = true;
        }

        public void ChangeDirection(Direction dir)
        {
            if ((direction == Direction.Up && dir == Direction.Down) ||
                (direction == Direction.Down && dir == Direction.Up) ||
                (direction == Direction.Left && dir == Direction.Right) ||
                (direction == Direction.Right && dir == Direction.Left))
                return;

            direction = dir;
        }

        public bool IsColliding(int width, int height)
        {
            if (Head.X < 0 || Head.Y < 0 || Head.X >= width || Head.Y >= height)
                return true;

            return Body.Skip(1).Contains(Head);
        }
    }
}
