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

    public static float Ceil(float f) => (float)Math.Ceiling((double)f);
    public static float Floor(float f) => (float)Math.Floor((double)f);
    public static float Round(float f) => (float)Math.Round((double)f);
    public static int CeilToInt(float f) => (int)Math.Ceiling((double)f);
    public static int FloorToInt(float f) => (int)Math.Floor((double)f);
    public static int RoundToInt(float f) => (int)Math.Round((double)f);

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
}
