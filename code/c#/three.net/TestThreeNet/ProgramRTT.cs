using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Extras.Curves;
using Three.Net.Extras.Geometries;
using Three.Net.Extras.Helpers;
using Three.Net.Lights;
using Three.Net.Loaders;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Scenes;
using Three.Net.Textures;

namespace TestThreeNet
{
    class ProgramRTT
    {
        private static Renderer renderer;
        private static float previousTime = 0;
        private static Scene scene, sceneRTT, sceneScreen;
        private static Camera camera, cameraRTT;
        private static Mesh zmesh1,zmesh2;

        static void Main(string[] args)
        {
            var mediaPath = Path.GetFullPath("../../../../../js/r68/examples/");
            var texturesPath = Path.Combine(mediaPath, "textures");

            renderer = new Renderer();

            Init();

            
            var stopwatch = Stopwatch.StartNew();
            while (!renderer.Done)
            {
                var now = (float)stopwatch.Elapsed.TotalSeconds;
                var deltaTime = now - previousTime;
                previousTime = now;

                Update(deltaTime, now);

                Render();
            }
        }

        private static void Render()
        {
            renderer.Clear();
            renderer.RenderFrame(sceneRTT, cameraRTT, rtTexture, true); // Render first scene into texture
            renderer.RenderFrame(sceneScreen, cameraRTT); // Render full screen quad with generated texture

            // Render second scene to screen
            // (using first scene as regular texture)
            renderer.RenderFrame(scene, camera);
        }

        private static CustomShaderMaterial material;
        private static RenderTarget rtTexture;
        private static void Update(float deltaTime, float now)
        {
            var mp = renderer.MousePosition;
            mp.x -= renderer.Width / 2f;
            mp.y -= renderer.Height / 2f;

            var pos = camera.Position;
            pos.x += (mp.x - pos.x) * deltaTime;
            pos.y += (-mp.y - pos.y) * deltaTime;
            camera.Position = pos;
            camera.LookAt(Vector3.Zero);

            if (zmesh1 != null && zmesh2 != null)
            {
                var r = zmesh1.Rotation;
                r.y = -now;
                zmesh1.Rotation = r;

                r = zmesh2.Rotation;
                r.y = -now  + (Mathf.Pi / 2);
                zmesh2.Rotation = r;
            }

            //if (material.uniforms.time.value > 1 || material.uniforms.time.value < 0)
            //{

            //    delta *= -1;

            //}

            //material.uniforms.time.value += deltaTime;
        }

        private static void Init()
        {
            camera = new PerspectiveCamera(renderer, 30, 1, 10000 )
            {
                Position = new Vector3(0,0,100)
            };

				cameraRTT = new OrthographicCamera(renderer, -10000, 10000 )
                {
                    Position= new Vector3(0,0,100)
                };

            scene = new Scene();
            sceneRTT = new Scene();
		    sceneScreen = new Scene();

				var light = new DirectionalLight( Color.White )
                {
                    Position = Vector3.UnitZ.Normalized()
                };
            sceneRTT.Add( light );

				light = new DirectionalLight(new Color(0xffaaaa))
                {
                    Position = Vector3.UnitNegativeZ.Normalized(),
                    Intensity = 1.5f
                };
				sceneRTT.Add( light );

				rtTexture = new RenderTarget(renderer.Width, renderer.Height)
            {
                MinFilter = TextureMinFilter.Linear,
                MagFilter = TextureMagFilter.Nearest,
                Format = Three.Net.Renderers.PixelFormat.RGB
            };

            var vertexShaderSource = @"
varying vec2 vUv;
void main() 
{
	vUv = uv;
	gl_Position = projectionMatrix * modelViewMatrix * vec4( position, 1.0 );
}";

            var fragment_shader_screenSource = @"
varying vec2 vUv;
uniform sampler2D tDiffuse;
void main() 
{
	gl_FragColor = texture2D( tDiffuse, vUv );
}";
            var fragment_shader_pass_1Source = @"
varying vec2 vUv;
uniform float time;
void main() 
{
	float r = vUv.x;
	if( vUv.y < 0.5 ) r = 0.0;
	float g = vUv.y;
	if( vUv.x < 0.5 ) g = 0.0;

	gl_FragColor = vec4( r, g, time, 1.0 );
}";
				material = new CustomShaderMaterial(renderer,vertexShaderSource,fragment_shader_pass_1Source, m => { });

				var materialScreen = new CustomShaderMaterial(renderer, vertexShaderSource, fragment_shader_screenSource, m => {})
                {
                    ShouldDepthWrite = false
                };

				var plane = new PlaneGeometry( renderer.Width, renderer.Height);
				var quad = new Mesh( plane, material )
                {
                    Position = new Vector3(0,0,-100)
                };
            sceneRTT.Add( quad );

				var geometry = new TorusGeometry( 100, 25, 15, 30 );

				var mat1 = new MeshPhongMaterial(renderer)
                {
                    Diffuse = new Color(0x555555),
                    Specular = new Color(0xffaa00),
                    Shininess = 5 
                };
				var mat2 = new MeshPhongMaterial(renderer)
                {
                    Diffuse = new Color(0x550000),
                    Specular = new Color(0xff2200),
                    Shininess = 5 
                };

				zmesh1 = new Mesh( geometry, mat1 )
                {
                    Position = new Vector3( 0, 0, 100 ),
                    Scale = new Vector3( 1.5f, 1.5f, 1.5f )
                };
				sceneRTT.Add( zmesh1 );

				zmesh2 = new Mesh( geometry, mat2 )
                {
                    Position = new Vector3( 0, 150, 100 ),
                    Scale = new Vector3( 0.75f, 0.75f, 0.75f)
                };
				sceneRTT.Add( zmesh2 );

				quad = new Mesh( plane, materialScreen ){
                    Position = new Vector3(0,0,-100)
                };
				sceneScreen.Add( quad );

				var n = 5;
            var sphereGeometry = new SphereGeometry( 10, 64, 32 );
            var material2 = new MeshBasicMaterial(renderer)
            {
                Diffuse = Color.White,
                DiffuseMap = rtTexture
            };

				for( var j = 0; j < n; j ++ ) {

					for( var i = 0; i < n; i ++ ) {

                        var mesh = new Mesh(sphereGeometry, material2)
                        {
                            Position = new Vector3(
                                ( i - ( n - 1 ) / 2 ) * 20, 
                                ( j - ( n - 1 ) / 2 ) * 20,
                                0),
                                Rotation = new Euler(0,-Mathf.Pi / 2, 0)
                        };
                        scene.Add( mesh );
					}
				}

                renderer.ShouldAutoClear = false;
        }
    }
}