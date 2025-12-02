using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Light2D))]
public class PlayerVisionLight : MonoBehaviour
{
    [Header("Shape")]
    [Tooltip("Max radius when visionLevel01 = 1.")]
    public float maxViewRadius = 8f;

    [Tooltip("Min radius when visionLevel01 = 0.")]
    public float minViewRadius = 3f;

    [Tooltip("Vision cone angle in degrees. Use 360 for full-circle vision.")]
    [Range(0f, 360f)]
    public float viewAngle = 120f;

    [Tooltip("Inner radius fraction of the outer radius (for soft falloff).")]
    [Range(0f, 1f)]
    public float innerRadiusFraction = 0.5f;

    [Tooltip("Inner angle fraction of the outer angle (for soft edge).")]
    [Range(0f, 1f)]
    public float innerAngleFraction = 0.3f;

    [Header("Orientation")]
    [Tooltip("If true, light aims at mouse. If false, use externalDirection.")]
    public bool lookAtMouse = true;

    [Tooltip("Turn speed in degrees per second. Higher = snappier, 0 = instant snap.")]
    public float rotationSmoothing = 720f;

    [Tooltip("Offset in degrees, because Light2D's 'forward' is usually up.")]
    public float rotationOffset = -90f;

    [Header("Dynamic Vision Level (0–1)")]
    [Range(0f, 1f)]
    [Tooltip("0 = minViewRadius, 1 = maxViewRadius. You can drive this from game code.")]
    public float visionLevel01 = 1f;

    [Header("Fallback / External Direction")]
    [Tooltip("Used if lookAtMouse == false. Should be normalized.")]
    public Vector2 externalDirection = Vector2.right;

    private Light2D _light2D;
    private Camera _cam;

    private void OnEnable()
    {
        CacheRefs();
        ApplyShapeNow();

        // In edit mode, just snap rotation to the current direction
        if (!Application.isPlaying)
        {
            ApplyRotationImmediate();
        }
    }

    private void Awake()
    {
        CacheRefs();

        if (Application.isPlaying)
        {
            ApplyShapeNow();
            ApplyRotationImmediate();
        }
    }

    private void OnValidate()
    {
        CacheRefs();

        minViewRadius = Mathf.Max(0f, minViewRadius);
        maxViewRadius = Mathf.Max(minViewRadius, maxViewRadius);

        if (!Application.isPlaying)
        {
            ApplyShapeNow();
            ApplyRotationImmediate();
        }
    }

    private void Update()
    {
        if (!Application.isPlaying)
            return;

        UpdateRotation(Time.deltaTime);
        ApplyShapeNow();
    }

    // ------------------------------
    // Public API
    // ------------------------------

    public void SetVisionLevel01(float t)
    {
        visionLevel01 = Mathf.Clamp01(t);
    }

    public void SetExternalDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            externalDirection = dir.normalized;
    }

    // ------------------------------
    // Internal helpers
    // ------------------------------

    private void CacheRefs()
    {
        if (_light2D == null)
            _light2D = GetComponent<Light2D>();

        if (Application.isPlaying && _cam == null)
            _cam = Camera.main;
    }

    private void UpdateRotation(float deltaTime)
    {
        Vector2 dir;

        if (lookAtMouse && _cam != null)
        {
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 origin = transform.position;
            dir = mouseWorld - (Vector3)origin;

            if (dir.sqrMagnitude < 0.0001f)
                return;

            dir.Normalize();
        }
        else
        {
            if (externalDirection.sqrMagnitude < 0.0001f)
                return;

            dir = externalDirection.normalized;
        }

        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;

        if (rotationSmoothing <= 0f)
        {
            // Instant snap
            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);
        }
        else
        {
            // Smoothly rotate with a max degrees-per-second
            float currentAngle = transform.eulerAngles.z;
            float maxDelta = rotationSmoothing * deltaTime;
            float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, maxDelta);
            transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
        }
    }

    private void ApplyRotationImmediate()
    {
        Vector2 dir;

        if (lookAtMouse && _cam != null && Application.isPlaying)
        {
            Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f;
            Vector2 origin = transform.position;
            dir = mouseWorld - (Vector3)origin;

            if (dir.sqrMagnitude < 0.0001f)
                dir = Vector2.right;
            else
                dir.Normalize();
        }
        else
        {
            dir = externalDirection.sqrMagnitude > 0.0001f
                ? externalDirection.normalized
                : Vector2.right;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + rotationOffset;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    private void ApplyShapeNow()
    {
        if (_light2D == null)
            return;

        float radius = Mathf.Lerp(minViewRadius, maxViewRadius, visionLevel01);

        _light2D.pointLightOuterRadius = radius;
        _light2D.pointLightInnerRadius = radius * innerRadiusFraction;

        _light2D.pointLightOuterAngle = viewAngle;
        _light2D.pointLightInnerAngle = viewAngle * innerAngleFraction;
    }
}
