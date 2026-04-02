using CodeWalker.GameFiles;
using CodeWalker.Graphic;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CodeWalker.TexMod;

public partial class TextureModDockForm
{
    static TextureModDockForm instance;
    // static TextureModProject workingProject;

    public static void ShowWindow(WorldForm worldForm)
    {
        instance = GetWindow(worldForm);
        if (instance == null) return;

        instance.Show();
        instance.Focus();
    }

    public static TextureModDockForm GetWindow(WorldForm worldForm)
    {
        if (instance == null || instance.IsDisposed)
        {
            try
            {
                instance = Create(worldForm);
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
                instance = null;
            }
        }
        return instance;
    }

    public static TextureModDockForm Create(WorldForm worldForm)
    {
        var save = false;
        var packageManifestFile = string.Empty;
        var workingDir = Settings.Default.TexModWorkingDir;
        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
        {
            save = true;
            var setupForm = new TexModSetupForm();
            setupForm.ProjectWorkingDir = workingDir;
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            workingDir = setupForm.ProjectWorkingDir;
            packageManifestFile = setupForm.PackageManifestFile;
        }
        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
        {
            return null;
        }
        if (save)
        {
            Settings.Default.TexModWorkingDir = workingDir;
        }
        var workingProject = TextureModProject.SetupWorkingProject(workingDir);
        if (!string.IsNullOrEmpty(packageManifestFile) && File.Exists(packageManifestFile))
        {
            workingProject.manifestFile = packageManifestFile;
        }
        else if (string.IsNullOrEmpty(workingProject.manifestFile) || !File.Exists(workingProject.manifestFile))
        {
            var setupForm = new TexModSetupForm();
            setupForm.ProjectWorkingDir = workingDir;
            setupForm.PackageManifestFile = workingProject.manifestFile;
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            workingProject.manifestFile = setupForm.PackageManifestFile;
        }
        workingProject.LoadPackageManifest();
        var form = new TextureModDockForm(
            workingProject,
            worldForm.Renderer,
            worldForm.GameFileCache.RpfMan,
            new GTAVTextureModAdapter(workingProject, worldForm.GameFileCache)
        );
        return form;
    }

    public static void ShowAddModSource(WorldForm worldForm, AddModSourceInfo info)
    {
        var window = GetWindow(worldForm);
        if (window == null) return;

        window.Show();
        window.Focus();
        window.BeginInvoke(() =>
        {
            window.AddModSource(info);
        });
    }

    public void AddModSource(AddModSourceInfo info)
    {
        var modTexture = working.modTexture;
        if (modTexture != null)
        {
            var texName = info.texName;
            var gameFile = info.gameFile;
            var sourceFile = adapter.MakeSourcePath(gameFile, texName);
            if (sourceFile == null)
            {
                return;
            }
            var sourceTexture = project.GetOrAddSourceTexture(sourceFile);
            foreach (var item in project.textureMappings)
            {
                if (item.modTexture == modTexture.id && item.sourceTexture == sourceTexture.id)
                {
                    return;
                }
            }
            var mapping = project.CreateMapping();
            mapping.sourceTexture = sourceTexture.id;
            mapping.modTexture = modTexture.id;
            mapping.lod = info.lod.Conv();
            mapping.name = texName;

            modTexture.position = info.position;
            modTexture.rotation = info.rotation;

            mappingControl?.RefreshListView(modTexture);
        }
    }

    private TextureModDockForm(TextureModProject workingProject, Renderer renderer, RpfManager rpfMan, TextureModAdapter adapter) : this()
    {
        this.project = workingProject;
        this.rpfManager = rpfMan;
        this.renderer = renderer;
        this.adapter = adapter;
    }

    public TextureModProject project;
    private TextureModAdapter adapter;
    private RpfManager rpfManager;
    private Renderer renderer;

    private D2DRenderTarget d2dRenderTarget;
    private WorkingState working = new();

    public bool isSolidColor
    {
        get => m_IsSolidColor;
        set
        {
            if (m_IsSolidColor != value)
            {
                m_IsSolidColor = value;
                PictureBoxRectTool.SetSolid(modTextureCanvas?.canvas, value);
                PictureBoxRectTool.SetSolid(gameTextureCanvas?.canvas, value);
                modTextureCanvas?.Repaint();
                gameTextureCanvas?.Repaint();
            }
        }
    }

    public bool isPainting
    {
        get => m_IsPainting;
        set
        {
            if (m_IsPainting != value)
            {
                m_IsPainting = value;
                gameTextureCanvas?.Repaint();
            }
        }
    }

    public bool isDrawTestColor
    {
        get => m_IsDrawTestColor;
        set => m_IsDrawTestColor = value;
    }

    public bool isSyncLod
    {
        get => m_IsSyncLod;
        set
        {
            m_IsSyncLod = value;
            if (value)
            {
                SyncRenderLod();
            }
        }
    }

    private bool m_IsPainting = true;
    private bool m_IsSolidColor;
    private bool m_IsDrawTestColor;
    private bool m_IsSyncLod;

    public void SelectTexMod(ModTexture modTexture)
    {
        if (working.modTexture == modTexture)
        {
            return;
        }
        ClearTexModSelection();
        ClearMappingSelection();
        working.modTexture = modTexture;

        if (modTexture != null)
        {
            if (imageCache.TryGetFromCache(modTexture.filename, out var bitmap))
            {
                working.modTextureBitmap = bitmap;
                modTextureCanvas?.SetImage(bitmap);
                OnModTextureReady(bitmap);
            }
            else
            {
                working.modTextureSource = new AsyncImageFileSource(working.modTexture.filename);
                working.modTextureSource.shared = true;
                working.modTextureSource.LoadAsync();
                modTextureCanvas?.SetImage(working.modTextureSource);
            }
            PictureBoxRectTool.SetSolid(modTextureCanvas?.canvas, isSolidColor);
            PictureBoxRectTool.SetRect(modTextureCanvas?.canvas, modTexture.sourceRect);
            PictureBoxViewer.LoadState(modTextureCanvas?.canvas, modTexture.editorState);
            toolsControl?.SetSrcRect(modTexture.sourceRect);
        }
        mappingControl?.RefreshListView(modTexture);
    }

    public void SelectTextureMapping(TextureMapping mapping)
    {
        if (working.mapping == mapping)
        {
            return;
        }
        ClearMappingSelection();
        working.mapping = mapping;

        if (mapping != null && project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
        {
            working.sourceTexture = sourceTexture;
            var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
            working.texNameHash = JenkHash.GenHash(textureName.ToLowerInvariant());
            if (imageCache.TryGetFromCache(sourceTexture.sourceFile, out var bitmap))
            {
                working.gameTextureBitmap = bitmap;
                gameTextureCanvas?.SetImage(bitmap);
                OnGameTextureReady(bitmap);
            }
            else
            {
                working.gameTextureSource = new AsyncGameTextureSource(adapter, sourceTexture.sourceFile);
                working.gameTextureSource.shared = true;
                working.gameTextureSource.LoadAsync();
                gameTextureCanvas?.SetImage(working.gameTextureSource);
            }
            PictureBoxViewer.LoadState(gameTextureCanvas?.canvas, mapping.editorState);
            PictureBoxRectTool.SetSolid(gameTextureCanvas?.canvas, isSolidColor);
            PictureBoxRectTool.SetRect(gameTextureCanvas?.canvas, mapping.targetRect);
            toolsControl?.SetDestRect(mapping.targetRect);
            propertyControl?.SelectObject(mapping);
            if (isSyncLod) SyncRenderLod();
        }
        propertyControl?.Refresh();
    }

    private void ClearTexModSelection()
    {
        if (working.modTexture != null && modTextureCanvas != null)
        {
            working.modTexture.editorState = PictureBoxViewer.SaveState(modTextureCanvas.canvas);
        }
        if (working.modTextureBitmap != null && working.modTexture != null)
        {
            var key = working.modTexture.filename;
            imageCache.ReturnToCache(key, working.modTextureBitmap);
            working.modTextureBitmap = null;
        }

        working.modTexture = null;
        Utilities.Dispose(ref working.modTextureSource);
        Utilities.Dispose(ref working.modTextureBitmap);
        modTextureCanvas?.ClearImage();
        mappingControl?.Clear();

        PictureBoxRectTool.SetRect(modTextureCanvas?.canvas, default);
        PictureBoxViewer.ResetViewer(modTextureCanvas?.canvas);
    }

    private void ClearMappingSelection()
    {
        if (working.mapping != null && gameTextureCanvas != null)
        {
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas.canvas);
        }
        if (working.sourceTexture != null && working.gameTextureBitmap != null)
        {
            var key = working.sourceTexture.sourceFile;
            imageCache.ReturnToCache(key, working.gameTextureBitmap);
            working.gameTextureBitmap = null;
        }

        working.mapping = null;
        working.texNameHash = 0;
        working.sourceTexture = null;
        propertyControl?.SelectObject(null);

        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);
        gameTextureCanvas?.ClearImage();

        PictureBoxRectTool.SetRect(gameTextureCanvas?.canvas, default);
        PictureBoxViewer.ResetViewer(gameTextureCanvas?.canvas);
    }

    private void SyncRenderLod()
    {
        if (working.mapping is { } mapping)
        {
            var lod = mapping.lod.Conv();
            var syncRoot = renderer.DXMan.syncroot;
            if (!Monitor.TryEnter(syncRoot, 50))
            {
                return;
            }
            try
            {
                renderer.renderworldMaxLOD = lod;
                renderer.renderhdtextures = mapping.lod == TextureLod.HiDR;
            }
            catch (Exception ex)
            {
                ex.ShowDialog();
            }
            finally
            {
                Monitor.Exit(syncRoot);
            }
        }
    }

    private void OnModTextureReady(SharpDX.Direct2D1.Bitmap bitmap)
    {
        if (working.modTexture.editorState == null && modTextureCanvas != null)
        {
            var imageSize = bitmap.PixelSize;
            PictureBoxViewer.FitViewer(modTextureCanvas.canvas, imageSize.Width, imageSize.Height);
            working.modTexture.editorState = PictureBoxViewer.SaveState(modTextureCanvas.canvas);
        }
        RequestTexturePaintingUpdate();
    }

    private void OnGameTextureReady(SharpDX.Direct2D1.Bitmap bitmap)
    {
        if (working.mapping.editorState == null && gameTextureCanvas != null)
        {
            var imageSize = bitmap.PixelSize;
            PictureBoxViewer.FitViewer(gameTextureCanvas.canvas, imageSize.Width, imageSize.Height);
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas.canvas);
        }
        RequestTexturePaintingUpdate();
    }

    private volatile int texturePaintingUpdatePending;

    public void RequestTexturePaintingUpdate()
    {
        if (Interlocked.CompareExchange(ref texturePaintingUpdatePending, 1, 0) == 0)
        {
            BeginInvoke(ExecuteTexturePaintingUpdate);
        }
    }

    private void ExecuteTexturePaintingUpdate()
    {
        Interlocked.Exchange(ref texturePaintingUpdatePending, 0);
        UpdateTexturePaintingCore();
    }

    private void UpdateTexturePaintingCore()
    {
        if (working.modTexture == null || working.mapping == null) return;

        var targetRect = working.mapping.targetRect;
        var baseImage = working.gameTextureBitmap;
        if (baseImage == null || baseImage.IsDisposed) return;

        var syncRoot = renderer.DXMan.syncroot;
        if (!Monitor.TryEnter(syncRoot, 50))
        {
            return;
        }
        try
        {
            var baseImagePixelSize = baseImage.PixelSize;
            d2dRenderTarget.SetTargetSize(working.mapping.id, baseImagePixelSize);
            d2dRenderTarget.BeginDraw();

            var overlay = working.modTextureBitmap;
            if (!isPainting || isDrawTestColor) overlay = null;
            DrawPreviewOverlay(
                d2dRenderTarget.target,
                baseImage,
                overlay,
                baseImagePixelSize,
                working.modTexture.sourceRect,
                working.mapping.targetRect,
                working.mapping.flipX,
                working.mapping.flipY,
                working.mapping.rotation
            );
            if (isDrawTestColor)
            {
                d2dRenderTarget.FillRectangle(targetRect.Raw());
            }
            d2dRenderTarget.EndDraw();

            var nameHash = working.texNameHash;
            renderer.RenderableCache.FindRenderableTexture(x =>
            {
                var tex = x.Key;
                if (tex.NameHash == nameHash && tex.Width == baseImagePixelSize.Width && tex.Height == baseImagePixelSize.Height)
                {
                    d2dRenderTarget.CopyTo(renderer.Device, x);
                }
                return false;
            });
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        finally
        {
            Monitor.Exit(syncRoot);
        }
    }

    private static void DrawPreviewOverlay(
        SharpDX.Direct2D1.RenderTarget target,
        SharpDX.Direct2D1.Bitmap baseImage,
        SharpDX.Direct2D1.Bitmap overlay,
        SharpDX.Size2 baseImageSize,
        System.Drawing.RectangleF srcRect,
        System.Drawing.RectangleF destRect,
        bool flipX,
        bool flipY,
        float rotation
    )
    {
        if (baseImage != null)
        {
            target.DrawBitmap(baseImage, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
        }
        if (overlay == null || overlay.IsDisposed) return;

        var imgBounds = new System.Drawing.RectangleF(0, 0, baseImageSize.Width, baseImageSize.Height);
        var clippedDest = System.Drawing.RectangleF.Intersect(destRect, imgBounds);

        if (clippedDest.Width <= 0 || clippedDest.Height <= 0)
            return;

        var scaleX = srcRect.Width / destRect.Width;
        var scaleY = srcRect.Height / destRect.Height;

        var dx = clippedDest.X - destRect.X;
        var dy = clippedDest.Y - destRect.Y;

        srcRect.X += dx * scaleX;
        srcRect.Y += dy * scaleY;
        srcRect.Width = clippedDest.Width * scaleX;
        srcRect.Height = clippedDest.Height * scaleY;

        var matrix = target.Transform;
        var center = new Vector2(
            (clippedDest.Left + clippedDest.Right) * 0.5f,
            (clippedDest.Top + clippedDest.Bottom) * 0.5f
        );
        target.Transform = Matrix3x2.Scaling(flipX ? -1 : 1, flipY ? -1 : 1, center) *
                           Matrix3x2.Rotation(rotation * Mathf.Deg2Rad, center) * matrix;

        target.DrawBitmap(
            overlay,
            clippedDest.Raw(),
            1f,
            SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
            srcRect.Raw()
        );
        target.Transform = matrix;
    }

    public void SaveProject()
    {
        // project.directory = SaveTreeView();
        // project.Save(Settings.Default.TexModWorkingDir);
    }

    internal ModTexture DuplicateModTexture(ModTexture srcModTexture)
    {
        if (srcModTexture == null) return null;

        var modTexture = srcModTexture.Clone();
        var mappings = project.FindTextureMapping(modTexture.id);
        modTexture.id = Guid.NewGuid();
        modTexture.createdAt = DateTimeOffset.Now;
        modTexture.name = $"{modTexture.name} (Clone)";
        var newMappings = new List<TextureMapping>();
        foreach (var mapping in mappings)
        {
            var clone = mapping.Clone();
            clone.id = Guid.NewGuid();
            clone.modTexture = modTexture.id;
            newMappings.Add(clone);
        }
        project.modTextures.Add(modTexture.id, modTexture);
        foreach (var mapping in newMappings)
        {
            project.textureMappings.Add(mapping);
        }
        return modTexture;
    }

    internal void ReimportTex(ModTexture modTexture)
    {
        if (modTexture == null) return;
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog1.FileName;
                if (string.IsNullOrEmpty(fileName)) return;
                var key = modTexture.filename;
                modTexture.filename = fileName;
                modTexture.name = Path.GetFileName(fileName);
                if (working.modTexture != null && working.modTexture.id == modTexture.id)
                {
                    if (working.modTextureBitmap != null)
                    {
                        imageCache.ReturnToCache(key, working.modTextureBitmap);
                        working.modTextureBitmap = null;
                        working.modTextureSource = new AsyncImageFileSource(fileName);
                        working.modTextureSource.shared = true;
                        working.modTextureSource.LoadAsync();
                        modTextureCanvas?.SetImage(working.modTextureSource);
                        gameTextureCanvas?.Repaint();
                        RequestTexturePaintingUpdate();
                    }
                }
                // RefreshModListView();
            }
            catch (Exception exception)
            {
                exception.ShowDialog();
            }
        }
    }

    internal void NewTexMod()
    {
        if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog1.FileName;
                if (string.IsNullOrEmpty(fileName)) return;

                using var decoder = new SharpDX.WIC.BitmapDecoder(
                    DXGraphic.wicFactory,
                    fileName,
                    SharpDX.WIC.DecodeOptions.CacheOnLoad
                );
                using var converter = new SharpDX.WIC.FormatConverter(DXGraphic.wicFactory);
                using var frame = decoder.GetFrame(0);
                converter.Initialize(
                    frame,
                    SharpDX.WIC.PixelFormat.Format32bppRGBA,
                    SharpDX.WIC.BitmapDitherType.None,
                    null,
                    0,
                    SharpDX.WIC.BitmapPaletteType.Custom
                );

                var width = converter.Size.Width;
                var height = converter.Size.Height;

                var modTexture = project.CreateTextureMod(fileName);
                modTexture.sourceRect = new System.Drawing.RectangleF(0, 0, width, height);
                // RefreshModListView();
            }
            catch (Exception exception)
            {
                exception.ShowDialog();
            }
        }
    }

    internal void DeleteTexMod(ModTexture modTexture)
    {
        project.DeleteModTexture(modTexture);
        if (working.modTexture != null && working.modTexture.id == modTexture.id)
        {
            SelectTexMod(null);
        }
    }

    internal void DeleteTexMapping(TextureMapping textureMapping)
    {
        project.DeleteTextureMapping(textureMapping);
        if (working.mapping != null && working.mapping.id == textureMapping.id)
        {
            ClearMappingSelection();
        }
        mappingControl?.RefreshListView(working.modTexture);
    }

    internal bool GetSrcImageRect(out System.Drawing.RectangleF rect)
    {
        if (working.modTexture is { } modTexture)
        {
            rect = modTexture.sourceRect;
            return true;
        }
        rect = default;
        return false;
    }

    internal bool GetDestTextureRect(out System.Drawing.RectangleF rect)
    {
        if (working.mapping is { } mapping)
        {
            rect = mapping.targetRect;
            return true;
        }
        rect = default;
        return false;
    }

    internal void SetSrcImageRect(System.Drawing.RectangleF rect)
    {
        if (working.modTexture is { } modTexture)
        {
            modTexture.sourceRect = rect;
            RequestTexturePaintingUpdate();
            PictureBoxRectTool.SetRect(modTextureCanvas?.canvas, rect);
            gameTextureCanvas?.Repaint();
            modTextureCanvas?.Repaint();
        }
    }

    internal void SetDestTextureRect(System.Drawing.RectangleF rect)
    {
        if (working.mapping is { } mapping)
        {
            mapping.targetRect = rect;
            RequestTexturePaintingUpdate();
            PictureBoxRectTool.SetRect(gameTextureCanvas?.canvas, rect);
            gameTextureCanvas?.Repaint();
        }
    }

    internal void FitSrcRectByDestWidth()
    {
        //var imageSize = modTextureCanvas.GetImageSize();

        //var width = numericUpDown1.Value;
        //var height = numericUpDown2.Value;
        //if (width <= 0) return;

        //rectBoxX.Value = 0;
        //rectBoxW.Value = imageSize.Width;
        //rectBoxH.Value = height / width * imageSize.Width;
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Width <= 0) return;
            if (working.modTextureBitmap != null)
            {
                var imageSize = working.modTextureBitmap.PixelSize;

                srcRect.X = 0;
                srcRect.Width = imageSize.Width;
                srcRect.Height = (descRect.Height / descRect.Width) * imageSize.Width;
                SetSrcImageRect(srcRect);
            }
        }
    }

    internal void FitSrcRectByDestHeight()
    {
        //if (!modTextureCanvas.HasImage()) return;
        //var imageSize = modTextureCanvas.GetImageSize();

        //var width = numericUpDown1.Value;
        //var height = numericUpDown2.Value;
        //if (height <= 0) return;

        //rectBoxY.Value = 0;
        //rectBoxH.Value = imageSize.Height;
        //rectBoxW.Value = width / height * imageSize.Width;
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Height <= 0) return;
            if (working.modTextureBitmap != null)
            {
                var imageSize = working.modTextureBitmap.PixelSize;

                srcRect.Y = 0;
                srcRect.Height = imageSize.Height;
                srcRect.Width = (descRect.Width / descRect.Height) * imageSize.Width;
                SetSrcImageRect(srcRect);
            }
        }
    }

    internal void ClipSrcRectByDestWidth()
    {
        // clip by width
        //var width = numericUpDown1.Value;
        //var height = numericUpDown2.Value;
        //if (width <= 0) return;

        //var w = rectBoxW.Value;
        //rectBoxH.Value = height / width * w;
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {

        }
    }

    internal void ClipSrcRectByDestHeight()
    {
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {

        }
    }

    internal void SetTextureLocation()
    {
        if (working.modTexture is { } modTexture)
        {
            if (renderer.camera.FollowEntity is { } followEntity)
            {
                modTexture.position = followEntity.Position;
                modTexture.rotation = renderer.camera.TargetRotation;
            }
            else
            {
                modTexture.position = renderer.camera.Position;
                modTexture.rotation = renderer.camera.TargetRotation;
            }
        }
    }

    internal void GotoTextureLocation()
    {
        if (working.modTexture is { } modTexture)
        {
            if (renderer.camera.FollowEntity is { } followEntity)
            {
                followEntity.Position = modTexture.position;
                renderer.camera.TargetRotation = modTexture.rotation;
            }
            else
            {
                renderer.camera.Position = modTexture.position;
                renderer.camera.TargetRotation = modTexture.rotation;
            }
        }
    }
}