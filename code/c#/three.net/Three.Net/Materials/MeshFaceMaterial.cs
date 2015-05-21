using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Renderers.GL4;

namespace Three.Net.Materials
{
    public class MeshFaceMaterial : MeshBasicMaterial
    {
        private List<MeshBasicMaterial> materials = new List<MeshBasicMaterial>();

        public MeshFaceMaterial(Renderer renderer, IEnumerable<MeshBasicMaterial> materials) : base(renderer)
        {
            this.materials.AddRange(materials);
        }
    }
}
