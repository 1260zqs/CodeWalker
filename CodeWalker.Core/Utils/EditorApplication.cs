using System;
using System.Collections.Generic;
using System.Text;

public static class EditorApplication
{
    private static readonly DateTime _startupTime = DateTime.Now;

    public static event Action update;
    public static double timeSinceStartup
    {
        get => (DateTime.Now - _startupTime).TotalSeconds;
    }

    public static void Startup()
    {
    }

    public static void Update()
    {
        if (update != null)
        {
            update();
        }
    }
}
