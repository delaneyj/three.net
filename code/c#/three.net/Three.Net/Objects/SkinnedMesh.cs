using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Materials;

namespace Three.Net.Objects
{
    public class SkinnedMesh : Mesh
    {
        public Skeleton Skeleton;

        public SkinnedMesh(Geometry geometry, MeshBasicMaterial material, bool useVertexTexture):base(geometry, material)
        {

        }
    }
}
