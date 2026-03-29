using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CodeWalker;

class TexModTreeView : TreeView
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    static extern int SetWindowTheme(IntPtr hWnd, string app, string id);

    public TexModTreeView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        SetWindowTheme(Handle, "Explorer", null);
        base.OnHandleCreated(e);
    }
}