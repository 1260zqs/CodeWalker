using System;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;

namespace CodeWalker.Graphic;

public class D3DRenderTarget : IDisposable
{
    static D3DRenderTarget()
    {
    }

    static SharpDX.Direct3D11.Device gDevice;
    static SharpDX.Direct3D11.VertexShader vertexShader;
    static SharpDX.Direct3D11.PixelShader pixelShader;
    static SharpDX.Direct3D11.VertexBufferBinding vertexBuffer;
    static SharpDX.Direct3D11.SamplerState sampler;
    static SharpDX.Direct3D11.InputLayout layout;

    public static void Init(SharpDX.Direct3D11.Device device)
    {
        var shaderCode = new byte[10];
        var vsByteCode = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderCode, "VSMain", "vs_5_0");
        var psByteCode = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderCode, "PSMain", "ps_5_0");

        vertexShader = new SharpDX.Direct3D11.VertexShader(device, vsByteCode);
        pixelShader = new SharpDX.Direct3D11.PixelShader(device, psByteCode);

        layout = new SharpDX.Direct3D11.InputLayout(device, vsByteCode, new[]
        {
            new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
            new SharpDX.Direct3D11.InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
        });

        var vertices = new[]
        {
            // pos      uv
            -1f, -1f, 0f, 1f,
            -1f, 1f, 0f, 0f,
            1f, -1f, 1f, 1f,
            1f, 1f, 1f, 0f,
        };
        var vb = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);
        vertexBuffer = new VertexBufferBinding(vb, sizeof(float) * 4, 0);
        sampler = new SharpDX.Direct3D11.SamplerState(device, new SamplerStateDescription()
        {
            Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ComparisonFunction = Comparison.Never,
            MinimumLod = 0,
            MaximumLod = float.MaxValue
        });
    }

    public static SharpDX.Direct3D11.RenderTargetView CreateRenderTarget(int width, int height, SharpDX.DXGI.Format format = SharpDX.DXGI.Format.B8G8R8X8_UNorm)
    {
        return CreateRenderTarget(gDevice, width, height, format);
    }

    public static SharpDX.Direct3D11.RenderTargetView CreateRenderTarget(SharpDX.Direct3D11.Device device, int width, int height, SharpDX.DXGI.Format format = SharpDX.DXGI.Format.B8G8R8X8_UNorm)
    {
        var stagingDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            Format = format,
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.RenderTarget,
            OptionFlags = ResourceOptionFlags.None,
            CpuAccessFlags = CpuAccessFlags.None,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
        };
        var texture = new SharpDX.Direct3D11.Texture2D(device, stagingDesc);
        return new SharpDX.Direct3D11.RenderTargetView(device, texture);
    }

    public static void Blit()
    {
        //Blit(gDevice);
    }

    public static void Draw(
        SharpDX.Direct3D11.Device device,
        SharpDX.Direct3D11.RenderTargetView rtv,
        int width, int height,
        SharpDX.Direct3D11.ShaderResourceView mainTex,
        SharpDX.Direct3D11.ShaderResourceView overlayTex
    )
    {
        var ctx = device.ImmediateContext;
        ctx.OutputMerger.SetRenderTargets(rtv);
        ctx.Rasterizer.SetViewport(0, 0, width, height);

        ctx.InputAssembler.InputLayout = layout;
        ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        ctx.InputAssembler.SetVertexBuffers(0, vertexBuffer);

        ctx.VertexShader.Set(vertexShader);
        ctx.PixelShader.Set(pixelShader);

        //using var srv = new ShaderResourceView(device, sourceTex);
        ctx.PixelShader.SetShaderResource(0, mainTex);
        ctx.PixelShader.SetSampler(0, sampler);

        ctx.Draw(4, 0);
    }

    public void Dispose()
    {
    }
}