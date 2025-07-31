using UnityEngine;
using DG.Tweening;
using System;

public class UICrafting : UIBase
{
    [SerializeField] private RectTransform HUD;
    
    [SerializeField] private float tweenDuration = 0.3f;
    [SerializeField] private AnimationCurve openCloseCurve;

    private Vector2 originPosition;
    private Vector2 targetPosition;

    private void Awake()
    {
        // 현재 위치, 목표 위치 저장
        originPosition = Contents.anchoredPosition;
        targetPosition = new Vector2(Contents.rect.width, 0);
    }

    private void Start()
    {
        Contents.gameObject.SetActive(false);
    }


    public override void Open()
    {
        HUD.gameObject.SetActive(false);
        base.Open();
        Contents.DOAnchorPos(targetPosition, tweenDuration)
            .SetEase(openCloseCurve);
    }

    public override void Close()
    {
        Contents.DOAnchorPos(originPosition, tweenDuration)
            .SetEase(openCloseCurve)
            .OnComplete(() => {
                base.Close();
                HUD.gameObject.SetActive(true);
            });
    }
}
