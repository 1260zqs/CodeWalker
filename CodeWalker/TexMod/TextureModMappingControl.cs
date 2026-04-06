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
using System.IO;
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
        this.mainForm = mainForm;

        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip, version, theme);
        vsExtender.SetStyle(contextMenuStrip1, version, theme);

        foreach (var value in Enum.GetValues(typeof(TextureLod)))
        {
            if ((TextureLod)value == TextureLod.Unknown)
            {
                continue;
            }
            var menuItem = new System.Windows.Forms.ToolStripMenuItem();
            menuItem.Text = Enum.GetName(typeof(TextureLod), value);
            menuItem.Tag = value;
            menuItem.Click += lodToolStripMenuItem_Click;
            lodToolStripMenuItem.DropDownItems.Add(menuItem);
        }

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

    public void RepaintListView()
    {
        textureMappingView.Invalidate();
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
        var mapping = listOfMappings[e.ItemIndex];
        e.Item = new ListViewItem(mapping.name);
        e.Item.SubItems.Add(mapping.lod.ToString());
        if (project.sourceTextures.TryGetValue(mapping.sourceTexture, out var sourceTexture))
        {
            var sourceFileName = string.Empty;
            var indexOf = sourceTexture.sourceFile.IndexOf(':');
            if (indexOf > 0)
            {
                var sourceFile = sourceTexture.sourceFile.Substring(0, indexOf);
                sourceFileName = Path.GetFileName(sourceFile);
            }
            e.Item.SubItems.Add(sourceFileName);
            e.Item.SubItems.Add(sourceTexture.sourceFile);
        }
        else
        {
            e.Item.SubItems.Add(string.Empty);
            e.Item.SubItems.Add(string.Empty);
        }
    }

    private void textureMappingView_SelectedIndexChanged(object sender, EventArgs e)
    {
        foreach (int index in textureMappingView.SelectedIndices)
        {
            mainForm.SelectTextureMapping(listOfMappings[index]);
            return;
        }
    }

    private void textureMappingView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
    {
    }

    private ListViewHitTestInfo lastHitTest;

    private void textureMappingView_MouseDown(object sender, MouseEventArgs e)
    {
        lastHitTest = textureMappingView.HitTest(e.Location);
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
        var isChecked = !toolStripButton1.Checked;
        toolStripButton1.Checked = isChecked;
        mainForm.isSyncLod = isChecked;
    }

    private void textureMappingView_DoubleClick(object sender, EventArgs e)
    {
    }

    private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
    {
        var showDelete = lastHitTest != null && lastHitTest.Item != null;
        deleteToolStripMenuItem.Visible = showDelete;
        toolStripSeparator1.Visible = showDelete;
        lodToolStripMenuItem.Visible = showDelete;
        copyValueToolStripMenuItem.ToolTipText = GetTextToCopy();
        copyValueToolStripMenuItem.Visible = lastHitTest != null && (lastHitTest.Item != null || lastHitTest.SubItem != null);
    }

    private void addToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var form = new AddTexModSourceForm();
        if (form.ShowDialog(this) == DialogResult.OK)
        {
            mainForm.AddModSource(
                form.sourceFileName,
                form.sourceTexName
            );
        }
    }

    private void copyValueToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is ToolStripMenuItem menuItem)
        {
            var text = menuItem.ToolTipText;
            if (string.IsNullOrEmpty(text)) return;
            Clipboard.SetText(text);
        }
    }

    private string GetTextToCopy()
    {
        if (lastHitTest == null) return null;
        if (lastHitTest.SubItem != null)
        {
            return lastHitTest.SubItem.Text;
        }
        if (lastHitTest.Item != null)
        {
            return lastHitTest.Item.Text;
        }
        return null;
    }

    private void lodToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (sender is System.Windows.Forms.ToolStripMenuItem menuItem)
        {
            if (menuItem.Tag is TextureLod lod)
            {
                mainForm.SetTextureMappingLod(lod);
            }
        }
    }

    private void deleteToolStripMenuItem1_Click(object sender, EventArgs e)
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

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
        var list = new List<TextureMapping>(listOfMappings);
        foreach (var mapping in list)
        {
            if (mapping.lod == TextureLod.HD)
            {
                mainForm.FindHiDRTexture(mapping);
            }
        }
    }
}