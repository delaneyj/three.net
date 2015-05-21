using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Renderers
{
    public enum TextureConstantMode
    {
        Multiply,
        Mix,
        Add
    }

    public enum WrapMode
    {
        Repeat,
        Clamp,
        MirroredRepeat
    }

    public enum VertexColorMode
    {
        None,
        Face,
        Vertex
    }

    public enum ShadowType
    {
        PCFShadowMap,
        PCFSoftShadowMap
    }

    public enum SideMode
    {
        Front,
        Back,
        Double
    }

    public enum ShadingMode
    {
        None,
        Smooth,
        Flat
    }

    public enum MaterialType
    {
        Default,
        Opaque,
        Transparent,
    }

    public enum BlendMode
    {
        None,
        Normal,
        Additive,
        Substractive,
        Multiply,
        Custom
    }

    public enum PixelFormat
    {
        Alpha,
        RGB,
        RGBA,
        Luminance,
        LuminanceAlpha
    }

    public enum PixelType
    {
        Byte,
        UnsignedByte,
        Short,
        UnsignedShort,
        Int,
        UnsignedInt,
        Float,
    }
}
