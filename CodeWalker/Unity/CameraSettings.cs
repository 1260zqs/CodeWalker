using SharpDX;

namespace Unity;

public class CameraSettings
{
    const float kDefaultPerspectiveFov = 60;
    const float defaultEasingDuration = .4f;
    const float kAbsoluteSpeedMin = .01f;
    const float kAbsoluteSpeedMax = 99f;
    const float kAbsoluteEasingDurationMin = .1f;
    const float kAbsoluteEasingDurationMax = 2f;

    public float speed;
    public float speedNormalized;
    public float speedMin;
    public float speedMax;
    public bool easingEnabled;
    public float easingDuration;
    public bool accelerationEnabled;
    public float fieldOfViewHorizontalOrVertical; // either horizontal or vertical depending on aspect ratio
    public float nearClip;
    public float farClip;
    public bool dynamicClip;
    public bool occlusionCulling;

    public float fieldOfView;

    public CameraSettings()
    {
        speed = 1f;
        speedNormalized = .5f;
        speedMin = .01f;
        speedMax = 2f;
        easingEnabled = true;
        easingDuration = defaultEasingDuration;
        fieldOfView = kDefaultPerspectiveFov;
        dynamicClip = true;
        occlusionCulling = false;
        nearClip = .03f;
        farClip = 10000f;
        accelerationEnabled = true;
    }
}