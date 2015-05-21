using System.Collections.Generic;
using System.Diagnostics;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Core
{
    public class IntersectionInfo
    {
        public readonly float Distance;
        public readonly Vector3 IntersectionPoint;
        public readonly Face3 Face;
        public readonly int FaceIndex;
        public readonly Object3D Object;

        public IntersectionInfo(float distance, Vector3 intersectionPoint, Face3 face, int faceIndex, Object3D o)
        {
            Distance = distance;
            IntersectionPoint = intersectionPoint;
            Face = face;
            FaceIndex = faceIndex;
            Object = o;
        }
    }

    public class Raycaster
    {
        internal Ray ray;
        public readonly float Near, Far;

        public Raycaster(Vector3 origin, Vector3 direction, float near = 0, float far = float.PositiveInfinity)
        {
            ray = new Ray(origin, direction);
            Near = near;
            Far = far;
        }

        public Raycaster() : this(Vector3.Zero,Vector3.Zero){}

        public IEnumerable<IntersectionInfo> IntersectObject(Object3D o, bool recursive)
        {
            List<IntersectionInfo> intersects = new List<IntersectionInfo>();

            o.Raycast(this, intersects);

            if (recursive)
            {
                foreach (var c in o.Children)
                {
                    var subresults = IntersectObject(c, true);

                    if (subresults != null)
                    {
                        intersects.AddRange(subresults);
                    }
                }
            }

            intersects.Sort(descendingDistanceSort);
            return intersects;
        }

        private int descendingDistanceSort(IntersectionInfo a, IntersectionInfo b)
        {
            return a.Distance.CompareTo(b.Distance);
        }

        public void Set(Vector3 origin, Vector3 direction)
        {
            ray.origin = origin;
            ray.direction = direction;
        }

        public IList<IntersectionInfo> IntersectObjects(IEnumerable<Object3D> objects, bool recursive = false)
        {
            List<IntersectionInfo> intersects = null;

            foreach(var o in objects)
            {
                var subintersects = IntersectObject(o, recursive);
                if (subintersects != null)
                {
                    if (intersects == null) intersects = new List<IntersectionInfo>();
                    intersects.AddRange(subintersects);
                }
            }

            intersects.Sort(descendingDistanceSort);
            return intersects;
        }
    }
}
