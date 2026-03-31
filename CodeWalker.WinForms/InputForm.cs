using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CodeWalker.WinForms;

public partial class InputForm : Form
{
    public InputForm()
    {
        InitializeComponent();
    }

    public string inputString { get; set; }

    public static InputForm Show(string title, string text)
    {
        var form = new InputForm();
        form.textBox.Text = text;
        return form;
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        inputString = textBox.Text;
        Close();
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        inputString = null;
        Close();
    }
}
