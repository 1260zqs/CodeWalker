using System;

/// <summary>
///   <para>Abstract base class for Animated Values.</para>
/// </summary>
public abstract class BaseAnimValue<T>
{
    private T m_Start;
    private T m_Target;
    private double m_LastTime;
    private double m_LerpPosition = 1.0;
    private float m_Speed;
    public float speed = 2f;
    public event Action valueChanged;
    private bool m_Animating;
    public AnimValueDriver driver;

    public bool isAnimating => m_Animating;

    protected T start => m_Start;

    public T target
    {
        get => m_Target;
        set
        {
            if (!m_Target.Equals(value))
            {
                BeginAnimating(value, this.value);
            }
        }
    }

    public T value
    {
        get => lerpPosition >= 1f ? target : GetValue();
        set => StopAnim(value);
    }

    protected BaseAnimValue(T value)
    {
        m_Start = value;
        m_Target = value;
    }

    protected BaseAnimValue(T value, Action callback)
    {
        m_Start = value;
        m_Target = value;
        valueChanged += callback;
    }

    protected abstract T GetValue();

    private static T2 Clamp<T2>(T2 val, T2 min, T2 max) where T2 : IComparable<T2>
    {
        if (val.CompareTo(min) < 0) return min;
        return val.CompareTo(max) > 0 ? max : val;
    }

    protected void BeginAnimating(T newTarget, T newStart)
    {
        BeginAnimating(newTarget, newStart, speed);
    }

    private void BeginAnimating(T newTarget, T newStart, float animationSpeed)
    {
        m_Speed = animationSpeed;
        m_Start = newStart;
        m_Target = newTarget;
        if (!m_Animating)
        {
            driver.update += Update;
        }
        m_Animating = true;
        m_LastTime = driver.timeSinceStartup;
        m_LerpPosition = 0.0;
    }

    private void Update()
    {
        if (m_Animating)
        {
            UpdateLerpPosition();
            if (lerpPosition >= 1f)
            {
                m_Animating = false;
                driver.update += Update;
            }
            valueChanged?.Invoke();
        }
    }

    protected float lerpPosition
    {
        get
        {
            var num = 1.0 - m_LerpPosition;
            return (float)(1.0 - num * num * num * num);
        }
    }

    private void UpdateLerpPosition()
    {
        var timeSinceStartup = driver.timeSinceStartup;
        var elapsed = timeSinceStartup - m_LastTime;
        m_LerpPosition = Clamp(m_LerpPosition + elapsed * m_Speed, 0.0, 1.0);
        m_LastTime = timeSinceStartup;
    }

    protected void StopAnim(T newValue)
    {
        var notify = false;
        if ((!newValue.Equals(GetValue()) || m_LerpPosition < 1.0) && valueChanged != null)
        {
            notify = true;
        }
        m_Target = newValue;
        m_Start = newValue;
        m_LerpPosition = 1.0;
        m_Animating = false;
        if (notify)
        {
            valueChanged.Invoke();
        }
    }

    internal void SetTarget(T newTarget, float animationSpeed)
    {
        if (!m_Target.Equals(newTarget))
        {
            BeginAnimating(newTarget, value, animationSpeed);
        }
    }
}