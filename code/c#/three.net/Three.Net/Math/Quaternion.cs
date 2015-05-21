using System;

namespace Three.Net.Math
{
    public struct Quaternion : IEquatable<Quaternion>
    {
        public static Quaternion Identity = new Quaternion(0,0,0,1);

        public float x, y, z, w;

        public Quaternion(float x, float y, float z, float w)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.w = w;
        }

        public Quaternion(Quaternion quaternion)
        {

            this.x = quaternion.x;
            this.y = quaternion.y;
            this.z = quaternion.z;
            this.w = quaternion.w;
            //this.onChangeCallback();
        }

        public static Quaternion From(Euler euler)
        {
            // http://www.mathworks.com/matlabcentral/fileexchange/
            // 	20696-function-to-convert-between-dcm-euler-angles-quaternions-and-euler-vectors/
            //	content/SpinCalc.m
            float x, y, z, w;
            var c1 = Mathf.Cos(euler.x / 2);
            var c2 = Mathf.Cos(euler.y / 2);
            var c3 = Mathf.Cos(euler.z / 2);
            var s1 = Mathf.Sin(euler.x / 2);
            var s2 = Mathf.Sin(euler.y / 2);
            var s3 = Mathf.Sin(euler.z / 2);

            if (euler.order == Euler.OrderMode.XYZ)
            {
                x = s1 * c2 * c3 + c1 * s2 * s3;
                y = c1 * s2 * c3 - s1 * c2 * s3;
                z = c1 * c2 * s3 + s1 * s2 * c3;
                w = c1 * c2 * c3 - s1 * s2 * s3;
            }
            else if (euler.order == Euler.OrderMode.YXZ)
            {
                x = s1 * c2 * c3 + c1 * s2 * s3;
                y = c1 * s2 * c3 - s1 * c2 * s3;
                z = c1 * c2 * s3 - s1 * s2 * c3;
                w = c1 * c2 * c3 + s1 * s2 * s3;
            }
            else if (euler.order == Euler.OrderMode.ZXY)
            {
                x = s1 * c2 * c3 - c1 * s2 * s3;
                y = c1 * s2 * c3 + s1 * c2 * s3;
                z = c1 * c2 * s3 + s1 * s2 * c3;
                w = c1 * c2 * c3 - s1 * s2 * s3;
            }
            else if (euler.order == Euler.OrderMode.ZYX)
            {
                x = s1 * c2 * c3 - c1 * s2 * s3;
                y = c1 * s2 * c3 + s1 * c2 * s3;
                z = c1 * c2 * s3 - s1 * s2 * c3;
                w = c1 * c2 * c3 + s1 * s2 * s3;
            }
            else if (euler.order == Euler.OrderMode.YZX)
            {
                x = s1 * c2 * c3 + c1 * s2 * s3;
                y = c1 * s2 * c3 + s1 * c2 * s3;
                z = c1 * c2 * s3 - s1 * s2 * c3;
                w = c1 * c2 * c3 - s1 * s2 * s3;
            }
            else if (euler.order == Euler.OrderMode.XZY)
            {

                x = s1 * c2 * c3 - c1 * s2 * s3;
                y = c1 * s2 * c3 - s1 * c2 * s3;
                z = c1 * c2 * s3 + s1 * s2 * c3;
                w = c1 * c2 * c3 + s1 * s2 * s3;
            }
            else throw new InvalidOperationException();

            //if (!update) this.onChangeCallback();
            return new Quaternion(x, y, z, w);
        }

        public static Quaternion FromAxisAngle(Vector3 axis, float angle)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/angleToQuaternion/index.htm
            // assumes axis is normalized
            float halfAngle = angle / 2, s = Mathf.Sin(halfAngle);
            var x = axis.x * s;
            var y = axis.y * s;
            var z = axis.z * s;
            var w = Mathf.Cos(halfAngle);
            //this.onChangeCallback();
            return new Quaternion(x, y, z, w);
        }

        public static Quaternion FromRotation(Matrix4 m)
        {
            // http://www.euclideanspace.com/maths/geometry/rotations/conversions/matrixToQuaternion/index.htm
            // assumes the upper 3x3 of m is a pure rotation matrix (i.e, unscaled)
            float x, y, z, w;
            var te = m.elements;
            float m11 = te[0], m12 = te[4], m13 = te[8];
            float m21 = te[1], m22 = te[5], m23 = te[9];
            float m31 = te[2], m32 = te[6], m33 = te[10];
            float trace = m11 + m22 + m33, s;

            if (trace > 0)
            {
                s = 0.5f / Mathf.Sqrt(trace + 1);
                w = 0.25f / s;
                x = (m32 - m23) * s;
                y = (m13 - m31) * s;
                z = (m21 - m12) * s;
            }
            else if (m11 > m22 && m11 > m33)
            {
                s = 2 * Mathf.Sqrt(1 + m11 - m22 - m33);
                w = (m32 - m23) / s;
                x = 0.25f * s;
                y = (m12 + m21) / s;
                z = (m13 + m31) / s;
            }
            else if (m22 > m33)
            {
                s = 2 * Mathf.Sqrt(1 + m22 - m11 - m33);
                w = (m13 - m31) / s;
                x = (m12 + m21) / s;
                y = 0.25f * s;
                z = (m23 + m32) / s;
            }
            else
            {
                s = 2 * Mathf.Sqrt(1 + m33 - m11 - m22);
                w = (m21 - m12) / s;
                x = (m13 + m31) / s;
                y = (m23 + m32) / s;
                z = 0.25f * s;
            }

            //this.onChangeCallback();
            return new Quaternion(x, y, z, w);
        }

        public static Quaternion SetFromUnitVectors(Vector3 vFrom, Vector3 vTo)
        {
            // http://lolengine.net/blog/2014/02/24/quaternion-from-two-vectors-final
            // assumes direction vectors vFrom and vTo are normalized

            //var EPS = 0.000001;
            Vector3 v1 = new Vector3();
            var r = vFrom.Dot(vTo) + 1;

            if (r < float.Epsilon)
            {
                r = 0;

                if (Mathf.Abs(vFrom.x) > Mathf.Abs(vFrom.z)) v1 = new Vector3(-vFrom.y, vFrom.x, 0);
                else v1 = new Vector3(0, -vFrom.z, vFrom.y);
            }
            else
            {
                v1 = Vector3.CrossVectors(vFrom, vTo);
            }

            var q = new Quaternion(v1.x,v1.y,v1.z,r);
            q.Normalize();
            return q;
        }

        public void Inverse()
        {
            Conjugate();
            Normalize();
        }

        public void Conjugate()
        {
            this.x *= -1;
            this.y *= -1;
            this.z *= -1;
            //this.onChangeCallback();
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
            return Mathf.Sqrt(LengthSquared());
        }

        public void Normalize()
        {
            var l = Length();

            if (l == 0)
            {
                this.x = 0;
                this.y = 0;
                this.z = 0;
                this.w = 1;
            }
            else
            {
                l = 1 / l;
                this.x *= l;
                this.y *= l;
                this.z *= l;
                this.w *= l;
            }

            //this.onChangeCallback();
        }

        public void Multiply(Quaternion q)
        {
            MultiplyQuaternions(this, q);
        }

        public void MultiplyQuaternions(Quaternion a, Quaternion b)
        {
            // from http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/code/index.htm
            float qax = a.x, qay = a.y, qaz = a.z, qaw = a.w;
            float qbx = b.x, qby = b.y, qbz = b.z, qbw = b.w;
            this.x = qax * qbw + qaw * qbx + qay * qbz - qaz * qby;
            this.y = qay * qbw + qaw * qby + qaz * qbx - qax * qbz;
            this.z = qaz * qbw + qaw * qbz + qax * qby - qay * qbx;
            this.w = qaw * qbw - qax * qbx - qay * qby - qaz * qbz;
            //this.onChangeCallback();
        }

        public void Slerp(Quaternion qb, float t)
        {
            // http://www.euclideanspace.com/maths/algebra/realNormedAlgebra/quaternions/slerp/
            float x = this.x, y = this.y, z = this.z, w = this.w;
            var cosHalfTheta = w * qb.w + x * qb.x + y * qb.y + z * qb.z;

            if (cosHalfTheta < 0)
            {
                this.w = -qb.w;
                this.x = -qb.x;
                this.y = -qb.y;
                this.z = -qb.z;
                cosHalfTheta = -cosHalfTheta;
            }
            else
            {
                this.w = qb.w;
                this.x = qb.x;
                this.y = qb.y;
                this.z = qb.z;
            }

            if (cosHalfTheta >= 1)
            {
                this.w = w;
                this.x = x;
                this.y = y;
                this.z = z;
                return;
            }

            var halfTheta = Mathf.Acos(cosHalfTheta);
            var sinHalfTheta = Mathf.Sqrt(1 - cosHalfTheta * cosHalfTheta);

            if (Mathf.Abs(sinHalfTheta) < 0.001f)
            {
                this.w = 0.5f * (w + this.w);
                this.x = 0.5f * (x + this.x);
                this.y = 0.5f * (y + this.y);
                this.z = 0.5f * (z + this.z);
                return;
            }

            float ratioA = Mathf.Sin((1 - t) * halfTheta) / sinHalfTheta,
            ratioB = Mathf.Sin(t * halfTheta) / sinHalfTheta;
            this.w = (w * ratioA + this.w * ratioB);
            this.x = (x * ratioA + this.x * ratioB);
            this.y = (y * ratioA + this.y * ratioB);
            this.z = (z * ratioA + this.z * ratioB);

            //this.onChangeCallback();
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() <<8 ^ z.GetHashCode() << 16 ^ w.GetHashCode() << 24;
        }

        public override bool Equals(object obj)
        {
            var q = (Quaternion)obj;
            if (q == null) return false;
            return Equals(q);
        }

        public bool Equals(Quaternion q)
        {
            return (q.x == x) && (q.y == y) && (q.z == z) && (q.w == w);
        }

        public static bool operator ==(Quaternion left, Quaternion right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Quaternion left, Quaternion right)
        {
            return !left.Equals(right);
        }

        public void FromArray(float[] array)
        {
            this.x = array[0];
            this.y = array[1];
            this.z = array[2];
            this.w = array[3];
            //this.onChangeCallback();
        }

        public float[] ToArray()
        {
            return new float[] { x, y, z, w };
        }

        public static Quaternion Slerp(Quaternion qa, Quaternion qb, float t)
        {
            var qm = new Quaternion(qa);
            qm.Slerp(qb, t);
            return qm;
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2} {3}",x,y,z,w);
        }
    }
}