using System.Collections.Generic;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class AxisHelper : Line
    {
        private AxisHelper(Geometry geometry, LineBasicMaterial material): base(geometry, material, LineType.Pieces)
        {

        }

        public static AxisHelper Create(Renderer renderer, float size = 1)
        {
            var geometry = new Geometry()
            {
                vertices = new List<Vector3>()
                {
                    Vector3.Zero,  new Vector3(size, 0, 0),
		     Vector3.Zero,  new Vector3(0, size, 0),
		     Vector3.Zero,  new Vector3(0, 0, size)
                },
                vertexColors = new List<Color>()
                {
                    new Color(1, 0, 0),  new Color(1, 0.5f, 0.5f),
		            new Color(0, 1, 0),  new Color(0.5f, 1, 0.5f),
		            new Color(0, 0, 1),  new Color(0.5f, 0.5f, 1),
                }
            };

            var axis = new AxisHelper(geometry, new LineBasicMaterial(renderer)
            {
                VertexColors = Net.Renderers.VertexColorMode.Vertex
            });
            return axis;
        }
    }
}
