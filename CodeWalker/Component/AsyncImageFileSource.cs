using System;
using System.Threading.Tasks;
using CodeWalker.Graphic;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using SharpDX.WIC;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker;

public class AsyncImageFileSource : AsyncBitmapSource
{
    class ImageFactory : Factory
    {
        FormatConverter converter;

        public ImageFactory(FormatConverter converter)
        {
            this.converter = converter;
        }

        public override Bitmap CreateBitmap(RenderTarget target)
        {
            try
            {
                var pixelFormat = new SharpDX.Direct2D1.PixelFormat(
                    Format.B8G8R8A8_UNorm,
                    SharpDX.Direct2D1.AlphaMode.Premultiplied
                );
                return Bitmap.FromWicBitmap(
                    target,
                    converter,
                    new BitmapProperties(pixelFormat)
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return null;
        }

        public override void Dispose()
        {
            Utilities.Dispose(ref converter);
        }
    }

    public readonly string filename;

    public AsyncImageFileSource(string filename)
    {
        this.filename = filename;
    }

    public override Task LoadAsync()
    {
        if (state == AsyncImageState.None)
        {
            state = AsyncImageState.Loading;
            return Task.Run(Run);
        }
        return Task.CompletedTask;
    }

    public override Bitmap CreateBitmap(RenderTarget target)
    {
        var bitmap = factory?.CreateBitmap(target);
        if (!shared) Utilities.Dispose(ref factory);
        return bitmap;
    }

    private void Run()
    {
        try
        {
            using var decoder = new BitmapDecoder(DXGraphic.wicFactory, filename, DecodeOptions.CacheOnLoad);
            using var frame = decoder.GetFrame(0);
            var converter = new FormatConverter(DXGraphic.wicFactory);

            converter.Initialize(
                frame,
                SharpDX.WIC.PixelFormat.Format32bppBGRA,
                BitmapDitherType.None,
                null,
                0,
                BitmapPaletteType.Custom
            );

            if (disposed)
            {
                converter.Dispose();
                return;
            }
            factory = new ImageFactory(converter);
            state = AsyncImageState.Ready;
        }
        catch (Exception ex)
        {
            state = AsyncImageState.Error;
            Console.WriteLine(ex);
        }
    }

    public override bool Equals(AsyncBitmapSource other)
    {
        if (other is AsyncImageFileSource x)
        {
            return x.filename == filename;
        }
        return false;
    }
}