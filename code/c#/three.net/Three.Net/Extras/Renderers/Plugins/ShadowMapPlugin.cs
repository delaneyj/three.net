using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Lights;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Plugins;
using Three.Net.Scenes;

namespace Three.Net.Extras.Renderers.Plugins
{
    public class ShadowMapPlugin : RenderPlugin
    {
        Frustum frustum = Frustum.Empty;
        Matrix4 projectionScreenMatrix = Matrix4.Identity;
        Vector3 matrixPosition = Vector3.Zero;
        List<Scene.BufferInfo> renderList = new List<Scene.BufferInfo>();

        protected internal override void Init(Net.Renderers.GL4.Renderer renderer)
        {
            base.Init(renderer);

            throw new NotImplementedException();
            //var depthShader = THREE.ShaderLib[ "depthRGBA" ];
            //var depthUniforms = THREE.UniformsUtils.clone( depthShader.uniforms );
            //
            //_depthMaterial = new THREE.ShaderMaterial( { fragmentShader: depthShader.fragmentShader, vertexShader: depthShader.vertexShader, uniforms: depthUniforms } );
            //_depthMaterialMorph = new THREE.ShaderMaterial( { fragmentShader: depthShader.fragmentShader, vertexShader: depthShader.vertexShader, uniforms: depthUniforms, morphTargets: true } );
            //_depthMaterialSkin = new THREE.ShaderMaterial( { fragmentShader: depthShader.fragmentShader, vertexShader: depthShader.vertexShader, uniforms: depthUniforms, skinning: true } );
            //_depthMaterialMorphSkin = new THREE.ShaderMaterial( { fragmentShader: depthShader.fragmentShader, vertexShader: depthShader.vertexShader, uniforms: depthUniforms, morphTargets: true, skinning: true } );
            //
            //_depthMaterial._shadowPass = true;
            //_depthMaterialMorph._shadowPass = true;
            //_depthMaterialSkin._shadowPass = true;
            //_depthMaterialMorphSkin._shadowPass = true;
        }

        protected internal override void Render(Scene scene, Camera camera, int viewportWidth, int viewportHeight)
        {
            if (!(renderer.shadowMapEnabled && renderer.shadowMapAutoUpdate)) return;

            Update(scene, camera);
        }

        private void Update(Scene scene, Camera camera)
        {
            // set GL state for depth map
            GL.ClearColor(1, 1, 1, 1);
            GL.Disable(EnableCap.Blend);
            GL.Enable(EnableCap.CullFace);
            GL.FrontFace(FrontFaceDirection.Ccw);

            GL.CullFace(renderer.shadowMapCullFace);

            renderer.DepthTest = true;
            var lights = new List<Light>();

            // preprocess lights
            // 	- skip lights that are not casting shadows
            //	- create virtual lights for cascaded shadow maps
            foreach (var light in scene.lights)
            {
                if (!light.DoesCastShadow) continue;

                var shadowLight = light as HasShadow;
                if (shadowLight != null && shadowLight.ShadowCascade)
                {
                    for (var n = 0; n < shadowLight.ShadowCascadeCount; n++)
                    {
                        VirtualLight virtualLight;

                        if (shadowLight.ShadowCascadeArray[n] == null)
                        {
                            virtualLight = CreateVirtualLight(light, n);
                            virtualLight.OriginalCamera = camera;

                            var gyro = new Gyroscope();
                            gyro.Position = shadowLight.ShadowCascadeOffset;

                            gyro.Add(virtualLight);
                            //gyro.Add(virtualLight.Target);
                            camera.Add(gyro);

                            shadowLight.ShadowCascadeArray[n] = virtualLight;
                            Debug.WriteLine("Created virtualLight {0}", virtualLight);
                        }
                        else
                        {
                            virtualLight = shadowLight.ShadowCascadeArray[n];
                        }

                        UpdateVirtualLight(light, n);
                        lights.Add(virtualLight);
                    }
                }
                else
                {
                    lights.Add(light);
                }

            }

            // render depth map
            foreach (var light in lights)
            {
                var hasShadow = light as HasShadow;
                if (hasShadow.shadowMap == null)
                {
                    var isSoftShadow = renderer.shadowMapType == ShadowType.PCFSoftShadowMap;
                    hasShadow.shadowMap = new RenderTarget(hasShadow.ShadowMapWidth, hasShadow.ShadowMapHeight);
                    hasShadow.shadowMap.MinFilter = isSoftShadow ? TextureMinFilter.Nearest : TextureMinFilter.Linear;
                    hasShadow.shadowMap.MagFilter = isSoftShadow ? TextureMagFilter.Nearest : TextureMagFilter.Linear; ;
                    hasShadow.shadowMap.Format = Three.Net.Renderers.PixelFormat.RGBA;

                    hasShadow.ShadowMapSize = new Vector2(hasShadow.ShadowMapWidth, hasShadow.ShadowMapHeight);
                    hasShadow.ShadowMatrix = Matrix4.Identity;
                }

                if (hasShadow.ShadowCamera == null)
                {

                    if (hasShadow is SpotLight) hasShadow.ShadowCamera = new PerspectiveCamera(renderer, hasShadow.ShadowCameraFov, hasShadow.ShadowCameraNear, hasShadow.ShadowCameraFar);
                    else if (hasShadow is DirectionalLight) hasShadow.ShadowCamera = new OrthographicCamera(hasShadow.ShadowCameraLeft, hasShadow.ShadowCameraRight, hasShadow.ShadowCameraTop, hasShadow.ShadowCameraBottom, hasShadow.ShadowCameraNear, hasShadow.ShadowCameraFar);
                    else throw new Exception("Unsupported light type for shadow");

                    scene.Add(hasShadow.ShadowCamera);
                    if (scene.AutoUpdate) scene.UpdateMatrixWorld();

                }

                if (hasShadow.ShadowCameraVisible /* && light.CameraHelper == null*/)
                {
                    throw new NotImplementedException();
                    //light.CameraHelper = new CameraHelper( hasShadow.ShadowCamera );
                    //hasShadow.ShadowCamera.Add( light.CameraHelper );
                }

                var virtualLight = light as VirtualLight;
                if (virtualLight != null && virtualLight.OriginalCamera == camera)
                {
                    UpdateShadowCamera(camera, light);
                }

                var shadowMap = hasShadow.shadowMap;
                var shadowMatrix = hasShadow.ShadowMatrix;
                var shadowCamera = hasShadow.ShadowCamera;

                shadowCamera.Position = Vector3.FromPosition(light.matrixWorld);
                matrixPosition = (light as HasTarget).Target;
                shadowCamera.LookAt(matrixPosition);
                shadowCamera.UpdateMatrixWorld();

                shadowCamera.matrixWorldInverse = Matrix4.GetInverse(shadowCamera.matrixWorld);

                //TODO : Creating helpers if ( light.cameraHelper ) light.cameraHelper.visible = light.shadowCameraVisible;
                //if (hasShadow.ShadowCameraVisible) light.cameraHelper.update();

                // compute shadow matrix

                shadowMatrix = new Matrix4(0.5f, 0.0f, 0.0f, 0.5f,
                                            0.0f, 0.5f, 0.0f, 0.5f,
                                            0.0f, 0.0f, 0.5f, 0.5f,
                                            0.0f, 0.0f, 0.0f, 1.0f);

                shadowMatrix.Multiply(shadowCamera.projectionMatrix);
                shadowMatrix.Multiply(shadowCamera.matrixWorldInverse);

                // update camera matrices and frustum
                projectionScreenMatrix.MultiplyMatrices(shadowCamera.projectionMatrix, shadowCamera.matrixWorldInverse);
                frustum = Frustum.FromMatrix(projectionScreenMatrix);

                // render shadow map
                renderer.SetRenderTarget(shadowMap);
                renderer.Clear();

                // set object matrices & frustum culling

                renderList.Clear();
                ProjectObject(scene, scene, shadowCamera);

                // render regular objects
                foreach (var glObject in renderList)
                {
                    var o = glObject.Object;
                    var buffer = glObject.Buffer;

                    throw new NotImplementedException();
                }
            }

            // restore GL state
            var clearColor = renderer.ClearColor;
            GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, 1);
            GL.Enable(EnableCap.Blend);

            if (renderer.shadowMapCullFace == CullFaceMode.Front) GL.CullFace(CullFaceMode.Back);

        }

        private void UpdateShadowCamera(Camera camera, Light light)
        {
            throw new NotImplementedException();
        }

        private void ProjectObject(Scene scene, Object3D o, Camera shadowCamera)
        {
            if (o.IsVisible)
            {
                List<Scene.BufferInfo> glObjects;
                if (scene.glObjects.TryGetValue(o.Id, out glObjects))
                {
                    if (o.DoesCastShadow && (!o.frustumCulled || frustum.IntersectsObject(o)))
                    {
                        foreach(var glObject in glObjects)
                        { 
                            o.modelViewMatrix.MultiplyMatrices(shadowCamera.matrixWorldInverse, o.matrixWorld);
                            renderList.Add(glObject);
                        }
                    }

                    foreach (var c in o.Children) ProjectObject(scene, c, shadowCamera);
                }
            }
        }

        private VirtualLight CreateVirtualLight(Light light, int cascade)
        {
            var hasShadow = light as HasShadow;
            var virtualLight = new VirtualLight(light.Color)
            {
                UseOnlyShadow = true,
                DoesCastShadow = true,
                ShadowCameraNear = hasShadow.ShadowCameraNear,
                ShadowCameraFar = hasShadow.ShadowCameraFar,
                ShadowCameraLeft = hasShadow.ShadowCameraLeft,
                ShadowCameraRight = hasShadow.ShadowCameraRight,
                ShadowCameraBottom = hasShadow.ShadowCameraBottom,
                ShadowCameraTop = hasShadow.ShadowCameraTop,
                ShadowCameraVisible = hasShadow.ShadowCameraVisible,
                ShadowDarkness = hasShadow.ShadowDarkness,
                ShadowBias = hasShadow.ShadowCascadeBias[cascade],
                ShadowMapWidth = hasShadow.ShadowCascadeWidth[cascade],
                ShadowMapHeight = hasShadow.ShadowCascadeHeight[cascade],
                PointsWorld = new List<Vector3>(),
                PointsFrustum = new List<Vector3>()
            };

            for (var i = 0; i < 8; i++)
            {
                virtualLight.PointsWorld.Add(Vector3.Zero);
            }

            var nearZ = hasShadow.ShadowCascadeNearZ[cascade];
            var farZ = hasShadow.ShadowCascadeFarZ[cascade];

            virtualLight.PointsFrustum.Add(new Vector3(-1, -1, nearZ));
            virtualLight.PointsFrustum.Add(new Vector3(1, -1, nearZ));
            virtualLight.PointsFrustum.Add(new Vector3(-1, 1, nearZ));
            virtualLight.PointsFrustum.Add(new Vector3(1, 1, nearZ));
            virtualLight.PointsFrustum.Add(new Vector3(-1, -1, farZ));
            virtualLight.PointsFrustum.Add(new Vector3(1, -1, farZ));
            virtualLight.PointsFrustum.Add(new Vector3(-1, 1, farZ));
            virtualLight.PointsFrustum.Add(new Vector3(1, 1, farZ));

            return virtualLight;
        }

        // Synchronize virtual light with the original light
        private void UpdateVirtualLight(Light light, int cascade)
        {
            var shadowLight = light as HasShadow;
            var hasTarget = light as HasTarget;
            var virtualLight = shadowLight.ShadowCascadeArray[cascade];

            virtualLight.Position = light.Position;
            virtualLight.Target = hasTarget.Target;
            virtualLight.LookAt(virtualLight.Target);

            virtualLight.ShadowCameraVisible = shadowLight.ShadowCameraVisible;
            virtualLight.ShadowDarkness = shadowLight.ShadowDarkness;

            virtualLight.ShadowBias = shadowLight.ShadowCascadeBias[cascade];

            var nearZ = shadowLight.ShadowCascadeNearZ[cascade];
            var farZ = shadowLight.ShadowCascadeFarZ[cascade];

            Debug.Assert(virtualLight.PointsFrustum.Count == 8);
            for (var p = 0; p < 8; p++)
            {
                var point = virtualLight.PointsFrustum[p];
                point.z = p < 4 ? nearZ : farZ;
                virtualLight.PointsFrustum[p] = point;
            }
        }

        // Fit shadow camera's ortho frustum to camera frustum
        private void UpdateShadowCamera(Camera camera, VirtualLight light)
        {
            var min = Vector3.Infinity;
            var max = Vector3.NegativeInfinity;

            for (var i = 0; i < 8; i++)
            {

                var p = light.PointsFrustum[i];
                Projector.UnprojectVector(p, camera.projectionMatrix, camera.matrixWorld);

                p.Apply(light.ShadowCamera.matrixWorldInverse);

                min.Min(p);
                min.Max(p);
            }

            var ortho = light.ShadowCamera as OrthographicCamera;
            if (ortho != null)
            {
                ortho.left = min.x;
                ortho.right = max.x;
                ortho.top = max.y;
                ortho.bottom = min.y;
            }
            else
            {
                throw new NotImplementedException("Should this just ignore the perspective version?");
            }

            // can't really fit near/far
            //shadowCamera.near = _min.z;
            //shadowCamera.far = _max.z;
            light.ShadowCamera.UpdateProjectionMatrix();
        }
    }
}