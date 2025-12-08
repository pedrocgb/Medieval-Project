using Sirenix.OdinInspector;
using UnityEngine;

public class AutoDisable : MonoBehaviour
{
    #region  Variables and Properties
    [FoldoutGroup("Settings", expanded: true)]
    [SerializeField] private bool _animationTimer = false;

    [FoldoutGroup("Settings", expanded: true), HideIf("_animationTimer")]
    [SerializeField] private float lifetime = 2f;

    [FoldoutGroup("Settings", expanded: true), ShowIf("_animationTimer")]
    [SerializeField] private Animator _animator;
    [FoldoutGroup("Settings", expanded: true), ShowIf("_animationTimer")]
    [SerializeField] private int _animatorLayerIndex = 0;
    #endregion

    // ======================================================================

    private void OnEnable()
    {
        if (!_animationTimer)
            Invoke(nameof(DisableSelf), lifetime);
        else 
            ScheduleDisableAtEndOfCurrentAnimation();
            
    }

    private void OnDisable()
    {
        // Clear pending Invoke when the object is turned off
        CancelInvoke();
    }

    // ======================================================================

    private void ScheduleDisableAtEndOfCurrentAnimation()
    {
        if (_animator == null) return;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(_animatorLayerIndex);

        // Total length in seconds of the current state
        float stateLength = stateInfo.length;

        // How far we are into the animation (0–1, can be >1 if looping)
        float normalizedTime = stateInfo.normalizedTime;

        // If it’s looping, use only the fractional part
        float normalized01 = normalizedTime % 1f;

        // Remaining portion of the animation
        float remainingNormalized = Mathf.Clamp01(1f - normalized01);

        // Remaining time in seconds
        float remainingTime = stateLength * remainingNormalized;

        // Adjust for Animator speed (speed 2.0 plays twice as fast, etc.)
        float speed = Mathf.Abs(_animator.speed);
        if (speed > 0f)
        {
            remainingTime /= speed;
        }
        else
        {
            // If speed is 0, animation isn't progressing → do nothing
            return;
        }

        Invoke(nameof(DisableSelf), remainingTime);
    }

    private void DisableSelf()
    {
        gameObject.SetActive(false);
    }
}
