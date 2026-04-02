using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModMappingControl : DockContent
{
    public TextureModMappingControl(TextureModDockForm mainForm)
    {
        InitializeComponent();
        this.Text = "Mapping";
        this.mainForm = mainForm;

        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip, version, theme);

        toolStripButton7.SetEnumDrop<View>(x => textureMappingView.View = x);
        toolStripButton7.SelectEnum(textureMappingView.View);

        this.toolStripButton1.Checked = mainForm.isSyncLod;
        this.toolStripButton1.Click += this.toolStripButton1_Click;
    }

    TextureModDockForm mainForm;
    TextureModProject project => mainForm.project;
    List<TextureMapping> listOfMappings = new();

    public void Clear()
    {
        listOfMappings.Clear();
        textureMappingView.VirtualListSize = 0;
        textureMappingView.SelectedIndices.Clear();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);
    }

    public void RefreshListView(ModTexture modTexture)
    {
        if (modTexture == null)
        {
            Clear();
            return;
        }
        project.FindTextureMapping(modTexture.id, listOfMappings);
        listOfMappings.Sort((x, y) => x.lod - y.lod);
        textureMappingView.VirtualListSize = listOfMappings.Count;
        if (listOfMappings.Count == 0)
        {
            textureMappingView.SelectedIndices.Clear();
        }
        else if (textureMappingView.SelectedIndices.Count == 0)
        {
            textureMappingView.SelectedIndices.Add(0);
        }
        else
        {
            foreach (int index in textureMappingView.SelectedIndices)
            {
                mainForm.SelectTextureMapping(listOfMappings[index]);
                return;
            }
        }
        textureMappingView.Refresh();
    }

    private void textureMappingView_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        var replacement = listOfMappings[e.ItemIndex];
        e.Item = new ListViewItem(replacement.name);
        e.Item.SubItems.Add(new ListViewItem.ListViewSubItem(e.Item, replacement.lod.ToString()));
    }

    private void textureMappingView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in textureMappingView.SelectedIndices)
        {
            mainForm.SelectTextureMapping(listOfMappings[index]);
            return;
        }
    }

    private void toolStripButton4_Click(object sender, EventArgs e)
    {
        var form = new AddTexModSourceForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            //var gameFile = adapter.GetSourceFile(form.sourceFileName);
            //if (gameFile != null)
            //{
            //    var info = new AddModSourceInfo();
            //    info.texName = form.sourceTexName;
            //    info.gameFile = gameFile;
            //    info.lod = (rage__eLodType)(-1);
            //    AddModSource(info);
            //}
        }
    }

    private void toolStripButton5_Click(object sender, EventArgs e)
    {
        // delete mapping
        foreach (int selectedIndex in textureMappingView.SelectedIndices)
        {
            var textureMapping = listOfMappings[selectedIndex];
            if (MessageBox.Show($"Delete {textureMapping.name}?", "Delete", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            mainForm.DeleteTexMapping(textureMapping);
            return;
        }
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
        mainForm.isSyncLod = toolStripButton1.Checked;
    }
}