using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeWalker.GameFiles;

namespace CodeWalker.TexMod;

public class GTAVTextureModAdapter : TextureModAdapter
{
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
        Texture texture = null;
        var hash = JenkHash.GenHash(texName.ToLowerInvariant());
        if (gameFile is YtdFile ytd)
        {
            texture = ytd.TextureDict?.Lookup(hash);
        }
        else if (gameFile is YddFile ydd)
        {
        }
        else if (gameFile is YdrFile ydr)
        {
            texture = ydr.Drawable.ShaderGroup.TextureDictionary.Lookup(hash);
        }
        else if (gameFile is YftFile yft)
        {
        }
        return texture;
    }
}