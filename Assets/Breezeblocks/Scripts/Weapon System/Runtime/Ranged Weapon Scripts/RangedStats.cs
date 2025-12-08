using Sirenix.OdinInspector;
using UnityEngine;

[CreateAssetMenu(fileName = "Ranged Weapon Stats", menuName = "Breezeblocks/Weapons/Ranged Weapon Stats")]
public class RangedStats : WeaponStats
{
    [FoldoutGroup("Bow Stats", expanded: true)]
    [SerializeField] private string _projectilePrefab;
    public string ProjectilePrefab => _projectilePrefab;

    // ==============================================================


    [FoldoutGroup("Bow Stats/Distance", expanded: true)]
    [SerializeField] private float _minDistance = 5f;
    public float MinDistance => _minDistance;
    [FoldoutGroup("Bow Stats/Distance", expanded: true)]
    [SerializeField] private float _maxDistance = 5f;
    public float MaxDistance => _maxDistance;

    // ==============================================================


    [FoldoutGroup("Bow Stats/Draw", expanded: true)]
    [SerializeField] private float _maxDrawTime = 2f;
    public float MaxDrawTime => _maxDrawTime;
    [FoldoutGroup("Bow Stats/Draw", expanded: true)]
    [SerializeField] private float _drawStaminaDrain = 3f;
    public float DrawStaminaDrain => _drawStaminaDrain;

    // ==============================================================

    [FoldoutGroup("Bow Stats/Speed", expanded: true)]
    [SerializeField] private float _minArrowSpeed = 1f;
    public float MinArrowSpeed => _minArrowSpeed;  
    [FoldoutGroup("Bow Stats/Speed", expanded: true)]
    [SerializeField] private float _maxArrowSpeed = 5f;
    public float MaxArrowSpeed => _maxArrowSpeed;

    // ==============================================================

    [FoldoutGroup("Bow Stats/Damage", expanded: true)]
    [SerializeField] private float _minDamageMult = 0.2f;
    public float MinDamageMult => _minDamageMult;
    [FoldoutGroup("Bow Stats/Damage", expanded: true)]
    [SerializeField] private float _maxDamageMult = 1f;
    public float MaxDamageMult => _maxDamageMult;

    // ==============================================================

    [FoldoutGroup("Bow Stats/Spread", expanded: true)]
    [SerializeField] private float _maxSpreadAngle = 5f;
    public float MaxSpreadAngle => _maxSpreadAngle;
    [FoldoutGroup("Bow Stats/Spread", expanded: true)]
    [SerializeField] private float _minSpreadAngle = 0.2f;
    public float MinSpreadAngle => _minSpreadAngle;

    // ==============================================================
}
