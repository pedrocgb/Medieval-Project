using UnityEngine;
using Sirenix.OdinInspector;

[CreateAssetMenu(fileName = "Melee Attack Shape", menuName = "Breezeblocks/Weapons/Melee Attack Shape")]
public class MeleeAttackShapeStats : WeaponStats
{
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] private MeleeAttackShape _shape = MeleeAttackShape.Swipe;
    public MeleeAttackShape Shape => _shape;
    [FoldoutGroup("Shape", expanded: true)]
    [SerializeField] private float _motionDuration = 0.25f;
    public float MotionDuration => _motionDuration;

    // ======================================================================

    [FoldoutGroup("Shape/Swipe", expanded: true), ShowIf("_shape", MeleeAttackShape.Swipe)]
    [SerializeField] private float _swipeArcDegrees = 90f;
    public float SwipeArcDegree => _swipeArcDegrees;
    [FoldoutGroup("Shape/Swipe", expanded: true), ShowIf("_shape", MeleeAttackShape.Swipe)]
    [SerializeField] private float _swipeCenterOffset = 0f;
    public float SwipeCenterOffset => _swipeCenterOffset;
    [FoldoutGroup("Shape/Swipe", expanded: true), ShowIf("_shape", MeleeAttackShape.Swipe)]
    [SerializeField] private float _swipeRadius = 1f;
    public float SwipeRadius => _swipeRadius;

    // ======================================================================

    [FoldoutGroup("Shape/Stab", expanded: true), ShowIf("_shape", MeleeAttackShape.Stab)]
    [SerializeField] private float _stabDistance = 1.5f;
    public float StabDistance => _stabDistance;

    [FoldoutGroup("Shape/Stab", expanded: true), ShowIf("_shape", MeleeAttackShape.Stab)]
    [SerializeField] private float _stabSideOffset = 0f;
    public float StabSideOffset => _stabSideOffset;

    // ======================================================================

    [FoldoutGroup("Hitbox", expanded: true)]
    [SerializeField] private float _hitboxRadius = 0.3f;
    public float HitBoxRadius => _hitboxRadius;

    [FoldoutGroup("Hitbox", expanded: true)]
    [SerializeField] private bool _stopOnHit = true;
    public bool StopOnHit => _stopOnHit;

    // ======================================================================

    [FoldoutGroup("Effects", expanded: true)]
    [SerializeField] private AnimationCurve damageOverTime = AnimationCurve.Linear(0, 1, 1, 1);
    [FoldoutGroup("Effects", expanded: true)]
    [SerializeField] private string _attackEffectPrefab = string.Empty;
    public string AttackEffectPrefab => _attackEffectPrefab;
    [FoldoutGroup("Effects", expanded: true)]
    [SerializeField] private string _hitEffectPrefab = string.Empty;
    public string HitEffectPrefab => _hitEffectPrefab;
}

public enum MeleeAttackShape
{
    Swipe,
    Stab
}
