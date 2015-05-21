using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Lights
{
    public class SpotLight : Light, OnlyShadow, HasTarget
    {
        public float Intensity;
        public float Distance;
        public float Angle;

        public bool UseOnlyShadow { get; protected set; }

        public Vector3 Target { get; set; }

        public SpotLight(Color color) : base(color)
        {
            DoesCastShadow = false;
            UseOnlyShadow = false;
        }
    }
}
