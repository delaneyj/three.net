using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using Three.Net.Cameras;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Plugins;
using Three.Net.Renderers.Shaders;
using Three.Net.Scenes;
using Three.Net.Textures;

namespace Three.Net.Extras.Renderers.Plugins
{
    public class DepthPassPlugin : RenderPlugin
    {
        bool IsEnabled = false;
        Frustum frustum = Frustum.Empty;
        Matrix4 projectionScreenMatrix = Matrix4.Identity;
List<Scene.BufferInfo> renderList = new List<Scene.BufferInfo>();
RenderTarget renderTarget = null;
Material depthMaterial = null;

        protected internal override void Init(Renderer renderer)
        {
            base.Init(renderer);

            throw new NotImplementedException("Todo Ian");
        }

        protected internal override void Render(Scene scene, Camera camera, int viewportWidth, int viewportHeight)
        {
            if (!IsEnabled) return;
            Update(scene, camera);
        }

        private void Update(Scene scene, Camera camera)
        {
            // set GL state for depth map
		    GL.ClearColor( 1, 1, 1, 1 );
		    GL.Disable( EnableCap.Blend);
            renderer.DepthTest = true;

		// update scene
		if ( scene.AutoUpdate) scene.UpdateMatrixWorld();

		// update camera matrices and frustum
		camera.matrixWorldInverse = Matrix4.GetInverse(camera.matrixWorld);
            projectionScreenMatrix.MultiplyMatrices( camera.projectionMatrix, camera.matrixWorldInverse );
		frustum = Frustum.FromMatrix( projectionScreenMatrix );

		// render depth map
            renderer.SetRenderTarget(renderTarget );
		renderer.Clear();

		// set object matrices & frustum culling
		renderList.Clear();
		ProjectObject(scene,scene,camera);

		// render regular objects
            foreach(var glObject in renderList) 
            {
			var o = glObject.Object;
			var buffer = glObject.Buffer;

			// TODO: create proper depth material for particles
			if(o is PointCloud /*&& !o.customDepthMaterial*/) continue;

           // var useMorphing = o.geometry.MorphTargets != null && o.geometry.MorphTargets.Count > 0; /* TODO IAN, need this for skinning? && object.Material.morphTargets */
           //     var useSkinning = o is SkinnedMesh; /* TODO IAN, need this for skinning? && object.Material.skinning */

                Material material;
			if (o.customDepthMaterial != null) material = o.customDepthMaterial;
			//else if ( useSkinning ) material = useMorphing ? depthMaterialMorphSkin : depthMaterialSkin;
            //else if ( useMorphing ) material = depthMaterialMorph;
            else material = depthMaterial;
                
			renderer.RenderBuffer( camera, scene.lights, null, material, buffer, o );
		}

		// restore GL state
		var clearColor = renderer.ClearColor;
            GL.ClearColor(clearColor.R, clearColor.G, clearColor.B, 1 );
            GL.Enable( EnableCap.Blend);
        }

        private void ProjectObject(Scene scene, Object3D o, Camera camera)
        {
            if (o.IsVisible)
            {
                List<Scene.BufferInfo> glObjects;
                if (scene.glObjects.TryGetValue(o.Id, out glObjects))
                {
                    if (!o.frustumCulled || frustum.IntersectsObject(o))
                    {
                        foreach (var glObject in glObjects)
                        {
                            o.modelViewMatrix.MultiplyMatrices(camera.matrixWorldInverse, o.matrixWorld);
                            renderList.Add(glObject);
                        }
                    }
                }

                foreach (var c in o.Children) ProjectObject(scene, c, camera);
            }
        }

        private uint CreateProgram()
        {
            var program = GL.CreateProgram();

            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

            GL.ShaderSource(vertexShader, string.Join(Environment.NewLine, new string[]
        {
            "uniform mat4 modelViewMatrix;",
            "uniform mat4 projectionMatrix;",
            "uniform float rotation;",
            "uniform vec2 scale;",
            "uniform vec2 uvOffset;",
            "uniform vec2 uvScale;",
            "attribute vec2 position;",
            "attribute vec2 uv;",
            "varying vec2 vUV;",
            "void main() {",
                "vUV = uvOffset + uv * uvScale;",
                "vec2 alignedPosition = position * scale;",
                "vec2 rotatedPosition;",
                "rotatedPosition.x = cos( rotation ) * alignedPosition.x - sin( rotation ) * alignedPosition.y;",
                "rotatedPosition.y = sin( rotation ) * alignedPosition.x + cos( rotation ) * alignedPosition.y;",
                "vec4 finalPosition;",
                "finalPosition = modelViewMatrix * vec4( 0.0, 0.0, 0.0, 1.0 );",
                "finalPosition.xy += rotatedPosition;",
                "finalPosition = projectionMatrix * finalPosition;",
                "gl_Position = finalPosition;",
            "}"
        }));

            GL.ShaderSource(fragmentShader, string.Join(Environment.NewLine, new string[]
        {
            "uniform vec3 color;",
            "uniform sampler2D map;",
            "uniform float opacity;",
            "uniform int fogType;",
            "uniform vec3 fogColor;",
            "uniform float fogDensity;",
            "uniform float fogNear;",
            "uniform float fogFar;",
            "uniform float alphaTest;",
            "varying vec2 vUV;",
            "void main() {",
                "vec4 texture = texture2D( map, vUV );",
                "if ( texture.a < alphaTest ) discard;",
                "gl_FragColor = vec4( color * texture.xyz, texture.a * opacity );",
                "if ( fogType > 0 ) {",
                    "float depth = gl_FragCoord.z / gl_FragCoord.w;",
                    "float fogFactor = 0.0;",
                    "if ( fogType == 1 ) {",
                        "fogFactor = smoothstep( fogNear, fogFar, depth );",
                    "} else {",
                        "const float LOG2 = 1.442695;",
                        "float fogFactor = exp2( - fogDensity * fogDensity * depth * depth * LOG2 );",
                        "fogFactor = 1.0 - clamp( fogFactor, 0.0, 1.0 );",
                    "}",
                    "gl_FragColor = mix( gl_FragColor, vec4( fogColor, gl_FragColor.w ), fogFactor );",
                "}",
            "}"
        }));

            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            return program;
        }
    }
}
