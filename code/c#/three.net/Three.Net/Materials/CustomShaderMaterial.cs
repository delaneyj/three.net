using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Renderers.GL4;

namespace Three.Net.Materials
{
    public class CustomShaderMaterial : Material
    {
        private readonly string vertexShaderSource, fragmentShaderSource;
        private readonly Action<CustomShaderMaterial> refreshUniforms;

        public CustomShaderMaterial(Renderer renderer, string vertexShaderSource, string fragmentShaderSource, Action<CustomShaderMaterial> refreshUniforms):base(renderer)
        {
            this.vertexShaderSource = vertexShaderSource;
            this.fragmentShaderSource = fragmentShaderSource;
            this.refreshUniforms = refreshUniforms;
        }

        public override string VertexShaderSource { get { return vertexShaderSource; } }

        public override string FragmentShaderSource { get { return fragmentShaderSource; } }

        protected override void RefreshUniforms()
        {
            refreshUniforms(this);
        }
    }
}
