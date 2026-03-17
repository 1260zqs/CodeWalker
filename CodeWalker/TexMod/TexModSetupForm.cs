using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

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
    }
}
