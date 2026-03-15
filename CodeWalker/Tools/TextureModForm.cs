using CodeWalker.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Tools;

public partial class TextureModForm : Form
{
    static TextureModForm instance;

    public static void ShowWindow(WorldForm worldForm)
    {
        instance = GetWindow(worldForm);
        instance.Show();
        instance.Focus();
    }

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

        PictureBoxViewer.AddFeature(previewPictureBox);
        PictureBoxRectTool.AddFeature(previewPictureBox, OnRectDrawingChange);

        PictureBoxViewer.AddFeature(pictureBox1);
        PictureBoxRectTool.AddFeature(pictureBox1, OnRectDrawingChange);
        pictureBox1Async = new AsyncPictureBox(pictureBox1);
        previewPictureBoxAsync = new AsyncPictureBox(previewPictureBox);
        previewPictureBoxAsync.onPaint = PaintPreviewPicture;
        UpdateGroupBoxVisibility();

        InitializeListView();
    }

    private void PaintPreviewPicture(Graphics g, Image image)
    {
        g.DrawImage(image, 0, 0);
        if (currentMod != null && currentReplacement != null)
        {
            var tex = pictureBox1Async.GetImage();
            if (tex == null)
            {
                previewPictureBox.Invalidate();
                return;
            }
            var srcRect = currentMod.sourceRect.Convert();
            var destRect = currentReplacement.targetRect.Convert();
            var imgBounds = new Rectangle(0, 0, image.Width, image.Height);
            var clippedDest = Rectangle.Intersect(destRect, imgBounds);

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

            g.DrawImage(tex, clippedDest, srcRect, GraphicsUnit.Pixel);
        }
    }

    private void OnRectDrawingChange(System.Drawing.Rectangle obj)
    {
        WritePanelDataToSource();
    }

    private AsyncPictureBox pictureBox1Async;
    private AsyncPictureBox previewPictureBoxAsync;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // dxPreview1.InitDevice();
        RefreshModListView();
        RefreshReplacementListView();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        EditorApplication.update += RenderUpdate;
        base.OnHandleCreated(e);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        EditorApplication.update -= RenderUpdate;
        base.OnHandleDestroyed(e);
    }

    private void InitializeListView()
    {
        modListView.Columns.Add("project");
        modListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        replacementListView.Columns.Add("reference");
        replacementListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

        repViewModeBtn.SetEnumDrop<View>(x => modListView.View = x);
        repViewModeBtn.SelectEnum(modListView.View);

        toolStripButton7.SetEnumDrop<View>(x => replacementListView.View = x);
        toolStripButton7.SelectEnum(replacementListView.View);
    }

    private void RefreshModListView()
    {
        modListView.VirtualListSize = project.modTextures.Count;
        if (modListView.VirtualListSize > 0 && modListView.SelectedIndices.Count == 0)
        {
            modListView.SelectedIndices.Add(0);
        }
        modListView.Invalidate();
    }

    private void RefreshReplacementListView(bool reload = false)
    {
        if (reload && currentMod != null)
        {
            project.FindTextureReplacements(currentMod.id, replacements);
        }
        replacementListView.VirtualListSize = replacements.Count;
        if (replacementListView.VirtualListSize > 0 && replacementListView.SelectedIndices.Count == 0)
        {
            replacementListView.SelectedIndices.Add(0);
        }
        replacementListView.Invalidate();
    }

    private void replacementListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var replacement = project.replacements[e.ItemIndex];
        e.Item = new ListViewItem(replacement.name);
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

    private void DisplayPicture(AsyncPictureBox pictureBox, string fileName)
    {
        var source = new ImageFilePictureSource(fileName);
        source.loader = adapter;
        DisplayPicture(pictureBox, source);
    }

    private void DisplayPicture(AsyncPictureBox pictureBox, AsyncPictureSource source)
    {
        if (pictureBox.pictureSource is { } x && x.Equals(source)) return;
        pictureBox.DisplayPicture(source);
        source.Load();
    }

    private void modListView_AfterLabelEdit(object sender, LabelEditEventArgs e)
    {
        if (string.IsNullOrEmpty(e.Label)) return;
        var replacement = project.modTextures.Values[e.Item];
        replacement.name = e.Label;
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
        // foreach (ListViewItem selectedItem in replacementListView.SelectedIndices)
        // {
        //     selectedItem.BeginEdit();
        //     break;
        // }
    }

    private void modListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in modListView.SelectedIndices)
        {
            if (currentMod != null)
            {
                currentMod.editorState = PictureBoxViewer.SaveState(pictureBox1);
            }
            OnSelectTexMod(project.modTextures.Values[index]);
            break;
        }
    }

    private void replacementListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in replacementListView.SelectedIndices)
        {
            var replacement = replacements[index];
            if (project.sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
            {
                if (currentReplacement != null)
                {
                    currentReplacement.editorState = PictureBoxViewer.SaveState(previewPictureBox);
                }
                OnReplacementSelected(replacement);
                var pictureSource = new GamePackPictureSource(sourceTexture.sourceFile);
                pictureSource.adapter = adapter;
                DisplayPicture(previewPictureBoxAsync, pictureSource);
                PictureBoxViewer.ResetViewer(previewPictureBox);
                PictureBoxViewer.LoadState(previewPictureBox, replacement.editorState);
            }
            return;
        }
    }

    private void saveProjectBtn_Click(object sender, EventArgs e)
    {
        project.Save("x:\\mod.xml");
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
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.GetRect(previewPictureBox, out var rect);
            rect.X = (int)rectBoxX.Value;
            PictureBoxRectTool.SetRect(previewPictureBox, rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.GetRect(pictureBox1, out var rect);
            rect.X = (int)rectBoxX.Value;
            PictureBoxRectTool.SetRect(pictureBox1, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxY_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.GetRect(previewPictureBox, out var rect);
            rect.Y = (int)rectBoxY.Value;
            PictureBoxRectTool.SetRect(previewPictureBox, rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.GetRect(pictureBox1, out var rect);
            rect.Y = (int)rectBoxY.Value;
            PictureBoxRectTool.SetRect(pictureBox1, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxW_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.GetRect(previewPictureBox, out var rect);
            rect.Width = (int)rectBoxW.Value;
            PictureBoxRectTool.SetRect(previewPictureBox, rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.GetRect(pictureBox1, out var rect);
            rect.Width = (int)rectBoxW.Value;
            PictureBoxRectTool.SetRect(pictureBox1, rect);
        }
        WritePanelDataToSource();
    }

    private void rectBoxH_ValueChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.GetRect(previewPictureBox, out var rect);
            rect.Height = (int)rectBoxH.Value;
            PictureBoxRectTool.SetRect(previewPictureBox, rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.GetRect(pictureBox1, out var rect);
            rect.Height = (int)rectBoxH.Value;
            PictureBoxRectTool.SetRect(pictureBox1, rect);
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
        groupBox1.Visible = tabPage == textureTabPage;
        groupBox2.Visible = tabPage == previewTabPage;
    }

    private void WritePanelDataToSource()
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.GetRect(previewPictureBox, out var rect);
            if (currentReplacement != null)
            {
                currentReplacement.targetRect = rect.Convert();
            }
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.GetRect(pictureBox1, out var rect);
            if (currentMod != null)
            {
                currentMod.sourceRect = rect.Convert();
            }
            SetRectBox(rect);
        }
        ApplyDrawing();
    }

    private void ReadPanelDataFromSource()
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            var rect = new System.Drawing.Rectangle();
            if (currentReplacement != null)
            {
                rect = currentReplacement.targetRect.Convert();
            }
            PictureBoxRectTool.SetRect(previewPictureBox, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(previewPictureBox);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(previewPictureBox);
            SetRectBox(rect);
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            var rect = new System.Drawing.Rectangle();
            if (currentMod != null)
            {
                rect = currentMod.sourceRect.Convert();
            }
            PictureBoxRectTool.SetRect(pictureBox1, rect);
            checkBox1.Checked = PictureBoxRectTool.GetPaintEnable(pictureBox1);
            checkBox2.Checked = PictureBoxRectTool.GetSolid(pictureBox1);
            SetRectBox(rect);

            numericUpDown1.Value = currentReplacement?.targetRect.Width ?? 0;
            numericUpDown2.Value = currentReplacement?.targetRect.Height ?? 0;
        }
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.SetSolid(previewPictureBox, checkBox2.Checked);
            previewPictureBox.Invalidate();
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.SetSolid(pictureBox1, checkBox2.Checked);
            pictureBox1.Invalidate();
        }
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        if (imageTabControl.SelectedTab == previewTabPage)
        {
            PictureBoxRectTool.SetPaintEnable(previewPictureBox, checkBox1.Checked);
            previewPictureBox.Invalidate();
        }
        else if (imageTabControl.SelectedTab == textureTabPage)
        {
            PictureBoxRectTool.SetPaintEnable(pictureBox1, checkBox1.Checked);
            pictureBox1.Invalidate();
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
        var tex = pictureBox1Async.GetImage();
        if (tex == null) return;

        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (width <= 0) return;

        rectBoxX.Value = 0;
        rectBoxW.Value = tex.Width;
        rectBoxH.Value = height / width * tex.Width;
    }

    private void button4_Click(object sender, EventArgs e)
    {
        var tex = pictureBox1Async.GetImage();
        if (tex == null) return;

        var width = numericUpDown1.Value;
        var height = numericUpDown2.Value;
        if (height <= 0) return;

        rectBoxY.Value = 0;
        rectBoxH.Value = tex.Height;
        rectBoxW.Value = width / height * tex.Width;
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
    }
}