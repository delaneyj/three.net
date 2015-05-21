
namespace Three.Net.Math
{
    public struct Ray
    {
        internal Vector3 origin;
        internal Vector3 direction;

        public Ray(Vector3? origin, Vector3? direction)
        {
            this.origin = origin.HasValue ? origin.Value : Vector3.Zero;
            this.direction = direction.HasValue ? direction.Value : Vector3.Zero;
        }

        public Vector3 At(float t) 
        {
		var result = direction;
            result.Multiply(t);
            result.Add(origin);
            return result;
        }

        public void Recast(float t) 
        {
            origin = At(t);
		}

        public Vector3 ClosestPointToPoint(Vector3 point)
        {
            var result = Vector3.SubtractVectors( point, origin );
		var directionDistance = result.Dot( direction );

		if ( directionDistance < 0 )  return origin;

            result = direction;
            result.Multiply(directionDistance);
            result.Add(origin);
            return result;
        }

        public float DistanceToPoint(Vector3 point)
        {
            var directionDistance = Vector3.SubtractVectors( point, origin ).Dot(direction);

			// point behind the ray
			if ( directionDistance < 0 ) return origin.DistanceTo( point );
            
            var v1 = direction;
            v1.Multiply( directionDistance );
            v1.Add(origin);
			return v1.DistanceTo( point );
        }

        public float DistanceSquareToSegment(Vector3 v0,Vector3 v1, ref Vector3 optionalPointOnRay, ref Vector3 optionalPointOnSegment)
        {
		// from http://www.geometrictools.com/LibMathematics/Distance/Wm5DistRay3Segment3.cpp
		// It returns the min distance between the ray and the segment
		// defined by v0 and v1
		// It can also set two optional targets :
		// - The closest point on the ray
		// - The closest point on the segment

		var segCenter = v0;
            segCenter.Add( v1 );
            segCenter.Multiply( 0.5f );

		var segDir = v1;
            segDir.Subtract(v0);
            segDir.Normalize();

		var segExtent = v0.DistanceTo( v1 ) / 2;
		var diff = origin;
            diff.Subtract(segCenter );

		var a01 = - direction.Dot( segDir );
		var b0 = diff.Dot( direction );
		var b1 = - diff.Dot( segDir );
		var c = diff.LengthSquared();
		var det = Mathf.Abs( 1 - a01 * a01 );
		var sqrDist = 0f;
            var s0 = 0f;
            var s1 = 0f;

            if ( det >= 0 ) {
			// The ray and segment are not parallel.
			s0 = a01 * b1 - b0;
			s1 = a01 * b0 - b1;
			var extDet = segExtent * det;

			if ( s0 >= 0 ) {

				if ( s1 >= - extDet ) {

					if ( s1 <= extDet ) {

						// region 0
						// Minimum at interior points of ray and segment.

						var invDet = 1 / det;
						s0 *= invDet;
						s1 *= invDet;
						sqrDist = s0 * ( s0 + a01 * s1 + 2 * b0 ) + s1 * ( a01 * s0 + s1 + 2 * b1 ) + c;

					} else {

						// region 1

						s1 = segExtent;
						s0 = Mathf.Max( 0, - ( a01 * s1 + b0 ) );
						sqrDist = - s0 * s0 + s1 * ( s1 + 2 * b1 ) + c;

					}

				} else {

					// region 5

					s1 = - segExtent;
					s0 = Mathf.Max( 0, - ( a01 * s1 + b0 ) );
					sqrDist = - s0 * s0 + s1 * ( s1 + 2 * b1 ) + c;

				}

			} else {

				if ( s1 <= - extDet ) {

					// region 4

					s0 = Mathf.Max( 0, - ( - a01 * segExtent + b0 ) );
					s1 = ( s0 > 0 ) ? - segExtent : Mathf.Min(Mathf.Max( - segExtent, - b1 ), segExtent );
					sqrDist = - s0 * s0 + s1 * ( s1 + 2 * b1 ) + c;

				} else if ( s1 <= extDet ) {

					// region 3

					s0 = 0;
					s1 = Mathf.Min(Mathf.Max( - segExtent, - b1 ), segExtent );
					sqrDist = s1 * ( s1 + 2 * b1 ) + c;

				} else {

					// region 2

					s0 = Mathf.Max( 0, - ( a01 * segExtent + b0 ) );
					s1 = ( s0 > 0 ) ? segExtent : Mathf.Min( Mathf.Max( - segExtent, - b1 ), segExtent );
					sqrDist = - s0 * s0 + s1 * ( s1 + 2 * b1 ) + c;

				}

			}

		} else {

			// Ray and segment are parallel.

			s1 = ( a01 > 0 ) ? - segExtent : segExtent;
			s0 = Mathf.Max( 0, - ( a01 * s1 + b0 ) );
			sqrDist = - s0 * s0 + s1 * ( s1 + 2 * b1 ) + c;

		}

		optionalPointOnRay = direction;
            optionalPointOnRay.Multiply(s0);
            optionalPointOnRay.Add(origin);

            optionalPointOnSegment = segDir;
            optionalPointOnSegment.Multiply( s1 );
            optionalPointOnSegment.Add( segCenter );

		return sqrDist;

        }

        public bool IsIntersectionSphere(Sphere sphere ) 
        {
            var distance = DistanceToPoint( sphere.Center);
            return  distance <= sphere.Radius;
        }

        public Vector3? IntersectSphere(Sphere sphere) 
        {
		// from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-sphere-intersection/
            var v1 = Vector3.SubtractVectors( sphere.Center, origin );
            var tca = v1.Dot(direction );
            var d2 = v1.Dot(v1) - tca * tca;
            var radius2 = sphere.Radius * sphere.Radius;
            if ( d2 > radius2 ) return null;

			var thc = Mathf.Sqrt( radius2 - d2 );
			var t0 = tca - thc; // t0 = first intersect point - entrance on front of sphere
			var t1 = tca + thc; // t1 = second intersect point - exit point on back of sphere
			if ( t0 < 0 && t1 < 0 ) return null; // test to see if both t0 and t1 are behind the ray - if so, return null

			// test to see if t0 is behind the ray:
			// if it is, the ray is inside the sphere, so return the second exit point scaled by t1,
			// in order to always return an intersect point that is in front of the ray.
			if ( t0 < 0 ) return At( t1);

			// else t0 is in front of the ray, so return the first collision point scaled by t0 
			return At( t0);
        }

        public bool IsIntersectionPlane(Plane plane ) 
        {
		// check if the ray lies on the plane first
		var distToPoint = plane.DistanceToPoint(origin );

		if ( distToPoint == 0 ) return true;
            var denominator = plane.Normal.Dot(direction );

		if ( denominator * distToPoint < 0 ) return true;
            
		return false; // ray origin is behind the plane (and is pointing behind it)
        }

        public float? DistanceToPlane(Plane plane ) 
        {
		var denominator = plane.Normal.Dot(direction );
		if ( denominator == 0 ) 
        {
			// line is coplanar, return origin
			if ( plane.DistanceToPoint(origin ) == 0 ) return 0;
            
			// Null is preferable to undefined since undefined means.... it is undefined
			return null;

		}

		var t = - (origin.Dot( plane.Normal ) + plane.Constant ) / denominator;
		if(t >= 0) return t;
            return null; // Return if the ray never intersects the plane
        }

        public float? IntersectPlane(Plane plane) 
        {
            return DistanceToPlane( plane );
        }

	    public bool IsIntersectionBox(Box3 box) 
        {
            return IntersectBox( box) != null;
		}

        public Vector3? IntersectBox(Box3 box)
        {
		// http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
            var invdirx = 1 / direction.x;
            var invdiry = 1 / direction.y;
            var invdirz = 1 / direction.z;
            float tmin,tmax,tymin, tymax, tzmin,tzmax;

		if ( invdirx >= 0 ) 
        {

			tmin = ( box.min.x - origin.x ) * invdirx;
			tmax = ( box.max.x - origin.x ) * invdirx;

		} else {

			tmin = ( box.max.x - origin.x ) * invdirx;
			tmax = ( box.min.x - origin.x ) * invdirx;
		}

		if ( invdiry >= 0 ) {

			tymin = ( box.min.y - origin.y ) * invdiry;
			tymax = ( box.max.y - origin.y ) * invdiry;

		} else {

			tymin = ( box.max.y - origin.y ) * invdiry;
			tymax = ( box.min.y - origin.y ) * invdiry;
		}

		if ( ( tmin > tymax ) || ( tymin > tmax ) ) return null;

		// These lines also handle the case where tmin or tmax is NaN
		// (result of 0 * Infinity). x !== x returns true if x is NaN
		if ( tymin > tmin || tymin != tmin ) tmin = tymin;
		if ( tymax < tmax || tymax != tmax ) tmax = tymax;

		if ( invdirz >= 0 ) {
			tzmin = ( box.min.z - origin.z ) * invdirz;
			tzmax = ( box.max.z - origin.z ) * invdirz;

		} else {

			tzmin = ( box.max.z - origin.z ) * invdirz;
			tzmax = ( box.min.z - origin.z ) * invdirz;
		}

		if ( ( tmin > tzmax ) || ( tzmin > tmax ) ) return null;

		if ( tzmin > tmin || tzmin != tmin ) tmin = tzmin;
		if ( tzmax < tmax || tzmax != tmax ) tmax = tzmax;

		//return point closest to the ray (positive side)

		if ( tmax < 0 ) return null;

		return At( tmin >= 0 ? tmin : tmax);
        }

        public Vector3? IntersectTriangle(Vector3 a, Vector3 b, Vector3 c, bool backfaceCulling) 
        {
            // from http://www.geometrictools.com/LibMathematics/Intersection/Wm5IntrRay3Triangle3.cpp
		// Compute the offset origin, edges, and normal.
		var diff = Vector3.Zero;
		var edge1 = Vector3.SubtractVectors( b, a );
		var edge2 = Vector3.SubtractVectors( c, a );
		var normal = Vector3.CrossVectors( edge1, edge2 );

			// Solve Q + t*D = b1*E1 + b2*E2 (Q = kDiff, D = ray direction,
			// E1 = kEdge1, E2 = kEdge2, N = Cross(E1,E2)) by
			//   |Dot(D,N)|*b1 = sign(Dot(D,N))*Dot(D,Cross(Q,E2))
			//   |Dot(D,N)|*b2 = sign(Dot(D,N))*Dot(D,Cross(E1,Q))
			//   |Dot(D,N)|*t = -sign(Dot(D,N))*Dot(Q,N)
			var DdN = direction.Dot( normal );
			int sign;

			if ( DdN > 0 ) {

				if ( backfaceCulling ) return null;
				sign = 1;

			} else if ( DdN < 0 ) {

				sign = - 1;
				DdN = - DdN;

			} else {

				return null;

			}

			diff = Vector3.SubtractVectors(origin, a );
            edge2 = Vector3.CrossVectors( diff, edge2 );
			var DdQxE2 = sign * direction.Dot(edge2);

			if ( DdQxE2 < 0 )  return null; // b1 < 0, no intersection
            
            edge1.Cross( diff );
			var DdE1xQ = sign * direction.Dot( edge1 );

			if ( DdE1xQ < 0 ) return null; // b2 < 0, no intersection
			if ( DdQxE2 + DdE1xQ > DdN ) return null; // b1+b2 > 1, no intersection

			var QdN = - sign * diff.Dot( normal ); // Line intersects triangle, check if ray does.

			if ( QdN < 0 )  return null;// t < 0, no intersection

			// Ray intersects triangle.
			return At( QdN / DdN);
        }

        public void Apply(Matrix4 m)
        {
            direction.Add(origin);
            direction.Apply(m);
            origin.Apply(m);
            direction.Subtract(origin);
            direction.Normalize();
        }
    }
}
