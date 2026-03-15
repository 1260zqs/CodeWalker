using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.GameFiles;

namespace CodeWalker.TexMod;

public interface ITextureModAdapter
{
    string MakeSourcePath(GameFile gameFile, string texName);
    GameFile GetSourceFile(string sourcePath);
    string GetSourceTextureName(string sourcePath);
    Texture GetSourceTexture(GameFile gameFile, string texName);
}

public abstract class TextureModAdapter : ITextureModAdapter, IResourceLoader
{
    static TextureModAdapter()
    {
    }

    class LoadingImage
    {
        public string fileName;
        public int referenceCount => callbacks.Count;
        public Dictionary<int, Action<Image>> callbacks = new();
    }

    class LoadedImage
    {
        public Image image;
        public string fileName;
        public int referenceCount;
    }

    public WorldForm worldForm;
    private object locker = new();
    private Dictionary<string, LoadedImage> loadedImages = new();
    private Dictionary<string, LoadingImage> loadingImages = new();
    private volatile int nextHandle;

    public virtual int LoadImage(string fileName, Action<Image> callback)
    {
        lock (locker)
        {
            if (loadedImages.TryGetValue(fileName, out var loadedImage))
            {
                loadedImage.referenceCount++;
                callback(loadedImage.image);
                return 0;
            }
            if (!loadingImages.TryGetValue(fileName, out var loadingImage))
            {
                loadingImage = new LoadingImage();
                loadingImages.Add(fileName, loadingImage);
            }
            var handle = Interlocked.Increment(ref nextHandle);
            loadingImage.callbacks.Add(handle, callback);
            Task.Run(() =>
            {
                var image = Image.FromFile(fileName);
                OnImageLoadDone(fileName, image);
            });
            return handle;
        }
    }

    private void OnImageLoadDone(string filename, Image image)
    {
        lock (locker)
        {
            if (loadingImages.TryGetValue(filename, out var loading))
            {
                loadingImages.Remove(filename);
            }
            if (loading == null || loading.referenceCount == 0)
            {
                image.Dispose();
                return;
            }
            var callbacks = loading.callbacks.Values.ToArray();
            if (!loadedImages.TryGetValue(filename, out var loadedImage))
            {
                loadedImage = new LoadedImage();
                loadedImage.image = image;
                loadedImage.fileName = filename;
                loadedImage.referenceCount = loading.referenceCount;
                loadedImages.Add(filename, loadedImage);
            }
            foreach (var callback in callbacks)
            {
                try
                {
                    callback(image);
                }
                catch (Exception)
                {
                    // ignore
                }
            }
        }
    }

    public virtual void UnLoadImage(int handle)
    {
        if (handle == 0) return;

        lock (locker)
        {
            // if (loadingImages.TryGetValue(handle, out var loading))
            // {
            //     if (--loading.referenceCount <= 0)
            //     {
            //         loadingImages.Remove(handle);
            //     }
            // }
        }
    }

    public abstract string MakeSourcePath(GameFile gameFile, string texName);
    public abstract GameFile GetSourceFile(string sourcePath);
    public abstract string GetSourceTextureName(string sourcePath);
    public abstract Texture GetSourceTexture(GameFile gameFile, string texName);
}