using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum TooltipAnchor
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft
}

public class ItemTooltipUI : MonoBehaviour
{
    public static ItemTooltipUI Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI typeText;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Positioning")]
    [Tooltip("Where the tooltip appears relative to the mouse position.")]
    [SerializeField] private TooltipAnchor anchor = TooltipAnchor.TopRight;

    [Tooltip("Padding from the mouse position in UI units (pixels).")]
    [SerializeField] private Vector2 padding = new Vector2(16f, 16f);

    [Header("Sizing")]
    [Tooltip("Minimum width for the tooltip panel.")]
    [SerializeField] private float minWidth = 220f;

    [Tooltip("Maximum width for the tooltip panel.")]
    [SerializeField] private float maxWidth = 340f;

    [Tooltip("Extra horizontal padding added on top of the widest text.")]
    [SerializeField] private float contentHorizontalPadding = 32f;

    private Canvas _canvas;
    private RectTransform _canvasRect;

    private ItemInstance _currentItem;
    private bool _isVisible;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _canvas = GetComponentInParent<Canvas>();
        _canvasRect = _canvas.GetComponent<RectTransform>();

        Hide();
    }

    private void Update()
    {
        // Follow mouse only while visible
        if (!_isVisible || panel == null)
            return;

        UpdatePosition(Input.mousePosition);
    }

    public void Show(ItemInstance item, Vector2 initialScreenPosition)
    {
        if (item == null || item.Definition == null || panel == null)
            return;

        // Do not show tooltip if context menu is open
        if (ItemContextMenuUI.Instance != null && ItemContextMenuUI.Instance.IsOpen)
            return;

        _currentItem = item;

        var def = item.Definition;

        string displayName = string.IsNullOrEmpty(def.DisplayName) ? def.name : def.DisplayName;
        string typeName = string.IsNullOrEmpty(def.TypeName) ? "" : def.TypeName;
        string desc = string.IsNullOrEmpty(def.Description) ? "" : def.Description;

        if (nameText != null)
            nameText.text = displayName;

        if (typeText != null)
            typeText.text = typeName;

        if (descriptionText != null)
            descriptionText.text = desc;

        // First: set panel to maxWidth so TMP can calculate preferredWidth freely
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxWidth);

        // Force layout update so preferredWidth is correct
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        // Now measure the widest text
        float maxContentWidth = 0f;

        if (nameText != null)
            maxContentWidth = Mathf.Max(maxContentWidth, nameText.preferredWidth);

        if (typeText != null && !string.IsNullOrEmpty(typeName))
            maxContentWidth = Mathf.Max(maxContentWidth, typeText.preferredWidth);

        if (descriptionText != null && !string.IsNullOrEmpty(desc))
            maxContentWidth = Mathf.Max(maxContentWidth, descriptionText.preferredWidth);

        // Add padding and clamp within min/max
        float targetWidth = Mathf.Clamp(
            maxContentWidth + contentHorizontalPadding,
            minWidth,
            maxWidth
        );

        // Apply final width
        panel.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, targetWidth);

        // Rebuild again so height fits new width
        LayoutRebuilder.ForceRebuildLayoutImmediate(panel);

        panel.gameObject.SetActive(true);
        _isVisible = true;

        // Position immediately to avoid 1-frame lag
        UpdatePosition(initialScreenPosition);
    }

    public void Hide()
    {
        _isVisible = false;
        _currentItem = null;

        if (panel != null)
            panel.gameObject.SetActive(false);
    }

    private void UpdatePosition(Vector2 screenPosition)
    {
        if (panel == null || _canvasRect == null)
            return;

        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect, screenPosition, cam, out var localPoint))
        {
            Vector2 offset = GetOffsetForAnchor();
            panel.anchoredPosition = localPoint + offset;
        }
    }

    private Vector2 GetOffsetForAnchor()
    {
        switch (anchor)
        {
            case TooltipAnchor.TopRight:
                return new Vector2(padding.x, padding.y);
            case TooltipAnchor.TopLeft:
                return new Vector2(-padding.x, padding.y);
            case TooltipAnchor.BottomRight:
                return new Vector2(padding.x, -padding.y);
            case TooltipAnchor.BottomLeft:
                return new Vector2(-padding.x, -padding.y);
            default:
                return new Vector2(padding.x, padding.y);
        }
    }
}
