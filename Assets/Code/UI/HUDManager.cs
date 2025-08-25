using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance { get; private set; }

    public GameObject hudPrefab;
    public Transform hudParent;

    private Dictionary<PlayerInput, PlayerHUD> playerHUDs = new Dictionary<PlayerInput, PlayerHUD>();
    private Dictionary<PlayerInput, PlayerSpellUI> playerSpellUIs = new Dictionary<PlayerInput, PlayerSpellUI>();

    private void Awake()
    {
        Instance = this;
    }

    public PlayerHUD RegisterPlayer(PlayerInput player, Sprite portrait, string playerName)
    {
        GameObject hudObj = Instantiate(hudPrefab, hudParent);
        PlayerHUD hud = hudObj.GetComponent<PlayerHUD>();
        PlayerSpellUI spellUI = hudObj.GetComponent<PlayerSpellUI>();

        hud.SetPortrait(portrait);

        playerHUDs[player] = hud;
        playerSpellUIs[player] = spellUI;

        return hud; // Devolvemos el HUD
    }

    public PlayerHUD GetHUD(PlayerInput player)
    {
        return playerHUDs[player];
    }

    public PlayerSpellUI GetSpellUI(PlayerInput player)
    {
        return playerHUDs[player].spellUI;
    }
}
