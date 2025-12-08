using UnityEngine;

public static class WeaponDamageByQuality
{
    public static float Get(float damage, ItemQuality quality)
    {
        switch (quality)
        {
            default:
            case ItemQuality.Poor:
                return damage * Constants.POOR_QUALITY_DAMAGE_MODIFIER;

            case ItemQuality.Crude:
                return damage * Constants.CRUDE_QUALITY_DAMAGE_MODIFIER;

            case ItemQuality.Fine:
                return damage * Constants.FINE_QUALITY_DAMAGE_MODIFIER;

            case ItemQuality.Superior:
                return damage * Constants.SUPERIOR_QUALITY_DAMAGE_MODIFIER;

            case ItemQuality.Excepcional:
                return damage * Constants.EXCEPCIONAL_QUALITY_DAMAGE_MODIFIER;

            case ItemQuality.MasterfullyCrafted:
                return damage * Constants.MASTERFULLY_QUALITY_DAMAGE_MODIFIER;
        }
    }
}
