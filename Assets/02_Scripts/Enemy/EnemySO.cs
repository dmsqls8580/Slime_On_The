using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType
{
    None,
    Melee,
    Ranged
}

public enum ProjectileName
{
    //Enemy
    None = 0,
    Bolt,
    
    // Boss
    TurtleSpell1 = 20, 
    TurtleSpell2,
    TurtleSpell3,
    TurtleSpell4,
    TurtleSpell5,
    TurtleSpell6,
    
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/EnemySO", order = 1)]
public class EnemySO : ScriptableObject, IStatProvider
{
    public int EnemyID;                        // Enemy ID
    public EnemyAttackType AttackType;         // Enemy 공격 타입
    public ProjectileName ProjectileID;                // Ranged 일때만 사용
    
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
}
