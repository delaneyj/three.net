using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Shaders;

namespace Three.Net.Materials
{
    public class MeshLambertMaterial : MeshBasicMaterial
    {
        public Color Ambient { get; set; }
        public Color Emissive { get; set; }
        public bool ShouldWrapAround { get; set; }
        public Color WrapRGB { get; set; }

        private static string vertexShaderSource;
        private static string fragmentShaderSource;

        public MeshLambertMaterial(Renderer renderer) : base(renderer)
        {
            Ambient = Color.White;
            Emissive = Color.Black;
            ShouldWrapAround = false;
            WrapRGB = Color.White;
            UseLights = true;
        }

        protected override void RefreshUniforms()
        {
            base.RefreshUniforms();

            Program.SetUniformData("ambient", Renderer.GammaInput ? Ambient.GammaToLinear() : Ambient);
            Program.SetUniformData("emissive", Renderer.GammaInput ?  Emissive.GammaToLinear() : Emissive);
            if (ShouldWrapAround)Program.SetUniformData("wrapRGB", WrapRGB);
        }

        protected override void CreateVertexShaderSource()
        {
            var lines = new List<string>();

            lines.Add("#define LAMBERT");

            lines.Add(ShaderChunks.lightfront_pars_vertex);

            if (Side == SideMode.Double)
            {
                lines.Add(ShaderChunks.lightback_pars_vertex);
            }

            if ((DiffuseMap != null) || (SpecularMap != null) || (AlphaMap != null) || (NormalMap != null) || (BumpMap != null)) 
            {
                lines.Add(ShaderChunks.map_pars_vertex);
            }

            if (LightMap != null)
            {
                lines.Add(ShaderChunks.lightmap_pars_vertex);
            }

            if ((EnvironmentMap != null) && (NormalMap == null) && (BumpMap == null))
            {
                lines.Add(ShaderChunks.envmap_pars_vertex);
            }

            lines.Add(ShaderChunks.lights_lambert_pars_vertex);

            if (ShouldWrapAround)
            {
                lines.Add(ShaderChunks.lights_lambert_pars_vertex_wrap);
            }

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_pars_vertex);
            }

            if (UseMorphTargets)
            {
                if (UseMorphNormals)
                {
                    lines.Add(ShaderChunks.morphtarget_pars_vertex_normals);
                }
                else
                {
                    lines.Add(ShaderChunks.morphtarget_pars_vertex_nonormals);
                }
            }

            if (UseSkinning)
            {
                lines.Add(ShaderChunks.skinning_pars_vertex);
            }

            lines.Add(ShaderChunks.shadowmap_pars_vertex);

            if (UseLogarithmicDepthBuffer)
            {
                lines.Add(ShaderChunks.logdepthbuf_pars_vertex);
            }

            lines.Add("void main() {");

            if ((DiffuseMap != null) || (SpecularMap != null) || (AlphaMap != null) || (NormalMap != null) || (BumpMap != null))
            {
                lines.Add(ShaderChunks.map_vertex);
            }

            if (LightMap != null)
            {
                lines.Add(ShaderChunks.lightmap_vertex);
            }

            if (VertexColors != VertexColorMode.None)
            {
                if (Renderer.GammaInput)
                {
                    lines.Add(ShaderChunks.color_vertex_gamma);
                }
                else
                {
                    lines.Add(ShaderChunks.color_vertex_nogamma);
                }
            }

            if (UseMorphNormals)
            {
                lines.Add(ShaderChunks.morphnormal_vertex);
            }

            if (UseSkinning)
            {
                lines.Add(ShaderChunks.skinbase_vertex);
            }

            if (UseSkinning)
            {
                lines.Add(ShaderChunks.skinnormal_vertex);

                if (UseMorphNormals)
                {
                    lines.Add(ShaderChunks.skinnormal_vertex_normals);
                }
                else
                {
                    lines.Add(ShaderChunks.skinnormal_vertex_nonormals);
                }
            }

            lines.Add(ShaderChunks.defaultnormal_vertex);

            if (UseSkinning)
            {
                lines.Add(ShaderChunks.defaultnormal_vertex_skinning);
            }
            else
            {
                if (UseMorphNormals)
                {
                    lines.Add(ShaderChunks.defaultnormal_vertex_morph);
                }
                else
                {
                    lines.Add(ShaderChunks.defaultnormal_vertex_noskin_nomorph);
                }
            }

            if (Side == SideMode.Back)
            {
                lines.Add(ShaderChunks.defaultnormal_vertex_flip);
            }

            lines.Add(ShaderChunks.defaultnormal_vertex_transformed);

            if (UseMorphTargets)
            {
                lines.Add(ShaderChunks.morphtarget_vertex);

                if(UseMorphNormals)
                {
                    lines.Add(ShaderChunks.morphtarget_vertex_morph);
                }

                lines.Add(ShaderChunks.morphtarget_vertex_position);
            }

            if (UseSkinning)
            {
                if (UseMorphTargets)
                {
                    lines.Add(ShaderChunks.skinning_vertex_morph);
                }
                else
                {
                    lines.Add(ShaderChunks.skinning_vertex_nomorph);
                }

                lines.Add(ShaderChunks.skinning_vertex);
            }

            
            lines.Add(ShaderChunks.default_vertex);

            if (UseSkinning)
            {
                lines.Add(ShaderChunks.default_vertex_skinning);
            }
            else
            {
                if (UseMorphNormals)
                {
                    lines.Add(ShaderChunks.default_vertex_morph);
                }
                else
                {
                    lines.Add(ShaderChunks.default_vertex_noskin_nomorph);
                }
            }

            lines.Add(ShaderChunks.default_vertex_glposition);

            if (UseLogarithmicDepthBuffer)
            {
                lines.Add(ShaderChunks.logdepthbuf_vertex);
            }

            lines.Add(ShaderChunks.worldpos_vertex);

            if ((EnvironmentMap != null) && (NormalMap == null) && (BumpMap == null))
            {
                lines.Add(ShaderChunks.envmap_vertex);
            }

            lines.Add(ShaderChunks.lights_lambert_vertex);

            lines.Add(ShaderChunks.shadowmap_vertex);

            lines.Add("}");

            vertexShaderSource = string.Join(Environment.NewLine, lines);
        }

        protected override void CreateFragmentShaderSource()
        {
            var lines = new List<string>();

            lines.Add(ShaderChunks.opacity_pars_fragment);

            lines.Add(ShaderChunks.lightfront_pars_fragment);

            if (Side == SideMode.Double)
            {
                lines.Add(ShaderChunks.lightback_pars_fragment);
            }

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_pars_fragment);
            }

            if ((DiffuseMap != null) || (SpecularMap != null) || (AlphaMap != null) || (NormalMap != null) || (BumpMap != null))
            {
                lines.Add(ShaderChunks.map_pars_fragment);
                
                if (DiffuseMap != null)
                {
                    lines.Add(ShaderChunks.map_pars_fragment_diffuse);
                }
            }

            if (AlphaMap != null)
            {
                lines.Add(ShaderChunks.alphamap_pars_fragment);
            }

            if (LightMap != null)
            {
                lines.Add(ShaderChunks.lightmap_pars_fragment);
            }

            if (EnvironmentMap != null)
            {
                lines.Add(ShaderChunks.envmap_pars_fragment);

                if ((BumpMap != null) || (NormalMap != null))
                {
                    lines.Add(ShaderChunks.envmap_pars_fragment_refract);
                }
                else
                {
                    lines.Add(ShaderChunks.envmap_pars_fragment_reflect);
                }
            }

            lines.Add(ShaderChunks.fog_pars_fragment);

            lines.Add(ShaderChunks.shadowmap_pars_fragment);

            if (SpecularMap != null)
            {
                lines.Add(ShaderChunks.specularmap_pars_fragment);
            }

            if (UseLogarithmicDepthBuffer)
            {
                lines.Add(ShaderChunks.logdepthbuf_pars_fragment);
            }

            lines.Add("void main() {");
            lines.Add(ShaderChunks.opacity_fragment);

            lines.Add(ShaderChunks.logdepthbuf_fragment);


            if (DiffuseMap != null)
            {
                lines.Add(ShaderChunks.map_fragment);

                if (Renderer.GammaInput)
                {
                    lines.Add(ShaderChunks.map_fragment_gammainput);
                }

                lines.Add(ShaderChunks.map_fragment_glfragcolor);
            }

            if (AlphaMap != null)
            {
                lines.Add(ShaderChunks.alphamap_fragment);
            }

            lines.Add(ShaderChunks.alphatest_fragment);

            lines.Add(ShaderChunks.specularmap_fragment);

            if (SpecularMap != null)
            {
                lines.Add(ShaderChunks.specularmap_fragment_specmap);
            }
            else
            {
                lines.Add(ShaderChunks.specularmap_fragment_nospecmap);
            }

            if (Side == SideMode.Double)
            {
                lines.Add(ShaderChunks.doubleside_color_fragment);
            }
            else
            {
                lines.Add(ShaderChunks.oneside_color_fragment);
            }

            if (LightMap != null)
            {
                lines.Add(ShaderChunks.lightmap_fragment);
            }

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_fragment);
            }

            if (EnvironmentMap != null)
            {
                lines.Add(ShaderChunks.envmap_fragment);

                if ((BumpMap != null) || (NormalMap != null))
                {
                    lines.Add(ShaderChunks.envmap_fragment_bumpnormal);
                }
                else
                {
                    lines.Add(ShaderChunks.envmap_fragment_nobumpnormal);
                }

                if (Side == SideMode.Double)
                {
                    lines.Add(ShaderChunks.envmap_fragment_doublesided);
                }
                else
                {
                    lines.Add(ShaderChunks.envmap_fragment_notdoublesided);
                }

                if (Renderer.GammaInput)
                {
                    lines.Add(ShaderChunks.envmap_fragment_gammainput);
                }

                lines.Add(ShaderChunks.envmap_fragment_combine);
            }

            lines.Add(ShaderChunks.shadowmap_fragment);

            if (Renderer.GammaOutput)
            {
                lines.Add(ShaderChunks.linear_to_gamma_fragment);
            }

            lines.Add(ShaderChunks.fog_fragment);

            lines.Add("}");

            fragmentShaderSource = string.Join(Environment.NewLine, lines);
        }

        public override string VertexShaderSource
        {
            get
            {
                if (vertexShaderSource == null)
                {
                    CreateVertexShaderSource();
                }

                return vertexShaderSource;
            }
        }
        public override string FragmentShaderSource
        {
            get
            {
                if (fragmentShaderSource == null)
                {
                    CreateFragmentShaderSource();
                }

                return fragmentShaderSource;
            }
        }
    }
}
