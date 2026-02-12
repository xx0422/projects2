using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Ez kell a listázáshoz
using Avalonia.Platform;
using Snake.Core.Model;

namespace Snake.Core.Persistence
{
    internal class LevelDataAccess
    {
        public (int width, int height, List<Position> obstacles) LoadLevel(string levelName)
        {
            // --- NYOMOZÁS START ---
            // Ez kiírja a konzolra (Output ablak), hogy milyen fájlokat lát a program.
            // Így megtudjuk, mi a pontos címe a fájloknak!

            // 1. Megpróbáljuk kitalálni a projekt nevét (Assembly Name)
            var assemblyName = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name;
            System.Diagnostics.Debug.WriteLine($"[DEBUG] A projekt neve (Assembly): {assemblyName}");

            // 2. Megpróbáljuk betölteni a fájlt a feltételezett útvonalon
            var fullPath = $"avares://{assemblyName}/Assets/Levels/{levelName}";
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Ezt próbálom betölteni: {fullPath}");

            var uri = new Uri(fullPath);

            // --- BETÖLTÉS ---
            if (AssetLoader.Exists(uri))
            {
                System.Diagnostics.Debug.WriteLine("[DEBUG] SIKER! A fájl létezik.");
                using (var stream = AssetLoader.Open(uri))
                using (var reader = new StreamReader(stream))
                {
                    var obstacles = new List<Position>();
                    int width = 0, height = 0;
                    bool firstLine = true;
                    string? line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                        if (firstLine)
                        {
                            width = int.Parse(parts[0]);
                            height = width;
                            firstLine = false;
                        }
                        else if (parts.Length >= 2)
                        {
                            obstacles.Add(new Position(int.Parse(parts[0]), int.Parse(parts[1])));
                        }
                    }
                    return (width, height, obstacles);
                }
            }
            else
            {
                // HA NEM TALÁLJA:
                System.Diagnostics.Debug.WriteLine("[HIBA] NEM TALÁLOM A FÁJLT!");
                System.Diagnostics.Debug.WriteLine("------------------------------------------------");
                System.Diagnostics.Debug.WriteLine("ELÉRHETŐ FÁJLOK LISTÁJA (Ezek vannak az APK/Exe-ben):");

                // Trükkös rész: Megpróbáljuk kilistázni, mi van az 'Assets' mappában
                try
                {
                    var assetsUri = new Uri($"avares://{assemblyName}/Assets/Levels");
                    var assets = AssetLoader.GetAssets(assetsUri, null);
                    foreach (var asset in assets)
                    {
                        System.Diagnostics.Debug.WriteLine($" -> {asset}");
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Nem sikerült listázni: {ex.Message}");
                }
                System.Diagnostics.Debug.WriteLine("------------------------------------------------");

                return (10, 10, new List<Position>());
            }
        }
    }
}