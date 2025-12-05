using CharactersStats;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

public class PlayerBase : ActorBase
{
    #region Variables and Properties
    [FoldoutGroup("Stats")]
    [SerializeField] private PlayerStats _playerStats;
    public PlayerStats PlayerStats => _playerStats;

    private Player _controlScheme;
    public Player ControlScheme => _controlScheme;
    #endregion

    // ==============================================================

    private void Awake()
    {
        _controlScheme = ReInput.players.GetPlayer(0);
    }

    protected override void Start()
    {
        base.Start();

        _strength = new CharacterStat(_playerStats.BaseStrength);
        _dexterity = new CharacterStat(_playerStats.BaseDexterity);
        _constitution = new CharacterStat(_playerStats.BaseConstitution);
        _intelligence = new CharacterStat(_playerStats.BaseIntelligence);
        _wisdom = new CharacterStat(_playerStats.BaseWisdom);
        _charisma = new CharacterStat(_playerStats.BaseCharisma);

        UpdateStrengthAttribute(_playerStats);
        UpdateDexterityAttribute(_playerStats);
        UpdateConstitutionAttribute(_playerStats);
        
        _acceleration = _playerStats.BaseAcceleration;
        _deceleration = _playerStats.BaseDeceleration;

        _dodgeCooldown = new CharacterStat(_playerStats.BaseDodgeCooldown);
        _dodgeDuration = new CharacterStat(_playerStats.BaseDodgeDuration);
        _dodgePower = new CharacterStat(_playerStats.BaseDodgePower);
        _dodgeStaminaCost = new CharacterStat(_playerStats.BaseDodgeStaminaCost);
        _dodgeFatigueCost = new CharacterStat(_playerStats.BaseDodgeFatigueCost);

        _staminaRegen = new CharacterStat(_playerStats.BaseStaminaRegen);
        _maxThirst = new CharacterStat(_playerStats.BaseThirst);
        _maxHunger = new CharacterStat(_playerStats.BaseHunger);

        _maxSanity = new CharacterStat(_playerStats.BaseSanity);
        _maxMorale = new CharacterStat(_playerStats.BaseMorale);
        _maxPain = new CharacterStat(_playerStats.BasePain);
        _maxIntoxication = new CharacterStat(_playerStats.BaseIntoxication);


        // Initialize current stats to max values
        _currentHeadHealth = _headHealth.Value;
        _currentTorsoHealth = _torsoHealth.Value;
        _currentLeftArmHealth = _leftArmHealth.Value;
        _currentRightArmHealth = _rightArmHealth.Value;
        _currentLeftLegHealth = _leftLegHealth.Value;
        _currentRightLegHealth = _rightLegHealth.Value;
        _currentStamina = _maxStamina.Value;
        _currentFatigue = _maxFatigue.Value;
        _currentSanity = _maxSanity.Value;
        _currentMorale = _maxMorale.Value;
        _currentPain = 0f;
        _currentIntoxication = 0f;
    }

    // ==============================================================
}
