using System;
using System.Collections.Generic;
using Three.Net.Core;

namespace Three.Net.Math
{
    public struct Box3 : IEquatable<Box3>
    {
        public static Box3 Empty = new Box3(Vector3.Infinity, Vector3.NegativeInfinity);

        internal Vector3 min, max;

        public Box3(Vector3 min, Vector3 max)
        {
            this.min = min;
            this.max = max;
        }


        public static Box3 FromPoints(List<Vector3> points)
        {
            var b = Box3.Empty;
            foreach (var p in points) b.ExpandByPoint(p);
            return b;
        }

        public static Box3 FromCenterAndSize(Vector3 center, Vector3 size)
        {
            var b = Box3.Empty;
            var halfSize = size;
            halfSize.Multiply(0.5f);

            b.min = Vector3.SubtractVectors(center, halfSize);
            b.max.AddVectors(center, halfSize);
            return b;
        }

        public static Box3 FromObject(Object3D o)
        {
            // Computes the world-axis-aligned bounding box of an object (including its children),
            // accounting for both the object's, and childrens', world transforms
            o.UpdateMatrixWorld(true);
            var b = Box3.Empty;
            o.Traverse(child =>
             {
                 if (child.geometry != null && child.geometry.vertices.Count > 0)
                {
                    foreach (var v in child.geometry.vertices)
                    {
                        var v1 = v;
                        v1.Apply(child.matrixWorld);
                        b.ExpandByPoint(v1);
                    }
                }
            });
            return b;
        }

        public bool IsEmpty()
        {
            // this is a more robust check for empty than ( volume <= 0 ) because volume can get positive with two negative axes
            return (max.x < min.x) || (max.y < min.y) || (max.z < min.z);
        }

        public Vector3 Center()
        {
            var result = Vector3.Zero;
            result.AddVectors(min, max);
            result.Multiply(0.5f);
            return result;
        }

        public Vector3 Size()
        {
            var result = Vector3.Zero;
            result = Vector3.SubtractVectors(max, min);
            return result;
        }

        public void ExpandByPoint(Vector3 point)
        {
            min.Min(point);
            max.Max(point);
        }

        public void ExpandByVector(Vector3 vector)
        {
            min.Subtract(vector);
            max.Add(vector);
        }

        public void ExpandByScalar(float scalar)
        {
            min.Add(-scalar);
            max.Add(scalar);
        }

        public bool ContainsPoint(Vector3 point)
        {
            if (point.x < this.min.x || point.x > this.max.x ||
             point.y < this.min.y || point.y > this.max.y ||
             point.z < this.min.z || point.z > this.max.z)
            {
                return false;
            }

            return true;
        }

        public bool ContainsBox(Box3 box)
        {

            if ((this.min.x <= box.min.x) && (box.max.x <= this.max.x) &&
                 (this.min.y <= box.min.y) && (box.max.y <= this.max.y) &&
                 (this.min.z <= box.min.z) && (box.max.z <= this.max.z))
            {

                return true;

            }

            return false;
        }

        //Returns point as a proportion of this box's width and height.
        public Vector3 GetParameter(Vector3 point)
        {
            // This can potentially have a divide by zero if the box
            // has a size dimension of 0.
            var x = (point.x - this.min.x) / (this.max.x - this.min.x);
            var y = (point.y - this.min.y) / (this.max.y - this.min.y);
            var z = (point.z - this.min.z) / (this.max.z - this.min.z);
            return new Vector3(x,y,z);
        }

        public bool IsIntersectionBox(Box3 box)
        {

            // using 6 splitting planes to rule out intersections.

            if (box.max.x < this.min.x || box.min.x > this.max.x ||
                 box.max.y < this.min.y || box.min.y > this.max.y ||
                 box.max.z < this.min.z || box.min.z > this.max.z)
            {

                return false;

            }

            return true;

        }

        public Vector3 ClampPoint(Vector3 point)
        {
            var result = point;
            result.Clamp(min, max);
            return result;
        }

        public float DistanceToPoint(Vector3 point)
        {
            var result = point;
            result.Clamp(min, max);
            result.Subtract(point);
            return result.Length();
        }

        public Sphere GetBoundingSphere()
        {
            return new Sphere(Center(), Size().Length() * 0.5f);
        }

        public void Intersect(Box3 box)
        {
            min.Max(box.min);
            max.Min(box.max);
        }

        public void Union(Box3 box)
        {
            min.Min(box.min);
            max.Max(box.max);
        }

        public void Apply(Matrix4 m)
        {
            // NOTE: I am using a binary pattern to specify all 2^3 combinations below
            var points = new List<Vector3>()
                {
			new Vector3( min.x, min.y, min.z ), // 000
			new Vector3( min.x, min.y, max.z ), // 001
			new Vector3( min.x, max.y, min.z ), // 010
			new Vector3( min.x, max.y, max.z ), // 011
			new Vector3( max.x, min.y, min.z ), // 100
			new Vector3( max.x, min.y, max.z ), // 101
			new Vector3( max.x, max.y, min.z ), // 110
			new Vector3( max.x, max.y, max.z ),  // 111
    };

            foreach (var p in points) p.Apply(m);
            var b = Box3.FromPoints(points);
            min = b.min;
            max = b.max;
        }

        public void Translate(Vector3 offset)
        {
            min.Add(offset);
            max.Add(offset);
        }


        public bool Equals(Box3 box)
        {
            return box.min == min && box.max == max;
        }

        public override int GetHashCode()
        {
            return min.GetHashCode() ^ max.GetHashCode() << 16;
        }

        public override bool Equals(object obj)
        {
            var b = (Box3)obj;
            if (b == null) return false;
            return Equals(b);
        }

        public static bool operator ==(Box3 left, Box3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Box3 left, Box3 right)
        {
            return !left.Equals(right);
        }
    }
}
