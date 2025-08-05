using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class UIButtonBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    protected Vector3 originalScale;

    [SerializeField] protected float enterDuration = 0.2f;
    [SerializeField] protected float exitDuration = 0.1f;

    protected virtual void Awake()
    {
        originalScale = transform.localScale;
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(SFX.Click);
    }

    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        SoundManager.Instance.PlaySFX(SFX.Toggle);
    }

    public virtual void OnPointerExit(PointerEventData eventData)
    {
        
    }
}