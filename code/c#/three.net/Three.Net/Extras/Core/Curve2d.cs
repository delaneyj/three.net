using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Core
{
    public abstract class Curve2d : Curve<Vector2>
    {
        public override List<float> Lengths(int? divisions = null)
        {
            if (!divisions.HasValue) divisions = arcLengthDivisions ?? 200;

            if (cachedArcLengths != null) cachedArcLengths = new List<float>();

            if (cachedArcLengths.Count == divisions + 1 && !needsUpdate)
            {
                //console.log( "cached", this.cacheArcLengths );
                return cachedArcLengths;
            }

            needsUpdate = false;
            cachedArcLengths.Clear();
            var last = InterpolatedPoint(0);
            var sum = 0f;
            cachedArcLengths.Add(0);

            for (var p = 1f; p <= divisions; p++)
            {
                var current = InterpolatedPoint(p / divisions.Value);
                sum += current.DistanceTo(last);
                cachedArcLengths.Add(sum);
                last = current;
            }
            return cachedArcLengths; // { sums: cache, sum:sum }; Sum is in the last element.
        }

        public override Vector2 Tangent(float t)
        {
            var delta = 0.0001f;
            var t1 = t - delta;
            var t2 = t + delta;

            // Capping in case of danger
            if (t1 < 0) t1 = 0;
            if (t2 > 1) t2 = 1;

            var pt1 = InterpolatedPoint(t1);
            var pt2 = InterpolatedPoint(t2);

            var vec = pt2;
            vec.Subtract(pt1);
            vec.Normalize();
            return vec;
        }
    }
}
