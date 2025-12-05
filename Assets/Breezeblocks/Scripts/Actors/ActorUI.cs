using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ActorUI : MonoBehaviour
{
    #region Variables and Properties
    private Canvas _worldCanvas = null;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private GameObject _bowIndicatorObject = null;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image _bowDrawIndicator = null;
    #endregion

    // ======================================================================

    private void Start()
    {
        if (_worldCanvas != null && _worldCanvas.worldCamera == null)
            _worldCanvas.worldCamera = Camera.main;
    }

    // ======================================================================

    #region Bow Methods
    public void EnableBowIndicator(bool enabled)
    {
        _bowIndicatorObject.SetActive(enabled);
    }

    public void UpdateBowIndicator(float percentage)
    {
        _bowDrawIndicator.fillAmount = percentage;
    }
    #endregion

    // ======================================================================
}
