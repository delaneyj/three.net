using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Renderers.GL4
{
    public class RenderTargetCube : RenderTarget
    {
        public List<int> glFrameBuffers = new List<int>();
        public List<int> glRenderBuffers = new List<int>();
        public int activeCubeFace = 0; // PX 0, NX 1, PY 2, NY 3, PZ 4, NZ 5

        public RenderTargetCube(int width, int height) : base(width,height)
        {

        }
    }
}
