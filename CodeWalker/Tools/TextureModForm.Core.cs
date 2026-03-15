using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using Color = System.Drawing.Color;

namespace CodeWalker.Tools;

public partial class TextureModForm
{
    public TextureModProject project;
    public TextureModAdapter adapter;

    private ModTexture currentMod;
    private TextureReplacement currentReplacement;
    private List<TextureReplacement> replacements = new();
    private bool applyDrawing;

    private void OnSelectTexMod(ModTexture modTexture)
    {
        currentMod = modTexture;
        currentReplacement = null;
        replacements.Clear();
        if (currentMod != null)
        {
            project.FindTextureReplacements(currentMod.id, replacements);
            DisplayPicture(pictureBox1Async, currentMod.filename);
            PictureBoxViewer.ResetViewer(pictureBox1Async.pictureBox);
            PictureBoxViewer.LoadState(pictureBox1, currentMod.editorState);
        }
        ReadPanelDataFromSource();
        replacementListView.SelectedIndices.Clear();
        RefreshReplacementListView();
    }

    private void OnReplacementSelected(TextureReplacement replacement)
    {
        currentReplacement = replacement;
        ReadPanelDataFromSource();
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
        var form = new TextureModForm();
        form.project = new CodeWalker.TexMod.TextureModProject();
        form.adapter = new CodeWalker.TexMod.GTAVTextureModAdapter();
        form.adapter.worldForm = worldForm;
        return form;
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

    public static void ShowAddModSource(WorldForm worldForm, GameFile gameFile, string texName)
    {
        var window = GetWindow(worldForm);
        window.AddModSource(gameFile, texName);
        window.Focus();
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
        var targetImage = pictureBox1Async.GetImage();
        var sourceImage = previewPictureBoxAsync.GetImage();
        if (sourceImage == null || targetImage == null) return;
        if (currentMod == null || currentReplacement == null) return;

        var image = (Image)targetImage;
        var sourceTex = (Image)sourceImage;

        var sourceRect = currentMod.sourceRect;
        var targetRect = currentReplacement.targetRect;
        var sourceTexture = project.sourceTextures[currentReplacement.sourceTexture];
        var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
        var nameHash = JenkHash.GenHash(textureName.ToLowerInvariant());

        var width = sourceTex.Width;
        var height = sourceTex.Height;
        using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using (var g = Graphics.FromImage(bitmap))
        {
            g.Clear(Color.Transparent);
            g.DrawImageUnscaled(sourceTex, 0, 0);
            if (checkBox3.Checked)
            {
                g.FillRectangle(Brushes.Lime, targetRect.Convert());
            }
            else
            {
                g.DrawImage(image, targetRect.Convert(), sourceRect.Convert(), GraphicsUnit.Pixel);
            }
        }

        lock (adapter.worldForm.RenderSyncRoot)
        {
            var tex = adapter.worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
                x.Key.NameHash == nameHash);
            if (tex == null) return;
            tex.Load(adapter.worldForm.Renderer.Device, bitmap);
        }
    }
}