using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class BoxHelper : Line
    {
        Object3D o;

        private BoxHelper(Geometry geometry, LineBasicMaterial material)
            : base(geometry, material, LineType.Pieces)
        {

        }
        public static BoxHelper Create(Renderer renderer, Object3D o)
        {
            var mat = new LineBasicMaterial(renderer)
            {
                Diffuse = new Color(0xffff00)
            };
            var geo = new Geometry();
            var boxHelper = new BoxHelper(geo, mat);
            boxHelper.o = o;
            boxHelper.Update();
            return boxHelper;
        }

        public void Update()
        {
            var geometry = o.geometry;
            if (geometry != null)
            {
                if (geometry.BoundingBox == Box3.Empty) geometry.ComputeBoundingBox();

                var min = geometry.BoundingBox.min;
                var max = geometry.BoundingBox.max;

                /*
                  5____4
                1/___0/|
                | 6__|_7
                2/___3/

                0: max.x, max.y, max.z
                1: min.x, max.y, max.z
                2: min.x, min.y, max.z
                3: max.x, min.y, max.z
                4: max.x, max.y, min.z
                5: min.x, max.y, min.z
                6: min.x, min.y, min.z
                7: max.x, min.y, min.z
                */

                geometry.vertices = new List<Vector3>()
            {
                new Vector3(max.x,max.y,max.z),
                new Vector3(min.x,max.y,max.z),
                new Vector3(min.x,max.y,max.z),
                new Vector3(min.x, min.y,max.z),
                new Vector3( min.x,min.y,max.z),
                new Vector3( max.x,min.y,max.z),
                new Vector3( max.x,min.y,max.z),
                new Vector3( max.x,max.y,max.z),
                //
               new Vector3(max.x,max.y,min.z),
               new Vector3(min.x,max.y,min.z),
                new Vector3(min.x,max.y,min.z),
                new Vector3(min.x,min.y,min.z),
                new Vector3(min.x,min.y, min.z),
                new Vector3(max.x,min.y, min.z),
               new Vector3( max.x,min.y, min.z),
               new Vector3( max.x,max.y, min.z),
                //
                new Vector3(max.x,max.y, max.z),
                new Vector3(max.x,max.y, min.z),
                new Vector3(min.x,max.y,max.z),
                new Vector3(min.x,max.y,min.z),
                new Vector3(min.x,min.y,max.z),
                new Vector3(min.x,min.y,min.z),
                new Vector3(max.x,min.y,max.z),
                new Vector3(max.x,min.y,min.z),
            };
                geometry.VerticesNeedUpdate = true;
                geometry.ComputeBoundingSphere();

                matrixAutoUpdate = false;
                matrixWorld = o.matrixWorld;
            }
        }
    }
}
