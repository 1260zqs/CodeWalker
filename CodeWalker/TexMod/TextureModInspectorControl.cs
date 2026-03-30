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

namespace CodeWalker.TexMod;

public partial class TextureModInspectorControl : DockContent
{
    public TextureModDockForm mainForm;

    public TextureModInspectorControl()
    {
        InitializeComponent();
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
    }

    private void button7_Click(object sender, EventArgs e)
    {
        mainForm.UpdateTexturePainting();
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.UpdateTexturePainting();
    }
}