using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker.TexMod;

public partial class TextureModImageControl : DockContent
{
    public TextureModImageControl()
    {
        InitializeComponent();

        PictureBoxViewer.AddFeature(canvas);
        PictureBoxRectTool.AddFeature(canvas, OnRectDrawingChange);
    }

    public Action<D2DCanvas, System.Drawing.RectangleF> onRectDrawingChange;

    private void OnRectDrawingChange(System.Drawing.RectangleF rectangle)
    {
        onRectDrawingChange?.Invoke(canvas, rectangle);
    }

    public void Repaint() => canvas.Invalidate();

    public Bitmap GetImage() => canvas.GetImage();

    public void SetImage(Bitmap bitmap) => canvas.SetImage(bitmap);

    public void SetImage(AsyncBitmapSource source) => canvas.SetImage(source);
    public void ClearImage() => canvas.ClearImage();
}