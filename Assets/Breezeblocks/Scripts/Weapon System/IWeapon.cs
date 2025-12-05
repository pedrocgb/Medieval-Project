using CharactersStats;

public interface IWeapon
{
    void OnEquipped(ActorBase owner, ItemInstance item, WeaponConfig config);
    void OnUnequipped();

    // The combat controller will call these
    void PerformPrimaryAttack();
    void PerformSecondaryAttack();

    void HoldPrimaryAttack();
    void HoldSecondaryAttack();
    void ReleasePrimaryAttack();
    void ReleaseSecondaryAttack();
}
