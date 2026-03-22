using CodeWalker.Properties;
using CodeWalker.Utils;
using SharpDX;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModForm : Form
{
    private TextureModForm()
    {
        InitializeComponent();
        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip1, version, theme);
        vsExtender.SetStyle(toolStrip2, version, theme);

        rectBoxX.Maximum = int.MaxValue;
        rectBoxY.Maximum = int.MaxValue;
        rectBoxW.Maximum = int.MaxValue;
        rectBoxH.Maximum = int.MaxValue;

        rectBoxX.Minimum = int.MinValue;
        rectBoxY.Minimum = int.MinValue;
        rectBoxW.Minimum = int.MinValue;
        rectBoxH.Minimum = int.MinValue;

        numericUpDown1.Maximum = int.MaxValue;
        numericUpDown1.Minimum = int.MinValue;

        numericUpDown2.Maximum = int.MaxValue;
        numericUpDown2.Minimum = int.MinValue;

        PictureBoxViewer.AddFeature(gameTextureCanvas);
        PictureBoxViewer.AddFeature(modTextureCanvas);

        PictureBoxRectTool.AddFeature(gameTextureCanvas, OnRectDrawingChange);
        PictureBoxRectTool.AddFeature(modTextureCanvas, OnRectDrawingChange);
        gameTextureCanvas.onPaint = PaintPreviewPicture;
        modTextureCanvas.onPaint = PaintTexturePicture;

        gameTextureCanvas.onBitmapLoaded = ResetImageViewer;
        modTextureCanvas.onBitmapLoaded = ResetImageViewer;
        UpdateGroupBoxVisibility();

        repViewModeBtn.SetEnumDrop<View>(x => modListView.View = x);
        repViewModeBtn.SelectEnum(modListView.View);

        toolStripButton7.SetEnumDrop<View>(x => textureMappingView.View = x);
        toolStripButton7.SelectEnum(textureMappingView.View);
    }

    static class TreeViewIcon
    {
        public const string folder = "folder";
        public const string folder_open = "folder_open";
        public const string document = "document";
    }

    private void LoadTreeView()
    {
        var imageList = new ImageList();
        imageList.Images.Add("folder", Resources.folder);
        imageList.Images.Add("folder-open", Resources.folder_open);
        imageList.Images.Add("document", Resources.document);
        treeView.ImageList = imageList;
        var root = new TreeNode("/");
        root.ImageKey = TreeViewIcon.folder;
        root.SelectedImageKey = TreeViewIcon.folder;

        var file = new TreeNode("texture.png");
        file.ImageKey = TreeViewIcon.document;
        file.SelectedImageKey = TreeViewIcon.document;

        root.Nodes.Add(file);

        var stateImageList = new ImageList();
        treeView.StateImageList = stateImageList;

        treeView.Nodes.Add(root);
        treeView.Refresh();
    }

    private void ResetImageViewer(D2DCanvas canvas)
    {
        var imageSize = canvas.GetImageSize();
        PictureBoxViewer.FitViewer(canvas, imageSize.Width, imageSize.Height);
        CheckImageUnload();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        modListView.VirtualListSize = project.modTextures.Count;
        if (modListView.VirtualListSize > 0 && modListView.SelectedIndices.Count == 0)
        {
            modListView.SelectedIndices.Add(0);
        }
        modListView.Refresh();
        LoadTreeView();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        d2dRenderTarget = new D2DRenderTarget();
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        Utilities.Dispose(ref d2dRenderTarget);
        base.OnHandleDestroyed(e);
    }

    private void PaintTexturePicture(D2DCanvas canvas, SharpDX.Direct2D1.RenderTarget target, SharpDX.Direct2D1.Bitmap bitmap)
    {
        PictureBoxViewer.Paint(canvas, bitmap);
        canvas.DrawBitmap(bitmap, 0, 0);
        PictureBoxRectTool.Paint(canvas);
    }

    private void PaintPreviewPicture(D2DCanvas canvas, SharpDX.Direct2D1.RenderTarget target, SharpDX.Direct2D1.Bitmap bitmap)
    {
        PictureBoxViewer.Paint(canvas, bitmap);
        if (working.mapping != null && working.modTexture != null)
        {
            DrawPreviewOverlay(
                target,
                gameTextureCanvas.GetImage(),
                modTextureCanvas.GetImage(),
                gameTextureCanvas.GetImageSize(),
                working.modTexture.sourceRect.Convert(),
                working.mapping.targetRect.Convert(),
                working.mapping.flipX,
                working.mapping.flipY,
                working.mapping.rotation
            );
        }
        PictureBoxRectTool.Paint(canvas);
    }

    private static void DrawPreviewOverlay(
        SharpDX.Direct2D1.RenderTarget target,
        SharpDX.Direct2D1.Bitmap baseImage,
        SharpDX.Direct2D1.Bitmap overlay,
        SharpDX.Size2 baseImageSize,
        System.Drawing.Rectangle srcRect,
        System.Drawing.Rectangle destRect,
        bool flipX,
        bool flipY,
        float rotation
    )
    {
        if (baseImage != null)
        {
            target.DrawBitmap(baseImage, 1f, SharpDX.Direct2D1.BitmapInterpolationMode.NearestNeighbor);
        }
        if (overlay == null) return;

        var imgBounds = new System.Drawing.Rectangle(0, 0, baseImageSize.Width, baseImageSize.Height);
        var clippedDest = System.Drawing.Rectangle.Intersect(destRect, imgBounds);

        if (clippedDest.Width <= 0 || clippedDest.Height <= 0)
            return;

        var scaleX = (float)srcRect.Width / destRect.Width;
        var scaleY = (float)srcRect.Height / destRect.Height;

        var dx = clippedDest.X - destRect.X;
        var dy = clippedDest.Y - destRect.Y;

        srcRect.X += (int)(dx * scaleX);
        srcRect.Y += (int)(dy * scaleY);
        srcRect.Width = (int)(clippedDest.Width * scaleX);
        srcRect.Height = (int)(clippedDest.Height * scaleY);

        var matrix = target.Transform;
        var center = new Vector2(
            (clippedDest.Left + clippedDest.Right) * 0.5f,
            (clippedDest.Top + clippedDest.Bottom) * 0.5f
        );
        target.Transform = Matrix3x2.Scaling(flipX ? -1 : 1, flipY ? -1 : 1, center) *
                           Matrix3x2.Rotation(rotation * Mathf.Deg2Rad, center) * matrix;

        target.DrawBitmap(
            overlay,
            clippedDest.Convert2(),
            1f,
            SharpDX.Direct2D1.BitmapInterpolationMode.Linear,
            srcRect.Convert2()
        );
        target.Transform = matrix;
    }

    private void OnRectDrawingChange(System.Drawing.Rectangle obj)
    {
        WritePanelDataToSource();
    }

    private void RefreshModListView()
    {
        modListView.VirtualListSize = project.modTextures.Count;
        if (modListView.VirtualListSize > 0 && modListView.SelectedIndices.Count == 0)
        {
            modListView.SelectedIndices.Add(0);
        }
        modListView.Refresh();
    }

    private void RefreshTextureMappingView(bool reload = false)
    {
        if (reload && working.modTexture != null)
        {
            project.FindTextureMapping(working.modTexture.id, replacements);
        }
        textureMappingView.VirtualListSize = replacements.Count;
        if (textureMappingView.VirtualListSize > 0 && textureMappingView.SelectedIndices.Count == 0)
        {
            textureMappingView.SelectedIndices.Add(0);
        }
        textureMappingView.Refresh();
    }

    private void replacementListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var replacement = replacements[e.ItemIndex];
        e.Item = new ListViewItem(replacement.name);
        e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, replacement.tag));
        e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, replacement.comment));
    }

    private void modListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var modTexture = project.modTextures.Values[e.ItemIndex];
        e.Item = new ListViewItem(modTexture.name);
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog1.FileName;
                if (string.IsNullOrEmpty(fileName)) return;
                using var stream = File.OpenRead(fileName);
                using var image = Image.FromStream(stream, false, false);
                var modTexture = project.CreateTextureMod(fileName);
                modTexture.sourceRect = new SharpDX.Rectangle(0, 0, image.Width, image.Height);

                RefreshModListView();
            }
            catch (Exception exception)
            {
                exception.ShowDialog();
            }
        }
    }

    private void modListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Label)) return;
        var replacement = project.modTextures.Values[e.Item];
        replacement.name = e.Label;
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
        var result = MessageBox.Show(
            Resources.Msg_BuildModConfirm,
            "Build",
            MessageBoxButtons.OKCancel,
            MessageBoxIcon.Question);
        if (result == DialogResult.OK)
        {
            BuildMod();
        }
    }

    private void modListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in modListView.SelectedIndices)
        {
            SelectTexMod(project.modTextures.Values[index]);
            break;
        }
    }

    private void replacementListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in textureMappingView.SelectedIndices)
        {
            SelectTextureMapping(replacements[index]);
            return;
        }
    }

    private void saveProjectBtn_Click(object sender, EventArgs e)
    {
        project.Save(Settings.Default.TexModWorkingDir);
    }

    private void SetRectBox(System.Drawing.Rectangle rectangle)
    {
        rectBoxX.Value = rectangle.X;
        rectBoxY.Value = rectangle.Y;
        rectBoxW.Value = rectangle.Width;
        rectBoxH.Value = rectangle.Height;
    }

    private void rectBoxX_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.X = (int)rectBoxX.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.X = (int)rectBoxX.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Y = (int)rectBoxY.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Y = (int)rectBoxY.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Width = (int)rectBoxW.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Width = (int)rectBoxW.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Height = (int)rectBoxH.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Height = (int)rectBoxH.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void imageTabControl_Selected(object sender, TabControlEventArgs e)
    {
        ReadPanelDataFromSource();
        UpdateGroupBoxVisibility();
    }

    private void UpdateGroupBoxVisibility()
    {
        var tabPage = imageTabControl.SelectedTab;
        groupBox1.Visible = tabPage == modTextureTabPage;
    }

    private void WritePanelDataToSource()
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            if (working.mapping != null)
            {
                working.mapping.targetRect = rect.Convert();
            }
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            if (working.modTexture != null)
            {
                working.modTexture.sourceRect = rect.Convert();
            }
            SetRectBox(rect);
        }
        ApplyDrawing();
    }

    private void ReadPanelDataFromSource()
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            var rect = new System.Drawing.Rectangle();
            if (working.mapping != null)
            {
                rect = working.mapping.targetRect.Convert();
            }
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(gameTextureCanvas);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(gameTextureCanvas);
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            var rect = new System.Drawing.Rectangle();
            if (working.modTexture != null)
            {
                rect = working.modTexture.sourceRect.Convert();
            }
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(modTextureCanvas);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(modTextureCanvas);
            SetRectBox(rect);

            numericUpDown1.Value = working.mapping?.targetRect.Width ?? 0;
            numericUpDown2.Value = working.mapping?.targetRect.Height ?? 0;
        }
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.SetSolid(gameTextureCanvas, checkBox2.Checked);
            gameTextureCanvas.Invalidate();
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.SetSolid(modTextureCanvas, checkBox2.Checked);
            modTextureCanvas.Invalidate();
        }
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.SetPaintEnable(gameTextureCanvas, checkBox1.Checked);
            gameTextureCanvas.Invalidate();
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.SetPaintEnable(modTextureCanvas, checkBox1.Checked);
            modTextureCanvas.Invalidate();
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        rectBoxH.Value = numericUpDown2.Value;
    }

    private void button1_Click(object sender, EventArgs e)
    {
        rectBoxW.Value = numericUpDown1.Value;
    }

    private void button3_Click(object sender, EventArgs e)
    {
        if (!modTextureCanvas.HasImage()) return;
        var imageSize = modTextureCanvas.GetImageSize();

        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (width <= 0) return;

        rectBoxX.Value = 0;
        rectBoxW.Value = imageSize.Width;
        rectBoxH.Value = height / width * imageSize.Width;
    }

    private void button4_Click(object sender, EventArgs e)
    {
        if (!modTextureCanvas.HasImage()) return;
        var imageSize = modTextureCanvas.GetImageSize();

        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (height <= 0) return;

        rectBoxY.Value = 0;
        rectBoxH.Value = imageSize.Height;
        rectBoxW.Value = width / height * imageSize.Width;
    }

    private void button6_Click(object sender, EventArgs e)
    {
        // clip by width
        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (width <= 0) return;

        var w = rectBoxW.Value;
        rectBoxH.Value = height / width * w;
    }

    private void button5_Click(object sender, EventArgs e)
    {
        // clip by height
        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (height <= 0) return;

        var h = rectBoxH.Value;
        rectBoxW.Value = width / height * h;
    }

    private void button7_Click(object sender, EventArgs e)
    {
        ApplyDrawing();
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        ApplyDrawing();
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        if (working == null) return;
        if (applyDrawing)
        {
            applyDrawing = false;
            ApplyDrawing();
        }
        if (working.modTextureSource != null && working.modTextureBitmap == null)
        {
            working.modTextureBitmap = working.modTextureSource.CreateBitmap(d2dRenderTarget.target);
            CheckImageUnload();
            gameTextureCanvas.Invalidate();
        }
        if (working.gameTextureSource != null && working.gameTextureBitmap == null)
        {
            working.gameTextureBitmap = working.gameTextureSource.CreateBitmap(d2dRenderTarget.target);
            CheckImageUnload();
            gameTextureCanvas.Invalidate();
        }
    }

    private void CheckImageUnload()
    {
        if (working == null) return;
        if (modTextureCanvas.HasImage() && working.modTextureBitmap != null)
        {
            Utilities.Dispose(ref working.modTextureSource);
        }
        if (gameTextureCanvas.HasImage() && working.gameTextureBitmap != null)
        {
            Utilities.Dispose(ref working.gameTextureSource);
        }
    }

    private void OnPropertyGridChanged()
    {
        gameTextureCanvas.Invalidate();
        RenderDrawing();
    }

    private void toolStripButton5_Click(object sender, EventArgs e)
    {
        // delete replacement
        foreach (int selectedIndex in textureMappingView.SelectedIndices)
        {
            var textureMapping = replacements[selectedIndex];
            if (MessageBox.Show($"Delete {textureMapping.name}?", "Delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            project.DeleteTextureMapping(textureMapping);
        }
        gameTextureCanvas.ClearImage();
        working.mapping = null;
        Utilities.Dispose(ref working.gameTextureBitmap);
        Utilities.Dispose(ref working.gameTextureSource);

        textureMappingView.VirtualListSize = 0;
        textureMappingView.SelectedIndices.Clear();
        RefreshTextureMappingView(true);
    }

    private void toolStripButton3_Click(object sender, EventArgs e)
    {
        // delete tex mod
        foreach (int selectedIndex in modListView.SelectedIndices)
        {
            var modTexture = project.modTextures.Values[selectedIndex];
            if (MessageBox.Show($"Delete {modTexture.name}?", "Delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            project.DeleteModTexture(modTexture);
        }
        modListView.VirtualListSize = 0;
        modListView.SelectedIndices.Clear();
        RefreshModListView();
        if (modListView.SelectedIndices.Count == 0)
        {
            SelectTexMod(null);
        }
    }

    private void toolStripButton8_Click(object sender, EventArgs e)
    {
        PackMod();
    }
}