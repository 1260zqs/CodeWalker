using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.WinForms;

public partial class ToastWidget : UserControl
{
    private readonly Image background;

    public ToastWidget()
    {
        InitializeComponent();
        background = imageList1.Images[0];

        SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        BackColor = Color.Transparent;
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
        var graphics = e.Graphics;
        if (Parent != null)
        {
            var state = graphics.Save();
            graphics.TranslateTransform(-Left, -Top);

            var pe = new PaintEventArgs(graphics, Parent.ClientRectangle);
            InvokePaintBackground(Parent, pe);
            InvokePaint(Parent, pe);

            graphics.Restore(state);
        }
        Draw9Slice(graphics, background, new Rectangle(0, 0, Width, Height), 10, 10, 10, 10);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
    }

    public static void Draw9Slice(Graphics g, Image img, Rectangle dest, int left, int top, int right, int bottom)
    {
        var w = img.Width;
        var h = img.Height;

        var src = new Rectangle[]
        {
            new Rectangle(0, 0, left, top),
            new Rectangle(left, 0, w - left - right, top),
            new Rectangle(w - right, 0, right, top),

            new Rectangle(0, top, left, h - top - bottom),
            new Rectangle(left, top, w - left - right, h - top - bottom),
            new Rectangle(w - right, top, right, h - top - bottom),

            new Rectangle(0, h - bottom, left, bottom),
            new Rectangle(left, h - bottom, w - left - right, bottom),
            new Rectangle(w - right, h - bottom, right, bottom)
        };

        var dst = new Rectangle[]
        {
            new Rectangle(dest.Left, dest.Top, left, top),
            new Rectangle(dest.Left + left, dest.Top, dest.Width - left - right, top),
            new Rectangle(dest.Right - right, dest.Top, right, top),

            new Rectangle(dest.Left, dest.Top + top, left, dest.Height - top - bottom),
            new Rectangle(dest.Left + left, dest.Top + top, dest.Width - left - right, dest.Height - top - bottom),
            new Rectangle(dest.Right - right, dest.Top + top, right, dest.Height - top - bottom),

            new Rectangle(dest.Left, dest.Bottom - bottom, left, bottom),
            new Rectangle(dest.Left + left, dest.Bottom - bottom, dest.Width - left - right, bottom),
            new Rectangle(dest.Right - right, dest.Bottom - bottom, right, bottom)
        };

        for (var i = 0; i < 9; i++)
        {
            g.DrawImage(img, dst[i], src[i], GraphicsUnit.Pixel);
        }
    }
}