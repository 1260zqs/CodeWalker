using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CodeWalker.TexMod;

public interface IResourceLoader
{
    int LoadImage(string fileName, Action<Image> callback);
    void UnLoadImage(int handle);
}

public static class PaintTool
{
    public static Font font;
    private static SolidBrush errorTexBrush;

    static PaintTool()
    {
        font = new Font("Consolas", 12f);
        errorTexBrush = new SolidBrush(Color.Red);
    }

    public static void DrawErrorTex(Graphics g, string text, int x, int y)
    {
        g.DrawString(text, font, errorTexBrush, x, y);
    }

    public static void DrawLoading(Graphics g)
    {
        g.DrawString("loading...", font, errorTexBrush, 0, 0);
    }
}

public class AsyncPictureBox
{
    private readonly PictureBox pictureBox;
    private Image image;
    private string resourceName;

    PixelOffsetMode pixelOffsetMode;
    InterpolationMode interpolationMode;

    private bool loading;
    private bool error;
    private int loadingHandle;

    public AsyncPictureBox(PictureBox pictureBox)
    {
        pixelOffsetMode = PixelOffsetMode.Half;
        interpolationMode = InterpolationMode.NearestNeighbor;
        this.pictureBox = pictureBox;
        this.pictureBox.Paint += OnPaint;
    }

    public void DisplayPicture(IResourceLoader loader, string fileName)
    {
        if (resourceName == fileName) return;

        loading = true;
        error = false;
        resourceName = fileName;
        loader.UnLoadImage(loadingHandle);
        loadingHandle = loader.LoadImage(resourceName, x =>
        {
            image = x;
            loading = false;
            error = x == null;
            pictureBox.Invalidate();
        });
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        e.Graphics.Clear(pictureBox.BackColor);
        if (loading)
        {
            PaintTool.DrawLoading(e.Graphics);
            pictureBox.Invalidate();
            return;
        }
        if (error)
        {
            PaintTool.DrawErrorTex(e.Graphics, "unable to load image", 0, 0);
            return;
        }

        if (image != null)
        {
            PictureBoxViewer.Paint(sender as PictureBox, e.Graphics);

            e.Graphics.PixelOffsetMode = pixelOffsetMode;
            e.Graphics.InterpolationMode = interpolationMode;
            e.Graphics.DrawImageUnscaled(image, 0, 0);
        }
    }
}