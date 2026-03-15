using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Utils;

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

public abstract class AsyncPictureSource
{
    public abstract bool Loading { get; }
    public abstract bool Error { get; }
    public abstract Image GetImage();
    public abstract void Load();
    public abstract void Unload();

    public abstract bool Equals(AsyncPictureSource other);
}

public class ImageFilePictureSource : AsyncPictureSource
{
    public override bool Loading => loading;
    public override bool Error => error;

    private bool loading;
    private bool error;
    private int loadingHandle;

    private Image image;
    private string resourceName;
    public IResourceLoader loader;

    public ImageFilePictureSource(string resourceName)
    {
        this.resourceName = resourceName;
    }

    public override Image GetImage()
    {
        return image;
    }

    public override void Unload()
    {
        loader?.UnLoadImage(loadingHandle);
    }

    public override void Load()
    {
        loading = true;
        error = false;
        loader.UnLoadImage(loadingHandle);
        loadingHandle = loader.LoadImage(resourceName, x =>
        {
            image = x;
            loading = false;
            error = x == null;
        });
    }

    public override bool Equals(AsyncPictureSource other)
    {
        if (other is ImageFilePictureSource x)
        {
            return x.resourceName == resourceName;
        }
        return false;
    }
}

class GamePackPictureSource(string sourceFile) : AsyncPictureSource
{
    public override bool Loading => loading;
    public override bool Error => error;

    private bool loading;
    private bool loaded;
    private bool error;
    private Task task;
    public TextureModAdapter adapter;
    private GameFile gameFile;
    private Image image;

    public override void Load()
    {
        if (loading || loaded) return;
        loading = true;
        loaded = false;
        error = false;
        task = Task.Run(async () =>
        {
            await Task.Yield();
            gameFile = adapter.GetSourceFile(sourceFile);
            if (gameFile == null)
            {
                loading = false;
                loaded = false;
                error = true;
                task = null;
                return;
            }
            gameFile.Use();
            var texName = adapter.GetSourceTextureName(sourceFile);
            while (loading)
            {
                await Task.Yield();
                if (gameFile.Loaded)
                {
                    var texture = adapter.GetSourceTexture(gameFile, texName);
                    if (texture == null)
                    {
                        loading = false;
                        loaded = false;
                        error = true;
                        task = null;
                        return;
                    }
                    try
                    {
                        image = texture.CreateImage(0);
                        image.Tag = texture;
                        loading = false;
                        loaded = true;
                        error = false;
                    }
                    catch (Exception)
                    {
                        loading = false;
                        loaded = false;
                        error = true;
                    }
                    task = null;
                }
            }
        });
    }

    public override Image GetImage()
    {
        return image;
    }

    public override void Unload()
    {
        loading = false;
        if (image != null)
        {
            image.Dispose();
            image = null;
        }
    }

    public override bool Equals(AsyncPictureSource other)
    {
        if (other is GamePackPictureSource x)
        {
            return x.gameFile == gameFile;
        }
        return false;
    }
}

public class AsyncPictureBox
{
    public readonly PictureBox pictureBox;

    PixelOffsetMode pixelOffsetMode;
    InterpolationMode interpolationMode;

    public AsyncPictureSource pictureSource;
    public Action<Graphics, Image> onPaint;

    public AsyncPictureBox(PictureBox pictureBox)
    {
        pixelOffsetMode = PixelOffsetMode.Half;
        interpolationMode = InterpolationMode.NearestNeighbor;
        this.pictureBox = pictureBox;
        this.pictureBox.Paint += OnPaint;
    }

    public void DisplayPicture(AsyncPictureSource source)
    {
        pictureSource?.Unload();
        pictureSource = source;
        pictureBox.Invalidate();
    }

    private void OnPaint(object sender, PaintEventArgs e)
    {
        e.Graphics.Clear(Color.Gray);
        if (pictureSource == null) return;
        if (pictureSource.Loading)
        {
            PaintTool.DrawLoading(e.Graphics);
            pictureBox.Invalidate();
            return;
        }
        if (pictureSource.Error)
        {
            PaintTool.DrawErrorTex(e.Graphics, "unable to load image", 0, 0);
            return;
        }
        if (pictureSource.GetImage() is { } image)
        {
            var pic = sender as PictureBox;
            PictureBoxViewer.Update(pic, image);
            PictureBoxViewer.Paint(pic, e.Graphics);

            e.Graphics.PixelOffsetMode = pixelOffsetMode;
            e.Graphics.InterpolationMode = interpolationMode;
            if (onPaint != null)
            {
                onPaint(e.Graphics, image);
            }
            else
            {
                e.Graphics.DrawImage(image, 0, 0);
            }
            PictureBoxRectTool.Paint(pic, e.Graphics);
        }
    }

    public Image GetImage()
    {
        return pictureSource?.GetImage();
    }
}