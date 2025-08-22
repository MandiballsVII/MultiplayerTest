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

    private void Awake()
    {
        Instance = this;
    }

    public PlayerHUD RegisterPlayer(PlayerInput player, Sprite portrait, string playerName)
    {
        GameObject hudObj = Instantiate(hudPrefab, hudParent);
        PlayerHUD hud = hudObj.GetComponent<PlayerHUD>();

        hud.SetPortrait(portrait);

        playerHUDs[player] = hud;

        return hud; // Devolvemos el HUD
    }

    public PlayerHUD GetHUD(PlayerInput player)
    {
        return playerHUDs[player];
    }
}
