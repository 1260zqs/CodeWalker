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
    protected int mip;
    protected Texture texture;
    protected object stateObject;

    protected AsyncTextureSource()
    {
    }

    public AsyncTextureSource(Texture texture, int mip)
    {
        this.mip = mip;
        this.texture = texture;
    }

    public override void Load()
    {
        if (state == AsyncImageState.None)
        {
            state = AsyncImageState.Loading;
            Task.Run(LoadTexture);
        }
    }

    public override Bitmap CreateBitmap(RenderTarget target)
    {
        if (stateObject != null)
        {
            bitmap = CreateBitmap(target, (StateObject)stateObject);
            stateObject = null;
        }
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
            stateObject = new StateObject()
            {
                data = data,
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

    private Bitmap CreateBitmap(RenderTarget target, StateObject stateObject)
    {
        try
        {
            var ptr = new DataPointer(stateObject.data.DataPointer, stateObject.dataSize);
            var pixelFormat = new PixelFormat(stateObject.format, AlphaMode.Ignore);
            var bmpProps = new BitmapProperties(pixelFormat);
            bitmap = new Bitmap(
                target,
                new Size2(width, height),
                ptr,
                stateObject.ddsRowPitch,
                bmpProps
            );
            Utilities.Dispose(ref stateObject.data);
            state = AsyncImageState.Loaded;
        }
        catch (Exception ex)
        {
            state = AsyncImageState.Error;
            Utilities.Dispose(ref stateObject.data);
            Utilities.Dispose(ref bitmap);
            Console.WriteLine(ex);
        }
        return bitmap;
    }

    public static Format GetPixelFormat(DDSIO.DXGI_FORMAT format)
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

    public override void Dispose()
    {
        base.Dispose();
    }

    public override bool Equals(AsyncBitmapSource other)
    {
        return ReferenceEquals(this, other);
    }

    class StateObject
    {
        public DataStream data;
        public int dataSize;
        public Format format;
        public int ddsRowPitch;
        public int ddsSlicePitch;
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

    public override void Load()
    {
        if (state == AsyncImageState.None)
        {
            state = AsyncImageState.Loading;
            Task.Run(Run);
        }
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