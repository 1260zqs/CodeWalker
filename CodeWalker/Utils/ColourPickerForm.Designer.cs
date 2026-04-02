namespace CodeWalker.Utils
{
    partial class ColourPickerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ColourPickerForm));
            this.Picker = new CodeWalker.Utils.ColourPicker();
            this.ButtonOk = new System.Windows.Forms.Button();
            this.ButtonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Picker
            // 
            this.Picker.Location = new System.Drawing.Point(0, 0);
            this.Picker.Name = "Picker";
            this.Picker.SelectedColour = System.Drawing.Color.Black;
            this.Picker.Size = new System.Drawing.Size(450, 332);
            this.Picker.TabIndex = 0;
            // 
            // ButtonOk
            // 
            this.ButtonOk.Location = new System.Drawing.Point(257, 342);
            this.ButtonOk.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonOk.Name = "ButtonOk";
            this.ButtonOk.Size = new System.Drawing.Size(88, 25);
            this.ButtonOk.TabIndex = 4;
            this.ButtonOk.Text = "Ok";
            this.ButtonOk.UseVisualStyleBackColor = true;
            // 
            // ButtonCancel
            // 
            this.ButtonCancel.Location = new System.Drawing.Point(353, 342);
            this.ButtonCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ButtonCancel.Name = "ButtonCancel";
            this.ButtonCancel.Size = new System.Drawing.Size(88, 25);
            this.ButtonCancel.TabIndex = 3;
            this.ButtonCancel.Text = "Cancel";
            this.ButtonCancel.UseVisualStyleBackColor = true;
            // 
            // ColourPickerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96, 96);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(454, 379);
            this.Controls.Add(this.ButtonOk);
            this.Controls.Add(this.ButtonCancel);
            this.Controls.Add(this.Picker);
            this.DoubleBuffered = true;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ColourPickerForm";
            this.Text = "Colour Picker - CodeWalker by dexyfex";
            this.ResumeLayout(false);

        }

        #endregion

        private ColourPicker Picker;
        private System.Windows.Forms.Button ButtonOk;
        private System.Windows.Forms.Button ButtonCancel;
    }
}