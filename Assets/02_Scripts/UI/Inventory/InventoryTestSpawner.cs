using UnityEngine;

public class InventoryTestSpawner : MonoBehaviour
{
    [Header("테스트용 아이템")]
    public ItemSO[] testItems;
    public int[] quantities;


    private void SpawnTestItems()
    {
        if (testItems == null || testItems.Length == 0) return;

        for (int i = 0; i < testItems.Length; i++)
        {
            var itemData = testItems[i];
            int quantity = (i < quantities.Length) ? quantities[i] : 1;

            if (itemData != null)
            {
                var newItem = new ItemInstanceData(itemData, quantity);
                bool added = InventoryManager.Instance.TryAddItemGlobally(itemData, quantity);
            }
        }
    }
}