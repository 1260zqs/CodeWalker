using System;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using SharpDX.DXGI;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using CodeWalker.Graphic;
using CodeWalker.Utils;
using Device = SharpDX.Direct3D11.Device;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker;

public delegate void D2DCanvasBitmapLoadedHandler(D2DCanvas canvas);

public delegate void D2DCanvasPaintHandler(D2DCanvas canvas, RenderTarget target, Bitmap bitmap);

public class D2DCanvas : Control
{
    SharpDX.Direct3D11.DeviceContext immediateContext;
    SwapChain swapChain;
    Texture2D backBuffer;
    RenderTargetView backBufferView;
    RenderTarget target;
    SolidColorBrush solidBrush;
    BitmapBrush tileBrush;

    Bitmap bitmap;
    Size2 imageSize;
    AsyncBitmapSource bitmapSource;

    public SharpDX.Direct3D11.Device d3dDevice;
    public SharpDX.DXGI.Factory d3dFactory;

    public RawMatrix3x2 transform
    {
        get => target.Transform;
        set => target.Transform = value;
    }

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
        d3dDevice ??= DXGraphic.GetDevice();
        d3dFactory ??= DXGraphic.d3dFactory;
        immediateContext = d3dDevice.ImmediateContext;
        var desc = new SwapChainDescription()
        {
            BufferCount = 1,
            ModeDescription = new ModeDescription(Width, Height, new Rational(60, 1), Format.R8G8B8A8_UNorm),
            IsWindowed = true,
            OutputHandle = Handle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.Discard,
            Usage = Usage.RenderTargetOutput
        };
        d3dFactory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAll);
        swapChain = new SwapChain(d3dFactory, d3dDevice, desc);
        CreateRenderTarget(d3dDevice);

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

    private void CreateRenderTarget(Device device)
    {
        backBuffer = SharpDX.Direct3D11.Resource.FromSwapChain<Texture2D>(swapChain, 0);
        backBufferView = new RenderTargetView(device, backBuffer);
        using var surface = backBuffer.QueryInterface<Surface>();
        var format = new SharpDX.Direct2D1.PixelFormat(
            Format.R8G8B8A8_UNorm,
            SharpDX.Direct2D1.AlphaMode.Ignore
        );
        target = new RenderTarget(DXGraphic.d2dFactory, surface, new(
            RenderTargetType.Hardware,
            format, 96, 96,
            RenderTargetUsage.None,
            FeatureLevel.Level_DEFAULT
        ));
        target.AntialiasMode = AntialiasMode.PerPrimitive;
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        d3dDevice = null;
        d3dFactory = null;
        immediateContext = null;
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref tileBrush);
        Utilities.Dispose(ref solidBrush);
        Utilities.Dispose(ref bitmapSource);
        Utilities.Dispose(ref target);
        Utilities.Dispose(ref backBufferView);
        Utilities.Dispose(ref backBuffer);
        Utilities.Dispose(ref swapChain);
        base.OnHandleDestroyed(e);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (target != null)
        {
            Utilities.Dispose(ref target);
            Utilities.Dispose(ref backBufferView);
            Utilities.Dispose(ref backBuffer);
            var desc = swapChain.Description;
            swapChain.ResizeBuffers(
                desc.BufferCount,
                Width,
                Height,
                desc.ModeDescription.Format,
                desc.Flags
            );
            CreateRenderTarget(d3dDevice);
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
        bitmap = null;
        bitmapSource = null;
        imageSize = Size2.Zero;
        Invalidate();
    }

    public void SetImage(SharpDX.Direct2D1.Bitmap bitmap)
    {
        ClearImage();
        this.bitmap = bitmap;
        imageSize = bitmap != null ? bitmap.PixelSize : Size2.Zero;
        Invalidate();
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

    protected override void OnPaintBackground(PaintEventArgs e)
    {
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (target == null) return;
        immediateContext.Rasterizer.SetViewport(new Viewport(0, 0, Width, Height));
        immediateContext.OutputMerger.SetTargets(backBufferView);
        target.BeginDraw();

        target.Transform = Matrix3x2.Identity;
        if (tileBrush != null)
        {
            target.FillRectangle(new RawRectangleF(0, 0, Width, Height), tileBrush);
        }
        else
        {
            target.Clear(new RawColor4(1, 1, 1, 1));
        }
        OnDraw();
        target.EndDraw();
        swapChain.Present(1, PresentFlags.None);
    }

    private void OnDraw()
    {
        if (bitmap != null)
        {
            if (bitmap.IsDisposed)
            {
                bitmap = null;
                return;
            }
            if (onPaint != null)
            {
#if !DEBUG
                try
                {
#endif
                onPaint(this, target, bitmap);
#if !DEBUG
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
#endif
            }
            else
            {
                target.DrawBitmap(bitmap, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
            }
            return;
        }
        if (isError || (bitmapSource != null && bitmapSource.error))
        {
            DrawText("unable to load image", 6, 0, Color.Red, DXGraphic.fontSegoeUI_16);
        }
        else if (bitmapSource == null)
        {
            return;
        }
        else if (bitmapSource.loading)
        {
            DrawText("loading...", 6, 0, Color.Orange, DXGraphic.fontSegoeUI_16);
            Invalidate();
        }
        else if (bitmapSource.state == AsyncImageState.Ready)
        {
            bitmap = bitmapSource.CreateBitmap(target);
            if (!(isError = bitmap == null))
            {
                imageSize = bitmap.PixelSize;
                onBitmapLoaded?.Invoke(this);
                bitmapSource = null;
            }
            Invalidate();
        }
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