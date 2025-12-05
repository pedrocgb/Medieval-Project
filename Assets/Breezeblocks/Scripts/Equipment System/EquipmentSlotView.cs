using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;

public class EquipmentSlotView : MonoBehaviour,
      IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables and Properties
    [FoldoutGroup("Slot", expanded: true)]
    [SerializeField] EquipmentSlot slot;
    public EquipmentSlot Slot => slot;

    [FoldoutGroup("Player's Equipment Manager", expanded: true)]
    [SerializeField] private EquipmentManager equipmentManager;
    public EquipmentManager EquipmentManager => equipmentManager;

    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image iconImage;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Sprite emptySprite;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private RectTransform iconRect;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private CanvasGroup iconCanvasGroup;

    // Drag state
    private bool _isDragging;
    private Vector2 _originalAnchoredPos;
    private Vector2 _pointerOffset;
    private Vector2 _lastDragScreenPos;
    private Camera _lastDragCamera;

    private InventoryGridView _currentHoverView;
    #endregion

    // ==============================================================


    private void OnEnable()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged += Refresh;
        }

        // Initial refresh
        Refresh();
    }

    private void OnDisable()
    {
        if (equipmentManager != null)
        {
            equipmentManager.OnEquipmentChanged -= Refresh;
        }
    }

    // ==============================================================

    /// <summary>
    /// Refreshes the icon based on the currently equipped item.
    /// </summary>
    public void Refresh()
    {
        if (iconImage == null || equipmentManager == null)
            return;

        var item = equipmentManager.GetEquipped(slot);

        if (item != null && item.Definition != null && item.Definition.Icon != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = item.Definition.Icon;
        }
        else
        {
            if (emptySprite != null)
            {
                iconImage.enabled = true;
                iconImage.sprite = emptySprite;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        // Reset icon position within the slot
        if (iconRect != null)
        {
            iconRect.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Gets the currently equipped item in this slot.
    /// </summary>
    /// <returns></returns>
    private ItemInstance GetEquippedItem()
    {
        return equipmentManager != null ? equipmentManager.GetEquipped(slot) : null;
    }

    // ==============================================================

    #region Drag Methods
    /// <summary>
    /// Begins dragging the equipped item icon.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnBeginDrag(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
        var item = GetEquippedItem();
        if (item == null || iconRect == null)
            return;

        _isDragging = true;
        _currentHoverView = null;

        _originalAnchoredPos = iconRect.anchoredPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                iconRect, eventData.position, eventData.pressEventCamera, out var localPoint))
        {
            _pointerOffset = localPoint;
        }

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.blocksRaycasts = false;
            iconCanvasGroup.alpha = 0.8f;
        }

        _lastDragScreenPos = eventData.position;
        _lastDragCamera = eventData.pressEventCamera;
    }
    /// <summary>
    /// Drags the equipped item icon.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || iconRect == null)
            return;

        var parentRect = iconRect.parent as RectTransform;
        if (parentRect == null)
            return;

        // Move icon visually
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out var localPoint))
        {
            Vector2 newAnchoredPos = localPoint - _pointerOffset;
            iconRect.anchoredPosition = newAnchoredPos;
        }

        _lastDragScreenPos = eventData.position;
        _lastDragCamera = eventData.pressEventCamera;

        // === NEW: grid hover + preview ===
        var item = GetEquippedItem();
        if (item == null)
            return;

        // Which inventory grid are we over now?
        InventoryGridView newHoverView = null;
        if (eventData.pointerEnter != null)
        {
            newHoverView = eventData.pointerEnter.GetComponentInParent<InventoryGridView>();
        }

        if (newHoverView != _currentHoverView)
        {
            _currentHoverView?.ClearHighlights();
            _currentHoverView = newHoverView;
        }

        if (_currentHoverView != null)
        {
            _currentHoverView.PreviewPlacement(
                item,
                item.Rotation,
                _lastDragScreenPos,
                _lastDragCamera
            );
        }
    }
    /// <summary>
    /// Ends dragging the equipped item icon.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging)
            return;

        _isDragging = false;

        if (iconCanvasGroup != null)
        {
            iconCanvasGroup.blocksRaycasts = true;
            iconCanvasGroup.alpha = 1f;
        }

        var item = GetEquippedItem();
        if (item == null)
        {
            Refresh();
            if (iconRect != null)
                iconRect.anchoredPosition = _originalAnchoredPos;

            if (_currentHoverView != null)
            {
                _currentHoverView.ClearHighlights();
                _currentHoverView = null;
            }
            return;
        }

        InventoryGridView targetView = null;
        if (eventData.pointerEnter != null)
        {
            targetView = eventData.pointerEnter.GetComponentInParent<InventoryGridView>();
        }

        if (targetView != null && targetView.GridSource != null)
        {
            var targetGrid = targetView.GridSource.Grid;
            if (targetGrid != null &&
                targetView.TryGetCellFromScreenPoint(
                    eventData.position,
                    eventData.pressEventCamera,
                    out int cellX,
                    out int cellY))
            {
                bool success = equipmentManager.TryUnequipToInventory(slot, targetGrid, cellX, cellY);

                if (success)
                {
                    targetView.Rebuild();
                    Refresh();

                    if (_currentHoverView != null)
                    {
                        _currentHoverView.ClearHighlights();
                        _currentHoverView = null;
                    }
                    return;
                }
            }
        }

        // No valid inventory / failed to unequip: snap icon back
        if (iconRect != null)
        {
            iconRect.anchoredPosition = _originalAnchoredPos;
        }

        if (_currentHoverView != null)
        {
            _currentHoverView.ClearHighlights();
            _currentHoverView = null;
        }
    }
    #endregion

    // ==============================================================

    #region On Pointer Methods
    /// <summary>
    /// Shows the tooltip for the equipped item.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        var item = GetEquippedItem();
        if (item == null) return;

        ItemTooltipUI.Instance?.Show(item, eventData.position);
    }
    /// <summary>
    /// Hides the tooltip.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }
    /// <summary>
    /// Handles right-click to show the context menu for this equipment slot.
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        var menu = ItemContextMenuUI.Instance;
        if (menu == null)
            return;

        if (menu.IsOpenForEquipmentSlot(this))
        {
            menu.Hide();
        }
        else
        {
            menu.ShowForEquipmentSlot(this, eventData.position);
        }
    }
    #endregion

    // ==============================================================
}
