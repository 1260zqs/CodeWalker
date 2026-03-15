// DxImageControl.cs
// Pure D3D11 WinForms control (Texture2D rendering) using external Device

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using CodeWalker.Rendering;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.WIC;
using Buffer = SharpDX.Direct3D11.Buffer;
using Color = SharpDX.Color;
using Device = SharpDX.Direct3D11.Device;

namespace CodeWalker;

public class DxImageControl : Control
{
    static Device device => DxGraphics.device;
    static DeviceContext context => DxGraphics.context;

    SwapChain swapChain;
    Texture2D backBuffer;
    RenderTargetView rtv;
    Buffer drawingBuffer;
    DrawCB drawCB;
    DxImage image;
    private float scaling;
    private Vector2 translation;

    struct DrawCB
    {
        public Matrix ObjectToWorld;
        public Matrix ViewProjection;
        public Vector4 ClipRect;
    }

    public DxImageControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);
        PictureBoxViewer.AddFeature(this);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        if (DxGraphics.device == null) return;
        CreateSwapChain();
    }

    private void CreateSwapChain()
    {
        var factory = new Factory1();
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
        swapChain = new SwapChain(factory, device, desc);

        backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        rtv = new RenderTargetView(device, backBuffer);
        drawingBuffer = new Buffer(device,
            Utilities.SizeOf<DrawCB>(),
            ResourceUsage.Default,
            BindFlags.ConstantBuffer,
            CpuAccessFlags.None,
            ResourceOptionFlags.None,
            0
        );
    }

    public void SetTexture(string file)
    {
        if (device == null) return;
        Utilities.Dispose(ref image);
        image = DxGraphics.LoadTexture(file);
        PictureBoxViewer.ResetViewer(this);
        Invalidate();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (swapChain == null) return;

        Utilities.Dispose(ref rtv);
        Utilities.Dispose(ref backBuffer);

        swapChain.ResizeBuffers(1, Width, Height, Format.R8G8B8A8_UNorm, SwapChainFlags.None);
        backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        rtv = new RenderTargetView(device, backBuffer);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (swapChain == null)
        {
            if (device != null)
            {
                CreateSwapChain();
            }
            Invalidate();
            return;
        }
        if (!Monitor.TryEnter(DxGraphics.syncroot, 10))
        {
            Invalidate();
            return;
        }

        //var rsDesc = new RasterizerStateDescription()
        //{
        //    CullMode = CullMode.Back,
        //    FillMode = FillMode.Solid,
        //    IsDepthClipEnabled = true,
        //    IsScissorEnabled = false
        //};

        context.OutputMerger.SetRenderTargets(rtv);
        context.OutputMerger.SetBlendState(DxGraphics.alphaBlendState);
        //using var rasterState = new RasterizerState(device, rsDesc);
        //context.Rasterizer.State = rasterState;
        context.Rasterizer.SetViewport(0, 0, Width, Height);
        context.ClearRenderTargetView(rtv, new Color(0.5f));

        if (image != null)
        {
            PictureBoxViewer.GetState(this, out scaling, out translation);
            //drawCB.ViewProjection = Matrix.OrthoRH(Width, Height, 0, 1);
            //var view = Matrix.LookAtLH(new Vector3(0, 0, -1), Vector3.Zero, Vector3.UnitY);
            drawCB.ViewProjection = Matrix.OrthoOffCenterLH(
                0, Width,
                0, Height,
                0, 1
            );
            SetClipRect(translation.X, translation.Y, image.width * scaling, image.height * scaling);
            //drawCB.ClipRect = new Vector4(0, 0, Width, Height);

            DrawImage(image);
            DrawImage(image, 12, 12);
        }

        swapChain.Present(1, PresentFlags.None);
        Monitor.Exit(DxGraphics.syncroot);
        //Invalidate();
    }

    private void SetClipRect(float x, float y, float width, float height)
    {
        drawCB.ClipRect = new Vector4(x, Height - (y + height), x + width, Height - y);
    }

    public void DrawImage(DxImage dxImage, int x = 0, int y = 0, DxImageShader shader = null)
    {
        shader ??= DxGraphics.imageShader;
        shader.SetPass(context);

        drawCB.ObjectToWorld = Matrix.Translation(x, y, 0) * Matrix.Scaling(scaling, scaling, 1) *
                               Matrix.Translation(translation.X, Height - translation.Y, 0);
        //Matrix.Translation(translation.X, Height - translation.Y, 0);
        //Matrix.Translation(translation.X, translation.Y, 0);

        context.UpdateSubresource(ref drawCB, drawingBuffer);
        context.VertexShader.SetConstantBuffer(0, drawingBuffer);
        context.PixelShader.SetConstantBuffer(0, drawingBuffer);

        dxImage.SetPass(context);
        context.Draw(4, 0);
    }

    protected override void Dispose(bool disposing)
    {
        Utilities.Dispose(ref image);
        Utilities.Dispose(ref rtv);
        Utilities.Dispose(ref backBuffer);
        Utilities.Dispose(ref swapChain);
        base.Dispose(disposing);
    }
}