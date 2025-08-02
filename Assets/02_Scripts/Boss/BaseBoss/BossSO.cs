using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/BossSO", order = 2)]
public class BossSO : ScriptableObject, IStatProvider
{
    public int BossID;
    public string BossName;
    public AttackType AttackType;
    public List<ProjectileName> ProjectileID;
    
    public List<StatData> BossStats;
    public List<StatData> Stats => BossStats;
    public List<DropItemData> DropItems;
    public Terrain SpawnTerrain;
}
