using UnityEngine;
using UnityEngine.U2D.IK;

public interface IDamageable
{
    public void TakeDamage(float damage, DamageType damageType, Limbs damageLimb);
    public void Die();
}

public enum DamageCategory
{
    Physical,
    Nature,
    Magical
}

public enum DamageType
{
    Slash,
    Piercing,
    Bludgeoning,

    Fire,
    Cold,
    Lightning,
    Poison,

    Necrotic,
    Radiance,
    Psychic
}

public enum Limbs
{
    Head,
    Torso,
    LeftArm,
    RightArm,
    LeftLeg,
    RightLeg
}