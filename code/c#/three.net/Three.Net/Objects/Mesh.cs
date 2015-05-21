using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Materials;
using Three.Net.Math;

namespace Three.Net.Objects
{
    public class Mesh : Object3D
    {
        public List<float> morphTargetInfluences;
        public Mesh(Geometry geometry, Material material)
        {
            Debug.Assert(material is CustomShaderMaterial || material is MeshBasicMaterial);
            this.geometry = geometry;
            this.Material = material;
        }

        internal override void Raycast(Raycaster raycaster, IList<IntersectionInfo> intersectionsList)
        {
            // Checking boundingSphere distance to ray
            if (geometry.BoundingSphere == null) geometry.ComputeBoundingSphere();

            var sphere = geometry.BoundingSphere;
            sphere.Apply(matrixWorld);

            if (!raycaster.ray.IsIntersectionSphere(sphere)) return;

            // Check boundingBox before continuing
            var inverseMatrix = Matrix4.GetInverse(matrixWorld);
            var ray = raycaster.ray;
            ray.Apply(inverseMatrix);

            if (geometry.BoundingBox == Box3.Empty) geometry.ComputeBoundingBox();
            
            if (!ray.IsIntersectionBox(geometry.BoundingBox)) return;
            
            var material = Material as MeshBasicMaterial;
            for (var f = 0; f < geometry.faces.Count; f++)
            {
                var face = geometry.faces[f];
                var a = geometry.vertices[face.A];
                var b = geometry.vertices[face.B];
                var c = geometry.vertices[face.C];

                if (material.UseMorphTargets)
                {
                    var vA = Vector3.Zero;
                    var vB = Vector3.Zero;
                    var vC = Vector3.Zero;

                    for (var t = 0; t < geometry.MorphTargets.Count; t++)
                    {
                        var influence = morphTargetInfluences[t];
                        if (influence == 0) continue;

                        var targets = geometry.MorphTargets[t].Vertices;

                        vA.x += (targets[face.A].x - a.x) * influence;
                        vA.y += (targets[face.A].y - a.y) * influence;
                        vA.z += (targets[face.A].z - a.z) * influence;

                        vB.x += (targets[face.B].x - b.x) * influence;
                        vB.y += (targets[face.B].y - b.y) * influence;
                        vB.z += (targets[face.B].z - b.z) * influence;

                        vC.x += (targets[face.C].x - c.x) * influence;
                        vC.y += (targets[face.C].y - c.y) * influence;
                        vC.z += (targets[face.C].z - c.z) * influence;
                    }

                    vA.Add(a);
                    vB.Add(b);
                    vC.Add(c);

                    a = vA;
                    b = vB;
                    c = vC;

                }

                Vector3? intersectionPoint = null;
                if (material.Side == Renderers.SideMode.Back) intersectionPoint = ray.IntersectTriangle(c, b, a, true);
                else intersectionPoint = ray.IntersectTriangle(a, b, c, material.Side != Renderers.SideMode.Double);

                if (!intersectionPoint.HasValue) continue;

                intersectionPoint.Value.Apply(matrixWorld);

                var distance = raycaster.ray.origin.DistanceTo(intersectionPoint.Value);

                if (distance < raycaster.Near || distance > raycaster.Far) continue;

                intersectionsList.Add(new IntersectionInfo(distance, intersectionPoint.Value, face, f, this));
            }
        }
    }
}