using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class OctahedronGeometry : PolyhedronGeometry
    {
        private OctahedronGeometry(Vector3[] vertices, int[] indices, float radius, int detail)
            : base(vertices, indices, radius, detail)
        {
        }

        public static OctahedronGeometry Create(float radius = 1, int detail = 0)
        {
            var vertices = new Vector3[]{
                new Vector3(1,0,0), new Vector3(-1,0,0), new Vector3(0,1,0), new Vector3(0,-1,0), new Vector3(0,0,1), new Vector3(0,0,-1)
            };

            var indices = new int[]{
                0, 2, 4,    0, 4, 3,    0, 3, 5,    0, 5, 2,    1, 2, 5,    1, 5, 3,    1, 3, 4,    1, 4, 2
            };

            return new OctahedronGeometry(vertices, indices, radius, detail);
        }
    }
}
