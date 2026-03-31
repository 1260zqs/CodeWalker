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
        public uint texNameHash;

        public AsyncGameTextureSource gameTextureSource;
        public SharpDX.Direct2D1.Bitmap gameTextureBitmap;

        public AsyncImageFileSource modTextureSource;
        public SharpDX.Direct2D1.Bitmap modTextureBitmap;
    }

    public TextureModDockForm()
    {
        InitializeComponent();
        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        dockPanel.Theme = theme;
        vsExtender.SetStyle(menuStrip1, version, theme);
    }

    TextureModExplorerControl explorerControl;
    TextureModImageControl modTextureCanvas;
    TextureModImageControl gameTextureCanvas;
    TextureModMappingControl mappingControl;
    TextureModPropertyControl propertyControl;
    TextureModToolsControl toolsControl;

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
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
        dockPanel.LoadFromXml(stream, CreateDockContent);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        var d3dDevice = DXGraphic.GetDevice();
        var d2dFactory = DXGraphic.d2dFactory;
        d2dRenderTarget = new D2DRenderTarget(d3dDevice, d2dFactory);
        d2dRenderTarget.SetTargetSize(Guid.Empty, new Size2(1, 1));
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        Utilities.Dispose(ref d2dRenderTarget);
        Utilities.Dispose(ref imageCache);
        base.OnHandleDestroyed(e);
    }

    private void OnPropertyGridChanged()
    {
        gameTextureCanvas?.Repaint();
        RequestTexturePaintingUpdate();
    }

    private void dockPanel_ContentAdded(object sender, DockContentEventArgs e)
    {
        var content = e.Content;
        if (content is TextureModExplorerControl explorer)
        {
            explorerControl = explorer;
        }
        else if (content is TextureModImageControl imageControl)
        {
            if (imageControl.Text == "Texture")
            {
                modTextureCanvas = imageControl;
            }
            else if (imageControl.Text == "Preview")
            {
                gameTextureCanvas = imageControl;
            }
        }
        else if (content is TextureModMappingControl mapping)
        {
            mappingControl = mapping;
        }
        else if (content is TextureModPropertyControl property)
        {
            propertyControl = property;
            propertyControl.onPropertyGridChanged = OnPropertyGridChanged;
        }
        else if (content is TextureModToolsControl inspector)
        {
            toolsControl = inspector;
        }
    }

    private void dockPanel_ContentRemoved(object sender, DockContentEventArgs e)
    {
    }

    private void OnDockContentClosed(object sender, EventArgs e)
    {
        OnContentRemoved(sender as IDockContent);
    }

    public void OnContentRemoved(IDockContent content)
    {
        if (content is TextureModExplorerControl)
        {
            if (ReferenceEquals(explorerControl, content))
            {
                project.directory = explorerControl.SerializeTreeView();
                explorerControl = null;
            }
        }
        else if (content is TextureModImageControl imageControl)
        {
            if (imageControl.Name == "Texture")
            {
                if (ReferenceEquals(modTextureCanvas, imageControl))
                {
                    modTextureCanvas = null;
                    var bitmap = imageControl.GetImage();
                    if (bitmap != null && ReferenceEquals(working.modTextureBitmap, bitmap))
                    {
                        if (!ReferenceEquals(bitmap.Tag, d2dRenderTarget))
                        {
                            // transfer the bitmap ownership
                            working.modTextureBitmap = new SharpDX.Direct2D1.Bitmap(
                                d2dRenderTarget.target,
                                bitmap
                            );
                            Utilities.Dispose(ref bitmap);
                        }
                    }
                }
            }
            else if (imageControl.Name == "Preview")
            {
                if (ReferenceEquals(gameTextureCanvas, imageControl))
                {
                    gameTextureCanvas = null;
                    var bitmap = imageControl.GetImage();
                    if (bitmap != null && ReferenceEquals(working.gameTextureBitmap, bitmap))
                    {
                        if (!ReferenceEquals(bitmap.Tag, d2dRenderTarget))
                        {
                            // transfer the bitmap ownership
                            working.gameTextureBitmap = new SharpDX.Direct2D1.Bitmap(
                                d2dRenderTarget.target,
                                bitmap
                            );
                            Utilities.Dispose(ref bitmap);
                        }
                    }
                }
            }
        }
        else if (content is TextureModMappingControl)
        {
            if (ReferenceEquals(mappingControl, content))
            {
                mappingControl = null;
            }
        }
        else if (content is TextureModPropertyControl)
        {
            if (ReferenceEquals(propertyControl, content))
            {
                propertyControl = null;
            }
        }
        else if (content is TextureModToolsControl)
        {
            if (ReferenceEquals(toolsControl, content))
            {
                toolsControl = null;
            }
        }
    }

    //@formatter:off
    private static readonly string kPersistExplorer = typeof(TextureModExplorerControl).ToString();
    private static readonly string kPersistPreview = $"{typeof(TextureModImageControl)}:Preview";
    private static readonly string kPersistTexture = $"{typeof(TextureModImageControl)}:Texture";
    private static readonly string kPersistMapping = typeof(TextureModMappingControl).ToString();
    private static readonly string kPersistProperty = typeof(TextureModPropertyControl).ToString();
    private static readonly string kPersistInspector = typeof(TextureModToolsControl).ToString();
    //@formatter:on

    private IDockContent CreateDockContent(string persistString)
    {
        DockContent content = null;
        if (persistString == kPersistExplorer)
        {
            var dockContent = new TextureModExplorerControl();
            dockContent.Text = "Explorer";
            dockContent.mainForm = this;
            content = dockContent;
        }
        else if (persistString == kPersistPreview)
        {
            var dockContent = new TextureModImageControl();
            dockContent.Text = "Preview";
            dockContent.Name = "Preview";
            dockContent.DockHandler.GetPersistStringCallback = () => kPersistPreview;
            dockContent.canvas.onPaint = PaintPreviewPicture;
            dockContent.canvas.onBitmapLoaded = OnBitmapLoaded;
            dockContent.onRectDrawingChange = OnDestRectDrawingChange;
            dockContent.SetImage(working.gameTextureBitmap);
            PictureBoxRectTool.AddFeature(dockContent.canvas, OnDrawPreviewRect);
            content = dockContent;
        }
        else if (persistString == kPersistTexture)
        {
            var dockContent = new TextureModImageControl();
            dockContent.Text = "Texture";
            dockContent.Name = "Texture";
            dockContent.DockHandler.GetPersistStringCallback = () => kPersistTexture;
            dockContent.canvas.onPaint = PaintTexturePicture;
            dockContent.canvas.onBitmapLoaded = OnBitmapLoaded;
            dockContent.onRectDrawingChange = OnSrcRectDrawingChange;
            dockContent.SetImage(working.modTextureBitmap);
            PictureBoxRectTool.AddFeature(dockContent.canvas, OnDrawTextureRect);
            content = dockContent;
        }
        else if (persistString == kPersistMapping)
        {
            var dockContent = new TextureModMappingControl();
            dockContent.Text = "Mapping";
            dockContent.mainForm = this;
            content = dockContent;
        }
        else if (persistString == kPersistProperty)
        {
            var dockContent = new TextureModPropertyControl();
            dockContent.Text = "Property";
            dockContent.mainForm = this;
            content = dockContent;
        }
        else if (persistString == kPersistInspector)
        {
            var dockContent = new TextureModToolsControl();
            dockContent.Text = "Tools";
            dockContent.mainForm = this;
            content = dockContent;
        }
        if (content != null)
        {
            content.Closed += OnDockContentClosed;
        }
        return content;
    }

    private void OnDrawTextureRect(System.Drawing.RectangleF rectangle)
    {
        if (working.modTexture != null)
        {
            working.modTexture.sourceRect = rectangle;
            gameTextureCanvas?.Repaint();
            modTextureCanvas?.Refresh();
            RequestTexturePaintingUpdate();
        }
    }

    private void OnDrawPreviewRect(System.Drawing.RectangleF rectangle)
    {
        if (working.mapping != null)
        {
            working.mapping.targetRect = rectangle;
            gameTextureCanvas?.Repaint();
            RequestTexturePaintingUpdate();
        }
    }

    private void OnSrcRectDrawingChange(D2DCanvas canvas, System.Drawing.RectangleF rect)
    {
        if (working.modTexture != null)
        {
            working.modTexture.sourceRect = rect;
            gameTextureCanvas?.Repaint();
            RequestTexturePaintingUpdate();
        }
    }

    private void OnDestRectDrawingChange(D2DCanvas canvas, System.Drawing.RectangleF rect)
    {
        if (working.mapping != null)
        {
            working.mapping.targetRect = rect;
            RequestTexturePaintingUpdate();
        }
    }

    private void OnBitmapLoaded(D2DCanvas canvas)
    {
        CheckImageUnload();
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
            var overlay = working.modTextureBitmap;
            if (!isPainting) overlay = null;
            DrawPreviewOverlay(
                target,
                canvas.GetImage(),
                overlay,
                canvas.GetImageSize(),
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
                        var textLayout = DXGraphic.CreateTextLayout(text, DXGraphic.fontPfArmaFive_6);

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

    private void saveLayoutToolStripMenuItem_Click(object sender, EventArgs e)
    {
        var stream = new MemoryStream();
        dockPanel.SaveAsXml(stream, Encoding.UTF8);
        stream.Position = 0;
        Settings.Default.texModFormLayout = Convert.ToBase64String(stream.ToArray());
#if DEBUG
        // Console.WriteLine($"{Width}x{Height}");
        // File.WriteAllBytes("c:\\layout.xml", stream.ToArray());
#endif
    }

    private void loadLayoutToolStripMenuItem_Click(object sender, EventArgs e)
    {
    }

    private void timer1_Tick(object sender, EventArgs e)
    {
        ImageCache_Update();
        CheckImageUnload();
    }

    private void CheckImageUnload()
    {
        if (working.modTextureSource != null && working.modTextureSource.ready)
        {
            if (modTextureCanvas != null)
            {
                working.modTextureBitmap = modTextureCanvas.GetImage();
            }
            else
            {
                working.modTextureBitmap = working.modTextureSource.CreateBitmap(d2dRenderTarget.target);
                if (working.modTextureBitmap != null)
                {
                    working.modTextureBitmap.Tag = d2dRenderTarget;
                }
            }
            if (working.modTextureBitmap != null)
            {
                Utilities.Dispose(ref working.modTextureSource);
                imageCache.AddToCache(working.modTexture.filename, working.modTextureBitmap);
                OnModTextureReady(working.modTextureBitmap);
                gameTextureCanvas?.Repaint();
            }
        }
        if (working.gameTextureSource != null && working.gameTextureSource.ready)
        {
            if (gameTextureCanvas != null)
            {
                working.gameTextureBitmap = gameTextureCanvas.GetImage();
            }
            else
            {
                working.gameTextureBitmap = working.gameTextureSource.CreateBitmap(d2dRenderTarget.target);
                if (working.gameTextureBitmap != null)
                {
                    working.gameTextureBitmap.Tag = d2dRenderTarget;
                }
            }
            if (working.gameTextureBitmap != null)
            {
                Utilities.Dispose(ref working.gameTextureSource);
                imageCache.AddToCache(working.sourceTexture.sourceFile, working.gameTextureBitmap);
                OnGameTextureReady(working.gameTextureBitmap);
                gameTextureCanvas?.Repaint();
            }
        }
    }

    private void packToolStripMenuItem_Click(object sender, EventArgs e)
    {
        BeginBuildTexMod();
    }

    private void buildOIVPackageToolStripMenuItem_Click(object sender, EventArgs e)
    {
        BeginBuildOIVPackage();
    }

    private void newToolStripMenuItem_Click(object sender, EventArgs e)
    {
        NewTexMod();
    }

    private void saveToolStripMenuItem_Click(object sender, EventArgs e)
    {
        SaveProject();
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        //Close();
    }

    private void importToolStripMenuItem_Click(object sender, EventArgs e)
    {
        explorerControl?.Action_ReimportTex();
    }

    private void duplicateToolStripMenuItem_Click(object sender, EventArgs e)
    {
        explorerControl?.Action_Duplicate();
    }

    private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
    {
        explorerControl?.Action_Delete();
    }

    private void exporerToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (explorerControl == null)
        {
            explorerControl = (TextureModExplorerControl)CreateDockContent(kPersistExplorer);
            explorerControl.Show(dockPanel, DockState.Float);
        }
        explorerControl.Focus();
    }

    private void propertyToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (propertyControl == null)
        {
            propertyControl = (TextureModPropertyControl)CreateDockContent(kPersistProperty);
            propertyControl.Show(dockPanel, DockState.Float);
        }
        propertyControl.Focus();
    }

    private void mappingWindowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (mappingControl == null)
        {
            mappingControl = (TextureModMappingControl)CreateDockContent(kPersistMapping);
            mappingControl.Show(dockPanel, DockState.Float);
        }
        mappingControl.Focus();
    }

    private void textureWindowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (modTextureCanvas == null)
        {
            modTextureCanvas = (TextureModImageControl)CreateDockContent(kPersistTexture);
            modTextureCanvas.Show(dockPanel, DockState.Float);
        }
        modTextureCanvas.Focus();
    }

    private void previewWindowToolStripMenuItem_Click(object sender, EventArgs e)
    {
        if (gameTextureCanvas == null)
        {
            gameTextureCanvas = (TextureModImageControl)CreateDockContent(kPersistPreview);
            gameTextureCanvas.Show(dockPanel, DockState.Float);
        }
        gameTextureCanvas.Focus();
    }
}