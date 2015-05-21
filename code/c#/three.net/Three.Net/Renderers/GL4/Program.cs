using Pencil.Gaming.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Renderers.Shaders;
using Three.Net.Textures;

namespace Three.Net.Renderers.GL4
{
    public class AttributeInfo
    {
        public string Name = String.Empty;
        public int GPUAddress = -1;
        public int Size = 0;
        public ActiveAttribType Type;
        public bool NeedsUpdate = true;
        public bool IsEnabled = false;
    }

    public class UniformInfo
    {
        public String Name = String.Empty;
        public int GPUAddress = -1;
        public int Size = 0;
        public ActiveUniformType Type;
    }


    class Program
    {
        public readonly int Id;
        public readonly int UsedTimes;
        public readonly int GLProgramId;
        public readonly int GLVertexShaderId;
        public readonly int GLFragmentShaderId;

        private static int programIdCount = 0;
        private Dictionary<string, UniformInfo> Uniforms = new Dictionary<string, UniformInfo>();
        private Dictionary<string, AttributeInfo> Attributes = new Dictionary<string, AttributeInfo>();

        public Program(Renderer renderer, string vertexShaderSource, string fragmentShaderSource, Dictionary<string, dynamic> defines)
        {
            currentAddress = 0;
            //var shadowMapTypeDefine = "SHADOWMAP_TYPE_BASIC";
            //dynamic shadowParameter;
            //if (defines.TryGetValue("shadowMapType", out shadowParameter))
            //{
            //    if (shadowParameter == ShadowType.PCFShadowMap) shadowMapTypeDefine = "SHADOWMAP_TYPE_PCF";
            //    else if (shadowParameter == ShadowType.PCFSoftShadowMap) shadowMapTypeDefine = "SHADOWMAP_TYPE_PCF_SOFT";
            //}

            var programId = (int)GL.CreateProgram();
            var prefixVertex = string.Empty;
            var prefixFragment = string.Empty;

            var lines = new List<string>();
            lines.Add("#version 430");
            if (defines.ContainsKey("supportsVertexTextures")) lines.Add("#define VERTEX_TEXTURES");
            if (renderer.GammaInput) lines.Add("#define GAMMA_INPUT");
            if (renderer.GammaOutput) lines.Add("#define GAMMA_OUTPUT");
            lines.Add("#define MAX_DIR_LIGHTS " + defines["maxDirLights"]);
            lines.Add("#define MAX_POINT_LIGHTS " + defines["maxPointLights"]);
            lines.Add("#define MAX_SPOT_LIGHTS " + defines["maxSpotLights"]);
            lines.Add("#define MAX_HEMI_LIGHTS " + defines["maxHemiLights"]);
            //lines.Add("#define MAX_SHADOWS " + parameters["maxShadows"]);
            //lines.Add("#define MAX_BONES " + parameters["maxBones"]);
            if (defines.ContainsKey("map") && defines["map"]) lines.Add("#define USE_MAP");
            //if (parameters["envMap"]) lines.Add("#define USE_ENVMAP");
            //if (parameters["lightMap"]) lines.Add("#define USE_LIGHTMAP");
            //if (parameters["bumpMap"]) lines.Add("#define USE_BUMPMAP");
            if (defines["normalMap"]) lines.Add("#define USE_NORMALMAP");
            if (defines["specularMap"]) lines.Add("#define USE_SPECULARMAP");
            //if (parameters["alphaMap"]) lines.Add("#define USE_ALPHAMAP");
            lines.Add("#define USE_COLOR");
            //if (parameters["skinning"]) lines.Add("#define USE_SKINNING");
            //if (parameters["useVertexTexture"]) lines.Add("#define BONE_TEXTURE");
            //if (parameters["morphTargets"]) lines.Add("#define USE_MORPHTARGETS");
            //if (parameters["morphNormals"]) lines.Add("#define USE_MORPHNORMALS");
            if (defines["wrapAround"]) lines.Add("#define WRAP_AROUND");
            if (defines["doubleSided"]) lines.Add("#define DOUBLE_SIDED");
            if (defines["flipSided"]) lines.Add("#define FLIP_SIDED");
            //if (parameters["shadowMapEnabled"]) lines.Add("#define USE_SHADOWMAP");
            //if (parameters["shadowMapEnabled"]) lines.Add("#define " + shadowMapTypeDefine);
            //if (parameters["shadowMapDebug"]) lines.Add("#define SHADOWMAP_DEBUG");
            //if (parameters["shadowMapCascade"]) lines.Add("#define SHADOWMAP_CASCADE");
            //if (parameters["sizeAttenuation"]) lines.Add("#define USE_SIZEATTENUATION");
            //if (parameters["logarithmicDepthBuffer"]) lines.Add("#define USE_LOGDEPTHBUF");
            lines.Add("uniform mat4 modelMatrix;");
            lines.Add("uniform mat4 modelViewMatrix;");
            lines.Add("uniform mat4 projectionMatrix;");
            lines.Add("uniform mat4 viewMatrix;");
            lines.Add("uniform mat3 normalMatrix;");
            lines.Add("uniform vec3 cameraPosition;");
            lines.Add("in vec3 position;");
            lines.Add("in vec3 normal;");
            lines.Add("in vec2 uv;");
            //lines.Add("attribute vec2 uv2;");
            //lines.Add("#define USE_COLOR");
            if (defines["vertexColors"] != VertexColorMode.None)
            {
                lines.Add("in vec3 color;");
            }
            //lines.Add("#endif");
            //lines.Add("#ifdef USE_MORPHTARGETS");
            //lines.Add("	attribute vec3 morphTarget0;");
            //lines.Add("	attribute vec3 morphTarget1;");
            //lines.Add("	attribute vec3 morphTarget2;");
            //lines.Add("	attribute vec3 morphTarget3;");
            //lines.Add("	#ifdef USE_MORPHNORMALS");
            //lines.Add("		attribute vec3 morphNormal0;");
            //lines.Add("		attribute vec3 morphNormal1;");
            //lines.Add("		attribute vec3 morphNormal2;");
            //lines.Add("		attribute vec3 morphNormal3;");
            //lines.Add("	#else");
            //lines.Add("		attribute vec3 morphTarget4;");
            //lines.Add("		attribute vec3 morphTarget5;");
            //lines.Add("		attribute vec3 morphTarget6;");
            //lines.Add("		attribute vec3 morphTarget7;");
            //lines.Add("	#endif");
            //lines.Add("#endif");
            //lines.Add("#ifdef USE_SKINNING");
            //lines.Add("	attribute vec4 skinIndex;");
            //lines.Add("	attribute vec4 skinWeight;");
            //lines.Add("#endif");
            lines.Add(string.Empty);

            prefixVertex = string.Join(Environment.NewLine, lines);

            lines.Clear();
            lines.Add("#define MAX_DIR_LIGHTS " + defines["maxDirLights"]);
            lines.Add("#define MAX_POINT_LIGHTS " + defines["maxPointLights"]);
            lines.Add("#define MAX_SPOT_LIGHTS " + defines["maxSpotLights"]);
            lines.Add("#define MAX_HEMI_LIGHTS " + defines["maxHemiLights"]);
            //lines.Add("#define MAX_SHADOWS " + parameters["maxShadows"]);
            if (defines.ContainsKey("alphaTest")) lines.Add(string.Format("#define ALPHATEST {0:F4}", defines["alphaTest"]));
            if (renderer.GammaInput) lines.Add("#define GAMMA_INPUT");
            if (renderer.GammaOutput) lines.Add("#define GAMMA_OUTPUT");
            if (defines["useFog"] && defines["fog"]) lines.Add("#define USE_FOG");
            if (defines["useFog"] && defines["fogExp"]) lines.Add("#define FOG_EXP2");
            if (defines.ContainsKey("map")) lines.Add("#define USE_MAP");
            //if (parameters["envMap"]) lines.Add("#define USE_ENVMAP");
            //if (parameters["lightMap"]) lines.Add("#define USE_LIGHTMAP");
            //if (parameters["bumpMap"]) lines.Add("#define USE_BUMPMAP");
            if (defines["normalMap"]) lines.Add("#define USE_NORMALMAP");
            //if (parameters["specularMap"]) lines.Add("#define USE_SPECULARMAP");
            //if (parameters["alphaMap"]) lines.Add("#define USE_ALPHAMAP");
            //if (parameters["vertexColors"]) lines.Add("#define USE_COLOR");
            //if (parameters["metal"]) lines.Add("#define METAL");
            if (defines["wrapAround"]) lines.Add("#define WRAP_AROUND");
            if (defines["doubleSided"]) lines.Add("#define DOUBLE_SIDED");
            if (defines["flipSided"]) lines.Add("#define FLIP_SIDED");
            //if (parameters["shadowMapEnabled"]) lines.Add("#define USE_SHADOWMAP");
            //if (parameters["shadowMapEnabled"]) lines.Add("#define " + shadowMapTypeDefine);
            //if (parameters["shadowMapDebug"]) lines.Add("#define SHADOWMAP_DEBUG");
            //if (parameters["shadowMapCascade"]) lines.Add("#define SHADOWMAP_CASCADE");
            //if (parameters["logarithmicDepthBuffer"]) lines.Add("#define USE_LOGDEPTHBUF");
            lines.Add("uniform mat4 viewMatrix;");
            lines.Add("uniform vec3 cameraPosition;");
            prefixFragment = string.Join(Environment.NewLine, lines);

            var glVertexShaderId = CreateGL4Shader(ShaderType.VertexShader, prefixVertex + vertexShaderSource);
            var glFragmentShaderId = CreateGL4Shader(ShaderType.FragmentShader, prefixFragment + fragmentShaderSource);

            GL.AttachShader(programId, glVertexShaderId);
            GL.AttachShader(programId, glFragmentShaderId);

            GL.LinkProgram(programId);

            var log = string.Empty;
            GL.GetProgramInfoLog(programId, out log);
            if (log != string.Empty) throw new InvalidDataException(log);


            //// clean up
            GL.DeleteShader(glVertexShaderId);
            GL.DeleteShader(glFragmentShaderId);

            Id = programIdCount++;
            UsedTimes = 1;
            GLProgramId = programId;
            GLVertexShaderId = glVertexShaderId;
            GLFragmentShaderId = glFragmentShaderId;

            CacheUniformLocations(programId);
            CacheAttributeLocations(programId);
        }

        private void CacheUniformLocations(int programId)
        {
            int uniformCount;
            GL.GetProgram(GLProgramId, ProgramParameter.ActiveUniforms, out uniformCount);
            for (int i = 0; i < uniformCount; i++)
            {
                var info = new UniformInfo();
                var length = 0;
                var sb = new StringBuilder();

                ActiveUniformType glUniformType;
                GL.GetActiveUniform(GLProgramId, i, 256, out length, out info.Size, out glUniformType, sb);

                info.Name = sb.ToString();
                info.Type = glUniformType;
                info.GPUAddress = GL.GetUniformLocation(GLProgramId, info.Name);

                Uniforms.Add(info.Name.Replace("[0]",string.Empty), info);
            }
        }

        private void CacheAttributeLocations(int programId)
        {
            int attributeCount;
            GL.GetProgram(GLProgramId, ProgramParameter.ActiveAttributes, out attributeCount);
            for (int i = 0; i < attributeCount; i++)
            {
                var info = new AttributeInfo();
                var length = 0;
                var name = new StringBuilder();

                ActiveAttribType glAttribType;
                GL.GetActiveAttrib(GLProgramId, i, 256, out length, out info.Size, out glAttribType, name);

                info.Name = name.ToString();
                info.GPUAddress = GL.GetAttribLocation(GLProgramId, info.Name);
                info.Type = glAttribType;

                Attributes.Add(name.ToString(), info);
            }
        }

        public static int CreateGL4Shader(ShaderType type, string source)
        {
            var shaderId = GL.CreateShader(type);
            GL.ShaderSource(shaderId, source);
            GL.CompileShader(shaderId);

            var shaderLog = string.Empty;
            GL.GetShaderInfoLog((int)shaderId, out shaderLog);
            var hadError = shaderLog != string.Empty;
            var errorPath = string.Format("GeneratedShaderErrors_{0}_{1}.txt", shaderId, type);
            var generatedPath = string.Format("GeneratedShader_{0}_{1}.glsl", shaderId, type);

            if (hadError)
            {
                if (File.Exists(generatedPath)) File.Delete(generatedPath);

                var msg = "{0} shader compile error: {1} line#:{2}";
                var error = string.Format(msg, type, shaderLog, AddLineNumbers(source));
                File.WriteAllText(errorPath, error);
                Debug.Assert(hadError, error);
            }
            else
            {
                if (File.Exists(errorPath)) File.Delete(errorPath);
                File.WriteAllText(generatedPath, source);
            }
            return (int)shaderId;
        }

        private static string AddLineNumbers(string source)
        {
            var lines = source.Split('\n');
            for (var i = 0; i < lines.Length; i++) lines[i] = string.Format("{0}: {1}", i + 1, lines[i]);
            return string.Join(Environment.NewLine, lines);
        }

        internal void SetAttributeData(string attributeName, int stride = 0, int offset = 0)
        {
            AttributeInfo info;
            if (!Attributes.TryGetValue(attributeName, out info))
            {
                //Debug.WriteLine("'{0}' is not an active shader parameter, not setting with string:{1} offset:{2}.", attributeName, stride, offset);
            }
            else
            {
                if (info.NeedsUpdate)
                {
                    var type = VertexAttribPointerType.Int;
                    int size = 1;
                    switch (info.Type)
                    {
                        case ActiveAttribType.Int:
                            type = VertexAttribPointerType.Int;
                            break;
                        case ActiveAttribType.Float:
                            type = VertexAttribPointerType.Float;
                            break;
                        case ActiveAttribType.FloatVec2:
                            type = VertexAttribPointerType.Float;
                            size = 2;
                            break;
                        case ActiveAttribType.FloatVec3:
                            type = VertexAttribPointerType.Float;
                            size = 3;
                            break;
                        default: throw new NotSupportedException();
                    }

                    EnableAttribute(attributeName);
                    GL.VertexAttribPointer(info.GPUAddress, size, type, false, stride * sizeof(float), offset * sizeof(float));
                }
            };
        }

        internal bool IsAttributeActive(string attributeName)
        {
            AttributeInfo info;
            if (Attributes.TryGetValue(attributeName, out info))    return info.IsEnabled;
            return false;
        }

        internal void SetUniformData(string uniformName, float value)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.Float);
                GL.Uniform1(info.GPUAddress, value);
            }
        }

        internal void SetUniformData(string uniformName, List<float> floats)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.Float);
                GL.Uniform1(info.GPUAddress, floats.Count, floats.ToArray());
            }
        }

        internal void SetUniformData(string uniformName, float x1, float y1, float x2, float y2)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatVec4);
                GL.Uniform4(info.GPUAddress, x1, y1, x2, y2);
            }
        }

        internal void SetUniformData(string uniformName, Vector3 data)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatVec3);
                GL.Uniform3(info.GPUAddress, data.x, data.y, data.z);
            }
        }

        internal void SetUniformData(string uniformName, Vector2 data)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatVec2);
                GL.Uniform2(info.GPUAddress, data.x, data.y);
            }
        }

        internal void SetUniformData(string uniformName, List<Vector3> vectors)
        {
            if (vectors.Count > 0)
            {
                UniformInfo info;
                if (Uniforms.TryGetValue(uniformName, out info))
                {
                    Debug.Assert(info.Type == ActiveUniformType.FloatVec3);
                    
                    var offset = 0;
                    var floats = new float[vectors.Count * 3];
                    foreach (var v in vectors)
                    {
                        floats[offset++] = v.x;
                        floats[offset++] = v.y;
                        floats[offset++] = v.z;
                    }
                    GL.Uniform3(info.GPUAddress, vectors.Count, floats);
                }
            }
        }

        internal void SetUniformData(string uniformName, Color color)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatVec3);
                GL.Uniform3(info.GPUAddress, color.R, color.G, color.B);
            }
        }

        internal void SetUniformData(string uniformName, List<Color> colors)
        {
            if (colors.Count > 0)
            {
                UniformInfo info;
                if (Uniforms.TryGetValue(uniformName, out info))
                {
                    Debug.Assert(info.Type == ActiveUniformType.FloatVec3);
                    var offset = 0;
                    var floats = new float[colors.Count * 3];
                    foreach (var color in colors)
                    {
                        floats[offset++] = color.R;
                        floats[offset++] = color.G;
                        floats[offset++] = color.B;
                    }
                    GL.Uniform3(info.GPUAddress, colors.Count, floats);
                }
            }
        }

        internal void SetUniformData(string uniformName, ref Matrix3 data, bool transpose = false)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatMat3);
                GL.UniformMatrix3(info.GPUAddress, 1, transpose, data.elements);
            }
        }

        internal void SetUniformData(string uniformName, ref Matrix4 data, bool transpose = false)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.FloatMat4);
                GL.UniformMatrix4(info.GPUAddress, 1, transpose, data.elements);
            }
        }

        static int currentAddress;

        internal void SetUniformData(string uniformName, Texture texture)
        {
            UniformInfo info;
            if (Uniforms.TryGetValue(uniformName, out info))
            {
                Debug.Assert(info.Type == ActiveUniformType.Sampler2D);
                var t = (TextureUnit)(currentAddress + 33984);
                //GL.DeleteTexture(currentAddress + 33984);
                GL.ActiveTexture(t);
                GL.BindTexture(texture.TextureTarget, texture.GPUAddress);
                GL.Uniform1(info.GPUAddress, currentAddress);

                if (currentAddress >= 31)
                {
                    currentAddress = 0;
                }
                else
                {
                    currentAddress++;
                }
            }
        }

        internal void EnableAttribute(string attributeName)
        {
            AttributeInfo info;
            if (Attributes.TryGetValue(attributeName, out info))
            {
                if (!info.IsEnabled)
                {
                    GL.EnableVertexAttribArray(info.GPUAddress);
                    info.IsEnabled = true;
                }
            }
        }

        internal void DisableAttribute(string attributeName)
        {
            AttributeInfo info;
            if (Attributes.TryGetValue(attributeName, out info))
            {
                GL.DisableVertexAttribArray(info.GPUAddress);
                info.IsEnabled = false;
            }
        }

        internal void DisableUnusedAttributes()
        {
            foreach (var info in Attributes.Values)
            {
                if (!info.IsEnabled)
                {
                    GL.DisableVertexAttribArray(info.GPUAddress);
                }
            }
        }

        internal void DisableAllAttributes()
        {
            foreach (var info in Attributes.Values)
            {
                GL.DisableVertexAttribArray(info.GPUAddress);
            }
        }
    }
}
