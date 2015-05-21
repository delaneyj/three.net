using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class TetrahedronGeometry : PolyhedronGeometry
    {
        private TetrahedronGeometry(Vector3[] vertices, int[] indices, float radius, int detail)
            : base(vertices, indices, radius, detail)
        {
        }

        public static TetrahedronGeometry Create(float radius = 1, int detail = 0)
        {
            var vertices = new Vector3[]
            {
                new Vector3(1,1,1), new Vector3(-1,-1,1), new Vector3(-1,1,-1), new Vector3(1,-1,-1)
            };
            
            var indices = new int[]
            {
		         2,1,0, 0,3,2, 1,3,0, 2,3,1
            };

            return new TetrahedronGeometry(vertices, indices, radius, detail);
        }
    }
}
