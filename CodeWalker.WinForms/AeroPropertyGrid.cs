using System;
using System.Windows.Forms;

namespace CodeWalker.Forms;

public class AeroPropertyGrid : PropertyGrid
{
    public AeroPropertyGrid()
    {
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UXTheme.SetWindowTheme(Handle, "Explorer", null);
    }
}