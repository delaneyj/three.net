using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class ClosedSplineCurve3 : Curve3d
    {
        private List<Vector3> points;

        public ClosedSplineCurve3(List<Vector3> points)
        {
            this.points = points;
        }

        public override Vector3 InterpolatedPoint(float t)
        {
            var point = (points.Count - 0) * t;
            // This needs to be from 0-length +1

            var intPoint = Mathf.Floor(point);
            var weight = point - intPoint;

            intPoint += intPoint > 0 ? 0 : (Mathf.Floor(Mathf.Abs(intPoint) / points.Count) + 1) * points.Count;

            var c0 = points[(intPoint - 1) % points.Count];
            var c1 = points[(intPoint) % points.Count];
            var c2 = points[(intPoint + 1) % points.Count];
            var c3 = points[(intPoint + 2) % points.Count];

            var x = Curve3d.InterpolateCatmullRom(c0.x, c1.x, c2.x, c3.x, weight);
            var y = Curve3d.InterpolateCatmullRom(c0.y, c1.y, c2.y, c3.y, weight);
            var z = Curve3d.InterpolateCatmullRom(c0.z, c1.z, c2.z, c3.z, weight);

            return new Vector3(x,y,z);
        }
    }
}
