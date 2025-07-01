using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerSO", menuName = "Sparta/PlayerSO", order= 0)]
public class PlayerSO : ScriptableObject, IStatProvider
{
    public int ID;
    public List<StatData> PlayerStat;
    public List<StatData> Stats => PlayerStat;
}
