using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeBar : Slider
{
    public int maxHealth = 100;
    public int currentHealth = 100;

    protected override void Awake()
    {
        currentHealth = maxHealth;
        base.Awake();
        minValue = 0;
        wholeNumbers = true;
        UpdateHealthBar();
    }

    private void UpdateVisibility()
    {
        if (currentHealth <= 0)
        {
            fillRect.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        else
        {
            fillRect.GetComponent<CanvasRenderer>().SetAlpha(1);
        }
    }

    private void UpdateHealthBar()
    {
        if(direction == Direction.BottomToTop)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(60, maxHealth * 3);
        }
        else
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(maxHealth * 3, 60);
        }
        currentHealth = Math.Clamp(currentHealth, 0, maxHealth);
        maxValue = maxHealth;
        value = currentHealth;
        UpdateVisibility();
    }

    public void MaxHPUp(int amount)
    {
        maxHealth = Math.Clamp(maxHealth + amount, 10, 100);
        UpdateHealthBar();
    }
    public void MaxHPDown(int amount)
    {
        maxHealth = Math.Clamp(maxHealth - amount, 10, 100);
        UpdateHealthBar();
    }
    public void HPUp(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        UpdateHealthBar();
    }
    public void HPDown(int amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0, maxHealth);
        UpdateHealthBar();
    }
}
