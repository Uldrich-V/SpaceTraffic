using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Globalization;
using SpaceTraffic.Models;

namespace SpaceTraffic.Data
{
    public class NacteniScn
    {
        public static Scenar Load(string filePath)
        {
            var scenario = new Scenar();

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"Soubor scénáře {filePath} nebyl nalezen.");
            }

            string currentSection = "";

            foreach (var line in File.ReadLines(filePath))
            {
                string trimmedLine = line.Trim();

                // Ignorujeme prázdné řádky a komentáře začínající '#'
                if (string.IsNullOrEmpty(trimmedLine) || trimmedLine.StartsWith("#"))
                    continue;

                // Detekce sekcí
                if (trimmedLine.StartsWith("["))
                {
                    currentSection = trimmedLine;
                    continue;
                }

                // Rozdělení řádku podle mezer (i více mezer za sebou)
                string[] parts = trimmedLine.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                // Zpracování podle toho, v jaké sekci se zrovna nacházíme
                if (currentSection == "[BODIES]" && parts.Length >= 10)
                {
                    scenario.Bodies.Add(new VesmirneTeleso
                    {
                        Name = parts[0],
                        Type = parts[1],
                        Size = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        ParentName = parts[3] == "-" ? "" : parts[3], // Pokud nemá parenta, může tam být pomlčka nebo nic, ošetříme to
                        OrbitRadius = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        InitialAngle = double.Parse(parts[5], CultureInfo.InvariantCulture),
                        AngularSpeed = double.Parse(parts[6], CultureInfo.InvariantCulture),
                        Warehouse = int.Parse(parts[7]),
                        ProdMin = int.Parse(parts[8]),
                        ProdMax = int.Parse(parts[9])
                    });
                }
                else if (currentSection == "[DESTINATIONS]" && parts.Length >= 3)
                {
                    scenario.Destinations.Add(new Destinace
                    {
                        Source = parts[0],
                        Target = parts[1],
                        Probability = int.Parse(parts[2])
                    });
                }
                else if (currentSection == "[SHIPS]" && parts.Length >= 7)
                {
                    scenario.Ships.Add(new Lod
                    {
                        Name = parts[0],
                        Speed = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        StartBodyName = parts[2],
                        Capacity = int.Parse(parts[3]),
                        TransferSpeed = int.Parse(parts[4]),
                        Strategy = Enum.Parse<LoadStrategies>(parts[5]), // Převede text "FULL", "MAX_LOADTIME" atd. na náš Enum
                        MaxLoadTime = int.Parse(parts[6])
                    });
                }
            }

            return scenario;
        }
    }
}
