using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Player
{
    public string name;
    public int score;
    public float time;

    
    public Player() { }

    public Player(string name, int score, float time)
    {
        this.name = name;
        this.score = score;
        this.time = time;
    }
}
