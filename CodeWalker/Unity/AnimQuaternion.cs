using System;
using SharpDX;

/// <summary>
///   <para>An animated Quaternion value.</para>
/// </summary>
public class AnimQuaternion : BaseAnimValue<Quaternion>
{
    private Quaternion m_Value;

    public AnimQuaternion() : base(Quaternion.Identity)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimQuaternion(Quaternion value) : base(value)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimQuaternion(Quaternion value, Action callback) : base(value, callback)
    {
    }

    /// <summary>
    ///   <para>Type specific implementation of BaseAnimValue_1.GetValue.</para>
    /// </summary>
    /// <returns>
    ///   <para>Current Value.</para>
    /// </returns>
    protected override Quaternion GetValue()
    {
        m_Value = Quaternion.Slerp(start, target, lerpPosition);
        return m_Value;
    }
}