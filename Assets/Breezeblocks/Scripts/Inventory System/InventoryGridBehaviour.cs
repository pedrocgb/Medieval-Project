using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

public class InventoryGridBehaviour : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Grid", expanded: true)]
    [SerializeField] private int _width = 0;
    public int Width => _width;
    [FoldoutGroup("Grid", expanded: true)]
    [SerializeField] private int _height = 0;
    public int Height => _height;
    [FoldoutGroup("Grid", expanded: true)]
    [SerializeField] InventoryGridView _gridView;
    public InventoryGridView GridView => _gridView;

    [FoldoutGroup("Starting Items", expanded: true)]
    [SerializeField] private List<Item> _startingItemDefinitions;

    public InventoryGrid Grid { get; private set; }
    #endregion

    // ======================================================================

    private void Awake()
    {
        Grid = new InventoryGrid(_width, _height);
    }

    private void Start()
    {
        // Place starting items
        foreach (var def in _startingItemDefinitions)
        {
            if (def == null) continue;

            var instance = new ItemInstance(def, 1);

            if (Grid.TryFindSpaceFor(instance, out int x, out int y))
            {
                Grid.TryPlace(instance, x, y);
                Debug.Log($"Placed {def.name} at ({x},{y}) in {name}");
            }
            else
            {
                Debug.LogWarning($"No space for {def.name} in grid {name}");
            }
        }

        // Rebuild UI (if any)
        if (_gridView != null)
        {
            _gridView.Rebuild();
        }
    }

    // ======================================================================
}
