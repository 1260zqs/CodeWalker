using SharpDX;
using SharpDX.DirectWrite;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeWalker.Graphic;

/// <summary>
/// Adding an embedded font resource to SharpDX.DirectWrite.Factory
/// https://stackoverflow.com/questions/77841104/adding-an-embedded-font-resource-to-sharpdx-directwrite-factory
/// </summary>
internal sealed class ResourceFontLoader : CallbackBase, FontCollectionLoader, FontFileLoader
{
    private readonly List<ResourceFontFileStream> fontStreams = new();
    private readonly List<ResourceFontFileEnumerator> enumerators = new();
    private readonly DataStream keyStream;

    public ResourceFontLoader()
    {
        var buffer = new byte[4 * 1024];
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.EndsWith(".ttf"))
            {
                using var stream = assembly.GetManifestResourceStream(name);
                if (stream == null) continue;
                var dataStream = new DataStream((int)stream.Length, true, true);
                while (true)
                {
                    var read = stream.Read(buffer, 0, buffer.Length);
                    if (read <= 0) break;
                    dataStream.Write(buffer, 0, read);
                }
                dataStream.Position = 0;
                fontStreams.Add(new ResourceFontFileStream(dataStream));
            }
        }

        keyStream = new DataStream(sizeof(int) * fontStreams.Count, true, true);
        for (var i = 0; i < fontStreams.Count; i++)
        {
            keyStream.Write(i);
        }
        keyStream.Position = 0;
    }

    public DataStream Key => keyStream;

    FontFileEnumerator FontCollectionLoader.CreateEnumeratorFromKey(Factory factory, DataPointer collectionKey)
    {
        var enumerator = new ResourceFontFileEnumerator(factory, this, collectionKey);
        enumerators.Add(enumerator);
        return enumerator;
    }

    FontFileStream FontFileLoader.CreateStreamFromKey(DataPointer fontFileReferenceKey)
    {
        var index = Utilities.Read<int>(fontFileReferenceKey.Pointer);
        return fontStreams[index];
    }
}

internal sealed class ResourceFontFileStream : CallbackBase, FontFileStream
{
    private readonly DataStream stream;

    public ResourceFontFileStream(DataStream stream)
    {
        this.stream = stream;
    }

    void FontFileStream.ReadFileFragment(out IntPtr fragmentStart, long fileOffset, long fragmentSize, out IntPtr fragmentContext)
    {
        lock (this)
        {
            fragmentContext = IntPtr.Zero;
            stream.Position = fileOffset;
            fragmentStart = stream.PositionPointer;
        }
    }

    void FontFileStream.ReleaseFileFragment(IntPtr fragmentContext)
    {
        // Nothing to release. No context are used
    }

    long FontFileStream.GetFileSize()
    {
        lock (this)
        {
            return stream.Length;
        }
    }

    long FontFileStream.GetLastWriteTime()
    {
        return 0;
    }
}

internal sealed class ResourceFontFileEnumerator : CallbackBase, FontFileEnumerator
{
    private Factory factory;
    private FontFileLoader loader;
    private DataStream keyStream;
    private FontFile currentFontFile;

    public ResourceFontFileEnumerator(Factory factory, FontFileLoader loader, DataPointer key)
    {
        this.factory = factory;
        this.loader = loader;
        keyStream = new DataStream(key.Pointer, key.Size, true, false);
    }

    bool FontFileEnumerator.MoveNext()
    {
        var moveNext = keyStream.RemainingLength != 0;
        if (moveNext)
        {
            if (currentFontFile != null)
            {
                currentFontFile.Dispose();
            }
            currentFontFile = new FontFile(factory, keyStream.PositionPointer, 4, loader);
            keyStream.Position += 4;
        }
        return moveNext;
    }

    FontFile FontFileEnumerator.CurrentFontFile
    {
        get
        {
            ((IUnknown)currentFontFile).AddReference();
            return currentFontFile;
        }
    }
}