using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public struct Vector4 : IEquatable<Vector4>
    {
        public float x, y, z, w;

        public Vector4(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public void Set(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
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
                    case 3: return w;
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
                    case 3: w = value; break;
                    default: throw new Exception("index is out of range: " + key);
                }
            }
        }

        public void Set(Vector4 v)
        {

            x = v.x;
            y = v.y;
            z = v.z;
            w = v.w;
        }

        public void Add(Vector4 v)
        {
            this.x += v.x;
            this.y += v.y;
            this.z += v.z;
            this.w += v.w;
        }

        public void Add(float s)
        {
            x += s;
            y += s;
            z += s;
            w += s;
        }

        public void AddVectors(Vector4 a, Vector4 b)
        {
            x = a.x + b.x;
            y = a.y + b.y;
            z = a.z + b.z;
            w = a.w + b.w;
        }

        public void Subtract(Vector4 v)
        {
            x -= v.x;
            y -= v.y;
            z -= v.z;
            w -= v.w;
        }

        public void SubtractVectors(Vector4 a, Vector4 b)
        {
            x = a.x - b.x;
            y = a.y - b.y;
            z = a.z - b.z;
            w = a.w - b.w;
        }

        public void Multiply(float scalar)
        {
            x *= scalar;
            y *= scalar;
            z *= scalar;
            w *= scalar;
        }

        public void Apply(Matrix4 m)
        {
            var x = this.x;
            var y = this.y;
            var z = this.z;
            var w = this.w;
            var e = m.elements;
            this.x = e[0] * x + e[4] * y + e[8] * z + e[12] * w;
            this.y = e[1] * x + e[5] * y + e[9] * z + e[13] * w;
            this.z = e[2] * x + e[6] * y + e[10] * z + e[14] * w;
            this.w = e[3] * x + e[7] * y + e[11] * z + e[15] * w;
        }

        public void Divide(float scalar)
        {
            if (scalar != 0)
            {
                var invScalar = 1 / scalar;
                x *= invScalar;
                y *= invScalar;
                z *= invScalar;
                w *= invScalar;
            }
            else
            {
                x = 0;
                y = 0;
                z = 0;
                w = 1;
            }
        }

        public void SetAxisAngleFromQuaternion(Quaternion q)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/quaternionToAngle/index.htm
            // q is assumed to be normalized

            w = 2 * Mathf.Acos(q.w);

            var s = Mathf.Sqrt(1 - q.w * q.w);

            if (s < 0.0001f)
            {
                x = 1;
                y = 0;
                z = 0;
            }
            else
            {
                x = q.x / s;
                y = q.y / s;
                z = q.z / s;
            }
        }

        public void SetAxisAngleFromRotationMatrix(Matrix4 m)
        {

            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToAngle/index.htm

            // assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)

            float angle, x, y, z,		// variables for result
                epsilon = 0.01f,		// margin to allow for rounding errors
                epsilon2 = 0.1f;		// margin to distinguish between 0 and 180 degrees

            var te = m.elements;

            float m11 = te[0], m12 = te[4], m13 = te[8];
            float m21 = te[1], m22 = te[5], m23 = te[9];
            float m31 = te[2], m32 = te[6], m33 = te[10];

            if ((Mathf.Abs(m12 - m21) < epsilon)
               && (Mathf.Abs(m13 - m31) < epsilon)
               && (Mathf.Abs(m23 - m32) < epsilon))
            {

                // singularity found
                // first check for identity matrix which must have +1 for all terms
                // in leading diagonal and zero in other terms

                if ((Mathf.Abs(m12 + m21) < epsilon2)
                   && (Mathf.Abs(m13 + m31) < epsilon2)
                   && (Mathf.Abs(m23 + m32) < epsilon2)
                   && (Mathf.Abs(m11 + m22 + m33 - 3) < epsilon2))
                {

                    // this singularity is identity matrix so angle = 0

                    Set(1, 0, 0, 0);
                    return; // zero angle, arbitrary axis
                }

                // otherwise this singularity is angle = 180
                angle = Mathf.Pi;
                var xx = (m11 + 1) / 2;
                var yy = (m22 + 1) / 2;
                var zz = (m33 + 1) / 2;
                var xy = (m12 + m21) / 4;
                var xz = (m13 + m31) / 4;
                var yz = (m23 + m32) / 4;

                if ((xx > yy) && (xx > zz))
                { // m11 is the largest diagonal term

                    if (xx < epsilon)
                    {

                        x = 0;
                        y = 0.707106781f;
                        z = 0.707106781f;

                    }
                    else
                    {

                        x = Mathf.Sqrt(xx);
                        y = xy / x;
                        z = xz / x;
                    }

                }
                else if (yy > zz)
                { // m22 is the largest diagonal term

                    if (yy < epsilon)
                    {

                        x = 0.707106781f;
                        y = 0;
                        z = 0.707106781f;

                    }
                    else
                    {

                        y = Mathf.Sqrt(yy);
                        x = xy / y;
                        z = yz / y;

                    }

                }
                else
                { // m33 is the largest diagonal term so base result on this

                    if (zz < epsilon)
                    {

                        x = 0.707106781f;
                        y = 0.707106781f;
                        z = 0;

                    }
                    else
                    {

                        z = Mathf.Sqrt(zz);
                        x = xz / z;
                        y = yz / z;

                    }

                }

                Set(x, y, z, angle);
                return; // return 180 deg rotation
            }

            // as we have reached here there are no singularities so we can handle normally

            var s = Mathf.Sqrt((m32 - m23) * (m32 - m23)
                              + (m13 - m31) * (m13 - m31)
                              + (m21 - m12) * (m21 - m12)); // used to normalize

            if (Mathf.Abs(s) < 0.001f) s = 1;

            // prevent divide by zero, should not happen if matrix is orthogonal and should be
            // caught by singularity test above, but I've left it in just in case

            this.x = (m32 - m23) / s;
            this.y = (m13 - m31) / s;
            this.z = (m21 - m12) / s;
            this.w = Mathf.Acos((m11 + m22 + m33 - 1) / 2);
        }

        public void Min(Vector4 v)
        {
            if (x > v.x) x = v.x;
            if (y > v.y) y = v.y;
            if (z > v.z) z = v.z;
            if (w > v.w) w = v.w;
        }

        public void Max(Vector4 v)
        {
            if (x < v.x) x = v.x;
            if (y < v.y) y = v.y;
            if (z < v.z) z = v.z;
            if (w < v.w) w = v.w;
        }

        public void Clamp(Vector4 min, Vector4 max)
        {
            // This function assumes min < max, if this assumption isn't true it will not operate correctly
            if (x < min.x) x = min.x;
            else if (x > max.x) x = max.x;

            if (y < min.y) y = min.y;
            else if (y > max.y) y = max.y;

            if (z < min.z) this.z = min.z;
            else if (z > max.z) z = max.z;

            if (w < min.w) w = min.w;
            else if (w > max.w) w = max.w;
        }

        public void Clamp(float minVal, float maxVal)
        {
            var min = new Vector4(minVal, minVal, minVal, minVal);
            var max = new Vector4(maxVal, maxVal, maxVal, maxVal);
            Clamp(min, max);
        }

        public void Floor()
        {
            x = Mathf.Floor(x);
            y = Mathf.Floor(y);
            z = Mathf.Floor(z);
            w = Mathf.Floor(w);
        }

        public void Ceiling()
        {
            x = Mathf.Ceiling(x);
            y = Mathf.Ceiling(y);
            z = Mathf.Ceiling(z);
            w = Mathf.Ceiling(w);
        }

        public void Round()
        {
           x = Mathf.Round(x);
           y = Mathf.Round(y);
           z = Mathf.Round(z);
           w = Mathf.Round(w);
        }

        public void RoundToZero()
        {
            x = (x < 0) ? Mathf.Ceiling(x) : Mathf.Floor(x);
            y = (y < 0) ? Mathf.Ceiling(y) : Mathf.Floor(y);
            z = (z < 0) ? Mathf.Ceiling(z) : Mathf.Floor(z);
            w = (w < 0) ? Mathf.Ceiling(w) : Mathf.Floor(w);
        }

        public void Negate()
        {
            x = -x;
            y = -y;
            z = -z;
            w = -w;
        }

        public float Dot(Vector4 v)
        {
            return x * v.x + y * v.y + z * v.z + w * v.w;
        }

        public float LengthSquared()
        {
            return x * x + y * y + z * z + w * w;
        }

        public float Length()
        {
            return Mathf.Sqrt(Length());
        }

        public float LengthManhattan()
        {
            return Mathf.Abs(x) + Mathf.Abs(y) + Mathf.Abs(z) + Mathf.Abs(w);
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

        public void Lerp(Vector4 v, float alpha)
        {
            x += (v.x - x) * alpha;
            y += (v.y - y) * alpha;
            z += (v.z - z) * alpha;
            w += (v.w - w) * alpha;
        }

        public bool Equals(Vector4 v)
        {
            return ((v.x == x) && (v.y == y) && (v.z == z) && (v.w == w));
        }

        public void FromArray(float[] array)
        {
            x = array[0];
            y = array[1];
            z = array[2];
            w = array[3];
        }

        public float[] ToArray()
        {
            return new float[] { x, y, z, w };
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}", x, y, z, w);
        }
    }
}