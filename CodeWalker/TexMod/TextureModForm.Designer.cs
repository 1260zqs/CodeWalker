using System.Windows.Forms;

namespace CodeWalker.TexMod
{
    partial class TextureModForm
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
            System.Windows.Forms.ColumnHeader Texture;
            System.Windows.Forms.ColumnHeader Tag;
            System.Windows.Forms.ColumnHeader Comment;
            System.Windows.Forms.ColumnHeader columnHeader1;
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureModForm));
            this.vsExtender = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(this.components);
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.topSplitContainer = new System.Windows.Forms.SplitContainer();
            this.projectListPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.saveProjectBtn = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton8 = new System.Windows.Forms.ToolStripButton();
            this.repViewModeBtn = new System.Windows.Forms.ToolStripDropDownButton();
            this.modListView = new System.Windows.Forms.ListView();
            this.imageContainer = new System.Windows.Forms.Panel();
            this.imageTabControl = new System.Windows.Forms.TabControl();
            this.gameTextureTabPage = new System.Windows.Forms.TabPage();
            this.modTextureTabPage = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.button7 = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button5 = new System.Windows.Forms.Button();
            this.button6 = new System.Windows.Forms.Button();
            this.button4 = new System.Windows.Forms.Button();
            this.button3 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.numericUpDown2 = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.rectLabel = new System.Windows.Forms.Label();
            this.rectBoxX = new System.Windows.Forms.NumericUpDown();
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.rectBoxY = new System.Windows.Forms.NumericUpDown();
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.rectBoxH = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.rectBoxW = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.textureMappingView = new System.Windows.Forms.ListView();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripDropDownButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.listView2 = new System.Windows.Forms.ListView();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.treeView = new System.Windows.Forms.TreeView();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gameTextureCanvas = new CodeWalker.D2DCanvas();
            this.modTextureCanvas = new CodeWalker.D2DCanvas();
            this.propertyGridFix1 = new CodeWalker.WinForms.PropertyGridFix();
            Texture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Tag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).BeginInit();
            this.mainSplitContainer.Panel1.SuspendLayout();
            this.mainSplitContainer.Panel2.SuspendLayout();
            this.mainSplitContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).BeginInit();
            this.topSplitContainer.Panel1.SuspendLayout();
            this.topSplitContainer.Panel2.SuspendLayout();
            this.topSplitContainer.SuspendLayout();
            this.projectListPanel.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.imageContainer.SuspendLayout();
            this.imageTabControl.SuspendLayout();
            this.gameTextureTabPage.SuspendLayout();
            this.modTextureTabPage.SuspendLayout();
            this.panel1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxW)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).BeginInit();
            this.bottomSplitContainer.Panel1.SuspendLayout();
            this.bottomSplitContainer.Panel2.SuspendLayout();
            this.bottomSplitContainer.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPage3.SuspendLayout();
            this.SuspendLayout();
            // 
            // Texture
            // 
            Texture.Text = "Texture";
            Texture.Width = 200;
            // 
            // Tag
            // 
            Tag.Text = "Tag";
            Tag.Width = 80;
            // 
            // Comment
            // 
            Comment.Text = "Comment";
            Comment.Width = 320;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Project";
            columnHeader1.Width = 312;
            // 
            // vsExtender
            // 
            this.vsExtender.DefaultRenderer = null;
            // 
            // mainSplitContainer
            // 
            this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.mainSplitContainer.Location = new System.Drawing.Point(0, 0);
            this.mainSplitContainer.Name = "mainSplitContainer";
            this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // mainSplitContainer.Panel1
            // 
            this.mainSplitContainer.Panel1.Controls.Add(this.topSplitContainer);
            this.mainSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(6);
            // 
            // mainSplitContainer.Panel2
            // 
            this.mainSplitContainer.Panel2.Controls.Add(this.bottomSplitContainer);
            this.mainSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(6);
            this.mainSplitContainer.Size = new System.Drawing.Size(983, 628);
            this.mainSplitContainer.SplitterDistance = 355;
            this.mainSplitContainer.TabIndex = 4;
            // 
            // topSplitContainer
            // 
            this.topSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.topSplitContainer.Location = new System.Drawing.Point(6, 6);
            this.topSplitContainer.Name = "topSplitContainer";
            // 
            // topSplitContainer.Panel1
            // 
            this.topSplitContainer.Panel1.Controls.Add(this.projectListPanel);
            this.topSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(6, 6, 3, 0);
            // 
            // topSplitContainer.Panel2
            // 
            this.topSplitContainer.Panel2.Controls.Add(this.imageContainer);
            this.topSplitContainer.Panel2.Controls.Add(this.panel1);
            this.topSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 6, 6, 0);
            this.topSplitContainer.Size = new System.Drawing.Size(971, 343);
            this.topSplitContainer.SplitterDistance = 323;
            this.topSplitContainer.TabIndex = 0;
            // 
            // projectListPanel
            // 
            this.projectListPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.projectListPanel.Controls.Add(this.toolStrip1);
            this.projectListPanel.Controls.Add(this.modListView);
            this.projectListPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.projectListPanel.Location = new System.Drawing.Point(6, 6);
            this.projectListPanel.Margin = new System.Windows.Forms.Padding(0);
            this.projectListPanel.Name = "projectListPanel";
            this.projectListPanel.Size = new System.Drawing.Size(314, 337);
            this.projectListPanel.TabIndex = 2;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton1,
            this.toolStripButton3,
            this.saveProjectBtn,
            this.toolStripButton2,
            this.toolStripButton8,
            this.repViewModeBtn});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.toolStrip1.Size = new System.Drawing.Size(312, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = global::CodeWalker.Properties.Resources._103;
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.ToolTipText = "New";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = global::CodeWalker.Properties.Resources._104;
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.ToolTipText = "Delete";
            this.toolStripButton3.Click += new System.EventHandler(this.toolStripButton3_Click);
            // 
            // saveProjectBtn
            // 
            this.saveProjectBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveProjectBtn.Image = ((System.Drawing.Image)(resources.GetObject("saveProjectBtn.Image")));
            this.saveProjectBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveProjectBtn.Name = "saveProjectBtn";
            this.saveProjectBtn.Size = new System.Drawing.Size(23, 22);
            this.saveProjectBtn.ToolTipText = "Save";
            this.saveProjectBtn.Click += new System.EventHandler(this.saveProjectBtn_Click);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.ToolTipText = "Build Mod";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // toolStripButton8
            // 
            this.toolStripButton8.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton8.Image = global::CodeWalker.Properties.Resources.box_zipper;
            this.toolStripButton8.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton8.Name = "toolStripButton8";
            this.toolStripButton8.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton8.ToolTipText = "Pack OIV";
            this.toolStripButton8.Click += new System.EventHandler(this.toolStripButton8_Click);
            // 
            // repViewModeBtn
            // 
            this.repViewModeBtn.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.repViewModeBtn.Image = ((System.Drawing.Image)(resources.GetObject("repViewModeBtn.Image")));
            this.repViewModeBtn.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.repViewModeBtn.Name = "repViewModeBtn";
            this.repViewModeBtn.Size = new System.Drawing.Size(29, 22);
            this.repViewModeBtn.ToolTipText = "View Mode";
            // 
            // modListView
            // 
            this.modListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.modListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.modListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            this.modListView.FullRowSelect = true;
            this.modListView.GridLines = true;
            this.modListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.modListView.HideSelection = false;
            this.modListView.LabelEdit = true;
            this.modListView.Location = new System.Drawing.Point(0, 25);
            this.modListView.Margin = new System.Windows.Forms.Padding(0);
            this.modListView.MultiSelect = false;
            this.modListView.Name = "modListView";
            this.modListView.Size = new System.Drawing.Size(312, 310);
            this.modListView.TabIndex = 0;
            this.modListView.UseCompatibleStateImageBehavior = false;
            this.modListView.View = System.Windows.Forms.View.Details;
            this.modListView.VirtualMode = true;
            this.modListView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.modListView_AfterLabelEdit);
            this.modListView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.modListView_RetrieveVirtualItem);
            this.modListView.SelectedIndexChanged += new System.EventHandler(this.modListView_SelectedIndexChanged);
            // 
            // imageContainer
            // 
            this.imageContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.imageContainer.Controls.Add(this.imageTabControl);
            this.imageContainer.Location = new System.Drawing.Point(3, 6);
            this.imageContainer.Margin = new System.Windows.Forms.Padding(0);
            this.imageContainer.Name = "imageContainer";
            this.imageContainer.Size = new System.Drawing.Size(405, 338);
            this.imageContainer.TabIndex = 0;
            // 
            // imageTabControl
            // 
            this.imageTabControl.Controls.Add(this.gameTextureTabPage);
            this.imageTabControl.Controls.Add(this.modTextureTabPage);
            this.imageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageTabControl.Location = new System.Drawing.Point(0, 0);
            this.imageTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.imageTabControl.Multiline = true;
            this.imageTabControl.Name = "imageTabControl";
            this.imageTabControl.Padding = new System.Drawing.Point(6, 6);
            this.imageTabControl.SelectedIndex = 0;
            this.imageTabControl.Size = new System.Drawing.Size(405, 338);
            this.imageTabControl.TabIndex = 0;
            this.imageTabControl.Selected += new System.Windows.Forms.TabControlEventHandler(this.imageTabControl_Selected);
            // 
            // gameTextureTabPage
            // 
            this.gameTextureTabPage.Controls.Add(this.gameTextureCanvas);
            this.gameTextureTabPage.Location = new System.Drawing.Point(4, 28);
            this.gameTextureTabPage.Name = "gameTextureTabPage";
            this.gameTextureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.gameTextureTabPage.Size = new System.Drawing.Size(397, 306);
            this.gameTextureTabPage.TabIndex = 0;
            this.gameTextureTabPage.Text = "Preview";
            this.gameTextureTabPage.UseVisualStyleBackColor = true;
            // 
            // modTextureTabPage
            // 
            this.modTextureTabPage.Controls.Add(this.modTextureCanvas);
            this.modTextureTabPage.Location = new System.Drawing.Point(4, 28);
            this.modTextureTabPage.Name = "modTextureTabPage";
            this.modTextureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.modTextureTabPage.Size = new System.Drawing.Size(397, 306);
            this.modTextureTabPage.TabIndex = 1;
            this.modTextureTabPage.Text = "Texture";
            this.modTextureTabPage.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.groupBox3);
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.groupBox2);
            this.panel1.Location = new System.Drawing.Point(414, 6);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(221, 337);
            this.panel1.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.checkBox3);
            this.groupBox3.Controls.Add(this.button7);
            this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox3.Location = new System.Drawing.Point(0, 256);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(219, 100);
            this.groupBox3.TabIndex = 60;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "groupBox3";
            // 
            // checkBox3
            // 
            this.checkBox3.AutoSize = true;
            this.checkBox3.Location = new System.Drawing.Point(5, 52);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(114, 16);
            this.checkBox3.TabIndex = 1;
            this.checkBox3.Text = "draw test color";
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // button7
            // 
            this.button7.Location = new System.Drawing.Point(4, 21);
            this.button7.Name = "button7";
            this.button7.Size = new System.Drawing.Size(105, 23);
            this.button7.TabIndex = 0;
            this.button7.Text = "update viewport";
            this.button7.UseVisualStyleBackColor = true;
            this.button7.Click += new System.EventHandler(this.button7_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button5);
            this.groupBox1.Controls.Add(this.button6);
            this.groupBox1.Controls.Add(this.button4);
            this.groupBox1.Controls.Add(this.button3);
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.button1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.numericUpDown1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.numericUpDown2);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox1.Location = new System.Drawing.Point(0, 112);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(219, 144);
            this.groupBox1.TabIndex = 58;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Target rect";
            // 
            // button5
            // 
            this.button5.Location = new System.Drawing.Point(108, 111);
            this.button5.Name = "button5";
            this.button5.Size = new System.Drawing.Size(99, 23);
            this.button5.TabIndex = 64;
            this.button5.Text = "clip by height";
            this.button5.UseVisualStyleBackColor = true;
            this.button5.Click += new System.EventHandler(this.button5_Click);
            // 
            // button6
            // 
            this.button6.Location = new System.Drawing.Point(4, 111);
            this.button6.Name = "button6";
            this.button6.Size = new System.Drawing.Size(93, 23);
            this.button6.TabIndex = 63;
            this.button6.Text = "clip by width";
            this.button6.UseVisualStyleBackColor = true;
            this.button6.Click += new System.EventHandler(this.button6_Click);
            // 
            // button4
            // 
            this.button4.Location = new System.Drawing.Point(108, 80);
            this.button4.Name = "button4";
            this.button4.Size = new System.Drawing.Size(99, 23);
            this.button4.TabIndex = 62;
            this.button4.Text = "fill by height";
            this.button4.UseVisualStyleBackColor = true;
            this.button4.Click += new System.EventHandler(this.button4_Click);
            // 
            // button3
            // 
            this.button3.Location = new System.Drawing.Point(4, 80);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(93, 23);
            this.button3.TabIndex = 61;
            this.button3.Text = "fill by width";
            this.button3.UseVisualStyleBackColor = true;
            this.button3.Click += new System.EventHandler(this.button3_Click);
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(147, 48);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(60, 23);
            this.button2.TabIndex = 60;
            this.button2.Text = "set";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(147, 21);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(60, 23);
            this.button1.TabIndex = 59;
            this.button1.Text = "set";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 55;
            this.label1.Text = "width:";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(60, 21);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.ReadOnly = true;
            this.numericUpDown1.Size = new System.Drawing.Size(80, 21);
            this.numericUpDown1.TabIndex = 56;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(5, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 12);
            this.label2.TabIndex = 57;
            this.label2.Text = "height:";
            // 
            // numericUpDown2
            // 
            this.numericUpDown2.Location = new System.Drawing.Point(60, 50);
            this.numericUpDown2.Name = "numericUpDown2";
            this.numericUpDown2.ReadOnly = true;
            this.numericUpDown2.Size = new System.Drawing.Size(80, 21);
            this.numericUpDown2.TabIndex = 58;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.rectLabel);
            this.groupBox2.Controls.Add(this.rectBoxX);
            this.groupBox2.Controls.Add(this.checkBox2);
            this.groupBox2.Controls.Add(this.rectBoxY);
            this.groupBox2.Controls.Add(this.checkBox1);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.rectBoxH);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.rectBoxW);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBox2.Location = new System.Drawing.Point(0, 0);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(219, 112);
            this.groupBox2.TabIndex = 59;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Sources Rect";
            // 
            // rectLabel
            // 
            this.rectLabel.AutoSize = true;
            this.rectLabel.Location = new System.Drawing.Point(6, 27);
            this.rectLabel.Name = "rectLabel";
            this.rectLabel.Size = new System.Drawing.Size(17, 12);
            this.rectLabel.TabIndex = 48;
            this.rectLabel.Text = "x:";
            // 
            // rectBoxX
            // 
            this.rectBoxX.Location = new System.Drawing.Point(25, 24);
            this.rectBoxX.Name = "rectBoxX";
            this.rectBoxX.Size = new System.Drawing.Size(80, 21);
            this.rectBoxX.TabIndex = 49;
            this.rectBoxX.ValueChanged += new System.EventHandler(this.rectBoxX_ValueChanged);
            // 
            // checkBox2
            // 
            this.checkBox2.AutoSize = true;
            this.checkBox2.Location = new System.Drawing.Point(78, 79);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(54, 16);
            this.checkBox2.TabIndex = 57;
            this.checkBox2.Text = "solid";
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // rectBoxY
            // 
            this.rectBoxY.Location = new System.Drawing.Point(140, 24);
            this.rectBoxY.Name = "rectBoxY";
            this.rectBoxY.Size = new System.Drawing.Size(80, 21);
            this.rectBoxY.TabIndex = 50;
            this.rectBoxY.ValueChanged += new System.EventHandler(this.rectBoxY_ValueChanged);
            // 
            // checkBox1
            // 
            this.checkBox1.AutoSize = true;
            this.checkBox1.Location = new System.Drawing.Point(8, 79);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(54, 16);
            this.checkBox1.TabIndex = 56;
            this.checkBox1.Text = "paint";
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 47);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 51;
            this.label5.Text = "w:";
            // 
            // rectBoxH
            // 
            this.rectBoxH.Location = new System.Drawing.Point(140, 44);
            this.rectBoxH.Name = "rectBoxH";
            this.rectBoxH.Size = new System.Drawing.Size(80, 21);
            this.rectBoxH.TabIndex = 55;
            this.rectBoxH.ValueChanged += new System.EventHandler(this.rectBoxH_ValueChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(121, 27);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 52;
            this.label6.Text = "y:";
            // 
            // rectBoxW
            // 
            this.rectBoxW.Location = new System.Drawing.Point(25, 44);
            this.rectBoxW.Name = "rectBoxW";
            this.rectBoxW.Size = new System.Drawing.Size(80, 21);
            this.rectBoxW.TabIndex = 54;
            this.rectBoxW.ValueChanged += new System.EventHandler(this.rectBoxW_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(121, 47);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 53;
            this.label7.Text = "h:";
            // 
            // bottomSplitContainer
            // 
            this.bottomSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.bottomSplitContainer.Location = new System.Drawing.Point(6, 6);
            this.bottomSplitContainer.Name = "bottomSplitContainer";
            // 
            // bottomSplitContainer.Panel1
            // 
            this.bottomSplitContainer.Panel1.Controls.Add(this.tabControl2);
            this.bottomSplitContainer.Panel1.Padding = new System.Windows.Forms.Padding(6, 0, 3, 6);
            // 
            // bottomSplitContainer.Panel2
            // 
            this.bottomSplitContainer.Panel2.Controls.Add(this.propertyGridFix1);
            this.bottomSplitContainer.Panel2.Padding = new System.Windows.Forms.Padding(3, 0, 6, 6);
            this.bottomSplitContainer.Size = new System.Drawing.Size(971, 257);
            this.bottomSplitContainer.SplitterDistance = 640;
            this.bottomSplitContainer.TabIndex = 0;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Controls.Add(this.tabPage3);
            this.tabControl2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl2.Location = new System.Drawing.Point(6, 0);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(631, 251);
            this.tabControl2.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.panel2);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(623, 225);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Replacements";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.textureMappingView);
            this.panel2.Controls.Add(this.toolStrip2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(617, 219);
            this.panel2.TabIndex = 0;
            // 
            // textureMappingView
            // 
            this.textureMappingView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textureMappingView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textureMappingView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            Texture,
            Tag,
            Comment});
            this.textureMappingView.FullRowSelect = true;
            this.textureMappingView.GridLines = true;
            this.textureMappingView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.textureMappingView.HideSelection = false;
            this.textureMappingView.Location = new System.Drawing.Point(0, 25);
            this.textureMappingView.Name = "textureMappingView";
            this.textureMappingView.Size = new System.Drawing.Size(615, 192);
            this.textureMappingView.TabIndex = 1;
            this.textureMappingView.UseCompatibleStateImageBehavior = false;
            this.textureMappingView.View = System.Windows.Forms.View.Details;
            this.textureMappingView.VirtualMode = true;
            this.textureMappingView.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.replacementListView_RetrieveVirtualItem);
            this.textureMappingView.SelectedIndexChanged += new System.EventHandler(this.replacementListView_SelectedIndexChanged);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStrip2.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripButton7});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(1, 0, 0, 0);
            this.toolStrip2.Size = new System.Drawing.Size(615, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = global::CodeWalker.Properties.Resources._103;
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.ToolTipText = "New";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = global::CodeWalker.Properties.Resources._104;
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.ToolTipText = "Delete";
            this.toolStripButton5.Click += new System.EventHandler(this.toolStripButton5_Click);
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
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.panel3);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(623, 225);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // panel3
            // 
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.listView2);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(617, 219);
            this.panel3.TabIndex = 0;
            // 
            // listView2
            // 
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView2.FullRowSelect = true;
            this.listView2.GridLines = true;
            this.listView2.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(0, 0);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(615, 217);
            this.listView2.TabIndex = 0;
            this.listView2.UseCompatibleStateImageBehavior = false;
            this.listView2.View = System.Windows.Forms.View.Details;
            // 
            // tabPage3
            // 
            this.tabPage3.Controls.Add(this.treeView);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(623, 225);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "tabPage3";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // treeView
            // 
            this.treeView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView.FullRowSelect = true;
            this.treeView.HideSelection = false;
            this.treeView.Indent = 20;
            this.treeView.ItemHeight = 20;
            this.treeView.Location = new System.Drawing.Point(3, 3);
            this.treeView.Name = "treeView";
            this.treeView.ShowLines = false;
            this.treeView.Size = new System.Drawing.Size(617, 219);
            this.treeView.TabIndex = 0;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = resources.GetString("openFileDialog1.Filter");
            this.openFileDialog1.RestoreDirectory = true;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // gameTextureCanvas
            // 
            this.gameTextureCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gameTextureCanvas.Location = new System.Drawing.Point(3, 3);
            this.gameTextureCanvas.Name = "gameTextureCanvas";
            this.gameTextureCanvas.Size = new System.Drawing.Size(391, 300);
            this.gameTextureCanvas.TabIndex = 0;
            this.gameTextureCanvas.Text = "d2DCanvas1";
            // 
            // modTextureCanvas
            // 
            this.modTextureCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.modTextureCanvas.Location = new System.Drawing.Point(3, 3);
            this.modTextureCanvas.Name = "modTextureCanvas";
            this.modTextureCanvas.Size = new System.Drawing.Size(391, 300);
            this.modTextureCanvas.TabIndex = 0;
            this.modTextureCanvas.Text = "d2DCanvas1";
            // 
            // propertyGridFix1
            // 
            this.propertyGridFix1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridFix1.HelpVisible = false;
            this.propertyGridFix1.Location = new System.Drawing.Point(3, 0);
            this.propertyGridFix1.Name = "propertyGridFix1";
            this.propertyGridFix1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGridFix1.Size = new System.Drawing.Size(318, 251);
            this.propertyGridFix1.TabIndex = 0;
            this.propertyGridFix1.ToolbarVisible = false;
            // 
            // TextureModForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(983, 628);
            this.Controls.Add(this.mainSplitContainer);
            this.Name = "TextureModForm";
            this.Text = "TextureModForm";
            this.mainSplitContainer.Panel1.ResumeLayout(false);
            this.mainSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.mainSplitContainer)).EndInit();
            this.mainSplitContainer.ResumeLayout(false);
            this.topSplitContainer.Panel1.ResumeLayout(false);
            this.topSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.topSplitContainer)).EndInit();
            this.topSplitContainer.ResumeLayout(false);
            this.projectListPanel.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.imageContainer.ResumeLayout(false);
            this.imageTabControl.ResumeLayout(false);
            this.gameTextureTabPage.ResumeLayout(false);
            this.modTextureTabPage.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxW)).EndInit();
            this.bottomSplitContainer.Panel1.ResumeLayout(false);
            this.bottomSplitContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bottomSplitContainer)).EndInit();
            this.bottomSplitContainer.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender vsExtender;
        private System.Windows.Forms.SplitContainer mainSplitContainer;
        private SplitContainer topSplitContainer;
        private SplitContainer bottomSplitContainer;
        private Panel panel1;
        private Panel imageContainer;
        private WinForms.PropertyGridFix propertyGridFix1;
        private ListView modListView;
        private TabControl imageTabControl;
        private TabPage gameTextureTabPage;
        private TabPage modTextureTabPage;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton saveProjectBtn;
        private Panel projectListPanel;
        private TabControl tabControl2;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Panel panel2;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButton4;
        private ToolStripButton toolStripButton5;
        private Panel panel3;
        private ListView textureMappingView;
        private ListView listView2;
        private ToolStripDropDownButton repViewModeBtn;
        private ToolStripDropDownButton toolStripButton7;
        private OpenFileDialog openFileDialog1;
        private NumericUpDown rectBoxH;
        private NumericUpDown rectBoxW;
        private Label label7;
        private Label label6;
        private Label label5;
        private NumericUpDown rectBoxY;
        private NumericUpDown rectBoxX;
        private Label rectLabel;
        private CheckBox checkBox2;
        private CheckBox checkBox1;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private Label label1;
        private NumericUpDown numericUpDown1;
        private Label label2;
        private NumericUpDown numericUpDown2;
        private Button button2;
        private Button button1;
        private Button button4;
        private Button button3;
        private Button button5;
        private Button button6;
        private GroupBox groupBox3;
        private CheckBox checkBox3;
        private Button button7;
        private D2DCanvas gameTextureCanvas;
        private D2DCanvas modTextureCanvas;
        private Timer timer1;
        private ToolStripButton toolStripButton3;
        private ToolStripButton toolStripButton8;
        private TabPage tabPage3;
        private TreeView treeView;
    }
}