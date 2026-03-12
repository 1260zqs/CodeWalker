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
            var imgList = new ImageList();
            imgList.ImageSize = new Size(1, 22);
            imgList.ColorDepth = ColorDepth.Depth32Bit;
            replacementListView.SmallImageList = imgList;

            sourceRefListView.Columns.Add("reference");
            for (var i = 0; i < 100; i++)
            {
                sourceRefListView.Items.AddRange([
                    new ListViewItem(Guid.NewGuid().ToString("N")),
                ]);
            }
            sourceRefListView.SmallImageList = imgList;
            sourceRefListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

            propertyGridFix1.SelectedObject = new SampleData();
        }

        public class SampleData
        {
            public string Name { get; set; } = "Example Item";
            public int Count { get; set; } = 5;
            public bool Enabled { get; set; } = true;
            public double Price { get; set; } = 19.99;
            public DateTime Created { get; set; } = DateTime.Now;
            public MyStruct MyStruct { get; set; } = new MyStruct();
        }

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class MyStruct
        {
            public string strVal;
            public string strVal2 { get; set; }
            public int intVal { get; set; }
            public override string ToString()
            {
                return string.Empty;
            }
        }
    }
}