using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Geometries;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class BoundingBoxHelper : Mesh
    {
        private Object3D o;
        private Box3 box;

        private BoundingBoxHelper(Geometry geometry, MeshBasicMaterial material)
            : base(geometry, material)
        {

        }

        // a helper to show the world-axis-aligned bounding box for an object
        public static BoundingBoxHelper Create(Renderer renderer, Object3D o, Color? color = null)
        {
            var c = color.HasValue ? color.Value : new Color(0x888888);
            var geo = new BoxGeometry(1, 1, 1);
            var mat = new MeshBasicMaterial(renderer)
            {
                Diffuse = c,
                UseWireframe = true
            };
            var boundingBoxHelper = new BoundingBoxHelper(geo, mat);
            boundingBoxHelper.o = o;
            boundingBoxHelper.box = Box3.Empty;
            return boundingBoxHelper;
        }

        public void Update()
        {
            box = Box3.FromObject(o);
            Scale = box.Size();
            Position = box.Center();
        }
    }
}