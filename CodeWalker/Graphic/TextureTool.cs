using System;
using System.IO;
using System.Runtime.InteropServices;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Device = SharpDX.Direct3D11.Device;

namespace CodeWalker.Graphic;

public static class TextureTool
{
    class BlitShader
    {
        public VertexShader vertexShader;
        public PixelShader pixelShader;
        public InputLayout inputLayout;

        public BlitShader(Device device, string name)
        {
            var vsByte = PathUtil.ReadAllBytes($"Shaders\\{name}VS.cso");
            var psByte = PathUtil.ReadAllBytes($"Shaders\\{name}PS.cso");
            var elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)
            };
            pixelShader = new PixelShader(device, psByte);
            vertexShader = new VertexShader(device, vsByte);
            inputLayout = new InputLayout(device, ShaderSignature.GetInputSignature(vsByte), elements);
        }

        public void Bind(DeviceContext immediateContext)
        {
            immediateContext.PixelShader.Set(pixelShader);
            immediateContext.VertexShader.Set(vertexShader);
            immediateContext.InputAssembler.InputLayout = inputLayout;
        }

        public void UnBind(DeviceContext immediateContext)
        {
            immediateContext.PixelShader.Set(null);
            immediateContext.VertexShader.Set(null);
            immediateContext.InputAssembler.InputLayout = null;
        }
    }

    class LanczosShader
    {
        public VertexShader vertexShader;
        public PixelShader pixelVerticalShader;
        public PixelShader pixelHorizontalShader;
        public InputLayout inputLayout;
        public DownscaleParams shaderParam;
        private SharpDX.Direct3D11.Buffer constantBuffer;

        [StructLayout(LayoutKind.Sequential, Pack = 4)]
        public struct DownscaleParams
        {
            public Vector2 SourceSize;
            public Vector2 TargetSize;
            public float ScaleX;
            public float ScaleY;
            private float padding1; // keep 16-byte alignment
            private float padding2;
        }

        public LanczosShader(Device device, string name)
        {
            var vsByte = PathUtil.ReadAllBytes($"Shaders\\{name}VS.cso");
            var psVByte = PathUtil.ReadAllBytes($"Shaders\\{name}VerticalPS.cso");
            var psHByte = PathUtil.ReadAllBytes($"Shaders\\{name}HorizontalPS.cso");
            var elements = new[]
            {
                new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)
            };

            vertexShader = new VertexShader(device, vsByte);
            pixelVerticalShader = new PixelShader(device, psVByte);
            pixelHorizontalShader = new PixelShader(device, psHByte);
            inputLayout = new InputLayout(device, ShaderSignature.GetInputSignature(vsByte), elements);
            constantBuffer = new SharpDX.Direct3D11.Buffer(
                device,
                Utilities.SizeOf<DownscaleParams>(),
                ResourceUsage.Default,
                BindFlags.ConstantBuffer,
                CpuAccessFlags.None,
                ResourceOptionFlags.None,
                0
            );
        }

        public void UpdateParam(int srcWidth, int srcHeight, int targetWidth, int targetHeight)
        {
            shaderParam.SourceSize = new Vector2(srcWidth, srcHeight);
            shaderParam.TargetSize = new Vector2(targetWidth, targetHeight);
            shaderParam.ScaleX = (float)targetWidth / srcWidth;
            shaderParam.ScaleY = (float)targetHeight / srcHeight;
        }

        public void BindVertical(DeviceContext immediateContext)
        {
            immediateContext.VertexShader.Set(vertexShader);
            immediateContext.PixelShader.Set(pixelVerticalShader);
            immediateContext.InputAssembler.InputLayout = inputLayout;
            immediateContext.UpdateSubresource(ref shaderParam, constantBuffer);
            immediateContext.PixelShader.SetConstantBuffer(0, constantBuffer);
        }

        public void BindHorizontal(DeviceContext immediateContext)
        {
            immediateContext.VertexShader.Set(vertexShader);
            immediateContext.PixelShader.Set(pixelHorizontalShader);
            immediateContext.InputAssembler.InputLayout = inputLayout;
            immediateContext.UpdateSubresource(ref shaderParam, constantBuffer);
            immediateContext.PixelShader.SetConstantBuffer(0, constantBuffer);
        }

        public void UnBind(DeviceContext immediateContext)
        {
            immediateContext.PixelShader.Set(null);
            immediateContext.VertexShader.Set(null);
            immediateContext.InputAssembler.InputLayout = null;
        }
    }

    private static VertexBufferBinding vertexBufferBinding;
    private static SharpDX.Direct3D11.Buffer vertexBuffer;

    private static SamplerState sampler;
    private static BlitShader blitShader;
    private static LanczosShader lanczosShader;

    static TextureTool()
    {
        var device = DXGraphic.GetDevice();
        blitShader = new BlitShader(device, "Blit");
        lanczosShader = new LanczosShader(device, "Lanczos");
        var vertices = new[]
        {
            // @formatter:off
            // pos  uv
            -1f, -1f, 0f, 1f,
            -1f,  1f, 0f, 0f,
             1f, -1f, 1f, 1f,
             1f,  1f, 1f, 0f,
            // @formatter:on
        };
        vertexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);
        vertexBufferBinding = new VertexBufferBinding(vertexBuffer, sizeof(float) * 4, 0);
        sampler = new SharpDX.Direct3D11.SamplerState(device, new SamplerStateDescription()
        {
            Filter = SharpDX.Direct3D11.Filter.MinMagMipLinear,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ComparisonFunction = Comparison.Never,
            MinimumLod = 0,
            MaximumLod = float.MaxValue
        });
    }

    public static void Test()
    {
        // var device = DXGraphic.GetDevice();
        // var texture2D = LoadTextureFromFile(device, "C:/CustomUVChecker_byValle_1K.png");
        // var rtTexture = GetRtTexture(texture2D.Description.Width, texture2D.Description.Height, Format.B8G8R8A8_UNorm);
        // Blit(texture2D, rtTexture);
        // SaveTexture("C:\\image.dds", rtTexture);
        //using var tex = Crop(texture2D, new Rectangle(384, 128, 300, 300));
        //using var downScale = Resize(tex, 100, 100);
        //SaveTexture("C:\\image1.png", downScale);
    }

    public static Texture2D Crop(Texture2D texture, SharpDX.Rectangle srcRect)
    {
        var device = texture.Device;
        var description = texture.Description;
        description.Width = srcRect.Width;
        description.Height = srcRect.Height;
        description.ArraySize = 1;
        description.MipLevels = 1;
        var rtTexture = new Texture2D(device, description);
        device.ImmediateContext.CopySubresourceRegion(
            texture, 0,
            new ResourceRegion(srcRect.Left, srcRect.Top, 0, srcRect.Right, srcRect.Bottom, 1),
            rtTexture, 0
        );
        return rtTexture;
    }

    public static Texture2D Resize(Texture2D texture, int targetWidth, int targetHeight, Format format)
    {
        var device = texture.Device;
        var currentRT = texture;
        var description = texture.Description;
        var currentWidth = description.Width;
        var currentHeight = description.Height;
        while (currentWidth / 2 > targetWidth || currentHeight / 2 > targetHeight)
        {
            var w = Math.Max(targetWidth, currentWidth / 2);
            var h = Math.Max(targetHeight, currentHeight / 2);
            //var nextRT = Lanczos(currentRT, w, h, format);
             var nextRT = GetRtTexture(device, w, h, format);
             Blit(currentRT, nextRT);
            currentRT.Dispose();
            currentRT = nextRT;
            currentWidth = w;
            currentHeight = h;
        }
        return Lanczos(currentRT, targetWidth, targetHeight, format);
    }

    private static Texture2D Lanczos(Texture2D src, int targetWidth, int targetHeight, Format format)
    {
        var device = src.Device;
        var immediateContext = device.ImmediateContext;
        var srcDesc = src.Description;

        var srcWidth = srcDesc.Width;
        var srcHeight = srcDesc.Height;
        using var rtHorizontal = GetRtTexture(device, targetWidth, srcHeight, format);
        var rtFinal = GetRtTexture(device, targetWidth, targetHeight, format);

        lanczosShader.UpdateParam(srcWidth, srcHeight, targetWidth, targetHeight);
        immediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        immediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);

        using (var rtvHorizontal = new RenderTargetView(device, rtHorizontal))
        {
            using var mainTex = new ShaderResourceView(device, src);
            immediateContext.OutputMerger.SetRenderTargets(rtvHorizontal);
            immediateContext.Rasterizer.SetViewport(0, 0, targetWidth, srcHeight);

            lanczosShader.BindHorizontal(immediateContext);
            immediateContext.PixelShader.SetShaderResource(0, mainTex);
            immediateContext.PixelShader.SetSampler(0, sampler);
            immediateContext.Draw(4, 0);
        }
        using (var rtvFinal = new RenderTargetView(device, rtFinal))
        {
            using var mainTex = new ShaderResourceView(device, rtHorizontal);
            immediateContext.OutputMerger.SetRenderTargets(rtvFinal);
            immediateContext.Rasterizer.SetViewport(0, 0, targetWidth, targetHeight);

            lanczosShader.BindVertical(immediateContext);
            immediateContext.PixelShader.SetShaderResource(0, mainTex);
            immediateContext.PixelShader.SetSampler(0, sampler);
            immediateContext.Draw(4, 0);
            lanczosShader.UnBind(immediateContext);
        }
        return rtFinal;
    }

    public static void Blit(Texture2D src, Texture2D dest)
    {
        var device = dest.Device;
        var description = dest.Description;
        using var srv = new ShaderResourceView(device, src);
        using var rtv = new SharpDX.Direct3D11.RenderTargetView(device, dest);

        var immediateContext = device.ImmediateContext;
        immediateContext.OutputMerger.SetRenderTargets(rtv);
        immediateContext.Rasterizer.SetViewport(0, 0, description.Width, description.Height);
        immediateContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        immediateContext.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
        blitShader.Bind(immediateContext);

        immediateContext.PixelShader.SetShaderResource(0, srv);
        immediateContext.PixelShader.SetSampler(0, sampler);
        immediateContext.Draw(4, 0);
        blitShader.UnBind(immediateContext);
    }

    private static Texture2D LoadTextureFromFile(Device device, string filePath)
    {
        using (var wicFactory = new ImagingFactory())
        using (var decoder = new BitmapDecoder(wicFactory, filePath, DecodeOptions.CacheOnDemand))
        using (var frame = decoder.GetFrame(0))
        using (var converter = new FormatConverter(wicFactory))
        {
            converter.Initialize(frame, PixelFormat.Format32bppRGBA, BitmapDitherType.None, null, 0, BitmapPaletteType.Custom);

            var size = frame.Size;
            var rowPitch = size.Width * 4;
            var dataSize = rowPitch * size.Height;

            using (var stream = new DataStream(dataSize, true, true))
            {
                converter.CopyPixels(rowPitch, stream.DataPointer, dataSize);
                var desc = new Texture2DDescription
                {
                    Width = size.Width,
                    Height = size.Height,
                    MipLevels = 1,
                    ArraySize = 1,
                    Format = Format.R8G8B8A8_UNorm,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Default,
                    BindFlags = BindFlags.ShaderResource,
                    CpuAccessFlags = CpuAccessFlags.None,
                    OptionFlags = ResourceOptionFlags.None
                };

                var tex = new Texture2D(device, desc);
                device.ImmediateContext.UpdateSubresource(new DataBox(stream.DataPointer, rowPitch, 0), tex, 0);
                return tex;
            }
        }
    }

    public static void SaveTexture(string filename, Texture2D texture)
    {
        var device = texture.Device;
        var description = texture.Description;
        var width = description.Width;
        var height = description.Height;
        using var cpuTexture = new SharpDX.Direct3D11.Texture2D(device, new()
        {
            Format = Format.B8G8R8A8_UNorm,
            Width = width,
            Height = height,
            ArraySize = 1,
            MipLevels = 1,
            CpuAccessFlags = CpuAccessFlags.Read,
            OptionFlags = ResourceOptionFlags.None,
            BindFlags = BindFlags.None,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Staging,
        });
        var immediateContext = device.ImmediateContext;
        immediateContext.CopySubresourceRegion(
            texture, 0, null,
            cpuTexture, 0
        );
        var dataBox = immediateContext.MapSubresource(
            cpuTexture,
            0,
            MapMode.Read,
            SharpDX.Direct3D11.MapFlags.None
        );
        var bitmap = new System.Drawing.Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
        var bmpData = bitmap.LockBits(
            new System.Drawing.Rectangle(0, 0, width, height),
            System.Drawing.Imaging.ImageLockMode.WriteOnly,
            System.Drawing.Imaging.PixelFormat.Format32bppArgb
        );
        for (var y = 0; y < height; y++)
        {
            var srcPtr = dataBox.DataPointer + y * dataBox.RowPitch;
            var dstPtr = bmpData.Scan0 + y * bmpData.Stride;
            Utilities.CopyMemory(dstPtr, srcPtr, width * 4);
        }
        bitmap.UnlockBits(bmpData);
        immediateContext.UnmapSubresource(cpuTexture, 0);
        bitmap.Save(filename, System.Drawing.Imaging.ImageFormat.Png);
        bitmap.Dispose();
    }

    private static Texture2D GetRtTexture(int width, int height, Format format, int mipLevels = 1)
    {
        var device = DXGraphic.GetDevice();
        return GetRtTexture(device, width, height, format, mipLevels);
    }

    public static Texture2D GetRtTexture(Device device, int width, int height, Format format, int mipLevels = 1)
    {
        return new Texture2D(device, new Texture2DDescription()
        {
            Format = format,
            Width = width,
            Height = height,
            ArraySize = 1,
            MipLevels = mipLevels,
            Usage = ResourceUsage.Default,
            CpuAccessFlags = CpuAccessFlags.None,
            OptionFlags = ResourceOptionFlags.None,
            BindFlags = BindFlags.ShaderResource | BindFlags.RenderTarget,
            SampleDescription = new SampleDescription(1, 0),
        });
    }
}