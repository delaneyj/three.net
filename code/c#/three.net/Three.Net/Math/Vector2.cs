using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public struct Vector2 : IEquatable<Vector2>
    {
        public static Vector2 Zero = new Vector2(0, 0);
        public static Vector2 Half = new Vector2(0.5f,0.5f);
        public static Vector2 One = new Vector2(1, 1);
        public static Vector2 UnitX = new Vector2(1,0);
        public static Vector2 UnitY = new Vector2(0,1);
        public static Vector2 UnitNegativeX = new Vector2(-1, 0);
        public static Vector2 UnitNegativeY = new Vector2(0, -1);

        public float x, y;

        public Vector2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public float this[int key]
        {
            get
            {
                switch (key)
                {

                    case 0: return x;
                    case 1: return y;
                    default: throw new Exception("index is out of range: " + key);

                }
            }
        }

        public void Add(Vector2 v)
        {
            x += v.x;
            y += v.y;
        }

        public void AddVectors(Vector2 a, Vector2 b)
        {
            x = a.x + b.x;
            y = a.y + b.y;
        }

        public void Add(float s)
        {
            x += s;
            y += s;
        }

        public void Subtract(Vector2 v)
        {
            x -= v.x;
            y -= v.y;
        }

        public void SubtractVectors(Vector2 a, Vector2 b)
        {
            x = a.x - b.x;
            y = a.y - b.y;
        }

        public void Multiply(Vector2 v)
        {
            x *= v.x;
            y *= v.y;
        }

        public void Multiply(float s)
        {
            x *= s;
            y *= s;
        }

        public void Divide(Vector2 v)
        {
            x /= v.x;
            y /= v.y;
        }

        public void Divide(float scalar)
        {
            if (scalar != 0)
            {

                var invScalar = 1 / scalar;
                x *= invScalar;
                y *= invScalar;
            }
            else
            {
                x = 0;
                y = 0;
            }
        }

        public void Min(Vector2 v)
        {
            if (this.x > v.x) this.x = v.x;
            if (this.y > v.y) this.y = v.y;
        }

        public void Max(Vector2 v)
        {
            if (this.x < v.x) this.x = v.x;
            if (this.y < v.y) this.y = v.y;
        }

        public void Clamp(Vector2 min, Vector2 max)
        {
            // This function assumes min < max, if this assumption isn't true it will not operate correctly
            if (this.x < min.x) this.x = min.x;
            else if (this.x > max.x) this.x = max.x;
            if (this.y < min.y) this.y = min.y;
            else if (this.y > max.y) this.y = max.y;

        }

        public void Clamp(float minVal, float maxVal)
        {
            var min = new Vector2(minVal, minVal);
            var max = new Vector2(maxVal, maxVal);
            Clamp(min, max);
        }

        public void Floor()
        {
            x = Mathf.Floor(x);
            y = Mathf.Floor(y);
        }

        public void Ceiling()
        {
            x = Mathf.Ceiling(x);
            y = Mathf.Ceiling(y);
        }

        public void Round()
        {
            x = Mathf.Round(x);
            y = Mathf.Round(y);
        }

        public void RoundToZero()
        {
            x = (x < 0) ? Mathf.Ceiling(x) : Mathf.Floor(x);
            y = (y < 0) ? Mathf.Ceiling(y) : Mathf.Floor(y);
        }

        public void Negate()
        {
            x = -x;
            y = -y;
        }

        public float Dot(Vector2 v)
        {
            return x * v.x + y * v.y;
        }

        public float LengthSquared()
        {
            return x * x + y * y;
        }

        public float Length()
        {
            return Mathf.Sqrt(Length());
        }

        public void Normalize()
        {
            Divide(Length());
        }

        public float DistanceTo(Vector2 v)
        {
            return Mathf.Sqrt(DistanceToSquared(v));
        }

        public float DistanceToSquared(Vector2 v)
        {
            float dx = this.x - v.x, dy = this.y - v.y;
            return dx * dx + dy * dy;
        }

        public void SetLength(float l)
        {
            var oldLength = Length();

            if (oldLength != 0 && l != oldLength) Multiply(l / oldLength);
        }

        public void Lerp(Vector2 v, float alpha)
        {
            x += (v.x - x) * alpha;
            y += (v.y - y) * alpha;
        }

        public bool Equals(Vector2 v)
        {
            return ((v.x == x) && (v.y == y));
        }

        public void FromArray(float[] array)
        {
            x = array[0];
            y = array[1];
        }

        public float[] ToArray()
        {
            return new float[] { x, y };
        }

        public override string ToString()
        {
            return string.Format("{0},{1}", x, y);
        }
    }
}
