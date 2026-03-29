using System;
using System.Reflection;
using SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace CodeWalker.Graphic;

public static class DXGraphic
{
    public static readonly SharpDX.Direct2D1.Factory1 d2dFactory;
    public static readonly SharpDX.WIC.ImagingFactory wicFactory;
    public static readonly SharpDX.DirectWrite.Factory dwriteFactory;

    public static readonly TextFormat fontSegoeUI_16;
    public static readonly TextFormat fontSegoeUI_12;

    public static TextFormat fontPfArmaFive_6;

    public static SharpDX.DXGI.Factory d3dFactory;
    private static SharpDX.Direct3D11.Device s_d3dDevice;
    public static SharpDX.Direct3D11.DeviceContext immediateContext;

    static DXGraphic()
    {
        wicFactory = new SharpDX.WIC.ImagingFactory();
        d2dFactory = new SharpDX.Direct2D1.Factory1(SharpDX.Direct2D1.FactoryType.MultiThreaded);
        dwriteFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);

        fontSegoeUI_16 = new TextFormat(dwriteFactory, "Segoe UI", 16f);
        fontSegoeUI_12 = new TextFormat(dwriteFactory, "Segoe UI", 12f);
    }

    public static void Initialize()
    {
        var fontLoader = new ResourceFontLoader();
        dwriteFactory.RegisterFontFileLoader(fontLoader);
        dwriteFactory.RegisterFontCollectionLoader(fontLoader);

        var fontCollection = new FontCollection(dwriteFactory, fontLoader, fontLoader.Key);
        fontPfArmaFive_6 = new TextFormat(
            dwriteFactory,
            "PF Arma Five",
            fontCollection,
            FontWeight.Normal,
            FontStyle.Normal,
            FontStretch.Normal,
            6
        );
    }

    public static SharpDX.Direct3D11.Device GetDevice()
    {
        if (s_d3dDevice == null)
        {
            d3dFactory = new SharpDX.DXGI.Factory1();
            s_d3dDevice = new SharpDX.Direct3D11.Device(
                SharpDX.Direct3D.DriverType.Hardware,
                SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport,
                SharpDX.Direct3D.FeatureLevel.Level_11_0
            );
            immediateContext = s_d3dDevice.ImmediateContext;
        }
        return s_d3dDevice;
    }

    public static SharpDX.Direct2D1.Bitmap LoadEmbeddedBitmap(RenderTarget target, string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
        if (stream == null) return null;
        using var wicStream = new WICStream(wicFactory, stream);
        using var decoder = new BitmapDecoder(
            wicFactory,
            wicStream,
            DecodeOptions.CacheOnLoad
        );

        using var frame = decoder.GetFrame(0);
        using var converter = new FormatConverter(wicFactory);
        converter.Initialize(
            frame,
            SharpDX.WIC.PixelFormat.Format32bppRGB,
            BitmapDitherType.None,
            null,
            0,
            BitmapPaletteType.Custom
        );
        return SharpDX.Direct2D1.Bitmap.FromWicBitmap(target, converter);
    }

    public static TextLayout CreateTextLayout(string text, TextFormat font = null)
    {
        return new TextLayout(
            dwriteFactory,
            text,
            font ?? fontSegoeUI_12,
            float.MaxValue,
            float.MaxValue
        );
    }
}