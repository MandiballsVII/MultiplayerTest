using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellSpawner : MonoBehaviour
{
    public SpellDatabase database;
    public GameObject pickupPrefab;     // prefab con SpellPickup
    public Transform[] points;          // posibles posiciones
    public float interval = 5f;

    private SpellData[] pool;

    IEnumerator Start()
    {
        // Filtrar por escuelas presentes
        var selections = CharacterSelectionManager.Instance.GetSelections(); // ya devuelves PlayerSelection
        var presentSchools = new HashSet<SpellSchool>(
            selections.Values.Select(v => v.character.school)
        );

        pool = database.allSpells
                       .Where(s => presentSchools.Contains(s.school))
                       .ToArray();

        while (true)
        {
            yield return new WaitForSeconds(interval);
            SpawnOne();
        }
    }

    void SpawnOne()
    {
        if (pool == null || pool.Length == 0 || points.Length == 0) return;

        var s = pool[Random.Range(0, pool.Length)];
        var p = points[Random.Range(0, points.Length)];

        var go = Instantiate(pickupPrefab, p.position, Quaternion.identity);
        var pick = go.GetComponent<SpellPickup>();
        pick.data = s;
    }
}
