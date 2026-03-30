using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.TexMod;

public partial class TextureModExplorerControl : DockContent
{
    public TextureModExplorerControl()
    {
        InitializeComponent();
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

            if (aFolder && !bFolder) return -1;
            if (!aFolder && bFolder) return 1;

            return string.Compare(a.Text, b.Text, StringComparison.OrdinalIgnoreCase);
        }
    }

    TextureModProject project => mainForm.project;
    TextureModDockForm mainForm;

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        LoadTreeView();
    }

    private void LoadTreeView()
    {
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
        treeView.Sort();
        treeView.Refresh();
    }

    private Dictionary<string, ProjectDirectory> SaveTreeView()
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
                //TODO:
                //mainForm.SelectTexMod(modTexture);
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

    private void newFolderMenuItem_Click(object sender, EventArgs e)
    {
        var treeNode = treeView.SelectedNode;
        if (treeNode == null) return;
        var node = new TreeNode("New Folder");
        node.ImageIndex = TreeViewIcon.folder;
        node.SelectedImageIndex = TreeViewIcon.folder;
        if (treeNode.Tag is ModTexture)
        {
            if (treeNode.Parent == null)
            {
                treeView.Nodes.Insert(0, node);
            }
            else
            {
                treeNode.Parent.Nodes.Add(node);
            }
        }
        else
        {
            treeNode.Nodes.Insert(0, node);
            treeNode.Expand();
        }
        SetTreeViewDirty();
    }

    private void duplicateMenuItem_Click(object sender, EventArgs e)
    {
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
    }

    private void SetTreeViewDirty()
    {
    }
}
