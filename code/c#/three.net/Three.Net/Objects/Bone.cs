using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Three.Net.Core;

namespace Three.Net.Objects
{
    public class Bone : Object3D
    {
        public float AccumulatedPositionWeight;
        public float AccumulatedRotationWeight;
        public float AccumulatedScaleWeight;
    }
}
