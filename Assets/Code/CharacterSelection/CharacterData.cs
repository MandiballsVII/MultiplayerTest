using UnityEngine;

[CreateAssetMenu(menuName = "Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public Sprite portrait;
    public Color uiColor;
}
