using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UIChest : UIBase
{
    [SerializeField] private List<InventorySlot> chestSlots;
    [SerializeField] private AnimationCurve JellyAnimationCurve;

    public void Initialize(int chestIndex)
    {
        int startIndex = SlotIndexScheme.GetChestStart(chestIndex);
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i].Initialize(startIndex + i);
        }
    }

    public void Refresh()
    {
        foreach (var slot in chestSlots)
        {
            slot.Refresh();
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