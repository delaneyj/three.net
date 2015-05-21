using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Math;

namespace Three.Net.Lights
{
    public class VirtualLight : DirectionalLight
    {
        public List<Vector3> PointsWorld;
        public List<Vector3> PointsFrustum;
        public Cameras.Camera OriginalCamera;

        public VirtualLight(Color color):base(color)
        {

        }
    }
}
