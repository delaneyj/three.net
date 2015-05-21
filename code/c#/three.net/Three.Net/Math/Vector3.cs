using System;

namespace Three.Net.Math
{
    public struct Vector3 : IEquatable<Vector3>
    {
        public static Vector3 Zero = new Vector3(0, 0, 0);
        public static Vector3 One = new Vector3(1, 1, 1);
        public static Vector3 UnitX = new Vector3(1,0,0);
        public static Vector3 UnitY = new Vector3(0,1,0);
        public static Vector3 UnitZ = new Vector3(0,0,1);
        public static Vector3 UnitNegativeX = new Vector3(-1, 0, 0);
        public static Vector3 UnitNegativeY = new Vector3(0, -1, 0);
        public static Vector3 UnitNegativeZ = new Vector3(0, 0, -1);
        public static Vector3 Infinity = new Vector3(float.PositiveInfinity, float.PositiveInfinity, float.PositiveInfinity);
        public static Vector3 NegativeInfinity = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        public float x, y, z;

        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public Vector3(Vector3 v)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = v.z;
        }

        public Vector3(Vector2 v, float z)
        {
            this.x = v.x;
            this.y = v.y;
            this.z = z;
        }

        public float this[int key]
        {
            get
            {
                switch (key)
                {
                    case 0: return x;
                    case 1: return y;
                    case 2: return z;
                    default: throw new Exception("index is out of range: " + key);

                }
            }
            set
            {
                switch (key)
                {
                    case 0: x = value; break;
                    case 1: y = value; break;
                    case 2: z = value; break;
                    default: throw new Exception("index is out of range: " + key);
                }
            }
        }

        public void Add(Vector3 v)
        {
            x += v.x;
            y += v.y;
            z += v.z;
        }

        public void Add(float s)
        {
            x += s;
            y += s;
            z += s;
        }

        public void AddVectors(Vector3 a, Vector3 b)
        {
            x = a.x + b.x;
            y = a.y + b.y;
            z = a.z + b.z;
        }

        public static Vector3 Add(params Vector3[] vectors)
        {
            var res = new Vector3();
            foreach (var v in vectors) res.Add(v);
            return res;
        }

        public void Subtract(Vector3 v)
        {
            x -= v.x;
            y -= v.y;
            z -= v.z;
        }

        public void Subtract(float s)
        {
            x -= s;
            y -= s;
            z -= s;
        }

        public static Vector3 SubtractVectors(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        public void Multiply(Vector3 v)
        {
            x *= v.x;
            y *= v.y;
            z *= v.z;
        }

        public void Multiply(float s)
        {
            x *= s;
            y *= s;
            z *= s;
        }

        public void MultiplyVectors(Vector3 a, Vector3 b)
        {
            x = a.x * b.x;
            y = a.y * b.y;
            z = a.z * b.z;
        }

        public void Divide(Vector3 v)
        {
            this.x /= v.x;
            this.y /= v.y;
            this.z /= v.z;
        }

        public void Divide(float scalar)
        {
            if (scalar != 0)
            {
                var invScalar = 1 / scalar;
                this.x *= invScalar;
                this.y *= invScalar;
                this.z *= invScalar;
            }
            else
            {
                this.x = 0;
                this.y = 0;
                this.z = 0;
            }
        }

        public void ApplyEuler(Euler euler)
        {
            var quaternion = Quaternion.From(euler);
            Apply(quaternion);
        }

        public void ApplyAxisAngle(Vector3 axis, float angle)
        {
            var q = Quaternion.FromAxisAngle(axis, angle);
            Apply(q);
        }

        public void Apply(Matrix3 m)
        {
            var x = this.x;
            var y = this.y;
            var z = this.z;
            var e = m.elements;

            this.x = e[0] * x + e[3] * y + e[6] * z;
            this.y = e[1] * x + e[4] * y + e[7] * z;
            this.z = e[2] * x + e[5] * y + e[8] * z;
        }

        public void Apply(Matrix4 affine)
        {
            float x = this.x, y = this.y, z = this.z;
            var e = affine.elements;
            this.x = e[0] * x + e[4] * y + e[8] * z + e[12];
            this.y = e[1] * x + e[5] * y + e[9] * z + e[13];
            this.z = e[2] * x + e[6] * y + e[10] * z + e[14];
        }

        public void ApplyProjection(Matrix4 projection)
        {
            float x = this.x, y = this.y, z = this.z;
            var e = projection.elements;
            var d = 1 / (e[3] * x + e[7] * y + e[11] * z + e[15]); // perspective divide

            this.x = (e[0] * x + e[4] * y + e[8] * z + e[12]) * d;
            this.y = (e[1] * x + e[5] * y + e[9] * z + e[13]) * d;
            this.z = (e[2] * x + e[6] * y + e[10] * z + e[14]) * d;
        }

        public void Apply(Quaternion q)
        {
            var x = this.x;
            var y = this.y;
            var z = this.z;

            var qx = q.x;
            var qy = q.y;
            var qz = q.z;
            var qw = q.w;

            // calculate quat * vector

            var ix = qw * x + qy * z - qz * y;
            var iy = qw * y + qz * x - qx * z;
            var iz = qw * z + qx * y - qy * x;
            var iw = -qx * x - qy * y - qz * z;

            // calculate result * inverse quat

            this.x = ix * qw + iw * -qx + iy * -qz - iz * -qy;
            this.y = iy * qw + iw * -qy + iz * -qx - ix * -qz;
            this.z = iz * qw + iw * -qz + ix * -qy - iy * -qx;
        }

        public void TransformDirection(Matrix4 affine)
        {
            // vector interpreted as a direction
            float x = this.x, y = this.y, z = this.z;
            var e = affine.elements;
            this.x = e[0] * x + e[4] * y + e[8] * z;
            this.y = e[1] * x + e[5] * y + e[9] * z;
            this.z = e[2] * x + e[6] * y + e[10] * z;
            Normalize();
        }

        public void Min(Vector3 v)
        {
            if (this.x > v.x) this.x = v.x;
            if (this.y > v.y) this.y = v.y;
            if (this.z > v.z) this.z = v.z;
        }

        public void Max(Vector3 v)
        {
            if (this.x < v.x) this.x = v.x;
            if (this.y < v.y) this.y = v.y;
            if (this.z < v.z) this.z = v.z;
        }

        public void Clamp(Vector3 min, Vector3 max)
        {
            // This function assumes min < max, if this assumption isn't true it will not operate correctly
            if (this.x < min.x) this.x = min.x;
            else if (this.x > max.x) this.x = max.x;

            if (this.y < min.y) this.y = min.y;
            else if (this.y > max.y) this.y = max.y;

            if (this.z < min.z) this.z = min.z;
            else if (this.z > max.z) this.z = max.z;
        }

        public void Clamp(float minVal, float maxVal)
        {
            var min = new Vector3(minVal, minVal, minVal);
            var max = new Vector3(maxVal, maxVal, maxVal);
            Clamp(min, max);
        }

        public void Floor()
        {
            this.x = Mathf.Floor(this.x);
            this.y = Mathf.Floor(this.y);
            this.z = Mathf.Floor(this.z);
        }

        public void Ceiling()
        {
            this.x = Mathf.Ceiling(this.x);
            this.y = Mathf.Ceiling(this.y);
            this.z = Mathf.Ceiling(this.z);
        }

        public void Round()
        {
            this.x = Mathf.Round(this.x);
            this.y = Mathf.Round(this.y);
            this.z = Mathf.Round(this.z);
        }

        public void RoundToZero()
        {
            this.x = (this.x < 0) ? Mathf.Ceiling(this.x) : Mathf.Floor(this.x);
            this.y = (this.y < 0) ? Mathf.Ceiling(this.y) : Mathf.Floor(this.y);
            this.z = (this.z < 0) ? Mathf.Ceiling(this.z) : Mathf.Floor(this.z);
        }

        public void Negate()
        {
            this.x = -this.x;
            this.y = -this.y;
            this.z = -this.z;
        }

        public float Dot(Vector3 v)
        {
            return this.x * v.x + this.y * v.y + this.z * v.z;
        }

        public float LengthSquared()
        {
            return this.x * this.x + this.y * this.y + this.z * this.z;
        }

        public float Length()
        {
            return Mathf.Sqrt(LengthSquared());
        }

        public float LengthManhattan()
        {
            return Mathf.Abs(this.x) + Mathf.Abs(this.y) + Mathf.Abs(this.z);
        }

        public void Normalize()
        {
            Divide(Length());
        }

        public void SetLength(float l)
        {
            var oldLength = Length();

            if (oldLength != 0 && l != oldLength) Multiply(l / oldLength);
        }

        public void Lerp(Vector3 v, float alpha)
        {
            x += (v.x - x) * alpha;
            y += (v.y - y) * alpha;
            z += (v.z - z) * alpha;
        }

        public void Cross(Vector3 v)
        {
            float x = this.x, y = this.y, z = this.z;
            this.x = y * v.z - z * v.y;
            this.y = z * v.x - x * v.z;
            this.z = x * v.y - y * v.x;
        }

        public static Vector3 CrossVectors(Vector3 a, Vector3 b)
        {
            float ax = a.x, ay = a.y, az = a.z;
            float bx = b.x, by = b.y, bz = b.z;
            return new Vector3(ay * bz - az * by, az * bx - ax * bz, ax * by - ay * bx);
        }

        public void ProjectOnVector(Vector3 vector)
        {
            var v1 = vector;
            v1.Normalize();
            var dot = Dot(v1);
            x = v1.x;
            y = v1.y;
            z = v1.z;
            Multiply(dot);
        }

        public void ProjectOnPlane(Vector3 planeNormal)
        {
            var v1 = new Vector3(this);
            v1.ProjectOnVector(planeNormal);
            Subtract(v1);
        }

        public void Reflect(Vector3 normal)
        {
            // reflect incident vector off plane orthogonal to normal normal is assumed to have unit length
            var v1 = normal;
            v1.Multiply(2 * Dot(normal));
            Subtract(v1);
        }

        public float AngleTo(Vector3 v)
        {
            var theta = Dot(v) / (Length() * v.Length());

            // clamp, to handle numerical problems
            return Mathf.Acos(Mathf.Clamp(theta, -1, 1));
        }

        public float DistanceTo(Vector3 v)
        {
            return Mathf.Sqrt(DistanceToSquared(v));
        }

        public float DistanceToSquared(Vector3 v)
        {
            var dx = this.x - v.x;
            var dy = this.y - v.y;
            var dz = this.z - v.z;
            return dx * dx + dy * dy + dz * dz;
        }

        public static Vector3 FromPosition(Matrix4 m)
        {
            return new Vector3(m.elements[12],m.elements[13],m.elements[14]);
        }

        public static Vector3 FromScale(Matrix4 m)
        {
            var x = new Vector3(m.elements[0],m.elements[1],m.elements[2]);
            var sx = x.Length();
            
            var y = new Vector3(m.elements[4], m.elements[5], m.elements[6]);
            var sy = y.Length();

            var z = new Vector3(m.elements[8], m.elements[9], m.elements[10]);
            var sz = z.Length();

            return new Vector3(sx,sy,sz);
        }

        public static Vector3 FromColumn(int index, Matrix4 matrix)
        {
            var offset = index * 4;
            var me = matrix.elements;
            return new Vector3(me[offset],me[offset + 1],me[offset + 2]);
        }

        public void FromArray(float[] array)
        {
            this.x = array[0];
            this.y = array[1];
            this.z = array[2];
        }

        public float[] ToArray()
        {
            return new float[] { this.x, this.y, this.z };
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() << 8 ^ z.GetHashCode() << 16;
        }

        public override bool Equals(object obj)
        {
            var v = (Vector3)obj;
            if (v == null) return false;
            return Equals(v);
        }

        public bool Equals(Vector3 v)
        {
            return ((v.x == this.x) && (v.y == this.y) && (v.z == this.z));
        }

        public static bool operator ==(Vector3 left, Vector3 right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Vector3 left, Vector3 right)
        {
            return !left.Equals(right);
        }

        public Vector3 Normalized()
        {
            var v = new Vector3(this);
            v.Normalize();
            return v;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2}", x, y, z);
        }
    }
}
