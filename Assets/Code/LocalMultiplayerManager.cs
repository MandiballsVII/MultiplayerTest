using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LocalMultiplayerManager : MonoBehaviour
{
    public GameObject playerPrefab;

    void Start()
    {
        // Player 1 teclado/ratón
        //GameObject p1 = Instantiate(playerPrefab, pos1, Quaternion.identity);
        //p1.GetComponent<PlayerInput>().SwitchCurrentControlScheme("KeyboardMouse");

        // Player 2 mando
        //GameObject p2 = Instantiate(playerPrefab, pos2, Quaternion.identity);
        //p2.GetComponent<PlayerInput>().SwitchCurrentControlScheme("Gamepad");
    }
}
