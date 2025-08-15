using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CharacterSelector : MonoBehaviour
{
    [Header("UI")]
    public Image portraitImage;
    public TMP_Text nameText;

    [Header("Characters")]
    public CharacterData[] characters;
    private int currentIndex = 0;

    private CharacterData currentCharacter;
    private PlayerInput playerInput;

    private bool locked = false;
    private bool hasJoined = false; // Primer submit solo une al jugador

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
    }

    private void Start()
    {
        CharacterSelectionManager.Instance.RegisterSelector(this);
    }

    private void OnDestroy()
    {
        CharacterSelectionManager.Instance?.UnregisterSelector(this);
        // Si estabas bloqueado al destruir, liberar el personaje
        if (locked && currentCharacter != null)
            CharacterSelectionManager.Instance.UnlockCharacter(currentCharacter, this);
    }

    public bool IsLocked() => locked;

    public void Initialize(string playerName, CharacterData defaultCharacter = null)
    {
        if (defaultCharacter != null)
        {
            int idx = System.Array.IndexOf(characters, defaultCharacter);
            currentIndex = idx >= 0 ? idx : 0;
        }

        currentCharacter = characters[currentIndex];
        nameText.text = playerName;
        UpdateUI();
    }

    // ====== Inputs (Unity Events del PlayerInput del panel) ======
    public void OnNavigate(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed || locked) return;

        float value = ctx.ReadValue<Vector2>().x;
        if (value > 0) NextCharacter();
        else if (value < 0) PreviousCharacter();
    }

    public void OnSubmit(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Primer Submit: solo marcar que ya se unió (no bloquea)
        if (!hasJoined)
        {
            hasJoined = true;
            return;
        }

        if (locked) return;

        // ¿Está cogido por OTRO?
        if (CharacterSelectionManager.Instance.IsCharacterTakenByOther(currentCharacter, this))
        {
            // Opcional: feedback (sonido/flash) de no disponible
            return;
        }

        // Bloquear este personaje para este jugador (primero me marco locked)
        locked = true;
        CharacterSelectionManager.Instance.LockCharacter(currentCharacter, this);
        RefreshLockedUI();
    }

    public void OnCancel(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        if (locked)
        {
            // Liberar el personaje
            CharacterSelectionManager.Instance.UnlockCharacter(currentCharacter, this);
            locked = false;
            UpdateUI();
        }
        else
        {
            // Podrías implementar “salir del lobby” aquí si quieres
            SceneManager.LoadScene("MainMenu");
        }
    }

    // ====== Navegación ======
    public void NextCharacter()
    {
        // Avanza hasta encontrar uno libre o el tuyo
        int safety = characters.Length;
        do
        {
            currentIndex = (currentIndex + 1) % characters.Length;
            currentCharacter = characters[currentIndex];
            safety--;
        }
        while (safety > 0 && CharacterSelectionManager.Instance.IsCharacterTakenByOther(currentCharacter, this));

        UpdateUI();
    }

    public void PreviousCharacter()
    {
        int safety = characters.Length;
        do
        {
            currentIndex = (currentIndex - 1 + characters.Length) % characters.Length;
            currentCharacter = characters[currentIndex];
            safety--;
        }
        while (safety > 0 && CharacterSelectionManager.Instance.IsCharacterTakenByOther(currentCharacter, this));

        UpdateUI();
    }

    // ====== Reacciones a cambios globales ======
    public void OnCharacterAvailabilityChanged()
    {
        // Si estoy encima de uno que otro acaba de coger y yo NO estoy bloqueado, me muevo
        if (!locked && CharacterSelectionManager.Instance.IsCharacterTakenByOther(currentCharacter, this))
        {
            NextCharacter();
            return;
        }

        // Si no, solo refresco colores/estado
        UpdateUI();
    }

    public void RefreshLockedUI()
    {
        // Tu personaje bloqueado debe verse “activo”, no gris.
        SetPortraitGray(false);
        // Aquí puedes deshabilitar botones o mostrar un check, etc.
    }

    // ====== UI ======
    public void UpdateUI()
    {
        currentCharacter = characters[currentIndex];
        portraitImage.sprite = currentCharacter.portrait;
        nameText.text = currentCharacter.characterName;

        // Gris solo si está cogido por OTRO
        bool takenByOther = CharacterSelectionManager.Instance.IsCharacterTakenByOther(currentCharacter, this);
        SetPortraitGray(takenByOther);
    }

    private void SetPortraitGray(bool gray)
    {
        portraitImage.color = gray ? Color.gray : Color.white;
    }
}
