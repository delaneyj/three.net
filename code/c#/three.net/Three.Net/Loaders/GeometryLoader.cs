using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;
using Three.Net.Math;
using Three.Net.Renderers.GL4;

namespace Three.Net.Loaders
{
    public class GeometryLoader : Loader
    {
        public GeometryLoader(LoadingManager manager)
        {

        }

        public override LoadedInfo Parse(Renderer renderer, JObject json, string texturePath)
        {
            var geometry = new Geometry();
            throw new NotImplementedException(); //geometry.faces = json["indices"];
            throw new NotImplementedException(); //geometry.vertices = json.vertices;
            throw new NotImplementedException(); //geometry.normals = json.normals;
            throw new NotImplementedException(); //geometry.uvs = json.uvs;

            throw new NotImplementedException();
            //Sphere boundingSphere = null;
            //var boundingSphere = json.boundingSphere;
            //if ( boundingSphere !== undefined ) {
            //geometry.boundingSphere = new Sphere(
            //	new THREE.Vector3().fromArray( boundingSphere.center !== undefined ? boundingSphere.center : [ 0, 0, 0 ] ),
            //	boundingSphere.radius
            //);
        }
    }
}
