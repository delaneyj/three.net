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
    class ProgramSprites
    {
        private static Renderer renderer;
        private static string mediaPath, texturesPath;
        private static float previousTime = 0, now = 0, deltaTime;
        private static Scene scene,sceneOrtho;
        private static Camera camera,cameraOrtho;
        private static Sprite spriteTL,spriteTR,spriteBL,spriteBR,spriteC;
        
        static void Main(string[] args)
        {
            Init();

            
            var stopwatch = Stopwatch.StartNew();
            while (!renderer.Done)
            {
                now = (float)stopwatch.Elapsed.TotalSeconds;
                deltaTime = now - previousTime;
                previousTime = now;

                Update(deltaTime, now);

                Render();
            }
        }

        private static void Render()
        {
            renderer.Clear();

            renderer.RenderFrame(sceneOrtho, cameraOrtho);
        }

        private static void Update(float deltaTime, float now)
        {
            
        }

        private static void Init()
        {
            mediaPath = Path.GetFullPath("../../../../../js/r68/examples/");
            texturesPath = Path.Combine(mediaPath, "textures");
            renderer = new Renderer();

            scene = new Scene()
            {
                Fog = new FogLinear(Color.Black, 1500,2100)
            };

            camera = new PerspectiveCamera(renderer, 60, 1, 2100)
            {
                Position = new Vector3(0, 0, 1500)
            };


            cameraOrtho = new OrthographicCamera(renderer, 1, 10000)
        {
            Position = new Vector3(0, 0, 150)
        };
            sceneOrtho = new Scene();

            var amount = 200;
            var radius = 500;


            sceneOrtho.Add(new Mesh(new SphereGeometry(100, 50, 50), new MeshBasicMaterial(renderer) { Diffuse = Color.Red}));
            

            var group = new Object3D();

            var materialA = new SpriteMaterial(renderer)
            {
                DiffuseMap = new Texture(Path.Combine(texturesPath, "sprite0.png")),
                Diffuse = Color.White,
                UseFog = true,
            };

            spriteTL = new Sprite(renderer, materialA)
            {
                Scale = new Vector3(materialA.DiffuseMap.Resolution.Width,materialA.DiffuseMap.Resolution.Height,1),                
            };
            sceneOrtho.Add(spriteTL);

            spriteTR = new Sprite(renderer, materialA)
            {
                Scale = new Vector3(materialA.DiffuseMap.Resolution.Width, materialA.DiffuseMap.Resolution.Height, 1),
            };
            sceneOrtho.Add(spriteTR);

            spriteBL = new Sprite(renderer, materialA)
            {
                Scale = new Vector3(materialA.DiffuseMap.Resolution.Width, materialA.DiffuseMap.Resolution.Height, 1),
            };
            sceneOrtho.Add(spriteBL);

            spriteBR = new Sprite(renderer, materialA)
            {
                Scale = new Vector3(materialA.DiffuseMap.Resolution.Width, materialA.DiffuseMap.Resolution.Height, 1),
            };
            sceneOrtho.Add(spriteBR);

            spriteC = new Sprite(renderer, materialA)
            {
                Scale = new Vector3(materialA.DiffuseMap.Resolution.Width, materialA.DiffuseMap.Resolution.Height, 1),
            };
            sceneOrtho.Add(spriteC);

            UpdateHUDSprites();

            var materialB = new SpriteMaterial(renderer)
            {
                DiffuseMap = new Texture(Path.Combine(texturesPath, "sprite1.png")),
                Diffuse = Color.White,
                UseFog = true,
            };

            var materialC = new SpriteMaterial(renderer)
            {
                DiffuseMap = new Texture(Path.Combine(texturesPath, "sprite2.png")),
                Diffuse = Color.White,
                UseFog = true,
            };

            mediaPath = Path.Combine(mediaPath, "../../tests/");

        }

        private static void UpdateHUDSprites()
        {
            var width = renderer.Width / 2f;
            var height = renderer.Height / 2f;

            var material = spriteTL.Material as SpriteMaterial;

            var imageWidth = material.DiffuseMap.Resolution.Width / 2f;
            var imageHeight = material.DiffuseMap.Resolution.Height / 2f;

            spriteTL.Position = new Vector3(-width + imageWidth, height - imageHeight, 1); // top left
            spriteTR.Position = new Vector3(width - imageWidth, height - imageHeight, 1); // top right
            spriteBL.Position = new Vector3(-width + imageWidth, -height + imageHeight, 1); // bottom left
            spriteBR.Position = new Vector3(width - imageWidth, -height + imageHeight, 1); // bottom right
            spriteC.Position = Vector3.UnitZ; // center
        }
    }
}