using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Objects
{
    public class Sprite : Object3D
    {
        static Geometry geo;

        public Sprite(Renderer renderer, SpriteMaterial material)
        {
            if(geo == null)
            {
                geo = new Geometry();
                geo.vertices.Add(new Vector3(-0.5f, -0.5f, 0));
                geo.vertices.Add(new Vector3(0.5f, -0.5f, 0));
                geo.vertices.Add(new Vector3(0.5f, 0.5f, 0));
            }

            geometry = geo;
            Material = material ?? new SpriteMaterial(renderer);
        }
    }
}
