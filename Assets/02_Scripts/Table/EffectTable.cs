using System;
using UnityEngine;

public enum EffectType
{
    DashEffect = 0, //플레이어는 0~20, Enemy는 21~40 
}

[System.Serializable]
public class EffectData
{
    public EffectType effectType;
    public GameObject effectPrefab;
    public float duration;
}

public class EffectTable : MonoBehaviour, ITable
{
    public Type Type { get; }
    public void AutoAssignDatas()
    {
        throw new NotImplementedException();
    }

    public void CreateTable()
    {
        throw new NotImplementedException();
    }
}
