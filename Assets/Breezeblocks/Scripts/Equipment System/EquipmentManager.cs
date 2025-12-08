using System;
using UnityEngine;
using CharactersStats;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using ObjectPool;

public class EquipmentManager : MonoBehaviour
{
    #region Variables and Properties
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] private InventoryGridBehaviour mainInventory;
    private WeaponHandler _weaponHandler;
    public EquipmentSet Equipment { get; private set; } = new EquipmentSet();

    [FoldoutGroup("Actor", expanded: true)]
    [SerializeField] private ActorBase _actor;
    [FoldoutGroup("Actor", expanded: true)]
    [SerializeField] private Transform _mainHandSocket;
    [FoldoutGroup("Actor", expanded: true)]
    [SerializeField] private Transform _offHandSocket;
    private ActorBase OwnerActor
    {
        get
        {
            if (_actor == null)
                _actor = GetComponentInParent<ActorBase>();
            return _actor;
        }
    }
    public event Action OnEquipmentChanged;

     // Track spawned weapon instances & runtimes per slot
    [SerializeField] private readonly Dictionary<EquipmentSlot, GameObject> _spawnedWeaponInstances = new();
    [SerializeField] private readonly Dictionary<EquipmentSlot, IWeapon> _weaponRuntimes = new();
    #endregion

    // ==============================================================

    private void Awake()
    {
         _weaponHandler = GetComponent<WeaponHandler>();
    }

    // ==============================================================

    #region Getters
    /// <summary>
    /// Gets the weapon runtime for the given slot (or null).
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public IWeapon GetWeaponRuntime(EquipmentSlot slot)
    {
        _weaponRuntimes.TryGetValue(slot, out var runtime);
        return runtime;
    }

    /// <summary>
    /// Gets the transform socket for the given equipment slot.
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    private Transform GetSocketForSlot(EquipmentSlot slot)
    {
        switch (slot)
        {
            case EquipmentSlot.MainHand:
                return _mainHandSocket;
            case EquipmentSlot.OffHand:
                return _offHandSocket;
            default:
                // Non-weapon slots (head, chest, etc)
                return null;
        }
    }

    /// <summary>
    /// Gets the item equipped in the given slot (or null).
    /// </summary>
    /// <param name="slot"></param>
    /// <returns></returns>
    public ItemInstance GetEquipped(EquipmentSlot slot)
    {
        return Equipment.Get(slot);
    }
    #endregion

    // ==============================================================

    #region Equip/Unequip Methods
    /// <summary>
    /// Notifies listeners that equipment has changed.
    /// </summary>
    public void NotifyEquipmentChanged()
    {
        OnEquipmentChanged?.Invoke();
    }

    /// <summary>
    /// Handles equipping a weapon into the given slot.
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="item"></param>
    private void HandleWeaponEquip(EquipmentSlot slot, ItemInstance item)
    {
        // First, clear anything previously in this slot
        HandleWeaponUnequip(slot);

        if (item == null || item.Definition == null || !item.Definition.IsWeapon)
            return;

        WeaponConfig config = item.Definition.WeaponConfig;
        if (config == null || config.WeaponPrefab == null)
            return;

        var socket = GetSocketForSlot(slot);
        if (socket == null)
        {
            Debug.LogWarning($"[EquipmentManager] No socket configured for weapon slot {slot}.");
            return;
        }

        // Spawn the weapon as a child of the correct socket
        var instance = ObjectPooler.instance.SpawnFromPool(config.WeaponPrefab, socket.localPosition, Quaternion.identity);
        instance.transform.SetParent(socket, false);
        _spawnedWeaponInstances[slot] = instance;

        var runtime = instance.GetComponent<IWeapon>();
        if (runtime != null)
        {
            _weaponHandler.ChangeWeapon(runtime);
            _weaponRuntimes[slot] = runtime;
            runtime.OnEquipped(OwnerActor, item, config);
        }
        else
        {
            Debug.LogWarning($"[EquipmentManager] Weapon prefab '{config.WeaponPrefab}' has no IWeaponRuntime component.");
        }
    }

    /// <summary>
    /// Handles unequipping a weapon from the given slot.
    /// </summary>
    /// <param name="slot"></param>
    private void HandleWeaponUnequip(EquipmentSlot slot)
    {
        // If we had a runtime for this slot, notify it
        if (_weaponRuntimes.TryGetValue(slot, out var runtime) && runtime != null)
        {
            runtime.OnUnequipped();

            // IMPORTANT: clear from WeaponHandler so it stops using a destroyed GO
            if (_weaponHandler != null)
            {
                // If your WeaponHandler has a CurrentWeapon property and you want to
                // be extra-safe, you can check equality here instead:
                // if (_weaponHandler.CurrentWeapon == runtime) { ... }
                _weaponHandler.ChangeWeapon(null);
            }
        }

        // Destroy the spawned instance (3D weapon model) if any
        if (_spawnedWeaponInstances.TryGetValue(slot, out var instance) && instance != null)
        {
            instance.SetActive(false);
        }

        _weaponRuntimes.Remove(slot);
        _spawnedWeaponInstances.Remove(slot);
    }

    /// <summary>
    /// Tries to equip an item from a source inventory grid into the given slot.
    /// </summary>
    /// <param name="slot"></param>
    /// <param name="item"></param>
    /// <param name="fromGrid"></param>
    /// <returns></returns>
    public bool TryEquipFromInventory(EquipmentSlot slot, ItemInstance item, InventoryGrid fromGrid)
    {
        if (item == null || item.Definition == null || fromGrid == null)
            return false;

        // What was previously in this slot? (for removing its modifiers if swapped)
        var previous = Equipment.Get(slot);

        bool result = Equipment.TryEquipFromInventory(slot, item, fromGrid);
        if (result)
        {
            // Remove modifiers from the previously equipped item (if any)
            if (previous != null)
            {
                RemoveItemStatModifiers(previous);
            }

            // Apply modifiers for the newly equipped item
            var newlyEquipped = Equipment.Get(slot);
            if (newlyEquipped != null)
            {
                ApplyItemStatModifiers(newlyEquipped);
            }

             // Weapons: destroy previous, spawn new (if weapon slot)
            HandleWeaponEquip(slot, newlyEquipped);

            OnEquipmentChanged?.Invoke();
        }

        return result;
    }

    /// <summary>
    /// Tries to auto-equip an item from a source inventory grid into any valid slot.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="fromGrid"></param>
    /// <returns></returns>
    public bool TryAutoEquipFromInventory(ItemInstance item, InventoryGrid fromGrid)
    {
        if (item == null || item.Definition == null || fromGrid == null)
            return false;

        foreach (EquipmentSlot slot in Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!EquipmentRules.CanEquip(slot, item.Definition))
                continue;

            // 🔹 Use the wrapper so modifiers are handled
            if (TryEquipFromInventory(slot, item, fromGrid))
            {
                return true;
            }
        }

            return false;
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

    // Destroy weapon prefab & clear runtime
    HandleWeaponUnequip(slot);

    // 🔹 NEW: remove all stat modifiers that came from this item
    RemoveItemStatModifiers(item);

    OnEquipmentChanged?.Invoke();
    return true;
    }

    #endregion

    // ==============================================================

    #region Stat Modifier Methods
    /// <summary>
    /// Applies stat modifiers from the given item to the owner actor's stats.
    /// </summary>
    /// <param name="item"></param>
    private void ApplyItemStatModifiers(ItemInstance item)
    {
        var actor = OwnerActor;
        if (actor == null || item == null || item.Definition == null)
            return;

        var modifiers = item.Definition.StatModifiers;
        if (modifiers == null || modifiers.Count == 0)
            return;

        foreach (var data in modifiers)
        {
            if (data == null)
                continue;

            var stat = actor.GetStat(data.Stat);
            if (stat == null)
            {
                Debug.LogWarning($"[EquipmentManager] Stat '{data.Stat}' not found on actor '{actor.name}'.");
                continue;
            }

            // Source = this item instance
            var runtimeMod = data.CreateRuntimeModifier(item);
            stat.AddModifier(runtimeMod);
        }
    }

    /// <summary>
    /// Removes all stat modifiers from the given item from the owner actor's stats.
    /// </summary>
    /// <param name="item"></param>
    private void RemoveItemStatModifiers(ItemInstance item)
    {
        var actor = OwnerActor;
        if (actor == null || item == null)
            return;

        // We don't care which stats exactly — we just
        // remove any modifiers whose Source == this item instance
        foreach (var stat in actor.AllStats)
        {
            stat?.RemoveAllModifiersFromSource(item);
        }
    }
    #endregion

    // ==============================================================
}
