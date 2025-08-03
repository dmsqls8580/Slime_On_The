using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UICookPot : UIBase
{
    [SerializeField] private List<InventorySlot> inputSlots;
    [SerializeField] private InventorySlot resultSlot;
    [SerializeField] private AnimationCurve JellyAnimationCurve;

    private InventoryManager inventoryManager;
    private CookingManager cookingManager;

    private CookPotObject cookPotObject;
    private int cookIndex;

    private bool ignoreNextSlotChange = false;
    public void IgnoreNextSlotChange() => ignoreNextSlotChange = true;

    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
        cookingManager = CookingManager.Instance;
    }

    public void Initialize(CookPotObject _cookPotObject)
    {
        cookPotObject = _cookPotObject;
        cookIndex = cookPotObject.CookIndex;

        int inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        for (int i = 0; i < inputSlots.Count; i++)
        {
            inputSlots[i].Initialize(inputStart + i);
        }

        int resultIndex = SlotIndexScheme.GetCookResultIndex(cookIndex);
        resultSlot.Initialize(resultIndex);

        inventoryManager.OnSlotChanged += OnAnySlotChanged;
        cookPotObject.Initialize(inputSlots, resultSlot);
    }

    private void OnDisable()
    {
        if (InventoryManager.HasInstance)
            inventoryManager.OnSlotChanged -= OnAnySlotChanged;
    }
    
    private void OnAnySlotChanged(int changedIndex)
    {
        if (ignoreNextSlotChange)
        {
            ignoreNextSlotChange = false;
            return;
        }

        int inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        int inputEnd = inputStart + SlotIndexScheme.CookInputSlotCount;

        if (changedIndex >= inputStart && changedIndex < inputEnd)
        {
            TryCook();
            return;
        }

        if (changedIndex == resultSlot.SlotIndex &&
            (cookPotObject.CurrentState == CookingState.Idle ||
            cookPotObject.CurrentState == CookingState.Finished))
        {
            cookPotObject.ChangeState(CookingState.Idle);
            TryCook();
            return;
        }
    }

    private void TryCook()
    {
        if (cookPotObject.CurrentState == CookingState.Finishing) return;

        if (cookPotObject.CurrentState == CookingState.Cooking)
        {
            cookPotObject.StopCook();
        }

        foreach (var slot in inputSlots)
        {
            if (!slot.HasItem()) return;
        }

        Dictionary<IngredientTag, float> tags = new();
        float cookingTime = 0f;

        // 각 슬롯을 순회하며 태그와 시간을 계산.
        foreach (var slot in inputSlots)
        {
            ItemSO data = slot.GetData().ItemData;
            if (data == null || data.cookableData == null) continue;

            // 태그 합산.
            List<TagValuePair> _tags = data.cookableData.tags;
            foreach (TagValuePair pair in _tags)
            {
                tags.TryGetValue(pair.tag, out float currentValue);
                tags[pair.tag] = currentValue + pair.value;
            }

            // 시간 합산.
            cookingTime += data.cookableData.contributionTime;
        }

        cookingManager.FindMatchingRecipe(tags, cookingTime, cookPotObject);
    }

    public override void Open()
    {
        base.Open();
        Contents.localScale = Vector3.zero;
        Contents.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve).SetUpdate(true);
    }

    public override void Close()
    {
        base.Close();
    }
}
