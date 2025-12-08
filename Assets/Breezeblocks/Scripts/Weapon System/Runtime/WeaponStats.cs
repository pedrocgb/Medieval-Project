using UnityEngine;
using Sirenix.OdinInspector;

public abstract class WeaponStats : ScriptableObject
{
    [FoldoutGroup("Quality", expanded: true)]
    [SerializeField] private ItemQuality _quality = ItemQuality.Crude;
    public ItemQuality Quality => _quality;

    // ======================================================================

    [FoldoutGroup("Damage", expanded: true)]
    [SerializeField] private float _minBaseDamage;
    public float MinDamage => WeaponDamageByQuality.Get(_minBaseDamage, _quality);
    [FoldoutGroup("Damage", expanded: true)]
    [SerializeField] private float _maxBaseDamage;
    public float MaxDamage => WeaponDamageByQuality.Get(_maxBaseDamage, _quality);

    [FoldoutGroup("Damage", expanded: true)]
    [SerializeField] private DamageType _damageType;
    public DamageType DamageType => _damageType;

    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _headHitChance = 0.05f;
    public float HeadHitChance => _headHitChance;
    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _torsoHitChance = 0.55f;
    public float TorsoHitChance => _torsoHitChance;
    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _leftArmHitChance = 0.1f;
    public float LeftArmHitChance => _leftArmHitChance;
    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _rightArmHitChance = 0.1f;
    public float RightArmHitChance => _rightArmHitChance;
    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _leftLegHitChance = 0.1f;
    public float LeftLegHitChance => _leftLegHitChance;
    [FoldoutGroup("Damage/Limbs", expanded: true)]
    [SerializeField] protected float _rightLegHitChance = 0.1f;
    public float RightLegHitChance => _rightLegHitChance;

    // ======================================================================

    [FoldoutGroup("Attack", expanded: true)]
    [SerializeField] private float _attackSpeed;
    public float AttackSpeed => _attackSpeed;

    // ======================================================================

    [FoldoutGroup("Stamina", expanded: true)]
    [SerializeField] private float _staminaCost;
    public float StaminaCost => _staminaCost;

    // ======================================================================

    [FoldoutGroup("Fatigue", expanded: true)]
    [SerializeField] private float _fatigueCost;
    public float FatigueCost => _fatigueCost;

    // ======================================================================

    public Limbs GetRandomLimb()
    {
        // Sum all chances (they don't need to add up to 1, we normalize implicitly)
        float total =
            HeadHitChance +
            TorsoHitChance +
            LeftArmHitChance +
            RightArmHitChance +
            LeftLegHitChance +
            RightLegHitChance;

        // Safety check
        if (total <= 0f)
        {
            Debug.LogWarning("Total limb hit chance is 0 or less. Defaulting to Torso.");
            return Limbs.Torso;
        }

        // Roll from 0 to total
        float roll = Random.Range(0f, total);

        // Walk through each limb's range
        if (roll < HeadHitChance)
            return Limbs.Head;

        roll -= HeadHitChance;
        if (roll < TorsoHitChance)
            return Limbs.Torso;

        roll -= TorsoHitChance;
        if (roll < LeftArmHitChance)
            return Limbs.LeftArm;

        roll -= LeftArmHitChance;
        if (roll < RightArmHitChance)
            return Limbs.RightArm;

        roll -= RightArmHitChance;
        if (roll < LeftLegHitChance)
            return Limbs.LeftLeg;

        // Whatever’s left goes to RightLeg
        return Limbs.RightLeg;
    }
}

public enum ItemQuality
{
    Poor,
    Crude,
    Fine,
    Superior,
    Excepcional,
    MasterfullyCrafted
}
