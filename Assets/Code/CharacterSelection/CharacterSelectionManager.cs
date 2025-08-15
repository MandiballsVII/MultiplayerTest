using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance;

    [Header("UI")]
    public Transform UIParent;
    public GameObject playerPanelPrefab;

    [Header("Characters")]
    public CharacterData[] allCharacters;

    // Quién tiene bloqueado cada personaje
    private readonly Dictionary<CharacterData, CharacterSelector> takenBy = new Dictionary<CharacterData, CharacterSelector>();
    // Paneles activos en la escena
    private readonly List<CharacterSelector> activeSelectors = new List<CharacterSelector>();

    [Header("Start Game")]
    public float countdownTime = 3f;
    public string gameSceneName = "NombreDeLaEscenaDelJuego";

    private bool gameStarting = false;
    private Coroutine countdownCo;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ===== API de consulta =====
    public bool IsCharacterTaken(CharacterData c) => takenBy.ContainsKey(c);

    public bool IsCharacterTakenByOther(CharacterData c, CharacterSelector self)
    {
        return takenBy.TryGetValue(c, out var owner) && owner != self;
    }

    public CharacterData GetFirstAvailable()
    {
        if (allCharacters == null || allCharacters.Length == 0)
        {
            Debug.LogError("[CharacterSelectionManager] allCharacters está vacío.");
            return null;
        }
        foreach (var c in allCharacters)
            if (!IsCharacterTaken(c)) return c;
        return allCharacters[0];
    }

    public void RegisterSelector(CharacterSelector selector)
    {
        if (!activeSelectors.Contains(selector))
            activeSelectors.Add(selector);
    }

    public void UnregisterSelector(CharacterSelector selector)
    {
        activeSelectors.Remove(selector);
    }

    // ===== Bloqueo / Desbloqueo =====
    public void LockCharacter(CharacterData character, CharacterSelector locker)
    {
        // Si otro ya lo tenía, no lo sobreescribas
        if (!takenBy.ContainsKey(character))
            takenBy.Add(character, locker);

        // Primero marca su UI como bloqueada (no debe auto-avanzar)
        locker.RefreshLockedUI();

        // Luego avisa al resto
        UpdateAllSelectorsUI(skip: locker);

        CheckAllPlayersLocked();
    }

    public void UnlockCharacter(CharacterData character, CharacterSelector locker)
    {
        if (takenBy.TryGetValue(character, out var owner) && owner == locker)
        {
            takenBy.Remove(character);
            UpdateAllSelectorsUI();
            CancelCountdownIfRunning();
        }
    }

    private void UpdateAllSelectorsUI(CharacterSelector skip = null)
    {
        foreach (var selector in activeSelectors)
        {
            if (selector == null) continue;
            if (selector == skip)
            {
                // Ya refrescó su UI al bloquear
                continue;
            }
            selector.OnCharacterAvailabilityChanged();
        }
    }

    // ===== Inicio de partida =====
    private void CheckAllPlayersLocked()
    {
        if (gameStarting) return;
        if (activeSelectors.Count == 0) return;

        foreach (var sel in activeSelectors)
            if (!sel.IsLocked()) return;

        // Todos los que están en escena están listos
        countdownCo = StartCoroutine(StartGameCountdown());
    }

    private IEnumerator StartGameCountdown()
    {
        gameStarting = true;

        float timer = countdownTime;
        while (timer > 0f)
        {
            Debug.Log($"Comienza en {Mathf.Ceil(timer)}...");
            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        SceneManager.LoadScene(gameSceneName);
    }

    public void CancelCountdownIfRunning()
    {
        if (countdownCo != null)
        {
            StopCoroutine(countdownCo);
            countdownCo = null;
            gameStarting = false;
            Debug.Log("Cuenta atrás cancelada.");
        }
    }
}
