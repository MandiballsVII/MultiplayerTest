using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputManager))]
public class PlayerJoinHandler : MonoBehaviour
{
    [SerializeField] private CharacterSelectionManager selectionManager;

    private void Reset()
    {
        selectionManager = FindObjectOfType<CharacterSelectionManager>();
    }

    // Asigna este método en el PlayerInputManager -> OnPlayerJoined (Invoke Unity Events)
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        if (selectionManager == null)
        {
            Debug.LogError("[PlayerJoinHandler] Falta referencia a CharacterSelectionManager.");
            return;
        }

        // 1) Parent dentro del UI
        if (selectionManager.UIParent == null)
        {
            Debug.LogError("[PlayerJoinHandler] UIParent no asignado en CharacterSelectionManager.");
            return;
        }

        // MUY IMPORTANTE: usar SetParent(..., false) para respetar el layout del Canvas
        playerInput.transform.SetParent(selectionManager.UIParent, worldPositionStays: false);
        playerInput.transform.localScale = Vector3.one;

        // 2) Obtener el CharacterSelector (puede estar en el root o en un hijo del prefab)
        var selector = playerInput.GetComponent<CharacterSelector>();
        if (selector == null) selector = playerInput.GetComponentInChildren<CharacterSelector>(true);

        if (selector == null)
        {
            Debug.LogError("[PlayerJoinHandler] El prefab no tiene CharacterSelector en el root ni en hijos.");
            return;
        }

        // 3) Pasar el array de personajes al selector
        selector.characters = selectionManager.allCharacters;

        // 4) Elegir un personaje por defecto libre e inicializar UI
        var defaultChar = selectionManager.GetFirstAvailable();
        selector.Initialize($"Jugador {playerInput.playerIndex + 1}", defaultChar);

        // 5) (Opcional) Forzar un UpdateUI con logs de seguridad si faltan refs
        if (selector.characters == null || selector.characters.Length == 0)
            Debug.LogError("[PlayerJoinHandler] selector.characters está vacío tras Initialize.");
    }

    // Asigna este método en el PlayerInputManager -> OnPlayerLeft si usas 'Join/Leave'
    public void OnPlayerLeft(PlayerInput playerInput)
    {
        var selector = playerInput.GetComponent<CharacterSelector>();
        if (selector == null) selector = playerInput.GetComponentInChildren<CharacterSelector>(true);

        // Si el jugador había bloqueado un personaje, podrías liberarlo aquí (necesitas exponer el actual en el selector)
        // selectionManager.UnlockCharacter(selector.CurrentCharacter);

        Destroy(playerInput.gameObject);
    }
}
