using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Lights
{
    public class DirectionalLight : Light, HasTarget, OnlyShadow, HasShadow
    {        
        public float Intensity;
        public Vector3 Target { get; set; }

        public bool UseOnlyShadow { get; set; }
        public float ShadowCameraFov { get; set; }
        public float ShadowCameraNear { get; set; }
        public float ShadowCameraFar { get; set; }
        public float ShadowCameraLeft { get; set; }
        public float ShadowCameraRight { get; set; }
        public float ShadowCameraTop { get; set; }
        public float ShadowCameraBottom { get; set; }
        public bool ShadowCameraVisible { get; set; }
        public float ShadowBias { get; set; }
        public float ShadowDarkness { get; set; }
        public int ShadowMapWidth { get; set; }
        public int ShadowMapHeight { get; set; }
        public List<float> ShadowCascadeBias { get; set; }
        public List<int> ShadowCascadeWidth { get; set; }
        public List<int> ShadowCascadeHeight { get; set; }
        public List<float> ShadowCascadeNearZ { get; set; }
        public List<float> ShadowCascadeFarZ { get; set; }
        public RenderTarget shadowMap { get; set; }
        public Vector2 ShadowMapSize { get; set; }
        public Matrix4 ShadowMatrix { get; set; }
        public Camera ShadowCamera { get; set; }
        public bool ShadowCascade { get; set; }
        public Vector3 ShadowCascadeOffset { get; set; }
        public int ShadowCascadeCount { get; set; }
        public List<VirtualLight> ShadowCascadeArray { get; set; }

        public DirectionalLight(Color color) : base(color)
        {
            Intensity = 1;
            UseOnlyShadow = false;
            ShadowCameraNear = 50;
            ShadowCameraFar = 5000;
            ShadowCameraLeft = -500;
            ShadowCameraRight = 500;
            ShadowCameraTop = 500;
            ShadowCameraBottom = -500;
            ShadowCameraVisible = false;
            ShadowBias = 0;
            ShadowDarkness = 0.5f;
            ShadowMapWidth = 512;
            ShadowMapHeight = 512;
            Target = Vector3.Zero;

            ShadowCascade = false;
            ShadowCascadeOffset = new Vector3( 0, 0, - 1000 );
	        ShadowCascadeCount = 2;
            ShadowCascadeBias = new List<float>(){  0, 0, 0 };
	        ShadowCascadeWidth = new List<int>(){  512, 512, 512 };
	        ShadowCascadeHeight = new List<int>(){ 512, 512, 512 };
            ShadowCascadeNearZ = new List<float>(){ - 1.000f, 0.990f, 0.998f };
	        ShadowCascadeFarZ  = new List<float>(){ 0.990f, 0.998f, 1.000f };
            ShadowCascadeArray = new List<VirtualLight>();
        }
    }
}
