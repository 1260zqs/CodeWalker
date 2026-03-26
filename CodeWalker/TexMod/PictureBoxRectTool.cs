using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using SharpDX;
using SharpDX.Mathematics.Interop;
using Color = System.Drawing.Color;
using Matrix = System.Drawing.Drawing2D.Matrix;
using Point = System.Drawing.Point;
using Rectangle = System.Drawing.Rectangle;

namespace CodeWalker;

public static class PictureBoxRectTool
{
    class StateObject
    {
        public bool drawing;
        public bool solid;
        public bool enable;
        public Point start;
        public System.Drawing.RectangleF rect;
        public Color color;

        public bool gdi;
        public bool d2d;
        public Matrix matrix;
        public bool matrixInvert;
        public Matrix3x2 matrix3x2;

        public Action<System.Drawing.RectangleF> notify;
    }

    static PictureBoxRectTool()
    {
        valueFactory = ValueFactory;
        pen = new Pen(defaultColor, 1);
        solidBrush = new SolidBrush(defaultColor);
    }

    private static Pen pen;
    private static SolidBrush solidBrush;
    private static Func<int, StateObject> valueFactory;
    private static ConcurrentDictionary<int, StateObject> stateObjects = new();
    private static Color defaultColor = Color.FromArgb(127, 255, 0, 0);

    public static void AddFeature(Control pictureBox, Action<System.Drawing.RectangleF> notify)
    {
        pictureBox.Disposed += OnDisposed;
        pictureBox.MouseUp += OnMouseUp;
        pictureBox.MouseDown += OnMouseDown;
        pictureBox.MouseMove += OnMouseMove;
        if (notify != null)
        {
            GetStageObject(pictureBox).notify = notify;
        }
    }

    private static void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
            {
                if (stateObject.enable && stateObject.drawing)
                {
                    var cur = ScreenToImage(stateObject, e.Location);
                    var x = Math.Min(stateObject.start.X, cur.X);
                    var y = Math.Min(stateObject.start.Y, cur.Y);
                    var w = Math.Abs(stateObject.start.X - cur.X);
                    var h = Math.Abs(stateObject.start.Y - cur.Y);

                    stateObject.rect = new Rectangle(x, y, w, h);
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
            }
        }
    }

    public static void Paint(CodeWalker.D2DCanvas canvas)
    {
        if (stateObjects.TryGetValue(GetHandle(canvas), out var stateObject))
        {
            stateObject.d2d = true;
            stateObject.gdi = false;
            stateObject.matrixInvert = false;
            stateObject.matrix3x2 = canvas.transform;
            var rect = stateObject.rect;
            if (rect.Width > 0 && rect.Height > 0)
            {
                if (stateObject.solid)
                {
                    canvas.FillRectangle(rect, new RawColor4(1f, 0, 0, 0.5f));
                }
                else
                {
                    var scaleVector = stateObject.matrix3x2.ScaleVector;
                    canvas.DrawRectangle(rect, new RawColor4(1f, 0, 0, 0.5f), 1f / scaleVector.X);
                }
            }
        }
    }

    public static void Paint(PictureBox pictureBox, Graphics graphics)
    {
        if (stateObjects.TryGetValue(GetHandle(pictureBox), out var stateObject))
        {
            var rect = stateObject.rect;
            stateObject.matrix = graphics.Transform;
            stateObject.matrixInvert = false;
            stateObject.gdi = true;
            stateObject.d2d = true;
            if (rect.Width > 0 && rect.Height > 0)
            {
                if (stateObject.solid)
                {
                    solidBrush.Color = stateObject.color;
                    graphics.FillRectangle(solidBrush, rect);
                }
                else
                {
                    pen.Color = stateObject.color;
                    var r = new System.Drawing.Rectangle();
                    r.X = (int)rect.X;
                    r.Y = (int)rect.Y;
                    r.Width = (int)rect.Width;
                    r.Height = (int)rect.Height;
                    graphics.DrawRectangle(pen, r);
                }
            }
        }
    }

    private static void OnMouseUp(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
                {
                    stateObject.drawing = false;
                    control.Invalidate();
                }
            }
        }
    }

    private static void OnMouseDown(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
                if (stateObject.enable)
                {
                    stateObject.drawing = true;
                    stateObject.solid = e.Button == MouseButtons.Right;
                    stateObject.start = ScreenToImage(stateObject, e.Location);
                    stateObject.rect = new System.Drawing.RectangleF(stateObject.start, Size.Empty);
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
            }
        }
    }

    static Point ScreenToImage(StateObject stateObject, Point p)
    {
        if (stateObject.gdi)
        {
            var pts = new[] { p };
            if (stateObject.matrix != null)
            {
                if (!stateObject.matrixInvert)
                {
                    stateObject.matrixInvert = true;
                    stateObject.matrix.Invert();
                }
                stateObject.matrix.TransformPoints(pts);
            }
            return pts[0];
        }
        else if (stateObject.d2d)
        {
            if (!stateObject.matrixInvert)
            {
                stateObject.matrixInvert = true;
                stateObject.matrix3x2.Invert();
            }
            var point = new Vector2(p.X, p.Y);
            Matrix3x2.TransformPoint(ref stateObject.matrix3x2, ref point, out var result);
            p.X = (int)result.X;
            p.Y = (int)result.Y;
        }
        return p;
    }

    private static void OnDisposed(object sender, EventArgs args)
    {
        if (sender is Control control)
        {
            stateObjects.TryRemove(GetHandle(control), out _);
        }
    }

    private static bool TryGetStageObject(Control control, out StateObject stateObject)
    {
        return stateObjects.TryGetValue(GetHandle(control), out stateObject);
    }

    private static StateObject GetStageObject(Control control)
    {
        return stateObjects.GetOrAdd(GetHandle(control), valueFactory);
    }

    private static int GetHandle(Control control)
    {
        return RuntimeHelpers.GetHashCode(control);
    }

    private static StateObject ValueFactory(int key)
    {
        var stateObject = new StateObject();
        stateObject.matrix3x2 = Matrix3x2.Identity;
        stateObject.color = defaultColor;
        stateObject.enable = true;
        return stateObject;
    }

    public static void SetRect(Control control, System.Drawing.RectangleF rect)
    {
        var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
        stateObject.rect = rect;
        control.Invalidate();
    }

    public static bool GetRect(Control control, out System.Drawing.RectangleF rect)
    {
        if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
        {
            rect = stateObject.rect;
            return true;
        }
        rect = default;
        return false;
    }

    public static void SetSolid(Control control, bool solid)
    {
        if (TryGetStageObject(control, out var stateObject))
        {
            stateObject.solid = solid;
        }
    }

    public static bool GetSolid(Control control)
    {
        if (TryGetStageObject(control, out var stateObject))
        {
            return stateObject.solid;
        }
        return false;
    }

    public static void SetPaintEnable(Control control, bool enable)
    {
        if (TryGetStageObject(control, out var stateObject))
        {
            stateObject.enable = enable;
        }
    }

    public static bool GetPaintEnable(Control control)
    {
        if (TryGetStageObject(control, out var stateObject))
        {
            return stateObject.enable;
        }
        return false;
    }
}