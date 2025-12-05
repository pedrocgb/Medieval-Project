using UnityEngine;

public abstract class WeaponBase : MonoBehaviour
{
    protected ActorBase _owner;
    protected WeaponConfig _config;

    public virtual void OnEquipped(ActorBase owner, ItemInstance item, WeaponConfig config)
    {
        _owner = owner;
        _config = config;
    }

    public virtual void OnUnequipped() { }

    public abstract void PerformPrimaryAttack();
    public abstract void PerformSecondaryAttack();

    public abstract void HoldPrimaryAttack();
    public abstract void HoldSecondaryAttack();
    public abstract void ReleasePrimaryAttack();
    public abstract void ReleaseSecondaryAttack();
}
