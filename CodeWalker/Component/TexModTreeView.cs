using System.Windows.Forms;

namespace CodeWalker;

class TexModTreeView : TreeView
{
    public TexModTreeView()
    {
        SetStyle(ControlStyles.OptimizedDoubleBuffer |
                 ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
    }
}