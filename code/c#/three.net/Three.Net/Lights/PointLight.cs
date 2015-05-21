using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Lights
{
    public class PointLight : Light
    {
        public float Intensity { get; set; }
        public float Distance { get; set; }

        public PointLight(Color color) : base(color)
        {
            Intensity = 1;
            Distance = 0;
        }
    }
}
