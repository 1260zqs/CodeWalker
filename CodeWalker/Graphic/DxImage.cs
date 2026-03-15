using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Buffer = SharpDX.Direct3D11.Buffer;

namespace CodeWalker;

public class DxImage : IDisposable
{
    private Texture2D texture;
    private ShaderResourceView srv;
    private Buffer vertexBuffer;
    private bool vertexDirty;

    private int m_Width;
    private int m_Height;
    private float m_PixelsPerUnit = 1f;

    public int width => m_Width;
    public int height => m_Height;

    public float pixelsPerUnit
    {
        get => m_PixelsPerUnit;
        set
        {
            m_PixelsPerUnit = value;
            UpdateVertex();
        }
    }

    public FilterMode filterMode;
    public TextureAddressMode wrapModeU;
    public TextureAddressMode wrapModeV;
    public TextureAddressMode wrapModeW;

    private DxImage()
    {
        wrapModeU = TextureAddressMode.Clamp;
        wrapModeV = TextureAddressMode.Clamp;
        wrapModeW = TextureAddressMode.Clamp;
    }

    public void SetPass(DeviceContext context)
    {
        context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        context.InputAssembler.SetVertexBuffers(0, new VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));

        var sampler = DxGraphics.GetSampler(this);
        context.PixelShader.SetSampler(0, sampler);
        context.PixelShader.SetShaderResource(0, srv);
    }

    private void UpdateVertex()
    {
        vertexDirty = true;
    }

    public static DxImage Create(Texture2D texture, int width, int height)
    {
        var image = new DxImage();
        image.texture = texture;
        image.m_Width = width;
        image.m_Height = height;

        image.vertexBuffer = DxGraphics.CreateQuad(
            width / image.pixelsPerUnit,
            height / image.pixelsPerUnit,
            new Vector2(0, 0)
        );
        image.srv = new ShaderResourceView(DxGraphics.device, texture);
        return image;
    }

    public void Dispose()
    {
        texture = null;
        Utilities.Dispose(ref srv);
        Utilities.Dispose(ref vertexBuffer);
    }
}