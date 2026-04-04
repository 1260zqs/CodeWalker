namespace CodeWalker
{
    partial class RectangleControl
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
            this.rectLabel = new System.Windows.Forms.Label();
            this.rectBoxX = new System.Windows.Forms.NumericUpDown();
            this.rectBoxY = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.rectBoxH = new System.Windows.Forms.NumericUpDown();
            this.label6 = new System.Windows.Forms.Label();
            this.rectBoxW = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxX)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxH)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxW)).BeginInit();
            this.SuspendLayout();
            // 
            // rectLabel
            // 
            this.rectLabel.AutoSize = true;
            this.rectLabel.Location = new System.Drawing.Point(3, 4);
            this.rectLabel.Name = "rectLabel";
            this.rectLabel.Size = new System.Drawing.Size(17, 12);
            this.rectLabel.TabIndex = 56;
            this.rectLabel.Text = "x:";
            // 
            // rectBoxX
            // 
            this.rectBoxX.DecimalPlaces = 2;
            this.rectBoxX.Location = new System.Drawing.Point(22, 1);
            this.rectBoxX.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.rectBoxX.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.rectBoxX.Name = "rectBoxX";
            this.rectBoxX.Size = new System.Drawing.Size(80, 21);
            this.rectBoxX.TabIndex = 1;
            // 
            // rectBoxY
            // 
            this.rectBoxY.DecimalPlaces = 2;
            this.rectBoxY.Location = new System.Drawing.Point(137, 1);
            this.rectBoxY.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.rectBoxY.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.rectBoxY.Name = "rectBoxY";
            this.rectBoxY.Size = new System.Drawing.Size(80, 21);
            this.rectBoxY.TabIndex = 2;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 34);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 12);
            this.label5.TabIndex = 59;
            this.label5.Text = "w:";
            // 
            // rectBoxH
            // 
            this.rectBoxH.DecimalPlaces = 2;
            this.rectBoxH.Location = new System.Drawing.Point(137, 31);
            this.rectBoxH.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.rectBoxH.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.rectBoxH.Name = "rectBoxH";
            this.rectBoxH.Size = new System.Drawing.Size(80, 21);
            this.rectBoxH.TabIndex = 4;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(118, 4);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(17, 12);
            this.label6.TabIndex = 60;
            this.label6.Text = "y:";
            // 
            // rectBoxW
            // 
            this.rectBoxW.DecimalPlaces = 2;
            this.rectBoxW.Location = new System.Drawing.Point(22, 31);
            this.rectBoxW.Maximum = new decimal(new int[] {
            -1,
            -1,
            -1,
            0});
            this.rectBoxW.Minimum = new decimal(new int[] {
            -1,
            -1,
            -1,
            -2147483648});
            this.rectBoxW.Name = "rectBoxW";
            this.rectBoxW.Size = new System.Drawing.Size(80, 21);
            this.rectBoxW.TabIndex = 3;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(118, 34);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(17, 12);
            this.label7.TabIndex = 61;
            this.label7.Text = "h:";
            // 
            // RectangleControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rectLabel);
            this.Controls.Add(this.rectBoxX);
            this.Controls.Add(this.rectBoxY);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.rectBoxH);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.rectBoxW);
            this.Controls.Add(this.label7);
            this.Name = "RectangleControl";
            this.Size = new System.Drawing.Size(222, 54);
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxX)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxH)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.rectBoxW)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label rectLabel;
        private System.Windows.Forms.NumericUpDown rectBoxX;
        private System.Windows.Forms.NumericUpDown rectBoxY;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown rectBoxH;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown rectBoxW;
        private System.Windows.Forms.Label label7;
    }
}
