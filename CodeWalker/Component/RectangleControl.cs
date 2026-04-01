using System;
using System.Threading;
using System.Windows.Forms;

namespace CodeWalker;

public partial class RectangleControl : UserControl
{
    public RectangleControl()
    {
        InitializeComponent();
    }

    private System.Drawing.RectangleF m_Rectangle;
    public event System.EventHandler<System.Drawing.RectangleF> onValueChanged;

    public void SetRectBox(System.Drawing.RectangleF rectangle)
    {
        m_Rectangle = rectangle;
        rectBoxX.Value = (decimal)rectangle.X;
        rectBoxY.Value = (decimal)rectangle.Y;
        rectBoxW.Value = (decimal)rectangle.Width;
        rectBoxH.Value = (decimal)rectangle.Height;
    }

    public System.Drawing.RectangleF GetRect()
    {
        return m_Rectangle;
    }

    private void rectBoxX_ValueChanged(object sender, EventArgs e)
    {
        BeginValueChange();
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
        BeginValueChange();
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
        BeginValueChange();
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
        BeginValueChange();
    }

    private volatile int valueChangedPending;

    private void BeginValueChange()
    {
        if (Interlocked.CompareExchange(ref valueChangedPending, 1, 0) == 0)
        {
            BeginInvoke(OnValueChanged);
        }
    }

    private void OnValueChanged()
    {
        Interlocked.Exchange(ref valueChangedPending, 0);
        onValueChanged?.Invoke(this, m_Rectangle);
    }
}