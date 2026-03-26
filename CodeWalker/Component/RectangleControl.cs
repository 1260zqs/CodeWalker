using System;
using System.Windows.Forms;

namespace CodeWalker;

public partial class RectangleControl : UserControl
{
    public RectangleControl()
    {
        InitializeComponent();
    }

    private System.Drawing.RectangleF m_Rectangle;

    public void SetRectBox(System.Drawing.RectangleF rectangle)
    {
        m_Rectangle = rectangle;
        rectBoxX.Value = (decimal)rectangle.X;
        rectBoxY.Value = (decimal)rectangle.Y;
        rectBoxW.Value = (decimal)rectangle.Width;
        rectBoxH.Value = (decimal)rectangle.Height;
    }

    private void rectBoxX_ValueChanged(object sender, EventArgs e)
    {
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
    }
}