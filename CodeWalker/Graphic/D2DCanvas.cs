using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using SharpDX.WIC;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Markup;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Factory = SharpDX.Direct2D1.Factory;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using WicFactory = SharpDX.WIC.ImagingFactory;

namespace CodeWalker;

public enum AsyncImageState : byte
{
    None,
    Loading,
    Ready,
    Loaded,
    Error,
    Disposed
}

public abstract class AsyncBitmapSource : IDisposable
{
    public abstract class Factory : IDisposable
    {
        public abstract Bitmap CreateBitmap(RenderTarget target);
        public abstract void Dispose();
    }

    public bool shared;
    public Factory factory;
    public AsyncImageState state;

    public bool disposed => state == AsyncImageState.Disposed;
    public bool loading => state == AsyncImageState.Loading;
    public bool error => state == AsyncImageState.Error;

    public abstract Bitmap CreateBitmap(RenderTarget target);
    public abstract void Load();
    public abstract bool Equals(AsyncBitmapSource other);

    public virtual void Dispose()
    {
        state = AsyncImageState.Disposed;
        Utilities.Dispose(ref factory);
    }
}

public class AsyncImageFileSource : AsyncBitmapSource
{
    class ImageFactory : Factory
    {
        public int width;
        public int height;
        public DataStream dataStream;

        public ImageFactory(DataStream data, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.dataStream = data;
        }

        public override Bitmap CreateBitmap(RenderTarget target)
        {
            try
            {
                var pixelFormat = new SharpDX.Direct2D1.
                    PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore);
                var bmpProps = new BitmapProperties(pixelFormat);

                var stride = width * 4;
                return new Bitmap(
                    target,
                    new Size2(width, height),
                    new DataPointer(dataStream.DataPointer, stride * height),
                    stride,
                    bmpProps
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        public override void Dispose()
        {
            Utilities.Dispose(ref dataStream);
        }
    }

    private string filename;

    public AsyncImageFileSource(string filename)
    {
        this.filename = filename;
    }

    public override void Load()
    {
        if (state == AsyncImageState.None)
        {
            state = AsyncImageState.Loading;
            Task.Run(Run);
        }
    }

    public override Bitmap CreateBitmap(RenderTarget target)
    {
        var bitmap = factory?.CreateBitmap(target);
        if (!shared) Utilities.Dispose(ref factory);
        return bitmap;
    }

    private void Run()
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

            var data = new DataStream(pixels.Length, true, true);
            data.Write(pixels, 0, pixels.Length);
            data.Position = 0;

            if (disposed)
            {
                data.Dispose();
                return;
            }
            factory = new ImageFactory(data, width, height);
            state = AsyncImageState.Ready;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            state = AsyncImageState.Error;
        }
    }

    public override bool Equals(AsyncBitmapSource other)
    {
        if (other is AsyncImageFileSource x)
        {
            return x.filename == filename;
        }
        return false;
    }
}

public delegate void D2DCanvasBitmapLoadedHandler(D2DCanvas canvas);
public delegate void D2DCanvasPaintHandler(D2DCanvas canvas, RenderTarget target, Bitmap bitmap);

public class D2DCanvas : Control
{
    public static readonly WicFactory wic;
    public static readonly Factory factory;
    public static readonly TextFormat fontSegoeUI_16;
    public static readonly TextFormat fontSegoeUI_12;

    static D2DCanvas()
    {
        wic = new WicFactory();
        factory = new Factory(FactoryType.MultiThreaded);

        using var dwFactory = new SharpDX.DirectWrite.Factory();
        fontSegoeUI_16 = new TextFormat(dwFactory, "Segoe UI", 16f);
        fontSegoeUI_12 = new TextFormat(dwFactory, "Segoe UI", 12f);
    }

    WindowRenderTarget target;
    SolidColorBrush solidBrush;
    BitmapBrush tileBrush;

    Bitmap bitmap;
    Size2 imageSize;
    AsyncBitmapSource bitmapSource;

    public RawMatrix3x2 transform => target.Transform;
    public D2DCanvasPaintHandler onPaint;
    public D2DCanvasBitmapLoadedHandler onBitmapLoaded;

    public D2DCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.UserPaint
                 | ControlStyles.SupportsTransparentBackColor, true);
    }

    static Bitmap LoadEmbeddedBitmap(WindowRenderTarget target, byte[] bytes)
    {
        using var stream = new MemoryStream(bytes);
        using var wicStream = new WICStream(wic, stream);
        using var decoder = new BitmapDecoder(
            wic,
            wicStream,
            DecodeOptions.CacheOnLoad
        );

        using var frame = decoder.GetFrame(0);
        using var converter = new FormatConverter(wic);
        converter.Initialize(
            frame,
            SharpDX.WIC.PixelFormat.Format32bppRGB,
            BitmapDitherType.None,
            null,
            0,
            BitmapPaletteType.Custom
        );

        return Bitmap.FromWicBitmap(target, converter);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);

        var props = new HwndRenderTargetProperties()
        {
            Hwnd = Handle,
            PixelSize = new Size2(Width, Height),
            PresentOptions = PresentOptions.Immediately
        };
        var format = new SharpDX.Direct2D1.PixelFormat(
            Format.R8G8B8A8_UNorm,
            SharpDX.Direct2D1.AlphaMode.Ignore
        );
        var rtProps = new RenderTargetProperties(
            RenderTargetType.Hardware,
            format, 0, 0,
            RenderTargetUsage.None,
            FeatureLevel.Level_DEFAULT
        );
        target = new WindowRenderTarget(factory, rtProps, props);

        solidBrush = new SolidColorBrush(target, new RawColor4(1f, 0, 0, 1f));
        var transparent = LoadEmbeddedBitmap(target, Properties.Resources.transparent);
        tileBrush = new BitmapBrush(target, transparent, new BitmapBrushProperties()
        {
            ExtendModeX = ExtendMode.Wrap,
            ExtendModeY = ExtendMode.Wrap,
            InterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor
        });

        if (bitmapSource != null)
        {
            bitmapSource.Load();
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref tileBrush);
        Utilities.Dispose(ref solidBrush);
        Utilities.Dispose(ref bitmapSource);
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

    public bool HasImage()
    {
        return bitmap != null;
    }

    public Size2 GetImageSize()
    {
        return imageSize;
    }

    public Bitmap GetImage()
    {
        return bitmap;
    }

    public void ClearImage()
    {
        imageSize = Size2.Zero;
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref bitmapSource);
    }

    public void SetImage(string imagePath)
    {
        SetImage(new AsyncImageFileSource(imagePath));
    }

    public void SetImage(AsyncBitmapSource source)
    {
        if (bitmapSource != null && bitmapSource.Equals(source))
        {
            return;
        }
        ClearImage();
        bitmapSource = source;
        if (target != null)
        {
            bitmapSource?.Load();
            Invalidate();
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (target == null) return;

        target.BeginDraw();
        target.Clear(new RawColor4(1, 1, 1, 1));
        target.Transform = Matrix3x2.Identity;
        target.FillRectangle(
            new RawRectangleF(0, 0, Width, Height),
            tileBrush
        );

        if (bitmapSource != null)
        {
            if (bitmapSource.error)
            {
                DrawText("unable to load image", 6, 0, Color.Red, fontSegoeUI_16);
            }
            else if (bitmapSource.loading)
            {
                DrawText("loading...", 6, 0, Color.Orange, fontSegoeUI_16);
                Invalidate();
            }
            else if (bitmap != null)
            {
                if (onPaint == null)
                {
                    target.DrawBitmap(bitmap, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
                }
                else
                {
                    try
                    {
                        onPaint(this, target, bitmap);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            }
            else if (bitmapSource.state == AsyncImageState.Ready)
            {
                Utilities.Dispose(ref bitmap);
                bitmap = bitmapSource.CreateBitmap(target);
                if (bitmap != null)
                {
                    imageSize = bitmap.PixelSize;
                    onBitmapLoaded?.Invoke(this);
                }
                Invalidate();
            }
        }
        target.EndDraw();
    }

    public void FillRectangle(in System.Drawing.Rectangle rectangle, in RawColor4 color)
    {
        solidBrush.Color = color;
        target.FillRectangle(rectangle.Convert2(), solidBrush);
    }

    public void DrawRectangle(in System.Drawing.Rectangle rectangle, in RawColor4 color, float thickness)
    {
        solidBrush.Color = color;
        target.DrawRectangle(rectangle.Convert2(), solidBrush, thickness);
    }

    public void DrawBitmap(Bitmap bitmap, int x, int y)
    {
        DrawBitmap(bitmap, new RawRectangleF(0, 0, bitmap.Size.Width, bitmap.Size.Height));
    }

    public void DrawBitmap(Bitmap bitmap, in RawRectangleF rectangle)
    {
        target.DrawBitmap(bitmap, rectangle, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
    }

    public void DrawBitmap(Bitmap bitmap, in System.Drawing.Rectangle destinationRectangle, in System.Drawing.Rectangle sourceRectangle)
    {
        target.DrawBitmap(
            bitmap,
            destinationRectangle.Convert2(),
            1f,
            SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
            sourceRectangle.Convert2()
        );
    }

    public void DrawBitmap(Bitmap bitmap, in RawRectangleF destinationRectangle, in RawRectangleF sourceRectangle)
    {
        target.DrawBitmap(
            bitmap,
            destinationRectangle,
            1f,
            SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
            sourceRectangle
        );
    }

    public void SetTransformation(float x, float y, float scale)
    {
        target.Transform = Matrix3x2.Scaling(scale, scale) * Matrix3x2.Translation(x, y);
    }

    public void DrawText(string text, int x, int y, in Color color, TextFormat font = null)
    {
        solidBrush.Color = color;
        target.DrawText(
            text,
            font ?? fontSegoeUI_12,
            new RawRectangleF(x, y, Width, Height),
            solidBrush,
            DrawTextOptions.None,
            MeasuringMode.Natural
        );
    }
}