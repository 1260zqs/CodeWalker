using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;
using SharpDX.WIC;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using CodeWalker.Utils;
using Bitmap = SharpDX.Direct2D1.Bitmap;
using Factory = SharpDX.Direct2D1.Factory;
using WicFactory = SharpDX.WIC.ImagingFactory;
using FactoryType = SharpDX.Direct2D1.FactoryType;
using BitmapInterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode;

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
    public int width;
    public int height;
    public AsyncImageState state;

    protected Bitmap bitmap;
    protected object stateObject;

    public bool disposed => state == AsyncImageState.Disposed;
    public bool loading => state == AsyncImageState.Loading;
    public bool error => state == AsyncImageState.Error;


    public Bitmap GetBitmap() => bitmap;

    public abstract Bitmap CreateBitmapOnMainThread();
    public abstract void Load(RenderTarget target);
    public abstract bool Equals(AsyncBitmapSource other);
    public virtual void Dispose()
    {
        state = AsyncImageState.Disposed;
        Utilities.Dispose(ref bitmap);
    }
}

public class AsyncImageFileSource : AsyncBitmapSource
{
    private string filename;
    private RenderTarget target;

    public AsyncImageFileSource(string filename)
    {
        this.filename = filename;
    }

    public override void Load(RenderTarget target)
    {
        if (state == AsyncImageState.None)
        {
            this.target = target;
            state = AsyncImageState.Loading;
            Task.Run(Run);
        }
    }

    public override Bitmap CreateBitmapOnMainThread()
    {
        if (stateObject != null)
        {
            bitmap = CreateBitmap((DataStream)stateObject);
            stateObject = null;
        }
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

            width = converter.Size.Width;
            height = converter.Size.Height;

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
            stateObject = data;
            state = AsyncImageState.Ready;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            state = AsyncImageState.Error;
        }
    }

    private Bitmap CreateBitmap(DataStream data)
    {
        try
        {
            var pixelFormat = new SharpDX.Direct2D1.
                PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore);
            var bmpProps = new BitmapProperties(pixelFormat);

            var stride = width * 4;
            bitmap = new Bitmap(
                target,
                new Size2(width, height),
                new DataPointer(data.DataPointer, stride * height),
                stride,
                bmpProps
            );
            Utilities.Dispose(ref data);
            state = AsyncImageState.Loaded;
        }
        catch (Exception ex)
        {
            state = AsyncImageState.Error;
            Utilities.Dispose(ref bitmap);
            Utilities.Dispose(ref data);
            Console.WriteLine(ex);
        }
        return bitmap;
    }

    public override void Dispose()
    {
        base.Dispose();
        if (stateObject is IDisposable disposable)
        {
            stateObject = null;
            Utilities.Dispose(ref disposable);
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

    SolidColorBrush brush;
    WindowRenderTarget target;
    AsyncBitmapSource bitmapSource;
    Bitmap safeBitmap;

    public RawMatrix3x2 transform => target.Transform;
    public D2DCanvasPaintHandler onPaint;

    public D2DCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint
                 | ControlStyles.UserPaint
                 | ControlStyles.Opaque, true);
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
        brush = new SolidColorBrush(target, new RawColor4(1f, 0, 0, 1f));
        if (bitmapSource != null)
        {
            bitmapSource.Load(target);
        }
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        safeBitmap = null;
        Utilities.Dispose(ref brush);
        Utilities.Dispose(ref target);
        Utilities.Dispose(ref bitmapSource);
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
        if (bitmapSource != null)
        {
            return bitmapSource.state == AsyncImageState.Loaded;
        }
        return false;
    }

    public Size2 GetImageSize()
    {
        if (bitmapSource != null)
        {
            return new Size2(bitmapSource.width, bitmapSource.height);
        }
        return Size2.Zero;
    }

    public Bitmap GetImage()
    {
        return safeBitmap;
    }

    public void ClearImage()
    {
        safeBitmap = null;
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
            bitmapSource?.Load(target);
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
            else if (bitmapSource.state == AsyncImageState.Ready)
            {
                safeBitmap = bitmapSource.CreateBitmapOnMainThread();
            }
            if (bitmapSource.state == AsyncImageState.Loaded)
            {
                if (bitmapSource.GetBitmap() is { } bitmap)
                {
                    if (onPaint == null)
                    {
                        target.DrawBitmap(
                            bitmap,
                            new RawRectangleF(0, 0, bitmap.Size.Width, bitmap.Size.Height),
                            1f,
                            BitmapInterpolationMode.NearestNeighbor
                        );
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
            }
            else
            {
                Invalidate();
            }
        }
        target.EndDraw();
    }

    public void FillRectangle(in System.Drawing.Rectangle rectangle, RawColor4 color)
    {
        brush.Color = color;
        target.FillRectangle(rectangle.Convert2(), brush);
    }

    public void DrawRectangle(in System.Drawing.Rectangle rectangle, in RawColor4 color, float thickness)
    {
        brush.Color = color;
        target.DrawRectangle(rectangle.Convert2(), brush, thickness);
    }

    public void DrawBitmap(Bitmap bitmap, int x, int y)
    {
        DrawBitmap(bitmap, new RawRectangleF(0, 0, bitmap.Size.Width, bitmap.Size.Height));
    }

    public void DrawBitmap(Bitmap bitmap, in RawRectangleF rectangle)
    {
        target.DrawBitmap(bitmap, rectangle, 1f, BitmapInterpolationMode.NearestNeighbor);
    }

    public void DrawBitmap(Bitmap bitmap, in System.Drawing.Rectangle destinationRectangle, in System.Drawing.Rectangle sourceRectangle)
    {
        target.DrawBitmap(
            bitmap,
            destinationRectangle.Convert2(),
            1f,
            BitmapInterpolationMode.NearestNeighbor,
            sourceRectangle.Convert2()
        );
    }

    public void DrawBitmap(Bitmap bitmap, in RawRectangleF destinationRectangle, in RawRectangleF sourceRectangle)
    {
        target.DrawBitmap(
            bitmap,
            destinationRectangle,
            1f,
            BitmapInterpolationMode.NearestNeighbor,
            sourceRectangle
        );
    }

    public void SetTransformation(float x, float y, float scale)
    {
        target.Transform = Matrix3x2.Scaling(scale, scale) * Matrix3x2.Translation(x, y);
    }

    public void DrawText(string text, int x, int y, in Color color, TextFormat font = null)
    {
        brush.Color = color;
        target.DrawText(
            text,
            font ?? fontSegoeUI_12,
            new RawRectangleF(x, y, Width, Height),
            brush,
            DrawTextOptions.None,
            MeasuringMode.Natural
        );
    }
}