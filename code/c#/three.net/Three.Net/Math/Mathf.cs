using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public static class Mathf
    {
        public const float Tau = 6.2831853071795864769252f;
        public const float Pi = Tau / 2;
        public const float NaturalLog2 = 0.69314718056f;

        public static float Cos(float value) { return (float)System.Math.Cos(value); }
        public static float Sin(float value) { return (float)System.Math.Sin(value); }
        public static float Tan(float value) { return (float)System.Math.Tan(value); }

        public static float Acos(float value) { return (float)System.Math.Acos(value); }
        public static float Asin(float value) { return (float)System.Math.Asin(value); }
        public static float Atan(float value) { return (float)System.Math.Atan(value); }
        public static float Atan2(float y, float x) { return (float)System.Math.Atan2(y,x); }

        public static float Clamp(float x, float a, float b) {return ( x < a ) ? a : ( ( x > b ) ? b : x );}

        public static float DegreesToRadians(float degree) { return degree * Tau / 360; }
        public static float RadiansToDegrees(float radians) { return radians * 360 / Tau; }

        public static int Abs(int value) { return System.Math.Abs(value); }
        public static float Abs(float value) { return System.Math.Abs(value); }

        public static float Sqrt(float value) { return (float)System.Math.Sqrt(value); }

        public static int Min(int a, int b) { return System.Math.Min(a, b); }
        public static float Min(float a, float b) { return System.Math.Min(a, b); }
        public static int Max(int a, int b) { return System.Math.Max(a, b); }
        public static float Max(float a, float b) { return System.Math.Max(a, b); }


        public static bool IsPowerOfTwo(int value) { return ( value & ( value - 1 ) ) == 0 && value != 0; }

        internal static int Pow(int x, int y){ return (int)System.Math.Pow(x,y); }
        internal static float Pow(float x, float y) { return (float)System.Math.Pow(x, y); }

        internal static int Round(float value) { return (int)System.Math.Round(value);}

        internal static int Floor(float value){return (int)System.Math.Floor(value);}
        internal static int Ceiling(float value) { return (int)System.Math.Ceiling(value); }

        internal static float Log(float value) { return (float)System.Math.Log(value);}

        public static Random Random = new Random();

        public static float RandomF(float a, float b)
        {
            return (float)(a + Random.NextDouble() * (b - a));
        }

        public static float RandomF(float a)
        {
            return RandomF() * a;
        }

        public static float RandomF()
        {
            return (float)Random.NextDouble();
        }

        public static bool RandomBool { get { return RandomF() > 0.5f; } }

        public static List<Vector3> Hilbert3D(Vector3 center, float size = 10, int iterations = 1, int v0 = 0, int v1 = 1, int v2 = 2, int v3 = 3, int v4 = 4, int v5 = 5, int v6 = 6, int v7 = 7)
        {
            var half = size / 2;

            var s = new Vector3[]{
                new Vector3( center.x - half, center.y + half, center.z - half ),
		        new Vector3( center.x - half, center.y + half, center.z + half ),
		        new Vector3( center.x - half, center.y - half, center.z + half ),
		        new Vector3( center.x - half, center.y - half, center.z - half ),
		        new Vector3( center.x + half, center.y - half, center.z - half ),
		        new Vector3( center.x + half, center.y - half, center.z + half ),
		        new Vector3( center.x + half, center.y + half, center.z + half ),
		        new Vector3( center.x + half, center.y + half, center.z - half )
            };

            var vec = new List<Vector3>()
            {
		        s[ v0 ],
		        s[ v1 ],
		        s[ v2 ],
		        s[ v3 ],
		        s[ v4 ],
		        s[ v5 ],
		        s[ v6 ],
		        s[ v7 ]
            };

            // Recurse iterations
	        if( --iterations >= 0 ) {
		        var tmp = new List<Vector3>();
                tmp.AddRange(Hilbert3D ( vec[ 0 ], half, iterations, v0, v3, v4, v7, v6, v5, v2, v1 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 1 ], half, iterations, v0, v7, v6, v1, v2, v5, v4, v3 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 2 ], half, iterations, v0, v7, v6, v1, v2, v5, v4, v3 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 3 ], half, iterations, v2, v3, v0, v1, v6, v7, v4, v5 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 4 ], half, iterations, v2, v3, v0, v1, v6, v7, v4, v5 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 5 ], half, iterations, v4, v3, v2, v5, v6, v1, v0, v7 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 6 ], half, iterations, v4, v3, v2, v5, v6, v1, v0, v7 ) );
		        tmp.AddRange(Hilbert3D ( vec[ 7 ], half, iterations, v6, v5, v2, v1, v0, v3, v4, v7 ) );
                return tmp; // Return recursive call
	        }

            return vec; // Return complete Hilbert Curve.
        }

        public static float Fit(float value, float oldMin, float oldMax, float newMin, float newMax)
        {
            if(oldMin > oldMax) 
            {
                var tmp = oldMin;
                oldMin = oldMax;
                oldMax = tmp;
            }

            return (((newMax - newMin) * (value - oldMin)) / (oldMax - oldMin)) + newMin;
        }
    }
}
