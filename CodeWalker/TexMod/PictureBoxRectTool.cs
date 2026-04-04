using SharpDX;
using SharpDX.Mathematics.Interop;
using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CodeWalker.Utils;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;

namespace CodeWalker;

public static class PictureBoxRectTool
{
    class StateObject
    {
        public bool drawing;
        public bool editing;
        public bool solid;
        public bool enable;
        public Vector2 mouseDownPoint;
        public Vector2 scaleVector;
        public System.Drawing.RectangleF rect;
        public System.Drawing.RectangleF tempRect;
        public SharpDX.Mathematics.Interop.RawColor4 color;

        public bool gdi;
        public bool d2d;
        public bool matrixInvert;
        public ViewTool activeTool;
        public ViewTool viewTool;

        public System.Drawing.Drawing2D.Matrix matrix;
        public Matrix3x2 matrix3x2;

        public Action<System.Drawing.RectangleF> notify;
    }

    enum ViewTool : byte
    {
        None,
        Move,
        ResizeLT,
        ResizeRT,
        ResizeLB,
        ResizeRB,

        ResizeL,
        ResizeR,
        ResizeT,
        ResizeB,
    }

    static PictureBoxRectTool()
    {
        valueFactory = ValueFactory;
    }

    private static Func<int, StateObject> valueFactory;
    private static ConcurrentDictionary<int, StateObject> stateObjects = new();
    private static RawColor4 defaultColor = new RawColor4(1, 0, 0, 0.7f);

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

    public static void Paint(PictureBox pictureBox, Graphics graphics)
    {
        // if (stateObjects.TryGetValue(GetHandle(pictureBox), out var stateObject))
        // {
        //     stateObject.gdi = true;
        //     stateObject.d2d = false;
        //     stateObject.matrixInvert = false;
        //     stateObject.matrix = graphics.Transform;
        //
        //     var rect = stateObject.rect;
        //     if (rect.Width > 0 && rect.Height > 0)
        //     {
        //         if (stateObject.solid)
        //         {
        //             solidBrush.Color = stateObject.color;
        //             graphics.FillRectangle(solidBrush, rect);
        //         }
        //         else
        //         {
        //             pen.Color = stateObject.color;
        //             var r = new System.Drawing.Rectangle();
        //             r.X = (int)rect.X;
        //             r.Y = (int)rect.Y;
        //             r.Width = (int)rect.Width;
        //             r.Height = (int)rect.Height;
        //             graphics.DrawRectangle(pen, r);
        //         }
        //     }
        // }
    }

    public static void Paint(CodeWalker.D2DCanvas canvas)
    {
        if (stateObjects.TryGetValue(GetHandle(canvas), out var stateObject))
        {
            stateObject.d2d = true;
            stateObject.gdi = false;
            stateObject.matrixInvert = false;
            stateObject.matrix3x2 = canvas.transform;
            stateObject.scaleVector = stateObject.matrix3x2.ScaleVector;

            var rect = stateObject.rect;
            if (rect.Width > 0 && rect.Height > 0)
            {
                if (!stateObject.editing)
                {
                    if (stateObject.solid)
                    {
                        canvas.FillRectangle(rect, stateObject.color);
                    }
                    else
                    {
                        var scaleVector = stateObject.scaleVector;
                        canvas.DrawRectangle(rect, stateObject.color, 1f / scaleVector.X);
                    }
                }
                else
                {
                    var scaleVector = stateObject.scaleVector;
                    var thickness = 1 / scaleVector.X;

                    var size = 10f / scaleVector.X;
                    var halfSize = size / 2f;
                    var color = new RawColor4(1, 0, 1, 1);

                    var topLeft = rect.TL();
                    var topRight = rect.TR();
                    var bottomLeft = rect.BL();
                    var bottomRight = rect.BR();

                    canvas.DrawRectangle(rect, color, thickness);
                    DrawHandle(canvas, stateObject.viewTool == ViewTool.ResizeLT, new(topLeft.X - halfSize, topLeft.Y - halfSize, size, size), color, thickness);
                    DrawHandle(canvas, stateObject.viewTool == ViewTool.ResizeRT, new(topRight.X - halfSize, topRight.Y - halfSize, size, size), color, thickness);
                    DrawHandle(canvas, stateObject.viewTool == ViewTool.ResizeLB, new(bottomLeft.X - halfSize, bottomLeft.Y - halfSize, size, size), color, thickness);
                    DrawHandle(canvas, stateObject.viewTool == ViewTool.ResizeRB, new(bottomRight.X - halfSize, bottomRight.Y - halfSize, size, size), color, thickness);
                    if (stateObject.viewTool == ViewTool.ResizeT)
                    {
                        canvas.DrawLine(topLeft, topRight, color, thickness * 2);
                    }
                    else if (stateObject.viewTool == ViewTool.ResizeB)
                    {
                        canvas.DrawLine(bottomLeft, bottomRight, color, thickness * 2);
                    }
                    else if (stateObject.viewTool == ViewTool.ResizeL)
                    {
                        canvas.DrawLine(topLeft, bottomLeft, color, thickness * 2);
                    }
                    else if (stateObject.viewTool == ViewTool.ResizeR)
                    {
                        canvas.DrawLine(topRight, bottomRight, color, thickness * 2);
                    }
                }
            }
        }
    }

    static void DrawHandle(D2DCanvas canvas, bool active, in System.Drawing.RectangleF rectangle, in RawColor4 color, float thickness)
    {
        if (active)
        {
            canvas.FillRectangle(rectangle, color);
            return;
        }
        canvas.DrawRectangle(rectangle, color, thickness);
    }

    private static void OnMouseDown(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                var stateObject = stateObjects.GetOrAdd(GetHandle(control), valueFactory);
                if (stateObject.editing)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    stateObject.activeTool = GetViewTool(stateObject, current);
                    SetViewToolCursor(stateObject.activeTool);

                    stateObject.tempRect = stateObject.rect;
                    stateObject.mouseDownPoint = current;
                    return;
                }
                if (stateObject.enable)
                {
                    stateObject.drawing = true;
                    stateObject.solid = e.Button == MouseButtons.Right;
                    stateObject.mouseDownPoint = ScreenToImage(stateObject, e.Location);
                    stateObject.rect = new(
                        stateObject.mouseDownPoint.X,
                        stateObject.mouseDownPoint.Y,
                        0, 0
                    );
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
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
                if (!stateObject.enable) return;

                if (stateObject.activeTool == ViewTool.ResizeRT)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.Y = stateObject.tempRect.Y + delta.Y;
                    stateObject.rect.Width = stateObject.tempRect.Width + delta.X;
                    stateObject.rect.Height = stateObject.tempRect.Height - delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeLT)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.X = stateObject.tempRect.X + delta.X;
                    stateObject.rect.Y = stateObject.tempRect.Y + delta.Y;
                    stateObject.rect.Width = stateObject.tempRect.Width - delta.X;
                    stateObject.rect.Height = stateObject.tempRect.Height - delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeLB)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.X = stateObject.tempRect.X + delta.X;
                    stateObject.rect.Width = stateObject.tempRect.Width - delta.X;
                    stateObject.rect.Height = stateObject.tempRect.Height + delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeRB)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.Width = stateObject.tempRect.Width + delta.X;
                    stateObject.rect.Height = stateObject.tempRect.Height + delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeL)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.X = stateObject.tempRect.X + delta.X;
                    stateObject.rect.Width = stateObject.tempRect.Width - delta.X;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeR)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.Width = stateObject.tempRect.Width + delta.X;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeT)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.Y = stateObject.tempRect.Y + delta.Y;
                    stateObject.rect.Height = stateObject.tempRect.Height - delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.ResizeB)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.Height = stateObject.tempRect.Height + delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.activeTool == ViewTool.Move)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    var delta = current - stateObject.mouseDownPoint;
                    stateObject.rect.X = stateObject.tempRect.X + delta.X;
                    stateObject.rect.Y = stateObject.tempRect.Y + delta.Y;
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
                else if (stateObject.editing)
                {
                    var current = ScreenToImage(stateObject, e.Location);
                    stateObject.viewTool = GetViewTool(stateObject, current);
                    SetViewToolCursor(stateObject.viewTool, false);
                    control.Invalidate();
                }
                else if (stateObject.drawing)
                {
                    var cur = ScreenToImage(stateObject, e.Location);
                    var x = Math.Min(stateObject.mouseDownPoint.X, cur.X);
                    var y = Math.Min(stateObject.mouseDownPoint.Y, cur.Y);
                    var w = Math.Abs(stateObject.mouseDownPoint.X - cur.X);
                    var h = Math.Abs(stateObject.mouseDownPoint.Y - cur.Y);

                    stateObject.rect = new(x, y, w, h);
                    stateObject.notify?.Invoke(stateObject.rect);
                    control.Invalidate();
                }
            }
        }
    }

    private static void SetViewToolCursor(ViewTool viewTool, bool move = true)
    {
        switch (viewTool)
        {
            case ViewTool.Move:
                if (move) Cursor.Current = Cursors.SizeAll;
                break;
            case ViewTool.ResizeLT:
            case ViewTool.ResizeRB:
                Cursor.Current = Cursors.SizeNWSE;
                break;
            case ViewTool.ResizeRT:
            case ViewTool.ResizeLB:
                Cursor.Current = Cursors.SizeNESW;
                break;
            case ViewTool.ResizeL:
            case ViewTool.ResizeR:
                Cursor.Current = Cursors.SizeWE;
                break;
            case ViewTool.ResizeT:
            case ViewTool.ResizeB:
                Cursor.Current = Cursors.SizeNS;
                break;
        }
    }

    private static ViewTool GetViewTool(StateObject stateObject, Vector2 mousePoint)
    {
        var radius = 10f / stateObject.scaleVector.X;

        var topLeft = stateObject.rect.TL();
        var topRight = stateObject.rect.TR();
        var bottomLeft = stateObject.rect.BL();
        var bottomRight = stateObject.rect.BR();

        if (Vector2.Distance(mousePoint, topLeft) <= radius)
        {
            return ViewTool.ResizeLT;
        }
        else if (Vector2.Distance(mousePoint, topRight) <= radius)
        {
            return ViewTool.ResizeRT;
        }
        else if (Vector2.Distance(mousePoint, bottomLeft) <= radius)
        {
            return ViewTool.ResizeLB;
        }
        else if (Vector2.Distance(mousePoint, bottomRight) <= radius)
        {
            return ViewTool.ResizeRB;
        }
        else if (Mathf.Abs(mousePoint.X - topLeft.X) <= radius)
        {
            return ViewTool.ResizeL;
        }
        else if (Mathf.Abs(mousePoint.X - topRight.X) <= radius)
        {
            return ViewTool.ResizeR;
        }
        else if (Mathf.Abs(mousePoint.Y - topLeft.Y) <= radius)
        {
            return ViewTool.ResizeT;
        }
        else if (Mathf.Abs(mousePoint.Y - bottomLeft.Y) <= radius)
        {
            return ViewTool.ResizeB;
        }
        else if (stateObject.rect.Contains(mousePoint.X, mousePoint.Y))
        {
            return ViewTool.Move;
        }
        return ViewTool.None;
    }

    private static void OnMouseUp(object sender, MouseEventArgs e)
    {
        if (sender is Control control)
        {
            if (e.Button == MouseButtons.Left || e.Button == MouseButtons.Right)
            {
                if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
                {
                    stateObject.activeTool = ViewTool.None;
                    stateObject.drawing = false;
                    control.Invalidate();
                }
            }
        }
    }

    static Vector2 ScreenToImage(StateObject stateObject, Point p)
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
            return new Vector2(pts[0].X, pts[0].Y);
        }
        if (stateObject.d2d)
        {
            if (!stateObject.matrixInvert)
            {
                stateObject.matrixInvert = true;
                stateObject.matrix3x2.Invert();
            }
            var point = new Vector2(p.X, p.Y);
            Matrix3x2.TransformPoint(ref stateObject.matrix3x2, ref point, out var result);
            return result;
        }
        return new Vector2(p.X, p.Y);
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

    public static void Clear(Control control)
    {
        if (control == null) return;
        stateObjects.TryRemove(GetHandle(control), out _);
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
        if (control == null) return;
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

    public static void SetEditMode(Control control, bool edit)
    {
        if (control == null) return;
        if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
        {
            stateObject.editing = edit;
        }
    }

    public static bool IsEditMode(Control control)
    {
        if (control == null) return false;
        if (stateObjects.TryGetValue(GetHandle(control), out var stateObject))
        {
            return stateObject.editing;
        }
        return false;
    }

    public static Vector2 GetCenter(Vector2[] v)
    {
        Vector2 center = Vector2.Zero;

        for (int i = 0; i < v.Length; i++)
            center += v[i];

        return center / v.Length;
    }

    public static void TransformRect(Vector2[] vertex, in System.Drawing.RectangleF rect, ref Matrix3x2 matrix3X2)
    {
        vertex[0] = new Vector2(rect.Left, rect.Top);
        vertex[1] = new Vector2(rect.Right, rect.Top);
        vertex[2] = new Vector2(rect.Right, rect.Bottom);
        vertex[3] = new Vector2(rect.Left, rect.Bottom);

        for (var i = 0; i < 4; i++)
        {
            var p = vertex[i];
            Matrix3x2.TransformPoint(ref matrix3X2, ref p, out vertex[i]);
        }
    }

    static bool PointInRect(Vector2 p, Vector2[] v)
    {
        static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        var b1 = Sign(p, v[0], v[1]) < 0.0f;
        var b2 = Sign(p, v[1], v[2]) < 0.0f;
        var b3 = Sign(p, v[2], v[3]) < 0.0f;
        var b4 = Sign(p, v[3], v[0]) < 0.0f;

        return (b1 == b2) && (b2 == b3) && (b3 == b4);
    }
}