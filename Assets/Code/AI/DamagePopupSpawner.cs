using UnityEngine;
using TMPro;

public class DamagePopupSpawner : MonoBehaviour
{
    public GameObject damagePopupPrefab;
    public Vector3 offset = new Vector3(0, 1f, 0);

    public void CreatePopup(float damageAmount, float healthPercent)
    {
        print("Creating damage popup: " + damageAmount);
        if (damagePopupPrefab == null) return;

        Vector3 pos = transform.position + offset;
        var popupGO = Instantiate(damagePopupPrefab, pos, Quaternion.identity);

        var popup = popupGO.GetComponent<DamagePopup>();
        if (popup != null)
        {
            popup.Setup(damageAmount, healthPercent);
        }
    }
}
