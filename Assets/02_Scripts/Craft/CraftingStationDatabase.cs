using System.Collections.Generic;
using UnityEngine;

public class CraftingStationDatabase : MonoBehaviour
{
    private Dictionary<int, CraftingStation> database;

    // 기본 제작 가능한 아이템 idx.
    private int[] normal = new int[]
    {
        0, 2, 101, 102, 103, 201, 202, 203, 301, 302,
        303, 401, 402, 403, 404, 411, 412, 413, 414, 416,
        421, 422, 423, 424, 425, 426, 431, 432, 433, 434,
        435, 436, 501, 502, 601, 602, 701, 801, 802
    };
    // 제작대로 제작 가능한 아이템 idx.
    private int[] workbench = new int[]
    {

    };
    // 모루로 제작 가능한 아이템 idx.
    private int[] anvil = new int[]
    {

    };

    private void Awake()
    {
        foreach (int _idx in normal)
            database[_idx] = CraftingStation.Normal;
        foreach (int _idx in workbench)
            database[_idx] = CraftingStation.Workbench;
        foreach (int _idx in anvil)
            database[_idx] = CraftingStation.Anvil;
    }

    public CraftingStation GetCraftingStation(int _idx)
    {
        return database[_idx];
    }

    public void UpdateCraftingStation(int _idx)
    {
        database[_idx] = CraftingStation.Normal;
    }
}
