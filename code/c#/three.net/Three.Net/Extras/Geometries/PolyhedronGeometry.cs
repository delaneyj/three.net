using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class PolyhedronGeometry : Geometry
    {
        private class VertexInfo
        {
            public Vector3 Position;
            public int Index;
            public Vector2 UV;
        }



        public PolyhedronGeometry(Vector3[] intialVertices, int[] indices, float radius = 1, int detail = 0)
        {
            var vertexSet = new List<VertexInfo>();

            foreach (var v in intialVertices) Prepare(v, vertexSet);

            var initialFaces = new List<Face3>();
            for (int i = 0; i < indices.Length; i += 3)
            {
                var i1 = indices[i];
                var i2 = indices[i + 1];
                var i3 = indices[i + 2];
                var v1 = vertexSet[i1].Position;
                var v2 = vertexSet[i2].Position;
                var v3 = vertexSet[i3].Position;
                initialFaces.Add(new Face3(i1, i2, i3, v1, v2, v3));
            }

            var centroid = Vector3.Zero;

            foreach(var f in initialFaces) Subdivide(vertexSet, f, detail);
            

            // Handle case when face straddles the seam
            foreach (var uvs in faceVertexUvs[0])
            {
                var x0 = uvs.A.x;
                var x1 = uvs.B.x;
                var x2 = uvs.C.x;

                var max = Mathf.Max(x0, Mathf.Max(x1, x2));
                var min = Mathf.Min(x0, Mathf.Min(x1, x2));

                if (max > 0.9f && min < 0.1f)
                { // 0.9 is somewhat arbitrary
                    if (x0 < 0.2f) uvs.A.x += 1;
                    if (x1 < 0.2f) uvs.B.x += 1;
                    if (x2 < 0.2f) uvs.C.x += 1;
                }
            }


            // Apply radius
            for (var i = 0; i < vertexSet.Count; i++)
            {
                var v = vertexSet[i].Position;
                v.Multiply(radius);
                vertices.Add(v);

                //vertices[i] = v;
            }

            MergeVertices();
            ComputeNormals();
            BoundingSphere = new Sphere(Vector3.Zero, radius);

        }

        // Project vector onto sphere's surface
        private VertexInfo Prepare(Vector3 vector, List<VertexInfo> vertexSet)
        {
            var p = vector.Normalized();
            var i = vertexSet.Count;
            var u = Azimuth(vector) / 2 / Mathf.Pi + 0.5f;
            var v = Inclination(vector) / Mathf.Pi + 0.5f;

            var vertex = new VertexInfo()
            {
                Position = p,
                Index = i,
                // Texture coords are equivalent to map coords, calculate angle and convert to fraction of a circle.
                UV = new Vector2(u, 1 - v)
            };
            vertexSet.Add(vertex);
            return vertex;
        }


        // Approximate a curved face with recursively sub-divided triangles.
        private void Make(VertexInfo v1, VertexInfo v2, VertexInfo v3)
        {
            var face = new Face3(v1.Index, v2.Index, v3.Index, v1.Position, v2.Position, v3.Position);
            faces.Add(face);

            var centroid = v1.Position;
            centroid.Add(v2.Position);
            centroid.Add(v3.Position);
            centroid.Divide(3);

            var azi = Azimuth(centroid);
            var uvA = CorrectUV(v1.UV, v1.Position, azi);
            var uvB = CorrectUV(v2.UV, v2.Position, azi);
            var uvC = CorrectUV(v3.UV, v3.Position, azi);
            var faceSet = new UVFaceSet(uvA,uvB,uvC);
            var uvs = faceVertexUvs[0];
            uvs.Add(faceSet);
        }

        // Analytically subdivide a face to the required detail level.
        private void Subdivide(List<VertexInfo> vertexSet, Face3 face, int detail)
        {
            var cols = Mathf.Pow(2, detail);
            var cells = Mathf.Pow(4, detail);
            var a = Prepare(vertexSet[face.A].Position, vertexSet);
            var b = Prepare(vertexSet[face.B].Position, vertexSet);
            var c = Prepare(vertexSet[face.C].Position, vertexSet);
            var v = new List<List<VertexInfo>>();

            // Construct all of the vertices for this subdivision.

            for (var i = 0; i <= cols; i++)
            {
                v.Add(new List<VertexInfo>());

                var offset = i / (float)cols;

                var aV = a.Position;
                aV.Lerp(c.Position, offset);
                var aj = Prepare(aV, vertexSet);

                var bV = b.Position;
                bV.Lerp(c.Position, offset);
                var bj = Prepare(bV, vertexSet);

                var rows = cols - i;

                for (var j = 0; j <= rows; j++)
                {

                    if (j == 0 && i == cols)
                    {
                        v[i].Add(aj);
                    }
                    else
                    {
                        var ajV = aj.Position;
                        ajV.Lerp(bj.Position, j / (float)rows);
                        v[i].Add(Prepare(ajV, vertexSet));
                    }
                }
            }

            // Construct all of the faces.
            for (var i = 0; i < cols; i++)
            {
                for (var j = 0; j < 2 * (cols - i) - 1; j++)
                {
                    var k = Mathf.Floor(j / 2);
                    if (j % 2 == 0)
                    {
                        Make(
                            v[i][k + 1],
                            v[i + 1][k],
                            v[i][k]
                        );
                    }
                    else
                    {
                        Make(
                            v[i][k + 1],
                            v[i + 1][k + 1],
                            v[i + 1][k]
                        );
                    }
                }
            }
        }

        // Angle around the Y axis, counter-clockwise when looking from above.
        private float Azimuth(Vector3 vector)
        {
            return Mathf.Atan2(vector.z, -vector.x);
        }

        // Angle above the XZ plane.
        private float Inclination(Vector3 vector)
        {
            return Mathf.Atan2(-vector.y, Mathf.Sqrt((vector.x * vector.x) + (vector.z * vector.z)));
        }

        // Texture fixing helper. Spheres have some odd behaviours.
        private Vector2 CorrectUV(Vector2 uv, Vector3 vector, float azimuth)
        {

            if ((azimuth < 0) && (uv.x == 1)) uv = new Vector2(uv.x - 1, uv.y);
            if ((vector.x == 0) && (vector.z == 0)) uv = new Vector2(azimuth / 2 / Mathf.Pi + 0.5f, uv.y);
            return uv;
        }
    }
}