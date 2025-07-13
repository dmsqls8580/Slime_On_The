using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class UIStartGame : UIBase
{
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    [SerializeField] private RectTransform content;

    public void OnClickCreateSave()
    {
        UIManager.Instance.Toggle<UICreateSave>();
    }

    public void OnClickCancel()
    {
        UIManager.Instance.Toggle<UIStartGame>();
    }
    
    
    public override void Open()
    {
        base.Open();
        content.localScale = Vector3.zero;
        content.DOScale(Vector3.one, 0.3f).SetEase(JellyAnimationCurve);
    }

    public override void Close()
    {
        base.Close();
    }
}
