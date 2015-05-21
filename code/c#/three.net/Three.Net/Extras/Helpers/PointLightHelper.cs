using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Geometries;
using Three.Net.Lights;
using Three.Net.Materials;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class PointLightHelper : Mesh
    {
        private PointLight light;

        private PointLightHelper(PointLight light, Geometry geometry, MeshBasicMaterial material)
            : base(geometry, material)
        {
            this.light = light;
            light.UpdateMatrixWorld();
            matrixWorld = light.matrixWorld;
            matrixAutoUpdate = false;
        }

        public static PointLightHelper Create(Renderer renderer, PointLight light)
        {
            var geometry = new SphereGeometry(0.25f, 4, 3);
            var material = new MeshBasicMaterial(renderer)
            {
                UseWireframe = true,
                UseFog = false,
                Diffuse = light.Color.Multiply(light.Intensity)
            };
            var helper = new PointLightHelper(light, geometry, material);
            return helper;
        }

    }
}