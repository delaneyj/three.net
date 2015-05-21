using System;
using System.Diagnostics;

namespace Three.Net.Math
{
    public struct Matrix4
    {
        public static Matrix4 Identity
        {
            get
            {
                return new Matrix4(1, 0, 0, 0,
                         0, 1, 0, 0,
                         0, 0, 1, 0,
                         0, 0, 0, 1);
            }
        }

        internal float[] elements;

        public Matrix4(float n11, float n12, float n13, float n14, float n21, float n22, float n23, float n24, float n31, float n32, float n33, float n34, float n41, float n42, float n43, float n44)
        {
            elements = new float[16];
            elements[0] = n11;
            elements[4] = n12;
            elements[8] = n13;
            elements[12] = n14;
            elements[1] = n21;
            elements[5] = n22;
            elements[9] = n23;
            elements[13] = n24;
            elements[2] = n31;
            elements[6] = n32;
            elements[10] = n33;
            elements[14] = n34;
            elements[3] = n41;
            elements[7] = n42;
            elements[11] = n43;
            elements[15] = n44;
        }

        public Matrix4(float[] elements)
        {
            Debug.Assert(elements.Length == 16);
            this.elements = elements;
        }

        public void CopyPosition(Matrix4 m)
        {
            var te = this.elements;
            var me = m.elements;
            te[12] = me[12];
            te[13] = me[13];
            te[14] = me[14];
        }

        public static Matrix4 ExtractRotation(Matrix4 m)
        {
            var res = Matrix4.Identity;
            var te = res.elements;
            var me = m.elements;
            var scaleX = 1 / new Vector3(me[0], me[1], me[2]).Length();
            var scaleY = 1 / new Vector3(me[4], me[5], me[6]).Length();
            var scaleZ = 1 / new Vector3(me[8], me[9], me[10]).Length();

            te[0] = me[0] * scaleX;
            te[1] = me[1] * scaleX;
            te[2] = me[2] * scaleX;

            te[4] = me[4] * scaleY;
            te[5] = me[5] * scaleY;
            te[6] = me[6] * scaleY;

            te[8] = me[8] * scaleZ;
            te[9] = me[9] * scaleZ;
            te[10] = me[10] * scaleZ;
            return res;
        }

        public static Matrix4 MakeRotationFromEuler(Euler euler)
        {
            var res = Matrix4.Identity;
            var te = res.elements;
            float x = euler.x, y = euler.y, z = euler.z;
            float a = Mathf.Cos(x), b = Mathf.Sin(x);
            float c = Mathf.Cos(y), d = Mathf.Sin(y);
            float e = Mathf.Cos(z), f = Mathf.Sin(z);

            if (euler.order == Euler.OrderMode.XYZ)
            {
                float ae = a * e, af = a * f, be = b * e, bf = b * f;

                te[0] = c * e;
                te[4] = -c * f;
                te[8] = d;

                te[1] = af + be * d;
                te[5] = ae - bf * d;
                te[9] = -b * c;

                te[2] = bf - ae * d;
                te[6] = be + af * d;
                te[10] = a * c;

            }
            else if (euler.order == Euler.OrderMode.YXZ)
            {
                float ce = c * e, cf = c * f, de = d * e, df = d * f;

                te[0] = ce + df * b;
                te[4] = de * b - cf;
                te[8] = a * d;

                te[1] = a * f;
                te[5] = a * e;
                te[9] = -b;

                te[2] = cf * b - de;
                te[6] = df + ce * b;
                te[10] = a * c;

            }
            else if (euler.order == Euler.OrderMode.ZXY)
            {
                float ce = c * e, cf = c * f, de = d * e, df = d * f;

                te[0] = ce - df * b;
                te[4] = -a * f;
                te[8] = de + cf * b;

                te[1] = cf + de * b;
                te[5] = a * e;
                te[9] = df - ce * b;

                te[2] = -a * d;
                te[6] = b;
                te[10] = a * c;
            }
            else if (euler.order == Euler.OrderMode.ZYX)
            {
                float ae = a * e, af = a * f, be = b * e, bf = b * f;

                te[0] = c * e;
                te[4] = be * d - af;
                te[8] = ae * d + bf;

                te[1] = c * f;
                te[5] = bf * d + ae;
                te[9] = af * d - be;

                te[2] = -d;
                te[6] = b * c;
                te[10] = a * c;

            }
            else if (euler.order == Euler.OrderMode.YZX)
            {
                float ac = a * c, ad = a * d, bc = b * c, bd = b * d;

                te[0] = c * e;
                te[4] = bd - ac * f;
                te[8] = bc * f + ad;

                te[1] = f;
                te[5] = a * e;
                te[9] = -b * e;

                te[2] = -d * e;
                te[6] = ad * f + bc;
                te[10] = ac - bd * f;
            }
            else if (euler.order == Euler.OrderMode.XZY)
            {
                float ac = a * c, ad = a * d, bc = b * c, bd = b * d;

                te[0] = c * e;
                te[4] = -f;
                te[8] = d * e;

                te[1] = ac * f + bd;
                te[5] = a * e;
                te[9] = ad * f - bc;

                te[2] = bc * f - ad;
                te[6] = b * e;
                te[10] = bd * f + ac;
            }

            // last column
            te[3] = 0;
            te[7] = 0;
            te[11] = 0;

            // bottom row
            te[12] = 0;
            te[13] = 0;
            te[14] = 0;
            te[15] = 1;

            return res;
        }

        public static Matrix4 MakeRotationFromQuaternion(Quaternion q)
        {
            var res = Matrix4.Identity;
            var te = res.elements;
            float x = q.x, y = q.y, z = q.z, w = q.w;
            float x2 = x + x, y2 = y + y, z2 = z + z;
            float xx = x * x2, xy = x * y2, xz = x * z2;
            float yy = y * y2, yz = y * z2, zz = z * z2;
            float wx = w * x2, wy = w * y2, wz = w * z2;

            te[0] = 1 - (yy + zz);
            te[4] = xy - wz;
            te[8] = xz + wy;

            te[1] = xy + wz;
            te[5] = 1 - (xx + zz);
            te[9] = yz - wx;

            te[2] = xz - wy;
            te[6] = yz + wx;
            te[10] = 1 - (xx + yy);

            // last column
            te[3] = 0;
            te[7] = 0;
            te[11] = 0;

            // bottom row
            te[12] = 0;
            te[13] = 0;
            te[14] = 0;
            te[15] = 1;

            return res;
        }

        public void LookAt(Vector3 eye, Vector3 target, Vector3 up)
        {
            var te = this.elements;
            var x = new Vector3();
            var y = new Vector3();
            var z = new Vector3();

            z = Vector3.SubtractVectors(eye, target);
            z.Normalize();

            if (z.Length() == 0) z.z = 1;

            x = Vector3.CrossVectors(up, z);
            x.Normalize();

            if (x.Length() == 0)
            {
                z.x += 0.0001f;
                x = Vector3.CrossVectors(up, z);
                x.Normalize();
            }

            y = Vector3.CrossVectors(z, x);

            te[0] = x.x; te[4] = y.x; te[8] = z.x;
            te[1] = x.y; te[5] = y.y; te[9] = z.y;
            te[2] = x.z; te[6] = y.z; te[10] = z.z;
        }

        public void Multiply(Matrix4 m)
        {
            MultiplyMatrices(this, m);
        }

        public void MultiplyMatrices(Matrix4 a, Matrix4 b)
        {
            if (elements == null) elements = new float[16];
            var ae = a.elements;
            var be = b.elements;

            float a11 = ae[0], a12 = ae[4], a13 = ae[8], a14 = ae[12];
            float a21 = ae[1], a22 = ae[5], a23 = ae[9], a24 = ae[13];
            float a31 = ae[2], a32 = ae[6], a33 = ae[10], a34 = ae[14];
            float a41 = ae[3], a42 = ae[7], a43 = ae[11], a44 = ae[15];
            float b11 = be[0], b12 = be[4], b13 = be[8], b14 = be[12];
            float b21 = be[1], b22 = be[5], b23 = be[9], b24 = be[13];
            float b31 = be[2], b32 = be[6], b33 = be[10], b34 = be[14];
            float b41 = be[3], b42 = be[7], b43 = be[11], b44 = be[15];

            elements[0] = a11 * b11 + a12 * b21 + a13 * b31 + a14 * b41;
            elements[4] = a11 * b12 + a12 * b22 + a13 * b32 + a14 * b42;
            elements[8] = a11 * b13 + a12 * b23 + a13 * b33 + a14 * b43;
            elements[12] = a11 * b14 + a12 * b24 + a13 * b34 + a14 * b44;

            elements[1] = a21 * b11 + a22 * b21 + a23 * b31 + a24 * b41;
            elements[5] = a21 * b12 + a22 * b22 + a23 * b32 + a24 * b42;
            elements[9] = a21 * b13 + a22 * b23 + a23 * b33 + a24 * b43;
            elements[13] = a21 * b14 + a22 * b24 + a23 * b34 + a24 * b44;

            elements[2] = a31 * b11 + a32 * b21 + a33 * b31 + a34 * b41;
            elements[6] = a31 * b12 + a32 * b22 + a33 * b32 + a34 * b42;
            elements[10] = a31 * b13 + a32 * b23 + a33 * b33 + a34 * b43;
            elements[14] = a31 * b14 + a32 * b24 + a33 * b34 + a34 * b44;

            elements[3] = a41 * b11 + a42 * b21 + a43 * b31 + a44 * b41;
            elements[7] = a41 * b12 + a42 * b22 + a43 * b32 + a44 * b42;
            elements[11] = a41 * b13 + a42 * b23 + a43 * b33 + a44 * b43;
            elements[15] = a41 * b14 + a42 * b24 + a43 * b34 + a44 * b44;
        }


        public void MultiplyToArray(Matrix4 a, Matrix4 b, ref float[] r)
        {
            var te = this.elements;

            MultiplyMatrices(a, b);

            r[0] = te[0]; r[1] = te[1]; r[2] = te[2]; r[3] = te[3];
            r[4] = te[4]; r[5] = te[5]; r[6] = te[6]; r[7] = te[7];
            r[8] = te[8]; r[9] = te[9]; r[10] = te[10]; r[11] = te[11];
            r[12] = te[12]; r[13] = te[13]; r[14] = te[14]; r[15] = te[15];
        }

        public void Multiply(float s)
        {
            var te = this.elements;

            te[0] *= s; te[4] *= s; te[8] *= s; te[12] *= s;
            te[1] *= s; te[5] *= s; te[9] *= s; te[13] *= s;
            te[2] *= s; te[6] *= s; te[10] *= s; te[14] *= s;
            te[3] *= s; te[7] *= s; te[11] *= s; te[15] *= s;
        }


        public float Determinant()
        {
            var te = this.elements;
            float n11 = te[0], n12 = te[4], n13 = te[8], n14 = te[12];
            float n21 = te[1], n22 = te[5], n23 = te[9], n24 = te[13];
            float n31 = te[2], n32 = te[6], n33 = te[10], n34 = te[14];
            float n41 = te[3], n42 = te[7], n43 = te[11], n44 = te[15];

            //TODO: make this more efficient
            //( based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm )

            return (
                n41 * (
                    +n14 * n23 * n32
                     - n13 * n24 * n32
                     - n14 * n22 * n33
                     + n12 * n24 * n33
                     + n13 * n22 * n34
                     - n12 * n23 * n34
                ) +
                n42 * (
                    +n11 * n23 * n34
                     - n11 * n24 * n33
                     + n14 * n21 * n33
                     - n13 * n21 * n34
                     + n13 * n24 * n31
                     - n14 * n23 * n31
                ) +
                n43 * (
                    +n11 * n24 * n32
                     - n11 * n22 * n34
                     - n14 * n21 * n32
                     + n12 * n21 * n34
                     + n14 * n22 * n31
                     - n12 * n24 * n31
                ) +
                n44 * (
                    -n13 * n22 * n31
                     - n11 * n23 * n32
                     + n11 * n22 * n33
                     + n13 * n21 * n32
                     - n12 * n21 * n33
                     + n12 * n23 * n31
                )

            );
        }

        public void Transpose()
        {
            var te = this.elements;
            float tmp;
            tmp = te[1]; te[1] = te[4]; te[4] = tmp;
            tmp = te[2]; te[2] = te[8]; te[8] = tmp;
            tmp = te[6]; te[6] = te[9]; te[9] = tmp;
            tmp = te[3]; te[3] = te[12]; te[12] = tmp;
            tmp = te[7]; te[7] = te[13]; te[13] = tmp;
            tmp = te[11]; te[11] = te[14]; te[14] = tmp;
        }

        public void FlattenToArrayOffset(ref float[] array, int offset)
        {
            var te = this.elements;
            array[offset] = te[0];
            array[offset + 1] = te[1];
            array[offset + 2] = te[2];
            array[offset + 3] = te[3];

            array[offset + 4] = te[4];
            array[offset + 5] = te[5];
            array[offset + 6] = te[6];
            array[offset + 7] = te[7];

            array[offset + 8] = te[8];
            array[offset + 9] = te[9];
            array[offset + 10] = te[10];
            array[offset + 11] = te[11];

            array[offset + 12] = te[12];
            array[offset + 13] = te[13];
            array[offset + 14] = te[14];
            array[offset + 15] = te[15];
        }

        public void SetPosition(Vector3 v)
        {
            var te = this.elements;
            te[12] = v.x;
            te[13] = v.y;
            te[14] = v.z;
        }

        //Sets this matrix to the inverse of matrix m.
        public static Matrix4 GetInverse(Matrix4 m)
        {
            // based on http://www.euclideanspace.com/maths/algebra/matrix/functions/inverse/fourD/index.htm
            var res = Matrix4.Identity;
            var te = res.elements;
            var me = m.elements;
            float n11 = me[0], n12 = me[4], n13 = me[8], n14 = me[12];
            float n21 = me[1], n22 = me[5], n23 = me[9], n24 = me[13];
            float n31 = me[2], n32 = me[6], n33 = me[10], n34 = me[14];
            float n41 = me[3], n42 = me[7], n43 = me[11], n44 = me[15];

            te[0] = n23 * n34 * n42 - n24 * n33 * n42 + n24 * n32 * n43 - n22 * n34 * n43 - n23 * n32 * n44 + n22 * n33 * n44;
            te[4] = n14 * n33 * n42 - n13 * n34 * n42 - n14 * n32 * n43 + n12 * n34 * n43 + n13 * n32 * n44 - n12 * n33 * n44;
            te[8] = n13 * n24 * n42 - n14 * n23 * n42 + n14 * n22 * n43 - n12 * n24 * n43 - n13 * n22 * n44 + n12 * n23 * n44;
            te[12] = n14 * n23 * n32 - n13 * n24 * n32 - n14 * n22 * n33 + n12 * n24 * n33 + n13 * n22 * n34 - n12 * n23 * n34;
            te[1] = n24 * n33 * n41 - n23 * n34 * n41 - n24 * n31 * n43 + n21 * n34 * n43 + n23 * n31 * n44 - n21 * n33 * n44;
            te[5] = n13 * n34 * n41 - n14 * n33 * n41 + n14 * n31 * n43 - n11 * n34 * n43 - n13 * n31 * n44 + n11 * n33 * n44;
            te[9] = n14 * n23 * n41 - n13 * n24 * n41 - n14 * n21 * n43 + n11 * n24 * n43 + n13 * n21 * n44 - n11 * n23 * n44;
            te[13] = n13 * n24 * n31 - n14 * n23 * n31 + n14 * n21 * n33 - n11 * n24 * n33 - n13 * n21 * n34 + n11 * n23 * n34;
            te[2] = n22 * n34 * n41 - n24 * n32 * n41 + n24 * n31 * n42 - n21 * n34 * n42 - n22 * n31 * n44 + n21 * n32 * n44;
            te[6] = n14 * n32 * n41 - n12 * n34 * n41 - n14 * n31 * n42 + n11 * n34 * n42 + n12 * n31 * n44 - n11 * n32 * n44;
            te[10] = n12 * n24 * n41 - n14 * n22 * n41 + n14 * n21 * n42 - n11 * n24 * n42 - n12 * n21 * n44 + n11 * n22 * n44;
            te[14] = n14 * n22 * n31 - n12 * n24 * n31 - n14 * n21 * n32 + n11 * n24 * n32 + n12 * n21 * n34 - n11 * n22 * n34;
            te[3] = n23 * n32 * n41 - n22 * n33 * n41 - n23 * n31 * n42 + n21 * n33 * n42 + n22 * n31 * n43 - n21 * n32 * n43;
            te[7] = n12 * n33 * n41 - n13 * n32 * n41 + n13 * n31 * n42 - n11 * n33 * n42 - n12 * n31 * n43 + n11 * n32 * n43;
            te[11] = n13 * n22 * n41 - n12 * n23 * n41 - n13 * n21 * n42 + n11 * n23 * n42 + n12 * n21 * n43 - n11 * n22 * n43;
            te[15] = n12 * n23 * n31 - n13 * n22 * n31 + n13 * n21 * n32 - n11 * n23 * n32 - n12 * n21 * n33 + n11 * n22 * n33;

            var det = n11 * te[0] + n21 * te[4] + n31 * te[8] + n41 * te[12];

            if (det == 0) throw new InvalidOperationException("Can't invert matrix, determinant is 0");

            res.Multiply(1 / det);
            return res;
        }

        public void Scale(Vector3 v)
        {
            var te = this.elements;
            float x = v.x, y = v.y, z = v.z;
            te[0] *= x; te[4] *= y; te[8] *= z;
            te[1] *= x; te[5] *= y; te[9] *= z;
            te[2] *= x; te[6] *= y; te[10] *= z;
            te[3] *= x; te[7] *= y; te[11] *= z;
        }

        public float GetMaxScaleOnAxis()
        {
            var te = this.elements;
            var scaleXSq = te[0] * te[0] + te[1] * te[1] + te[2] * te[2];
            var scaleYSq = te[4] * te[4] + te[5] * te[5] + te[6] * te[6];
            var scaleZSq = te[8] * te[8] + te[9] * te[9] + te[10] * te[10];
            return Mathf.Sqrt(Mathf.Max(scaleXSq, Mathf.Max(scaleYSq, scaleZSq)));
        }

        public static Matrix4 MakeTranslation(float x, float y, float z)
        {
            return new Matrix4(1, 0, 0, x,
                                0, 1, 0, y,
                                0, 0, 1, z,
                                0, 0, 0, 1);
        }

        public static Matrix4 MakeRotationX(float theta)
        {
            float c = Mathf.Cos(theta), s = Mathf.Sin(theta);
            return new Matrix4(1, 0, 0, 0,
                               0, c, -s, 0,
                               0, s, c, 0,
                               0, 0, 0, 1);
        }

        public static Matrix4 MakeRotationY(float theta)
        {
            float c = Mathf.Cos(theta), s = Mathf.Sin(theta);
            return new Matrix4(c, 0, s, 0,
                                0, 1, 0, 0,
                                -s, 0, c, 0,
                                0, 0, 0, 1);
        }

        public static Matrix4 MakeRotationZ(float theta)
        {
            float c = Mathf.Cos(theta), s = Mathf.Sin(theta);
            return new Matrix4(c, -s, 0, 0,
                                s, c, 0, 0,
                                0, 0, 1, 0,
                                0, 0, 0, 1);
        }

        public static Matrix4 MakeRotationAxis(Vector3 axis, float angle)
        {
            // Based on http://www.gamedev.net/reference/articles/article1199.asp
            var c = Mathf.Cos(angle);
            var s = Mathf.Sin(angle);
            var t = 1 - c;
            float x = axis.x, y = axis.y, z = axis.z;
            float tx = t * x, ty = t * y;

            return new Matrix4(tx * x + c, tx * y - s * z, tx * z + s * y, 0,
                                tx * y + s * z, ty * y + c, ty * z - s * x, 0,
                                tx * z - s * y, ty * z + s * x, t * z * z + c, 0,
                                0, 0, 0, 1);
        }

        public static Matrix4 MakeScale(float x, float y, float z)
        {
            return new Matrix4(x, 0, 0, 0,
                                0, y, 0, 0,
                                0, 0, z, 0,
                                0, 0, 0, 1);
        }

        public static Matrix4 Compose(Vector3 position, Quaternion quaternion, Vector3 scale)
        {
            var m = MakeRotationFromQuaternion(quaternion);
            m.Scale(scale);
            m.SetPosition(position);
            return m;
        }

        public void Decompose(ref Vector3 position, ref Quaternion quaternion, ref Vector3 scale)
        {
            var te = this.elements;
            var sx = new Vector3(te[0], te[1], te[2]).Length();
            var sy = new Vector3(te[4], te[5], te[6]).Length();
            var sz = new Vector3(te[8], te[9], te[10]).Length();

            // if determine is negative, we need to invert one scale
            var det = Determinant();
            if (det < 0) sx = -sx;

            position.x = te[12];
            position.y = te[13];
            position.z = te[14];

            // scale the rotation part
            var matrix = new Matrix4(elements);

            var invSX = 1 / sx;
            var invSY = 1 / sy;
            var invSZ = 1 / sz;

            matrix.elements[0] *= invSX;
            matrix.elements[1] *= invSX;
            matrix.elements[2] *= invSX;

            matrix.elements[4] *= invSY;
            matrix.elements[5] *= invSY;
            matrix.elements[6] *= invSY;

            matrix.elements[8] *= invSZ;
            matrix.elements[9] *= invSZ;
            matrix.elements[10] *= invSZ;

            quaternion = Quaternion.FromRotation(matrix);

            scale.x = sx;
            scale.y = sy;
            scale.z = sz;
        }

        public static Matrix4 MakeFrustum(float left, float right, float bottom, float top, float near, float far)
        {
            var res = Matrix4.Identity;
            var te = res.elements;

            var x = (2 * near) / (right - left);
            var y = (2 * near) / (top - bottom);
            var a = (right + left) / (right - left);
            var b = (top + bottom) / (top - bottom);
            var c = -(far + near) / (far - near);
            var d = -(2 * far * near) / (far - near);

            te[0] = x; te[4] = 0; te[8] = a; te[12] = 0;
            te[1] = 0; te[5] = y; te[9] = b; te[13] = 0;
            te[2] = 0; te[6] = 0; te[10] = c; te[14] = d;
            te[3] = 0; te[7] = 0; te[11] = -1; te[15] = 0;
            return res;
        }

        public static Matrix4 MakePerspective(float fov, float aspect, float near, float far)
        {
            var ymax = near * Mathf.Tan(Mathf.DegreesToRadians(fov) * 0.5f);
            var ymin = -ymax;
            var xmin = ymin * aspect;
            var xmax = ymax * aspect;
            return MakeFrustum(xmin, xmax, ymin, ymax, near, far);
        }

        public static Matrix4 MakeOrthographic(float left, float right, float top, float bottom, float near, float far)
        {
            var res = Matrix4.Identity;
            var te = res.elements;
            var w = right - left;
            var h = top - bottom;
            var p = far - near;

            var x = (right + left) / w;
            var y = (top + bottom) / h;
            var z = (far + near) / p;

            te[0] = 2 / w; te[4] = 0; te[8] = 0; te[12] = -x;
            te[1] = 0; te[5] = 2 / h; te[9] = 0; te[13] = -y;
            te[2] = 0; te[6] = 0; te[10] = -2 / p; te[14] = -z;
            te[3] = 0; te[7] = 0; te[11] = 0; te[15] = 1;
            return res;
        }

        public void FromArray(float[] array)
        {
            Debug.Assert(array.Length == elements.Length);
            elements = array;
        }

        public float[] ToArray()
        {
            var te = this.elements;

            return new float[]{
			te[ 0 ], te[ 1 ], te[ 2 ], te[ 3 ],
			te[ 4 ], te[ 5 ], te[ 6 ], te[ 7 ],
			te[ 8 ], te[ 9 ], te[ 10 ], te[ 11 ],
			te[ 12 ], te[ 13 ], te[ 14 ], te[ 15 ]
		};
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2},{3}  {4},{5},{6},{7}  {8},{9},{10},{11}  {12},{13},{14},{15}",   elements[0],elements[1],elements[2],elements[3],
                                                                                                                elements[4],elements[5],elements[6],elements[7],
                                                                                                                elements[8],elements[9],elements[10],elements[11],
                                                                                                                elements[12],elements[13],elements[14],elements[15]);
        }
    }
}