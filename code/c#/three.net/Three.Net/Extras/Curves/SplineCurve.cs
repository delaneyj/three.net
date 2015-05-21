using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Extras.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class SplineCurve : Curve2d
    {
        private List<Vector2> points;

        public SplineCurve(List<Vector2> points)
        {
            this.points = points ?? new List<Vector2>();
        }

        public override Math.Vector2 InterpolatedPoint(float t)
        {
	        var point = ( points.Count - 1 ) * t;
            var intPoint = Mathf.Floor( point );
	        var weight = point - intPoint;

            var c = new int[]{
                intPoint == 0 ? intPoint : intPoint - 1,
                intPoint,
                intPoint  > points.Count - 2 ? points.Count -1 : intPoint + 1,
                intPoint  > points.Count - 3 ? points.Count -1 : intPoint + 2,
            };

            var x = Curve2d.InterpolateCatmullRom( points[ c[ 0 ] ].x, points[ c[ 1 ] ].x, points[ c[ 2 ] ].x, points[ c[ 3 ] ].x, weight );
            var y = Curve2d.InterpolateCatmullRom( points[ c[ 0 ] ].y, points[ c[ 1 ] ].y, points[ c[ 2 ] ].y, points[ c[ 3 ] ].y, weight );
            return new Vector2(x, y);
        }
    }
}
