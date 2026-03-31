using CodeWalker.Graphic;
using CodeWalker.Properties;
using CodeWalker.Utils;
using SharpDX.WIC;
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

public partial class TextureModExplorerControl : DockContent
{
    public TextureModExplorerControl()
    {
        InitializeComponent();

        var theme = Settings.Default.GetProjectWindowTheme();
        var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
        vsExtender.SetStyle(toolStrip, version, theme);
        vsExtender.SetStyle(m_ProjectTreeViewContextMenu, version, theme);

        // repViewModeBtn.SetEnumDrop<View>(x => treeView.View = x);
        // repViewModeBtn.SelectEnum(modListView.View);
    }

    static class TreeViewIcon
    {
        public const int picture = 0;
        public const int folder = 1;
        public const int document = 2;
    }

    class NodeSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            var a = (TreeNode)x;
            var b = (TreeNode)y;

            var aFolder = a.Tag is not ModTexture;
            var bFolder = b.Tag is not ModTexture;
            if (aFolder != bFolder) return aFolder ? -1 : 1;

            return string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
        }
    }

    TextureModProject project => mainForm.project;
    public TextureModDockForm mainForm;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        BeginInvoke(LoadTreeView);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        base.OnHandleDestroyed(e);
    }

    private void LoadTreeView()
    {
        if (project == null) return;
        var files = new HashSet<Guid>(project.modTextures.Keys);
        foreach (var kv in project.directory)
        {
            var path = kv.Key;
            var folder = kv.Value;
            var parts = path.Split('/');

            var currentNodes = treeView.Nodes;
            foreach (var part in parts)
            {
                TreeNode next = null;
                foreach (TreeNode n in currentNodes)
                {
                    if (n.Text == part)
                    {
                        next = n;
                        break;
                    }
                }
                if (next == null)
                {
                    next = new TreeNode(part);
                    next.ImageIndex = TreeViewIcon.folder;
                    next.SelectedImageIndex = TreeViewIcon.folder;
                    currentNodes.Add(next);
                }
                currentNodes = next.Nodes;
            }
            foreach (var guid in folder.files)
            {
                files.Remove(guid);
                AddFile(currentNodes, guid);
            }
        }

        foreach (var guid in files)
        {
            AddFile(treeView.Nodes, guid);
        }

        void AddFile(TreeNodeCollection nodes, Guid guid)
        {
            if (project.modTextures.TryGetValue(guid, out var modTexture))
            {
                var node = new TreeNode();
                node.Text = modTexture.name;
                node.ImageIndex = TreeViewIcon.picture;
                node.SelectedImageIndex = TreeViewIcon.picture;
                node.Tag = modTexture;
                nodes.Add(node);
            }
        }
        treeView.ImageList = m_ProjectTreeViewIcons;
        treeView.TreeViewNodeSorter = new NodeSorter();
        treeView.Refresh();
    }

    public Dictionary<string, ProjectDirectory> SerializeTreeView()
    {
        var result = new Dictionary<string, ProjectDirectory>();
        foreach (TreeNode node in treeView.Nodes)
        {
            Traverse(node, null);
        }
        return result;

        void Traverse(TreeNode node, string parentPath)
        {
            if (node.Tag is ModTexture) return;

            var path = string.IsNullOrEmpty(parentPath)
                ? node.Text
                : parentPath + "/" + node.Text;

            var dir = new ProjectDirectory
            {
                name = node.Text,
                path = path
            };

            foreach (TreeNode child in node.Nodes)
            {
                if (child.Tag is ModTexture tex)
                {
                    dir.files.Add(tex.id);
                }
            }
            result[path] = dir;
            foreach (TreeNode child in node.Nodes)
            {
                Traverse(child, path);
            }
        }
    }

    private void treeView_BeforeLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
    }

    private void treeView_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
    {
        if (e.Node.Tag is ModTexture modTexture)
        {
            if (!string.IsNullOrEmpty(e.Label))
            {
                modTexture.name = e.Label;
                SetTreeViewDirty();
            }
        }
    }

    private void treeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        if (e.Action is TreeViewAction.ByMouse or TreeViewAction.ByKeyboard)
        {
            if (e.Node.Tag is ModTexture modTexture)
            {
                mainForm.SelectTexMod(modTexture);
            }
        }
    }

    private void treeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
    {
        if (e.Button == MouseButtons.Right)
        {
            treeView.SelectedNode = e.Node;
            importMenuItem.Visible = e.Node.Tag is ModTexture;
            duplicateMenuItem.Visible = e.Node.Tag is ModTexture;
            toolStripSeparator1.Visible = true;
            toolStripSeparator2.Visible = true;
            newFolderMenuItem.Visible = true;
            m_ProjectTreeViewContextMenu.Show(treeView, e.Location);
        }
    }

    private void treeView_MouseUp(object sender, MouseEventArgs e)
    {
        //var node = treeView.GetNodeAt(e.X, e.Y);
        //if (node != null && node.Tag is ModTexture modTexture)
        //{
        //    SelectTexMod(modTexture);
        //}
    }

    private void treeView_ItemDrag(object sender, ItemDragEventArgs e)
    {
        DoDragDrop(e.Item, DragDropEffects.Move);
    }

    private void treeView_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(TreeNode)))
        {
            e.Effect = DragDropEffects.Move;
        }
    }

    private void treeView_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(TreeNode)))
        {
            var pt = treeView.PointToClient(new Point(e.X, e.Y));
            treeView.SelectedNode = treeView.GetNodeAt(pt);
            e.Effect = DragDropEffects.Move;

            var margin = treeView.ItemHeight;
            if (pt.Y < margin)
            {
                // scroll up
                var first = treeView.TopNode;
                if (first?.PrevVisibleNode is { } prevVisibleNode)
                {
                    treeView.TopNode = prevVisibleNode;
                }
            }
            else if (pt.Y > treeView.Height - margin)
            {
                // scroll down
                var first = treeView.TopNode;
                if (first?.NextVisibleNode is { } nextVisibleNode)
                {
                    treeView.TopNode = nextVisibleNode;
                }
            }
        }
    }

    private void treeView_DragDrop(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(typeof(TreeNode)))
        {
            var dragged = (TreeNode)e.Data.GetData(typeof(TreeNode));
            var pt = treeView.PointToClient(new Point(e.X, e.Y));
            var target = treeView.GetNodeAt(pt);

            if (dragged == null || target == null || dragged == target) return;

            // prevent dropping into itself or child
            var parent = target;
            while (parent != null)
            {
                if (parent == dragged) return;
                parent = parent.Parent;
            }

            if (target.Tag is ModTexture)
            {
                dragged.Remove();
                if (target.Parent != null)
                {
                    target.Parent.Nodes.Add(dragged);
                }
                else
                {
                    treeView.Nodes.Add(dragged);
                }
            }
            else
            {
                dragged.Remove();
                target.Nodes.Add(dragged);
                target.Expand();
            }
            SetTreeViewDirty();
        }
    }

    private void treeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
    {
        if (e.Node.Tag is ModTexture) return;

        // e.Node.ImageIndex = TreeViewIcon.folder;
        // e.Node.SelectedImageIndex = TreeViewIcon.folder;
    }

    private void treeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
    {
        if (e.Node.Tag is ModTexture) return;

        // e.Node.ImageIndex = TreeViewIcon.folder;
        // e.Node.SelectedImageIndex = TreeViewIcon.folder;
    }

    private TreeNode CreateNodeAtParent(TreeNode node, string text)
    {
        var treeNode = new TreeNode(text);
        AddNodeAfter(node, treeNode);
        return treeNode;
    }

    private void AddNodeAfter(TreeNode baseNode, TreeNode newNode)
    {
        TreeNodeCollection collection = null;
        if (baseNode.Parent != null)
        {
            collection = baseNode.Parent.Nodes;
        }
        else
        {
            collection = treeView.Nodes;
        }
        collection.Add(newNode);
    }

    private void newFolderMenuItem_Click(object sender, EventArgs e)
    {
        var treeNode = treeView.SelectedNode;
        if (treeNode == null) return;
        var node = new TreeNode("New Folder");
        node.ImageIndex = TreeViewIcon.folder;
        node.SelectedImageIndex = TreeViewIcon.folder;
        if (treeNode.Tag is ModTexture)
        {
            AddNodeAfter(treeNode, node);
        }
        else
        {
            treeNode.Nodes.Insert(0, node);
            treeNode.Expand();
        }
        node.BeginEdit();
        SetTreeViewDirty();
    }

    private void duplicateMenuItem_Click(object sender, EventArgs e)
    {
        Action_Duplicate();
    }

    private void renameMenuItem_Click(object sender, EventArgs e)
    {
        var treeNode = treeView.SelectedNode;
        if (treeNode != null)
        {
            treeNode.BeginEdit();
        }
    }

    private void importMenuItem_Click(object sender, EventArgs e)
    {
        Action_ReimportTex();
    }

    private void SetTreeViewDirty()
    {
    }

    private void toolStripButton1_Click(object sender, EventArgs e)
    {
        mainForm.NewTexMod();
    }

    private void toolStripButton3_Click(object sender, EventArgs e)
    {
        Action_Delete();
    }

    private void toolStripButton6_Click(object sender, EventArgs e)
    {
        //import
        Action_ReimportTex();
    }

    private void toolStripButton9_Click(object sender, EventArgs e)
    {
        // duplicate
        Action_Duplicate();
    }

    private void saveProjectBtn_Click(object sender, EventArgs e)
    {
        mainForm.SaveProject();
    }

    private void toolStripButton2_Click(object sender, EventArgs e)
    {
        mainForm.BeginBuildTexMod();
    }

    private void toolStripButton8_Click(object sender, EventArgs e)
    {
        mainForm.BeginBuildOIVPackage();
    }

    private void deleteMenuItem_Click(object sender, EventArgs e)
    {
        Action_Delete();
    }

    public void Action_ReimportTex()
    {
        if (treeView.SelectedNode is { } treeNode)
        {
            if (treeNode.Tag is ModTexture modTexture)
            {
                mainForm.ReimportTex(modTexture);
                treeNode.Text = modTexture.name;
            }
        }
    }

    public void Action_Delete()
    {
        // delete tex mod
        if (treeView.SelectedNode is { } node)
        {
            if (node.Tag is ModTexture modTexture)
            {
                var text = $"Delete {modTexture.name}?";
                if (MessageBox.Show(text, "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    mainForm.DeleteTexMod(modTexture);
                    node.Remove();
                }
            }
            else
            {
                var list = new List<ModTexture>();
                Traverse(list, node);
                var text = $"Delete {node.Text} with {list.Count} mod textures?";
                if (MessageBox.Show(text, "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                {
                    foreach (var texture in list)
                    {
                        mainForm.DeleteTexMod(texture);
                    }
                    node.Remove();
                }
            }
        }

        static void Traverse(List<ModTexture> list, TreeNode treeNode)
        {
            foreach (TreeNode node in treeNode.Nodes)
            {
                if (node.Tag is ModTexture modTexture)
                {
                    list.Add(modTexture);
                    continue;
                }
                Traverse(list, node);
            }
        }
    }

    public void Action_Duplicate()
    {
        if (treeView.SelectedNode is { } node)
        {
            if (node.Tag is ModTexture modTexture)
            {
                modTexture = mainForm.DuplicateModTexture(modTexture);
                var treeNode = CreateNodeAtParent(node, modTexture.name);
                treeNode.Tag = modTexture;
                treeView.SelectedNode = treeNode;
            }
            else
            {
                var clone = CloneTreeNode(node);
                // ReSharper disable once LocalizableElement
                clone.Text = $"{node.Text} (Clone)";
                AddNodeAfter(node, clone);
            }
        }

        TreeNode CloneTreeNode(TreeNode treeNode)
        {
            var modTexture = treeNode.Tag as ModTexture;
            if (modTexture != null)
            {
                var clone = mainForm.DuplicateModTexture(modTexture);
                clone.name = modTexture.name;
                modTexture = clone;
            }
            var newNode = new TreeNode(treeNode.Text)
            {
                Name = treeNode.Name,
                Tag = modTexture,
                ImageIndex = treeNode.ImageIndex,
                SelectedImageIndex = treeNode.SelectedImageIndex,
                StateImageIndex = treeNode.StateImageIndex,
            };
            foreach (TreeNode child in treeNode.Nodes)
            {
                newNode.Nodes.Add(CloneTreeNode(child));
            }
            return newNode;
        }
    }
}