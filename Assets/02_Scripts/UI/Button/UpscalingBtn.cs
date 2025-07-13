using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class UpscalingBtn : UIButtonBase
{
    [SerializeField] private AnimationCurve JellyAnimationCurve;
    [SerializeField] private float scaleMultiplier = 1.05f;

    
    public override void OnPointerEnter(PointerEventData eventData)
    {
        Vector3 targetScale = originalScale * scaleMultiplier;
        transform.DOKill();
        transform.DOScale(targetScale, enterDuration).SetEase(JellyAnimationCurve).SetUpdate(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        transform.DOKill();
        transform.DOScale(originalScale, exitDuration).SetEase(Ease.OutBack).SetUpdate(true);
    }
}
