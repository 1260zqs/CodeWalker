using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.Graphic;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;

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

    public bool isPainting
    {
        get => m_IsPainting;
        set => m_IsPainting = value;
    }

    public bool isDrawTestColor
    {
        get => m_IsDrawTestColor;
        set => m_IsDrawTestColor = value;
    }

    private bool m_IsPainting = true;
    private bool m_IsDrawTestColor;

    public void SelectTexMod(ModTexture modTexture)
    {
        if (working.modTexture == modTexture)
        {
            return;
        }
        if (working.modTexture != null && modTextureCanvas != null)
        {
            working.modTexture.editorState = PictureBoxViewer.SaveState(modTextureCanvas.canvas);
        }
        if (working.mapping != null && gameTextureCanvas != null)
        {
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas.canvas);
        }
        if (working.modTextureBitmap != null && working.modTexture != null)
        {
            var key = working.modTexture.filename;
            imageCache.ReturnToPool(key, working.modTextureBitmap);
            working.modTextureBitmap = null;
        }
        if (working.gameTextureBitmap != null && working.sourceTexture != null)
        {
            var key = working.sourceTexture.sourceFile;
            imageCache.ReturnToPool(key, working.gameTextureBitmap);
            working.gameTextureBitmap = null;
        }
        mappingControl?.Clear();
        working.modTexture = modTexture;
        working.texNameHash = 0;
        working.mapping = null;

        Utilities.Dispose(ref working.modTextureSource);
        Utilities.Dispose(ref working.modTextureBitmap);
        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);

        modTextureCanvas?.ClearImage();
        gameTextureCanvas?.ClearImage();
        PictureBoxViewer.ResetViewer(modTextureCanvas?.canvas);
        PictureBoxViewer.ResetViewer(gameTextureCanvas?.canvas);
        if (modTexture != null)
        {
            if (imageCache.TryGetFromPool(modTexture.filename, out var bitmap))
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
            PictureBoxViewer.LoadState(modTextureCanvas?.canvas, modTexture.editorState);
        }
        propertyControl?.SelectObject(null);
        mappingControl?.RefreshListView(modTexture);
    }

    public void SelectTextureMapping(TextureMapping mapping)
    {
        if (working.mapping == mapping)
        {
            return;
        }
        if (working.mapping != null && gameTextureCanvas != null)
        {
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas.canvas);
        }
        if (working.sourceTexture != null && working.gameTextureBitmap != null)
        {
            var key = working.sourceTexture.sourceFile;
            imageCache.ReturnToPool(key, working.gameTextureBitmap);
            working.gameTextureBitmap = null;
        }

        working.texNameHash = 0;
        working.mapping = mapping;
        working.sourceTexture = null;
        propertyControl?.SelectObject(null);

        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);
        PictureBoxViewer.ResetViewer(gameTextureCanvas?.canvas);
        gameTextureCanvas?.ClearImage();

        if (mapping != null && project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
        {
            working.sourceTexture = sourceTexture;
            var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
            working.texNameHash = JenkHash.GenHash(textureName.ToLowerInvariant());
            if (imageCache.TryGetFromPool(sourceTexture.sourceFile, out var bitmap))
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
            propertyControl?.SelectObject(mapping);
        }
        propertyControl?.Refresh();
    }

    private void OnModTextureReady(Bitmap bitmap)
    {
        if (working.modTexture.editorState == null && modTextureCanvas != null)
        {
            var imageSize = bitmap.PixelSize;
            PictureBoxViewer.FitViewer(modTextureCanvas.canvas, imageSize.Width, imageSize.Height);
            working.modTexture.editorState = PictureBoxViewer.SaveState(modTextureCanvas.canvas);
        }
        UpdateTexturePainting();
    }

    private void OnGameTextureReady(Bitmap bitmap)
    {
        if (working.mapping.editorState == null && gameTextureCanvas != null)
        {
            var imageSize = bitmap.PixelSize;
            PictureBoxViewer.FitViewer(gameTextureCanvas.canvas, imageSize.Width, imageSize.Height);
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas.canvas);
        }
        UpdateTexturePainting();
    }

    public void UpdateTexturePainting()
    {
        BeginInvoke(CommitTexturePainting);
    }

    private void CommitTexturePainting()
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
}