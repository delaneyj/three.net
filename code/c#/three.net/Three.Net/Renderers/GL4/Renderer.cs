using System;
using System.Collections.Generic;
using System.Diagnostics;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Lights;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Scenes;
using System.Linq;
using Three.Net.Renderers.Shaders;
using System.Text;
using Three.Net.Renderers.GL4;
using System.Threading.Tasks;
using System.Threading;
using Three.Net.Textures;
using Pencil.Gaming.Graphics;
using Three.Net.Utils;
using Three.Net.Renderers.Plugins;

namespace Three.Net.Renderers.GL4
{
    class LightCache
    {
        public Color Ambient = Color.Black;
        public DirectionalInfo Directional = new DirectionalInfo();
        public PointInfo Point = new PointInfo();
        public SpotInfo Spot = new SpotInfo();
        public HemiInfo Hemi = new HemiInfo();

        internal void Clear()
        {
            Directional.Clear();
            Point.Clear();
            Spot.Clear();
            Hemi.Clear();
        }

        public class DirectionalInfo
        {
            public List<Color> Colors = new List<Color>();
            public List<Vector3> Positions = new List<Vector3>();

            internal void Clear()
            {
                Colors.Clear();
                Positions.Clear();
            }
        }

        public class PointInfo
        {
            public List<Color> Colors = new List<Color>();
            public List<Vector3> Positions = new List<Vector3>();
            public List<float> Distances = new List<float>();

            internal void Clear()
            {
                Colors.Clear();
                Positions.Clear();
                Distances.Clear();
            }
        }

        public class SpotInfo
        {
            public List<Color> Colors = new List<Color>();
            public List<Vector3> Positions = new List<Vector3>();
            public List<float> Distances = new List<float>();
            public List<Vector3> Directions = new List<Vector3>();
            public List<float> AngleCosine = new List<float>();
            public List<float> Exponents = new List<float>();

            internal void Clear()
            {
                Colors.Clear();
                Positions.Clear();
                Distances.Clear();
                Directions.Clear();
                AngleCosine.Clear();
                Exponents.Clear();
            }
        }

        public class HemiInfo
        {
            public List<Color> SkyColors = new List<Color>();
            public List<Color> GroundColors = new List<Color>();
            public List<Vector3> Positions = new List<Vector3>();

            internal void Clear()
            {
                SkyColors.Clear();
                GroundColors.Clear();
                Positions.Clear();
            }
        }
    }

    public struct LightCountInfo
    {
        public int DirectionalLightCount;
        public int PointLightCount;
        public int SpotLightCount;
        public int HemiLightCount;
    }

    public struct RuntimeInfo
    {
        public struct RenderInfo
        {
            public int DrawCalls;
            public int Vertices;
            public int Faces;
            public int Points;

            public override string ToString()
            {
                return string.Format("Render( DrawCalls:{0}, Vertices:{1}, Faces:{2}, Points:{3})", DrawCalls, Vertices, Faces, Points);
            }

            public override int GetHashCode()
            {
                return DrawCalls ^ Vertices << 8 ^ Faces << 16 ^ Points << 24;
            }
        }

        public struct MemoryInfo
        {
            public int Textures;
            public int Geometries;
            public int Programs;

            public override string ToString()
            {
                return string.Format("Memory( Textures:{0}, Geometries:{1}, Programs:{2})", Textures, Geometries, Programs);
            }

            public override int GetHashCode()
            {
                return Textures ^ Geometries << 12 ^ Programs << 24;
            }
        }

        public RenderInfo Render;
        public MemoryInfo Memory;

        internal void ResetRender()
        {
            Render.DrawCalls = 0;
            Render.Vertices = 0;
            Render.Faces = 0;
            Render.Points = 0;
        }

        public override string ToString()
        {
            return string.Format("Per frame Info: {0} {1}", Render, Memory);
        }

        public override int GetHashCode()
        {
            return Render.GetHashCode() ^ Memory.GetHashCode() << 16;
        }
    }

    public class Renderer
    {
        public bool Done { get; private set; }
        public int Width { get { return window.Width; } set { window.Width = value; } }
        public int Height { get { return window.Height; } set { window.Height = value; } }
        public float AspectRatio { get { return Width / (float)Height; } }
        
        private int previousInfoHash;
        private Frustum frustum;
        private bool shouldSortObjects = true;
        private List<Scene.BufferInfo> opaqueObjects = new List<Scene.BufferInfo>();
        private List<Scene.BufferInfo> transparentObjects = new List<Scene.BufferInfo>();
        private Dictionary<string, Program> programs = new Dictionary<string, Program>();

        private PencilGamingWindows window;

        private RuntimeInfo info = new RuntimeInfo()
        {
            Render = new RuntimeInfo.RenderInfo(),
            Memory = new RuntimeInfo.MemoryInfo()
        };

        private List<RenderPlugin> renderPluginsPre = new List<RenderPlugin>();
        private List<RenderPlugin> renderPluginsPost = new List<RenderPlugin>();
        public bool ShouldAutoClear = true;
        private bool shouldAutoClearColor = true;
        private bool shouldAutoClearDepth = true;
        private bool shouldAutoClearStencil = true;
        private bool lightsNeedUpdate = true;
        private LightCache lightsCache = new LightCache();
        private int currentProgram;
        private Camera currentCamera;
        private int? currentFramebuffer;
        private uint? currentMaterialId;
        private long? currentGeometryGroupHash;
        private bool? previousDepthTest;
        private bool? previousDepthWrite;
        private bool? previousDoubleSided;
        private bool? previousFlipSided;
        private float? previousLineWidth;
        private Color previousClearColor = Color.White;
        private int viewportWidth = 0;
        private int viewportHeight = 0;
        private int viewportX = 0;
        private int viewportY = 0;
        private int currentWidth;
        private int currentHeight;
        private BlendMode? previousBlending;
        private BlendEquationMode? oldBlendEquation;
        private BlendingFactorSrc? oldBlendSrc;
        private BlendingFactorDest? oldBlendDst;
        private bool previousPolygonOffset;
        private float previousPolygonOffsetFactor;
        private float previousPolygonOffsetUnits;
        //private int usedTextureUnits;
        public bool GammaInput = false;
        public bool GammaOutput = false;

        // shadow map
        internal bool shadowMapEnabled = false;
        internal bool shadowMapAutoUpdate = true;
        internal ShadowType shadowMapType = ShadowType.PCFShadowMap;
        internal CullFaceMode shadowMapCullFace = CullFaceMode.Front;
        //private bool shadowMapDebug = false;
        //private bool shadowMapCascade = false;

        // morphs
        //private int maxMorphTargets = 8;
        //private int maxMorphNormals = 4;

        // flags
        //private bool autoScaleCubemaps = true;
        private int maxTextures;
        private int maxVertexTextures;
        private int maxTextureSize;
        private int maxCubemapSize;
        private bool supportsVertexTextures;
        private bool supportsBoneTextures;
       // private bool useAlpha = false;
       // private bool useDepth = true;
       // private bool useStencil = true;
       // private bool useAntialias = false;
       // private bool usePremultipliedAlpha = true;
       // private bool preserveDrawingBuffer = false;
        private bool useLogarithmicDepthBuffer = false;

        private Matrix4 projectionScreenMatrix = Matrix4.Identity;

        public Subscribable<PencilKeyInfo> OnKeyboard;
        private Trigger<PencilKeyInfo> onKeyboard;

        public Vector2 MousePosition { get { return window.MousePosition; } }
        public Vector2 MousePositionNormalized { get 
        {
            var mp = MousePosition;
            mp.x = Mathf.Fit(mp.x,0,Width,-1,1);
            mp.y = Mathf.Fit(mp.y, 0, Height, 1, -1);
            return mp; 
        }

        }

        public Renderer(int width = 1920, int height = 1080)
        {
            Done = false;
            
            onKeyboard = new Trigger<PencilKeyInfo>();
            OnKeyboard = onKeyboard;

            var fullscreen = false;
            var vsync = true;
            window = new PencilGamingWindows(width, height, "Three.Net Window", fullscreen, vsync);
            window.CreateWindow();
            window.OnExit.Subscribe(w =>
            {
                Done = true;
            });

            window.OnWindowResized.Subscribe(w =>
            {
                GL.Viewport(0, 0, Width, Height);
                if (currentCamera != null) currentCamera.UpdateProjectionMatrix();
                window.MakeCurrent();
            });

            window.OnKeyboard.Subscribe(info => 
                {
                    switch (info.Key)
                    {
                        case Pencil.Gaming.Key.Escape: window.Exit(); break;
                        //case Pencil.Gaming.Key.F: window.IsFullScreen = !window.IsFullScreen; break;
                        default: onKeyboard.Fire(info); break;
                    }
                });

            GL.ClearColor(1, 1, 1, 1);
            GL.GetInteger(GetPName.MaxTextureImageUnits, out maxTextures);
            GL.GetInteger(GetPName.MaxVertexTextureImageUnits, out maxVertexTextures);
            GL.GetInteger(GetPName.MaxTextureSize, out maxTextureSize);
            GL.GetInteger(GetPName.MaxCubeMapTextureSize, out maxCubemapSize);
            supportsVertexTextures = (maxVertexTextures > 0);
            supportsBoneTextures = supportsVertexTextures;

            AddPostPlugin(new SpritePlugin());
        }

        private void AddPostPlugin(RenderPlugin plugin)
        {
            plugin.Init( this );
		    renderPluginsPost.Add( plugin );
        }

        private void AddPrePlugin(RenderPlugin plugin)
        {
            plugin.Init(this);
            renderPluginsPre.Add(plugin);
        }

        public Color ClearColor { get { return previousClearColor; } }

        public void RenderFrame(Scene scene, Camera camera, RenderTarget renderTarget = null, bool forceClear = false)
        {
            var rebuiltWindow = window.ProcessEvents();
            if (rebuiltWindow) return;

            currentMaterialId = null;
            currentCamera = null;
            lightsNeedUpdate = true;

            if (scene.AutoUpdate) scene.UpdateMatrixWorld(); // update scene graph
            if (!camera.HasParent)
            {
                camera.UpdateMatrixWorld(); // update camera matrices and frustum
            }

            UpdateSkeletons(scene);

            camera.matrixWorldInverse = Matrix4.GetInverse(camera.matrixWorld);

            projectionScreenMatrix.MultiplyMatrices(camera.projectionMatrix, camera.matrixWorldInverse);
            frustum = Frustum.FromMatrix(projectionScreenMatrix);

            InitObjects(scene);

            opaqueObjects.Clear();
            transparentObjects.Clear();


            ProjectObject(scene, scene, camera);

            if (shouldSortObjects)
            {
                opaqueObjects.Sort(PainterSortStable);
                transparentObjects.Sort(ReversePainterSortStable);
            }

            RenderPlugins(renderPluginsPre, scene, camera); // custom render plugins (pre pass)

            info.ResetRender();

            SetRenderTarget(renderTarget);

            if (ShouldAutoClear || forceClear)
            {
                var clearColor = scene.BackgroundColor;
                if (clearColor != previousClearColor)
                {
                    GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, 1);
                    previousClearColor = clearColor;
                }
                Clear(shouldAutoClearColor, shouldAutoClearDepth, shouldAutoClearStencil);
            }

            Material overrideMaterial = null;
            if (scene.ShouldOverrideMaterial)
            {
                overrideMaterial = scene.OverrideMaterial;
                SetBlending(overrideMaterial.Blending, overrideMaterial.BlendEquation, overrideMaterial.BlendSource, overrideMaterial.BlendDestination);
                DepthTest = overrideMaterial.ShouldDepthTest;
                DepthWrite = overrideMaterial.ShouldDepthWrite;
                SetPolygonOffset(overrideMaterial.ShouldPolygonOffset, overrideMaterial.PolygonOffsetFactor, overrideMaterial.PolygonOffsetUnits);
                RenderObjects(opaqueObjects, camera, scene.lights, scene.Fog, true, overrideMaterial);
                RenderObjects(transparentObjects, camera, scene.lights, scene.Fog, true, overrideMaterial);
            }
            else
            {
                SetBlending(BlendMode.None);
                RenderObjects(opaqueObjects, camera, scene.lights, scene.Fog, false, overrideMaterial); // opaque pass (front-to-back order)
                RenderObjects(transparentObjects, camera, scene.lights, scene.Fog, true, overrideMaterial);  // transparent pass (back-to-front order)
            }

            RenderPlugins(renderPluginsPost, scene, camera); // custom render plugins (post pass)

           // if (renderTarget != null) throw new NotImplementedException();  // Generate mipmap if we're using any kind of mipmap filtering

            // Ensure depth buffer writing is enabled so it can be cleared on next render
            DepthTest = true;
            DepthWrite = true;

            var infoHash = info.GetHashCode();
            if (previousInfoHash != infoHash)
            {
                previousInfoHash = infoHash;
                Debug.WriteLine(info);
            }

            window.SwapBuffers();
        }

        private void InitObjects(Scene scene)
        {
            while (scene.objectsAdded.Count > 0)
            {
                AddObject(scene.objectsAdded[0], scene);
                scene.objectsAdded.RemoveAt(0);
            }

            while (scene.objectsRemoved.Count > 0)
            {
                RemoveObject(scene.objectsRemoved[0], scene);
                scene.objectsRemoved.RemoveAt(0);
            }
        }

        public void Clear(bool autoClearColor = true, bool autoClearDepth = true, bool autoClearStencil = true)
        {
            var bits = (ClearBufferMask)0;
            if (autoClearColor) bits |= ClearBufferMask.ColorBufferBit;
            if (autoClearDepth) bits |= ClearBufferMask.DepthBufferBit;
            if (autoClearStencil) bits |= ClearBufferMask.StencilBufferBit;
            GL.Clear(bits);
        }

        private void SetFaceCulling(Material material)
        {
            var doubleSided = material.Side == SideMode.Double;
            var flipSided = material.Side == SideMode.Back;

            if (previousDoubleSided != doubleSided)
            {
                if (doubleSided) GL.Disable(EnableCap.CullFace);
                else GL.Enable(EnableCap.CullFace);
                previousDoubleSided = doubleSided;
            }

            if (previousFlipSided != flipSided)
            {
                GL.FrontFace(flipSided ? FrontFaceDirection.Cw : FrontFaceDirection.Ccw);
                previousFlipSided = flipSided;
            }
        }

        private Program SetProgram(Camera camera, List<Light> lights, Fog fog, Material material, Object3D o)
        {
            if (material.DoesNeedUpdate)
            {
                if (material.Program != null) DeallocateMaterial(material);
                InitMaterial(material, lights, fog, o);
                material.DoesNeedUpdate = false;
            }

            var refreshProgram = false;
            var refreshMaterial = false;

            var program = material.Program;

            if (program.GLProgramId != currentProgram)
            {
                GL.UseProgram(program.GLProgramId);
                currentProgram = program.GLProgramId;

                refreshProgram = true;
                refreshMaterial = true;
            }

            if (material.Id != currentMaterialId)
            {
                currentMaterialId = material.Id;
                refreshMaterial = true;
            }

            var basic = material as MeshBasicMaterial;
            var lambert = basic as MeshLambertMaterial;

            if (refreshProgram || camera != currentCamera)
            {
                var projection = camera.projectionMatrix;
                program.SetUniformData("projectionMatrix", ref projection);

                if (useLogarithmicDepthBuffer) program.SetUniformData("logDepthBufFC", 2 / (Mathf.Log(camera.far + 1) / Mathf.NaturalLog2));

                if (camera != currentCamera) currentCamera = camera;

                // load material specific uniforms
                // (shader material also gets them for the sake of genericity)

                if (/* material is ShaderMaterial ||*/ material is MeshPhongMaterial || (basic != null && basic.EnvironmentMap != null))
                {
                    {
                        var v = Vector3.FromPosition(camera.matrixWorld);
                        program.SetUniformData("cameraPosition", v);
                    }
                }


                if (material is MeshPhongMaterial || lambert != null /*|| material instanceof THREE.ShaderMaterial ||*/ || (basic != null && basic.UseSkinning))
                {
                    var view = camera.matrixWorldInverse;
                    program.SetUniformData("viewMatrix", ref view);
                }
            }

            if ((basic != null && basic.UseSkinning)) throw new NotImplementedException();

            if (refreshMaterial)
            {
                if (fog != null && material.UseFog)
                {
                    program.SetUniformData("fogColor", fog.Color);

                    var fogLinear = fog as FogLinear;
                    if (fogLinear != null)
                    {
                        program.SetUniformData("fogNear", fogLinear.Near);
                        program.SetUniformData("fogFar", fogLinear.Far);
                    }
                    else
                    {
                        var fogExp = fog as FogExp2;
                        program.SetUniformData("fogDensity", fogExp.Density);
                    }
                }

                if (material.UseLights)
                {
                    if (lightsNeedUpdate)
                    {
                        SetupLights(lights);
                        lightsNeedUpdate = false;
                    }

                    program.SetUniformData("ambientLightColor", lightsCache.Ambient);

                    program.SetUniformData("directionalLightColor", lightsCache.Directional.Colors);
                    program.SetUniformData("directionalLightDirection", lightsCache.Directional.Positions);

                    program.SetUniformData("pointLightColor", lightsCache.Point.Colors);
                    program.SetUniformData("pointLightPosition", lightsCache.Point.Positions);
                    program.SetUniformData("pointLightDistance", lightsCache.Point.Distances);

                    program.SetUniformData("spotLightColor", lightsCache.Spot.Colors);
                    program.SetUniformData("spotLightPosition", lightsCache.Spot.Positions);
                    program.SetUniformData("spotLightDistance", lightsCache.Spot.Distances);
                    program.SetUniformData("spotLightDirection", lightsCache.Spot.Directions);
                    program.SetUniformData("spotLightAngleCos", lightsCache.Spot.AngleCosine);
                    program.SetUniformData("spotLightExponent", lightsCache.Spot.Exponents);

                    program.SetUniformData("hemisphereLightSkyColor", lightsCache.Hemi.SkyColors);
                    program.SetUniformData("hemisphereLightGroundColor", lightsCache.Hemi.GroundColors);
                    program.SetUniformData("hemisphereLightDirection", lightsCache.Hemi.Positions);
                }

                material.RefreshAllUniforms();
            }

            LoadUniformsMatrices(program, o);
            var world = o.matrixWorld;
            program.SetUniformData("modelMatrix", ref world);

            return program;
        }

        private void LoadUniformsMatrices(Program program, Object3D o)
        {
            program.SetUniformData("modelViewMatrix", ref o.modelViewMatrix);
            program.SetUniformData("normalMatrix", ref o.normalMatrix);
        }

        private void SetupLights(List<Light> lights)
        {
            float r = 0, g = 0, b = 0;
            lightsCache.Clear();

            foreach (var light in lights)
            {
                var onlyShadow = light as OnlyShadow;
                if (onlyShadow != null && onlyShadow.UseOnlyShadow) continue;

                var color = light.Color;

                var ambient = light as AmbientLight;
                if (ambient != null)
                {
                    if (!light.IsVisible) continue;

                    r += GammaInput ? color.R * color.R : color.R;
                    g += GammaInput ? color.G * color.G : color.G;
                    b += GammaInput ? color.B * color.B : color.B;
                }
                else
                {
                    var directional = light as DirectionalLight;

                    if (directional != null)
                    {
                        if (!directional.IsVisible) continue;

                        var direction = directional.Target;

                        direction.Normalize();
                        lightsCache.Directional.Positions.Add(direction);
                        if (GammaInput) color.SetColorGamma(directional.Intensity);
                        else color.SetColorLinear(directional.Intensity);
                        lightsCache.Directional.Colors.Add(color);
                    }
                    else
                    {
                        var point = light as PointLight;
                        if (point != null)
                        {
                            if (!point.IsVisible) continue;
                            if (GammaInput) color.SetColorGamma(point.Intensity);
                            else color.SetColorLinear(point.Intensity);
                            lightsCache.Point.Colors.Add(color);
                            light.UpdateMatrixWorld();
                            lightsCache.Point.Positions.Add(Vector3.FromPosition(light.matrixWorld));
                            lightsCache.Point.Distances.Add(point.Distance);
                        }
                        else
                        {
                            var spot = light as SpotLight;
                            if (spot != null)
                            {
                                if (!spot.IsVisible) continue;
                                if (GammaInput) color.SetColorGamma(spot.Intensity);
                                else color.SetColorLinear(spot.Intensity);
                                lightsCache.Spot.Colors.Add(color);
                                var pos = light.Position;
                                lightsCache.Spot.Positions.Add(pos);
                                lightsCache.Spot.Distances.Add(spot.Distance);
                                var direction = new Vector3();
                                var target = spot.Target;
                                direction = Vector3.SubtractVectors(pos, target);
                                direction.Normalize();
                                lightsCache.Spot.Directions.Add(direction);
                                lightsCache.Spot.AngleCosine.Add(Mathf.Cos(spot.Angle));
                                lightsCache.Spot.Exponents.Add(light.Exponent);
                            }
                            else
                            {
                                var hemi = light as HemisphericalLight;
                                if (hemi != null)
                                {
                                    if (!light.IsVisible) continue;
                                    throw new NotImplementedException();
                                    //var direction = Vector3.FromPosition(light.CachedWorld);
                                    //direction.Normalize();
                                    //lightsCache.Hemi.Positions.Add(direction);
                                    //var skyColor = hemi.Color;
                                    //var groundColor = hemi.GroundColor;
                                    //if (GammaInput)
                                    //{
                                    //    skyColor.SetColorGamma(hemi.Intensity);
                                    //    groundColor.SetColorGamma(hemi.Intensity);
                                    //}
                                    //else
                                    //{
                                    //    skyColor.SetColorLinear(hemi.Intensity);
                                    //    groundColor.SetColorLinear(hemi.Intensity); 
                                    //}
                                }
                                else
                                {
                                    throw new NotSupportedException();
                                }
                            }
                        }
                    }
                }
            }

            lightsCache.Ambient = new Color(r, g, b);
        }

        private void InitMaterial(Material material, List<Light> lights, Fog fog, Object3D o)
        {
            var maxLightCountInfo = AllocateLights(lights);
            // var maxShadows = AllocateShadows(lights);
            //var maxBones = AllocateBones(skinnedMesh);

            var parameters = new Dictionary<string, dynamic>()
            {
                {"supportsVertexTextures", supportsVertexTextures},
                //{"map", material.DiffuseMap != null},
                //{"envMap", material.envMap != null},
                //{"lightMap", material.lightMap!= null},
                //{"bumpMap",material.bumpMap!= null},
                {"normalMap", (material is MeshBasicMaterial) && (material as MeshBasicMaterial).NormalMap!= null},
                {"specularMap", (material is MeshBasicMaterial) && (material as MeshBasicMaterial).SpecularMap!= null},
                //{"alphaMap", material.alphaMap!= null},
                {"vertexColors", (material is MeshBasicMaterial) ? (material as MeshBasicMaterial).VertexColors : VertexColorMode.None },
                {"fog",fog!= null},
                {"useFog",material.UseFog },
                {"fogExp", fog is FogExp2},
                //{"sizeAttenuation", material is PointCloudMaterial && (material as PointCloudMaterial).SizeAttenuation },
                //{"logarithmicDepthBuffer", useLogarithmicDepthBuffer},
                //{"skinning", material.UseSkinning},
                //{"maxBones", maxBones},
               // {"useVertexTexture", supportsBoneTextures && skinnedMesh != null && skinnedMesh.Skeleton.UseVertexTexture},
                //{"morphTargets", material.UseMorphTargets},
                //{"morphNormals", material.UseMorphNormals},
                //{"maxMorphTargets", maxMorphTargets},
                //{"maxMorphNormals", maxMorphNormals},
                {"maxDirLights", maxLightCountInfo.DirectionalLightCount},
                {"maxPointLights", maxLightCountInfo.PointLightCount},
                {"maxSpotLights", maxLightCountInfo.SpotLightCount},
                {"maxHemiLights", maxLightCountInfo.HemiLightCount},
                //{"maxShadows", maxShadows},
                //{"shadowMapEnabled", shadowMapEnabled && (o as Mesh).DoesReceiveShadow && maxShadows > 0},
                //{"shadowMapType", shadowMapType},
                //{"shadowMapDebug", shadowMapDebug},
                //{"shadowMapCascade", shadowMapCascade},
                {"alphaTest", material.ShouldAlphaTest},
                //{"metal", (material is MeshPhongMaterial && (material as MeshPhongMaterial).UseMetal)},
                {"wrapAround", ((material is MeshLambertMaterial && (material as MeshLambertMaterial).ShouldWrapAround) || (material is MeshPhongMaterial && (material as MeshPhongMaterial).ShouldWrapAround))},
                {"doubleSided", material.Side == SideMode.Double},
                {"flipSided", material.Side == SideMode.Back}
            };

            var basic = material as MeshBasicMaterial;
            if (basic != null)
            {
                parameters.Add("map", basic.DiffuseMap != null);
            }

            var hash = string.Format("{0}_{1}", material.GetType().Name, material.VertexShaderSource.GetHashCode() ^ material.FragmentShaderSource.GetHashCode());

            Program program;

            // Check if code has been already compiled
            if (programs.TryGetValue(hash, out program))
            {
                // TODO: Anything extra need to happen here?
            }
            else
            {
                program = new Program(this, material.VertexShaderSource, material.FragmentShaderSource, parameters);
                programs.Add(hash, program);
                info.Memory.Programs = programs.Count;
            }

            material.Program = program;
        }

        private int AllocateBones(SkinnedMesh skinnedMesh)
        {
            if (skinnedMesh != null) throw new NotImplementedException();
            return 0;
        }

        private int AllocateShadows(IEnumerable<Light> lights)
        {
            var maxShadows = 0;
            foreach (var light in lights)
            {
                throw new NotImplementedException();
            }
            return maxShadows;
        }

        private LightCountInfo AllocateLights(List<Light> lights)
        {
            var lightInfo = new LightCountInfo();

            foreach (var light in lights)
            {
                var onlyShadow = light as OnlyShadow;
                if (!light.IsVisible || (onlyShadow != null && onlyShadow.UseOnlyShadow)) continue;

                if (light is DirectionalLight) lightInfo.DirectionalLightCount++;
                if (light is PointLight) lightInfo.PointLightCount++;
                if (light is SpotLight) lightInfo.SpotLightCount++;
                if (light is HemisphericalLight) lightInfo.HemiLightCount++;
            }

            return lightInfo;
        }

        private void DeallocateMaterial(Material material)
        {
            throw new NotImplementedException();
        }

        private void RenderObjects(List<Scene.BufferInfo> renderList, Camera camera, List<Light> lights, Fog fog, bool useBlending, Material overrideMaterial)
        {
            Material material;

            for (var i = renderList.Count - 1; i >= 0; i--)
            {
                var glObject = renderList[i];
                var o = glObject.Object;
                var buffer = glObject.Buffer;

                SetupMatrices(o, camera);

                if (overrideMaterial != null)
                {
                    material = overrideMaterial;
                }
                else
                {
                    material = glObject.Materials[MaterialType.Opaque];
                    if (material == null) continue;
                    if (useBlending) SetBlending(material.Blending, material.BlendEquation, material.BlendSource, material.BlendDestination);
                    DepthTest = material.ShouldDepthTest;
                    DepthWrite = material.ShouldDepthWrite;
                    SetPolygonOffset(material.ShouldPolygonOffset, material.PolygonOffsetFactor, material.PolygonOffsetUnits);
                }

                SetFaceCulling(material);
                RenderBuffer(camera, lights, fog, material, buffer, o);
            }
        }

        private void SetupMatrices(Object3D o, Camera camera)
        {
            o.modelViewMatrix.MultiplyMatrices(camera.matrixWorldInverse, o.matrixWorld);
            o.normalMatrix = Matrix3.GetNormalMatrix(o.modelViewMatrix);
        }

        public void RenderBuffer(Camera camera, List<Light> lights, Fog fog, Material material, GeometryGroup group, Object3D o)
        {
            if (!material.IsVisible) return;

            var program = SetProgram(camera, lights, fog, material, o);

            var updateBuffers = false;
            var geometryGroupHash = (o.Id << 24) + (group.Id << 12) + program.Id;

            if (geometryGroupHash != currentGeometryGroupHash)
            {
                currentGeometryGroupHash = geometryGroupHash;
                updateBuffers = true;
            }

            // vertices
            if (updateBuffers)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glVertexBuffer.Value);
                program.SetAttributeData("position");

                var basic = material as MeshBasicMaterial;
                if (basic != null && group.uvArray != null && group.uvArray.Length > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glUVBuffer.Value);
                    program.SetAttributeData("uv");
                }

                //if (group.glCustomAttributesList != null) throw new NotImplementedException();

                if (group.colorArray != null && group.colorArray.Length > 0 && (material as MeshBasicMaterial).VertexColors != VertexColorMode.None)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glColorBuffer.GetValueOrDefault());
                    program.SetAttributeData("color");
                }

                //if (program.IsAttributeActive("normal")) throw new NotImplementedException();

                if (group.normalArray != null && group.normalArray.Length > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glNormalBuffer.Value);
                    program.SetAttributeData("normal");
                }

                if (program.IsAttributeActive("tangent")) throw new NotImplementedException();
                if (program.IsAttributeActive("uv2")) throw new NotImplementedException();
                if (group.lineDistanceArray != null && group.lineDistanceArray.Length > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glLineDistanceBuffer.Value);
                    program.SetAttributeData("lineDistance");
                }
            }

            program.DisableUnusedAttributes();

            // render mesh
            var mesh = o as Mesh;
            if (mesh != null)
            {
                var basic = material as MeshBasicMaterial;
                if ((basic != null && basic.UseWireframe))
                {
                    if (updateBuffers) GL.BindBuffer(BufferTarget.ElementArrayBuffer, group.glLineBuffer.Value);
                    GL.DrawElements(BeginMode.Lines, group.glLineCount, DrawElementsType.UnsignedInt, 0);
                }
                else
                {
                    if (updateBuffers) GL.BindBuffer(BufferTarget.ElementArrayBuffer, group.glFaceBuffer.Value);
                    GL.DrawElements(BeginMode.Triangles, group.glFaceCount, DrawElementsType.UnsignedInt, 0);
                }

                info.Render.DrawCalls++;
                info.Render.Vertices += group.glFaceCount;
                info.Render.Faces += group.glFaceCount / 3;
            }
            else
            {
                var line = o as Line;
                if (line != null)
                {
                    var lineMaterial = material as LineBasicMaterial;
                    if (previousLineWidth != lineMaterial.LineWidth)
                    {
                        GL.LineWidth(lineMaterial.LineWidth);
                        previousLineWidth = lineMaterial.LineWidth;
                    }

                    BeginMode type;
                    switch (line.Type)
                    {
                        case LineType.Pieces: type = BeginMode.Lines; break;
                        case LineType.Strip: type = BeginMode.LineStrip; break;
                        default: throw new NotSupportedException();
                    }
                    GL.DrawArrays(type, 0, group.glLineCount);

                    info.Render.DrawCalls++;
                    info.Render.Vertices += group.glLineCount;
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

        public void SetRenderTarget(RenderTarget renderTarget)
        {
            var cube = renderTarget as RenderTargetCube;
            var isCube = cube != null;

            if (renderTarget != null && !renderTarget.glFramebuffer.HasValue)
            {
                if (!renderTarget.UseDepthBuffer) renderTarget.UseDepthBuffer = true;
                if (!renderTarget.UseStencilBuffer) renderTarget.UseDepthBuffer = true;

                //renderTarget.addEventListener( 'dispose', onRenderTargetDispose );
                renderTarget.glTexture = GL.GenTexture();

                info.Memory.Textures++;

                // Setup texture, create render and frame buffers
                var isTargetPowerOfTwo = Mathf.IsPowerOfTwo(renderTarget.Resolution.Width) && Mathf.IsPowerOfTwo(renderTarget.Resolution.Height);
                var glFormat = renderTarget.Format.ToGL4();
                var glInternal = renderTarget.Format.ToGL4Internal();
                var glType = renderTarget.Type.ToGL4();

                if (isCube)
                {
                    cube.glFrameBuffers.Clear();
                    cube.glRenderBuffers.Clear();

                    GL.BindTexture(TextureTarget.TextureCubeMap, renderTarget.glTexture);
                    SetTextureParameters(TextureTarget.TextureCubeMap, renderTarget, isTargetPowerOfTwo);

                    var targets = new List<TextureTarget>()
                {
                    TextureTarget.TextureCubeMapPositiveX, TextureTarget.TextureCubeMapNegativeX,
                    TextureTarget.TextureCubeMapPositiveY, TextureTarget.TextureCubeMapNegativeY,
                    TextureTarget.TextureCubeMapPositiveZ, TextureTarget.TextureCubeMapNegativeZ,
                };

                    foreach (var target in targets)
                    {
                        var framebufferId = GL.GenFramebuffer();
                        var renderbufferId = GL.GenRenderbuffer();
                        cube.glFrameBuffers.Add(framebufferId);
                        cube.glRenderBuffers.Add(renderbufferId);

                        GL.TexImage2D(target, 0, glInternal, renderTarget.Resolution.Width, renderTarget.Resolution.Height, 0, glFormat, glType, IntPtr.Zero);

                        SetupFrameBuffer(framebufferId, renderTarget, target);
                        setupRenderBuffer(renderbufferId, renderTarget);

                    }

                    if (isTargetPowerOfTwo) GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);

                }
                else
                {
                    renderTarget.glFramebuffer = GL.GenFramebuffer();

                    if (renderTarget.shareDepthFrom != null)
                    {
                        renderTarget.glRenderbuffer = renderTarget.shareDepthFrom.glRenderbuffer;
                    }
                    else
                    {
                        renderTarget.glRenderbuffer = GL.GenRenderbuffer();
                    }

                    GL.BindTexture(TextureTarget.Texture2D, renderTarget.glTexture);
                    SetTextureParameters(TextureTarget.Texture2D, renderTarget, isTargetPowerOfTwo);

                    GL.TexImage2D(TextureTarget.Texture2D, 0, glInternal, renderTarget.Resolution.Width, renderTarget.Resolution.Height, 0, glFormat, glType, IntPtr.Zero);

                    SetupFrameBuffer(renderTarget.glFramebuffer.Value, renderTarget, TextureTarget.Texture2D);

                    if (renderTarget.shareDepthFrom != null)
                    {
                        if (renderTarget.UseDepthBuffer)
                        {
                            if (renderTarget.UseStencilBuffer)
                            {
                                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderTarget.glRenderbuffer.Value);
                            }
                            else
                            {
                                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderTarget.glRenderbuffer.Value);
                            }
                        }
                    }
                    else
                    {
                        setupRenderBuffer(renderTarget.glRenderbuffer.Value, renderTarget);
                    }

                    if (isTargetPowerOfTwo) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                }

                // Release everything
                GL.BindTexture(isCube ? TextureTarget.TextureCubeMap : TextureTarget.Texture2D, 0);
                GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
            }

            int? framebuffer;
            int width, height, vx, vy;

            if (renderTarget != null)
            {
                framebuffer = isCube ? cube.glFrameBuffers[cube.activeCubeFace] : renderTarget.glFramebuffer.Value;
                width = renderTarget.Resolution.Width;
                height = renderTarget.Resolution.Height;
                vx = 0;
                vy = 0;
            }
            else
            {
                framebuffer = null;
                width = viewportWidth;
                height = viewportHeight;
                vx = viewportX;
                vy = viewportY;
            }

            if (framebuffer != currentFramebuffer)
            {
                if (framebuffer == null)
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                }
                else
                {
                    GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebuffer.Value);
                }
                GL.Viewport(vx, vy, width, height);
                currentFramebuffer = framebuffer;
            }

            currentWidth = width;
            currentHeight = height;
        }

        private void setupRenderBuffer(int renderbufferId, RenderTarget renderTarget)
        {
            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, renderbufferId);

            if (renderTarget.UseDepthBuffer && !renderTarget.UseStencilBuffer)
            {
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent16, renderTarget.Resolution.Width, renderTarget.Resolution.Height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, renderbufferId);

                /* For some reason this is not working. Defaulting to RGBA4.
                } else if ( ! renderTarget.depthBuffer && renderTarget.stencilBuffer ) {

                    _gl.renderbufferStorage( _gl.RENDERBUFFER, _gl.STENCIL_INDEX8, renderTarget.width, renderTarget.height );
                    _gl.framebufferRenderbuffer( _gl.FRAMEBUFFER, _gl.STENCIL_ATTACHMENT, _gl.RENDERBUFFER, renderbuffer );
                */
            }
            else if (renderTarget.UseDepthBuffer && renderTarget.UseStencilBuffer)
            {
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Depth24Stencil8, renderTarget.Resolution.Width, renderTarget.Resolution.Height);
                GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthStencilAttachment, RenderbufferTarget.Renderbuffer, renderbufferId);
            }
            else
            {
                GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.Rgba4, renderTarget.Resolution.Width, renderTarget.Resolution.Height);
            }
        }

        private void SetupFrameBuffer(int framebufferId, RenderTarget renderTarget, TextureTarget target)
        {
            GL.BindFramebuffer(FramebufferTarget.Framebuffer, framebufferId);
            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, target, renderTarget.glTexture, 0);
        }

        private void SetTextureParameters(TextureTarget textureTarget, RenderTarget renderTarget, bool isTargetPowerOfTwo)
        {

		    if ( isTargetPowerOfTwo ) {
                int value;

                value = (int)renderTarget.WrapS;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureWrapS, ref value);
                value = (int)renderTarget.WrapT;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureWrapT, ref value);

                value = (int)renderTarget.MagFilter;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureMagFilter, ref value);
                value = (int)renderTarget.MinFilter;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureMinFilter, ref value);
		    } else {
                int value;

                value = (int)WrapMode.Clamp;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureWrapS, ref value);
                GL.TexParameterI(textureTarget, TextureParameterName.TextureWrapT, ref value);

                value = (int)renderTarget.MagFilter;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureMagFilter, ref value);
                value = (int)renderTarget.MinFilter;
                GL.TexParameterI(textureTarget, TextureParameterName.TextureMinFilter, ref value);
		    }

		    /*if ( _glExtensionTextureFilterAnisotropic && texture.type !== THREE.FloatType ) {

			    if ( texture.anisotropy > 1 || texture.__oldAnisotropy ) {

				    _gl.texParameterf( textureType, _glExtensionTextureFilterAnisotropic.TEXTURE_MAX_ANISOTROPY_EXT, Math.min( texture.anisotropy, _maxAnisotropy ) );
				    texture.__oldAnisotropy = texture.anisotropy;

			    }

		    }*/
        }

        private void RenderPlugins(List<RenderPlugin> plugins, Scene scene, Camera camera)
        {
            foreach (var plugin in plugins)
            {
                // reset state for plugin (to start from clean slate)
                currentProgram = -1;
                currentCamera = null;
                previousBlending = null;
                previousDepthTest = null;
                previousDepthWrite = null;
                previousDoubleSided = null;
                previousFlipSided = null;
                currentGeometryGroupHash = null;
                currentMaterialId = null;
                lightsNeedUpdate = true;

                plugin.Render(scene, camera, currentWidth, currentHeight);

                // reset state after plugin (anything could have changed)
                currentProgram = -1;
                currentCamera = null;
                previousBlending = null;
                previousDepthTest = null;
                previousDepthWrite = null;
                previousDoubleSided = null;
                previousFlipSided = null;
                currentGeometryGroupHash = null;
                currentMaterialId = null;
                lightsNeedUpdate = true;
            }
        }

        private void SetPolygonOffset(bool polygonOffset, float factor, float units)
        {
            if (previousPolygonOffset != polygonOffset)
            {
                if (polygonOffset) GL.Enable(EnableCap.PolygonOffsetFill);
                else GL.Disable(EnableCap.PolygonOffsetFill);

                previousPolygonOffset = polygonOffset;
            }

            if (polygonOffset && (previousPolygonOffsetFactor != factor || previousPolygonOffsetUnits != units))
            {
                GL.PolygonOffset(factor, units);
                previousPolygonOffsetFactor = factor;
                previousPolygonOffsetUnits = units;
            }
        }

        public bool? DepthWrite
        {
            get
            {
                return previousDepthWrite;
            }
            set
            {
                if (previousDepthWrite != value)
                {
                    if (value.HasValue) GL.DepthMask(value.Value);
                    previousDepthWrite = value;
                }
            }
        }

        public bool? DepthTest
        {
            get
            {
                return previousDepthTest;
            }
            set
            {
                if (previousDepthTest != value)
                {
                    if (value.HasValue)
                    {
                        if (value.Value) GL.Enable(EnableCap.DepthTest);
                        else GL.Disable(EnableCap.DepthTest);
                    }
                    previousDepthTest = value;
                }
            }
        }

        public void SetBlending(BlendMode blending, BlendEquationMode equation = BlendEquationMode.FuncAdd, BlendingFactorSrc? source = null, BlendingFactorDest? destination = null)
        {
            if (blending != previousBlending)
            {
                switch (blending)
                {
                    case BlendMode.None:
                        GL.Disable(EnableCap.Blend);
                        break;
                    case BlendMode.Additive:
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);
                        GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.One);
                        break;
                    case BlendMode.Substractive:
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);
                        GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcColor);
                        break;
                    case BlendMode.Multiply:
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquation(BlendEquationMode.FuncAdd);
                        GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.SrcColor);
                        break;
                    case BlendMode.Custom:
                        GL.Enable(EnableCap.Blend);
                        break;
                    default:
                        GL.Enable(EnableCap.Blend);
                        GL.BlendEquationSeparate(BlendEquationMode.FuncAdd, BlendEquationMode.FuncAdd);
                        GL.BlendFuncSeparate(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha, BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                        break;
                }

                previousBlending = blending;
            }

            if (blending == BlendMode.Custom)
            {
                if (equation != oldBlendEquation)
                {
                    GL.BlendEquation(equation);
                    oldBlendEquation = equation;
                }

                if (source != oldBlendSrc || destination != oldBlendDst)
                {
                    GL.BlendFunc(source.Value, destination.Value);
                    oldBlendSrc = source;
                    oldBlendDst = destination;
                }
            }
            else
            {
                oldBlendEquation = null;
                oldBlendSrc = null;
                oldBlendDst = null;
            }
        }

        private int ReversePainterSortStable(Scene.BufferInfo a, Scene.BufferInfo b)
        {
            var v = a.Z.CompareTo(b.Z);
            if (v != 0) return v;
            return Nullable.Compare(a.Id, b.Id);
        }

        private int PainterSortStable(Scene.BufferInfo a, Scene.BufferInfo b)
        {
            var v = b.Z.CompareTo(a.Z);
            if (v != 0) return v;
            return Nullable.Compare(a.Id, b.Id);
        }

        private void ProjectObject(Scene scene, Object3D o, Camera camera)
        {
            if (!o.IsVisible) return;

            List<Scene.BufferInfo> glObjects;
            if (scene.glObjects.TryGetValue(o.Id, out glObjects))
            {
                if (!o.frustumCulled || frustum.IntersectsObject(o))
                {
                    UpdateObject(scene, o);

                    for (var i = 0; i < glObjects.Count; i++)
                    {
                        var glObject = glObjects[i];

                        UnrollBufferMaterial(glObject);

                        glObject.ShouldRender = true;

                        if (shouldSortObjects)
                        {
                            if (o.Zdepth.HasValue)
                            {
                                glObject.Z = o.Zdepth.Value;
                            }
                            else
                            {
                                var v = Vector3.FromPosition(o.matrixWorld);
                                v.ApplyProjection(projectionScreenMatrix);
                                glObject.Z = v.z;
                            }
                        }
                    }
                }
            }
            foreach (var c in o.Children) ProjectObject(scene, c, camera);
        }

        private void UnrollBufferMaterial(Scene.BufferInfo glObject)
        {
            var o = glObject.Object;
            var buffer = glObject.Buffer;
            var geometry = o.geometry;
            var material = o.Material;

            if (material != null)
            {
                if (material.IsTransparent)
                {
                    glObject.Materials[MaterialType.Transparent] = material;
                    transparentObjects.Add(glObject);
                }
                else
                {
                    glObject.Materials[MaterialType.Opaque] = material;
                    opaqueObjects.Add(glObject);
                }
            }
        }

        private void UpdateObject(Scene scene, Object3D o)
        {
            var geometry = o.geometry;

            Material material = null;

            var bufferedGeometry = geometry as BufferedGeometry;
            if (bufferedGeometry != null)
            {
                SetDirectBuffers(bufferedGeometry, BufferUsageHint.DynamicDraw);
            }
            else
            {
                var mesh = o as Mesh;
                if (mesh != null)
                {
                    // check all geometry groups
                    if (geometry.BuffersNeedUpdate || geometry.groupsNeedUpdate)
                    {

                        if (bufferedGeometry != null) InitDirectBuffers(bufferedGeometry);
                        else if (mesh != null) InitGeometryGroups(scene, mesh, geometry);
                    }

                    foreach (var group in geometry.Groups)
                    {
                        material = mesh.Material;

                        if (geometry.BuffersNeedUpdate || geometry.groupsNeedUpdate)
                        {
                            InitMeshBuffers(group, mesh);
                        }

                        if (geometry.VerticesNeedUpdate || geometry.MorphTargetsNeedUpdate || geometry.ElementsNeedUpdate ||
                             geometry.UvsNeedUpdate || geometry.NormalsNeedUpdate ||
                             geometry.ColorsNeedUpdate || geometry.TangentsNeedUpdate)
                        {
                            SetMeshBuffers(group, mesh, BufferUsageHint.DynamicDraw, !geometry.dynamic, material);
                        }
                    }

                    geometry.VerticesNeedUpdate = false;
                    geometry.MorphTargetsNeedUpdate = false;
                    geometry.ElementsNeedUpdate = false;
                    geometry.UvsNeedUpdate = false;
                    geometry.NormalsNeedUpdate = false;
                    geometry.ColorsNeedUpdate = false;
                    geometry.TangentsNeedUpdate = false;
                    geometry.BuffersNeedUpdate = false;
                }
                else
                {
                    var line = o as Line;
                    if (line != null)
                    {
                        material = line.Material;

                        if (geometry.VerticesNeedUpdate || geometry.ColorsNeedUpdate || geometry.LineDistancesNeedUpdate)
                        {
                            SetLineBuffers(geometry, BufferUsageHint.DynamicDraw);
                        }

                        geometry.VerticesNeedUpdate = false;
                        geometry.ColorsNeedUpdate = false;
                        geometry.LineDistancesNeedUpdate = false;
                    }
                    else
                    {
                        var pointCloud = o as PointCloud;
                        throw new NotImplementedException();
                    }
                }
            }
        }

        private void SetLineBuffers(Geometry geometry, BufferUsageHint hint)
        {
            var group = geometry.groupsList[0];

            if (geometry.VerticesNeedUpdate)
            {
                var offset = 0;
                foreach (var vertex in geometry.vertices)
                {
                    group.vertexArray[offset++] = vertex.x;
                    group.vertexArray[offset++] = vertex.y;
                    group.vertexArray[offset++] = vertex.z;
                }

                if (!group.glVertexBuffer.HasValue) group.glVertexBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glVertexBuffer.Value);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.vertexArray.Length * sizeof(float)), group.vertexArray, hint);
            }

            if (geometry.ColorsNeedUpdate && geometry.vertexColors != null)
            {
                var offset = 0;
                foreach (var color in geometry.vertexColors)
                {
                    group.colorArray[offset++] = color.R;
                    group.colorArray[offset++] = color.G;
                    group.colorArray[offset++] = color.B;
                }

                if (!group.glColorBuffer.HasValue) group.glColorBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glColorBuffer.Value);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.colorArray.Length * sizeof(float)), group.colorArray, hint);
            }

            if (geometry.LineDistancesNeedUpdate)
            {
                var verts = geometry.vertices;
                if (verts != null && verts.Count > 0)
                {
                    var d = 0f;
                    group.lineDistanceArray[0] = 0;
                    for (var i = 1; i < verts.Count; i++)
                    {
                        d += verts[i].DistanceTo(verts[i - 1]);
                        group.lineDistanceArray[i] = d;
                    }

                    if (!group.glLineDistanceBuffer.HasValue) group.glLineDistanceBuffer = GL.GenBuffer();
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glLineDistanceBuffer.Value);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.lineDistanceArray.Length * sizeof(float)), group.lineDistanceArray, hint);
                }
            }
        }

        private void SetMeshBuffers(GeometryGroup group, Mesh mesh, BufferUsageHint hint, bool shouldDipose, Material material)
        {
            if (!group.inittedArrays) return;

            var normalType = BufferGuessNormalType(material);
            var vertexColorType = BufferGuessVertexColorType(material);
            var uvType = BufferGuessUVType(material);
            var needsSmoothNormals = normalType == ShadingMode.Smooth;
            var geometry = mesh.geometry;

            HandleDirtyVertices(geometry, group, hint);

            if (geometry.MorphTargetsNeedUpdate && geometry.MorphTargets != null && geometry.MorphTargets.Count > 0) throw new NotImplementedException();
            if (geometry.SkinIndices != null && geometry.SkinIndices.Count > 0) throw new NotImplementedException();

            if (geometry.ColorsNeedUpdate && geometry.vertexColors != null)
            {
                var offset = 0;
                foreach (var color in geometry.vertexColors)
                {
                    group.colorArray[offset++] = color.R;
                    group.colorArray[offset++] = color.G;
                    group.colorArray[offset++] = color.B;
                }

                if (!group.glColorBuffer.HasValue) group.glColorBuffer = GL.GenBuffer();
                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glColorBuffer.Value);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.colorArray.Length * sizeof(float)), group.colorArray, hint);
            }

            if (geometry.TangentsNeedUpdate && geometry.hasTangents) throw new NotImplementedException();

            HandleDirtyNormals(geometry, group, normalType, hint);


            HandleDirtyUvs(geometry, group, uvType, hint);

            if (geometry.UvsNeedUpdate && geometry.faceVertexUvs.Count >= 2 && uvType) throw new NotImplementedException();

            if (geometry.ElementsNeedUpdate)
            {
                uint vertexIndex = 0;
                var offsetFace = 0;
                var offsetLine = 0;

                for (var f = 0; f < group.FacesIndicies.Count; f++)
                {
                    group.faceArray[offsetFace] = vertexIndex;
                    group.faceArray[offsetFace + 1] = vertexIndex + 1;
                    group.faceArray[offsetFace + 2] = vertexIndex + 2;
                    offsetFace += 3;
                    group.lineArray[offsetLine] = vertexIndex;
                    group.lineArray[offsetLine + 1] = vertexIndex + 1;
                    group.lineArray[offsetLine + 2] = vertexIndex;
                    group.lineArray[offsetLine + 3] = vertexIndex + 2;
                    group.lineArray[offsetLine + 4] = vertexIndex + 1;
                    group.lineArray[offsetLine + 5] = vertexIndex + 2;
                    offsetLine += 6;
                    vertexIndex += 3;
                }

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, group.glFaceBuffer.Value);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(group.faceArray.Length * sizeof(uint)), group.faceArray, hint);

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, group.glLineBuffer.Value);
                GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(group.lineArray.Length * sizeof(uint)), group.lineArray, hint);
            }

            if (group.glCustomAttributesList != null && group.glCustomAttributesList.Count > 0) throw new NotImplementedException();
            if (shouldDipose) throw new NotImplementedException();
        }

        private void HandleDirtyNormals(Geometry geometry, GeometryGroup group, ShadingMode normalType, BufferUsageHint hint)
        {
            if (geometry.NormalsNeedUpdate && normalType != ShadingMode.None)
            {
                var needsSmoothNormals = normalType == ShadingMode.Smooth;
                var offset_normal = 0;
                foreach (var faceIndex in group.FacesIndicies)
                {
                    var face = geometry.faces[faceIndex];

                    if (needsSmoothNormals)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            Vector3 vn;
                            switch (i)
                            {
                                case 0: vn = face.NormalA; break;
                                case 1: vn = face.NormalB; break;
                                case 2: vn = face.NormalC; break;
                                default: throw new NotSupportedException();
                            }
                            group.normalArray[offset_normal++] = vn.x;
                            group.normalArray[offset_normal++] = vn.y;
                            group.normalArray[offset_normal++] = vn.z;
                        }
                    }
                    else
                    {
                        var faceNormal = Vector3.Zero;
                        faceNormal.Add(face.NormalA);
                        faceNormal.Add(face.NormalB);
                        faceNormal.Add(face.NormalC);
                        faceNormal.Divide(3);

                        for (var i = 0; i < 3; i++)
                        {
                            group.normalArray[offset_normal++] = faceNormal.x;
                            group.normalArray[offset_normal++] = faceNormal.y;
                            group.normalArray[offset_normal++] = faceNormal.z;
                        }

                    }
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glNormalBuffer.Value);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.normalArray.Length * sizeof(float)), group.normalArray, hint);
            }
        }

        private void HandleDirtyUvs(Geometry geometry, GeometryGroup group, bool uvType, BufferUsageHint hint)
        {
            if (geometry.UvsNeedUpdate && geometry.faceVertexUvs.Count >= 1 && geometry.faceVertexUvs[0].Count > 0 && uvType)
            {
                var offset_uv = 0;
                var uvFaceSets = geometry.faceVertexUvs[0];
                foreach (var faceIndex in group.FacesIndicies)
                {
                    var uvFaceSet = uvFaceSets[faceIndex];

                    group.uvArray[offset_uv++] = uvFaceSet.A.x;
                    group.uvArray[offset_uv++] = uvFaceSet.A.y;
                    group.uvArray[offset_uv++] = uvFaceSet.B.x;
                    group.uvArray[offset_uv++] = uvFaceSet.B.y;
                    group.uvArray[offset_uv++] = uvFaceSet.C.x;
                    group.uvArray[offset_uv++] = uvFaceSet.C.y;
                }

                if (offset_uv > 0)
                {
                    GL.BindBuffer(BufferTarget.ArrayBuffer, group.glUVBuffer.Value);
                    GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.uvArray.Length * sizeof(float)), group.uvArray, hint);
                }
            }
        }

        private void HandleDirtyVertices(Geometry geometry, GeometryGroup group, BufferUsageHint hint)
        {
            var offset = 0;
            if (geometry.VerticesNeedUpdate)
            {
                for (var f = 0; f < group.FacesIndicies.Count; f++)
                {
                    var index = group.FacesIndicies[f];
                    var face = geometry.faces[index];
                    var v1 = geometry.vertices[face.A];
                    var v2 = geometry.vertices[face.B];
                    var v3 = geometry.vertices[face.C];

                    group.vertexArray[offset++] = v1.x;
                    group.vertexArray[offset++] = v1.y;
                    group.vertexArray[offset++] = v1.z;
                    group.vertexArray[offset++] = v2.x;
                    group.vertexArray[offset++] = v2.y;
                    group.vertexArray[offset++] = v2.z;
                    group.vertexArray[offset++] = v3.x;
                    group.vertexArray[offset++] = v3.y;
                    group.vertexArray[offset++] = v3.z;
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, group.glVertexBuffer.Value);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(group.vertexArray.Length * sizeof(float)), group.vertexArray, hint);
            }
        }

        void SetDirectBuffers(BufferedGeometry bufferedGeometry, BufferUsageHint bufferUsageHint)
        {
            throw new NotImplementedException();
        }

        private void HandleAddedOrRemovedObjects(Scene scene)
        {
            while (scene.objectsAdded.Count > 0)
            {
                var o = scene.objectsAdded[0];
                scene.objectsAdded.Remove(o);
                AddObject(o, scene);
            }

            while (scene.objectsRemoved.Count > 0)
            {
                var o = scene.objectsRemoved[0];
                scene.objectsRemoved.Remove(o);
                RemoveObject(o, scene);
            }
        }

        private void RemoveObject(Object3D o, Scene scene)
        {
            throw new NotImplementedException();
        }

        private void AddObject(Object3D o, Scene scene)
        {
            var geometry = o.geometry;

            if (geometry != null && !geometry.glInit)
            {
                geometry.glInit = true;
                //geometry.addEventListener( 'dispose', onGeometryDispose );

                var bufferedGeometry = geometry as BufferedGeometry;

                if (bufferedGeometry != null)
                {
                    InitDirectBuffers(bufferedGeometry);
                }
                else
                {
                    var mesh = o as Mesh;
                    var line = o as Line;
                    var pointCloud = o as PointCloud;

                    if (mesh != null)
                    {
                        if (mesh.glActive) RemoveObject(mesh, scene);
                        InitGeometryGroups(scene, mesh, geometry);
                    }
                    else if (line != null)
                    {
                        if (geometry.groupsList == null)
                        {
                            CreateLineBuffers(geometry);
                            InitLineBuffers(geometry, line);

                            geometry.VerticesNeedUpdate = true;
                            geometry.ColorsNeedUpdate = true;
                            geometry.LineDistancesNeedUpdate = true;
                        }
                    }
                    else if (pointCloud != null)
                    {
                        var group = geometry.groupsList[0];
                        if (group.glVertexBuffer == null)
                        {
                            CreateParticleBuffers(geometry);
                            InitParticleBuffers(geometry, pointCloud);
                            geometry.VerticesNeedUpdate = true;
                            geometry.ColorsNeedUpdate = true;
                        }
                    }
                }
            }

            if (!o.glActive)
            {
                var mesh = o as Mesh;
                if (mesh != null)
                {
                    var bufferedGeometry = geometry as BufferedGeometry;

                    if (bufferedGeometry != null)
                    {
                        //AddBuffer(scene.glObjects, bufferedGeometry, mesh);
                        throw new NotImplementedException();
                    }
                    else if (geometry != null)
                    {
                        foreach (var group in geometry.Groups)
                        {
                            AddBuffer(scene.glObjects, group, mesh);
                        }
                    }

                }
                else if (o is Line || o is PointCloud)
                {
                    geometry = o.geometry;
                    AddBuffer(scene.glObjects, geometry.groupsList[0], o);
                }

                o.glActive = true;
            }
        }

        private void InitDirectBuffers(BufferedGeometry bufferedGeometry)
        {
            throw new NotImplementedException();
        }

        private void InitLineBuffers(Geometry geometry, Line line)
        {
            var nvertices = geometry.vertices.Count;
            var group = geometry.groupsList[0];
            group.vertexArray = new float[nvertices * 3];
            group.colorArray = new float[nvertices * 3];
            group.lineDistanceArray = new float[nvertices * 1];
            group.glLineCount = nvertices;
        }

        private void CreateLineBuffers(Geometry geometry)
        {
            var group = new GeometryGroup()
            {
                glVertexBuffer = GL.GenBuffer(),
                glColorBuffer = GL.GenBuffer(),
                glLineDistanceBuffer = GL.GenBuffer()
            };
            geometry.groupsList = new List<GeometryGroup>() { group };
            info.Memory.Geometries++;
        }

        private void AddBuffer(Dictionary<uint, List<Scene.BufferInfo>> dict, GeometryGroup group, Object3D o)
        {
            List<Scene.BufferInfo> list;
            if (!dict.TryGetValue(o.Id, out list))
            {
                list = new List<Scene.BufferInfo>();
                dict.Add(o.Id, list);
            }

            list.Add(new Scene.BufferInfo()
            {
                Id = o.Id,
                Buffer = group,
                Object = o,
                Materials = new Dictionary<MaterialType, Material>()
                {
                    {MaterialType.Opaque, null},
                    {MaterialType.Transparent, null},
                    {MaterialType.Default, null}
                },
                Z = 0
            }
        );
        }

        private void InitParticleBuffers(Geometry geometry, PointCloud pointCloud)
        {
            throw new NotImplementedException();
        }

        private void CreateParticleBuffers(Geometry geometry)
        {
            throw new NotImplementedException();
        }

        private void InitGeometryGroups(Scene scene, Mesh mesh, Geometry geometry)
        {
            //var g, geometryGroup, material,
            var addBuffers = false;
            var material = mesh.Material;
            var bufferedGeometry = geometry as BufferedGeometry;

            //If isn't buffered or is buffered and needs update
            if (!(bufferedGeometry != null && !bufferedGeometry.groupsNeedUpdate))
            {
                scene.glObjects.Remove(mesh.Id);
                geometry.MakeGroups();
                geometry.groupsNeedUpdate = false;
            }

            // create separate VBOs per geometry chunk
            foreach (var group in geometry.Groups)
            {
                // initialise VBO on the first access
                if (!group.glVertexBuffer.HasValue)
                {
                    CreateMeshBuffers(group);
                    InitMeshBuffers(group, mesh);
                    geometry.VerticesNeedUpdate = true;
                    geometry.MorphTargetsNeedUpdate = true;
                    geometry.ElementsNeedUpdate = true;
                    geometry.UvsNeedUpdate = true;
                    geometry.NormalsNeedUpdate = true;
                    geometry.TangentsNeedUpdate = true;
                    geometry.ColorsNeedUpdate = true;
                    addBuffers = true;
                }
                else
                {
                    addBuffers = false;
                }

                if (addBuffers || mesh.glActive == false) AddBuffer(scene.glObjects, group, mesh);
            }
            mesh.glActive = true;
        }

        private void InitMeshBuffers(GeometryGroup geometryGroup, Mesh mesh)
        {
            var geometry = mesh.geometry;
            var faces3 = geometryGroup.FacesIndicies;
            var nvertices = faces3.Count * 3;
            var ntris = faces3.Count * 1;
            var nlines = faces3.Count * 3;
            var material = mesh.Material;
            var uvType = BufferGuessUVType(material);
            var normalType = BufferGuessNormalType(material);
            var vertexColorType = BufferGuessVertexColorType(material);

            var msg = "uvType:{0} normalType:{1} vertexColorType:{2} mesh:{3} geoGroup:{4} material:{5}";
            Debug.WriteLine(msg, uvType, normalType, vertexColorType, mesh, geometryGroup, material);

            geometryGroup.vertexArray = new float[nvertices * 3];

            if (normalType != ShadingMode.None) geometryGroup.normalArray = new float[nvertices * 3];
            if (geometry.hasTangents) geometryGroup.tangentArray = new float[nvertices * 4];
            if (vertexColorType != VertexColorMode.None) geometryGroup.colorArray = new float[nvertices * 3];
            if (uvType)
            {
                if (geometry.faceVertexUvs.Count > 0) geometryGroup.uvArray = new float[nvertices * 2];
                if (geometry.faceVertexUvs.Count > 1) geometryGroup.uv2Array = new float[nvertices * 2];
            }

            if (geometry.SkinWeights != null && geometry.SkinIndices != null)
            {
                if (geometry.SkinWeights.Count > 0 && geometry.SkinIndices.Count > 0)
                {
                    geometryGroup.skinIndexArray = new float[nvertices * 4];
                    geometryGroup.skinWeightArray = new float[nvertices * 4];
                }
            }

            geometryGroup.faceArray = new uint[ntris * 3];
            geometryGroup.lineArray = new uint[nlines * 2];

            if (geometryGroup.MorphTargetCount > 0)
            {
                geometryGroup.morphTargetsArrays = new List<float[]>();

                for (var m = 0; m < geometryGroup.MorphTargetCount; m++)
                {
                    geometryGroup.morphTargetsArrays.Add(new float[nvertices * 3]);
                }
            }

            if (geometryGroup.MorphNormalsCount > 0)
            {
                geometryGroup.morphNormalsArrays = new List<float[]>();

                for (var m = 0; m < geometryGroup.MorphNormalsCount; m++)
                {
                    geometryGroup.morphNormalsArrays.Add(new float[nvertices * 3]);
                }
            }

            geometryGroup.glFaceCount = ntris * 3;
            geometryGroup.glLineCount = nlines * 2;

            geometryGroup.inittedArrays = true;
        }

        private VertexColorMode BufferGuessVertexColorType(Material material)
        {
            if (material is MeshBasicMaterial)return (material as MeshBasicMaterial).VertexColors;
            return VertexColorMode.None;
        }

        private ShadingMode BufferGuessNormalType(Material material)
        {
            var basic = material as MeshBasicMaterial;

            // only MeshBasicMaterial and MeshDepthMaterial don't need normals
            if (basic != null && basic.EnvironmentMap == null && !material.GetType().IsSubclassOf(typeof(MeshBasicMaterial)) /*|| material is MeshDepthMaterial*/) return ShadingMode.None;

            return material.Shading;
        }

        private bool BufferGuessUVType(Material material)
        {
            if (material is CustomShaderMaterial) return true;

            // material must use some texture to require uvs
            var basic = material as MeshBasicMaterial;
            if (basic != null)
            {
                return basic.DiffuseMap != null || basic.LightMap != null || basic.SpecularMap != null || basic.AlphaMap != null || basic.NormalMap != null;
            }

            return false;
        }

        //private Material GetBufferMaterial(Object3D o, GeometryGroup group)
        //{
        //    if (!(o.Material is MeshBasicMaterial)) throw new NotImplementedException();
        //    return mesh.Material;
        //}

        private void CreateMeshBuffers(GeometryGroup group)
        {
            group.glVertexBuffer = GL.GenBuffer();
            group.glNormalBuffer = GL.GenBuffer();
            group.glTangentBuffer = GL.GenBuffer();
            group.glColorBuffer = GL.GenBuffer();
            group.glUVBuffer = GL.GenBuffer();
            group.glUV2Buffer = GL.GenBuffer();

            group.glSkinIndicesBuffer = GL.GenBuffer();
            group.glSkinWeightsBuffer = GL.GenBuffer();

            group.glFaceBuffer = GL.GenBuffer();
            group.glLineBuffer = GL.GenBuffer();

            if (group.MorphTargetCount > 0)
            {
                group.glMorphTargetsBuffers = new List<int>();

                for (var m = 0; m < group.MorphTargetCount; m++)
                {
                    group.glMorphTargetsBuffers.Add(GL.GenBuffer());
                }
            }

            if (group.MorphNormalsCount > 0)
            {
                group.glMorphNormalsBuffers = new List<int>();

                for (var m = 0; m < group.MorphNormalsCount; m++)
                {
                    group.glMorphNormalsBuffers.Add(GL.GenBuffer());
                }
            }

            info.Memory.Geometries++;
        }

        private void UpdateSkeletons(Object3D o)
        {
            var skinned = o as SkinnedMesh;
            if (skinned != null) skinned.Skeleton.Update();

            foreach (var c in o.Children) UpdateSkeletons(c);
        }

        internal void SetTexture(Texture texture, int slot)
        {
            if (texture.NeedsUpdate)
            {
                if (!texture.glInit)
                {
                    texture.glInit = true;
                    //texture.addEventListener( 'dispose', onTextureDispose );
                    texture.glTexture = GL.GenTexture();
                    info.Memory.Textures++;
                }

                GL.ActiveTexture(TextureUnit.Texture0 + slot);
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);

                //GL.PixelStore(PixelStoreParameter.flip _gl.UNPACK_FLIP_Y_WEBGL, texture.flipY );
                //GL.PixelStore( PixelStoreParameter.a _gl.UNPACK_PREMULTIPLY_ALPHA_WEBGL, texture.premultiplyAlpha );
                GL.PixelStore(PixelStoreParameter.UnpackAlignment, texture.UnpackAlignment);

                var isImagePowerOfTwo = Mathf.IsPowerOfTwo(texture.Resolution.Width) && Mathf.IsPowerOfTwo(texture.Resolution.Height);
                var glFormat = texture.Format.ToGL4();
                var glType = texture.Type.ToGL4();

                SetTextureParameters(TextureTarget.Texture2D, texture);

                // TODO Investigate var mipmap, mipmaps = texture.mipmaps;
                var mipmaps = new List<dynamic>();

                if (texture is DataTexture)
                {

                    // use manually created mipmaps if available
                    // if there are no manual mipmaps
                    // set 0 level mipmap and then use GL to generate other mipmap levels
                    if (mipmaps.Count > 0 && isImagePowerOfTwo)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }
                else if (texture is CompressedTexture)
                {
                    throw new NotImplementedException();
                }
                else
                { // regular Texture (image, video, canvas)

                    // use manually created mipmaps if available
                    // if there are no manual mipmaps
                    // set 0 level mipmap and then use GL to generate other mipmap levels

                    if (mipmaps.Count > 0 && isImagePowerOfTwo)
                    {
                        throw new NotImplementedException();
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                }

                //if (texture.ShouldGenerateMipmaps && isImagePowerOfTwo) GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
                //texture.NeedsUpdate = false;
                //if (texture.OnUpdate != null) texture.OnUpdate();
            }
            else
            {

                GL.ActiveTexture(TextureUnit.Texture0 + slot);
                GL.BindTexture(TextureTarget.Texture2D, texture.glTexture);

            }
        }

        private void SetTextureParameters(TextureTarget textureType, Texture texture)
        {
            int value;

            value = (int)texture.WrapS;
            GL.TexParameterI(textureType, TextureParameterName.TextureWrapS, ref value);
            value = (int)texture.WrapT;
            GL.TexParameterI(textureType, TextureParameterName.TextureWrapT, ref value);

            value = (int)texture.MagFilter;
            GL.TexParameterI(textureType, TextureParameterName.TextureMagFilter, ref value);
            value = (int)texture.MinFilter;
            GL.TexParameterI(textureType, TextureParameterName.TextureMinFilter, ref value);
        }
    }
}