using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace CodeWalker.TexMod;

public partial class TextureModForm
{
    static TextureModForm instance;
    static TextureModProject workingProject;

    public static void ShowWindow(WorldForm worldForm)
    {
        instance = GetWindow(worldForm);
        if (instance == null) return;

        instance.Show();
        instance.Focus();
    }

    public static void ShowAddModSource(WorldForm worldForm, GameFile gameFile, string texName)
    {
        var window = GetWindow(worldForm);
        if (window == null) return;

        window.Show();
        window.Focus();
        window.Invoke(() =>
        {
            window.AddModSource(gameFile, texName);
        });
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
        var save = false;
        var packageManifestFile = string.Empty;
        var workingDir = Settings.Default.TexModWorkingDir;
        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
        {
            save = true;
            var setupForm = new TexModSetupForm();
            setupForm.ProjectWorkingDir = workingDir;
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            workingDir = setupForm.ProjectWorkingDir;
            packageManifestFile = setupForm.PackageManifestFile;
        }
        if (string.IsNullOrEmpty(workingDir) || !Directory.Exists(workingDir))
        {
            return null;
        }
        if (save)
        {
            Settings.Default.TexModWorkingDir = workingDir;
        }
        if (workingProject == null)
        {
            workingProject = TextureModProject.SetupWorkingProject(workingDir);
        }
        if (!string.IsNullOrEmpty(packageManifestFile) && File.Exists(packageManifestFile))
        {
            workingProject.manifestFile = packageManifestFile;
        }
        else if (string.IsNullOrEmpty(workingProject.manifestFile) || !File.Exists(workingProject.manifestFile))
        {
            var setupForm = new TexModSetupForm();
            setupForm.ProjectWorkingDir = workingDir;
            setupForm.PackageManifestFile = workingProject.manifestFile;
            if (setupForm.ShowDialog() != DialogResult.OK)
            {
                return null;
            }
            workingProject.manifestFile = setupForm.PackageManifestFile;
        }
        workingProject.LoadPackageManifest();
        var form = new TextureModForm();
        form.project = workingProject;
        form.worldForm = worldForm;
        form.adapter = new GTAVTextureModAdapter(workingProject, worldForm);
        return form;
    }

    public TextureModProject project;
    public TextureModAdapter adapter;
    public WorldForm worldForm;

    private ModTexture currentMod;
    private TextureReplacement currentReplacement;
    private List<TextureReplacement> replacements = new();
    private bool applyDrawing;

    private void SelectTexMod(ModTexture modTexture)
    {
        if (currentMod != null)
        {
            currentMod.editorState = PictureBoxViewer.SaveState(textureCanvas);
        }
        replacements.Clear();
        currentMod = modTexture;
        currentReplacement = null;
        textureCanvas.ClearImage();
        previewCanvas.ClearImage();
        PictureBoxViewer.ResetViewer(textureCanvas);
        PictureBoxViewer.ResetViewer(previewCanvas);
        if (currentMod != null)
        {
            textureCanvas.SetImage(currentMod.filename);
            PictureBoxViewer.LoadState(textureCanvas, currentMod.editorState);
        }
        ReadPanelDataFromSource();
        propertyGridFix1.SelectedObject = null;
        replacementListView.SelectedIndices.Clear();
        RefreshReplacementListView(true);
    }

    private void SelecteTexReplacement(TextureReplacement replacement)
    {
        propertyGridFix1.SelectedObject = null;
        if (project.sourceTextures.TryGetValue(replacement.sourceTexture, out var sourceTexture))
        {
            if (currentReplacement != null)
            {
                currentReplacement.editorState = PictureBoxViewer.SaveState(previewCanvas);
            }
            currentReplacement = replacement;
            ReadPanelDataFromSource();

            PictureBoxViewer.ResetViewer(previewCanvas);
            PictureBoxViewer.LoadState(previewCanvas, replacement.editorState);
            previewCanvas.SetImage(new AsyncGameTextureSource(adapter, sourceTexture.sourceFile));
            propertyGridFix1.SelectedObject = TextureReplacementPropertyObject.From(replacement);
        }
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
        // return;
        // var targetImage = pictureBox1Async.GetImage();
        // var sourceImage = previewPictureBoxAsync.GetImage();
        // if (sourceImage == null || targetImage == null) return;
        if (currentMod == null || currentReplacement == null) return;
        //
        // var image = (Image)targetImage;
        // var sourceTex = (Image)sourceImage;
        //
        // var sourceRect = currentMod.sourceRect;
         var targetRect = currentReplacement.targetRect;
        var sourceTexture = project.sourceTextures[currentReplacement.sourceTexture];
        var textureName = adapter.GetSourceTextureName(sourceTexture.sourceFile);
        var nameHash = JenkHash.GenHash(textureName.ToLowerInvariant());
        //
        // var width = sourceTex.Width;
        // var height = sourceTex.Height;
        // using var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        // using (var g = Graphics.FromImage(bitmap))
        // {
        //     g.Clear(Color.Transparent);
        //     g.DrawImageUnscaled(sourceTex, 0, 0);
        //     if (checkBox3.Checked)
        //     {
        //         g.FillRectangle(Brushes.Lime, targetRect.Convert());
        //     }
        //     else
        //     {
        //         g.DrawImage(image, targetRect.Convert(), sourceRect.Convert(), GraphicsUnit.Pixel);
        //     }
        // }
        //
        var sourceTex = previewCanvas.GetImage();
        if (sourceTex == null) return;
        // if (d2dRenderTarget == null)
        // {
        //     var d3dDevice = new SharpDX.Direct3D11.Device(
        //         SharpDX.Direct3D.DriverType.Hardware,
        //         SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport
        //     );
        //     var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
        //     var d2dFactory = new SharpDX.Direct2D1.Factory1();
        //     var d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice);
        //     d2dRenderTarget = new SharpDX.Direct2D1.DeviceContext(
        //         d2dDevice,
        //         SharpDX.Direct2D1.DeviceContextOptions.None
        //     );
        // }
        if (!Monitor.TryEnter(worldForm.Renderer.DXMan.syncroot, 50))
        {
            Thread.Sleep(10); //don't hog CPU when not able to render...
            return;
        }
        // var d3dDevice = worldForm.Renderer.Device;
        // var stagingTextureDesc = new SharpDX.Direct3D11.Texture2DDescription
        // {
        //     Width = sourceTex.PixelSize.Width,
        //     Height = sourceTex.PixelSize.Height,
        //     MipLevels = 1,
        //     ArraySize = 1,
        //     Format = Format.B8G8R8A8_UNorm,
        //     Usage = ResourceUsage.Default,
        //     BindFlags = BindFlags.RenderTarget | BindFlags.ShaderResource,
        //     CpuAccessFlags = CpuAccessFlags.None,
        //     SampleDescription = new SampleDescription(1, 0),
        //     OptionFlags = ResourceOptionFlags.SharedKeyedmutex
        // };
        //
        // using var d3dTexture = new SharpDX.Direct3D11.Texture2D(d3dDevice, stagingTextureDesc);
        // using var renderTargetView = new SharpDX.Direct3D11.RenderTargetView(d3dDevice, d3dTexture);
        //
        // var factory1 = new SharpDX.DXGI.Factory1();
        // var adapter1 = factory1.GetAdapter1(0);
        // var factory2D = new SharpDX.Direct2D1.Factory(FactoryType.SingleThreaded, DebugLevel.Information);
        //
        // var sharedResource = d3dTexture.QueryInterface<SharpDX.DXGI.Resource>();
        // var device10 = new Device10(adapter1, SharpDX.Direct3D10.DeviceCreationFlags.BgraSupport, FeatureLevel.Level_10_0);
        // var textureD3D10 = device10.OpenSharedResource<SharpDX.Direct3D10.Texture2D>(sharedResource.SharedHandle);
        //
        //
        // var renderTargetProperties = new RenderTargetProperties(
        //     new SharpDX.Direct2D1.PixelFormat(
        //         SharpDX.DXGI.Format.B8G8R8A8_UNorm,
        //         SharpDX.Direct2D1.AlphaMode.Premultiplied
        //     )
        // );
        // var surface = d3dTexture.QueryInterface<SharpDX.DXGI.Surface>();
        // using var d2dFactory = new SharpDX.Direct2D1.Factory();
        // using var d2dRenderTargetForTexture = new RenderTarget(
        //     d2dFactory,
        //     surface,
        //     renderTargetProperties
        // );
        //
        // d2dRenderTargetForTexture.BeginDraw();
        // d2dRenderTargetForTexture.Clear(new RawColor4(0, 0, 0, 0));
        //
        // //d2dRenderTarget.DrawBitmap(sourceTex, 1, BitmapInterpolationMode.NearestNeighbor);
        // using var solidBrush = new SolidColorBrush(d2dRenderTargetForTexture, new RawColor4(1f, 0, 0, 1f));
        // d2dRenderTargetForTexture.FillRectangle(new RawRectangleF(0, 0, 200, 200), solidBrush);
        //
        // d2dRenderTargetForTexture.EndDraw();
        var bmpProps = new BitmapProperties1(
            new SharpDX.Direct2D1.PixelFormat(
                SharpDX.DXGI.Format.B8G8R8A8_UNorm,
                SharpDX.Direct2D1.AlphaMode.Premultiplied
            ),
            96, 96,
            BitmapOptions.Target | BitmapOptions.CannotDraw
        );
        using var bitmap = new SharpDX.Direct2D1.Bitmap1(
            d2dRenderTarget,
            sourceTex.PixelSize,
            bmpProps
        );

        d2dRenderTarget.Target = bitmap;
        d2dRenderTarget.BeginDraw();
        d2dRenderTarget.Clear(new RawColor4(0f, 0, 0, 0));

        //d2dRenderTarget.DrawBitmap(sourceTex, 1, BitmapInterpolationMode.NearestNeighbor);
        using var solidBrush = new SolidColorBrush(d2dRenderTarget, new RawColor4(0f, 1f, 0, 1f));
        d2dRenderTarget.FillRectangle(new RawRectangleF(targetRect.X, targetRect.Top, targetRect.Right, targetRect.Bottom), solidBrush);

        d2dRenderTarget.EndDraw();
        d2dRenderTarget.Target = null;

        var tex = worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
            x.Key.NameHash == nameHash);
        if (tex != null)
        {
            var bitmapProperties = new BitmapProperties1
            {
                BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.CpuRead,
                PixelFormat = bitmap.PixelFormat
            };
            var deviceContext2d = d2dRenderTarget.QueryInterface<SharpDX.Direct2D1.DeviceContext>();
            using var origin = new Bitmap1(deviceContext2d, sourceTex.PixelSize, bitmapProperties);
            origin.CopyFromBitmap(bitmap);

            tex.Load(worldForm.Renderer.Device, origin);
        }
        Monitor.Exit(worldForm.Renderer.DXMan.syncroot);
    }

    public class TextureReplacementPropertyObject
    {
        [ReadOnly(true)]
        public string id { get; set; }

        public string tag { get; set; }
        public string name { get; set; }
        public string comment { get; set; }

        [ReadOnly(true)]
        public string modTexture { get; set; }

        [ReadOnly(true)]
        public string sourceTexture { get; set; }

        public string targetRect { get; set; }

        public bool flipX { get; set; }
        public bool flipY { get; set; }
        public float rotation { get; set; }

        public static TextureReplacementPropertyObject From(TextureReplacement replacement)
        {
            var propertyObject = new TextureReplacementPropertyObject();
            propertyObject.id = replacement.id.ToString("N");
            propertyObject.tag = replacement.tag;
            propertyObject.name = replacement.name;
            propertyObject.comment = replacement.comment;

            propertyObject.flipX = replacement.flipX;
            propertyObject.flipX = replacement.flipX;
            propertyObject.rotation = replacement.rotation;

            return propertyObject;
        }
    }
}