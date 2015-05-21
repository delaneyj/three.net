using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class PlaneGeometry : Geometry
    {
        public PlaneGeometry(float width = 2, float height = 2, int widthSegments = 1, int heightSegments = 1)
        {
            var widthHalf = width / 2;
            var heightHalf = height / 2;
            var gridX = widthSegments;
            var gridZ = heightSegments;
            var gridX1 = gridX + 1;
            var gridZ1 = gridZ + 1;
            var segmentWidth = width / (float)gridX;
            var segmentHeight = height / (float)gridZ;
            var normal = Vector3.UnitZ;

            for (var iz = 0; iz < gridZ1; iz++)
            {
                var y = iz * segmentHeight - heightHalf;
                for (var ix = 0; ix < gridX1; ix++)
                {
                    var x = ix * segmentWidth - widthHalf;
                    vertices.Add(new Vector3(x, -y, 0));
                }
            }

            for (var iz = 0; iz < gridZ; iz++)
            {
                for (var ix = 0; ix < gridX; ix++)
                {
                    var a = ix + gridX1 * iz;
                    var b = ix + gridX1 * (iz + 1);
                    var c = (ix + 1) + gridX1 * (iz + 1);
                    var d = (ix + 1) + gridX1 * iz;

                    var uva = new Vector2(ix / gridX, 1 - iz / gridZ);
                    var uvb = new Vector2(ix / gridX, 1 - (iz + 1) / gridZ);
                    var uvc = new Vector2((ix + 1) / gridX, 1 - (iz + 1) / gridZ);
                    var uvd = new Vector2((ix + 1) / gridX, 1 - iz / gridZ);

                    faces.Add(new Face3(a, b, d, normal));
                    faceVertexUvs[0].Add(new UVFaceSet(uva, uvb, uvd));

                    faces.Add(new Face3(b, c, d, normal));
                    faceVertexUvs[0].Add(new UVFaceSet(uvb, uvc, uvd));

                }
            }
        }
    }
}
