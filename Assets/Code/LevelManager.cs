using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public void LoadLevel(string levelName)
    {
        // Load the specified level
        UnityEngine.SceneManagement.SceneManager.LoadScene(levelName);
    }
}
