using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIInventory : UIBase
{
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private AnimationCurve openCloseCurve;
    
    [SerializeField] private List<InventorySlot> inventorySlots;

    private Vector2 originPosition;
    private Vector2 targetPosition;
    
    private void Awake()
    {
        // 현재 위치, 목표 위치 저장
        originPosition = Contents.anchoredPosition;
        targetPosition = new Vector2(0, Contents.rect.height);
        Contents.gameObject.SetActive(false);
    }

    private void Start()
    {
        for (int i = 0; i < inventorySlots.Count; i++)
            inventorySlots[i].Initialize(i);
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
