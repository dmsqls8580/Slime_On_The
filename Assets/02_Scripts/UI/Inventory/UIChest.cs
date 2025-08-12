using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;

public class UIChest : UIBase
{
    [SerializeField] private List<InventorySlot> chestSlots;
    [SerializeField] private AnimationCurve JellyAnimationCurve;

    private InventoryManager inventoryManager;
    
    private ChestObject chestObject;
    private int chestIndex;
    public int ChestIndex => chestIndex;
    
    private void Awake()
    {
        inventoryManager = InventoryManager.Instance;
    }
    
    public void Initialize(ChestObject _chestObject)
    {
        chestObject = _chestObject;
        chestIndex = chestObject.ChestIndex;

        int chestStart = SlotIndexScheme.GetChestStart(chestIndex);
        for (int i = 0; i < chestSlots.Count; i++)
        {
            chestSlots[i].Initialize(chestStart + i);
        }
    }
    
    public override void Open()
    {
        base.Open();
        Contents.localScale = Vector3.zero;
        Contents.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve);
    }

    public override void Close()
    {
        TooltipManager.Instance.Hide();
        base.Close();
        chestIndex = -1;
    }
}