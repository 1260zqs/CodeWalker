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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextureModMappingControl));
            System.Windows.Forms.ColumnHeader Texture;
            System.Windows.Forms.ColumnHeader Tag;
            System.Windows.Forms.ColumnHeader Comment;
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton5 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton7 = new System.Windows.Forms.ToolStripDropDownButton();
            this.textureMappingView = new System.Windows.Forms.ListView();
            Texture = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Tag = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            Comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip2.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip2
            // 
            this.toolStrip2.AutoSize = false;
            this.toolStrip2.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.toolStrip2.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton4,
            this.toolStripButton5,
            this.toolStripButton7});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Padding = new System.Windows.Forms.Padding(1, 0, 1, 0);
            this.toolStrip2.Size = new System.Drawing.Size(284, 25);
            this.toolStrip2.TabIndex = 1;
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
            // textureMappingView
            // 
            this.textureMappingView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textureMappingView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            Texture,
            Tag,
            Comment});
            this.textureMappingView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textureMappingView.FullRowSelect = true;
            this.textureMappingView.GridLines = true;
            this.textureMappingView.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.textureMappingView.HideSelection = false;
            this.textureMappingView.Location = new System.Drawing.Point(0, 25);
            this.textureMappingView.Name = "textureMappingView";
            this.textureMappingView.Size = new System.Drawing.Size(284, 236);
            this.textureMappingView.TabIndex = 2;
            this.textureMappingView.UseCompatibleStateImageBehavior = false;
            this.textureMappingView.View = System.Windows.Forms.View.Details;
            this.textureMappingView.VirtualMode = true;
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
            // TextureModMappingControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.textureMappingView);
            this.Controls.Add(this.toolStrip2);
            this.Name = "TextureModMappingControl";
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripButton toolStripButton5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripButton7;
        private System.Windows.Forms.ListView textureMappingView;
    }
}
