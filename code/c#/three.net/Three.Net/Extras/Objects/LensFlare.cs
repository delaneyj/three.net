using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;
using Three.Net.Renderers;
using Three.Net.Textures;

namespace Three.Net.Extras.Objects
{
    public class LensFlare : Object3D
    {
        public class LensFlareInfo
        {
            public Texture Texture;
            public float Size, Distance;
            public float x, y, Z;
            public float Scale, Opacity, Rotation, WantedRotation;
            public Color Color;
            public BlendMode Blending;
        }

        Vector3 PositionScreen = Vector3.Zero;
        List<LensFlareInfo> lenFlareInfos = new List<LensFlareInfo>();

        public LensFlare(Texture texture, float size, float distance, BlendMode blending, Color color)
        {
            if (texture != null) AddFlare(color, texture, size, distance, blending);
        }

        private void AddFlare(Color color, Texture texture, float size = -1, float distance = 0, BlendMode blending = BlendMode.Normal, float opacity = 1)
        {
            distance = Mathf.Min(distance, Mathf.Max(0, distance));
            var lensFlareInfo = new LensFlareInfo()
            {
                Texture = texture, 			// THREE.Texture
                Size = size, 				// size in pixels (-1 = use texture.width)
                Distance = distance, 		// distance (0-1) from light source (0=at light source)
                x = 0,
                y = 0,
                Z = 0,		// screen position (-1 => 1) z = 0 is ontop z = 1 is back
                Scale = 1, 					// scale
                Rotation = 1, 				// rotation
                Opacity = opacity,			// opacity
                Color = color,				// color
                Blending = blending		// blending
            };
            lenFlareInfos.Add(lensFlareInfo);
        }

        private void AddFlare(Texture texture, float size = -1, float distance = 0, BlendMode blending = BlendMode.Normal, float opacity = 1)
        {
            AddFlare(Color.White, texture, size, distance, blending, opacity);
        }

        // Update lens flares update positions on all flares based on the screen position Set myLensFlare.customUpdateCallback to alter the flares in your project specific way.
        private void UpdateLensFlares()
        {
            var vecX = -PositionScreen.x * 2;
            var vecY = -PositionScreen.y * 2;
            foreach (var flare in lenFlareInfos)
            {
                flare.x = PositionScreen.x + vecX * flare.Distance;
                flare.y = PositionScreen.y + vecY * flare.Distance;

                flare.WantedRotation = flare.x * Mathf.Pi * 0.25f;
                flare.Rotation += (flare.WantedRotation - flare.Rotation) * 0.25f;
            }
        }
    }
}
