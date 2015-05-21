using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class IcosahedronGeometry : PolyhedronGeometry
    {
        private IcosahedronGeometry(Vector3[] vertices, int[] indices, float radius, int detail) : base(vertices,indices, radius, detail)
        {
        }

        public static IcosahedronGeometry Create(float radius = 1, int detail = 0)
        {
            var t = ( 1 + Mathf.Sqrt( 5 ) ) / 2;
            var vertices = new Vector3[]
            {
                new Vector3(-1,t,0), new Vector3(1,t,0), new Vector3(-1,-t,0), new Vector3(1,-t,0),
                new Vector3(0,-1,t), new Vector3(0,1,t), new Vector3(0,-1,-t), new Vector3(0,1,-t),
                new Vector3(t,0,-1), new Vector3(t,0,1), new Vector3(-t,0,-1), new Vector3(-t,0,1)
            };
            
            var indices = new int[]
            {
		         0, 11,  5,    0,  5,  1,    0,  1,  7,    0,  7, 10,    0, 10, 11,
		         1,  5,  9,    5, 11,  4,   11, 10,  2,   10,  7,  6,    7,  1,  8,
		         3,  9,  4,    3,  4,  2,    3,  2,  6,    3,  6,  8,    3,  8,  9,
		         4,  9,  5,    2,  4, 11,    6,  2, 10,    8,  6,  7,    9,  8,  1
            };

            return new IcosahedronGeometry(vertices, indices, radius, detail );
        }
    }
}