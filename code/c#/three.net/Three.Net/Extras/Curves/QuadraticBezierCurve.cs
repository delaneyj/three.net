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
    public class QuadraticBezierCurve : Curve2d
    {
        private Vector2 v0, v1, v2;
        public QuadraticBezierCurve(Vector2 v0, Vector2 v1, Vector2 v2)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
        }

        public override Vector2 InterpolatedPoint(float t)
        {
            var tx = Shape.Bezier2(t, v0.x, v1.x, v2.x);
            var ty = Shape.Bezier2(t, v0.y, v1.y, v2.y);
            return new Vector2(tx, ty);
        }

        public override Vector2 Tangent(float t)
        {
            var tx = Curve2d.TangentQuadraticBezier(t, v0.x, v1.x, v2.x);
            var ty = Curve2d.TangentQuadraticBezier(t, v0.y, v1.y, v2.y);

            // returns unit vector
            var tangent = new Vector2(tx, ty);
            tangent.Normalize();
            return tangent;
        }
    }
}
