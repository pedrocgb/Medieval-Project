using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Item", menuName = "Breezeblocks/Item")]
public class Item : SerializedScriptableObject
{
    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private string _id = string.Empty;
    public string Id => _id;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private string _displayName = string.Empty;
    public string DisplayName => _displayName;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField, TextArea(8, 20)] private string _description = string.Empty;
    public string Description => _description;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private Sprite _icon;
    public Sprite Icon => _icon;

    [FoldoutGroup("Basic Info", expanded: true)]
    [SerializeField] private string _typeName = string.Empty;
    public string TypeName => _typeName;

    // ======================================================================

    [FoldoutGroup("Rarity", expanded: true)]
    [SerializeField] private ItemRarity _rarity = ItemRarity.Commmon;
    public ItemRarity Rarity => _rarity;

    // ======================================================================

    [FoldoutGroup("Consumable and Durability", expanded: true)]
    [SerializeField] private float _maxDurability = 100f;
    public float MaxDurability => _maxDurability;
    [FoldoutGroup("Consumable and Durability", expanded: true)]
    [SerializeField] private bool _isConsumable = false;
    public bool IsConsumable => _isConsumable;
    [FoldoutGroup("Consumable and Durability", expanded: true), ShowIf("_isConsumable")]
    [SerializeField] private int _maxConsumes = 3;
    public int MaxConsumes => _maxConsumes;

    // ======================================================================

    [FoldoutGroup("Modifiers", expanded: true)]
    [SerializeField] private List<ItemStatModifierData> _statModifiers = new();
    public List<ItemStatModifierData> StatModifiers => _statModifiers;

    // ======================================================================

    [FoldoutGroup("Equipment", expanded: true)]
    [SerializeField] private bool _equipable = false;
    public bool Equipable => _equipable;

    [FoldoutGroup("Equipment", expanded: true), ShowIf(nameof(_equipable))]
    [SerializeField] private EquipTag[] _equipTags;
    public EquipTag[] EquipTags => _equipTags;
    [FoldoutGroup("Equipment/Weapon", expanded: true)]
    [SerializeField] private bool _isWeapon = false;
    public bool IsWeapon => _isWeapon;

    [FoldoutGroup("Equipment/Weapon", expanded: true), ShowIf(nameof(_isWeapon))]
    [SerializeField] private WeaponConfig _weaponConfig;
    public WeaponConfig WeaponConfig => _weaponConfig;

    // ======================================================================

    [FoldoutGroup("Inventory Tetris", expanded: true)]
    [SerializeField, Min(1)] private int _width = 1;
    public int Width => _width;

    [FoldoutGroup("Inventory Tetris", expanded: true)]
    [SerializeField, Min(1)] private int _height = 1;
    public int Height => _height;

    // ======================================================================

    [FoldoutGroup("Inventory Tetris/Stack", expanded: true)]
    [SerializeField] private bool _stackable = false;
    public bool Stackable => _stackable;

    [FoldoutGroup("Inventory Tetris/Stack", expanded: true), ShowIf(nameof(_stackable))]
    [SerializeField, Min(1)] private int _maxStack = 1;
    public int MaxStack => _maxStack;


    // ======================================================================
    // Shape mask is fully driven by the custom editor, so we hide it from Odin/Unity.

    [SerializeField, HideInInspector]
    private bool[] _shapeMask;
    public bool[] ShapeMask => _shapeMask;

    /// <summary>
    /// Returns true if this item uses a custom shape mask instead of a simple rectangle.
    /// </summary>
    public bool HasCustomShape =>
        _shapeMask != null && _shapeMask.Length == Width * Height;
}

public enum ItemRarity
{
    Commmon,
    Rare,
    Epic,
    Legendary,
    Unique
}

