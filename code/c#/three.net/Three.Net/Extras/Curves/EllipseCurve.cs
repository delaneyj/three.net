using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class EllipseCurve : Curve2d
    {
        private Vector2 center, radius;
        private float startAngle, endAngle;
        private bool isClockwise;

        public EllipseCurve(Vector2 center, Vector2 radius, float startAngle, float endAngle, bool isClockwise)
        {
            this.center = center;
            this.radius = radius;
            this.startAngle = startAngle;
            this.endAngle = endAngle;
            this.isClockwise = isClockwise;
        }

        public override Vector2 InterpolatedPoint(float t)
        {
            var deltaAngle = endAngle - startAngle;
            if (deltaAngle < 0) deltaAngle += Mathf.Tau;
            if (deltaAngle > Mathf.Tau) deltaAngle -= Mathf.Tau;
            var angle = isClockwise ? endAngle + (1 - t) * (Mathf.Tau - deltaAngle) : startAngle + t * deltaAngle;
            var tx = center.x + radius.x * Mathf.Cos(angle);
            var ty = center.y + radius.y * Mathf.Sin(angle);
            return new Vector2(tx, ty);
        }
    }
}
