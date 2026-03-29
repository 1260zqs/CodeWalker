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
using CodeWalker.Properties;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModDockForm : Form
{
    TextureModExplorerControl explorer;
    TextureModImageControl image1;
    TextureModImageControl image2;
    TextureModMappingControl mapping;
    TextureModPropertyControl property;
    TextureModInspectorControl inspector;

    public TextureModDockForm()
    {
        InitializeComponent();
        byte[] layout = null;
        if (string.IsNullOrEmpty(Settings.Default.texModFormLayout))
        {
            layout = Resources.texModFormLayout;
        }
        else
        {
            layout = Convert.FromBase64String(Settings.Default.texModFormLayout);
        }
        using var stream = new MemoryStream(layout);
        dockPanel.LoadFromXml(stream, DeserializeDockContent);
    }

    private IDockContent DeserializeDockContent(string persistString)
    {
        if (persistString == "CodeWalker.TexMod.TextureModExplorerControl")
        {
            var dockContent = new TextureModExplorerControl();
            dockContent.Text = "Explorer";
            return dockContent;
        }
        if (persistString == "CodeWalker.TexMod.TextureModImageControl:Preview")
        {
            var dockContent = new TextureModImageControl();
            dockContent.Text = "Preview";
            dockContent.DockHandler.GetPersistStringCallback = () => "CodeWalker.TexMod.TextureModImageControl:Preview";
            return dockContent;
        }
        if (persistString == "CodeWalker.TexMod.TextureModImageControl:Texture")
        {
            var dockContent = new TextureModImageControl();
            dockContent.Text = "Texture";
            dockContent.DockHandler.GetPersistStringCallback = () => "CodeWalker.TexMod.TextureModImageControl:Texture";
            return dockContent;
        }
        if (persistString == "CodeWalker.TexMod.TextureModMappingControl")
        {
            var dockContent = new TextureModMappingControl();
            dockContent.Text = "Mapping";
            return dockContent;
        }
        if (persistString == "CodeWalker.TexMod.TextureModPropertyControl")
        {
            var dockContent = new TextureModPropertyControl();
            dockContent.Text = "Property";
            return dockContent;
        }
        if (persistString == "CodeWalker.TexMod.TextureModInspectorControl")
        {
            var dockContent = new TextureModInspectorControl();
            dockContent.Text = "Inspector";
            return dockContent;
        }
        return null;
    }

    private void xxxxToolStripMenuItem_Click(object sender, EventArgs e)
    {
#if DEBUG

#endif
        var stream = new MemoryStream();
        dockPanel.SaveAsXml(stream, Encoding.UTF8);
        stream.Position = 0;
        Settings.Default.texModFormLayout = Convert.ToBase64String(stream.ToArray());
    }
}