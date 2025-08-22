using System;
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

        OnHealthChanged?.Invoke(currentHealth);
        OnManaChanged?.Invoke(currentMana);
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        OnHealthChanged?.Invoke(currentHealth / maxHealth);
    }

    public void UseMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        OnManaChanged?.Invoke(currentMana / maxMana);
    }
}
