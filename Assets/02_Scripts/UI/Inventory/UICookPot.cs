using _02_Scripts.Manager;
using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UICookPot : UIBase
{
    [SerializeField] private List<InventorySlot> inputSlots;
    [SerializeField] private InventorySlot resultSlot;
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    [SerializeField] private Image processImage;
    [SerializeField] private Image targetImage;

    private InventoryManager inventoryManager;
    private CookingManager cookingManager;

    private CookPotObject cookPotObject;
    private int cookIndex;
    public int CookIndex => cookIndex;

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
    }

    private void OnDisable()
    {
        if (InventoryManager.HasInstance)
            inventoryManager.OnSlotChanged -= OnAnySlotChanged;
    }
    
    private void OnAnySlotChanged(int changedIndex)
    {
        int inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        int inputEnd = inputStart + SlotIndexScheme.CookInputSlotCount;
        
        if (changedIndex >= inputStart && changedIndex < inputEnd)
        {
            if (inventoryManager.GetItem(changedIndex) != null)
            {
                if (inventoryManager.GetItem(changedIndex).ItemData != cookPotObject.prevItems[changedIndex % 10])
                {
                    cookPotObject.StopCook();
                }
                cookPotObject.prevItems[changedIndex%10] = inventoryManager.GetItem(changedIndex).ItemData;
            }
            
            TryCook();
            return;
        }

        if (changedIndex == inputEnd &&
            (cookPotObject.CurrentState == CookingState.Idle ||
            cookPotObject.CurrentState == CookingState.Finished))
        {
            cookPotObject.ChangeState(CookingState.Idle);
            cookPotObject.finishedItem = null;
            TryCook();
            return;
        }
    }

    private void TryCook()
    {
        foreach (var slot in inputSlots)
        {
            if (!slot.HasItem())
            {
                cookPotObject.StopCook();
                return;
            }
        }
        
        if (cookPotObject.CurrentState == CookingState.Finishing) return;

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

        ItemSO targetItem = cookingManager.FindMatchingRecipe(tags);
        cookPotObject.StartCook(targetItem, cookingTime);
    }

    public void RefreshProcessImg(int index, float processPercentage)
    {
        if(CookIndex != index) return;
        processImage.fillAmount = 1 - processPercentage;
    }

    public void RefreshTargetImg(int index, ItemSO target)
    {
        if(CookIndex != index) return;
        
        if (target == null) targetImage.gameObject.SetActive(false);
        else
        {
            targetImage.gameObject.SetActive(true);
            targetImage.sprite = target.icon;
        }
    }

    public override void Open()
    {
        base.Open();
        processImage.fillAmount = 0;
        Contents.localScale = Vector3.zero;
        Contents.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve);
    }

    public override void Close()
    {
        base.Close();
        cookIndex = -1;
    }
}