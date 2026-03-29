using System;
using SharpDX;

static class UnityMathExtensions
{
    public static float SqrMagnitude(this Vector3 v)
    {
        return v.X * v.X + v.Y * v.Y + v.Z * v.Z;
    }

    public static Quaternion AngleAxis(float angle, Vector3 axis)
    {
        if (Mathf.Approximately(axis.SqrMagnitude(), 0))
            return Quaternion.Identity;

        axis.Normalize();

        var rad = angle * Mathf.Deg2Rad;
        var half = rad * 0.5f;

        var sin = Mathf.Sin(half);
        var cos = Mathf.Cos(half);

        return new Quaternion(
            axis.X * sin,
            axis.Y * sin,
            axis.Z * sin,
            cos
        );
    }

    public static Vector3 Multiply(in this Quaternion rotation, Vector3 point)
    {
        var x = rotation.X * 2F;
        var y = rotation.Y * 2F;
        var z = rotation.Z * 2F;
        var xx = rotation.X * x;
        var yy = rotation.Y * y;
        var zz = rotation.Z * z;
        var xy = rotation.X * y;
        var xz = rotation.X * z;
        var yz = rotation.Y * z;
        var wx = rotation.W * x;
        var wy = rotation.W * y;
        var wz = rotation.W * z;

        Vector3 res;
        res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
        res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
        res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
        return res;
    }
#if false
    extension(Vector3)
    {
        public static Vector3 forward => Vector3.ForwardLH;
    }

    extension(Vector3 v)
    {
        public float sqrMagnitude => v.X * v.X + v.Y * v.Y + v.Z * v.Z;
        public Vector3 normalized => Vector3.Normalize(v);
    }

    extension(Quaternion q)
    {
    }

    extension(Quaternion)
    {
        public static Quaternion identity => Quaternion.Identity;


    }

    public static Vector3 Multiply(in this Quaternion rotation, Vector3 point)
    {
        var x = rotation.X * 2F;
        var y = rotation.Y * 2F;
        var z = rotation.Z * 2F;
        var xx = rotation.X * x;
        var yy = rotation.Y * y;
        var zz = rotation.Z * z;
        var xy = rotation.X * y;
        var xz = rotation.X * z;
        var yz = rotation.Y * z;
        var wx = rotation.W * x;
        var wy = rotation.W * y;
        var wz = rotation.W * z;

        Vector3 res;
        res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
        res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
        res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
        return res;
    }
#endif
}