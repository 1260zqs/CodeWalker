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
using System.Threading.Tasks;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Direct2D1;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    private void PackMod()
    {
        try
        {
            Task.Run(PackModAsync).Wait();
            MessageBox.Show(
                "texture pack completed.",
                "Information",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception e)
        {
            e.ShowDialog();
        }
    }

    class ModPack
    {
        public Guid id;
        public string localFilePath;
        public string localSavePath;

        // public string sourceFile;
        public string sourcePath;
        public string sourceTexName;

        public byte[] fileBytes;
        public SourceTexture sourceTex;
        public Texture originTexture;
        public GameFileType fileType;
        public RpfFileEntry fileEntry;
        public GameFile localFile;
        public GameFile sourceFile;
        public List<TextureReplacement> replacements;
        public Bitmap sourceBitmap;
    }

    private async Task PackModAsync()
    {
        void Log(string msg)
        {
            Console.WriteLine(msg);
        }
        var directoryName = Path.GetDirectoryName(project.manifestFile);
        var targetFolder = Path.Combine(directoryName, "content");
        if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

        // 1. extract file
        var modPacks = new List<ModPack>();
        var progressIndex = 0;
        foreach (var kv in project.sourceTextures)
        {
            progressIndex++;
            var sourceTexture = kv.Value;
            var sourceFileName = adapter.GetSourceFileName(sourceTexture.sourceFile);
            var sourceTexName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
            var filename = Path.GetFileName(sourceFileName);

            // var ext = Path.GetExtension(filename);
            // var fn = Path.GetFileNameWithoutExtension(filename);
            // var hash = JenkHash.GenHash(sourceFileName.ToLowerInvariant());
            // var localFile = $"texmod\\{fn}_{hash:x8}{ext}";

            var localFile = $"texmod\\{filename}";
            var savepath = Path.Combine(targetFolder, localFile);

            var modPack = new ModPack();
            modPack.id = kv.Key;
            modPack.sourceTex = kv.Value;
            modPack.localFilePath = localFile;
            modPack.localSavePath = savepath;
            modPack.sourcePath = sourceFileName;
            modPack.sourceTexName = sourceTexName;

            modPacks.Add(modPack);
            if (File.Exists(savepath))
            {
                Log($"read file {localFile} ({progressIndex}/{project.sourceTextures.Count})");
                modPack.fileBytes = File.ReadAllBytes(savepath);
                continue;
            }
            var bytes = adapter.ExtractSourceFile(sourceFileName);
            if (bytes == null)
            {
                throw new Exception("unable to extract source file");
            }
            modPack.fileBytes = bytes;
            Log($"write file {localFile} ({progressIndex}/{project.sourceTextures.Count})");
            File.WriteAllBytes(savepath, bytes);
        }

        // 2. load pack file
        for (var i = 0; i < modPacks.Count; i++)
        {
            var modPack = modPacks[i];
            var name = Path.GetFileName(modPack.sourcePath);
            modPack.fileType = GameFileUtils.GetFileTypeByExtension(Path.GetExtension(name));
            var file = GameFileUtils.CreateFileObject(modPack.fileType);
            if (file is PackedFile packedFile)
            {
                Log($"load game file {name} ({i + 1}/{modPacks.Count})");
                modPack.fileEntry = GameFileUtils.CreateFileEntry(name, modPack.localSavePath, ref modPack.fileBytes);
                packedFile.Load(modPack.fileBytes, modPack.fileEntry);
                modPack.localFile = file;
                continue;
            }
            throw new Exception("unable to pack file");
        }

        var imageCache = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();
        for (var i = 0; i < modPacks.Count; i++)
        {
            var modPack = modPacks[i];
            // load game source texture
            Log($"lookup texture ({i + 1}/{modPacks.Count}) : {modPack.sourceTexName}");
            modPack.replacements = project.FindSourceTextureReplacements(modPack.id);
            var rpfEntry = worldForm.GameFileCache.RpfMan.GetEntry(modPack.sourcePath);
            modPack.sourceFile = GameFileUtils.CreateFileObject(modPack.fileType);
            worldForm.GameFileCache.RpfMan.LoadFile(modPack.sourceFile as PackedFile, rpfEntry);
            var findTexture = GameFileUtils.FindTexture(modPack.sourceFile, modPack.sourceTexName);
            if (findTexture == null)
            {
                throw new Exception("unable to load source texture");
            }
            modPack.originTexture = findTexture;
            Log($"prepare bitmap ({i + 1}/{modPacks.Count}) : {findTexture.Name}");
            var textureSource = new AsyncTextureSource(findTexture, 0);
            await textureSource.LoadAsync();
            modPack.sourceBitmap = textureSource.CreateBitmap(d2dRenderTarget.target);
            if (modPack.sourceBitmap == null)
            {
                throw new Exception("unable to load source bitmap");
            }

            var drawList = new List<(TextureReplacement replacement, SharpDX.Direct2D1.Bitmap bitmap)>();
            foreach (var replacement in modPack.replacements)
            {
                var modTexture = project.modTextures[replacement.modTexture];
                if (!imageCache.TryGetValue(modTexture.filename, out var bitmap))
                {
                    Log($"load image ({i + 1}/{modPacks.Count}) : {modTexture.filename}");
                    var imageSource = new AsyncImageFileSource(modTexture.filename);
                    await imageSource.LoadAsync();
                    bitmap = imageSource.CreateBitmap(d2dRenderTarget.target);
                    imageCache.Add(modTexture.filename, bitmap);
                }
                if (bitmap == null)
                {
                    throw new Exception("unable to load image bitmap");
                }
                drawList.Add((replacement, bitmap));
            }

            try
            {
                Log($"begin draw texture ({i + 1}/{modPacks.Count}) : {modPack.sourceTexName}");

                d2dRenderTarget.SetTargetSize(null, modPack.sourceBitmap.PixelSize);
                d2dRenderTarget.BeginDraw();
                d2dRenderTarget.target.DrawBitmap(modPack.sourceBitmap, 1, BitmapInterpolationMode.NearestNeighbor);
                foreach (var (replacement, bitmap) in drawList)
                {
                    Log($"draw texture, {replacement.name}");
                    var modTexture = project.modTextures[replacement.modTexture];
                    DrawPreviewOverlay(
                        d2dRenderTarget.target,
                        null,
                        bitmap,
                        modPack.sourceBitmap.PixelSize,
                        modTexture.sourceRect.Convert(),
                        replacement.targetRect.Convert(),
                        replacement.flipX,
                        replacement.flipY,
                        replacement.rotation
                    );
                }
                d2dRenderTarget.EndDraw();

                Log($"encode texture, {modPack.sourceTexName}");
                var encodeBytes = d2dRenderTarget.Encode(
                    Utils.NVTT.Format.Format_DXT1,
                    Utils.NVTT.Quality.Quality_Normal
                );
                var tex = DDSIO.GetTexture(encodeBytes);
                var originTexture = modPack.originTexture;

                tex.Name = originTexture.Name;
                tex.NameHash = originTexture.NameHash;
                tex.Usage = originTexture.Usage;
                tex.UsageFlags = originTexture.UsageFlags;
                tex.Unknown_32h = originTexture.Unknown_32h;
                tex.NamePointer = originTexture.NamePointer;

                GameFileUtils.ReplaceTexture(modPack.localFile, tex);
                Utilities.Dispose(ref modPack.sourceBitmap);
            }
            catch (Exception e)
            {
                e.ShowDialog();
            }
        }
        foreach (var bitmap in imageCache.Values)
        {
            bitmap.Dispose();
        }
        imageCache.Clear();

        for (var i = 0; i < modPacks.Count; i++)
        {
            var modPack = modPacks[i];
            if (modPack.localFile is PackedFile packedFile)
            {
                try
                {
                    var bytes = packedFile.Save();
                    Log($"write packed file ({i + 1}/{modPacks.Count}) : {modPack.localFilePath}");
                    File.WriteAllBytes(modPack.localSavePath, bytes);
                }
                catch (Exception e)
                {
                    e.ShowDialog();
                }
            }
        }
        Console.WriteLine("mod pack ok");
    }
}