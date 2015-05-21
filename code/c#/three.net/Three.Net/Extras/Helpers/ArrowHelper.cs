using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Geometries;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class ArrowHelper : Object3D
    {
        private static Geometry lineGeometry;
        private static CylinderGeometry coneGeometry;
        private Line line;
        private Mesh cone;

        public static ArrowHelper Create(Renderer renderer, Vector3 dir, Vector3 origin, float length = 1, Color? color = null, float? headLength = null, float? headWidth = null)
        {
            var c = color.HasValue ? color.Value : new Color(0xffff00);

            if (lineGeometry == null)
            {
                lineGeometry = new Geometry();
                lineGeometry.vertices.Add(Vector3.Zero);
                lineGeometry.vertices.Add(Vector3.UnitY);
                lineGeometry.vertexColors = new List<Color>()
                {
                    c,
                    c,
                };
            }

            if (coneGeometry == null)
            {
                coneGeometry = new CylinderGeometry(0, 0.5f, 1, 5, 1);
                var m = Matrix4.MakeTranslation(0, -0.5f, 0);
                coneGeometry.Apply(m);
            }

            var arrowHelper = new ArrowHelper();

            // dir is assumed to be normalized
            
            var head = headLength.HasValue ? headLength.Value : 0.2f * length;
            var width = headWidth.HasValue ? headWidth.Value : 0.2f * head;

            arrowHelper.Position = origin;
            arrowHelper.line = new Line(lineGeometry, new LineBasicMaterial(renderer)
            {
                Diffuse = c,
                VertexColors = Net.Renderers.VertexColorMode.Vertex
            });
            arrowHelper.line.matrixAutoUpdate = false;
            arrowHelper.Add(arrowHelper.line);

            arrowHelper.cone = new Mesh(coneGeometry, new MeshBasicMaterial(renderer)
            {
                Diffuse = c
            });
            arrowHelper.cone.matrixAutoUpdate = false;
            arrowHelper.Add(arrowHelper.cone);

            arrowHelper.SetDirection(dir.Normalized());
            arrowHelper.SetLength(length, head, width);

            return arrowHelper;
        }

        private void SetDirection(Vector3 dir)
        {
            // dir is assumed to be normalized
            if (dir.y > 0.99999f) Quaternion = Quaternion.Identity;
            else if (dir.y < -0.99999f) Quaternion = new Quaternion(1, 0, 0, 0);
            else
            {
                var axis = new Vector3(dir.z, 0, -dir.x);
                axis.Normalize();
                var radians = Mathf.Acos(dir.y);
                Quaternion = Quaternion.FromAxisAngle(axis, radians);
            }
        }

        private void SetLength(float length, float headLength, float headWidth)
        {
            line.Scale = new Vector3(1, length, 1);
            line.UpdateMatrix();

            cone.Scale = new Vector3(headWidth, headLength, headWidth);
            var p = cone.Position;
            p.y = length;
            cone.Position = p;
            cone.UpdateMatrix();
        }
    }
}
