using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Lights
{
    public class HemisphericalLight:Light
    {
        public Color GroundColor { get; set; }
        public float Intensity { get; set; }

        public HemisphericalLight(Color skyColor, Color groundColor, float intensity  = 1) : base(skyColor)
        {
            GroundColor = groundColor;
            Intensity = intensity;

            Position = new Vector3(0, 100, 0);
        }
    }
}
