using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers.GL4;
using Three.Net.Textures;

namespace Three.Net.Renderers.Shaders
{
    public class UniformDefaults :Dictionary<string, dynamic>
    {
    }

    internal static class UniformsLib
    {
        private static Dictionary<string, UniformDefaults> sets = new Dictionary<string, UniformDefaults>();

        static UniformsLib()
        {
            var common = new UniformDefaults();
            common.Add("diffuse", Color.White);
            common.Add("opacity", 1);
            common.Add("offsetRepeat", new Vector4(0, 0, 1, 1));
            common.Add("flipEnvMap", -1);
            common.Add("useRefract", 0);
            common.Add("reflectivity", 1);
            common.Add("refractionRatio", 0.98f);
            common.Add("combine", 0);
            common.Add("morphTargetInfluences", 0);
            sets.Add("common", common);

            var bump = new UniformDefaults();
            bump.Add("bumpScale", 1);
            sets.Add("bump", bump);

            var normalmap = new UniformDefaults();
            normalmap.Add("normalMap", null);
            normalmap.Add("normalScale", Vector2.One);
            sets.Add("normalmap", normalmap);

            var fog = new UniformDefaults();
            fog.Add("fogDensity", 0.00025f);
            fog.Add("fogNear", 1);
            fog.Add("fogFar", 2000);
            fog.Add("fogColor", Color.White);
            sets.Add("fog", fog);

            var lights = new UniformDefaults();
            sets.Add("lights", lights);

            var particle = new UniformDefaults();
            particle.Add("psColor", Color.Whitesmoke);
            particle.Add("opacity", 1);
            particle.Add("size", 1);
            particle.Add("scale", 1);
            particle.Add("fogDensity", 0.00025f);
            particle.Add("fogNear", 1);
            particle.Add("fogFar", 2000);
            particle.Add("fogColor", Color.White);
            sets.Add("particle", particle);
        }

        public static dynamic UniformDefault(string key)
        {
            return sets[key];
        }
    }
}
