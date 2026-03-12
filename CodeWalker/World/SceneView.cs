using SharpDX;
using System;
using System.Windows.Forms;

namespace CodeWalker.World;

public class SceneView
{
    internal const float k_MaxSceneViewSize = 3.2e34f;

    static readonly Vector3 kDefaultPivot = Vector3.Zero;
    static readonly Quaternion kDefaultRotation = (new Vector3(-1f, -0.7f, -1f)).LookRotation();

    public Camera camera;
    private AnimBool m_Ortho = new AnimBool();
    private AnimFloat m_Size = new AnimFloat(10f);
    private AnimVector3 m_Position = new AnimVector3(kDefaultPivot);
    private AnimQuaternion m_Rotation = new AnimQuaternion(kDefaultRotation);

    public bool orthographic => camera.IsOrthographic;

    public Quaternion rotation
    {
        get => camera.ViewQuaternion;
        set => camera.ViewQuaternion = value;
    }

    public Vector3 pivot
    {
        get => this.m_Position.value;
        set => this.m_Position.value = value;
    }

    public float perspectiveFov
    {
        get => this.camera.FieldOfView;
    }

    public float size
    {
        get => m_Size.value;
        set => m_Size.value = ValidateSceneSize(value);
    }

    public float targetSize
    {
        get => m_Size.target;
        set => m_Size.target = ValidateSceneSize(value);
    }

    public void HandleKeyDown()
    {
    }

    public void HandleScrollWheel(MouseEventArgs e)
    {
    }

    public void HandleMouseDown(EventArgs e)
    {
    }

    static float ValidateSceneSize(float value)
    {
        if (value == 0f || float.IsNaN(value))
            return float.Epsilon;
        if (value > k_MaxSceneViewSize)
            return k_MaxSceneViewSize;
        if (value < -k_MaxSceneViewSize)
            return -k_MaxSceneViewSize;
        return value;
    }

    public void LookAt(Vector3 point, Quaternion direction, float newSize, bool ortho, bool instant)
    {
        ResetMotion();
        this.FixNegativeSize();
        if (instant)
        {
            this.m_Position.value = point;
            this.m_Rotation.value = direction;
            this.size = Math.Abs(newSize);
            this.m_Ortho.value = ortho;
            //this.draggingLocked = SceneView.DraggingLockedState.NotDragging;
        }
        else
        {
            this.m_Position.target = point;
            this.m_Rotation.target = direction;
            this.m_Size.target = Math.Abs(newSize);
            this.m_Ortho.target = ortho;
        }
    }

    public void FixNegativeSize()
    {
    }

    public void ResetMotion()
    {
    }
}