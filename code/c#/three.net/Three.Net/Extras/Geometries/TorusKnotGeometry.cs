using System.Collections.Generic;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class TorusKnotGeometry : Geometry
    {
        public TorusKnotGeometry(float radius = 1, float tubeDiameter = 0.4f, int radialSegments = 64, int tubularSegments = 8, int p = 2, int q = 3, float heightScale = 1)
        {
            var grid = new List<List<int>>();
            var tang = Vector3.Zero;
            var n = Vector3.Zero;
            var bitan = Vector3.Zero;

            for (var i = 0f; i < radialSegments; ++i)
            {
                var row = new List<int>();
                grid.Add(row);
                var u = i / radialSegments * p * Mathf.Tau;
                var p1 = GetPosition(u, q, p, radius, heightScale);
                var p2 = GetPosition(u + 0.01f, q, p, radius, heightScale);
                tang = Vector3.SubtractVectors(p2, p1);
                n.AddVectors(p2, p1);

                bitan = Vector3.CrossVectors(tang, n);
                n = Vector3.CrossVectors(bitan, tang);
                bitan.Normalize();
                n.Normalize();

                for (var j = 0f; j < tubularSegments; ++j)
                {
                    var v = j / tubularSegments * Mathf.Tau;
                    var cx = -tubeDiameter * Mathf.Cos(v); // TODO: Hack: Negating it so it faces outside.
                    var cy = tubeDiameter * Mathf.Sin(v);

                    var pos = new Vector3(p1.x + cx * n.x + cy * bitan.x,
                                          p1.y + cx * n.y + cy * bitan.y,
                                          p1.z + cx * n.z + cy * bitan.z);
                    row.Add(vertices.Count);
                    vertices.Add(pos);
                }
            }

            for (var i = 0; i < radialSegments; ++i)
            {
                for (var j = 0; j < tubularSegments; ++j)
                {
                    var ip = (i + 1) % radialSegments;
                    var jp = (j + 1) % tubularSegments;

                    var a = grid[i][j];
                    var b = grid[ip][j];
                    var c = grid[ip][jp];
                    var d = grid[i][jp];

                    var uva = new Vector2(i / (float)radialSegments, j / (float)tubularSegments);
                    var uvb = new Vector2((i + 1) / (float)radialSegments, j / (float)tubularSegments);
                    var uvc = new Vector2((i + 1) / (float)radialSegments, (j + 1) / (float)tubularSegments);
                    var uvd = new Vector2(i / (float)radialSegments, (j + 1) / (float)tubularSegments);

                    faces.Add(new Face3(a, b, d));
                    faceVertexUvs[0].Add(new UVFaceSet(uva, uvb, uvd));
                    faces.Add(new Face3(b, c, d));
                    faceVertexUvs[0].Add(new UVFaceSet(uvb, uvc, uvd));
                }
            }

            ComputeNormals();
        }

        private Vector3 GetPosition(float u, int q, int p, float radius, float heightScale)
        {
            var cu = Mathf.Cos(u);
            var su = Mathf.Sin(u);
            var quOverP = q / (float)p * u;
            var cs = Mathf.Cos(quOverP);

            var tx = radius * (2 + cs) * 0.5f * cu;
            var ty = radius * (2 + cs) * su * 0.5f;
            var tz = heightScale * radius * Mathf.Sin(quOverP) * 0.5f;

            return new Vector3(tx, ty, tz);
        }
    }
}
