using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SharpDX;
using Color = System.Drawing.Color;

namespace CodeWalker;

public static class PictureBoxViewer
{
    class StateObject
    {
        public float zoom = 1f;
        public Vector2 pan;

        public bool panning;
        public Vector2 panStart;
        public Vector2 panOrigin;
        public float miniZoom = 0.1f;
        public float maxZoom = 20f;
    }

    static PictureBoxViewer()
    {
        valueFactory = ValueFactory;
    }

    private static Func<int, StateObject> valueFactory;
    private static ConcurrentDictionary<int, StateObject> stateObjects = new();

    public static void AddFeature(Control pictureBox)
    {
        pictureBox.Disposed += OnDisposed;
        pictureBox.MouseWheel += OnMouseWheel;
        pictureBox.MouseDown += OnMouseDown;
        pictureBox.MouseUp += OnMouseUp;
        pictureBox.MouseMove += OnMouseMove;
    }

    public static void SimplePaint(PictureBox pictureBox)
    {
        pictureBox.Paint += OnPaint;
    }

    public static bool GetState(Control control, out float zoom, out Vector2 pan)
    {
        if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
        {
            pan = stateObject.pan;
            zoom = stateObject.zoom;
            return true;
        }
        zoom = 1f;
        pan = default;
        return false;
    }

    private static void OnPaint(object sender, PaintEventArgs e)
    {
        if (sender is PictureBox pictureBox)
        {
            e.Graphics.Clear(Color.Gray);
            Paint(pictureBox, e.Graphics);

            var image = pictureBox.Image;
            if (image != null)
            {
                e.Graphics.PixelOffsetMode = PixelOffsetMode.Half;
                e.Graphics.InterpolationMode = InterpolationMode.NearestNeighbor;
                e.Graphics.DrawImage(image, 0, 0);
            }
        }
    }

    public static void ResetViewer(Control control)
    {
        stateObjects.TryRemove(GetHandle(control), out _);
    }

    internal static void Update(Control control, Image image)
    {
        Update(control, image.Width, image.Height);
    }

    internal static void Update(Control control, int width, int height)
    {
        var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
        stateObject.miniZoom = Math.Min((float)control.Width / width, (float)control.Height / height) * 0.8f;
        stateObject.maxZoom = Math.Max((float)control.Width / width, (float)control.Height / height) * 100;
    }

    internal static void Paint(D2DCanvas canvas, SharpDX.Direct2D1.Bitmap bitmap)
    {
        if (stateObjects.TryGetValue(GetHandle(canvas), out var stateObject))
        {
            var pan = stateObject.pan;
            var zoom = stateObject.zoom;

            var infoStr = string.Empty;
            if (bitmap != null)
            {
                var pixelSize = bitmap.PixelSize;
                infoStr = $"\npixelSize: {pixelSize.Width} x {pixelSize.Height}";
            }
            canvas.DrawText(
                $"pan: {pan.X:F0}, {pan.Y:F0}\nzoom: {zoom * 100:F1}%{infoStr}",
                6, 0,
                new SharpDX.Color(0, 0, 0, 0.5f)
            );
            canvas.SetTransformation(pan.X, pan.Y, zoom);
        }
    }

    internal static void Paint(PictureBox pictureBox, Graphics graphics)
    {
        if (stateObjects.TryGetValue(GetHandle(pictureBox), out var stateObject))
        {
            var pan = stateObject.pan;
            var zoom = stateObject.zoom;
            graphics.TranslateTransform(pan.X, pan.Y);
            graphics.ScaleTransform(zoom, zoom);
        }
    }

    private static void OnDisposed(object sender, EventArgs args)
    {
        if (sender is Control control)
        {
            stateObjects.TryRemove(GetHandle(control), out _);
        }
    }

    private static void OnMouseWheel(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);

            var oldZoom = stateObject.zoom;
            if (e.Delta > 0) stateObject.zoom *= 1.2f;
            else stateObject.zoom /= 1.2f;
            //stateObject.zoom = Mathf.Clamp(stateObject.zoom, stateObject.miniZoom, stateObject.maxZoom);

            float mx = e.X;
            float my = e.Y;
            stateObject.pan.X = mx - (mx - stateObject.pan.X) * (stateObject.zoom / oldZoom);
            stateObject.pan.Y = my - (my - stateObject.pan.Y) * (stateObject.zoom / oldZoom);

            control.Invalidate();
        }
    }

    private static void OnMouseUp(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
            {
                if (e.Button == MouseButtons.Middle)
                {
                    stateObject.panning = false;
                }
            }
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
            {
                if (stateObject.panning)
                {
                    stateObject.pan.X = stateObject.panOrigin.X + (e.X - stateObject.panStart.X);
                    stateObject.pan.Y = stateObject.panOrigin.Y + (e.Y - stateObject.panStart.Y);
                    control.Invalidate();
                }
            }
        }
    }

    private static void OnMouseDown(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
            if (e.Button == MouseButtons.Middle)
            {
                stateObject.panning = true;
                stateObject.panStart = new Vector2(e.Location.X, e.Location.Y);
                stateObject.panOrigin = stateObject.pan;
            }
        }
    }

    private static int GetHandle(Control control)
    {
        return RuntimeHelpers.GetHashCode(control);
    }

    private static StateObject ValueFactory(int key)
    {
        return new StateObject();
    }

    public static void LoadState(Control control, object stateObject)
    {
        if (stateObject is StateObject x)
        {
            stateObjects[GetHandle(control)] = x;
        }
    }

    public static object SaveState(Control control)
    {
        stateObjects.TryGetValue(GetHandle(control), out var stateObject);
        return stateObject;
    }

    public static void FitViewer(Control control, int width, int height)
    {
        var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
        var maxSize = Math.Max(width, height);
        var maxView = Math.Min(control.Width, control.Height);
        if (maxSize > 0)
        {
            stateObject.zoom = (maxView * 0.8f) / maxSize;

            var scaledWidth = width * stateObject.zoom;
            var scaledHeight = height * stateObject.zoom;

            var offsetX = (control.Width - scaledWidth) * 0.5f;
            var offsetY = (control.Height - scaledHeight) * 0.5f;

            stateObject.pan = new Vector2(offsetX, offsetY);
        }
    }
}