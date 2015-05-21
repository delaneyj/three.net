using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Core
{
    public abstract class Curve<T>
    {
        protected List<float> cachedArcLengths;
        protected bool needsUpdate = true;
        protected int? arcLengthDivisions = null;

        // Virtual base class method to overwrite and implement in subclasses. 0 <= t <= 1
        public abstract T InterpolatedPoint(float t);

        // Get point at relative position in curve according to arc length. 0 <= u <= 1
        public virtual T InterpolatedPointAt(float u)
        {
            Debug.Assert(u >= 0 && u <= 1);
            var t = UtoTmapping(u);
            return InterpolatedPoint(t);
        }

        // Returns a unit vector tangent at t
        // In case any sub curve does not implement its tangent derivation,
        // 2 points a small delta apart will be used to find its gradient
        // which seems to give a reasonable approximation
        public abstract T Tangent(float t);

        // Get sequence of points using getPoint( t )
        public List<T> InterpolatedPoints(int divisions = 5)
        {
            var points = new List<T>(divisions);
            for (var d = 0f; d <= divisions; d++)
            {
                var p = InterpolatedPoint(d / divisions);
                points.Add(p);
            }
            return points;
        }

        // Get sequence of points using getPointAt( u )
        public List<T> SpacedPoints(int divisions = 5)
        {
            var points = new List<T>(divisions);
            for (var d = 0f; d <= divisions; d++)
            {
                var p = InterpolatedPointAt(d / divisions);
                points.Add(p);
            }
            return points;
        }

        

        // Get total curve arc length
        public float Length
        {
            get
            {
                var lengths = Lengths();
                return lengths.Last();
            }
        }

        // Get list of cumulative segment lengths
        public abstract List<float> Lengths(int? divisions = null);

        public void UpdateArcLengths()
        {
            needsUpdate = true;
            Lengths();
        }

        // Given u ( 0 .. 1 ), get a t to find p. This gives you points which are equi distance
        public float UtoTmapping(float u, float? distance = null)
        {
            var arcLengths = Lengths();
            int i = 0, il = arcLengths.Count;
            var targetArcLength = distance ?? u * arcLengths[il - 1]; // The targeted u distance value to get


            //var time = Date.now();
            // binary search for the index with largest value smaller than target u distance
            int low = 0, high = il - 1;

            while (low <= high)
            {
                i = Mathf.Floor(low + (high - low) / 2f); // less likely to overflow, though probably not issue here, JS doesn't really have integers, all numbers are floats

                var comparison = arcLengths[i] - targetArcLength;

                if (comparison < 0)
                {
                    low = i + 1;
                    continue;
                }
                else if (comparison > 0)
                {
                    high = i - 1;
                    continue;
                }
                else
                {
                    high = i;
                    break;
                    // DONE
                }
            }

            i = high;

            //console.log('b' , i, low, high, Date.now()- time);

            float t;

            if (arcLengths[i] == targetArcLength)
            {
                t = i / (il - 1);
                return t;

            }

            // we could get finer grain at lengths, or use simple interpolatation between two points

            var lengthBefore = arcLengths[i];
            var lengthAfter = arcLengths[i + 1];

            var segmentLength = lengthAfter - lengthBefore;

            // determine where we are between the 'before' and 'after' points

            var segmentFraction = (targetArcLength - lengthBefore) / segmentLength;

            // add that fractional amount to t
            t = (i + segmentFraction) / (float)(il - 1);
            return t;
        }
        
        public T TangentAt(float u)
        {
            var t = UtoTmapping(u);
            return Tangent(t);
        }

        public static float TangentQuadraticBezier(float t, float p0, float p1, float p2)
        {
            return 2 * (1 - t) * (p1 - p0) + 2 * t * (p2 - p1);
        }

        // Puay Bing, thanks for helping with this derivative!
        public static float TangentCubicBezier(float t, float p0, float p1, float p2, float p3)
        {
            return -3 * p0 * (1 - t) * (1 - t) +
                3 * p1 * (1 - t) * (1 - t) - 6 * t * p1 * (1 - t) +
                6 * t * p2 * (1 - t) - 3 * t * t * p2 +
                3 * t * t * p3;
        }

        public static float TangentSpline(float t, float p0, float p1, float p2, float p3)
        {
            // To check if my formulas are correct
            var h00 = 6 * t * t - 6 * t; 	// derived from 2t^3 − 3t^2 + 1
            var h10 = 3 * t * t - 4 * t + 1; // t^3 − 2t^2 + t
            var h01 = -6 * t * t + 6 * t; 	// − 2t3 + 3t2
            var h11 = 3 * t * t - 2 * t;	// t3 − t2
            return h00 + h10 + h01 + h11;
        }

        // Catmull-Rom
        public static float InterpolateCatmullRom(float p0, float p1, float p2, float p3, float t)
        {
            var v0 = (p2 - p0) / 2;
            var v1 = (p3 - p1) / 2;
            var t2 = t * t;
            var t3 = t * t2;
            return (2 * p1 - 2 * p2 + v0 + v1) * t3 + (-3 * p1 + 3 * p2 - 2 * v0 - v1) * t2 + v0 * t + p1;
        }
    }
}
