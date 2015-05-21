using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Scenes
{
    public class FogLinear :Fog
    {
        public float Near { get; set; }
        public float Far { get; set; }

        public FogLinear(Color color, float near = 1, float far = 1000) : base(color)
        {
            Near = near;
            Far = far;
        }
    }
}
