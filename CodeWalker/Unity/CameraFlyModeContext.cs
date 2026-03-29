// Unity C# reference source
// Copyright (c) Unity Technologies. For terms of use, see
// https://unity3d.com/legal/licenses/Unity_Reference_Only_License

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpDX;

namespace Unity;

class CameraFlyModeContext
{
#if true
    struct TimeHelper
    {
        private float deltaTime;
        private long lastTime;

        public void Begin()
        {
            lastTime = DateTime.Now.Ticks;
        }

        public float Update()
        {
            deltaTime = (DateTime.Now.Ticks - lastTime) / 10000000.0f;
            lastTime = DateTime.Now.Ticks;
            return deltaTime;
        }
    }

    enum ShortcutStage
    {
        Begin,
        End,
    }

    struct ShortcutArguments(ShortcutStage stage, object context)
    {
        public object context = context;
        public ShortcutStage stage = stage;
    }

    public struct InputEvent
    {
        public bool actionKey;
        public bool keyDown;
        public bool keyUp;
        public Keys keyCode;

        public static InputSamplingScope OnKeyDown(Keys keyCode, bool actionKey, CameraFlyModeContext context, ViewTool currentViewTool, bool orthographic)
        {
            var inputEvent = new InputEvent();
            inputEvent.actionKey = actionKey;
            inputEvent.keyCode = keyCode;
            inputEvent.keyDown = true;
            inputEvent.keyUp = false;

            return new InputSamplingScope(inputEvent, context, currentViewTool, orthographic);
        }

        public static InputSamplingScope OnKeyUp(Keys keyCode, bool actionKey, CameraFlyModeContext context, ViewTool currentViewTool, bool orthographic)
        {
            var inputEvent = new InputEvent();
            inputEvent.actionKey = actionKey;
            inputEvent.keyCode = keyCode;
            inputEvent.keyDown = false;
            inputEvent.keyUp = true;

            return new InputSamplingScope(inputEvent, context, currentViewTool, orthographic);
        }
    }

    public struct InputSamplingScope : IDisposable
    {
        bool active => m_ArrowKeysActive || m_Context.active;

        public bool currentlyMoving => active && !Mathf.Approximately(currentInputVector.SqrMagnitude(), 0f);

        public Vector3 currentInputVector => active ? m_Context.currentInputVector : Vector3.Zero;

        public bool inputVectorChanged => active && !Mathf.Approximately((m_Context.currentInputVector - m_Context.previousVector).SqrMagnitude(), 0f);

        private bool m_Disposed;
        readonly bool m_ArrowKeysActive;
        readonly CameraFlyModeContext m_Context;
        private InputEvent inputEvent;

        // controlID will get hotControl if using arrow keys while shortcut context is not active
        // passing a value of zero disables the arrow keys
        public InputSamplingScope(InputEvent inputEvent, CameraFlyModeContext context, ViewTool currentViewTool, bool orthographic = false)
        {
            m_ArrowKeysActive = false;
            m_Disposed = false;
            m_Context = context;
            m_Context.active = currentViewTool == ViewTool.FPS;
            this.inputEvent = inputEvent;

            if (m_Context.active)
            {
                m_ArrowKeysActive = DoArrowKeys(orthographic);
            }
            else
            {
                ForceArrowKeysUp(orthographic);
            }

            if (currentlyMoving && Mathf.Approximately(m_Context.previousVector.SqrMagnitude(), 0f))
            {
                m_Context.timer.Begin();
            }
        }

        public void Dispose()
        {
            if (m_Disposed) return;
            m_Disposed = true;
            m_Context.previousVector = currentInputVector;
        }

        void ForceArrowKeysUp(bool orthographic)
        {
            foreach (var key in m_Context.keysDown)
            {
                var action = keyBindings[key];
                action(orthographic, new ShortcutArguments(ShortcutStage.End, m_Context));
            }
            m_Context.keysDown.Clear();
        }

        bool DoArrowKeys(bool orthographic)
        {
            if (inputEvent.actionKey) return false;
            if (inputEvent.keyDown)
            {
                if (keyBindings.TryGetValue(inputEvent.keyCode, out var action))
                {
                    action(orthographic, new ShortcutArguments(ShortcutStage.Begin, m_Context));
                    m_Context.keysDown.Add(inputEvent.keyCode);
                    return true;
                }
            }
            else if (inputEvent.keyUp)
            {
                if (keyBindings.TryGetValue(inputEvent.keyCode, out var action))
                {
                    action(orthographic, new ShortcutArguments(ShortcutStage.End, m_Context));
                    m_Context.keysDown.Remove(inputEvent.keyCode);
                    return true;
                }
            }
            return false;
        }
    }

    public bool active;
    public float deltaTime => timer.Update();

    private TimeHelper timer;
    private Vector3 previousVector;
    private Vector3 currentInputVector;
    private HashSet<Keys> keysDown = new();

    // @formatter:off
    static Dictionary<Keys, Action<bool, ShortcutArguments>> keyBindings = new()
    {
        { Keys.E, (orthographic, args) => { if (!orthographic) WalkUp(args); } },
        { Keys.Q, (orthographic, args) => { if (!orthographic) WalkDown(args); } },
        { Keys.W, (orthographic, args) => { if (orthographic) WalkUp(args); else WalkForward(args); } },
        { Keys.S, (orthographic, args) => { if (orthographic) WalkDown(args); else WalkBackward(args); } },
        { Keys.A, (orthographic, args) => WalkLeft(args) },
        { Keys.D, (orthographic, args) => WalkRight(args) },
    };
    // @formatter:on

    static void WalkForward(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.Y = args.stage == ShortcutStage.Begin ? 1f : context.currentInputVector.Y > 0f ? 0f : context.currentInputVector.Y;
    }

    static void WalkBackward(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.Y = args.stage == ShortcutStage.Begin ? -1f : context.currentInputVector.Y < 0f ? 0f : context.currentInputVector.Y;
    }

    static void WalkLeft(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.X = args.stage == ShortcutStage.Begin ? -1f : context.currentInputVector.X < 0f ? 0f : context.currentInputVector.X;
    }

    static void WalkRight(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.X = args.stage == ShortcutStage.Begin ? 1f : context.currentInputVector.X > 0f ? 0f : context.currentInputVector.X;
    }

    static void WalkUp(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.Z = args.stage == ShortcutStage.Begin ? 1f : context.currentInputVector.Z > 0f ? 0f : context.currentInputVector.Z;
    }

    static void WalkDown(ShortcutArguments args)
    {
        var context = (CameraFlyModeContext)args.context;
        context.currentInputVector.Z = args.stage == ShortcutStage.Begin ? -1f : context.currentInputVector.Z < 0f ? 0f : context.currentInputVector.Z;
    }
#endif
}