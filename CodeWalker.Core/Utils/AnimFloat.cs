using System;

/// <summary>
///   <para>An animated float value.</para>
/// </summary>
public class AnimFloat : BaseAnimValue<float>
{
    private float m_Value;

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    public AnimFloat(float value) : base(value)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimFloat(float value, Action callback) : base(value, callback)
    {
    }

    /// <summary>
    ///   <para>Type specific implementation of BaseAnimValue_1.GetValue.</para>
    /// </summary>
    /// <returns>
    ///   <para>Current Value.</para>
    /// </returns>
    protected override float GetValue()
    {
        m_Value = Mathf.Lerp(start, target, lerpPosition);
        return m_Value;
    }
}
