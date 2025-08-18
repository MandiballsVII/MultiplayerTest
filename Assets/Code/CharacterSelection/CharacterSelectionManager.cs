using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager Instance { get; private set; }

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
    public TMP_Text countdownText;

    private bool gameStarting = false;
    private Coroutine countdownCo;

    private readonly Dictionary<int, PlayerSelection> selectedPlayers = new Dictionary<int, PlayerSelection>();


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
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
        // Si entra un jugador nuevo en el lobby, cancelar la cuenta atrás
        CancelCountdownIfRunning();
    }

    public void UnregisterSelector(CharacterSelector selector)
    {
        activeSelectors.Remove(selector);
    }

    // ===== Bloqueo / Desbloqueo =====
    public void LockCharacter(CharacterData character, CharacterSelector locker, int playerIndex)
    {
        if (!takenBy.ContainsKey(character))
            takenBy.Add(character, locker);

        locker.RefreshLockedUI();
        UpdateAllSelectorsUI(skip: locker);

        // Guardamos la selección del jugador (con su dispositivo)
        var device = locker.GetComponent<PlayerInput>().devices[0];
        selectedPlayers[playerIndex] = new PlayerSelection(character, device);

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
            if (countdownText != null)
                countdownText.text = $"Game starts in {Mathf.Ceil(timer)}...";

            yield return new WaitForSeconds(1f);
            timer -= 1f;
        }

        if (countdownText != null)
            countdownText.text = "";

        SceneManager.LoadScene(gameSceneName);
    }

    public void CancelCountdownIfRunning()
    {
        if (countdownCo != null)
        {
            StopCoroutine(countdownCo);
            countdownCo = null;
            gameStarting = false;

            if (countdownText != null)
            {
                countdownText.text = "Countdown canceled";
                StartCoroutine(ClearCountdownTextAfterDelay(3f));
            }

            Debug.Log("Cuenta atrás cancelada.");
        }
    }
    private IEnumerator ClearCountdownTextAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (countdownText != null)
            countdownText.text = "";
    }

    public Dictionary<int, PlayerSelection> GetSelections()
    {
        return selectedPlayers;
    }
}
public class PlayerSelection
{
    public CharacterData character;
    public InputDevice device;

    public PlayerSelection(CharacterData character, InputDevice device)
    {
        this.character = character;
        this.device = device;
    }
}
