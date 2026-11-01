using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceTraffic.Models;

public class VesmirneTeleso
{
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Size { get; set; }

    // Název tělesa, kolem kterého obíhá (např. "Sun"). Pokud neobíhá nic, bude prázdný.
    public string ParentName { get; set; } = string.Empty;
    public double OrbitRadius { get; set; }
    public double InitialAngle { get; set; }
    public double AngularSpeed { get; set; }

    // Náklad a produkce
    public int Warehouse { get; set; }
    public int ProdMin { get; set; }
    public int ProdMax { get; set; }
}