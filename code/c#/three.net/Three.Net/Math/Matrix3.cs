using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public struct Matrix3
    {
        public static Matrix3 Identity = new Matrix3(1, 0, 0, 
                                                     0, 1, 0, 
                                                     0, 0, 1);
        internal float[] elements;

        public Matrix3(float n11, float n12, float n13, float n21, float n22, float n23, float n31, float n32, float n33)
        {
            elements = new float[9];
            elements[0] = n11;
            elements[3] = n12;
            elements[6] = n13;
            elements[1] = n21;
            elements[4] = n22;
            elements[7] = n23;
            elements[2] = n31;
            elements[5] = n32;
            elements[8] = n33;
        }

        public void Set(float n11, float n12, float n13, float n21, float n22, float n23, float n31, float n32, float n33)
        {
            var te = elements;
            te[0] = n11; te[3] = n12; te[6] = n13;
            te[1] = n21; te[4] = n22; te[7] = n23;
            te[2] = n31; te[5] = n32; te[8] = n33;
        }

        public void Set(Matrix3 m)
        {
            var me = m.elements;
            Set(me[0], me[3], me[6],
                me[1], me[4], me[7],
                me[2], me[5], me[8]);
        }

        public void Multiply(float s)
        {
            var te = this.elements;
            te[0] *= s; te[3] *= s; te[6] *= s;
            te[1] *= s; te[4] *= s; te[7] *= s;
            te[2] *= s; te[5] *= s; te[8] *= s;
        }

        public float Determinant()
        {
            var te = this.elements;
            float a = te[0], b = te[1], c = te[2],
                  d = te[3], e = te[4], f = te[5],
                  g = te[6], h = te[7], i = te[8];
            return a * e * i - a * f * h - b * d * i + b * f * g + c * d * h - c * e * g;
        }

        //Set this matrix to the inverse of the passed matrix.
        public static Matrix3 GetInverse(Matrix4 matrix)
        {
            // ( based on http://code.google.com/p/webgl-mjs/ )
            var me = matrix.elements;
            var res = Matrix3.Identity;
            var te = res.elements;
            te[0] = me[10] * me[5] - me[6] * me[9];
            te[1] = -me[10] * me[1] + me[2] * me[9];
            te[2] = me[6] * me[1] - me[2] * me[5];
            te[3] = -me[10] * me[4] + me[6] * me[8];
            te[4] = me[10] * me[0] - me[2] * me[8];
            te[5] = -me[6] * me[0] + me[2] * me[4];
            te[6] = me[9] * me[4] - me[5] * me[8];
            te[7] = -me[9] * me[0] + me[1] * me[8];
            te[8] = me[5] * me[0] - me[1] * me[4];
            var det = me[0] * te[0] + me[1] * te[3] + me[2] * te[6];

            if (det == 0) throw new InvalidOperationException("Can't invert matrix, determinant is 0");

            res.Multiply(1 / det);
            return res;
        }

        public void Transpose()
        {
            float tmp;
            var m = this.elements;

            tmp = m[1]; m[1] = m[3]; m[3] = tmp;
            tmp = m[2]; m[2] = m[6]; m[6] = tmp;
            tmp = m[5]; m[5] = m[7]; m[7] = tmp;
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
        }

        //Set this matrix as the normal matrix of the passed matrix4. The normal matrix is the inverse transpose of the matrix.
        public static Matrix3 GetNormalMatrix(Matrix4 m)
        {
            var res = Matrix3.GetInverse(m);
            res.Transpose();
            return res;
        }

        public void TransposeIntoArray(ref float[] r)
        {
            var m = this.elements;
            r[0] = m[0];
            r[1] = m[3];
            r[2] = m[6];
            r[3] = m[1];
            r[4] = m[4];
            r[5] = m[7];
            r[6] = m[2];
            r[7] = m[5];
            r[8] = m[8];
        }

        public void FromArray(float[] array)
        {
            Debug.Assert(array.Length == elements.Length);
            elements = array;
        }

        public float[] ToArray()
        {
            var te = this.elements;
            return new float[9]
            {
                te[ 0 ], te[ 1 ], te[ 2 ],
			te[ 3 ], te[ 4 ], te[ 5 ],
			te[ 6 ], te[ 7 ], te[ 8 ]
            };
        }

        public override string ToString()
        {
            return string.Format("{0},{1},{2} {3},{4},{5} {6},{7},{8}", elements[0], elements[1], elements[2],
                                                                           elements[3], elements[4], elements[5],
                                                                           elements[6], elements[7], elements[8]);
        }
    }
}
