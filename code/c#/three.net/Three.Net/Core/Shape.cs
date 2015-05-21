using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Core
{
    //Defines a 2d shape plane using paths.
    public class Shape
    {
        #region Quad Bezier Functions
        #region Bezier Curves formulas obtained from http://en.wikipedia.org/wiki/B%C3%A9zier_curve
        public static float Bezier2P0(float t, float p)
        {
            var k = 1 - t;
            return k * k * p;
        }

        public static float Bezier2P1(float t, float p)
        {
            return 2 * (1 - t) * t * p;
        }

        public static float Bezier2P2(float t, float p)
        {
            return t * t * p;
        }

        public static float Bezier2(float t, float p0, float p1, float p2)
        {
            return Bezier2P0(t, p0) + Bezier2P1(t, p1) + Bezier2P2(t, p2);
        }
        #endregion

        #region Cubic Bezier Functions
        public static float Bezier3P0(float t, float p)
        {
            var k = 1 - t;
            return k * k * k * p;
        }

        public static float Bezier3P1(float t, float p)
        {
            var k = 1 - t;
            return 3 * k * k * t * p;
        }

        public static float Bezier3P2(float t, float p)
        {
            var k = 1 - t;
            return 3 * k * t * t * p;
        }

        public static float Bezier3P3(float t, float p)
        {

            return t * t * t * p;

        }

        public static float Bezier3(float t, float p0, float p1, float p2, float p3)
        {

            return Bezier3P0(t, p0) + Bezier3P1(t, p1) + Bezier3P2(t, p2) + Bezier3P3(t, p3);

        }
        #endregion
        #endregion
    }
}
