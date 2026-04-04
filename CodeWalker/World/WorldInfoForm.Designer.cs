using CodeWalker.WinForms;

namespace CodeWalker.World
{
    partial class WorldInfoForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WorldInfoForm));
            this.SelectionTabControl = new System.Windows.Forms.TabControl();
            this.SelectionTexturesTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.SelDrawableTexturesListView = new CodeWalker.Forms.AeroListView();
            this.nameColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.InfoColumn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.largeImageList = new System.Windows.Forms.ImageList(this.components);
            this.smallImageList = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripSaveButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSaveAllButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripPaintButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripGridButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripListViewButton = new System.Windows.Forms.ToolStripButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.pathTextBox = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SelTextureDimensionsLabel = new System.Windows.Forms.Label();
            this.SelTextureMipTrackBar = new System.Windows.Forms.TrackBar();
            this.SelTextureMipLabel = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SelTextureDictionaryTextBox = new System.Windows.Forms.TextBox();
            this.SelTextureNameTextBox = new System.Windows.Forms.TextBox();
            this.SelDrawableTexturePictureBox = new System.Windows.Forms.PictureBox();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.SelDrawableTexturePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionModelsTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.SelDrawableModelsTreeView = new CodeWalker.WinForms.TreeViewFix();
            this.SelDrawableModelPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionEntityTabPage = new System.Windows.Forms.TabPage();
            this.SelEntityPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionArchetypeTabPage = new System.Windows.Forms.TabPage();
            this.SelArchetypePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionDrawableTabPage = new System.Windows.Forms.TabPage();
            this.SelDrawablePropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionExtensionTabPage = new System.Windows.Forms.TabPage();
            this.SelExtensionPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.SelectionHierarchyTabPage = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.HierarchyTreeView = new System.Windows.Forms.TreeView();
            this.HierarchyPropertyGrid = new CodeWalker.WinForms.PropertyGridFix();
            this.MouseSelectCheckBox = new System.Windows.Forms.CheckBox();
            this.SelectionNameTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.SelectionModeComboBox = new System.Windows.Forms.ComboBox();
            this.SaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.FolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.vsExtender = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(this.components);
            this.sizeColumnHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SelectionTabControl.SuspendLayout();
            this.SelectionTexturesTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelDrawableTexturePictureBox)).BeginInit();
            this.tabPage4.SuspendLayout();
            this.SelectionModelsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SelectionEntityTabPage.SuspendLayout();
            this.SelectionArchetypeTabPage.SuspendLayout();
            this.SelectionDrawableTabPage.SuspendLayout();
            this.SelectionExtensionTabPage.SuspendLayout();
            this.SelectionHierarchyTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.SuspendLayout();
            // 
            // SelectionTabControl
            // 
            this.SelectionTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionTabControl.Controls.Add(this.SelectionTexturesTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionModelsTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionEntityTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionArchetypeTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionDrawableTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionExtensionTabPage);
            this.SelectionTabControl.Controls.Add(this.SelectionHierarchyTabPage);
            this.SelectionTabControl.Location = new System.Drawing.Point(10, 44);
            this.SelectionTabControl.Name = "SelectionTabControl";
            this.SelectionTabControl.SelectedIndex = 0;
            this.SelectionTabControl.Size = new System.Drawing.Size(825, 495);
            this.SelectionTabControl.TabIndex = 28;
            // 
            // SelectionTexturesTabPage
            // 
            this.SelectionTexturesTabPage.Controls.Add(this.splitContainer2);
            this.SelectionTexturesTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionTexturesTabPage.Name = "SelectionTexturesTabPage";
            this.SelectionTexturesTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionTexturesTabPage.TabIndex = 4;
            this.SelectionTexturesTabPage.Text = "Textures";
            this.SelectionTexturesTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.panel1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer2.Size = new System.Drawing.Size(817, 469);
            this.splitContainer2.SplitterDistance = 327;
            this.splitContainer2.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.SelDrawableTexturesListView);
            this.panel1.Controls.Add(this.toolStrip1);
            this.panel1.Location = new System.Drawing.Point(6, 6);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(318, 457);
            this.panel1.TabIndex = 0;
            // 
            // SelDrawableTexturesListView
            // 
            this.SelDrawableTexturesListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturesListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelDrawableTexturesListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameColumn,
            this.sizeColumnHeader,
            this.InfoColumn});
            this.SelDrawableTexturesListView.FullRowSelect = true;
            this.SelDrawableTexturesListView.HideSelection = false;
            this.SelDrawableTexturesListView.LargeImageList = this.largeImageList;
            this.SelDrawableTexturesListView.Location = new System.Drawing.Point(0, 25);
            this.SelDrawableTexturesListView.MultiSelect = false;
            this.SelDrawableTexturesListView.Name = "SelDrawableTexturesListView";
            this.SelDrawableTexturesListView.Size = new System.Drawing.Size(316, 430);
            this.SelDrawableTexturesListView.SmallImageList = this.smallImageList;
            this.SelDrawableTexturesListView.TabIndex = 7;
            this.SelDrawableTexturesListView.TileSize = new System.Drawing.Size(64, 64);
            this.SelDrawableTexturesListView.UseCompatibleStateImageBehavior = false;
            this.SelDrawableTexturesListView.View = System.Windows.Forms.View.Details;
            this.SelDrawableTexturesListView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.SelDrawableTexturesTreeView_ItemSelectionChanged);
            // 
            // nameColumn
            // 
            this.nameColumn.Text = "Name";
            this.nameColumn.Width = 154;
            // 
            // InfoColumn
            // 
            this.InfoColumn.DisplayIndex = 1;
            this.InfoColumn.Text = "Info";
            this.InfoColumn.Width = 100;
            // 
            // largeImageList
            // 
            this.largeImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.largeImageList.ImageSize = new System.Drawing.Size(100, 100);
            this.largeImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // smallImageList
            // 
            this.smallImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.smallImageList.ImageSize = new System.Drawing.Size(20, 20);
            this.smallImageList.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSaveButton,
            this.toolStripSaveAllButton,
            this.toolStripPaintButton,
            this.toolStripGridButton,
            this.toolStripListViewButton});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.toolStrip1.Size = new System.Drawing.Size(316, 25);
            this.toolStrip1.TabIndex = 6;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripSaveButton
            // 
            this.toolStripSaveButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSaveButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSaveButton.Image")));
            this.toolStripSaveButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSaveButton.Name = "toolStripSaveButton";
            this.toolStripSaveButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripSaveButton.Text = "Save";
            this.toolStripSaveButton.Click += new System.EventHandler(this.SaveTextureButton_Click);
            // 
            // toolStripSaveAllButton
            // 
            this.toolStripSaveAllButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripSaveAllButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripSaveAllButton.Image")));
            this.toolStripSaveAllButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSaveAllButton.Name = "toolStripSaveAllButton";
            this.toolStripSaveAllButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripSaveAllButton.Text = "Save All";
            this.toolStripSaveAllButton.Click += new System.EventHandler(this.SaveAllTexturesButton_Click);
            // 
            // toolStripPaintButton
            // 
            this.toolStripPaintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripPaintButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripPaintButton.Image")));
            this.toolStripPaintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripPaintButton.Name = "toolStripPaintButton";
            this.toolStripPaintButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripPaintButton.Text = "Paint Texture";
            this.toolStripPaintButton.Click += new System.EventHandler(this.modBtn_Click);
            // 
            // toolStripGridButton
            // 
            this.toolStripGridButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripGridButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripGridButton.Image")));
            this.toolStripGridButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripGridButton.Name = "toolStripGridButton";
            this.toolStripGridButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripGridButton.Text = "Grid View";
            this.toolStripGridButton.Click += new System.EventHandler(this.toolStripGridButton_Click);
            // 
            // toolStripListViewButton
            // 
            this.toolStripListViewButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripListViewButton.Image = ((System.Drawing.Image)(resources.GetObject("toolStripListViewButton.Image")));
            this.toolStripListViewButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripListViewButton.Name = "toolStripListViewButton";
            this.toolStripListViewButton.Size = new System.Drawing.Size(23, 22);
            this.toolStripListViewButton.Text = "List View";
            this.toolStripListViewButton.Click += new System.EventHandler(this.toolStripListViewButton_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Controls.Add(this.tabPage4);
            this.tabControl1.Location = new System.Drawing.Point(3, 6);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(478, 457);
            this.tabControl1.TabIndex = 31;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.label8);
            this.tabPage3.Controls.Add(this.pathTextBox);
            this.tabPage3.Controls.Add(this.label4);
            this.tabPage3.Controls.Add(this.SelTextureDimensionsLabel);
            this.tabPage3.Controls.Add(this.SelTextureMipTrackBar);
            this.tabPage3.Controls.Add(this.SelTextureMipLabel);
            this.tabPage3.Controls.Add(this.label3);
            this.tabPage3.Controls.Add(this.label2);
            this.tabPage3.Controls.Add(this.SelTextureDictionaryTextBox);
            this.tabPage3.Controls.Add(this.SelTextureNameTextBox);
            this.tabPage3.Controls.Add(this.SelDrawableTexturePictureBox);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(470, 431);
            this.tabPage3.TabIndex = 0;
            this.tabPage3.Text = "Texture";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 8);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(35, 12);
            this.label8.TabIndex = 48;
            this.label8.Text = "Name:";
            // 
            // pathTextBox
            // 
            this.pathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pathTextBox.Location = new System.Drawing.Point(48, 34);
            this.pathTextBox.Name = "pathTextBox";
            this.pathTextBox.ReadOnly = true;
            this.pathTextBox.Size = new System.Drawing.Size(416, 21);
            this.pathTextBox.TabIndex = 39;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 38);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(35, 12);
            this.label4.TabIndex = 38;
            this.label4.Text = "Path:";
            // 
            // SelTextureDimensionsLabel
            // 
            this.SelTextureDimensionsLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureDimensionsLabel.AutoSize = true;
            this.SelTextureDimensionsLabel.Location = new System.Drawing.Point(7, 390);
            this.SelTextureDimensionsLabel.Name = "SelTextureDimensionsLabel";
            this.SelTextureDimensionsLabel.Size = new System.Drawing.Size(11, 12);
            this.SelTextureDimensionsLabel.TabIndex = 37;
            this.SelTextureDimensionsLabel.Text = "-";
            // 
            // SelTextureMipTrackBar
            // 
            this.SelTextureMipTrackBar.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipTrackBar.AutoSize = false;
            this.SelTextureMipTrackBar.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.SelTextureMipTrackBar.LargeChange = 1;
            this.SelTextureMipTrackBar.Location = new System.Drawing.Point(59, 405);
            this.SelTextureMipTrackBar.Maximum = 0;
            this.SelTextureMipTrackBar.Name = "SelTextureMipTrackBar";
            this.SelTextureMipTrackBar.Size = new System.Drawing.Size(187, 29);
            this.SelTextureMipTrackBar.TabIndex = 36;
            this.SelTextureMipTrackBar.Scroll += new System.EventHandler(this.SelTextureMipTrackBar_Scroll);
            // 
            // SelTextureMipLabel
            // 
            this.SelTextureMipLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.SelTextureMipLabel.AutoSize = true;
            this.SelTextureMipLabel.Location = new System.Drawing.Point(40, 410);
            this.SelTextureMipLabel.Name = "SelTextureMipLabel";
            this.SelTextureMipLabel.Size = new System.Drawing.Size(11, 12);
            this.SelTextureMipLabel.TabIndex = 35;
            this.SelTextureMipLabel.Text = "0";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 410);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 34;
            this.label3.Text = "Mip:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(207, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 33;
            this.label2.Text = "Dictionary:";
            // 
            // SelTextureDictionaryTextBox
            // 
            this.SelTextureDictionaryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelTextureDictionaryTextBox.Location = new System.Drawing.Point(284, 6);
            this.SelTextureDictionaryTextBox.Name = "SelTextureDictionaryTextBox";
            this.SelTextureDictionaryTextBox.ReadOnly = true;
            this.SelTextureDictionaryTextBox.Size = new System.Drawing.Size(180, 21);
            this.SelTextureDictionaryTextBox.TabIndex = 32;
            // 
            // SelTextureNameTextBox
            // 
            this.SelTextureNameTextBox.Location = new System.Drawing.Point(48, 6);
            this.SelTextureNameTextBox.Name = "SelTextureNameTextBox";
            this.SelTextureNameTextBox.ReadOnly = true;
            this.SelTextureNameTextBox.Size = new System.Drawing.Size(150, 21);
            this.SelTextureNameTextBox.TabIndex = 31;
            // 
            // SelDrawableTexturePictureBox
            // 
            this.SelDrawableTexturePictureBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturePictureBox.BackColor = System.Drawing.Color.DarkGray;
            this.SelDrawableTexturePictureBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SelDrawableTexturePictureBox.Location = new System.Drawing.Point(3, 61);
            this.SelDrawableTexturePictureBox.Name = "SelDrawableTexturePictureBox";
            this.SelDrawableTexturePictureBox.Size = new System.Drawing.Size(461, 322);
            this.SelDrawableTexturePictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.SelDrawableTexturePictureBox.TabIndex = 29;
            this.SelDrawableTexturePictureBox.TabStop = false;
            this.SelDrawableTexturePictureBox.Paint += new System.Windows.Forms.PaintEventHandler(this.SelDrawableTexturePictureBox_Paint);
            this.SelDrawableTexturePictureBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.SelDrawableTexturePictureBox_MouseDown);
            this.SelDrawableTexturePictureBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.SelDrawableTexturePictureBox_MouseMove);
            this.SelDrawableTexturePictureBox.MouseUp += new System.Windows.Forms.MouseEventHandler(this.SelDrawableTexturePictureBox_MouseUp);
            this.SelDrawableTexturePictureBox.MouseWheel += new System.Windows.Forms.MouseEventHandler(this.SelDrawableTexturePictureBox_MouseWheel);
            // 
            // tabPage4
            // 
            this.tabPage4.Controls.Add(this.SelDrawableTexturePropertyGrid);
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(470, 431);
            this.tabPage4.TabIndex = 1;
            this.tabPage4.Text = "Info";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // SelDrawableTexturePropertyGrid
            // 
            this.SelDrawableTexturePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableTexturePropertyGrid.HelpVisible = false;
            this.SelDrawableTexturePropertyGrid.LineColor = System.Drawing.SystemColors.ControlDark;
            this.SelDrawableTexturePropertyGrid.Location = new System.Drawing.Point(3, 6);
            this.SelDrawableTexturePropertyGrid.Name = "SelDrawableTexturePropertyGrid";
            this.SelDrawableTexturePropertyGrid.Size = new System.Drawing.Size(461, 421);
            this.SelDrawableTexturePropertyGrid.TabIndex = 28;
            this.SelDrawableTexturePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionModelsTabPage
            // 
            this.SelectionModelsTabPage.Controls.Add(this.splitContainer1);
            this.SelectionModelsTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionModelsTabPage.Name = "SelectionModelsTabPage";
            this.SelectionModelsTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionModelsTabPage.TabIndex = 3;
            this.SelectionModelsTabPage.Text = "Models";
            this.SelectionModelsTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SelDrawableModelsTreeView);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.SelDrawableModelPropertyGrid);
            this.splitContainer1.Size = new System.Drawing.Size(817, 469);
            this.splitContainer1.SplitterDistance = 327;
            this.splitContainer1.TabIndex = 0;
            // 
            // SelDrawableModelsTreeView
            // 
            this.SelDrawableModelsTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableModelsTreeView.CheckBoxes = true;
            this.SelDrawableModelsTreeView.Location = new System.Drawing.Point(6, 6);
            this.SelDrawableModelsTreeView.Name = "SelDrawableModelsTreeView";
            this.SelDrawableModelsTreeView.Size = new System.Drawing.Size(318, 457);
            this.SelDrawableModelsTreeView.TabIndex = 0;
            this.SelDrawableModelsTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableModelsTreeView_AfterCheck);
            this.SelDrawableModelsTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SelDrawableModelsTreeView_AfterSelect);
            // 
            // SelDrawableModelPropertyGrid
            // 
            this.SelDrawableModelPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawableModelPropertyGrid.HelpVisible = false;
            this.SelDrawableModelPropertyGrid.Location = new System.Drawing.Point(3, 6);
            this.SelDrawableModelPropertyGrid.Name = "SelDrawableModelPropertyGrid";
            this.SelDrawableModelPropertyGrid.Size = new System.Drawing.Size(477, 457);
            this.SelDrawableModelPropertyGrid.TabIndex = 27;
            this.SelDrawableModelPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionEntityTabPage
            // 
            this.SelectionEntityTabPage.Controls.Add(this.SelEntityPropertyGrid);
            this.SelectionEntityTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionEntityTabPage.Name = "SelectionEntityTabPage";
            this.SelectionEntityTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionEntityTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionEntityTabPage.TabIndex = 0;
            this.SelectionEntityTabPage.Text = "Entity";
            this.SelectionEntityTabPage.UseVisualStyleBackColor = true;
            // 
            // SelEntityPropertyGrid
            // 
            this.SelEntityPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelEntityPropertyGrid.HelpVisible = false;
            this.SelEntityPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.SelEntityPropertyGrid.Name = "SelEntityPropertyGrid";
            this.SelEntityPropertyGrid.Size = new System.Drawing.Size(805, 457);
            this.SelEntityPropertyGrid.TabIndex = 25;
            this.SelEntityPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionArchetypeTabPage
            // 
            this.SelectionArchetypeTabPage.Controls.Add(this.SelArchetypePropertyGrid);
            this.SelectionArchetypeTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionArchetypeTabPage.Name = "SelectionArchetypeTabPage";
            this.SelectionArchetypeTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionArchetypeTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionArchetypeTabPage.TabIndex = 1;
            this.SelectionArchetypeTabPage.Text = "Archetype";
            this.SelectionArchetypeTabPage.UseVisualStyleBackColor = true;
            // 
            // SelArchetypePropertyGrid
            // 
            this.SelArchetypePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelArchetypePropertyGrid.HelpVisible = false;
            this.SelArchetypePropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.SelArchetypePropertyGrid.Name = "SelArchetypePropertyGrid";
            this.SelArchetypePropertyGrid.Size = new System.Drawing.Size(805, 457);
            this.SelArchetypePropertyGrid.TabIndex = 26;
            this.SelArchetypePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionDrawableTabPage
            // 
            this.SelectionDrawableTabPage.Controls.Add(this.SelDrawablePropertyGrid);
            this.SelectionDrawableTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionDrawableTabPage.Name = "SelectionDrawableTabPage";
            this.SelectionDrawableTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionDrawableTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionDrawableTabPage.TabIndex = 2;
            this.SelectionDrawableTabPage.Text = "Drawable";
            this.SelectionDrawableTabPage.UseVisualStyleBackColor = true;
            // 
            // SelDrawablePropertyGrid
            // 
            this.SelDrawablePropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelDrawablePropertyGrid.HelpVisible = false;
            this.SelDrawablePropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.SelDrawablePropertyGrid.Name = "SelDrawablePropertyGrid";
            this.SelDrawablePropertyGrid.Size = new System.Drawing.Size(805, 457);
            this.SelDrawablePropertyGrid.TabIndex = 28;
            this.SelDrawablePropertyGrid.ToolbarVisible = false;
            // 
            // SelectionExtensionTabPage
            // 
            this.SelectionExtensionTabPage.Controls.Add(this.SelExtensionPropertyGrid);
            this.SelectionExtensionTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionExtensionTabPage.Name = "SelectionExtensionTabPage";
            this.SelectionExtensionTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.SelectionExtensionTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionExtensionTabPage.TabIndex = 5;
            this.SelectionExtensionTabPage.Text = "Extension";
            this.SelectionExtensionTabPage.UseVisualStyleBackColor = true;
            // 
            // SelExtensionPropertyGrid
            // 
            this.SelExtensionPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelExtensionPropertyGrid.HelpVisible = false;
            this.SelExtensionPropertyGrid.Location = new System.Drawing.Point(6, 6);
            this.SelExtensionPropertyGrid.Name = "SelExtensionPropertyGrid";
            this.SelExtensionPropertyGrid.Size = new System.Drawing.Size(805, 457);
            this.SelExtensionPropertyGrid.TabIndex = 29;
            this.SelExtensionPropertyGrid.ToolbarVisible = false;
            // 
            // SelectionHierarchyTabPage
            // 
            this.SelectionHierarchyTabPage.Controls.Add(this.splitContainer3);
            this.SelectionHierarchyTabPage.Location = new System.Drawing.Point(4, 22);
            this.SelectionHierarchyTabPage.Name = "SelectionHierarchyTabPage";
            this.SelectionHierarchyTabPage.Size = new System.Drawing.Size(817, 469);
            this.SelectionHierarchyTabPage.TabIndex = 6;
            this.SelectionHierarchyTabPage.Text = "Hierarchy";
            this.SelectionHierarchyTabPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(0, 0);
            this.splitContainer3.Name = "splitContainer3";
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.HierarchyTreeView);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.HierarchyPropertyGrid);
            this.splitContainer3.Size = new System.Drawing.Size(817, 469);
            this.splitContainer3.SplitterDistance = 327;
            this.splitContainer3.TabIndex = 0;
            // 
            // HierarchyTreeView
            // 
            this.HierarchyTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HierarchyTreeView.FullRowSelect = true;
            this.HierarchyTreeView.HideSelection = false;
            this.HierarchyTreeView.Location = new System.Drawing.Point(6, 6);
            this.HierarchyTreeView.Name = "HierarchyTreeView";
            this.HierarchyTreeView.Size = new System.Drawing.Size(318, 457);
            this.HierarchyTreeView.TabIndex = 0;
            this.HierarchyTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.HierarchyTreeView_AfterSelect);
            // 
            // HierarchyPropertyGrid
            // 
            this.HierarchyPropertyGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.HierarchyPropertyGrid.HelpVisible = false;
            this.HierarchyPropertyGrid.Location = new System.Drawing.Point(3, 6);
            this.HierarchyPropertyGrid.Name = "HierarchyPropertyGrid";
            this.HierarchyPropertyGrid.Size = new System.Drawing.Size(477, 457);
            this.HierarchyPropertyGrid.TabIndex = 26;
            this.HierarchyPropertyGrid.ToolbarVisible = false;
            // 
            // MouseSelectCheckBox
            // 
            this.MouseSelectCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MouseSelectCheckBox.AutoSize = true;
            this.MouseSelectCheckBox.Location = new System.Drawing.Point(611, 13);
            this.MouseSelectCheckBox.Name = "MouseSelectCheckBox";
            this.MouseSelectCheckBox.Size = new System.Drawing.Size(180, 16);
            this.MouseSelectCheckBox.TabIndex = 26;
            this.MouseSelectCheckBox.Text = "Mouse select (right click)";
            this.MouseSelectCheckBox.UseVisualStyleBackColor = true;
            this.MouseSelectCheckBox.CheckedChanged += new System.EventHandler(this.MouseSelectCheckBox_CheckedChanged);
            // 
            // SelectionNameTextBox
            // 
            this.SelectionNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionNameTextBox.Location = new System.Drawing.Point(49, 11);
            this.SelectionNameTextBox.Name = "SelectionNameTextBox";
            this.SelectionNameTextBox.ReadOnly = true;
            this.SelectionNameTextBox.Size = new System.Drawing.Size(377, 21);
            this.SelectionNameTextBox.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 12);
            this.label1.TabIndex = 30;
            this.label1.Text = "Name:";
            // 
            // label25
            // 
            this.label25.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(444, 14);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(35, 12);
            this.label25.TabIndex = 32;
            this.label25.Text = "Mode:";
            // 
            // SelectionModeComboBox
            // 
            this.SelectionModeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.SelectionModeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SelectionModeComboBox.FormattingEnabled = true;
            this.SelectionModeComboBox.Items.AddRange(new object[] {
            "Entity",
            "Entity Extension",
            "Archetype Extension",
            "Time Cycle Modifier",
            "Car Generator",
            "Grass",
            "Water Quad",
            "Collision",
            "Nav Mesh",
            "Path",
            "Train Track",
            "Lod Lights",
            "Mlo Instance",
            "Scenario",
            "Audio",
            "Occlusion"});
            this.SelectionModeComboBox.Location = new System.Drawing.Point(481, 11);
            this.SelectionModeComboBox.Name = "SelectionModeComboBox";
            this.SelectionModeComboBox.Size = new System.Drawing.Size(121, 20);
            this.SelectionModeComboBox.TabIndex = 31;
            this.SelectionModeComboBox.SelectedIndexChanged += new System.EventHandler(this.SelectionModeComboBox_SelectedIndexChanged);
            // 
            // vsExtender
            // 
            this.vsExtender.DefaultRenderer = null;
            // 
            // sizeColumnHeader
            // 
            this.sizeColumnHeader.DisplayIndex = 2;
            this.sizeColumnHeader.Text = "Size";
            // 
            // WorldInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(847, 550);
            this.Controls.Add(this.label25);
            this.Controls.Add(this.SelectionModeComboBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.SelectionNameTextBox);
            this.Controls.Add(this.SelectionTabControl);
            this.Controls.Add(this.MouseSelectCheckBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WorldInfoForm";
            this.Text = "Info - CodeWalker by dexyfex";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WorldInfoForm_FormClosed);
            this.Load += new System.EventHandler(this.WorldInfoForm_Load);
            this.SelectionTabControl.ResumeLayout(false);
            this.SelectionTexturesTabPage.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SelTextureMipTrackBar)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.SelDrawableTexturePictureBox)).EndInit();
            this.tabPage4.ResumeLayout(false);
            this.SelectionModelsTabPage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.SelectionEntityTabPage.ResumeLayout(false);
            this.SelectionArchetypeTabPage.ResumeLayout(false);
            this.SelectionDrawableTabPage.ResumeLayout(false);
            this.SelectionExtensionTabPage.ResumeLayout(false);
            this.SelectionHierarchyTabPage.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl SelectionTabControl;
        private System.Windows.Forms.TabPage SelectionEntityTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelEntityPropertyGrid;
        private System.Windows.Forms.TabPage SelectionArchetypeTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelArchetypePropertyGrid;
        private System.Windows.Forms.TabPage SelectionDrawableTabPage;
        private System.Windows.Forms.CheckBox MouseSelectCheckBox;
        private System.Windows.Forms.TextBox SelectionNameTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage SelectionModelsTabPage;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private TreeViewFix SelDrawableModelsTreeView;
        private CodeWalker.WinForms.PropertyGridFix SelDrawableModelPropertyGrid;
        private System.Windows.Forms.TabPage SelectionTexturesTabPage;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PictureBox SelDrawableTexturePictureBox;
        private CodeWalker.WinForms.PropertyGridFix SelDrawableTexturePropertyGrid;
        private CodeWalker.WinForms.PropertyGridFix SelDrawablePropertyGrid;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TrackBar SelTextureMipTrackBar;
        private System.Windows.Forms.Label SelTextureMipLabel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox SelTextureDictionaryTextBox;
        private System.Windows.Forms.TextBox SelTextureNameTextBox;
        private System.Windows.Forms.Label SelTextureDimensionsLabel;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.ComboBox SelectionModeComboBox;
        private System.Windows.Forms.TabPage SelectionExtensionTabPage;
        private CodeWalker.WinForms.PropertyGridFix SelExtensionPropertyGrid;
        private System.Windows.Forms.TabPage SelectionHierarchyTabPage;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TreeView HierarchyTreeView;
        private PropertyGridFix HierarchyPropertyGrid;
        private System.Windows.Forms.SaveFileDialog SaveFileDialog;
        private System.Windows.Forms.FolderBrowserDialog FolderBrowserDialog;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox pathTextBox;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripSaveButton;
        private WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender vsExtender;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripButton toolStripSaveAllButton;
        private System.Windows.Forms.ToolStripButton toolStripPaintButton;
        private System.Windows.Forms.ToolStripButton toolStripGridButton;
        private System.Windows.Forms.ToolStripButton toolStripListViewButton;
        private Forms.AeroListView SelDrawableTexturesListView;
        private System.Windows.Forms.ColumnHeader nameColumn;
        private System.Windows.Forms.ColumnHeader InfoColumn;
        private System.Windows.Forms.ImageList smallImageList;
        private System.Windows.Forms.ImageList largeImageList;
        private System.Windows.Forms.ColumnHeader sizeColumnHeader;
    }
}