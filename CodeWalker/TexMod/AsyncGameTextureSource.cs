using System;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;
using AlphaMode = SharpDX.Direct2D1.AlphaMode;

namespace CodeWalker;

public class AsyncGameTextureSource : AsyncImageSource
{
    private WindowRenderTarget target;
    public TextureModAdapter adapter;
    private GameFile gameFile;
    private string sourceFile;

    public AsyncGameTextureSource(TextureModAdapter adapter, string sourceFile)
    {
        this.adapter = adapter;
        this.sourceFile = sourceFile;
    }

    public override void Load(WindowRenderTarget target)
    {
        if (state == AsyncImageState.None)
        {
            this.target = target;
            state = AsyncImageState.Loading;
            Task.Run(Start);
        }
    }

    async Task Start()
    {
        gameFile = adapter.GetSourceFile(sourceFile);
        if (gameFile == null)
        {
            state = AsyncImageState.Error;
            return;
        }
        gameFile.Use();
        var texName = adapter.GetSourceTextureName(sourceFile);
        while (loading)
        {
            if (gameFile.Loaded)
            {
                var texture = adapter.GetSourceTexture(gameFile, texName);
                if (texture == null)
                {
                    state = AsyncImageState.Error;
                    return;
                }
                try
                {
                    var data = DDSIO.GetPixelDataStream(
                        texture,
                        0,
                        out var dataSize,
                        out width,
                        out height,
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
                    var ptr = new DataPointer(data.DataPointer, dataSize);
                    uploadBitmap = () => UploadBitmap(ptr, data, pixelFormat, ddsRowPitch);
                    state = AsyncImageState.Ready;
                }
                catch (Exception)
                {
                    state = AsyncImageState.Error;
                }
            }
        }
    }

    private Bitmap UploadBitmap(DataPointer pointer, DataStream data, Format format, int pitch)
    {
        try
        {
            var pixelFormat = new PixelFormat(format, AlphaMode.Premultiplied);
            var bmpProps = new BitmapProperties(pixelFormat);
            bitmap = new Bitmap(
                target,
                new Size2(width, height),
                pointer,
                pitch,
                bmpProps
            );
            data.Dispose();
            data = null;
        }
        catch (Exception ex)
        {
            state = AsyncImageState.Error;
            Console.WriteLine(ex);
            data?.Dispose();
        }
        return bitmap;
    }

    private Format GetPixelFormat(DDSIO.DXGI_FORMAT format)
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
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_A8_UNORM: // TextureFormat.D3DFMT_A8
                return Format.A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM: // TextureFormat.D3DFMT_A8B8G8R8
                return Format.R8G8B8A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_R8_UNORM: // TextureFormat.D3DFMT_L8
                return Format.R8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_B8G8R8A8_UNORM: // TextureFormat.D3DFMT_A8R8G8B8
                return Format.B8G8R8A8_UNorm;
            case DDSIO.DXGI_FORMAT.DXGI_FORMAT_B8G8R8X8_UNORM: // TextureFormat.D3DFMT_X8R8G8B8
                return Format.B8G8R8X8_UNorm;
            default:
                return Format.Unknown;
        }
    }

    public override void Dispose()
    {
        uploadBitmap = null;
        Utilities.Dispose(ref bitmap);
    }

    public override bool Equals(AsyncImageSource other)
    {
        if (other is AsyncGameTextureSource x)
        {
            return x.sourceFile == sourceFile;
        }
        return false;
    }
}