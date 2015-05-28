using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Three.Net.Cameras;
using Three.Net.Core;
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
    class ProgramLight
    {
        static void Main(string[] args)
        {
            var mediaPath = Path.GetFullPath("../../../../../js/r68/examples/");
            var texturesPath = Path.Combine(mediaPath, "textures");

            var renderer = new Renderer();

            var scene = new Scene()
            {
                //Fog = new FogExp2(Color.Blue, 0.24f)
            };

            var camera = new PerspectiveCamera(renderer, 75)
            {
                Position = new Vector3(0, 0, 2)
            };

            //// create a point light
            scene.Add(new PointLight(Color.Gold)
            {
                Position = new Vector3(3, 2, 2),
                Intensity = 1
            });
            
            scene.Add(new DirectionalLight(Color.Blue)
            {
                Target = new Vector3(1,0,0),
               Intensity = 2
            });

            SpotLight s = new SpotLight(Color.Red)
            {
                Intensity = 5,
                Distance = 10,
                Angle = 106.7f,
                Exponent = 200,
                Position = new Vector3(-2, 0, 4),
                Target = new Vector3(0,0,0),
            };

            scene.Add(s);
            
            //var loader = new JsonLoader();
            mediaPath = Path.Combine(mediaPath, "../../tests/");
            //var path = Path.Combine(mediaPath, "frankie.obj");
            //var fullpath = Path.GetFullPath(path);
            //Debug.Assert(File.Exists(fullpath));
            //var frankie = ObjMtlLoader.Parse(renderer, fullpath);
            //frankie.Scale.Multiply(0.4f);
            //scene.Add(frankie);
            //var json = JObject.Parse(File.ReadAllText(fullpath));
            //var info = loader.Parse(renderer, json, mediaPath);
            //var monkey = new Mesh(info.Geometry, new MeshFaceMaterial(renderer, info.Materials));
            //scene.Add(monkey);

                var materialMap = new MeshPhongMaterial(renderer)
                //var materialMap = new MeshBasicMaterial(renderer)
                //var materialMap = new MeshLambertMaterial(renderer)
                {
                    Hardness = 1,

                    Shininess = 10,

                    Specular = new Color(.001f, .05f, .1f),

                    //DiffuseColor = Color.Red,
                    //Ambient = Color.Red,
                    ShouldWrapAround = true,

                    NormalMap = new Texture(Path.Combine(texturesPath, "planets\\earth_normal_2048.jpg")),
                    SpecularMap = new Texture(Path.Combine(texturesPath, "planets\\earth_specular_2048.jpg")),
                    DiffuseMap = new Texture(Path.Combine(texturesPath, "planets\\earth_atmos_2048.jpg")),

                    //DiffuseColor = Color.Red,

                };

                var materialNoMap = new MeshPhongMaterial(renderer)
                //var materialNoMap = new MeshBasicMaterial(renderer)
                //var materialNoMap = new MeshLambertMaterial(renderer)
                {
                    Hardness = 1,

                    Shininess = 10,

                    Specular = new Color(.001f, .05f, .1f),

                    ShouldWrapAround = true,

                    NormalMap = new Texture(Path.Combine(texturesPath, "planets\\earth_normal_2048.jpg")),
                    SpecularMap = new Texture(Path.Combine(texturesPath, "planets\\earth_specular_2048.jpg")),
                    DiffuseMap = new Texture(Path.Combine(texturesPath, "planets\\earth_atmos_2048.jpg")),

                    //DiffuseColor = Color.Blue,
                };

            var cube = CreateCube(renderer, scene, texturesPath, materialMap);
            var cube2
                = CreateCube(renderer, scene, texturesPath, materialNoMap);
            var cube3 = CreateCube(renderer, scene, texturesPath, materialMap);

            var p = cube.Position;

            //p.x -= 2;

            cube.Position = p;

            p = cube2.Position;
            p.x += 2;

            cube2.Position = p;

            scene.Add(cube);
            //scene.Add(cube2);
            //scene.Add(cube3);
            //frankie.Add(cube);
            //cube.Position = new Vector3(0, 1, 1);

            var lines = CreateHilbertCube(renderer, scene);
            lines.Scale = new Vector3(4, 4, 4);
            //scene.Add(lines);
            //lines.Position = new Vector3(0, 1, -1);

            var arrowHelper = ArrowHelper.Create(renderer, new Vector3(1,1,0), Vector3.Zero, 0.35f, Color.Red);
            arrowHelper.Position = new Vector3(0.25f, 0.25f, 0.25f);
            //scene.Add(arrowHelper);

            var axisHelper = AxisHelper.Create(renderer, 0.25f);
            axisHelper.Position = new Vector3(-0.25f, 0.25f, 0.25f);
            //scene.Add(axisHelper);

            //var boundingBoxHelper = BoundingBoxHelper.Create(renderer, frankie);
            //scene.Add(boundingBoxHelper);

            var previousTime = 0f;
            var stopwatch = Stopwatch.StartNew();
            while (!renderer.Done)
            {
                var now = (float)stopwatch.Elapsed.TotalSeconds;
                var deltaTime = now - previousTime;
                previousTime = now;

                //var offset = material.DiffuseMap.Offset;
                //offset.x += deltaTime * 0.75f;
                //offset.y -= deltaTime * 0.5f;
                //material.DiffuseMap.Offset = offset;

                //var r = frankie.Rotation;
                //r.y += deltaTime;
                //frankie.Rotation = r;

                //r = axisHelper.Rotation;
                //r.x = Mathf.Pi / 4;
                //r.y -= deltaTime;
                //axisHelper.Rotation = r;
                
                var r = cube.Rotation;
                r.y -= deltaTime / 2;
                r.x = Mathf.Sin(now / 4);
                cube.Rotation = r;

                r = cube2.Rotation;
                r.y -= deltaTime / 2;
                cube2.Rotation = r;
                //boundingBoxHelper.Update();

                renderer.RenderFrame(scene, camera);
            }
        }

        private static Mesh CreateCube(Renderer renderer, Scene scene, string texturesPath, MeshBasicMaterial material)
        {
            var cube = new Mesh(new SphereGeometry(1, 50, 50), material);
            return cube;
        }

        private static Line CreateHilbertCube(Renderer renderer, Scene scene)
        {
            var points = Mathf.Hilbert3D(Vector3.Zero, 0.25f, 2);
            var spline = new SplineCurve3(points);
            var geometrySpline = new Geometry();
            geometrySpline.vertexColors = new List<Color>();
            var subdivisions = 6;
            var count = points.Count * subdivisions;
            for (var i = 0; i < count; i++)
            {
                var index = i / (float)(points.Count * subdivisions);
                var position = spline.InterpolatedPoint(index);
                geometrySpline.vertices.Add(position);

                var hsl = Color.FromHSV(index, 1, 1);
                geometrySpline.vertexColors.Add(hsl);
            }

            var lines = new Line(geometrySpline, new LineDashedMaterial(renderer)
            {
                //DiffuseColor = Color.Cornflowerblue,
                VertexColors = VertexColorMode.Vertex,
                LineWidth = 0.01f,
                DashSize = 0.006f,
                GapSize = 0.002f,
            });
            return lines;
        }
    }
}
