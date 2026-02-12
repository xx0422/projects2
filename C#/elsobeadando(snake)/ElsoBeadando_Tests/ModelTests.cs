using elsobeadando;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ElsoBeadando_Tests
{
    [TestClass]
    public class ModelTests
    {
        private Modell model = null!;

        [TestInitialize]
        public void Setup()
        {
            model = new Modell(10, 10);
        }

        [TestMethod]
        public void Snake_Moves_WhenUpdateCalled()
        {
            Point oldHead = model.GetSnakeBody.First();
            model.Update();
            Point newHead = model.GetSnakeBody.First();

            Assert.AreNotEqual(oldHead, newHead, "A kígyó nem mozdult el az Update hívása után.");
        }

        [TestMethod]
        public void Food_IsGeneratedWithinBounds()
        {
            Point food = model.GetFoodPosition;
            Assert.IsTrue(food.X >= 0 && food.X < model.GetWidth, "Az étel X koordinátája érvénytelen.");
            Assert.IsTrue(food.Y >= 0 && food.Y < model.GetHeight, "Az étel Y koordinátája érvénytelen.");
        }

        [TestMethod]
        public void Score_Changes_WhenFoodEaten()
        {
            Point head = model.GetSnakeBody.First();
            Point foodPos = new Point(head.X + 1, head.Y); 

            var foodField = typeof(Modell)
                .GetField("food", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(model);
            foodField?.GetType().GetProperty("Position")?.SetValue(foodField, foodPos);

            int oldScore = model.GetScore;
            model.Update();

            Assert.IsTrue(model.GetScore > oldScore,
                $"A pontszámnak nőnie kellett volna. Head: {head}, Food: {foodPos}, Score: {model.GetScore}");
        }

        [TestMethod]
        public void GameOver_WhenSnakeHitsWall()
        {
            bool eventTriggered = false;
            model.GameEnded += (_, e) => eventTriggered = true;

            for (int i = 0; i < model.GetWidth + model.GetHeight + 10; i++)
                model.Update();

            Assert.IsTrue(model.IsGameOver, "A kígyónak ütközés után véget kellett volna érnie a játéknak.");
            Assert.IsTrue(eventTriggered, "A GameEnded eseménynek ki kellett volna váltódnia.");
        }

        [TestMethod]
        public void ScoreChanged_Event_Triggered()
        {
            bool eventTriggered = false;
            model.ScoreChanged += _ => eventTriggered = true;

            Point head = model.GetSnakeBody.First();
            Point foodPos = new Point(head.X + 1, head.Y);

            var foodField = typeof(Modell)
                .GetField("food", BindingFlags.NonPublic | BindingFlags.Instance)
                ?.GetValue(model);
            foodField?.GetType().GetProperty("Position")?.SetValue(foodField, foodPos);

            model.Update();

            Assert.IsTrue(eventTriggered, "A ScoreChanged eseménynek ki kellett volna váltódnia étel elfogyasztásakor.");
        }

        [TestMethod]
        public void SnakeMoved_Event_Triggered()
        {
            bool movedTriggered = false;
            model.SnakeMoved += (_, e) => movedTriggered = true;

            model.Update();

            Assert.IsTrue(movedTriggered, "A SnakeMoved eseménynek ki kellett volna váltódnia a kígyó mozgásakor.");
        }

        [TestMethod]
        public void LoadLevel_LoadsObstaclesCorrectly()
        {
            string tempPath = Path.GetTempFileName();
            string[] levelData =
            {
                "10",
                "1 1",
                "2 2",
                "3 3"
            };
            File.WriteAllLines(tempPath, levelData);

            model.LoadLevel(tempPath);

            Assert.AreEqual(3, model.Obstacles.Count,
                $"Az akadályok száma nem egyezik (várt: 3, kapott: {model.Obstacles.Count}).");

            Assert.IsTrue(model.Obstacles.Contains(new Point(1, 1)), "Hiányzik az (1,1) akadály.");
            Assert.IsTrue(model.Obstacles.Contains(new Point(2, 2)), "Hiányzik a (2,2) akadály.");
            Assert.IsTrue(model.Obstacles.Contains(new Point(3, 3)), "Hiányzik a (3,3) akadály.");

            File.Delete(tempPath);
        }

        [TestMethod]
        public void GameEnded_Event_Fires_OnCollision()
        {
            bool gameOverFired = false;
            model.GameEnded += (_, e) => gameOverFired = true;

            for (int i = 0; i < 50 && !model.IsGameOver; i++)
                model.Update();

            Assert.IsTrue(gameOverFired, "A GameEnded eseménynek ütközéskor le kellett volna futnia.");
        }
    }
}
