using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Lights
{
    public abstract class Light : Object3D
    {
        public float Exponent;
        public Color Color { get; set; }

        public Light(Color? color)
        {
            Color = color.HasValue ? color.Value : Color.White;
        }
    }
}
