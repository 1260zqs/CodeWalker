using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.GameFiles;
using CodeWalker.Utils;

namespace CodeWalker.TexMod;

public class GTAVTextureModAdapter : TextureModAdapter
{
    public WorldForm worldForm;
    public TextureModProject project;

    public GTAVTextureModAdapter(TextureModProject project, WorldForm form)
    {
        this.project = project;
        this.worldForm = form;
    }

    public override string MakeSourcePath(GameFile gameFile, string texName)
    {
        if (gameFile is YdrFile ydr)
        {
            //return null;
        }
        return $"{gameFile.RpfFileEntry.Path}:{texName}";
    }

    public override string GetSourceTextureName(string sourcePath)
    {
        var indexOf = sourcePath.IndexOf(':');
        if (indexOf > 0)
        {
            return sourcePath.Substring(indexOf + 1);
        }
        return null;
    }

    public override GameFile GetSourceFile(string sourcePath)
    {
        var indexOf = sourcePath.IndexOf(':');
        if (indexOf > 0)
        {
            var entryPath = sourcePath.Substring(0, indexOf);
            if (project.manifest != null)
            {
                var modEntryPath = project.manifest.FindArchiveFileSource(entryPath);
                if (!string.IsNullOrEmpty(modEntryPath))
                {
                    var path = Path.GetDirectoryName(project.manifestFile);
                    var filename = Path.Combine(path, "content", modEntryPath);
                    if (File.Exists(filename))
                    {
                        var file = GameFileUtils.LoadFile(filename);
                        if (file == null)
                        {
                            return file;
                        }
                    }
                }
            }
            var entry = worldForm.GameFileCache.RpfMan.GetEntry(entryPath);
            if (entry != null)
            {
                var gameFile = worldForm.GameFileCache.GetFile(entry);
                if (gameFile != null)
                {
                    if (gameFile.Loaded)
                    {
                        // just hit return
                    }
                    else if (gameFile.LoadQueued)
                    {
                    }
                    else if (gameFile is PackedFile packedFile)
                    {
                        var data = entry.File.ExtractFile((RpfFileEntry)entry);
                        if (data != null)
                        {
                            packedFile.Load(data, (RpfFileEntry)entry);
                        }
                    }
                }
                return gameFile;
            }
        }
        return null;
    }

    public override Texture GetSourceTexture(GameFile gameFile, string texName)
    {
        return GameFileUtils.FindTexture(gameFile, texName);
    }
}