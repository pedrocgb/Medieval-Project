using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteAlways]
[RequireComponent(typeof(Light2D))]
public class PlayerVisionLight : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] private float maxViewRadius = 8f;
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] private float minViewRadius = 3f;

    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] [Range(0f, 360f)] public float viewAngle = 120f;
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] [Range(0f, 1f)] public float innerRadiusFraction = 0.5f;
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] [Range(0f, 1f)] public float innerAngleFraction = 0.3f;

    [FoldoutGroup("Orientation")]
    [SerializeField] private bool lookAtMouse = true;
    [FoldoutGroup("Orientation")]
    [SerializeField] private float rotationSmoothing = 720f;
    public float RotationSmoothing { get { return rotationSmoothing; } set { rotationSmoothing = value; } }
    [FoldoutGroup("Orientation")]
    [SerializeField] private float rotationOffset = -90f;

    [FoldoutGroup("Orientation")]
    [SerializeField] [ReadOnly] private float visionLevel01 = 1f;

    [FoldoutGroup("Orientation")]
    [SerializeField] private Vector2 externalDirection = Vector2.right;

    private Light2D _light2D;
    private Camera _cam;
    #endregion

    // ==============================================================

    #region Unity Callbacks
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
    #endregion

    // ==============================================================

    #region Setters
    /// <summary>
    /// Sets the vision level in the range [0, 1].
    /// </summary>
    /// <param name="t"></param>
    public void SetVisionLevel01(float t)
    {
        visionLevel01 = Mathf.Clamp01(t);
    }

    /// <summary>
    /// Sets the external direction for the light to face.
    /// </summary>
    /// <param name="dir"></param>
    public void SetExternalDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.0001f)
            externalDirection = dir.normalized;
    }
    #endregion

    // ==============================================================

    #region Private Methods
    /// <summary>
    /// Caches component references.
    /// </summary>
    private void CacheRefs()
    {
        if (_light2D == null)
            _light2D = GetComponent<Light2D>();

        if (Application.isPlaying && _cam == null)
            _cam = Camera.main;
    }

    /// <summary>
    /// Updates the rotation of the light.
    /// </summary>
    /// <param name="deltaTime"></param>
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

    /// <summary>
    /// Applies rotation immediately (used in edit mode).
    /// </summary>
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

    /// <summary>
    /// Applies the shape of the light based on vision level.
    /// </summary>
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
    #endregion

    // ==============================================================
}
