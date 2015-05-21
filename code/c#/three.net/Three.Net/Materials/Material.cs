using Pencil.Gaming.Graphics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Shaders;
using Three.Net.Scenes;
using Three.Net.Textures;

namespace Three.Net.Materials
{
    public abstract class Material
    {
        public Color Diffuse { get; set; }
        public VertexColorMode VertexColors = VertexColorMode.None;

        private static uint nextId = 0;
        public string Name = string.Empty;

        public readonly uint Id = nextId++;
        public readonly Guid Guid = Guid.NewGuid();

        internal SideMode Side = SideMode.Front;

        internal bool IsTransparent = false;

        internal ShadingMode Shading = ShadingMode.None;
        internal BlendMode Blending = BlendMode.Normal;        
        internal BlendingFactorSrc BlendSource = BlendingFactorSrc.SrcAlpha;
        internal BlendingFactorDest BlendDestination = BlendingFactorDest.OneMinusSrcAlpha;
        internal BlendEquationMode BlendEquation = BlendEquationMode.FuncAdd;

        protected bool UseLogarithmicDepthBuffer;

        public bool ShouldDepthTest = true;
        public bool ShouldDepthWrite = true;

        internal bool ShouldPolygonOffset = false;
        internal float PolygonOffsetFactor = 0;
        internal float PolygonOffsetUnits = 0;

        public bool UseAlphaTest = false;
        internal float ShouldAlphaTest = 0;

        internal bool IsVisible = true;
        internal bool DoesNeedUpdate = true;
        public bool UseFog { get; set; }

        internal Program Program;
        protected Renderer Renderer;
        public bool UseLights = false;

        public abstract string VertexShaderSource { get; }
        public abstract string FragmentShaderSource { get; }

        public Material(Renderer renderer)
        {
            Diffuse = Color.White;
            Renderer = renderer;
            UseFog  = true;
        }

        internal void RefreshAllUniforms()
        {
            Program.SetUniformData("diffuse", Diffuse);

            RefreshUniforms();
        }

        protected abstract void RefreshUniforms();
    }
}
