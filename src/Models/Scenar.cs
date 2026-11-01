using System;
using System.Collections.Generic;
using System.Text;

namespace SpaceTraffic.Models
{
    public class Scenar
    {
        public List<VesmirneTeleso> Bodies { get; set; } = new();
        public List<Destinace> Destinations { get; set; } = new();
        public List<Lod> Ships { get; set; } = new();
    }
}
