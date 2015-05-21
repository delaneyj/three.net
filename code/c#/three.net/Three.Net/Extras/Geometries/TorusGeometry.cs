using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class TorusGeometry : Geometry
    {
        public TorusGeometry(float radius = 1, float tubeDiameter = 0.4f, int radialSegments = 16, int tubularSegments = 24, float arc = Mathf.Tau)
        {
            var center = Vector3.Zero;
            var uvs = new List<Vector2>();
            var normals = new List<Vector3>();

            for (var j = 0f; j <= radialSegments; j++)
            {
                for (var i = 0f; i <= tubularSegments; i++)
                {
                    var u = i / tubularSegments * arc;
                    var v = j / radialSegments * Mathf.Tau;
                    center.x = radius * Mathf.Cos(u);
                    center.y = radius * Mathf.Sin(u);

                    var vector = new Vector3((radius + tubeDiameter * Mathf.Cos(v)) * Mathf.Cos(u),
                                             (radius + tubeDiameter * Mathf.Cos(v)) * Mathf.Sin(u),
                                             tubeDiameter * Mathf.Sin(v));
                    vertices.Add(vector);
                    uvs.Add(new Vector2(i / (float)tubularSegments, j / (float)radialSegments));
                    var n = vector;
                    n.Subtract(center);
                    n.Normalize();
                    normals.Add(n);
                }
            }

            for (var j = 1; j <= radialSegments; j++)
            {
                for (var i = 1; i <= tubularSegments; i++)
                {
                    var a = (tubularSegments + 1) * j + i - 1;
                    var b = (tubularSegments + 1) * (j - 1) + i - 1;
                    var c = (tubularSegments + 1) * (j - 1) + i;
                    var d = (tubularSegments + 1) * j + i;

                    faces.Add(new Face3(a, b, d, normals[a], normals[b], normals[d]));
                    faceVertexUvs[0].Add(new UVFaceSet(uvs[a], uvs[b], uvs[d]));
                    faces.Add(new Face3(b, c, d, normals[b], normals[c], normals[d]));
                    faceVertexUvs[0].Add(new UVFaceSet(uvs[b], uvs[c], uvs[d]));
                }
            }

            ComputeNormals();
        }
    }
}