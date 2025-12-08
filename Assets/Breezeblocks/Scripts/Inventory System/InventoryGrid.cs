using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pure data / logic for a Tarkov-like grid inventory.
/// No Unity-specific stuff here.
/// </summary>
public class InventoryGrid
{
    #region Variables and Properties
    public int Width { get; private set; }
    public int Height { get; private set; }

    private ItemInstance[,] _cells;

    private readonly List<ItemInstance> _items = new List<ItemInstance>();
    public IReadOnlyList<ItemInstance> Items => _items;
    #endregion

    // ==============================================================

    public InventoryGrid(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("InventoryGrid dimensions must be positive.");

        Width = width;
        Height = height;
        _cells = new ItemInstance[width, height];
    }

    public ItemInstance GetItemAt(int x, int y)
    {
        if (!InBounds(x, y)) return null;
        return _cells[x, y];
    }

    public bool InBounds(int x, int y) =>
        x >= 0 && y >= 0 && x < Width && y < Height;

    // ==============================================================

    #region Movement and Placement
    /// <summary>
    /// Checks if the item could be placed at the given top-left cell (x, y),
    /// assuming it is not already in this grid.
    /// For now, we only allow placements in empty cells (no stacking yet).
    /// </summary>
    public bool CanPlace(ItemInstance item, int x, int y)
    {
        return CanPlaceWithRotation(item, x, y, item.Rotation, ignoreSelf: false);
    }

    /// <summary>
    /// Places the item at x,y if possible. Returns true if successful.
    /// Does NOT automatically remove it from a previous grid.
    /// Call RemoveFromCurrentGridBefore if needed.
    /// </summary>
    public bool TryPlace(ItemInstance item, int x, int y)
    {
        if (!CanPlace(item, x, y))
            return false;

        var def = item.Definition;
        int srcW = def.Width;
        int srcH = def.Height;
        int rotation = item.Rotation;

        bool hasMask = def.HasCustomShape;
        var mask = def.ShapeMask;

        // Fill cells only where shape says the item occupies
        for (int ox = 0; ox < srcW; ox++)
        {
            for (int oy = 0; oy < srcH; oy++)
            {
                bool occupied =
                    !hasMask || (mask != null && mask[ox + oy * srcW]);

                if (!occupied)
                    continue;

                RotateLocal(ox, oy, rotation, srcW, srcH, out int rx, out int ry);

                int gx = x + rx;
                int gy = y + ry;

                _cells[gx, gy] = item;
            }
        }

        item.X = x;
        item.Y = y;
        item.OwnerGrid = this;

        if (!_items.Contains(item))
            _items.Add(item);

        return true;
    }


    /// <summary>
    /// Removes the item from this grid (clears all its cells).
    /// </summary>
    public bool Remove(ItemInstance item)
    {
        if (item == null) return false;
        if (item.OwnerGrid != this) return false;

        bool found = false;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (_cells[x, y] == item)
                {
                    _cells[x, y] = null;
                    found = true;
                }
            }
        }

        if (found)
        {
            _items.Remove(item);
            item.OwnerGrid = null;
        }

        return found;
    }

    /// <summary>
    /// Tries to find the first available spot where the item fits.
    /// Very basic: left-to-right, top-to-bottom scanning.
    /// </summary>
    public bool TryFindSpaceFor(ItemInstance item, out int outX, out int outY)
    {
        outX = -1;
        outY = -1;

        for (int x = 0; x < Width; x++)
        {
            for (int y = 0; y < Height; y++)
            {
                if (CanPlace(item, x, y))
                {
                    outX = x;
                    outY = y;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Tries to move an item to a new top-left position.
    /// If placement fails, item is restored to original position.
    /// </summary>
    public bool TryMove(ItemInstance item, int newX, int newY)
    {
        if (item == null) return false;
        if (item.OwnerGrid != this) return false;

        int oldX = item.X;
        int oldY = item.Y;

        // Temporarily remove so its own cells are free
        Remove(item);

        // Try place at new position
        if (TryPlace(item, newX, newY))
        {
            return true;
        }

        // Failed: restore to original spot
        if (!TryPlace(item, oldX, oldY))
        {
            // This should never happen unless something else modified the grid
            Debug.LogError("InventoryGrid.TryMove: Failed to restore item to original position.");
        }

        return false;
    }
    #endregion

    // ==============================================================

    #region Rotation
    /// <summary>
    /// Tries to move an item to a new top-left position with a new rotation.
    /// If placement fails, item and rotation are restored.
    /// </summary>
    public bool TryMoveAndRotate(ItemInstance item, int newX, int newY, int newRotation)
    {
        if (item == null) return false;
        if (item.OwnerGrid != this) return false;

        int oldX = item.X;
        int oldY = item.Y;
        int oldRotation = item.Rotation;

        // Remove first so its cells are freed
        Remove(item);

        item.Rotation = newRotation;

        if (TryPlace(item, newX, newY))
        {
            return true;
        }

        // Failed: revert
        item.Rotation = oldRotation;
        if (!TryPlace(item, oldX, oldY))
        {
            Debug.LogError("InventoryGrid.TryMoveAndRotate: Failed to restore item to original position.");
        }

        return false;
    }

    public static void RotateLocal(
    int ox, int oy,
    int rotation,
    int srcWidth, int srcHeight,
    out int rx, out int ry)
    {
        rotation = ((rotation % 360) + 360) % 360;

        switch (rotation)
        {
            case 0:
                rx = ox;
                ry = oy;
                break;

            case 90:
                // 90° clockwise around top-left, bounding box becomes [srcHeight x srcWidth]
                rx = srcHeight - 1 - oy;
                ry = ox;
                break;

            // If you later add 180/270, handle here.
            default:
                // For now clamp to 0
                rx = ox;
                ry = oy;
                break;
        }
    }


    /// <summary>
    /// Checks if the item can be placed at (x,y) with a given rotation.
    /// If ignoreSelf is true, cells occupied by this same item are treated as empty.
    /// Useful for previews while dragging.
    /// </summary>
    public bool CanPlaceWithRotation(ItemInstance item, int x, int y, int rotation, bool ignoreSelf = false)
    {
        if (item == null || item.Definition == null)
            return false;

        var def = item.Definition;
        int srcW = def.Width;
        int srcH = def.Height;

        // Rotated bounding box
        int rotW = (rotation % 180 == 0) ? srcW : srcH;
        int rotH = (rotation % 180 == 0) ? srcH : srcW;

        // Quick bounds check for full bounding rectangle
        if (x < 0 || y < 0 || x + rotW > Width || y + rotH > Height)
            return false;

        bool hasMask = def.HasCustomShape;
        var mask = def.ShapeMask;

        // Check each occupied cell (according to shape mask or full rectangle)
        for (int ox = 0; ox < srcW; ox++)
        {
            for (int oy = 0; oy < srcH; oy++)
            {
                bool occupied =
                    !hasMask || (mask != null && mask[ox + oy * srcW]);

                if (!occupied)
                    continue;

                RotateLocal(ox, oy, rotation, srcW, srcH, out int rx, out int ry);

                int gx = x + rx;
                int gy = y + ry;

                // We already did bounding check for the full box, but be defensive
                if (!InBounds(gx, gy))
                    return false;

                var occupant = _cells[gx, gy];

                if (occupant != null)
                {
                    if (!ignoreSelf || occupant != item)
                        return false;
                }
            }
        }

        return true;
    }
    #endregion

    // ==============================================================

    #region Stacking
    /// <summary>
    /// Returns true if any amount from 'source' could be stacked into 'target'
    /// (at least 1 unit), without exceeding MaxStack.
    /// Does NOT modify anything and does NOT require both items to be in this grid.
    /// </summary>
    public bool CanStack(ItemInstance source, ItemInstance target)
    {
        if (source == null || target == null) return false;
        if (source == target) return false;

        if (source.Definition == null || target.Definition == null) return false;
        if (source.Definition != target.Definition) return false;
        if (!target.Definition.Stackable) return false;

        int capacityLeft = target.Definition.MaxStack - target.StackCount;
        if (capacityLeft <= 0) return false;

        return source.StackCount > 0;
    }


    /// <summary>
    /// Same-grid stacking: moves as many units as possible from 'source' into 'target',
    /// up to target's MaxStack. If 'source' runs out, it is removed from THIS grid.
    /// Returns true if at least 1 unit was moved.
    /// </summary>
    public bool TryStack(ItemInstance source, ItemInstance target)
    {
        if (source == null || target == null) return false;
        if (source.OwnerGrid != this || target.OwnerGrid != this) return false;
        if (!CanStack(source, target)) return false;

        int capacityLeft = target.Definition.MaxStack - target.StackCount;
        int amountToMove = Mathf.Min(capacityLeft, source.StackCount);

        if (amountToMove <= 0)
            return false;

        target.StackCount += amountToMove;
        source.StackCount -= amountToMove;

        if (source.StackCount <= 0)
        {
            Remove(source);
        }

        return true;
    }
    #endregion
}
