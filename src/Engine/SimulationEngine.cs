namespace SpaceTraffic.Engine;

using System;
using System.Collections.Generic;
using System.Linq;
using SpaceTraffic.Model;

public class SimulationEngine
{
    public DateTime CurrentDate { get; private set; } = new DateTime(2026, 9, 21);
    public double TotalSimDays { get; private set; } = 0.0;

    public List<CelestialBody> Bodies { get; private set; } = new();
    public List<SpaceShip> Ships { get; private set; } = new();
    public Dictionary<string, List<DestinationOption>> Destinations { get; private set; } = new();

    private readonly Random _rnd = new();

    public void LoadScenario(List<CelestialBody> b, Dictionary<string, List<DestinationOption>> d, List<SpaceShip> s)
    {
        Bodies = b;
        Destinations = d;
        Ships = s;
        UpdateBodyPositions(0);
    }

    public void ExecuteDiscreteStep()
    {
        TotalSimDays += 1.0;
        CurrentDate = CurrentDate.AddDays(1);

        UpdateBodyPositions(TotalSimDays);
        foreach (var body in Bodies)
        {
            if (body.ProdMax > 0)
                body.Warehouse += _rnd.Next(body.ProdMin, body.ProdMax + 1);
        }

        foreach (var ship in Ships)
        {
            ProcessShipStep(ship);
        }
    }

    private void ProcessShipStep(SpaceShip ship)
    {
        if (ship.State == ShipState.DockedUnloading)
        {
            ship.IsLoadingArrow = false;
            int unloadAmount = Math.Min(ship.CurrentCargo, ship.TransferSpeed);
            ship.CurrentCargo -= unloadAmount;
            if (ship.DockedBody != null) ship.DockedBody.Warehouse += unloadAmount;

            if (ship.CurrentCargo == 0)
            {
                ship.State = ShipState.DockedLoading;
                ship.DaysLoadingSession = 0;
            }
        }
        else if (ship.State == ShipState.DockedLoading)
        {
            ship.IsLoadingArrow = true;
            if (ship.DockedBody != null && ship.DockedBody.Warehouse > 0)
            {
                int loadAmount = Math.Min(ship.Capacity - ship.CurrentCargo,
                                 Math.Min(ship.DockedBody.Warehouse, ship.TransferSpeed));
                ship.CurrentCargo += loadAmount;
                ship.DockedBody.Warehouse -= loadAmount;
            }

            if (ship.Strategy.ShouldFinishLoading(ship, ship.DaysLoadingSession))
            {
                ship.TargetBody = SelectDestination(ship.DockedBody!.Name);
                ship.DockedBody = null;
                ship.State = ShipState.InTransit;
            }
            else
            {
                ship.DaysLoadingSession++;
            }
        }
        else if (ship.State == ShipState.InTransit)
        {
            double dx = ship.TargetBody!.X - ship.X;
            double dy = ship.TargetBody!.Y - ship.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);

            if (distance <= ship.Speed)
            {
                ship.DockedBody = ship.TargetBody;
                ship.X = ship.DockedBody.X;
                ship.Y = ship.DockedBody.Y;
                ship.State = ShipState.DockedUnloading;
            }
            else
            {
                ship.X += (dx / distance) * ship.Speed;
                ship.Y += (dy / distance) * ship.Speed;
            }
        }

        if (ship.DockedBody != null)
        {
            ship.X = ship.DockedBody.X;
            ship.Y = ship.DockedBody.Y;
        }
    }

    private CelestialBody? SelectDestination(string sourceName)
    {
        if (!Destinations.TryGetValue(sourceName, out var list) || list.Count == 0) return null;
        int roll = _rnd.Next(1, 101);
        int acc = 0;
        foreach (var opt in list)
        {
            acc += opt.Probability;
            if (roll <= acc) return Bodies.FirstOrDefault(b => b.Name == opt.TargetName);
        }
        return Bodies.FirstOrDefault(b => b.Name == list[0].TargetName);
    }

    public void UpdateBodyPositions(double simDays)
    {
        foreach (var body in Bodies)
        {
            double angleDeg = body.InitialAngle + (body.AngularSpeed * simDays);
            double angleRad = angleDeg * Math.PI / 180.0;

            double parentX = body.Parent?.X ?? 0.0;
            double parentY = body.Parent?.Y ?? 0.0;

            body.X = parentX + body.OrbitRadius * Math.Cos(angleRad);
            body.Y = parentY + body.OrbitRadius * Math.Sin(angleRad);
        }
    }
}