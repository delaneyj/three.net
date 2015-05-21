using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public struct Sphere
    {
        public static Sphere Empty = new Sphere(Vector3.Zero,0);

        public Vector3 Center;
        public float Radius;

        public Sphere(Vector3 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Sphere)) return false;
            var sphere2 = (Sphere)obj;
            return Center == sphere2.Center && Radius == sphere2.Radius;
        }

        public static bool operator ==(Sphere a, Sphere b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Sphere a, Sphere b)
        {
            return !a.Equals(b);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }

        public void Apply(Matrix4 matrix)
        {
            Center.Apply(matrix);
            Radius *= matrix.GetMaxScaleOnAxis();
        }

        public static Sphere FromPoints(List<Vector3> points)
        {
            var box = Box3.Empty;
            var center = Box3.FromPoints( points).Center();
            var maxRadiusSq = 0f;

            foreach(var p in points)
            {
                maxRadiusSq = Mathf.Max(maxRadiusSq, center.DistanceToSquared(p));
            }

            return new Sphere(center, Mathf.Sqrt(maxRadiusSq));
        }
    }
}
