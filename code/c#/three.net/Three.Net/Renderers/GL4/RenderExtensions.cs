using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Renderers.GL4
{
    public static class RenderExtensions
    {
        public static Pencil.Gaming.Graphics.PixelFormat ToGL4(this PixelFormat format)
        {
            switch(format)
            {
                case PixelFormat.Alpha: return Pencil.Gaming.Graphics.PixelFormat.Alpha;
                case PixelFormat.Luminance: return Pencil.Gaming.Graphics.PixelFormat.Luminance;
                case PixelFormat.LuminanceAlpha: return Pencil.Gaming.Graphics.PixelFormat.LuminanceAlpha;
                case PixelFormat.RGB: return Pencil.Gaming.Graphics.PixelFormat.Rgb;
                case PixelFormat.RGBA: return Pencil.Gaming.Graphics.PixelFormat.Rgba;
                default: throw new NotSupportedException();
            }
        }

        public static Pencil.Gaming.Graphics.PixelInternalFormat ToGL4Internal(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.Alpha: return Pencil.Gaming.Graphics.PixelInternalFormat.Alpha;
                case PixelFormat.Luminance: return Pencil.Gaming.Graphics.PixelInternalFormat.Luminance;
                case PixelFormat.LuminanceAlpha: return Pencil.Gaming.Graphics.PixelInternalFormat.LuminanceAlpha;
                case PixelFormat.RGB: return Pencil.Gaming.Graphics.PixelInternalFormat.Rgb;
                case PixelFormat.RGBA: return Pencil.Gaming.Graphics.PixelInternalFormat.Rgba;
                default: throw new NotSupportedException();
            }
        }


        public static Pencil.Gaming.Graphics.PixelType ToGL4(this PixelType format)
        {
            switch (format)
            {
                case PixelType.Byte: return Pencil.Gaming.Graphics.PixelType.Byte;
                case PixelType.Short: return Pencil.Gaming.Graphics.PixelType.Short;
                case PixelType.UnsignedShort: return Pencil.Gaming.Graphics.PixelType.UnsignedShort;
                case PixelType.Int: return Pencil.Gaming.Graphics.PixelType.Int;
                case PixelType.UnsignedInt: return Pencil.Gaming.Graphics.PixelType.UnsignedInt;
                case PixelType.Float: return Pencil.Gaming.Graphics.PixelType.Float;
                case PixelType.UnsignedByte: return Pencil.Gaming.Graphics.PixelType.UnsignedByte;
                default: throw new NotSupportedException();
            }
        }
    }
}
