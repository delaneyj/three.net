using Three.Net.Cameras;
using Three.Net.Renderers.GL4;
using Three.Net.Scenes;

namespace Three.Net.Renderers.Plugins
{
   public abstract class RenderPlugin
    {
       protected Renderer renderer;
       internal protected RenderTarget RenderTarget = null;

       internal protected virtual void Init(Renderer renderer)
       {
           this.renderer = renderer;
       }

       internal protected abstract void Render(Scene scene, Camera camera, int viewportWidth, int viewportHeight);
    }
}
