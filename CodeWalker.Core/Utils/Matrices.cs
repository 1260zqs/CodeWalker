using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeWalker
{
    public static class MatrixExtensions
    {
        public static Vector3 MultiplyW(this Matrix m, Vector3 v)
        {
            var x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            var y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            var z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            var w = (((m.M14 * v.X) + (m.M24 * v.Y)) + (m.M34 * v.Z)) + m.M44;
            var iw = 1.0f / Math.Abs(w);
            return new Vector3(x * iw, y * iw, z * iw);
        }

        public static Vector3 Multiply(this Matrix m, Vector3 v)
        {
            var x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            var y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            var z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            return new Vector3(x, y, z);
            //this quick mul ignores W...
        }

        public static Vector3 MultiplyRot(this Matrix m, Vector3 v)
        {
            var x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)); // + m.M41;
            var y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)); // + m.M42;
            var z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)); // + m.M43;
            return new Vector3(x, y, z);
            //this quick mul ignores W and translation...
        }

        public static Vector4 Multiply(this Matrix m, Vector4 v)
        {
            var x = (((m.M11 * v.X) + (m.M21 * v.Y)) + (m.M31 * v.Z)) + m.M41;
            var y = (((m.M12 * v.X) + (m.M22 * v.Y)) + (m.M32 * v.Z)) + m.M42;
            var z = (((m.M13 * v.X) + (m.M23 * v.Y)) + (m.M33 * v.Z)) + m.M43;
            var w = (((m.M14 * v.X) + (m.M24 * v.Y)) + (m.M34 * v.Z)) + m.M44;
            return new Vector4(x, y, z, w);
        }

        public static Quaternion ToQuaternion(this Matrix m)
        {
            m.TranslationVector = Vector3.Zero;
            return Quaternion.RotationMatrix(m);
        }
    }
}