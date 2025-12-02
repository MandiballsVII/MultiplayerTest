using UnityEngine;

public enum StatusType
{
    None,
    Slow,
    Stun,
    Silence,
    Poison,
    Invulnerability
}

[System.Serializable]
public class StatusEffectDefinition
{
    public StatusType type;

    [Tooltip("Slow: 0.5 reduce la velocidad al 50%. Ignorado en efectos que no usan magnitud.")]
    public float magnitude = 1f;

    [Tooltip("Duración del efecto en segundos.")]
    public float duration = 1f;
}
