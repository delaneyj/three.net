using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Renderers;
using Three.Net.Renderers.GL4;
using Three.Net.Textures;

namespace Three.Net.Loaders
{
    public class LoadedInfo
    {
        public Geometry Geometry;
        public readonly List<MeshBasicMaterial> Materials = new List<MeshBasicMaterial>();
    }


    public abstract class Loader
    {
        protected readonly LoadingManager manager;

        public Loader(LoadingManager manager = null)
        {
            this.manager = manager ?? LoadingManager.DefaultLoadingManager;
        }

        public abstract LoadedInfo Parse(Renderer renderer, JObject json, string texturePath);

        protected MeshBasicMaterial[] InitMaterials(Renderer renderer, JToken[] materialsJson, string texturePath)
        {
            var materials = new List<MeshBasicMaterial>();
            foreach (var materialJson in materialsJson)
            {
                var m = CreateMaterial(renderer, materialJson, texturePath);
                materials.Add(m);
            }
            return materials.ToArray();
        }

        private MeshBasicMaterial CreateMaterial(Renderer renderer, JToken materialJson, string texturePath = null)
        {
            dynamic material = new MeshLambertMaterial(renderer);

            var shadingData = materialJson["shading"];
            if (shadingData != null)
            {
                var shading = shadingData.Value<string>().ToLower();
                if (shading == "phong") throw new NotImplementedException();
                else if (shading == "basic") material = new MeshBasicMaterial(renderer);
            }

            material.DiffuseColor = new Color(0xeeeeee);
            //material.Opacity = 1;
            material.UseWireframe = false;

            var blendingData = materialJson["blending"];
            if (blendingData != null)
            {
                BlendMode mode;
                switch(blendingData.Value<string>())
                {
                    case "NormalBlending": mode = BlendMode.Normal; break;
                    default: throw new NotSupportedException();
                }
                material.Blending = mode;
            }

            var transparentData = materialJson["transparent"];
            if (transparentData != null) material.IsTransparent = transparentData.Value<bool>();

            var depthTestData = materialJson["depthTest"];
            if (depthTestData != null) material.ShouldDepthTest = depthTestData.Value<bool>();

            var depthWriteData = materialJson["depthWrite"];
            if (depthWriteData != null) material.ShouldDepthWrite = depthWriteData.Value<bool>();

            var isVisibleData = materialJson["visible"];
            if (isVisibleData != null) material.isVisible = isVisibleData.Value<bool>();

            var isFlipSidedData = materialJson["flipSided"];
            if (isFlipSidedData != null) material.side = SideMode.Back;

            var isDoubleSidedData = materialJson["doubleSided"];
            if (isFlipSidedData != null) material.side = SideMode.Double;

            var useWireframeData = materialJson["wireframe"];
            if (useWireframeData != null) material.useWireframe = useWireframeData.Value<bool>();

            var vertexColorsData = materialJson["vertexColors"];
            if (vertexColorsData != null)
            {
                if (vertexColorsData.Value<string>() == "face") material.VertexColors = VertexColorMode.Face;
                else material.VertexColors = VertexColorMode.Vertex;
            }

            #region colors

            var colorDiffuseData = materialJson["colorDiffuse"];
            if (colorDiffuseData != null)
            {
                var rgb = colorDiffuseData.Values<float>().ToArray();
                material.DiffuseColor = new Color(rgb[0], rgb[1], rgb[2]);
            }
            else
            {
                var debugColorData = materialJson["DbgColor"];
                if (debugColorData != null)
                {
                    material.DiffuseColor = new Color(debugColorData.Value<uint>());
                }
            }

            var colorSpecularData = materialJson["colorSpecular"];
            if (material is MeshPhongMaterial && colorSpecularData != null)
            {
                var rgb = colorSpecularData.Values<float>().ToArray();
                material.Specular = new Color(rgb[0], rgb[1], rgb[2]);
            }

            var ambientData = materialJson["colorAmbient"];
            if (ambientData != null)
            {
                var rgb = ambientData.Values<float>().ToArray();
                material.Ambient = new Color(rgb[0], rgb[1], rgb[2]);
            }

            var emissiveData = materialJson["colorEmissive"];
            if (emissiveData != null)
            {
                var rgb = emissiveData.Values<float>().ToArray();
                material.Emissive = new Color(rgb[0], rgb[1], rgb[2]);
            }
            #endregion

            #region modifiers
            // TODO is opactiy needed?
            //if (transparentData != null)
            //{
            //    material.opacity = transparentData.Value<float>();
            //}

            var shininessData = materialJson["shininess"];
            if (shininessData != null) material.shininess = shininessData.Value<float>();
            #endregion

            #region textures
            var mapDiffuseData = materialJson["mapDiffuse"];
            if(mapDiffuseData != null && texturePath != null) 
            {
                var d = materialJson["mapDiffuse"].Value<string>();
                var r =  materialJson["mapDiffuseRepeat"];
                var o = materialJson["mapDiffuseOffset"];
                var wrapData = materialJson["mapDiffuseWrap"];
                WrapMode wrapS;
                switch(wrapData[0].Value<string>())
                {
                    case "repeat": wrapS = WrapMode.Repeat;break;
                    default: throw new NotSupportedException();
                }
                WrapMode wrapT;
                switch (wrapData[1].Value<string>())
                {
                    case "repeat": wrapT = WrapMode.Repeat; break;
                    default: throw new NotSupportedException();
                }
                
                var  a = materialJson["mapDiffuseAnisotropy"];
                CreateTexture(texturePath, materialJson, "map", d, null, null, wrapS, wrapT, null);
            }

            var mapLightData = materialJson["mapLight"];
            if(mapLightData != null && texturePath != null) 
            {
                var d = materialJson["mapLight"];
                var r =  materialJson["mapLightRepeat"];
                var o = materialJson["mapLightOffset"];
                var w = materialJson["mapLightWrap"];
                var a = materialJson["mapLightAnisotropy"];
                throw new NotImplementedException();//CreateTexture( mpars, "lightMap", d,r, o, w, a);
            }


            var mapBumpData = materialJson["mapBump"];
            if(mapBumpData != null && texturePath != null) 
            {
                var d = materialJson["mapBump"];
                var r =  materialJson["mapBumpRepeat"];
                var o = materialJson["mapBumpOffset"];
                var w = materialJson["mapBumpWrap"];
                var a = materialJson["mapBumpAnisotropy"];
                throw new NotImplementedException();//CreateTexture( mpars, "bumpMap", d,r, o, w, a);
            }

            var mapNormalData = materialJson["mapNormal"];
            if(mapNormalData != null && texturePath != null) 
            {
                var d = materialJson["mapNormal"];
                var r =  materialJson["mapNormalRepeat"];
                var o = materialJson["mapNormalOffset"];
                var w = materialJson["mapNormalWrap"];
                var a = materialJson["mapNormalAnisotropy"];
                throw new NotImplementedException();//CreateTexture( mpars, "normalMap", d,r, o, w, a);
            }

            var mapSpecularData = materialJson["mapSpecular"];
            if(mapSpecularData != null && texturePath != null) 
            {
                var d = materialJson["mapSpecular"];
                var r =  materialJson["mapSpecularRepeat"];
                var o = materialJson["mapSpecularOffset"];
                var w = materialJson["mapSpecularWrap"];
                var a = materialJson["mapSpecularAnisotropy"];
                throw new NotImplementedException();//CreateTexture( mpars, "specularMap", d,r, o, w, a);
            }

            var mapAlphaData = materialJson["mapAlpha"];
            if(mapAlphaData != null && texturePath != null) 
            {
                var d = materialJson["mapAlpha"];
                var r =  materialJson["mapAlphaRepeat"];
                var o = materialJson["mapAlphaOffset"];
                var w = materialJson["mapAlphaWrap"];
                var a = materialJson["mapAlphaAnisotropy"];
                throw new NotImplementedException();//CreateTexture( mpars, "alphaMap", d,r, o, w, a);
            }
#endregion

            var mapBumpScaleData =  materialJson["mapBumpScale"];
            if(mapBumpScaleData != null) material.bumpScale = mapBumpScaleData.Value<float>();

            if(mapNormalData != null)
            {
                throw new NotImplementedException();
            }

            return material;
        }

        private void CreateTexture(string texturePath, dynamic mpars, string name, string sourceFile, Vector2? repeat, Vector2? offset, WrapMode? wrapS, WrapMode? wrapT, float? anisotropy)
        {
            var fullPath = Path.GetFullPath(Path.Combine(texturePath, sourceFile));
            //var loader = Loader.Handlers.get( fullPath );

            var texture = new Texture(fullPath);

            if (repeat.HasValue)
            {
                var r = repeat.Value;
                texture.Repeat = r;
                if (r.x != 1) texture.WrapS = WrapMode.Repeat;
                if (r.y != 1) texture.WrapT = WrapMode.Repeat;
            }

            if (offset.HasValue) texture.Offset = offset.Value;

            if (wrapS.HasValue) texture.WrapS = wrapS.Value;
            if (wrapT.HasValue) texture.WrapT = wrapT.Value;

            if (anisotropy.HasValue) texture.Anisotropy = anisotropy.Value;

            mpars["name"] = texture.Name;
        }

        protected bool NeedsTangents(IEnumerable<Material> materials)
        {
            foreach (var m in materials)
            {
                if (m is CustomShaderMaterial) return true;
            }

            return false;
        }

        private Material CreateMaterial(JObject materialData, string texturePath)
        {
            throw new NotImplementedException();
        }
    }
}
