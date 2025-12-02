using System;
using System.Collections.Generic;

public enum EquipmentSlot
{
    // Head
    Helmet,
    Face,
    Necklace,
    Earrings,

    // Body
    Armor,
    Backpack,
    Cape,
    Boots,
    Belt,
    Pouch,

    // Rings
    LeftRing,
    RightRing,

    // Hands
    MainHand,
    OffHand,
    Gloves
}

public static class EquipmentRules
{
    private static readonly Dictionary<EquipmentSlot, EquipTag[]> SlotAllowedTags =
        new Dictionary<EquipmentSlot, EquipTag[]>
        {
            // Head
            { EquipmentSlot.Helmet,  new[] { EquipTag.Helmet } },
            { EquipmentSlot.Face,    new[] { EquipTag.Face } },
            { EquipmentSlot.Necklace,    new[] { EquipTag.Necklace } },
            { EquipmentSlot.Earrings,    new[] { EquipTag.Earrings } },

            // Body
            { EquipmentSlot.Armor,    new[] { EquipTag.Armor } },
            { EquipmentSlot.Backpack,     new[] { EquipTag.Backpack } },
            { EquipmentSlot.Cape,     new[] { EquipTag.Cape } },
            { EquipmentSlot.Boots,       new[] { EquipTag.Boots } },

            // Hands
            { EquipmentSlot.MainHand,    new[] { EquipTag.Weapon } },
            { EquipmentSlot.OffHand,     new[] { EquipTag.Weapon, EquipTag.Shield } },
            { EquipmentSlot.Gloves,      new[] { EquipTag.Gloves } },            

            // Belt
            { EquipmentSlot.Belt,        new[] { EquipTag.Belt } },
            { EquipmentSlot.Pouch,        new[] { EquipTag.Pouch } },

            // Rings
            { EquipmentSlot.LeftRing,    new[] { EquipTag.Ring } },
            { EquipmentSlot.RightRing,   new[] { EquipTag.Ring } }
        };

    public static bool CanEquip(EquipmentSlot slot, ItemDefinition def)
    {
        if (def == null || !def.Equipable || def.EquipTags == null || def.EquipTags.Length == 0)
            return false;

        if (!SlotAllowedTags.TryGetValue(slot, out var allowed))
            return false;

        foreach (var tag in def.EquipTags)
        {
            foreach (var a in allowed)
            {
                if (tag == a)
                    return true;
            }
        }

        return false;
    }
}

/// <summary>
/// Holds equipped items per slot and handles equip/swap logic.
/// </summary>
[Serializable]
public class EquipmentSet
{
    private readonly Dictionary<EquipmentSlot, ItemInstance> _slots = new();

    public ItemInstance Get(EquipmentSlot slot)
    {
        _slots.TryGetValue(slot, out var item);
        return item;
    }

    public ItemInstance Unequip(EquipmentSlot slot)
    {
        if (!_slots.TryGetValue(slot, out var item) || item == null)
            return null;

        _slots[slot] = null;
        return item;
    }

    public void ForceSet(EquipmentSlot slot, ItemInstance item)
    {
        _slots[slot] = item;
    }

    /// <summary>
    /// Tries to equip an item from a given inventory grid into a slot.
    /// - Ignores item size/shape; only checks EquipTags via EquipmentRules.
    /// - If slot empty: remove from grid and equip.
    /// - If slot occupied: only swap if old item can fit back into the same grid.
    /// Returns true if equipped (or swapped) successfully.
    /// </summary>
    public bool TryEquipFromInventory(EquipmentSlot slot, ItemInstance item, InventoryGrid fromGrid)
    {
        if (item == null || item.Definition == null || fromGrid == null)
            return false;

        if (!EquipmentRules.CanEquip(slot, item.Definition))
            return false;

        // Item must actually be in that grid
        if (item.OwnerGrid != fromGrid)
            return false;

        var existing = Get(slot);

        // Slot empty: just move item from inventory to equipment
        if (existing == null)
        {
            bool removed = fromGrid.Remove(item);
            if (!removed)
                return false;

            item.OwnerGrid = null;
            _slots[slot] = item;
            return true;
        }

        // Slot occupied: try to swap
        // 1) Can the existing item fit back into the same inventory grid?
        if (!fromGrid.TryFindSpaceFor(existing, out int x, out int y))
        {
            // No space -> cannot swap
            return false;
        }

        // 2) Remove new item from inventory
        bool removedNew = fromGrid.Remove(item);
        if (!removedNew)
            return false;

        // 3) Place old item into inventory
        bool placedOld = fromGrid.TryPlace(existing, x, y);
        if (!placedOld)
        {
            // Extremely unlikely, but if this fails, try to restore new item.
            fromGrid.TryPlace(item, item.X, item.Y);
            return false;
        }

        // 4) Equip new item
        existing.OwnerGrid = null;
        item.OwnerGrid = null;
        _slots[slot] = item;

        return true;
    }

    public IEnumerable<ItemInstance> GetAllEquipped()
    {
        foreach (var kvp in _slots)
        {
            if (kvp.Value != null)
                yield return kvp.Value;
        }
    }

    public IReadOnlyDictionary<EquipmentSlot, ItemInstance> Slots => _slots;
}
