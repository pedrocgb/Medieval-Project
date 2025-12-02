using System.Collections.Generic;
using UnityEngine;

public class InventoryGridBehaviour : MonoBehaviour
{
    [Header("Grid Size (cells)")]
    public int width = 10;
    public int height = 4;

    [Header("Starting Items (Definitions only for now)")]
    public List<ItemDefinition> startingItemDefinitions;

    [Header("Optional UI View")]
    public InventoryGridView gridView;

    public InventoryGrid Grid { get; private set; }

    private void Awake()
    {
        Grid = new InventoryGrid(width, height);
    }

    private void Start()
    {
        // Place starting items
        foreach (var def in startingItemDefinitions)
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
        if (gridView != null)
        {
            gridView.Rebuild();
        }
    }
}
