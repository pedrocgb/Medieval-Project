using System;

[Serializable]
public class ItemInstance
{
    public ItemDefinition Definition;

    // Top-left position of the item in its current grid (in cell coordinates)
    public int X;
    public int Y;

    // 0 or 90 degrees for now
    public int Rotation; // 0 or 90

    public int StackCount = 1;

    // Back-reference to the grid that owns this item (null if not placed)
    [NonSerialized] public InventoryGrid OwnerGrid;

    public ItemInstance(ItemDefinition definition, int stackCount = 1)
    {
        Definition = definition;
        StackCount = Math.Clamp(stackCount, 1,
            definition != null && definition.Stackable ? definition.MaxStack : 1);
        Rotation = 0;
    }

    /// <summary>
    /// Effective width in cells considering rotation (0/90).
    /// We ignore custom shapes for now.
    /// </summary>
    public int Width =>
        (Rotation % 180 == 0) ? Definition.Width : Definition.Height;

    /// <summary>
    /// Effective height in cells considering rotation (0/90).
    /// </summary>
    public int Height =>
        (Rotation % 180 == 0) ? Definition.Height : Definition.Width;
}
