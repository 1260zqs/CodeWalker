using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace CodeWalker;

public static class PictureBoxViewer
{
    class StateObject
    {
        public float zoom = 1f;
        public PointF pan;

        public bool panning;
        public PointF panStart;
        public PointF panOrigin;
    }

    static PictureBoxViewer()
    {
        valueFactory = ValueFactory;
    }

    private static Func<int, StateObject> valueFactory;
    private static ConcurrentDictionary<int, StateObject> stateObjects = new();

    public static void AddFeature(PictureBox pictureBox)
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

    public static void ResetViewer(this PictureBox pictureBox)
    {
        stateObjects.TryRemove(GetHandle(pictureBox), out _);
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
        if (sender is PictureBox pictureBox)
        {
            stateObjects.TryRemove(GetHandle(pictureBox), out _);
        }
    }

    private static void OnMouseWheel(object sender, MouseEventArgs e)
    {
        if (sender is PictureBox pictureBox)
        {
            var stateObject = stateObjects.GetOrAdd(GetHandle(pictureBox), valueFactory);

            var oldZoom = stateObject.zoom;
            if (e.Delta > 0) stateObject.zoom *= 1.1f;
            else stateObject.zoom /= 1.1f;
            stateObject.zoom = Mathf.Clamp(stateObject.zoom, 0.1f, 20f);

            float mx = e.X;
            float my = e.Y;
            stateObject.pan.X = mx - (mx - stateObject.pan.X) * (stateObject.zoom / oldZoom);
            stateObject.pan.Y = my - (my - stateObject.pan.Y) * (stateObject.zoom / oldZoom);

            pictureBox.Invalidate();
        }
    }

    private static void OnMouseUp(object sender, MouseEventArgs e)
    {
        if (sender is PictureBox pictureBox)
        {
            if (stateObjects.TryGetValue(GetHandle(pictureBox), out var stateObject))
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
        if (sender is PictureBox pictureBox)
        {
            if (stateObjects.TryGetValue(GetHandle(pictureBox), out var stateObject))
            {
                if (stateObject.panning)
                {
                    stateObject.pan.X = stateObject.panOrigin.X + (e.X - stateObject.panStart.X);
                    stateObject.pan.Y = stateObject.panOrigin.Y + (e.Y - stateObject.panStart.Y);
                    pictureBox.Invalidate();
                }
            }
        }
    }

    private static void OnMouseDown(object sender, MouseEventArgs e)
    {
        if (sender is PictureBox pictureBox)
        {
            var stateObject = stateObjects.GetOrAdd(GetHandle(pictureBox), valueFactory);
            if (e.Button == MouseButtons.Middle)
            {
                stateObject.panning = true;
                stateObject.panStart = e.Location;
                stateObject.panOrigin = stateObject.pan;
            }
        }
    }

    private static int GetHandle(PictureBox pictureBox)
    {
        return RuntimeHelpers.GetHashCode(pictureBox);
    }

    private static StateObject ValueFactory(int key)
    {
        return new StateObject();
    }
}