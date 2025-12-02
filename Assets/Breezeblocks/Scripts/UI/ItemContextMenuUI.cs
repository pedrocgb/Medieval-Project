using UnityEngine;
using UnityEngine.UI;
using System;

public enum ContextMenuAnchor
{
    TopRight,
    TopLeft,
    BottomRight,
    BottomLeft
}

public class ItemContextMenuUI : MonoBehaviour
{
    public static ItemContextMenuUI Instance { get; private set; }

    [Header("Refs")]
    [SerializeField] private RectTransform panel;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button unequipButton;
    [SerializeField] private Button dropButton;

    [Header("Managers")]
    [SerializeField] private EquipmentManager equipmentManager;
    [SerializeField] private InventoryGridBehaviour mainInventory; // for auto-unequip/drop fallback

    [Header("Positioning")]
    [Tooltip("Where the context menu appears relative to the mouse position.")]
    [SerializeField] private ContextMenuAnchor anchor = ContextMenuAnchor.BottomRight;

    [Tooltip("Padding from the mouse position in UI units (pixels).")]
    [SerializeField] private Vector2 padding = new Vector2(12f, 12f);

    // Current context
    private ItemInstance _item;
    private InventoryItemView _inventoryView;      // if opened from inventory
    private EquipmentSlotView _equipmentSlotView;  // if opened from equipment
    public bool IsOpen => panel != null && panel.gameObject.activeSelf;

    private Canvas _canvas;
    private RectTransform _canvasRect;

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

        // Wire button handlers
        if (equipButton != null) equipButton.onClick.AddListener(OnEquipClicked);
        if (unequipButton != null) unequipButton.onClick.AddListener(OnUnequipClicked);
        if (dropButton != null) dropButton.onClick.AddListener(OnDropClicked);
    }

    public void ShowForInventoryItem(InventoryItemView view, Vector2 screenPosition)
    {
        if (view == null) return;
        _inventoryView = view;
        _equipmentSlotView = null;
        _item = view.Item;

        if (_item == null || _item.Definition == null)
            return;

        SetupButtonsForInventoryItem();

        // ✅ Hide tooltip when opening context menu
        ItemTooltipUI.Instance?.Hide();

        PositionAt(screenPosition);
        panel.gameObject.SetActive(true);
    }


    public void ShowForEquipmentSlot(EquipmentSlotView slotView, Vector2 screenPosition)
    {
        if (slotView == null || slotView.equipmentManager == null)
            return;

        _inventoryView = null;
        _equipmentSlotView = slotView;
        _item = slotView.equipmentManager.GetEquipped(slotView.slot);

        if (_item == null || _item.Definition == null)
            return;

        SetupButtonsForEquipmentItem();

        // ✅ Hide tooltip when opening context menu
        ItemTooltipUI.Instance?.Hide();

        PositionAt(screenPosition);
        panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        if (panel != null)
            panel.gameObject.SetActive(false);

        _item = null;
        _inventoryView = null;
        _equipmentSlotView = null;
    }

    private void PositionAt(Vector2 screenPosition)
    {
        if (panel == null || _canvasRect == null)
            return;

        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvasRect,
                screenPosition,
                cam,
                out var localPoint))
        {
            Vector2 offset = GetOffsetForAnchor();
            panel.anchoredPosition = localPoint + offset;
        }
    }

    private Vector2 GetOffsetForAnchor()
    {
        switch (anchor)
        {
            case ContextMenuAnchor.TopRight:
                return new Vector2(padding.x, padding.y);
            case ContextMenuAnchor.TopLeft:
                return new Vector2(-padding.x, padding.y);
            case ContextMenuAnchor.BottomRight:
                return new Vector2(padding.x, -padding.y);
            case ContextMenuAnchor.BottomLeft:
                return new Vector2(-padding.x, -padding.y);
            default:
                return new Vector2(padding.x, -padding.y);
        }
    }

    private void SetupButtonsForInventoryItem()
    {
        bool equipable = _item.Definition.Equipable;
        if (equipButton != null)
            equipButton.gameObject.SetActive(equipable);

        if (unequipButton != null)
            unequipButton.gameObject.SetActive(false); // can't unequip from inventory

        if (dropButton != null)
            dropButton.gameObject.SetActive(true);
    }

    private void SetupButtonsForEquipmentItem()
    {
        if (equipButton != null)
            equipButton.gameObject.SetActive(false);

        if (unequipButton != null)
            unequipButton.gameObject.SetActive(true);

        if (dropButton != null)
            dropButton.gameObject.SetActive(true);
    }

    // ----- Button handlers -----

    private void OnEquipClicked()
    {
        if (_inventoryView == null || _item == null || equipmentManager == null)
        {
            Hide();
            return;
        }

        var gridView = _inventoryView.GridView;
        var gridBehaviour = gridView != null ? gridView.gridSource : null;
        var fromGrid = gridBehaviour != null ? gridBehaviour.Grid : null;

        if (fromGrid == null)
        {
            Hide();
            return;
        }

        bool success = equipmentManager.TryAutoEquipFromInventory(_item, fromGrid);
        if (success)
        {
            gridView.Rebuild();
        }

        Hide();
    }

    private void OnUnequipClicked()
    {
        if (_equipmentSlotView == null || _item == null)
        {
            Hide();
            return;
        }

        var mgr = _equipmentSlotView.equipmentManager;
        if (mgr == null)
        {
            Hide();
            return;
        }

        if (mainInventory == null)
        {
            Debug.LogWarning("ItemContextMenu: mainInventory not set, cannot auto-unequip.");
            Hide();
            return;
        }

        var grid = mainInventory.Grid;
        if (grid == null)
        {
            Hide();
            return;
        }

        if (grid.TryFindSpaceFor(_item, out int x, out int y))
        {
            bool success = mgr.TryUnequipToInventory(_equipmentSlotView.slot, grid, x, y);
            if (success && mainInventory.gridView != null)
            {
                mainInventory.gridView.Rebuild();
            }
        }
        else
        {
            Debug.Log("No space in main inventory to unequip item.");
        }

        Hide();
    }

    private void OnDropClicked()
    {
        if (_inventoryView != null && _item != null)
        {
            var gridView = _inventoryView.GridView;
            var gridBehaviour = gridView != null ? gridView.gridSource : null;
            var grid = gridBehaviour != null ? gridBehaviour.Grid : null;

            if (grid != null)
            {
                grid.Remove(_item);
                gridView.Rebuild();
            }
        }
        else if (_equipmentSlotView != null && _item != null)
        {
            var mgr = _equipmentSlotView.equipmentManager;
            if (mgr != null)
            {
                var removed = mgr.Equipment.Unequip(_equipmentSlotView.slot);
                if (removed != null)
                {
                    mgr.NotifyEquipmentChanged();
                }
            }
        }

        Hide();
    }

    public bool IsOpenForInventoryItem(InventoryItemView view)
    {
        return IsOpen && _inventoryView == view;
    }

    public bool IsOpenForEquipmentSlot(EquipmentSlotView slotView)
    {
        return IsOpen && _equipmentSlotView == slotView;
    }
}
