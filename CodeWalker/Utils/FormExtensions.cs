using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Forms;
using CodeWalker.GameFiles;
using Image = System.Drawing.Image;

namespace CodeWalker.Utils;

public static class FormExtensions
{
    public static SharpDX.Vector2 TL(in this System.Drawing.RectangleF rect)
    {
        return new SharpDX.Vector2(rect.Left, rect.Top);
    }

    public static SharpDX.Vector2 TR(in this System.Drawing.RectangleF rect)
    {
        return new SharpDX.Vector2(rect.Right, rect.Top);
    }

    public static SharpDX.Vector2 BL(in this System.Drawing.RectangleF rect)
    {
        return new SharpDX.Vector2(rect.Left, rect.Bottom);
    }

    public static SharpDX.Vector2 BR(in this System.Drawing.RectangleF rect)
    {
        return new SharpDX.Vector2(rect.Right, rect.Bottom);
    }

    public static SharpDX.Size2 UpScale(in this SharpDX.Size2 size2, int factor)
    {
        return new SharpDX.Size2(size2.Width * factor, size2.Height * factor);
    }

    public static System.Drawing.RectangleF UpScale(in this System.Drawing.RectangleF rectangle, float factor)
    {
        return new System.Drawing.RectangleF(
            rectangle.X * factor,
            rectangle.Y * factor,
            rectangle.Width * factor,
            rectangle.Height * factor
        );
    }

    public static SharpDX.Mathematics.Interop.RawRectangleF Raw(in this System.Drawing.RectangleF rectangle)
    {
        return new SharpDX.Mathematics.Interop.RawRectangleF(
            rectangle.Left,
            rectangle.Top,
            rectangle.Right,
            rectangle.Bottom
        );
    }

    public static SharpDX.Mathematics.Interop.RawRectangleF Raw(in this System.Drawing.Rectangle rectangle)
    {
        return new SharpDX.Mathematics.Interop.RawRectangleF(
            rectangle.Left,
            rectangle.Top,
            rectangle.Right,
            rectangle.Bottom
        );
    }

    public static SharpDX.Mathematics.Interop.RawRectangleF Raw(in this SharpDX.Rectangle rectangle)
    {
        return new SharpDX.Mathematics.Interop.RawRectangleF(
            rectangle.Left,
            rectangle.Top,
            rectangle.Right,
            rectangle.Bottom
        );
    }

    public static SharpDX.Rectangle Convert(in this System.Drawing.Rectangle rectangle)
    {
        return new SharpDX.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    public static SharpDX.Vector2 Pivot(in this System.Drawing.RectangleF rectangle, float x, float y)
    {
        return new SharpDX.Vector2(
            rectangle.Left + (rectangle.Right - rectangle.Left) * x,
            rectangle.Top + (rectangle.Bottom - rectangle.Top) * y
        );
    }

    public static SharpDX.Mathematics.Interop.RawRectangleF ToRawRect(this SharpDX.Size2 size, int x = 0, int y = 0)
    {
        return new SharpDX.Mathematics.Interop.RawRectangleF(x, y, x + size.Width, y + size.Height);
    }

    public static System.Drawing.RectangleF ToRect(this SharpDX.Size2 size, int x = 0, int y = 0)
    {
        return new System.Drawing.RectangleF(x, y, size.Width, size.Height);
    }

    public static System.Drawing.Rectangle Convert(in this SharpDX.Rectangle rectangle)
    {
        return new System.Drawing.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }

    public static Image CreateBitmap(this Texture tex, int mip)
    {
        var cmip = Math.Min(Math.Max(mip, 0), tex.Levels - 1);
        var pixels = DDSIO.GetPixels(tex, cmip);
        var w = tex.Width >> cmip;
        var h = tex.Height >> cmip;

        if (pixels != null)
        {
        }
        return null;
    }

    public static Image CreateImage(this Texture tex, int mip)
    {
        var cmip = Math.Min(Math.Max(mip, 0), tex.Levels - 1);
        var pixels = DDSIO.GetPixels(tex, cmip);
        var w = tex.Width >> cmip;
        var h = tex.Height >> cmip;

        if (pixels != null)
        {
            var bmp = new Bitmap(w, h, PixelFormat.Format32bppArgb);
            var boundsRect = new System.Drawing.Rectangle(0, 0, w, h);
            var bmpData = bmp.LockBits(boundsRect, ImageLockMode.WriteOnly, bmp.PixelFormat);
            var ptr = bmpData.Scan0;
            var bytes = bmpData.Stride * bmp.Height;
            Marshal.Copy(pixels, 0, ptr, bytes);
            bmp.UnlockBits(bmpData);
            return bmp;
        }
        return null;
    }

    public static void ShowDialog(this Exception exception)
    {
        if (exception == null) return;
        MessageBox.Show($"{exception.GetType()}\n{exception.Message}", null, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void SelectEnum<TEnum>(this ToolStripDropDownButton dropDownButton, TEnum value) where TEnum : Enum
    {
        var name = value.ToString();
        foreach (ToolStripMenuItem menuItem in dropDownButton.DropDownItems)
        {
            menuItem.Checked = menuItem.Text == name;
        }
    }

    public static void SelectValueDrop<TValue>(this System.Windows.Forms.ComboBox comboBox, TValue[] values, TValue value)
    {
        comboBox.SelectedIndex = Array.IndexOf(values, value);
    }

    public static void SetValueDrop<TValue>(this System.Windows.Forms.ComboBox comboBox, string[] names, TValue[] values, Action<TValue> onValue, TValue defaultValue)
    {
        comboBox.Items.Clear();
        foreach (var name in names)
        {
            comboBox.Items.Add(new { Name = name });
        }
        comboBox.SelectedIndex = Array.IndexOf(values, defaultValue);
        comboBox.SelectedIndexChanged += (sender, e) =>
        {
            onValue(values[comboBox.SelectedIndex]);
        };
    }

    public static void SetEnumDrop<TEnum>(this ToolStripDropDownButton dropDownButton, Action<TEnum> onValue) where TEnum : struct
    {
        dropDownButton.DropDownItems.Clear();
        foreach (var name in Enum.GetNames(typeof(TEnum)))
        {
            var menuItem = new ToolStripMenuItem(name);
            dropDownButton.DropDownItems.Add(menuItem);
        }

        void ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (onValue != null)
            {
                try
                {
                    onValue((TEnum)Enum.Parse(typeof(TEnum), e.ClickedItem.Text));
                }
                catch (Exception exception)
                {
                    exception.ShowDialog();
                    return;
                }
            }
            foreach (ToolStripMenuItem menuItem in dropDownButton.DropDownItems)
            {
                menuItem.Checked = menuItem.Text == e.ClickedItem.Text;
            }
        }
        dropDownButton.DropDownItemClicked += ItemClicked;
    }
}