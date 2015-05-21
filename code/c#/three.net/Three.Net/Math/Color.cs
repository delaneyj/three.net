using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public static class ColorExtension
    {
        public static Vector3 ToVector3(this Color color)
        {
            return new Vector3(color.R, color.G, color.B);
        }

        public static System.Drawing.Color ToSystemDrawing(this Color color)
        {
            var r = Mathf.Round(color.R * 255);
            var g = Mathf.Round(color.G * 255);
            var b = Mathf.Round(color.B * 255);
            return System.Drawing.Color.FromArgb(r,g,b);
        }
    }

    public struct Color : IEquatable<Color>
    {
        public static Color Aliceblue = new Color(0xF0F8FF);
        public static Color Antiquewhite = new Color(0xFAEBD7);
        public static Color Aqua = new Color(0x00FFFF);
        public static Color Aquamarine = new Color(0x7FFFD4);
        public static Color Azure = new Color(0xF0FFFF);
        public static Color Beige = new Color(0xF5F5DC); 
        public static Color Bisque = new Color(0xFFE4C4);
        public static Color Black = new Color(0x000000);
        public static Color Blanchedalmond = new Color(0xFFEBCD);
        public static Color Blue = new Color(0x0000FF);
        public static Color Blueviolet = new Color(0x8A2BE2);
        public static Color Brown = new Color(0xA52A2A);
        public static Color Burlywood = new Color(0xDEB887);
        public static Color Cadetblue = new Color(0x5F9EA0);
        public static Color Chartreuse = new Color(0x7FFF00);
        public static Color Chocolate = new Color(0xD2691E);
        public static Color Coral = new Color(0xFF7F50);
        public static Color Cornflowerblue = new Color(0x6495ED);
        public static Color Cornsilk = new Color(0xFFF8DC);
        public static Color Crimson = new Color(0xDC143C);
        public static Color Cyan = new Color(0x00FFFF);
        public static Color Darkblue = new Color(0x00008B);
        public static Color Darkcyan = new Color(0x008B8B);
        public static Color Darkgoldenrod = new Color(0xB8860B);
        public static Color Darkgray = new Color(0xA9A9A9);
        public static Color Darkgreen = new Color(0x006400);
        public static Color Darkgrey = new Color(0xA9A9A9);
        public static Color Darkkhaki = new Color(0xBDB76B);
        public static Color Darkmagenta = new Color(0x8B008B);
        public static Color Darkolivegreen = new Color(0x556B2F); 
        public static Color Darkorange = new Color(0xFF8C00); 
        public static Color Darkorchid = new Color(0x9932CC); 
        public static Color Darkred = new Color(0x8B0000); 
        public static Color Darksalmon = new Color(0xE9967A); 
        public static Color Darkseagreen = new Color(0x8FBC8F);
        public static Color Darkslateblue = new Color(0x483D8B); 
        public static Color Darkslategray = new Color(0x2F4F4F); 
        public static Color Darkslategrey = new Color(0x2F4F4F); 
        public static Color Darkturquoise = new Color(0x00CED1); 
        public static Color Darkviolet = new Color(0x9400D3);
        public static Color Deeppink = new Color(0xFF1493); 
        public static Color Deepskyblue = new Color(0x00BFFF); 
        public static Color Dimgray = new Color(0x696969); 
        public static Color Dimgrey = new Color(0x696969); 
        public static Color Dodgerblue = new Color(0x1E90FF); 
        public static Color Firebrick = new Color(0xB22222);
        public static Color Floralwhite = new Color(0xFFFAF0);
        public static Color Forestgreen = new Color(0x228B22); 
        public static Color Fuchsia = new Color(0xFF00FF); 
        public static Color Gainsboro = new Color(0xDCDCDC); 
        public static Color Ghostwhite = new Color(0xF8F8FF); 
        public static Color Gold = new Color(0xFFD700);
        public static Color Goldenrod = new Color(0xDAA520); 
        public static Color Gray = new Color(0x808080); 
        public static Color Green = new Color(0x008000); 
        public static Color Greenyellow = new Color(0xADFF2F); 
        public static Color Grey = new Color(0x808080); 
        public static Color Honeydew = new Color(0xF0FFF0); 
        public static Color Hotpink = new Color(0xFF69B4);
        public static Color Indianred = new Color(0xCD5C5C); 
        public static Color Indigo = new Color(0x4B0082); 
        public static Color Ivory = new Color(0xFFFFF0); 
        public static Color Khaki = new Color(0xF0E68C); 
        public static Color Lavender = new Color(0xE6E6FA); 
        public static Color Lavenderblush = new Color(0xFFF0F5); 
        public static Color Lawngreen = new Color(0x7CFC00);
        public static Color Lemonchiffon = new Color(0xFFFACD); 
        public static Color Lightblue = new Color(0xADD8E6); 
        public static Color Lightcoral = new Color(0xF08080); 
        public static Color Lightcyan = new Color(0xE0FFFF); 
        public static Color Lightgoldenrodyellow = new Color(0xFAFAD2); 
        public static Color Lightgray = new Color(0xD3D3D3);
        public static Color Lightgreen = new Color(0x90EE90); 
        public static Color Lightgrey = new Color(0xD3D3D3);
        public static Color Lightpink = new Color(0xFFB6C1); 
        public static Color Lightsalmon = new Color(0xFFA07A); 
        public static Color Lightseagreen = new Color(0x20B2AA); 
        public static Color Lightskyblue = new Color(0x87CEFA);
        public static Color Lightslategray = new Color(0x778899); 
        public static Color Lightslategrey = new Color(0x778899); 
        public static Color Lightsteelblue = new Color(0xB0C4DE); 
        public static Color Lightyellow = new Color(0xFFFFE0); 
        public static Color Lime = new Color(0x00FF00); 
        public static Color Limegreen = new Color(0x32CD32);
        public static Color Linen = new Color(0xFAF0E6); 
        public static Color Magenta = new Color(0xFF00FF); 
        public static Color Maroon = new Color(0x800000); 
        public static Color Mediumaquamarine = new Color(0x66CDAA); 
        public static Color Mediumblue = new Color(0x0000CD); 
        public static Color Mediumorchid = new Color(0xBA55D3);
        public static Color Mediumpurple = new Color(0x9370DB); 
        public static Color Mediumseagreen = new Color(0x3CB371); 
        public static Color Mediumslateblue = new Color(0x7B68EE); 
        public static Color Mediumspringgreen = new Color(0x00FA9A); 
        public static Color Mediumturquoise = new Color(0x48D1CC);
        public static Color Mediumvioletred = new Color(0xC71585); 
        public static Color Midnightblue = new Color(0x191970); 
        public static Color Mintcream = new Color(0xF5FFFA); 
        public static Color Mistyrose = new Color(0xFFE4E1); 
        public static Color Moccasin = new Color(0xFFE4B5); 
        public static Color Navajowhite = new Color(0xFFDEAD);
        public static Color Navy = new Color(0x000080); 
        public static Color Oldlace = new Color(0xFDF5E6); 
        public static Color Olive = new Color(0x808000); 
        public static Color Olivedrab = new Color(0x6B8E23); 
        public static Color Orange = new Color(0xFFA500); 
        public static Color Orangered = new Color(0xFF4500); 
        public static Color Orchid = new Color(0xDA70D6);
        public static Color Palegoldenrod = new Color(0xEEE8AA); 
        public static Color Palegreen = new Color(0x98FB98); 
        public static Color Paleturquoise = new Color(0xAFEEEE); 
        public static Color Palevioletred = new Color(0xDB7093); 
        public static Color Papayawhip = new Color(0xFFEFD5); 
        public static Color Peachpuff = new Color(0xFFDAB9);
        public static Color Peru = new Color(0xCD853F); 
        public static Color Pink = new Color(0xFFC0CB); 
        public static Color Plum = new Color(0xDDA0DD); 
        public static Color Powderblue = new Color(0xB0E0E6); 
        public static Color Purple = new Color(0x800080); 
        public static Color Red = new Color(0xFF0000); 
        public static Color Rosybrown = new Color(0xBC8F8F);
        public static Color Royalblue = new Color(0x4169E1); 
        public static Color Saddlebrown = new Color(0x8B4513); 
        public static Color Salmon = new Color(0xFA8072); 
        public static Color Sandybrown = new Color(0xF4A460); 
        public static Color Seagreen = new Color(0x2E8B57); 
        public static Color Seashell = new Color(0xFFF5EE);
        public static Color Sienna = new Color(0xA0522D); 
        public static Color Silver = new Color(0xC0C0C0); 
        public static Color Skyblue = new Color(0x87CEEB); 
        public static Color Slateblue = new Color(0x6A5ACD); 
        public static Color Slategray = new Color(0x708090); 
        public static Color Slategrey = new Color(0x708090); 
        public static Color Snow = new Color(0xFFFAFA);
        public static Color Springgreen = new Color(0x00FF7F); 
        public static Color Steelblue = new Color(0x4682B4); 
        public static Color Tan = new Color(0xD2B48C); 
        public static Color Teal = new Color(0x008080); 
        public static Color Thistle = new Color(0xD8BFD8); 
        public static Color Tomato = new Color(0xFF6347); 
        public static Color Turquoise = new Color(0x40E0D0);
        public static Color Violet = new Color(0xEE82EE); 
        public static Color Wheat = new Color(0xF5DEB3); 
        public static Color White = new Color(0xFFFFFF); 
        public static Color Whitesmoke = new Color(0xF5F5F5); 
        public static Color Yellow = new Color(0xFFFF00); 
        public static Color Yellowgreen = new Color(0x9ACD32);

        public float R, G, B;

        public Color(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }

        public Color(uint hex)
        {
            R = (hex >> 16 & 255) / 255f;
            G = (hex >> 8 & 255) / 255f;
            B = (hex & 255) / 255f;
        }

        public Color(Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
        }

        internal Color GammaToLinear()
        {
            R *= R;
            G *= G;
            B *= B;
            return this;
        }

        internal void SetColorGamma(float intensity)
        {
            var sq = intensity * intensity;
            R = R * R * sq;
            G = G * G * sq;
            B = B * B * sq;
        }

        internal void SetColorLinear(float intensity)
        {
            R = R * intensity;
            G = G * intensity;
            B = B * intensity;
        }

        public static Color FromHSV(float h, float s, float v)
        {
            float r = 0, g = 0, b = 0;

            var step = h / (1 / 6f);
            var pos = step - Mathf.Floor(step); // the hue position within the current step
            var m = (Mathf.Floor(step) % 2 != 0) ? (1 - pos) * v : pos * v; // mix color value adjusted to the brightness(v)
            var max = 1 * v;
            var min = (1 - s) * v;
            var med = m + ((1 - s) * (v - m));

            switch (Mathf.Floor(step))
            {
                case 0:
                    r = max;
                    g = med;
                    b = min;
                    break;
                case 1:
                    r = med;
                    g = max;
                    b = min;
                    break;
                case 2:
                    r = min;
                    g = max;
                    b = med;
                    break;
                case 3:
                    r = min;
                    g = med;
                    b = max;
                    break;
                case 4:
                    r = med;
                    g = min;
                    b = max;
                    break;
                case 5:
                    r = max;
                    g = min;
                    b = med;
                    break;
            }

            return new Color(r, g, b);
        }

        public override int GetHashCode()
        {
            return R.GetHashCode() ^ G.GetHashCode() << 12 ^ B.GetHashCode() << 24;
        }

        public bool Equals(Color c)
        {
            return c.R == R && c.G == G && c.B == B;
        }

        public override bool Equals(object obj)
        {
            var c = (Color)obj;
            if (c == null) return false;
            return Equals(c);
        }

        public static bool operator ==(Color left, Color right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Color left, Color right)
        {
            return !left.Equals(right);
        }

        public Color Multiply(float value)
        {
            return new Color(R * value, G * value, B * value);
        }

        public static Color Random()
        {
            return new Color(Mathf.RandomF(),Mathf.RandomF(),Mathf.RandomF());
        }

        public static Color Lerp(Color minColor, Color maxColor, float alpha)
        {
            var r = minColor.R + (maxColor.R - minColor.R) * alpha;
            var g = minColor.G + (maxColor.G - minColor.G) * alpha;
            var b = minColor.B + (maxColor.B - minColor.B) * alpha;

            return new Color(r,g,b);
        }
    }
}
