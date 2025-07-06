using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "EnemySO", order = 1)]
public class EnemySO : ScriptableObject, IStatProvider
{
    public int EnemyID;                        // Enemy ID
    public EnemyAttackType AttackType;         // Enemy 공격 타입
    public string projectileID;                // Ranged 일때만 사용
    public Transform ProjectileSpawn;
    
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
}
