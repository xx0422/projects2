using NUnit.Framework;
using masodikbeadando.Model;

namespace masodikbeadando.Model.Tests
{
    public class SnakeTests
    {
        [Test]
        public void SnakeMovesRightByDefault()
        {
            Snake snake = new Snake(new Position(5,5));
            snake.Body.Clear();
            snake.Body.Add(new Position(5, 5));
            snake.Body.Add(new Position(4, 5));

            snake.Move();

            Assert.That(snake.Head, Is.EqualTo(new Position(6, 5)));
        }

        [Test]
        public void SnakeTurnsLeftCorrectly()
        {
            // Starting direction: Right
            Snake snake = new Snake(new Position(5, 5));
            snake.Body.Clear();
            snake.Body.Add(new Position(5, 5));
            snake.Body.Add(new Position(4, 5));

            // Turning left when going right = UP
            snake.TurnLeft();
            snake.Move();

            Assert.That(snake.Head, Is.EqualTo(new Position(5, 4)));
        }

        [Test]
        public void SnakeTurnsRightCorrectly()
        {
            // Starting direction: Right
            Snake snake = new Snake(new Position(5, 5));
            snake.Body.Clear();
            snake.Body.Add(new Position(5, 5));
            snake.Body.Add(new Position(4, 5));

            // Turning right when going right = DOWN
            snake.TurnRight();
            snake.Move();

            Assert.That(snake.Head, Is.EqualTo(new Position(5, 6)));
        }

        [Test]
        public void SnakeGrowsCorrectly()
        {
            Snake snake = new Snake(new Position(5, 5));
            snake.Body.Clear();
            snake.Body.Add(new Position(5, 5));
            snake.Body.Add(new Position(4, 5));

            int before = snake.Body.Count;

            snake.Grow();
            snake.Move();

            Assert.That(snake.Body.Count, Is.EqualTo(before + 1));
        }
    }
}
