using System;
using UnityEngine;

[CreateAssetMenu(fileName = "EffectTable", menuName = "Tables/EffectTable", order = 2)]
public class EffectTableSO : BaseTable<int, EffectDataSO>
{
    protected override string[] DataPath { get; }
    public override void CreateTable()
    {
        DataDic.Clear();
        foreach (var data in dataList)
        {
            DataDic[data.effectID] = data;
        }
    }
    
}
