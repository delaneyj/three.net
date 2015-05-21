using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Three.Net.Math
{
    public struct Euler : IEquatable<Euler>
    {
        public enum OrderMode
        {
            XYZ,
            YXZ,
            ZYX,
            YZX,
            XZY,
            ZXY
        }

        public const OrderMode DefaultOrder = OrderMode.XYZ;
        public static Euler Default = new Euler(0, 0, 0, OrderMode.XYZ);

        public float x, y, z;
        public OrderMode order;

        public Euler(float x, float y, float z, OrderMode order = DefaultOrder)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.order = order;
        }

        public static bool operator ==(Euler left, Euler right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Euler left, Euler right)
        {
            return !left.Equals(right);
        }

        public override bool Equals(object other)
        {
            if (other is Euler == false) return false;
            return this == (Euler)other;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ order.GetHashCode();
        }

        public bool Equals(Euler other)
        {
            return x == other.x && y == other.y && z == other.z && order == other.order;
        }

        internal static Euler From(Quaternion q, OrderMode order = DefaultOrder)
        {
            // q is assumed to be normalized
            // http://www.mathworks.com/matlabcentral/fileexchange/20696-function-to-convert-between-dcm-euler-angles-quaternions-and-euler-vectors/content/SpinCalc.m

            var sqx = q.x * q.x;
            var sqy = q.y * q.y;
            var sqz = q.z * q.z;
            var sqw = q.w * q.w;

            float x, y, z;

            switch (order)
            {
                case OrderMode.XYZ:
                    x = Mathf.Atan2(2 * (q.x * q.w - q.y * q.z), (sqw - sqx - sqy + sqz));
                    y = Mathf.Asin(Mathf.Clamp(2 * (q.x * q.z + q.y * q.w), -1, 1));
                    z = Mathf.Atan2(2 * (q.z * q.w - q.x * q.y), (sqw + sqx - sqy - sqz));
                    break;
                case OrderMode.YXZ:

                    x = Mathf.Asin(Mathf.Clamp(2 * (q.x * q.w - q.y * q.z), -1, 1));
                    y = Mathf.Atan2(2 * (q.x * q.z + q.y * q.w), (sqw - sqx - sqy + sqz));
                    z = Mathf.Atan2(2 * (q.x * q.y + q.z * q.w), (sqw - sqx + sqy - sqz));
                    break;
                case OrderMode.ZXY:
                    x = Mathf.Asin(Mathf.Clamp(2 * (q.x * q.w + q.y * q.z), -1, 1));
                    y = Mathf.Atan2(2 * (q.y * q.w - q.z * q.x), (sqw - sqx - sqy + sqz));
                    z = Mathf.Atan2(2 * (q.z * q.w - q.x * q.y), (sqw - sqx + sqy - sqz));
                    break;
                case OrderMode.ZYX:
                    x = Mathf.Atan2(2 * (q.x * q.w + q.z * q.y), (sqw - sqx - sqy + sqz));
                    y = Mathf.Asin(Mathf.Clamp(2 * (q.y * q.w - q.x * q.z), -1, 1));
                    z = Mathf.Atan2(2 * (q.x * q.y + q.z * q.w), (sqw + sqx - sqy - sqz));
                    break;
                case OrderMode.YZX:

                    x = Mathf.Atan2(2 * (q.x * q.w - q.z * q.y), (sqw - sqx + sqy - sqz));
                    y = Mathf.Atan2(2 * (q.y * q.w - q.x * q.z), (sqw + sqx - sqy - sqz));
                    z = Mathf.Asin(Mathf.Clamp(2 * (q.x * q.y + q.z * q.w), -1, 1));
                    break;
                case OrderMode.XZY:
                    x = Mathf.Atan2(2 * (q.x * q.w + q.y * q.z), (sqw - sqx + sqy - sqz));
                    y = Mathf.Atan2(2 * (q.x * q.z + q.y * q.w), (sqw + sqx - sqy - sqz));
                    z = Mathf.Asin(Mathf.Clamp(2 * (q.z * q.w - q.x * q.y), -1, 1));
                    break;
                default: throw new NotSupportedException(string.Format("Given unsupported order: {0}.", order));
            }
            return new Euler(x, y, z, order);
        }
    }
}
