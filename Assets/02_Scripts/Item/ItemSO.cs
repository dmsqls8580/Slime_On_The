using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum ItemType
{
    None = 0,
    Material = 1 << 0,
    Tool = 1 << 1,
    Equipable = 1 << 2,
    Eatable = 1 << 3,
    Placeable = 1 << 4,
    BuffItem = 1 << 5
}

public enum ToolType 
{ 
    None, 
    Axe, 
    Pickaxe,
    Shovel,
    FishingRod,
    Hoe, 
    WateringCan 
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

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class ItemSO : ScriptableObject
{
    [Header("기본 정보")]
    public string idx;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("공통")]
    public ItemType itemTypes; // 다중 타입 가능
    public bool stackable;
    public int maxStack;

    [Header("타입별 데이터")]
    public MaterialData materialData;
    public ToolData toolData;
    public EquipableData equipableData;
    public EatableData eatableData;
    public PlaceableData placeableData;

    [Header("레시피")]
    public List<RecipeIngredient> recipe;
}

[System.Serializable]
public class MaterialData
{
    public bool rottenable;
}

[System.Serializable]
public class ToolData
{
    public ToolType toolType;
    public float power;
    public float luck;
    public float durability;
    public float actSpd;
}

[System.Serializable]
public class EquipableData
{
    public EquipType equipableType;
    public float maxHealth;
    public float atk;
    public float def;
    public float spd;
    public float crt;
}

[System.Serializable]
public class EatableData
{
    public float recoverHP;
    public float fullness;
    public float slimeGauge;
    public float duration;
    public bool rottenable;
    public bool permanent;
}

[System.Serializable]
public class PlaceableData
{
    public PlaceableInfo placeableInfo;
}

[System.Serializable]
public class RecipeIngredient
{
    public ItemSO item;
    public int amount;
}