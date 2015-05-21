using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Three.Net.Extras.Core;
using Three.Net.Materials;
using Three.Net.Math;
using Three.Net.Scenes;

namespace Three.Net.Core
{
    public class Object3D
    {
        private static uint nextId = 0;

        public readonly uint Id = nextId++;
        public readonly Guid Guid = Guid.NewGuid();

        private string name = string.Empty;

        protected Object3D Parent = null;
        protected List<Object3D> children = new List<Object3D>();

        public Vector3 Up = Vector3.UnitY;
        public Material Material;

        public Vector3 Position = Vector3.Zero;
        public Euler Rotation { get { return rotation; } set { rotation = value; quaternion = Quaternion.From(rotation); } }
        public Quaternion Quaternion { get { return quaternion; } set { quaternion = value; rotation = Euler.From(quaternion); } }
        public Vector3 Scale = Vector3.One;

        internal bool matrixWorldNeedsUpdate = false;
        internal bool matrixAutoUpdate = true;
        internal Matrix4 matrix = Matrix4.Identity;
        public Matrix4 matrixWorld {get; internal set;}
        internal Matrix4 modelViewMatrix = Matrix4.Identity;
        internal Matrix3 normalMatrix = Matrix3.Identity;
        internal bool IsVisible = true;
        internal Geometry geometry = null;
        internal bool glActive = false;
        internal float? Zdepth = null;
        internal bool frustumCulled = true;

        private Euler rotation = Euler.Default;
        private Quaternion quaternion = Quaternion.Identity;

        public bool DoesCastShadow = true;
        public string Name;
        public Material customDepthMaterial;

        public Object3D()
        {
            matrixWorld = Matrix4.Identity;
        }

        public bool HasParent { get { return Parent != null; } }

        public IEnumerable<Object3D> Children { get { return children.AsEnumerable(); } }

        public void Apply(Matrix4 m) 
        {
            matrix.MultiplyMatrices(m, matrix);
            matrix.Decompose(ref Position, ref quaternion, ref Scale);
        }

        public void SetRotationFromAxisAngle(Vector3 axis,float angle ) 
        {
            // assumes axis is normalized
            Quaternion = Quaternion.FromAxisAngle( axis, angle );
        }

        //public void SetRotationFromEuler: function ( euler ) { Quaternion.setFromEuler( euler, true ); };

        public void RotateOnAxis(Vector3 axis, float angle) 
        {
		    // rotate object on axis in object space
		    // axis is assumed to be normalized
		    var q1 = Quaternion.FromAxisAngle( axis, angle );
            var q2 = Quaternion;
            q2.Multiply(q1);
            Quaternion = q2;
        }

        public void RotateX(float angle)
        {
            RotateOnAxis(Vector3.UnitX, angle );
        }

        public void RotateY(float angle)
        {
            RotateOnAxis(Vector3.UnitY, angle );
        }

        public void RotateZ(float angle)
        {
            RotateOnAxis(Vector3.UnitZ, angle );
        }

        // translate object by distance along axis in object space axis is assumed to be normalized
        public void TranslateOnAxis(Vector3 axis, float distance) 
        {
		var v1 = axis;
            v1.Apply( Quaternion );
            v1.Multiply( distance );
            Position.Add(v1);
        }

        public void TranslateX(float distance)
        {
            TranslateOnAxis(Vector3.UnitX, distance );
        }

        public void TranslateY(float distance)
        {
            TranslateOnAxis(Vector3.UnitY, distance );
        }

        public void TranslateZ(float distance)
        {
            TranslateOnAxis(Vector3.UnitZ, distance );
        }

        public Vector3 LocalToWorld(Vector3 vector ) 
        {
            vector.Apply(matrixWorld);
            return vector;
        }

        public Vector3 WorldToLocal(Vector3 vector)
        {
            var m1 = Matrix4.GetInverse(matrixWorld);
            vector.Apply(m1);
            return vector;
        }

        public void LookAt(Vector3 vector) 
        {
		// This routine does not support objects with rotated and/or translated parent(s)
            var m1 = Matrix4.Identity;
            m1.LookAt(vector, Position, Up);
            Quaternion = Quaternion.FromRotation(m1);
        }

        public void Add(params Object3D[] objects)
        {
            foreach(var o in objects)
            {
                if ( o.HasParent) o.Parent.Remove(o);

                o.Parent = this;
			    //object.dispatchEvent( { type: 'added' } );

                children.Add(o);

			// add to scene
			var parent = this;
			while (parent.HasParent)  parent = parent.Parent;

                var scene = parent as Scene;
                if (scene != null)  scene.AddObject(o);
            }
        }

        public void Remove(params Object3D[] objects) 
        {
            foreach(var o in objects)
            {
                o.Parent = null;
			//object.dispatchEvent( { type: 'removed' } );

                children.Remove(o);

                // remove from scene
			var parent = this;
			while (parent.HasParent) parent = parent.Parent;

                var scene = parent as Scene;
			if ( scene != null) scene.RemoveObject(o);
            }
        }

        public void UpdateMatrix() 
        {
            matrix = Matrix4.Compose(Position, Quaternion, Scale );
            matrixWorldNeedsUpdate = true;
        }

        public virtual void UpdateMatrixWorld(bool force = false)
        {
            if (matrixAutoUpdate) UpdateMatrix();

            if (matrixWorldNeedsUpdate || force)
            {
                if (!HasParent) matrixWorld = matrix;
                else matrixWorld.MultiplyMatrices(Parent.matrixWorld, matrix);

                matrixWorldNeedsUpdate = false;
                force = true;
            }

            // update children
            foreach (var c in children) c.UpdateMatrixWorld(force);
        }

        public void Traverse(Action<Object3D> callback)
        {
            callback(this);
            foreach (var c in children) c.Traverse(callback);
        }

        public void TraverseVisible(Action<Object3D> callback ) 
        {
            if(!IsVisible) return;

            callback( this );
            foreach (var c in children) c.TraverseVisible(callback);
        }

        virtual internal void Raycast(Raycaster raycaster, IList<IntersectionInfo> intersectionList) { }
    }
}

