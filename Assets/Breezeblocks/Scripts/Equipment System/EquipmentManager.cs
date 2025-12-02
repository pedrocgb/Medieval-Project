using System;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
    [Header("Reference to the main inventory used for swaps, if needed")]
    public InventoryGridBehaviour mainInventory;

    public EquipmentSet Equipment { get; private set; } = new EquipmentSet();

    public event Action OnEquipmentChanged;

    public void NotifyEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke();
    }

    public bool TryEquipFromInventory(EquipmentSlot slot, ItemInstance item, InventoryGrid fromGrid)
    {
        bool result = Equipment.TryEquipFromInventory(slot, item, fromGrid);
        if (result)
        {
            OnEquipmentChanged?.Invoke();
        }
        return result;
    }

    public ItemInstance GetEquipped(EquipmentSlot slot)
    {
        return Equipment.Get(slot);
    }

    /// <summary>
    /// Tries to unequip an item from a slot into a target inventory grid at (cellX, cellY).
    /// Ignores item size for the equip side, but placement in inventory must be valid.
    /// </summary>
    public bool TryUnequipToInventory(EquipmentSlot slot, InventoryGrid targetGrid, int cellX, int cellY)
    {
        if (targetGrid == null)
            return false;

        var item = Equipment.Get(slot);
        if (item == null)
            return false;

        // Check if we can place it in the target grid (no stacking from equipment for now)
        if (!targetGrid.CanPlaceWithRotation(item, cellX, cellY, item.Rotation, ignoreSelf: false))
            return false;

        // Temporarily remove from equipment
        var removed = Equipment.Unequip(slot);
        if (removed == null)
            return false;

        bool placed = targetGrid.TryPlace(item, cellX, cellY);
        if (!placed)
        {
            // Restore if something weird happens
            Equipment.ForceSet(slot, item);
            return false;
        }

        OnEquipmentChanged?.Invoke();
        return true;
    }

    public bool TryAutoEquipFromInventory(ItemInstance item, InventoryGrid fromGrid)
    {
        if (item == null || item.Definition == null || fromGrid == null)
            return false;

        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!EquipmentRules.CanEquip(slot, item.Definition))
                continue;

            if (Equipment.TryEquipFromInventory(slot, item, fromGrid))
            {
                OnEquipmentChanged?.Invoke();
                return true;
            }
        }

        return false;
    }
}
