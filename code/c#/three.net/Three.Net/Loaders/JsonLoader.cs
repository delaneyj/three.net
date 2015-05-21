using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Loaders
{
    public class JsonLoader : Loader
    {
        public override LoadedInfo Parse(Renderer renderer, JObject json, String texturePath)
        {
            var geometry = new Geometry();

            var scaleData = json["scale"];
            var scale = scaleData != null ? 1 / scaleData.Value<float>() : 1;

            ParseModel(json, geometry, scale);
            ParseSkin(json, geometry);
            ParseMorphing(json, geometry, scale);

            geometry.ComputeNormals();
            geometry.ComputeBoundingSphere();

            var info = new LoadedInfo()
            {
                Geometry = geometry
            };

            var materialsJson = json["materials"];
            if (materialsJson != null)
            {
                var materialsData = materialsJson.ToArray();

                if (materialsData.Length != 0)
                {
                    var materials = InitMaterials(renderer, materialsData, texturePath);

                    if (NeedsTangents(materials)) geometry.ComputeTangents();

                    info.Materials.AddRange(materials);
                }
            }

            return info;
        }

        private void ParseMorphing(JObject json, Geometry geometry, float scale)
        {
            var morphTargetsData = json["morphTargets"];
            
            if (morphTargetsData != null)
            {
                var morphtargets = morphTargetsData.ToArray();
                if (morphtargets.Length > 0)
                {
                    for (var i = 0; i < morphtargets.Length; i++)
                    {
                        var morphTargetData = morphtargets[i];


                        var morphTarget = new MorphTargetInfo()
                            {
                                Name = morphTargetData[i]["name"].Value<string>()
                            };
                        geometry.MorphTargets.Add(morphTarget);

                        var dstVertices = morphTarget.Vertices;
                        var srcVertices = morphTargetData["vertices"].Values<float>().ToArray();

                        for (var v = 0; v < srcVertices.Length; v += 3)
                        {
                            var vertex = new Vector3();
                            vertex.x = srcVertices[v] * scale;
                            vertex.y = srcVertices[v + 1] * scale;
                            vertex.z = srcVertices[v + 2] * scale;
                            dstVertices.Add(vertex);
                        }
                    }
                }
            }

            var morphColorsData = json["morphColors"];
            if (morphColorsData != null)
            {
                geometry.MorphColors = new List<MorphColorInfo>();
                var morphColors = morphColorsData.ToArray();
                for (var i = 0; i < morphColors.Length; i++)
                {
                    var colorData = morphColors[i];
                    var morphColor = new MorphColorInfo()
                    {
                        Name = colorData["name"].Value<string>()
                    };
                    geometry.MorphColors.Add(morphColor);

                    var dstColors = morphColor.Colors;
                    var srcColors = colorData["colors"].Values<float>().ToArray();

                    for (var c = 0; c < srcColors.Length; c += 3)
                    {
                        var r = srcColors[c];
                        var g = srcColors[c + 1];
                        var b = srcColors[c + 2];
                        var color = new Color(r, g, b);
                        dstColors.Add(color);
                    }
                }
            }
        }

        private void ParseSkin(JObject json, Geometry geometry)
        {
            var influencesPerVertexData = json["influencesPerVertex"];
            var influencesPerVertex = influencesPerVertexData != null ? influencesPerVertexData.Value<int>() : 2;

            var skinWeightsData = json["skinWeights"];
            if (skinWeightsData != null)
            {
                var skinWeights = skinWeightsData.Values<float>().ToArray();
                if (skinWeights.Length > 0)
                {
                    geometry.SkinWeights = new List<Vector4>();
                    for (var i = 0; i < skinWeights.Length; i += influencesPerVertex)
                    {
                        var x = skinWeights[i];
                        var y = (influencesPerVertex > 1) ? skinWeights[i + 1] : 0;
                        var z = (influencesPerVertex > 2) ? skinWeights[i + 2] : 0;
                        var w = (influencesPerVertex > 3) ? skinWeights[i + 3] : 0;
                        geometry.SkinWeights.Add(new Vector4(x, y, z, w));
                    }
                }
            }

            var skinIndicesData = json["skinIndices"];
            if (skinIndicesData != null)
            {
                var skinIndices = skinIndicesData.Values<int>().ToArray();
                if (skinIndices.Length > 0)
                {
                    geometry.SkinIndices = new List<Vector4>();
                    for (var i = 0; i < skinIndices.Length; i += influencesPerVertex)
                    {
                        var a = skinIndices[i];
                        var b = (influencesPerVertex > 1) ? skinIndices[i + 1] : 0;
                        var c = (influencesPerVertex > 2) ? skinIndices[i + 2] : 0;
                        var d = (influencesPerVertex > 3) ? skinIndices[i + 3] : 0;
                        geometry.SkinIndices.Add(new Vector4(a, b, c, d));
                    }
                }
            }

            var bonesData = json["bones"];
            if (bonesData != null)
            {
                var bones = bonesData.ToArray();

                if (bones.Length > 0)
                {
                    if ((geometry.SkinWeights.Count != geometry.SkinIndices.Count || geometry.SkinIndices.Count != geometry.vertices.Count))
                    {
                        var format = "When skinning, number of vertices ({0}), skinIndices ({1}), and skinWeights ({2}) should match.";
                        var msg = string.Format(format, geometry.vertices.Count, geometry.SkinIndices.Count, geometry.SkinWeights.Count);
                        throw new Exception(msg);
                    }

                    // could change this to json.animations[0] or remove completely
                    var animationData = json["animation"];
                    throw new NotImplementedException();

                    //var animationsData = json["animations"];
                    //if (animationsData != null) throw new NotImplementedException();
                }
            }
        }

        private void ParseModel(JObject json, Geometry geometry,  float scale)
        {
            var verticesData = json["vertices"].Values<float>().ToArray();
            var facesData = json["faces"].Values<int>().ToArray();
            var normalsData = json["normals"].Values<float>().ToArray();
            var colorsData = json["colors"].Values<uint>().ToArray();
            
            var uvsData = new List<List<float>>();
            foreach (var array in json["uvs"])
            {
                var uvData = array.Values<float>().ToArray();
                uvsData.Add(new List<float>(uvData));
            }

            var nUvLayers = 0;
            
            if(uvsData.Count > 0)
            {
                geometry.faceVertexUvs.Clear();
                // disregard empty arrays
                foreach (var row in uvsData)
                {
                    nUvLayers++;
                    geometry.faceVertexUvs.Add(new List<UVFaceSet>());
                }
            }

           
            var offset = 0;
            while (offset < verticesData.Length)
            {
                var x = verticesData[offset++] * scale;
                var y = verticesData[offset++] * scale;
                var z = verticesData[offset++] * scale;
                var v = new Vector3(x, y, z);
                geometry.vertices.Add(v);
            }

            offset = 0;
            while (offset < facesData.Length)
            {
                var type = facesData[offset++];
                var isQuad = IsBitSet(type, 0);
                var hasMaterial = IsBitSet(type, 1);
                var hasFaceVertexUv = IsBitSet(type, 3);
                var hasFaceNormal = IsBitSet(type, 4);
                var hasFaceVertexNormal = IsBitSet(type, 5);
                var hasFaceColor = IsBitSet(type, 6);
                var hasFaceVertexColor = IsBitSet(type, 7);

                //Debug.WriteLine("type:{0} bits( IsQuad:{1} Material:{2} FaceVertexUv:{3} FaceNormal:{4} FaceVertexNormal:{5} FaceColor:{6} FaceVertexColor:{7}", type, isQuad, hasMaterial, hasFaceVertexUv, hasFaceNormal, hasFaceVertexNormal, hasFaceColor, hasFaceVertexColor);

                if (isQuad != 0)
                {
                    var a = facesData[offset++];
                    var b = facesData[offset++];
                    var c = facesData[offset++];
                    var d = facesData[offset++];
                    var faceA = new Face3(a, b, d);
                    var faceB = new Face3(b, c, d);

                    if (hasMaterial != 0)
                    {
                        var materialIndex = facesData[offset++];
                    }

                    if (hasFaceVertexUv != 0)
                    {
                        for (var i = 0; i < nUvLayers; i++)
                        {
                            var uvLayer = uvsData[ i ];

                            var uvIndexA = facesData[offset++];
                            var uvIndexB = facesData[offset++];
                            var uvIndexC = facesData[offset++];
                            var uvIndexD = facesData[offset++];

                            var uA = uvLayer[uvIndexA * 2];
                            var vA = uvLayer[uvIndexA * 2 + 1];
                            var uvA = new Vector2(uA, vA);

                            var uB = uvLayer[uvIndexB * 2];
                            var vB = uvLayer[uvIndexB * 2 + 1];
                            var uvB = new Vector2(uB, vB);

                            var uC = uvLayer[uvIndexC * 2];
                            var vC = uvLayer[uvIndexC * 2 + 1];
                            var uvC = new Vector2(uC, vC);

                            var uD = uvLayer[uvIndexD * 2];
                            var vD = uvLayer[uvIndexD * 2 + 1];
                            var uvD = new Vector2(uD, vD);


                            geometry.faceVertexUvs[i].Add(new UVFaceSet(uvA,uvB,uvD));
                            geometry.faceVertexUvs[i].Add(new UVFaceSet(uvB,uvC,uvD));
                        }
                    }

                    if (hasFaceNormal != 0)
                    {
                        var normalIndex = facesData[offset++] * 3;

                        var x = normalsData[normalIndex++];
                        var y = normalsData[normalIndex++];
                        var z = normalsData[normalIndex];
                        var n = new Vector3(x, y, z); ;
                        faceA.NormalA = n;
                        faceA.NormalB = n;
                        faceA.NormalC = n;
                        faceB.NormalA = n;
                        faceB.NormalB = n;
                        faceB.NormalC = n;
                    }

                    if (hasFaceVertexNormal != 0)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            var normalIndex = facesData[offset++] * 3;
                            var x = normalsData[normalIndex++];
                            var y = normalsData[normalIndex++];
                            var z = normalsData[normalIndex];
                            var n = new Vector3(x, y, z);

                            switch(i)
                            {
                                case 0: faceA.NormalA = n; break;
                                case 1: faceA.NormalB = n; faceB.NormalA = n; break;
                                case 2: faceB.NormalB = n; break;
                                case 3: faceA.NormalC = n; faceB.NormalC = n; break;
                            }
                        }
                    }

                    if (hasFaceColor != 0)
                    {
                        var colorIndex = facesData[offset++];
                        var hex = colorsData[colorIndex];
                        var color = new Color(hex);
                        faceA.ColorA = color;
                        faceA.ColorB = color;
                        faceA.ColorC = color;
                        faceB.ColorA = color;
                        faceB.ColorB = color;
                        faceB.ColorC = color;
                    }

                    if (hasFaceVertexColor != 0)
                    {
                        for (var i = 0; i < 4; i++)
                        {
                            var colorIndex = facesData[offset++];
                            var hex = colorsData[colorIndex];
                            var color = new Color(hex);

                            switch (i)
                            {
                                case 0: faceA.ColorA = color; break;
                                case 1: faceA.ColorB = color; faceB.ColorA = color; break;
                                case 2: faceB.ColorB = color; break;
                                case 3: faceA.ColorC = color; faceB.ColorC = color; break;
                            }
                        }
                    }

                    geometry.faces.Add(faceA);
                    geometry.faces.Add(faceB);
                }
                else
                {
                    var a = facesData[offset++];
                    var b = facesData[offset++];
                    var c = facesData[offset++];
                    var face = new Face3(a, b, c);

                    if (hasFaceVertexUv != 0)
                    {
                        for (var i = 0; i < nUvLayers; i++)
                        {
                            var uvLayer = uvsData[i];

                            var uvIndexA = facesData[offset++];
                            var uvIndexB = facesData[offset++];
                            var uvIndexC = facesData[offset++];

                            var uA = uvLayer[uvIndexA * 2];
                            var vA = uvLayer[uvIndexA * 2 + 1];
                            var uvA = new Vector2(uA, vA);

                            var uB = uvLayer[uvIndexB * 2];
                            var vB = uvLayer[uvIndexB * 2 + 1];
                            var uvB = new Vector2(uB, vB);

                            var uC = uvLayer[uvIndexC * 2];
                            var vC = uvLayer[uvIndexC * 2 + 1];
                            var uvC = new Vector2(uC, vC);


                            geometry.faceVertexUvs[i].Add(new UVFaceSet(uvA, uvB, uvC));
                        }
                    }

                    if (hasFaceNormal != 0)
                    {
                        var normalIndex = facesData[offset++] * 3;
                        var x = normalsData[normalIndex++];
                        var y = normalsData[normalIndex++];
                        var z = normalsData[normalIndex];
                        var n = new Vector3(x, y, z);
                        face.NormalA = n;
                        face.NormalB = n;
                        face.NormalC = n;
                    }

                    if (hasFaceVertexNormal != 0)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            var normalIndex = facesData[offset++] * 3;
                            var x = normalsData[normalIndex++];
                            var y = normalsData[normalIndex++];
                            var z = normalsData[normalIndex];
                            var n = new Vector3(x, y, z);
                            switch (i)
                            {
                                case 0: face.NormalA = n; break;
                                case 1: face.NormalB = n; break;
                                case 2: face.NormalC = n; break;
                            }
                        }
                    }

                    if (hasFaceColor != 0)
                    {
                        var colorIndex = facesData[offset++];
                        var hex = colorsData[colorIndex];
                        var color = new Color(hex);
                        face.ColorA = color;
                        face.ColorB = color;
                        face.ColorC = color;
                    }

                    if (hasFaceVertexColor != 0)
                    {
                        for (var i = 0; i < 3; i++)
                        {
                            var colorIndex = facesData[offset++];
                            var hex = colorsData[colorIndex];
                            var color = new Color(hex);
                            switch (i)
                            {
                                case 0: face.ColorA = color; break;
                                case 1: face.ColorB = color; break;
                                case 2: face.ColorC = color; break;
                            }
                        }
                    }

                    geometry.faces.Add(face);
                }
            }
        }

        private int IsBitSet(int value, int position)
        {
            return value & (1 << position);
        }
    }
}
