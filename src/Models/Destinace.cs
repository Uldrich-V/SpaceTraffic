using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceTraffic.Models;

public class Destinace
{
    public string Source { get; set; } = string.Empty;
    public string Target { get; set; } = string.Empty; // V zadání jako "destination"
    public int Probability { get; set; }
}
