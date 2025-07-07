using UnityEngine;

[CreateAssetMenu(fileName = "BossTable", menuName = "Tables/BossTable", order = 2)]
public class BossTable : BaseTable<int, BossSO>
{
    protected override string[] DataPath => new[] { "Assets/10_Tables/ScriptableObj/Boss" };
    public override void CreateTable()
    {
        Type = GetType();
        DataDic.Clear();
        foreach (BossSO SO in dataList)
        {
            dataList[SO.BossID] = SO;
        }
    }
}
