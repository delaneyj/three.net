using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Extras.Core;
using Three.Net.Materials;
using Three.Net.Math;

namespace Three.Net.Objects
{
    public enum LineType
    {
        Strip,
        Pieces
    }

    public class Line : Object3D
    {
        public LineType Type;

        public Line(Geometry geometry, LineBasicMaterial material, LineType type = LineType.Strip)
        {
            this.geometry = geometry;
            Material = material;
            this.Type = type;
        }

        internal override void Raycast(Extras.Core.Raycaster raycaster, IList<IntersectionInfo> intersectionsList)
        {
            if ( geometry.BoundingSphere == Sphere.Empty) geometry.ComputeBoundingSphere();

		// Checking boundingSphere distance to ray
		var sphere = geometry.BoundingSphere;
		sphere.Apply(matrixWorld );

		if (!raycaster.ray.IsIntersectionSphere( sphere )) return;

            var inverseMatrix = Matrix4.GetInverse(matrixWorld );
		    var ray = raycaster.ray;
            ray.Apply( inverseMatrix );

            throw new NotImplementedException();
        }
    }
}
