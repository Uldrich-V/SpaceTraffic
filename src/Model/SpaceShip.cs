namespace SpaceTraffic.Model;

using SpaceTraffic.Strategy;

public enum ShipState
{
    DockedUnloading,
    DockedLoading,
    InTransit
}

public class SpaceShip
{
    public string Name { get; set; } = string.Empty;
    public double Speed { get; set; }
    public int Capacity { get; set; }
    public int CurrentCargo { get; set; }
    public int TransferSpeed { get; set; }
    public ILoadingStrategy Strategy { get; set; } = null!;

    public CelestialBody? DockedBody { get; set; }
    public CelestialBody? TargetBody { get; set; }
    public ShipState State { get; set; }

    public double X { get; set; }
    public double Y { get; set; }

    public int DaysLoadingSession { get; set; }
    public bool IsLoadingArrow { get; set; }
}