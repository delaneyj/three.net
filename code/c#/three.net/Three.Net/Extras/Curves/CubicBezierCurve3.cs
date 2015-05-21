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
    public class CubicBezierCurve3 : Curve3d
    {
        private Vector3 v0, v1, v2, v3;

        public CubicBezierCurve3(Vector3 v0, Vector3 v1, Vector3 v2, Vector3 v3)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public override Vector3 InterpolatedPoint(float t)
        {
            var tx = Shape.Bezier3(t, this.v0.x, this.v1.x, this.v2.x, this.v3.x);
            var ty = Shape.Bezier3(t, this.v0.y, this.v1.y, this.v2.y, this.v3.y);
            var tz = Shape.Bezier3(t, this.v0.z, this.v1.z, this.v2.z, this.v3.z);

            return new Vector3(tx, ty, tz);
        }
    }
}
