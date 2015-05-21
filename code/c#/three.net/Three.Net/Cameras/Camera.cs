using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Three.Net.Core;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Cameras
{
    public abstract class Camera : Object3D
    {
        public Matrix4 matrixWorldInverse { get; internal set; }
        public Matrix4 projectionMatrix { get; internal set; }

        internal protected float near, far;
        public Camera(float near = 0.1f, float far = 2000)
        {
            matrixWorldInverse = Matrix4.Identity;
            projectionMatrix = Matrix4.Identity;
            this.near = near;
            this.far = far;
        }

        internal abstract void UpdateProjectionMatrix();
    }
}
