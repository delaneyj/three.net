using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Curves
{
    public class ArcCurve : EllipseCurve
    {
        public ArcCurve(Vector2 center, Vector2 radius,float startAngle,float endAngle, bool isClockwise ) : base(center,radius,startAngle,endAngle, isClockwise)
        {

        }
    }
}
