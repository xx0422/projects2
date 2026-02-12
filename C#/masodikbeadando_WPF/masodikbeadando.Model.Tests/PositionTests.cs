using NUnit.Framework;
using masodikbeadando.Model;

namespace masodikbeadando.Model.Tests
{
    public class PositionTests
    {
        [Test]
        public void PositionsWithSameCoordinatesAreEqual()
        {
            var a = new Position(2, 3);
            var b = new Position(2, 3);

            Assert.IsTrue(a == b);
            Assert.IsFalse(a != b);
        }

        [Test]
        public void PositionsWithDifferentCoordinatesAreNotEqual()
        {
            var a = new Position(1, 5);
            var b = new Position(2, 5);

            Assert.IsTrue(a != b);
            Assert.IsFalse(a == b);
        }
    }
}
