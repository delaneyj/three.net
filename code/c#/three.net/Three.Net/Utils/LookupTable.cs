using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Utils
{
    class LookupTable
    {
        public static readonly List<ColorInfo> Rainbow = new List<ColorInfo>(){ new ColorInfo(0, new Color(0x0000FF)), new ColorInfo(0.2f, new Color(0x00FFFF)), new ColorInfo(0.5f, new Color(0x00FF00)), new ColorInfo(0.8f, new Color(0xFFFF00)), new ColorInfo(1, new Color(0xFF0000))};
        public static readonly List<ColorInfo>  CoolToWarm = new List<ColorInfo>(){ new ColorInfo(0, new Color(0x3C4EC2)), new ColorInfo(0.2f,new Color(0x9BBCFF)), new ColorInfo(0.5f, new Color(0xDCDCDC)), new ColorInfo(0.8f, new Color(0xF6A385)), new ColorInfo(1, new Color(0xB40426))};
        public static readonly List<ColorInfo>  Blackbody = new List<ColorInfo>(){ new ColorInfo(0,  new Color(0x000000)),  new ColorInfo(0.2f, new Color(0x780000)),new ColorInfo(0.5f, new Color(0xE63200)), new ColorInfo( 0.8f, new Color(0xFFFF00)),  new ColorInfo(1, new Color(0xFFFFFF))};
        public static readonly List<ColorInfo> Grayscale = new List<ColorInfo>() { new ColorInfo(0, new Color(0x000000)), new ColorInfo(0.2f, new Color(0x404040)), new ColorInfo(0.5f, new Color(0x7F7F80)), new ColorInfo(0.8f, new Color(0xBFBFBF)), new ColorInfo(1, new Color(0xFFFFFF)) };

        public class ColorInfo
        {
            public readonly Color Color;
            public readonly float Position;

            public ColorInfo(float position, Color color)
            {
                Debug.Assert(position >= 0);
                Debug.Assert(position <= 1);
                Color = color;
                Position = position;
            }
        }

        private List<Color> lutTable = new List<Color>();

        public LookupTable(List<ColorInfo> colorInfos = null, int numberOfColors = 256)
        {
            colorInfos = colorInfos ?? Rainbow;
            var step = 1f / numberOfColors;

            for (var i = 0f; i <= 1; i += step)
            {
                for (var j = 0; j < colorInfos.Count - 1; j++)
                {
                    var current = colorInfos[j];
                    var next = colorInfos[j + 1];

                    if (i >= current.Position && i < next.Position)
                    {
                        var min = current.Position;
                        var max = next.Position;
                        var minColor = new Color(current.Color);
                        var maxColor = new Color(next.Color);
                        var color = Color.Lerp(minColor, maxColor, (i - min) / (max - min));

                        lutTable.Add(color);
                    }
                }
            }
        }
    }
}
