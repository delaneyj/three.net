using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Textures;

namespace Three.Net.Renderers.GL4
{
    public class RenderTarget : Texture
    {
        public bool UseDepthBuffer = true, UseStencilBuffer = true;
        public int? glFramebuffer,glRenderbuffer;
        public RenderTarget shareDepthFrom;

        public RenderTarget(int width, int height)
        {
            Resolution = new System.Drawing.Size(width, height);
        }
    }
}
