using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Core data/logic for a Tarkov-like inventory grid.
/// Rectangular only. Handles collisions and stacking.
/// </summary>
public class InventoryGrid
{
    #region Fields & Properties

    public int Width { get; private set; }
    public int Height { get; private set; }

    // One reference per cell; null means empty.
    private ItemInstance[,] _cells;

    // For quick iteration
    private readonly List<ItemInstance> _items = new List<ItemInstance>();
    public IReadOnlyList<ItemInstance> Items => _items;

    #endregion

    // ======================================================================

    public InventoryGrid(int width, int height)
    {
        if (width <= 0 || height <= 0)
            throw new ArgumentException("InventoryGrid dimensions must be positive.");

        Width = width;
        Height = height;
        _cells = new ItemInstance[width, height];
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && y >= 0 && x < Width && y < Height;
    }

    public ItemInstance GetItemAt(int x, int y)
    {
        if (!InBounds(x, y))
            return null;

        return _cells[x, y];
    }

    // ======================================================================
    #region Placement & Movement

    /// <summary>
    /// Can the item be placed at (x,y) using its current rotation?
    /// </summary>
    public bool CanPlace(ItemInstance item, int x, int y)
    {
        if (item == null || item.Definition == null)
            return false;

        return CanPlaceWithRotation(item, x, y, item.Rotation, ignoreSelf: false);
    }

    /// <summary>
    /// Checks if the item could be placed at (x,y) with a specific rotation.
    /// - respects item shape (if HasCustomShape)
    /// - checks grid bounds
    /// - checks collisions in _cells
    /// If ignoreSelf is true, cells already occupied by this item are treated as empty
    /// (useful for drag previews while item is still in the grid).
    /// </summary>
    public bool CanPlaceWithRotation(
        ItemInstance item,
        int originX,
        int originY,
        int rotation,
        bool ignoreSelf = false)
    {
        if (item == null || item.Definition == null)
            return false;

        var def = item.Definition;
        int srcW = def.Width;
        int srcH = def.Height;

        // Rotated bounding box (0 or 90 degrees)
        int rotW = (rotation % 180 == 0) ? srcW : srcH;
        int rotH = (rotation % 180 == 0) ? srcH : srcW;

        // Quick rectangle bounds check.
        if (originX < 0 || originY < 0 ||
            originX + rotW > Width || originY + rotH > Height)
        {
            return false;
        }

        bool hasShape = def.HasCustomShape;
        var shapeMask = def.ShapeMask;

        for (int ox = 0; ox < srcW; ox++)
        {
            for (int oy = 0; oy < srcH; oy++)
            {
                bool occupiedByShape =
                    !hasShape || (shapeMask != null && shapeMask[ox + oy * srcW]);

                if (!occupiedByShape)
                    continue;

                RotateLocal(ox, oy, rotation, srcW, srcH, out int rx, out int ry);

                int gx = originX + rx;
                int gy = originY + ry;

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

    /// <summary>
    /// Attempts to place an item at (x,y) using its current rotation.
    /// Returns true if successful.
    /// </summary>
    public bool TryPlace(ItemInstance item, int x, int y)
    {
        if (!CanPlace(item, x, y))
            return false;

        WriteItemToCells(item, x, y, item.Rotation);

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
        if (item == null)
            return false;

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
            if (item.OwnerGrid == this)
                item.OwnerGrid = null;
        }

        return found;
    }

    /// <summary>
    /// Tries to move an item to a new top-left position (same rotation).
    /// If placement fails, item is restored to original spot.
    /// </summary>
    public bool TryMove(ItemInstance item, int newX, int newY)
    {
        if (item == null || item.OwnerGrid != this)
            return false;

        int oldX = item.X;
        int oldY = item.Y;

        // Temporarily clear current cells
        Remove(item);

        if (TryPlace(item, newX, newY))
        {
            return true;
        }

        // Failed: restore to original
        if (!TryPlace(item, oldX, oldY))
        {
            Debug.LogError("InventoryGrid.TryMove: Failed to restore item to original position.");
        }

        return false;
    }

    /// <summary>
    /// Tries to move an item to a new position and rotation.
    /// If placement fails, item & rotation are restored.
    /// </summary>
    public bool TryMoveAndRotate(ItemInstance item, int newX, int newY, int newRotation)
    {
        if (item == null || item.OwnerGrid != this)
            return false;

        int oldX = item.X;
        int oldY = item.Y;
        int oldRot = item.Rotation;

        Remove(item);

        item.Rotation = newRotation;

        if (TryPlace(item, newX, newY))
        {
            return true;
        }

        // Failed: restore
        item.Rotation = oldRot;
        if (!TryPlace(item, oldX, oldY))
        {
            Debug.LogError("InventoryGrid.TryMoveAndRotate: Failed to restore item to original position.");
        }

        return false;
    }

    /// <summary>
    /// Scans the grid left-to-right, top-to-bottom for the first free spot.
    /// Keeps item's current rotation.
    /// </summary>
    public bool TryFindSpaceFor(ItemInstance item, out int outX, out int outY)
    {
        outX = -1;
        outY = -1;

        if (item == null || item.Definition == null)
            return false;

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

    #endregion

    // ======================================================================
    #region Internal Helper: write item into _cells

    private void WriteItemToCells(ItemInstance item, int originX, int originY, int rotation)
    {
        var def = item.Definition;
        int srcW = def.Width;
        int srcH = def.Height;

        bool hasShape = def.HasCustomShape;
        var shapeMask = def.ShapeMask;

        for (int ox = 0; ox < srcW; ox++)
        {
            for (int oy = 0; oy < srcH; oy++)
            {
                bool occupiedByShape =
                    !hasShape || (shapeMask != null && shapeMask[ox + oy * srcW]);

                if (!occupiedByShape)
                    continue;

                RotateLocal(ox, oy, rotation, srcW, srcH, out int rx, out int ry);

                int gx = originX + rx;
                int gy = originY + ry;

                _cells[gx, gy] = item;
            }
        }
    }

    #endregion

    // ======================================================================
    #region Rotation math

    /// <summary>
    /// Rotates local item coords (ox,oy) by 0 or 90 degrees around top-left.
    /// srcWidth/Height are the unrotated item dimensions.
    /// </summary>
    public static void RotateLocal(
        int ox, int oy,
        int rotation,
        int srcWidth,
        int srcHeight,
        out int rx,
        out int ry)
    {
        rotation = ((rotation % 360) + 360) % 360;

        switch (rotation)
        {
            case 0:
                rx = ox;
                ry = oy;
                break;

            case 90:
                // 90° clockwise around top-left
                // result bounding box is [srcHeight x srcWidth]
                rx = srcHeight - 1 - oy;
                ry = ox;
                break;

            // If you ever support 180/270, add them here.
            default:
                rx = ox;
                ry = oy;
                break;
        }
    }

    #endregion

    // ======================================================================
    #region Stacking

    /// <summary>
    /// True if some amount from source could be stacked into target (same item def, stackable, capacity left).
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
    /// Same-grid stacking. Moves as many units from source into target as possible.
    /// If source hits 0, it is removed from THIS grid.
    /// </summary>
    public bool TryStack(ItemInstance source, ItemInstance target)
    {
        if (source == null || target == null) return false;
        if (source.OwnerGrid != this || target.OwnerGrid != this) return false;
        if (!CanStack(source, target)) return false;

        int capacityLeft = target.Definition.MaxStack - target.StackCount;
        int toMove = Mathf.Min(capacityLeft, source.StackCount);

        if (toMove <= 0)
            return false;

        target.StackCount += toMove;
        source.StackCount -= toMove;

        if (source.StackCount <= 0)
        {
            Remove(source);
        }

        return true;
    }

    #endregion
}
