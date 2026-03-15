using System;
using System.Collections.Concurrent;
using CodeWalker.Rendering;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace CodeWalker;

struct Vertex
{
    public Vector2 position;
    public Vector2 texcoord;
}

public enum FilterMode
{
    Point,
    Bilinear,
    Trilinear,
}

public static class DxGraphics
{
    public static object syncroot = new();

    public static Device device;
    public static DeviceContext context;
    public static BlendState alphaBlendState;

    public static DxImageShader imageShader;
    private static ConcurrentDictionary<int, SamplerState> sampler = new();
    private static ImagingFactory2 wic = new();

    public static void Initialize()
    {
        device = CreateDevice();
        context = device.ImmediateContext;
        imageShader = DxImageShader.Create(device, "Shaders\\Image");

        var desc = new BlendStateDescription();
        desc.RenderTarget[0].IsBlendEnabled = true;
        desc.RenderTarget[0].SourceBlend = BlendOption.SourceAlpha;
        desc.RenderTarget[0].DestinationBlend = BlendOption.InverseSourceAlpha;
        desc.RenderTarget[0].BlendOperation = BlendOperation.Add;
        desc.RenderTarget[0].SourceAlphaBlend = BlendOption.One;
        desc.RenderTarget[0].DestinationAlphaBlend = BlendOption.InverseSourceAlpha;
        desc.RenderTarget[0].AlphaBlendOperation = BlendOperation.Add;
        desc.RenderTarget[0].RenderTargetWriteMask = ColorWriteMaskFlags.All;

        alphaBlendState = new BlendState(device, desc);
    }

    private static Device CreateDevice()
    {
        FeatureLevel[] levels =
        [
            FeatureLevel.Level_11_0,
            FeatureLevel.Level_10_1,
            FeatureLevel.Level_10_0
        ];
        return new Device(DriverType.Hardware, DeviceCreationFlags.None, levels);
    }

    public static SamplerState GetSampler(DxImage dxImage)
    {
        var key = (byte)dxImage.filterMode
                  | ((byte)dxImage.wrapModeU << 4)
                  | ((byte)dxImage.wrapModeV << 8)
                  | ((byte)dxImage.wrapModeW << 12);
        return sampler.GetOrAdd(key, GetSampler);
    }

    private static SamplerState GetSampler(int key)
    {
        var filter = (FilterMode)(key & 0xF) switch
        {
            FilterMode.Point => Filter.MinMagMipPoint,
            FilterMode.Bilinear => Filter.MinMagLinearMipPoint,
            FilterMode.Trilinear => Filter.MinMagMipLinear,
            _ => throw new ArgumentOutOfRangeException()
        };
        return new SamplerState(device, new SamplerStateDescription()
        {
            Filter = filter,
            AddressU = (TextureAddressMode)((key >> 4) & 0xF),
            AddressV = (TextureAddressMode)((key >> 8) & 0xF),
            AddressW = (TextureAddressMode)((key >> 12) & 0xF)
        });
    }

    public static Buffer CreateQuad(float width, float height, Vector2 pivot)
    {
        var ox = width * pivot.X;
        var oy = height * pivot.Y;
        Vertex[] verts =
        [
            new Vertex { position = new Vector2(-ox, oy), texcoord = new Vector2(0, 0) },
            new Vertex { position = new Vector2(width - ox, oy), texcoord = new Vector2(1, 0) },
            new Vertex { position = new Vector2(-ox, oy - height), texcoord = new Vector2(0, 1) },
            new Vertex { position = new Vector2(width - ox, oy - height), texcoord = new Vector2(1, 1) }
        ];
        return Buffer.Create(device, BindFlags.VertexBuffer, verts);
    }

    public static DxImage LoadTexture(string file)
    {
        using var decoder = new BitmapDecoder(wic, file, DecodeOptions.CacheOnLoad);
        using var frame = decoder.GetFrame(0);

        using var converter = new FormatConverter(wic);
        converter.Initialize(
            frame,
            PixelFormat.Format32bppBGRA,
            BitmapDitherType.None,
            null,
            0.0,
            BitmapPaletteType.Custom
        );

        var width = converter.Size.Width;
        var height = converter.Size.Height;
        var stride = width * 4;

        using var buffer = new DataStream(height * stride, true, true);
        converter.CopyPixels(stride, buffer);

        var texDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            MipLevels = 1,
            ArraySize = 1,
            Format = Format.B8G8R8A8_UNorm,
            Usage = ResourceUsage.Immutable,
            BindFlags = BindFlags.ShaderResource,
            CpuAccessFlags = CpuAccessFlags.None,
            SampleDescription = new SampleDescription(1, 0)
        };

        var dataRect = new DataRectangle(buffer.DataPointer, stride);
        using var texture = new Texture2D(device, texDesc, dataRect);
        return DxImage.Create(texture, width, height);
    }

    public static void DrawImage(DxImage dxImage)
    {
        DrawImage(dxImage, imageShader);
    }

    public static void DrawImage(DxImage dxImage, DxImageShader shader)
    {
        shader.SetPass(context);
        dxImage.SetPass(context);
        context.Draw(4, 0);
    }
}