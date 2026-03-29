using System;
using System.Collections.Generic;
using System.Text;

public struct Mathf
{
    public static volatile float FloatMinNormal = 1.1754944E-38f;
    public static volatile float FloatMinDenormal = float.Epsilon;
    public static bool IsFlushToZeroEnabled = FloatMinDenormal == 0f;

    public const float PI = 3.1415927f;
    public const float Rad2Deg = 57.29578f;
    public const float Deg2Rad = 0.017453292f;
    public const float Infinity = float.PositiveInfinity;
    public const float NegativeInfinity = float.NegativeInfinity;
    public static readonly float Epsilon = IsFlushToZeroEnabled ? FloatMinNormal : FloatMinDenormal;

    public static float Pow(float f, float p) => (float)Math.Pow(f, p);
    public static float Sin(float f) => (float)Math.Sin(f);
    public static float Cos(float f) => (float)Math.Cos(f);
    public static float Ceil(float f) => (float)Math.Ceiling((double)f);
    public static float Floor(float f) => (float)Math.Floor((double)f);
    public static float Round(float f) => (float)Math.Round((double)f);
    public static int CeilToInt(float f) => (int)Math.Ceiling((double)f);
    public static int FloorToInt(float f) => (int)Math.Floor((double)f);
    public static int RoundToInt(float f) => (int)Math.Round((double)f);
    public static float Sign(float f) => f >= 0F ? 1F : -1F;

    public static float Clamp(float value, float min, float max)
    {
        if ((double)value < (double)min)
            value = min;
        else if ((double)value > (double)max)
            value = max;
        return value;
    }

    public static int Clamp(int value, int min, int max)
    {
        if (value < min)
            value = min;
        else if (value > max)
            value = max;
        return value;
    }

    public static float Clamp01(float value)
    {
        if ((double)value < 0.0) return 0.0f;
        return (double)value > 1.0 ? 1f : value;
    }

    public static float Lerp(float a, float b, float t) => a + (b - a) * Mathf.Clamp01(t);

    public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;

    public static float LerpAngle(float a, float b, float t)
    {
        float num = Mathf.Repeat(b - a, 360f);
        if ((double)num > 180.0) num -= 360f;
        return a + num * Mathf.Clamp01(t);
    }

    public static float Repeat(float t, float length)
    {
        return Mathf.Clamp(t - Mathf.Floor(t / length) * length, 0.0f, length);
    }

    public static float InverseLerp(float a, float b, float value)
    {
        return (double)a != (double)b ? Mathf.Clamp01((float)(((double)value - (double)a) / ((double)b - (double)a))) : 0.0f;
    }

    public static float Abs(float f) => Math.Abs(f);
    public static float Min(float a, float b) => a < b ? a : b;
    public static float Max(float a, float b) => a > b ? a : b;

    public static bool Approximately(float a, float b)
    {
        // If a or b is zero, compare that the other is less or equal to epsilon.
        // If neither a or b are 0, then find an epsilon that is good for
        // comparing numbers at the maximum magnitude of a and b.
        // Floating points have about 7 significant digits, so
        // 1.000001f can be represented while 1.0000001f is rounded to zero,
        // thus we could use an epsilon of 0.000001f for comparing values close to 1.
        // We multiply this epsilon by the biggest magnitude of a and b.
        return Abs(b - a) < Max(0.000001f * Max(Abs(a), Abs(b)), Epsilon * 8);
    }
}