using System.Windows.Forms;

namespace CodeWalker.Graphic;

class TexModTreeView : TreeView
{
    const int TVM_ENSUREVISIBLE = 0x1114;

    public TexModTreeView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == TVM_ENSUREVISIBLE)
            return;
        base.WndProc(ref m);
    }
}