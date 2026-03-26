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
        public int width;
        public int height;
        public DataStream dataStream;

        public ImageFactory(DataStream data, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.dataStream = data;
        }

        public override Bitmap CreateBitmap(RenderTarget target)
        {
            try
            {
                var pixelFormat = new SharpDX.Direct2D1.
                    PixelFormat(Format.R8G8B8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Ignore);
                var bmpProps = new BitmapProperties(pixelFormat);

                var stride = width * 4;
                return new Bitmap(
                    target,
                    new Size2(width, height),
                    new DataPointer(dataStream.DataPointer, stride * height),
                    stride,
                    bmpProps
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
            Utilities.Dispose(ref dataStream);
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
            int width;
            int height;
            byte[] pixels;
            using (var decoder = new BitmapDecoder(DXGraphic.wicFactory, filename, DecodeOptions.CacheOnLoad))
            {
                using var converter = new FormatConverter(DXGraphic.wicFactory);
                using var frame = decoder.GetFrame(0);

                converter.Initialize(
                    frame,
                    SharpDX.WIC.PixelFormat.Format32bppRGBA,
                    BitmapDitherType.None,
                    null,
                    0,
                    BitmapPaletteType.Custom
                );

                width = converter.Size.Width;
                height = converter.Size.Height;

                var stride = width * 4;
                pixels = new byte[stride * height];
                converter.CopyPixels(pixels, stride);
            }

            var data = new DataStream(pixels.Length, true, true);
            data.Write(pixels, 0, pixels.Length);
            data.Position = 0;

            if (disposed)
            {
                data.Dispose();
                return;
            }
            factory = new ImageFactory(data, width, height);
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