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
    interface HasShadow
    {
        float ShadowCameraFov { get; set; }
        float ShadowCameraNear { get; set; }
        float ShadowCameraFar { get; set; }
        float ShadowCameraLeft { get; set; }
        float ShadowCameraRight { get; set; }
        float ShadowCameraTop { get; set; }
        float ShadowCameraBottom { get; set; }
        bool ShadowCameraVisible { get; set; }
        float ShadowBias { get; set; }
        float ShadowDarkness { get; set; }
        int ShadowMapWidth { get; set; }
        int ShadowMapHeight { get; set; }
        List<float> ShadowCascadeBias { get; set; }
        List<int> ShadowCascadeWidth { get; set; }
        List<int> ShadowCascadeHeight { get; set; }
        List<float> ShadowCascadeNearZ { get; set; }
        List<float> ShadowCascadeFarZ { get; set; }
        List<VirtualLight> ShadowCascadeArray { get; set; }

        RenderTarget shadowMap { get; set; }
        Vector2 ShadowMapSize { get; set; }
        Matrix4 ShadowMatrix { get; set; }
        Camera ShadowCamera { get; set; }

        bool ShadowCascade { get; set; }
        Vector3 ShadowCascadeOffset { get; set; }
        int ShadowCascadeCount { get; set; }


        
    }
}