using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.HID;

public class GameSetupManager : MonoBehaviour
{
    public Transform[] spawnPoints; // Asigna en el inspector

    IEnumerator Start()
    {
        yield return null; // Esperar un frame para asegurar que todo está inicializado
        var selections = CharacterSelectionManager.Instance.GetSelections();
        int spawnIndex = 0;

        foreach (var kvp in selections)
        {
            int playerIndex = kvp.Key;
            var selection = kvp.Value;

            // Establecer el prefab correcto
            PlayerInputManager.instance.playerPrefab = selection.character.characterPrefab;

            // Crear el jugador y asignar el dispositivo correcto
            var pi = PlayerInputManager.instance.JoinPlayer(-1, -1, null, selection.device);

            // Obtener GameObject y PlayerController
            var playerObj = pi.gameObject;
            var playerController = playerObj.GetComponent<PlayerController>();

            // Crear HUD y configurarlo (SOLO UNA VEZ)
            PlayerHUD hud = HUDManager.Instance.RegisterPlayer(
                playerController.PlayerInput,
                playerController.CharacterData.portrait,
                playerController.CharacterData.characterName
            );

            hud.Configure(playerController.maxHealth, playerController.maxMana);
            hud.UpdateHealth(playerController.maxHealth);
            hud.UpdateMana(playerController.maxMana);

            // Vincular eventos para actualizar barras
            playerController.OnHealthChanged += (value) => hud.UpdateHealth(value);
            playerController.OnManaChanged += (value) => hud.UpdateMana(value);

            // Colocar en el spawn correspondiente
            pi.transform.position = spawnPoints[spawnIndex].position;

            spawnIndex++;
        }
    }

}
