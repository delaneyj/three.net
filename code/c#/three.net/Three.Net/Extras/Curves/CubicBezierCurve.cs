using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class CubicBezierCurve : Curve2d
    {
        Vector2 v0, v1, v2, v3;

        public CubicBezierCurve(Vector2 v0, Vector2 v1, Vector2 v2, Vector2 v3)
        {
            this.v0 = v0;
            this.v1 = v1;
            this.v2 = v2;
            this.v3 = v3;
        }

        public override Vector2 InterpolatedPoint(float t)
        {
            var tx = Shape.Bezier3(t, v0.x, v1.x, v2.x, v3.x);
            var ty = Shape.Bezier3(t, v0.y, v1.y, v2.y, v3.y);
            return new Vector2(tx, ty);
        }

        public override Vector2 Tangent(float t)
        {
            var tx = Curve<Vector2>.TangentCubicBezier(t, v0.x, v1.x, v2.x, v3.x);
            var ty = Curve<Vector2>.TangentCubicBezier(t, v0.y, v1.y, v2.y, v3.y);
            var tangent = new Vector2(tx, ty);
            tangent.Normalize();
            return tangent;
        }
    }
}
