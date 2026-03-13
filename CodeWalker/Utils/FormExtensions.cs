using System;
using System.Windows.Controls;
using System.Windows.Forms;

namespace CodeWalker.Utils;

public static class FormExtensions
{
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