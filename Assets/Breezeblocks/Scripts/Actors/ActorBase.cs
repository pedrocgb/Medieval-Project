using CharactersStats;
using Sirenix.OdinInspector;
using UnityEngine;

public abstract class ActorBase : MonoBehaviour
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
    protected float _currentHeadHealth;
    protected float _currentTorsoHealth;
    protected float _currentLeftArmHealth;
    protected float _currentRightArmHealth;
    protected float _currentLeftLegHealth;
    protected float _currentRightLegHealth;

    protected float _acceleration;
    protected float _deceleration;
    protected float _currentCarryWeight;

    protected float _currentFatigue;
    protected float _currentThirst;
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

    protected float _currentStamina;
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

    //// Defensive Stats
    protected CharacterStat _headProtection;
    protected CharacterStat _torsoProtection;
    protected CharacterStat _armsProtection;
    protected CharacterStat _legsProtection;

    protected CharacterStat _physicalResistance;
    protected CharacterStat _slashResistance;
    protected CharacterStat _pierceResistance;
    protected CharacterStat _bluntResistance;

    protected CharacterStat _fireResistance;
    protected CharacterStat _coldResistance;
    protected CharacterStat _electricResistance;
    protected CharacterStat _acidResistance;


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

    private void FractionChecker()
    {
        _fractionTimer += Time.deltaTime;

        if (_fractionTimer >= Constants.FRACTION)
        {
            PassiveStaminaRegen();

            _fractionTimer = 0f;
        }
    }

    private void DeltaTimeChecker()
    {
         ElapseBreathTaking();
    }
    #endregion

    // ======================================================================

    public virtual void UpdateStrengthAttribute(ActorStats stats)
    {
        _maxCarryWeight = new CharacterStat(stats.BaseCarryWeight + (_strength.Value * Constants.WEIGHT_PER_STRENGTH));
    }

    public virtual void UpdateDexterityAttribute(ActorStats stats)
    {
        _moveSpeed = new CharacterStat(stats.BaseMoveSpeed + (_dexterity.Value * Constants.MOVESPEED_PER_DEXTERITY));
        _runMultiplier = new CharacterStat(stats.BaseRunMultiplier + (_dexterity.Value * Constants.RUNMULTIPLIER_PER_DEXTERITY));
    }

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

    // ======================================================================

    #region Passive Stat Changes
    protected virtual void PassiveThirstGain()
    {
        // Thirst Drain
        if (_currentThirst <= _maxThirst.Value)
        {
            _currentThirst += _currentThirstDrain.Value;
            _currentThirst = Mathf.Clamp(_currentThirst, 0f, _maxThirst.Value);
        }
    }

    protected virtual void PassiveHungerGain()
    {
        // Hunger Drain
        if (_currentHunger <= _maxHunger.Value)
        {
            _currentHunger += _currentHungerDrain.Value;
            _currentHunger = Mathf.Clamp(_currentHunger, 0f, _maxHunger.Value);
        }
    }

    protected virtual void PassiveFatigueDrain()
    {
        // Fatigue Drain
        if (_currentFatigue > 0f)
        {
            _currentFatigue -= _currentFatigueDrain.Value * _currentLoadConfig.fatiguePenalty;
            _currentFatigue = Mathf.Clamp(_currentFatigue, 0f, _maxFatigue.Value);
        }
    }

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

    protected void TakeBreath()
    {
        _staminaDepleted = true;
        _takingBreath = true;
        _takeBreathElapsedTime = 0f;
    }

    private void ElapseBreathTaking()
    {
        if (_takingBreath)
        {
            _takeBreathElapsedTime += Time.deltaTime;

            if (_takeBreathElapsedTime >= Constants.MAX_BREATH_TAKING_TIME)
                _takingBreath = false;
        }  
    }

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

    public virtual void DrainFatigue(float amount)
    {
        _currentFatigue -= (amount * _currentLoadConfig.fatiguePenalty);
        _currentFatigue = Mathf.Clamp(_currentFatigue, 0f, _maxFatigue.Value);
    }
    #endregion

    // ======================================================================

    #region Movement Stat Changes
    public virtual bool CanSprint()
    {
        if (CheckElapsedTakeBreathTime(Constants.RUN_STAMINA_THRESHOLD) && _currentLoadConfig.canRun && !StaminaDepleted)
            return true;

        return false;
    }

    public virtual void DrainSprintStamina()
    {
        DrainStamina((_runStaminaDrain * _currentLoadConfig.staminaPenalty) * Time.deltaTime);
    }

    public virtual void DrainSprintFatigue()
    {
        DrainFatigue((_sprintFatigueDrain * _currentLoadConfig.fatiguePenalty) * Time.deltaTime);
    }

    public virtual void DrainWalkFatigue()
    {
        DrainFatigue((_walkFatigueDrain * _currentLoadConfig.fatiguePenalty) * Time.deltaTime);
    }
    #endregion

    // ======================================================================

    #region Dodge Methods
    public bool CanDodge()
    {
        if (_currentStamina >= _dodgeStaminaCost.Value * _currentLoadConfig.staminaPenalty && !StaminaDepleted)
            return true;

        return false;
    }

    public virtual void DrainDodgeStamina()
    {
        DrainStamina((_dodgeStaminaCost.Value * _currentLoadConfig.staminaPenalty));
    }

    public virtual void DrainDodgeFatigue()
    {
        DrainFatigue((_dodgeFatigueCost.Value * _currentLoadConfig.fatiguePenalty));
    }
    #endregion

    // ======================================================================

    #region Weight System Methods
    public void UpdateLoadStage(CarryLoadStage newStage)
    {
        _currentLoadStage = newStage;
        _currentLoadConfig = CarryWeightSettings.GetCurrentLoadConfig(_currentLoadStage);
    }
    #endregion
}
