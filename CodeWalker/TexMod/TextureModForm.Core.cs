using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Forms;
using Color = System.Drawing.Color;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    static TextureModForm instance;
    static CodeWalker.TexMod.TextureModProject workingProject;

    public static void ShowWindow(WorldForm worldForm)
    {
        instance = GetWindow(worldForm);
        if (instance == null) return;

        instance.Show(worldForm);
        instance.Focus();
    }

    public static void ShowAddModSource(WorldForm worldForm, GameFile gameFile, string texName)
    {
        var window = GetWindow(worldForm);
        if (window == null) return;

        window.AddModSource(gameFile, texName);
        window.Show(window);
        window.Focus();
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
            workingProject = CodeWalker.TexMod.TextureModProject.SetupWorkingProject(workingDir);
        }
        if (string.IsNullOrEmpty(workingProject.manifestFile) || !File.Exists(workingProject.manifestFile))
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
        form.adapter = new CodeWalker.TexMod.GTAVTextureModAdapter(worldForm);
        return form;
    }

    public TextureModProject project;
    public TextureModAdapter adapter;

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
        replacementListView.SelectedIndices.Clear();
        RefreshReplacementListView(true);
    }

    private void SelecteTexReplacement(TextureReplacement replacement)
    {
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
        // var targetImage = pictureBox1Async.GetImage();
        // var sourceImage = previewPictureBoxAsync.GetImage();
        // if (sourceImage == null || targetImage == null) return;
        // if (currentMod == null || currentReplacement == null) return;
        //
        // var image = (Image)targetImage;
        // var sourceTex = (Image)sourceImage;
        //
        // var sourceRect = currentMod.sourceRect;
        // var targetRect = currentReplacement.targetRect;
        // var sourceTexture = project.sourceTextures[currentReplacement.sourceTexture];
        // var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
        // var nameHash = JenkHash.GenHash(textureName.ToLowerInvariant());
        //
        // var width = sourceTex.Width;
        // var height = sourceTex.Height;
        // using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        // using (var g = Graphics.FromImage(bitmap))
        // {
        //     g.Clear(Color.Transparent);
        //     g.DrawImageUnscaled(sourceTex, 0, 0);
        //     if (checkBox3.Checked)
        //     {
        //         g.FillRectangle(Brushes.Lime, targetRect.Convert());
        //     }
        //     else
        //     {
        //         g.DrawImage(image, targetRect.Convert(), sourceRect.Convert(), GraphicsUnit.Pixel);
        //     }
        // }
        //
        // lock (adapter.worldForm.RenderSyncRoot)
        // {
        //     var tex = adapter.worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
        //         x.Key.NameHash == nameHash);
        //     if (tex == null) return;
        //     tex.Load(adapter.worldForm.Renderer.Device, bitmap);
        // }
    }
}