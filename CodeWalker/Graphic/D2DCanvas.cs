using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Factory = SharpDX.Direct2D1.Factory;
using WicFactory = SharpDX.WIC.ImagingFactory;

namespace CodeWalker;

public class AsyncImageSource : IDisposable
{
    private string filename;
    private WindowRenderTarget target;
    private SharpDX.Direct2D1.Bitmap bitmap;
    private Func<SharpDX.Direct2D1.Bitmap> uploadBitmap;

    public bool loading;
    public bool ready;
    public bool error;

    public AsyncImageSource(string filename)
    {
        this.filename = filename;
    }

    public SharpDX.Direct2D1.Bitmap GetBitmap()
    {
        if (bitmap != null)
        {
            return bitmap;
        }
        if (error) return null;
        if (ready && uploadBitmap != null)
        {
            bitmap = uploadBitmap();
            uploadBitmap = null;
        }
        return bitmap;
    }

    public void StartLoad(WindowRenderTarget target)
    {
        if (loading || ready) return;
        this.target = target;

        error = false;
        ready = false;
        loading = true;

        Task.Run(Load);
    }

    private void Load()
    {
        try
        {
            using var decoder = new BitmapDecoder(D2DCanvas.wic, filename, DecodeOptions.CacheOnLoad);
            using var converter = new FormatConverter(D2DCanvas.wic);
            using var frame = decoder.GetFrame(0);

            converter.Initialize(
                frame,
                SharpDX.WIC.PixelFormat.Format32bppRGBA,
                BitmapDitherType.None,
                null,
                0,
                BitmapPaletteType.Custom
            );

            var width = converter.Size.Width;
            var height = converter.Size.Height;

            var stride = width * 4;
            var pixels = new byte[stride * height];
            converter.CopyPixels(pixels, stride);

            var dataStream = new DataStream(pixels.Length, true, true);
            dataStream.Write(pixels, 0, pixels.Length);
            dataStream.Position = 0;

            uploadBitmap = () => UploadBitmap(width, height, dataStream);
            loading = false;
        }
        catch (Exception)
        {
            loading = false;
            ready = false;
            error = true;
        }
    }

    private SharpDX.Direct2D1.Bitmap UploadBitmap(int width, int height, DataStream data)
    {
        SharpDX.Direct2D1.Bitmap bitmap = null;
        try
        {
            var format = new SharpDX.Direct2D1.PixelFormat(
                Format.B8G8R8A8_UNorm,
                SharpDX.Direct2D1.AlphaMode.Premultiplied
            );
            var bmpProps = new BitmapProperties(format);

            var stride = width * 4;
            bitmap = new SharpDX.Direct2D1.Bitmap(
                target,
                new Size2(width, height),
                new DataPointer(data.DataPointer, stride * height),
                stride,
                bmpProps
            );
            data.Dispose();
            ready = true;
        }
        catch (Exception)
        {
            loading = false;
            error = true;
            data.Dispose();
        }
        return bitmap;
    }

    public void Dispose()
    {
        Utilities.Dispose(ref bitmap);
    }
}

public class D2DCanvas : Control
{
    public static readonly WicFactory wic;
    public static readonly Factory factory;

    static D2DCanvas()
    {
        wic = new WicFactory();
        factory = new Factory(FactoryType.MultiThreaded);
    }

    WindowRenderTarget target;
    AsyncImageSource imageSource;

    public D2DCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
            | ControlStyles.SupportsTransparentBackColor
            | ControlStyles.UserPaint, true);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        var props = new HwndRenderTargetProperties()
        {
            Hwnd = Handle,
            PixelSize = new Size2(Width, Height),
            PresentOptions = PresentOptions.None
        };
        var format = new SharpDX.Direct2D1.PixelFormat(
            Format.Unknown,
            SharpDX.Direct2D1.AlphaMode.Premultiplied
        );
        var rtProps = new RenderTargetProperties(
            RenderTargetType.Default,
            format, 0, 0,
            RenderTargetUsage.None,
            FeatureLevel.Level_DEFAULT
        );
        target = new WindowRenderTarget(factory, rtProps, props);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        Utilities.Dispose(ref target);
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (target != null)
        {
            target.Resize(new Size2(Width, Height));
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (target == null) return;

        var c = BackColor;
        target.BeginDraw();
        target.Clear(new RawColor4(c.R, c.G, c.B, c.A));

        if (imageSource != null)
        {
            if (imageSource.error)
            {

            }
            else if (imageSource.loading)
            {
                Invalidate();
            }
            else if (imageSource.ready)
            {
                var bitmap = imageSource.GetBitmap();
                if (bitmap != null)
                {
                    target.DrawBitmap(
                        bitmap,
                        new RawRectangleF(0, 0, bitmap.Size.Width, bitmap.Size.Height),
                        1f,
                        SharpDX.Direct2D1.BitmapInterpolationMode.Linear
                    );
                }
            }
        }
        target.EndDraw();
    }
}
