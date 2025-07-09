using System.Collections.Generic;
using UnityEngine;



[CreateAssetMenu(fileName = "EnemySO", menuName = "ScriptableObjects/BossSO", order = 2)]
public class BossSO : ScriptableObject, IStatProvider
{
    public int BossID;
    public List<ProjectileName> ProjectileID;
    public float IdleDuration;
    public float Cast1Duration;
    public float Cast2Duration;
    public float StompDuration;
    
    public List<StatData> BossStats;
    public List<StatData> Stats => BossStats;
}
