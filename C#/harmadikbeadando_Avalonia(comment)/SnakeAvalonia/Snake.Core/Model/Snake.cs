using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using static Snake.Core.Model.Directions;

namespace Snake.Core.Model
{
    public class Snake
    {
        public List<Position> Body { get; private set; } = new();
        private Direction direction = Direction.Right;
        public Position Head => Body.First();
        private bool hasGrown = false;

        public Snake(Position start)
        {
            int initialLength = 5;

            for (int i = 0; i < initialLength; i++)
            {
                Body.Add(new Position(start.X - i, start.Y));
            }
        }

        public void Move()
        {
            Position newHead = Head;

            switch (direction)
            {
                case Direction.Up:
                    newHead = new Position(newHead.X, newHead.Y - 1);
                    break;

                case Direction.Down:
                    newHead = new Position(newHead.X, newHead.Y + 1);
                    break;

                case Direction.Left:
                    newHead = new Position(newHead.X - 1, newHead.Y);
                    break;

                case Direction.Right:
                    newHead = new Position(newHead.X + 1, newHead.Y);
                    break;
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

        public void TurnLeft()
        {
            switch (direction)
            {
                case Direction.Up: direction = Direction.Left; break;
                case Direction.Left: direction = Direction.Down; break;
                case Direction.Down: direction = Direction.Right; break;
                case Direction.Right: direction = Direction.Up; break;
            }
        }

        public void TurnRight()
        {
            switch (direction)
            {
                case Direction.Up: direction = Direction.Right; break;
                case Direction.Right: direction = Direction.Down; break;
                case Direction.Down: direction = Direction.Left; break;
                case Direction.Left: direction = Direction.Up; break;
            }
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
