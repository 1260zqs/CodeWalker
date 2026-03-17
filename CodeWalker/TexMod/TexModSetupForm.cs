using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsAPICodePack.Dialogs;

namespace CodeWalker.TexMod
{
    public partial class TexModSetupForm : Form
    {
        public string ProjectWorkingDir { get; set; }
        public string PackageManifestFile { get; set; }

        public TexModSetupForm()
        {
            InitializeComponent();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            textBox1.Text = PackageManifestFile;
            textBox2.Text = ProjectWorkingDir;            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                textBox1.Text = openFileDialog.FileName;
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            using (var dialog = new CommonOpenFileDialog())
            {
                dialog.Title = "Select Folder";
                dialog.IsFolderPicker = true;
                if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
                {
                    textBox2.Text = dialog.FileName;
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ProjectWorkingDir = textBox2.Text;
            PackageManifestFile = textBox1.Text;
            Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}