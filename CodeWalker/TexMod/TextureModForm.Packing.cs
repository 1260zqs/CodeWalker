using System;
using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using SharpDX;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    private void PackMod()
    {
        var directoryName = Path.GetDirectoryName(project.manifestFile);
        var targetFolder = Path.Combine(directoryName, "content", "texmod");
        if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

        // 1. extract file
        var nameTable = new Dictionary<Guid, string>();
        foreach (var kv in project.sourceTextures)
        {
            var sourceTexture = kv.Value;
            var sourceFileName = adapter.GetSourceFileName(sourceTexture.sourceFile);
            var filename = Path.GetFileName(sourceFileName);

            var ext = Path.GetExtension(filename);
            var fn = Path.GetFileNameWithoutExtension(filename);
            var hash = JenkHash.GenHash(sourceFileName.ToLowerInvariant());
            var savepath = Path.Combine(targetFolder, $"{fn}_{hash}{ext}");

            nameTable[kv.Key] = savepath;
            if (File.Exists(savepath))
            {
                continue;
            }
            var bytes = adapter.ExtractSourceFile(sourceFileName);
            if (bytes == null)
            {
                continue;
            }
            File.WriteAllBytes(savepath, bytes);
        }

        // 2. load loca lfile
        var fileTable = new Dictionary<Guid, GameFile>();
        foreach (var kv in nameTable)
        {
            fileTable[kv.Key] = GameFileUtils.LoadFile(kv.Value);
        }

        var textureReplacements = new List<TextureReplacement>();
        foreach (var kv in fileTable)
        {
            var modTex = project.modTextures[kv.Key];
            var fileSource = new AsyncImageFileSource(modTex.filename);
            // fileSource.Load();
            project.FindTextureReplacements(kv.Key, textureReplacements);
            foreach (var replacement in textureReplacements)
            {
                // replacement.sourceTexture
            }
        }
    }
}