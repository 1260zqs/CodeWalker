using System;
using System.Threading;
using System.Windows.Forms;

namespace CodeWalker;

public partial class RectangleControl : UserControl
{
    public RectangleControl()
    {
        InitializeComponent();
        AddEvent();
    }

    private System.Drawing.RectangleF m_Rectangle;
    private event System.EventHandler<System.Drawing.RectangleF> m_OnValueChanged;

    public event System.EventHandler<System.Drawing.RectangleF> OnValueChanged
    {
        add => m_OnValueChanged += value;
        remove => m_OnValueChanged -= value;
    }

    public void SetRect(in System.Drawing.RectangleF rectangle)
    {
        m_Rectangle = rectangle;
        rectBoxX.Value = (decimal)rectangle.X;
        rectBoxY.Value = (decimal)rectangle.Y;
        rectBoxW.Value = (decimal)rectangle.Width;
        rectBoxH.Value = (decimal)rectangle.Height;
    }

    public void SetRectWithoutNotify(in System.Drawing.RectangleF rectangle)
    {
        RemoveEvent();
        SetRect(rectangle);
        AddEvent();
    }

    private void AddEvent()
    {
        this.rectBoxX.ValueChanged += this.rectBoxX_ValueChanged;
        this.rectBoxH.ValueChanged += this.rectBoxH_ValueChanged;
        this.rectBoxW.ValueChanged += this.rectBoxW_ValueChanged;
        this.rectBoxY.ValueChanged += this.rectBoxY_ValueChanged;
    }

    private void RemoveEvent()
    {
        this.rectBoxX.ValueChanged -= this.rectBoxX_ValueChanged;
        this.rectBoxH.ValueChanged -= this.rectBoxH_ValueChanged;
        this.rectBoxW.ValueChanged -= this.rectBoxW_ValueChanged;
        this.rectBoxY.ValueChanged -= this.rectBoxY_ValueChanged;
    }

    public System.Drawing.RectangleF GetRect()
    {
        return m_Rectangle;
    }

    private void rectBoxX_ValueChanged(object sender, EventArgs e)
    {
        MarkValueChanged();
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
        MarkValueChanged();
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
        MarkValueChanged();
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
        MarkValueChanged();
    }

    private volatile int valueChangedPending;

    private void MarkValueChanged()
    {
        if (Interlocked.CompareExchange(ref valueChangedPending, 1, 0) == 0)
        {
            BeginInvoke(FireValueChangedEvent);
        }
    }

    private void FireValueChangedEvent()
    {
        Interlocked.Exchange(ref valueChangedPending, 0);
        m_OnValueChanged?.Invoke(this, m_Rectangle);
    }
}