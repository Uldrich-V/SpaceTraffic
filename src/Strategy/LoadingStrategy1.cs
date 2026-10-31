namespace SpaceTraffic.Strategy;

using SpaceTraffic.Model;

public interface ILoadingStrategy
{
    bool ShouldFinishLoading(SpaceShip ship, int daysLoadingSession);
}

public class FullStrategy : ILoadingStrategy
{
    public bool ShouldFinishLoading(SpaceShip ship, int daysLoadingSession)
        => ship.CurrentCargo >= ship.Capacity;
}

public class ImmediateStrategy : ILoadingStrategy
{
    public bool ShouldFinishLoading(SpaceShip ship, int daysLoadingSession)
    {
        if (ship.CurrentCargo >= ship.Capacity) return true;
        return ship.DockedBody != null && ship.DockedBody.Warehouse < ship.TransferSpeed;
    }
}

public class MaxLoadTimeStrategy : ILoadingStrategy
{
    private readonly int _maxLoadTime;
    public MaxLoadTimeStrategy(int maxLoadTime) => _maxLoadTime = maxLoadTime;

    public bool ShouldFinishLoading(SpaceShip ship, int daysLoadingSession)
        => ship.CurrentCargo >= ship.Capacity || daysLoadingSession >= _maxLoadTime;
}