using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Spells/SpellDatabase")]
public class SpellDatabase : ScriptableObject
{
    public List<SpellData> allSpells;
}
