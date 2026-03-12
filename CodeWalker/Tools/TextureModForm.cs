using CodeWalker.Properties;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

namespace CodeWalker.Tools
{
    public partial class TextureModForm : Form
    {
        public TextureModForm()
        {
            InitializeComponent();
            var theme = Settings.Default.GetProjectWindowTheme();
            var version = VisualStudioToolStripExtender.VsVersion.Vs2015;
            vsExtender.SetStyle(toolStrip1, version, theme);
            vsExtender.SetStyle(toolStrip2, version, theme);

            InitializeListView();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            // dxPreview1.InitDevice();
        }

        private void InitializeListView()
        {
            replacementListView.Columns.Add("project");
            for (var i = 0; i < 100; i++)
            {
                replacementListView.Items.AddRange([
                    new ListViewItem(Guid.NewGuid().ToString("N")),
                ]);
            }
            replacementListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            sourceRefListView.Columns.Add("reference");
            for (var i = 0; i < 100; i++)
            {
                sourceRefListView.Items.AddRange([
                    new ListViewItem(Guid.NewGuid().ToString("N")),
                ]);
            }
            sourceRefListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }
    }
}