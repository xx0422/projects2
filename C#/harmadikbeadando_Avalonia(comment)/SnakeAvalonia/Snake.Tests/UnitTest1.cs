using Xunit; // Vagy NUnit.Framework, attól függ mit választottál
using Snake.Core.Model;
using System.Collections.Generic;
using System.Linq; // Ez kell a .First() használatához

namespace Snake.Tests
{
    public class SnakeGameTests
    {
        [Fact]
        public void SnakeMovesForward_CoordinateChanges()
        {
            // 1. ELÕKÉSZÍTÉS
            // JAVÍTÁS: A konstruktornak egy Position objektumot adunk át, nem két int-et!
            // Például a (5, 5) koordinátára tesszük a kígyó fejét.
            var startPos = new Position(5, 5);
            var snake = new Snake.Core.Model.Snake(startPos);

            // Lekérjük a fej jelenlegi pozícióját
            var headBefore = snake.Head;

            // 2. CSELEKVÉS (Lépés)
            // A te kódodban a Move() nem kér paramétert (a Modell kezeli az ütközést),
            // vagy ha kér, akkor adjunk neki egy üres akadálylistát.
            // A feltöltött fájljaid alapján: snake.Move();
            snake.Move();

            // 3. ELLENÕRZÉS
            // A fejnek meg kellett változnia (elmozdult)
            Assert.NotEqual(headBefore, snake.Head);
        }

        [Fact]
        public void SnakeHitsWall_IsCollidingReturnsTrue()
        {
            // 1. ELÕKÉSZÍTÉS
            // Létrehozunk egy kígyót a (0, 0) ponton
            var snake = new Snake.Core.Model.Snake(new Position(0, 0));

            // Beállítjuk az irányt balra (hogy biztosan kimenjen a pályáról)
            snake.TurnLeft(); // Ha alapból jobbra néz, balra fordulva felfelé megy (0, -1)
            // Vagy ha (0,0)-n van és felfelé lép, az már pályaelhagyás.

            // 2. CSELEKVÉS
            snake.Move();

            // 3. ELLENÕRZÉS
            // Megnézzük, hogy ütközik-e egy 10x10-es pálya falával.
            // Mivel (0, -1) vagy (-1, 0) koordinátára lépett, ennek igaznak kell lennie.
            bool isColliding = snake.IsColliding(10, 10);

            Assert.True(isColliding);
        }

        [Fact]
        public void SnakeGrows_WhenEating()
        {
            // 1. Elõkészítés
            var snake = new Snake.Core.Model.Snake(new Position(5, 5));
            int lengthBefore = snake.Body.Count;

            // 2. Cselekvés
            snake.Grow(); // Jelezzük, hogy evett
            snake.Move(); // FONTOS: Lépnünk kell egyet, hogy a növekedés megtörténjen!

            // 3. Ellenõrzés
            Assert.Equal(lengthBefore + 1, snake.Body.Count);
        }
    }
}