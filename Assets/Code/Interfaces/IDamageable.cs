using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public interface IDamageable
{
    public void TakeDamage();
    public void Heal();
}
