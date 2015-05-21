using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class SplineCurve3 : Curve3d
    {
        private List<Vector3> points;

        public SplineCurve3(List<Vector3> points)
        {
            this.points = points;
        }

        public override Vector3 InterpolatedPoint(float t)
        {
            var point = (points.Count - 1) * t;
            var intPoint = Mathf.Floor(point);
            var weight = point - intPoint;
            var c0 = intPoint == 0 ? intPoint : intPoint - 1;
            var c1 = intPoint;
            var c2 = intPoint > points.Count - 2 ? points.Count - 1 : intPoint + 1;
            var c3 = intPoint > points.Count - 3 ? points.Count - 1 : intPoint + 2;

            var pt0 = points[c0];
            var pt1 = points[c1];
            var pt2 = points[c2];
            var pt3 = points[c3];

            var x = Curve3d.InterpolateCatmullRom(pt0.x, pt1.x, pt2.x, pt3.x, weight);
            var y = Curve3d.InterpolateCatmullRom(pt0.y, pt1.y, pt2.y, pt3.y, weight);
            var z = Curve3d.InterpolateCatmullRom(pt0.z, pt1.z, pt2.z, pt3.z, weight);

            return new Vector3(x, y, z);
        }
    }
}
