using System.Collections.Generic;
using UnityEngine;

public class InventoryTestInitializer : MonoBehaviour
{
    [SerializeField] private List<TempItemSO> testItems;

    private void Awake()
    {
        if (testItems == null || testItems.Count == 0)
        {
            Debug.LogWarning("테스트 아이템이 비어있습니다.");
            return;
        }

        var testData = FakeInventoryData.Generate(20, testItems);
        InventoryManager.Instance.SetAll(testData.ToArray());
    }
}