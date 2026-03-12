using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.DXGI;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;
using Factory2 = SharpDX.DXGI.Factory2;
using Device = SharpDX.Direct3D11.Device;
using DeviceContext = SharpDX.Direct3D11.DeviceContext;

namespace CodeWalker.Rendering;

public partial class DxPreview : UserControl
{
    Device device;
    DeviceContext context;
    SwapChain swapChain;
    RenderTargetView rtv;
    Texture2D backBuffer;

    public DxPreview()
    {
        InitializeComponent();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        // InitDevice();
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        rtv?.Dispose();
        backBuffer?.Dispose();
        swapChain?.Dispose();
        context?.Dispose();
        device?.Dispose();

        rtv = null;
        backBuffer = null;
        swapChain = null;
        context = null;
        device = null;
        base.OnHandleDestroyed(e);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        // if (context == null) return;
        // context.OutputMerger.SetRenderTargets(rtv);
        // context.ClearRenderTargetView(rtv, Color4.Black);
        // swapChain.Present(1, PresentFlags.None);
        // Invalidate();
    }

    public void InitDevice()
    {
        var desc = new SwapChainDescription()
        {
            BufferCount = 2,
            ModeDescription = new ModeDescription(
                Width,
                Height,
                new Rational(60, 1),
                Format.R8G8B8A8_UNorm
            ),
            IsWindowed = true,
            OutputHandle = Handle,
            SampleDescription = new SampleDescription(1, 0),
            SwapEffect = SwapEffect.Discard,
            Usage = Usage.RenderTargetOutput
        };

        Device.CreateWithSwapChain(
            DriverType.Hardware,
            DeviceCreationFlags.BgraSupport,
            desc,
            out device,
            out swapChain
        );

        using (var factory = swapChain.GetParent<Factory>())
        {
            factory.MakeWindowAssociation(Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        context = device.ImmediateContext;
        backBuffer = swapChain.GetBackBuffer<Texture2D>(0);
        rtv = new RenderTargetView(device, backBuffer);
    }

    // protected override void Dispose(bool disposing)
    // {
    //     if (disposing)
    //     {
    //         rtv?.Dispose();
    //         backBuffer?.Dispose();
    //         swapChain?.Dispose();
    //         context?.Dispose();
    //         device?.Dispose();
    //
    //         rtv = null;
    //         backBuffer = null;
    //         swapChain = null;
    //         context = null;
    //         device = null;
    //     }
    //     base.Dispose(disposing);
    // }
}