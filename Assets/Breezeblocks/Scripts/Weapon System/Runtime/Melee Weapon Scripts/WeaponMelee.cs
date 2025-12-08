using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using ObjectPool;

public class WeaponMelee : WeaponBase, IWeapon
{
    #region Variables and Properties

    [FoldoutGroup("Hitbox", expanded: true)]
    [SerializeField] private LayerMask _hitLayers;
    [FoldoutGroup("Hitbox", expanded: true)]
    [SerializeField] private Transform _firePoint;

    private bool _isAttacking;
    private Coroutine _attackRoutine;
    #endregion

    // ======================================================================

    #region  IWeapon Implementation_firePoint
    public override void OnEquipped(ActorBase owner, ItemInstance itemInstance, WeaponConfig config)
    {
        base.OnEquipped(owner, itemInstance, config);
    }

    public override void OnUnequipped()
    {
        if (_attackRoutine != null)
        {
            StopCoroutine(_attackRoutine);
            _attackRoutine = null;
        }
        _isAttacking = false;
    }

    public override void PerformPrimaryAttack()
    {
        if (_config == null )
            return;
        
        TryStartAttack(_config.PrimaryAttackShape);
    }

    public override void PerformSecondaryAttack()
    {
        if (_config == null)
            return;

        TryStartAttack(_config.SecondaryAttackShape);
    }

    public override void HoldPrimaryAttack() {}
    public override void HoldSecondaryAttack() {}
    public override void ReleasePrimaryAttack() {}
    public override void ReleaseSecondaryAttack() {}
    #endregion

    // ======================================================================

    #region  Attack Logic
    private void TryStartAttack(MeleeAttackShapeStats attackDef)
    {
        if (_isAttacking ||
            !_owner.CanConsumeStamina(attackDef.StaminaCost) ||
            _attackSpeedTimeStamp > Time.time)
            return;

        _owner.DrainStamina(attackDef.StaminaCost);
        _owner.DrainStamina(attackDef.FatigueCost);

        ObjectPooler.instance.SpawnFromPool(attackDef.AttackEffectPrefab, _firePoint.position, transform.rotation);

        _attackRoutine = StartCoroutine(AttackRoutine(attackDef));

        _attackSpeedTimeStamp = Time.time + attackDef.AttackSpeed;
    }

    private IEnumerator AttackRoutine(MeleeAttackShapeStats def)
    {
        _isAttacking = true;

        float duration = Mathf.Max(0.01f, def.MotionDuration);
        float t = 0f;

        // Pivot is the weapon object itself (which you rotate with the mouse)
        Transform pivot = _firePoint;

        // We no longer cache forward & angles here; we'll compute them inside the loop
        var alreadyHit = new HashSet<Collider2D>();

        while (t < duration)
        {
            float normalized = t / duration;
            Vector3 pivotPos = pivot.position; // weapon may move with the player

            Vector2 rayOrigin = pivotPos;
            Vector2 rayDir;
            float rayLength;

            if (def.Shape == MeleeAttackShape.Swipe)
            {
                // 🔴 Use the WEAPON's current right as forward
                Vector2 forward = pivot.right.normalized;

                float baseAngle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
                float centerAngle = baseAngle + def.SwipeCenterOffset;
                float halfArc = def.SwipeArcDegree * 0.5f;

                float startAngle = centerAngle - halfArc;
                float endAngle   = centerAngle + halfArc;

                float angle = Mathf.Lerp(startAngle, endAngle, normalized);
                rayDir    = AngleToDirection(angle);
                rayLength = def.SwipeRadius;
            }
            else // Stab
            {
                // 🔴 Stab also follows weapon rotation
                Vector2 forward = pivot.right.normalized;

                rayDir    = forward;
                rayLength = Mathf.Lerp(0f, def.StabDistance, normalized);
            }

            // Debug draw so you see the arc/thrust in Scene/Game view
            Debug.DrawLine(rayOrigin,
                           rayOrigin + rayDir * rayLength,
                           Color.red,
                           0f,
                           false);
        

            // Ray-based hit detection along the swipe/stab line
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, rayDir, rayLength, _hitLayers);

            if (hit.collider != null && !alreadyHit.Contains(hit.collider))
            {
                alreadyHit.Add(hit.collider);
                ApplyDamageAndEffect(def, hit.collider, hit.point, hit.normal);

                if (def.StopOnHit)
                    break;
            }

            t += Time.deltaTime;
            yield return null;
        }

        _isAttacking = false;
        _attackRoutine = null;
    }

    private Vector2 AngleToDirection(float angleDeg)
    {
        float rad = angleDeg * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    private void ApplyDamageAndEffect(
    MeleeAttackShapeStats def,
    Collider2D target,
    Vector2 hitPoint,
    Vector2 hitNormal)
    {
        // 1) Damage
        IDamageable damageable = target.gameObject.GetComponent<IDamageable>();
        if (damageable != null)
        {
            float finalMinDamage = _owner.GetDamageWithModifier(def.MinDamage, def.DamageType);
            float finalMaxDamage = _owner.GetDamageWithModifier(def.MaxDamage, def.DamageType);
            float randomizeDamage = Random.Range(finalMinDamage, finalMaxDamage);

            damageable.TakeDamage(randomizeDamage, def.DamageType, def.GetRandomLimb());
        }

        // 2) VFX
        if (def.HitEffectPrefab != string.Empty)
        {
            // Orient the effect to "face" the camera, but rotated by the surface normal.
            // For 2D top-down, forward is usually -Z or +Z.
            Quaternion rot = Quaternion.identity;

            // If you want to align to normal (e.g. sparks on walls), you can do:
            // float angle = Mathf.Atan2(hitNormal.y, hitNormal.x) * Mathf.Rad2Deg;
            // rot = Quaternion.Euler(0f, 0f, angle);
            
            ObjectPooler.instance.SpawnFromPool(def.HitEffectPrefab, hitPoint, rot);
        }
    }

    #endregion

    // ======================================================================
}
