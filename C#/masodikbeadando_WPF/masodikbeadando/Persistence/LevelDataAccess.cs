using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using masodikbeadando.Model;

namespace masodikbeadando.Persistence
{
    internal class LevelDataAccess
    {
        public (int width, int height, List<Position> obstacles) LoadLevel(string filePath)
        {
            var obstacles = new List<Position>();
            string[] lines = File.ReadAllLines(filePath);
            bool firstLine = true;
            int width = 0, height = 0;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (firstLine)
                {
                    width = int.Parse(parts[0]);
                    height = width;
                    firstLine = false;
                }
                else
                {
                    int x = int.Parse(parts[0]);
                    int y = int.Parse(parts[1]);
                    obstacles.Add(new Position(x, y));
                }
            }

            return (width, height, obstacles);
        }
    }
}
