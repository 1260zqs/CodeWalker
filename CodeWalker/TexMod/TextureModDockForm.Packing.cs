using System;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;

namespace CodeWalker.TexMod;

public partial class TextureModDockForm
{
    private void BuildMod()
    {
        var result = new ActionResult();
        var cts = new CancellationTokenSource();
        var progressForm = ProgressForm.Create("Build TexMod", cts);
        Task.Run(async () =>
        {
            try
            {
                await BuildModAsync(cts.Token, progressForm, result);
            }
            catch (OperationCanceledException)
            {
                result.message = "operation cancelled";
            }
            catch (Exception e)
            {
                result.exception = e;
            }
            finally
            {
                progressForm.ClearProgress();
            }
            cts.Dispose();
        }, cts.Token);
        progressForm.ShowDialog(this);
        result.ShowMessageBox(this);
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
        public List<TextureMapping> mappings;
        public Bitmap sourceBitmap;
    }

    class ActionResult
    {
        public bool success;
        public string message;
        public Exception exception;

        public void ShowMessageBox(IWin32Window owner = null)
        {
            if (success)
            {
                MessageBox.Show(
                    owner,
                    message ?? "success",
                    "Information",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else if (exception != null)
            {
                exception.ShowDialog();
            }
            else
            {
                MessageBox.Show(
                    message ?? "unknown error",
                    null,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }
    }

    private async Task BuildModAsync(CancellationToken cts, ProgressForm progress, ActionResult result)
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
        var localPackFileCache = new Dictionary<string, GameFile>();
        var sourcePackFileCache = new Dictionary<string, GameFile>();
        progress.SetMaxValue(project.sourceTextures.Count);
        foreach (var kv in project.sourceTextures)
        {
            var sourceTexture = kv.Value;
            var sourceFileName = adapter.GetSourceFileName(sourceTexture.sourceFile);
            var sourceTexName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
            var filename = Path.GetFileName(sourceFileName);

            // var ext = Path.GetExtension(filename);
            // var fn = Path.GetFileNameWithoutExtension(filename);
            // var hash = JenkHash.GenHash(sourceFileName.ToLowerInvariant());
            // var localFile = $"texmod\\{fn}_{hash:x8}{ext}";
            progress.IncreaseValue($"extract {filename}");
            cts.ThrowIfCancellationRequested();

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
                continue;
            }
            var bytes = adapter.ExtractSourceFile(sourceFileName);
            if (bytes == null)
            {
                throw new Exception("unable to extract source file");
            }
            modPack.fileBytes = bytes;
            File.WriteAllBytes(savepath, bytes);
        }

        // 2. load pack file
        progress.SetMaxValue(modPacks.Count);
        foreach (var modPack in modPacks)
        {
            cts.ThrowIfCancellationRequested();
            var name = Path.GetFileName(modPack.sourcePath);
            progress.IncreaseValue($"load file {name}");

            if (localPackFileCache.TryGetValue(modPack.localSavePath, out var file))
            {
                modPack.localFile = file;
                modPack.fileEntry = file.RpfFileEntry;
                modPack.fileType = file.Type;
                continue;
            }
            modPack.fileType = GameFileUtils.GetFileTypeByExtension(Path.GetExtension(name));
            file = GameFileUtils.CreateFileObject(modPack.fileType);
            if (file is PackedFile packedFile)
            {
                modPack.fileBytes ??= File.ReadAllBytes(modPack.localSavePath);
                modPack.fileEntry = GameFileUtils.CreateFileEntry(name, modPack.localSavePath, ref modPack.fileBytes);
                packedFile.Load(modPack.fileBytes, modPack.fileEntry);
                modPack.localFile = file;
                localPackFileCache.Add(modPack.localSavePath, file);
                continue;
            }
            throw new Exception("unable to pack file");
        }

        progress.SetMaxValue(modPacks.Count);
        var imageCache = new Dictionary<string, SharpDX.Direct2D1.Bitmap>();
        foreach (var modPack in modPacks)
        {
            // load game source texture
            cts.ThrowIfCancellationRequested();
            progress.IncreaseValue($"draw texture {modPack.sourceTexName}");
            modPack.mappings = project.FindSourceTextureMapping(modPack.id);

            if (!sourcePackFileCache.TryGetValue(modPack.sourcePath, out var file))
            {
                file = GameFileUtils.CreateFileObject(modPack.fileType);
                var rpfEntry = rpfManager.GetEntry(modPack.sourcePath);
                rpfManager.LoadFile(file as PackedFile, rpfEntry);
                sourcePackFileCache.Add(modPack.sourcePath, file);
            }
            modPack.sourceFile = file;

            var findTexture = GameFileUtils.FindTexture(modPack.sourceFile, modPack.sourceTexName);
            if (findTexture == null)
            {
                throw new Exception("unable to load source texture");
            }
            modPack.originTexture = findTexture;

            // progress.UpdateStatusTex($"load texture {modPack.sourceTexName}");
            var textureSource = new AsyncTextureSource(findTexture, 0);
            await textureSource.LoadAsync();
            modPack.sourceBitmap = textureSource.CreateBitmap(d2dRenderTarget.target);
            if (modPack.sourceBitmap == null)
            {
                throw new Exception("unable to load source bitmap");
            }

            var drawList = new List<(TextureMapping mapping, SharpDX.Direct2D1.Bitmap bitmap)>();
            foreach (var mapping in modPack.mappings)
            {
                var modTexture = project.modTextures[mapping.modTexture];
                if (!imageCache.TryGetValue(modTexture.filename, out var bitmap))
                {
                    // progress.UpdateStatusTex($"load image {modTexture.filename}");
                    var imageSource = new AsyncImageFileSource(modTexture.filename);
                    await imageSource.LoadAsync();
                    bitmap = imageSource.CreateBitmap(d2dRenderTarget.target);
                    imageCache.Add(modTexture.filename, bitmap);
                }
                if (bitmap == null)
                {
                    throw new Exception("unable to load image bitmap");
                }
                drawList.Add((mapping, bitmap));
            }
            if (drawList.Count == 0) continue;

            // progress.UpdateStatusTex($"draw texture {modPack.sourceTexName}");
            d2dRenderTarget.SetTargetSize(modPack.id, modPack.sourceBitmap.PixelSize);
            d2dRenderTarget.BeginDraw();
            d2dRenderTarget.target.DrawBitmap(modPack.sourceBitmap, 1, BitmapInterpolationMode.NearestNeighbor);
            foreach (var (mapping, bitmap) in drawList)
            {
                //Log($"draw texture, {mapping.name}");
                var modTexture = project.modTextures[mapping.modTexture];
                DrawPreviewOverlay(
                    d2dRenderTarget.target,
                    null,
                    bitmap,
                    modPack.sourceBitmap.PixelSize,
                    modTexture.sourceRect,
                    mapping.targetRect,
                    mapping.flipX,
                    mapping.flipY,
                    mapping.rotation
                );
            }
            d2dRenderTarget.EndDraw();

            // progress.UpdateStatusTex($"encode texture {modPack.sourceTexName}");
            var encodeBytes = d2dRenderTarget.EncodeTexture(
                Utils.NVTT.Format.Format_DXT1,
                Utils.NVTT.Quality.Quality_Normal
            );
            var tex = DDSIO.GetTexture(encodeBytes);
            var originTexture = modPack.originTexture;
            //File.WriteAllBytes($"C:\\tex\\{modPack.sourceTexName}.dds", encodeBytes);

            tex.Name = originTexture.Name;
            tex.NameHash = originTexture.NameHash;
            tex.Usage = originTexture.Usage;
            tex.UsageFlags = originTexture.UsageFlags;
            tex.Unknown_32h = originTexture.Unknown_32h;
            tex.NamePointer = originTexture.NamePointer;

            GameFileUtils.ReplaceTexture(modPack.localFile, tex);
            Utilities.Dispose(ref modPack.sourceBitmap);
        }
        foreach (var bitmap in imageCache.Values)
        {
            bitmap.Dispose();
        }
        imageCache.Clear();

        progress.SetMaxValue(modPacks.Count);
        foreach (var modPack in modPacks)
        {
            if (modPack.localFile is PackedFile packedFile)
            {
                progress.IncreaseValue($"write file {modPack.localFilePath}");
                var bytes = packedFile.Save();
                File.WriteAllBytes(modPack.localSavePath, bytes);
            }
        }
        foreach (var modPack in modPacks)
        {
            project.manifest.SetPatchFile(modPack.sourcePath, modPack.localFilePath);
        }
        project.SaveManifest();
        result.success = true;
        progress.ClearProgress();
        Console.WriteLine("mod pack ok");
    }

    private void PackMod()
    {
        var folder = Path.GetDirectoryName(project.manifestFile)!;

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog();
        saveFileDialog.FileName = "package.oiv";
        saveFileDialog.InitialDirectory = folder;
        saveFileDialog.Filter = "OpenIV Package Format (*.oiv)|*.oiv|Zip (*.zip)|*.zip;|All Files (*.*)|*.*";

        if (saveFileDialog.ShowDialog() == true)
        {
            var result = new ActionResult();
            var fileName = saveFileDialog.FileName;
            var cts = new CancellationTokenSource();
            var progress = ProgressForm.Create("Create OpenIV Package", cts);
            Task.Run(() =>
            {
                PackModTask(fileName, cts.Token, progress, result);
                cts.Dispose();
            }, cts.Token);
            progress.ShowDialog(this);
            result.ShowMessageBox(this);
        }
    }

    private void PackModTask(string zipPath, CancellationToken cts, ProgressForm progress, ActionResult result)
    {
        // pack oiv
        var folder = Path.GetDirectoryName(project.manifestFile)!;
        string tmpPath = null;
        try
        {
            var tempName = Path.GetFileName(zipPath);
            var saveDir = Path.GetDirectoryName(zipPath);
            tmpPath = $"{saveDir}\\~{tempName}";

            var fileInfo = new FileInfo(tmpPath);
            using var stream = fileInfo.OpenWrite();
            fileInfo.Attributes = FileAttributes.Hidden | FileAttributes.Temporary;
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, false))
            {
                archive.CreateEntryFromFile($"{folder}\\icon.png", "icon.png");
                archive.CreateEntryFromFile($"{folder}\\assembly.xml", "assembly.xml");

                var contentDir = Path.Combine(folder, "content");
                var files = Directory.GetFiles(contentDir, "*", SearchOption.AllDirectories);
                for (var i = 0; i < files.Length; i++)
                {
                    var file = files[i];
                    var entry = file.Substring(folder.Length + 1);
                    progress.UpdateProgress(entry, i + 1, files.Length);
                    archive.CreateEntryFromFile(file, entry);
                    if (cts.IsCancellationRequested)
                    {
                        result.message = "operation cancelled";
                        Console.WriteLine("pack Cancellation Requested");
                        break;
                    }
                }
            }
            stream.Close();
            if (cts.IsCancellationRequested)
            {
                fileInfo.Delete();
            }
            else
            {
                var attr = fileInfo.Attributes;
                attr &= ~FileAttributes.Temporary;
                attr &= ~FileAttributes.Hidden;
                fileInfo.Attributes = attr;
                CommitTempFile(tmpPath, zipPath);
                result.success = true;
                result.message = "pack success";
            }
            progress.ClearProgress();
            Console.WriteLine("pack mod done");
        }
        catch (Exception exception)
        {
            result.exception = exception;
            progress.ClearProgress();
        }
        finally
        {
            if (tmpPath != null && File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }
        }
    }

    private static void CommitTempFile(string tmpPath, string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Replace(tmpPath, filePath, null);
            }
            else
            {
                File.Move(tmpPath, filePath);
            }
        }
        catch (Exception ex)
        {
            ex.ShowDialog();
        }
        finally
        {
            if (File.Exists(tmpPath))
            {
                File.Delete(tmpPath);
            }
        }
    }
}