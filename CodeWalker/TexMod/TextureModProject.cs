using CodeWalker.Utils;
using SharpDX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace CodeWalker.TexMod;

public class TextureModProject
{
    public string manifestFile;
    public PackageManifest manifest;
    public ProjectDirectory directory = new();
    public List<TextureMapping> textureMappings = new();
    public SortedList<Guid, ModTexture> modTextures = new();
    public SortedList<Guid, SourceTexture> sourceTextures = new();

    public static TextureModProject SetupWorkingProject(string workingDir)
    {
        var project = new TextureModProject();
        var projectFile = Path.Combine(workingDir, "texturemod.xml");
        if (File.Exists(projectFile))
        {
            try
            {
                project.Load(projectFile);
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
        }
        //MakesureFolder(Path.Combine(workingDir, "content"));
        MakesureFolder(Path.Combine(workingDir, "working"));
        return project;
    }

    public void Save(string workingDir)
    {
        var filename = Path.Combine(workingDir, "texturemod.xml");
        TextureModProjectExtension.Save(this, filename);
    }

    public void SaveManifest()
    {
        var settings = new XmlWriterSettings
        {
            Indent = true,
            Encoding = Encoding.UTF8
        };
        using (var writer = XmlWriter.Create(manifestFile, settings))
        {
            manifest.WriteTo(writer);
            writer.Flush();
        }
    }

    private static void MakesureFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
    }

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

    public TextureMapping CreateReplacement()
    {
        var replacement = new TextureMapping();
        replacement.id = Guid.NewGuid();
        textureMappings.Add(replacement);
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

    public List<TextureMapping> FindTextureMapping(Guid modeTexId)
    {
        var list = new List<TextureMapping>();
        FindTextureMapping(modeTexId, list);
        return list;
    }

    public void FindTextureMapping(Guid modeTexId, List<TextureMapping> list)
    {
        list.Clear();
        foreach (var replacement in textureMappings)
        {
            if (replacement.modTexture == modeTexId)
            {
                list.Add(replacement);
            }
        }
    }

    public List<TextureMapping> FindSourceTextureMapping(Guid sourceTexId)
    {
        var list = new List<TextureMapping>();
        foreach (var replacement in textureMappings)
        {
            if (replacement.sourceTexture == sourceTexId)
            {
                list.Add(replacement);
            }
        }
        return list;
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
        foreach (var replacement in textureMappings)
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
        foreach (var replacement in textureMappings)
        {
            if (!string.IsNullOrWhiteSpace(replacement.tag))
            {
                list.Add(replacement.tag);
            }
        }
        return list;
    }

    public void DeleteTextureMapping(TextureMapping mapping)
    {
        for (var i = 0; i < textureMappings.Count; i++)
        {
            if (textureMappings[i].id == mapping.id)
            {
                textureMappings.RemoveAt(i);
                break;
            }
        }
        foreach (var textureReplacement in textureMappings)
        {
            if (mapping.sourceTexture == textureReplacement.sourceTexture)
            {
                return;
            }
        }
        sourceTextures.Remove(mapping.sourceTexture);
    }

    public void DeleteModTexture(ModTexture modTexture)
    {
        modTextures.Remove(modTexture.id);
        var mappings = FindTextureMapping(modTexture.id);
        foreach (var mapping in mappings)
        {
            DeleteTextureMapping(mapping);
        }
    }
}

public class ProjectDirectory
{
    public string name;
    public ProjectDirectory parent;
    public List<ProjectDirectory> directories = new();
    public HashSet<Guid> files = new();

    public bool isRoot => parent == null;

    public string path
    {
        get
        {
            if (parent == null) return "/";
            if (parent.isRoot) return parent.path + name;
            return $"{parent.path}/{name}";
        }
    }

    private void Rebuild()
    {
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
    public Vector3 rotation;

    public object editorState;

    public ModTexture Clone()
    {
        var clone = new ModTexture();
        clone.id = id;
        clone.createdAt = createdAt;
        clone.sourceRect = sourceRect;
        clone.filename = filename;
        clone.name = name;

        clone.position = position;
        clone.rotation = rotation;
        return clone;
    }
}

public class SourceTexture
{
    public Guid id;
    public string sourceFile;
    public string localFile;
}

public class TextureMapping
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

    public TextureMapping Clone()
    {
        var clone = new TextureMapping();
        clone.id = id;

        clone.tag = tag;
        clone.name = name;
        clone.comment = comment;
        clone.modTexture = modTexture;

        clone.sourceTexture = sourceTexture;
        clone.targetRect = targetRect;

        clone.flipX = flipX;
        clone.flipY = flipY;
        clone.rotation = rotation;
        return clone;
    }
}