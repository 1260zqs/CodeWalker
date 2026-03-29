namespace CodeWalker.TexMod
{
    partial class TextureModPropertyControl
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
            this.propertyGridFix1 = new CodeWalker.WinForms.PropertyGridFix();
            this.SuspendLayout();
            // 
            // propertyGridFix1
            // 
            this.propertyGridFix1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGridFix1.HelpVisible = false;
            this.propertyGridFix1.Location = new System.Drawing.Point(0, 0);
            this.propertyGridFix1.Name = "propertyGridFix1";
            this.propertyGridFix1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGridFix1.Size = new System.Drawing.Size(284, 261);
            this.propertyGridFix1.TabIndex = 1;
            this.propertyGridFix1.ToolbarVisible = false;
            // 
            // TextureModPropertyControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.propertyGridFix1);
            this.Name = "TextureModPropertyControl";
            this.ResumeLayout(false);

        }

        #endregion

        private WinForms.PropertyGridFix propertyGridFix1;
    }
}
