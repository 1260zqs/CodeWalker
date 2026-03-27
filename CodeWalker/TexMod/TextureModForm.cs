using CodeWalker.Properties;
using CodeWalker.Utils;
using SharpDX;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SharpDX.WIC;
using WeifenLuo.WinFormsUI.Docking;
using CodeWalker.Graphic;

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

        rectBoxX.Maximum = decimal.MaxValue;
        rectBoxY.Maximum = decimal.MaxValue;
        rectBoxW.Maximum = decimal.MaxValue;
        rectBoxH.Maximum = decimal.MaxValue;

        rectBoxX.Minimum = decimal.MinValue;
        rectBoxY.Minimum = decimal.MinValue;
        rectBoxW.Minimum = decimal.MinValue;
        rectBoxH.Minimum = decimal.MinValue;

        numericUpDown1.Maximum = decimal.MaxValue;
        numericUpDown1.Minimum = decimal.MinValue;

        numericUpDown2.Maximum = decimal.MaxValue;
        numericUpDown2.Minimum = decimal.MinValue;

        PictureBoxViewer.AddFeature(gameTextureCanvas);
        PictureBoxViewer.AddFeature(modTextureCanvas);

        PictureBoxRectTool.AddFeature(gameTextureCanvas, OnRectDrawingChange);
        PictureBoxRectTool.AddFeature(modTextureCanvas, OnRectDrawingChange);
        gameTextureCanvas.onPaint = PaintPreviewPicture;
        modTextureCanvas.onPaint = PaintTexturePicture;

        gameTextureCanvas.onBitmapLoaded = OnBitmapLoaded;
        modTextureCanvas.onBitmapLoaded = OnBitmapLoaded;
        UpdateGroupBoxVisibility();

        repViewModeBtn.SetEnumDrop<View>(x => modListView.View = x);
        repViewModeBtn.SelectEnum(modListView.View);

        toolStripButton7.SetEnumDrop<View>(x => textureMappingView.View = x);
        toolStripButton7.SelectEnum(textureMappingView.View);
        this.KeyDown += (sender, e) =>
        {
            if (e.Alt)
            {
                drawBoxFrame = true;
                gameTextureCanvas.Invalidate();
            }
        };
        this.KeyUp += (sender, e) =>
        {
            if (e.Alt)
            {
                drawBoxFrame = false;
                gameTextureCanvas.Invalidate();
            }
        };
    }

    private void OnBitmapLoaded(D2DCanvas canvas)
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
        var d3dDevice = DXGraphic.GetDevice();
        var d2dFactory = DXGraphic.d2dFactory;
        d2dRenderTarget = new D2DRenderTarget(d3dDevice, d2dFactory);
        d2dRenderTarget.SetTargetSize(new Size2(1, 1));
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        Utilities.Dispose(ref d2dRenderTarget);
        base.OnHandleDestroyed(e);
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        imageCache.Clear();
    }

    private void PaintTexturePicture(D2DCanvas canvas, SharpDX.Direct2D1.RenderTarget target, SharpDX.Direct2D1.Bitmap bitmap)
    {
        PictureBoxViewer.Paint(canvas, bitmap);
        canvas.DrawBitmap(bitmap, 0, 0);
        PictureBoxRectTool.Paint(canvas);
    }

    private bool drawBoxFrame;

    private void PaintPreviewPicture(D2DCanvas canvas, SharpDX.Direct2D1.RenderTarget target, SharpDX.Direct2D1.Bitmap bitmap)
    {
        PictureBoxViewer.Paint(canvas, bitmap);
        if (working.mapping != null && working.modTexture != null)
        {
            var overlay = modTextureCanvas.GetImage();
            if (!checkBox1.Checked) overlay = null;
            DrawPreviewOverlay(
                target,
                gameTextureCanvas.GetImage(),
                overlay,
                gameTextureCanvas.GetImageSize(),
                working.modTexture.sourceRect,
                working.mapping.targetRect,
                working.mapping.flipX,
                working.mapping.flipY,
                working.mapping.rotation
            );
        }
        if (drawBoxFrame && working.mapping != null)
        {
            PictureBoxViewer.GetState(canvas, out var zoom, out _);
            foreach (var mapping in project.textureMappings)
            {
                if (mapping.sourceTexture == working.mapping.sourceTexture)
                {
                    var rect = mapping.targetRect;
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        var color = new SharpDX.Mathematics.Interop.RawColor4(1f, 0f, 1f, 1f);
                        canvas.DrawRectangle(rect, color, 1f / zoom);

                        var text = $"{mapping.name} Left={mapping.targetRect.Left} Top={mapping.targetRect.Top} Right={mapping.targetRect.Right} Bottom={mapping.targetRect.Bottom}";
                        var textLayout = DXGraphic.CreateTextLayout(text, DXGraphic.fontSegoeUI_12);

                        var metrics = textLayout.Metrics;
                        var textRect = new SharpDX.Mathematics.Interop.RawRectangleF();
                        textRect.Left = rect.Left;
                        textRect.Right = rect.Left + metrics.Width;
                        textRect.Top = rect.Top - metrics.Height;
                        textRect.Bottom = rect.Top;
                        canvas.FillRectangle(textRect, color);
                        canvas.DrawTextLayout(textRect.Left, textRect.Top, textLayout, SharpDX.Color.White);
                    }
                }
            }
        }
        PictureBoxRectTool.Paint(canvas);
    }

    private List<TextureMapping> tempList = new();

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
        if (overlay == null) return;

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

    private void OnRectDrawingChange(System.Drawing.RectangleF obj)
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
            project.FindTextureMapping(working.modTexture.id, listOfMappings);
        }
        textureMappingView.VirtualListSize = listOfMappings.Count;
        if (textureMappingView.VirtualListSize > 0 && textureMappingView.SelectedIndices.Count == 0)
        {
            textureMappingView.SelectedIndices.Add(0);
        }
        textureMappingView.Refresh();
    }

    private void replacementListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var replacement = listOfMappings[e.ItemIndex];
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
        if (openFileDialog1.ShowDialog(this) == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog1.FileName;
                if (string.IsNullOrEmpty(fileName)) return;

                using var decoder = new BitmapDecoder(DXGraphic.wicFactory, fileName, DecodeOptions.CacheOnLoad);
                using var converter = new FormatConverter(DXGraphic.wicFactory);
                using var frame = decoder.GetFrame(0);

                converter.Initialize(
                    frame,
                    SharpDX.WIC.PixelFormat.Format32bppRGBA,
                    BitmapDitherType.None,
                    null,
                    0,
                    BitmapPaletteType.Custom
                );

                var width = converter.Size.Width;
                var height = converter.Size.Height;

                var modTexture = project.CreateTextureMod(fileName);
                modTexture.sourceRect = new System.Drawing.RectangleF(0, 0, width, height);

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
            this,
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
            SelectTextureMapping(listOfMappings[index]);
            return;
        }
    }

    private void saveProjectBtn_Click(object sender, EventArgs e)
    {
        project.directory = SaveTreeView();
        project.Save(Settings.Default.TexModWorkingDir);
    }

    private void SetRectBox(System.Drawing.RectangleF rectangle)
    {
        rectBoxX.Value = (decimal)rectangle.X;
        rectBoxY.Value = (decimal)rectangle.Y;
        rectBoxW.Value = (decimal)rectangle.Width;
        rectBoxH.Value = (decimal)rectangle.Height;
    }

    private void rectBoxX_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.X = (float)rectBoxX.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.X = (float)rectBoxX.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Y = (float)rectBoxY.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Y = (float)rectBoxY.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Width = (float)rectBoxW.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Width = (float)rectBoxW.Value;
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            rect.Height = (float)rectBoxH.Value;
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            rect.Height = (float)rectBoxH.Value;
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
        panel1.AutoSize = false;
        var tabPage = imageTabControl.SelectedTab;
        groupBox1.Visible = tabPage == modTextureTabPage;
        groupBox3.Visible = tabPage == gameTextureTabPage;
        groupBox4.Visible = tabPage == modTextureTabPage;
    }

    private void WritePanelDataToSource()
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            PictureBoxRectTool.GetRect(gameTextureCanvas, out var rect);
            if (working.mapping != null)
            {
                working.mapping.targetRect = rect;
            }
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            PictureBoxRectTool.GetRect(modTextureCanvas, out var rect);
            if (working.modTexture != null)
            {
                working.modTexture.sourceRect = rect;
            }
            SetRectBox(rect);
        }
        ApplyDrawing();
    }

    private void ReadPanelDataFromSource()
    {
        if (imageTabControl.SelectedTab == gameTextureTabPage)
        {
            var rect = new System.Drawing.RectangleF();
            if (working.mapping != null)
            {
                rect = working.mapping.targetRect;
            }
            PictureBoxRectTool.SetRect(gameTextureCanvas, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(gameTextureCanvas);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(gameTextureCanvas);
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == modTextureTabPage)
        {
            var rect = new System.Drawing.RectangleF();
            if (working.modTexture != null)
            {
                rect = working.modTexture.sourceRect;
            }
            PictureBoxRectTool.SetRect(modTextureCanvas, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(modTextureCanvas);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(modTextureCanvas);
            SetRectBox(rect);

            numericUpDown1.Value = (decimal)(working.mapping?.targetRect.Width ?? 0);
            numericUpDown2.Value = (decimal)(working.mapping?.targetRect.Height ?? 0);
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
            RenderDrawing();
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
        // ApplyDrawing();
        RenderDrawing();
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
        if (working.modTextureSource != null && (!modTextureCanvas.IsHandleCreated || !modTextureCanvas.Visible))
        {
            if (modTextureCanvas.GetImage() == null && working.modTextureSource.state == AsyncImageState.Ready)
            {
                var bitmap = working.modTextureSource.CreateBitmap(d2dRenderTarget.target);
                modTextureCanvas.SetImage(bitmap);
                CheckImageUnload();
            }
        }
        if (working.modTextureSource != null && working.modTextureBitmap == null)
        {
            working.modTextureBitmap = modTextureCanvas.GetImage();
            gameTextureCanvas.Invalidate();
            CheckImageUnload();
        }
        if (working.gameTextureSource != null && working.gameTextureBitmap == null)
        {
            working.gameTextureBitmap = gameTextureCanvas.GetImage();
            gameTextureCanvas.Invalidate();
            CheckImageUnload();
        }
    }

    private void CheckImageUnload()
    {
        if (working == null) return;
        if (modTextureCanvas.HasImage() && working.modTextureBitmap != null)
        {
            if (working.modTextureSource != null)
            {
                Utilities.Dispose(ref working.modTextureSource);
                imageCache.CreateCacheItem(working.modTexture.filename, working.modTextureBitmap);
            }
        }
        if (gameTextureCanvas.HasImage() && working.gameTextureBitmap != null)
        {
            if (working.gameTextureSource != null)
            {
                Utilities.Dispose(ref working.gameTextureSource);
                imageCache.CreateCacheItem(working.sourceTexture.sourceFile, working.gameTextureBitmap);
            }
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
            var textureMapping = listOfMappings[selectedIndex];
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

    private void button9_Click(object sender, EventArgs e)
    {
        if (working.modTexture != null)
        {
            lock (renderer.RenderSyncRoot)
            {
                var camera = renderer.camera;
                if (camera.FollowEntity != null)
                {
                    camera.FollowEntity.Position = working.modTexture.position;
                }
                else
                {
                }
                camera.Position = working.modTexture.position;
                camera.TargetRotation = working.modTexture.rotation;
                camera.CurrentRotation = working.modTexture.rotation;
            }
        }
    }

    private void button8_Click(object sender, EventArgs e)
    {
        if (working.modTexture != null)
        {
            lock (renderer.RenderSyncRoot)
            {
                var camera = renderer.camera;
                if (camera.FollowEntity != null)
                {
                    working.modTexture.position = camera.FollowEntity.Position;
                }
                else
                {
                    working.modTexture.position = camera.Position;
                }
                working.modTexture.rotation = camera.TargetRotation;
            }
        }
    }

    private void toolStripButton4_Click(object sender, EventArgs e)
    {
        var form = new AddTexModSourceForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            var gameFile = adapter.GetSourceFile(form.sourceFileName);
            if (gameFile != null)
            {
                AddModSource(gameFile, form.sourceTexName);
            }
        }
    }

    private void toolStripButton6_Click(object sender, EventArgs e)
    {
        if (openFileDialog1.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var fileName = openFileDialog1.FileName;
                if (string.IsNullOrEmpty(fileName)) return;
                if (working.modTexture != null)
                {
                    working.modTexture.filename = fileName;
                    working.modTexture.name = Path.GetFileName(fileName);
                }
                RefreshModListView();
            }
            catch (Exception exception)
            {
                exception.ShowDialog();
            }
        }
    }

    private void toolStripButton9_Click(object sender, EventArgs e)
    {
        if (working.modTexture != null)
        {
            // duplicate
            var modTexture = working.modTexture.Clone();
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
            RefreshModListView();
        }
    }
}