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

public struct AddModSourceInfo
{
    public GameFile gameFile;
    public string texName;
    public rage__eLodType lod;
    public Vector3 position;
    public Vector3 rotation;
    public bool hasCameraInfo;
    public bool hidr;
}

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
            worldForm.GameFileCache,
            new GTAVTextureModAdapter(workingProject, worldForm.GameFileCache)
        );
        form.setRenderLod = worldForm.SetRenderLod;
        form.setRenderHdTex = worldForm.SetRenderHdTex;
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

    public void AddModSource(string sourceFileName, string sourceTexName)
    {
        var gameFile = adapter.GetSourceFile(sourceFileName);
        if (gameFile != null)
        {
            var info = new AddModSourceInfo();
            info.texName = sourceTexName;
            info.gameFile = gameFile;
            info.lod = (rage__eLodType)(-1);
            info.hasCameraInfo = false;
            AddModSource(info);
        }
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
            mapping.lod = info.hidr ? TextureLod.HiDR : info.lod.Conv();
            mapping.name = texName;

            if (info.hasCameraInfo)
            {
                modTexture.position = info.position;
                modTexture.rotation = info.rotation;
            }

            mappingControl?.RefreshListView(modTexture);
        }
    }

    private TextureModDockForm(TextureModProject workingProject, Renderer renderer, GameFileCache gameFileCache, TextureModAdapter adapter) : this()
    {
        this.project = workingProject;
        this.gameFileCache = gameFileCache;
        this.rpfManager = gameFileCache.RpfMan;
        this.renderer = renderer;
        this.adapter = adapter;
    }

    public TextureModProject project;
    private TextureModAdapter adapter;
    private GameFileCache gameFileCache;
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
                RequestTexturePaintingUpdate();
            }
        }
    }

    public bool isDrawTestColor
    {
        get => m_IsDrawTestColor;
        set
        {
            if (m_IsDrawTestColor != value)
            {
                m_IsDrawTestColor = value;
                RequestTexturePaintingUpdate();
            }
        }
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
            modTextureCanvas?.UpdateEditState();
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
            modTextureCanvas?.UpdateEditState();
            toolsControl?.SelectTextureMapping(mapping);
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
                setRenderLod?.Invoke(lod);
                setRenderHdTex?.Invoke(mapping.lod == TextureLod.HiDR);
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

    private List<RenderableTexture> paintingTextureList = new();
    private bool requestNextTexturePaintingUpdate;
    private volatile int texturePaintingUpdatePending;
    private Action<rage__eLodType> setRenderLod;
    private Action<bool> setRenderHdTex;

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
        UpdateTexturePaintingNow();
    }

    private void UpdateTexturePaintingNow()
    {
        var mapping = working.mapping;
        var modTexture = working.modTexture;
        if (modTexture == null || mapping == null) return;

        var targetRect = mapping.targetRect;
        var sourceRect = modTexture.sourceRect;
        var baseImage = working.gameTextureBitmap;
        if (baseImage == null || baseImage.IsDisposed) return;
        if (targetRect.Width <= 0 || targetRect.Height <= 0) return;
        if (sourceRect.Width <= 0 || sourceRect.Height <= 0) return;

        var drawing = false;
        var syncRoot = renderer.DXMan.syncroot;
        if (!Monitor.TryEnter(syncRoot, 50))
        {
            return;
        }
        try
        {
            paintingTextureList.Clear();
            var pixelSize = baseImage.PixelSize;
            var nameHash = working.texNameHash;
            renderer.RenderableCache.FindRenderableTexture(x =>
            {
                var tex = x.Key;
                if (tex.NameHash == nameHash && tex.Width == pixelSize.Width && tex.Height == pixelSize.Height && x.IsLoaded)
                {
                    paintingTextureList.Add(x);
                }
                return false;
            });
            if (paintingTextureList.Count == 0)
            {
                // Console.WriteLine($"requestNextTexturePaintingUpdate {DateTime.Now}");
                requestNextTexturePaintingUpdate = true;
                return;
            }

            var upScale = 1;
            var baseImageSize = pixelSize.UpScale(upScale);
            d2dRenderTarget.SetTargetSize(mapping.id, baseImageSize);
            d2dRenderTarget.BeginDraw();
            drawing = true;

            var overlay = working.modTextureBitmap;
            if (!isPainting || isDrawTestColor) overlay = null;
            if (overlay != null && mapping.mipTexture != null)
            {
                if (mapping.mipRect1 != targetRect || mapping.mipRect2 != sourceRect)
                {
                    mapping.mipTexture.Dispose();
                    mapping.mipTexture = null;
                }
            }
            if (overlay != null && mapping.mipTexture == null)
            {
                if (UseMipTexture(sourceRect, targetRect))
                {
                    mapping.mipTexture = CreateMipTexture(overlay, sourceRect, targetRect);
                    mapping.mipRect1 = mapping.targetRect;
                    mapping.mipRect2 = modTexture.sourceRect;
                }
            }
            if (overlay != null && mapping.mipTexture != null)
            {
                overlay = mapping.mipTexture;
                sourceRect = overlay.PixelSize.ToRect();
            }
            DrawPreviewOverlay(
                d2dRenderTarget.target,
                baseImage,
                overlay,
                baseImageSize,
                sourceRect,
                mapping.targetRect.UpScale(upScale),
                mapping.flipX,
                mapping.flipY,
                mapping.swap
            );
            if (isDrawTestColor)
            {
                d2dRenderTarget.FillRectangle(targetRect.UpScale(upScale).Raw());
            }
            d2dRenderTarget.EndDraw();
            drawing = false;

            foreach (var texture in paintingTextureList)
            {
                d2dRenderTarget.CopyTo(renderer.Device, texture);
            }
            paintingTextureList.Clear();
        }
        catch (Exception e)
        {
            if (drawing) d2dRenderTarget.EndDraw();
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
        in System.Drawing.RectangleF srcRect,
        in System.Drawing.RectangleF destRect,
        bool flipX,
        bool flipY,
        bool swap
    )
    {
        if (baseImage != null)
        {
            target.DrawBitmap(baseImage, baseImageSize.ToRawRect(), 1f, BitmapInterpolationMode.NearestNeighbor);
        }
        if (overlay == null || overlay.IsDisposed) return;
        if (destRect.Width <= 0 || destRect.Height <= 0) return;

        var imgBounds = overlay.PixelSize.ToRect();
        var clippedDest = System.Drawing.RectangleF.Intersect(srcRect, imgBounds);
        if (clippedDest.Width <= 0 || clippedDest.Height <= 0) return;

        var matrix = target.Transform;
        var rotation = swap ? 90f * Mathf.Deg2Rad : 0f;
        var sx = swap ? destRect.Width / destRect.Height : 1f;
        var sy = swap ? destRect.Height / destRect.Width : 1f;
        var center = destRect.Pivot(0.5f, 0.5f);

        target.PushAxisAlignedClip(baseImageSize.ToRawRect(), AntialiasMode.PerPrimitive);
        target.Transform = Matrix3x2.Scaling(sy, sx, center) *
                           Matrix3x2.Scaling(flipX ? -1 : 1, flipY ? -1 : 1, center) *
                           Matrix3x2.Rotation(rotation, center) *
                           matrix;

        target.DrawBitmap(
            overlay,
            destRect.Raw(),
            1f,
            BitmapInterpolationMode.Linear,
            clippedDest.Raw()
        );
        target.Transform = matrix;
        target.PopAxisAlignedClip();
    }

    private bool UseMipTexture(
        in System.Drawing.RectangleF sourceRect,
        in System.Drawing.RectangleF targetRect
    )
    {
        var srcW = sourceRect.Width;
        var srcH = sourceRect.Height;

        var dstW = targetRect.Width;
        var dstH = targetRect.Height;

        var scaleX = dstW / srcW;
        var scaleY = dstH / srcH;

        var minScale = Math.Min(scaleX, scaleY);

        // If we're shrinking a lot, use a mip texture
        return minScale < 0.25f;
    }

    private SharpDX.Direct2D1.Bitmap CreateMipTexture(
        SharpDX.Direct2D1.Bitmap overlay,
        in System.Drawing.RectangleF sourceRect,
        in System.Drawing.RectangleF targetRect
    )
    {
        var imageSize = overlay.PixelSize;
        using var deviceContext = d2dRenderTarget.target.QueryInterface<SharpDX.Direct2D1.DeviceContext>();
        var bitmapProperties = new SharpDX.Direct2D1.BitmapProperties1();
        bitmapProperties.BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.CpuRead;
        bitmapProperties.PixelFormat = overlay.PixelFormat;
        using var bitmap1 = new SharpDX.Direct2D1.Bitmap1(
            deviceContext,
            new SharpDX.Size2(imageSize.Width, imageSize.Height),
            bitmapProperties
        );
        bitmap1.CopyFromBitmap(overlay);
        using var rtTexture = TextureTool.GetRtTexture(
            d2dRenderTarget.d3dDevice,
            imageSize.Width,
            imageSize.Height,
            bitmapProperties.PixelFormat.Format
        );
        var map = bitmap1.Map(MapOptions.Read);
        d2dRenderTarget.d3dDevice.ImmediateContext.UpdateSubresource(
            new SharpDX.DataBox(map.DataPointer, map.Pitch, 0),
            rtTexture
        );
        bitmap1.Unmap();
        using var texture = TextureTool.Crop(rtTexture, sourceRect.DxRect());
        var w = Mathf.CeilToInt(targetRect.Width);
        var h = Mathf.CeilToInt(targetRect.Height);
        var scale = Math.Max(128f / w, 128f / h);
        var mipTexture = TextureTool.Resize(
            texture,
            Mathf.CeilToInt(w * scale),
            Mathf.CeilToInt(h * scale),
            d2dRenderTarget.format
        );
        using var surface = mipTexture.QueryInterface<SharpDX.DXGI.Surface>();
        var pixelFormat = new SharpDX.Direct2D1.PixelFormat(
            d2dRenderTarget.format,
            SharpDX.Direct2D1.AlphaMode.Premultiplied
        );
        return new SharpDX.Direct2D1.Bitmap(
            d2dRenderTarget.target, surface,
            new SharpDX.Direct2D1.BitmapProperties(pixelFormat)
        );
    }

    public void SaveProject()
    {
        if (explorerControl != null)
        {
            project.directory = explorerControl.SerializeTreeView();
        }
        project.Save(Settings.Default.TexModWorkingDir);
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
        if (openFileDialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog.FileName;
                if (string.IsNullOrEmpty(fileName)) return;
                var key = modTexture.filename;
                modTexture.filename = fileName;
                modTexture.name = Path.GetFileName(fileName);
                if (working.modTexture != null && working.modTexture.id == modTexture.id)
                {
                    foreach (var mapping in project.FindTextureMapping(working.modTexture.id))
                    {
                        if (mapping.mipTexture != null)
                        {
                            mapping.mipTexture.Dispose();
                            mapping.mipTexture = null;
                        }
                    }
                    if (working.modTextureBitmap != null)
                    {
                        imageCache.ReturnToCache(key, working.modTextureBitmap);
                        working.modTextureBitmap = null;
                    }
                    working.modTextureSource = new AsyncImageFileSource(fileName);
                    working.modTextureSource.shared = true;
                    working.modTextureSource.LoadAsync();
                    modTextureCanvas?.SetImage(working.modTextureSource);
                    gameTextureCanvas?.Repaint();
                    RequestTexturePaintingUpdate();
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
        if (openFileDialog.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog.FileName;
                if (string.IsNullOrEmpty(fileName)) return;
                foreach (var modTex in project.modTextures.Values)
                {
                    if (modTex.filename.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        var name = Path.GetFileName(fileName);
                        var dialogResult = MessageBox.Show(
                            $"{name} already exists!\ncontinue?",
                            "Duplicate",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Information
                        );
                        if (dialogResult != DialogResult.Yes)
                        {
                            return;
                        }
                    }
                }

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
                explorerControl?.NewTexModNode(modTexture);
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
            toolsControl?.SetSrcRectWithoutNotify(rect);
            PictureBoxRectTool.SetRect(modTextureCanvas?.canvas, rect);
            gameTextureCanvas?.Repaint();
            modTextureCanvas?.Repaint();
            RequestTexturePaintingUpdate();
        }
    }

    internal void SetDestTextureRect(System.Drawing.RectangleF rect)
    {
        if (working.mapping is { } mapping)
        {
            mapping.targetRect = rect;
            toolsControl?.SetDestRect(rect);
            PictureBoxRectTool.SetRect(gameTextureCanvas?.canvas, rect);
            gameTextureCanvas?.Repaint();
            RequestTexturePaintingUpdate();
        }
    }

    internal bool GetSrcImageSize(out SharpDX.Size2 pixelSize)
    {
        if (working.modTextureBitmap != null)
        {
            pixelSize = working.modTextureBitmap.PixelSize;
            return true;
        }
        pixelSize = default;
        return false;
    }

    internal bool GetDestImageSize(out SharpDX.Size2 pixelSize)
    {
        if (working.gameTextureBitmap != null)
        {
            pixelSize = working.gameTextureBitmap.PixelSize;
            return true;
        }
        pixelSize = default;
        return false;
    }

    internal void FitSrcRectByDestWidth()
    {
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Width <= 0) return;
            if (working.modTextureBitmap != null)
            {
                var imageSize = working.modTextureBitmap.PixelSize;

                srcRect.X = 0;
                srcRect.Width = imageSize.Width;
                srcRect.Height = (descRect.Height / descRect.Width) * imageSize.Width;
                toolsControl?.SetSrcRectWithoutNotify(srcRect);
                SetSrcImageRect(srcRect);
            }
        }
    }

    internal void FitSrcRectByDestHeight()
    {
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Height <= 0) return;
            if (working.modTextureBitmap != null)
            {
                var imageSize = working.modTextureBitmap.PixelSize;

                srcRect.Y = 0;
                srcRect.Height = imageSize.Height;
                srcRect.Width = (descRect.Width / descRect.Height) * imageSize.Height;
                toolsControl?.SetSrcRectWithoutNotify(srcRect);
                SetSrcImageRect(srcRect);
            }
        }
    }

    internal void ClipSrcRectByDestWidth()
    {
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Width <= 0) return;

            srcRect.Height = (descRect.Height / descRect.Width) * srcRect.Width;
            toolsControl?.SetSrcRectWithoutNotify(srcRect);
            SetSrcImageRect(srcRect);
        }
    }

    internal void ClipSrcRectByDestHeight()
    {
        if (GetSrcImageRect(out var srcRect) && GetDestTextureRect(out var descRect))
        {
            if (descRect.Height <= 0) return;

            srcRect.Width = (descRect.Width / descRect.Height) * srcRect.Height;
            toolsControl?.SetSrcRectWithoutNotify(srcRect);
            SetSrcImageRect(srcRect);
        }
    }

    public void SetTextureMappingFlipX(bool flip)
    {
        if (working.mapping is { } mapping)
        {
            if (mapping.flipX != flip)
            {
                mapping.flipX = flip;
                gameTextureCanvas?.Repaint();
                mappingControl?.RepaintListView();
                RequestTexturePaintingUpdate();
            }
        }
    }

    public void SetTextureMappingFlipY(bool flip)
    {
        if (working.mapping is { } mapping)
        {
            if (mapping.flipY != flip)
            {
                mapping.flipY = flip;
                gameTextureCanvas?.Repaint();
                mappingControl?.RepaintListView();
                RequestTexturePaintingUpdate();
            }
        }
    }

    public void SetTextureMappingSwap(bool swap)
    {
        if (working.mapping is { } mapping)
        {
            if (mapping.swap != swap)
            {
                mapping.swap = swap;
                gameTextureCanvas?.Repaint();
                mappingControl?.RepaintListView();
                RequestTexturePaintingUpdate();
            }
        }
    }

    public void SetTextureMappingLod(TextureLod lod)
    {
        if (working.mapping is { } mapping)
        {
            if (mapping.lod != lod)
            {
                mapping.lod = lod;
                mappingControl?.RepaintListView();
            }
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
                renderer.camera.CurrentRotation = modTexture.rotation;
            }
            else
            {
                renderer.camera.Position = modTexture.position;
                renderer.camera.TargetRotation = modTexture.rotation;
                renderer.camera.CurrentRotation = modTexture.rotation;
            }
        }
    }
}