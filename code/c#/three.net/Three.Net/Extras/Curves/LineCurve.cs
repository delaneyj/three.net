using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class LineCurve : Curve2d
    {
        Vector2 v1, v2;

        public LineCurve(Vector2 v1, Vector2 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public override Vector2 InterpolatedPoint(float t)
        {
            var point = v2;
            point.Subtract(v1);
            point.Multiply(t);
            point.Add(v1);
            return point;
        }

        public override Vector2 InterpolatedPointAt(float u)
        {
            return InterpolatedPoint(u);
        }

        public override Vector2 Tangent(float t)
        {
            var tangent = v2;
            tangent.Subtract(v1);
            tangent.Normalize();
            return tangent;
        }
    }
}
