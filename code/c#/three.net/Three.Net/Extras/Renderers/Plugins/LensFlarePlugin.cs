using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Extras.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.Plugins;

namespace Three.Net.Extras.Renderers.Plugins
{
    public class LensFlarePlugin : RenderPlugin
    {
        private List<LensFlare> flares = new List<LensFlare>();

        protected internal override void Init(Net.Renderers.GL4.Renderer renderer)
        {
            base.Init(renderer);

            throw new NotImplementedException();
        }

        protected internal override void Render(Scenes.Scene scene, Cameras.Camera camera, int viewportWidth, int viewportHeight)
        {
            throw new NotImplementedException();
        }
    }
}
