using CodeWalker;
using SharpDX;
using SharpDX.D3DCompiler;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Runtime.InteropServices;

namespace Unity;

public class SceneViewGrid : IDisposable
{
    [StructLayout(LayoutKind.Sequential)]
    struct Vertex
    {
        public Vector2 Position;
        public Vector2 UV;

        public static int sizeInBytes => Utilities.SizeOf<Vertex>();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    struct CameraBufferData
    {
        // @formatter:off
        public RawMatrix InvViewProj;     // Inverse(View * Projection)
        public Vector4 CameraDirection;
        public Vector4 CameraPosition;
        public ViewportF ViewportParams; // (Width, Height, MinDepth, MaxDepth)
        public float GridScale = 1f;
        public float MajorGridDiv = 20f;
        public float FadeDistance = 200f;
        public float LineThickness = 0.04f;
        public float unused;
        public float     unused1;
        //public float     unused2;
        //public float     unused3;

        public CameraBufferData()
        {
            InvViewProj = Matrix.Identity;
            CameraPosition = Vector4.Zero;
        }
        // @formatter:on

        public static int sizeInBytes => Utilities.SizeOf<CameraBufferData>();
        public static int alignedSize => (sizeInBytes + 15) & ~15;
    }

    private VertexBufferBinding vertexBufferBinding;
    private SharpDX.Direct3D11.Buffer vertexBuffer;
    private SharpDX.Direct3D11.Buffer constantBuffer;
    private SharpDX.Direct3D11.Buffer indexBuffer;

    private VertexShader vertexShader;
    private PixelShader pixelShader;
    private InputLayout inputLayout;

    private CameraBufferData cameraBufferData;
    private DepthStencilState gridDepthState;
    private RasterizerState rasterizerState;
    private BlendState gridBlendState;

    public SceneViewGrid(SharpDX.Direct3D11.Device device, string shaderName)
    {
        var vsByte = PathUtil.ReadAllBytes($"{shaderName}VS.cso");
        var psByte = PathUtil.ReadAllBytes($"{shaderName}PS.cso");
        var elements = new[]
        {
            new InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
            new InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0)
        };
        pixelShader = new PixelShader(device, psByte);
        vertexShader = new VertexShader(device, vsByte);
        inputLayout = new InputLayout(device, ShaderSignature.GetInputSignature(vsByte), elements);

        vertexBuffer = SharpDX.Direct3D11.Buffer.Create(
            device,
            BindFlags.VertexBuffer,
            CreateVertex2()
        );
        vertexBufferBinding = new VertexBufferBinding(vertexBuffer, Vertex.sizeInBytes, 0);
        indexBuffer = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.IndexBuffer, new[]
        {
            0u, 1u, 2u, 1u, 3u, 2u
        });
        cameraBufferData = new CameraBufferData();
        constantBuffer = new SharpDX.Direct3D11.Buffer(device,
            CameraBufferData.sizeInBytes,
            ResourceUsage.Default,
            BindFlags.ConstantBuffer,
            CpuAccessFlags.None,
            ResourceOptionFlags.None,
            0
        );
        gridDepthState = new DepthStencilState(device, new DepthStencilStateDescription()
        {
            IsDepthEnabled = true,
            DepthComparison = Comparison.LessEqual,
            DepthWriteMask = DepthWriteMask.Zero,
            IsStencilEnabled = false
        });

        var rasterDesc = new RasterizerStateDescription()
        {
            FillMode = FillMode.Solid,
            CullMode = CullMode.None,
            IsScissorEnabled = false,
            IsDepthClipEnabled = true
        };
        rasterizerState = new RasterizerState(device, rasterDesc);

        // var blendDesc = BlendStateDescription.Default();
        // var renderTarget = blendDesc.RenderTarget;
        // for (var i = 0; i < renderTarget.Length; i++)
        // {
        //     renderTarget[i] = new RenderTargetBlendDescription()
        //     {
        //         IsBlendEnabled = true,
        //         SourceBlend = BlendOption.SourceAlpha,
        //         DestinationBlend = BlendOption.InverseSourceAlpha,
        //         BlendOperation = BlendOperation.Add,
        //         SourceAlphaBlend = BlendOption.One,
        //         DestinationAlphaBlend = BlendOption.Zero,
        //         AlphaBlendOperation = BlendOperation.Add,
        //         RenderTargetWriteMask = ColorWriteMaskFlags.All
        //     };
        // }
        // gridBlendState = new BlendState(device, blendDesc);
    }

    public float gridScale
    {
        get => cameraBufferData.GridScale;
        set => cameraBufferData.GridScale = value;
    }

    public float majorGridDiv
    {
        get => cameraBufferData.MajorGridDiv;
        set => cameraBufferData.MajorGridDiv = value;
    }

    public float fadeDistance
    {
        get => cameraBufferData.FadeDistance;
        set => cameraBufferData.FadeDistance = value;
    }

    public float lineThickness
    {
        get => cameraBufferData.LineThickness;
        set => cameraBufferData.LineThickness = value;
    }

    public void Draw(DeviceContext context, in Matrix invViewProj, Vector3 cameraPos, Vector3 cameraDir)
    {
        // Update constant buffer (same as before)
        cameraBufferData.InvViewProj = invViewProj;
        cameraBufferData.CameraPosition = new Vector4(cameraPos.X, cameraPos.Y, cameraPos.Z, 0);
        cameraBufferData.CameraDirection = new Vector4(cameraDir.X, cameraDir.Y, cameraDir.Z, 0);
        cameraBufferData.ViewportParams = context.Rasterizer.GetViewports<ViewportF>()[0];
        context.UpdateSubresource(ref cameraBufferData, constantBuffer);

        // Set shaders and states
        context.InputAssembler.InputLayout = inputLayout;
        context.VertexShader.Set(vertexShader);
        context.PixelShader.Set(pixelShader);
        context.PixelShader.SetConstantBuffer(0, constantBuffer);

        context.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
        context.InputAssembler.SetVertexBuffers(0, vertexBufferBinding);
        context.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);
        context.DrawIndexed(6, 0, 0);
        Console.WriteLine(cameraPos);

        // ← create once with CullMode.None, FillMode.Solid
        context.Rasterizer.State = rasterizerState;
        // Disable depth write so grid doesn't z-fight
        // Depth-Stencil: disable depth write + usually comparison Always or LessEqual
        context.OutputMerger.DepthStencilState = gridDepthState;
        //context.OutputMerger.DepthStencilReference = 0;
        // Blend state - usually you want alpha blending for the grid
        //context.OutputMerger.BlendState = gridBlendState;

        context.Draw(4, 0);
    }

    // @formatter:off
    private static Vertex[] CreateVertex() =>
    [
        new Vertex { Position = new Vector2(-1.0F, -1.0f), UV = new Vector2(0.0f, 0.0f) },
        new Vertex { Position = new Vector2(1.0f, 0.0f), UV = new Vector2(1.0f, 0.0f) },
        new Vertex { Position = new Vector2(0.0f, 1.0f), UV = new Vector2(0.0f, 1.0f) },
        new Vertex { Position = new Vector2( 1.0f, 1.0f), UV = new Vector2(1.0f, 1.0f) },
    ];
    private static float[] CreateVertex2() =>
    [
        // -1.0f, -1.0f, 0.0f, 1.0f,  1.0f, -1.0f, 1.0f, 1.0f,
        // -1.0f,  1.0f, 0.0f, 0.0f,  1.0f,  1.0f, 1.0f, 0.0f,
        
        -1.0f, -1.0f, 0.0f, 1.0f,       1.0f, -1.0f, 1.0f, 1.0f,
        -1.0f,  1.0f, 0.0f, 0.0f,       1.0f,  1.0f, 1.0f, 0.0f,
    ];
    // @formatter:on

    public void Dispose()
    {
        Utilities.Dispose(ref vertexBuffer);
        Utilities.Dispose(ref constantBuffer);
        Utilities.Dispose(ref vertexShader);
        Utilities.Dispose(ref pixelShader);
        Utilities.Dispose(ref inputLayout);
        Utilities.Dispose(ref gridDepthState);
    }
}