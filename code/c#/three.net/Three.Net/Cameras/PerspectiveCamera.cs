using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Cameras
{
    public class PerspectiveCamera : Camera
    {
        private float fov, fullWidth, fullHeight, x, y, width, height;
        private bool useViewOffset = false;
        private Renderer renderer;

        public PerspectiveCamera(Renderer renderer, float fov = 50, float near = 0.1f, float far = 2000)
            : base(near, far)
        {
            this.fov = fov;
            this.renderer = renderer;
            UpdateProjectionMatrix();
        }

        /**
        * Uses Focal Length (in mm) to estimate and set FOV
        * 35mm (fullframe) camera is used if frame size is not specified;
        * Formula based on http://www.bobatkins.com/photography/technical/field_of_view.html
        */
        public void SetLens(float focalLength, float frameHeight = 24)
        {
            fov = 2 * Mathf.RadiansToDegrees(Mathf.Atan(frameHeight / (focalLength * 2)));
            UpdateProjectionMatrix();
        }

        /**
         * Sets an offset in a larger frustum. This is useful for multi-window or
         * multi-monitor/multi-machine setups.
         *
         * For example, if you have 3x2 monitors and each monitor is 1920x1080 and
         * the monitors are in grid like this
         *
         *   +---+---+---+
         *   | A | B | C |
         *   +---+---+---+
         *   | D | E | F |
         *   +---+---+---+
         *
         * then for each monitor you would call it like this
         *
         *   var w = 1920;
         *   var h = 1080;
         *   var fullWidth = w * 3;
         *   var fullHeight = h * 2;
         *
         *   --A--
         *   camera.setOffset( fullWidth, fullHeight, w * 0, h * 0, w, h );
         *   --B--
         *   camera.setOffset( fullWidth, fullHeight, w * 1, h * 0, w, h );
         *   --C--
         *   camera.setOffset( fullWidth, fullHeight, w * 2, h * 0, w, h );
         *   --D--
         *   camera.setOffset( fullWidth, fullHeight, w * 0, h * 1, w, h );
         *   --E--
         *   camera.setOffset( fullWidth, fullHeight, w * 1, h * 1, w, h );
         *   --F--
         *   camera.setOffset( fullWidth, fullHeight, w * 2, h * 1, w, h );
         *
         *   Note there is no reason monitors have to be the same size or in a grid.
         */
        public void SetViewOffset(float fullWidth, float fullHeight, float x, float y, float width, float height)
        {
            useViewOffset = true;
            this.fullWidth = fullWidth;
            this.fullHeight = fullHeight;
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
            UpdateProjectionMatrix();
        }

        internal override void UpdateProjectionMatrix()
        {
            if (useViewOffset)
            {
                var aspect = fullWidth / fullHeight;
                var top = Mathf.Tan(Mathf.DegreesToRadians(fov * 0.5f)) * near;
                var bottom = -top;
                var left = aspect * bottom;
                var right = aspect * top;
                var width = Mathf.Abs(right - left);
                var height = Mathf.Abs(top - bottom);

                projectionMatrix = Matrix4.MakeFrustum(
                    left + x * width / fullWidth,
                    left + (x + width) * width / fullWidth,
                    top - (y + height) * height / fullHeight,
                    top - y * height / fullHeight,
                    near, far);
            }
            else
            {
                projectionMatrix = Matrix4.MakePerspective(fov, renderer.AspectRatio, near, far);
            }
        }
    }
}
