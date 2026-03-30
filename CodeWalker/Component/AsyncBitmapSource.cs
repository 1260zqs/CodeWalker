using System;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Direct2D1;

namespace CodeWalker;

public enum AsyncImageState : byte
{
    None,
    Loading,
    Ready,
    Loaded,
    Error,
    Disposed
}

public abstract class AsyncBitmapSource : IDisposable
{
    public abstract class Factory : IDisposable
    {
        public abstract Bitmap CreateBitmap(RenderTarget target);
        public abstract void Dispose();
    }

    public bool shared;
    public Factory factory;
    public AsyncImageState state;

    public bool disposed => state == AsyncImageState.Disposed;
    public bool loading => state == AsyncImageState.Loading;
    public bool error => state == AsyncImageState.Error;
    public bool ready => state == AsyncImageState.Ready;

    public abstract Bitmap CreateBitmap(RenderTarget target);
    public abstract Task LoadAsync();
    public abstract bool Equals(AsyncBitmapSource other);

    public virtual void Dispose()
    {
        state = AsyncImageState.Disposed;
        Utilities.Dispose(ref factory);
    }
}