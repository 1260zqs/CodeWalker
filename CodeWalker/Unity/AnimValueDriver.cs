using System;

public class AnimValueDriver
{
    private readonly DateTime startupTime;

    public double timeSinceStartup
    {
        get => (DateTime.Now - startupTime).TotalSeconds;
    }

    public AnimValueDriver()
    {
        startupTime = DateTime.Now;
    }

    public event Action update;

    public T Get<T, TValue>() where T : BaseAnimValue<TValue>
    {
        var animValue = Activator.CreateInstance<T>();
        animValue.driver = this;
        return animValue;
    }

    public T Get<T, TValue>(TValue value) where T : BaseAnimValue<TValue>
    {
        var animValue = Activator.CreateInstance<T>();
        animValue.value = value;
        animValue.driver = this;
        return animValue;
    }

    public void Update()
    {
        if (update != null)
        {
            update();
        }
    }
}