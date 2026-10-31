namespace SpaceTraffic.Model;

public enum BodyType
{
    Star,
    Planet,
    Moon,
    Asteroid
}

public class CelestialBody
{
    public string Name { get; set; } = string.Empty;
    public BodyType Type { get; set; }
    public double Size { get; set; }
    public CelestialBody? Parent { get; set; }
    public double OrbitRadius { get; set; }
    public double InitialAngle { get; set; }
    public double AngularSpeed { get; set; }
    public int Warehouse { get; set; }
    public int ProdMin { get; set; }
    public int ProdMax { get; set; }

    public double X { get; set; }
    public double Y { get; set; }
}

public record DestinationOption(string TargetName, int Probability);