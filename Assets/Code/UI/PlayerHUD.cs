using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHUD : MonoBehaviour
{
    public Image portraitImage;
    public Slider healthSlider;
    public Slider manaSlider;
    public PlayerSpellUI spellUI;

    public void SetPortrait(Sprite sprite)
    {
        portraitImage.sprite = sprite;
    }

    public void UpdateHealth(float value)
    {
        healthSlider.value = value;
    }

    public void UpdateMana(float value)
    {
        manaSlider.value = value;
    }
    public void Configure(float maxHealth, float maxMana)
    {
        healthSlider.minValue = 0;
        healthSlider.maxValue = maxHealth;
        manaSlider.minValue = 0;
        manaSlider.maxValue = maxMana;
    }
}
