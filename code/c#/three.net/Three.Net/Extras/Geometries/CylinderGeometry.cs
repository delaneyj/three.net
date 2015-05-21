using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class CylinderGeometry : Geometry
    {
        public CylinderGeometry(float radiusTop = 1, float radiusBottom = 1, float height = 2, int radialSegments = 12, int heightSegments = 1, bool openEnded = false)
        {
            var heightHalf = height / 2;
            var verticeIndicies = new List<List<int>>();
            var uvs = new List<List<Vector2>>();
            var uv = faceVertexUvs[0];
            int x, y;

            for (y = 0; y <= heightSegments; y++)
            {
                var verticesRow = new List<int>();
                var uvsRow = new List<Vector2>();

                var v = y / heightSegments;
                var radius = v * (radiusBottom - radiusTop) + radiusTop;

                for (x = 0; x <= radialSegments; x++)
                {
                    var u = x / (float)radialSegments;
                    var vX = radius * Mathf.Sin(u * Mathf.Tau);
                    var vY = -v * height + heightHalf;
                    var vZ = radius * Mathf.Cos(u * Mathf.Tau);
                    vertices.Add(new Vector3(vX,vY,vZ));
                    verticesRow.Add(vertices.Count - 1);
                    uvsRow.Add(new Vector2(u, 1 - v));
                }

                verticeIndicies.Add(verticesRow);
                uvs.Add(uvsRow);
            }

            var tanTheta = (radiusBottom - radiusTop) / height;
            Vector3 na, nb;

            for (x = 0; x < radialSegments; x++)
            {
                if (radiusTop != 0)
                {
                    na = vertices[verticeIndicies[0][x]];
                    nb = vertices[verticeIndicies[0][x + 1]];
                }
                else
                {
                    na = vertices[verticeIndicies[1][x]];
                    nb = vertices[verticeIndicies[1][x + 1]];
                }

                na.y = Mathf.Sqrt(na.x * na.x + na.z * na.z) * tanTheta;
                na.Normalize();

                nb.y = Mathf.Sqrt(nb.x * nb.x + nb.z * nb.z) * tanTheta;
                nb.Normalize();

                for (y = 0; y < heightSegments; y++)
                {
                    var v1 = verticeIndicies[y][x];
                    var v2 = verticeIndicies[y + 1][x];
                    var v3 = verticeIndicies[y + 1][x + 1];
                    var v4 = verticeIndicies[y][x + 1];

                    var uv1 = uvs[y][x];
                    var uv2 = uvs[y + 1][x];
                    var uv3 = uvs[y + 1][x + 1];
                    var uv4 = uvs[y][x + 1];

                    faces.Add(new Face3(v1, v2, v4, na));
                    uv.Add(new UVFaceSet(uv1,uv2,uv4));

                    faces.Add(new Face3(v2, v3, v4, na));
                    uv.Add(new UVFaceSet(uv2,uv3, uv4));
                }
            }

            // top cap
            if (!openEnded && radiusTop > 0)
            {
                vertices.Add(new Vector3(0, heightHalf, 0));

                for (x = 0; x < radialSegments; x++)
                {
                    var v1 = verticeIndicies[0][x];
                    var v2 = verticeIndicies[0][x + 1];
                    var v3 = vertices.Count - 1;
                    var n = Vector3.UnitY;

                    var uv1 = uvs[0][x];
                    var uv2 = uvs[0][x + 1];
                    var uv3 = new Vector2(uv2.x, 0);
                    faces.Add(new Face3(v1, v2, v3, n));
                    uv.Add(new UVFaceSet(uv1, uv2, uv3));
                }
            }

            // bottom cap
            if (!openEnded && radiusBottom > 0)
            {
                vertices.Add(new Vector3(0, -heightHalf, 0));

                for (x = 0; x < radialSegments; x++)
                {
                    var v1 = verticeIndicies[y][x + 1];
                    var v2 = verticeIndicies[y][x];
                    var v3 = vertices.Count - 1;
                    var n = Vector3.UnitNegativeY;
                    var uv1 = uvs[y][x + 1];
                    var uv2 = uvs[y][x];
                    var uv3 = new Vector2(uv2.x, 1);

                    faces.Add(new Face3(v1, v2, v3, n));
                    uv.Add(new UVFaceSet(uv1, uv2, uv3));
                }
            }

            ComputeNormals();
        }
    }
}
