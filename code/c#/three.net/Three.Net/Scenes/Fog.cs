using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Scenes
{
    public abstract class Fog
    {
        public Color Color { get; set; }

        public Fog(Color color)
        {
            Color = color;
        }
    }
}
