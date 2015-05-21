using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Shaders;

namespace Three.Net.Materials
{
    public class LineDashedMaterial : LineBasicMaterial
    {
        public float Scale{get;set;}
        public float DashSize{get;set;}
        public float GapSize { get; set; }

        private static string vertexShaderSource;
        private static string fragmentShaderSource;

        public LineDashedMaterial(Renderer renderer) : base(renderer)
        {
            Scale = 1f;
            DashSize = 0.03f;
            GapSize = 0.05f;
        }

        protected override void RefreshUniforms()
        {
            base.RefreshUniforms();

            Program.SetUniformData("dashSize", DashSize);
            Program.SetUniformData("totalSize", DashSize + GapSize);
            Program.SetUniformData("scale", Scale);
        }

        protected override void CreateVertexShaderSource()
        {
            var lines = new List<string>();

            lines.Add("uniform float scale;");
            lines.Add("in float lineDistance;");
            lines.Add("out float vLineDistance;");

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_pars_vertex);
            }

            lines.Add("void main() {");

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

            lines.Add("vLineDistance = scale * lineDistance;");

            lines.Add(ShaderChunks.default_vertex);

            lines.Add(ShaderChunks.default_vertex_noskin_nomorph);

            lines.Add(ShaderChunks.default_vertex_glposition);

            lines.Add("}");

            vertexShaderSource = string.Join(Environment.NewLine, lines);
        }

        protected override void CreateFragmentShaderSource()
        {
            var lines = new List<string>();

            lines.Add("uniform vec3 diffuse;");

            lines.Add(ShaderChunks.opacity_pars_fragment);

            lines.Add("uniform float dashSize;");
            lines.Add("uniform float totalSize;");
            lines.Add("in float vLineDistance;");

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_pars_fragment);
            }

            lines.Add(ShaderChunks.fog_pars_fragment);

            lines.Add("void main() {");

            lines.Add("if ( mod( vLineDistance, totalSize ) > dashSize ) {");
            lines.Add("discard;");
            lines.Add("}");

            lines.Add(ShaderChunks.opacity_fragment);

            if (VertexColors != VertexColorMode.None)
            {
                lines.Add(ShaderChunks.color_fragment);
            }

            if (Diffuse != null)
            {
                lines.Add(ShaderChunks.pure_diffuse_fragment);
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
