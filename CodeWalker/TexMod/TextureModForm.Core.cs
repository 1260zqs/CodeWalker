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

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    static TextureModForm instance;
    static TextureModProject workingProject;
    private D2DRenderTarget d2dRenderTarget;

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
        form.adapter = new GTAVTextureModAdapter(workingProject, worldForm);
        return form;
    }

    public TextureModProject project;
    public TextureModAdapter adapter;
    public WorldForm worldForm;

    private ModTexture currentMod;
    private TextureReplacement currentReplacement;
    private List<TextureReplacement> replacements = new();
    private bool applyDrawing;

    private void SelectTexMod(ModTexture modTexture)
    {
        if (currentMod != null)
        {
            currentMod.editorState = PictureBoxViewer.SaveState(textureCanvas);
        }
        replacements.Clear();
        currentMod = modTexture;
        currentReplacement = null;
        textureCanvas.ClearImage();
        previewCanvas.ClearImage();
        PictureBoxViewer.ResetViewer(textureCanvas);
        PictureBoxViewer.ResetViewer(previewCanvas);
        if (currentMod != null)
        {
            textureCanvas.SetImage(currentMod.filename);
            PictureBoxViewer.LoadState(textureCanvas, currentMod.editorState);
        }
        ReadPanelDataFromSource();
        propertyGridFix1.SelectedObject = null;
        replacementListView.SelectedIndices.Clear();
        RefreshReplacementListView(true);
    }

    private void SelecteTexReplacement(TextureReplacement replacement)
    {
        propertyGridFix1.SelectedObject = null;
        if (project.sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
        {
            if (currentReplacement != null)
            {
                currentReplacement.editorState = PictureBoxViewer.SaveState(previewCanvas);
            }
            currentReplacement = replacement;
            ReadPanelDataFromSource();

            PictureBoxViewer.ResetViewer(previewCanvas);
            PictureBoxViewer.LoadState(previewCanvas, replacement.editorState);
            previewCanvas.SetImage(new AsyncGameTextureSource(adapter, sourceTexture.sourceFile));
            propertyGridFix1.SelectedObject = TextureReplacementPropertyObject.From(replacement);
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
        if (applyDrawing)
        {
            applyDrawing = false;
            RenderDrawing();
        }
    }

    private void RenderDrawing()
    {
        applyDrawing = true;
    }

    private void ApplyDrawing()
    {
        // return;
        // var targetImage = pictureBox1Async.GetImage();
        // var sourceImage = previewPictureBoxAsync.GetImage();
        // if (sourceImage == null || targetImage == null) return;
        if (currentMod == null || currentReplacement == null) return;
        //
        // var image = (Image)targetImage;
        // var sourceTex = (Image)sourceImage;
        //
        // var sourceRect = currentMod.sourceRect;
        var targetRect = currentReplacement.targetRect;
        var sourceTexture = project.sourceTextures[currentReplacement.sourceTexture];
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

            //d2dRenderTarget.target.DrawBitmap(sourceTex, 1, BitmapInterpolationMode.NearestNeighbor);
            if (checkBox3.Checked)
            {
                d2dRenderTarget.FillRectangle(targetRect.Convert2());
            }

            d2dRenderTarget.EndDraw();

            var texture = worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
                x.Key.NameHash == nameHash);
            d2dRenderTarget.CopyTo(worldForm.Renderer.Device, texture);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Monitor.Exit(worldForm.Renderer.DXMan.syncroot);
    }

    public class TextureReplacementPropertyObject
    {
        [ReadOnly(true)]
        public string id { get; set; }

        public string tag { get; set; }
        public string name { get; set; }
        public string comment { get; set; }

        [ReadOnly(true)]
        public string modTexture { get; set; }

        [ReadOnly(true)]
        public string sourceTexture { get; set; }

        public string targetRect { get; set; }

        public bool flipX { get; set; }
        public bool flipY { get; set; }
        public float rotation { get; set; }

        public static TextureReplacementPropertyObject From(TextureReplacement replacement)
        {
            var propertyObject = new TextureReplacementPropertyObject();
            propertyObject.id = replacement.id.ToString("N");
            propertyObject.tag = replacement.tag;
            propertyObject.name = replacement.name;
            propertyObject.comment = replacement.comment;

            propertyObject.flipX = replacement.flipX;
            propertyObject.flipX = replacement.flipX;
            propertyObject.rotation = replacement.rotation;

            return propertyObject;
        }
    }
}