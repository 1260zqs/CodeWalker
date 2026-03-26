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
using CodeWalker.Rendering;
using SharpDX;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    class WorkingState
    {
        public ModTexture modTexture;
        public TextureMapping mapping;
        public SourceTexture sourceTexture;

        public AsyncGameTextureSource gameTextureSource;
        public SharpDX.Direct2D1.Bitmap gameTextureBitmap;

        public AsyncImageFileSource modTextureSource;
        public SharpDX.Direct2D1.Bitmap modTextureBitmap;
    }

    static TextureModForm instance;
    // static TextureModProject workingProject;

    public static void ShowWindow(WorldForm worldForm)
    {
        instance = GetWindow(worldForm);
        if (instance == null) return;

        instance.Show();
        instance.Focus();
    }

    public static void ShowAddModSource(WorldForm worldForm, GameFile gameFile, string texName)
    {
        var window = GetWindow(worldForm);
        if (window == null) return;

        window.Show();
        window.Focus();
        window.Invoke(() =>
        {
            window.AddModSource(gameFile, texName);
        });
    }

    public static TextureModForm GetWindow(WorldForm worldForm)
    {
        if (instance == null || instance.IsDisposed)
        {
            instance = Create(worldForm);
        }
        return instance;
    }

    public static TextureModForm Create(WorldForm worldForm)
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
        var form = new TextureModForm(
            workingProject,
            worldForm.Renderer,
            worldForm.GameFileCache.RpfMan,
            new GTAVTextureModAdapter(workingProject, worldForm.GameFileCache)
        );
        return form;
    }

    private TextureModProject project;
    private TextureModAdapter adapter;
    private RpfManager rpfManager;
    private Renderer renderer;

    private CodeWalker.Graphic.D2DRenderTarget d2dRenderTarget;
    private WorkingState working = new();

    // private ModTexture currentMod;
    // private TextureReplacement currentReplacement;
    private List<TextureMapping> listOfMappings = new();
    private bool applyDrawing;

    private TextureModForm(TextureModProject workingProject, Renderer renderer, RpfManager rpfMan, TextureModAdapter adapter) : this()
    {
        this.project = workingProject;
        this.rpfManager = rpfMan;
        this.renderer = renderer;
        this.adapter = adapter;
    }

    private void SelectTexMod(ModTexture modTexture)
    {
        if (working.modTexture == modTexture)
        {
            return;
        }
        if (working.modTexture != null)
        {
            working.modTexture.editorState = PictureBoxViewer.SaveState(modTextureCanvas);
        }
        if (working.modTextureBitmap != null && working.modTexture != null)
        {
            var key = working.modTexture.filename;
            //imageCache.ReturnToPool(key, working.modTextureBitmap);
            //working.modTextureBitmap = null;
        }
        listOfMappings.Clear();
        working.modTexture = modTexture;
        working.mapping = null;

        Utilities.Dispose(ref working.modTextureSource);
        Utilities.Dispose(ref working.modTextureBitmap);
        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);

        modTextureCanvas.ClearImage();
        gameTextureCanvas.ClearImage();
        PictureBoxViewer.ResetViewer(modTextureCanvas);
        PictureBoxViewer.ResetViewer(gameTextureCanvas);
        if (modTexture != null)
        {
            if (imageCache.TryGetFromPool(modTexture.filename, out var bitmap))
            {
                working.modTextureBitmap = bitmap;
                modTextureCanvas.SetImage(bitmap);
            }
            else
            {
                working.modTextureSource = new AsyncImageFileSource(working.modTexture.filename);
                working.modTextureSource.shared = true;
                working.modTextureSource.LoadAsync();
                modTextureCanvas.SetImage(working.modTextureSource);
            }
            PictureBoxViewer.LoadState(modTextureCanvas, modTexture.editorState);
        }

        ReadPanelDataFromSource();
        propertyGridFix1.SelectedObject = null;
        textureMappingView.VirtualListSize = 0;
        textureMappingView.SelectedIndices.Clear();
        RefreshTextureMappingView(true);
    }

    private void SelectTextureMapping(TextureMapping mapping)
    {
        if (working.mapping == mapping)
        {
            return;
        }
        if (working.mapping != null)
        {
            working.mapping.editorState = PictureBoxViewer.SaveState(gameTextureCanvas);
        }
        if (working.gameTextureBitmap != null && working.sourceTexture != null)
        {
            var key = working.sourceTexture.sourceFile;
            //imageCache.ReturnToPool(key, working.gameTextureBitmap);
            //working.gameTextureBitmap = null;
        }

        working.mapping = mapping;
        working.sourceTexture = null;
        propertyGridFix1.SelectedObject = null;

        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);
        PictureBoxViewer.ResetViewer(gameTextureCanvas);
        gameTextureCanvas.ClearImage();

        if (mapping != null && project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
        {
            working.sourceTexture = sourceTexture;
            if (imageCache.TryGetFromPool(sourceTexture.sourceFile, out var bitmap))
            {
                working.gameTextureBitmap = bitmap;
                gameTextureCanvas.SetImage(bitmap);
            }
            else
            {
                working.gameTextureSource = new AsyncGameTextureSource(adapter, sourceTexture.sourceFile);
                working.gameTextureSource.shared = true;
                working.gameTextureSource.LoadAsync();
                gameTextureCanvas.SetImage(working.gameTextureSource);
            }
            ReadPanelDataFromSource();
            PictureBoxViewer.LoadState(gameTextureCanvas, mapping.editorState);
            propertyGridFix1.SelectedObject = TextureReplacementPropertyObject.From(project, mapping, OnPropertyGridChanged);
        }
        propertyGridFix1.Refresh();
    }

    public void AddModSource(GameFile gameFile, string texName)
    {
        foreach (int index in modListView.SelectedIndices)
        {
            var modTexture = project.modTextures.Values[index];
            if (modTexture != null)
            {
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
                var replacement = project.CreateReplacement();
                replacement.sourceTexture = sourceTexture.id;
                replacement.modTexture = modTexture.id;
                replacement.name = texName;
                RefreshTextureMappingView(true);
                return;
            }
        }
    }

    private void RenderDrawing()
    {
        applyDrawing = true;
    }

    private void ApplyDrawing()
    {
        if (working.modTexture == null || working.mapping == null) return;

        var targetRect = working.mapping.targetRect;
        var sourceTexture = project.sourceTextures[working.mapping.sourceTexture];
        var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
        var nameHash = JenkHash.GenHash(textureName.ToLowerInvariant());

        var sourceTex = gameTextureCanvas.GetImage();
        if (sourceTex == null) return;

        if (!Monitor.TryEnter(renderer.DXMan.syncroot, 50))
        {
            return;
        }
        try
        {
            d2dRenderTarget.SetTargetSize(renderer.Device, sourceTex.PixelSize);
            d2dRenderTarget.BeginDraw();

            var overlay = working.modTextureBitmap;
            if (!checkBox1.Checked) overlay = null;
            DrawPreviewOverlay(
                d2dRenderTarget.target,
                working.gameTextureBitmap,
                overlay,
                sourceTex.PixelSize,
                working.modTexture.sourceRect,
                working.mapping.targetRect,
                working.mapping.flipX,
                working.mapping.flipY,
                working.mapping.rotation
            );
            if (checkBox3.Checked)
            {
                d2dRenderTarget.FillRectangle(targetRect.Raw());
            }
            d2dRenderTarget.EndDraw();

            var pixelSize = sourceTex.PixelSize;
            var texture = renderer.RenderableCache.FindRenderableTexture(x =>
            {
                var tex = x.Key;
                if (tex.NameHash == nameHash)
                {
                    return tex.Width == pixelSize.Width && tex.Height == pixelSize.Height;
                }
                return false;
            });
            d2dRenderTarget.CopyTo(renderer.Device, texture);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Monitor.Exit(renderer.DXMan.syncroot);
    }

    public class TextureReplacementPropertyObject
    {
        [ReadOnly(true)]
        public string id { get; set; }

        public string tag
        {
            get => sourceObject.tag;
            set
            {
                sourceObject.tag = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public string name
        {
            get => sourceObject.name;
            set
            {
                sourceObject.name = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public string comment
        {
            get => sourceObject.comment;
            set
            {
                sourceObject.comment = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        [ReadOnly(true)]
        public string modTexture { get; set; }

        [ReadOnly(true)]
        public string sourceTexture { get; set; }

        public string targetRect { get; set; }

        public bool flipX
        {
            get => sourceObject.flipX;
            set
            {
                sourceObject.flipX = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public bool flipY
        {
            get => sourceObject.flipY;
            set
            {
                sourceObject.flipY = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        public float rotation
        {
            get => sourceObject.rotation;
            set
            {
                sourceObject.rotation = value;
                onPropertyGridChanged?.Invoke();
            }
        }

        private TextureMapping sourceObject;
        private Action onPropertyGridChanged;

        public static TextureReplacementPropertyObject From(TextureModProject project, TextureMapping mapping, Action onPropertyGridChanged)
        {
            var propertyObject = new TextureReplacementPropertyObject();
            if (project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
            {
                propertyObject.sourceTexture = sourceTexture.sourceFile;
            }
            if (project.modTextures.TryGetValue(mapping.modTexture, out var modTexture))
            {
                propertyObject.modTexture = modTexture.filename;
            }
            propertyObject.id = mapping.id.ToString("N");
            propertyObject.sourceObject = mapping;
            propertyObject.onPropertyGridChanged = onPropertyGridChanged;
            return propertyObject;
        }
    }
}