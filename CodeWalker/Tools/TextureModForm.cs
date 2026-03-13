using CodeWalker.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using WeifenLuo.WinFormsUI.Docking;
using Rectangle = SharpDX.Rectangle;

namespace CodeWalker.Tools;

public partial class TextureModForm : Form
{
    private AsyncPictureBox previewPictureBoxAsync;

    public TextureModForm()
    {
        InitializeComponent();
        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip1, version, theme);
        vsExtender.SetStyle(toolStrip2, version, theme);

        PictureBoxViewer.AddFeature(previewPictureBox);
        previewPictureBoxAsync = new AsyncPictureBox(previewPictureBox);

        InitializeListView();
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        // dxPreview1.InitDevice();
        RefreshModListView();
        RefreshReplacementListView();
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
        modListView.Invalidate();
    }

    private void RefreshReplacementListView()
    {
        replacementListView.VirtualListSize = replacements.Count;
        replacementListView.Invalidate();
    }

    private void modListView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var replacement = project.modTextures.Values[e.ItemIndex];
        e.Item = new ListViewItem(replacement.name);
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
                project.CreateTextureMod(fileName, out var modTexture, out _);
                modTexture.sourceRect = new Rectangle(0, 0, image.Width, image.Height);
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
        pictureBox.DisplayPicture(adapter, fileName);
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
            OnSelectTexMod(project.modTextures.Values[index]);
            break;
        }
    }
}