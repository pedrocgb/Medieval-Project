using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal; // Needed for Light2D

public class DayNightCycle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light2D globalLight;

    [Header("Time Settings")]
    [Tooltip("How long (in real-time seconds) a full in-game day takes.")]
    [SerializeField] private float dayLengthInSeconds = 600f; // 10 min by default

    [Tooltip("Start time of day in hours (0-24). 0 = midnight, 6 = 6 AM, 12 = noon, etc.")]
    [Range(0f, 24f)]
    [SerializeField] private float startHour = 8f;

    [Tooltip("Automatically advance time of day.")]
    [SerializeField] private bool autoAdvance = true;

    [Tooltip("Use unscaled time (ignores Time.timeScale).")]
    [SerializeField] private bool useUnscaledTime = false;

    [Header("Debug / Info (0-1)")]
    [Range(0f, 1f)]
    [SerializeField][ReadOnly] private float timeOfDay01 = 0f; // 0-1 representation of day time

    [Header("Visual Settings")]
    [Tooltip("Color of the global light over the day (x-axis is timeOfDay01 0-1).")]
    [SerializeField] private Gradient lightColorOverDay;

    [Tooltip("Intensity curve of the global light over the day (x-axis is timeOfDay01 0-1).")]
    [SerializeField]
    private AnimationCurve lightIntensityOverDay =
        AnimationCurve.Linear(0f, 0.2f, 1f, 0.2f);

    [Header("Debug Time (24h style)")]
    [SerializeField] [ReadOnly] private int currentHour;        // 0-23
    [SerializeField] [ReadOnly] private int currentMinute;      // 0-59
    [SerializeField] [ReadOnly] private int currentSecond;      // 0-59
    [SerializeField] [ReadOnly] private string currentTimeText; // "HH:MM:SS"

    [Header("On-Screen Debug Clock")]
    [SerializeField] private bool showDebugClock = false;
    [SerializeField] private TextMeshProUGUI _debugClock;

    private void Reset()
    {
        // Simple default gradient (midnight blue -> dawn -> day -> dusk -> night)
        lightColorOverDay = new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(new Color(0.02f, 0.02f, 0.1f), 0f),   // midnight
                new GradientColorKey(new Color(0.8f, 0.4f, 0.2f), 0.25f),  // sunrise
                new GradientColorKey(new Color(1f, 1f, 0.95f), 0.5f),      // midday
                new GradientColorKey(new Color(1f, 0.5f, 0.2f), 0.75f),    // sunset
                new GradientColorKey(new Color(0.02f, 0.02f, 0.1f), 1f)    // midnight
            },
            alphaKeys = new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
        };

        lightIntensityOverDay = new AnimationCurve(
            new Keyframe(0f, 0.2f),   // midnight
            new Keyframe(0.25f, 0.7f),// sunrise
            new Keyframe(0.5f, 1f),   // midday
            new Keyframe(0.75f, 0.7f),// sunset
            new Keyframe(1f, 0.2f));  // midnight
    }

    private void Start()
    {
        SetTimeOfDayHours(startHour);
        ApplyLighting();
        UpdateDebugTime();
    }

    private void Update()
    {
        if (autoAdvance)
        {
            float delta = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (dayLengthInSeconds <= 0.01f)
                dayLengthInSeconds = 0.01f;

            float dayFractionDelta = delta / dayLengthInSeconds; // how much of the day passes this frame
            timeOfDay01 += dayFractionDelta;
            if (timeOfDay01 > 1f)
                timeOfDay01 -= 1f; // loop day
        }

        ApplyLighting();
        UpdateDebugTime();
    }

    private void ApplyLighting()
    {
        if (globalLight == null) return;

        if (lightColorOverDay != null)
            globalLight.color = lightColorOverDay.Evaluate(timeOfDay01);

        if (lightIntensityOverDay != null && lightIntensityOverDay.keys.Length > 0)
            globalLight.intensity = lightIntensityOverDay.Evaluate(timeOfDay01);
    }

    /// <summary>
    /// Set time of day with a 0-1 value. 0 = midnight, 0.5 = noon, 1 = midnight again.
    /// </summary>
    public void SetTimeOfDay01(float value)
    {
        timeOfDay01 = Mathf.Repeat(value, 1f);
        ApplyLighting();
        UpdateDebugTime();
    }

    /// <summary>
    /// Set time of day in in-game hours (0-24). 0 = midnight, 6 = 6 AM, 12 = noon, etc.
    /// </summary>
    public void SetTimeOfDayHours(float hour)
    {
        hour = Mathf.Repeat(hour, Constants.HOURS_PER_DAY);
        timeOfDay01 = hour / Constants.HOURS_PER_DAY;
        ApplyLighting();
        UpdateDebugTime();
    }

    /// <summary>
    /// Get time of day in in-game hours (0-24).
    /// </summary>
    public float GetTimeOfDayHours()
    {
        return timeOfDay01 * Constants.HOURS_PER_DAY;
    }

    /// <summary>
    /// Get current in-game time as a formatted string "HH:MM:SS".
    /// </summary>
    public string GetCurrentTimeText()
    {
        return currentTimeText;
    }

    private void UpdateDebugTime()
    {
        // total in-game seconds in a 24h day
        float totalSecondsInDay = Constants.HOURS_PER_DAY * Constants.MINUTES_PER_HOUR * Constants.SECONDS_PER_MINUTE;

        // current in-game seconds since midnight
        float secondsSinceMidnight = timeOfDay01 * totalSecondsInDay;

        currentHour = Mathf.FloorToInt(secondsSinceMidnight / (Constants.MINUTES_PER_HOUR * Constants.SECONDS_PER_MINUTE));
        secondsSinceMidnight -= currentHour * Constants.MINUTES_PER_HOUR * Constants.SECONDS_PER_MINUTE;

        currentMinute = Mathf.FloorToInt(secondsSinceMidnight / Constants.SECONDS_PER_MINUTE);
        secondsSinceMidnight -= currentMinute * Constants.SECONDS_PER_MINUTE;

        currentSecond = Mathf.FloorToInt(secondsSinceMidnight);

        currentHour = Mathf.Clamp(currentHour, 0, 23);
        currentMinute = Mathf.Clamp(currentMinute, 0, 59);
        currentSecond = Mathf.Clamp(currentSecond, 0, 59);

        currentTimeText = $"{currentHour:00}:{currentMinute:00}:{currentSecond:00}";
        if (showDebugClock)
            _debugClock.text = currentTimeText;
    }
}
