using System;
using System.Runtime.InteropServices;

namespace CodeWalker;

public static class UXTheme
{
    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    public static extern int SetWindowTheme(IntPtr hWnd, string app, string id);
}