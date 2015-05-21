using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Core
{
    public class Gyroscope : Object3D
    {
        private Vector3 translationWorld = Vector3.Zero;
private Vector3 translationObject = Vector3.Zero;
private Quaternion  quaternionWorld = Quaternion.Identity;
private Quaternion quaternionObject = Quaternion.Identity;
private Vector3 scaleWorld = Vector3.Zero;
private Vector3 scaleObject = Vector3.Zero;

        public override void UpdateMatrixWorld(bool force = false)
        {
            if(matrixAutoUpdate) UpdateMatrix();

            // update matrixWorld
            if (matrixWorldNeedsUpdate || force)
            {
                if (HasParent)
                {
                    matrixWorld.MultiplyMatrices(Parent.matrixWorld, matrix);

                    matrixWorld.Decompose(ref translationWorld, ref quaternionWorld, ref scaleWorld);
                    matrix.Decompose(ref translationObject, ref quaternionObject, ref scaleObject);
                    matrixWorld= Matrix4.Compose(translationWorld, quaternionObject, scaleWorld);
                }
                else
                {
                    matrixWorld = matrix;
                }


                matrixWorldNeedsUpdate = false;
                force = true;
            }

            // update children
            foreach (var c in Children) c.UpdateMatrixWorld(force);
        }
    }
}
