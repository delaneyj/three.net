using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Core
{
    public class Face3
    {
        public int A,B,C;
        public Vector3 NormalA, NormalB, NormalC;
        public Color ColorA, ColorB, ColorC;
        public Vector4 TangentA, TangentB, TangentC;

        public Face3(int a, int b, int c, Vector3 aNormal,Vector3 bNormal,Vector3 cNormal)
        {
            A = a;
            B = b;
            C = c;
            NormalA = aNormal;
            NormalB = bNormal;
            NormalC = cNormal;
        }

        public Face3(int a, int b, int c, Vector3 faceNormal) : this(a,b,c,faceNormal, faceNormal, faceNormal)
        {

        }

        public Face3(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
    }
}
