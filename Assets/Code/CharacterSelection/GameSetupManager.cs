using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameSetupManager : MonoBehaviour
{
    public Transform[] spawnPoints; // Asigna en el inspector

    void Start()
    {
        var selections = CharacterSelectionManager.Instance.GetSelections();
        int spawnIndex = 0;

        foreach (var kvp in selections)
        {
            int playerIndex = kvp.Key;
            var selection = kvp.Value;

            // Decimos qué prefab corresponde
            PlayerInputManager.instance.playerPrefab = selection.character.characterPrefab;

            // Creamos el jugador vinculándolo al dispositivo correcto
            var pi = PlayerInputManager.instance.JoinPlayer(-1, -1, null, selection.device);

            // Lo colocamos en su spawn
            pi.transform.position = spawnPoints[spawnIndex].position;

            spawnIndex++;
        }
    }
}
