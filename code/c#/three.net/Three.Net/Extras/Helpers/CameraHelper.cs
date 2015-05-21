using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;

namespace Three.Net.Extras.Helpers
{
    public class CameraHelper : Line
    {
        private Dictionary<string, List<int>> pointMap = new Dictionary<string, List<int>>();
        private Camera camera;

        private CameraHelper(Camera camera, Geometry geometry, LineBasicMaterial material)
            : base(geometry, material, LineType.Pieces)
        {
            this.camera = camera;
            this.matrixWorld = camera.matrixWorld;
            this.matrixAutoUpdate = false;
        }

        public static CameraHelper Create(Renderer renderer, Camera camera)
        {
            var pointMap = new Dictionary<string, List<int>>();

            var geometry = new Geometry();
            var material = new LineBasicMaterial(renderer)
            {
                Diffuse = Color.White,
                VertexColors = VertexColorMode.Vertex
            };
            var cameraHelper = new CameraHelper(camera, geometry, material);

            //Colors
            var hexFrustum = new Color(0xffaa00);
            var hexCone = new Color(0xff0000);
            var hexUp = new Color(0x00aaff);
            var hexTarget = new Color(0xffffff);
            var hexCross = new Color(0x333333);

            // near
            cameraHelper.AddLine("n1", "n2", hexFrustum);
            cameraHelper.AddLine("n2", "n4", hexFrustum);
            cameraHelper.AddLine("n4", "n3", hexFrustum);
            cameraHelper.AddLine("n3", "n1", hexFrustum);
            
            // far
            cameraHelper.AddLine("f1", "f2", hexFrustum);
            cameraHelper.AddLine("f2", "f4", hexFrustum);
            cameraHelper.AddLine("f4", "f3", hexFrustum);
            cameraHelper.AddLine("f3", "f1", hexFrustum);
            
            // sides
            cameraHelper.AddLine("n1", "f1", hexFrustum);
            cameraHelper.AddLine("n2", "f2", hexFrustum);
            cameraHelper.AddLine("n3", "f3", hexFrustum);
            cameraHelper.AddLine("n4", "f4", hexFrustum);
            
            // cone
            cameraHelper.AddLine("p", "n1", hexCone);
            cameraHelper.AddLine("p", "n2", hexCone);
            cameraHelper.AddLine("p", "n3", hexCone);
            cameraHelper.AddLine("p", "n4", hexCone);
            
            // up
            cameraHelper.AddLine("u1", "u2", hexUp);
            cameraHelper.AddLine("u2", "u3", hexUp);
            cameraHelper.AddLine("u3", "u1", hexUp);
            
            // target
            cameraHelper.AddLine("c", "t", hexTarget);
            cameraHelper.AddLine("p", "c", hexCross);
            
            // cross
            cameraHelper.AddLine("cn1", "cn2", hexCross);
            cameraHelper.AddLine("cn3", "cn4", hexCross);
            cameraHelper.AddLine("cf1", "cf2", hexCross);
            cameraHelper.AddLine("cf3", "cf4", hexCross);

            cameraHelper.Update();
            return cameraHelper;
        }

        private void AddLine(string a, string b, Color hex)
        {
            AddPoint(a, hex);
            AddPoint(b, hex);
        }

        private void AddPoint(string id, Color hex)
        {
            geometry.vertices.Add(Vector3.Zero);

            if (geometry.vertexColors == null) geometry.vertexColors = new List<Color>();
            geometry.vertexColors.Add(hex);

            List<int> list;
            if (!pointMap.TryGetValue(id, out list))
            {
                list = new List<int>();
                pointMap[id] = list;
            }

            list.Add(geometry.vertices.Count - 1);
        }

        public void Update()
        {
            float w = 1, h = 1;

            // center / target
            SetPoint("c", 0, 0, -1);
            SetPoint("t", 0, 0, 1);

            // near
            SetPoint("n1", -w, -h, -1);
            SetPoint("n2", w, -h, -1);
            SetPoint("n3", -w, h, -1);
            SetPoint("n4", w, h, -1);

            // far
            SetPoint("f1", -w, -h, 1);
            SetPoint("f2", w, -h, 1);
            SetPoint("f3", -w, h, 1);
            SetPoint("f4", w, h, 1);

            // up
            SetPoint("u1", w * 0.7f, h * 1.1f, -1);
            SetPoint("u2", -w * 0.7f, h * 1.1f, -1);
            SetPoint("u3", 0, h * 2, -1);

            // cross
            SetPoint("cf1", -w, 0, 1);
            SetPoint("cf2", w, 0, 1);
            SetPoint("cf3", 0, -h, 1);
            SetPoint("cf4", 0, h, 1);

            SetPoint("cn1", -w, 0, -1);
            SetPoint("cn2", w, 0, -1);
            SetPoint("cn3", 0, -h, -1);
            SetPoint("cn4", 0, h, -1);

            Rotation = camera.Rotation;
            Position = camera.Position;
            Scale = camera.Scale;
        }

        private void SetPoint(string point, float x, float y, float z)
        {
            var vector = new Vector3(x, y, z);
            Projector.UnprojectVector(vector, camera.projectionMatrix, camera.matrixWorld);
            List<int> list;
            if (pointMap.TryGetValue(point, out list))
            {
                foreach (var p in list)
                {
                    geometry.vertices[p] = vector;
                }
            }

            geometry.VerticesNeedUpdate = true;
        }
    }
}