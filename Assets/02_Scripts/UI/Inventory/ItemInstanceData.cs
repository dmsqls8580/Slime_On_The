using UnityEngine;

public class ItemInstanceData
{
    public TempItemSO ItemSO;
    public int Quantity;

    public ItemInstanceData(TempItemSO itemSO, int quantity)
    {
        ItemSO = itemSO;
        Quantity = quantity;
    }

    public bool IsValid => ItemSO != null && Quantity > 0;
    public int Idx => ItemSO != null ? ItemSO.Idx : 0;
    public string Name => ItemSO != null ? ItemSO.ItemName : "";
    public Sprite Icon => ItemSO != null ? ItemSO.ItemIcon : null;
}