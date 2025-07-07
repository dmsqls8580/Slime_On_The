using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIInventory : UIBase
{
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private AnimationCurve openCloseCurve;
    
    [SerializeField] private List<InventorySlot> inventorySlots;
    [SerializeField] private List<EquipSlot> equipSlots;    

    private Vector2 originPosition;
    private Vector2 targetPosition;
    
    private HoldManager holdManager;
    private InventoryManager inventoryManager;
    
    private void Awake()
    {
        holdManager = HoldManager.Instance;
        inventoryManager = InventoryManager.Instance;
        
        // 현재 위치, 목표 위치 저장
        originPosition = Contents.anchoredPosition;
        targetPosition = new Vector2(0, Contents.rect.height);
        
        Contents.gameObject.SetActive(false);
    }

    private void Start()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].Initialize(i);
        }
        for (int i = 0; i < equipSlots.Count; i++)
        {
            equipSlots[i].Initialize(i);
        }
    }
    

    public void RefreshAll()
    {
        foreach (var slot in inventorySlots)
            slot.Refresh();

        foreach (var slot in equipSlots)
            slot.Refresh();
    }
    
    
    public override void Open()
    {
        base.Open();
        Contents.DOAnchorPos(targetPosition, tweenDuration)
            .SetEase(openCloseCurve);
    }

    public override void Close()
    {
        Contents.DOAnchorPos(originPosition, tweenDuration)
            .SetEase(openCloseCurve)
            .OnComplete(() => base.Close());
    }
}
