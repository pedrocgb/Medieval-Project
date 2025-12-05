using Sirenix.OdinInspector;
using UnityEngine;

public enum WeaponAttackKind
{
    None,
    Slash,
    Thrust,
    Shoot,
}

[CreateAssetMenu(menuName = "Items/Weapon Config")]
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

    [FoldoutGroup("Info/Stats", expanded: true)]
    [SerializeField] private WeaponStats _weaponStats;
    public WeaponStats Stats => _weaponStats;

    // ======================================================================

    [FoldoutGroup("Info/Attack", expanded: true)]
    [SerializeField] private WeaponAttackKind _primaryAttack;
    public WeaponAttackKind PrimaryAttack => _primaryAttack;
    [FoldoutGroup("Info/Attack", expanded: true)]
    [SerializeField] private WeaponAttackKind _secondaryAttack;
    public WeaponAttackKind SecondaryAttack => _secondaryAttack;

    // ======================================================================
}
