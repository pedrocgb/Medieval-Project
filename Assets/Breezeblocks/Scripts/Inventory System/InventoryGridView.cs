using ObjectPool;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI view for an InventoryGrid.
/// It draws background slots and item views, and handles visual previews.
/// </summary>
public class InventoryGridView : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private InventoryGridBehaviour _gridSource;
    public InventoryGridBehaviour GridSource => _gridSource;
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private RectTransform content;

    [FoldoutGroup("Prefab", expanded: true)]
    [SerializeField] private GameObject _slotPrefab;
    [FoldoutGroup("Prefab", expanded: true)]
    [SerializeField] private InventoryItemView _itemViewPrefab;

    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField] private float _cellSize = 64f;
    public float CellSize => _cellSize;
    [FoldoutGroup("Layout", expanded: true)]
    [SerializeField] private float  _cellSpacing = 2f;

    [FoldoutGroup("Colors", expanded: true)]
    [SerializeField] private Color _normalSlotColor = Color.white;
    [FoldoutGroup("Colors", expanded: true)]
    [SerializeField] private Color _validSlotColor = Color.green;
    [FoldoutGroup("Colors", expanded: true)]
    [SerializeField] private Color _invalidSlotColor = Color.red;

    private readonly List<GameObject> _slotInstances = new();
    private readonly List<InventoryItemView> _itemInstances = new();
    private Image[,] _slotImages;

    public InventoryGrid Grid => _gridSource != null ? _gridSource.Grid : null;
    #endregion

    // ======================================================================

    #region Grid Handler
    /// <summary>
    /// Rebuild inventory grid view.
    /// </summary>
    public void Rebuild()
    {
        if (_gridSource == null || _gridSource.Grid == null)
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

        var grid = _gridSource.Grid;
        int width = grid.Width;
        int height = grid.Height;

        // Set the visual size of the content rect, but DO NOT touch anchors/pivot.
        float totalWidth = width * _cellSize;
        float totalHeight = height * _cellSize;
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, totalWidth);
        content.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);

        _slotImages = new Image[width, height];

        // Cache rect & pivot for positioning math
        Rect rect = content.rect;
        Vector2 pivot = content.pivot;

        // 1) Build background slots
        if (_slotPrefab != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    GameObject slotGO = Instantiate(_slotPrefab, content);
                    _slotInstances.Add(slotGO);

                    RectTransform rt = slotGO.GetComponent<RectTransform>();
                    if (rt != null)
                    {
                        rt.anchorMin = new Vector2(0.5f, 0.5f);
                        rt.anchorMax = new Vector2(0.5f, 0.5f);
                        rt.pivot = new Vector2(0.5f, 0.5f);
                        rt.sizeDelta = new Vector2(_cellSize, _cellSize);

                        // Local "from left / from top" for this cell
                        float xFromLeft = x * _cellSize + _cellSize * 0.5f;
                        float yFromTop = y * _cellSize + _cellSize * 0.5f;

                        // Convert to anchoredPosition relative to pivot
                        float anchoredX = xFromLeft - rect.width * pivot.x;
                        float anchoredY = rect.height * (1f - pivot.y) - yFromTop;

                        rt.anchoredPosition = new Vector2(anchoredX, anchoredY);
                    }

                    Image img = slotGO.GetComponent<Image>();
                    if (img != null)
                    {
                        img.color = _normalSlotColor;
                        _slotImages[x, y] = img;
                    }
                }
            }
        }

        // 2) Build item views
        if (_itemViewPrefab != null)
        {
            foreach (var item in grid.Items)
            {
                InventoryItemView itemView = Instantiate(_itemViewPrefab, content);
                _itemInstances.Add(itemView);
                itemView.Bind(item, _cellSize);
            }
        }
    }

    /// <summary>
    /// Clear inventory
    /// </summary>
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

        var grid = _gridSource?.Grid;
        if (grid == null)
            return false;

        int width = grid.Width;
        int height = grid.Height;

        Rect rect = content.rect;
        Vector2 pivot = content.pivot;

        // Convert localPoint (pivot-relative) to "from left / from top" coordinates
        float xFromLeft = localPoint.x + rect.width * pivot.x;
        float yFromTop = rect.height * (1f - pivot.y) - localPoint.y;

        float gridWidthPx = width * _cellSize;
        float gridHeightPx = height * _cellSize;

        // Check if inside grid bounds
        if (xFromLeft < 0 || xFromLeft > gridWidthPx || yFromTop < 0 || yFromTop > gridHeightPx)
            return false;

        cellX = Mathf.FloorToInt(xFromLeft / _cellSize);
        cellY = Mathf.FloorToInt(yFromTop / _cellSize);

        if (cellX < 0 || cellY < 0 || cellX >= width || cellY >= height)
            return false;

        return true;
    }
    #endregion

    // ======================================================================

    #region Item movement
    /// <summary>
    /// Try to move item in the grid.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="targetX"></param>
    /// <param name="targetY"></param>
    /// <returns></returns>
    public bool TryMoveItem(ItemInstance item, int targetX, int targetY)
    {
        if (_gridSource == null || _gridSource.Grid == null || item == null)
            return false;

        bool moved = _gridSource.Grid.TryMove(item, targetX, targetY);
        if (moved)
        {
            Rebuild();
        }
        return moved;
    }

    /// <summary>
    /// Try to move item in the grid when object is rotated.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="targetX"></param>
    /// <param name="targetY"></param>
    /// <param name="targetRotation"></param>
    /// <returns></returns>
    public bool TryMoveItemWithRotation(ItemInstance item, int targetX, int targetY, int targetRotation)
    {
        if (_gridSource == null || _gridSource.Grid == null || item == null)
            return false;

        bool moved = _gridSource.Grid.TryMoveAndRotate(item, targetX, targetY, targetRotation);
        if (moved)
        {
            Rebuild();
        }
        return moved;
    }
    #endregion

    // ======================================================================

    #region Preview Methods
    /// <summary>
    /// Clear all items highlights.
    /// </summary>
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
                    img.color = _normalSlotColor;
            }
        }
    }

    /// <summary>
    /// Preview the placement of the item on the grid.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="rotation"></param>
    /// <param name="screenPosition"></param>
    /// <param name="uiCamera"></param>
    public void PreviewPlacement(ItemInstance item, int rotation, Vector2 screenPosition, Camera uiCamera)
    {
        ClearHighlights();

        if (item == null || _gridSource == null || _gridSource.Grid == null || _slotImages == null)
            return;

        var grid = _gridSource.Grid;
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

        Color color = canPlace ? _validSlotColor : _invalidSlotColor;

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
    #endregion

    // ======================================================================

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

    // ======================================================================

}
