using UnityEngine;

public abstract class WeaponHandler : MonoBehaviour
{

    protected IWeapon _currentWeapon;

    public void ChangeWeapon(IWeapon newWeapon)
    {
        _currentWeapon = newWeapon;
    }

    public abstract void OnPrimaryAttackInput();
    public abstract void OnSecondaryAttackInput();

    public abstract void OnPrimaryHoldInput();
    public abstract void OnPrimaryReleaseInput();
    public abstract void OnSecondaryHoldInput();
    public abstract void OnSecondaryReleaseInput();
}
