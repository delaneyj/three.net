using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Renderers.GL4;

namespace Three.Net.Materials
{
    public class SpriteMaterial : MeshBasicMaterial
    {
        public uint Opacity;
        public uint Rotation;

        public SpriteMaterial(Renderer renderer):base(renderer)
        {

        }
    }
}
