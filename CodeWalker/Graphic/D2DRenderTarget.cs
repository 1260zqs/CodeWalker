using System;
using System.Runtime.InteropServices;
using System.Threading;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;

namespace CodeWalker.Graphic;

public class D2DRenderTarget : IDisposable
{
    private SharpDX.Direct2D1.Device d2dDevice;
    public SharpDX.Direct2D1.RenderTarget target;

    private SolidColorBrush solidBrush;
    private SharpDX.Direct3D11.Texture2D rtTexture;
    private SharpDX.Direct3D11.Texture2D stagingTexture;
    private SharpDX.Direct3D11.Texture2D targetTexture;

    private SharpDX.Size2 pixelSize;
    private SharpDX.DXGI.Format format;

    private SharpDX.Direct3D11.Device d3dDevice;
    private SharpDX.Direct2D1.Factory1 d2dFactory;
    private IntPtr sharedHandle;
    private KeyedMutex mutexA;
    private KeyedMutex mutexB;
    private bool targetTextureInvalid;

    private const int kMutexKey = 0;

    public D2DRenderTarget(SharpDX.Direct3D11.Device d3dDevice, SharpDX.Direct2D1.Factory1 d2dFactory)
    {
        format = SharpDX.DXGI.Format.R8G8B8A8_UNorm;
        this.d3dDevice = d3dDevice;
        this.d2dFactory = d2dFactory;
    }

    public void BeginDraw()
    {
        mutexA.Acquire(kMutexKey, 100);
        target.BeginDraw();
        target.Clear(new RawColor4());
    }

    public void EndDraw()
    {
        target.EndDraw();
        mutexA.Release(kMutexKey);
    }

    public byte[] EncodeTexture(CodeWalker.Utils.NVTT.Format texFormat, CodeWalker.Utils.NVTT.Quality quality)
    {
        byte[] bytes = null;
        mutexA.Acquire(kMutexKey, 100);
        try
        {
            using var cpuTexture = new SharpDX.Direct3D11.Texture2D(d3dDevice, new()
            {
                Format = Format.B8G8R8A8_UNorm,
                Width = pixelSize.Width,
                Height = pixelSize.Height,
                ArraySize = 1,
                MipLevels = 1,
                CpuAccessFlags = CpuAccessFlags.Read,
                OptionFlags = ResourceOptionFlags.None,
                BindFlags = BindFlags.None,
                SampleDescription = new SampleDescription(1, 0),
            });
            var immediateContext = d3dDevice.ImmediateContext;
            immediateContext.CopySubresourceRegion(
                rtTexture, 0, null,
                cpuTexture, 0
            );
            var dataBox = immediateContext.MapSubresource(
                cpuTexture,
                0,
                MapMode.Read,
                SharpDX.Direct3D11.MapFlags.None
            );
            var success = CodeWalker.Utils.NVTT.Compress(
                dataBox.DataPointer,
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
            immediateContext.UnmapSubresource(cpuTexture, 0);
        }
        finally
        {
            mutexA.Release(kMutexKey);
        }
        return bytes;
    }

    public void CopyTo(SharpDX.Direct3D11.Device device, CodeWalker.Rendering.RenderableTexture renderableTexture)
    {
        if (renderableTexture == null || !renderableTexture.IsLoaded) return;
        if (targetTexture == null || targetTexture.IsDisposed || targetTextureInvalid)
        {
            targetTextureInvalid = false;
            targetTexture = new SharpDX.Direct3D11.Texture2D(device, new()
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
            });
        }
        if (!ReferenceEquals(renderableTexture.Texture2D, targetTexture))
        {
            renderableTexture.Texture2D?.Dispose();
            renderableTexture.ShaderResourceView?.Dispose();
            renderableTexture.Texture2D = targetTexture;
            renderableTexture.ShaderResourceView = new(device, targetTexture);
        }
        if (stagingTexture == null)
        {
            stagingTexture = device.OpenSharedResource<Texture2D>(sharedHandle);
            mutexB = stagingTexture.QueryInterface<KeyedMutex>();
        }

        mutexB.Acquire(kMutexKey, 100);
        try
        {
            device.ImmediateContext.CopySubresourceRegion(
                stagingTexture, 0, null,
                targetTexture, 0
            );
        }
        finally
        {
            mutexB.Release(kMutexKey);
        }
    }

    public void SetTargetSize(SharpDX.Size2 pixelSize)
    {
        if (this.pixelSize == pixelSize)
        {
            return;
        }
        this.Release();
        this.pixelSize = pixelSize;
        targetTextureInvalid = true;

        rtTexture = new SharpDX.Direct3D11.Texture2D(d3dDevice, new()
        {
            Format = format,
            Width = pixelSize.Width,
            Height = pixelSize.Height,
            ArraySize = 1,
            MipLevels = 1,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.SharedKeyedmutex,
            BindFlags = BindFlags.RenderTarget,
            SampleDescription = new SampleDescription(1, 0),
        });

        mutexA = rtTexture.QueryInterface<KeyedMutex>();
        using var surface = rtTexture.QueryInterface<Surface>();
        var pixelFormat = new SharpDX.Direct2D1.PixelFormat(
            format,
            SharpDX.Direct2D1.AlphaMode.Premultiplied
        );
        target = new RenderTarget(d2dFactory, surface, new(
            RenderTargetType.Hardware,
            pixelFormat, 96, 96,
            RenderTargetUsage.None,
            FeatureLevel.Level_DEFAULT
        ));

        using var dxgi = rtTexture.QueryInterface<SharpDX.DXGI.Resource>();
        sharedHandle = dxgi.SharedHandle;
        solidBrush = new SolidColorBrush(target, new RawColor4(0f, 1f, 0, 1f));
    }

    public void FillRectangle(in RawRectangleF rectangle)
    {
        solidBrush.Color = new RawColor4(0, 1, 0, 1);
        target.FillRectangle(rectangle, solidBrush);
    }

    private void Release()
    {
        pixelSize = Size2.Zero;
        sharedHandle = IntPtr.Zero;
        Utilities.Dispose(ref stagingTexture);

        Utilities.Dispose(ref rtTexture);
        Utilities.Dispose(ref target);
        Utilities.Dispose(ref mutexA);
        Utilities.Dispose(ref mutexB);
    }

    public void Dispose()
    {
        Release();
        GC.SuppressFinalize(this);
    }
}