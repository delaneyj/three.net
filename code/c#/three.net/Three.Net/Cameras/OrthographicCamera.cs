using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Cameras
{
    public class OrthographicCamera : Camera
    {
        internal float left,right,top,bottom;

        public OrthographicCamera(Renderer renderer, float near = 0.1f, float far = 2000)
            : base(near, far)
        {
            this.left = renderer.Width / -2;
            this.right = renderer.Width / 2;
            this.top = renderer.Height / 2;
            this.bottom = renderer.Height / -2;
            UpdateProjectionMatrix();
        }

        public OrthographicCamera(float left, float right, float top, float bottom, float near= 0.1f, float far=2000) :base(near,far)
        {
            this.left = left;
	        this.right = right;
	        this.top = top;
	        this.bottom = bottom;
            UpdateProjectionMatrix();
        }

        internal override void UpdateProjectionMatrix()
        {
            projectionMatrix = Matrix4.MakeOrthographic(left, right, top, bottom, near, far);
        }
    }
}
