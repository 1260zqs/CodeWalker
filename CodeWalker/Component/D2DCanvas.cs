using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using CodeWalker.Graphic;
using CodeWalker.Utils;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker;

public delegate void D2DCanvasBitmapLoadedHandler(D2DCanvas canvas);
public delegate void D2DCanvasPaintHandler(D2DCanvas canvas, RenderTarget target, Bitmap bitmap);

public class D2DCanvas : Control
{
    WindowRenderTarget target;
    SolidColorBrush solidBrush;
    BitmapBrush tileBrush;

    Bitmap bitmap;
    Size2 imageSize;
    AsyncBitmapSource bitmapSource;

    public RawMatrix3x2 transform => target.Transform;
    public D2DCanvasPaintHandler onPaint;
    public D2DCanvasBitmapLoadedHandler onBitmapLoaded;
    private bool isError;

    public D2DCanvas()
    {
        const ControlStyles flags = ControlStyles.UserPaint
                                    | ControlStyles.AllPaintingInWmPaint
                                    | ControlStyles.SupportsTransparentBackColor;
        SetStyle(flags, true);
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
        target = new WindowRenderTarget(DXGraphic.d2dFactory, rtProps, props);

        solidBrush = new SolidColorBrush(target, new RawColor4(1f, 0, 0, 1f));
        var transparent = DXGraphic.LoadEmbeddedBitmap(target, "transparent.bmp");
        if (transparent != null)
        {
            tileBrush = new BitmapBrush(target, transparent, new BitmapBrushProperties()
            {
                ExtendModeX = ExtendMode.Wrap,
                ExtendModeY = ExtendMode.Wrap,
                InterpolationMode = SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor
            });
        }

        bitmapSource?.LoadAsync();
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
        isError = false;
        imageSize = Size2.Zero;
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref bitmapSource);
        Invalidate();
    }

    public void SetImage(SharpDX.Direct2D1.Bitmap bitmap)
    {
        isError = false;
        this.bitmap = bitmap;
        if (bitmap != null)
        {
            imageSize = bitmap.PixelSize;
        }
        else
        {
            imageSize = Size2.Zero;
        }
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
            bitmapSource?.LoadAsync();
        }
    }

    internal RenderTarget GetRenderTargetInternal() => target;
    internal void CreateImageFromExtern(RenderTarget target) => CreateBitmap(target);

    private void CreateBitmap(RenderTarget target)
    {
        Utilities.Dispose(ref bitmap);
        bitmap = bitmapSource.CreateBitmap(target);
        if (!(isError = bitmap == null))
        {
            imageSize = bitmap.PixelSize;
            onBitmapLoaded?.Invoke(this);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (target == null) return;

        target.BeginDraw();
        target.Transform = Matrix3x2.Identity;
        if (tileBrush != null)
        {
            target.FillRectangle(
                new RawRectangleF(0, 0, Width, Height),
                tileBrush
            );
        }
        else
        {
            target.Clear(new RawColor4(1, 1, 1, 1));
        }

        if (bitmapSource != null)
        {
            if (bitmapSource.error || isError)
            {
                DrawText("unable to load image", 6, 0, Color.Red, DXGraphic.fontSegoeUI_16);
            }
            else if (bitmapSource.loading)
            {
                DrawText("loading...", 6, 0, Color.Orange, DXGraphic.fontSegoeUI_16);
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
                CreateBitmap(target);
                Invalidate();
            }
        }
        target.EndDraw();
    }

    public void FillRectangle(in System.Drawing.RectangleF rectangle, in RawColor4 color)
    {
        solidBrush.Color = color;
        target.FillRectangle(rectangle.Raw(), solidBrush);
    }

    public void FillRectangle(in SharpDX.Mathematics.Interop.RawRectangleF rectangle, in RawColor4 color)
    {
        solidBrush.Color = color;
        target.FillRectangle(rectangle, solidBrush);
    }

    public void DrawRectangle(in System.Drawing.RectangleF rectangle, in RawColor4 color, float thickness)
    {
        solidBrush.Color = color;
        target.DrawRectangle(rectangle.Raw(), solidBrush, thickness);
    }

    public void DrawBitmap(Bitmap bitmap, int x, int y)
    {
        DrawBitmap(bitmap, new RawRectangleF(0, 0, bitmap.Size.Width, bitmap.Size.Height));
    }

    public void DrawBitmap(Bitmap bitmap, in RawRectangleF rectangle)
    {
        target.DrawBitmap(bitmap, rectangle, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
    }

    public void DrawBitmap(Bitmap bitmap, in System.Drawing.RectangleF destinationRectangle, in System.Drawing.Rectangle sourceRectangle)
    {
        target.DrawBitmap(
            bitmap,
            destinationRectangle.Raw(),
            1f,
            SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor,
            sourceRectangle.Raw()
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

    public void DrawTextLayout(float x, float y, TextLayout textLayout, in SharpDX.Color color)
    {
        solidBrush.Color = color;
        target.DrawTextLayout(new RawVector2(x, y), textLayout, solidBrush);
    }

    public void DrawText(string text, float x, float y, in SharpDX.Color color, TextFormat font = null)
    {
        solidBrush.Color = color;
        target.DrawText(
            text,
            font ?? DXGraphic.fontSegoeUI_12,
            new RawRectangleF(x, y, x + Width, y + Height),
            solidBrush,
            DrawTextOptions.None,
            MeasuringMode.Natural
        );
    }
}