using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class BoxGeometry : Geometry
    {
        public readonly int WidthSegments, HeightSegments, DepthSegments;
        private enum Direction { x, y, z, UNKNOWN }

        public BoxGeometry(float width = 2, float height = 2, float depth = 2, int widthSegments = 1, int heightSegments = 1, int depthSegments = 1)
        {
            WidthSegments = widthSegments;
            HeightSegments = heightSegments;
            DepthSegments = depthSegments;

            var width_half = width / 2;
            var height_half = height / 2;
            var depth_half = depth / 2;

            BuildPlane(Direction.z, Direction.y, -1, -1, depth, height, width_half, 0); // px
            BuildPlane(Direction.z, Direction.y, 1, -1, depth, height, -width_half, 1); // nx
            BuildPlane(Direction.x, Direction.z, 1, 1, width, depth, height_half, 2); // py
            BuildPlane(Direction.x, Direction.z, 1, -1, width, depth, -height_half, 3); // ny
            BuildPlane(Direction.x, Direction.y, 1, -1, width, height, depth_half, 4); // pz
            BuildPlane(Direction.x, Direction.y, -1, -1, width, height, -depth_half, 5); // nz

            MergeVertices();
            ComputeNormals();
        }

        private void BuildPlane(Direction u, Direction v, int udir, int vdir, float width, float height, float depth, int materialIndex)
        {
            var gridX = WidthSegments;
            var gridY = HeightSegments;

            var width_half = width / 2;
            var height_half = height / 2;
            var offset = vertices.Count;
            var w = Direction.UNKNOWN;

            if ((u == Direction.x && v == Direction.y) || (u == Direction.y && v == Direction.x))
            {
                w = Direction.z;
            }
            else if ((u == Direction.x && v == Direction.z) || (u == Direction.z && v == Direction.x))
            {
                w = Direction.y;
                gridY = DepthSegments;
            }
            else if ((u == Direction.z && v == Direction.y) || (u == Direction.y && v == Direction.z))
            {
                w = Direction.x;
                gridX = DepthSegments;
            }

            var gridX1 = gridX + 1;
            var gridY1 = gridY + 1;
            var segment_width = width / gridX;
            var segment_height = height / gridY;
            var normal = new Vector3();

            normal[(int)w] = depth > 0 ? 1 : -1;

            for (var iy = 0; iy < gridY1; iy++)
            {
                for (var ix = 0; ix < gridX1; ix++)
                {
                    var vector = new Vector3();
                    vector[(int)u] = (ix * segment_width - width_half) * udir;
                    vector[(int)v] = (iy * segment_height - height_half) * vdir;
                    vector[(int)w] = depth;
                    vertices.Add(vector);
                }

            }

            var uvs = faceVertexUvs[0];
            for (var iy = 0; iy < gridY; iy++)
            {
                for (var ix = 0; ix < gridX; ix++)
                {
                    var a = ix + gridX1 * iy;
                    var b = ix + gridX1 * (iy + 1);
                    var c = (ix + 1) + gridX1 * (iy + 1);
                    var d = (ix + 1) + gridX1 * iy;

                    var uva = new Vector2(ix / gridX, 1 - iy / gridY);
                    var uvb = new Vector2(ix / gridX, 1 - (iy + 1) / gridY);
                    var uvc = new Vector2((ix + 1) / gridX, 1 - (iy + 1) / gridY);
                    var uvd = new Vector2((ix + 1) / gridX, 1 - iy / gridY);

                    faces.Add(new Face3(a + offset, b + offset, d + offset, normal, normal, normal));
                    faces.Add(new Face3(b + offset, c + offset, d + offset, normal, normal, normal));

                    
                    uvs.Add(new UVFaceSet(uva, uvb, uvd));
                    uvs.Add(new UVFaceSet(uvb, uvc, uvd));
                }
            }
        }
    }
}
