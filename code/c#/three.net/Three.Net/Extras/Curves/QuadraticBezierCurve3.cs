using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class QuadraticBezierCurve3 : Curve3d
    {
        private Vector3 v0, v1, v2;

        public QuadraticBezierCurve3(Vector3 v0, Vector3 v1, Vector3 v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }

        public override Vector3 InterpolatedPoint(float t)
        {
            var tx = Shape.Bezier2(t, this.v0.x, this.v1.x, this.v2.x);
            var ty = Shape.Bezier2(t, this.v0.y, this.v1.y, this.v2.y);
            var tz = Shape.Bezier2(t, this.v0.z, this.v1.z, this.v2.z);
            return new Vector3(tx, ty, tz);
        }
    }
}
