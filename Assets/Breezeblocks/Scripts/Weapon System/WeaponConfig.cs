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
    [Header("Prefab/Visuals")]
    public GameObject weaponPrefab;

    [Header("Which equipment slot is this for?")]
    public EquipmentSlot slot = EquipmentSlot.MainHand;

    [SerializeField] private WeaponStats _weaponStats;
    public WeaponStats Stats => _weaponStats;

    [Header("Attack options")]
    public WeaponAttackKind primaryAttack = WeaponAttackKind.Slash;
    public WeaponAttackKind secondaryAttack = WeaponAttackKind.None;

    [Header("Optional: animator overrides, sounds, etc")]
    public AnimatorOverrideController animatorOverride;
}
