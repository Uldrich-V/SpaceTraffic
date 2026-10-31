namespace SpaceTraffic.Parser;

using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using SpaceTraffic.Model;
using SpaceTraffic.Strategy;

public class ScenarioParser
{
    public static (List<CelestialBody> bodies, Dictionary<string, List<DestinationOption>> destinations, List<SpaceShip> ships) Parse(string scenarioPath)
    {
        var bodies = new List<CelestialBody>();
        var destinations = new Dictionary<string, List<DestinationOption>>();
        var ships = new List<SpaceShip>();

        if (!File.Exists(scenarioPath))
            throw new FileNotFoundException($"Scénář nebyl nalezen na cestě: {scenarioPath}");

        string currentSection = "";
        foreach (var rawLine in File.ReadLines(scenarioPath))
        {
            string line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;

            if (line.StartsWith("[") && line.EndsWith("]"))
            {
                currentSection = line.ToUpper();
                continue;
            }

            var parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

            switch (currentSection)
            {
                case "[BODIES]":
                    var body = new CelestialBody
                    {
                        Name = parts[0],
                        Type = Enum.Parse<BodyType>(parts[1], true),
                        Size = double.Parse(parts[2], CultureInfo.InvariantCulture),
                        OrbitRadius = double.Parse(parts[4], CultureInfo.InvariantCulture),
                        InitialAngle = double.Parse(parts[5], CultureInfo.InvariantCulture),
                        AngularSpeed = double.Parse(parts[6], CultureInfo.InvariantCulture),
                        Warehouse = int.Parse(parts[7]),
                        ProdMin = int.Parse(parts[8]),
                        ProdMax = int.Parse(parts[9])
                    };
                    if (parts[3] != "-")
                        body.Parent = bodies.Find(b => b.Name == parts[3]);
                    bodies.Add(body);
                    break;

                case "[DESTINATIONS]":
                    string src = parts[0];
                    if (!destinations.ContainsKey(src)) destinations[src] = new();
                    destinations[src].Add(new DestinationOption(parts[1], int.Parse(parts[2])));
                    break;

                case "[SHIPS]":
                    string stratType = parts[5].ToUpper();
                    int maxTime = parts.Length > 6 ? int.Parse(parts[6]) : 0;

                    ILoadingStrategy strategy = stratType switch
                    {
                        "FULL" => new FullStrategy(),
                        "IMMEDIATE" => new ImmediateStrategy(),
                        "MAX_LOADTIME" => new MaxLoadTimeStrategy(maxTime),
                        _ => throw new ArgumentException($"Neznámá strategie: {stratType}")
                    };

                    var startBody = bodies.Find(b => b.Name == parts[2]);
                    var ship = new SpaceShip
                    {
                        Name = parts[0],
                        Speed = double.Parse(parts[1], CultureInfo.InvariantCulture),
                        DockedBody = startBody,
                        Capacity = int.Parse(parts[3]),
                        TransferSpeed = int.Parse(parts[4]),
                        Strategy = strategy,
                        State = ShipState.DockedUnloading,
                        X = startBody?.X ?? 0,
                        Y = startBody?.Y ?? 0
                    };
                    ships.Add(ship);
                    break;
            }
        }

        return (bodies, destinations, ships);
    }
}