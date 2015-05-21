using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Three.Net.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Objects;
using Three.Net.Renderers.GL4;
using Three.Net.Textures;
using System.Linq;

namespace Three.Net.Loaders
{
    public static class ObjMtlLoader
    {
        // v float float float
        private static readonly Regex vertex_pattern = new Regex(@"v( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)");

        // vn float float float
        private static readonly Regex normal_pattern = new Regex(@"vn( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)");

        // vt float float
        private static readonly Regex uv_pattern = new Regex(@"vt( +[\d|\.|\+|\-|e]+)( +[\d|\.|\+|\-|e]+)");

        // f vertex vertex vertex ...
        private static readonly Regex face_pattern1 = new Regex(@"f( +\d+)( +\d+)( +\d+)( +\d+)?");

        // f vertex/uv vertex/uv vertex/uv ...
        private static readonly Regex face_pattern2 = new Regex(@"f( +(\d+)\/(\d+))( +(\d+)\/(\d+))( +(\d+)\/(\d+))( +(\d+)\/(\d+))?");

        // f vertex/uv/normal vertex/uv/normal vertex/uv/normal ...
        private static readonly Regex face_pattern3 = new Regex(@"f( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))( +(\d+)\/(\d+)\/(\d+))?");

        // f vertex//normal vertex//normal vertex//normal ... 
        private static readonly Regex face_pattern4 = new Regex(@"f( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))( +(\d+)\/\/(\d+))?");

        private static readonly Regex objectTest = new Regex(@"^o ");
        private static readonly Regex groupTest = new Regex(@"^g ");
        private static readonly Regex useMTLTest = new Regex(@"^usemtl ");
        private static readonly Regex mtlTest = new Regex("^mtllib ");
        private static readonly Regex smoothShadingTest = new Regex(@"^s ");

        private static Renderer renderer;
        private static Geometry geometry;
        private static MeshBasicMaterial material;
        private static Mesh mesh;
        private static List<int> vertexIndicies;
        private static List<Vector3> normals;
        private static List<Vector2> uvs2;
        private static Dictionary<string, MeshBasicMaterial> materials = new Dictionary<string, MeshBasicMaterial>();

        public static Object3D Parse(Renderer renderer, string path, Action<string> mtllibCallback = null)
        {
            ObjMtlLoader.renderer = renderer;

            var text = File.ReadAllText(path);

            var vector = new Func<string, string, string, Vector3>((x, y, z) =>
            {
                var vx = Convert.ToSingle(x);
                var vy = Convert.ToSingle(y);
                var vz = Convert.ToSingle(z);
                return new Vector3(vx, vy, vz);
            });

            var uv = new Func<string, string, Vector2>((u, v) =>
            {
                var vu = Convert.ToSingle(u);
                var vv = Convert.ToSingle(v);
                return new Vector2(vu, vv);
            });

            var parseVertexIndex = new Func<string, int>((indexString) =>
            {
                var index = Convert.ToInt32(indexString);
                return index >= 0 ? index - 1 : index + vertexIndicies.Count;
            });

            var parseNormalIndex = new Func<string, int>((indexString) =>
            {
                var index = Convert.ToInt32(indexString);
                return index >= 0 ? index - 1 : index + normals.Count;
            });

            var parseUVIndex = new Func<string, int>((indexString) =>
            {
                var index = Convert.ToInt32(indexString);
                return index >= 0 ? index - 1 : index + uvs2.Count;
            });

            var add_face = new Action<string, string, string, List<string>>((a, b, c, normals_inds) =>
            {

                if (normals_inds == null)
                {

                    geometry.faces.Add(new Face3(
                        vertexIndicies[parseVertexIndex(a)] - 1,
                        vertexIndicies[parseVertexIndex(b)] - 1,
                        vertexIndicies[parseVertexIndex(c)] - 1
                    ));
                }
                else
                {
                    geometry.faces.Add(new Face3(
                        vertexIndicies[parseVertexIndex(a)] - 1,
                        vertexIndicies[parseVertexIndex(b)] - 1,
                        vertexIndicies[parseVertexIndex(c)] - 1,
                        normals[parseNormalIndex(normals_inds[0])],
                        normals[parseNormalIndex(normals_inds[1])],
                        normals[parseNormalIndex(normals_inds[2])]));
                }
            });

            var add_uvs = new Action<string, string, string>((a, b, c) =>
            {

                geometry.faceVertexUvs[0].Add(new UVFaceSet(uvs2[parseUVIndex(a)], uvs2[parseUVIndex(b)], uvs2[parseUVIndex(c)]));
            });

            var handle_face_line = new Action<List<string>, List<string>, List<string>>((faces, uvs, normals_inds) =>
            {

                if (faces.Count == 3)
                {

                    add_face(faces[0], faces[1], faces[2], normals_inds);

                    if (uvs != null && uvs.Count > 0)
                    {
                        add_uvs(uvs[0], uvs[1], uvs[2]);
                    }

                }
                else
                {

                    if (normals_inds != null && normals_inds.Count > 0)
                    {
                        add_face(faces[0], faces[1], faces[3], new List<string>() { normals_inds[0], normals_inds[1], normals_inds[3] });
                        add_face(faces[1], faces[2], faces[3], new List<string>() { normals_inds[1], normals_inds[2], normals_inds[3] });

                    }
                    else
                    {
                        add_face(faces[0], faces[1], faces[3], null);
                        add_face(faces[1], faces[2], faces[3], null);

                    }

                    if (uvs != null && uvs.Count > 0)
                    {
                        add_uvs(uvs[0], uvs[1], uvs[3]);
                        add_uvs(uvs[1], uvs[2], uvs[3]);

                    }

                }

            });

            var o = new Object3D();

            var lines = text.Split('\n');

            // create mesh if no objects in text
            if (lines.Where(l => l.StartsWith("o")).Count() == 0)
            {
                geometry = new Geometry();
                material = new MeshBasicMaterial(renderer);
                mesh = new Mesh(geometry, material);
                o.Add(mesh);
            }

            vertexIndicies = new List<int>();
            normals = new List<Vector3>();
            uvs2 = new List<Vector2>();

            // fixes

            //text = text.Replace("\r\n",string.Empty); // handles line continuations \
            foreach (var l in lines)
            {
                if (l.Length == 0 || l[0] == '#') continue;

                var line = l.Trim();
                var result = line.Split(' ','/');

                if (vertex_pattern.IsMatch(line))
                {
                    // ["v 1.0 2.0 3.0", "1.0", "2.0", "3.0"]
                    geometry.vertices.Add(
                        vector(
                            result[1], result[2], result[3]
                        )
                    );
                    vertexIndicies.Add(geometry.vertices.Count);
                }

                else if (normal_pattern.IsMatch(line))
                {
                    // ["vn 1.0 2.0 3.0", "1.0", "2.0", "3.0"]
                    normals.Add(vector(result[1], result[2], result[3]));
                }
                else if (uv_pattern.IsMatch(line))
                {
                    // ["vt 0.1 0.2", "0.1", "0.2"]
                    uvs2.Add(
                        uv(
                            result[1], result[2]
                        )
                    );
                }
                else if (face_pattern1.IsMatch(line))
                {
                    // ["f 1 2 3", "1", "2", "3", undefined]WDEAADEAWAAAADD
                    if (result.Length == 4)
                    {
                        handle_face_line(new List<string>() { result[1], result[2], result[3] }, null, null);
                    }
                    else
                    {
                        handle_face_line(new List<string>() { result[1], result[2], result[3], result[4] }, null, null);
                    }
                }
                else if (face_pattern2.IsMatch(line))
                {
                    // ["f 1/1 2/2 3/3", " 1/1", "1", "1", " 2/2", "2", "2", " 3/3", "3", "3", undefined, undefined, undefined]
                    if (result.Length == 7)
                    {
                        handle_face_line(
                            new List<string>() { result[1], result[3], result[5] }, //faces
                            new List<string>() { result[2], result[4], result[6] }, //uv
                            null
                        );
                    }
                    else
                    {
                        handle_face_line(
                            new List<string>() { result[1], result[3], result[5], result[7] }, //faces
                            new List<string>() { result[2], result[4], result[6], result[8] }, //uv
                            null
                        );
                    }
                }
                else if (face_pattern3.IsMatch(line))
                {
                    if (result.Length == 10)
                    {
                        handle_face_line(
                           new List<string>() { result[1], result[4], result[7], }, //faces
                           new List<string>() { result[2], result[5], result[8], }, //uv
                           new List<string>() { result[3], result[6], result[9], } //normal
                       );
                    }
                    else
                    {
                        // ["f 1/1/1 2/2/2 3/3/3", " 1/1/1", "1", "1", "1", " 2/2/2", "2", "2", "2", " 3/3/3", "3", "3", "3", undefined, undefined, undefined, undefined]
                        handle_face_line(
                            new List<string>() { result[1], result[4], result[7], result[10] }, //faces
                            new List<string>() { result[2], result[5], result[8], result[11] }, //uv
                            new List<string>() { result[3], result[6], result[9], result[12] } //normal
                        );
                    }
                }
                else if (face_pattern4.IsMatch(line))
                {
                    if (result.Length == 10)
                    {
                        // ["f 1//1 2//2 3//3", " 1//1", "1", "1", " 2//2", "2", "2", " 3//3", "3", "3", undefined, undefined, undefined]
                        handle_face_line(
                            new List<string>() { result[1], result[4], result[7] }, //faces
                            null, //uv
                            new List<string>() { result[3], result[6], result[9] } //normal
                        );
                    }
                    else
                    {
                        // ["f 1//1 2//2 3//3", " 1//1", "1", "1", " 2//2", "2", "2", " 3//3", "3", "3", undefined, undefined, undefined]
                        handle_face_line(
                            new List<string>() { result[1], result[4], result[7], result[10] }, //faces
                            null, //uv
                            new List<string>() { result[3], result[6], result[9], result[12] } //normal
                        );
                    }
                }
                else if (objectTest.IsMatch(line))
                {
                    geometry = new Geometry();
                    material = new MeshBasicMaterial(renderer);

                    mesh = new Mesh(geometry, material);
                    mesh.Name = line.Substring(2).Trim();
                    o.Add(mesh);
                }
                else if (groupTest.IsMatch(line))
                {
                    // group
                }
                else if (useMTLTest.IsMatch(line))
                {
                    // material
                    material = materials[result[1].Trim()];
                    mesh.Material = material;
                }
                else if (mtlTest.IsMatch(line))
                {
                    // mtl file
                    var objPath = Path.GetDirectoryName(path);
                    var fullPath = Path.Combine(objPath, result[1].Trim());
                    ParseMaterials(fullPath);
                }
                else if (smoothShadingTest.IsMatch(line))
                {
                    // smooth shading
                }
                else
                {
                    throw new NotSupportedException(line);
                }
            }

            var children = o.Children;
            foreach (var c in o.Children)
            {
                var geometry = c.geometry;
                geometry.ComputeNormals();
                geometry.ComputeBoundingSphere();
            }

            return o;
        }

        private static void ParseMaterials(string fileName)
        {
            MeshBasicMaterial currentMaterial = null;
            var path = Path.GetDirectoryName(fileName);
            var lines = File.ReadAllLines(fileName);
            foreach (var line in lines)
            {
                if (line.Length == 0 || line.StartsWith("#")) continue;

                var parts = line.Trim().Split(' ');

                if (parts[0] == "newmtl")
                {
                    currentMaterial = new MeshPhongMaterial(renderer)
                    {
                        Name = parts[1].Trim()
                    };
                    materials.Add(currentMaterial.Name, currentMaterial);
                }
                else if (parts[0] == "Ns")
                {
                    var phong = currentMaterial as MeshPhongMaterial;
                    if (phong != null)
                    {
                        var hardness = Convert.ToSingle(parts[1]);
                        phong.Hardness = hardness;
                    }
                }
                else if (parts[0] == "Ni")
                {

                }
                else if (parts[0] == "Ka")
                {
                    var lambert = currentMaterial as MeshLambertMaterial;
                    if (lambert != null)
                    {
                        var r = Convert.ToSingle(parts[1]);
                        var g = Convert.ToSingle(parts[2]);
                        var b = Convert.ToSingle(parts[3]);
                        lambert.Ambient = new Color(r, g, b);
                    }
                }
                else if (parts[0] == "Kd")
                {
                    var r = Convert.ToSingle(parts[1]);
                    var g = Convert.ToSingle(parts[2]);
                    var b = Convert.ToSingle(parts[3]);
                    currentMaterial.Diffuse = new Color(r, g, b);
                }
                else if (parts[0] == "map_Kd")
                {
                    var fullPath = Path.Combine(path, parts[1].Trim());
                    currentMaterial.DiffuseMap = new Texture(fullPath);
                }
                else if (parts[0] == "Ks")
                {
                    var phong = currentMaterial as MeshPhongMaterial;
                    if (phong != null)
                    {
                        var r = Convert.ToSingle(parts[1]);
                        var g = Convert.ToSingle(parts[2]);
                        var b = Convert.ToSingle(parts[3]);
                        phong.Specular = new Color(r, g, b);
                    }
                }
                else if (parts[0] == "d")
                {

                }
                else if (parts[0] == "illum")
                {

                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
    }
}