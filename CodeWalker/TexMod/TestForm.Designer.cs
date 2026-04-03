namespace CodeWalker.TexMod
{
    partial class TestForm
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
            this.d2DCanvas1 = new CodeWalker.D2DCanvas();
            this.SuspendLayout();
            // 
            // d2DCanvas1
            // 
            this.d2DCanvas1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.d2DCanvas1.Location = new System.Drawing.Point(0, 0);
            this.d2DCanvas1.Name = "d2DCanvas1";
            this.d2DCanvas1.Size = new System.Drawing.Size(800, 450);
            this.d2DCanvas1.TabIndex = 0;
            this.d2DCanvas1.Text = "d2DCanvas1";
            // 
            // TestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.d2DCanvas1);
            this.Name = "TestForm";
            this.Text = "TestForm";
            this.ResumeLayout(false);

        }

        #endregion

        private D2DCanvas d2DCanvas1;
    }
}