using System;
using System.Collections.Generic;
using System.IO;
using SharpDX;
using Rectangle = SharpDX.Rectangle;

namespace CodeWalker.TexMod;

public class TextureModProject
{
    public string workingPath;
    public List<TextureReplacement> replacements = new();
    public SortedList<Guid, ModTexture> modTextures = new();
    public SortedList<Guid, SourceTexture> sourceTextures = new();

    public ModTexture CreateTextureMod(string filename)
    {
        var modTexture = new ModTexture();
        modTexture.id = Guid.NewGuid();
        modTexture.filename = filename;
        modTexture.name = Path.GetFileName(filename);
        modTexture.createdAt = DateTimeOffset.Now;
        modTextures.Add(modTexture.id, modTexture);
        return modTexture;
    }

    public TextureReplacement CreateReplacement()
    {
        var replacement = new TextureReplacement();
        replacement.id = Guid.NewGuid();
        replacements.Add(replacement);
        return replacement;
    }

    public SourceTexture GetOrAddSourceTexture(string sourceFile)
    {
        var sourceTexture = FindSourceTexture(sourceFile);
        if (sourceTexture == null)
        {
            sourceTexture = new SourceTexture();
            sourceTexture.id = Guid.NewGuid();
            sourceTexture.sourceFile = sourceFile;
            sourceTextures.Add(sourceTexture.id, sourceTexture);
        }
        return sourceTexture;
    }

    public List<TextureReplacement> FindTextureReplacements(Guid modeTexId)
    {
        var list = new List<TextureReplacement>();
        FindTextureReplacements(modeTexId, list);
        return list;
    }

    public void FindTextureReplacements(Guid modeTexId, List<TextureReplacement> list)
    {
        list.Clear();
        foreach (var replacement in replacements)
        {
            if (replacement.modTexture == modeTexId)
            {
                list.Add(replacement);
            }
        }
    }

    public SourceTexture FindSourceTexture(string sourceFile)
    {
        foreach (var texture in sourceTextures.Values)
        {
            if (texture.sourceFile == sourceFile)
            {
                return texture;
            }
        }
        return null;
    }

    public List<SourceTexture> FindSourceTextures(Guid modeTexId)
    {
        var list = new List<SourceTexture>();
        foreach (var replacement in replacements)
        {
            if (replacement.modTexture == modeTexId)
            {
                if (sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
                {
                    list.Add(sourceTexture);
                }
            }
        }
        return list;
    }

    public List<string> GetTags()
    {
        var list = new List<string>();
        foreach (var replacement in replacements)
        {
            if (!string.IsNullOrWhiteSpace(replacement.tag))
            {
                list.Add(replacement.tag);
            }
        }
        return list;
    }
}

public class ModTexture
{
    public Guid id;
    public DateTimeOffset createdAt;
    public Rectangle sourceRect;
    public string filename;
    public string name;

    public Vector3 position;
    public Vector3 lookAtDirection;

    public object editorState;
}

public class SourceTexture
{
    public Guid id;
    public string sourceFile;
    public string localFile;
}

public class TextureReplacement
{
    public Guid id;

    public string tag;
    public string name;
    public string comment;

    public Guid modTexture;
    public Guid sourceTexture;
    public Rectangle targetRect;

    public bool flipX;
    public bool flipY;
    public float rotation;

    public object editorState;
}