using UnityEngine;

public enum StatusType
{
    None,
    Slow,
    Stun,
    Silence,
    Poison,
    Accelerate,
    DamageReduction,
    Invulnerability
}

[System.Serializable]
public class StatusEffectDefinition
{
    public StatusType type;

    [Tooltip("Slow: 0.5 reduce la velocidad al 50%. Accelerate: 1.5 aumenta la velocidad al 150%. Ignorado en efectos que no usan magnitud.")]
    public float magnitude = 1f;

    [Tooltip("Duración del efecto en segundos.")]
    public float duration = 1f;
}
