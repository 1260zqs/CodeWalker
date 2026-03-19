using CodeWalker.GameFiles;
using CodeWalker.Properties;
using CodeWalker.TexMod;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.Direct3D;
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
    private D2DRenderTarget d2dRenderTarget;

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

        var sourceTex = previewCanvas.GetImage();
        if (sourceTex == null) return;

        if (!Monitor.TryEnter(worldForm.Renderer.DXMan.syncroot, 50))
        {
            return;
        }
        d2dRenderTarget.BeginDraw();

        //d2dRenderTarget.DrawBitmap(sourceTex, 1, BitmapInterpolationMode.NearestNeighbor);
        //d2dRenderTarget.FillRectangle(new RawRectangleF(targetRect.X, targetRect.Top, targetRect.Right, targetRect.Bottom));

        d2dRenderTarget.EndDraw();

        var tex = worldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
            x.Key.NameHash == nameHash);
        if (tex != null && tex.Texture2D != null)
        {
            d2dRenderTarget.CopyTo(worldForm.Renderer.Device, tex.Texture2D);
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

public class D3DRenderTarget : IDisposable
{
    static D3DRenderTarget()
    {

    }

    static SharpDX.Direct3D11.Device gDevice;
    static SharpDX.Direct3D11.VertexShader vertexShader;
    static SharpDX.Direct3D11.PixelShader pixelShader;
    static SharpDX.Direct3D11.VertexBufferBinding vertexBuffer;
    static SharpDX.Direct3D11.SamplerState sampler;
    static SharpDX.Direct3D11.InputLayout layout;

    public static void Init(SharpDX.Direct3D11.Device device)
    {
        var shaderCode = new byte[10];
        var vsByteCode = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderCode, "VSMain", "vs_5_0");
        var psByteCode = SharpDX.D3DCompiler.ShaderBytecode.Compile(shaderCode, "PSMain", "ps_5_0");

        vertexShader = new SharpDX.Direct3D11.VertexShader(device, vsByteCode);
        pixelShader = new SharpDX.Direct3D11.PixelShader(device, psByteCode);

        layout = new SharpDX.Direct3D11.InputLayout(device, vsByteCode, new[]
        {
            new SharpDX.Direct3D11.InputElement("POSITION", 0, Format.R32G32_Float, 0, 0),
            new SharpDX.Direct3D11.InputElement("TEXCOORD", 0, Format.R32G32_Float, 8, 0),
        });

        var vertices = new[]
        {
            // pos      uv
            -1f, -1f,   0f, 1f,
            -1f,  1f,   0f, 0f,
             1f, -1f,   1f, 1f,
             1f,  1f,   1f, 0f,
        };
        var vb = SharpDX.Direct3D11.Buffer.Create(device, BindFlags.VertexBuffer, vertices);
        vertexBuffer = new VertexBufferBinding(vb, sizeof(float) * 4, 0);
        sampler = new SharpDX.Direct3D11.SamplerState(device, new SamplerStateDescription()
        {
            Filter = SharpDX.Direct3D11.Filter.MinMagMipPoint,
            AddressU = TextureAddressMode.Clamp,
            AddressV = TextureAddressMode.Clamp,
            AddressW = TextureAddressMode.Clamp,
            ComparisonFunction = Comparison.Never,
            MinimumLod = 0,
            MaximumLod = float.MaxValue
        });
    }

    public static SharpDX.Direct3D11.RenderTargetView CreateRenderTarget(int width, int height, SharpDX.DXGI.Format format = SharpDX.DXGI.Format.B8G8R8X8_UNorm)
    {
        return CreateRenderTarget(gDevice, width, height, format);
    }

    public static SharpDX.Direct3D11.RenderTargetView CreateRenderTarget(SharpDX.Direct3D11.Device device, int width, int height, SharpDX.DXGI.Format format = SharpDX.DXGI.Format.B8G8R8X8_UNorm)
    {
        var stagingDesc = new Texture2DDescription
        {
            Width = width,
            Height = height,
            Format = format,
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.RenderTarget,
            OptionFlags = ResourceOptionFlags.None,
            CpuAccessFlags = CpuAccessFlags.None,
            SampleDescription = new SampleDescription(1, 0),
            Usage = ResourceUsage.Default,
        };
        var texture = new SharpDX.Direct3D11.Texture2D(device, stagingDesc);
        return new SharpDX.Direct3D11.RenderTargetView(device, texture);
    }

    public static void Blit()
    {
        //Blit(gDevice);
    }

    public static void Draw(
        SharpDX.Direct3D11.Device device,
        SharpDX.Direct3D11.RenderTargetView rtv,
        int width, int height,
        SharpDX.Direct3D11.ShaderResourceView mainTex,
        SharpDX.Direct3D11.ShaderResourceView overlayTex
        )
    {
        var ctx = device.ImmediateContext;
        ctx.OutputMerger.SetRenderTargets(rtv);
        ctx.Rasterizer.SetViewport(0, 0, width, height);

        ctx.InputAssembler.InputLayout = layout;
        ctx.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
        ctx.InputAssembler.SetVertexBuffers(0, vertexBuffer);

        ctx.VertexShader.Set(vertexShader);
        ctx.PixelShader.Set(pixelShader);

        //using var srv = new ShaderResourceView(device, sourceTex);
        ctx.PixelShader.SetShaderResource(0, mainTex);
        ctx.PixelShader.SetSampler(0, sampler);

        ctx.Draw(4, 0);
    }

    public void Dispose()
    {

    }
}

public class D2DRenderTarget : IDisposable
{
    private SharpDX.Direct3D11.Device d3dDevice;
    private SharpDX.Direct2D1.Device d2dDevice;
    private SharpDX.Direct2D1.DeviceContext target;

    private SolidColorBrush solidBrush;
    private SharpDX.Direct2D1.Bitmap1 rt;
    private SharpDX.Direct2D1.Bitmap1 bitmap;
    private SharpDX.Direct3D11.Texture2D stagingTexture;

    private SharpDX.Size2 pixelSize;
    private SharpDX.DXGI.Format format;

    public D2DRenderTarget()
    {
        format = SharpDX.DXGI.Format.B8G8R8A8_UNorm;
        d3dDevice = new SharpDX.Direct3D11.Device(
            SharpDX.Direct3D.DriverType.Hardware,
            SharpDX.Direct3D11.DeviceCreationFlags.BgraSupport
        );
        using var dxgiDevice = d3dDevice.QueryInterface<SharpDX.DXGI.Device>();
        using var d2dFactory = new SharpDX.Direct2D1.Factory1();
        d2dDevice = new SharpDX.Direct2D1.Device(d2dFactory, dxgiDevice);
        target = new SharpDX.Direct2D1.DeviceContext(
            d2dDevice,
            SharpDX.Direct2D1.DeviceContextOptions.None
        );

        solidBrush = new SolidColorBrush(target, new RawColor4(0f, 1f, 0, 1f));
    }

    public void BeginDraw()
    {
        target.Target = rt;
        target.BeginDraw();
        target.Clear(new RawColor4(0f, 0, 0, 0));
    }

    public void EndDraw()
    {
        target.EndDraw();
        target.Target = null;
    }

    public void CopyTo(SharpDX.Direct3D11.Device device, SharpDX.Direct3D11.Texture2D texture)
    {
        if (texture == null) return;
        bitmap.CopyFromBitmap(rt);
        var d2dData = bitmap.Map(MapOptions.Read);
        device.ImmediateContext.UpdateSubresource(
            new DataBox(
                d2dData.DataPointer,
                d2dData.Pitch,
                pixelSize.Height
            ),
            stagingTexture, 0
        );
        bitmap.Unmap();
        //device.ImmediateContext.CopyResource(stagingTexture, texture);
        device.ImmediateContext.CopySubresourceRegion(
            stagingTexture, 0, null,
            texture, 0
        );
    }

    public void SetTargetSize(SharpDX.Direct3D11.Device device, SharpDX.Size2 pixelSize)
    {
        if (this.pixelSize == pixelSize)
        {
            return;
        }
        this.Release();
        this.pixelSize = pixelSize;
        var stagingDesc = new Texture2DDescription
        {
            Width = pixelSize.Width,
            Height = pixelSize.Height,
            ArraySize = 1,
            MipLevels = 1,
            BindFlags = BindFlags.None,
            Format = format,
            OptionFlags = ResourceOptionFlags.None,
            CpuAccessFlags = CpuAccessFlags.Read | CpuAccessFlags.Write,
            SampleDescription = new SampleDescription(1, 0),
        };

        var bmpProps = new BitmapProperties1(
            new SharpDX.Direct2D1.PixelFormat(
                format,
                SharpDX.Direct2D1.AlphaMode.Premultiplied
            ),
            96, 96,
            BitmapOptions.Target | BitmapOptions.CannotDraw
        );
        var bitmapProperties = new BitmapProperties1
        {
            PixelFormat = rt.PixelFormat,
            BitmapOptions = BitmapOptions.CannotDraw | BitmapOptions.CpuRead,
        };

        rt = new SharpDX.Direct2D1.Bitmap1(target, pixelSize, bmpProps);
        bitmap = new SharpDX.Direct2D1.Bitmap1(target, pixelSize, bitmapProperties);
        stagingTexture = new SharpDX.Direct3D11.Texture2D(device, stagingDesc);
    }

    private void Release()
    {
        Utilities.Dispose(ref rt);
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref stagingTexture);
    }

    public void Dispose()
    {
        Utilities.Dispose(ref rt);
        Utilities.Dispose(ref bitmap);
        Utilities.Dispose(ref stagingTexture);

        Utilities.Dispose(ref target);
        Utilities.Dispose(ref d2dDevice);
        Utilities.Dispose(ref d3dDevice);
    }
}
