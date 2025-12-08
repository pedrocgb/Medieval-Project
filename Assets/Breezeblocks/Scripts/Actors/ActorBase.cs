using CharactersStats;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D.IK;

public abstract class ActorBase : MonoBehaviour, IStatCollection, IDamageable
{
    #region Variables and Properties
    //// Attributes
    protected CharacterStat _strength;
    protected CharacterStat _dexterity;
    protected CharacterStat _constitution;
    protected CharacterStat _intelligence;
    protected CharacterStat _wisdom;
    protected CharacterStat _charisma;
    public CharacterStat Strength => _strength;
    public CharacterStat Dexterity => _dexterity;
    public CharacterStat Constitution => _constitution;
    public CharacterStat Intelligence => _intelligence;
    public CharacterStat Wisdom => _wisdom;
    public CharacterStat Charisma => _charisma;


    //// Character Stats
    // Health
    protected CharacterStat _headHealth;
    protected CharacterStat _torsoHealth;
    protected CharacterStat _leftArmHealth;
    protected CharacterStat _rightArmHealth;
    protected CharacterStat _leftLegHealth;
    protected CharacterStat _rightLegHealth;
    public float MaxHealth
    {
        get
        {
            return (_headHealth.Value + _torsoHealth.Value + _leftArmHealth.Value + _leftLegHealth.Value) / 4;
        }
    }
    protected CharacterStat _maxFatigue;
    protected CharacterStat _currentFatigueDrain;
    protected float _baseFatigueDrain = 0.05f;
    // Movement
    protected CharacterStat _moveSpeed;
    protected CharacterStat _runMultiplier;
    public CharacterStat MoveSpeed => _moveSpeed;
    public CharacterStat RunMultiplier => _runMultiplier;
    // Needs
    protected CharacterStat _maxThirst;
    protected CharacterStat _currentThirstDrain;
    protected float _baseThirstDrain = 0.03f;
    protected CharacterStat _maxHunger;
    protected CharacterStat _currentHungerDrain;
    protected float _baseHungerDrain = 0.025f;
    // Mental
    protected CharacterStat _maxPain;
    protected CharacterStat _currentPainDrain;
    protected float _basePainDrain = 0.1f;
    protected CharacterStat _maxSanity;
    protected CharacterStat _currentSanityRegen;
    protected float _baseSanityRegen = 0.02f;
    protected CharacterStat _maxMorale;
    protected CharacterStat _currentMoraleRegen;
    protected float _baseMoraleRegen = 0.01f;
    protected CharacterStat _maxIntoxication;
    protected CharacterStat _currentIntoxicationDrain;
    protected float _baseIntoxicationDrain = 0.02f;


    //// Current Stats
    [ShowInInspector, ReadOnly]
    protected float _currentHeadHealth;
    [ShowInInspector, ReadOnly]
    protected float _currentTorsoHealth;
    [ShowInInspector, ReadOnly]
    protected float _currentLeftArmHealth;
    [ShowInInspector, ReadOnly]
    protected float _currentRightArmHealth;
    [ShowInInspector, ReadOnly]
    protected float _currentLeftLegHealth;
    [ShowInInspector, ReadOnly]
    protected float _currentRightLegHealth;

    protected float _acceleration;
    protected float _deceleration;
    protected float _currentCarryWeight;

    [ShowInInspector, ReadOnly]
    protected float _currentFatigue;
    [ShowInInspector, ReadOnly]
    protected float _currentThirst;
    [ShowInInspector, ReadOnly]
    protected float _currentHunger;

    protected float _currentPain;
    protected float _currentSanity;
    protected float _currentMorale;
    protected float _currentIntoxication;

    protected float _runStaminaDrain = 15f;
    protected float _walkFatigueDrain = 0.05f;
    protected float _sprintFatigueDrain = 0.25f;

    public float Acceleration => _acceleration;
    public float Deceleration => _deceleration;
    public float RunStaminaDrain => _runStaminaDrain;
    public float WalkFatigueDrain => _walkFatigueDrain;
    public float SprintFatigueDrain => _sprintFatigueDrain;


    //// Timers
    private float _secondTimer = 0f;
    private float _fractionTimer = 0f;

    //// Stamina System
    protected CharacterStat _maxStamina;
    protected CharacterStat _staminaRegen;

    [ShowInInspector, ReadOnly]
    protected float _currentStamina;
    public float CurrentStamina => _currentStamina;
    private float _consumedStaminaTimeStamp = 0f;
    private float _takeBreathElapsedTime = 0f;

    private bool _consumedStamina = false;
    private bool _takingBreath = false;
    private bool _staminaDepleted = false;
    public bool StaminaDepleted => _staminaDepleted;

    //// Dodge System
    protected CharacterStat _dodgeCooldown;
    public CharacterStat DodgeCooldown => _dodgeCooldown;
    protected CharacterStat _dodgeDuration;
    public CharacterStat DodgeDuration => _dodgeDuration;
    protected CharacterStat _dodgePower;
    public CharacterStat DodgePower => _dodgePower;
    protected CharacterStat _dodgeStaminaCost;
    protected CharacterStat _dodgeFatigueCost;

    //// Weight System
    [FoldoutGroup("Debug")]
    [SerializeField] protected CarryLoadStage _currentLoadStage = CarryLoadStage.Light;
    public CarryLoadStage CurrentLoadStage => _currentLoadStage;
    protected CarryWeightSettings.LoadConfig _currentLoadConfig;
    public CarryWeightSettings.LoadConfig CurrentLoadConfig => _currentLoadConfig;

    protected CharacterStat _maxCarryWeight;
    public CharacterStat MaxCarryWeight => _maxCarryWeight;
    public float CurrentCarryWeight => _currentCarryWeight;

    //// Runtime Stats
    private Dictionary<StatId, CharacterStat> _stats = new();

    /// Protection Stats
    protected CharacterStat _headProtection;
    protected CharacterStat _headNatureProtection;
    protected CharacterStat _headMagicalProtection;
    protected CharacterStat _bodyProtection;
    protected CharacterStat _bodyNatureProtection;
    protected CharacterStat _bodyMagicalProtection;
    protected CharacterStat _armsProtection;
    protected CharacterStat _armsNatureProtection;
    protected CharacterStat _armsMagicalProtection;
    protected CharacterStat _legsProtection;
    protected CharacterStat _legsNatureProtection;
    protected CharacterStat _legsMagicalProtection;

    /// Resistance Stats
    protected CharacterStat _physicalResistance;
    protected CharacterStat _slashingResistance;
    protected CharacterStat _piercingResistance;
    protected CharacterStat _bluntResistance;

    protected CharacterStat _natureResistance;
    protected CharacterStat _fireResistance;
    protected CharacterStat _coldResistance;
    protected CharacterStat _lightningResistance;
    protected CharacterStat _poisonResistance;

    protected CharacterStat _magicalResistance;
    protected CharacterStat _necroticResistance;
    protected CharacterStat _radiantResistance;
    protected CharacterStat _psychicResistance;

    /// Weapon Stats
    protected CharacterStat _attackSpeed;

    /// Damage Stats
    protected CharacterStat _overallDamageModifier;

    protected CharacterStat _physicalDamageModifier;
    protected CharacterStat _slashDamageModifier;
    protected CharacterStat _piercingDamageModifier;
    protected CharacterStat _bludgeoningDamageModifier;

    protected CharacterStat _natureDamageModifier;
    protected CharacterStat _fireDamageModifier;
    protected CharacterStat _coldDamageModifier;
    protected CharacterStat _lightningDamageModifier;
    protected CharacterStat _poisonDamageModifier;

    protected CharacterStat _magicalDamageModifier;
    protected CharacterStat _necroticDamageModifier;
    protected CharacterStat _radiantDamageModifier;
    protected CharacterStat _psychicDamageModifier;

    /// Weapons and Equipments
    private WeaponConfig _equippedWeapon;
    public WeaponConfig EquippedWeapon => _equippedWeapon;
    private bool _isDrawingBow = false;
    public bool IsDrawingBow { get  { return _isDrawingBow; } set { _isDrawingBow = value; } }

    //// Components
    [FoldoutGroup("Components", expanded: true)]
    [SerializeField] protected ActorUI _ui;
    public ActorUI UI => _ui;
    #endregion

    // ======================================================================

    #region Unity Methods
    protected virtual void Start()
    {
        UpdateLoadStage(CurrentLoadStage);

        _currentThirstDrain = new CharacterStat(_baseThirstDrain);
        _currentHungerDrain = new CharacterStat(_baseHungerDrain);
        _currentFatigueDrain = new CharacterStat(_baseFatigueDrain);

        _currentPainDrain = new CharacterStat(_basePainDrain);
        _currentIntoxicationDrain = new CharacterStat(_baseIntoxicationDrain);
        _currentMoraleRegen = new CharacterStat(_baseMoraleRegen);
        _currentSanityRegen = new CharacterStat(_baseSanityRegen);

        _headProtection = new CharacterStat(0f);
        _headNatureProtection = new CharacterStat(0f);
        _headMagicalProtection = new CharacterStat(0f);
        _bodyProtection = new CharacterStat(0f);
        _bodyNatureProtection = new CharacterStat(0f);
        _bodyMagicalProtection = new CharacterStat(0f);
        _armsProtection = new CharacterStat(0f);
        _armsNatureProtection = new CharacterStat(0f);
        _armsMagicalProtection = new CharacterStat(0f);
        _legsProtection = new CharacterStat(0f);
        _legsNatureProtection = new CharacterStat(0f);
        _legsMagicalProtection = new CharacterStat(0f);

        _physicalResistance = new CharacterStat(0f);
        _slashingResistance = new CharacterStat(0f);
        _piercingResistance = new CharacterStat(0f);
        _bluntResistance = new CharacterStat(0f);

        _natureResistance = new CharacterStat(0f);
        _fireResistance = new CharacterStat(0f);
        _coldResistance = new CharacterStat(0f);
        _lightningResistance = new CharacterStat(0f);
        _poisonResistance = new CharacterStat(0f);

        _magicalResistance = new CharacterStat(0f);
        _necroticResistance = new CharacterStat(0f);
        _radiantResistance = new CharacterStat(0f);
        _psychicResistance = new CharacterStat(0f);

        _overallDamageModifier = new CharacterStat(0f);

        _physicalDamageModifier = new CharacterStat(0f);
        _slashDamageModifier = new CharacterStat(0f);
        _piercingDamageModifier = new CharacterStat(0f);
        _bludgeoningDamageModifier = new CharacterStat(0f);

        _natureDamageModifier = new CharacterStat(0f);
        _fireDamageModifier = new CharacterStat(0f);
        _coldDamageModifier = new CharacterStat(0f);
        _lightningDamageModifier = new CharacterStat(0f);
        _poisonDamageModifier = new CharacterStat(0f);

        _magicalDamageModifier = new CharacterStat(0f);
        _necroticDamageModifier = new CharacterStat(0f);
        _radiantDamageModifier = new CharacterStat(0f);
        _psychicDamageModifier = new CharacterStat(0f);

    RegisterStats();
    }

    protected virtual void Update()
    {
        DeltaTimeChecker();
        FractionChecker();
        SecondChecker();
        StaminaCooldownHandler();
    }
    #endregion

    // =====================================================================

    #region Time Checkers
    /// <summary>
    /// Check when a second has passed.
    /// </summary>
    private void SecondChecker()
    {
        _secondTimer += Time.deltaTime;

        if (_secondTimer >= Constants.SECONDS)
        {
            PassiveThirstGain();
            PassiveHungerGain();
            PassiveFatigueDrain();

            _secondTimer = 0f;            
        }
    }

    /// <summary>
    /// Check when a fraction of a second has passed.
    /// </summary>
    private void FractionChecker()
    {
        _fractionTimer += Time.deltaTime;

        if (_fractionTimer >= Constants.FRACTION)
        {
            PassiveStaminaRegen();

            _fractionTimer = 0f;
        }
    }

    /// <summary>
    /// Check every frame for delta time related updates.
    /// </summary>
    private void DeltaTimeChecker()
    {
         ElapseBreathTaking();
    }
    #endregion

    // ======================================================================

    #region Stats Methods
    /// <summary>
    /// Register all stats in the dictionary for easy access.
    /// </summary>
    protected void RegisterStats()
    {
        _stats = new Dictionary<StatId, CharacterStat>
        {
            { StatId.HeadProtection, _headProtection },
            { StatId.HeadNatureProtection, _headNatureProtection },
            { StatId.HeadMagicalProtection, _headMagicalProtection },
            { StatId.BodyProtection, _bodyProtection },
            { StatId.BodyNatureProtection, _bodyNatureProtection },
            { StatId.BodyMagicalProtection, _bodyMagicalProtection },
            { StatId.ArmsProtection, _armsProtection },
            { StatId.ArmsNatureProtection, _armsNatureProtection },
            { StatId.ArmsMagicalProtection, _armsMagicalProtection },
            { StatId.LegsProtection, _legsProtection },
            { StatId.LegsNatureProtection, _legsNatureProtection },
            { StatId.LegsMagicalProtection, _legsMagicalProtection },

            { StatId.PhysicalResistance, _physicalResistance },
            { StatId.SlashingResistance, _slashingResistance },
            { StatId.PiercingResistance, _piercingResistance },
            { StatId.BluntResistance, _bluntResistance },

            { StatId.NatureResistance, _natureResistance },
            { StatId.FireResistance, _fireResistance },
            { StatId.ColdResistance, _coldResistance },
            { StatId.LightningResistance, _lightningResistance },
            { StatId.PoisonResistance, _poisonResistance },

            { StatId.MagicalResistance, _magicalResistance },
            { StatId.NecroticResistance, _necroticResistance },
            { StatId.RadiantResistance, _radiantResistance },
            { StatId.PsychicResistance, _psychicResistance },

            { StatId.OverallDamageModifier, _overallDamageModifier },

            { StatId.PhysicalDamageModifier, _physicalDamageModifier },
            { StatId.SlashingDamageModifier, _slashDamageModifier },
            { StatId.PiercingDamageModifier, _piercingDamageModifier },
            { StatId.BludgeoninDamageModifier, _bludgeoningDamageModifier },

            { StatId.NatureDamageModifier, _natureDamageModifier },
            { StatId.FireDamagedModifier, _fireDamageModifier },
            { StatId.ColdDamageModifier, _coldDamageModifier },
            { StatId.LightningDamageModifier, _lightningDamageModifier },
            { StatId.PoisonDamageModifier, _poisonDamageModifier },

            { StatId.MagicalDamageModifier, _magicalDamageModifier },
            { StatId.NecroticDamageModifier, _necroticDamageModifier },
            { StatId.RadiantDamageModifier, _radiantDamageModifier },
            { StatId.PsychicDamageModifier, _psychicDamageModifier },
        };

        Debug.Log(_stats.Count + " stats registered for " + gameObject.name);
    }

    /// <summary>
    /// Get a stat by its ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public CharacterStat GetStat(StatId id)
    {
        return _stats.TryGetValue(id, out var stat) ? stat : null;
    }

    /// <summary>
    /// Get all stats in the collection.
    /// </summary>
    public IEnumerable<CharacterStat> AllStats => _stats.Values;
    #endregion

    // ======================================================================

    #region Attribute Update Methods
    /// <summary>
    /// Update Strength related stats.
    /// </summary>
    /// <param name="stats"></param>
    public virtual void UpdateStrengthAttribute(ActorStats stats)
    {
        _maxCarryWeight = new CharacterStat(stats.BaseCarryWeight + (_strength.Value * Constants.WEIGHT_PER_STRENGTH));
    }

    /// <summary>
    /// Update Dexterity related stats.
    /// </summary>
    /// <param name="stats"></param>
    public virtual void UpdateDexterityAttribute(ActorStats stats)
    {
        _moveSpeed = new CharacterStat(stats.BaseMoveSpeed + (_dexterity.Value * Constants.MOVESPEED_PER_DEXTERITY));
        _runMultiplier = new CharacterStat(stats.BaseRunMultiplier + (_dexterity.Value * Constants.RUNMULTIPLIER_PER_DEXTERITY));
    }

    /// <summary>
    /// Update Constitution related stats.
    /// </summary>
    /// <param name="stats"></param>
    public virtual void UpdateConstitutionAttribute(ActorStats stats)
    {
        _headHealth = new CharacterStat(stats.BaseHeadHealth + (_constitution.Value * Constants.HEAD_HEALTH_PER_CONSTITUTION));
        _torsoHealth = new CharacterStat(stats.BaseTorsoHealth + (_constitution.Value * Constants.TORSO_HEALTH_PER_CONSTITUTION));
        _leftArmHealth = new CharacterStat(stats.BaseArmsHealth + (_constitution.Value * Constants.ARMS_HEALTH_PER_CONSTITUTION));
        _rightArmHealth = new CharacterStat(stats.BaseArmsHealth + (_constitution.Value * Constants.ARMS_HEALTH_PER_CONSTITUTION));
        _leftLegHealth = new CharacterStat(stats.BaseLegsHealth + (_constitution.Value * Constants.LEGS_HEALTH_PER_CONSTITUTION));
        _rightLegHealth = new CharacterStat(stats.BaseLegsHealth + (_constitution.Value * Constants.LEGS_HEALTH_PER_CONSTITUTION));

        _maxStamina = new CharacterStat(stats.BaseStamina + (_constitution.Value * Constants.STAMINA_PER_CONSTITUTION));
        _maxFatigue = new CharacterStat(stats.BaseFatigue + (_constitution.Value * Constants.FATIGUE_PER_CONSTITUTION));
    }
    #endregion

    // ======================================================================

    #region Damage Methods
    public float GetDamageWithModifier(float damageValue, DamageType type)
    {
        return CalculateDamage(damageValue, type);
    }

    private float CalculateDamage(float initialDamage, DamageType type)
    {
        // 1) Type-specific modifier (e.g. +10% Slash, +5% Fire, etc.)
        float damageModType = type switch
        {
            DamageType.Slash => _slashDamageModifier.Value,
            DamageType.Piercing => _piercingDamageModifier.Value,
            DamageType.Bludgeoning => _bludgeoningDamageModifier.Value,

            DamageType.Fire => _fireDamageModifier.Value,
            DamageType.Cold => _coldDamageModifier.Value,
            DamageType.Lightning => _lightningDamageModifier.Value,
            DamageType.Poison => _poisonDamageModifier.Value,

            DamageType.Necrotic => _necroticDamageModifier.Value,
            DamageType.Radiance => _radiantDamageModifier.Value,
            DamageType.Psychic => _psychicDamageModifier.Value,

            _ => 0f
        };

        // 2) Group modifier (physical / nature / magical)
        float groupModifier = type switch
        {
            // Physical
            DamageType.Slash or DamageType.Piercing or DamageType.Bludgeoning
                => _physicalDamageModifier.Value,

            // Nature (elemental / natural)
            DamageType.Fire or DamageType.Cold or DamageType.Lightning or DamageType.Poison
                => _natureDamageModifier.Value,

            // Magical
            DamageType.Necrotic or DamageType.Radiance or DamageType.Psychic
                => _magicalDamageModifier.Value,

            _ => 0f
        };

        // 3) Overall modifier (applies to all damage)
        float overall = _overallDamageModifier.Value;

        // 4) Sum all percentage bonuses, then add base 100% (1f)
        float totalBonus = damageModType + groupModifier + overall;
        float finalMulitplier = 1f + totalBonus;

        return initialDamage * finalMulitplier;
    }
    #endregion

    // ======================================================================

    #region Damage Methods
    public void TakeDamage(float damage, DamageType damageType, Limbs damageLimb)
    {
        DamageLimb(damage, damageType, damageLimb);
    }

    private void DamageLimb(float incomingDamage, DamageType damageType, Limbs damagedLimb)
    {
        // 1) Pick resistance based on damage type
        float resistance = damageType switch
        {
            DamageType.Slash => _slashingResistance.Value + _physicalResistance.Value,
            DamageType.Piercing => _piercingResistance.Value + _physicalResistance.Value,
            DamageType.Bludgeoning => _bluntResistance.Value + _physicalResistance.Value,

            DamageType.Fire => _fireResistance.Value + _natureResistance.Value,
            DamageType.Cold => _coldResistance.Value + _physicalResistance.Value,
            DamageType.Lightning => _lightningResistance.Value + _physicalResistance.Value,
            DamageType.Poison => _poisonResistance.Value + _physicalResistance.Value,

            DamageType.Radiance => _radiantResistance.Value + _magicalResistance.Value,
            DamageType.Necrotic => _necroticResistance.Value + _magicalResistance.Value,
            DamageType.Psychic => _psychicResistance.Value + _magicalResistance.Value,
            _ => 0f
        };

        // 2) Apply to the correct limb + protection
        switch (damagedLimb)
        {
            case Limbs.Head:
                ApplyDamageToHealth(ref _currentHeadHealth, incomingDamage, resistance, _headProtection.Value);
                break;

            case Limbs.Torso:
                ApplyDamageToHealth(ref _currentTorsoHealth, incomingDamage, resistance, _bodyProtection.Value);
                break;

            case Limbs.LeftArm:
                ApplyDamageToHealth(ref _currentLeftArmHealth, incomingDamage, resistance, _armsProtection.Value);
                break;

            case Limbs.RightArm:
                ApplyDamageToHealth(ref _currentRightArmHealth, incomingDamage, resistance, _armsProtection.Value);
                break;

            case Limbs.LeftLeg:
                ApplyDamageToHealth(ref _currentLeftLegHealth, incomingDamage, resistance, _armsProtection.Value);
                break;

            case Limbs.RightLeg:
                ApplyDamageToHealth(ref _currentRightLegHealth, incomingDamage, resistance, _armsProtection.Value);
                break;
        }
    }

    private void ApplyDamageToHealth(ref float limbHealth,
                                     float incomingDamage,
                                     float resistancePercent,
                                     float flatProtection)
    {
        // incoming * (1 - R) - flat
        float afterResist = incomingDamage * (1f - resistancePercent);
        float afterFlat = afterResist - flatProtection;
        float finalDamage = Mathf.Max(0f, afterFlat);

        limbHealth -= finalDamage;
    }

    public void Die()
    {
        
    }
    #endregion

    // ======================================================================

    #region Passive Stat Changes
    /// <summary>
    /// Passive Thirst Gain over time.
    /// </summary>
    protected virtual void PassiveThirstGain()
    {
        // Thirst Drain
        if (_currentThirst <= _maxThirst.Value)
        {
            _currentThirst += _currentThirstDrain.Value;
            _currentThirst = Mathf.Clamp(_currentThirst, 0f, _maxThirst.Value);
        }
    }

    /// <summary>
    /// Passive Hunger Gain over time.
    /// </summary>
    protected virtual void PassiveHungerGain()
    {
        // Hunger Drain
        if (_currentHunger <= _maxHunger.Value)
        {
            _currentHunger += _currentHungerDrain.Value;
            _currentHunger = Mathf.Clamp(_currentHunger, 0f, _maxHunger.Value);
        }
    }

    /// <summary>
    /// Passive Fatigue Drain over time.
    /// </summary>
    protected virtual void PassiveFatigueDrain()
    {
        // Fatigue Drain
        if (_currentFatigue > 0f)
        {
            _currentFatigue -= _currentFatigueDrain.Value * _currentLoadConfig.fatiguePenalty;
            _currentFatigue = Mathf.Clamp(_currentFatigue, 0f, _maxFatigue.Value);
        }
    }

    /// <summary>
    /// Passive Stamina Regen over time.
    /// </summary>
    protected virtual void PassiveStaminaRegen()
    {
        // Stamina Regen
        if (_consumedStamina) return;

        if (_staminaDepleted)
            _staminaDepleted = false;

        if (_currentStamina < _maxStamina.Value)
        {
            _currentStamina += _staminaRegen.Value / _currentLoadConfig.staminaPenalty;
            _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina.Value);
        }
    }

    /// <summary>
    /// Handle Stamina Cooldown after consumption.
    /// </summary>
    protected virtual void StaminaCooldownHandler()
    {
        if (_consumedStamina && Time.time >= _consumedStaminaTimeStamp)
        {
            _consumedStamina = false;
        }
    }
    #endregion

    // ======================================================================

    #region Active Stat Changes
    /// <summary>
    /// Drain Stamina by a certain amount.
    /// </summary>
    /// <param name="amount"></param>
    public virtual void DrainStamina(float amount)
    {
        if (_staminaDepleted) return;

        _currentStamina -= (amount * _currentLoadConfig.staminaPenalty);
        _currentStamina = Mathf.Clamp(_currentStamina, 0f, _maxStamina.Value);

        _consumedStamina = true;
        _consumedStaminaTimeStamp = Time.time + Constants.STAMINA_COOLDOWN;

        if (_currentStamina <= 0f)
            TakeBreath();
        
    }

    public bool CanConsumeStamina(float amount)
    {
        if (_currentStamina >= amount)
            return true;

        return false;
    }

    /// <summary>
    /// Starts breath taking when stamina is depleted.
    /// </summary>
    protected void TakeBreath()
    {
        _staminaDepleted = true;
        _takingBreath = true;
        _takeBreathElapsedTime = 0f;
    }

    /// <summary>
    /// Elapse the breath taking time.
    /// </summary>
    private void ElapseBreathTaking()
    {
        if (_takingBreath)
        {
            _takeBreathElapsedTime += Time.deltaTime;

            if (_takeBreathElapsedTime >= Constants.MAX_BREATH_TAKING_TIME)
                _takingBreath = false;
        }  
    }

    /// <summary>
    /// Check if the elapsed take breath time has passed a certain value.
    /// </summary>
    /// <param name="Value"></param>
    /// <returns></returns>
    public bool CheckElapsedTakeBreathTime(float Value)
    {
        if (_takingBreath)
        {
            if (_takeBreathElapsedTime >= Value)
                return true;
        }
        else if (!_takingBreath && !_staminaDepleted)
            return true;

        return false;
    }

    /// <summary>
    /// Drain Fatigue by a certain amount.
    /// </summary>
    /// <param name="amount"></param>
    public virtual void DrainFatigue(float amount)
    {
        _currentFatigue -= (amount * _currentLoadConfig.fatiguePenalty);
        _currentFatigue = Mathf.Clamp(_currentFatigue, 0f, _maxFatigue.Value);
    }
    #endregion

    // ======================================================================

    #region Movement Stat Changes
    /// <summary>
    /// Check if the actor can sprint.
    /// </summary>
    /// <returns></returns>
    public virtual bool CanSprint()
    {
        if (CheckElapsedTakeBreathTime(Constants.RUN_STAMINA_THRESHOLD) && _currentLoadConfig.canRun && !StaminaDepleted)
            return true;

        return false;
    }

    /// <summary>
    /// Drain Sprint Stamina over time.
    /// </summary>
    public virtual void DrainSprintStamina()
    {
        DrainStamina((_runStaminaDrain * _currentLoadConfig.staminaPenalty) * Time.deltaTime);
    }

    /// <summary>
    /// Drain Sprint Fatigue over time.
    /// </summary>
    public virtual void DrainSprintFatigue()
    {
        DrainFatigue((_sprintFatigueDrain * _currentLoadConfig.fatiguePenalty) * Time.deltaTime);
    }

    /// <summary>
    /// Drain Walk Fatigue over time.
    /// </summary>
    public virtual void DrainWalkFatigue()
    {
        DrainFatigue((_walkFatigueDrain * _currentLoadConfig.fatiguePenalty) * Time.deltaTime);
    }
    #endregion

    // ======================================================================

    #region Dodge Methods
    /// <summary>
    /// Check if the actor can dodge.
    /// </summary>
    /// <returns></returns>
    public bool CanDodge()
    {
        if (_currentStamina >= _dodgeStaminaCost.Value * _currentLoadConfig.staminaPenalty && !StaminaDepleted)
            return true;

        return false;
    }

    /// <summary>
    /// Drain Dodge Stamina cost.
    /// </summary>
    public virtual void DrainDodgeStamina()
    {
        DrainStamina((_dodgeStaminaCost.Value * _currentLoadConfig.staminaPenalty));
    }

    /// <summary>
    /// Drain Dodge Fatigue cost.
    /// </summary>
    public virtual void DrainDodgeFatigue()
    {
        DrainFatigue((_dodgeFatigueCost.Value * _currentLoadConfig.fatiguePenalty));
    }
    #endregion

    // ======================================================================

    #region Weight System Methods
    /// <summary>
    /// Update the current carry load stage and config.
    /// </summary>
    /// <param name="newStage"></param>
    public void UpdateLoadStage(CarryLoadStage newStage)
    {
        _currentLoadStage = newStage;
        _currentLoadConfig = CarryWeightSettings.GetCurrentLoadConfig(_currentLoadStage);
    }
    #endregion
}
