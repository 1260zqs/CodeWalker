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

public partial class TextureModToolsControl : DockContent
{
    TextureModDockForm mainForm;

    public TextureModToolsControl(TextureModDockForm mainForm)
    {
        InitializeComponent();
        this.mainForm = mainForm;

        this.checkBox1.Checked = mainForm.isPainting;
        this.checkBox2.Checked = mainForm.isSolidColor;
        this.checkBox3.Checked = mainForm.isDrawTestColor;

        this.checkBox1.CheckedChanged += checkBox1_CheckedChanged;
        this.checkBox2.CheckedChanged += checkBox2_CheckedChanged;
        this.checkBox3.CheckedChanged += checkBox3_CheckedChanged;

        this.flipXToggle.CheckedChanged += flipXToggle_CheckedChanged;
        this.flipYToggle.CheckedChanged += flipYToggle_CheckedChanged;
        this.swapToggle.CheckedChanged += swapToggle_CheckedChanged;
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
    }

    private void button7_Click(object sender, EventArgs e)
    {
        mainForm.RequestTexturePaintingUpdate();
    }

    private void checkBox3_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.isDrawTestColor = checkBox3.Checked;
    }

    public void SetSrcRect(in System.Drawing.RectangleF rect)
    {
        srcRectangleControl.SetRect(rect);
    }

    public void SetSrcRectWithoutNotify(in System.Drawing.RectangleF rect)
    {
        srcRectangleControl.SetRectWithoutNotify(rect);
    }

    public void SetDestRect(in System.Drawing.RectangleF rect)
    {
        destRectangleControl.SetRect(rect);
    }

    public void SelectTextureMapping(TextureMapping mapping)
    {
        SetDestRect(mapping.targetRect);
        flipXToggle.Checked = mapping.flipX;
        flipYToggle.Checked = mapping.flipY;
        swapToggle.Checked = mapping.swap;
    }

    private void srcRectangleControl_OnValueChanged(object sender, System.Drawing.RectangleF e)
    {
        mainForm.SetSrcImageRect(e);
    }

    private void destRectangleControl_OnValueChanged(object sender, System.Drawing.RectangleF e)
    {
        mainForm.SetDestTextureRect(e);
    }

    private void button1_Click(object sender, EventArgs e)
    {
        if (mainForm.GetSrcImageRect(out var srcRect) && mainForm.GetDestTextureRect(out var destRect))
        {
            srcRect.Width = destRect.Width;
            mainForm.SetSrcImageRect(srcRect);
        }
    }

    private void button2_Click(object sender, EventArgs e)
    {
        if (mainForm.GetSrcImageRect(out var srcRect) && mainForm.GetDestTextureRect(out var destRect))
        {
            srcRect.Height = destRect.Height;
            mainForm.SetSrcImageRect(srcRect);
        }
    }

    private void button3_Click(object sender, EventArgs e)
    {
        mainForm.FitSrcRectByDestWidth();
    }

    private void button4_Click(object sender, EventArgs e)
    {
        mainForm.FitSrcRectByDestHeight();
    }

    private void button6_Click(object sender, EventArgs e)
    {
        mainForm.ClipSrcRectByDestWidth();
    }

    private void button5_Click(object sender, EventArgs e)
    {
        mainForm.ClipSrcRectByDestHeight();
    }

    private void checkBox1_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.isPainting = this.checkBox1.Checked;
    }

    private void checkBox2_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.isSolidColor = this.checkBox2.Checked;
    }

    private void button9_Click(object sender, EventArgs e)
    {
        mainForm.GotoTextureLocation();
    }

    private void button8_Click(object sender, EventArgs e)
    {
        mainForm.SetTextureLocation();
    }

    private void button10_Click(object sender, EventArgs e)
    {
        if (mainForm.GetDestTextureRect(out var destRect) && mainForm.GetDestImageSize(out var pixelSize))
        {
            destRect.X = 0;
            destRect.Width = pixelSize.Width;
            mainForm.SetDestTextureRect(destRect);
        }
    }

    private void button11_Click(object sender, EventArgs e)
    {
        if (mainForm.GetDestTextureRect(out var destRect) && mainForm.GetDestImageSize(out var pixelSize))
        {
            destRect.Y = 0;
            destRect.Height = pixelSize.Height;
            mainForm.SetDestTextureRect(destRect);
        }
    }

    private void button12_Click(object sender, EventArgs e)
    {
        if (mainForm.GetDestTextureRect(out var destRect) && mainForm.GetDestImageSize(out var pixelSize))
        {
            destRect.Width = pixelSize.Width / 2f;
            mainForm.SetDestTextureRect(destRect);
        }
    }

    private void button13_Click(object sender, EventArgs e)
    {
        if (mainForm.GetDestTextureRect(out var destRect) && mainForm.GetDestImageSize(out var pixelSize))
        {
            destRect.Height = pixelSize.Height / 2f;
            mainForm.SetDestTextureRect(destRect);
        }
    }

    private void flipXToggle_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.SetTextureMappingFlipX(flipXToggle.Checked);
    }

    private void flipYToggle_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.SetTextureMappingFlipY(flipYToggle.Checked);
    }

    private void swapToggle_CheckedChanged(object sender, EventArgs e)
    {
        mainForm.SetTextureMappingSwap(swapToggle.Checked);
    }
}