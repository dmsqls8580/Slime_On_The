using UnityEngine;

[CreateAssetMenu(fileName = "EnemyTable", menuName = "Tables/EnemyTable", order = 1)]
public class EnemyTable : BaseTable<int, EnemySO>
{
    protected override string[] DataPath => new[] { "Assets/10_Tables/ScriptableObj/Enemy" };
    public override void CreateTable()
    {
        Type = GetType();
        DataDic.Clear();
        foreach (EnemySO SO in dataList)
        {
            DataDic[SO.EnemyID] = SO;
        }
    }
}