using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeWalker.Forms;

public class AeroTreeView : TreeView
{
    public AeroTreeView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UXTheme.SetWindowTheme(Handle, "Explorer", null);
    }
}