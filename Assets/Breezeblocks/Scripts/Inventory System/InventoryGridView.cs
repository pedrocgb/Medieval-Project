using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI view for an InventoryGrid.
/// It draws background slots and item views, and handles visual previews.
/// </summary>
public class InventoryGridView : MonoBehaviour
{
    [Header("References")]
    [Tooltip("Behaviour that owns the InventoryGrid (e.g. PlayerInventory, ChestInventory, etc.).")]
    public InventoryGridBehaviour gridSource;

    [Tooltip("RectTransform where the grid is drawn. This can be the same object as this component or a child.")]
    [SerializeField] private RectTransform content;

    [Header("Prefabs")]
    [Tooltip("Prefab for a single slot cell (background). Must have an Image and RectTransform.")]
    [SerializeField] private GameObject slotPrefab;

    [Tooltip("Prefab for an item view. Must have InventoryItemView component.")]
    [SerializeField] private InventoryItemView itemViewPrefab;

    [Header("Layout")]
    [Tooltip("Size of one grid cell in UI units (pixels).")]
    [SerializeField] private float cellSize = 64f;
    [SerializeField] private float  cellSpacing = 2f; // not used in this version, but kept for future

    [Header("Colors")]
    [SerializeField] private Color normalSlotColor = Color.white;
    [SerializeField] private Color validSlotColor = Color.green;
    [SerializeField] private Color invalidSlotColor = Color.red;

    private readonly List<GameObject> _slotInstances = new();
    private readonly List<InventoryItemView> _itemInstances = new();

    // 2D array to quickly access slot Images by [x,y]
    private Image[,] _slotImages;

    public float CellSize => cellSize;
    public InventoryGrid Grid => gridSource != null ? gridSource.Grid : null;

    public void Rebuild()
    {
        if (gridSource == null || gridSource.Grid == null)
        {
            Debug.LogWarning($"{name}: No gridSource or grid to rebuild from.");
            return;
        }

        if (content == null)
        {
            Debug.LogWarning($"{name}: InventoryGridView 'content' is not assigned.");
            return;
        }

        ClearChildren();

        var grid = gridSource.Grid;
        int width = grid.Width;
        int height = grid.Height;

        // Set the visual size of the content rect, but DO NOT touch anchors/pivot.
        float totalWidth = width * cellSize;
        float totalHeight = height * cellSize;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        _slotImages = new Image[width, height];

        // Cache rect & pivot for positioning math
        Rect rect = content.rect;
        Vector2 pivot = content.pivot;

        // 1) Build background slots
        if (slotPrefab != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject slotGO = Instantiate(slotPrefab, content);
                    _slotInstances.Add(slotGO);

                    RectTransform rt = slotGO.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.sizeDelta = new Vector2(cellSize, cellSize);

                        // Local "from left / from top" for this cell
                        float xFromLeft = x * cellSize + cellSize * 0.5f;
                        float yFromTop = y * cellSize + cellSize * 0.5f;

                        // Convert to anchoredPosition relative to pivot
                        float anchoredX = xFromLeft - rect.width * pivot.x;
                        float anchoredY = rect.height * (1f - pivot.y) - yFromTop;

                        rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
                    }

                    Image img = slotGO.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = normalSlotColor;
                        _slotImages[x, y] = img;
                    }
                }
            }
        }

        // 2) Build item views
        if (itemViewPrefab != null)
        {
            foreach (var item in grid.Items)
            {
                InventoryItemView itemView = Instantiate(itemViewPrefab, content);
                _itemInstances.Add(itemView);
                itemView.Bind(item, cellSize);
            }
        }
    }

    private void ClearChildren()
    {
        foreach (var slot in _slotInstances)
        {
            if (slot != null)
                Destroy(slot);
        }
        _slotInstances.Clear();

        foreach (var itemView in _itemInstances)
        {
            if (itemView != null)
                Destroy(itemView.gameObject);
        }
        _itemInstances.Clear();

        _slotImages = null;
    }

    /// <summary>
    /// Converts a screen point (mouse/finger) into grid cell coordinates.
    /// Works with any pivot/anchor on 'content'.
    /// </summary>
    public bool TryGetCellFromScreenPoint(Vector2 screenPosition, Camera uiCamera, out int cellX, out int cellY)
    {
        cellX = -1;
        cellY = -1;

        if (content == null)
        {
            Debug.LogWarning($"{name}: InventoryGridView 'content' is not assigned. Cannot convert screen point.");
            return false;
        }

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                content, screenPosition, uiCamera, out var localPoint))
        {
            return false;
        }

        var grid = gridSource?.Grid;
        if (grid == null)
            return false;

        int width = grid.Width;
        int height = grid.Height;

        Rect rect = content.rect;
        Vector2 pivot = content.pivot;

        // Convert localPoint (pivot-relative) to "from left / from top" coordinates
        float xFromLeft = localPoint.x + rect.width * pivot.x;
        float yFromTop = rect.height * (1f - pivot.y) - localPoint.y;

        float gridWidthPx = width * cellSize;
        float gridHeightPx = height * cellSize;

        // Check if inside grid bounds
        if (xFromLeft < 0 || xFromLeft > gridWidthPx || yFromTop < 0 || yFromTop > gridHeightPx)
            return false;

        cellX = Mathf.FloorToInt(xFromLeft / cellSize);
        cellY = Mathf.FloorToInt(yFromTop / cellSize);

        if (cellX < 0 || cellY < 0 || cellX >= width || cellY >= height)
            return false;

        return true;
    }

    public bool TryMoveItem(ItemInstance item, int targetX, int targetY)
    {
        if (gridSource == null || gridSource.Grid == null || item == null)
            return false;

        bool moved = gridSource.Grid.TryMove(item, targetX, targetY);
        if (moved)
        {
            Rebuild();
        }
        return moved;
    }

    public bool TryMoveItemWithRotation(ItemInstance item, int targetX, int targetY, int targetRotation)
    {
        if (gridSource == null || gridSource.Grid == null || item == null)
            return false;

        bool moved = gridSource.Grid.TryMoveAndRotate(item, targetX, targetY, targetRotation);
        if (moved)
        {
            Rebuild();
        }
        return moved;
    }

    public void ClearHighlights()
    {
        if (_slotImages == null) return;

        int width = _slotImages.GetLength(0);
        int height = _slotImages.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                var img = _slotImages[x, y];
                if (img != null)
                    img.color = normalSlotColor;
            }
        }
    }

    public void PreviewPlacement(ItemInstance item, int rotation, Vector2 screenPosition, Camera uiCamera)
    {
        ClearHighlights();

        if (item == null || gridSource == null || gridSource.Grid == null || _slotImages == null)
            return;

        var grid = gridSource.Grid;
        var def = item.Definition;
        if (def == null) return;

        if (!TryGetCellFromScreenPoint(screenPosition, uiCamera, out int cellX, out int cellY))
            return;

        bool canPlace = grid.CanPlaceWithRotation(item, cellX, cellY, rotation, ignoreSelf: true);

        if (!canPlace)
        {
            var target = grid.GetItemAt(cellX, cellY);
            if (target != null && target != item)
            {
                if (grid.CanStack(item, target))
                {
                    canPlace = true;
                }
            }
        }

        int srcW = def.Width;
        int srcH = def.Height;
        bool hasMask = def.HasCustomShape;
        var mask = def.ShapeMask;

        Color color = canPlace ? validSlotColor : invalidSlotColor;

        for (int ox = 0; ox < srcW; ox++)
        {
            for (int oy = 0; oy < srcH; oy++)
            {
                bool occupied = !hasMask || (mask != null && mask[ox + oy * srcW]);
                if (!occupied)
                    continue;

                InventoryGrid.RotateLocal(ox, oy, rotation, srcW, srcH, out int rx, out int ry);

                int gx = cellX + rx;
                int gy = cellY + ry;

                if (gx < 0 || gy < 0 || gx >= grid.Width || gy >= grid.Height)
                    continue;

                var img = _slotImages[gx, gy];
                if (img != null)
                    img.color = color;
            }
        }
    }

    private void OnEnable()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.RegisterView(this);
    }

    private void OnDisable()
    {
        if (InventoryUIManager.Instance != null)
            InventoryUIManager.Instance.UnregisterView(this);
    }
}
