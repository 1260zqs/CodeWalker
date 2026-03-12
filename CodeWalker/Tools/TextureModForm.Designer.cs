using System.Windows.Forms;

namespace CodeWalker.Tools
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureModForm));
            this.vsExtender = new WeifenLuo.WinFormsUI.Docking.VisualStudioToolStripExtender(this.components);
            this.mainSplitContainer = new System.Windows.Forms.SplitContainer();
            this.topSplitContainer = new System.Windows.Forms.SplitContainer();
            this.projectListPanel = new System.Windows.Forms.Panel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.replacementListView = new System.Windows.Forms.ListView();
            this.imageContainer = new System.Windows.Forms.Panel();
            this.imageTabControl = new System.Windows.Forms.TabControl();
            this.previewTabPage = new System.Windows.Forms.TabPage();
            this.textureTabPage = new System.Windows.Forms.TabPage();
            this.panel1 = new System.Windows.Forms.Panel();
            this.bottomSplitContainer = new System.Windows.Forms.SplitContainer();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.panel2 = new System.Windows.Forms.Panel();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton6 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripButton();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.panel3 = new System.Windows.Forms.Panel();
            this.propertyGridFix1 = new CodeWalker.WinForms.PropertyGridFix();
            this.sourceRefListView = new System.Windows.Forms.ListView();
            this.listView2 = new System.Windows.Forms.ListView();
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
            this.SuspendLayout();
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
            this.projectListPanel.Controls.Add(this.replacementListView);
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
            this.toolStripButton2,
            this.toolStripButton3});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip1.Size = new System.Drawing.Size(312, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton1.Text = "toolStripButton1";
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton2.Text = "toolStripButton2";
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton3.Text = "toolStripButton3";
            // 
            // replacementListView
            // 
            this.replacementListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.replacementListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.replacementListView.FullRowSelect = true;
            this.replacementListView.GridLines = true;
            this.replacementListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.replacementListView.HideSelection = false;
            this.replacementListView.LabelEdit = true;
            this.replacementListView.Location = new System.Drawing.Point(0, 25);
            this.replacementListView.Margin = new System.Windows.Forms.Padding(0);
            this.replacementListView.MultiSelect = false;
            this.replacementListView.Name = "replacementListView";
            this.replacementListView.Size = new System.Drawing.Size(312, 310);
            this.replacementListView.TabIndex = 0;
            this.replacementListView.UseCompatibleStateImageBehavior = false;
            this.replacementListView.View = System.Windows.Forms.View.Details;
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
            this.imageTabControl.Controls.Add(this.previewTabPage);
            this.imageTabControl.Controls.Add(this.textureTabPage);
            this.imageTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.imageTabControl.Location = new System.Drawing.Point(0, 0);
            this.imageTabControl.Margin = new System.Windows.Forms.Padding(0);
            this.imageTabControl.Name = "imageTabControl";
            this.imageTabControl.SelectedIndex = 0;
            this.imageTabControl.Size = new System.Drawing.Size(405, 338);
            this.imageTabControl.TabIndex = 0;
            // 
            // previewTabPage
            // 
            this.previewTabPage.Location = new System.Drawing.Point(4, 22);
            this.previewTabPage.Name = "previewTabPage";
            this.previewTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.previewTabPage.Size = new System.Drawing.Size(397, 312);
            this.previewTabPage.TabIndex = 0;
            this.previewTabPage.Text = "preview";
            this.previewTabPage.UseVisualStyleBackColor = true;
            // 
            // textureTabPage
            // 
            this.textureTabPage.Location = new System.Drawing.Point(4, 22);
            this.textureTabPage.Name = "textureTabPage";
            this.textureTabPage.Padding = new System.Windows.Forms.Padding(3);
            this.textureTabPage.Size = new System.Drawing.Size(397, 312);
            this.textureTabPage.TabIndex = 1;
            this.textureTabPage.Text = "texture";
            this.textureTabPage.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Location = new System.Drawing.Point(414, 6);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(221, 337);
            this.panel1.TabIndex = 0;
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
            this.tabPage1.Text = "Reference";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.sourceRefListView);
            this.panel2.Controls.Add(this.toolStrip2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(617, 219);
            this.panel2.TabIndex = 0;
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
            this.toolStripButton6,
            this.toolStripButton7});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(0);
            this.toolStrip2.Size = new System.Drawing.Size(615, 25);
            this.toolStrip2.TabIndex = 0;
            this.toolStrip2.Text = "toolStrip2";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton4.Text = "toolStripButton4";
            // 
            // toolStripButton5
            // 
            this.toolStripButton5.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton5.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton5.Image")));
            this.toolStripButton5.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton5.Name = "toolStripButton5";
            this.toolStripButton5.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton5.Text = "toolStripButton5";
            // 
            // toolStripButton6
            // 
            this.toolStripButton6.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton6.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton6.Image")));
            this.toolStripButton6.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton6.Name = "toolStripButton6";
            this.toolStripButton6.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton6.Text = "toolStripButton6";
            // 
            // toolStripButton7
            // 
            this.toolStripButton7.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton7.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton7.Image")));
            this.toolStripButton7.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton7.Name = "toolStripButton7";
            this.toolStripButton7.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton7.Text = "toolStripButton7";
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
            // propertyGridFix1
            // 
            this.propertyGridFix1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridFix1.HelpVisible = false;
            this.propertyGridFix1.Location = new System.Drawing.Point(3, 0);
            this.propertyGridFix1.Name = "propertyGridFix1";
            this.propertyGridFix1.Size = new System.Drawing.Size(318, 251);
            this.propertyGridFix1.TabIndex = 0;
            this.propertyGridFix1.ToolbarVisible = false;
            // 
            // sourceRefListView
            // 
            this.sourceRefListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sourceRefListView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.sourceRefListView.FullRowSelect = true;
            this.sourceRefListView.GridLines = true;
            this.sourceRefListView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.sourceRefListView.HideSelection = false;
            this.sourceRefListView.Location = new System.Drawing.Point(0, 25);
            this.sourceRefListView.Name = "sourceRefListView";
            this.sourceRefListView.Size = new System.Drawing.Size(615, 192);
            this.sourceRefListView.TabIndex = 1;
            this.sourceRefListView.UseCompatibleStateImageBehavior = false;
            this.sourceRefListView.View = System.Windows.Forms.View.Details;
            // 
            // listView2
            // 
            this.listView2.BorderStyle = System.Windows.Forms.BorderStyle.None;
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
        private ListView replacementListView;
        private TabControl imageTabControl;
        private TabPage previewTabPage;
        private TabPage textureTabPage;
        private ToolStrip toolStrip1;
        private ToolStripButton toolStripButton1;
        private ToolStripButton toolStripButton2;
        private ToolStripButton toolStripButton3;
        private Panel projectListPanel;
        private TabControl tabControl2;
        private TabPage tabPage1;
        private TabPage tabPage2;
        private Panel panel2;
        private ToolStrip toolStrip2;
        private ToolStripButton toolStripButton4;
        private ToolStripButton toolStripButton5;
        private ToolStripButton toolStripButton6;
        private ToolStripButton toolStripButton7;
        private Panel panel3;
        private ListView sourceRefListView;
        private ListView listView2;
    }
}