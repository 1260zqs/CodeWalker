using System;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Buffer = SharpDX.Direct3D11.Buffer;
using Device = SharpDX.Direct3D11.Device;

namespace CodeWalker;

public class DxImageShader : IDisposable
{
    private VertexShader vertexShader;
    private PixelShader pixelShader;
    private InputLayout inputLayout;

    public void SetPass(DeviceContext context)
    {
        context.InputAssembler.InputLayout = inputLayout;
        context.VertexShader.Set(vertexShader);
        context.PixelShader.Set(pixelShader);
    }

    public static DxImageShader Create(Device device, string path)
    {
        var material = new DxImageShader();
        var vsByte = PathUtil.ReadAllBytes($"{path}VS.cso");
        var psByte = PathUtil.ReadAllBytes($"{path}PS.cso");

        var elements = new[]
        {
            new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)
        };
        material.pixelShader = new PixelShader(device, psByte);
        material.vertexShader = new VertexShader(device, vsByte);
        material.inputLayout = new InputLayout(device, ShaderSignature.GetInputSignature(vsByte), elements);

        return material;
    }

    public void Dispose()
    {
        Utilities.Dispose(ref inputLayout);
        Utilities.Dispose(ref vertexShader);
        Utilities.Dispose(ref pixelShader);
    }
}