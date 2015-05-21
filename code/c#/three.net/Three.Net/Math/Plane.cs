using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Three.Net.Math
{
    public class Plane
    {
        public static Plane Empty { get { return new Plane(Vector3.UnitX, 0); } }
        
        public Vector3 Normal;
        public float Constant;

        public Plane(Vector3 normal, float constant)
        {
            Normal = normal;
            Constant = constant;
        }

        public Plane(float x, float y, float z, float w)
        {
            Normal = new Vector3(x, y, z);
            Constant = w;
        }

        public void Normalize()
        {
            // Note: will lead to a divide by zero if the plane is invalid.
            var inverseNormalLength = 1 / Normal.Length();
            Normal.Multiply(inverseNormalLength);
            Constant *= inverseNormalLength;
        }

        public float DistanceToPoint(Vector3 point)
        {
            var v = new Vector3(Normal);
            return v.Dot(point) + Constant;
        }
    }
}
