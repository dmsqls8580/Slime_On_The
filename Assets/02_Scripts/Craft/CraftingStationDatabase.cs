using System.Collections.Generic;
using UnityEngine;

public class CraftingStationDatabase : MonoBehaviour
{
    private Dictionary<int, CraftingStation> database = new Dictionary<int, CraftingStation>();

    // 기본 제작 가능한 아이템 idx.
    private int[] normal = new int[]
    {
        2, 
        301, 304,
        401, 402, 403, 
        411, 412, 413,
        501, 502, 503, 
        601, 631, 
        701, 702,
        802
    };
    // 제작대로 제작 가능한 아이템 idx.
    private int[] workbench = new int[]
    {
        201, 202,
        302, 303,
        421, 422, 423,
        431, 432, 433,
        451, 453, 454,
        511, 512, 513,
        521, 522, 523,
        602,
    };
    // 모루로 제작 가능한 아이템 idx.
    private int[] anvil = new int[]
    {
        203, 
        441, 442, 443,
        452, 
        531, 532, 533,
        603
    };

    private void Awake()
    {
        foreach (int _idx in normal)
            database.Add(_idx, CraftingStation.Normal);
        foreach (int _idx in workbench)
            database.Add(_idx, CraftingStation.Workbench);
        foreach (int _idx in anvil)
            database.Add(_idx, CraftingStation.Anvil);
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
