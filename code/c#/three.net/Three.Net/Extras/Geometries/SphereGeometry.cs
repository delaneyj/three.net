using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class SphereGeometry : Geometry
    {
        public SphereGeometry(float radius = 1, int widthSegments = 8, int heightSegments = 6, float phiStart = 0, float phiLength = Mathf.Tau, float thetaStart = 0, float thetaLength = Mathf.Pi)
        {
            widthSegments = Mathf.Max(3, widthSegments);
            heightSegments = Mathf.Max(2, heightSegments);
            var uv = faceVertexUvs[0];

            var uvs = new List<List<Vector2>>();
            var verticiesIndicies = new List<List<int>>();
            for (var y = 0; y <= heightSegments; y ++ ) 
            {
                var verticesIndexRow = new List<int>();
		        var uvsRow = new List<Vector2>();

		for (var x = 0; x <= widthSegments; x ++ ) 
        {
			var u = x / (float)widthSegments;
			var v = y / (float)heightSegments;

			var vX = - radius * Mathf.Cos( phiStart + u * phiLength ) * Mathf.Sin( thetaStart + v * thetaLength );
			var vY = radius * Mathf.Cos( thetaStart + v * thetaLength );
			var vZ = radius * Mathf.Sin( phiStart + u * phiLength ) * Mathf.Sin( thetaStart + v * thetaLength );
            vertices.Add( new Vector3(vX,vY,vZ));

			verticesIndexRow.Add( vertices.Count - 1 );
			uvsRow.Add( new Vector2( u, 1 - v ) );
		}

		verticiesIndicies.Add( verticesIndexRow );
		uvs.Add( uvsRow );
            }

            for (var y = 0; y < heightSegments; y++)
            {
                for (var x = 0; x < widthSegments; x++)
                {
                    var v1 = verticiesIndicies[y][x + 1];
                    var v2 = verticiesIndicies[y][x];
                    var v3 = verticiesIndicies[y + 1][x];
                    var v4 = verticiesIndicies[y + 1][x + 1];

                    var n1 = vertices[v1].Normalized();
                    var n2 = vertices[v2].Normalized();
                    var n3 = vertices[v3].Normalized();
                    var n4 = vertices[v4].Normalized();

                    var uv1 = uvs[y][x + 1];
                    var uv2 = uvs[y][x];
                    var uv3 = uvs[y + 1][x];
                    var uv4 = uvs[y + 1][x + 1];

                    if (Mathf.Abs(vertices[v1].y) == radius)
                    {
                        uv1.x = (uv1.x + uv2.x) / 2;
                        faces.Add(new Face3(v1, v3, v4, n1,n3,n4));
                        uv.Add(new UVFaceSet(uv1, uv3, uv4));
                    }
                    else if (Mathf.Abs(vertices[v3].y) == radius)
                    {
                        uv3.x = (uv3.x + uv4.x) / 2;
                        faces.Add(new Face3(v1, v2, v3, n1,n2,n3));
                        uv.Add(new UVFaceSet(uv1,uv2,uv3));
                    }
                    else
                    {
                        faces.Add(new Face3(v1, v2, v4, n1,n2,n4));
                        uv.Add(new UVFaceSet(uv1,uv2,uv4));
                        faces.Add(new Face3(v2, v3, v4, n2,n3,n4));
                        uv.Add(new UVFaceSet(uv2,uv3,uv4));
                    }
                }
            }

            ComputeNormals();
	        BoundingSphere = new Sphere( Vector3.Zero, radius );
        }
    }
}
