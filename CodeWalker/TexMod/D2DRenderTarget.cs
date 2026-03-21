using System;
using System.Runtime.InteropServices;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace CodeWalker.TexMod;

public class D2DRenderTarget : IDisposable
{
    private SharpDX.Direct3D11.Device d3dDevice;
    private SharpDX.Direct2D1.Device d2dDevice;
    public SharpDX.Direct2D1.DeviceContext target;

    private SolidColorBrush solidBrush;
    private SharpDX.Direct2D1.Bitmap1 rt;
    private SharpDX.Direct2D1.Bitmap1 bitmap;
    private SharpDX.Direct3D11.Texture2D stagingTexture;
    public SharpDX.Direct3D11.Texture2D targetTexture;

    private SharpDX.Size2 pixelSize;
    private SharpDX.DXGI.Format format;

    public D2DRenderTarget()
    {
        format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
        d3dDevice = new SharpDX.Direct3D11.Device(
            SharpDX.Direct3D.DriverType.Hardware,
            SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport
        );
        using var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
        using var d2dFactory = new SharpDX.Direct2D1.Factory1();
        d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice);
        target = new SharpDX.Direct2D1.DeviceContext(
            d2dDevice,
            SharpDX.Direct2D1.DeviceContextOptions.None
        );

        solidBrush = new SolidColorBrush(target, new RawColor4(0f, 1f, 0, 1f));
    }

    public void BeginDraw()
    {
        target.Target = rt;
        target.BeginDraw();
        target.Clear(new RawColor4(0f, 0, 0, 0));
    }

    public void EndDraw()
    {
        target.EndDraw();
        target.Target = null;
    }

    public byte[] Encode(CodeWalker.Utils.NVTT.Format texFormat, CodeWalker.Utils.NVTT.Quality quality)
    {
        byte[] bytes = null;
        bitmap.CopyFromBitmap(rt);
        var map = bitmap.Map(MapOptions.Read);
        try
        {
            var success = CodeWalker.Utils.NVTT.Compress(
                map.DataPointer,
                pixelSize.Width,
                pixelSize.Height,
                Utils.NVTT.InputFormat.InputFormat_BGRA_8UB,
                texFormat,
                quality,
                out var ptr,
                out var size
            );
            if (success)
            {
                var dataSize = (int)size;
                bytes = new byte[dataSize];
                Marshal.Copy(ptr, bytes, 0, dataSize);
                CodeWalker.Utils.NVTT.FreeBuffer(ptr);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            bitmap.Unmap();
        }
        return bytes;
    }

    public void CopyTo(SharpDX.Direct3D11.Device device, CodeWalker.Rendering.RenderableTexture renderableTexture)
    {
        if (renderableTexture == null || !renderableTexture.IsLoaded) return;
        if (targetTexture == null || targetTexture.IsDisposed)
        {
            var texDesc = new Texture2DDescription
            {
                Format = format,
                Width = pixelSize.Width,
                Height = pixelSize.Height,
                ArraySize = 1,
                MipLevels = 1,
                CpuAccessFlags = CpuAccessFlags.None,
                OptionFlags = ResourceOptionFlags.None,
                BindFlags = BindFlags.ShaderResource,
                SampleDescription = new SampleDescription(1, 0),
            };
            targetTexture = new SharpDX.Direct3D11.Texture2D(device, texDesc);
        }
        if (!ReferenceEquals(renderableTexture.Texture2D, targetTexture))
        {
            renderableTexture.Texture2D?.Dispose();
            renderableTexture.ShaderResourceView?.Dispose();
            renderableTexture.Texture2D = targetTexture;
            renderableTexture.ShaderResourceView = new(device, targetTexture);
        }
        bitmap.CopyFromBitmap(rt);

        var d2dData = bitmap.Map(MapOptions.Read);
        device.ImmediateContext.UpdateSubresource(new(d2dData.DataPointer,
            d2dData.Pitch, pixelSize.Height
        ), stagingTexture);
        bitmap.Unmap();

        device.ImmediateContext.CopySubresourceRegion(
            stagingTexture, 0, null,
            targetTexture, 0
        );
    }

    public void SetTargetSize(SharpDX.Direct3D11.Device device, SharpDX.Size2 pixelSize)
    {
        if (this.pixelSize == pixelSize)
        {
            return;
        }
        this.Release();
        this.pixelSize = pixelSize;
        var texDesc = new Texture2DDescription
        {
            Width = pixelSize.Width,
            Height = pixelSize.Height,
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.None,
            Format = format,
            OptionFlags = ResourceOptionFlags.None,
            CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
            SampleDescription = new SampleDescription(1, 0),
        };

        var bmpProps = new BitmapProperties1(
            new SharpDX.Direct2D1.PixelFormat(
                format,
                SharpDX.Direct2D1.AlphaMode.Premultiplied
            ),
            96, 96,
            BitmapOptions.Target | BitmapOptions.CannotDraw
        );
        var bitmapProperties = new BitmapProperties1
        {
            PixelFormat = bmpProps.PixelFormat,
            BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.CpuRead,
        };

        rt = new SharpDX.Direct2D1.Bitmap1(target, pixelSize, bmpProps);
        bitmap = new SharpDX.Direct2D1.Bitmap1(target, pixelSize, bitmapProperties);
        stagingTexture = new SharpDX.Direct3D11.Texture2D(device, texDesc);
        Utilities.Dispose(ref targetTexture);
    }

    public void FillRectangle(in RawRectangleF rectangle)
    {
        target.FillRectangle(rectangle, solidBrush);
    }

    private void Release()
    {
        Utilities.Dispose(ref rt);
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref stagingTexture);
    }

    public void Dispose()
    {
        Utilities.Dispose(ref rt);
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref stagingTexture);

        Utilities.Dispose(ref target);
        Utilities.Dispose(ref d2dDevice);
        Utilities.Dispose(ref d3dDevice);
    }
}