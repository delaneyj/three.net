using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;

namespace Three.Net.Math
{
    public class Frustum
    {
        public static Frustum Empty = new Frustum(Plane.Empty,Plane.Empty,Plane.Empty,Plane.Empty,Plane.Empty,Plane.Empty);

        private Plane[] planes;

        public Frustum(Plane p0, Plane p1, Plane p2, Plane p3, Plane p4, Plane p5)
        {
            planes = new Plane[]
            {
                p0,
                p1,
                p2,
                p3,
                p4,
                p5,
            };
        }        

        public static Frustum  FromMatrix(Matrix4 projectionScreen)
        {
            var me = projectionScreen.elements;
            float me0 = me[0], me1 = me[1], me2 = me[2], me3 = me[3];
            float me4 = me[4], me5 = me[5], me6 = me[6], me7 = me[7];
            float me8 = me[8], me9 = me[9], me10 = me[10], me11 = me[11];
            float me12 = me[12], me13 = me[13], me14 = me[14], me15 = me[15];

            var f = new Frustum(
                new Plane(me3 - me0, me7 - me4, me11 - me8, me15 - me12),
                new Plane(me3 + me0, me7 + me4, me11 + me8, me15 + me12),
                new Plane(me3 + me1, me7 + me5, me11 + me9, me15 + me13),
                new Plane(me3 - me1, me7 - me5, me11 - me9, me15 - me13),
                new Plane(me3 - me2, me7 - me6, me11 - me10, me15 - me14),
                new Plane(me3 + me2, me7 + me6, me11 + me10, me15 + me14));
            foreach(var p in f.planes) p.Normalize();            
            return f;
        }

        public bool IntersectsObject(Object3D o)
        {
            var geometry = o.geometry;
            if (geometry.BoundingSphere == Sphere.Empty) geometry.ComputeBoundingSphere();
            var sphere = geometry.BoundingSphere;
            sphere.Apply(o.matrixWorld);
            return IntersectsSphere(sphere);
        }

        private bool IntersectsSphere(Sphere sphere)
        {
            var center = sphere.Center;
            var negRadius = -sphere.Radius;

            foreach(var plane in planes)
            {
                var distance = plane.DistanceToPoint(center);
                if (distance < negRadius) return false;
            }

            return true;

        }
    }
}
