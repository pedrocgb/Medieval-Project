using UnityEngine;
using Sirenix.OdinInspector;
using CharactersStats;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Inventory/Item Definition")]
public class ItemDefinition : ScriptableObject
{
    [Header("Identity")]
    public string Id; // e.g. "akm", "medkit_small"

    [Header("Presentation")]
    public string DisplayName;
    [TextArea]
    public string Description;

    [Tooltip("Short type label for UI, e.g. Sword, Helmet, Ring.")]
    public string TypeName;

    [FoldoutGroup("Modifiers")]
    [SerializeField] private List<StatModifier> _modifiers = new List<StatModifier>();
    public List<StatModifier> Modifiers => _modifiers;

    [Header("Visual")]
    public Sprite Icon;

    [Header("Size (in grid cells)")]
    [Min(1)] public int Width = 1;
    [Min(1)] public int Height = 1;

    [Header("Stacking")]
    public bool Stackable = false;
    [Min(1)] public int MaxStack = 1;

    [Header("Equip")]
    public bool Equipable = false;

    [Tooltip("Tags used for equipment rules, e.g. Weapon, Helmet, Ring, etc.")]
    public EquipTag[] EquipTags;


    [Header("Advanced Shape (optional)")]
    [Tooltip("Optional custom shape mask; rows = Height, columns = Width; index = x + y * Width")]
    [HideInInspector]
    public bool[] ShapeMask;

    /// <summary>
    /// Returns true if this item uses a custom shape mask instead of a simple rectangle.
    /// </summary>
    public bool HasCustomShape =>
        ShapeMask != null && ShapeMask.Length == Width * Height;
}
