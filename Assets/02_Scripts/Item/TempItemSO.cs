using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Item_", menuName = "TempItemSO")]
public class TempItemSO : ScriptableObject
{
    [Header("기본 정보")] 
    public int idx;
    public string itemName;
    public Sprite icon;
    [Min(1)]
    public int maxStack = 1;

    [Tooltip("사용 가능 여부")]
    public bool isUsable;

    [Tooltip("아이템 사용 방식")]
    [ConditionalField(nameof(isUsable))]
    public UseType useType;
    
    [Tooltip("사용 시 효과량")]
    [ConditionalField(nameof(useType), UseType.Consume)]
    public int effectValue;
    
    [Tooltip("장비 장착 위치")]
    [ConditionalField(nameof(useType), UseType.Equip)]
    public EquipType equipType;
    
    [Header("제작 관련")]
    [Tooltip("제작 가능 여부")]
    public bool isCraftable;

    [Tooltip("제작에 필요한 재료 목록")]
    [ConditionalField(nameof(isCraftable))]
    public List<RecipeMaterial> craftMaterials = new();
}

[Serializable]
public class RecipeMaterial
{
    [Tooltip("필요한 재료 아이템")]
    public TempItemSO material;

    [Tooltip("필요한 수량")]
    [Min(1)]
    public int requiredQuantity = 1;
}

public enum UseType
{
    Consume,
    Equip,
    Place,
    Special
}

public enum EquipType
{
    Chest = 0,
    Leg = 1,
    Boots = 2,
    Amulet = 3,
    Ring = 4,
    Core = 5,
}