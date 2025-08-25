using UnityEngine;
using UnityEngine.UI;

public class PlayerSpellUI : MonoBehaviour
{
    public Image projectileSlot;
    public Image selfSlot;
    public Image summonSlot;
    public Image areaSlot;

    public Sprite emptyIcon; // icono por defecto

    public void UpdateSpellIcon(SpellType type, Sprite icon)
    {
        switch (type)
        {
            case SpellType.Projectile:
                projectileSlot.sprite = icon != null ? icon : emptyIcon;
                break;
            case SpellType.Self:
                selfSlot.sprite = icon != null ? icon : emptyIcon;
                break;
            case SpellType.Summon:
                summonSlot.sprite = icon != null ? icon : emptyIcon;
                break;
            case SpellType.Area:
                areaSlot.sprite = icon != null ? icon : emptyIcon;
                break;
        }
    }
}
