using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;

namespace Three.Net.Extras.Geometries
{
    public class ParametricGeometry : Geometry
    {
        public static Func<float,float,Vector3> Klein = (v, u) =>
        {
		    u *= Mathf.Pi;
		    v *= Mathf.Tau;
		    u = u * 2;
		    float x, y, z;
		    if (u < Mathf.Pi)
            {
			    x = 3f * Mathf.Cos(u) * (1f + Mathf.Sin(u)) + (2f * (1f - Mathf.Cos(u) / 2f)) * Mathf.Cos(u) * Mathf.Cos(v);
			    z = -8f * Mathf.Sin(u) - 2f * (1f - Mathf.Cos(u) / 2f) * Mathf.Sin(u) * Mathf.Cos(v);
		    } 
            else 
            {
			    x = 3f * Mathf.Cos(u) * (1f + Mathf.Sin(u)) + (2f * (1f - Mathf.Cos(u) / 2f)) * Mathf.Cos(v + Mathf.Pi);
			    z = -8f * Mathf.Sin(u);
		    }
		    y = -2f * (1f - Mathf.Cos(u) / 2f) * Mathf.Sin(v);
            var vector = new Vector3(x, y, z);
            vector.Divide(4);
            return vector;
	    };
        
        public static Func<float,float,Vector3> Mobius2d = (u, t) =>
        {
            // flat mobius strip
		    // http://www.wolframalpha.com/input/?i=M%C3%B6bius+strip+parametric+equations&lk=1&a=ClashPrefs_*Surface.MoebiusStrip.SurfaceProperty.ParametricEquations-
		    u = u - 0.5f;
		    var v = 2 * Mathf.Pi * t;
            var a = 2;
            var x = Mathf.Cos(v) * (a + u * Mathf.Cos(v/2));
		    var y = Mathf.Sin(v) * (a + u * Mathf.Cos(v/2));
		    var z = u * Mathf.Sin(v/2);
		    return new Vector3(x, y, z);
        };

        public static Func<float, float, Vector3> Mobius3d = (u, t) =>
        {
            // volumetric mobius strip
            u *= Mathf.Pi;
            t *= Mathf.Tau;

            u = u * 2;
            float phi = u / 2, major = 2.25f, a = 0.125f, b = 0.65f;
            var x = a * Mathf.Cos(t) * Mathf.Cos(phi) - b * Mathf.Sin(t) * Mathf.Sin(phi);
            var z = a * Mathf.Cos(t) * Mathf.Sin(phi) + b * Mathf.Sin(t) * Mathf.Cos(phi);
            var y = (major + x) * Mathf.Sin(u);
            x = (major + x) * Mathf.Cos(u);
            return new Vector3(x, y, z);
        };

        public ParametricGeometry(Func<float, float, Vector3> func, int slices, int stacks)
        {
            var uvs = faceVertexUvs[0];
            var stackCount = stacks + 1;
            var sliceCount = slices + 1;

            for (var i = 0f; i <= stacks; i++)
            {
                var v = i / stacks;
                for (var j = 0f; j <= slices; j++)
                {
                    var u = j / slices;
                    var p = func(u, v);
                    vertices.Add(p);
                }
            }

            for (var i = 0; i < stacks; i++)
            {
                for (var j = 0; j < slices; j++)
                {
                    var a = i * sliceCount + j;
                    var b = i * sliceCount + j + 1;
                    var c = (i + 1) * sliceCount + j + 1;
                    var d = (i + 1) * sliceCount + j;

                    var slicesF = (float)slices;
                    var stacksF = (float)stacks;
                    var uva = new Vector2(j / slicesF, i / stacksF);
                    var uvb = new Vector2((j + 1) / slicesF, i / stacksF);
                    var uvc = new Vector2((j + 1) / slicesF, (i + 1) / stacksF);
                    var uvd = new Vector2(j / slicesF, (i + 1) / stacksF);

                    faces.Add(new Face3(a, b, d));
                    uvs.Add(new UVFaceSet(uva, uvb, uvd));
                    faces.Add(new Face3(b, c, d));
                    uvs.Add(new UVFaceSet(uvb, uvc, uvd));
                }
            }

            // console.log(this);

            // magic bullet
            // var diff = this.mergeVertices();
            // console.log('removed ', diff, ' vertices by merging');
            ComputeNormals();
        }
    }
}