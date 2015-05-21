using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Lights;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;

namespace Three.Net.Scenes
{
    public class Scene : Object3D
    {
        public class BufferInfo
        {
            public uint? Id;
            public GeometryGroup Buffer;
            public Object3D Object;
            public Dictionary<MaterialType, Material> Materials;
            public float Z;
            public bool ShouldRender;
        }

        public List<Object3D> objectsAdded = new List<Object3D>();
        public List<Object3D> objectsRemoved = new List<Object3D>();
        internal List<Light> lights = new List<Light>();
        public bool AutoUpdate = true;

        public Dictionary<uint, List<BufferInfo>> glObjects = new Dictionary<uint, List<BufferInfo>>();
        public List<Scene.BufferInfo> glObjectsImmediate;
        public bool ShouldOverrideMaterial;
        public Material OverrideMaterial;
        public Fog Fog = null;

        public Scene()
        {
            matrixAutoUpdate = false;
        }

        private Color backgroundColor = new Color(0.1f, 0.1f, 0.1f);
        public Color BackgroundColor
        {
            get
            {
                return Fog != null ? Fog.Color : backgroundColor;
            }
        }

        public void AddObject(Object3D o)
        {
            var light = o as Light;

            if (light != null)
            {
                if (!lights.Contains(light)) lights.Add(light);

                var targetLight = light as HasTarget;
            }
            else
            {
                var camera = o as Camera;
                var bone = o as Bone;

                if (!(camera != null || bone != null))
                {
                    objectsAdded.Add(o);

                    // check if previously removed
                    objectsRemoved.Remove(o);
                }
            }

            //dispatchEvent( { type: 'objectAdded', object: object } );
            //object.dispatchEvent( { type: 'addedToScene', scene: this } );

            foreach (var c in o.Children) AddObject(c);
        }

        internal void RemoveObject(Object3D o)
        {
            var light = o as Light;
            if (light != null)
            {
                lights.Remove(light);

                var directional = light as DirectionalLight;
                if (directional != null) throw new NotImplementedException();
            }
            else
            {
                var camera = o as Camera;

                if (camera == null)
                {
                    objectsRemoved.Add(o);

                    // check if previously added
                    objectsAdded.Remove(o);
                }
            }

            foreach (var c in o.Children) RemoveObject(c);
        }
    }
}
