namespace CodeWalker.TexMod
{
    partial class TextureModMappingControl
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ColumnHeader Texture;
            System.Windows.Forms.ColumnHeader Lod;
            System.Windows.Forms.ColumnHeader File;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureModMappingControl));
            this.toolStrip = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripDropDownButton();
            this.vsExtender = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(this.components);
            this.textureMappingView = new CodeWalker.Forms.AeroListView();
            this.Source = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.copyValueToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lodToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.largeImageList = new System.Windows.Forms.ImageList(this.components);
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            Texture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Lod = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            File = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Texture
            // 
            Texture.Text = "Texture";
            Texture.Width = 260;
            // 
            // Lod
            // 
            Lod.Text = "Lod";
            // 
            // File
            // 
            File.Text = "File";
            File.Width = 160;
            // 
            // toolStrip
            // 
            this.toolStrip.AutoSize = false;
            this.toolStrip.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripButton1,
            this.toolStripButton7});
            this.toolStrip.Location = new System.Drawing.Point(0, 0);
            this.toolStrip.Name = "toolStrip";
            this.toolStrip.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.toolStrip.Size = new System.Drawing.Size(713, 25);
            this.toolStrip.TabIndex = 1;
            this.toolStrip.Text = "toolStrip";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = global::CodeWalker.Properties.Resources._103;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.ToolTipText = "New";
            this.toolStripButton4.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = global::CodeWalker.Properties.Resources._101;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.ToolTipText = "Delete";
            this.toolStripButton5.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "Sync Lod";
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(29, 22);
            this.toolStripButton7.ToolTipText = "View Mode";
            // 
            // vsExtender
            // 
            this.vsExtender.DefaultRenderer = null;
            // 
            // textureMappingView
            // 
            this.textureMappingView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textureMappingView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            Texture,
            Lod,
            File,
            this.Source});
            this.textureMappingView.ContextMenuStrip = this.contextMenuStrip1;
            this.textureMappingView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureMappingView.FullRowSelect = true;
            this.textureMappingView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.textureMappingView.HideSelection = false;
            this.textureMappingView.LargeImageList = this.largeImageList;
            this.textureMappingView.Location = new System.Drawing.Point(0, 25);
            this.textureMappingView.MultiSelect = false;
            this.textureMappingView.Name = "textureMappingView";
            this.textureMappingView.Size = new System.Drawing.Size(713, 236);
            this.textureMappingView.SmallImageList = this.smallImageList;
            this.textureMappingView.TabIndex = 2;
            this.textureMappingView.UseCompatibleStateImageBehavior = false;
            this.textureMappingView.View = System.Windows.Forms.View.Details;
            this.textureMappingView.VirtualMode = true;
            this.textureMappingView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.textureMappingView_ItemSelectionChanged);
            this.textureMappingView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.textureMappingView_RetrieveVirtualItem);
            this.textureMappingView.SelectedIndexChanged += new System.EventHandler(this.textureMappingView_SelectedIndexChanged);
            this.textureMappingView.DoubleClick += new System.EventHandler(this.textureMappingView_DoubleClick);
            this.textureMappingView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.textureMappingView_MouseDown);
            // 
            // Source
            // 
            this.Source.Text = "Source";
            this.Source.Width = 200;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.copyValueToolStripMenuItem,
            this.lodToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(143, 98);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Image = global::CodeWalker.Properties.Resources._103;
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // copyValueToolStripMenuItem
            // 
            this.copyValueToolStripMenuItem.Image = global::CodeWalker.Properties.Resources._079;
            this.copyValueToolStripMenuItem.Name = "copyValueToolStripMenuItem";
            this.copyValueToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.copyValueToolStripMenuItem.Text = "Copy Value";
            this.copyValueToolStripMenuItem.Click += new System.EventHandler(this.copyValueToolStripMenuItem_Click);
            // 
            // lodToolStripMenuItem
            // 
            this.lodToolStripMenuItem.Image = global::CodeWalker.Properties.Resources._020;
            this.lodToolStripMenuItem.Name = "lodToolStripMenuItem";
            this.lodToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.lodToolStripMenuItem.Text = "LOD";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(139, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::CodeWalker.Properties.Resources._101;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(142, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem1_Click);
            // 
            // largeImageList
            // 
            this.largeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.largeImageList.ImageSize = new System.Drawing.Size(64, 64);
            this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // smallImageList
            // 
            this.smallImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.smallImageList.ImageSize = new System.Drawing.Size(1, 20);
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // TextureModMappingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(713, 261);
            this.Controls.Add(this.textureMappingView);
            this.Controls.Add(this.toolStrip);
            this.Name = "TextureModMappingControl";
            this.toolStrip.ResumeLayout(false);
            this.toolStrip.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton7;
        private CodeWalker.Forms.AeroListView textureMappingView;
        private WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender vsExtender;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ColumnHeader Source;
        private System.Windows.Forms.ImageList smallImageList;
        private System.Windows.Forms.ImageList largeImageList;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem copyValueToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem lodToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
    }
}
