using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "EnemyData", menuName = "ScriptableObjects/EnemySO")]
public class EnemySO : ScriptableObject
{
    public int EnemyIDX;
    public int Health;
    public float Speed;
    public float MoveDelay;
    public int AttackDamage;
    public float AttackRange;

}
