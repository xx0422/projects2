using NUnit.Framework;
using masodikbeadando.Model;

namespace masodikbeadando.Model.Tests
{
    public class ModellTests
    {
        [Test]
        public void ModellInitializesBasicGameState()
        {
            Modell m = new Modell(10, 10);

            Assert.That(m.GetWidth, Is.EqualTo(10));
            Assert.That(m.GetHeight, Is.EqualTo(10));
            Assert.IsNotNull(m.Snake);
            Assert.IsNotNull(m.Food);
        }

        [Test]
        public void ModellFoodIsInsideBoard()
        {
            Modell m = new Modell(10, 10);
            var pos = m.Food.Position;

            Assert.That(pos.X, Is.InRange(0, 9));
            Assert.That(pos.Y, Is.InRange(0, 9));
        }

        [Test]
        public void GameEndsOnWallCollision()
        {
            Modell model = new Modell(5, 5);

            model.Snake.Body.Clear();
            model.Snake.Body.Add(new Position(0, 0));
            model.Snake.Body.Add(new Position(1, 0));

            model.Snake.TurnLeft();


            bool ended = false;
            model.GameEnded += (_, _) => ended = true;

            model.Update(); // crash into wall

            Assert.IsTrue(ended);
        }

        [Test]
        public void GameEndsOnSelfCollision()
        {
            Modell model = new Modell(10, 10);

            model.Snake.Body.Clear();
            model.Snake.Body.Add(new Position(5, 5));
            model.Snake.Body.Add(new Position(5, 6));
            model.Snake.Body.Add(new Position(5, 7));
            model.Snake.Body.Add(new Position(6, 7));
            model.Snake.Body.Add(new Position(6, 6)); // near loop

            model.Snake.TurnRight();


            bool ended = false;
            model.GameEnded += (_, _) => ended = true;

            model.Update();

            Assert.IsTrue(ended);
        }
    }
}
