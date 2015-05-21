using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;
using Three.Net.Renderers;

namespace Three.Net.Textures
{
    public class Texture
    {
        private static uint nextId = 0;

        public readonly uint Id = nextId++;
        public readonly Guid Guid = Guid.NewGuid();

        public string Name = string.Empty;
        public readonly string Path = string.Empty;

        public int GPUAddress { get; private set; }
        public Size Resolution { get; protected set; }

        public WrapMode WrapS = WrapMode.Clamp;
        public WrapMode WrapT = WrapMode.Clamp;
        public TextureMinFilter MinFilter = TextureMinFilter.LinearMipmapLinear;
        public TextureMagFilter MagFilter = TextureMagFilter.Linear;
        public TextureTarget TextureTarget { get; private set; }
        public Three.Net.Renderers.PixelFormat Format { get; set; }
        public Three.Net.Renderers.PixelType Type { get; set; }
        public float Anisotropy = 1;

        public Vector2 Offset = Vector2.Zero;
        public Vector2 Repeat = Vector2.One;

        public bool ShouldGenerateMipmaps = true;
        public bool PremultiplyAlpha = false;
        public int UnpackAlignment = 4; // valid values: 1, 2, 4, 8 (see http://www.khronos.org/opengles/sdk/docs/man/xhtml/glPixelStorei.xml)

        internal bool NeedsUpdate = true;
        public bool glInit;
        public int glTexture;

        internal Action OnUpdate = null;

        protected Texture()
        {
            Format = Three.Net.Renderers.PixelFormat.RGBA;
            Type = Renderers.PixelType.UnsignedByte;
        }

        public Texture(string bitmapPath, bool flipY = true)
        {
            Path = bitmapPath;
            using (var bitmap = Bitmap.FromFile(bitmapPath) as Bitmap)
            {
                HandleLoadingBitmapData(bitmap, flipY);
            }
        }

        public Texture(int width, int height, Three.Net.Math.Color color)
        {
            using (var bitmap = new Bitmap(width, height))
            {
                using (var g = Graphics.FromImage(bitmap))
                {
                    var brush = new SolidBrush(color.ToSystemDrawing());
                    g.FillRectangle(brush, new Rectangle(0, 0, width, height));
                }
            }
        }

        private void HandleLoadingBitmapData(Bitmap bitmap, bool flipY = true)
        {
            /* .net library has methods for converting many image formats so I exploit that by using 
              * .net to convert any filetype to a bitmap.  Then the bitmap is locked into memory so
              * that the garbage collector doesn't touch it, and it is read via OpenGL glTexImage2D. */
            if (flipY) bitmap.RotateFlip(RotateFlipType.RotateNoneFlipY);     // bitmaps read from bottom up, so flip it
            Resolution = bitmap.Size;

            // must be Format32bppArgb file format, so convert it if it isn't in that format
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            // set the texture target and then generate the texture ID
            TextureTarget = TextureTarget.Texture2D;
            GPUAddress = GL.GenTexture();

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1); // set pixel alignment
            GL.BindTexture(TextureTarget, GPUAddress);     // bind the texture to memory in OpenGL

            //Gl.TexParameteri(TextureTarget, TextureParameterName.GenerateMipmap, 0);
            GL.TexImage2D(TextureTarget, 0, PixelInternalFormat.Rgba8, bitmapData.Width, bitmapData.Height, 0, Pencil.Gaming.Graphics.PixelFormat.Bgra, Pencil.Gaming.Graphics.PixelType.UnsignedByte, bitmapData.Scan0);
            GL.TexParameter(TextureTarget, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexParameter(TextureTarget, TextureParameterName.TextureMinFilter, (int)MinFilter);


            if (ShouldGenerateMipmaps) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);

            NeedsUpdate = false;

            bitmap.UnlockBits(bitmapData);
        }
    }
}
