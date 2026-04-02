using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Bitmap = SharpDX.Direct2D1.Bitmap;

namespace CodeWalker.TexMod;

public partial class TextureModDockForm
{
    class ImageCache : IDisposable
    {
        class CacheItem
        {
            public int refCount;
            public DateTime lastTime;
            public SharpDX.Direct2D1.Bitmap bitmap;
        }

        private List<string> keys = new();
        private Dictionary<string, CacheItem> cache = new();
        public TimeSpan maxCacheAge = TimeSpan.FromMinutes(1);

        public void Update()
        {
            keys.Clear();
            var now = DateTime.Now;
            foreach (var pair in cache)
            {
                var item = pair.Value;
                if (item.refCount <= 0 && now - item.lastTime >= maxCacheAge)
                {
                    keys.Add(pair.Key);
                }
            }
            for (var i = keys.Count - 1; i >= 0; i--)
            {
                cache.Remove(keys[i]);
            }
        }

        public SharpDX.Direct2D1.Bitmap GetFromPool(string key)
        {
            if (cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem.refCount++;
                return cacheItem.bitmap;
            }
            return null;
        }

        public bool TryGetFromCache(string key, out SharpDX.Direct2D1.Bitmap bitmap)
        {
            if (cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem.refCount++;
                bitmap = cacheItem.bitmap;
                return true;
            }
            bitmap = null;
            return false;
        }

        public void ReturnToCache(string key, SharpDX.Direct2D1.Bitmap bitmap)
        {
            if (!cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem = new CacheItem();
                cache.Add(key, cacheItem);
            }
            cacheItem.refCount--;
            cacheItem.bitmap = bitmap;
            cacheItem.lastTime = DateTime.Now;
        }

        public void AddToCache(string key, Bitmap bitmap)
        {
            if (!cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem = new CacheItem();
                cacheItem.refCount = 1;
                cacheItem.bitmap = bitmap;
                cache.Add(key, cacheItem);
            }
        }

        public void UpdateCacheItem(string key, Bitmap bitmap)
        {
            if (cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem.bitmap = bitmap;
            }
        }

        public void Dispose()
        {
            foreach (var cacheItem in cache.Values)
            {
                Utilities.Dispose(ref cacheItem.bitmap);
            }
            cache.Clear();
        }
    }

    private ImageCache imageCache = new();

    private void ImageCache_Update()
    {
        imageCache.Update();
    }
}