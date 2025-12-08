using UnityEngine;

public static class Constants
{
    //// Time Constants
    public const float HOURS_PER_DAY = 24f;
    public const float MINUTES_PER_HOUR = 60f;
    public const float SECONDS_PER_MINUTE = 60f;

    public const float FRACTION = 0.1f;
    public const float SECONDS = 1f;
    public const float MINUTES = 60f;
    public const float HOURS = 3600f;

    //// Attributes Constants
    // Strength
    public const float WEIGHT_PER_STRENGTH = 3f;

    // Dexterity
    public const float MOVESPEED_PER_DEXTERITY = 0.1f;
    public const float RUNMULTIPLIER_PER_DEXTERITY = 0.05f;

    // Constitution
    public const float HEAD_HEALTH_PER_CONSTITUTION = 1f;
    public const float TORSO_HEALTH_PER_CONSTITUTION = 5f;
    public const float ARMS_HEALTH_PER_CONSTITUTION = 3f;
    public const float LEGS_HEALTH_PER_CONSTITUTION = 3f;
    public const float STAMINA_PER_CONSTITUTION = 2f;
    public const float FATIGUE_PER_CONSTITUTION = 3f;

    //// Stamina Constants
    public const float RUN_STAMINA_THRESHOLD = 2f;
    public const float MAX_BREATH_TAKING_TIME = 10f;
    public const float STAMINA_COOLDOWN = 1.5f;
    public const float DRAW_WALK_PENALTY = 3F;

    //// Equipments and Items Constants
    public const float POOR_QUALITY_DAMAGE_MODIFIER = 0.5f;
    public const float CRUDE_QUALITY_DAMAGE_MODIFIER = 0.25f;
    public const float FINE_QUALITY_DAMAGE_MODIFIER = 0f;
    public const float SUPERIOR_QUALITY_DAMAGE_MODIFIER = 1.2f;
    public const float EXCEPCIONAL_QUALITY_DAMAGE_MODIFIER = 1.4f;
    public const float MASTERFULLY_QUALITY_DAMAGE_MODIFIER = 2f;

}
