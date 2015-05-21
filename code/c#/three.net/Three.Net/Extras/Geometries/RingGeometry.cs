using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class RingGeometry : Geometry
    {
        public RingGeometry(float innerRadius = 0.25f, float outerRadius = 1, int thetaSegments = 8, int phiSegments = 8, float thetaStart = 0, float thetaLength = Mathf.Tau)
        {
            thetaSegments = Mathf.Max(3, thetaSegments);
            phiSegments = Mathf.Max(1, phiSegments);

            var radius = innerRadius;
            var radiusStep = ((outerRadius - innerRadius) / phiSegments);
            var uvs = new List<Vector2>();

            for (var i = 0; i < phiSegments + 1; i++)
            { // concentric circles inside ring
                for (var o = 0f; o < thetaSegments + 1; o++)
                { // number of segments per circle
                    var segment = thetaStart + o / thetaSegments * thetaLength;
                    var v = new Vector3(Mathf.Cos(segment), Mathf.Sin(segment), 0);
                    v.Multiply(radius);
                    vertices.Add(v);
                    uvs.Add(new Vector2((v.x / outerRadius + 1) / 2, (v.y / outerRadius + 1) / 2));
                }

                radius += radiusStep;
            }

            var n = Vector3.UnitZ;

            for (var i = 0; i < phiSegments; i++)
            { // concentric circles inside ring
                var thetaSegment = i * (thetaSegments + 1);
                for (var o = 0; o < thetaSegments; o++)
                { // number of segments per circle
                    var segment = o + thetaSegment;
                    var v1 = segment;
                    var v2 = segment + thetaSegments + 1;
                    var v3 = segment + thetaSegments + 2;

                    faces.Add(new Face3(v1, v2, v3, n));
                    faceVertexUvs[0].Add(new UVFaceSet(uvs[v1], uvs[v2], uvs[v3]));

                    v1 = segment;
                    v2 = segment + thetaSegments + 2;
                    v3 = segment + 1;

                    faces.Add(new Face3(v1, v2, v3, n));
                    faceVertexUvs[0].Add(new UVFaceSet(uvs[v1], uvs[v2], uvs[v3]));

                }
            }

            //ComputeFaceNormals();
            BoundingSphere = new Sphere(Vector3.Zero, radius);
        }
    }
}
