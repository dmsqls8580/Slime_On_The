using System.Collections.Generic;
using UnityEngine;

public class CraftingStationDatabase : MonoBehaviour
{
    private Dictionary<int, CraftingStation> database = new Dictionary<int, CraftingStation>();

    // 기본 제작 가능한 아이템 idx.
    private int[] normal = new int[]
    {
        2,                  //판자
        301, 304,           //작업대, 나무상자
        401, 402, 403,      //나무도구
        411, 412, 413,      //돌도구
        501, 502, 503,      //나무방어구
        601, 631,           //돌반지, 파란목걸이
        701, 702,           //나무건축물
        802                 //고기
    };
    // 제작대로 제작 가능한 아이템 idx.
    private int[] workbench = new int[]
    {
        201, 202,           //구리, 철주괴
        302, 303,           //모루, 요리솥
        421, 422, 423,      //구리도구
        431, 432, 433,      //철도구
        451, 453, 454,      //철낚싯대, 금낚싯대, 망치
        511, 512, 513,      //구리방어구
        521, 522, 523,      //철방어구
        602,                //철반지
    };
    // 모루로 제작 가능한 아이템 idx.
    private int[] anvil = new int[]
    {
        203,                //금주괴
        441, 442, 443,      //금도구
        452,                //금낚싯대
        531, 532, 533,      //금방어구
        603                 //금반지
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
