using UnityEngine;

public class PlayerWeaponHandler : WeaponHandler
{
    private EquipmentManager _equipment;
    private PlayerBase _player;

    private void Awake()
    {
        _equipment = GetComponent<EquipmentManager>();
        _player = GetComponent<PlayerBase>();
    }

    private void Update()
    {
        // Here you would check for player input and call the appropriate methods
        if (_player.ControlScheme.GetButtonDown("Primary Action"))
        {
            OnPrimaryAttackInput();
        }
        if (_player.ControlScheme.GetButtonDown("Secondary Action"))
        {
            OnSecondaryAttackInput();
        }



        if (_player.ControlScheme.GetButton("Primary Action"))
        {
            OnPrimaryHoldInput();
        }
        if (_player.ControlScheme.GetButton("Secondary Action"))
        {
            OnSecondaryHoldInput();
        }


        if (_player.ControlScheme.GetButtonUp("Primary Action"))
        {
            OnPrimaryReleaseInput();
        }
        if (_player.ControlScheme.GetButtonUp("Secondary Action"))
        {
            OnSecondaryReleaseInput();
        }
    }

    public override void OnPrimaryAttackInput()
    {
        _currentWeapon?.PerformPrimaryAttack();
    }

    public override void OnSecondaryAttackInput()
    {
        _currentWeapon?.PerformSecondaryAttack();
    }

    public override void OnPrimaryHoldInput()
    {
        _currentWeapon?.HoldPrimaryAttack();
    }
    public override void OnPrimaryReleaseInput()
    {
        _currentWeapon?.ReleasePrimaryAttack();
    }

    public override void OnSecondaryHoldInput()
    {
        _currentWeapon?.HoldSecondaryAttack();
    }
    public override void OnSecondaryReleaseInput()
    {
        _currentWeapon?.ReleaseSecondaryAttack();
    }
}
