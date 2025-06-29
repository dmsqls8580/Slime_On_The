using System.Collections.Generic;
using UnityEngine;

public class FakeInventoryData
{
    public static List<ItemInstanceData> Generate(int count, List<TempItemSO> itemPool)
    {
        List<ItemInstanceData> result = new();
        for (int i = 0; i < count; i++)
        {
            var itemSo = itemPool[Random.Range(0, itemPool.Count)];
            result.Add(new ItemInstanceData(itemSo, Random.Range(1, itemSo.MaxStack + 1)));
        }
        return result;
    }
}