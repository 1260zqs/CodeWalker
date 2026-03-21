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
    class WorkingState
    {
        public ModTexture modTexture;
        public TextureReplacement replacement;

        public AsyncBitmapSource modTextureSource;
        public AsyncBitmapSource replaceTextureSource;

        public SharpDX.Direct2D1.Bitmap modTextureBitmap;
        public SharpDX.Direct2D1.Bitmap replaceTextureBitmap;
    }

    static TextureModForm instance;
    static TextureModProject workingProject;

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
        if (workingProject == null)
        {
            workingProject = TextureModProject.SetupWorkingProject(workingDir);
        }
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
        var form = new TextureModForm();
        form.project = workingProject;
        form.worldForm = worldForm;
        form.adapter = new GTAVTextureModAdapter(workingProject, worldForm.GameFileCache);
        return form;
    }

    public TextureModProject project;
    public TextureModAdapter adapter;
    public WorldForm worldForm;

    private D2DRenderTarget d2dRenderTarget;
    private WorkingState working = new();

    // private ModTexture currentMod;
    // private TextureReplacement currentReplacement;
    private List<TextureReplacement> replacements = new();
    private bool applyDrawing;

    private void SelectTexMod(ModTexture modTexture)
    {
        if (working.modTexture != null)
        {
            working.modTexture.editorState = PictureBoxViewer.SaveState(textureCanvas);
        }
        replacements.Clear();
        working.modTexture = modTexture;
        working.replacement = null;

        Utilities.Dispose(ref working.modTextureSource);
        Utilities.Dispose(ref working.modTextureBitmap);
        Utilities.Dispose(ref working.replaceTextureBitmap);
        Utilities.Dispose(ref working.replaceTextureSource);

        textureCanvas.ClearImage();
        previewCanvas.ClearImage();
        PictureBoxViewer.ResetViewer(textureCanvas);
        PictureBoxViewer.ResetViewer(previewCanvas);
        if (modTexture != null)
        {
            working.modTextureSource = new AsyncImageFileSource(working.modTexture.filename);
            working.modTextureSource.shared = true;
            working.modTextureSource.LoadAsync();
            textureCanvas.SetImage(working.modTextureSource);
            PictureBoxViewer.LoadState(textureCanvas, modTexture.editorState);
        }

        ReadPanelDataFromSource();
        propertyGridFix1.SelectedObject = null;
        replacementListView.VirtualListSize = 0;
        replacementListView.SelectedIndices.Clear();
        RefreshReplacementListView(true);
    }

    private void SelecteTexReplacement(TextureReplacement replacement)
    {
        propertyGridFix1.SelectedObject = null;
        if (working.replacement != null)
        {
            working.replacement.editorState = PictureBoxViewer.SaveState(previewCanvas);
        }
        working.replacement = replacement;
        Utilities.Dispose(ref working.replaceTextureBitmap);
        Utilities.Dispose(ref working.replaceTextureSource);
        PictureBoxViewer.ResetViewer(previewCanvas);
        previewCanvas.ClearImage();

        if (project.sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
        {
            ReadPanelDataFromSource();
            working.replaceTextureSource = new AsyncGameTextureSource(adapter, sourceTexture.sourceFile);
            working.replaceTextureSource.shared = true;
            working.replaceTextureSource.LoadAsync();
            previewCanvas.SetImage(working.replaceTextureSource);
            PictureBoxViewer.LoadState(previewCanvas, replacement.editorState);
            propertyGridFix1.SelectedObject = TextureReplacementPropertyObject.From(replacement, OnPropertyGridChanged);
            propertyGridFix1.Refresh();
        }
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
                foreach (var item in project.replacements)
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
                RefreshReplacementListView(true);
                return;
            }
        }
    }

    private void RenderUpdate()
    {

    }

    private void RenderDrawing()
    {
        applyDrawing = true;
    }

    private void ApplyDrawing()
    {
#if true
        // return;
        // var targetImage = pictureBox1Async.GetImage();
        // var sourceImage = previewPictureBoxAsync.GetImage();
        // if (sourceImage == null || targetImage == null) return;
        if (working.modTexture == null || working.replacement == null) return;
        //
        // var image = (Image)targetImage;
        // var sourceTex = (Image)sourceImage;
        //
        // var sourceRect = currentMod.sourceRect;
        var targetRect = working.replacement.targetRect;
        var sourceTexture = project.sourceTextures[working.replacement.sourceTexture];
        var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
        var nameHash = JenkHash.GenHash(textureName.ToLowerInvariant());

        var sourceTex = previewCanvas.GetImage();
        if (sourceTex == null) return;

        if (!Monitor.TryEnter(worldForm.Renderer.DXMan.syncroot, 50))
        {
            return;
        }
        try
        {
            d2dRenderTarget.SetTargetSize(worldForm.Renderer.Device, sourceTex.PixelSize);
            d2dRenderTarget.BeginDraw();

            DrawPreviewOverlay(
                d2dRenderTarget.target,
                working.replaceTextureBitmap,
                working.modTextureBitmap,
                working.replaceTextureBitmap.PixelSize,
                working.modTexture.sourceRect.Convert(),
                working.replacement.targetRect.Convert(),
                working.replacement.flipX,
                working.replacement.flipY,
                working.replacement.rotation
            );
            if (checkBox3.Checked)
            {
                d2dRenderTarget.FillRectangle(targetRect.Convert2());
            }

            d2dRenderTarget.EndDraw();
            if (checkBox3.Checked)
            {
                // checkBox3.Checked = false;
                // var bytes = d2dRenderTarget.Encode(Utils.NVTT.Format.Format_DXT1, Utils.NVTT.Quality.Quality_Normal);
                // if (bytes != null)
                // {
                //     File.WriteAllBytes("C:\\xx.dds", bytes);
                // }
            }
            var texture = worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
                x.Key.NameHash == nameHash);
            d2dRenderTarget.CopyTo(worldForm.Renderer.Device, texture);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Monitor.Exit(worldForm.Renderer.DXMan.syncroot);
#endif
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

        private TextureReplacement sourceObject;
        private Action onPropertyGridChanged;

        public static TextureReplacementPropertyObject From(TextureReplacement replacement, Action onPropertyGridChanged)
        {
            var propertyObject = new TextureReplacementPropertyObject();
            propertyObject.id = replacement.id.ToString("N");
            propertyObject.sourceObject = replacement;
            propertyObject.onPropertyGridChanged = onPropertyGridChanged;
            return propertyObject;
        }
    }
}