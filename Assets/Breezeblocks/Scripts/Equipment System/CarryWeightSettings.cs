using UnityEngine;

public static class CarryWeightSettings
{
    #region Variables and Properties
    [System.Serializable]
    public class LoadConfig
    {
        public float moveSpeedMultiplier = 1f;
        public float turnSpeedMultiplier = 1f;
        public float fatiguePenalty = 1f;
        public float staminaPenalty = 1f;
        public bool canRun = true;
    }
    public const float MaxOverburdenedSpeed = 0.75f;
    public static LoadConfig GetCurrentLoadConfig(CarryLoadStage Config)
    {
        switch (Config)
        {
            case CarryLoadStage.Medium:
                return mediumConfig;
            case CarryLoadStage.Heavy:
                return heavyConfig;
            case CarryLoadStage.Overburdened:
                return overburdenedConfig;
            case CarryLoadStage.Light:
            default:
                return lightConfig;
        }
    }
    #endregion

    // ======================================================================

    #region Load Configurations
    [Header("Load Configs")]
    public static LoadConfig lightConfig = new LoadConfig
    {
        moveSpeedMultiplier = 1f,
        turnSpeedMultiplier = 1f,
        fatiguePenalty = 1f,
        staminaPenalty = 1f,
        canRun = true
    };

    public static LoadConfig mediumConfig = new LoadConfig
    {
        moveSpeedMultiplier = 0.85f,
        turnSpeedMultiplier = 0.9f,
        fatiguePenalty = 1.5f,
        staminaPenalty = 2f,
        canRun = true
    };

    public static LoadConfig heavyConfig = new LoadConfig
    {
        moveSpeedMultiplier = 0.65f,
        turnSpeedMultiplier = 0.6f,
        fatiguePenalty = 2f,
        staminaPenalty = 3f,
        canRun = true
    };

    public static LoadConfig overburdenedConfig = new LoadConfig
    {
        moveSpeedMultiplier = 0.4f,
        turnSpeedMultiplier = 0.4f,
        fatiguePenalty = 3f,
        staminaPenalty = 4f,
        canRun = false
    };

    // ======================================================================
    #endregion
}

public enum CarryLoadStage
{
    Light = 0,
    Medium = 1,
    Heavy = 2,
    Overburdened = 3
}
