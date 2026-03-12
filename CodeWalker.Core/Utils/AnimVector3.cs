using System;
using System.Text;
using System.Collections.Generic;
using SharpDX;

public class AnimVector3 : BaseAnimValue<Vector3>
{
    private Vector3 m_Value;

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimVector3() : base(Vector3.Zero)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimVector3(Vector3 value) : base(value)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimVector3(Vector3 value, Action callback) : base(value, callback)
    {
    }

    /// <summary>
    ///   <para>Type specific implementation of BaseAnimValue_1.GetValue.</para>
    /// </summary>
    /// <returns>
    ///   <para>Current Value.</para>
    /// </returns>
    protected override Vector3 GetValue()
    {
        this.m_Value = Vector3.Lerp(base.start, base.target, base.lerpPosition);
        return this.m_Value;
    }
}