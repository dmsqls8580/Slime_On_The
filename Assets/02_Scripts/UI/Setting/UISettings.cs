using DG.Tweening;
using System;
using TMPro;
using UnityEngine;

public class UISettings : UIBase
{
    [SerializeField] private int nowState;
    [SerializeField] private RectTransform[] menuBtnImage;
    [SerializeField] private TextMeshProUGUI[] menuBtnText;
    [SerializeField] private GameObject[] menuSettings;
    
    [SerializeField] private float tweenDuration = 0.5f;
    [SerializeField] private AnimationCurve openCurve;
    [SerializeField] private AnimationCurve closeCurve;
    [SerializeField] private float fixedHeight = 100f; 
    
    private Tween openTween;
    private Tween closeTween;

    public void OnInputMenuButton(int direction)
    {
        int state = (nowState + direction) % menuBtnImage.Length;
        if(state == -1) state = menuBtnImage.Length - 1;
        OnClickMenuButton(state);
    }

    public void OnClickMenuButton(int state)
    {
        if(state == nowState) return;
        
        for (int i = 0; i < menuBtnImage.Length; i++)
        {
            int index = i;
            menuBtnText[index].gameObject.SetActive(false);
			menuSettings[index].SetActive(false);
        }

        for (int i = 0; i < menuBtnImage.Length; i++)
        {
            int index = i;
            
            if (index == state)
            {
                openTween?.Kill();
                float time = 0f;
                bool textShown = false;

                openTween = DOTween.To(() => time, x => time = x, tweenDuration, tweenDuration)
                    .SetUpdate(true)
                    .OnUpdate(() =>
                    {
                        float t = time / tweenDuration;
                        float curveValue = openCurve.Evaluate(t);
                        float width = curveValue * 100f;

                        menuBtnImage[index].sizeDelta = new Vector2(width, fixedHeight);

                        if (!textShown && t >= 0.5f)
                        {
                            menuBtnText[index].gameObject.SetActive(true);
							menuSettings[index].SetActive(true);
                            textShown = true;
                        }
                    });
            }
            else if (index == nowState)
            {
                closeTween?.Kill();
                float time = 0f;

                closeTween = DOTween.To(() => time, x => time = x, tweenDuration, tweenDuration)
                    .SetUpdate(true)
                    .OnUpdate(() =>
                    {
                        float curveValue = closeCurve.Evaluate(time / tweenDuration);
                        float width = curveValue * 100f;

                        menuBtnImage[index].sizeDelta = new Vector2(width, fixedHeight);
                    });
            }
        }
        
        nowState = state;
    }
    
    
    public override void Open()
    {
        base.Open();
        OnClickMenuButton(0);
    }
    
    public override void Close()
    {
        nowState = -2;
        base.Close();
    }
}
