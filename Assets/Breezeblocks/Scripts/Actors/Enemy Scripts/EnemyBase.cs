using CharactersStats;
using Sirenix.OdinInspector;
using UnityEngine;

public class EnemyBase : ActorBase
{
    [FoldoutGroup("Stats", expanded: true)]
    [SerializeField] EnemyStats _enemyStats = null;

    protected override void Start()
    {
        base.Start();

        _strength = new CharacterStat(_enemyStats.BaseStrength);
        _dexterity = new CharacterStat(_enemyStats.BaseDexterity);
        _constitution = new CharacterStat(_enemyStats.BaseConstitution);
        _intelligence = new CharacterStat(_enemyStats.BaseIntelligence);
        _wisdom = new CharacterStat(_enemyStats.BaseWisdom);
        _charisma = new CharacterStat(_enemyStats.BaseCharisma);

        UpdateStrengthAttribute(_enemyStats);
        UpdateDexterityAttribute(_enemyStats);
        UpdateConstitutionAttribute(_enemyStats);

        _acceleration = _enemyStats.BaseAcceleration;
        _deceleration = _enemyStats.BaseDeceleration;

        _dodgeCooldown = new CharacterStat(_enemyStats.BaseDodgeCooldown);
        _dodgeDuration = new CharacterStat(_enemyStats.BaseDodgeDuration);
        _dodgePower = new CharacterStat(_enemyStats.BaseDodgePower);
        _dodgeStaminaCost = new CharacterStat(_enemyStats.BaseDodgeStaminaCost);
        _dodgeFatigueCost = new CharacterStat(_enemyStats.BaseDodgeFatigueCost);

        _staminaRegen = new CharacterStat(_enemyStats.BaseStaminaRegen);
        _maxThirst = new CharacterStat(_enemyStats.BaseThirst);
        _maxHunger = new CharacterStat(_enemyStats.BaseHunger);

        _maxSanity = new CharacterStat(_enemyStats.BaseSanity);
        _maxMorale = new CharacterStat(_enemyStats.BaseMorale);
        _maxPain = new CharacterStat(_enemyStats.BasePain);
        _maxIntoxication = new CharacterStat(_enemyStats.BaseIntoxication);


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
}
