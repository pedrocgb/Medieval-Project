using UnityEngine;
using Sirenix.OdinInspector;

public abstract class ActorStats : ScriptableObject
{
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseStrength = 8;
    public int BaseStrength => _baseStrength;
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseDexterity = 8;
    public int BaseDexterity => _baseDexterity;
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseConstitution = 8;
    public int BaseConstitution => _baseConstitution;
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseIntelligence = 8;
    public int BaseIntelligence => _baseIntelligence;
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseWisdom = 8;
    public int BaseWisdom => _baseWisdom;
    [FoldoutGroup("Main Attributes")]
    [SerializeField] private int _baseCharisma = 8;
    public int BaseCharisma => _baseCharisma;


    [FoldoutGroup("Stats")]
    [FoldoutGroup("Stats/Health")]
    [SerializeField] private float _baseHeadHealth = 10f;
    public float BaseHeadHealth => _baseHeadHealth;
    [FoldoutGroup("Stats/Health")]
    [SerializeField] private float _baseTorsoHealth = 50f;
    public float BaseTorsoHealth => _baseTorsoHealth;
    [FoldoutGroup("Stats/Health")]
    [SerializeField] private float _baseArmsHealth = 25f;
    public float BaseArmsHealth => _baseArmsHealth;
    [FoldoutGroup("Stats/Health")]
    [SerializeField] private float _baseLegsHealth = 25f;
    public float BaseLegsHealth => _baseLegsHealth;

    [FoldoutGroup("Stats/Movement")]
    [SerializeField] private float _baseMoveSpeed = 2f;
    public float BaseMoveSpeed => _baseMoveSpeed;
    [FoldoutGroup("Stats/Movement")]
    [SerializeField] private float _baseRunMultiplier = 1.6f;
    public float BaseRunMultiplier => _baseRunMultiplier;
    [FoldoutGroup("Stats/Movement")]
    [SerializeField] private float _baseAcceleration = 12f;
    public float BaseAcceleration => _baseAcceleration;
    [FoldoutGroup("Stats/Movement")]
    [SerializeField] private float _baseDeceleration = 16f;
    public float BaseDeceleration => _baseDeceleration;

    [FoldoutGroup("Stats/Dodge")]
    [SerializeField] private float _baseDodgeCooldown = 1f;
    public float BaseDodgeCooldown => _baseDodgeCooldown;
    [FoldoutGroup("Stats/Dodge")]
    [SerializeField] private float _baseDodgeDuration = 0.5f;
    public float BaseDodgeDuration => _baseDodgeDuration;
    [FoldoutGroup("Stats/Dodge")]
    [SerializeField] private float _baseDodgePower = 5f;
    public float BaseDodgePower => _baseDodgePower;
    [FoldoutGroup("Stats/Dodge")]
    [SerializeField] private float _baseDodgeStaminaCost = 20f;
    public float BaseDodgeStaminaCost => _baseDodgeStaminaCost;
    [FoldoutGroup("Stats/Dodge")]
    [SerializeField] private float _baseDodgeFatigueCost = 10f;
    public float BaseDodgeFatigueCost => _baseDodgeFatigueCost;

    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseCarryWeight = 0f;
    public float BaseCarryWeight => _baseCarryWeight;
    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseThirst = 100f;
    public float BaseThirst => _baseThirst;
    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseHunger = 100f;
    public float BaseHunger => _baseHunger;
    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseStamina = 100f;
    public float BaseStamina => _baseStamina;
    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseStaminaRegen = 10;
    public float BaseStaminaRegen => _baseStaminaRegen;
    [FoldoutGroup("Stats/Survival")]
    [SerializeField] private float _baseFatigue = 0f;
    public float BaseFatigue => _baseFatigue;

    [FoldoutGroup("Stats/Mental")]
    [SerializeField] private float _baseSanity = 100f;
    public float BaseSanity => _baseSanity;
    [FoldoutGroup("Stats/Mental")]
    [SerializeField] private float _baseMorale = 100f;
    public float BaseMorale => _baseMorale;
    [FoldoutGroup("Stats/Mental")]
    [SerializeField] private float _basePain = 0f;
    public float BasePain => _basePain;
    [FoldoutGroup("Stats/Mental")]
    [SerializeField] private float _baseIntoxication = 0f;
    public float BaseIntoxication => _baseIntoxication;
}
