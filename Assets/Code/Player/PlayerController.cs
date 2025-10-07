using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour, IFactionMember
{
    public PlayerInput PlayerInput { get; private set; }
    public CharacterData CharacterData;

    public int maxMana = 100;
    private float currentMana;

    private Health health;

    public event Action<float, float> OnManaChanged; // current, max

    [Header("Facción")]
    [SerializeField] private Faction faction = Faction.Player;
    public Faction Faction => faction;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
        health = GetComponent<Health>();
    }

    private void Start()
    {
        currentMana = maxMana;

        // sincronizar HUD
        OnManaChanged?.Invoke(currentMana, maxMana);

        // ejemplo: conectar HUD a health
        health.OnHealthChanged += (cur, max) =>
        {
            // tu HUD lo puede leer
        };

        StartCoroutine(RegenerateMana());
    }

    private System.Collections.IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            currentMana = Mathf.Clamp(currentMana + 1, 0, maxMana);
            OnManaChanged?.Invoke(currentMana, maxMana);
        }
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana, maxMana);
    }

    public bool HasMana(float amount) => currentMana >= amount;
}
