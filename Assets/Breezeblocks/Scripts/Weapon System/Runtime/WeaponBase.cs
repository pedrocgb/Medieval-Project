using ObjectPool;
using Rewired.Interfaces;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour, IPooledObjects
{
    protected ActorBase _owner;
    protected WeaponConfig _config;
    protected float _attackSpeedTimeStamp;

    private Vector3 _startPos;
    private Quaternion _startRot;

    private void Start()
    {
        _startPos = transform.localPosition;
        _startRot = transform.localRotation;
    }

    public void OnObjectSpawn()
    {
        transform.localPosition = _startPos;
        transform.localRotation = _startRot;
    }

    // ======================================================================

    public virtual void OnEquipped(ActorBase owner, ItemInstance item, WeaponConfig config)
    {
        _owner = owner;
        _config = config;
    }

    public virtual void OnUnequipped() { }

    // ======================================================================

    public abstract void PerformPrimaryAttack();
    public abstract void PerformSecondaryAttack();

    // ======================================================================

    public abstract void HoldPrimaryAttack();
    public abstract void HoldSecondaryAttack();
    public abstract void ReleasePrimaryAttack();
    public abstract void ReleaseSecondaryAttack();


    // ======================================================================
}
