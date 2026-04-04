using CodeWalker.GameFiles;
using CodeWalker.Rendering;
using CodeWalker.Utils;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeWalker.Properties;
using CodeWalker.TexMod;
using WeifenLuo.WinFormsUI.Docking;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace CodeWalker.World
{
    public partial class WorldInfoForm : Form
    {
        class ImageListAsync
        {
            public object syncRoot = new();

            private WorldInfoForm owner;
            private Archetype arch;
            private ListView listView;
            private ImageList smallImageList;
            private ImageList largeImageList;
            private CancellationTokenSource cts = new();
            public List<TextureBase> texturesToLoad = new();

            public ImageListAsync(WorldInfoForm form)
            {
                this.owner = form;
                this.arch = form.Selection.Archetype;
                this.smallImageList = form.smallImageList;
                this.largeImageList = form.largeImageList;
                this.listView = form.SelDrawableTexturesListView;
            }

            public void Cancel()
            {
                if (!cts.IsCancellationRequested)
                {
                    cts.Cancel();
                }
            }

            public Task LoadAsync()
            {
                return Task.Run(Load);
            }

            private void Load()
            {
                var smallImages = new Dictionary<uint, Image>();
                var largeImages = new Dictionary<uint, Image>();
                var smallImageSize = smallImageList.ImageSize;
                var largeImageSize = largeImageList.ImageSize;
                for (var i = 0; i < texturesToLoad.Count; i++)
                {
                    var index = i;
                    if (cts.IsCancellationRequested) break;
                    var textureBase = texturesToLoad[i];
                    var texture = textureBase as Texture;
                    if (texture == null)
                    {
                        var errorString = string.Empty;
                        texture = owner.TryGetTexture(arch, textureBase, out var ytd, ref errorString);
                    }
                    if (texture == null) continue;
                    if (!smallImages.TryGetValue(texture.NameHash, out var smallImage))
                    {
                        smallImage = LoadImage(smallImageSize, texture);
                        smallImages.Add(texture.NameHash, smallImage);
                    }
                    if (!largeImages.TryGetValue(texture.NameHash, out var largeImage))
                    {
                        largeImage = LoadImage(largeImageSize, texture);
                        largeImages.Add(texture.NameHash, largeImage);
                    }
                    owner.Invoke(() =>
                    {
                        if (cts.IsCancellationRequested)
                        {
                            smallImage?.Dispose();
                            largeImage?.Dispose();
                            return;
                        }
                        var oldSmall = smallImageList.Images[index];
                        smallImageList.Images[index] = smallImage;
                        oldSmall?.Dispose();

                        var oldLarge = largeImageList.Images[index];
                        largeImageList.Images[index] = largeImage;
                        oldLarge?.Dispose();
                        listView.RedrawItems(index, index, false);
                    });
                }
            }

            private Image LoadImage(Size imageSize, Texture texture)
            {
                var mip = FindMpis(
                    texture.Width,
                    texture.Height,
                    imageSize.Width,
                    imageSize.Height,
                    texture.Levels
                );
                return texture.CreateImage(mip);
            }

            private static int FindMpis(int width, int height, int targetWidth, int targetHeight, int maxMips)
            {
                var mip = 0;
                while (mip < maxMips - 1)
                {
                    var w = width >> (mip + 1);
                    var h = height >> (mip + 1);
                    if (w <= targetWidth && h <= targetHeight)
                    {
                        break;
                    }
                    mip++;
                }
                return mip;
            }

            public void GeneratePlaceholder()
            {
                var placeholder = new Bitmap(1, 1, PixelFormat.Format32bppArgb);
                for (var i = 0; i < texturesToLoad.Count; i++)
                {
                    smallImageList.Images.Add((Image)placeholder.Clone());
                    largeImageList.Images.Add((Image)placeholder.Clone());
                }
                placeholder.Dispose();
            }
        }

        WorldForm WorldForm;
        MapSelection Selection;
        string SelectionMode = "";
        bool MouseSelectEnable = false;
        Texture currentTex; // Used by save button
        GameFile currentTexOwner;
        private Image displayingImage;
        private ImageListAsync imageListAsync;

        public WorldInfoForm(WorldForm worldForm)
        {
            WorldForm = worldForm;
            InitializeComponent();
            SelDrawableTexturesListView.UseCompatibleStateImageBehavior = false;

            var theme = Settings.Default.GetProjectWindowTheme();
            var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
            vsExtender.SetStyle(toolStrip1, version, theme);

            UpdateViewModeButton();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            Utilities.Dispose(ref displayingImage);
            base.OnHandleDestroyed(e);
        }

        private void MouseSelectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (MouseSelectCheckBox.Checked != MouseSelectEnable)
            {
                MouseSelectEnable = MouseSelectCheckBox.Checked;
                WorldForm.OnInfoFormSelectionModeChanged(SelectionMode, MouseSelectEnable);
            }
        }

        private void SelectionModeComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (SelectionModeComboBox.Text != SelectionMode)
            {
                SelectionMode = SelectionModeComboBox.Text;
                WorldForm.OnInfoFormSelectionModeChanged(SelectionMode, MouseSelectEnable);
            }
        }

        public void SetSelectionMode(string mode, bool enable)
        {
            SelectionMode = mode;
            MouseSelectEnable = enable;
            SelectionModeComboBox.Text = mode;
            MouseSelectCheckBox.Checked = enable;
        }

        public void SetSelection(MapSelection item)
        {
            Selection = item;

            SelectionNameTextBox.Text = item.GetNameString("Nothing selected");
            //SelEntityPropertyGrid.SelectedObject = item.EntityDef;
            SelArchetypePropertyGrid.SelectedObject = item.Archetype;
            SelDrawablePropertyGrid.SelectedObject = item.Drawable;
            SelDrawableModelPropertyGrid.SelectedObject = null;
            SelDrawableModelsTreeView.Nodes.Clear();
            SelDrawableTexturesListView.Items.Clear();
            SelDrawableTexturePropertyGrid.SelectedObject = null;
            SelDrawableTexturePictureBox.Image = null;
            HierarchyTreeView.Nodes.Clear();
            imageListAsync?.Cancel();
            smallImageList.Images.Clear();
            largeImageList.Images.Clear();
            imageListAsync = new ImageListAsync(this);
            if (item.Drawable != null)
            {
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.High, "High Detail", true);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Med, "Medium Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Low, "Low Detail", false);
                AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.VLow, "Very Low Detail", false);
                //AddSelectionDrawableModelsTreeNodes(item.Drawable.DrawableModels?.Extra, "X Detail", false);
            }
            foreach (ListViewItem listViewItem in SelDrawableTexturesListView.Items)
            {
                imageListAsync.texturesToLoad.Add((TextureBase)listViewItem.Tag);
            }
            imageListAsync.GeneratePlaceholder();
            imageListAsync.LoadAsync();

            if (item.EntityDef != null)
            {
                AddSelectionEntityHierarchyNodes(item.EntityDef);
            }

            if (item.MultipleSelectionItems != null)
            {
                SelectionEntityTabPage.Text = "Multiple items";
                SelEntityPropertyGrid.SelectedObject = item.MultipleSelectionItems;
            }
            else if (item.TimeCycleModifier != null)
            {
                SelectionEntityTabPage.Text = "Time Cycle Modifier";
                SelEntityPropertyGrid.SelectedObject = item.TimeCycleModifier;
            }
            else if (item.CarGenerator != null)
            {
                SelectionEntityTabPage.Text = "Car Generator";
                SelEntityPropertyGrid.SelectedObject = item.CarGenerator;
            }
            else if (item.LodLight != null)
            {
                SelectionEntityTabPage.Text = "LOD Light";
                SelEntityPropertyGrid.SelectedObject = item.LodLight;
            }
            else if (item.GrassBatch != null)
            {
                SelectionEntityTabPage.Text = "Grass";
                SelEntityPropertyGrid.SelectedObject = item.GrassBatch;
            }
            else if (item.BoxOccluder != null)
            {
                SelectionEntityTabPage.Text = "Box Occluder";
                SelEntityPropertyGrid.SelectedObject = item.BoxOccluder;
            }
            else if (item.OccludeModelTri != null)
            {
                SelectionEntityTabPage.Text = "Occlude Model Triangle";
                SelEntityPropertyGrid.SelectedObject = item.OccludeModelTri;
            }
            else if (item.WaterQuad != null)
            {
                SelectionEntityTabPage.Text = "Water Quad";
                SelEntityPropertyGrid.SelectedObject = item.WaterQuad;
            }
            else if (item.CalmingQuad != null)
            {
                SelectionEntityTabPage.Text = "Water Calming Quad";
                SelEntityPropertyGrid.SelectedObject = item.CalmingQuad;
            }
            else if (item.WaveQuad != null)
            {
                SelectionEntityTabPage.Text = "Water Wave Quad";
                SelEntityPropertyGrid.SelectedObject = item.WaveQuad;
            }
            else if (item.NavPoly != null)
            {
                SelectionEntityTabPage.Text = "Nav Poly";
                SelEntityPropertyGrid.SelectedObject = item.NavPoly;
            }
            else if (item.NavPoint != null)
            {
                SelectionEntityTabPage.Text = "Nav Point";
                SelEntityPropertyGrid.SelectedObject = item.NavPoint;
            }
            else if (item.NavPortal != null)
            {
                SelectionEntityTabPage.Text = "Nav Portal";
                SelEntityPropertyGrid.SelectedObject = item.NavPortal;
            }
            else if (item.PathNode != null)
            {
                SelectionEntityTabPage.Text = "Path Node";
                SelEntityPropertyGrid.SelectedObject = item.PathNode;
            }
            else if (item.TrainTrackNode != null)
            {
                SelectionEntityTabPage.Text = "Train Track Node";
                SelEntityPropertyGrid.SelectedObject = item.TrainTrackNode;
            }
            else if (item.ScenarioNode != null)
            {
                SelectionEntityTabPage.Text = item.ScenarioNode.FullTypeName;
                SelEntityPropertyGrid.SelectedObject = item.ScenarioNode;
            }
            else if (item.Audio != null)
            {
                SelectionEntityTabPage.Text = item.Audio.FullTypeName;
                SelEntityPropertyGrid.SelectedObject = item.Audio;
            }
            else
            {
                SelectionEntityTabPage.Text = "Entity";
                SelEntityPropertyGrid.SelectedObject = item.EntityDef;
            }

            if (item.EntityExtension != null)
            {
                SelectionExtensionTabPage.Text = "Entity Extension";
                SelExtensionPropertyGrid.SelectedObject = item.EntityExtension;
            }
            else if (item.ArchetypeExtension != null)
            {
                SelectionExtensionTabPage.Text = "Archetype Extension";
                SelExtensionPropertyGrid.SelectedObject = item.ArchetypeExtension;
            }
            else if (item.CollisionVertex != null)
            {
                SelectionExtensionTabPage.Text = "Collision Vertex";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionVertex;
            }
            else if (item.CollisionPoly != null)
            {
                SelectionExtensionTabPage.Text = "Collision Polygon";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionPoly;
            }
            else if (item.CollisionBounds != null)
            {
                SelectionExtensionTabPage.Text = "Collision Bounds";
                SelExtensionPropertyGrid.SelectedObject = item.CollisionBounds;
            }
            else
            {
                SelectionExtensionTabPage.Text = "Extension";
                SelExtensionPropertyGrid.SelectedObject = null;
            }
        }

        private void AddSelectionDrawableModelsTreeNodes(DrawableModel[] models, string prefix, bool check)
        {
            if (models == null) return;

            for (var mi = 0; mi < models.Length; mi++)
            {
                var model = models[mi];
                var mprefix = prefix + " " + (mi + 1).ToString();
                var mnode = SelDrawableModelsTreeView.Nodes.Add(mprefix + " " + model.ToString());
                mnode.Tag = model;
                mnode.Checked = check;

                if (model.Geometries == null) continue;
                foreach (var geom in model.Geometries)
                {
                    var gname = geom.ToString();
                    var gnode = mnode.Nodes.Add(gname);
                    gnode.Tag = geom;
                    gnode.Checked = true; // check;

                    if ((geom.Shader != null) && (geom.Shader.ParametersList != null) && (geom.Shader.ParametersList.Hashes != null))
                    {
                        var pl = geom.Shader.ParametersList;
                        var h = pl.Hashes;
                        var p = pl.Parameters;
                        for (var ip = 0; ip < h.Length; ip++)
                        {
                            var hash = pl.Hashes[ip];
                            var parm = pl.Parameters[ip];
                            if (parm.Data is TextureBase tex)
                            {
                                var t = tex as Texture;
                                var listItem = new ListViewItem(tex.Name.Trim());
                                listItem.Tag = tex;
                                listItem.SubItems.Add($"{tex.Width}x{tex.Height}");
                                listItem.SubItems.Add(t != null ? "embedded" : string.Empty);
                                listItem.ImageIndex = SelDrawableTexturesListView.Items.Count;
                                SelDrawableTexturesListView.Items.Add(listItem);
                            }
                        }
                    }
                }

                mnode.Expand();
            }
        }

        private void AddSelectionEntityHierarchyNodes(YmapEntityDef entity)
        {
            if (entity == null) return;

            var e = entity;
            TreeNode tn = null;
            TreeNode seltn = null;

            while (e != null)
            {
                var newtn = new TreeNode(e.Name);
                newtn.Tag = e;
                if (tn != null)
                {
                    newtn.Nodes.Add(tn);
                }
                else
                {
                    seltn = newtn;
                }
                if (e.Children != null)
                {
                    foreach (var c in e.Children)
                    {
                        if ((tn != null) && (c == tn.Tag)) continue;
                        var ctn = new TreeNode(c.Name);
                        ctn.Tag = c;
                        newtn.Nodes.Add(ctn);
                    }
                }

                tn = newtn;
                e = e.Parent;
            }

            if (tn != null)
            {
                HierarchyTreeView.Nodes.Add(tn);
                tn.ExpandAll();
            }
            if (seltn != null)
            {
                HierarchyTreeView.SelectedNode = seltn;
            }
        }

        private void DisplayTexture(Texture tex, int mip)
        {
            try
            {
                var cmip = Math.Min(Math.Max(mip, 0), tex.Levels - 1);
                var w = tex.Width >> cmip;
                var h = tex.Height >> cmip;
                var image = tex.CreateImage(mip);

                Utilities.Dispose(ref displayingImage);
                displayingImage = image;
                SelDrawableTexturePictureBox.Image = image;
                SelTextureDimensionsLabel.Text = $"{w} x {h}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error reading texture mip:\n{ex}");
                SelDrawableTexturePictureBox.Image = null;
            }
        }

        private void SelectTexture(TextureBase texbase, bool mipchange)
        {
            var tex = texbase as Texture;
            YtdFile ytd = null;
            var errstr = string.Empty;
            currentTexOwner = null;
            if ((tex == null) && (texbase != null))
            {
                var arch = Selection.Archetype;
                tex = TryGetTexture(arch, texbase, out ytd, ref errstr);
            }
            if (tex != null)
            {
                currentTex = tex;
                var mip = 0;
                if (mipchange)
                {
                    mip = SelTextureMipTrackBar.Value;
                    if (mip >= tex.Levels) mip = tex.Levels - 1;
                }
                else
                {
                    SelTextureMipTrackBar.Maximum = tex.Levels - 1;
                }
                DisplayTexture(tex, mip);
                ResetPictureBox();

                //try get owner drawable to get the name for the dictionary textbox...
                object owner = null;
                if (Selection.Drawable != null)
                {
                    owner = Selection.Drawable.Owner;
                }
                var ydr = owner as YdrFile;
                var ydd = owner as YddFile;
                var yft = owner as YftFile;

                SelTextureNameTextBox.Text = tex.Name;
                SelTextureDictionaryTextBox.Text = (ytd != null) ? ytd.Name : (ydr != null) ? ydr.Name : (ydd != null) ? ydd.Name : (yft != null) ? yft.Name : string.Empty;
                toolStripSaveButton.Enabled = true;
                toolStripSaveAllButton.Enabled = true;
                if (ytd != null)
                {
                    currentTexOwner = ytd;
                    pathTextBox.Text = ytd.RpfFileEntry?.Path;
                }
                else if (owner is GameFile gameFile)
                {
                    currentTexOwner = gameFile;
                    pathTextBox.Text = gameFile.RpfFileEntry?.Path;
                }
                else
                {
                    pathTextBox.Text = string.Empty;
                }
            }
            else
            {
                ResetPictureBox();
                SelDrawableTexturePictureBox.Image = null;
                SelTextureNameTextBox.Text = errstr;
                SelTextureDictionaryTextBox.Text = string.Empty;
                pathTextBox.Text = string.Empty;
                SelTextureMipTrackBar.Value = 0;
                SelTextureMipTrackBar.Maximum = 0;
                SelTextureDimensionsLabel.Text = "-";
                toolStripSaveButton.Enabled = false;
                toolStripSaveAllButton.Enabled = false;
                currentTex = null;
            }
        }

        private Texture TryGetTexture(Archetype arch, TextureBase texbase, out YtdFile ytd, ref string errstr)
        {
            //need to load from txd.
            var texhash = texbase.NameHash;
            var txdHash = (arch != null) ? arch.TextureDict.Hash : 0;
            var tex = TryGetTextureFromYtd(texhash, txdHash, out ytd);
            if (tex == null)
            {
                //search parent ytds...
                var ptxdhash = WorldForm.GameFileCache.TryGetParentYtdHash(txdHash);
                while ((ptxdhash != 0) && (tex == null))
                {
                    tex = TryGetTextureFromYtd(texhash, ptxdhash, out ytd);
                    if (tex == null)
                    {
                        ptxdhash = WorldForm.GameFileCache.TryGetParentYtdHash(ptxdhash);
                    }
                    else
                    {
                    }
                }
                if (tex == null)
                {
                    ytd = WorldForm.GameFileCache.TryGetTextureDictForTexture(texhash);
                    if (ytd != null)
                    {
                        var tries = 0;
                        while (!ytd.Loaded && (tries < 500)) //wait upto ~5 sec
                        {
                            System.Threading.Thread.Sleep(10);
                            tries++;
                        }
                        if (ytd.Loaded)
                        {
                            tex = ytd.TextureDict.Lookup(texhash);
                        }
                    }
                    if (tex == null)
                    {
                        ytd = null;
                        errstr = "<Couldn't find texture!>";
                    }
                }
            }
            return tex;
        }

        private Texture TryGetTextureFromYtd(uint texHash, uint txdHash, out YtdFile ytd)
        {
            if (txdHash != 0)
            {
                ytd = WorldForm.GameFileCache.GetYtd(txdHash);
                if (ytd != null)
                {
                    var tries = 0;
                    while (!ytd.Loaded && (tries < 500)) //wait upto ~5 sec
                    {
                        System.Threading.Thread.Sleep(10);
                        tries++;
                    }
                    if (ytd.Loaded)
                    {
                        return ytd.TextureDict.Lookup(texHash);
                    }
                }
            }
            ytd = null;
            return null;
        }

        private void WorldInfoForm_Load(object sender, EventArgs e)
        {
            SetSelection(Selection);
        }

        private void WorldInfoForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            WorldForm.OnInfoFormClosed();
        }

        public void SyncSelDrawableModelsTreeNode(TreeNode node)
        {
            //called by the world form when a selection treeview node is checked/unchecked.
            foreach (TreeNode mnode in SelDrawableModelsTreeView.Nodes)
            {
                if (mnode.Tag == node.Tag)
                {
                    if (mnode.Checked != node.Checked)
                    {
                        mnode.Checked = node.Checked;
                    }
                }
                foreach (TreeNode gnode in mnode.Nodes)
                {
                    if (gnode.Tag == node.Tag)
                    {
                        if (gnode.Checked != node.Checked)
                        {
                            gnode.Checked = node.Checked;
                        }
                    }
                }
            }
        }

        private void SelDrawableModelsTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            WorldForm.SyncSelDrawableModelsTreeNode(e.Node);
        }

        private void SelDrawableModelsTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            SelDrawableModelPropertyGrid.SelectedObject = e.Node?.Tag;
        }

        private void SelDrawableTexturesTreeView_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            foreach (ListViewItem selectedItem in SelDrawableTexturesListView.SelectedItems)
            {
                var textureBase = selectedItem.Tag as TextureBase;
                SelDrawableTexturePropertyGrid.SelectedObject = textureBase;
                SelTextureMipTrackBar.Value = 0;
                SelTextureMipLabel.Text = "0";

                SelectTexture(textureBase, false);
                return;
            }
        }

        private void SelTextureMipTrackBar_Scroll(object sender, EventArgs e)
        {
            foreach (ListViewItem selectedItem in SelDrawableTexturesListView.SelectedItems)
            {
                var textureBase = selectedItem.Tag as TextureBase;
                SelTextureMipLabel.Text = SelTextureMipTrackBar.Value.ToString();
                SelectTexture(textureBase, true);
                return;
            }
        }

        private void HierarchyTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var sele = HierarchyTreeView.SelectedNode?.Tag as YmapEntityDef;
            HierarchyPropertyGrid.SelectedObject = sele;
        }

        private void SaveAllTexturesButton_Click(object sender, EventArgs e)
        {
            if (FolderBrowserDialog.ShowDialogNew() != DialogResult.OK) return;
            var folderpath = FolderBrowserDialog.SelectedPath;
            if (!folderpath.EndsWith("\\")) folderpath += "\\";

            var texs = new List<Texture>();
            // foreach (TreeNode modelnode in SelDrawableTexturesTreeView.Nodes)
            // {
            //     foreach (TreeNode geomnode in modelnode.Nodes)
            //     {
            //         foreach (TreeNode texnode in geomnode.Nodes)
            //         {
            //             var texbase = texnode.Tag as TextureBase;
            //             var tex = texbase as Texture;
            //             var errstr = "";
            //             if ((tex == null) && (texbase != null))
            //             {
            //                 tex = TryGetTexture(texbase, out _, ref errstr);
            //             }
            //             if (tex != null)
            //             {
            //                 if (!texs.Contains(tex))
            //                 {
            //                     texs.Add(tex);
            //                 }
            //             }
            //         }
            //     }
            // }

            foreach (var tex in texs)
            {
                var fpath = folderpath + tex.Name + ".dds";
                var dds = DDSIO.GetDDSFile(tex);
                File.WriteAllBytes(fpath, dds);
            }
        }

        private void SaveTextureButton_Click(object sender, EventArgs e)
        {
            if (currentTex == null) return;
            var fname = currentTex.Name + ".dds";
            SaveFileDialog.FileName = fname;
            if (SaveFileDialog.ShowDialog() != DialogResult.OK) return;
            var fpath = SaveFileDialog.FileName;
            var dds = DDSIO.GetDDSFile(currentTex);
            File.WriteAllBytes(fpath, dds);
        }

        float _zoom = 1f;
        PointF _pan;

        Color _rectColor = Color.FromArgb(127, 255, 0, 0);

        bool _panning;
        PointF _panStart;
        PointF _panOrigin;

        bool _drawing;
        bool _isFill;
        Rectangle _rect;
        Point _start;

        private void ResetPictureBox()
        {
            _zoom = 1f;
            _pan = default;
            _rect = new Rectangle();
            if (displayingImage != null)
            {
                var maxSize = Math.Max(displayingImage.Width, displayingImage.Height);
                var maxView = Math.Min(SelDrawableTexturePictureBox.Width, SelDrawableTexturePictureBox.Height);
                if (maxSize > 0)
                {
                    _zoom = (maxView * 0.9f) / maxSize;

                    var scaledWidth = displayingImage.Width * _zoom;
                    var scaledHeight = displayingImage.Height * _zoom;

                    var offsetX = (SelDrawableTexturePictureBox.Width - scaledWidth) * 0.5f;
                    var offsetY = (SelDrawableTexturePictureBox.Height - scaledHeight) * 0.5f;

                    _pan = new PointF(offsetX, offsetY);
                }
            }
        }

        private void SelDrawableTexturePictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            var oldZoom = _zoom;

            if (e.Delta > 0)
                _zoom *= 1.1f;
            else
                _zoom /= 1.1f;

            if (_zoom < 0.1f) _zoom = 0.1f;
            if (_zoom > 20f) _zoom = 20f;

            float mx = e.X;
            float my = e.Y;

            _pan.X = mx - (mx - _pan.X) * (_zoom / oldZoom);
            _pan.Y = my - (my - _pan.Y) * (_zoom / oldZoom);

            SelDrawableTexturePictureBox.Invalidate();
        }

        private void SelDrawableTexturePictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (currentTex == null) return;
            if (e.Button == MouseButtons.Middle)
            {
                _panning = true;
                _panStart = e.Location;
                _panOrigin = _pan;
            }
            else if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                _drawing = true;
                _isFill = e.Button == MouseButtons.Right;
                _start = ScreenToImage(e.Location);
                _rect = new Rectangle(_start, Size.Empty);
            }
        }

        Point ScreenToImage(Point p)
        {
            return new Point((int)((p.X - _pan.X) / _zoom), (int)((p.Y - _pan.Y) / _zoom));
        }

        private void SelDrawableTexturePictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Middle)
            {
                _panning = false;
            }
            else if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                _drawing = false;
                ApplyPaintDrawing();
            }
        }

        private void ApplyPaintDrawing()
        {
            if (currentTex == null || _rect.Width <= 0 || _rect.Height <= 0) return;

            lock (WorldForm.RenderSyncRoot)
            {
                var texture = WorldForm.Renderer.RenderableCache.FindRenderableTexture(x =>
                    x.Key.NameHash == currentTex.NameHash);
                if (texture == null) return;

                var width = currentTex.Width;
                var height = currentTex.Height;
                var bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);
                using (var g = Graphics.FromImage(bitmap))
                {
                    g.Clear(Color.Transparent);
                    g.DrawImageUnscaled(SelDrawableTexturePictureBox.Image, 0, 0);
                    if (_isFill)
                    {
                        using (var brush = new SolidBrush(_rectColor))
                        {
                            g.FillRectangle(brush, _rect);
                        }
                    }
                    else
                    {
                        using (var pen = new Pen(_rectColor, 1))
                        {
                            g.DrawRectangle(pen, _rect);
                        }
                    }
                }
                texture.Load(WorldForm.Renderer.Device, bitmap);
                bitmap.Dispose();
            }
        }

        private void SelDrawableTexturePictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (currentTex == null) return;
            if (_panning)
            {
                _pan.X = _panOrigin.X + (e.X - _panStart.X);
                _pan.Y = _panOrigin.Y + (e.Y - _panStart.Y);

                SelDrawableTexturePictureBox.Invalidate();
            }
            if (_drawing)
            {
                var cur = ScreenToImage(e.Location);

                var x = Math.Min(_start.X, cur.X);
                var y = Math.Min(_start.Y, cur.Y);
                var w = Math.Abs(_start.X - cur.X);
                var h = Math.Abs(_start.Y - cur.Y);

                _rect = new Rectangle(x, y, w, h);
                SelDrawableTexturePictureBox.Invalidate();
            }
        }

        private void SelDrawableTexturePictureBox_Paint(object sender, PaintEventArgs e)
        {
            if (currentTex == null) return;
            e.Graphics.Clear(SelDrawableTexturePictureBox.BackColor);

            using (var font = new Font("Consolas", 12f))
            using (var brush = new SolidBrush(Color.Lime))
            {
                e.Graphics.DrawString($"zoom: {_zoom * 100:F2}%", font, brush, 10, 4);
                e.Graphics.DrawString($"pan: {_pan}", font, brush, 10, 20);
            }

            e.Graphics.TranslateTransform(_pan.X, _pan.Y);
            e.Graphics.ScaleTransform(_zoom, _zoom);

            e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            if (SelDrawableTexturePictureBox.Image != null)
                e.Graphics.DrawImage(SelDrawableTexturePictureBox.Image, 0, 0);

            if (_rect.Width > 0 && _rect.Height > 0)
            {
                if (_isFill)
                {
                    using (var brush = new SolidBrush(_rectColor))
                    {
                        e.Graphics.FillRectangle(brush, _rect);
                    }
                }
                else
                {
                    using (var pen = new Pen(_rectColor, 1 / _zoom))
                    {
                        e.Graphics.DrawRectangle(pen, _rect);
                    }
                }
            }
        }

        private void modBtn_Click(object sender, EventArgs e)
        {
            if (currentTexOwner != null)
            {
                var info = new AddModSourceInfo();
                info.texName = currentTex.Name;
                info.gameFile = currentTexOwner;
                info.lod = this.WorldForm.Renderer.renderworldMaxLOD;
                info.position = this.WorldForm.Renderer.camera.FollowEntity.Position;
                info.rotation = this.WorldForm.Renderer.camera.CurrentRotation;
                info.hasCameraInfo = true;
                TexMod.TextureModDockForm.ShowAddModSource(WorldForm, info);
            }
        }

        private void UpdateViewModeButton()
        {
            toolStripGridButton.Visible = SelDrawableTexturesListView.View != View.LargeIcon;
            toolStripListViewButton.Visible = SelDrawableTexturesListView.View == View.LargeIcon;
        }

        private void toolStripGridButton_Click(object sender, EventArgs e)
        {
            SelDrawableTexturesListView.View = View.LargeIcon;
            UpdateViewModeButton();
        }

        private void toolStripListViewButton_Click(object sender, EventArgs e)
        {
            SelDrawableTexturesListView.View = View.Details;
            UpdateViewModeButton();
        }
    }
}