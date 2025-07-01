using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemySO", menuName = "EnemySO", order = 1)]
public class EnemySO : ScriptableObject, IStatProvider
{
    public int EnemyIDX;         // Enemy IDX
    
    public List<StatData> EnemyStats;
    public List<StatData> Stats => EnemyStats;
}
