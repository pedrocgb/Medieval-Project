using System.Collections.Generic;
using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public static InventoryUIManager Instance { get; private set; }

    private readonly List<InventoryGridView> _views = new();

    public IReadOnlyList<InventoryGridView> Views => _views;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void RegisterView(InventoryGridView view)
    {
        if (view == null) return;
        if (!_views.Contains(view))
            _views.Add(view);
    }

    public void UnregisterView(InventoryGridView view)
    {
        if (view == null) return;
        _views.Remove(view);
    }

    /// <summary>
    /// Returns the first InventoryGridView whose rect contains the given screen point.
    /// </summary>
    public InventoryGridView GetViewUnderPoint(Vector2 screenPosition, Camera uiCamera)
    {
        foreach (var view in _views)
        {
            if (view == null) continue;

            var rect = view.GetComponent<RectTransform>();
            if (rect == null) continue;

            if (RectTransformUtility.RectangleContainsScreenPoint(rect, screenPosition, uiCamera))
            {
                return view;
            }
        }

        return null;
    }
}
