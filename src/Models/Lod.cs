using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceTraffic.Models;

public enum LoadStrategies
{
    FULL,
    IMMEDIATE,
    MAX_LOADTIME
}

public enum LodStav
{
    NA_CESTE,
    VYKLADKA,
    NAKLADKA
}

public class Lod
{
    public string Name { get; set; } = string.Empty;
    public double Speed { get; set; }
    public string StartBodyName { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int TransferSpeed { get; set; }
    public LoadStrategies Strategy { get; set; }
    public int MaxLoadTime { get; set; }

    // Simulační stav lodě
    public int CurrentLoad { get; set; } = 0;
    public string TargetBodyName { get; set; } = string.Empty;
    public LodStav Stav { get; set; } = LodStav.NAKLADKA;
    public int DobaCekaniDni { get; set; } = 0;

    // Aktuální fyzikální pozice lodě
    public double CurrentX { get; set; }
    public double CurrentY { get; set; }
}