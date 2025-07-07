using UnityEngine;

public class ItemInstanceData
{
    public TempItemSO ItemData  { get;}
    public int Quantity { get; set; }

    public bool IsValid => ItemData != null && Quantity > 0;
    
    public ItemInstanceData(TempItemSO itemData, int quantity)
    {
        this.ItemData = itemData;
        this.Quantity = Mathf.Clamp(quantity, 1, itemData.maxStack);
    }
    
    // TODO: JSON / CSV 등의 데이터베이스 도입시 아래와 같은 형태로 사용
    /*
    public int idx { get; }
     
    public ItemInstanceData(int idx, int quantity)
    {
        // 예: JSON 기반 데이터베이스에서 ID로 SO를 찾는 구조
        this.ItemData = MyItemDatabase.Instance.GetItemById(idx);
        this.Quantity = Mathf.Clamp(quantity, 1, ItemData.maxStack);
    }
    */
}