using System.Collections.Generic;
using UnityEngine;

public enum EnemyAttackType
{
    None,
    Neutral,
    Aggressive,
}

public enum ProjectileName
{
    //Enemy
    None = 20,
    Bolt,
    Spark,
    
    // Boss
    TurtleSpell0 = 40,
    TurtleSpell1,
    TurtleSpell2,
    TurtleSpell3,
    TurtleSpell4,
    TurtleSpell5,
    
}

public enum Terrain
{
    Grass,
    Forest,
    Rocky,
    Desert,
    Marsh
}

[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/EnemySO", order = 1)]
public class EnemySO : ScriptableObject, IStatProvider
{
    public int EnemyID;                        // Enemy ID
    public string EnemyName;
    public EnemyAttackType AttackType;         // Enemy 공격 타입
    public ProjectileName ProjectileID;  // Ranged 일때만 사용
    
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
    public List<DropItemData> DropItems;
    public Terrain SpawnTerrain;
}
