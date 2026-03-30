using CodeWalker.GameFiles;
using CodeWalker.Graphic;
using CodeWalker.Properties;
using CodeWalker.Rendering;
using SharpDX;
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
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModDockForm : Form
{
    class WorkingState
    {
        public ModTexture modTexture;
        public TextureMapping mapping;
        public SourceTexture sourceTexture;

        public AsyncGameTextureSource gameTextureSource;
        public SharpDX.Direct2D1.Bitmap gameTextureBitmap;

        public AsyncImageFileSource modTextureSource;
        public SharpDX.Direct2D1.Bitmap modTextureBitmap;
    }

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

    TextureModExplorerControl explorer;
    TextureModImageControl image1;
    TextureModImageControl image2;
    TextureModMappingControl mapping;
    TextureModPropertyControl property;
    TextureModInspectorControl inspector;

    public TextureModProject project;
    private TextureModAdapter adapter;
    private RpfManager rpfManager;
    private Renderer renderer;

    private D2DRenderTarget d2dRenderTarget;
    private WorkingState working = new();

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        var d3dDevice = DXGraphic.GetDevice();
        var d2dFactory = DXGraphic.d2dFactory;
        d2dRenderTarget = new D2DRenderTarget(d3dDevice, d2dFactory);
        d2dRenderTarget.SetTargetSize(Guid.Empty, new Size2(1, 1));
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