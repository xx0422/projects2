using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elsobeadando.Persistence
{
    internal class LevelDataAccess
    {
        public (int width, int height, List<Point> obstacles) LoadLevel(string filePath)
        {
            var obstacles = new List<Point>();
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
                    obstacles.Add(new Point(x, y));
                }
            }

            return (width, height, obstacles);
        }
    }
}
