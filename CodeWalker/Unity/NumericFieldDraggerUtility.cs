// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using SharpDX;

namespace UnityEditor;

class NumericFieldDraggerUtility
{
    internal static float Acceleration(bool shiftPressed, bool altPressed)
    {
        return (shiftPressed ? 4 : 1) * (altPressed ? .25f : 1);
    }

    static bool s_UseYSign = false;

    internal static float NiceDelta(Vector2 deviceDelta, float acceleration)
    {
        deviceDelta.Y = -deviceDelta.Y;

        if (Mathf.Abs(Mathf.Abs(deviceDelta.X) - Mathf.Abs(deviceDelta.Y)) / Mathf.Max(Mathf.Abs(deviceDelta.X), Mathf.Abs(deviceDelta.Y)) > .1f)
        {
            if (Mathf.Abs(deviceDelta.X) > Mathf.Abs(deviceDelta.Y))
                s_UseYSign = false;
            else
                s_UseYSign = true;
        }

        if (s_UseYSign)
            return Mathf.Sign(deviceDelta.Y) * deviceDelta.Length() * acceleration;
        else
            return Mathf.Sign(deviceDelta.X) * deviceDelta.Length() * acceleration;
    }

    const float kDragSensitivity = .03f;

    internal static double CalculateFloatDragSensitivity(double value)
    {
        if (double.IsInfinity(value) || double.IsNaN(value))
        {
            return 0.0;
        }
        return Math.Max(1, Math.Pow(Math.Abs(value), 0.5)) * kDragSensitivity;
    }

    internal static long CalculateIntDragSensitivity(long value)
    {
        return (long)Math.Max(1, Math.Pow(Math.Abs((double)value), 0.5) * kDragSensitivity);
    }
}