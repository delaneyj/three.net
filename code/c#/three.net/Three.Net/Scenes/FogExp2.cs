using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Scenes
{
    public class FogExp2 : Fog
    {
        public float Density { get; set; }

        public FogExp2(Color color, float density = 0.0025f) : base(color)
        {
            Density = density;
        }
    }
}
