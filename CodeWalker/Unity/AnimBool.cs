using System;

/// <summary>
///   <para>Lerp from 0 - 1.</para>
/// </summary>
[Serializable]
public class AnimBool : BaseAnimValue<bool>
{
    private float m_Value;

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimBool() : base(false)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimBool(bool value) : base(value)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimBool(Action callback) : base(false, callback)
    {
    }

    /// <summary>
    ///   <para>Constructor.</para>
    /// </summary>
    /// <param name="value">Start Value.</param>
    /// <param name="callback"></param>
    public AnimBool(bool value, Action callback) : base(value, callback)
    {
    }

    /// <summary>
    ///   <para>Retuns the float value of the tween.</para>
    /// </summary>
    public float faded
    {
        get
        {
            GetValue();
            return m_Value;
        }
    }

    /// <summary>
    ///   <para>Type specific implementation of BaseAnimValue_1.GetValue.</para>
    /// </summary>
    /// <returns>
    ///   <para>Current value.</para>
    /// </returns>
    protected override bool GetValue()
    {
        var val = target ? 0f : 1f;
        m_Value = Mathf.Lerp(val, 1f - val, lerpPosition);
        return m_Value > 0.5f;
    }

    /// <summary>
    ///   <para>Returns a value between from and to depending on the current value of the bools animation.</para>
    /// </summary>
    /// <param name="from">Value to lerp from.</param>
    /// <param name="to">Value to lerp to.</param>
    public float Fade(float from, float to)
    {
        return Mathf.Lerp(from, to, faded);
    }
}