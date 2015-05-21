using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class LineCurve3 : Curve3d
    {
        private Vector3 v1, v2;

        public LineCurve3(Vector3 v1, Vector3 v2)
        {
            this.v1 = v1;
            this.v2 = v2;
        }

        public override Vector3 InterpolatedPoint(float t)
        {
            var r = Vector3.Zero;
            r = Vector3.SubtractVectors(v2, v1); // diff
            r.Multiply(t);
            r.Add(v1);
            return r;
        }
    }
}
