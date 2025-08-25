using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerInput PlayerInput { get; private set; }
    public CharacterData CharacterData;

    public int maxHealth = 100;
    public int maxMana = 100;

    private float currentHealth;
    private float currentMana;

    public event Action<float> OnHealthChanged;
    public event Action<float> OnManaChanged;

    private void Awake()
    {
        PlayerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        currentHealth = maxHealth;
        currentMana = maxMana;

        // Inicializa HUD con valores absolutos
        OnHealthChanged?.Invoke(currentHealth);
        OnManaChanged?.Invoke(currentMana);

        StartCoroutine(RegenerateMana());
    }

    private IEnumerator RegenerateMana()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f); // 1 por segundo
            currentMana = Mathf.Clamp(currentMana + 1, 0, maxMana);
            OnManaChanged?.Invoke(currentMana);
        }
    }
    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana);
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana); // valor absoluto
    }
    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth); // valor absoluto
    }

    public bool HasMana(float amount) => currentMana >= amount;
    public void Heal(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth); // valor absoluto
    }
}
