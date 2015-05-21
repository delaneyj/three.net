using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Cameras;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Renderers.Plugins;
using Three.Net.Scenes;
using Three.Net.Textures;

namespace Three.Net.Extras.Renderers.Plugins
{
    public class SpritePlugin : RenderPlugin
    { 
        private Texture texture;
        private int program;
        private List<Sprite> sprites = new List<Sprite>();
        private Dictionary<string, int> attributes;
        private Dictionary<string, int> uniforms;
        private int vertexBuffer, elementBuffer;

        protected internal override void Init(Renderer renderer)
        {
            base.Init(renderer);

            var vertices = new float[]
            {
                - 0.5f, - 0.5f, 0, 0, 
			      0.5f, - 0.5f, 1, 0,
			      0.5f,   0.5f, 1, 1,
			    - 0.5f,   0.5f, 0, 1
            };

            var faces = new ushort[]
                {
                    0, 1, 2,
			        0, 2, 3
                };

            vertexBuffer = GL.GenBuffer();
            elementBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(vertices.Length * sizeof(float)), vertices, BufferUsageHint.StaticDraw);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(faces.Length * sizeof(ushort)), faces, BufferUsageHint.StaticDraw);

            program = CreateProgram();

            attributes = new Dictionary<string, int>()
        {
			{"position",GL.GetAttribLocation(program, "position")},
			{"uv",      GL.GetAttribLocation(program, "uv")}
		};

            uniforms = new Dictionary<string, int>()
        {
			{"uvOffset", GL.GetUniformLocation( program, "uvOffset")},
			{"uvScale", GL.GetUniformLocation( program, "uvScale")},
			{"rotation", GL.GetUniformLocation( program, "rotation")},
			{"scale", GL.GetUniformLocation( program, "scale")},
			{"color", GL.GetUniformLocation( program, "'color")},
			{"map", GL.GetUniformLocation( program, "map")},
			{"opacity", GL.GetUniformLocation( program, "opacity")},
			{"modelViewMatrix", GL.GetUniformLocation( program, "modelViewMatrix")},
			{"projectionMatrix", GL.GetUniformLocation( program, "projectionMatrix")},
			{"fogType", GL.GetUniformLocation( program, "fogType")},
			{"fogDensity", GL.GetUniformLocation( program, "fogDensity")},
			{"fogNear", GL.GetUniformLocation( program, "fogNear")},
			{"fogFar", GL.GetUniformLocation( program, "fogFar")},
			{"fogColor", GL.GetUniformLocation( program, "fogColor")},
			{"alphaTest", GL.GetUniformLocation( program, "alphaTest")},
		};

            texture = new Texture(8, 8, Color.White);
            texture.NeedsUpdate = true;
        }

        private int CreateProgram()
        {
            var program = GL.CreateProgram();
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(vertexShader, string.Join(Environment.NewLine,
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
                "}"));

            GL.ShaderSource(fragmentShader, string.Join(Environment.NewLine,
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
            "}"));

            GL.CompileShader(vertexShader);
            GL.CompileShader(fragmentShader);

            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);

            GL.LinkProgram(program);

            return (int)program;
        }

        protected internal override void Render(Scene scene, Camera camera, int viewportWidth, int viewportHeight)
        {
            sprites.Clear();

            scene.TraverseVisible(child =>
            {
                if (child is Sprite)
                {
                    sprites.Add(child as Sprite);
                }
            });

            if (sprites.Count == 0) return;

            // setup gl
            GL.UseProgram(program);

            GL.EnableVertexAttribArray(attributes["position"]);
            GL.EnableVertexAttribArray(attributes["uv"]);

            GL.Disable(EnableCap.CullFace);
            GL.Enable(EnableCap.Blend);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBuffer);
            GL.VertexAttribPointer(attributes["position"], 2, VertexAttribPointerType.Float, false, 2 * 8, 0);
            GL.VertexAttribPointer(attributes["uv"], 2, VertexAttribPointerType.Float, false, 2 * 8, 8);

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBuffer);

            GL.UniformMatrix4(uniforms["projectionMatrix"], 1, false, camera.projectionMatrix.elements);

            GL.ActiveTexture(TextureUnit.Texture0);
            GL.Uniform1(uniforms["map"], 0);

            var oldFogType = 0;
            var sceneFogType = 0;
            var fog = scene.Fog;

            if (fog != null)
            {
                GL.Uniform3(uniforms["fogColor"], fog.Color.R, fog.Color.G, fog.Color.B);
                var linear = fog as FogLinear;

                if (linear != null)
                {
                    GL.Uniform1(uniforms["fogNear"], linear.Near);
                    GL.Uniform1(uniforms["fogFar"], linear.Far);

                    GL.Uniform1(uniforms["fogType"], 1);
                    oldFogType = 1;
                    sceneFogType = 1;
                }
                else
                {
                    var exponential = fog as FogExp2;
                    if (exponential != null)
                    {
                        GL.Uniform1(uniforms["fogDensity"], exponential.Density);
                        GL.Uniform1(uniforms["fogType"], 2);
                        oldFogType = 2;
                        sceneFogType = 2;
                    }
                    else
                    {
                        GL.Uniform1(uniforms["fogType"], 0);
                        oldFogType = 0;
                        sceneFogType = 0;
                    }
                }


                // update positions and sort
                foreach (var sprite in sprites)
                {
                    var material = sprite.Material;
                    sprite.modelViewMatrix.MultiplyMatrices(camera.matrixWorldInverse, sprite.matrixWorld);
                    sprite.Zdepth = -sprite.modelViewMatrix.elements[14];
                }
                sprites.Sort(painterSortStable);

                // render all sprites
                foreach (var sprite in sprites)
                {
                    var material = sprite.Material as SpriteMaterial;

                    GL.Uniform1(uniforms["alphaTest"], material.ShouldAlphaTest);
                    GL.UniformMatrix4(uniforms["modelViewMatrix"], 1, false, sprite.modelViewMatrix.elements);

                    var fogType = 0;

                    if (scene.Fog != null && material.UseFog)
                    {
                        fogType = sceneFogType;
                    }

                    if (oldFogType != fogType)
                    {
                        GL.Uniform1(uniforms["fogType"], fogType);
                        oldFogType = fogType;
                    }

                    if (material.DiffuseMap != null)
                    {
                        GL.Uniform2(uniforms["uvOffset"], material.DiffuseMap.Offset.x, material.DiffuseMap.Offset.y);
                        GL.Uniform2(uniforms["uvScale"], material.DiffuseMap.Repeat.x, material.DiffuseMap.Repeat.y);
                    }
                    else
                    {
                        GL.Uniform2(uniforms["uvOffset"], 0, 0);
                        GL.Uniform2(uniforms["uvScale"], 1, 1);
                    }

                    GL.Uniform1(uniforms["opacity"], material.Opacity);
                    GL.Uniform3(uniforms["color"], material.Diffuse.R, material.Diffuse.G, material.Diffuse.B);

                    GL.Uniform1(uniforms["rotation"], material.Rotation);
                    GL.Uniform2(uniforms["scale"], sprite.Scale.x, sprite.Scale.y);

                    renderer.SetBlending(material.Blending, material.BlendEquation, material.BlendSource, material.BlendDestination);
                    renderer.DepthTest = material.ShouldDepthTest;
                    renderer.DepthWrite = material.ShouldDepthWrite;

                    if (material.DiffuseMap != null && material.DiffuseMap.Resolution.Width > 0) renderer.SetTexture(material.DiffuseMap, 0);
                    else renderer.SetTexture(texture, 0);

                    GL.DrawElements(BeginMode.Triangles, 6, DrawElementsType.UnsignedShort, 0);

                }

                // restore gl
                GL.Enable(EnableCap.CullFace);
            }
        }

        private int painterSortStable(Sprite a, Sprite b)
        {
            if (a.Zdepth.HasValue && b.Zdepth.HasValue && a.Zdepth != b.Zdepth) return Mathf.Round(b.Zdepth.Value - a.Zdepth.Value);
            else return Convert.ToInt32(b.Id - a.Id);
        }
    }
}
