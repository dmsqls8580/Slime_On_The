using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UICookPot : UIBase
{
    [SerializeField] private List<InventorySlot> inputSlots; // 3칸
    [SerializeField] private InventorySlot resultSlot;       // 1칸
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    private CookPotObject boundCookPot;
    private int cookIndex;

    public void Initialize(int cookPotIndex, CookPotObject cookPot)
    {
        boundCookPot = cookPot;
        cookIndex = cookPotIndex;
        
        int inputStart = SlotIndexScheme.GetCookInputStart(cookPotIndex);
        for (int i = 0; i < inputSlots.Count; i++)
        {
            inputSlots[i].Initialize(inputStart + i);
        }

        int resultIndex = SlotIndexScheme.GetCookResultIndex(cookPotIndex);
        resultSlot.Initialize(resultIndex);
        
        InventoryManager.Instance.OnSlotChanged += OnAnySlotChanged;
    }
    
    private void OnDisable()
    {
        if (InventoryManager.HasInstance)
            InventoryManager.Instance.OnSlotChanged -= OnAnySlotChanged;
    }

    public void Refresh()
    {
        foreach (var slot in inputSlots)
        {
            slot.Refresh();
        }
        resultSlot.Refresh();
        // cpo.tryCook()
    }
    
    private void OnAnySlotChanged(int changedIndex)
    {
        int inputStart = SlotIndexScheme.GetCookInputStart(cookIndex);
        int inputEnd = inputStart + SlotIndexScheme.CookInputSlotCount;

        if (changedIndex >= inputStart && changedIndex < inputEnd)
        {
            TryCookIfReady();
        }
    }
    
    private void TryCookIfReady()
    {
        bool allFilled = true;
        foreach (var slot in inputSlots)
        {
            var data = slot.GetData();
            if (data == null || !data.IsValid)
            {
                allFilled = false;
                break;
            }
        }

        if (allFilled)
        {
            boundCookPot.TryCook();
        }
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