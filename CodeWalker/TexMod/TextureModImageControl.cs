using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker.TexMod;

public partial class TextureModImageControl : DockContent
{
    public TextureModImageControl()
    {
        InitializeComponent();

        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip1, version, theme);

        toolStripComboBox1.Items.AddRange(new object[] { 0.5f, 1.0f, 2.0f, 3.0f });
        toolStripComboBox1.ComboBox.Format += (s, e) =>
        {
            e.Value = $"{(float)e.ListItem * 100}%";
        };
        toolStripComboBox1.SelectedIndex = 1;

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

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
        ActiveControl = null;
        if (canvas.HasImage())
        {
            var pixelSize = canvas.GetImageSize();
            PictureBoxViewer.FitViewer(canvas, pixelSize.Width, pixelSize.Height);
            canvas.Invalidate(true);
        }
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
        ActiveControl = null;
        PictureBoxViewer.ResetViewer(canvas);
        canvas.Invalidate(true);
    }

    private void toolStripButton3_Click(object sender, EventArgs e)
    {
        ActiveControl = null;
        var x = (canvas.Right - canvas.Left) / 2f;
        var y = (canvas.Bottom - canvas.Top) / 2f;
        PictureBoxViewer.Zoom(canvas, false, x, y);
    }

    private void toolStripButton4_Click(object sender, EventArgs e)
    {
        ActiveControl = null;
        var x = (canvas.Right - canvas.Left) / 2f;
        var y = (canvas.Bottom - canvas.Top) / 2f;
        PictureBoxViewer.Zoom(canvas, true, x, y);
    }

    private void toolStripComboBox1_SelectedIndexChanged(object sender, EventArgs e)
    {
        ActiveControl = null;
        var zoom = (float)toolStripComboBox1.SelectedItem;
        var x = (canvas.Right - canvas.Left) / 2f;
        var y = (canvas.Bottom - canvas.Top) / 2f;
        PictureBoxViewer.SetZoom(canvas, zoom, x, y);
    }
}