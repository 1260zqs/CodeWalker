using System;
using System.Windows.Forms;

namespace CodeWalker.TexMod;

public partial class AddTexModSourceForm : Form
{
    public string sourceFileName { get; set; }
    public string sourceTexName { get; set; }

    public AddTexModSourceForm()
    {
        InitializeComponent();
    }

    private void okBtn_Click(object sender, EventArgs e)
    {
        sourceTexName = textBox1.Text;
        sourceFileName = textBox2.Text;
        if (string.IsNullOrEmpty(sourceFileName))
        {
            MessageBox.Show("Please set the source file");
            return;
        }
        if (string.IsNullOrEmpty(sourceTexName))
        {
            MessageBox.Show("Please set the source tex name");
            return;
        }
        DialogResult = DialogResult.OK;
        Close();
    }

    private void cancelBtn_Click(object sender, EventArgs e)
    {
        sourceFileName = null;
        sourceTexName = null;
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private void textBox2_TextChanged(object sender, EventArgs e)
    {
        var sourcePath = textBox2.Text;
        var indexOf = sourcePath.IndexOf(':');
        if (indexOf > 0)
        {
            textBox1.Text = sourcePath.Substring(indexOf + 1);
            textBox2.Text = sourcePath.Substring(0, indexOf);
        }
    }
}