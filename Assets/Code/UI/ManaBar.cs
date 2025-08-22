using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBar : Slider
{
    public int maxMana = 100;
    public int currentMana = 100;

    protected override void Awake()
    {
        currentMana = maxMana;
        base.Awake();
        minValue = 0;
        wholeNumbers = true;
        UpdateManaBar();
    }

    private void UpdateVisibility()
    {
        if (currentMana <= 0)
        {
            fillRect.GetComponent<CanvasRenderer>().SetAlpha(0);
        }
        else
        {
            fillRect.GetComponent<CanvasRenderer>().SetAlpha(1);
        }
    }

    private void UpdateManaBar()
    {
        if (direction == Direction.BottomToTop)
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(60, maxMana * 3);
        }
        else
        {
            gameObject.GetComponent<RectTransform>().sizeDelta = new Vector2(maxMana * 3, 60);
        }
        currentMana = Math.Clamp(currentMana, 0, maxMana);
        maxValue = maxMana;
        value = currentMana;
        UpdateVisibility();
    }

    public void MaxManaUp(int amount)
    {
        maxMana = Math.Clamp(maxMana + amount, 10, 100);
        UpdateManaBar();
    }
    public void MaxManaDown(int amount)
    {
        maxMana = Math.Clamp(maxMana - amount, 10, 100);
        UpdateManaBar();
    }
    public void ManaUp(int amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, maxMana);
        UpdateManaBar();
    }
    public void ManaDown(int amount)
    {
        currentMana = Mathf.Clamp(currentMana - amount, 0, maxMana);
        UpdateManaBar();
    }
}
