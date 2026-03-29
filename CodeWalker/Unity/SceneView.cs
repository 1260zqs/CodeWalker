using CodeWalker.World;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;
using Unity;

enum MotionState
{
    kInactive,
    kActive,
    kDragging
}

public enum ViewTool
{
    None = -1,
    Orbit = 0,
    Pan = 1,
    Zoom = 2,
    FPS = 3
}

enum DraggingLockedState
{
    NotDragging, // Default state. Scene view camera is snapped to selected object instantly
    Dragging, // User is dragging from handles. Scene view camera holds still.
    LookAt // Temporary state after dragging or selection change, where we return scene view camera smoothly to selected object
}

public class SceneView
{
    const float k_MaxSceneViewSize = 3.2e38f;
    const float kDefaultViewSize = 10f;
    const float s_FPSScrollWheelMultiplier = 0.01f;
    const float k_FlySpeedAcceleration = 1.8f;
    const float k_MaxCameraFarClip = 1.844674E+19f;
    const float k_MinCameraNearClip = 1e-5f;

    public Camera camera;
    public Rectangle viewRect;

    private AnimValueDriver animValueDriver = new();
    private AnimFloat m_Size;
    private AnimBool m_Ortho;
    private AnimVector3 m_Position;
    private AnimQuaternion m_Rotation;
    private object m_LastLockedObject;
    private bool m_ViewIsLockedToObject;
    private bool isRotationLocked;
    private bool m_2DMode;

    private float s_StartZoom;
    private float s_ZoomSpeed;
    private float s_TotalMotion;
    private Vector3 s_Motion;
    private bool s_Moving;
    private float k_FlySpeed = 9f;
    private float s_FlySpeedTarget;
    private AnimVector3 s_FlySpeed;
    private CameraFlyModeContext s_CameraFlyModeContext = new();
    private CameraSettings cameraSettings = new();

    private bool isMouseDragging;
    private Vector2 mouseDragStartPoint;
    private MouseButtons mouseDragButton;
    public object syncRoot = new object();

    private MouseButtons mouseButtonDown;
    private DraggingLockedState draggingLocked;
    private ViewTool m_ViewTool = ViewTool.Pan;
    private ViewTool lockedViewTool = ViewTool.None;
    private MotionState currentState;

    private Keys modifierKeys;
    private HashSet<Keys> pressedKeys = new();

    public SceneView()
    {
        m_Size = animValueDriver.Get<AnimFloat, float>(kDefaultViewSize);
        m_Ortho = animValueDriver.Get<AnimBool, bool>();
        m_Position = animValueDriver.Get<AnimVector3, Vector3>(new Vector3(0, 0, 0));
        m_Rotation = animValueDriver.Get<AnimQuaternion, Quaternion>(
            //Quaternion.Identity
            Quaternion.LookAtLH((new Vector3(0, 1, 0)), new Vector3(0, 0, 0), Vector3.UnitZ)
            //Quaternion.LookAtLH((new Vector3(0, 1, -1)), new Vector3(0, 0, 0), Vector3.UnitZ)
        );
        s_FlySpeed = animValueDriver.Get<AnimVector3, Vector3>(Vector3.Zero);
    }

    public bool actionPressed => modifierKeys.HasFlag(Keys.ControlKey);
    private bool shiftPressed => modifierKeys.HasFlag(Keys.Shift);
    private bool altPressed => modifierKeys.HasFlag(Keys.Alt);

    private bool viewIsLockedToObject
    {
        get => m_ViewIsLockedToObject;
        set
        {
            if (value) m_LastLockedObject = new object();
            else m_LastLockedObject = null;

            m_ViewIsLockedToObject = value;
            draggingLocked = DraggingLockedState.LookAt;
        }
    }

    private bool in2DMode
    {
        get => m_2DMode;
        set
        {
            var viewTool = GetViewTool();
            if (m_2DMode != value && viewTool != ViewTool.FPS && viewTool != ViewTool.Orbit)
            {
                m_2DMode = value;
                // On2DModeChange();
            }
        }
    }

    private bool viewToolActive => lockedViewTool != ViewTool.None
                                   || mouseButtonDown == MouseButtons.Right
                                   || mouseButtonDown == MouseButtons.Middle
                                   || altPressed;

    public float perspectiveFov => camera.perspectiveFov;

    public float size
    {
        get => m_Size.value;
        set => m_Size.value = ValidateSceneSize(value);
    }

    public Vector3 pivot
    {
        get => m_Position.value;
        set => m_Position.value = value;
    }

    // The direction of the scene view.
    public Quaternion rotation
    {
        get => m_Rotation.value;
        set => m_Rotation.value = value;
    }

    public float cameraDistance
    {
        get
        {
            float res;
            if (!camera.IsOrthographic)
            {
                var fov = m_Ortho.Fade(perspectiveFov, 0);
                res = GetPerspectiveCameraDistance(size, fov);
            }
            else
                res = size * 2f;

            // clamp to allowed range in case scene view size was huge
            return Mathf.Clamp(res, -k_MaxSceneViewSize, k_MaxSceneViewSize);
        }
    }

    public bool orthographic
    {
        get => m_Ortho.value;
        set
        {
            m_Ortho.value = value;
            // svRot.UpdateGizmoLabel(this, m_Rotation.target * Vector3.forward, m_Ortho.target);
        }
    }

    public void Sync()
    {
        animValueDriver.Update();
        HandleViewToolCursor();
        if (lockedViewTool == ViewTool.FPS)
        {
            FixNegativeSize();
        }
        UpdateMovement();
        camera.ViewQuaternion = rotation;
        camera.Position = pivot + Vector3.Transform(new Vector3(0, -cameraDistance, 0), camera.ViewQuaternion);
    }

    private void SyncNow()
    {
        if (!Monitor.TryEnter(syncRoot, 50))
        {
            return;
        }
        camera.ViewQuaternion = rotation;
        camera.Position = pivot + Vector3.Transform(new Vector3(0, -cameraDistance, 0), camera.ViewQuaternion);
        camera.Update(0);
        Monitor.Exit(syncRoot);
    }

    public void HandleMouseDown(MouseEventArgs eventArgs)
    {
        mouseButtonDown = eventArgs.Button;
        if (mouseDragButton == MouseButtons.None)
        {
            isMouseDragging = true;
            mouseDragButton = eventArgs.Button;
            mouseDragStartPoint.X = eventArgs.Location.X;
            mouseDragStartPoint.Y = eventArgs.Location.Y;
        }
        HandleMouseDown();
    }

    public void HandleMouseMove(MouseEventArgs eventArgs)
    {
        if (isMouseDragging)
        {
            var delta = new Vector2();
            delta.X = mouseDragStartPoint.X - eventArgs.Location.X;
            delta.Y = mouseDragStartPoint.Y - eventArgs.Location.Y;
            mouseDragStartPoint.X = eventArgs.Location.X;
            mouseDragStartPoint.Y = eventArgs.Location.Y;
            if (delta.X != 0 || delta.Y != 0)
            {
                HandleMouseDrag(delta);
            }
        }
    }

    public void HandleMouseUp(MouseEventArgs eventArgs)
    {
        if (mouseButtonDown == eventArgs.Button)
        {
            mouseButtonDown = MouseButtons.None;
        }
        if (eventArgs.Button == mouseDragButton)
        {
            isMouseDragging = false;
            mouseDragButton = MouseButtons.None;
        }
        HandleMouseUp(eventArgs.Button);
    }

    public void HandleMouseWheel(MouseEventArgs eventArgs)
    {
        HandleScrollWheel(
            eventArgs.Delta / -120f,
            new Vector2(eventArgs.X, eventArgs.Y),
            in2DMode == altPressed
        );
        Console.WriteLine(cameraDistance);
    }

    public void HandleKeyDown(KeyEventArgs eventArgs)
    {
        modifierKeys = eventArgs.Modifiers;
        var keyCode = eventArgs.KeyCode;
        var actionKey = eventArgs.Modifiers.HasFlag(Keys.ControlKey);
        using (var inputSamplingScope = CameraFlyModeContext.InputEvent.OnKeyDown(keyCode, actionKey, s_CameraFlyModeContext, lockedViewTool, orthographic))
        {
            if (inputSamplingScope.currentlyMoving)
                viewIsLockedToObject = false;

            if (inputSamplingScope.inputVectorChanged)
                s_FlySpeedTarget = 0f;

            s_Motion = inputSamplingScope.currentInputVector;
        }
        UpdateMovement();

        if (pressedKeys.Contains(keyCode))
        {
            return;
        }
        if (keyCode == Keys.Escape)
        {
            ResetDragState();
        }
    }

    public void HandleKeyUp(KeyEventArgs eventArgs)
    {
        modifierKeys = eventArgs.Modifiers;
        pressedKeys.Remove(eventArgs.KeyCode);

        var keyCode = eventArgs.KeyCode;
        var actionKey = eventArgs.Modifiers.HasFlag(Keys.ControlKey);
        using (var inputSamplingScope = CameraFlyModeContext.InputEvent.OnKeyUp(keyCode, actionKey, s_CameraFlyModeContext, lockedViewTool, orthographic))
        {
            if (inputSamplingScope.currentlyMoving)
                viewIsLockedToObject = false;

            if (inputSamplingScope.inputVectorChanged)
                s_FlySpeedTarget = 0f;

            s_Motion = inputSamplingScope.currentInputVector;
        }
        UpdateMovement();
    }

    private void HandleViewToolCursor()
    {
        if (!viewToolActive) return;

        // var cursor = MouseCursor.Arrow;
        // switch (viewTool)
        // {
        //     case ViewTool.Pan:
        //         cursor = MouseCursor.Pan;
        //         break;
        //     case ViewTool.Orbit:
        //         cursor = MouseCursor.Orbit;
        //         break;
        //     case ViewTool.FPS:
        //         cursor = MouseCursor.FPS;
        //         break;
        //     case ViewTool.Zoom:
        //         cursor = MouseCursor.Zoom;
        //         break;
        // }
        // if (cursor != MouseCursor.Arrow)
        // {
        //     // AddCursorRect(cameraRect, cursor);
        // }
    }

    private void HandleScrollWheel(float zoomDelta, Vector2 mousePosition, bool zoomTowardsCenter)
    {
        float targetSize;
        if (!camera.IsOrthographic)
        {
            const float deltaCutoff = .3f;
            var relativeDelta = Mathf.Abs(size) * zoomDelta * 0.10f;
            if (relativeDelta > 0 && relativeDelta < deltaCutoff)
                relativeDelta = deltaCutoff;
            else if (relativeDelta < 0 && relativeDelta > -deltaCutoff)
                relativeDelta = -deltaCutoff;

            targetSize = size + relativeDelta;
        }
        else
        {
            targetSize = Mathf.Abs(size) * (zoomDelta * .010f + 1.0f);
        }

        var initialDistance = cameraDistance;
        if (!(float.IsNaN(targetSize) || float.IsInfinity(targetSize)))
        {
            targetSize = Mathf.Min(k_MaxSceneViewSize, targetSize);
            size = targetSize;
        }

        if (!zoomTowardsCenter && Mathf.Abs(cameraDistance) < 1.0e7f)
        {
            var percentage = 1f - (cameraDistance / initialDistance);
            var mouseRay = camera.ScreenPointToRay(mousePosition.X, mousePosition.X);
            var mousePivot = mouseRay.Position + mouseRay.Direction * cameraDistance;
            var pivotVector = mousePivot - pivot;
            pivot += pivotVector * percentage;
        }
    }

    private void HandleMouseDown()
    {
        currentState = MotionState.kInactive;
        if (viewToolActive)
        {
            var wantedViewTool = GetViewTool();
            // Check if we want to lock a view tool
            if (lockedViewTool != wantedViewTool)
            {
                lockedViewTool = wantedViewTool;

                // Set up zoom parameters
                s_StartZoom = size;
                s_ZoomSpeed = Math.Max(Math.Abs(s_StartZoom), .3f);
                s_TotalMotion = 0;

                // we're not dragging yet, but enter this state so we can cleanup correctly
                currentState = MotionState.kActive;
            }
        }
    }

    private void HandleMouseUp(MouseButtons button)
    {
        // Move pivot to clicked point.
        if (button == MouseButtons.Right && currentState != MotionState.kDragging)
        {
            // if (RaycastWorld(guiEvent.mousePosition, out var hit))
            // {
            //     var targetSize = size;
            //     if (!orthographic)
            //     {
            //         var currentPosition = pivot - rotation.Multiply(Vector3.forward) * cameraDistance;
            //         targetSize = size * Vector3.Dot(hit.point - currentPosition, rotation.Multiply(Vector3.forward)) / cameraDistance;
            //     }
            //     LookAt(hit.point, rotation, targetSize);
            // }
        }
        ResetDragState();
    }

    private void HandleMouseDrag(Vector2 delta)
    {
        switch (lockedViewTool)
        {
            case ViewTool.Orbit:
            {
                if (!in2DMode && !isRotationLocked)
                {
                    OrbitCameraBehavior(delta);
                }
                break;
            }
            case ViewTool.Pan:
            {
                FixNegativeSize();
                SyncNow();
                viewIsLockedToObject = false;
                var position = pivot;
                var screenPos = camera.WorldToScreenPoint(position);
                screenPos += new Vector3(delta, 0);
                var worldDelta = camera.ScreenToWorldPoint(screenPos) - position;
                const float pixelsPerPoint = 1f;
                worldDelta *= pixelsPerPoint;
                if (shiftPressed) worldDelta *= 4;
                pivot += worldDelta;
                break;
            }
            case ViewTool.Zoom:
            {
                var zoomDelta = GetNiceMouseDeltaZoom(delta) * (shiftPressed ? 9 : 3);
                if (orthographic)
                {
                    size = Mathf.Max(.0001f, size * (1 + zoomDelta * .001f));
                }
                else
                {
                    s_TotalMotion += zoomDelta;
                    if (s_TotalMotion < 0)
                    {
                        size = s_StartZoom * (1 + s_TotalMotion * .001f);
                    }
                    else
                    {
                        size += zoomDelta * s_ZoomSpeed * .003f;
                    }
                }
                break;
            }
            case ViewTool.FPS:
            {
                if (!in2DMode && !isRotationLocked)
                {
                    if (!orthographic)
                    {
                        viewIsLockedToObject = false;
                        var rot = rotation;
                        var camPos = pivot - rot.Multiply(Vector3.UnitY) * cameraDistance;
                        var pitchRot = Quaternion.RotationAxis(rot.Multiply(Vector3.UnitX), delta.Y * .003f);
                        var yawRot = Quaternion.RotationAxis(Vector3.UnitZ, delta.X * .003f);
                        rotation = Quaternion.Normalize(yawRot * (pitchRot * rot));
                        pivot = camPos + rot.Multiply(Vector3.UnitY) * cameraDistance;
                    }
                    else
                    {
                        // We want orbit behavior in orthograpic when using FPS
                        OrbitCameraBehavior(delta);
                    }
                }
                break;
            }
        }
    }

    private void OrbitCameraBehavior(Vector2 delta)
    {
        FixNegativeSize();
        var rot = rotation;
        var pitchRot = Quaternion.RotationAxis(rot.Multiply(Vector3.UnitX), delta.Y * 0.003f);
        var yawRot = Quaternion.RotationAxis(Vector3.UnitZ, delta.X * 0.003f);
        //rot = UnityMathExtensions.AngleAxis(-e.delta.Y * .003f * Mathf.Rad2Deg, rot.Multiply(Vector3.UnitX)) * rot;
        //rot = UnityMathExtensions.AngleAxis(-e.delta.X * .003f * Mathf.Rad2Deg, Vector3.UnitZ) * rot;
        if (size < 0)
        {
            size = 0;
            pivot = camera.Position;
        }
        rotation = Quaternion.Normalize(yawRot * (pitchRot * rot));
    }

    private void UpdateMovement()
    {
        var movement = GetMovementDirection();
        if (s_Moving)
        {
            pivot += rotation.Multiply(movement);
        }
    }

    private void FixNegativeSize()
    {
        if (size == 0f) size = float.Epsilon;
        if (size < 0)
        {
            var fov = perspectiveFov;
            var distance = GetPerspectiveCameraDistance(size, fov);
            var p = m_Position.value + Vector3.Transform(new Vector3(0, -distance, 0), rotation);
            size = -size;
            distance = GetPerspectiveCameraDistance(size, fov);
            m_Position.value = p + Vector3.Transform(new Vector3(0, distance, 0), rotation);
        }
    }

    private void ResetDragState()
    {
        currentState = MotionState.kInactive;
        lockedViewTool = ViewTool.None;
        mouseButtonDown = MouseButtons.None;
        s_Motion = Vector3.Zero;
    }

    private void ResetMotion()
    {
        s_Motion = Vector3.Zero;
        s_FlySpeed.value = Vector3.Zero;
        s_Moving = false;
    }

    private ViewTool GetViewTool()
    {
        if (viewToolActive)
        {
            if (lockedViewTool == ViewTool.None)
            {
                var noModifiers = !actionPressed && !altPressed;
                if (((mouseButtonDown == MouseButtons.None || mouseButtonDown == MouseButtons.Left) && noModifiers)
                    || ((mouseButtonDown == MouseButtons.None || mouseButtonDown == MouseButtons.Left) && actionPressed)
                    || mouseButtonDown == MouseButtons.Middle
                    || (in2DMode || isRotationLocked) && !(mouseButtonDown == MouseButtons.Right && altPressed)
                   )
                {
                    m_ViewTool = ViewTool.Pan;
                }
                else if (mouseButtonDown == MouseButtons.Right && altPressed)
                {
                    m_ViewTool = ViewTool.Zoom;
                }
                else if ((mouseButtonDown == MouseButtons.None || mouseButtonDown == MouseButtons.Left) && altPressed)
                {
                    m_ViewTool = ViewTool.Orbit;
                }
                else if (mouseButtonDown == MouseButtons.Right && !altPressed)
                {
                    m_ViewTool = ViewTool.FPS;
                }
            }
        }
        else
        {
            m_ViewTool = ViewTool.Pan;
        }
        return m_ViewTool;
    }

    static float ValidateSceneSize(float value)
    {
        if (value == 0f)
            return float.Epsilon;
        if (value > k_MaxSceneViewSize)
            return k_MaxSceneViewSize;
        if (value < -k_MaxSceneViewSize)
            return -k_MaxSceneViewSize;
        return value;
    }

    private float GetNiceMouseDeltaZoom(Vector2 delta)
    {
        // Decide which direction the mouse delta goes.
        // Problem is that when the user zooms horizontal and vertical, it can jitter back and forth.
        // So we only update from which axis we pick the sign if x and y
        // movement is not very close to each other
        var useYSignZoom = false;
        if (Mathf.Abs(Mathf.Abs(delta.X) - Mathf.Abs(delta.Y)) / Mathf.Max(Mathf.Abs(delta.X), Mathf.Abs(delta.Y)) > .1f)
        {
            if (Mathf.Abs(delta.X) <= Mathf.Abs(delta.Y))
            {
                useYSignZoom = true;
            }
        }

        var acceleration = UnityEditor.NumericFieldDraggerUtility.Acceleration(shiftPressed, altPressed);
        if (useYSignZoom)
        {
            return Mathf.Sign(delta.Y) * delta.Length() * acceleration;
        }
        return Mathf.Sign(delta.X) * delta.Length() * acceleration;
    }

    static float GetPerspectiveCameraDistance(float objectSize, float fov)
    {
        //        A
        //        |\        We want to place camera at a
        //        | \       distance that, at the given FOV,
        //        |  \      would enclose a sphere of radius
        //     _..+.._\     "size". Here |BC|=size, and we
        //   .'   |   '\    need to find |AB|. ACB is a right
        //  /     |    _C   angle, andBAC is half the FOV. So
        // |      | _-   |  that gives: sin(BAC)=|BC|/|AB|,
        // |      B      |  and thus |AB|=|BC|/sin(BAC).
        // |             |
        //  \           /
        //   '._     _.'
        //      `````
        return (float)(objectSize / Math.Sin(fov * 0.5f * Mathf.Deg2Rad));
    }

    internal Vector2 GetDynamicClipPlanes()
    {
        var farClip = Mathf.Clamp(2000f * size, 1000f, k_MaxCameraFarClip);
        return new Vector2(farClip * 0.000005f, farClip);
    }

    private Vector3 GetMovementDirection()
    {
        s_Moving = s_Motion.SqrMagnitude() > 0f;
        var deltaTime = s_CameraFlyModeContext.deltaTime;
        var speedModifier = 1f; //s_SceneView.cameraSettings.speed;

        if (shiftPressed)
            speedModifier *= 5f;

        if (s_Moving)
        {
            if (cameraSettings.accelerationEnabled)
                s_FlySpeedTarget = s_FlySpeedTarget < Mathf.Epsilon ? k_FlySpeed : s_FlySpeedTarget * Mathf.Pow(k_FlySpeedAcceleration, deltaTime);
            else
                s_FlySpeedTarget = k_FlySpeed;
        }
        else
        {
            s_FlySpeedTarget = 0f;
        }

        if (cameraSettings.easingEnabled)
        {
            var easingDuration = 1f;
            s_FlySpeed.speed = 1f / easingDuration;
            s_FlySpeed.target = Vector3.Normalize(s_Motion) * s_FlySpeedTarget * speedModifier;
        }
        else
        {
            s_FlySpeed.value = Vector3.Normalize(s_Motion) * s_FlySpeedTarget * speedModifier;
        }

        return s_FlySpeed.value * deltaTime;
    }

    public bool Frame(Vector3 pos, float boundSize, bool instant = true)
    {
        if (float.IsInfinity(boundSize))
            return false;

        if (boundSize < Mathf.Epsilon)
            boundSize = 10;

        // We snap instantly into target on playmode, because things might be moving fast and lerping lags behind
        LookAt(pos, m_Rotation.target, boundSize, m_Ortho.value, instant);
        return true;
    }

    public void LookAt(Vector3 point, Quaternion direction, float newSize, bool ortho, bool instant)
    {
        ResetMotion();
        FixNegativeSize();
        if (instant)
        {
            m_Position.value = point;
            m_Rotation.value = direction;
            size = Mathf.Abs(newSize);
            m_Ortho.value = ortho;
            draggingLocked = DraggingLockedState.NotDragging;
        }
        else
        {
            m_Position.target = point;
            m_Rotation.target = direction;
            m_Size.target = Mathf.Abs(newSize);
            m_Ortho.target = ortho;
        }
    }

    bool RaycastWorld(Vector2 position, out object hit)
    {
        hit = default;
        return false;
    }
}