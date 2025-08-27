using UnityEngine;

public class PlayerStatus : MonoBehaviour
{
    private PlayerManager player; // Tu script de jugador
    private PlayerController playerController;
    private PlayerSpellBook playerSpellBook;
    private bool isImmobilized;

    private float originalSpeed;

    void Start()
    {
        player = GetComponent<PlayerManager>();
        playerController = GetComponent<PlayerController>();
        playerSpellBook = GetComponent<PlayerSpellBook>();
        originalSpeed = player.movementSpeed;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        StopCoroutine("SlowEffect");
        StartCoroutine(SlowEffect(multiplier, duration));
    }

    public void ApplyImmobilize(float duration)
    {
        StopCoroutine("ImmobilizeEffect");
        StartCoroutine(ImmobilizeEffect(duration));
    }

    public void ApplyDamage(int amount)
    {
        playerController.TakeDamage(amount);
    }

    public void ApplySilence(float duration)
    {
        StopCoroutine("SilenceEffect");
        StartCoroutine(SilenceEffect(duration));
    }

    private System.Collections.IEnumerator SlowEffect(float multiplier, float duration)
    {
        player.movementSpeed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        player.movementSpeed = originalSpeed;
    }

    private System.Collections.IEnumerator ImmobilizeEffect(float duration)
    {
        isImmobilized = true;
        player.controlsEnabled = false;
        yield return new WaitForSeconds(duration);
        player.controlsEnabled = true;
        isImmobilized = false;
    }

    private System.Collections.IEnumerator SilenceEffect(float duration)
    {
        // Aquí bloqueas la lógica de lanzar hechizos en PlayerSpellBook si isSilenced es true
        playerSpellBook.silenced = true;
        yield return new WaitForSeconds(duration);
        playerSpellBook.silenced = false;
    }
    public bool IsImmobilized() => isImmobilized;
}
