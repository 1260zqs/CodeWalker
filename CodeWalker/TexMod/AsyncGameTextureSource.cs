using CodeWalker.GameFiles;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using System;
using System.Threading.Tasks;
using System.Windows.Markup;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace CodeWalker;

public class AsyncTextureSource : AsyncBitmapSource
{
    class TextureFactory : Factory
    {
        public int width;
        public int height;

        public int dataSize;
        public DataStream dataStream;
        public Format format;
        public int ddsRowPitch;
        public int ddsSlicePitch;

        public override Bitmap CreateBitmap(RenderTarget target)
        {
            try
            {
                var ptr = new DataPointer(dataStream.DataPointer, dataSize);
                var pixelFormat = new PixelFormat(format, AlphaMode.Ignore);
                var bmpProps = new BitmapProperties(pixelFormat);
                return new Bitmap(
                    target,
                    new Size2(width, height),
                    ptr,
                    ddsRowPitch,
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

    protected int mip;
    protected Texture texture;

    protected AsyncTextureSource()
    {
    }

    public AsyncTextureSource(Texture texture, int mip)
    {
        this.mip = mip;
        this.texture = texture;
    }

    public override Task LoadAsync()
    {
        if (state == AsyncImageState.None)
        {
            state = AsyncImageState.Loading;
            return Task.Run(LoadTexture);
        }
        return Task.CompletedTask;
    }

    public override Bitmap CreateBitmap(RenderTarget target)
    {
        var bitmap = factory?.CreateBitmap(target);
        if (!shared) Utilities.Dispose(ref factory);
        return bitmap;
    }

    protected void LoadTexture()
    {
        try
        {
            var data = DDSIO.GetPixelDataStream(
                texture,
                mip,
                out var dataSize,
                out var width,
                out var height,
                out var ddsRowPitch,
                out var ddsSlicePitch,
                out var format
            );
            var pixelFormat = GetPixelFormat(format);
            if (pixelFormat == Format.Unknown)
            {
                data.Dispose();
                state = AsyncImageState.Error;
                return;
            }
            factory = new TextureFactory()
            {
                width = width,
                height = height,
                dataStream = data,
                dataSize = dataSize,
                format = pixelFormat,
                ddsRowPitch = ddsRowPitch,
                ddsSlicePitch = ddsSlicePitch,
            };
            state = AsyncImageState.Ready;
        }
        catch (Exception ex)
        {
            state = AsyncImageState.Error;
            Console.WriteLine(ex);
        }
    }

    static Format GetPixelFormat(DDSIO.DXGI_FORMAT format)
    {
        switch (format)
        {
            // compressed
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC1_UNORM: // TextureFormat.D3DFMT_DXT1
                return Format.BC1_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC2_UNORM: // TextureFormat.D3DFMT_DXT3
                return Format.BC2_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC3_UNORM: // TextureFormat.D3DFMT_DXT5
                return Format.BC3_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC4_UNORM: // TextureFormat.D3DFMT_ATI1
                return Format.BC4_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC5_UNORM: // TextureFormat.D3DFMT_ATI2
                return Format.BC5_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_BC7_UNORM: // TextureFormat.D3DFMT_BC7
                return Format.BC7_UNorm;
            // uncompressed
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_B5G5R5A1_UNORM: // TextureFormat.D3DFMT_A1R5G5B5
                return Format.B5G5R5A1_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM: // TextureFormat.D3DFMT_A8B8G8R8
                return Format.R8G8B8A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM: // TextureFormat.D3DFMT_A8R8G8B8
                return Format.B8G8R8A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM: // TextureFormat.D3DFMT_X8R8G8B8
                return Format.B8G8R8X8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM: // TextureFormat.D3DFMT_A8
                return Format.A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM: // TextureFormat.D3DFMT_L8
                return Format.R8_UNorm;
            default:
                return Format.Unknown;
        }
    }

    public override bool Equals(AsyncBitmapSource other)
    {
        return ReferenceEquals(this, other);
    }
}

public class AsyncGameTextureSource : AsyncTextureSource
{
    public TextureModAdapter adapter;
    private GameFile gameFile;
    private string sourceFile;

    public AsyncGameTextureSource(TextureModAdapter adapter, string sourceFile)
    {
        this.adapter = adapter;
        this.sourceFile = sourceFile;
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

    private void Run()
    {
        gameFile = adapter.GetSourceFile(sourceFile);
        if (gameFile == null)
        {
            state = AsyncImageState.Error;
            return;
        }
        gameFile.Use();
        while (loading)
        {
            if (gameFile.Loaded)
            {
                var texName = adapter.GetSourceTextureName(sourceFile);
                texture = adapter.GetSourceTexture(gameFile, texName);
                if (texture == null)
                {
                    state = AsyncImageState.Error;
                    return;
                }
                LoadTexture();
            }
        }
    }

    public override bool Equals(AsyncBitmapSource other)
    {
        if (other is AsyncGameTextureSource x)
        {
            return x.sourceFile == sourceFile;
        }
        return false;
    }
}