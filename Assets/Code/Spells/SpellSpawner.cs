using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpellSpawner : MonoBehaviour
{
    public SpellDatabase database;   // Asigna tu base de datos (con todos los SpellData)
    public GameObject pickupPrefab;  // Asigna el prefab genérico
    public Transform[] points;       // Puntos posibles en escena

    [Tooltip("Cuántos pickups crear como máximo (no respawnea). Si es 0 o menor, usa todos los puntos.")]
    public int maxPickups = 0;

    IEnumerator Start()
    {
        // 1) Qué escuelas están presentes en esta partida
        var selections = CharacterSelectionManager.Instance.GetSelections();
        var presentSchools = new HashSet<SpellSchool>(
            selections.Values.Select(v => v.character.school)
        );

        // 2) Filtrar pool por escuelas presentes
        var pool = database.allSpells
                           .Where(s => presentSchools.Contains(s.school))
                           .ToList();

        if (pool.Count == 0 || points == null || points.Length == 0)
            yield break;

        // 3) Elegir puntos a usar (barajados)
        var shuffledPoints = points.OrderBy(_ => Random.value).ToList();
        int countToSpawn = (maxPickups > 0)
            ? Mathf.Min(maxPickups, shuffledPoints.Count)
            : shuffledPoints.Count;

        // 4) (opcional) Evitar duplicados de hechizos hasta agotar pool
        //    Si hay menos spells que puntos, empezará a repetir.
        var shuffledSpells = pool.OrderBy(_ => Random.value).ToList();
        int spellIdx = 0;

        for (int i = 0; i < countToSpawn; i++)
        {
            var p = shuffledPoints[i];

            // Elige spell: sin repetición hasta agotar
            if (spellIdx >= shuffledSpells.Count) spellIdx = 0;
            var s = shuffledSpells[spellIdx++];

            // Instancia pickup genérico y le inyecta la data
            var go = Instantiate(pickupPrefab, p.position, Quaternion.identity);
            if (go.TryGetComponent<SpellPickup>(out var pick))
            {
                pick.SetData(s); // <-- aquí se asigna el SpellData concreto
            }

            // (opcional) yield para repartir la carga en varios frames
            yield return null;
        }
    }
}
