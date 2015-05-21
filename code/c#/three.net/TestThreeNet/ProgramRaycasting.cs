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
    class ProgramRaycasting
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

            var camera = new OrthographicCamera(renderer, -1000, 1000)
            {
                Position = new Vector3(0, 0, 2)
            };

            //// create a point light
            scene.Add(new DirectionalLight(Color.White)
            {
                Target = Vector3.UnitX
            });


            var geometry = new BoxGeometry(20, 20, 20);

            for (var i = 0; i < 2000; i++)
            {
                var o = new Mesh(geometry, new MeshLambertMaterial(renderer) { Diffuse = Color.Random() });
                o.Position = new Vector3(Mathf.RandomF(-400, 400), Mathf.RandomF(-400, 400), Mathf.RandomF(-400, 400));
                o.Rotation = new Euler(Mathf.Tau * Mathf.RandomF(), Mathf.Tau * Mathf.RandomF(), Mathf.Tau * Mathf.RandomF());
                o.Scale = new Vector3(Mathf.RandomF(0.5f, 1.5f), Mathf.RandomF(0.5f, 1.5f), Mathf.RandomF(0.5f, 1.5f));
                scene.Add(o);
            }

            var raycaster = new Raycaster();
            Object3D INTERSECTED = null;
            Color previousColor = Color.White;

            var radius = 100;
            var previousTime = 0f;
            var stopwatch = Stopwatch.StartNew();
            while (!renderer.Done)
            {
                var now = (float)stopwatch.Elapsed.TotalSeconds;
                var deltaTime = now - previousTime;
                previousTime = now;

                var offset = now / 4;
                var sin = Mathf.Sin(offset) * radius;
                var cos = Mathf.Cos(offset) * radius;
                camera.Position = new Vector3(sin, sin, cos);
                camera.LookAt(Vector3.Zero);

                #region FindIntersections
                var vector = Projector.UnprojectVector(new Vector3(renderer.MousePositionNormalized,-1),camera.projectionMatrix, camera.matrixWorld);
                var direction = new Vector3(0, 0, -1);
                direction.TransformDirection(camera.matrixWorld);

                raycaster.Set(vector, direction);

                var intersects = raycaster.IntersectObjects(scene.Children);

                if (intersects != null &&  intersects.Count > 0)
                {
                    var first = intersects[0];

                    if (INTERSECTED != first.Object)
                    {
                        if (INTERSECTED != null)
                        {
                            var basic = INTERSECTED.Material as MeshLambertMaterial;
                            basic.Emissive = previousColor;
                        }

                        var firstMat = first.Object.Material as MeshLambertMaterial;

                        INTERSECTED = first.Object;
                        previousColor = firstMat.Emissive;
                        firstMat.Emissive = Color.Red;
                    }
                }
                else
                {
                    if (INTERSECTED != null)
                    {
                        (INTERSECTED.Material as MeshLambertMaterial).Emissive = previousColor;
                    }

                    INTERSECTED = null;

                }
                #endregion

                renderer.RenderFrame(scene, camera);
            }
        }
    }
}