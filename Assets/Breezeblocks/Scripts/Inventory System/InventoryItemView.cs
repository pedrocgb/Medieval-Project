using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Sirenix.OdinInspector;

/// <summary>
/// Visual representation of an ItemInstance in the UI.
/// Handles dragging, rotation preview, and placement preview.
/// </summary>
public class InventoryItemView : MonoBehaviour,
    IBeginDragHandler, IDragHandler, IEndDragHandler,
    IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private Image iconImage;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private TextMeshProUGUI stackText;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private CanvasGroup canvasGroup;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private RectTransform rectTransform;
    private RectTransform dragRoot;

    private Vector2 _originalAnchoredPos;
    private Transform _originalParent;

    private ItemInstance _item;
    private RectTransform _rectTransform;
    private CanvasGroup _canvasGroup;
    private InventoryGridView _gridView;
    private InventoryGridView _currentHoverView;
    private RectTransform _mainCanvas;

    private float _originalCellX;
    private float _originalCellY;

    // Drag state
    private Vector2 _originalAnchoredPosition;
    private bool _isDragging;
    private Vector2 _pointerOffset;

    // 🔹 Single rotation source used during drag and preview
    private int _dragRotation;

    // Rotation preview while dragging (no longer separate)
    private float _cellSize;

    // For preview updates
    private Vector2 _lastDragScreenPos;
    private Camera _lastDragCamera;

    public ItemInstance Item => _item;
    public InventoryGridView GridView => _gridView;
    #endregion

    // ======================================================================

    private void Awake()
    {
        _rectTransform = (RectTransform)transform;
        _canvasGroup = GetComponent<CanvasGroup>();
        _gridView = GetComponentInParent<InventoryGridView>();
        _currentHoverView = _gridView;
        _mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas")?.GetComponent<RectTransform>();
    }

    private void Update()
    {
        if (!_isDragging || _item == null || _currentHoverView == null) return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            // 🔹 Rotate using _dragRotation (only one variable now)
            _dragRotation = (_dragRotation + 90) % 180;
            UpdateIconSizeForRotation();
            UpdateIconRotation();

            // Re-run preview at the same screen position
            _currentHoverView.PreviewPlacement(
                _item,
                _dragRotation,
                _lastDragScreenPos,
                _lastDragCamera  // can be null for overlay canvas, that's fine
            );
        }
    }

    // ======================================================================

    #region Item Handler
    /// <summary>
    /// Bind this view to a specific item and position it in the grid.
    /// </summary>
    public void Bind(ItemInstance item, float cellSize)
    {
        _item = item;
        _cellSize = cellSize;

        if (_rectTransform == null)
            _rectTransform = (RectTransform)transform;

        _rectTransform.anchorMin = new Vector2(0f, 1f);
        _rectTransform.anchorMax = new Vector2(0f, 1f);
        _rectTransform.pivot = new Vector2(0f, 1f);

        // 🔹 Initialize drag rotation from item's current rotation
        _dragRotation = item.Rotation;
        ApplySizeForRotation(_dragRotation);
        UpdateIconRotation();

        float xPos = item.X * cellSize;
        float yPos = item.Y * cellSize;
        _rectTransform.anchoredPosition = new Vector2(xPos, -yPos);

        _originalCellX = item.X;
        _originalCellY = item.Y;

        if (iconImage != null && item.Definition != null)
        {
            iconImage.sprite = item.Definition.Icon;
            iconImage.preserveAspect = true;
        }

        // Stack text
        if (stackText != null)
        {
            if (item.Definition != null && item.Definition.Stackable)
            {
                if (item.StackCount > 1)
                {
                    stackText.text = item.StackCount.ToString();
                    stackText.gameObject.SetActive(true);
                }
                else
                {
                    stackText.gameObject.SetActive(false);
                }
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Change object size for current rotation
    /// </summary>
    private void ApplySizeForRotation(int rotation)
    {
        if (_item == null || _item.Definition == null || _rectTransform == null)
            return;

        int w = (rotation % 180 == 0) ? _item.Definition.Width : _item.Definition.Height;
        int h = (rotation % 180 == 0) ? _item.Definition.Height : _item.Definition.Width;

        _rectTransform.sizeDelta = new Vector2(w * _cellSize, h * _cellSize);
    }

    private void UpdateIconSizeForRotation()
    {
        if (rectTransform == null || _item == null || _item.Definition == null)
            return;

        int srcW = _item.Definition.Width;
        int srcH = _item.Definition.Height;

        // 🔹 Use _dragRotation consistently
        int w = (_dragRotation % 180 == 0) ? srcW : srcH;
        int h = (_dragRotation % 180 == 0) ? srcH : srcW;

        rectTransform.sizeDelta = new Vector2(w * _gridView.CellSize, h * _gridView.CellSize);
    }

    private void UpdateIconRotation()
    {
        if (iconImage == null) return;

        // rotate sprite in 90° increments
        iconImage.rectTransform.localEulerAngles = new Vector3(0, 0, -_dragRotation);

        // NOTE: we still rely on UpdateIconSizeForRotation() to resize correctly.
    }
    #endregion

    // ======================================================================

    #region Drag Callbacks
    public void OnBeginDrag(PointerEventData eventData)
    {
        // hide tooltip when starting drag
        ItemTooltipUI.Instance?.Hide();

        if (_item == null || rectTransform == null)
            return;

        _isDragging = true;
        _currentHoverView = null;

        // 🔹 Start drag rotation from the item's current rotation
        _dragRotation = _item.Rotation;

        _originalParent = rectTransform.parent;
        _originalAnchoredPos = rectTransform.anchoredPosition;

        // Decide where the item will live while dragging
        RectTransform parentRect;
        if (dragRoot != null)
        {
            parentRect = dragRoot;
            rectTransform.SetParent(dragRoot, worldPositionStays: false);
        }
        else
        {
            parentRect = _originalParent as RectTransform;
        }

        // Compute pointer offset in parent space
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out var localPoint))
        {
            // Offset from where the item is to where the mouse is, in parent space
            _pointerOffset = localPoint - rectTransform.anchoredPosition;
        }

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0.8f;
        }

        _lastDragScreenPos = eventData.position;
        _lastDragCamera = eventData.pressEventCamera;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging || rectTransform == null)
            return;

        var parentRect = rectTransform.parent as RectTransform;
        if (parentRect == null)
            return;

        // Move item in the parent's local space, using stored offset
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect, eventData.position, eventData.pressEventCamera, out var localPoint))
        {
            rectTransform.anchoredPosition = localPoint - _pointerOffset;
        }

        _lastDragScreenPos = eventData.position;
        _lastDragCamera = eventData.pressEventCamera;

        var item = _item;
        if (item == null)
            return;

        // Find which grid we're over
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
                _dragRotation,            // 🔹 always use _dragRotation for preview
                _lastDragScreenPos,
                _lastDragCamera
            );
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_isDragging || _item == null || _gridView == null)
        {
            _isDragging = false;
            _gridView?.ClearHighlights();
            _currentHoverView?.ClearHighlights();
            return;
        }

        _isDragging = false;

        if (_canvasGroup != null)
        {
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.alpha = 1f;
        }

        // 1) Check if we dropped on an equipment slot
        var equipSlotView = eventData.pointerEnter != null
            ? eventData.pointerEnter.GetComponentInParent<EquipmentSlotView>()
            : null;

        if (equipSlotView != null && equipSlotView.EquipmentManager != null)
        {
            var gridBehaviour = _gridView.GridSource;
            var fromGrid = gridBehaviour != null ? gridBehaviour.Grid : null;

            if (fromGrid != null)
            {
                bool equipped = equipSlotView.EquipmentManager.TryEquipFromInventory(
                    equipSlotView.Slot,
                    _item,
                    fromGrid
                );

                if (equipped)
                {
                    // Item was removed from the grid and equipped
                    _gridView.Rebuild();
                    _gridView.ClearHighlights();
                    _currentHoverView?.ClearHighlights();
                    return;
                }
            }

            // Equip failed (wrong tag / no space for swap / etc):
            // snap back to original position & rotation
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
            _dragRotation = _item.Rotation;
            ApplySizeForRotation(_dragRotation);

            _gridView.ClearHighlights();
            _currentHoverView?.ClearHighlights();
            return;
        }

        // 2) No equipment slot: fallback to inventory <-> inventory logic

        // Determine target inventory under pointer
        InventoryGridView targetView = null;
        if (eventData.pointerEnter != null)
        {
            targetView = eventData.pointerEnter.GetComponentInParent<InventoryGridView>();
        }
        if (targetView == null)
            targetView = _gridView;

        var sourceGridBehaviour = _gridView.GridSource;
        var sourceGrid = sourceGridBehaviour != null ? sourceGridBehaviour.Grid : null;

        var targetGridBehaviour = targetView.GridSource;
        var targetGrid = targetGridBehaviour != null ? targetGridBehaviour.Grid : null;

        bool handled = false;

        if (targetGrid != null &&
            targetView.TryGetCellFromScreenPoint(
                eventData.position,
                eventData.pressEventCamera,
                out int cellX,
                out int cellY))
        {
            var targetItem = targetGrid.GetItemAt(cellX, cellY);

            // 2.1 STACKING
            if (targetItem != null && targetItem != _item)
            {
                if (targetGrid == sourceGrid)
                {
                    // Same-grid stacking
                    if (targetGrid.TryStack(_item, targetItem))
                    {
                        targetView.Rebuild();
                        handled = true;
                    }
                }
                else
                {
                    // Cross-grid stacking
                    if (targetGrid.CanStack(_item, targetItem) && sourceGrid != null)
                    {
                        int capacityLeft = targetItem.Definition.MaxStack - targetItem.StackCount;
                        int amountToMove = Mathf.Min(capacityLeft, _item.StackCount);

                        targetItem.StackCount += amountToMove;
                        _item.StackCount -= amountToMove;

                        if (_item.StackCount <= 0)
                        {
                            sourceGrid.Remove(_item);
                        }

                        targetView.Rebuild();
                        _gridView.Rebuild();
                        handled = true;
                    }
                }
            }

            // 2.2 MOVE (if not already handled by stacking)
            if (!handled)
            {
                if (targetGrid == sourceGrid)
                {
                    // Same-grid move
                    bool moved = targetView.TryMoveItemWithRotation(_item, cellX, cellY, _dragRotation);
                    if (!moved)
                    {
                        _rectTransform.anchoredPosition = _originalAnchoredPosition;
                        _dragRotation = _item.Rotation;
                        ApplySizeForRotation(_dragRotation);
                    }
                }
                else
                {
                    // Cross-grid move
                    if (sourceGrid != null &&
                        targetGrid.CanPlaceWithRotation(_item, cellX, cellY, _dragRotation, ignoreSelf: false))
                    {
                        sourceGrid.Remove(_item);

                        // 🔹 Only now actually commit the new rotation to the item
                        _item.Rotation = _dragRotation;

                        bool placed = targetGrid.TryPlace(_item, cellX, cellY);
                        if (!placed)
                        {
                            Debug.LogWarning("Cross-grid move failed; restoring to source grid.");
                            if (sourceGrid.TryFindSpaceFor(_item, out int sx, out int sy))
                            {
                                sourceGrid.TryPlace(_item, sx, sy);
                            }
                        }
                        else
                        {
                            targetView.Rebuild();
                            _gridView.Rebuild();
                            handled = true;
                        }
                    }
                    else
                    {
                        _rectTransform.anchoredPosition = _originalAnchoredPosition;
                        _dragRotation = _item.Rotation;
                        ApplySizeForRotation(_dragRotation);
                    }
                }
            }
        }
        else
        {
            // Dropped outside any valid cell
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
            _dragRotation = _item.Rotation;
            ApplySizeForRotation(_dragRotation);
        }

        _gridView.ClearHighlights();
        if (targetView != _gridView)
            targetView.ClearHighlights();
    }
    #endregion

    // ======================================================================

    #region On Pointer Callbacks
    public void OnPointerDown(PointerEventData eventData)
    {
        // Reserved for future click / context menu.
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_item == null) return;
        ItemTooltipUI.Instance?.Show(_item, eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ItemTooltipUI.Instance?.Hide();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right)
            return;

        var menu = ItemContextMenuUI.Instance;
        if (menu == null)
            return;

        // If menu is already open for THIS item → close it (toggle)
        if (menu.IsOpenForInventoryItem(this))
        {
            menu.Hide();
        }
        else
        {
            menu.ShowForInventoryItem(this, eventData.position);
        }
    }
    #endregion

    // ======================================================================

}
