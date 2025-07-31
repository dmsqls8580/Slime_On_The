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
    Cookable = 1 << 4,
    Cooked = 1 << 5,
    Placeable = 1 << 6,
    BuffItem = 1 << 7,
}

public enum ToolType
{ 
    None = 0,
    Axe = 1, 
    Pickaxe = 2,
    Shovel = 3,
    FishingRod = 4,
    Hoe = 5, 
    WateringCan = 6, 
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

public enum IngredientTag
{
    None = 0,
    Meat = 1,
    Fruit = 2,
    Vegetable = 3,
    Egg = 4,
    Monster = 5,
    Ice = 6,
}

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Item")]
public class ItemSO : ScriptableObject, ITool
{
    [Header("기본 정보")]
    public int idx;
    public string itemName;
    [TextArea] public string description;
    public Sprite icon;
    
    public float AttackPow => toolData.power;
    public float AttackSpd => toolData.actSpd;
    public ToolType ToolType => toolData.toolType;

    [Header("공통")]
    public ItemType itemTypes; // 다중 타입 가능
    public bool stackable;
    public int maxStack;

    [Header("타입별 데이터")]
    public MaterialData materialData;
    public ToolData toolData;
    public EquipableData equipableData;
    public EatableData eatableData;
    public CookableData cookableData;
    public CookedData cookedData;
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
    public int formId;
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
public struct TagValuePair
{
    public IngredientTag tag;
    public float value;
}

[System.Serializable]
public class CookableData
{
    public List<TagValuePair> tags;
    public float contributionTime;
}

[System.Serializable]
public class CookedData
{
    public int priority;
    public List<TagValuePair> requiredTags;
    public List<TagValuePair> forbiddenTags;
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