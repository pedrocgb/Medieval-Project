using CharactersStats;
using UnityEngine;

[System.Serializable]
public class ItemStatModifierData
{
    public StatId Stat;                    // which stat to modify
    public StatModType ModType;            // Flat / PercentAdd / PercentMulti
    public float Value;                    // amount
    public int Order = 0;                  // optional explicit order (otherwise 0)

    public StatModifier CreateRuntimeModifier(object source)
    {
        // Use the overload that lets us pass a Source (the item instance). :contentReference[oaicite:2]{index=2}
        return new StatModifier(Value, ModType, Order, source);
    }
    
}
