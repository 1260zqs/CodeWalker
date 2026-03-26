using SharpDX;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    class ImageCache
    {
        class CacheItem
        {
            public int refCount;
            public DateTime lastUseAt;
            public SharpDX.Direct2D1.Bitmap bitmap;
            public SharpDX.Direct2D1.Bitmap d2dBitmap;
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
                if (item.refCount == 0 && now - item.lastUseAt >= maxCacheAge)
                {
                    keys.Add(pair.Key);
                }
            }
            for (var i = keys.Count - 1; i >= 0; i--)
            {
                cache.Remove(keys[i]);
            }
        }

        public void Clear()
        {
            foreach (var cacheItem in cache.Values)
            {
                Utilities.Dispose(ref cacheItem.bitmap);
            }
            cache.Clear();
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

        public bool TryGetFromPool(string key, out SharpDX.Direct2D1.Bitmap bitmap)
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

        public void ReturnToPool(string key, SharpDX.Direct2D1.Bitmap bitmap)
        {
            if (!cache.TryGetValue(key, out var cacheItem))
            {
                cacheItem = new CacheItem();
                cache.Add(key, cacheItem);
            }
            cacheItem.bitmap = bitmap;
            cacheItem.lastUseAt = DateTime.Now;
            cacheItem.refCount = Math.Max(cacheItem.refCount - 1, 0);
        }
    }

    private ImageCache imageCache = new();

    private void ImageCache_Update()
    {
        imageCache.Update();
    }
}