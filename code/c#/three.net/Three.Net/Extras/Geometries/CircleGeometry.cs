using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class CircleGeometry : Geometry
    {
        public CircleGeometry(float radius=1,int segments=8,float thetaStart = 0, float thetaLength = Mathf.Tau)
        {
            segments = Mathf.Max(3, segments);
            var uvs = new List<Vector2>();
            
            vertices.Add(Vector3.Zero);
            uvs.Add(Vector2.Half);

            for(var i = 0f; i <= segments; i ++ ) 
            {
                
		        var segment = thetaStart + i / segments * thetaLength;

                var v = new Vector3(Mathf.Cos( segment ),radius * Mathf.Sin( segment ),0);
                v.Multiply(radius);
                vertices.Add(v);
                uvs.Add(new Vector2(( v.x / radius + 1 ) / 2, ( v.y / radius + 1 ) / 2 ));
            }

            var n = Vector3.UnitZ;

            for (var i = 1; i <= segments; i++)
            {
                faces.Add(new Face3(i, i + 1, 0, n));
                var faceSet = new UVFaceSet(uvs[i], uvs[i + 1], Vector2.Half);
                faceVertexUvs[0].Add(faceSet);
            }

            ComputeNormals();
            BoundingSphere = new Sphere(Vector3.Zero,radius );
        }
    }
}
