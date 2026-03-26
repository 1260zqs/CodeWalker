using System;
using System.Reflection;
using SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;

namespace CodeWalker.Graphic;

public static class DXGraphic
{
    public static readonly SharpDX.Direct2D1.Factory d2dFactory;
    public static readonly SharpDX.WIC.ImagingFactory wicFactory;
    public static readonly SharpDX.DirectWrite.Factory dwriteFactory;

    public static readonly TextFormat fontSegoeUI_16;
    public static readonly TextFormat fontSegoeUI_12;

    public static TextFormat fontPfArmaFive_6;

    static DXGraphic()
    {
        wicFactory = new SharpDX.WIC.ImagingFactory();
        d2dFactory = new SharpDX.Direct2D1.Factory(SharpDX.Direct2D1.FactoryType.MultiThreaded);
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
            "pf_arma_five",
            fontCollection,
            FontWeight.Regular,
            FontStyle.Normal,
            FontStretch.Normal,
            6
        );
    }

    public static SharpDX.Direct2D1.Bitmap LoadEmbeddedBitmap(RenderTarget target, string name)
    {
        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
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