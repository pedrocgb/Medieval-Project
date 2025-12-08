using Sirenix.OdinInspector;
using UnityEngine;

public enum WeaponCategory
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "New Weapon Config", menuName = "Breezeblocks/Weapons/Config")]
public class WeaponConfig : ScriptableObject
{
    [FoldoutGroup("Info", expanded: true)]
    [FoldoutGroup("Info/Prefab", expanded: true)]
    [SerializeField] private string _weaponPrefab = string.Empty;
    public string WeaponPrefab => _weaponPrefab;

    // ======================================================================

    [FoldoutGroup("Info/Slot", expanded: true)]
    [SerializeField] private EquipmentSlot _slot;
    public EquipmentSlot Slot => _slot;

    // ======================================================================

    [FoldoutGroup("Info/Attack", expanded: true)]
    [SerializeField] private WeaponCategory _category;
    public WeaponCategory Category => _category;

    // ======================================================================

    [FoldoutGroup("Info/Attack", expanded: true), ShowIf("_category", WeaponCategory.Ranged)]
    [SerializeField] private RangedStats _rangedStats;
    public RangedStats RangedStats => _rangedStats;
    [FoldoutGroup("Info/Attack", expanded: true), ShowIf("_category", WeaponCategory.Melee)]
    [SerializeField] private MeleeAttackShapeStats _primaryAttackShape;
    public MeleeAttackShapeStats PrimaryAttackShape => _primaryAttackShape;
    [FoldoutGroup("Info/Attack", expanded: true), ShowIf("_category", WeaponCategory.Melee)]
    [SerializeField] private MeleeAttackShapeStats _secondaryAttackShape;
    public MeleeAttackShapeStats SecondaryAttackShape => _secondaryAttackShape;

    // ======================================================================
}
